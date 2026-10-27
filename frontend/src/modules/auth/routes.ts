import type { RouteRecordRaw } from 'vue-router'

const guest = { layout: 'auth', public: true, guestOnly: true } as const

export const authRoutes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'login',
    component: () => import('./pages/LoginPage.vue'),
    meta: guest,
  },
  {
    path: '/register',
    name: 'register',
    component: () => import('./pages/RegisterPage.vue'),
    meta: guest,
  },
]
