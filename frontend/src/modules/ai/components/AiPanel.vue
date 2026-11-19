<script setup lang="ts">
import { useMediaQuery, useOnline } from '@vueuse/core'
import { computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { toast } from 'vue-sonner'
import { Button } from '@/components/ui/button'
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet'
import { errorMessage } from '@/i18n/errors'
import { ApiError } from '@/lib/api'
import { useSessionStore } from '@/modules/auth'
import { runAiAction, type AiAction } from '../lib/stream'
import { ArrowBigUpIcon } from '@lucide/vue'
import { Input } from '@/components/ui/input'

const props = defineProps<{
  /** The text to work on: the selection, or the whole note when nothing is selected. */
  getInput: () => {
    text: string
    selection: boolean
  }
}>()
const open = defineModel<boolean>('open', { required: true })
const emit = defineEmits<{ apply: [text: string, mode: 'replace' | 'insert'] }>()

const { t } = useI18n()
const session = useSessionStore()
const online = useOnline()

const side = computed(() => (useMediaQuery('(min-width: 640px)').value ? 'right' : 'bottom'))

const LANGUAGES = [
  'English',
  'Ukrainian',
  'Polish',
  'German',
  'French',
  'Spanish',
  'Italian',
  'Portuguese',
  'Dutch',
  'Czech',
  'Slovak',
  'Romanian',
  'Swedish',
  'Norwegian',
  'Japanese',
  'Korean',
  'Chinese',
  'Arabic',
] as const

const language = ref<string>('English')
const customLanguage = ref('')
const translationLanguage = computed(() =>
  language.value === 'Custom' ? customLanguage.value.trim() : language.value,
)

const prompt = ref('')

const result = ref('')
const running = ref(false)
const error = ref<string | null>(null)

let lastAction: AiAction = 'improve'
let controller: AbortController | undefined

async function run(action: AiAction): Promise<void> {
  const input = props.getInput()

  lastAction = action
  result.value = ''
  error.value = null

  if (action !== 'custom' && !input.text.trim()) {
    error.value = t('ai.nothing')
    return
  }

  controller?.abort()

  const current = new AbortController()

  controller = current
  running.value = true

  try {
    await runAiAction(
      {
        action,
        text: input.text,
        language: action === 'translate' ? translationLanguage.value : undefined,
        instruction: action === 'custom' ? prompt.value.trim() : undefined,
      },
      (delta) => {
        result.value += delta
      },
      current.signal,
    )
  } catch (cause) {
    if (!current.signal.aborted) {
      error.value = errorMessage(cause instanceof ApiError ? cause.code : 'unknown')
    }
  } finally {
    if (controller === current) {
      running.value = false
    }
  }
}

function stop(): void {
  controller?.abort()
  running.value = false
}

function apply(mode: 'replace' | 'insert'): void {
  emit('apply', result.value, mode)
  open.value = false
}

async function copy(): Promise<void> {
  await navigator.clipboard.writeText(result.value)
  toast(t('ai.copied'))
}

async function allow(): Promise<void> {
  error.value = null

  try {
    await session.setAiConsent(true)
  } catch (cause) {
    error.value = errorMessage(cause instanceof ApiError ? cause.code : 'unknown')
  }
}

watch(open, (value) => {
  if (!value) {
    stop()
    result.value = ''
    error.value = null
  }
})
</script>

<template>
  <Sheet v-model:open="open">
    <SheetContent
      :side="side"
      class="gap-3 p-4"
    >
      <SheetHeader class="p-0">
        <SheetTitle>{{ $t('ai.title') }}</SheetTitle>
        <SheetDescription>{{ $t('ai.description') }}</SheetDescription>
      </SheetHeader>

      <div
        v-if="!session.profile?.aiConsent"
        class="grid gap-3"
      >
        <p class="text-sm text-muted-foreground">
          {{ $t('ai.consentText') }}
        </p>
        <Button @click="allow">
          {{ $t('ai.allow') }}
        </Button>
      </div>

      <template v-else>
        <p
          v-if="!online"
          class="text-sm text-muted-foreground"
        >
          {{ $t('ai.offline') }}
        </p>

        <div class="flex flex-wrap items-center gap-2">
          <Button
            variant="outline"
            :disabled="running || !online"
            @click="run('improve')"
          >
            {{ $t('ai.improve') }}
          </Button>
          <Button
            variant="outline"
            :disabled="running || !online"
            @click="run('summarize')"
          >
            {{ $t('ai.summarize') }}
          </Button>
        </div>

        <div class="flex flex-wrap items-center gap-2">
          <select
            v-model="language"
            class="h-9 rounded-md border border-input bg-popover px-2 text-sm text-popover-foreground dark:scheme-dark"
            :aria-label="$t('ai.translateTo')"
          >
            <option
              v-for="name in LANGUAGES"
              :key="name"
              :value="name"
              class="bg-popover text-popover-foreground"
            >
              {{ $t(`ai.languages.${name}`) }}
            </option>
            <option value="Custom" class="bg-popover text-popover-foreground">
              {{ $t('ai.languages.Custom') }}
            </option>
          </select>
          <Input
            v-if="language === 'Custom'"
            v-model="customLanguage"
            maxlength="40"
            class="max-w-48"
            :placeholder="$t('ai.customLanguage')"
            :aria-label="$t('ai.customLanguage')"
          />
          <Button
            variant="outline"
            :disabled="running || !online || (language === 'Custom' && !translationLanguage)"
            @click="run('translate')"
          >
            {{ $t('ai.translate') }}
          </Button>
        </div>

        <form
          class="flex items-center gap-2"
          @submit.prevent="run('custom')"
        >
          <Input
            v-model="prompt"
            maxlength="1000"
            :placeholder="$t('ai.promptPlaceholder')"
            :aria-label="$t('ai.promptPlaceholder')"
          />
          <Button
            type="submit"
            size="icon"
            :disabled="running || !online || !prompt.trim()"
            :aria-label="$t('ai.send')"
            class="cursor-pointer"
          >
            <ArrowBigUpIcon class="size-4" />
          </Button>
        </form>

        <div
          v-if="result || running"
          class="max-h-64 overflow-y-auto whitespace-pre-wrap rounded-xl bg-muted/60 p-3 text-sm"
        >
          {{ result }}
          <span
            v-if="running"
            class="ml-0.5 inline-block h-4 w-1 animate-pulse bg-foreground/60 align-middle"
          />
        </div>

        <p
          v-if="error"
          role="alert"
          class="text-sm text-destructive"
        >
          {{ error }}
        </p>

        <div v-if="running">
          <Button
            variant="ghost"
            size="sm"
            @click="stop"
          >
            {{ $t('ai.stop') }}
          </Button>
        </div>
        <div
          v-else-if="result || error"
          class="flex flex-wrap gap-2"
        >
          <template v-if="result">
            <Button
              size="sm"
              @click="apply('replace')"
            >
              {{ $t('ai.replace') }}
            </Button>
            <Button
              size="sm"
              variant="outline"
              @click="apply('insert')"
            >
              {{ $t('ai.insert') }}
            </Button>
            <Button
              size="sm"
              variant="outline"
              @click="copy"
            >
              {{ $t('ai.copy') }}
            </Button>
          </template>
          <Button
            size="sm"
            variant="ghost"
            :disabled="!online"
            @click="run(lastAction)"
          >
            {{ $t('ai.retry') }}
          </Button>
        </div>
      </template>
    </SheetContent>
  </Sheet>
</template>
