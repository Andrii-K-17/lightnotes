<script setup lang="ts">
import { RouterLink, type RouteLocationRaw } from 'vue-router'

defineProps<{
  active?: boolean
  to?: RouteLocationRaw
}>()
</script>

<template>
  <component
    :is="to ? RouterLink : 'button'"
    :to="to"
    :type="to ? undefined : 'button'"
    class="flex w-full items-center gap-3 rounded-xl px-3 py-2 text-left text-sm outline-none transition-colors hover:bg-accent/60 focus-visible:ring-2 focus-visible:ring-ring"
    :class="active ? 'bg-accent font-medium text-accent-foreground' : 'text-foreground/80'"
    :aria-current="active ? 'true' : undefined"
  >
    <slot name="icon" />
    <span class="min-w-0 flex-1 truncate">
      <slot />
    </span>
  </component>
</template>
