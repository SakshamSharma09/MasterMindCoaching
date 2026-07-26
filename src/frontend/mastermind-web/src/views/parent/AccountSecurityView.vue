<template>
  <section class="mx-auto max-w-2xl">
    <div class="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm sm:p-8">
      <p class="text-sm font-bold uppercase tracking-[0.16em] text-indigo-600">Parent account</p>
      <h1 class="mt-2 text-3xl font-black text-slate-950">Account security</h1>
      <p class="mt-2 text-sm leading-6 text-slate-600">
        Your primary mobile is your login identity and can only be changed by the institute admin.
      </p>

      <div v-if="loading" class="mt-8 rounded-2xl bg-slate-50 p-5 text-sm text-slate-500">Loading account details...</div>

      <form v-else class="mt-8 space-y-6" @submit.prevent="saveEmail">
        <div>
          <label class="mb-2 block text-sm font-bold text-slate-700">Primary mobile</label>
          <input :value="details.primaryMobile" disabled
            class="w-full rounded-2xl border border-slate-200 bg-slate-100 px-4 py-3 text-slate-600">
          <p class="mt-2 text-xs text-slate-500">Contact MasterMind Coaching Classes if this number needs correction.</p>
        </div>

        <div v-if="details.secondaryMobile">
          <label class="mb-2 block text-sm font-bold text-slate-700">Secondary parent mobile</label>
          <input :value="details.secondaryMobile" disabled
            class="w-full rounded-2xl border border-slate-200 bg-slate-100 px-4 py-3 text-slate-600">
        </div>

        <div>
          <label for="security-email" class="mb-2 block text-sm font-bold text-slate-700">Recovery email</label>
          <input id="security-email" v-model.trim="email" type="email" required autocomplete="email"
            class="w-full rounded-2xl border border-slate-200 px-4 py-3 outline-none focus:border-indigo-500 focus:ring-4 focus:ring-indigo-100">
          <p class="mt-2 text-xs leading-5 text-slate-500">You can change your email here. Use Email OTP on the login page to verify it.</p>
        </div>

        <p v-if="message" class="rounded-2xl p-4 text-sm font-semibold"
          :class="failed ? 'bg-red-50 text-red-700' : 'bg-emerald-50 text-emerald-700'">{{ message }}</p>

        <div class="flex flex-wrap gap-3">
          <button :disabled="saving" class="rounded-2xl bg-indigo-600 px-5 py-3 font-bold text-white disabled:opacity-50">
            {{ saving ? 'Saving...' : 'Save recovery email' }}
          </button>
          <router-link to="/change-password" class="rounded-2xl border border-slate-200 px-5 py-3 font-bold text-slate-700">
            Change password
          </router-link>
        </div>
      </form>
    </div>
  </section>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { authService } from '@/services/authService'

const loading = ref(true)
const saving = ref(false)
const email = ref('')
const message = ref('')
const failed = ref(false)
const details = reactive({ primaryMobile: '', secondaryMobile: '' })

onMounted(async () => {
  try {
    const response = await authService.getAccountSecurity()
    email.value = response.data?.email || ''
    details.primaryMobile = response.data?.primaryMobile || ''
    details.secondaryMobile = response.data?.secondaryMobile || ''
  } catch (error: any) {
    failed.value = true
    message.value = error.response?.data?.message || 'Account details could not be loaded.'
  } finally {
    loading.value = false
  }
})

const saveEmail = async () => {
  saving.value = true
  failed.value = false
  message.value = ''
  try {
    const response = await authService.updateRecoveryEmail(email.value)
    message.value = response.message || 'Recovery email updated.'
  } catch (error: any) {
    failed.value = true
    message.value = error.response?.data?.message || 'Recovery email could not be updated.'
  } finally {
    saving.value = false
  }
}
</script>
