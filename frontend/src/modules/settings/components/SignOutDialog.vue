<script setup lang="ts">
import { ref } from 'vue'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { pendingChanges } from '@/data'
import { useLiveQuery } from '@/lib/live-query'

const open = defineModel<boolean>('open', { required: true })
const emit = defineEmits<{ confirm: [wipe: boolean] }>()

const wipe = ref(true)
const { data: pending } = useLiveQuery(pendingChanges)
</script>

<template>
  <AlertDialog v-model:open="open">
    <AlertDialogContent>
      <AlertDialogHeader>
        <AlertDialogTitle>
          {{ $t('settings.signOutTitle') }}
        </AlertDialogTitle>
        <AlertDialogDescription>
          {{ $t('settings.signOutDescription') }}
        </AlertDialogDescription>
      </AlertDialogHeader>

      <label class="flex items-start gap-3 text-sm">
        <input
          v-model="wipe"
          type="checkbox"
          class="mt-0.5 size-4 accent-primary"
        />
        <span>
          {{ $t('settings.wipe') }}
        </span>
      </label>
      <p
        v-if="wipe && pending"
        role="alert"
        class="text-sm text-destructive"
      >
        {{ $t('settings.unsynced', pending) }}
      </p>

      <AlertDialogFooter>
        <AlertDialogCancel>
          {{ $t('common.cancel') }}
        </AlertDialogCancel>
        <AlertDialogAction @click="emit('confirm', wipe)">
          {{ $t('settings.signOut') }}
        </AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>
</template>
