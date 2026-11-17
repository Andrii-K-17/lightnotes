<script setup lang="ts">
import { ImageOff } from '@lucide/vue'
import { NodeViewWrapper, nodeViewProps } from '@tiptap/vue-3'
import { useAttachmentUrl } from './useAttachmentUrl'

const props = defineProps(nodeViewProps)

const { url, failed } = useAttachmentUrl(() => props.node.attrs.attachmentId as string | null)
</script>

<template>
  <NodeViewWrapper class="my-3">
    <img
      v-if="url"
      :src="url"
      :alt="(node.attrs.alt as string | null) ?? ''"
      class="max-h-128 max-w-full rounded-xl"
      :class="{ 'ring-2 ring-primary': selected }"
      draggable="false"
    />
    <div
      v-else
      class="flex h-32 items-center justify-center gap-2 rounded-xl bg-muted text-sm text-muted-foreground"
    >
      <ImageOff
        v-if="failed"
        class="size-4"
      />
      {{ failed ? $t('editor.imageUnavailable') : $t('editor.imageLoading') }}
    </div>
  </NodeViewWrapper>
</template>
