import { useEventListener } from '@vueuse/core'
import { useRouter } from 'vue-router'
import { noteRepository } from '@/data'
import { useNotesView } from '../stores/view'

export function useNewNote() {
  const router = useRouter()
  const view = useNotesView()

  async function createNote(): Promise<void> {
    const note = await noteRepository.create(view.scope.kind === 'folder' ? view.scope.id : null)
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
