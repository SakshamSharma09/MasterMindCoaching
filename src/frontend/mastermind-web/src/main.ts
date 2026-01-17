import { createApp } from 'vue'
import { createPinia } from 'pinia'
import piniaPluginPersistedState from 'pinia-plugin-persistedstate'
import router from './router'
import App from './App.vue'

import './assets/styles/main.css'

const app = createApp(App)
const pinia = createPinia()

pinia.use(piniaPluginPersistedState)

app.use(pinia)
app.use(router)

app.mount('#app')