import Dexie, { type EntityTable } from 'dexie'

/**
 * `dirty` means unsynced changes.
 * `conflict` means the server copy differs.
 */
export type SyncStatus = 'clean' | 'dirty' | 'conflict'

/** Tiptap document JSON. */
export type RichText = Record<string, unknown>

interface SyncFields {
  /** Latest server version known to this device. */
  serverVersion: number
  syncStatus: SyncStatus
  /** Deletion time in epoch milliseconds, or null. */
  deletedAt: number | null
}

export interface NoteRecord extends SyncFields {
  id: string
  folderId: string | null
  title: string
  content: RichText
  /** Plain text used for previews and search. */
  contentText: string
  pinned: boolean
  tagIds: string[]
  createdAt: number
  /** Last user edit time, used to keep list ordering stable after sync. */
  editedAt: number
  purgedAt: number | null
}

export interface FolderRecord extends SyncFields {
  id: string
  name: string
  /** Fractional index for manual ordering. */
  position: string
  createdAt: number
}

export interface TagRecord extends SyncFields {
  id: string
  name: string
  nameLower: string
  createdAt: number
}

export interface AttachmentRecord {
  id: string
  noteId: string
  contentType: string
  size: number
  blob: Blob | null
  uploadState: 'pending' | 'uploaded'
}

export interface MetaRecord {
  key: string
  value: unknown
}

class LightNotesDb extends Dexie {
  notes!: EntityTable<NoteRecord, 'id'>
  folders!: EntityTable<FolderRecord, 'id'>
  tags!: EntityTable<TagRecord, 'id'>
  attachments!: EntityTable<AttachmentRecord, 'id'>
  meta!: EntityTable<MetaRecord, 'key'>

  constructor(userId: string) {
    super(databaseName(userId))

    this.version(1).stores({
      notes: 'id, folderId, editedAt, deletedAt, syncStatus, *tagIds',
      folders: 'id, position, deletedAt, syncStatus',
      tags: 'id, nameLower, deletedAt, syncStatus',
      attachments: 'id, noteId, uploadState',
      meta: 'key',
    })
  }
}

export type { LightNotesDb }

const databaseName = (userId: string) => `lightnotes-${userId}`

let current: LightNotesDb | undefined

/** Opens the signed-in user's database. */
export async function openDatabase(userId: string): Promise<void> {
  if (current?.name === databaseName(userId)) {
    return
  }

  current?.close()
  current = new LightNotesDb(userId)
  await current.open()
}

export function closeDatabase(): void {
  current?.close()
  current = undefined
}

/** Returns the open database for repository use. */
export function getDb(): LightNotesDb {
  if (!current) {
    throw new Error('The local database is not open.')
  }

  return current
}
