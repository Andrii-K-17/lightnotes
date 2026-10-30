<script setup lang="ts">
import { useNewNoteShortcut } from '@/modules/notes'

useNewNoteShortcut()
</script>

<template>
  <div class="min-h-svh bg-background text-foreground">
    <slot />
  </div>
</template>
