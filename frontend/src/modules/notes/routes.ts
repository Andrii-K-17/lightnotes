import type { RouteRecordRaw } from 'vue-router'

export const notesRoutes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('./layout/NotesShell.vue'),
    children: [
      {
        path: '',
        name: 'home',
        component: () => import('./pages/NoNoteSelected.vue'),
      },
      {
        path: 'notes/:id',
        name: 'note',
        component: () => import('./pages/NoteEditorPage.vue'),
        props: true,
      },
    ],
  },
]
