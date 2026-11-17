<script setup lang="ts">
import { EditorContent, useEditor, type JSONContent } from '@tiptap/vue-3'
import { ref, watch, provide } from 'vue'
import { toast } from 'vue-sonner'
import { useI18n } from 'vue-i18n'
import { type RichText, attachmentRepository } from '@/data'
import type { EditorChanges } from '../composables/useAutosave'
import EditorToolbar from './EditorToolbar.vue'
import { createExtensions } from './extensions'
import { MAX_IMAGE_BYTES, prepareImage } from '@/lib/image'
import { noteIdKey } from './note-id'
import './editor.css'

const TITLE_MAX_LENGTH = 200

const props = defineProps<{
  title: string
  content: RichText
  autofocus?: boolean
  noteId: string
}>()

const emit = defineEmits<{
  change: [changes: EditorChanges]
  blur: []
}>()

const { t } = useI18n()

const title = ref(props.title)
let applyingRemote = false

watch(
  title,
  (value) => {
    if (!applyingRemote) {
      emit('change', { title: value })
    }
  },
  { flush: 'sync' },
)

/** Creates the rich-text editor and reports document edits to the parent. */
const editor = useEditor({
  content: props.content as JSONContent,
  extensions: createExtensions(t('editor.placeholder')),
  autofocus: props.autofocus ? 'start' : false,
  editorProps: {
    attributes: { class: 'note-prose min-h-[60vh] outline-none' },
    scrollMargin: { top: 80, bottom: 80, left: 0, right: 0 },
    scrollThreshold: { top: 80, bottom: 95, left: 0, right: 0 },
    handlePaste: (_view, event) => takeImages(event.clipboardData?.files),
    handleDrop: (_view, event) => takeImages(event.dataTransfer?.files),
  },
  onUpdate: ({ editor }) =>
    emit('change', {
      content: editor.getJSON(),
      contentText: editor.getText({ blockSeparator: '\n' }),
    }),
  onBlur: () => emit('blur'),
})

provide(noteIdKey, props.noteId)

/** Compresses the images, stores them locally and inserts them at the cursor. */
async function insertImages(files: File[]): Promise<void> {
  const nodes = []

  for (const file of files) {
    try {
      const blob = await prepareImage(file)

      if (blob.size > MAX_IMAGE_BYTES) {
        toast.error(t('editor.imageTooLarge'))
        continue
      }

      const attachment = await attachmentRepository.add(props.noteId, blob)

      nodes.push({ type: 'image', attrs: { attachmentId: attachment.id } })
    } catch {
      toast.error(t('editor.imageFailed'))
    }
  }

  if (nodes.length > 0) {
    editor.value?.chain().focus().insertContent(nodes).run()
  }
}

/** Paste and drop: handles the event only when it carries images. */
function takeImages(files: FileList | null | undefined): boolean {
  const images = Array.from(files ?? []).filter((file) => file.type.startsWith('image/'))

  if (images.length === 0) {
    return false
  }

  void insertImages(images)

  return true
}

/** Replaces title and document with a newer version from elsewhere, keeping the caret in place. */
function applyRemote(remoteTitle: string, document: RichText): void {
  const instance = editor.value

  if (!instance) {
    return
  }

  applyingRemote = true
  title.value = remoteTitle
  applyingRemote = false

  const { from } = instance.state.selection

  instance.commands.setContent(document as JSONContent, { emitUpdate: false })

  if (instance.isFocused) {
    instance.commands.setTextSelection(from)
  }
}

defineExpose({ applyRemote })
</script>

<template>
  <EditorToolbar
    v-if="editor"
    :editor="editor"
    @images="insertImages"
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

  <slot name="meta" />

  <EditorContent :editor="editor" />
</template>
