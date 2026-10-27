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
import GoogleSignIn from '../components/GoogleSignIn.vue'

const route = useRoute()
const session = useSessionStore()
const { email, password, errorCode, busy, submit } = useAuthForm(session.login)
const error = computed(() => (errorCode.value ? errorMessage(errorCode.value) : null))
</script>

<template>
  <AuthCard
    :title="$t('auth.login.title')"
    :description="$t('auth.login.description')"
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
          maxlength="128"
          autocomplete="current-password"
        />
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
        {{ $t('auth.login.submit') }}
      </Button>
    </form>

    <template #footer>
      <GoogleSignIn />

      <p class="text-sm text-muted-foreground">
        {{ $t('auth.login.noAccount') }}
        <RouterLink
          class="underline"
          :to="{ name: 'register', query: route.query }"
        >
          {{ $t('auth.login.toRegister') }}
        </RouterLink>
      </p>
    </template>
  </AuthCard>
</template>
