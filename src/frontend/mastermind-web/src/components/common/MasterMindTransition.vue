<template>
  <transition
    :name="transitionName"
    :appear="appear"
    :mode="mode"
    @before-enter="onBeforeEnter"
    @enter="onEnter"
    @after-enter="onAfterEnter"
    @before-leave="onBeforeLeave"
    @leave="onLeave"
    @after-leave="onAfterLeave"
  >
    <slot />
  </transition>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  type?: 'fade' | 'slide' | 'scale' | 'slide-up' | 'slide-left' | 'slide-right' | 'slide-down'
  duration?: 'fast' | 'normal' | 'slow'
  delay?: number
  appear?: boolean
  mode?: 'default' | 'out-in' | 'in-out'
  stagger?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  type: 'fade',
  duration: 'normal',
  delay: 0,
  appear: true,
  mode: 'default',
  stagger: false
})

const emit = defineEmits<{
  beforeEnter: []
  enter: []
  afterEnter: []
  beforeLeave: []
  leave: []
  afterLeave: []
}>()

const transitionName = computed(() => `mastermind-${props.type}`)

const durationClass = computed(() => {
  const durations = {
    fast: 'duration-200',
    normal: 'duration-300',
    slow: 'duration-500'
  }
  return durations[props.duration]
})

const onBeforeEnter = (el: Element) => {
  if (props.stagger) {
    ;(el as HTMLElement).style.animationDelay = `${props.delay}ms`
  }
  emit('beforeEnter')
}

const onEnter = (el: Element) => {
  emit('enter')
}

const onAfterEnter = (el: Element) => {
  emit('afterEnter')
}

const onBeforeLeave = (el: Element) => {
  emit('beforeLeave')
}

const onLeave = (el: Element) => {
  emit('leave')
}

const onAfterLeave = (el: Element) => {
  emit('afterLeave')
}
</script>

<style scoped>
/* Fade transition */
.mastermind-fade-enter-active,
.mastermind-fade-leave-active {
  transition: all 0.3s ease-out;
}

.mastermind-fade-enter-from,
.mastermind-fade-leave-to {
  opacity: 0;
}

/* Slide transition */
.mastermind-slide-enter-active,
.mastermind-slide-leave-active {
  transition: all 0.3s ease-out;
}

.mastermind-slide-enter-from {
  opacity: 0;
  transform: translateX(-20px);
}

.mastermind-slide-leave-to {
  opacity: 0;
  transform: translateX(20px);
}

/* Scale transition */
.mastermind-scale-enter-active,
.mastermind-scale-leave-active {
  transition: all 0.3s ease-out;
}

.mastermind-scale-enter-from,
.mastermind-scale-leave-to {
  opacity: 0;
  transform: scale(0.9);
}

/* Slide Up transition */
.mastermind-slide-up-enter-active,
.mastermind-slide-up-leave-active {
  transition: all 0.4s ease-out;
}

.mastermind-slide-up-enter-from {
  opacity: 0;
  transform: translateY(30px) scale(0.95);
}

.mastermind-slide-up-leave-to {
  opacity: 0;
  transform: translateY(-30px) scale(0.95);
}

/* Slide Left transition */
.mastermind-slide-left-enter-active,
.mastermind-slide-left-leave-active {
  transition: all 0.3s ease-out;
}

.mastermind-slide-left-enter-from {
  opacity: 0;
  transform: translateX(30px);
}

.mastermind-slide-left-leave-to {
  opacity: 0;
  transform: translateX(-30px);
}

/* Slide Right transition */
.mastermind-slide-right-enter-active,
.mastermind-slide-right-leave-active {
  transition: all 0.3s ease-out;
}

.mastermind-slide-right-enter-from {
  opacity: 0;
  transform: translateX(-30px);
}

.mastermind-slide-right-leave-to {
  opacity: 0;
  transform: translateX(30px);
}

/* Slide Down transition */
.mastermind-slide-down-enter-active,
.mastermind-slide-down-leave-active {
  transition: all 0.4s ease-out;
}

.mastermind-slide-down-enter-from {
  opacity: 0;
  transform: translateY(-30px) scale(0.95);
}

.mastermind-slide-down-leave-to {
  opacity: 0;
  transform: translateY(30px) scale(0.95);
}

/* Stagger animation support */
.mastermind-stagger-enter-active {
  transition: all 0.5s ease-out;
}

.mastermind-stagger-enter-from {
  opacity: 0;
  transform: translateY(20px);
}
</style>
