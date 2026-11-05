import { newId } from '@/lib/id'
import { tokenize } from '@/lib/search'
import { getDb, type NoteRecord } from './db'
import { DEFAULT_SORT, noteRepository } from './repositories/note-repository'

const WORDS = [
  'нотатка',
  'проєкт',
  'зустріч',
  'ідея',
  'список',
  'покупки',
  'подорож',
  'книга',
  'рецепт',
  'завдання',
  'план',
  'бюджет',
  'здоров’я',
  'музика',
  'код',
  'дизайн',
  'м’ясо',
  'ґанок',
  'notebook',
  'project',
  'meeting',
  'idea',
  'shopping',
  'travel',
  'recipe',
  'budget',
  'health',
  'music',
  'design',
  'roadmap',
  'summary',
  'draft',
  'review',
  'release',
  'backlog',
  'sprint',
]

const pick = () => WORDS[Math.floor(Math.random() * WORDS.length)]!
const words = (count: number) => Array.from({ length: count }, pick).join(' ')

function makeNote(): NoteRecord {
  const paragraphs = Array.from({ length: 5 }, () => words(30))
  const createdAt = Date.now() - Math.floor(Math.random() * 2 * 365 * 86_400_000)
  const title = words(3)
  const contentText = paragraphs.join('\n')

  return {
    id: newId(),
    folderId: null,
    title,
    content: JSON.stringify({
      type: 'doc',
      content: paragraphs.map((text) => ({ type: 'paragraph', content: [{ type: 'text', text }] })),
    }),
    contentText,
    pinned: false,
    tagIds: [],
    searchTokens: tokenize(title, contentText),
    createdAt,
    editedAt: createdAt,
    deletedAt: null,
    purgedAt: null,
    serverVersion: 0,
    syncStatus: 'dirty',
  }
}

/** Adds synthetic notes. Dev accounts only: they are real, unsynced notes until cleared. */
async function seedNotes(count = 10_000): Promise<void> {
  for (let done = 0; done < count; done += 1000) {
    await getDb().notes.bulkAdd(Array.from({ length: Math.min(1000, count - done) }, makeNote))
  }

  console.log(`Added ${count} notes`)
}

async function clearNotes(): Promise<void> {
  await getDb().notes.clear()
}

async function benchSearch(text: string, runs = 5): Promise<void> {
  const times: number[] = []
  let found = 0

  for (let run = 0; run < runs; run++) {
    const start = performance.now()

    found = (
      await noteRepository.query({ scope: { kind: 'all' }, tagIds: [], text, sort: DEFAULT_SORT })
    ).length
    times.push(performance.now() - start)
  }

  times.sort((a, b) => a - b)
  console.log(
    `"${text}": ${found} notes, median ${times[Math.floor(runs / 2)]!.toFixed(1)} ms ` +
      `(min ${times[0]!.toFixed(1)}, max ${times[runs - 1]!.toFixed(1)})`,
  )
}

/** Exposes helpers as `lightnotes.*` in the browser console. */
export function installDevTools(): void {
  Object.assign(window, { lightnotes: { seedNotes, clearNotes, benchSearch } })
}
