import { liveQuery } from 'dexie'
import { onScopeDispose, shallowRef, watch, type WatchSource } from 'vue'

/** Exposes a Dexie live query as Vue refs and reruns it when dependencies change. */
export function useLiveQuery<T>(querier: () => T | Promise<T>, deps: WatchSource[] = []) {
  const data = shallowRef<T>()
  const error = shallowRef<unknown>()
  let subscription: { unsubscribe: () => void } | undefined

  watch(
    deps,
    () => {
      subscription?.unsubscribe()
      subscription = liveQuery(querier).subscribe({
        next: (value) => {
          data.value = value
          error.value = undefined
        },
        error: (cause) => {
          error.value = cause
        },
      })
    },
    { immediate: true },
  )

  onScopeDispose(() => subscription?.unsubscribe())

  return {
    data,
    error,
  }
}
