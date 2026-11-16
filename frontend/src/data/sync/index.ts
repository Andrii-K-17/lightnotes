import { getDb } from '../db'
import { pullAll, getSyncCursor } from './pull'
import { pushAll } from './push'

export { getSyncCursor }

export interface SyncReport {
  pushed: number
  rejected: number
  conflicts: number
  pulled: number
}

/** One sync cycle: push local changes, then pull everything new. */
export async function syncOnce(onProgress?: (received: number) => void): Promise<SyncReport> {
  const push = await pushAll()
  const pulled = await pullAll(onProgress)

  return {
    pushed: push.sent,
    rejected: push.rejected,
    conflicts: push.conflicts,
    pulled,
  }
}

/** Number of entities with local changes waiting to be pushed. */
export async function pendingChanges(): Promise<number> {
  const db = getDb()
  const counts = await Promise.all([
    db.folders.where('syncStatus').equals('dirty').count(),
    db.tags.where('syncStatus').equals('dirty').count(),
    db.notes.where('syncStatus').equals('dirty').count(),
  ])

  return counts.reduce((sum, count) => sum + count, 0)
}
