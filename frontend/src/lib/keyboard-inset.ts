import { useEventListener } from '@vueuse/core'
import { ref } from 'vue'

export function useKeyboardInset() {
  const inset = ref(0)
  const viewport = window.visualViewport

  function update(): void {
    if (!viewport) {
      return
    }

    inset.value =
      viewport.scale > 1.01
        ? 0
        : Math.max(0, Math.round(window.innerHeight - viewport.height - viewport.offsetTop))
  }

  if (viewport) {
    useEventListener(viewport, 'resize', update)
    useEventListener(viewport, 'scroll', update)
    update()
  }

  return inset
}
