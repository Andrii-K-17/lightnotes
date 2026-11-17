import { inject, onScopeDispose, ref, watch } from 'vue'
import { attachmentRepository, downloadAttachment } from '@/data'
import { noteIdKey } from './note-id'

/** A displayable URL for a stored image: from the device, otherwise downloaded once and kept. */
export function useAttachmentUrl(id: () => string | null) {
  const noteId = inject(noteIdKey, '')
  const url = ref<string>()
  const failed = ref(false)

  function release(): void {
    if (url.value) {
      URL.revokeObjectURL(url.value)
      url.value = undefined
    }
  }

  watch(
    id,
    async (attachmentId) => {
      release()
      failed.value = false

      if (!attachmentId) {
        failed.value = true
        return
      }

      try {
        const stored = await attachmentRepository.get(attachmentId)
        const blob = stored?.blob ?? (await downloadAttachment(attachmentId, noteId))

        if (blob) {
          url.value = URL.createObjectURL(blob)
        } else {
          failed.value = true
        }
      } catch {
        failed.value = true
      }
    },
    { immediate: true },
  )

  onScopeDispose(release)

  return {
    url,
    failed,
  }
}
