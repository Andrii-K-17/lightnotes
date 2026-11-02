<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import AppLayout from './layouts/AppLayout.vue'
import AuthLayout from './layouts/AuthLayout.vue'
import { useTheme } from '@/lib/theme.ts'
import { Toaster } from '@/components/ui/sonner'

const route = useRoute()
const layout = computed(() => (route.meta.layout === 'auth' ? AuthLayout : AppLayout))

useTheme()
</script>

<template>
  <component :is="layout">
    <RouterView />
  </component>

  <Toaster />
</template>
