<script setup lang="ts">
import { FileText, Inbox, Pin, Settings, Trash2 } from '@lucide/vue'
import type { Component } from 'vue'
import type { NoteScope } from '@/data'
import { isSameScope, useNotesView } from '../stores/view'
import SidebarItem from './SidebarItem.vue'

const emit = defineEmits<{ navigate: [] }>()
const view = useNotesView()

const sections: {
  scope: NoteScope
  icon: Component
  label: string
}[] = [
  { scope: { kind: 'all' }, icon: FileText, label: 'sidebar.all' },
  { scope: { kind: 'pinned' }, icon: Pin, label: 'sidebar.pinned' },
  { scope: { kind: 'unfiled' }, icon: Inbox, label: 'sidebar.unfiled' },
  { scope: { kind: 'trash' }, icon: Trash2, label: 'sidebar.trash' },
]

function select(scope: NoteScope): void {
  view.select(scope)
  emit('navigate')
}
</script>

<template>
  <nav
    class="flex h-full flex-col gap-1 overflow-y-auto p-3"
    :aria-label="$t('sidebar.title')"
  >
    <p class="px-3 pb-3 pt-2 text-lg font-semibold tracking-tight">
      {{ $t('app.name') }}
    </p>

    <SidebarItem
      v-for="section in sections"
      :key="section.scope.kind"
      :active="isSameScope(view.scope, section.scope)"
      @click="select(section.scope)"
    >
      <template #icon>
        <component
          :is="section.icon"
          class="size-4"
        />
      </template>
      {{ $t(section.label) }}
    </SidebarItem>

    <div class="mt-auto pt-3">
      <SidebarItem :to="{ name: 'settings' }">
        <template #icon>
          <Settings class="size-4" />
        </template>
        {{ $t('settings.title') }}
      </SidebarItem>
    </div>
  </nav>
</template>
