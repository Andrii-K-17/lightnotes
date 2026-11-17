import Image from '@tiptap/extension-image'
import { mergeAttributes, VueNodeViewRenderer } from '@tiptap/vue-3'
import NoteImageView from './NoteImageView.vue'

/** An image that points at a stored attachment instead of a URL. */
export const NoteImage = Image.extend({
  addAttributes() {
    return {
      attachmentId: {
        default: null,
        parseHTML: (element) => element.getAttribute('data-attachment-id'),
        renderHTML: (attributes) =>
          attributes.attachmentId ? { 'data-attachment-id': attributes.attachmentId } : {},
      },
      alt: { default: null },
    }
  },

  parseHTML() {
    return [{ tag: 'img[data-attachment-id]' }]
  },

  renderHTML({ HTMLAttributes }) {
    return ['img', mergeAttributes(this.options.HTMLAttributes, HTMLAttributes)]
  },

  addNodeView() {
    return VueNodeViewRenderer(NoteImageView)
  },
})
