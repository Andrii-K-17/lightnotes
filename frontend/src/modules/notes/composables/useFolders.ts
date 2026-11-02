import { inject, provide, type InjectionKey, type ShallowRef } from 'vue'
import { folderRepository, type FolderRecord } from '@/data'
import { useLiveQuery } from '@/lib/live-query'

type Folders = Readonly<ShallowRef<FolderRecord[] | undefined>>

const key: InjectionKey<Folders> = Symbol('folders')

/** Provides the reactive folder list to descendant components. */
export function provideFolders(): Folders {
  const { data } = useLiveQuery(() => folderRepository.list())

  provide(key, data)

  return data
}

/** Returns the provided folder list. */
export function useFolders(): Folders {
  const folders = inject(key)

  if (!folders) {
    throw new Error('Folders are provided by NotesShell.')
  }

  return folders
}
