import type { RouteRecordRaw } from 'vue-router'

export const notesRoutes: RouteRecordRaw[] = [
  { path: '/', name: 'home', component: () => import('./pages/NotesPage.vue') },
]
