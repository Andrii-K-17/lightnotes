<script setup lang="ts">
import { RouterLink, useRouter } from 'vue-router'
import { Button } from '@/components/ui/button'
import { useTheme } from '@/lib/theme'
import { useSessionStore } from '@/modules/auth'
import { ref } from 'vue'
import { errorMessage } from '@/i18n/errors'
import { ApiError } from '@/lib/api'

const router = useRouter()
const session = useSessionStore()
const theme = useTheme()

const themes = ['auto', 'light', 'dark'] as const

const signOutError = ref<string | null>(null)

async function signOut(): Promise<void> {
  signOutError.value = null

  try {
    await session.logout()
  } catch (error) {
    signOutError.value = errorMessage(error instanceof ApiError ? error.code : 'unknown')
    return
  }

  await router.replace({ name: 'login' })
}
</script>

<template>
  <div class="mx-auto grid max-w-3xl gap-8 p-4">
    <div class="flex items-center justify-between">
      <h1 class="text-2xl font-semibold">{{ $t('settings.title') }}</h1>
      <RouterLink
        class="text-sm underline"
        :to="{ name: 'home' }"
      >
        {{ $t('settings.back') }}
      </RouterLink>
    </div>

    <section class="grid gap-3">
      <h2 class="text-lg font-medium">{{ $t('settings.appearance') }}</h2>
      <div class="flex gap-2">
        <Button
          v-for="mode in themes"
          :key="mode"
          :variant="theme === mode ? 'default' : 'outline'"
          :aria-pressed="theme === mode"
          @click="theme = mode"
        >
          {{ $t(`settings.theme.${mode}`) }}
        </Button>
      </div>
    </section>

    <section class="grid gap-3">
      <h2 class="text-lg font-medium">{{ $t('settings.account') }}</h2>
      <p class="text-sm text-muted-foreground">{{ session.profile?.email }}</p>
      <Button
        class="w-fit"
        variant="outline"
        @click="signOut"
      >
        {{ $t('settings.signOut') }}
      </Button>
      <p
        v-if="signOutError"
        role="alert"
        class="text-sm text-destructive"
      >
        {{ signOutError }}
      </p>
    </section>
  </div>
</template>
