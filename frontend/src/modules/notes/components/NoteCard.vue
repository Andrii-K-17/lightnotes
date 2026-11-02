<script setup lang="ts">
import { RouterLink } from 'vue-router'
import type { NoteRecord } from '@/data'
import NoteCardBody from './NoteCardBody.vue'
import NoteMenu from './NoteMenu.vue'

defineProps<{ note: NoteRecord }>()

const card =
  'flex h-full min-h-36 flex-col gap-2 rounded-2xl border bg-card p-4 text-card-foreground outline-none'
</script>

<template>
  <div class="relative">
    <div
      v-if="note.deletedAt !== null"
      :class="card"
    >
      <NoteCardBody :note="note" />
    </div>
    <RouterLink
      v-else
      :to="{ name: 'note', params: { id: note.id } }"
      active-class="border-primary/50 bg-accent/60"
      :class="[
        card,
        'transition-colors hover:bg-accent/40 focus-visible:ring-2 focus-visible:ring-ring',
      ]"
    >
      <NoteCardBody :note="note" />
    </RouterLink>

    <div class="absolute right-2 top-2">
      <NoteMenu :note="note" />
    </div>
  </div>
</template>
