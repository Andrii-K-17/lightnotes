<script setup lang="ts">
import { useNewNoteShortcut } from '@/modules/notes'
import { useSyncScheduler } from '@/modules/sync'

useNewNoteShortcut()
useSyncScheduler()
</script>

<template>
  <div class="min-h-svh bg-background text-foreground">
    <slot />
  </div>
</template>
