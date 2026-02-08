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
            Welcome Back
          </h1>
          <p class="text-indigo-100 text-sm mb-4">
            Sign in to continue your learning journey
          </p>
        </div>

        <!-- Form Section -->
        <div class="px-8 py-8 relative">
          <!-- Known Devices Section -->
          <div v-if="knownDevices.length > 0" class="mb-6">
            <h3 class="text-lg font-semibold text-gray-900 mb-3 flex items-center gap-2">
              <i class="fas fa-mobile-alt text-indigo-500"></i>
              Your Devices
            </h3>
            
            <div class="space-y-2">
              <div
                v-for="device in knownDevices"
                :key="device.deviceId"
                @click="selectDevice(device)"
                class="flex items-center justify-between p-3 rounded-xl border-2
                       transition-all duration-300 cursor-pointer
                       hover:bg-indigo-50 hover:border-indigo-300 hover:shadow-md"
                :class="{
                  'border-green-200 bg-green-50': device.isTrusted,
                  'border-gray-200 bg-gray-50': !device.isTrusted,
                  'ring-2 ring-indigo-500': selectedDevice?.deviceId === device.deviceId
                }"
              >
                <div class="flex items-center gap-3">
                  <div class="w-10 h-10 bg-gradient-to-br from-indigo-500 to-purple-600 rounded-xl 
                              flex items-center justify-center text-white font-bold">
                    <i :class="getDeviceIcon(device.deviceType)"></i>
                  </div>
                  <div>
                    <div class="font-medium text-gray-900">{{ device.deviceName }}</div>
                    <div class="text-sm text-gray-500">{{ device.lastUsedAt ? formatDate(device.lastUsedAt) : 'Never used' }}</div>
                  </div>
                </div>
                
                <div class="flex items-center gap-2">
                  <span v-if="device.isTrusted" class="text-xs bg-green-100 text-green-800 px-2 py-1 rounded-full">
                    <i class="fas fa-check mr-1"></i>Trusted
                  </span>
                  <button
                    v-if="!device.isTrusted && selectedDevice?.deviceId !== device.deviceId"
                    @click="trustDevice(device)"
                    class="text-xs bg-indigo-100 text-indigo-700 px-2 py-1 rounded-full hover:bg-indigo-200"
                  >
                    <i class="fas fa-shield-alt mr-1"></i>Trust
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- Login Form -->
          <form @submit.prevent="handleSubmit" class="space-y-6">
            <!-- Enhanced Email/Mobile Input -->
            <div class="space-y-3">
              <label
                for="identifier"
                class="block text-sm font-semibold text-gray-700 flex items-center gap-2"
              >
                <i class="fas fa-envelope text-indigo-500"></i>
                Email or Mobile Number
              </label>

              <div class="relative group">
                <input
                  id="identifier"
                  v-model="form.identifier"
                  type="text"
                  required
                  placeholder="Enter your email or mobile number"
                  class="w-full px-6 py-4 rounded-2xl text-base
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
                         pl-14"
                  :disabled="isLoading"
                />
                
                <!-- Animated input icon -->
                <div class="absolute left-4 top-1/2 transform -translate-y-1/2 
                           transition-all duration-300 group-hover:scale-110 group-hover:rotate-12">
                  <i class="fas fa-user text-indigo-400 group-hover:text-indigo-600 transition-colors"></i>
                </div>
              </div>
            </div>

            <!-- Enhanced Submit Button -->
            <button
              type="submit"
              :disabled="isLoading || !form.identifier"
              class="w-full py-4 rounded-2xl font-bold text-white text-lg
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
                  d="M4 12a8 8 0 018-8v4a4 4 0 008 8H4z"
                />
              </svg>

              <i v-else class="fas fa-sign-in-alt"></i>
              <span>{{ isLoading ? 'Requesting OTP...' : 'Send OTP' }}</span>
            </button>

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
          </form>

          <!-- Demo Account Quick Access -->
          <div class="mt-8 border-t border-gray-200/60 pt-6">
            <h3 class="text-lg font-semibold text-gray-900 mb-4 text-center">
              <i class="fas fa-star text-yellow-500 mr-2"></i>
              Quick Access - Demo Accounts
            </h3>
            
            <div class="space-y-3">
              <!-- Enhanced Admin Account -->
              <div
                class="group p-4 bg-gradient-to-r from-blue-50 to-indigo-50 border border-blue-200/60 
                       rounded-2xl cursor-pointer hover:from-blue-100 hover:to-indigo-100 
                       transition-all duration-300 hover:shadow-lg hover:scale-[1.02]
                       relative overflow-hidden animate-stagger-1"
                @click="useDemoAccount('admin@mastermind.com')"
              >
                <div class="absolute inset-0 bg-gradient-to-r from-blue-400/5 to-indigo-400/5 opacity-0 
                            group-hover:opacity-100 transition-opacity duration-300"></div>
                <div class="relative flex items-center justify-between">
                  <div class="flex items-center">
                    <div class="w-10 h-10 bg-gradient-to-br from-blue-500 to-indigo-600 rounded-xl 
                                flex items-center justify-center shadow-md group-hover:shadow-lg 
                                transition-all duration-300 group-hover:scale-110">
                      <i class="fas fa-shield-alt text-white"></i>
                    </div>
                    <div>
                      <div class="font-semibold text-gray-900">Administrator</div>
                      <div class="text-sm text-gray-600">Full system access</div>
                    </div>
                  </div>
                  <div class="flex items-center gap-2 text-blue-600 opacity-0 
                               group-hover:opacity-100 transition-opacity duration-300 transform group-hover:translate-x-1">
                    <i class="fas fa-arrow-right"></i>
                  </div>
                </div>
              </div>

              <!-- Enhanced Teacher Account -->
              <div
                class="group p-4 bg-gradient-to-r from-green-50 to-emerald-50 border border-green-200/60 
                       rounded-2xl cursor-pointer hover:from-green-100 hover:to-emerald-100 
                       transition-all duration-300 hover:shadow-lg hover:scale-[1.02]
                       relative overflow-hidden animate-stagger-2"
                @click="useDemoAccount('teacher@mastermind.com')"
              >
                <div class="absolute inset-0 bg-gradient-to-r from-green-400/5 to-emerald-400/5 opacity-0 
                            group-hover:opacity-100 transition-opacity duration-300"></div>
                <div class="relative flex items-center justify-between">
                  <div class="flex items-center">
                    <div class="w-10 h-10 bg-gradient-to-br from-green-500 to-emerald-600 rounded-xl 
                                flex items-center justify-center shadow-md group-hover:shadow-lg 
                                transition-all duration-300 group-hover:scale-110">
                      <i class="fas fa-chalkboard-teacher text-white"></i>
                    </div>
                    <div>
                      <div class="font-semibold text-gray-900">Teacher</div>
                      <div class="text-sm text-gray-600">Class management access</div>
                    </div>
                  </div>
                  <div class="flex items-center gap-2 text-green-600 opacity-0 
                               group-hover:opacity-100 transition-opacity duration-300 transform group-hover:translate-x-1">
                    <i class="fas fa-arrow-right"></i>
                  </div>
                </div>
              </div>

              <!-- Enhanced Parent Account -->
              <div
                class="group p-4 bg-gradient-to-r from-purple-50 to-pink-50 border border-purple-200/60 
                       rounded-2xl cursor-pointer hover:from-purple-100 hover:to-pink-100 
                       transition-all duration-300 hover:shadow-lg hover:scale-[1.02]
                       relative overflow-hidden animate-stagger-3"
                @click="useDemoAccount('parent@mastermind.com')"
              >
                <div class="absolute inset-0 bg-gradient-to-r from-purple-400/5 to-pink-400/5 opacity-0 
                            group-hover:opacity-100 transition-opacity duration-300"></div>
                <div class="relative flex items-center justify-between">
                  <div class="flex items-center">
                    <div class="w-10 h-10 bg-gradient-to-br from-purple-500 to-pink-600 rounded-xl 
                                flex items-center justify-center shadow-md group-hover:shadow-lg 
                                transition-all duration-300 group-hover:scale-110">
                      <i class="fas fa-heart text-white"></i>
                    </div>
                    <div>
                      <div class="font-semibold text-gray-900">Parent</div>
                      <div class="text-sm text-gray-600">Student monitoring access</div>
                    </div>
                  </div>
                  <div class="flex items-center gap-2 text-purple-600 opacity-0 
                               group-hover:opacity-100 transition-opacity duration-300 transform group-hover:translate-x-1">
                    <i class="fas fa-arrow-right"></i>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import type { Device } from '@/types/device'

const router = useRouter()
const authStore = useAuthStore()

const form = reactive({
  identifier: ''
})

const isLoading = ref(false)
const error = ref('')
const knownDevices = ref<Device[]>([])
const selectedDevice = ref<Device | null>(null)

const handleSubmit = async () => {
  if (!form.identifier) {
    error.value = 'Please enter your email or mobile number'
    return
  }

  isLoading.value = true
  error.value = ''

  try {
    // Store selected device info
    if (selectedDevice.value) {
      localStorage.setItem('selected_device_id', selectedDevice.value.deviceId)
    }

    await authStore.requestOtp(form.identifier)
    
    // Redirect to OTP verification page
    router.push({
      name: 'OtpVerify',
      query: {
        identifier: form.identifier,
        type: form.identifier.includes('@') ? 'email' : 'mobile'
      }
    })
  } catch (err: any) {
    error.value = err.response?.data?.error || 'Failed to send OTP'
  } finally {
    isLoading.value = false
  }
}

const selectDevice = (device: Device) => {
  selectedDevice.value = device
}

const trustDevice = async (device: Device) => {
  try {
    // Call API to trust device
    const response = await fetch('/api/auth/device/trust', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authStore.token}`
      },
      body: JSON.stringify({ deviceId: device.deviceId })
    })

    if (response.ok) {
      device.isTrusted = true
    }
  } catch (err) {
    console.error('Error trusting device:', err)
  }
}

const useDemoAccount = (email: string) => {
  form.identifier = email
  error.value = ''
}

const getDeviceIcon = (deviceType: string) => {
  switch (deviceType.toLowerCase()) {
    case 'mobile': return 'fas fa-mobile-alt'
    case 'tablet': return 'fas fa-tablet-alt'
    case 'desktop': return 'fas fa-desktop'
    default: return 'fas fa-question-circle'
  }
}

const formatDate = (dateString: string) => {
  if (!dateString) return 'Never'
  return new Date(dateString).toLocaleDateString()
}

const loadKnownDevices = async () => {
  try {
    const response = await fetch('/api/auth/devices', {
      headers: {
        'Authorization': `Bearer ${authStore.token}`
      }
    })

    if (response.ok) {
      const devices = await response.json()
      knownDevices.value = devices
    }
  } catch (err) {
    console.error('Error loading devices:', err)
  }
}

onMounted(async () => {
  // Load known devices when component mounts
  if (authStore.isAuthenticated) {
    await loadKnownDevices()
    
    // Auto-select most recently used device
    if (knownDevices.value.length > 0) {
      selectedDevice.value = knownDevices.value[0]
    }
  }
})
</script>
