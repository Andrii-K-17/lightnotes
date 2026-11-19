<script setup lang="ts">
import { ArrowLeft, ChevronLeft, Sparkles, X } from '@lucide/vue'
import { computed, onBeforeUnmount, ref, useTemplateRef, watch } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { Button } from '@/components/ui/button'
import { noteRepository, parseContent, type NoteRecord } from '@/data'
import { useAutosave } from '../composables/useAutosave'
import NoteEditor from '../editor/NoteEditor.vue'
import { useLiveQuery } from '@/lib/live-query'
import NoteMenu from '../components/NoteMenu.vue'
import NoteTags from '../editor/NoteTags.vue'
import { AiPanel } from '@/modules/ai'
import { useNotesView } from '../stores/view'

const props = defineProps<{ id: string }>()

const router = useRouter()
const view = useNotesView()
const note = ref<NoteRecord>()
const editor = useTemplateRef<InstanceType<typeof NoteEditor>>('editor')

const aiOpen = ref(false)
const getAiInput = () => editor.value?.aiInput() ?? { text: '', selection: false }
const applyAi = (text: string, mode: 'replace' | 'insert') => editor.value?.applyAi(text, mode)

function closeEditor(): void {
  view.editorExpanded = false
  view.middleHidden = false
  void router.replace({ name: 'home' })
}

function expandEditor(): void {
  view.editorExpanded = true
  view.middleHidden = true
}

function shrinkEditor(): void {
  view.editorExpanded = false
  view.middleHidden = false
}

/** The title and content (JSON string) that the editor currently reflects. */
const known = { title: '', content: '' }
/** Server version that the editor's text derives from. */
let baseVersion = 0

const { schedule, flush, hasPending } = useAutosave({
  onWrite: ({ title, content }) => {
    if (title !== undefined) {
      known.title = title
    }

    if (content !== undefined) {
      known.content = content
    }
  },
})

const initialContent = computed(() => (note.value ? parseContent(note.value.content) : undefined))

/** Saves pending edits, then drops the note if it was left empty. */
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

    known.title = found.title
    known.content = found.content
    baseVersion = found.serverVersion
    note.value = found
  },
  { immediate: true },
)

const { data: liveNote } = useLiveQuery(() => noteRepository.get(props.id), [() => props.id])

watch(liveNote, (live) => {
  if (!note.value) {
    return
  }

  if (!live || live.purgedAt) {
    void router.replace({ name: 'home' })
    return
  }

  if (live.id !== props.id || live.id !== note.value.id) {
    return
  }

  if (live.title === known.title && live.content === known.content) {
    baseVersion = live.serverVersion
    return
  }

  if (hasPending()) {
    return
  }

  known.title = live.title
  known.content = live.content
  baseVersion = live.serverVersion
  editor.value?.applyRemote(live.title, parseContent(live.content))
})

onBeforeUnmount(() => void leave(props.id))
</script>

<template>
  <div class="mx-auto max-w-5xl px-4 pb-24 pt-4">
    <header class="mb-2 flex items-center justify-between">
      <Button
        as-child
        variant="ghost"
        size="icon"
        class="sm:hidden"
        :aria-label="$t('common.back')"
      >
        <RouterLink :to="{ name: 'home' }">
          <ArrowLeft />
        </RouterLink>
      </Button>

      <div>
        <Button
          variant="ghost"
          size="icon"
          class="max-lg:hidden"
          :aria-label="$t(view.editorExpanded ? 'view.shrinkEditor' : 'view.expandEditor')"
          :title="$t(view.editorExpanded ? 'view.shrinkEditor' : 'view.expandEditor')"
          @click="view.editorExpanded ? shrinkEditor() : expandEditor()"
        >
          <ChevronLeft
            class="transition-transform duration-300 ease-in-out"
            :class="{ 'rotate-180': view.editorExpanded }"
          />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          class="max-lg:hidden"
          :aria-label="$t('view.closeEditor')"
          :title="$t('view.closeEditor')"
          @click="closeEditor"
        >
          <X />
        </Button>
      </div>

      <div class="flex">
        <Button
          variant="ghost"
          size="icon"
          :aria-label="$t('ai.open')"
          @click="aiOpen = true"
        >
          <Sparkles />
        </Button>
        <NoteMenu
          v-if="liveNote"
          :note="liveNote"
          @trashed="router.replace({ name: 'home' })"
        />
      </div>
    </header>

    <NoteEditor
      v-if="note && initialContent"
      ref="editor"
      :key="note.id"
      :title="note.title"
      :content="initialContent"
      :autofocus="note.title === '' && note.contentText === ''"
      @change="(changes) => schedule(note!.id, changes, baseVersion)"
      @blur="flush"
      :note-id="note.id"
    >
      <template #meta>
        <NoteTags
          :note-id="note.id"
          :tag-ids="liveNote?.tagIds ?? note.tagIds"
        />
      </template>
    </NoteEditor>

    <AiPanel
      v-model:open="aiOpen"
      :get-input="getAiInput"
      @apply="applyAi"
    />
  </div>
</template>
