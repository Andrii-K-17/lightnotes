<script setup lang="ts">
import { Pin } from '@lucide/vue'
import type { NoteRecord } from '@/data'
import { displayPreview, displayTitle, formatEditedAt } from '../lib/display'
import { RouterLink } from 'vue-router'

const props = defineProps<{ note: NoteRecord }>()

const title = () => displayTitle(props.note)
</script>

<template>
  <RouterLink
    :to="{ name: 'note', params: { id: note.id } }"
    class="flex min-h-36 flex-col gap-2 rounded-2xl border bg-card p-4 text-card-foreground outline-none transition-colors hover:bg-accent/40 focus-visible:ring-2 focus-visible:ring-ring"
  >
    <div class="flex items-start justify-between gap-2">
      <h2
        class="line-clamp-2 font-medium"
        :class="{ 'text-muted-foreground': !title() }"
      >
        {{ title() || $t('notes.untitled') }}
      </h2>
      <Pin
        v-if="note.pinned"
        class="mt-1 size-4 shrink-0 text-primary"
        :aria-label="$t('notes.pinned')"
      />
    </div>
    <p class="line-clamp-4 text-sm text-muted-foreground">
      {{ displayPreview(note) }}
    </p>
    <time class="mt-auto text-xs text-muted-foreground">
      {{ formatEditedAt(note.editedAt) }}
    </time>
  </RouterLink>
</template>
