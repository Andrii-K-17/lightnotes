import { useStorage } from '@vueuse/core'
import { defineStore } from 'pinia'
import type { NoteScope } from '@/data'

export const useNotesView = defineStore('notes-view', () => {
  const scope = useStorage<NoteScope>('lightnotes.scope', { kind: 'all' })

  function select(next: NoteScope): void {
    scope.value = next
  }

  return {
    scope,
    select,
  }
})

export function isSameScope(a: NoteScope, b: NoteScope): boolean {
  return a.kind === b.kind && (a.kind !== 'folder' || (b.kind === 'folder' && a.id === b.id))
}
