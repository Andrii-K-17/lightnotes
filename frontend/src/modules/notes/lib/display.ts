import type { NoteRecord } from '@/data'

function firstLine(text: string): string {
  return (
    text
      .split('\n')
      .find((line) => line.trim() !== '')
      ?.trim() ?? ''
  )
}

/** Title for lists: the title, otherwise the first line of the text. */
export function displayTitle(note: Pick<NoteRecord, 'title' | 'contentText'>): string {
  return note.title.trim() || firstLine(note.contentText)
}

export function displayPreview(note: Pick<NoteRecord, 'title' | 'contentText'>): string {
  const text = note.contentText.trim()
  return note.title.trim() ? text : text.slice(firstLine(text).length).trim()
}

const sameYear = new Intl.DateTimeFormat(undefined, { month: 'short', day: 'numeric' })
const otherYear = new Intl.DateTimeFormat(undefined, {
  year: 'numeric',
  month: 'short',
  day: 'numeric',
})
const time = new Intl.DateTimeFormat(undefined, { hour: '2-digit', minute: '2-digit' })

export function formatEditedAt(timestamp: number, now = new Date()): string {
  const date = new Date(timestamp)

  if (date.toDateString() === now.toDateString()) {
    return time.format(date)
  }

  return (date.getFullYear() === now.getFullYear() ? sameYear : otherYear).format(date)
}
