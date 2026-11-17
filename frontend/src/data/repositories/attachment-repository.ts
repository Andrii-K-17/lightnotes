import { newId } from '@/lib/id'
import { getDb, type AttachmentRecord } from '../db'

/** Stores an image picked by the user. */
async function add(noteId: string, blob: Blob): Promise<AttachmentRecord> {
  const attachment: AttachmentRecord = {
    id: newId(),
    noteId,
    contentType: blob.type,
    size: blob.size,
    blob,
    uploadState: 'pending',
  }

  await getDb().attachments.add(attachment)

  return attachment
}

/** An image downloaded from the server, kept for offline use. */
async function cache(id: string, noteId: string, blob: Blob): Promise<void> {
  await getDb().attachments.put({
    id,
    noteId,
    contentType: blob.type,
    size: blob.size,
    blob,
    uploadState: 'uploaded',
  })
}

export const attachmentRepository = {
  add,
  cache,
  get: (id: string) => getDb().attachments.get(id),
}
