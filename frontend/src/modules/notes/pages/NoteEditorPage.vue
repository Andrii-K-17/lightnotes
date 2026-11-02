<script setup lang="ts">
import { ArrowLeft } from '@lucide/vue'
import { onBeforeUnmount, ref, watch } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { Button } from '@/components/ui/button'
import { noteRepository, type NoteRecord } from '@/data'
import { useAutosave } from '../composables/useAutosave'
import NoteEditor from '../editor/NoteEditor.vue'
import { useLiveQuery } from '@/lib/live-query'
import NoteMenu from '../components/NoteMenu.vue'

const props = defineProps<{ id: string }>()

const router = useRouter()
const note = ref<NoteRecord>()
const { schedule, flush } = useAutosave()
const { data: liveNote } = useLiveQuery(() => noteRepository.get(props.id), [() => props.id])

async function leave(id: string): Promise<void> {
  await flush()
  await noteRepository.discardIfEmpty(id)
}

watch(
  () => props.id,
  async (id, previousId) => {
    if (previousId) {
      await leave(previousId)
    }

    note.value = undefined
    const found = await noteRepository.get(id)

    if (!found || found.deletedAt !== null) {
      await router.replace({ name: 'home' })
      return
    }

    note.value = found
  },
  { immediate: true },
)

onBeforeUnmount(() => void leave(props.id))
</script>

<template>
  <div class="mx-auto max-w-3xl px-4 pb-24 pt-4">
    <header class="mb-2 flex items-center justify-between">
      <Button
        as-child
        variant="ghost"
        size="icon"
        :aria-label="$t('common.back')"
      >
        <RouterLink :to="{ name: 'home' }">
          <ArrowLeft />
        </RouterLink>
      </Button>
      <NoteMenu
        v-if="liveNote"
        :note="liveNote"
        @trashed="router.replace({ name: 'home' })"
      />
    </header>

    <NoteEditor
      v-if="note"
      :key="note.id"
      :title="note.title"
      :content="note.content"
      :autofocus="note.title === '' && note.contentText === ''"
      @change="(changes) => schedule(note!.id, changes)"
      @blur="flush"
    />
  </div>
</template>
