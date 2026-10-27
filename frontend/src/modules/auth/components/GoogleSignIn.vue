<script setup lang="ts">
import { Button } from '@/components/ui/button'
</script>

<template>
  <div class="grid gap-4">
    <div class="flex items-center gap-3 text-xs text-muted-foreground">
      <span class="h-px flex-1 bg-border" />
      {{ $t('auth.or') }}
      <span class="h-px flex-1 bg-border" />
    </div>

    <Button
      as-child
      variant="outline"
      class="w-full"
    >
      <a href="/api/v1/auth/google/login">
        {{ $t('auth.google') }}
      </a>
    </Button>
  </div>
</template>
