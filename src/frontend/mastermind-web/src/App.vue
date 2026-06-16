<template>
  <div id="app" class="app-safe-area min-h-[100dvh] mentor-app-shell relative overflow-hidden">
    <div class="mentor-paper-plane pointer-events-none absolute inset-0 -z-10"></div>
    
    <!-- Main content -->
    <div class="relative z-10">
      <RouterView />
    </div>

    <MindGuide />

    <!-- Global Toast Notifications -->
    <ToastContainer />
  </div>
</template>

<script setup lang="ts">
import { RouterView } from 'vue-router'
import MindGuide from '@/components/common/MindGuide.vue'
import ToastContainer from '@/components/common/ToastContainer.vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

// Initialize auth from stored tokens on app startup
authStore.initializeAuth()
</script>

<style scoped>
.mentor-app-shell {
  background:
    linear-gradient(120deg, rgba(245, 192, 78, 0.08), rgba(119, 214, 201, 0.12) 38%, rgba(96, 73, 232, 0.08)),
    #f7f9fc;
  background-attachment: fixed;
}

.mentor-paper-plane {
  background:
    linear-gradient(rgba(12, 31, 59, 0.035) 1px, transparent 1px),
    linear-gradient(90deg, rgba(12, 31, 59, 0.025) 1px, transparent 1px),
    radial-gradient(circle at 18% 14%, rgba(245, 192, 78, 0.14), transparent 28%),
    radial-gradient(circle at 85% 22%, rgba(119, 214, 201, 0.16), transparent 30%);
  background-size: 34px 34px, 34px 34px, auto, auto;
  mask-image: linear-gradient(180deg, black 0%, black 82%, transparent 100%);
}

/* Custom shimmer effect for premium feel */
@keyframes shimmer {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(100%); }
}

.shimmer::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(
    90deg,
    transparent,
    rgba(255, 255, 255, 0.2),
    transparent
  );
  animation: shimmer 2s infinite;
}
</style>
