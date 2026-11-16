<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppLayout from './layouts/AppLayout.vue'
import AuthLayout from './layouts/AuthLayout.vue'
import { useTheme } from '@/lib/theme.ts'
import { Toaster } from '@/components/ui/sonner'

const route = useRoute()
const router = useRouter()
const layout = computed(() => (route.meta.layout === 'auth' ? AuthLayout : AppLayout))

const ready = ref(false)

void router.isReady().then(() => {
  ready.value = true
})

useTheme()
</script>

<template>
  <component
    :is="layout"
    v-if="ready"
  >
    <RouterView />
  </component>

  <Toaster />
</template>
