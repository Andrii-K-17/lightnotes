import { computed, shallowRef } from 'vue'

interface BeforeInstallPromptEvent extends Event {
  prompt: () => Promise<void>
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>
}

const deferred = shallowRef<BeforeInstallPromptEvent | null>(null)
const installed = shallowRef(false)

/** Checks whether the app is already running in standalone mode. */
function isStandalone(): boolean {
  return (
    window.matchMedia('(display-mode: standalone)').matches ||
    (navigator as Navigator & { standalone?: boolean }).standalone === true
  )
}

/** Detects iOS devices that require manual PWA installation. */
function isIos(): boolean {
  const { userAgent, maxTouchPoints } = navigator

  return (
    /iPad|iPhone|iPod/.test(userAgent) || (userAgent.includes('Macintosh') && maxTouchPoints > 1)
  )
}

/** Captures PWA install events once at startup. */
export function listenForInstall(): void {
  installed.value = isStandalone()

  window.addEventListener('beforeinstallprompt', (event) => {
    event.preventDefault()
    deferred.value = event as BeforeInstallPromptEvent
  })

  window.addEventListener('appinstalled', () => {
    deferred.value = null
    installed.value = true
  })
}

/** Provides PWA installation state and actions for UI components. */
export function useInstall() {
  /** Shows the browser install prompt and waits for the user's choice. */
  async function install(): Promise<void> {
    const event = deferred.value

    if (!event) {
      return
    }

    deferred.value = null
    await event.prompt()
    await event.userChoice
  }

  return {
    installed,
    canInstall: computed(() => deferred.value !== null),
    needsManualInstall: computed(() => !installed.value && isIos()),
    install,
  }
}
