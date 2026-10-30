<script setup lang="ts">
import { EditorContent, useEditor, type JSONContent } from '@tiptap/vue-3'
import { useI18n } from 'vue-i18n'
import type { RichText } from '@/data'
import { createExtensions } from './extensions'
import EditorToolbar from './EditorToolbar.vue'
import './editor.css'

const props = defineProps<{
  content: RichText
  autofocus?: boolean
}>()

const emit = defineEmits<{
  change: [content: RichText, text: string]
  blur: []
}>()

const { t } = useI18n()

const editor = useEditor({
  content: props.content as JSONContent,
  extensions: createExtensions(t('editor.placeholder')),
  autofocus: props.autofocus ? 'start' : false,
  editorProps: { attributes: { class: 'note-prose min-h-[60vh] outline-none' } },
  onUpdate: ({ editor }) => {
    emit('change', editor.getJSON(), editor.getText({ blockSeparator: '\n' }))
  },
  onBlur: () => emit('blur'),
})
</script>

<template>
  <EditorToolbar
    v-if="editor"
    :editor="editor"
  />
  <EditorContent
    :editor="editor"
    class="pt-3"
  />
</template>
