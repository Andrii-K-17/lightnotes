import { inject, provide, type InjectionKey, type ShallowRef } from 'vue'
import { tagRepository, type TagRecord } from '@/data'
import { useLiveQuery } from '@/lib/live-query'

type Tags = Readonly<ShallowRef<TagRecord[] | undefined>>

const key: InjectionKey<Tags> = Symbol('tags')

export function provideTags(): Tags {
  const { data } = useLiveQuery(() => tagRepository.list())

  provide(key, data)

  return data
}

export function useTags(): Tags {
  const tags = inject(key)
  if (!tags) {
    throw new Error('Tags are provided by NotesShell.')
  }

  return tags
}
