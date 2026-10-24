import { createRouter, createWebHistory } from 'vue-router'
import { notesRoutes } from '@/modules/notes'

export const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [...notesRoutes],
})
