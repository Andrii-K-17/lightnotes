interface NoteText {
  title: string
  contentText: string
}

function firstLine(text: string): string {
  return (
    text
      .split('\n')
      .find((line) => line.trim() !== '')
      ?.trim() ?? ''
  )
}

export function displayTitle(note: NoteText): string {
  return note.title.trim() || firstLine(note.contentText)
}

/** Text under the title, without the line already used as the title. */
export function displayPreview(note: NoteText): string {
  const text = note.contentText.trim()

  return note.title.trim() ? text : text.slice(firstLine(text).length).trim()
}
