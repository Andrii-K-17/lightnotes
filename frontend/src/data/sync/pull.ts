import type { EntityTable, IDType } from 'dexie'
import { getDb, type FolderRecord, type NoteRecord, type SyncStatus, type TagRecord } from '../db'
import { syncApi, type PullResponse } from './api'
import { folderRecord, noteRecord, tagRecord } from './wire'

const CURSOR_KEY = 'lastPulledSeq'

/** Fields shared by local records received from the server. */
interface Synced {
  id: string
  serverVersion: number
  syncStatus: SyncStatus
}

/** Returns the sequence number from which the next pull should continue. */
export async function getSyncCursor(): Promise<number> {
  const record = await getDb().meta.get(CURSOR_KEY)

  return typeof record?.value === 'number' ? record.value : 0
}

/**
 * Pulls and applies pages until the server has no more changes.
 * @returns The total number of entities received.
 */
export async function pullAll(onProgress?: (received: number) => void): Promise<number> {
  let received = 0

  for (;;) {
    const since = await getSyncCursor()
    const page = await syncApi.pull(since)

    await applyPage(page)

    received += page.folders.length + page.tags.length + page.notes.length
    onProgress?.(received)

    if (!page.hasMore) {
      return received
    }

    if (Number(page.nextSeq) <= since) {
      throw new Error('The server sent a page that does not advance the cursor.')
    }
  }
}

/** Merges a page into the local database and advances the cursor atomically. */
async function applyPage(page: PullResponse): Promise<void> {
  const db = getDb()

  await db.transaction('rw', db.folders, db.tags, db.notes, db.meta, async () => {
    for (const dto of page.folders) {
      await merge<FolderRecord>(
        db.folders,
        dto.id,
        Number(dto.version),
        Boolean(dto.purgedAt),
        () => folderRecord(dto),
      )
    }

    for (const dto of page.tags) {
      await merge<TagRecord>(db.tags, dto.id, Number(dto.version), Boolean(dto.purgedAt), () =>
        tagRecord(dto),
      )
    }

    for (const dto of page.notes) {
      await merge<NoteRecord>(db.notes, dto.id, Number(dto.version), Boolean(dto.purgedAt), () =>
        noteRecord(dto),
      )
    }

    await db.meta.put({
      key: CURSOR_KEY,
      value: Number(page.nextSeq),
    })
  })
}

/** Merges a server entity unless local edits or a newer version should be kept. */
async function merge<T extends Synced>(
  table: EntityTable<T, 'id'>,
  id: string,
  version: number,
  purged: boolean,
  build: () => T,
): Promise<void> {
  const local = await table.get(id as IDType<T, 'id'>)

  if (local && (local.syncStatus === 'dirty' || version <= local.serverVersion)) {
    return
  }

  if (purged) {
    if (local) {
      await table.delete(id as IDType<T, 'id'>)
    }

    return
  }

  await table.put(build())
}
