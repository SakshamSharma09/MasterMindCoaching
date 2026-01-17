<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <!-- Header -->
      <div class="text-center">
        <div class="mx-auto h-16 w-16 bg-indigo-600 rounded-full flex items-center justify-center">
          <svg class="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
          </svg>
        </div>
        <h2 class="mt-6 text-3xl font-extrabold text-gray-900">
          Verify Your Account
        </h2>
        <p class="mt-2 text-sm text-gray-600">
          We've sent a 6-digit OTP to <strong>{{ identifier }}</strong>
        </p>
      </div>

      <!-- OTP Form -->
      <div class="bg-white py-8 px-6 shadow-lg rounded-lg">
        <form @submit.prevent="handleSubmit" class="space-y-6">
          <!-- OTP Input -->
          <div>
            <label for="otp" class="block text-sm font-medium text-gray-700">
              Enter OTP
            </label>
            <div class="mt-1">
              <input
                id="otp"
                v-model="form.otp"
                type="text"
                maxlength="6"
                required
                class="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm text-center text-2xl tracking-widest"
                placeholder="000000"
                :disabled="isLoading"
                @input="handleOtpInput"
              />
            </div>
            <p class="mt-2 text-sm text-gray-500">
              OTP expires in {{ countdown }} seconds
            </p>
          </div>

          <!-- User Registration Fields (for new users) -->
          <div v-if="isNewUser" class="space-y-4 border-t border-gray-200 pt-4">
            <h3 class="text-lg font-medium text-gray-900">Complete Your Registration</h3>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <label for="firstName" class="block text-sm font-medium text-gray-700">
                  First Name
                </label>
                <input
                  id="firstName"
                  v-model="form.firstName"
                  type="text"
                  required
                  class="mt-1 appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                  placeholder="John"
                  :disabled="isLoading"
                />
              </div>
              <div>
                <label for="lastName" class="block text-sm font-medium text-gray-700">
                  Last Name
                </label>
                <input
                  id="lastName"
                  v-model="form.lastName"
                  type="text"
                  required
                  class="mt-1 appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                  placeholder="Doe"
                  :disabled="isLoading"
                />
              </div>
            </div>

            <div>
              <label for="role" class="block text-sm font-medium text-gray-700">
                Role
              </label>
              <select
                id="role"
                v-model="form.role"
                required
                class="mt-1 block w-full px-3 py-2 border border-gray-300 bg-white rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                :disabled="isLoading"
              >
                <option value="">Select Role</option>
                <option value="Admin">Administrator</option>
                <option value="Teacher">Teacher</option>
                <option value="Parent">Parent</option>
              </select>
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
              :disabled="isLoading || !form.otp || (isNewUser && (!form.firstName || !form.lastName || !form.role))"
              class="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <span v-if="isLoading" class="absolute left-1/2 transform -translate-x-1/2">
                <svg class="animate-spin h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
              </span>
              <span v-else>
                {{ isNewUser ? 'Complete Registration' : 'Verify OTP' }}
              </span>
            </button>
          </div>

          <!-- Resend OTP -->
          <div class="text-center">
            <button
              type="button"
              @click="resendOtp"
              :disabled="isResending || countdown > 0"
              class="text-sm text-indigo-600 hover:text-indigo-500 disabled:text-gray-400 disabled:cursor-not-allowed"
            >
              <span v-if="isResending">Resending...</span>
              <span v-else-if="countdown > 0">Resend OTP in {{ countdown }}s</span>
              <span v-else>Resend OTP</span>
            </button>
          </div>
        </form>

        <!-- Back to Login -->
        <div class="mt-6 text-center">
          <router-link
            to="/login"
            class="text-sm text-gray-600 hover:text-gray-900"
          >
            ← Back to Login
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const identifier = ref(route.query.identifier as string || '')
const type = ref(route.query.type as string || 'email')

const form = reactive({
  otp: '',
  firstName: '',
  lastName: '',
  role: '' as 'Admin' | 'Teacher' | 'Parent' | ''
})

const isLoading = ref(false)
const isResending = ref(false)
const error = ref<string | null>(null)
const countdown = ref(60)
const isNewUser = ref(false)
const countdownInterval = ref<NodeJS.Timeout | null>(null)

const handleOtpInput = (event: Event) => {
  const target = event.target as HTMLInputElement
  // Only allow numbers
  target.value = target.value.replace(/\D/g, '')
  form.otp = target.value
}

const startCountdown = () => {
  countdown.value = 60
  countdownInterval.value = setInterval(() => {
    countdown.value--
    if (countdown.value <= 0) {
      if (countdownInterval.value) {
        clearInterval(countdownInterval.value)
        countdownInterval.value = null
      }
    }
  }, 1000)
}

const handleSubmit = async () => {
  if (!form.otp || form.otp.length !== 6) {
    error.value = 'Please enter a valid 6-digit OTP'
    return
  }

  if (isNewUser.value && (!form.firstName || !form.lastName || !form.role)) {
    error.value = 'Please fill in all required fields'
    return
  }

  isLoading.value = true
  error.value = null

  try {
    const userData = isNewUser.value ? {
      firstName: form.firstName,
      lastName: form.lastName,
      role: form.role
    } : undefined

    await authStore.verifyOtp(identifier.value, form.otp, userData)

    // Redirect based on user role
    const role = authStore.userRole
    if (role === 'Admin') {
      router.push({ name: 'AdminDashboard' })
    } else if (role === 'Teacher') {
      router.push({ name: 'TeacherDashboard' })
    } else if (role === 'Parent') {
      router.push({ name: 'ParentDashboard' })
    }
  } catch (err: any) {
    error.value = err.response?.data?.message || 'OTP verification failed'
    // Check if this is a new user registration
    if (err.response?.status === 404) {
      isNewUser.value = true
    }
  } finally {
    isLoading.value = false
  }
}

const resendOtp = async () => {
  isResending.value = true
  error.value = null

  try {
    await authStore.requestOtp(identifier.value, type.value as 'email' | 'mobile')
    startCountdown()
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Failed to resend OTP'
  } finally {
    isResending.value = false
  }
}

onMounted(() => {
  if (!identifier.value) {
    router.push('/login')
    return
  }
  startCountdown()
})

onUnmounted(() => {
  if (countdownInterval.value) {
    clearInterval(countdownInterval.value)
  }
})
</script>

<style scoped>
/* Additional styles if needed */
</style>