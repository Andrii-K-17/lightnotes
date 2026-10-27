import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { i18n } from '@/i18n'
import './main.css'

import App from '@/app/App.vue'
import { router } from '@/app/router'
import { installAuth } from './modules/auth'

const app = createApp(App)

app.use(createPinia())
installAuth(() => router.replace({ name: 'login' }))
app.use(router)
app.use(i18n)

app.mount('#app')
