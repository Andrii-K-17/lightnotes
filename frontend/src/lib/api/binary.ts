import { authFetch } from './client'
import { ApiError } from './errors'

async function send(request: Request): Promise<Response> {
  try {
    return await authFetch(request)
  } catch {
    throw new ApiError(0, 'network_error')
  }
}

async function toError(response: Response): Promise<ApiError> {
  const retryAfter = Number.parseInt(response.headers.get('Retry-After') ?? '', 10)
  const body: unknown = await response.json().catch(() => null)

  return ApiError.fromBody(response.status, body, Number.isFinite(retryAfter) ? retryAfter : null)
}

/** Uploads raw bytes with PUT. */
export async function uploadBlob(path: string, blob: Blob): Promise<void> {
  const response = await send(
    new Request(path, {
      method: 'PUT',
      body: blob,
      headers: {
        'Content-Type': blob.type,
        'X-Requested-With': 'fetch',
      },
    }),
  )

  if (!response.ok) {
    throw await toError(response)
  }
}

/**
 * Downloads a file.
 * `null` when the server does not have it.
 * */
export async function downloadBlob(path: string): Promise<Blob | null> {
  const response = await send(new Request(path, { headers: { 'X-Requested-With': 'fetch' } }))

  if (response.status === 404) {
    return null
  }

  if (!response.ok) {
    throw await toError(response)
  }

  return response.blob()
}
