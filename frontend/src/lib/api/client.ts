import createClient from 'openapi-fetch'
import type { paths } from './schema'

export type RefreshResult = 'renewed' | 'rejected' | 'unreachable'

/** Session callbacks used to authenticate requests and handle expired sessions. */
export interface AuthHooks {
  getAccessToken: () => string | null
  refresh: () => Promise<RefreshResult>
  onSessionLost: () => void
}

const AUTH_PATH = '/api/v1/auth/'

let hooks: AuthHooks | undefined

/** Registers the session callbacks used to authorize API requests. */
export function configureAuth(value: AuthHooks): void {
  hooks = value
}

/** Adds the current access token to a request when one is available. */
function attachToken(request: Request): void {
  const token = hooks?.getAccessToken()

  if (token) {
    request.headers.set('Authorization', `Bearer ${token}`)
  }
}

/**
 * Adds the access token.
 * On 401 renews it once and replays the request.
 */
async function authFetch(request: Request): Promise<Response> {
  if (!hooks || new URL(request.url).pathname.startsWith(AUTH_PATH)) {
    return fetch(request)
  }

  const replay = request.clone()
  attachToken(request)

  const response = await fetch(request)

  if (response.status !== 401) {
    return response
  }

  const outcome = await hooks.refresh()

  if (outcome === 'rejected') {
    hooks.onSessionLost()
  }

  if (outcome !== 'renewed') {
    return response
  }

  attachToken(replay)

  return fetch(replay)
}

export const api = createClient<paths>({
  baseUrl: location.origin,
  headers: { 'X-Requested-With': 'fetch' },
  fetch: authFetch,
})
