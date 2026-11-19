import StarterKit from '@tiptap/starter-kit'
import { Placeholder } from '@tiptap/extensions'
import { TaskItem, TaskList } from '@tiptap/extension-list'
import CodeBlockLowlight from '@tiptap/extension-code-block-lowlight'
import { common, createLowlight } from 'lowlight'
import { ListDepthLimit } from './list-depth'
import { NoteImage } from './image-extension'

export function createExtensions(placeholder: string) {
  const lowlight = createLowlight(common)

  return [
    StarterKit.configure({
      codeBlock: false,
      link: {
        openOnClick: false,
        autolink: true,
        defaultProtocol: 'https',
      },
    }),
    CodeBlockLowlight.configure({
      lowlight,
      defaultLanguage: 'plaintext',
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
