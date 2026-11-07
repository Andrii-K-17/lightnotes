<script setup lang="ts">
import { ArrowDownUp, FileText, LayoutGrid, List, Menu, Plus, Search, X } from '@lucide/vue'
import { refDebounced, useEventListener } from '@vueuse/core'
import { computed, ref, toRef, useTemplateRef } from 'vue'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { Input } from '@/components/ui/input'
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet'
import { noteRepository, type NoteSort } from '@/data'
import { useLiveQuery } from '@/lib/live-query'
import { useFolders } from '../composables/useFolders'
import { useNewNote } from '../composables/useNewNote'
import { useTags } from '../composables/useTags'
import { useNotesView } from '../stores/view'
import NoteCard from './NoteCard.vue'
import NotesSidebar from './NotesSidebar.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'

const { t } = useI18n()
const view = useNotesView()
const folders = useFolders()
const tags = useTags()
const { createNote } = useNewNote()

const drawerOpen = ref(false)
const confirmEmpty = ref(false)
const searchInput = useTemplateRef<InstanceType<typeof Input>>('searchInput')

const searchText = refDebounced(toRef(view, 'query'), 150)

const { data: notes } = useLiveQuery(
  () =>
    noteRepository.query({
      scope: view.scope,
      tagIds: view.tagIds,
      text: searchText.value,
      sort: view.sort,
    }),
  [() => view.scope, () => view.tagIds, searchText, () => view.sort],
)

const title = computed(() => {
  const scope = view.scope

  return scope.kind === 'folder'
    ? (folders.value?.find((folder) => folder.id === scope.id)?.name ?? '')
    : t(`sidebar.${scope.kind}`)
})

const selectedTags = computed(() =>
  view.tagIds
    .map((id) => tags.value?.find((tag) => tag.id === id))
    .filter((tag) => tag !== undefined),
)

const filtering = computed(() => searchText.value.trim() !== '' || view.tagIds.length > 0)

const sortBy = computed({
  get: () => view.sort.by as string,
  set: (by: string) => (view.sort = { ...view.sort, by: by as NoteSort['by'] }),
})
const sortDirection = computed({
  get: () => view.sort.direction as string,
  set: (direction: string) =>
    (view.sort = { ...view.sort, direction: direction as NoteSort['direction'] }),
})

useEventListener(window, 'keydown', (event) => {
  if (event.code === 'KeyK' && (event.ctrlKey || event.metaKey)) {
    event.preventDefault()
    searchInput.value?.$el.focus()
  }
})
</script>

<template>
  <div class="relative flex h-full flex-col">
    <header class="flex items-center gap-1 px-4 pb-3 pt-5">
      <Button
        variant="ghost"
        size="icon"
        class="-ml-2 lg:hidden"
        :aria-label="$t('sidebar.open')"
        @click="drawerOpen = true"
      >
        <Menu />
      </Button>
      <h1 class="min-w-0 flex-1 truncate text-2xl font-semibold tracking-tight">
        {{ title }}
      </h1>

      <Button
        variant="ghost"
        size="icon"
        :aria-label="view.viewMode === 'grid' ? $t('view.list') : $t('view.grid')"
        @click="view.viewMode = view.viewMode === 'grid' ? 'list' : 'grid'"
      >
        <List v-if="view.viewMode === 'grid'" />
        <LayoutGrid v-else />
      </Button>

      <DropdownMenu>
        <DropdownMenuTrigger as-child>
          <Button
            variant="ghost"
            size="icon"
            :aria-label="$t('sort.label')"
          >
            <ArrowDownUp />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent
          align="end"
          class="w-48"
        >
          <DropdownMenuRadioGroup v-model="sortBy">
            <DropdownMenuRadioItem value="edited">
              {{ $t('sort.edited') }}
            </DropdownMenuRadioItem>
            <DropdownMenuRadioItem value="created">
              {{ $t('sort.created') }}
            </DropdownMenuRadioItem>
            <DropdownMenuRadioItem value="title">
              {{ $t('sort.title') }}
            </DropdownMenuRadioItem>
          </DropdownMenuRadioGroup>

          <DropdownMenuSeparator />

          <DropdownMenuRadioGroup v-model="sortDirection">
            <DropdownMenuRadioItem value="desc">
              {{ $t('sort.desc') }}
            </DropdownMenuRadioItem>
            <DropdownMenuRadioItem value="asc">
              {{ $t('sort.asc') }}
            </DropdownMenuRadioItem>
          </DropdownMenuRadioGroup>
        </DropdownMenuContent>
      </DropdownMenu>
    </header>

    <div class="px-4 pb-3">
      <div class="relative">
        <Search
          class="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground"
        />
        <Input
          ref="searchInput"
          v-model="view.query"
          type="text"
          class="rounded-full bg-muted/60 pl-10 pr-10"
          :placeholder="$t('search.placeholder')"
          :aria-label="$t('search.placeholder')"
          @keydown.esc="view.query = ''"
        />
        <Button
          v-if="view.query"
          variant="ghost"
          size="icon"
          class="absolute right-2 top-1/2 size-7 -translate-y-1/2 rounded-full"
          :aria-label="$t('search.clear')"
          @click="view.query = ''"
        >
          <X class="size-4" />
        </Button>
      </div>
    </div>

    <div
      v-if="selectedTags.length"
      class="flex flex-wrap items-center gap-2 px-4 pb-3"
    >
      <span
        v-for="tag in selectedTags"
        :key="tag.id"
        class="inline-flex items-center gap-1 rounded-full bg-accent px-3 py-1 text-xs text-accent-foreground"
      >
        {{ tag.name }}
        <button
          type="button"
          class="rounded-full p-0.5 outline-none hover:bg-background/60"
          :aria-label="$t('tags.remove', { name: tag.name })"
          @click="view.toggleTag(tag.id)"
        >
          <X class="size-3" />
        </button>
      </span>

      <button
        type="button"
        class="text-xs text-muted-foreground underline"
        @click="view.clearTags()"
      >
        {{ $t('tags.clearFilter') }}
      </button>
    </div>

    <div
      v-if="view.scope.kind === 'trash'"
      class="flex items-center justify-between gap-3 px-4 pb-3 text-sm text-muted-foreground"
    >
      <span>
        {{ $t('notes.trashHint') }}
      </span>
      <Button
        variant="outline"
        size="sm"
        :disabled="!notes?.length"
        @click="confirmEmpty = true"
      >
        {{ $t('notes.emptyTrash') }}
      </Button>
    </div>

    <div class="flex-1 overflow-y-auto px-4 pb-28">
      <div
        v-if="!notes"
        class="grid gap-3"
        :class="view.viewMode === 'grid' ? 'grid-cols-2' : 'grid-cols-1'"
        aria-hidden="true"
      >
        <div
          v-for="n in 8"
          :key="n"
          class="h-36 animate-pulse rounded-2xl bg-muted"
        />
      </div>

      <div
        v-else-if="notes.length === 0"
        class="flex flex-col items-center gap-3 py-20 text-center"
      >
        <FileText class="size-12 text-muted-foreground/60" />
        <p class="text-lg font-medium">
          {{ filtering ? $t('notes.noResults') : $t(`notes.empty.${view.scope.kind}`) }}
        </p>
        <p
          v-if="!filtering && (view.scope.kind === 'all' || view.scope.kind === 'folder')"
          class="text-sm text-muted-foreground"
        >
          {{ $t('notes.empty.hint') }}
        </p>
      </div>

      <div
        v-else
        class="grid gap-3"
        :class="view.viewMode === 'grid' ? 'grid-cols-2' : 'grid-cols-1'"
      >
        <NoteCard
          v-for="note in notes"
          :key="note.id"
          :note="note"
          :compact="view.viewMode === 'list'"
        />
      </div>
    </div>

    <Button
      v-if="view.scope.kind !== 'trash'"
      size="icon"
      class="absolute bottom-6 right-6 size-14 rounded-full shadow-lg"
      :aria-label="$t('notes.new')"
      @click="createNote"
    >
      <Plus class="size-6" />
    </Button>

    <ConfirmDialog
      v-model:open="confirmEmpty"
      :title="$t('notes.emptyTrashTitle')"
      :description="$t('notes.emptyTrashDescription')"
      :confirm-label="$t('notes.emptyTrash')"
      :cancel-label="$t('common.cancel')"
      @confirm="noteRepository.emptyTrash()"
    />

    <Sheet v-model:open="drawerOpen">
      <SheetContent
        side="left"
        class="w-72 p-0"
      >
        <SheetHeader class="sr-only">
          <SheetTitle>
            {{ $t('sidebar.title') }}
          </SheetTitle>

          <SheetDescription>
            {{ $t('sidebar.title') }}
          </SheetDescription>
        </SheetHeader>

        <NotesSidebar @navigate="drawerOpen = false" />
      </SheetContent>
    </Sheet>
  </div>
</template>
