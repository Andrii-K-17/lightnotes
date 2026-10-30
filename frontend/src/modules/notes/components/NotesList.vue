<script setup lang="ts">
import { FileText, Menu, Plus } from '@lucide/vue'
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet'
import { noteRepository } from '@/data'
import { useLiveQuery } from '@/lib/live-query'
import { useNewNote } from '../composables/useNewNote'
import { useNotesView } from '../stores/view'
import NoteCard from './NoteCard.vue'
import NotesSidebar from './NotesSidebar.vue'

const { t } = useI18n()
const view = useNotesView()
const { createNote } = useNewNote()
const drawerOpen = ref(false)

const { data: notes } = useLiveQuery(() => noteRepository.list(view.scope), [() => view.scope])

const title = computed(() => (view.scope.kind === 'folder' ? '' : t(`sidebar.${view.scope.kind}`)))
</script>

<template>
  <div class="relative flex h-full flex-col">
    <header class="flex items-center gap-2 px-4 pb-3 pt-5">
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
    </header>

    <div class="flex-1 overflow-y-auto px-4 pb-28">
      <div
        v-if="!notes"
        class="grid grid-cols-2 gap-3 sm:grid-cols-1"
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
          {{ $t(`notes.empty.${view.scope.kind}`) }}
        </p>
        <p
          v-if="view.scope.kind === 'all' || view.scope.kind === 'folder'"
          class="text-sm text-muted-foreground"
        >
          {{ $t('notes.empty.hint') }}
        </p>
      </div>

      <div
        v-else
        class="grid grid-cols-2 gap-3 sm:grid-cols-1"
      >
        <NoteCard
          v-for="note in notes"
          :key="note.id"
          :note="note"
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
