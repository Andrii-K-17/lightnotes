import type { components } from '@/lib/api'
import { tokenize } from '@/lib/search'
import type { FolderRecord, NoteRecord, TagRecord } from '../db'

type Schemas = components['schemas']

export type NoteDto = Schemas['NoteDto']
export type FolderDto = Schemas['FolderDto']
export type TagDto = Schemas['TagDto']

const toIso = (ms: number): string => new Date(ms).toISOString()
const toIsoOrNull = (ms: number | null): string | null => (ms === null ? null : toIso(ms))
const toMsOrNull = (value: string | null | undefined): number | null =>
  value ? Date.parse(value) : null

// ---- Convert local records to sync changes ----

export function folderChange(folder: FolderRecord): Schemas['FolderChange'] {
  return {
    id: folder.id,
    createdAt: toIso(folder.createdAt),
    name: folder.name,
    position: folder.position,
    deletedAt: toIsoOrNull(folder.deletedAt),
  }
}

export function tagChange(tag: TagRecord): Schemas['TagChange'] {
  return {
    id: tag.id,
    createdAt: toIso(tag.createdAt),
    name: tag.name,
    deletedAt: toIsoOrNull(tag.deletedAt),
  }
}

export function noteChange(note: NoteRecord): Schemas['NoteChange'] {
  const isNew = note.serverVersion === 0

  return {
    id: note.id,
    baseVersion: note.serverVersion,
    createdAt: toIso(note.createdAt),
    content:
      isNew || note.contentDirty
        ? {
            title: note.title,
            content: note.content,
            contentText: note.contentText,
            editedAt: toIso(note.editedAt),
          }
        : null,
    meta:
      isNew || note.metaDirty
        ? {
            folderId: note.folderId,
            tagIds: note.tagIds,
            pinned: note.pinned,
            deletedAt: toIsoOrNull(note.deletedAt),
            purgedAt: toIsoOrNull(note.purgedAt),
          }
        : null,
  }
}

// ---- Convert server entities to local records ----

export function folderRecord(dto: FolderDto): FolderRecord {
  return {
    id: dto.id,
    name: dto.name,
    position: dto.position,
    createdAt: Date.parse(dto.createdAt),
    deletedAt: toMsOrNull(dto.deletedAt),
    serverVersion: Number(dto.version),
    syncStatus: 'clean',
  }
}

export function tagRecord(dto: TagDto): TagRecord {
  return {
    id: dto.id,
    name: dto.name,
    nameLower: dto.name.toLowerCase(),
    createdAt: Date.parse(dto.createdAt),
    deletedAt: toMsOrNull(dto.deletedAt),
    serverVersion: Number(dto.version),
    syncStatus: 'clean',
  }
}

export function noteRecord(dto: NoteDto): NoteRecord {
  return {
    id: dto.id,
    folderId: dto.folderId ?? null,
    title: dto.title,
    content: dto.content,
    contentText: dto.contentText,
    pinned: dto.pinned,
    tagIds: dto.tagIds,
    searchTokens: tokenize(dto.title, dto.contentText),
    createdAt: Date.parse(dto.createdAt),
    editedAt: Date.parse(dto.editedAt),
    deletedAt: toMsOrNull(dto.deletedAt),
    purgedAt: toMsOrNull(dto.purgedAt),
    serverVersion: Number(dto.version),
    syncStatus: 'clean',
    contentDirty: false,
    metaDirty: false,
  }
}
