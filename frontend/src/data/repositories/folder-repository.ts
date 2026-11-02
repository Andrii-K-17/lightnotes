import { generateKeyBetween } from 'fractional-indexing'
import { newId } from '@/lib/id'
import { getDb, type FolderRecord } from '../db'

export const FOLDER_NAME_MAX_LENGTH = 60

const normalizeName = (name: string) => name.trim().slice(0, FOLDER_NAME_MAX_LENGTH)

/** Live folders in manual order. */
function list(): Promise<FolderRecord[]> {
  return getDb()
    .folders.orderBy('position')
    .filter((folder) => folder.deletedAt === null)
    .toArray()
}

/** Adds a folder after the last one. */
async function create(name: string): Promise<FolderRecord | undefined> {
  const trimmed = normalizeName(name)

  if (!trimmed) {
    return undefined
  }

  const last = (await list()).at(-1)
  const folder: FolderRecord = {
    id: newId(),
    name: trimmed,
    position: generateKeyBetween(last?.position ?? null, null),
    createdAt: Date.now(),
    deletedAt: null,
    serverVersion: 0,
    syncStatus: 'dirty',
  }

  await getDb().folders.add(folder)

  return folder
}

async function rename(id: string, name: string): Promise<void> {
  const trimmed = normalizeName(name)

  if (trimmed) {
    await getDb().folders.update(id, { name: trimmed, syncStatus: 'dirty' })
  }
}

/** Deletes a folder. */
async function remove(id: string): Promise<void> {
  const db = getDb()
  const now = Date.now()

  await db.transaction('rw', db.folders, db.notes, async () => {
    await db.notes
      .where('folderId')
      .equals(id)
      .filter((note) => note.deletedAt === null)
      .modify({ deletedAt: now, syncStatus: 'dirty' })
    await db.folders.update(id, { deletedAt: now, syncStatus: 'dirty' })
  })
}

export const folderRepository = {
  list,
  create,
  rename,
  remove,
}
