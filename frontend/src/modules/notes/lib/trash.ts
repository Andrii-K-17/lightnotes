export const TRASH_RETENTION_DAYS = 30

const DAY_MS = 86_400_000

export function daysLeftInTrash(deletedAt: number, now = Date.now()): number {
  return Math.max(0, TRASH_RETENTION_DAYS - Math.floor((now - deletedAt) / DAY_MS))
}
