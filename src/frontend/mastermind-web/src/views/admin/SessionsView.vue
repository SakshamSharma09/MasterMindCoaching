<template>
  <div class="space-y-6">
    <!-- Page Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Academic Sessions</h1>
        <p class="text-gray-600">Manage academic sessions and their associated data</p>
      </div>
      <button
        @click="showCreateModal = true"
        class="btn-primary"
      >
        <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path>
        </svg>
        Create New Session
      </button>
    </div>

    <!-- Sessions List -->
    <div class="card">
      <div class="card-header">
        <h3 class="card-title">All Sessions</h3>
        <p class="card-description">Manage academic year sessions</p>
      </div>
      
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Session Details
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Duration
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Statistics
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="session in sessions" :key="session.id" class="hover:bg-gray-50">
              <td class="px-6 py-4 whitespace-nowrap">
                <div>
                  <div class="text-sm font-medium text-gray-900">{{ session.displayName }}</div>
                  <div class="text-sm text-gray-500">{{ session.academicYear }}</div>
                  <div v-if="session.description" class="text-xs text-gray-400 mt-1">
                    {{ session.description }}
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm text-gray-900">
                  {{ formatDate(session.startDate) }} - {{ formatDate(session.endDate) }}
                </div>
                <div class="text-xs text-gray-500">
                  {{ getDuration(session.startDate, session.endDate) }}
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span
                  class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                  :class="getStatusClass(session.status, session.isActive)"
                >
                  <span v-if="session.isActive" class="w-2 h-2 mr-1 bg-green-400 rounded-full"></span>
                  {{ session.isActive ? 'Active' : session.status }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="grid grid-cols-2 gap-2 text-sm">
                  <div>
                    <span class="text-gray-500">Students:</span>
                    <span class="ml-1 font-medium">{{ session.activeStudents }}/{{ session.totalStudents }}</span>
                  </div>
                  <div>
                    <span class="text-gray-500">Classes:</span>
                    <span class="ml-1 font-medium">{{ session.activeClasses }}/{{ session.totalClasses }}</span>
                  </div>
                  <div>
                    <span class="text-gray-500">Teachers:</span>
                    <span class="ml-1 font-medium">{{ session.totalTeachers }}</span>
                  </div>
                  <div>
                    <span class="text-gray-500">Revenue:</span>
                    <span class="ml-1 font-medium">₹{{ session.totalRevenue.toLocaleString() }}</span>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                <div class="flex items-center space-x-2">
                  <button
                    @click="navigateToSession(session.id)"
                    class="text-blue-600 hover:text-blue-900"
                    title="View Session Details"
                  >
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path>
                    </svg>
                  </button>
                  <button
                    v-if="!session.isActive && session.status !== 'Completed'"
                    @click="activateSession(session.id)"
                    class="text-green-600 hover:text-green-900"
                    title="Activate Session"
                  >
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                    </svg>
                  </button>
                  <button
                    @click="editSession(session)"
                    class="text-indigo-600 hover:text-indigo-900"
                    title="Edit Session"
                  >
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path>
                    </svg>
                  </button>
                  <button
                    v-if="session.status === 'Completed'"
                    @click="archiveSession(session.id)"
                    class="text-gray-600 hover:text-gray-900"
                    title="Archive Session"
                  >
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4"></path>
                    </svg>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        
        <div v-if="sessions.length === 0" class="text-center py-12">
          <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"></path>
          </svg>
          <h3 class="mt-2 text-sm font-medium text-gray-900">No sessions</h3>
          <p class="mt-1 text-sm text-gray-500">Get started by creating a new academic session.</p>
          <div class="mt-6">
            <button @click="showCreateModal = true" class="btn-primary">
              <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path>
              </svg>
              Create Session
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Create/Edit Session Modal -->
    <div v-if="showCreateModal || showEditModal" class="fixed inset-0 z-50 overflow-y-auto">
      <div class="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
        <div class="fixed inset-0 transition-opacity" @click="closeModals">
          <div class="absolute inset-0 bg-gray-500 opacity-75"></div>
        </div>

        <div class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
          <form @submit.prevent="isEditing ? updateSession() : createSession()">
            <div class="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
              <div class="mb-4">
                <h3 class="text-lg leading-6 font-medium text-gray-900">
                  {{ isEditing ? 'Edit Session' : 'Create New Session' }}
                </h3>
                <p class="mt-1 text-sm text-gray-500">
                  {{ isEditing ? 'Update session information' : 'Add a new academic session' }}
                </p>
              </div>

              <div class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700">Session Name *</label>
                  <input
                    v-model="formData.name"
                    type="text"
                    required
                    class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                    placeholder="e.g., 2025-26"
                  />
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700">Display Name</label>
                  <input
                    v-model="formData.displayName"
                    type="text"
                    class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                    placeholder="e.g., Academic Year 2025-26"
                  />
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700">Description</label>
                  <textarea
                    v-model="formData.description"
                    rows="3"
                    class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                    placeholder="Optional description for the session"
                  ></textarea>
                </div>

                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Start Date *</label>
                    <input
                      v-model="formData.startDate"
                      type="date"
                      required
                      class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                    />
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700">End Date *</label>
                    <input
                      v-model="formData.endDate"
                      type="date"
                      required
                      class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                    />
                  </div>
                </div>

                <div v-if="isEditing">
                  <label class="block text-sm font-medium text-gray-700">Status</label>
                  <select
                    v-model="formData.status"
                    class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  >
                    <option value="Planned">Planned</option>
                    <option value="Active">Active</option>
                    <option value="Completed">Completed</option>
                    <option value="Suspended">Suspended</option>
                    <option value="Cancelled">Cancelled</option>
                  </select>
                </div>
              </div>
            </div>

            <div class="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
              <button
                type="submit"
                :disabled="loading"
                class="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-blue-600 text-base font-medium text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50"
              >
                {{ loading ? 'Saving...' : (isEditing ? 'Update' : 'Create') }}
              </button>
              <button
                type="button"
                @click="closeModals"
                class="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useSessionStore, type Session } from '@/stores/session'
import { format, differenceInDays, parseISO } from 'date-fns'

const router = useRouter()
const sessionStore = useSessionStore()

// Modal states
const showCreateModal = ref(false)
const showEditModal = ref(false)
const isEditing = ref(false)
const editingSession = ref<Session | null>(null)

// Form data
const formData = ref({
  name: '',
  displayName: '',
  description: '',
  startDate: '',
  endDate: '',
  status: 'Planned'
})

// Computed
const sessions = computed(() => sessionStore.sessions)
const loading = computed(() => sessionStore.loading)

// Methods
const createSession = async () => {
  try {
    await sessionStore.createSession({
      ...formData.value,
      academicYear: formData.value.name,
      status: formData.value.status as any,
      isActive: false
    })
    closeModals()
  } catch (error) {
    console.error('Failed to create session:', error)
  }
}

const editSession = (session: Session) => {
  editingSession.value = session
  formData.value = {
    name: session.name,
    displayName: session.displayName,
    description: session.description || '',
    startDate: session.startDate.split('T')[0],
    endDate: session.endDate.split('T')[0],
    status: session.status
  }
  isEditing.value = true
  showEditModal.value = true
}

const updateSession = async () => {
  // TODO: Implement update functionality
  console.log('Update session:', editingSession.value?.id, formData.value)
  closeModals()
}

const activateSession = async (sessionId: number) => {
  try {
    await sessionStore.activateSession(sessionId)
  } catch (error) {
    console.error('Failed to activate session:', error)
  }
}

const archiveSession = async (sessionId: number) => {
  // TODO: Implement archive functionality
  console.log('Archive session:', sessionId)
}

const navigateToSession = (sessionId: number) => {
  sessionStore.selectSession(sessionId)
  router.push('/admin/dashboard')
}

const closeModals = () => {
  showCreateModal.value = false
  showEditModal.value = false
  isEditing.value = false
  editingSession.value = null
  resetForm()
}

const resetForm = () => {
  formData.value = {
    name: '',
    displayName: '',
    description: '',
    startDate: '',
    endDate: '',
    status: 'Planned'
  }
}

const formatDate = (dateString: string) => {
  return format(new Date(dateString), 'MMM dd, yyyy')
}

const getDuration = (startDate: string, endDate: string) => {
  const days = differenceInDays(parseISO(endDate), parseISO(startDate))
  const months = Math.floor(days / 30)
  const remainingDays = days % 30
  
  if (months > 0) {
    return `${months} month${months > 1 ? 's' : ''}${remainingDays > 0 ? ` ${remainingDays} days` : ''}`
  }
  return `${days} days`
}

const getStatusClass = (status: string, isActive: boolean) => {
  if (isActive) {
    return 'bg-green-100 text-green-800'
  }
  
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
  sessionStore.loadSessions()
})
</script>
