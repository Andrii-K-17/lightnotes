<script setup lang="ts">
import { Check, Clock, CloudOff, LoaderCircle, TriangleAlert } from '@lucide/vue'
import { pendingChanges } from '@/data'
import { errorMessage } from '@/i18n/errors'
import { useLiveQuery } from '@/lib/live-query'
import { useSyncStore } from '../stores/sync'

defineProps<{ compact?: boolean }>()

const sync = useSyncStore()
const { data: pending } = useLiveQuery(pendingChanges)
</script>

<template>
  <div
    class="flex items-center gap-2 text-xs text-muted-foreground"
    role="status"
    aria-live="polite"
  >
    <template v-if="sync.state === 'syncing'">
      <LoaderCircle class="size-4 animate-spin" />
      <span :class="{ 'sr-only': compact }">
        {{ $t('sync.syncing') }}
      </span>
    </template>

    <template v-else-if="sync.state === 'offline'">
      <CloudOff class="size-4" />
      
      <span :class="{ 'sr-only': compact }">
        {{ $t('sync.offline') }}

        <template v-if="pending">
          {{ `· ${$t('sync.waiting', pending)}` }}
        </template>
      </span>
    </template>

    <button
      v-else-if="sync.state === 'error'"
      type="button"
      class="flex items-center gap-2 rounded-md text-destructive outline-none focus-visible:ring-2 focus-visible:ring-ring"
      :title="errorMessage(sync.errorCode ?? 'unknown')"
      @click="sync.retry()"
    >
      <TriangleAlert class="size-4" />
      <span :class="{ 'sr-only': compact }">
        {{ $t('sync.error') }} {{ `· ${$t('sync.retry')}` }}
      </span>
    </button>

    <template v-else-if="pending">
      <Clock class="size-4" />
      <span :class="{ 'sr-only': compact }">
        {{ $t('sync.waiting', pending) }}
      </span>
    </template>

    <template v-else>
      <Check class="size-4 text-primary" />
      <span :class="{ 'sr-only': compact }">
        {{ $t('sync.synced') }}
      </span>
    </template>
  </div>
</template>
