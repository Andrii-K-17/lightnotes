import { ApiError, authFetch } from '@/lib/api'

export type AiAction = 'improve' | 'summarize' | 'translate'

export interface AiRequest {
  action: AiAction
  text: string
  /** Target language of a translation. */
  language?: string
}

/** One server-sent event: lines `event: name` and `data: ...`. */
function parseEvent(raw: string): { event: string; data: string } {
  let event = 'message'
  const data: string[] = []

  for (const line of raw.split('\n')) {
    if (line.startsWith('event:')) {
      event = line.slice(6).trim()
    } else if (line.startsWith('data:')) {
      data.push(line.slice(5).replace(/^ /, ''))
    }
  }

  return {
    event,
    data: data.join('\n'),
  }
}

/** Runs an AI action and reports the reply piece by piece. */
export async function runAiAction(
  request: AiRequest,
  onDelta: (text: string) => void,
  signal: AbortSignal,
): Promise<void> {
  let response: Response

  try {
    response = await authFetch(
      new Request('/api/v1/ai/actions', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'X-Requested-With': 'fetch',
        },
        body: JSON.stringify(request),
        signal,
      }),
    )
  } catch (error) {
    throw signal.aborted ? error : new ApiError(0, 'network_error')
  }

  if (!response.ok) {
    throw ApiError.fromBody(response.status, await response.json().catch(() => null))
  }

  if (!response.body) {
    throw new ApiError(0, 'network_error')
  }

  const reader = response.body.pipeThrough(new TextDecoderStream()).getReader()
  let buffer = ''

  try {
    for (;;) {
      const { done, value } = await reader.read()

      if (done) {
        return
      }

      buffer = (buffer + value).replaceAll('\r\n', '\n')

      for (let end = buffer.indexOf('\n\n'); end !== -1; end = buffer.indexOf('\n\n')) {
        const { event, data } = parseEvent(buffer.slice(0, end))

        buffer = buffer.slice(end + 2)

        if (event === 'delta') {
          onDelta(JSON.parse(data) as string)
        } else if (event === 'error') {
          throw new ApiError(502, JSON.parse(data) as string)
        } else if (event === 'done') {
          return
        }
      }
    }
  } catch (error) {
    if (error instanceof ApiError || signal.aborted) {
      throw error
    }

    throw new ApiError(0, 'network_error')
  }
}
