<script setup lang="ts">
import { EditorContent, useEditor, type JSONContent } from '@tiptap/vue-3'
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import type { RichText } from '@/data'
import EditorToolbar from './EditorToolbar.vue'
import { createExtensions } from './extensions'
import './editor.css'

const TITLE_MAX_LENGTH = 200

const props = defineProps<{
  title: string
  content: RichText
  autofocus?: boolean
}>()

const emit = defineEmits<{
  change: [changes: { title?: string; content?: RichText; contentText?: string }]
  blur: []
}>()

const { t } = useI18n()

const title = ref(props.title)
watch(title, (value) => emit('change', { title: value }))

const editor = useEditor({
  content: props.content as JSONContent,
  extensions: createExtensions(t('editor.placeholder')),
  autofocus: props.autofocus ? 'start' : false,
  editorProps: {
    attributes: { class: 'note-prose min-h-[60vh] outline-none' },
    scrollMargin: { top: 80, bottom: 80, left: 0, right: 0 },
    scrollThreshold: { top: 80, bottom: 95, left: 0, right: 0 },
  },
  onUpdate: ({ editor }) =>
    emit('change', {
      content: editor.getJSON(),
      contentText: editor.getText({ blockSeparator: '\n' }),
    }),
  onBlur: () => emit('blur'),
})
</script>

<template>
  <EditorToolbar
    v-if="editor"
    :editor="editor"
  />

  <input
    v-model="title"
    class="mb-2 mt-4 w-full bg-transparent text-2xl font-semibold tracking-tight outline-none placeholder:text-muted-foreground/60"
    :maxlength="TITLE_MAX_LENGTH"
    :placeholder="$t('editor.titlePlaceholder')"
    :aria-label="$t('editor.titlePlaceholder')"
    @keydown.enter.prevent="editor?.commands.focus('start')"
    @blur="emit('blur')"
  />
  <EditorContent :editor="editor" />
</template>
