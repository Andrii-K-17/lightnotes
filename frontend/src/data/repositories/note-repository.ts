import { newId } from '@/lib/id'
import { getDb, type NoteRecord, type RichText } from '../db'

export const EMPTY_DOC: RichText = {
  type: 'doc',
  content: [{ type: 'paragraph' }],
}

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
  const blocks = (note.content.content as Block[] | undefined) ?? []

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
    content: EMPTY_DOC,
    contentText: '',
    pinned: false,
    tagIds: [],
    createdAt: now,
    editedAt: now,
    deletedAt: null,
    purgedAt: null,
    serverVersion: 0,
    syncStatus: 'dirty',
  }

  await getDb().notes.add(note)

  return note
}

/** Notes of a scope. */
async function list(scope: NoteScope): Promise<NoteRecord[]> {
  const notes = await getDb()
    .notes.orderBy('editedAt')
    .reverse()
    .filter((note) => inScope(note, scope))
    .toArray()

  return scope.kind === 'trash'
    ? notes.sort((a, b) => (b.deletedAt ?? 0) - (a.deletedAt ?? 0))
    : notes.sort((a, b) => Number(b.pinned) - Number(a.pinned))
}

/** Saves a user edit and marks the note for sync. */
async function save(
  id: string,
  changes: Partial<Pick<NoteRecord, 'title' | 'content' | 'contentText'>>,
): Promise<void> {
  await getDb().notes.update(id, {
    ...changes,
    editedAt: Date.now(),
    syncStatus: 'dirty',
  })
}

/** Deletes a note that was created and left empty. */
async function discardIfEmpty(id: string): Promise<void> {
  const note = await getDb().notes.get(id)

  if (note && note.serverVersion === 0 && isBlank(note)) {
    await getDb().notes.delete(id)
  }
}

export const noteRepository = {
  create,
  get: (id: string) => getDb().notes.get(id),
  list,
  save,
  discardIfEmpty,
}
