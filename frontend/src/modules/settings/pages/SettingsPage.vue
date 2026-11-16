<script setup lang="ts">
import { RouterLink, useRouter } from 'vue-router'
import { Button } from '@/components/ui/button'
import { useTheme } from '@/lib/theme'
import { useSessionStore } from '@/modules/auth'
import { ref } from 'vue'
import { errorMessage } from '@/i18n/errors'
import { ApiError } from '@/lib/api'
import SignOutDialog from '../components/SignOutDialog.vue'
import { useSyncStore } from '@/modules/sync'

const router = useRouter()
const session = useSessionStore()
const theme = useTheme()
const sync = useSyncStore()
const signOutOpen = ref(false)

const themes = ['auto', 'light', 'dark'] as const

const signOutError = ref<string | null>(null)

async function signOut(wipe: boolean): Promise<void> {
  signOutError.value = null

  try {
    if (wipe) {
      await sync.run()
    }

    await session.logout({ wipe })
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
      <h1 class="text-2xl font-semibold">
        {{ $t('settings.title') }}
      </h1>
      <RouterLink
        class="text-sm underline"
        :to="{ name: 'home' }"
      >
        {{ $t('settings.back') }}
      </RouterLink>
    </div>

    <section class="grid gap-3">
      <h2 class="text-lg font-medium">
        {{ $t('settings.appearance') }}
      </h2>
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
      <h2 class="text-lg font-medium">
        {{ $t('settings.account') }}
      </h2>
      <p class="text-sm text-muted-foreground">
        {{ session.profile?.email }}
      </p>
      <Button
        class="w-fit"
        variant="outline"
        @click="signOutOpen = true"
      >
        {{ $t('settings.signOut') }}
      </Button>

      <SignOutDialog
        v-model:open="signOutOpen"
        @confirm="signOut"
      />

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
