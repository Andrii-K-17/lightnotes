<script setup lang="ts">
import { MoreHorizontal, Pencil, Plus, Tag, Trash2 } from '@lucide/vue'
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { toast } from 'vue-sonner'
import ConfirmDialog from '@/components/ConfirmDialog.vue'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { TAG_NAME_MAX_LENGTH, tagRepository, type TagRecord } from '@/data'
import { useTags } from '../composables/useTags'
import { useNotesView } from '../stores/view'
import InlineNameInput from './InlineNameInput.vue'
import SidebarItem from './SidebarItem.vue'

const { t } = useI18n()
const view = useNotesView()
const tags = useTags()

/** "new" while adding, a tag id while renaming, otherwise null. */
const editing = ref<string | null>(null)

const deleteOpen = ref(false)
const deleteName = ref('')
let deleteTarget: TagRecord | undefined

async function addTag(name: string): Promise<void> {
  editing.value = null
  await tagRepository.ensure(name)
}

async function renameTag(id: string, name: string): Promise<void> {
  editing.value = null

  if ((await tagRepository.rename(id, name)) === 'duplicate') {
    toast.error(t('tags.duplicate'))
  }
}

function requestDelete(tag: TagRecord): void {
  deleteTarget = tag
  deleteName.value = tag.name
  deleteOpen.value = true
}

async function confirmDelete(): Promise<void> {
  if (deleteTarget) {
    await tagRepository.remove(deleteTarget.id)
  }
}
</script>

<template>
  <div class="mt-4">
    <div class="flex items-center justify-between px-3 pb-1">
      <h2 class="text-xs font-medium tracking-wide text-muted-foreground">
        {{ $t('sidebar.tags') }}
      </h2>
      <Button
        variant="ghost"
        size="icon"
        class="size-7"
        :aria-label="$t('tags.add')"
        @click="editing = 'new'"
      >
        <Plus class="size-4" />
      </Button>
    </div>

    <template
      v-for="tag in tags"
      :key="tag.id"
    >
      <InlineNameInput
        v-if="editing === tag.id"
        :initial="tag.name"
        :max-length="TAG_NAME_MAX_LENGTH"
        :placeholder="$t('tags.namePlaceholder')"
        @commit="(name) => renameTag(tag.id, name)"
        @cancel="editing = null"
      />
      <div
        v-else
        class="relative"
      >
        <SidebarItem
          :active="view.tagIds.includes(tag.id)"
          :aria-pressed="view.tagIds.includes(tag.id)"
          @click="view.toggleTag(tag.id)"
        >
          <template #icon><Tag class="size-4" /></template>
          {{ tag.name }}
        </SidebarItem>

        <DropdownMenu>
          <DropdownMenuTrigger as-child>
            <Button
              variant="ghost"
              size="icon"
              class="absolute right-1 top-1/2 size-7 -translate-y-1/2"
              :aria-label="$t('tags.actions')"
            >
              <MoreHorizontal class="size-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent
            align="end"
            class="w-40"
          >
            <DropdownMenuItem @select="editing = tag.id">
              <Pencil class="mr-2 size-4" />
              {{ $t('tags.rename') }}
            </DropdownMenuItem>
            <DropdownMenuItem
              class="text-destructive focus:text-destructive"
              @select="requestDelete(tag)"
            >
              <Trash2 class="mr-2 size-4" />
              {{ $t('tags.delete') }}
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </template>

    <InlineNameInput
      v-if="editing === 'new'"
      :max-length="TAG_NAME_MAX_LENGTH"
      :placeholder="$t('tags.namePlaceholder')"
      @commit="addTag"
      @cancel="editing = null"
    />

    <ConfirmDialog
      v-model:open="deleteOpen"
      :title="$t('tags.deleteTitle', { name: deleteName })"
      :description="$t('tags.deleteDescription')"
      :confirm-label="$t('tags.delete')"
      :cancel-label="$t('common.cancel')"
      @confirm="confirmDelete"
    />
  </div>
</template>
