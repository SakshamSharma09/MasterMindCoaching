<template>
  <div
    class="min-h-screen flex items-center justify-center p-4
           bg-gradient-to-br from-indigo-700 via-purple-700 to-pink-600
           relative overflow-hidden"
  >
    <!-- Animated Background Elements -->
    <div class="absolute inset-0">
      <!-- Floating orbs -->
      <div class="absolute top-20 left-20 w-72 h-72 bg-pink-500/20 rounded-full blur-3xl animate-pulse"></div>
      <div class="absolute top-40 right-32 w-96 h-96 bg-indigo-500/20 rounded-full blur-3xl animate-pulse delay-1000"></div>
      <div class="absolute bottom-32 left-1/2 w-80 h-80 bg-purple-500/20 rounded-full blur-3xl animate-pulse delay-2000"></div>
      
      <!-- Animated particles -->
      <div class="absolute top-1/4 left-1/4 w-2 h-2 bg-white/40 rounded-full animate-ping delay-300"></div>
      <div class="absolute top-3/4 right-1/3 w-1 h-1 bg-white/60 rounded-full animate-ping delay-700"></div>
      <div class="absolute bottom-1/4 left-1/3 w-1.5 h-1.5 bg-white/50 rounded-full animate-ping delay-1100"></div>
      
      <!-- Gradient mesh -->
      <div class="absolute inset-0 bg-gradient-to-t from-black/10 via-transparent to-transparent"></div>
    </div>

    <div class="relative w-full max-w-md animate-slide-in-bottom">
      <!-- Main Card with Enhanced Glass Effect -->
      <div
        class="bg-white/95 backdrop-blur-2xl rounded-3xl shadow-2xl overflow-hidden
               border border-white/30
               transform transition-all duration-500 hover:scale-[1.02]
               relative animate-glow-pulse"
      >
        <!-- Subtle glow effect -->
        <div class="absolute inset-0 bg-gradient-to-r from-indigo-500/5 to-purple-500/5 rounded-3xl"></div>
        
        <!-- Header with Enhanced Design -->
        <div
          class="px-8 pt-10 pb-8 text-center
                 bg-gradient-to-br from-indigo-600 via-purple-600 to-pink-600
                 relative overflow-hidden"
        >
          <!-- Animated background pattern -->
          <div class="absolute inset-0 opacity-10">
            <div class="absolute top-0 left-0 w-full h-full bg-white/20"></div>
          </div>
          
          <div
            class="relative mx-auto w-20 h-20 bg-white rounded-3xl
                   flex items-center justify-center shadow-2xl mb-5
                   transition-all duration-300 hover:scale-110 hover:rotate-6
                   border-2 border-white/50"
          >
            <i class="fas fa-graduation-cap text-4xl bg-gradient-to-br from-indigo-600 to-purple-600 bg-clip-text text-transparent"></i>
          </div>

          <h1 class="text-3xl font-bold text-white tracking-wide mb-2">
            MasterMind
          </h1>
          <p class="text-indigo-100 text-sm mb-4">
            Coaching Management System
          </p>
          
        </div>

        <!-- Form Section -->
        <div class="px-8 py-8 relative">
          <form @submit.prevent="handleSubmit" class="space-y-6">
            <!-- Enhanced Input Field -->
            <div class="space-y-2">
              <label
                for="identifier"
                class="block text-sm font-semibold text-gray-700 flex items-center gap-2"
              >
                <i class="fas fa-user-circle text-indigo-500"></i>
                Email Address
              </label>

              <div class="relative group">
                <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                  <i class="fas fa-envelope text-gray-400 group-focus-within:text-indigo-500 transition-colors"></i>
                </div>

                <input
                  id="identifier"
                  v-model="form.identifier"
                  type="text"
                  required
                  :disabled="isLoading"
                  placeholder="you@example.com"
                  class="w-full pl-12 pr-4 py-4 rounded-2xl
                         border border-gray-200
                         focus:ring-2 focus:ring-indigo-500/50
                         focus:border-indigo-500
                         transition-all duration-300
                         disabled:bg-gray-50
                         bg-gray-50/50
                         hover:bg-white
                         focus:bg-white
                         shadow-sm hover:shadow-md
                         focus:shadow-lg"
                />
                
                <!-- Animated input border -->
                <div class="absolute inset-0 rounded-2xl bg-gradient-to-r from-indigo-500 to-purple-500 opacity-0 
                            group-focus-within:opacity-20 transition-opacity duration-300 pointer-events-none"></div>
              </div>
            </div>

            <div v-if="isAdminEmail" class="space-y-2">
              <label
                for="password"
                class="block text-sm font-semibold text-gray-700 flex items-center gap-2"
              >
                <i class="fas fa-lock text-indigo-500"></i>
                Password
              </label>
              <div class="relative group">
                <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                  <i class="fas fa-key text-gray-400 group-focus-within:text-indigo-500 transition-colors"></i>
                </div>
                <input
                  id="password"
                  v-model="form.password"
                  type="password"
                  required
                  :disabled="isLoading"
                  placeholder="Enter admin password"
                  class="w-full pl-12 pr-4 py-4 rounded-2xl border border-gray-200 focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all duration-300 disabled:bg-gray-50 bg-gray-50/50 hover:bg-white focus:bg-white shadow-sm hover:shadow-md focus:shadow-lg"
                />
              </div>
            </div>

            <!-- Enhanced Error Message -->
            <div
              v-if="error"
              class="flex items-start gap-3 p-4
                     rounded-2xl bg-red-50/80 border border-red-200/50
                     backdrop-blur-sm animate-shake"
            >
              <div class="w-5 h-5 bg-red-100 rounded-full flex items-center justify-center flex-shrink-0 mt-0.5">
                <i class="fas fa-exclamation text-red-600 text-xs"></i>
              </div>
              <p class="text-sm text-red-700 font-medium">{{ error }}</p>
            </div>

            <!-- Enhanced Submit Button -->
            <button
              type="submit"
              :disabled="isLoading || !form.identifier.trim() || (isAdminEmail && !form.password.trim())"
              class="w-full py-4 rounded-2xl font-bold text-white text-base
                     bg-gradient-to-r from-indigo-600 via-purple-600 to-pink-600
                     hover:from-indigo-700 hover:via-purple-700 hover:to-pink-700
                     focus:ring-4 focus:ring-indigo-500/25
                     transition-all duration-300 disabled:opacity-50
                     flex items-center justify-center gap-3
                     shadow-lg hover:shadow-xl
                     transform hover:scale-[1.02]
                     relative overflow-hidden group"
            >
              <!-- Button shimmer effect -->
              <div class="absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent 
                          -translate-x-full group-hover:translate-x-full transition-transform duration-700"></div>
              
              <svg
                v-if="isLoading"
                class="w-5 h-5 animate-spin"
                fill="none"
                viewBox="0 0 24 24"
              >
                <circle
                  class="opacity-25"
                  cx="12"
                  cy="12"
                  r="10"
                  stroke="currentColor"
                  stroke-width="4"
                />
                <path
                  class="opacity-75"
                  fill="currentColor"
                  d="M4 12a8 8 0 018-8v4a4 4
                     0 00-4 4H4z"
                />
              </svg>

              <i v-else class="fas fa-paper-plane"></i>
              <span>{{ buttonText }}</span>
            </button>
          </form>

        </div>

        <!-- Enhanced Footer -->
        <div class="pb-6 text-center">
          <div class="flex items-center justify-center gap-2 text-xs text-gray-500">
            <i class="fas fa-lock text-green-500"></i>
            <span>Secure login powered by OTP verification</span>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const form = reactive({
  identifier: '',
  password: ''
})

const isLoading = ref(false)
const error = ref<string | null>(null)

// Computed property to determine button text
const buttonText = computed(() => {
  if (isLoading.value) return 'Authenticating…'
  
  return isAdminEmail.value ? 'Login with Password' : 'Send OTP'
})

const isAdminEmail = computed(() =>
  form.identifier.trim().toLowerCase() === 'themastermindcoachingclasses@gmail.com'
)

const handleSubmit = async () => {
  if (!form.identifier.trim()) return

  isLoading.value = true
  error.value = null

  try {
    if (isAdminEmail.value) {
      await authStore.loginWithPassword(form.identifier.trim(), form.password)
      const role = authStore.userRole
      if (role === 'Admin') {
        router.push({ name: 'AdminDashboard' })
      }
    } else {
      // OTP flow for non-admin users is email-only
      const isEmail = form.identifier.includes('@')
      if (!isEmail) {
        throw new Error('Please enter a valid email address.')
      }

      await authStore.requestOtp(form.identifier, 'email')

      router.push({
        name: 'OtpVerify',
        query: { identifier: form.identifier, type: 'email' }
      })
    }
  } catch (err: any) {
    error.value =
      err.response?.data?.message ||
      err.message ||
      'Failed to authenticate. Please try again.'
  } finally {
    isLoading.value = false
  }
}
</script>
