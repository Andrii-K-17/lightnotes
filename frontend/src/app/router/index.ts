import { createRouter, createWebHistory } from 'vue-router'
import { notesRoutes } from '@/modules/notes'
import { authRoutes } from '@/modules/auth'
import { installGuards } from './guards'
import { settingsRoutes } from '@/modules/settings'

export const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [...notesRoutes, ...authRoutes, ...settingsRoutes],
})

installGuards(router)
