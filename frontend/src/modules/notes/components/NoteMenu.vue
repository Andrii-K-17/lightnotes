<script setup lang="ts">
import { MoreHorizontal, Pin, PinOff, RotateCcw, Trash2 } from '@lucide/vue'
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { toast } from 'vue-sonner'
import ConfirmDialog from '@/components/ConfirmDialog.vue'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { noteRepository, type NoteRecord } from '@/data'

const props = defineProps<{ note: NoteRecord }>()
const emit = defineEmits<{ trashed: [] }>()

const { t } = useI18n()
const confirmPurge = ref(false)
const trashed = computed(() => props.note.deletedAt !== null)

async function moveToTrash(): Promise<void> {
  const id = props.note.id

  await noteRepository.moveToTrash(id)
  emit('trashed')
  toast(t('notes.movedToTrash'), {
    action: {
      label: t('common.undo'),
      onClick: () => void noteRepository.restore(id),
    },
  })
}
</script>

<template>
  <DropdownMenu>
    <DropdownMenuTrigger as-child>
      <Button
        variant="ghost"
        size="icon"
        class="size-8 cursor-pointer"
        :aria-label="$t('notes.actions')"
      >
        <MoreHorizontal class="size-4" />
      </Button>
    </DropdownMenuTrigger>

    <DropdownMenuContent
      align="end"
      class="w-48"
    >
      <template v-if="!trashed">
        <DropdownMenuItem
          @select="noteRepository.setPinned(note.id, !note.pinned)"
          class="cursor-pointer"
        >
          <component
            :is="note.pinned ? PinOff : Pin"
            class="mr-2 size-4"
          />
          {{ note.pinned ? $t('notes.unpin') : $t('notes.pin') }}
        </DropdownMenuItem>

        <DropdownMenuSeparator />

        <DropdownMenuItem
          class="text-destructive focus:text-destructive cursor-pointer"
          @select="moveToTrash"
        >
          <Trash2 class="mr-2 size-4" />
          {{ $t('notes.delete') }}
        </DropdownMenuItem>
      </template>

      <template v-else>
        <DropdownMenuItem @select="noteRepository.restore(note.id)">
          <RotateCcw class="mr-2 size-4" />
          {{ $t('notes.restore') }}
        </DropdownMenuItem>

        <DropdownMenuSeparator />

        <DropdownMenuItem
          class="text-destructive focus:text-destructive"
          @select="confirmPurge = true"
        >
          <Trash2 class="mr-2 size-4" />
          {{ $t('notes.deleteForever') }}
        </DropdownMenuItem>
      </template>
    </DropdownMenuContent>
  </DropdownMenu>

  <ConfirmDialog
    v-model:open="confirmPurge"
    :title="$t('notes.deleteForeverTitle')"
    :description="$t('notes.deleteForeverDescription')"
    :confirm-label="$t('notes.deleteForever')"
    :cancel-label="$t('common.cancel')"
    @confirm="noteRepository.purge(note.id)"
  />
</template>
