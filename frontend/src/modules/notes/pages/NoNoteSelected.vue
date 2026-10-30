<script setup lang="ts">
import { FileText } from '@lucide/vue'
</script>

<template>
  <div class="flex h-full flex-col items-center justify-center gap-3 text-muted-foreground">
    <FileText class="size-12 opacity-50" />
    <p>
      {{ $t('notes.noneSelected') }}
    </p>
  </div>
</template>
