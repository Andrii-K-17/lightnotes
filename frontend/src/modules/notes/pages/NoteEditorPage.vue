<script setup lang="ts">
import { ArrowLeft } from '@lucide/vue'
import { ref, watch } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { Button } from '@/components/ui/button'
import { noteRepository, type NoteRecord } from '@/data'
import NoteEditor from '../editor/NoteEditor.vue'

const props = defineProps<{ id: string }>()

const router = useRouter()
const note = ref<NoteRecord>()

watch(
  () => props.id,
  async (id) => {
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
</script>

<template>
  <div class="mx-auto max-w-3xl px-4 pb-24 pt-4">
    <header class="mb-3">
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
    </header>

    <NoteEditor
      v-if="note"
      :key="note.id"
      :content="note.content"
      :autofocus="note.title === '' && note.contentText === ''"
    />
  </div>
</template>
