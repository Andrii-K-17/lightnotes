import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { closeDatabase, openDatabase } from '@/data'
import { ApiError, type RefreshResult } from '@/lib/api'
import { authApi } from '../api'
import { clearCachedProfile, readCachedProfile, writeCachedProfile } from '../profile-cache'
import type { Profile } from '../types'

type Status = 'unknown' | 'authenticated' | 'anonymous'

/** Manages the user's authentication state and in-memory access token. */
export const useSessionStore = defineStore('session', () => {
  const status = ref<Status>('unknown')
  const profile = ref<Profile | null>(null)
  const isAuthenticated = computed(() => status.value === 'authenticated')

  let accessToken: string | null = null
  let restoring: Promise<void> | undefined
  let refreshing: Promise<RefreshResult> | undefined

  function getAccessToken(): string | null {
    return accessToken
  }

  async function signIn(loaded: Profile): Promise<void> {
    await openDatabase(loaded.id)
    profile.value = loaded
    status.value = 'authenticated'
  }

  async function loadProfile(): Promise<void> {
    const loaded = await authApi.me()

    await signIn(loaded)
    writeCachedProfile(loaded)
  }

  async function login(email: string, password: string): Promise<void> {
    accessToken = (await authApi.login(email, password)).accessToken
    await loadProfile()
  }

  async function register(email: string, password: string): Promise<void> {
    await authApi.register(email, password)
    await login(email, password)
  }

  async function logout(): Promise<void> {
    try {
      await authApi.logout()
    } catch (error) {
      if (error instanceof ApiError && error.isNetwork) {
        throw error
      }
    }

    clear()
  }

  function clear(): void {
    closeDatabase()
    clearCachedProfile()
    accessToken = null
    profile.value = null
    status.value = 'anonymous'
  }

  async function renewAccessToken(): Promise<RefreshResult> {
    try {
      accessToken = (await authApi.refresh()).accessToken

      return 'renewed'
    } catch (error) {
      return error instanceof ApiError && error.status === 401 ? 'rejected' : 'unreachable'
    }
  }

  function refresh(): Promise<RefreshResult> {
    refreshing ??= navigator.locks.request('ln-refresh', renewAccessToken).finally(() => {
      refreshing = undefined
    })
    return refreshing
  }

  async function restoreSession(): Promise<void> {
    const outcome = await refresh()

    if (outcome === 'rejected') {
      return clear()
    }

    if (outcome === 'renewed') {
      try {
        return await loadProfile()
      } catch (error) {
        if (!(error instanceof ApiError && error.isNetwork)) {
          return clear()
        }
      }
    }

    const cached = readCachedProfile()

    if (cached) {
      await signIn(cached)
    } else {
      clear()
    }
  }

  function restore(): Promise<void> {
    restoring ??= restoreSession().catch(clear)
    return restoring
  }

  return {
    status,
    profile,
    isAuthenticated,
    getAccessToken,
    login,
    register,
    logout,
    clear,
    refresh,
    restore,
  }
})
