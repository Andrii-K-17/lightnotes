/** Represents an error returned by the API. */
export class ApiError extends Error {
  readonly status: number
  readonly code: string
  readonly fieldErrors: Record<string, string[]>

  constructor(status: number, code: string, fieldErrors: Record<string, string[]> = {}) {
    super(code)
    this.name = 'ApiError'
    this.status = status
    this.code = code
    this.fieldErrors = fieldErrors
  }

  /** Builds an API error from an HTTP response body. */
  static fromBody(status: number, body: unknown): ApiError {
    const problem = isRecord(body) ? body : {}
    const code = typeof problem.code === 'string' ? problem.code : `http.${status}`

    return new ApiError(status, code, readFieldErrors(problem.errors))
  }
}

/** Checks whether a value is a string. */
function isString(value: unknown): value is string {
  return typeof value === 'string'
}

/** Checks whether a value is a non-null object. */
function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

/** Extracts string validation messages keyed by normalized field names. */
function readFieldErrors(errors: unknown): Record<string, string[]> {
  const result: Record<string, string[]> = {}

  if (!isRecord(errors)) {
    return result
  }

  for (const [field, messages] of Object.entries(errors)) {
    if (Array.isArray(messages)) {
      result[field.charAt(0).toLowerCase() + field.slice(1)] = messages.filter(isString)
    }
  }

  return result
}
