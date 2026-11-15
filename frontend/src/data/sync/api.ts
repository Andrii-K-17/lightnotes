import { api, unwrap } from '@/lib/api'
import type { components } from '@/lib/api'

type Schemas = components['schemas']

export type PushRequest = Schemas['PushRequest']
export type PushResponse = Schemas['PushResponse']
export type PullResponse = Schemas['PullResponse']

export const syncApi = {
  push: (body: PushRequest) => unwrap(api.POST('/api/v1/sync/push', { body })),
  pull: (since: number) => unwrap(api.GET('/api/v1/sync/pull', { params: { query: { since } } })),
}
