<script setup lang="ts">
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

defineProps<{
  title: string
  description: string
  confirmLabel: string
  cancelLabel: string
}>()

const open = defineModel<boolean>('open', { required: true })
const emit = defineEmits<{ confirm: [] }>()
</script>

<template>
  <AlertDialog v-model:open="open">
    <AlertDialogContent>
      <AlertDialogHeader>
        <AlertDialogTitle>
          {{ title }}
        </AlertDialogTitle>

        <AlertDialogDescription>
          {{ description }}
        </AlertDialogDescription>
      </AlertDialogHeader>

      <AlertDialogFooter>
        <AlertDialogCancel>
          {{ cancelLabel }}
        </AlertDialogCancel>

        <AlertDialogAction @click="emit('confirm')">
          {{ confirmLabel }}
        </AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>
</template>
