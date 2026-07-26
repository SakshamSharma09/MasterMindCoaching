<template>
  <main class="flex min-h-screen items-center justify-center bg-[#f7f9fc] px-4 py-10">
    <section class="w-full max-w-md rounded-[2rem] border border-slate-200 bg-white p-6 shadow-xl sm:p-8">
      <p class="text-sm font-bold uppercase tracking-[0.18em] text-[#6049e8]">Parent invitation</p>
      <h1 class="mt-2 text-3xl font-black text-slate-950">Set your password</h1>
      <p class="mt-3 text-sm leading-6 text-slate-600">
        Create a password once, then use your registered mobile number for future logins.
      </p>

      <div v-if="checking" class="mt-6 rounded-2xl bg-slate-50 p-4 text-sm text-slate-600">
        Checking your invitation…
      </div>

      <form v-else-if="valid" class="mt-6 space-y-5" @submit.prevent="accept">
        <p class="rounded-2xl bg-indigo-50 p-4 text-sm text-slate-700">
          Account: <strong>{{ invitationName }}</strong><br>
          Mobile: <strong>{{ maskedMobile }}</strong>
        </p>
        <div>
          <label for="new-password" class="mb-2 block text-sm font-bold text-slate-700">New password</label>
          <input id="new-password" v-model="password" type="password" minlength="8" required autocomplete="new-password"
            class="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-4 outline-none focus:border-[#6049e8] focus:ring-4 focus:ring-[#6049e8]/10">
        </div>
        <div>
          <label for="confirm-password" class="mb-2 block text-sm font-bold text-slate-700">Confirm password</label>
          <input id="confirm-password" v-model="confirmPassword" type="password" minlength="8" required autocomplete="new-password"
            class="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-4 outline-none focus:border-[#6049e8] focus:ring-4 focus:ring-[#6049e8]/10">
        </div>
        <p v-if="error" class="rounded-2xl bg-red-50 p-4 text-sm font-semibold text-red-700">{{ error }}</p>
        <button :disabled="saving" class="w-full rounded-2xl bg-[#6049e8] px-5 py-4 font-black text-white disabled:opacity-50">
          {{ saving ? 'Saving…' : 'Set password' }}
        </button>
      </form>

      <div v-else class="mt-6 rounded-2xl border border-red-200 bg-red-50 p-4 text-sm text-red-700">
        {{ error }}
      </div>
    </section>
  </main>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { authService } from '@/services/authService'

const route = useRoute()
const router = useRouter()
const token = String(route.query.token || '')
const checking = ref(true)
const valid = ref(false)
const saving = ref(false)
const password = ref('')
const confirmPassword = ref('')
const maskedMobile = ref('')
const invitationName = ref('Parent')
const error = ref('')

onMounted(async () => {
  if (!token) {
    error.value = 'Invitation token is missing.'
    checking.value = false
    return
  }
  try {
    const response = await authService.validateInvitation(token)
    maskedMobile.value = response.data?.mobile || ''
    invitationName.value = response.data?.name || 'Parent'
    valid.value = true
  } catch (err: any) {
    error.value = err.response?.data?.message || 'This invitation is invalid or expired.'
  } finally {
    checking.value = false
  }
})

const accept = async () => {
  error.value = ''
  if (password.value !== confirmPassword.value) {
    error.value = 'Passwords do not match.'
    return
  }
  saving.value = true
  try {
    const response = await authService.acceptInvitation(token, password.value)
    await router.replace({ name: 'Login', query: { mobile: response.data?.mobile || '', invited: '1' } })
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Password could not be set.'
  } finally {
    saving.value = false
  }
}
</script>
