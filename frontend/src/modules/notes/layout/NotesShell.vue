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
      class="min-w-0 border-r max-sm:flex-1 sm:w-80 sm:shrink-0 lg:w-85"
      :class="{ 'max-sm:hidden': hasNote }"
    >
      <NotesList />
    </section>

    <main
      class="min-w-0 flex-1 overflow-y-auto"
      :class="{ 'max-sm:hidden': !hasNote }"
    >
      <RouterView />
    </main>
  </div>
</template>
