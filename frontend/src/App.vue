<script setup lang="ts">
import { Button } from '@/components/ui/button'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
</script>

<template>
  <div class="p-4">
    <Button>{{ t('app.name') }}</Button>
  </div>
</template>
