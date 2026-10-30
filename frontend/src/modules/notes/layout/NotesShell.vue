<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import NotesList from '../components/NotesList.vue'
import NotesSidebar from '../components/NotesSidebar.vue'

const route = useRoute()

const hasNote = computed(() => route.name === 'note')
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
