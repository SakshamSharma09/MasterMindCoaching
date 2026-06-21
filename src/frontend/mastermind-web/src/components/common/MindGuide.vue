<template>
  <aside class="mind-guide" :class="{ 'mind-guide--auth': isAuthRoute }" aria-label="MasterMind learning guide">
    <div class="mind-guide__bubble">
      <p class="mind-guide__eyebrow">{{ guideContext }}</p>
      <p class="mind-guide__text">{{ guideMessage }}</p>
    </div>

    <div class="mind-guide__orb" aria-hidden="true">
      <span class="mind-guide__ring ring-one"></span>
      <span class="mind-guide__ring ring-two"></span>
      <img class="mind-guide__image" :src="mindGuideImage" alt="" />
      <span class="mind-guide__spark spark-one"></span>
      <span class="mind-guide__spark spark-two"></span>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import mindGuideImage from '@/assets/images/mind-guide.png'

const route = useRoute()

const isAuthRoute = computed(() => route.path.includes('login') || route.path.includes('otp-verify'))

const guideContext = computed(() => {
  if (route.path.startsWith('/admin')) return 'Admin mentor'
  if (route.path.startsWith('/teacher')) return 'Teacher companion'
  if (route.path.startsWith('/parent')) return 'Family guide'
  return 'Learning guide'
})

const guideMessage = computed(() => {
  const path = route.path
  if (path.includes('finance')) return 'Keep every fee, receipt, and reminder clear for families.'
  if (path.includes('template-zone')) return 'Turn student moments into warm, share-ready messages.'
  if (path.includes('students')) return 'Student details are the heart of every class plan.'
  if (path.includes('classes')) return 'Build classes around subjects, rhythm, and confidence.'
  if (path.includes('attendance')) return 'Mark presence quickly, then notice patterns early.'
  if (path.includes('remarks')) return 'Write remarks that help parents and students take action.'
  if (path.includes('sessions')) return 'A clean academic year keeps every record in its place.'
  if (path.includes('login')) return 'Choose your role path and continue securely.'
  return 'I will stay nearby while you guide learning with care.'
})
</script>

<style scoped>
.mind-guide {
  position: fixed;
  right: clamp(0.75rem, 2vw, 1.5rem);
  bottom: calc(1rem + env(safe-area-inset-bottom));
  z-index: 35;
  display: flex;
  align-items: flex-end;
  gap: 0.8rem;
  pointer-events: none;
}

.mind-guide__bubble {
  width: min(286px, 32vw);
  padding: 0.9rem 1rem;
  border: 1px solid rgba(217, 161, 45, 0.46);
  border-radius: 1.15rem 1.15rem 0.35rem 1.15rem;
  background:
    linear-gradient(135deg, rgba(255, 255, 255, 0.96), rgba(246, 251, 255, 0.9)),
    linear-gradient(90deg, rgba(217, 161, 45, 0.14) 1px, transparent 1px);
  background-size: auto, 18px 18px;
  box-shadow: 0 18px 45px -30px rgba(10, 29, 57, 0.5);
  transform: translateY(-0.45rem);
}

.mind-guide__eyebrow {
  margin: 0;
  color: #9b6b12;
  font-size: 0.68rem;
  font-weight: 800;
  letter-spacing: 0.14em;
  text-transform: uppercase;
}

.mind-guide__text {
  margin: 0.25rem 0 0;
  color: #10223f;
  font-size: 0.82rem;
  font-weight: 650;
  line-height: 1.45;
}

.mind-guide__orb {
  position: relative;
  width: 128px;
  height: 128px;
  border-radius: 999px;
  background:
    radial-gradient(circle at 30% 20%, rgba(255, 255, 255, 0.96), rgba(255, 255, 255, 0.62) 32%, transparent 62%),
    linear-gradient(135deg, rgba(10, 29, 57, 0.92), rgba(25, 166, 140, 0.86));
  box-shadow:
    0 28px 50px -28px rgba(10, 29, 57, 0.58),
    inset 0 1px 0 rgba(255, 255, 255, 0.45);
  transform-style: preserve-3d;
  animation: guideFloat 5.6s ease-in-out infinite;
}

.mind-guide__image {
  position: absolute;
  inset: 6px;
  width: calc(100% - 12px);
  height: calc(100% - 12px);
  border-radius: 999px;
  object-fit: cover;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.38);
  transform: translateZ(18px);
}

.mind-guide__ring {
  position: absolute;
  inset: -8px;
  border-radius: 999px;
  border: 1px solid rgba(217, 161, 45, 0.45);
}

.ring-one {
  animation: ringPulse 3.8s ease-in-out infinite;
}

.ring-two {
  inset: 8px;
  border-color: rgba(119, 214, 201, 0.36);
  animation: ringPulse 3.8s ease-in-out 1.2s infinite;
}

.mind-guide__spark {
  position: absolute;
  width: 9px;
  height: 9px;
  border-radius: 999px;
  background: #f5c04e;
  box-shadow: 0 0 22px rgba(245, 192, 78, 0.9);
}

.spark-one {
  right: -2px;
  top: 16px;
  animation: sparkPulse 2.6s ease-in-out infinite;
}

.spark-two {
  left: 6px;
  bottom: 18px;
  background: #77d6c9;
  box-shadow: 0 0 22px rgba(119, 214, 201, 0.9);
  animation: sparkPulse 2.6s ease-in-out 1.1s infinite;
}

.mind-guide--auth {
  right: 1rem;
}

.mind-guide--auth .mind-guide__bubble {
  display: none;
}

@keyframes guideFloat {
  0%, 100% { transform: translateY(0) rotateY(-7deg) rotateX(2deg); }
  50% { transform: translateY(-9px) rotateY(7deg) rotateX(-2deg); }
}

@keyframes ringPulse {
  0%, 100% { transform: scale(0.96); opacity: 0.55; }
  50% { transform: scale(1.06); opacity: 0.9; }
}

@keyframes sparkPulse {
  0%, 100% { transform: scale(0.75); opacity: 0.5; }
  50% { transform: scale(1.25); opacity: 1; }
}

@media (max-width: 900px) {
  .mind-guide {
    right: 0.6rem;
    bottom: calc(0.7rem + env(safe-area-inset-bottom));
    transform: scale(0.76);
    transform-origin: right bottom;
  }

  .mind-guide__bubble {
    display: none;
  }
}

@media (prefers-reduced-motion: reduce) {
  .mind-guide__orb,
  .mind-guide__ring,
  .mind-guide__spark {
    animation: none;
  }
}
</style>
