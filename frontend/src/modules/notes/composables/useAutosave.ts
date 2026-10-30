import { useDebounceFn, useEventListener } from '@vueuse/core'
import { onBeforeUnmount } from 'vue'
import { noteRepository, type NoteRecord } from '@/data'

type Changes = Partial<Pick<NoteRecord, 'title' | 'content' | 'contentText'>>

const DEBOUNCE_MS = 800
const MAX_WAIT_MS = 5000

export function useAutosave() {
  let pending:
    | {
        id: string
        changes: Changes
      }
    | undefined

  async function flush(): Promise<void> {
    if (!pending) {
      return
    }

    const { id, changes } = pending
    pending = undefined

    await noteRepository.save(id, changes)
  }

  const flushLater = useDebounceFn(flush, DEBOUNCE_MS, { maxWait: MAX_WAIT_MS })

  function schedule(id: string, changes: Changes): void {
    if (pending && pending.id !== id) {
      void flush()
    }

    pending = {
      id,
      changes: {
        ...pending?.changes,
        ...changes,
      },
    }

    void flushLater()
  }

  useEventListener(document, 'visibilitychange', () => {
    if (document.visibilityState === 'hidden') {
      void flush()
    }
  })
  useEventListener(window, 'pagehide', () => void flush())

  onBeforeUnmount(() => void flush())

  return {
    schedule,
    flush,
  }
}
