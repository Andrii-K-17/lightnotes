import type { InjectionKey } from 'vue'

/**
 * The note being edited.
 * Image views need it to cache downloaded files.
 */
export const noteIdKey: InjectionKey<string> = Symbol('noteId')
