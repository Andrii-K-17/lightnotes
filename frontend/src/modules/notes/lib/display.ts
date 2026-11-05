export { displayPreview, displayTitle } from '@/lib/note-text'

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
