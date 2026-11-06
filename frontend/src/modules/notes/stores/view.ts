import { useStorage } from '@vueuse/core'
import { defineStore } from 'pinia'
import type { NoteScope } from '@/data'
import { ref } from 'vue'

export const useNotesView = defineStore('notes-view', () => {
  const scope = useStorage<NoteScope>('lightnotes.scope', { kind: 'all' })
  const tagIds = ref<string[]>([])

  function select(next: NoteScope): void {
    scope.value = next
  }

  function toggleTag(id: string): void {
    tagIds.value = tagIds.value.includes(id)
      ? tagIds.value.filter((tagId) => tagId !== id)
      : [...tagIds.value, id]
  }

  function clearTags(): void {
    tagIds.value = []
  }

  return {
    scope,
    tagIds,
    select,
    toggleTag,
    clearTags,
  }
})

export function isSameScope(a: NoteScope, b: NoteScope): boolean {
  return a.kind === b.kind && (a.kind !== 'folder' || (b.kind === 'folder' && a.id === b.id))
}
