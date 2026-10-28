import StarterKit from '@tiptap/starter-kit'
import { Placeholder } from '@tiptap/extensions'

export function createExtensions(placeholder: string) {
  return [StarterKit, Placeholder.configure({ placeholder })]
}
