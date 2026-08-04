<template>
  <main class="min-h-[100dvh] overflow-y-auto bg-[#f7f9fc] text-slate-950">
    <div class="mx-auto grid min-h-[100dvh] w-full max-w-6xl grid-cols-1 lg:grid-cols-[1.05fr_0.95fr]">
      <section class="hidden flex-col justify-between overflow-hidden bg-[#061a33] p-10 text-white lg:flex">
        <div>
          <div class="inline-flex items-center gap-3 rounded-2xl bg-white/10 px-4 py-3">
            <span class="flex h-11 w-11 items-center justify-center rounded-xl bg-[#f3b33d] text-[#061a33]">
              <i class="fas fa-book-open text-xl"></i>
            </span>
            <div>
              <p class="text-sm font-semibold uppercase tracking-[0.22em] text-[#f3d58a]">MasterMind</p>
              <p class="text-sm text-white/75">Coaching Classes</p>
            </div>
          </div>

          <div class="mt-16 max-w-xl">
            <p class="text-sm font-semibold uppercase tracking-[0.28em] text-[#77d6c9]">Student access desk</p>
            <h1 class="mt-5 text-5xl font-black leading-tight">
              One secure doorway for admins, teachers, and parents.
            </h1>
            <p class="mt-5 text-lg leading-8 text-white/72">
              Admins use password login. Teachers and parents can verify by email OTP first, then use their registered mobile number and password.
            </p>
          </div>
        </div>

        <div class="grid grid-cols-3 gap-3">
          <div class="rounded-2xl border border-white/10 bg-white/8 p-4">
            <p class="text-2xl font-black text-[#f3b33d]">01</p>
            <p class="mt-2 text-sm text-white/70">Email OTP verifies the real account owner.</p>
          </div>
          <div class="rounded-2xl border border-white/10 bg-white/8 p-4">
            <p class="text-2xl font-black text-[#77d6c9]">02</p>
            <p class="mt-2 text-sm text-white/70">Mobile password login keeps daily access quick.</p>
          </div>
          <div class="rounded-2xl border border-white/10 bg-white/8 p-4">
            <p class="text-2xl font-black text-white">03</p>
            <p class="mt-2 text-sm text-white/70">Role-based dashboards open automatically.</p>
          </div>
        </div>
      </section>

      <section class="flex items-center justify-center px-3 pb-[max(0.75rem,env(safe-area-inset-bottom))] pt-[max(0.75rem,env(safe-area-inset-top))] sm:px-6 sm:py-8 lg:px-10">
        <div class="w-full max-w-md">
          <div class="mb-3 flex items-center gap-3 sm:mb-6 lg:hidden">
            <span class="flex h-12 w-12 items-center justify-center rounded-2xl bg-[#6049e8] text-white shadow-lg shadow-indigo-200">
              <i class="fas fa-book-open text-xl"></i>
            </span>
            <div>
              <p class="text-xl font-black text-slate-950">MasterMind</p>
              <p class="text-sm text-slate-500">Coaching Classes</p>
            </div>
          </div>

          <div class="rounded-[1.5rem] border border-slate-200 bg-white p-4 shadow-2xl shadow-slate-200/70 sm:rounded-[2rem] sm:p-7">
            <div class="mb-4 sm:mb-6">
              <p class="text-sm font-semibold uppercase tracking-[0.18em] text-[#6049e8]">Secure login</p>
              <h2 class="mt-2 text-3xl font-black text-slate-950">Welcome back</h2>
              <p class="mt-2 text-sm leading-6 text-slate-500">
                Choose the access method assigned to your role.
              </p>
            </div>

            <div class="mb-4 grid grid-cols-3 gap-1 rounded-2xl bg-slate-100 p-1 sm:mb-6 sm:gap-2">
              <button
                v-for="option in loginOptions"
                :key="option.value"
                type="button"
                class="min-h-11 rounded-xl px-1 py-2 text-[11px] font-bold leading-tight transition sm:px-3 sm:py-3 sm:text-sm"
                :class="loginMode === option.value ? 'bg-white text-slate-950 shadow-sm' : 'text-slate-500 hover:text-slate-800'"
                @click="selectMode(option.value)"
              >
                {{ option.label }}
              </button>
            </div>

            <form class="space-y-4 sm:space-y-5" @submit.prevent="handleSubmit">
              <div>
                <label for="identifier" class="mb-2 block text-sm font-bold text-slate-700">{{ identifierLabel }}</label>
                <div class="relative">
                  <i :class="identifierIcon" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400"></i>
                  <input
                    id="identifier"
                    v-model="form.identifier"
                    :type="loginMode === 'mobile-password' ? 'tel' : 'email'"
                    :inputmode="loginMode === 'mobile-password' ? 'tel' : 'email'"
                    required
                    :disabled="isLoading"
                    :placeholder="identifierPlaceholder"
                    autocomplete="username"
                    class="w-full rounded-2xl border border-slate-200 bg-slate-50 py-4 pl-12 pr-4 text-base font-semibold text-slate-950 outline-none transition focus:border-[#6049e8] focus:bg-white focus:ring-4 focus:ring-[#6049e8]/10"
                  />
                </div>
              </div>

              <div v-if="requiresPassword">
                <label for="password" class="mb-2 block text-sm font-bold text-slate-700">Password</label>
                <div class="relative">
                  <i class="fas fa-lock absolute left-4 top-1/2 -translate-y-1/2 text-slate-400"></i>
                  <input
                    id="password"
                    v-model="form.password"
                    type="password"
                    required
                    minlength="6"
                    :disabled="isLoading"
                    placeholder="Enter your password"
                    autocomplete="current-password"
                    class="w-full rounded-2xl border border-slate-200 bg-slate-50 py-4 pl-12 pr-4 text-base font-semibold text-slate-950 outline-none transition focus:border-[#6049e8] focus:bg-white focus:ring-4 focus:ring-[#6049e8]/10"
                  />
                </div>
              </div>

              <div class="rounded-2xl border border-[#dbeafe] bg-[#eff6ff] p-4 text-sm leading-6 text-slate-600">
                <p class="font-bold text-slate-800">{{ helperTitle }}</p>
                <p class="mt-1">{{ helperText }}</p>
              </div>

              <p v-if="error" class="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-semibold text-red-700">
                {{ error }}
              </p>

              <button
                type="submit"
                :disabled="isSubmitDisabled"
                class="flex w-full items-center justify-center gap-3 rounded-2xl bg-[#6049e8] px-5 py-4 text-base font-black text-white shadow-lg shadow-indigo-200 transition hover:bg-[#503bd1] focus:outline-none focus:ring-4 focus:ring-[#6049e8]/25 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <svg v-if="isLoading" class="h-5 w-5 animate-spin" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z" />
                </svg>
                <i v-else :class="submitIcon"></i>
                <span>{{ buttonText }}</span>
              </button>
            </form>

            <div class="mt-6 border-t border-slate-100 pt-5 text-sm leading-6 text-slate-500">
              <p>
                First-time parent or teacher? Use the private joining link sent by MasterMind to set your recovery email and password.
                Already set up but forgot the password? Use
                <button type="button" class="font-bold text-[#6049e8]" @click="selectMode('email-otp')">email OTP</button>
                to access your account, then change the password in Account Security.
              </p>
              <p class="mt-3">
                By using this app, you agree to our
                <router-link to="/privacy-policy" class="font-bold text-[#6049e8] hover:text-[#503bd1]">
                  Privacy Policy
                </router-link>.
              </p>
            </div>
          </div>
        </div>
      </section>
    </div>
  </main>
</template>

<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

type LoginMode = 'admin-password' | 'email-otp' | 'mobile-password'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const loginOptions: Array<{ label: string; value: LoginMode }> = [
  { label: 'Admin', value: 'admin-password' },
  { label: 'Email OTP', value: 'email-otp' },
  { label: 'Mobile Password', value: 'mobile-password' }
]

const form = reactive({
  identifier: '',
  password: ''
})

const loginMode = ref<LoginMode>('mobile-password')
const isLoading = ref(false)
const error = ref<string | null>(null)

const requiresPassword = computed(() => loginMode.value !== 'email-otp')
const identifierLabel = computed(() => loginMode.value === 'mobile-password' ? 'Registered mobile number' : 'Email address')
const identifierPlaceholder = computed(() => {
  if (loginMode.value === 'admin-password') return 'themastermindcoachingclasses@gmail.com'
  if (loginMode.value === 'mobile-password') return '9876543210'
  return 'parent-or-teacher@example.com'
})
const identifierIcon = computed(() => loginMode.value === 'mobile-password' ? 'fas fa-mobile-alt' : 'fas fa-envelope')
const submitIcon = computed(() => loginMode.value === 'email-otp' ? 'fas fa-paper-plane' : 'fas fa-arrow-right')
const buttonText = computed(() => {
  if (isLoading.value) return 'Checking...'
  return loginMode.value === 'email-otp' ? 'Send email OTP' : 'Login securely'
})
const helperTitle = computed(() => {
  if (loginMode.value === 'admin-password') return 'Admin access'
  if (loginMode.value === 'mobile-password') return 'Parent and teacher quick login'
  return 'First-time or password reset access'
})
const helperText = computed(() => {
  if (loginMode.value === 'admin-password') return 'Use the official institute email and your current admin password.'
  if (loginMode.value === 'mobile-password') return 'Use the primary mobile number saved by admin and the password you created from your invitation link.'
  return 'We send OTP to the recovery email saved by the parent or teacher.'
})

const isSubmitDisabled = computed(() =>
  isLoading.value ||
  !form.identifier.trim() ||
  (requiresPassword.value && !form.password.trim())
)

const selectMode = (mode: LoginMode) => {
  loginMode.value = mode
  error.value = null
  form.password = ''
  if (mode === 'admin-password') {
    form.identifier = 'themastermindcoachingclasses@gmail.com'
  } else {
    form.identifier = ''
  }
}

const redirectByRole = () => {
  const role = authStore.userRole
  if (role === 'Admin') router.push({ name: 'AdminDashboard' })
  else if (role === 'Teacher') router.push({ name: 'TeacherDashboard' })
  else if (role === 'Parent') router.push({ name: 'ParentDashboard' })
  else router.push({ name: 'Login' })
}

const handleSubmit = async () => {
  if (!form.identifier.trim()) return

  isLoading.value = true
  error.value = null

  try {
    if (loginMode.value === 'email-otp') {
      if (!form.identifier.includes('@')) {
        throw new Error('Please enter your recovery email address.')
      }

      await authStore.requestOtp(form.identifier.trim(), 'email')
      router.push({
        name: 'OtpVerify',
        query: { identifier: form.identifier.trim(), type: 'email' }
      })
      return
    }

    if (loginMode.value === 'admin-password' && form.identifier.trim().toLowerCase() !== 'themastermindcoachingclasses@gmail.com') {
      throw new Error('Admin login is only available for the official institute email.')
    }

    await authStore.loginWithPassword(form.identifier.trim(), form.password)
    redirectByRole()
  } catch (err: any) {
    error.value =
      err.response?.data?.message ||
      err.message ||
      'Failed to authenticate. Please check the details and try again.'
  } finally {
    isLoading.value = false
  }
}

selectMode('mobile-password')
if (route.query.mobile) {
  form.identifier = String(route.query.mobile)
}
</script>
