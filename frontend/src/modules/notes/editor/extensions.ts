import StarterKit from '@tiptap/starter-kit'
import { Placeholder } from '@tiptap/extensions'

export function createExtensions(placeholder: string) {
  return [
    StarterKit.configure({
      link: {
        openOnClick: false,
        autolink: true,
        defaultProtocol: 'https',
      },
    }),
    Placeholder.configure({ placeholder }),
  ]
}
