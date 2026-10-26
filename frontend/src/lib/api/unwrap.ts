import { ApiError } from './errors'

/** Result shape returned by the API client. */
interface Settled<TData> {
  data?: TData
  error?: unknown
  response: Response
}

/** Returns response data or throws an API error. */
export async function unwrap<TData>(request: Promise<Settled<TData>>): Promise<TData> {
  let settled: Settled<TData>

  try {
    settled = await request
  } catch {
    throw new ApiError(0, 'network_error')
  }

  if (!settled.response.ok) {
    throw ApiError.fromBody(settled.response.status, settled.error)
  }

  return settled.data as TData
}

/** Resolves an API request that does not return data. */
export async function unwrapEmpty(request: Promise<Settled<unknown>>): Promise<void> {
  await unwrap(request)
}
