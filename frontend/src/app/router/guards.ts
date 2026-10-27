import type { Router } from 'vue-router'
import { useSessionStore } from '@/modules/auth'

export function installGuards(router: Router): void {
  router.beforeEach(async (to) => {
    const session = useSessionStore()

    await session.restore()

    if (to.meta.guestOnly && session.isAuthenticated) {
      return { name: 'home' }
    }

    if (!to.meta.public && !session.isAuthenticated) {
      return { name: 'login', query: { redirect: to.fullPath } }
    }
  })
}
