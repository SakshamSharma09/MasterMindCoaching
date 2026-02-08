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
            <i class="fas fa-shield-check text-4xl bg-gradient-to-br from-indigo-600 to-purple-600 bg-clip-text text-transparent"></i>
          </div>

          <h1 class="text-3xl font-bold text-white tracking-wide mb-2">
            Verify Your Account
          </h1>
          <p class="text-indigo-100 text-sm mb-4">
            We've sent a 6-digit OTP to
          </p>
          <div class="inline-flex items-center gap-2 px-4 py-2 bg-white/20 rounded-xl backdrop-blur-sm">
            <i class="fas fa-envelope text-white/80"></i>
            <span class="text-white font-semibold">{{ identifier }}</span>
          </div>
        </div>

        <!-- Form Section -->
        <div class="px-8 py-8 relative">
          <form @submit.prevent="handleSubmit" class="space-y-6">
            <!-- Enhanced OTP Input -->
            <div class="space-y-3">
              <label
                for="otp"
                class="block text-sm font-semibold text-gray-700 flex items-center gap-2"
              >
                <i class="fas fa-key text-indigo-500"></i>
                Enter One-Time Password
              </label>

              <div class="relative group">
                <input
                  id="otp"
                  v-model="form.otp"
                  type="text"
                  maxlength="6"
                  required
                  :disabled="isLoading"
                  placeholder="000000"
                  @input="handleOtpInput"
                  class="w-full px-6 py-5 rounded-2xl text-center
                         border-2 border-gray-200
                         focus:ring-2 focus:ring-indigo-500/50
                         focus:border-indigo-500
                         transition-all duration-300
                         disabled:bg-gray-50
                         bg-gray-50/50
                         hover:bg-white
                         focus:bg-white
                         shadow-sm hover:shadow-md
                         focus:shadow-lg
                         text-3xl font-mono tracking-widest
                         letter-spacing: 0.2em"
                />
                
                <!-- Animated input border -->
                <div class="absolute inset-0 rounded-2xl bg-gradient-to-r from-indigo-500 to-purple-500 opacity-0 
                            group-focus-within:opacity-20 transition-opacity duration-300 pointer-events-none"></div>
              </div>
              
              <!-- Countdown Timer -->
              <div class="flex items-center justify-center gap-2 text-sm">
                <div class="w-2 h-2 bg-green-500 rounded-full animate-pulse"></div>
                <span class="text-gray-600 font-medium">
                  OTP expires in <span class="text-indigo-600 font-bold">{{ countdown }}</span> seconds
                </span>
              </div>
            </div>

            <!-- User Registration Fields (for new users) -->
            <div v-if="isNewUser" class="space-y-5 border-t border-gray-200/60 pt-6 animate-slide-in-bottom">
              <h3 class="text-lg font-bold text-gray-900 flex items-center gap-2">
                <i class="fas fa-user-plus text-indigo-500"></i>
                Complete Your Registration
              </h3>

              <div class="grid grid-cols-2 gap-4">
                <div class="space-y-2">
                  <label for="firstName" class="block text-sm font-semibold text-gray-700">
                    First Name
                  </label>
                  <div class="relative group">
                    <input
                      id="firstName"
                      v-model="form.firstName"
                      type="text"
                      required
                      placeholder="John"
                      :disabled="isLoading"
                      class="w-full px-4 py-3 rounded-xl
                             border border-gray-200
                             focus:ring-2 focus:ring-indigo-500/50
                             focus:border-indigo-500
                             transition-all duration-300
                             disabled:bg-gray-50
                             bg-gray-50/50
                             hover:bg-white
                             focus:bg-white
                             shadow-sm"
                    />
                  </div>
                </div>
                
                <div class="space-y-2">
                  <label for="lastName" class="block text-sm font-semibold text-gray-700">
                    Last Name
                  </label>
                  <div class="relative group">
                    <input
                      id="lastName"
                      v-model="form.lastName"
                      type="text"
                      required
                      placeholder="Doe"
                      :disabled="isLoading"
                      class="w-full px-4 py-3 rounded-xl
                             border border-gray-200
                             focus:ring-2 focus:ring-indigo-500/50
                             focus:border-indigo-500
                             transition-all duration-300
                             disabled:bg-gray-50
                             bg-gray-50/50
                             hover:bg-white
                             focus:bg-white
                             shadow-sm"
                    />
                  </div>
                </div>
              </div>

              <div class="space-y-2">
                <label for="role" class="block text-sm font-semibold text-gray-700">
                  <i class="fas fa-user-tag text-indigo-500 mr-2"></i>
                  Select Your Role
                </label>
                <div class="relative group">
                  <select
                    id="role"
                    v-model="form.role"
                    required
                    :disabled="isLoading"
                    class="w-full px-4 py-3 rounded-xl
                           border border-gray-200 bg-white
                           focus:ring-2 focus:ring-indigo-500/50
                           focus:border-indigo-500
                           transition-all duration-300
                           disabled:bg-gray-50
                           shadow-sm appearance-none
                           cursor-pointer"
                  >
                    <option value="">Choose your role...</option>
                    <option value="Admin">🛡️ Administrator</option>
                    <option value="Teacher">👨‍🏫 Teacher</option>
                    <option value="Parent">👨‍👩‍👧‍👦 Parent</option>
                  </select>
                  <div class="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
                    <i class="fas fa-chevron-down text-gray-400 group-focus-within:text-indigo-500 transition-colors"></i>
                  </div>
                </div>
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
              :disabled="isLoading || !form.otp || (isNewUser && (!form.firstName || !form.lastName || !form.role))"
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

              <i v-else class="fas fa-check-circle"></i>
              <span>{{ isLoading ? 'Verifying…' : (isNewUser ? 'Complete Registration' : 'Verify OTP') }}</span>
            </button>

            <!-- Enhanced Resend OTP -->
            <div class="text-center">
              <button
                type="button"
                @click="resendOtp"
                :disabled="isResending || countdown > 0"
                class="inline-flex items-center gap-2 px-4 py-2 rounded-xl
                       text-sm font-medium text-indigo-600
                       hover:text-indigo-700 hover:bg-indigo-50
                       disabled:text-gray-400 disabled:cursor-not-allowed
                       transition-all duration-300"
              >
                <i v-if="isResending" class="fas fa-spinner animate-spin"></i>
                <i v-else class="fas fa-redo"></i>
                <span v-if="isResending">Resending...</span>
                <span v-else-if="countdown > 0">Resend OTP in {{ countdown }}s</span>
                <span v-else>Resend OTP</span>
              </button>
            </div>
          </form>

          <!-- Enhanced Back to Login -->
          <div class="mt-6 text-center">
            <router-link
              to="/login"
              class="inline-flex items-center gap-2 text-sm text-gray-500 
                     hover:text-indigo-600 transition-colors duration-300
                     group"
            >
              <i class="fas fa-arrow-left group-hover:-translate-x-1 transition-transform duration-300"></i>
              <span>Back to Login</span>
            </router-link>
          </div>
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