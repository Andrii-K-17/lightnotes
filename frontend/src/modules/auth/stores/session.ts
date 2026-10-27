import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import type { components } from '@/lib/api'
import { authApi } from '../api'

export type Profile = components['schemas']['UserProfile']
type Status = 'unknown' | 'authenticated' | 'anonymous'

/** Manages the user's authentication state and in-memory access token. */
export const useSessionStore = defineStore('session', () => {
  const status = ref<Status>('unknown')
  const profile = ref<Profile | null>(null)
  const isAuthenticated = computed(() => status.value === 'authenticated')

  let accessToken: string | null = null
  let restoring: Promise<void> | undefined
  let refreshing: Promise<boolean> | undefined

  function getAccessToken(): string | null {
    return accessToken
  }

  async function loadProfile(): Promise<void> {
    profile.value = await authApi.me()
    status.value = 'authenticated'
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
    } finally {
      clear()
    }
  }

  function clear(): void {
    accessToken = null
    profile.value = null
    status.value = 'anonymous'
  }

  async function renewAccessToken(): Promise<boolean> {
    try {
      accessToken = (await authApi.refresh()).accessToken
      return true
    } catch {
      return false
    }
  }

  function refresh(): Promise<boolean> {
    refreshing ??= navigator.locks.request('ln-refresh', renewAccessToken).finally(() => {
      refreshing = undefined
    })
    return refreshing
  }

  /** Restores the session from the refresh cookie. Runs once, later calls share the result. */
  function restore(): Promise<void> {
    restoring ??= (async () => {
      try {
        if (await refresh()) {
          await loadProfile()
          return
        }
      } catch {}

      clear()
    })()

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
