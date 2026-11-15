import { useDebounceFn, useEventListener, useIntervalFn } from '@vueuse/core'
import { onBeforeUnmount, onMounted, watch } from 'vue'
import { pendingChanges } from '@/data'
import { useLiveQuery } from '@/lib/live-query'
import { useSyncStore } from '../stores/sync'

const AFTER_SAVE_MS = 3000
const WHILE_DIRTY_MS = 30_000

/** Starts background sync while the signed-in app is mounted. */
export function useSyncScheduler(): void {
  const sync = useSyncStore()
  const { data: pending } = useLiveQuery(pendingChanges)

  onMounted(() => void sync.start())
  onBeforeUnmount(() => sync.stop())

  // Retry as soon as the browser reconnects.
  useEventListener(window, 'online', () => void sync.run())

  // Sync when returning to the tab, unless an error retry is pending.
  useEventListener(document, 'visibilitychange', () => {
    if (document.visibilityState === 'visible' && sync.state !== 'error') {
      void sync.run()
    }
  })

  const runSoon = useDebounceFn(() => void sync.run(), AFTER_SAVE_MS)

  watch(pending, (count) => {
    if (count) {
      void runSoon()
    }
  })

  // Retry periodically while unsynced changes remain.
  useIntervalFn(() => {
    if (pending.value && sync.state !== 'error') {
      void sync.run()
    }
  }, WHILE_DIRTY_MS)
}
