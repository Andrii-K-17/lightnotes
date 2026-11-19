import { registerSW } from 'virtual:pwa-register'
import { toast } from 'vue-sonner'
import { i18n } from '@/i18n'

const UPDATE_CHECK_MS = 60 * 60 * 1000

/** Caches the app so it opens offline, and offers new versions with a toast. */
export function registerPwa(): void {
  const updateSW = registerSW({
    onNeedRefresh() {
      const { t } = i18n.global

      toast(t('pwa.update'), {
        duration: Infinity,
        action: { label: t('pwa.reload'), onClick: () => void updateSW(true) },
      })
    },
    onOfflineReady() {
      toast(i18n.global.t('pwa.offlineReady'))
    },
    onRegisteredSW(_url, registration) {
      if (registration) {
        setInterval(() => void registration.update().catch(() => undefined), UPDATE_CHECK_MS)
      }
    },
  })
}
