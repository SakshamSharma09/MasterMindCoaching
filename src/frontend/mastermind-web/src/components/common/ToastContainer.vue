<template>
  <Teleport to="body">
    <div class="fixed top-4 right-4 z-[9999] flex flex-col gap-3 max-w-sm w-full pointer-events-none">
      <TransitionGroup
        enter-active-class="transition-all duration-300 ease-out"
        enter-from-class="translate-x-full opacity-0 scale-95"
        enter-to-class="translate-x-0 opacity-100 scale-100"
        leave-active-class="transition-all duration-200 ease-in"
        leave-from-class="translate-x-0 opacity-100 scale-100"
        leave-to-class="translate-x-full opacity-0 scale-95"
      >
        <div
          v-for="toast in toasts"
          :key="toast.id"
          :class="[
            'pointer-events-auto rounded-xl shadow-lg border backdrop-blur-md overflow-hidden',
            typeStyles[toast.type].container
          ]"
        >
          <div class="flex items-start gap-3 p-4">
            <!-- Icon -->
            <div :class="['flex-shrink-0 w-6 h-6 mt-0.5', typeStyles[toast.type].icon]">
              <!-- Success -->
              <svg v-if="toast.type === 'success'" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <!-- Error -->
              <svg v-else-if="toast.type === 'error'" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <!-- Warning -->
              <svg v-else-if="toast.type === 'warning'" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
              </svg>
              <!-- Info -->
              <svg v-else fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>

            <!-- Content -->
            <div class="flex-1 min-w-0">
              <p :class="['text-sm font-semibold', typeStyles[toast.type].title]">
                {{ toast.title }}
              </p>
              <p v-if="toast.message" :class="['mt-1 text-xs leading-relaxed', typeStyles[toast.type].message]">
                {{ toast.message }}
              </p>
            </div>

            <!-- Close Button -->
            <button
              @click="removeToast(toast.id)"
              :class="['flex-shrink-0 p-1 rounded-lg transition-colors', typeStyles[toast.type].close]"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          <!-- Progress Bar -->
          <div class="h-0.5 w-full" :class="typeStyles[toast.type].progressBg">
            <div
              :class="['h-full', typeStyles[toast.type].progress]"
              :style="{ animation: `toast-progress ${toast.duration}ms linear forwards` }"
            />
          </div>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { useToast } from '@/composables/useToast'

const { toasts, removeToast } = useToast()

const typeStyles: Record<string, Record<string, string>> = {
  success: {
    container: 'bg-green-50/90 border-green-200',
    icon: 'text-green-500',
    title: 'text-green-900',
    message: 'text-green-700',
    close: 'text-green-400 hover:text-green-600 hover:bg-green-100',
    progressBg: 'bg-green-100',
    progress: 'bg-green-500'
  },
  error: {
    container: 'bg-red-50/90 border-red-200',
    icon: 'text-red-500',
    title: 'text-red-900',
    message: 'text-red-700',
    close: 'text-red-400 hover:text-red-600 hover:bg-red-100',
    progressBg: 'bg-red-100',
    progress: 'bg-red-500'
  },
  warning: {
    container: 'bg-amber-50/90 border-amber-200',
    icon: 'text-amber-500',
    title: 'text-amber-900',
    message: 'text-amber-700',
    close: 'text-amber-400 hover:text-amber-600 hover:bg-amber-100',
    progressBg: 'bg-amber-100',
    progress: 'bg-amber-500'
  },
  info: {
    container: 'bg-blue-50/90 border-blue-200',
    icon: 'text-blue-500',
    title: 'text-blue-900',
    message: 'text-blue-700',
    close: 'text-blue-400 hover:text-blue-600 hover:bg-blue-100',
    progressBg: 'bg-blue-100',
    progress: 'bg-blue-500'
  }
}
</script>

<style scoped>
@keyframes toast-progress {
  from { width: 100%; }
  to { width: 0%; }
}
</style>
