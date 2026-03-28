<template>
  <div class="relative">
    <!-- Session Selector Button -->
    <button
      @click="showDropdown = !showDropdown"
      class="flex items-center space-x-2 px-4 py-2 bg-white border border-gray-300 rounded-lg shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500"
    >
      <svg class="w-5 h-5 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"></path>
      </svg>
      <span class="font-medium text-gray-900">
        {{ selectedSession?.displayName || 'Select Session' }}
      </span>
      <svg class="w-4 h-4 text-gray-400" :class="{ 'rotate-180': showDropdown }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path>
      </svg>
    </button>

    <!-- Dropdown Menu -->
    <div
      v-if="showDropdown"
      class="absolute right-0 mt-2 w-80 bg-white border border-gray-200 rounded-lg shadow-lg z-50"
    >
      <div class="p-4 border-b border-gray-200">
        <div class="flex items-center justify-between">
          <h3 class="text-sm font-semibold text-gray-900">Academic Sessions</h3>
          <button
            @click="showCreateForm = true"
            class="text-sm text-blue-600 hover:text-blue-700 font-medium"
          >
            + New Session
          </button>
        </div>
      </div>

      <!-- Session List -->
      <div class="max-h-64 overflow-y-auto">
        <div
          v-for="session in sessions"
          :key="session.id"
          @click="selectSession(session.id)"
          class="px-4 py-3 hover:bg-gray-50 cursor-pointer border-b border-gray-100 last:border-b-0"
          :class="{ 'bg-blue-50': selectedSessionId === session.id }"
        >
          <div class="flex items-center justify-between">
            <div>
              <div class="font-medium text-gray-900">{{ session.displayName }}</div>
              <div class="text-sm text-gray-500">{{ session.academicYear }}</div>
              <div class="text-xs text-gray-400">
                {{ formatDate(session.startDate) }} - {{ formatDate(session.endDate) }}
              </div>
            </div>
            <div class="flex items-center space-x-2">
              <span
                v-if="session.isActive"
                class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800"
              >
                Active
              </span>
              <span
                v-else
                class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium"
                :class="getStatusClass(session.status)"
              >
                {{ session.status }}
              </span>
              <button
                v-if="!session.isActive && session.status !== 'Completed'"
                @click.stop="activateSession(session.id)"
                class="text-blue-600 hover:text-blue-700"
                title="Activate this session"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                </svg>
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Create Session Form -->
      <div v-if="showCreateForm" class="p-4 border-t border-gray-200 bg-gray-50">
        <h4 class="text-sm font-semibold text-gray-900 mb-3">Create New Session</h4>
        <form @submit.prevent="createSession" class="space-y-3">
          <div>
            <label class="block text-xs font-medium text-gray-700 mb-1">Session Name</label>
            <input
              v-model="createForm.name"
              type="text"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="e.g., 2025-26"
            />
          </div>
          <div>
            <label class="block text-xs font-medium text-gray-700 mb-1">Display Name</label>
            <input
              v-model="createForm.displayName"
              type="text"
              class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="e.g., Academic Year 2025-26"
            />
          </div>
          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-xs font-medium text-gray-700 mb-1">Start Date</label>
              <input
                v-model="createForm.startDate"
                type="date"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-700 mb-1">End Date</label>
              <input
                v-model="createForm.endDate"
                type="date"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
            </div>
          </div>
          <div class="flex justify-end space-x-2">
            <button
              type="button"
              @click="showCreateForm = false; resetCreateForm()"
              class="px-3 py-1 text-sm text-gray-600 hover:text-gray-700"
            >
              Cancel
            </button>
            <button
              type="submit"
              :disabled="loading"
              class="px-3 py-1 bg-blue-600 text-white text-sm rounded-md hover:bg-blue-700 disabled:opacity-50"
            >
              {{ loading ? 'Creating...' : 'Create' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Overlay to close dropdown -->
    <div
      v-if="showDropdown"
      @click="showDropdown = false"
      class="fixed inset-0 z-40"
    ></div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useSessionStore, type Session } from '@/stores/session'
import { format } from 'date-fns'

const sessionStore = useSessionStore()

// Local state
const showDropdown = ref(false)
const showCreateForm = ref(false)
const createForm = ref({
  name: '',
  displayName: '',
  startDate: '',
  endDate: ''
})

// Computed
const sessions = computed(() => sessionStore.sessions)
const selectedSessionId = computed(() => sessionStore.selectedSessionId)
const selectedSession = computed(() => sessionStore.selectedSession)
const loading = computed(() => sessionStore.loading)

// Methods
const selectSession = (sessionId: number) => {
  sessionStore.selectSession(sessionId)
  showDropdown.value = false
}

const activateSession = async (sessionId: number) => {
  try {
    await sessionStore.activateSession(sessionId)
  } catch (error) {
    console.error('Failed to activate session:', error)
  }
}

const createSession = async () => {
  try {
    await sessionStore.createSession({
      ...createForm.value,
      academicYear: createForm.value.name,
      status: 'Planned',
      isActive: false
    })
    showCreateForm.value = false
    resetCreateForm()
  } catch (error) {
    console.error('Failed to create session:', error)
  }
}

const resetCreateForm = () => {
  createForm.value = {
    name: '',
    displayName: '',
    startDate: '',
    endDate: ''
  }
}

const formatDate = (dateString: string) => {
  return format(new Date(dateString), 'MMM dd, yyyy')
}

const getStatusClass = (status: string) => {
  const classes = {
    'Planned': 'bg-blue-100 text-blue-800',
    'Active': 'bg-green-100 text-green-800',
    'Completed': 'bg-gray-100 text-gray-800',
    'Suspended': 'bg-yellow-100 text-yellow-800',
    'Cancelled': 'bg-red-100 text-red-800'
  }
  return classes[status as keyof typeof classes] || 'bg-gray-100 text-gray-800'
}

// Load sessions on mount
onMounted(() => {
  if (sessions.value.length === 0) {
    sessionStore.loadSessions()
  }
})
</script>

<style scoped>
.rotate-180 {
  transform: rotate(180deg);
}
</style>
