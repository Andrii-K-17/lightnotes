<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { errorMessage } from '@/i18n/errors'
import AuthCard from '../components/AuthCard.vue'
import { useAuthForm } from '../composables/useAuthForm'
import { useSessionStore } from '../stores/session'

const route = useRoute()
const session = useSessionStore()
const { email, password, errorCode, busy, submit } = useAuthForm(session.register)
const error = computed(() => (errorCode.value ? errorMessage(errorCode.value) : null))
</script>

<template>
  <AuthCard
    :title="$t('auth.register.title')"
    :description="$t('auth.register.description')"
  >
    <form
      class="grid gap-4"
      @submit.prevent="submit"
    >
      <div class="grid gap-2">
        <Label for="email">{{ $t('auth.email') }}</Label>
        <Input
          id="email"
          v-model="email"
          type="email"
          required
          maxlength="254"
          autocomplete="email"
        />
      </div>
      <div class="grid gap-2">
        <Label for="password">{{ $t('auth.password') }}</Label>
        <Input
          id="password"
          v-model="password"
          type="password"
          required
          minlength="10"
          maxlength="128"
          autocomplete="new-password"
        />
        <p class="text-xs text-muted-foreground">
          {{ $t('auth.register.passwordHint') }}
        </p>
      </div>
      <p
        v-if="error"
        role="alert"
        class="text-sm text-destructive"
      >
        {{ error }}
      </p>
      <Button
        type="submit"
        :disabled="busy"
      >
        {{ $t('auth.register.submit') }}
      </Button>
    </form>

    <template #footer>
      <p class="text-sm text-muted-foreground">
        {{ $t('auth.register.haveAccount') }}
        <RouterLink
          class="underline"
          :to="{ name: 'login', query: route.query }"
        >
          {{ $t('auth.register.toLogin') }}
        </RouterLink>
      </p>
    </template>
  </AuthCard>
</template>
