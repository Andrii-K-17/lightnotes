<script setup lang="ts">
import { usePwaPrompts } from '@/app/pwa-prompts'
import { useNewNoteShortcut } from '@/modules/notes'
import { useSyncScheduler } from '@/modules/sync'

useNewNoteShortcut()
useSyncScheduler()
usePwaPrompts()
</script>

<template>
  <div class="min-h-svh bg-background text-foreground">
    <slot />
  </div>
</template>
