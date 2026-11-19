import { onBeforeUnmount, onMounted } from 'vue'
import { toast } from 'vue-sonner'
import { i18n } from '@/i18n'
import { useInstall } from '@/lib/pwa-install'

const FIRST_DELAY_MS = 30_000
const REMIND_AFTER_MS = 7 * 24 * 60 * 60 * 1000
const REMINDED_KEY = 'lightnotes.installRemindedAt'

function remindedRecently(): boolean {
  try {
    return Date.now() - Number(localStorage.getItem(REMINDED_KEY) ?? 0) < REMIND_AFTER_MS
  } catch {
    return false
  }
}

function markReminded(): void {
  try {
    localStorage.setItem(REMINDED_KEY, String(Date.now()))
  } catch {}
}

export function usePwaPrompts(): void {
  const { canInstall, needsManualInstall, installed, install } = useInstall()
  let timer: ReturnType<typeof setTimeout> | undefined

  function remind(): void {
    if (installed.value || remindedRecently()) {
      return
    }

    const { t } = i18n.global

    if (canInstall.value) {
      markReminded()
      toast(t('pwa.installPrompt'), {
        duration: Infinity,
        action: { label: t('pwa.install'), onClick: () => void install() },
        cancel: { label: t('pwa.later'), onClick: () => undefined },
      })
    } else if (needsManualInstall.value) {
      markReminded()
      toast(t('pwa.iosHint'), {
        duration: Infinity,
        cancel: { label: t('pwa.gotIt'), onClick: () => undefined },
      })
    }
  }

  onMounted(() => {
    if (installed.value) {
      void navigator.storage?.persist?.()
    }

    timer = setTimeout(remind, FIRST_DELAY_MS)
  })

  onBeforeUnmount(() => clearTimeout(timer))
}
