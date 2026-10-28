import type { RouteRecordRaw } from 'vue-router'

export const notesRoutes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'home',
    component: () => import('./pages/NotesPage.vue'),
  },
  {
    path: '/notes/:id',
    name: 'note',
    component: () => import('./pages/NoteEditorPage.vue'),
    props: true,
  },
]
