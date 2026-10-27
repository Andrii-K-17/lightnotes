import { api, unwrap, unwrapEmpty } from '@/lib/api'

export const authApi = {
  register: (email: string, password: string) =>
    unwrapEmpty(api.POST('/api/v1/auth/register', { body: { email, password } })),

  login: (email: string, password: string) =>
    unwrap(api.POST('/api/v1/auth/login', { body: { email, password } })),

  refresh: () => unwrap(api.POST('/api/v1/auth/refresh')),

  logout: () => unwrapEmpty(api.POST('/api/v1/auth/logout')),

  me: () => unwrap(api.GET('/api/v1/me')),
}
