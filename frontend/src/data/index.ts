export {
  closeDatabase,
  openDatabase,
  type AttachmentRecord,
  type FolderRecord,
  type NoteRecord,
  type RichText,
  type SyncStatus,
  type TagRecord,
} from './db'
export {
  DEFAULT_SORT,
  EMPTY_CONTENT,
  EMPTY_DOC,
  noteRepository,
  parseContent,
  type NoteEdit,
  type NoteQuery,
  type NoteScope,
  type NoteSort,
  MAX_TAGS_PER_NOTE,
} from './repositories/note-repository'
export { FOLDER_NAME_MAX_LENGTH, folderRepository } from './repositories/folder-repository'
export { TAG_NAME_MAX_LENGTH, tagRepository } from './repositories/tag-repository'
