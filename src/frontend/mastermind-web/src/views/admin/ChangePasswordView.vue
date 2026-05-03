<template>
  <div class="max-w-2xl mx-auto">
    <div class="card-premium">
      <div class="mb-6">
        <h1 class="text-2xl font-bold text-surface-900">Change Password</h1>
        <p class="text-surface-500 mt-1">Update your admin account password.</p>
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
import authService from '@/services/authService'

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
    success.value = res?.message || 'Password updated successfully.'
    form.password = ''
    form.confirmPassword = ''
  } catch (e: any) {
    error.value = e?.response?.data?.message || 'Failed to update password.'
  } finally {
    isLoading.value = false
  }
}
</script>
