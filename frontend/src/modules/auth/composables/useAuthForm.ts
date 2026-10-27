import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ApiError } from '@/lib/api'

/** Provides shared form state and submission handling for authentication pages. */
export function useAuthForm(action: (email: string, password: string) => Promise<void>) {
  const route = useRoute()
  const router = useRouter()

  const email = ref('')
  const password = ref('')
  const errorCode = ref<string | null>(null)
  const busy = ref(false)

  /** Runs the auth action, navigates to the requested page, and records any error. */
  async function submit(): Promise<void> {
    if (busy.value) {
      return
    }

    busy.value = true
    errorCode.value = null

    try {
      await action(email.value.trim(), password.value)
      await router.replace(redirectTarget(route.query.redirect))
    } catch (error) {
      errorCode.value = error instanceof ApiError ? error.code : 'unknown'
    } finally {
      busy.value = false
    }
  }

  return {
    email,
    password,
    errorCode,
    busy,
    submit,
  }
}

/** Returns a safe in-app redirect path, falling back to the home page. */
function redirectTarget(redirect: unknown): string {
  return typeof redirect === 'string' && redirect.startsWith('/') && !redirect.startsWith('//')
    ? redirect
    : '/'
}
