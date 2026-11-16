import { getDb, type FolderRecord, type LightNotesDb, type NoteRecord, type TagRecord } from '../db'
import { syncApi, type PushResponse } from './api'
import { folderChange, noteChange, noteRecord, tagChange, tagRecord, type NoteDto } from './wire'

const MAX_CHANGES = 100
const MAX_NOTE_CHARACTERS = 2_000_000
const MAX_BATCHES_PER_CYCLE = 50

interface Batch {
  folders: FolderRecord[]
  tags: TagRecord[]
  notes: NoteRecord[]
}

export interface PushSummary {
  sent: number
  rejected: number
  conflicts: number
}

/** Indexes entities by their ID. */
const byId = <T extends { id: string }>(items: T[]) => new Map(items.map((item) => [item.id, item]))

/** Checks whether the folder fields sent to the server are unchanged. */
const sameFolder = (a: FolderRecord, b: FolderRecord) =>
  a.name === b.name && a.position === b.position && a.deletedAt === b.deletedAt

/** Checks whether the tag fields sent to the server are unchanged. */
const sameTag = (a: TagRecord, b: TagRecord) => a.name === b.name && a.deletedAt === b.deletedAt

/** Checks whether the note content sent to the server is unchanged. */
const sameContent = (a: NoteRecord, b: NoteRecord) =>
  a.title === b.title && a.content === b.content && a.contentText === b.contentText

/** Checks whether the note metadata sent to the server is unchanged. */
const sameMeta = (a: NoteRecord, b: NoteRecord) =>
  a.folderId === b.folderId &&
  a.pinned === b.pinned &&
  a.deletedAt === b.deletedAt &&
  a.purgedAt === b.purgedAt &&
  [...a.tagIds].sort().join() === [...b.tagIds].sort().join()

/** Collects dirty entities for a batch, prioritizing folders, then tags, then notes. */
async function collect(): Promise<Batch> {
  const db = getDb()

  const folders = await db.folders.where('syncStatus').equals('dirty').limit(MAX_CHANGES).toArray()
  let room = MAX_CHANGES - folders.length
  const tags =
    room > 0 ? await db.tags.where('syncStatus').equals('dirty').limit(room).toArray() : []
  room -= tags.length
  const candidates =
    room > 0 ? await db.notes.where('syncStatus').equals('dirty').limit(room).toArray() : []

  const notes: NoteRecord[] = []
  let characters = 0

  for (const note of candidates) {
    const size = note.content.length + note.contentText.length

    if (notes.length > 0 && characters + size > MAX_NOTE_CHARACTERS) {
      break
    }

    notes.push(note)
    characters += size
  }

  return {
    folders,
    tags,
    notes,
  }
}

/** Pushes dirty entities in batches and summarizes the server's responses. */
export async function pushAll(): Promise<PushSummary> {
  const summary: PushSummary = {
    sent: 0,
    rejected: 0,
    conflicts: 0,
  }

  for (let batchNumber = 0; batchNumber < MAX_BATCHES_PER_CYCLE; batchNumber++) {
    const batch = await collect()
    const count = batch.folders.length + batch.tags.length + batch.notes.length

    if (count === 0) {
      break
    }

    const response = await syncApi.push({
      folders: batch.folders.map(folderChange),
      tags: batch.tags.map(tagChange),
      notes: batch.notes.map(noteChange),
    })
    const { handled, rejected, conflicts } = await applyResults(batch, response)

    if (handled === 0) {
      throw new Error('The server answered without a result for any pushed change.')
    }

    summary.sent += count
    summary.rejected += rejected
    summary.conflicts += conflicts
  }

  return summary
}

/** Applies server results, keeping entities edited during the request dirty. */
async function applyResults(
  batch: Batch,
  response: PushResponse,
): Promise<{
  handled: number
  rejected: number
  conflicts: number
}> {
  const db = getDb()
  const folders = byId(batch.folders)
  const tags = byId(batch.tags)
  const notes = byId(batch.notes)
  let handled = 0
  let rejected = 0
  let conflicts = 0

  await db.transaction('rw', db.folders, db.tags, db.notes, async () => {
    for (const result of response.folders) {
      const sent = folders.get(result.id)
      const local = await db.folders.get(result.id)

      if (!sent || !local) {
        continue
      }

      handled++
      const unchanged = sameFolder(local, sent)

      if (result.status === 'Applied') {
        await db.folders.update(local.id, {
          serverVersion: Number(result.version),
          syncStatus: unchanged ? 'clean' : 'dirty',
        })
      } else {
        rejected++

        if (unchanged) {
          await db.folders.update(local.id, { syncStatus: 'rejected' })
        }
      }
    }

    for (const result of response.tags) {
      const sent = tags.get(result.id)
      const local = await db.tags.get(result.id)

      if (!sent || !local) {
        continue
      }

      handled++
      const unchanged = sameTag(local, sent)

      if (result.status === 'Applied') {
        await db.tags.update(local.id, {
          serverVersion: Number(result.version),
          syncStatus: unchanged ? 'clean' : 'dirty',
        })
      } else if (result.status === 'Merged' && result.mergedInto && result.current) {
        await mergeTag(db, local.id, result.mergedInto, result.current)
      } else {
        rejected++

        if (unchanged) {
          if (result.current) {
            await db.tags.put(tagRecord(result.current))
          } else {
            await db.tags.update(local.id, { syncStatus: 'rejected' })
          }
        }
      }
    }

    for (const result of response.notes) {
      const sent = notes.get(result.id)
      const local = await db.notes.get(result.id)

      if (!sent || !local) {
        continue
      }

      handled++

      if (result.status === 'Applied') {
        await markNoteApplied(db, local, sent, Number(result.version))
      } else if (result.status === 'Conflict' && result.current) {
        conflicts++
        await resolveNoteConflict(db, local, sent, result.current)
      } else {
        rejected++

        if (sameContent(local, sent) && sameMeta(local, sent)) {
          await db.notes.update(local.id, { syncStatus: 'rejected' })
        }
      }
    }
  })

  return {
    handled,
    rejected,
    conflicts,
  }
}

/** Replaces a duplicate local tag with the server tag and updates note references. */
async function mergeTag(
  db: LightNotesDb,
  localId: string,
  serverId: string,
  current: Parameters<typeof tagRecord>[0],
) {
  await db.tags.delete(localId)

  if (!(await db.tags.get(serverId))) {
    await db.tags.put(tagRecord(current))
  }

  await db.notes
    .where('tagIds')
    .equals(localId)
    .modify((note) => {
      note.tagIds = [...new Set(note.tagIds.map((id) => (id === localId ? serverId : id)))]
      note.syncStatus = 'dirty'
      note.metaDirty = true
    })
}

/** Updates a note's version and keeps any edits made while the request was in flight dirty. */
async function markNoteApplied(
  db: LightNotesDb,
  local: NoteRecord,
  sent: NoteRecord,
  version: number,
): Promise<void> {
  const sentContent = sent.serverVersion === 0 || sent.contentDirty
  const sentMeta = sent.serverVersion === 0 || sent.metaDirty

  const contentDirty = local.contentDirty && !(sentContent && sameContent(local, sent))
  const metaDirty = local.metaDirty && !(sentMeta && sameMeta(local, sent))

  await db.notes.update(local.id, {
    serverVersion: version,
    contentDirty,
    metaDirty,
    contentBaseVersion: contentDirty ? version : null,
    syncStatus: contentDirty || metaDirty ? 'dirty' : 'clean',
  })
}

/**
 * Resolves a server conflict for a note.
 * - If the local note hasn't changed since it was sent, accepts the server version (`current`).
 * - If the user edited the note again while the request was in flight, leaves it
 *   marked as 'dirty' with its old base version to be synced in the next cycle.
 */
async function resolveNoteConflict(
  db: LightNotesDb,
  local: NoteRecord,
  sent: NoteRecord,
  current: NoteDto,
): Promise<void> {
  if (sameContent(local, sent) && sameMeta(local, sent)) {
    await db.notes.put(noteRecord(current))
    return
  }
}
