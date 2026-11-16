import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { getSyncCursor, pendingChanges, syncOnce } from '@/data'
import { ApiError } from '@/lib/api'
import { toast } from 'vue-sonner'
import { i18n } from '@/i18n'

type State = 'idle' | 'syncing' | 'offline' | 'error'

const OFFLINE_PROBE_MS = 30_000
const MAX_BACKOFF_MS = 5 * 60_000

/** Calculates a retry delay that grows with each failure, up to the maximum. */
function backoff(failures: number): number {
  const delay = Math.min(2000 * 2 ** (failures - 1), MAX_BACKOFF_MS)
  return delay * (0.8 + Math.random() * 0.4)
}

export const useSyncStore = defineStore('sync', () => {
  const state = ref<State>('idle')
  const errorCode = ref<string | null>(null)
  const lastSyncedAt = ref<number | null>(null)

  /** Number of entities received in the current sync cycle. */
  const received = ref(0)

  /** True until this device completes its first sync. */
  const needsInitialSync = ref(false)
  const initialSyncing = computed(() => needsInitialSync.value && state.value === 'syncing')

  let failures = 0
  let timer: ReturnType<typeof setTimeout> | undefined
  let running: Promise<void> | undefined
  let stopped = true

  /** Schedules the next sync attempt. */
  function schedule(delayMs: number): void {
    clearTimeout(timer)
    timer = setTimeout(() => void run(), delayMs)
  }

  /** Runs one sync cycle and schedules another if there are pending changes. */
  async function cycle(): Promise<void> {
    clearTimeout(timer)
    state.value = 'syncing'
    received.value = 0

    try {
      const report = await navigator.locks.request('ln-sync', () =>
        syncOnce((count) => {
          received.value = count
        }),
      )

      if (report.conflicts > 0) {
        toast(i18n.global.t('sync.conflict'))
      }

      failures = 0
      errorCode.value = null
      state.value = 'idle'
      lastSyncedAt.value = Date.now()
      needsInitialSync.value = false

      if (!stopped && (await pendingChanges()) > 0) {
        schedule(1000)
      }
    } catch (error) {
      fail(error)
    }
  }

  /** Updates the sync state and schedules a retry after a failure. */
  function fail(error: unknown): void {
    if (stopped) {
      return
    }

    if (error instanceof ApiError && error.isNetwork) {
      state.value = 'offline'
      schedule(OFFLINE_PROBE_MS)
      return
    }

    failures++
    state.value = 'error'
    errorCode.value = error instanceof ApiError ? error.code : 'unknown'

    const retryAfter = error instanceof ApiError ? error.retryAfterSeconds : null

    schedule(retryAfter !== null ? retryAfter * 1000 : backoff(failures))
  }

  /** Starts a sync cycle unless one is already running. */
  function run(): Promise<void> {
    if (stopped) {
      return Promise.resolve()
    }

    running ??= cycle().finally(() => {
      running = undefined
    })

    return running
  }

  /** Retries immediately without the current backoff delay. */
  function retry(): Promise<void> {
    failures = 0

    return run()
  }

  /** Initializes sync state and starts the first cycle. */
  async function start(): Promise<void> {
    stopped = false
    needsInitialSync.value = (await getSyncCursor()) === 0
    await run()
  }

  /** Stops scheduled sync attempts and resets the visible state. */
  function stop(): void {
    stopped = true
    clearTimeout(timer)
    state.value = 'idle'
  }

  return {
    state,
    errorCode,
    lastSyncedAt,
    received,
    initialSyncing,
    run,
    retry,
    start,
    stop,
  }
})
