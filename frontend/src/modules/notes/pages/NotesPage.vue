<script setup lang="ts">
import { RouterLink, useRouter } from 'vue-router'
import { Button } from '@/components/ui/button'
import { noteRepository } from '@/data'
import { FileText, Plus, Settings } from '@lucide/vue'
import { useLiveQuery } from '@/lib/live-query'
import NoteCard from '../components/NoteCard.vue'

const { data: notes } = useLiveQuery(() => noteRepository.listActive())

const router = useRouter()

async function createNote(): Promise<void> {
  const note = await noteRepository.create()
  await router.push({ name: 'note', params: { id: note.id } })
}
</script>

<template>
  <div class="mx-auto max-w-5xl px-4 pb-28 pt-6">
    <header class="mb-5 flex items-center justify-between">
      <h1 class="text-3xl font-semibold tracking-tight">
        {{ $t('notes.title') }}
      </h1>
      <Button
        as-child
        variant="ghost"
        size="icon"
        :aria-label="$t('settings.title')"
      >
        <RouterLink :to="{ name: 'settings' }">
          <Settings />
        </RouterLink>
      </Button>
    </header>

    <div
      v-if="!notes"
      class="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4"
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
      class="flex flex-col items-center gap-3 py-24 text-center"
    >
      <FileText class="size-12 text-muted-foreground/60" />
      <p class="text-lg font-medium">
        {{ $t('notes.empty.title') }}
      </p>
      <p class="text-sm text-muted-foreground">
        {{ $t('notes.empty.hint') }}
      </p>
    </div>

    <div
      v-else
      class="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4"
    >
      <NoteCard
        v-for="note in notes"
        :key="note.id"
        :note="note"
      />
    </div>

    <Button
      size="icon"
      class="fixed bottom-6 right-6 size-14 rounded-full shadow-lg"
      :aria-label="$t('notes.new')"
      @click="createNote"
    >
      <Plus class="size-6" />
    </Button>
  </div>
</template>
