import { newId } from '@/lib/id'
import { textCollator } from '@/lib/collator'
import { getDb, type TagRecord } from '../db'

export const TAG_NAME_MAX_LENGTH = 40

const normalizeName = (name: string) => name.trim().slice(0, TAG_NAME_MAX_LENGTH)
const nameKey = (name: string) => name.toLowerCase()

/** Tags in alphabetical order. */
async function list(): Promise<TagRecord[]> {
  const tags = await getDb()
    .tags.filter((tag) => tag.deletedAt === null)
    .toArray()

  return tags.sort((a, b) => textCollator.compare(a.name, b.name))
}

function findByName(name: string): Promise<TagRecord | undefined> {
  return getDb()
    .tags.where('nameLower')
    .equals(nameKey(name))
    .filter((tag) => tag.deletedAt === null)
    .first()
}

/** The tag with this name, created if it does not exist yet. */
async function ensure(name: string): Promise<TagRecord | undefined> {
  const trimmed = normalizeName(name)

  if (!trimmed) {
    return undefined
  }

  const existing = await findByName(trimmed)

  if (existing) {
    return existing
  }

  const tag: TagRecord = {
    id: newId(),
    name: trimmed,
    nameLower: nameKey(trimmed),
    createdAt: Date.now(),
    deletedAt: null,
    serverVersion: 0,
    syncStatus: 'dirty',
  }

  await getDb().tags.add(tag)

  return tag
}

export type RenameResult = 'renamed' | 'duplicate' | 'invalid'

async function rename(id: string, name: string): Promise<RenameResult> {
  const trimmed = normalizeName(name)

  if (!trimmed) {
    return 'invalid'
  }

  const clash = await findByName(trimmed)

  if (clash && clash.id !== id) {
    return 'duplicate'
  }

  await getDb().tags.update(id, {
    name: trimmed,
    nameLower: nameKey(trimmed),
    syncStatus: 'dirty',
  })

  return 'renamed'
}

/** Deletes a tag and removes it from every note that has it. */
async function remove(id: string): Promise<void> {
  const db = getDb()

  await db.transaction('rw', db.tags, db.notes, async () => {
    await db.notes
      .where('tagIds')
      .equals(id)
      .modify((note) => {
        note.tagIds = note.tagIds.filter((tagId) => tagId !== id)
        note.syncStatus = 'dirty'
      })
    await db.tags.update(id, {
      deletedAt: Date.now(),
      syncStatus: 'dirty',
    })
  })
}

export const tagRepository = {
  list,
  ensure,
  rename,
  remove,
}
