import { newId } from '@/lib/id'
import { getDb, type NoteRecord, type RichText } from '../db'

export const EMPTY_DOC: RichText = { type: 'doc', content: [{ type: 'paragraph' }] }

export const noteRepository = {
  /** A new empty note, saved locally. */
  async create(): Promise<NoteRecord> {
    const now = Date.now()
    const note: NoteRecord = {
      id: newId(),
      folderId: null,
      title: '',
      content: EMPTY_DOC,
      contentText: '',
      pinned: false,
      tagIds: [],
      createdAt: now,
      editedAt: now,
      deletedAt: null,
      serverVersion: 0,
      syncStatus: 'dirty',
    }

    await getDb().notes.add(note)

    return note
  },

  get: (id: string) => getDb().notes.get(id),

  async listActive(): Promise<NoteRecord[]> {
    const notes = await getDb()
      .notes.orderBy('editedAt')
      .reverse()
      .filter((note) => note.deletedAt === null)
      .toArray()

    return notes.sort((a, b) => Number(b.pinned) - Number(a.pinned))
  },

  /** Saves a user edit and marks the note for sync. */
  async save(
    id: string,
    changes: Partial<Pick<NoteRecord, 'title' | 'content' | 'contentText'>>,
  ): Promise<void> {
    await getDb().notes.update(id, { ...changes, editedAt: Date.now(), syncStatus: 'dirty' })
  },
}
