import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { i18n } from '@/i18n'
import '@fontsource-variable/plus-jakarta-sans'
import './main.css'

import App from '@/app/App.vue'
import { registerPwa } from '@/app/pwa'
import { router } from '@/app/router'
import { listenForInstall } from '@/lib/pwa-install'
import { installAuth } from './modules/auth'

const app = createApp(App)

app.use(createPinia())
installAuth(() =>
  router.replace({
    name: 'login',
    query: { redirect: router.currentRoute.value.fullPath },
  }),
)
app.use(router)
app.use(i18n)

listenForInstall()
registerPwa()

app.mount('#app')
