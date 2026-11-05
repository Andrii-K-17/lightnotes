/** Apostrophes of every kind are dropped. */
const APOSTROPHES = /['\u2018\u2019\u02BC`\u00B4]/g
const WORD = /[\p{L}\p{N}]+/gu

/** Bounds the index size of very long notes. */
const MAX_TOKENS = 2000

export function normalize(text: string): string {
  return text.normalize('NFC').toLowerCase().replace(APOSTROPHES, '')
}

/** Unique lowercase words of the given texts, in order of appearance. */
export function tokenize(...texts: string[]): string[] {
  const tokens = new Set<string>()

  for (const text of texts) {
    for (const match of normalize(text).matchAll(WORD)) {
      tokens.add(match[0])

      if (tokens.size >= MAX_TOKENS) {
        return [...tokens]
      }
    }
  }

  return [...tokens]
}
