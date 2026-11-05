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
  EMPTY_CONTENT,
  EMPTY_DOC,
  noteRepository,
  parseContent,
  type NoteEdit,
  type NoteScope,
} from './repositories/note-repository'
export { FOLDER_NAME_MAX_LENGTH, folderRepository } from './repositories/folder-repository'
