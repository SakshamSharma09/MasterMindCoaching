<template>
  <div id="app" class="min-h-screen bg-gradient-to-br from-mastermind-50 via-white to-primary-50 relative overflow-hidden">
    <!-- Background decorative elements -->
    <div class="absolute inset-0 overflow-hidden pointer-events-none">
      <div class="absolute -top-40 -right-40 w-80 h-80 bg-gradient-to-br from-mastermind-200/30 to-primary-200/30 rounded-full blur-3xl animate-float-soft"></div>
      <div class="absolute -bottom-40 -left-40 w-80 h-80 bg-gradient-to-tr from-primary-200/30 to-mastermind-200/30 rounded-full blur-3xl animate-float-soft" style="animation-delay: 3s;"></div>
      <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-96 h-96 bg-gradient-to-r from-accent-100/20 to-mastermind-100/20 rounded-full blur-3xl animate-pulse"></div>
    </div>
    
    <!-- Main content -->
    <div class="relative z-10">
      <RouterView />
    </div>

    <!-- Global Toast Notifications -->
    <ToastContainer />
  </div>
</template>

<script setup lang="ts">
import { RouterView } from 'vue-router'
import ToastContainer from '@/components/common/ToastContainer.vue'
</script>

<style scoped>
/* App-specific styles */
#app {
  background-attachment: fixed;
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