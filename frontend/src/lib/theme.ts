import { useColorMode } from '@vueuse/core'

export function useTheme() {
  return useColorMode({ emitAuto: true })
}
