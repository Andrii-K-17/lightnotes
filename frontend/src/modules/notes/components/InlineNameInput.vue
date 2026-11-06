<script setup lang="ts">
import { ref } from 'vue'
import { Input } from '@/components/ui/input'

const props = defineProps<{
  initial?: string
  maxLength: number
  placeholder: string
}>()
const emit = defineEmits<{
  commit: [name: string]
  cancel: []
}>()

const name = ref(props.initial ?? '')
let finished = false

function finish(commit: boolean): void {
  if (finished) {
    return
  }

  finished = true

  if (commit) {
    emit('commit', name.value)
  } else {
    emit('cancel')
  }
}

const vFocus = {
  mounted: (element: HTMLElement) => {
    element.focus()

    if (element instanceof HTMLInputElement) {
      element.select()
    }
  },
}
</script>

<template>
  <Input
    v-model="name"
    v-focus
    class="my-1 h-9"
    :maxlength="maxLength"
    :placeholder="placeholder"
    :aria-label="placeholder"
    @keydown.enter.prevent="finish(true)"
    @keydown.esc="finish(false)"
    @blur="finish(true)"
  />
</template>
