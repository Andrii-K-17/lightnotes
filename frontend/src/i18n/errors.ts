import { i18n } from './index'

/** Localized text for an API error code. */
export function errorMessage(code: string): string {
  const { t, te } = i18n.global
  const key = `errors.${code}`

  return te(key) ? t(key) : t('errors.unknown')
}
