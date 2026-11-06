<script setup lang="ts">
import { X } from '@lucide/vue'
import { computed, ref } from 'vue'
import {
  MAX_TAGS_PER_NOTE,
  TAG_NAME_MAX_LENGTH,
  noteRepository,
  tagRepository,
  type TagRecord,
} from '@/data'
import { useTags } from '../composables/useTags'

const props = defineProps<{ noteId: string; tagIds: string[] }>()

const tags = useTags()
const draft = ref('')

const attached = computed(() =>
  props.tagIds
    .map((id) => tags.value?.find((tag) => tag.id === id))
    .filter((tag): tag is TagRecord => tag !== undefined),
)
const available = computed(() => (tags.value ?? []).filter((tag) => !props.tagIds.includes(tag.id)))
const full = computed(() => props.tagIds.length >= MAX_TAGS_PER_NOTE)

async function add(): Promise<void> {
  const tag = await tagRepository.ensure(draft.value)

  draft.value = ''

  if (tag) {
    await noteRepository.addTag(props.noteId, tag.id)
  }
}
</script>

<template>
  <div class="mb-3 flex flex-wrap items-center gap-2">
    <span
      v-for="tag in attached"
      :key="tag.id"
      class="inline-flex items-center gap-1 rounded-full bg-secondary px-3 py-1 text-xs text-secondary-foreground"
    >
      {{ tag.name }}
      <button
        type="button"
        class="rounded-full p-0.5 outline-none hover:bg-background/60 focus-visible:ring-2 focus-visible:ring-ring"
        :aria-label="$t('tags.remove', { name: tag.name })"
        @click="noteRepository.removeTag(noteId, tag.id)"
      >
        <X class="size-3" />
      </button>
    </span>

    <input
      v-model="draft"
      list="note-tag-suggestions"
      class="min-w-24 flex-1 bg-transparent text-sm outline-none placeholder:text-muted-foreground/60 disabled:cursor-not-allowed"
      :maxlength="TAG_NAME_MAX_LENGTH"
      :disabled="full"
      :placeholder="full ? $t('tags.limit') : $t('tags.addToNote')"
      :aria-label="$t('tags.addToNote')"
      @keydown.enter.prevent="add"
    />
    <datalist id="note-tag-suggestions">
      <option
        v-for="tag in available"
        :key="tag.id"
        :value="tag.name"
      />
    </datalist>
  </div>
</template>
