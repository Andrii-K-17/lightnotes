<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { useSessionStore } from '@/modules/auth'

const session = useSessionStore()
</script>

<template>
  <div class="flex items-center justify-between p-4">
    <h1 class="text-2xl font-semibold">{{ $t('app.name') }}</h1>
    <RouterLink
      class="text-sm underline"
      :to="{ name: 'settings' }"
    >
      {{ $t('settings.title') }}
    </RouterLink>
  </div>
  <p class="px-4 text-sm text-muted-foreground">{{ session.profile?.email }}</p>
</template>
