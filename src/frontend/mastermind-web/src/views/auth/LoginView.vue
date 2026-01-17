<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <!-- Header -->
      <div class="text-center">
        <div class="mx-auto h-16 w-16 bg-indigo-600 rounded-full flex items-center justify-center">
          <svg class="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.746 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"></path>
          </svg>
        </div>
        <h2 class="mt-6 text-3xl font-extrabold text-gray-900">
          Welcome to MasterMind
        </h2>
        <p class="mt-2 text-sm text-gray-600">
          Coaching Classes Management System
        </p>
      </div>

      <!-- Login Form -->
      <div class="bg-white py-8 px-6 shadow-lg rounded-lg">
        <form @submit.prevent="handleSubmit" class="space-y-6">
          <!-- Identifier Input -->
          <div>
            <label for="identifier" class="block text-sm font-medium text-gray-700">
              Email or Mobile Number
            </label>
            <div class="mt-1 relative">
              <input
                id="identifier"
                v-model="form.identifier"
                type="text"
                required
                class="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm"
                placeholder="Enter email or mobile number"
                :disabled="isLoading"
              />
            </div>
          </div>

          <!-- Error Message -->
          <div v-if="error" class="rounded-md bg-red-50 p-4">
            <div class="flex">
              <div class="flex-shrink-0">
                <svg class="h-5 w-5 text-red-400" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
                </svg>
              </div>
              <div class="ml-3">
                <p class="text-sm font-medium text-red-800">
                  {{ error }}
                </p>
              </div>
            </div>
          </div>

          <!-- Submit Button -->
          <div>
            <button
              type="submit"
              :disabled="isLoading || !form.identifier.trim()"
              class="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <span v-if="isLoading" class="absolute left-1/2 transform -translate-x-1/2">
                <svg class="animate-spin h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
              </span>
              <span v-else>
                Send OTP
              </span>
            </button>
          </div>
        </form>

        <!-- Demo Credentials -->
        <div class="mt-6 border-t border-gray-200 pt-6">
          <div class="text-sm text-gray-600">
            <p class="font-medium mb-2">Demo Credentials:</p>
            <div class="space-y-1 text-xs">
              <p><strong>Admin:</strong> admin@mastermind.com</p>
              <p><strong>Teacher:</strong> teacher@mastermind.com</p>
              <p><strong>Parent:</strong> parent@mastermind.com</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const form = reactive({
  identifier: ''
})

const isLoading = ref(false)
const error = ref<string | null>(null)

const handleSubmit = async () => {
  if (!form.identifier.trim()) return

  isLoading.value = true
  error.value = null

  try {
    // Determine if identifier is email or mobile
    const isEmail = form.identifier.includes('@')
    const type = isEmail ? 'email' : 'mobile'

    await authStore.requestOtp(form.identifier, type)

    // Navigate to OTP verification
    router.push({
      name: 'OtpVerify',
      query: {
        identifier: form.identifier,
        type
      }
    })
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Failed to send OTP. Please try again.'
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
/* Additional styles if needed */
</style>