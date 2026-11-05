import { newId } from '@/lib/id'
import { getDb, type NoteRecord, type RichText } from '../db'
import { textCollator } from '@/lib/collator'
import { displayTitle } from '@/lib/note-text'
import { tokenize } from '@/lib/search'

export const EMPTY_DOC: RichText = {
  type: 'doc',
  content: [{ type: 'paragraph' }],
}

export const EMPTY_CONTENT = JSON.stringify(EMPTY_DOC)

/** Parses stored note content; unreadable content becomes an empty document. */
export function parseContent(content: string): RichText {
  try {
    return JSON.parse(content) as RichText
  } catch {
    return EMPTY_DOC
  }
}

/** Edits coming from the editor. */
export type NoteEdit = Partial<Pick<NoteRecord, 'title' | 'contentText'>> & { content?: RichText }

export type NoteScope =
  | { kind: 'all' }
  | { kind: 'unfiled' }
  | { kind: 'pinned' }
  | { kind: 'trash' }
  | {
      kind: 'folder'
      id: string
    }

type Block = {
  type?: string
  content?: unknown[]
}

function isBlank(note: NoteRecord): boolean {
  const blocks = (parseContent(note.content).content as Block[] | undefined) ?? []

  return (
    note.title.trim() === '' &&
    note.contentText.trim() === '' &&
    blocks.every((block) => block.type === 'paragraph' && !block.content?.length)
  )
}

function inScope(note: NoteRecord, scope: NoteScope): boolean {
  if (note.purgedAt) {
    return false
  }

  if (scope.kind === 'trash') {
    return note.deletedAt !== null
  }

  if (note.deletedAt !== null) {
    return false
  }

  switch (scope.kind) {
    case 'all':
      return true
    case 'unfiled':
      return note.folderId === null
    case 'pinned':
      return note.pinned
    case 'folder':
      return note.folderId === scope.id
  }
}

/** A new empty note, saved locally. */
async function create(folderId: string | null = null): Promise<NoteRecord> {
  const now = Date.now()
  const note: NoteRecord = {
    id: newId(),
    folderId,
    title: '',
    content: EMPTY_CONTENT,
    contentText: '',
    pinned: false,
    tagIds: [],
    createdAt: now,
    editedAt: now,
    deletedAt: null,
    purgedAt: null,
    serverVersion: 0,
    syncStatus: 'dirty',
    searchTokens: [],
  }

  await getDb().notes.add(note)

  return note
}

export interface NoteSort {
  by: 'edited' | 'created' | 'title'
  direction: 'asc' | 'desc'
}

export const DEFAULT_SORT: NoteSort = { by: 'edited', direction: 'desc' }

export interface NoteQuery {
  scope: NoteScope
  tagIds: string[]
  text: string
  sort: NoteSort
}

function comparator(scope: NoteScope, sort: NoteSort): (a: NoteRecord, b: NoteRecord) => number {
  if (scope.kind === 'trash') {
    return (a, b) => (b.deletedAt ?? 0) - (a.deletedAt ?? 0)
  }

  const sign = sort.direction === 'asc' ? 1 : -1
  const primary = (a: NoteRecord, b: NoteRecord): number => {
    switch (sort.by) {
      case 'edited':
        return a.editedAt - b.editedAt
      case 'created':
        return a.createdAt - b.createdAt
      case 'title':
        return textCollator.compare(displayTitle(a), displayTitle(b))
    }
  }

  return (a, b) => Number(b.pinned) - Number(a.pinned) || sign * primary(a, b)
}

async function searchNotes(terms: string[]): Promise<NoteRecord[]> {
  const db = getDb()
  let ids: Set<string> | undefined

  for (const term of terms) {
    const keys = new Set(await db.notes.where('searchTokens').startsWith(term).primaryKeys())

    ids = ids ? new Set([...ids].filter((id) => keys.has(id))) : keys

    if (ids.size === 0) {
      return []
    }
  }

  const notes = await db.notes.bulkGet([...(ids ?? [])])

  return notes.filter((note): note is NoteRecord => note !== undefined)
}

async function query({ scope, tagIds, text, sort }: NoteQuery): Promise<NoteRecord[]> {
  const matching = (note: NoteRecord) =>
    inScope(note, scope) && tagIds.every((id) => note.tagIds.includes(id))

  const terms = tokenize(text)
  const notes =
    terms.length > 0
      ? (await searchNotes(terms)).filter(matching)
      : await getDb().notes.filter(matching).toArray()

  return notes.sort(comparator(scope, sort))
}

/** Saves a user edit, refreshes the search index and marks the note for sync. */
async function save(id: string, { content, ...rest }: NoteEdit): Promise<void> {
  const db = getDb()

  await db.transaction('rw', db.notes, async () => {
    const note = await db.notes.get(id)

    if (!note) {
      return
    }

    const title = rest.title ?? note.title
    const contentText = rest.contentText ?? note.contentText

    await db.notes.update(id, {
      ...rest,
      ...(content && { content: JSON.stringify(content) }),
      searchTokens: tokenize(title, contentText),
      editedAt: Date.now(),
      syncStatus: 'dirty',
    })
  })
}

/** Deletes a note that was created and left empty. */
async function discardIfEmpty(id: string): Promise<void> {
  const note = await getDb().notes.get(id)

  if (note && note.serverVersion === 0 && isBlank(note)) {
    await getDb().notes.delete(id)
  }
}

async function setPinned(id: string, pinned: boolean): Promise<void> {
  await getDb().notes.update(id, {
    pinned,
    syncStatus: 'dirty',
  })
}

async function moveToTrash(id: string): Promise<void> {
  await getDb().notes.update(id, {
    deletedAt: Date.now(),
    syncStatus: 'dirty',
  })
}

/**
 * Restores a trashed note.
 * If its folder is gone, the note returns to the root.
 */
async function restore(id: string): Promise<void> {
  const db = getDb()

  await db.transaction('rw', db.notes, db.folders, async () => {
    const note = await db.notes.get(id)

    if (!note) {
      return
    }

    const folder = note.folderId ? await db.folders.get(note.folderId) : undefined
    const folderExists = folder !== undefined && folder.deletedAt === null

    await db.notes.update(id, {
      deletedAt: null,
      folderId: folderExists ? note.folderId : null,
      syncStatus: 'dirty',
    })
  })
}

/** Deletes a note forever. */
async function purge(id: string): Promise<void> {
  const db = getDb()
  const note = await db.notes.get(id)

  if (!note) {
    return
  }

  if (note.serverVersion === 0) {
    await db.notes.delete(id)
    return
  }

  await db.notes.update(id, {
    purgedAt: Date.now(),
    title: '',
    content: EMPTY_CONTENT,
    contentText: '',
    pinned: false,
    tagIds: [],
    folderId: null,
    syncStatus: 'dirty',
  })
}

async function emptyTrash(): Promise<void> {
  const ids = await getDb()
    .notes.filter((note) => note.deletedAt !== null && !note.purgedAt)
    .primaryKeys()

  for (const id of ids) {
    await purge(id)
  }
}

async function moveTo(id: string, folderId: string | null): Promise<void> {
  await getDb().notes.update(id, {
    folderId,
    syncStatus: 'dirty',
  })
}

export const noteRepository = {
  create,
  get: (id: string) => getDb().notes.get(id),
  query,
  list: (scope: NoteScope) => query({ scope, tagIds: [], text: '', sort: DEFAULT_SORT }),
  save,
  discardIfEmpty,
  setPinned,
  moveTo,
  moveToTrash,
  restore,
  purge,
  emptyTrash,
}
