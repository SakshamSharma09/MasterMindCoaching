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
          
          <!-- Enhanced demo badge -->
          <div class="inline-flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-yellow-400/20 to-orange-400/20 
                      rounded-full text-xs text-yellow-100 border border-yellow-300/30
                      backdrop-blur-sm">
            <span class="w-2 h-2 bg-yellow-400 rounded-full animate-pulse"></span>
            Demo Mode Active
          </div>
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
                Email or Phone Number
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
                  placeholder="you@example.com or 9876543210"
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
              :disabled="isLoading || !form.identifier.trim()"
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

          <!-- Enhanced Demo Accounts Section -->
          <div class="mt-8">
            <div class="relative mb-6">
              <div class="absolute inset-0 flex items-center">
                <div class="w-full border-t border-gray-200/60"></div>
              </div>
              <div class="relative flex justify-center">
                <span class="bg-white px-4 text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Quick Access
                </span>
              </div>
            </div>

            <div class="space-y-3">
              <!-- Enhanced Admin Account -->
              <div
                class="group p-4 bg-gradient-to-r from-blue-50 to-indigo-50 border border-blue-200/60 
                       rounded-2xl cursor-pointer hover:from-blue-100 hover:to-indigo-100 
                       transition-all duration-300 hover:shadow-lg hover:scale-[1.02]
                       relative overflow-hidden animate-stagger-1"
                @click="form.identifier = 'admin@mastermind.com'"
              >
                <div class="absolute inset-0 bg-gradient-to-r from-blue-400/5 to-indigo-400/5 opacity-0 
                            group-hover:opacity-100 transition-opacity duration-300"></div>
                <div class="relative flex items-center justify-between">
                  <div class="flex items-center">
                    <div class="w-10 h-10 bg-gradient-to-br from-blue-500 to-indigo-600 rounded-xl 
                                flex items-center justify-center mr-4 shadow-md group-hover:shadow-lg 
                                transition-all duration-300 group-hover:scale-110">
                      <i class="fas fa-shield-alt text-white"></i>
                    </div>
                    <div>
                      <p class="font-semibold text-gray-900 text-sm">Administrator</p>
                      <p class="text-xs text-gray-600">Full system access</p>
                    </div>
                  </div>
                  <div class="flex items-center gap-2">
                    <code class="text-xs bg-white/80 px-3 py-1.5 rounded-lg border border-blue-200/60 
                                   font-mono text-gray-700 backdrop-blur-sm">admin@mastermind.com</code>
                    <i class="fas fa-arrow-right text-blue-500 opacity-0 group-hover:opacity-100 
                               transition-opacity duration-300 transform group-hover:translate-x-1"></i>
                  </div>
                </div>
              </div>

              <!-- Enhanced Teacher Account -->
              <div
                class="group p-4 bg-gradient-to-r from-green-50 to-emerald-50 border border-green-200/60 
                       rounded-2xl cursor-pointer hover:from-green-100 hover:to-emerald-100 
                       transition-all duration-300 hover:shadow-lg hover:scale-[1.02]
                       relative overflow-hidden animate-stagger-2"
                @click="form.identifier = 'teacher@mastermind.com'"
              >
                <div class="absolute inset-0 bg-gradient-to-r from-green-400/5 to-emerald-400/5 opacity-0 
                            group-hover:opacity-100 transition-opacity duration-300"></div>
                <div class="relative flex items-center justify-between">
                  <div class="flex items-center">
                    <div class="w-10 h-10 bg-gradient-to-br from-green-500 to-emerald-600 rounded-xl 
                                flex items-center justify-center mr-4 shadow-md group-hover:shadow-lg 
                                transition-all duration-300 group-hover:scale-110">
                      <i class="fas fa-chalkboard-teacher text-white"></i>
                    </div>
                    <div>
                      <p class="font-semibold text-gray-900 text-sm">Teacher</p>
                      <p class="text-xs text-gray-600">Class management</p>
                    </div>
                  </div>
                  <div class="flex items-center gap-2">
                    <code class="text-xs bg-white/80 px-3 py-1.5 rounded-lg border border-green-200/60 
                                   font-mono text-gray-700 backdrop-blur-sm">teacher@mastermind.com</code>
                    <i class="fas fa-arrow-right text-green-500 opacity-0 group-hover:opacity-100 
                               transition-opacity duration-300 transform group-hover:translate-x-1"></i>
                  </div>
                </div>
              </div>

              <!-- Enhanced Parent Account -->
              <div
                class="group p-4 bg-gradient-to-r from-purple-50 to-pink-50 border border-purple-200/60 
                       rounded-2xl cursor-pointer hover:from-purple-100 hover:to-pink-100 
                       transition-all duration-300 hover:shadow-lg hover:scale-[1.02]
                       relative overflow-hidden animate-stagger-3"
                @click="form.identifier = 'parent@mastermind.com'"
              >
                <div class="absolute inset-0 bg-gradient-to-r from-purple-400/5 to-pink-400/5 opacity-0 
                            group-hover:opacity-100 transition-opacity duration-300"></div>
                <div class="relative flex items-center justify-between">
                  <div class="flex items-center">
                    <div class="w-10 h-10 bg-gradient-to-br from-purple-500 to-pink-600 rounded-xl 
                                flex items-center justify-center mr-4 shadow-md group-hover:shadow-lg 
                                transition-all duration-300 group-hover:scale-110">
                      <i class="fas fa-heart text-white"></i>
                    </div>
                    <div>
                      <p class="font-semibold text-gray-900 text-sm">Parent</p>
                      <p class="text-xs text-gray-600">Student monitoring</p>
                    </div>
                  </div>
                  <div class="flex items-center gap-2">
                    <code class="text-xs bg-white/80 px-3 py-1.5 rounded-lg border border-purple-200/60 
                                   font-mono text-gray-700 backdrop-blur-sm">parent@mastermind.com</code>
                    <i class="fas fa-arrow-right text-purple-500 opacity-0 group-hover:opacity-100 
                               transition-opacity duration-300 transform group-hover:translate-x-1"></i>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Enhanced Footer -->
        <div class="pb-6 text-center">
          <div class="flex items-center justify-center gap-2 text-xs text-gray-500">
            <i class="fas fa-lock text-green-500"></i>
            <span>Secure login powered by OTP verification</span>
          </div>
        </div>
      </div>

      <!-- Floating action hint -->
      <div class="mt-6 text-center">
        <p class="text-white/80 text-sm animate-pulse">
          <i class="fas fa-mouse-pointer mr-2"></i>
          Click on demo accounts for quick access
        </p>
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
  identifier: ''
})

const isLoading = ref(false)
const error = ref<string | null>(null)

// Computed property to determine button text
const buttonText = computed(() => {
  if (isLoading.value) return 'Authenticating…'
  
  const testEmails = ['admin@mastermind.com', 'teacher@mastermind.com', 'parent@mastermind.com']
  if (testEmails.includes(form.identifier)) {
    return 'Quick Access'
  }
  
  return 'Send OTP'
})

const handleSubmit = async () => {
  if (!form.identifier.trim()) return

  isLoading.value = true
  error.value = null

  try {
    // Check if this is a test email for bypass authentication
    const testEmails = ['admin@mastermind.com', 'teacher@mastermind.com', 'parent@mastermind.com']
    
    if (testEmails.includes(form.identifier)) {
      // Use bypass authentication for test emails
      await authStore.bypassAuth(form.identifier)
      
      // Redirect to appropriate dashboard based on role
      const role = authStore.userRole
      if (role === 'Admin') {
        router.push({ name: 'AdminDashboard' })
      } else if (role === 'Teacher') {
        router.push({ name: 'TeacherDashboard' })
      } else if (role === 'Parent') {
        router.push({ name: 'ParentDashboard' })
      }
    } else {
      // Normal OTP flow for other emails
      const isEmail = form.identifier.includes('@')
      const type = isEmail ? 'email' : 'mobile'

      await authStore.requestOtp(form.identifier, type)

      router.push({
        name: 'OtpVerify',
        query: { identifier: form.identifier, type }
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
