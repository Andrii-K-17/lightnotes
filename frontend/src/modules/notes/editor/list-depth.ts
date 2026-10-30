import { Extension } from '@tiptap/vue-3'
import { Editor } from '@tiptap/core'

export const MAX_LIST_DEPTH = 3

const LIST_TYPES = new Set(['bulletList', 'orderedList', 'taskList'])

/** Number of lists the cursor is nested in. */
export function listDepth(editor: Editor): number {
  const { $from } = editor.state.selection
  let depth = 0

  for (let level = $from.depth; level > 0; level--) {
    if (LIST_TYPES.has($from.node(level).type.name)) {
      depth++
    }
  }

  return depth
}

/** Stops Tab from nesting lists deeper than `MAX_LIST_DEPTH`. */
export const ListDepthLimit = Extension.create({
  name: 'listDepthLimit',
  priority: 1000,

  addKeyboardShortcuts() {
    return {
      Tab: ({ editor }) => listDepth(editor) >= MAX_LIST_DEPTH,
    }
  },
})
