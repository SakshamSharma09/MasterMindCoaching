<template>
  <div class="max-w-2xl mx-auto">
    <div class="card-premium">
      <div class="mb-6">
        <h1 class="text-2xl font-bold text-surface-900">Change Password</h1>
        <p class="text-surface-500 mt-1">Update the password for your account.</p>
      </div>

      <div v-if="authStore.user?.mobile" class="mb-5 rounded-2xl border border-primary-100 bg-primary-50/70 p-4">
        <p class="text-sm font-semibold text-primary-900">Mobile password login number</p>
        <p class="mt-1 text-lg font-bold text-primary-700">{{ authStore.user.mobile }}</p>
        <p class="mt-1 text-sm text-primary-700/80">Use this mobile number with the password you set here.</p>
      </div>

      <form class="space-y-4" @submit.prevent="submit">
        <div>
          <label class="block text-sm font-medium text-surface-700 mb-2">New Password</label>
          <input
            v-model="form.password"
            type="password"
            minlength="6"
            required
            class="w-full rounded-xl border border-surface-300 px-4 py-3 focus:outline-none focus:ring-2 focus:ring-primary-500/30 focus:border-primary-500"
          />
        </div>

        <div>
          <label class="block text-sm font-medium text-surface-700 mb-2">Confirm Password</label>
          <input
            v-model="form.confirmPassword"
            type="password"
            minlength="6"
            required
            class="w-full rounded-xl border border-surface-300 px-4 py-3 focus:outline-none focus:ring-2 focus:ring-primary-500/30 focus:border-primary-500"
          />
        </div>

        <p v-if="error" class="text-sm text-error-600">{{ error }}</p>
        <p v-if="success" class="text-sm text-success-600">{{ success }}</p>

        <button
          type="submit"
          :disabled="isLoading"
          class="btn-premium px-5 py-3"
        >
          {{ isLoading ? 'Updating...' : 'Update Password' }}
        </button>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import authService from '@/services/authService'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const form = reactive({
  password: '',
  confirmPassword: ''
})

const isLoading = ref(false)
const error = ref('')
const success = ref('')

const submit = async () => {
  error.value = ''
  success.value = ''
  if (form.password !== form.confirmPassword) {
    error.value = 'Passwords do not match.'
    return
  }

  isLoading.value = true
  try {
    const res: any = await authService.setPassword(form.password, form.confirmPassword)
    const shouldReturnToLogin = authStore.userRole !== 'Admin'
    success.value = shouldReturnToLogin
      ? 'Password saved. Redirecting you to login...'
      : (res?.message || 'Password updated successfully.')
    form.password = ''
    form.confirmPassword = ''

    if (shouldReturnToLogin) {
      window.setTimeout(async () => {
        await authStore.logout()
        router.push({ name: 'Login' })
      }, 900)
    }
  } catch (e: any) {
    error.value = e?.response?.data?.message || 'Failed to update password.'
  } finally {
    isLoading.value = false
  }
}
</script>
