import { createRouter, createWebHistory } from 'vue-router'
import { notesRoutes } from '@/modules/notes'
import { authRoutes } from '@/modules/auth'
import { installGuards } from './guards'

export const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [...notesRoutes, ...authRoutes],
})

installGuards(router)
