<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { errorMessage } from '@/i18n/errors'
import AuthCard from '../components/AuthCard.vue'
import { useSessionStore } from '../stores/session'

const route = useRoute()
const router = useRouter()
const session = useSessionStore()

const errorCode = computed(() => {
  if (typeof route.query.error === 'string') {
    return route.query.error
  }
  return session.isAuthenticated ? null : 'auth.google_failed'
})

onMounted(() => {
  if (!errorCode.value) {
    void router.replace({ name: 'home' })
  }
})
</script>

<template>
  <AuthCard
    v-if="errorCode"
    :title="$t('auth.callback.title')"
    :description="errorMessage(errorCode)"
  >
    <template #footer>
      <RouterLink
        class="text-sm underline"
        :to="{ name: 'login' }"
      >
        {{ $t('auth.callback.back') }}
      </RouterLink>
    </template>
  </AuthCard>
</template>
