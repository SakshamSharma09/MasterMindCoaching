<template>
  <aside class="mind-guide" :class="{ 'mind-guide--auth': isAuthRoute }" aria-label="MasterMind learning guide">
    <div class="mind-guide__bubble">
      <p class="mind-guide__eyebrow">{{ guideContext }}</p>
      <p class="mind-guide__text">{{ guideMessage }}</p>
    </div>

    <div class="mind-guide__stage" aria-hidden="true">
      <div class="mind-guide__halo"></div>
      <div class="mind-guide__figure">
        <div class="mind-guide__brain">
          <span class="mind-guide__fold fold-one"></span>
          <span class="mind-guide__fold fold-two"></span>
          <span class="mind-guide__fold fold-three"></span>
          <span class="mind-guide__spark spark-one"></span>
          <span class="mind-guide__spark spark-two"></span>
        </div>
        <div class="mind-guide__face">
          <span class="mind-guide__eye eye-left"></span>
          <span class="mind-guide__eye eye-right"></span>
          <span class="mind-guide__smile"></span>
        </div>
        <div class="mind-guide__book">
          <span></span>
          <span></span>
        </div>
      </div>
      <div class="mind-guide__shadow"></div>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'

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
  gap: 0.75rem;
  pointer-events: none;
  filter: drop-shadow(0 24px 40px rgba(10, 29, 57, 0.22));
}

.mind-guide__bubble {
  width: min(280px, 32vw);
  padding: 0.9rem 1rem;
  border: 1px solid rgba(223, 183, 88, 0.5);
  border-radius: 1.1rem 1.1rem 0.35rem 1.1rem;
  background:
    linear-gradient(135deg, rgba(255, 255, 255, 0.96), rgba(245, 250, 255, 0.9)),
    linear-gradient(90deg, rgba(223, 183, 88, 0.18) 1px, transparent 1px);
  background-size: auto, 18px 18px;
  box-shadow: 0 18px 45px -30px rgba(10, 29, 57, 0.5);
  transform: translateY(-0.35rem);
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

.mind-guide__stage {
  position: relative;
  width: 112px;
  height: 134px;
  perspective: 520px;
}

.mind-guide--auth {
  right: 1rem;
}

.mind-guide--auth .mind-guide__bubble {
  display: none;
}

.mind-guide__halo {
  position: absolute;
  inset: 3px 8px auto;
  height: 88px;
  border-radius: 999px;
  background: radial-gradient(circle, rgba(254, 211, 110, 0.55), rgba(119, 214, 201, 0.18) 52%, transparent 68%);
  animation: guideGlow 3.8s ease-in-out infinite;
}

.mind-guide__figure {
  position: absolute;
  inset: 8px 9px 12px;
  transform-style: preserve-3d;
  animation: guideFloat 5.5s ease-in-out infinite;
}

.mind-guide__brain {
  position: absolute;
  left: 18px;
  top: 3px;
  width: 62px;
  height: 50px;
  border: 3px solid #0b2141;
  border-radius: 48% 52% 44% 50%;
  background: linear-gradient(145deg, #fff8e6, #f0bf46 55%, #b9821f);
  box-shadow: inset -8px -10px 18px rgba(11, 33, 65, 0.16), inset 8px 8px 14px rgba(255, 255, 255, 0.8);
  transform: rotateX(13deg) rotateY(-16deg);
}

.mind-guide__fold {
  position: absolute;
  border: 2px solid rgba(11, 33, 65, 0.62);
  border-left: 0;
  border-bottom: 0;
  border-radius: 999px;
}

.fold-one {
  left: 12px;
  top: 13px;
  width: 19px;
  height: 16px;
}

.fold-two {
  left: 27px;
  top: 10px;
  width: 20px;
  height: 21px;
  transform: rotate(92deg);
}

.fold-three {
  left: 18px;
  top: 27px;
  width: 27px;
  height: 13px;
  transform: rotate(170deg);
}

.mind-guide__spark {
  position: absolute;
  width: 8px;
  height: 8px;
  border-radius: 999px;
  background: #77d6c9;
  box-shadow: 0 0 18px rgba(119, 214, 201, 0.9);
}

.spark-one {
  right: -8px;
  top: 2px;
  animation: sparkPulse 2.6s ease-in-out infinite;
}

.spark-two {
  left: -11px;
  bottom: 6px;
  background: #f5c04e;
  animation: sparkPulse 2.6s ease-in-out 1.1s infinite;
}

.mind-guide__face {
  position: absolute;
  left: 23px;
  top: 43px;
  width: 54px;
  height: 54px;
  border: 3px solid #0b2141;
  border-radius: 42% 42% 48% 48%;
  background: linear-gradient(145deg, #ffffff, #deefff);
  box-shadow: inset -8px -8px 16px rgba(15, 54, 96, 0.12);
  transform: translateZ(20px);
}

.mind-guide__eye {
  position: absolute;
  top: 20px;
  width: 7px;
  height: 7px;
  border-radius: 999px;
  background: #0b2141;
}

.eye-left { left: 15px; }
.eye-right { right: 15px; }

.mind-guide__smile {
  position: absolute;
  left: 18px;
  top: 31px;
  width: 18px;
  height: 8px;
  border-bottom: 3px solid #0b2141;
  border-radius: 0 0 999px 999px;
}

.mind-guide__book {
  position: absolute;
  left: 16px;
  bottom: 7px;
  display: flex;
  width: 68px;
  height: 34px;
  transform: rotateX(58deg) rotateZ(-2deg);
  transform-origin: center;
}

.mind-guide__book span {
  flex: 1;
  border: 3px solid #0b2141;
  background: linear-gradient(145deg, #ffffff, #d9fff7);
}

.mind-guide__book span:first-child {
  border-radius: 10px 3px 3px 10px;
}

.mind-guide__book span:last-child {
  border-left: 0;
  border-radius: 3px 10px 10px 3px;
}

.mind-guide__shadow {
  position: absolute;
  left: 23px;
  right: 18px;
  bottom: 1px;
  height: 16px;
  border-radius: 999px;
  background: rgba(10, 29, 57, 0.24);
  filter: blur(5px);
  animation: guideShadow 5.5s ease-in-out infinite;
}

@keyframes guideFloat {
  0%, 100% { transform: translateY(0) rotateY(-8deg) rotateX(2deg); }
  50% { transform: translateY(-9px) rotateY(9deg) rotateX(-2deg); }
}

@keyframes guideShadow {
  0%, 100% { transform: scaleX(0.94); opacity: 0.22; }
  50% { transform: scaleX(0.72); opacity: 0.14; }
}

@keyframes guideGlow {
  0%, 100% { transform: scale(0.94); opacity: 0.65; }
  50% { transform: scale(1.08); opacity: 0.95; }
}

@keyframes sparkPulse {
  0%, 100% { transform: scale(0.75); opacity: 0.5; }
  50% { transform: scale(1.25); opacity: 1; }
}

@media (max-width: 900px) {
  .mind-guide {
    right: 0.55rem;
    bottom: calc(0.7rem + env(safe-area-inset-bottom));
    transform: scale(0.82);
    transform-origin: right bottom;
  }

  .mind-guide__bubble {
    display: none;
  }
}

@media (prefers-reduced-motion: reduce) {
  .mind-guide__figure,
  .mind-guide__halo,
  .mind-guide__shadow,
  .mind-guide__spark {
    animation: none;
  }
}
</style>
