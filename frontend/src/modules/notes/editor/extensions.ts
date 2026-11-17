import StarterKit from '@tiptap/starter-kit'
import { Placeholder } from '@tiptap/extensions'
import { TaskItem, TaskList } from '@tiptap/extension-list'
import { ListDepthLimit } from './list-depth'
import { NoteImage } from './image-extension'

export function createExtensions(placeholder: string) {
  return [
    StarterKit.configure({
      link: {
        openOnClick: false,
        autolink: true,
        defaultProtocol: 'https',
      },
    }),
    TaskList,
    TaskItem.configure({
      nested: true,
      HTMLAttributes: { 'data-type': 'taskItem' },
    }),
    NoteImage,
    ListDepthLimit,
    Placeholder.configure({ placeholder }),
  ]
}
