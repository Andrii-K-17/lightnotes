import type { Profile } from './types'

const KEY = 'lightnotes.profile'

/**
 * The last signed-in profile.
 * Lets the app start without the server.
 */
export function readCachedProfile(): Profile | null {
  try {
    const value: unknown = JSON.parse(localStorage.getItem(KEY) ?? 'null')

    if (typeof value === 'object' && value !== null && 'id' in value && 'email' in value) {
      return value as Profile
    }
  } catch {}

  return null
}

export function writeCachedProfile(profile: Profile): void {
  try {
    localStorage.setItem(KEY, JSON.stringify(profile))
  } catch {}
}

export function clearCachedProfile(): void {
  try {
    localStorage.removeItem(KEY)
  } catch {}
}
