import { ApiError, downloadBlob, uploadBlob } from '@/lib/api'
import { getDb } from '../db'
import { attachmentRepository } from '../repositories/attachment-repository'

const isTransient = (error: ApiError): boolean =>
  error.isNetwork || error.status >= 500 || error.status === 401 || error.status === 429

/** Uploads images whose note the server already knows; the rest wait for a later cycle. */
export async function uploadPending(): Promise<void> {
  const db = getDb()
  const pending = await db.attachments.where('uploadState').equals('pending').toArray()

  for (const attachment of pending) {
    const note = await db.notes.get(attachment.noteId)

    if (!attachment.blob || !note || note.serverVersion === 0) {
      continue
    }

    try {
      await uploadBlob(
        `/api/v1/attachments/${attachment.id}?noteId=${attachment.noteId}`,
        attachment.blob,
      )

      await db.attachments.update(attachment.id, { uploadState: 'uploaded' })
    } catch (error) {
      if (!(error instanceof ApiError) || isTransient(error)) {
        throw error
      }

      await db.attachments.update(attachment.id, { uploadState: 'failed' })
    }
  }
}

/**
 * Fetches an image from the server and keeps a copy;
 * null if the server does not have it yet.
 */
export async function downloadAttachment(id: string, noteId: string): Promise<Blob | null> {
  const blob = await downloadBlob(`/api/v1/attachments/${id}`)

  if (blob) {
    await attachmentRepository.cache(id, noteId, blob)
  }

  return blob
}
