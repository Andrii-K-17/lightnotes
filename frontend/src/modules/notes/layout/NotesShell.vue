<script setup lang="ts">
import { computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import NotesList from '../components/NotesList.vue'
import NotesSidebar from '../components/NotesSidebar.vue'
import { useNotesView } from '../stores/view.ts'
import { provideFolders } from '../composables/useFolders.ts'
import { provideTags } from '../composables/useTags.ts'

const route = useRoute()
const view = useNotesView()
const folders = provideFolders()
const tags = provideTags()

const hasNote = computed(() => route.name === 'note')

watch(hasNote, (open) => {
  if (!open) {
    view.editorExpanded = false
    view.middleHidden = false
  }
})

watch([folders, () => view.scope], ([list, scope]) => {
  if (list && scope.kind === 'folder' && !list.some((folder) => folder.id === scope.id)) {
    view.select({ kind: 'all' })
  }
})

watch(tags, (list) => {
  if (list && view.tagIds.some((id) => !list.some((tag) => tag.id === id))) {
    view.tagIds = view.tagIds.filter((id) => list.some((tag) => tag.id === id))
  }
})
</script>

<template>
  <div class="flex h-dvh">
    <aside class="hidden w-64 shrink-0 border-r lg:block">
      <NotesSidebar />
    </aside>

    <section
      class="min-w-0 border-r transition-[width,flex-basis,opacity,transform] duration-300 ease-in-out max-sm:absolute max-sm:inset-y-0 max-sm:left-0 max-sm:z-20 max-sm:bg-background"
      :class="[
        !hasNote
          ? 'w-full max-sm:translate-x-0 max-sm:opacity-100 sm:flex-1'
          : view.middleHidden
            ? 'w-0 overflow-hidden border-r-0 opacity-0 sm:w-0 lg:w-0'
            : 'max-sm:-translate-x-full max-sm:opacity-0 sm:w-80 sm:shrink-0 lg:w-85',
      ]"
    >
      <NotesList />
    </section>

    <main
      v-if="hasNote"
      class="min-w-0 flex-1 overflow-y-auto transition-[width,flex-basis] duration-300 ease-in-out"
    >
      <RouterView />
    </main>
  </div>
</template>
