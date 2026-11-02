<script setup lang="ts">
import { Folder, MoreHorizontal, Pencil, Plus, Trash2 } from '@lucide/vue'
import { ref } from 'vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { Input } from '@/components/ui/input'
import { FOLDER_NAME_MAX_LENGTH, folderRepository, type FolderRecord } from '@/data'
import { useFolders } from '../composables/useFolders'
import { isSameScope, useNotesView } from '../stores/view'
import SidebarItem from './SidebarItem.vue'

const emit = defineEmits<{ navigate: [] }>()

const view = useNotesView()
const folders = useFolders()

const editing = ref<string | null>(null)
const draft = ref('')

const deleteOpen = ref(false)
const deleteName = ref('')
let deleteTarget: FolderRecord | undefined

const vFocus = {
  mounted: (element: HTMLElement) => {
    element.focus()

    if (element instanceof HTMLInputElement) {
      element.select()
    }
  },
}

function select(folder: FolderRecord): void {
  view.select({ kind: 'folder', id: folder.id })
  emit('navigate')
}

function startAdd(): void {
  draft.value = ''
  editing.value = 'new'
}

function startRename(folder: FolderRecord): void {
  draft.value = folder.name
  editing.value = folder.id
}

async function commit(): Promise<void> {
  const id = editing.value

  editing.value = null

  if (id === 'new') {
    const created = await folderRepository.create(draft.value)

    if (created) {
      select(created)
    }
  } else if (id) {
    await folderRepository.rename(id, draft.value)
  }
}

function cancel(): void {
  editing.value = null
}

function requestDelete(folder: FolderRecord): void {
  deleteTarget = folder
  deleteName.value = folder.name
  deleteOpen.value = true
}

async function confirmDelete(): Promise<void> {
  const folder = deleteTarget

  if (!folder) {
    return
  }

  await folderRepository.remove(folder.id)

  if (isSameScope(view.scope, { kind: 'folder', id: folder.id })) {
    view.select({ kind: 'all' })
  }
}
</script>

<template>
  <div class="mt-4">
    <div class="flex items-center justify-between px-3 pb-1">
      <h2 class="text-xs font-medium tracking-wide text-muted-foreground">
        {{ $t('sidebar.folders') }}
      </h2>
      <Button
        variant="ghost"
        size="icon"
        class="size-7"
        :aria-label="$t('folders.add')"
        @click="startAdd"
      >
        <Plus class="size-4" />
      </Button>
    </div>

    <template
      v-for="folder in folders"
      :key="folder.id"
    >
      <Input
        v-if="editing === folder.id"
        v-model="draft"
        v-focus
        class="my-1 h-9"
        :maxlength="FOLDER_NAME_MAX_LENGTH"
        :aria-label="$t('folders.namePlaceholder')"
        @keydown.enter.prevent="commit"
        @keydown.esc="cancel"
        @blur="commit"
      />
      <div
        v-else
        class="group relative"
      >
        <SidebarItem
          :active="isSameScope(view.scope, { kind: 'folder', id: folder.id })"
          @click="select(folder)"
        >
          <template #icon>
            <Folder class="size-4" />
          </template>
          {{ folder.name }}
        </SidebarItem>

        <DropdownMenu>
          <DropdownMenuTrigger as-child>
            <Button
              variant="ghost"
              size="icon"
              class="absolute right-1 top-1/2 size-7 -translate-y-1/2"
              :aria-label="$t('folders.actions')"
            >
              <MoreHorizontal class="size-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent
            align="end"
            class="w-40"
          >
            <DropdownMenuItem @select="startRename(folder)">
              <Pencil class="mr-2 size-4" />
              {{ $t('folders.rename') }}
            </DropdownMenuItem>
            <DropdownMenuItem
              class="text-destructive focus:text-destructive"
              @select="requestDelete(folder)"
            >
              <Trash2 class="mr-2 size-4" />
              {{ $t('folders.delete') }}
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </template>

    <Input
      v-if="editing === 'new'"
      v-model="draft"
      v-focus
      class="my-1 h-9"
      :maxlength="FOLDER_NAME_MAX_LENGTH"
      :placeholder="$t('folders.namePlaceholder')"
      :aria-label="$t('folders.namePlaceholder')"
      @keydown.enter.prevent="commit"
      @keydown.esc="cancel"
      @blur="commit"
    />

    <ConfirmDialog
      v-model:open="deleteOpen"
      :title="$t('folders.deleteTitle', { name: deleteName })"
      :description="$t('folders.deleteDescription')"
      :confirm-label="$t('folders.delete')"
      :cancel-label="$t('common.cancel')"
      @confirm="confirmDelete"
    />
  </div>
</template>
