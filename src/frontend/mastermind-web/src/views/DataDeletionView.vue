<template>
  <main class="min-h-screen bg-slate-50 px-4 py-10 text-slate-900">
    <section class="mx-auto max-w-2xl rounded-3xl border border-slate-200 bg-white p-6 shadow-sm sm:p-10">
      <p class="text-sm font-bold uppercase tracking-[0.18em] text-[#6049e8]">MasterMind Coaching Classes</p>
      <h1 class="mt-2 text-3xl font-black">Request account and data deletion</h1>
      <p class="mt-4 leading-7 text-slate-600">
        Parents and teachers can request deletion here even after uninstalling the app. We will verify ownership,
        delete the account and associated personal data, and explain any fee, payment, security, or audit records
        that must be retained for legal or accounting purposes.
      </p>
      <form class="mt-8 space-y-5" @submit.prevent="submit">
        <div>
          <label for="deletion-id" class="mb-2 block text-sm font-bold">Registered email or mobile</label>
          <input id="deletion-id" v-model="identifier" required class="w-full rounded-2xl border border-slate-200 px-4 py-4">
        </div>
        <div>
          <label for="deletion-reason" class="mb-2 block text-sm font-bold">Reason (optional)</label>
          <textarea id="deletion-reason" v-model="reason" rows="4" class="w-full rounded-2xl border border-slate-200 px-4 py-4" />
        </div>
        <p v-if="message" class="rounded-2xl p-4 text-sm font-semibold" :class="failed ? 'bg-red-50 text-red-700' : 'bg-emerald-50 text-emerald-700'">{{ message }}</p>
        <button :disabled="loading" class="rounded-2xl bg-[#6049e8] px-6 py-4 font-black text-white disabled:opacity-50">
          {{ loading ? 'Sending…' : 'Request deletion' }}
        </button>
      </form>
      <router-link to="/privacy-policy" class="mt-8 inline-block font-bold text-[#6049e8]">Read the privacy policy</router-link>
    </section>
  </main>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { authService } from '@/services/authService'
const identifier = ref('')
const reason = ref('')
const loading = ref(false)
const failed = ref(false)
const message = ref('')
const submit = async () => {
  loading.value = true
  failed.value = false
  message.value = ''
  try {
    const response = await authService.requestPublicAccountDeletion(identifier.value, reason.value)
    message.value = response.message
  } catch (error: any) {
    failed.value = true
    message.value = error.response?.data?.message || 'The request could not be submitted.'
  } finally {
    loading.value = false
  }
}
</script>
