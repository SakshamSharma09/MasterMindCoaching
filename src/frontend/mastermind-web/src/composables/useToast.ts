import { ref } from 'vue'

export type ToastType = 'success' | 'error' | 'warning' | 'info'

export interface Toast {
  id: number
  type: ToastType
  title: string
  message?: string
  duration: number
}

const toasts = ref<Toast[]>([])
let nextId = 0

const MAX_TOASTS = 5

function addToast(type: ToastType, title: string, message?: string, duration = 4000): void {
  const id = nextId++
  toasts.value.push({ id, type, title, message, duration })

  if (toasts.value.length > MAX_TOASTS) {
    toasts.value.splice(0, toasts.value.length - MAX_TOASTS)
  }

  if (duration > 0) {
    setTimeout(() => removeToast(id), duration)
  }
}

function removeToast(id: number): void {
  const index = toasts.value.findIndex(t => t.id === id)
  if (index !== -1) {
    toasts.value.splice(index, 1)
  }
}

export function useToast() {
  return {
    toasts,
    removeToast,
    success(title: string, message?: string) {
      addToast('success', title, message)
    },
    error(title: string, message?: string) {
      addToast('error', title, message, 5000)
    },
    warning(title: string, message?: string) {
      addToast('warning', title, message, 4500)
    },
    info(title: string, message?: string) {
      addToast('info', title, message)
    }
  }
}
