import { configureAuth } from '@/lib/api'
import { useSessionStore } from './stores/session'

/** Connects the API client to the session. */
export function installAuth(onSessionLost: () => void): void {
  const session = useSessionStore()

  configureAuth({
    getAccessToken: session.getAccessToken,
    refresh: session.refresh,
    onSessionLost: () => {
      session.clear()
      onSessionLost()
    },
  })
}
