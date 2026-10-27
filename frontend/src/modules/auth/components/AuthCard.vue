<script setup lang="ts">
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'

defineProps<{ title: string; description: string }>()
</script>

<template>
  <Card>
    <CardHeader>
      <CardTitle>
        {{ title }}
      </CardTitle>
      <CardDescription>
        {{ description }}
      </CardDescription>
    </CardHeader>

    <CardContent>
      <slot />
    </CardContent>

    <CardFooter class="grid gap-4">
      <slot name="footer" />
    </CardFooter>
  </Card>
</template>
