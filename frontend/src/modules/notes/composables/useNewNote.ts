import { useEventListener } from '@vueuse/core'
import { useRouter } from 'vue-router'
import { noteRepository } from '@/data'

export function useNewNote() {
  const router = useRouter()

  async function createNote(): Promise<void> {
    const note = await noteRepository.create()

    await router.push({ name: 'note', params: { id: note.id } })
  }

  return { createNote }
}

export function useNewNoteShortcut(): void {
  const { createNote } = useNewNote()

  useEventListener(window, 'keydown', (event) => {
    if (
      event.code !== 'KeyN' ||
      event.shiftKey ||
      !(event.ctrlKey || event.metaKey || event.altKey)
    ) {
      return
    }

    event.preventDefault()
    void createNote()
  })
}
