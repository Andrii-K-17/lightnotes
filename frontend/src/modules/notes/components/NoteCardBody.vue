<script setup lang="ts">
import { Pin } from '@lucide/vue'
import type { NoteRecord } from '@/data'
import { displayPreview, displayTitle, formatEditedAt } from '../lib/display'
import { daysLeftInTrash } from '../lib/trash'

defineProps<{ note: NoteRecord }>()
</script>

<template>
  <div class="flex items-start gap-2 pr-8">
    <Pin
      v-if="note.pinned"
      class="mt-1 size-4 shrink-0 text-primary"
      :aria-label="$t('notes.pinned')"
    />
    <h2
      class="line-clamp-2 font-medium"
      :class="{ 'text-muted-foreground': !displayTitle(note) }"
    >
      {{ displayTitle(note) || $t('notes.untitled') }}
    </h2>
  </div>
  <p class="line-clamp-4 text-sm text-muted-foreground">
    {{ displayPreview(note) }}
  </p>
  <p
    v-if="note.deletedAt !== null"
    class="mt-auto text-xs text-muted-foreground"
  >
    {{ $t('notes.daysLeft', daysLeftInTrash(note.deletedAt)) }}
  </p>
  <time
    v-else
    class="mt-auto text-xs text-muted-foreground"
  >
    {{ formatEditedAt(note.editedAt) }}
  </time>
</template>
