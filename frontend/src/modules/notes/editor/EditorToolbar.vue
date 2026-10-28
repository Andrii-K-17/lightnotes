<script setup lang="ts">
import {
  Bold,
  Code,
  Heading1,
  Heading2,
  Heading3,
  Italic,
  Link as LinkIcon,
  Quote,
  Redo2,
  Strikethrough,
  Underline,
  Undo2,
} from '@lucide/vue'
import type { Editor } from '@tiptap/vue-3'
import { nextTick, ref, useTemplateRef, type Component } from 'vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'

const props = defineProps<{ editor: Editor }>()

interface Action {
  id: string
  icon: Component
  label: string
  active?: () => boolean
  disabled?: () => boolean
  run: () => void
}

const chain = () => props.editor.chain().focus()
const isActive = (name: string, attributes?: Record<string, unknown>) =>
  props.editor.isActive(name, attributes)

const groups: Action[][] = [
  [
    {
      id: 'undo',
      icon: Undo2,
      label: 'editor.undo',
      disabled: () => !props.editor.can().undo(),
      run: () => chain().undo().run(),
    },
    {
      id: 'redo',
      icon: Redo2,
      label: 'editor.redo',
      disabled: () => !props.editor.can().redo(),
      run: () => chain().redo().run(),
    },
  ],
  [
    {
      id: 'bold',
      icon: Bold,
      label: 'editor.bold',
      active: () => isActive('bold'),
      run: () => chain().toggleBold().run(),
    },
    {
      id: 'italic',
      icon: Italic,
      label: 'editor.italic',
      active: () => isActive('italic'),
      run: () => chain().toggleItalic().run(),
    },
    {
      id: 'underline',
      icon: Underline,
      label: 'editor.underline',
      active: () => isActive('underline'),
      run: () => chain().toggleUnderline().run(),
    },
    {
      id: 'strike',
      icon: Strikethrough,
      label: 'editor.strike',
      active: () => isActive('strike'),
      run: () => chain().toggleStrike().run(),
    },
  ],
  [
    {
      id: 'h1',
      icon: Heading1,
      label: 'editor.heading1',
      active: () => isActive('heading', { level: 1 }),
      run: () => chain().toggleHeading({ level: 1 }).run(),
    },
    {
      id: 'h2',
      icon: Heading2,
      label: 'editor.heading2',
      active: () => isActive('heading', { level: 2 }),
      run: () => chain().toggleHeading({ level: 2 }).run(),
    },
    {
      id: 'h3',
      icon: Heading3,
      label: 'editor.heading3',
      active: () => isActive('heading', { level: 3 }),
      run: () => chain().toggleHeading({ level: 3 }).run(),
    },
  ],
  [
    {
      id: 'quote',
      icon: Quote,
      label: 'editor.quote',
      active: () => isActive('blockquote'),
      run: () => chain().toggleBlockquote().run(),
    },
    {
      id: 'code',
      icon: Code,
      label: 'editor.code',
      active: () => isActive('code'),
      run: () => chain().toggleCode().run(),
    },
  ],
]

const linkOpen = ref(false)
const linkUrl = ref('')
const linkInput = useTemplateRef<InstanceType<typeof Input>>('linkInput')

async function openLink(): Promise<void> {
  linkUrl.value = (props.editor.getAttributes('link').href as string | undefined) ?? ''
  linkOpen.value = true

  await nextTick()
  linkInput.value?.$el.focus()
}

function applyLink(): void {
  const href = linkUrl.value.trim()

  if (href) {
    chain().extendMarkRange('link').setLink({ href }).run()
  } else {
    chain().extendMarkRange('link').unsetLink().run()
  }

  linkOpen.value = false
}

function removeLink(): void {
  chain().extendMarkRange('link').unsetLink().run()
  linkOpen.value = false
}
</script>

<template>
  <div class="sticky top-0 z-10 -mx-4 border-b bg-background/90 px-4 py-2 backdrop-blur">
    <form
      v-if="linkOpen"
      class="flex items-center gap-2"
      @submit.prevent="applyLink"
      @keydown.esc="linkOpen = false"
    >
      <Input
        ref="linkInput"
        v-model="linkUrl"
        inputmode="url"
        :placeholder="$t('editor.linkUrl')"
        :aria-label="$t('editor.linkUrl')"
      />
      <Button
        type="submit"
        size="sm"
      >
        {{ $t('editor.linkApply') }}
      </Button>
      <Button
        v-if="editor.isActive('link')"
        type="button"
        size="sm"
        variant="ghost"
        @click="removeLink"
      >
        {{ $t('editor.linkRemove') }}
      </Button>
    </form>

    <div
      v-else
      class="flex items-center gap-1 overflow-x-auto"
      role="toolbar"
      :aria-label="$t('editor.toolbar')"
    >
      <template
        v-for="(group, index) in groups"
        :key="index"
      >
        <span
          v-if="index > 0"
          class="mx-1 h-5 w-px shrink-0 bg-border"
          aria-hidden="true"
        />
        <Button
          v-for="action in group"
          :key="action.id"
          type="button"
          size="icon"
          :variant="action.active?.() ? 'secondary' : 'ghost'"
          :disabled="action.disabled?.()"
          :aria-label="$t(action.label)"
          :title="$t(action.label)"
          :aria-pressed="action.active ? action.active() : undefined"
          @mousedown.prevent
          @click="action.run()"
        >
          <component
            :is="action.icon"
            class="size-4"
          />
        </Button>
      </template>

      <span
        class="mx-1 h-5 w-px shrink-0 bg-border"
        aria-hidden="true"
      />
      <Button
        type="button"
        size="icon"
        :variant="editor.isActive('link') ? 'secondary' : 'ghost'"
        :disabled="editor.state.selection.empty && !editor.isActive('link')"
        :aria-label="$t('editor.link')"
        :title="$t('editor.link')"
        @mousedown.prevent
        @click="openLink"
      >
        <LinkIcon class="size-4" />
      </Button>
    </div>
  </div>
</template>
