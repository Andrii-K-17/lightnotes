import { useDebounceFn, useEventListener } from '@vueuse/core'
import { onBeforeUnmount } from 'vue'
import { noteRepository, type RichText } from '@/data'

export interface EditorChanges {
  title?: string
  content?: RichText
  contentText?: string
}

/** An edit waiting to be saved for one note. */
interface Pending {
  id: string
  baseVersion: number
  changes: EditorChanges
}

interface Options {
  /** Called before saving with the title and serialized content being written. */
  onWrite?: (written: { title?: string; content?: string }) => void
}

const DEBOUNCE_MS = 800
const MAX_WAIT_MS = 5000

/** Saves editor changes after a short delay and flushes them when the page is left. */
export function useAutosave(options: Options = {}) {
  let pending: Pending | undefined

  /** Writes the queued changes to the note repository. */
  async function flush(): Promise<void> {
    if (!pending) {
      return
    }

    const { id, baseVersion, changes } = pending
    pending = undefined

    const content = changes.content ? JSON.stringify(changes.content) : undefined

    options.onWrite?.({ title: changes.title, content })

    await noteRepository.save(
      id,
      {
        title: changes.title,
        content,
        contentText: changes.contentText,
      },
      baseVersion,
    )
  }

  const flushLater = useDebounceFn(flush, DEBOUNCE_MS, { maxWait: MAX_WAIT_MS })

  /** Queues changes and saves them after a delay, keeping the original base version. */
  function schedule(id: string, changes: EditorChanges, baseVersion: number): void {
    if (pending && pending.id !== id) {
      void flush()
    }

    pending = {
      id,
      baseVersion: pending?.baseVersion ?? baseVersion,
      changes: {
        ...pending?.changes,
        ...changes,
      },
    }

    void flushLater()
  }

  /** Returns whether unsaved editor changes are queued. */
  const hasPending = (): boolean => pending !== undefined

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
    hasPending,
  }
}
