<template>
  <div class="space-y-6">
    <div class="flex flex-col gap-4 rounded-2xl border border-slate-200 bg-white/95 p-5 shadow-card sm:flex-row sm:items-center sm:justify-between">
      <div>
        <p class="text-sm font-semibold uppercase tracking-wide text-blue-600">Academic Setup</p>
        <h1 class="mt-1 text-2xl font-bold text-gray-900">Academic Sessions</h1>
        <p class="mt-1 text-sm text-gray-600">
          Switch academic years and keep students, classes, teachers, and finance data separated.
        </p>
      </div>
      <button type="button" class="btn-primary w-full sm:w-auto" @click="showCreateModal = true">
        <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
        </svg>
        Create Session
      </button>
    </div>

    <div class="space-y-4">
      <div class="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 class="text-lg font-semibold text-gray-900">All Sessions</h2>
          <p class="text-sm text-gray-500">
            {{ sessions.length }} academic year{{ sessions.length === 1 ? '' : 's' }} configured
          </p>
        </div>
        <span v-if="selectedSession" class="inline-flex w-fit rounded-full bg-blue-50 px-3 py-1 text-xs font-semibold text-blue-700">
          Viewing {{ selectedSession.displayName }}
        </span>
      </div>

      <div v-if="sessions.length > 0" class="grid grid-cols-1 gap-4 xl:grid-cols-2">
        <article
          v-for="session in sessions"
          :key="session.id"
          class="relative overflow-hidden rounded-2xl border bg-white p-5 shadow-card transition-all duration-200 hover:-translate-y-0.5 hover:shadow-lg"
          :class="session.isActive ? 'border-blue-200 ring-2 ring-blue-500/10' : 'border-slate-200'"
        >
          <div class="absolute inset-y-0 left-0 w-1.5" :class="session.isActive ? 'bg-blue-600' : 'bg-slate-200'"></div>

          <div class="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
            <div class="min-w-0 pl-2">
              <div class="flex flex-wrap items-center gap-2">
                <h3 class="truncate text-lg font-bold text-gray-950">{{ session.displayName }}</h3>
                <span
                  class="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold"
                  :class="getStatusClass(session.status, session.isActive)"
                >
                  <span v-if="session.isActive" class="mr-1.5 h-2 w-2 rounded-full bg-green-400"></span>
                  {{ session.isActive ? 'Active' : session.status }}
                </span>
              </div>
              <p class="mt-1 text-sm text-gray-500">{{ session.academicYear }}</p>
              <p v-if="session.description" class="mt-2 line-clamp-2 text-sm text-gray-500">
                {{ session.description }}
              </p>
            </div>

            <button type="button" class="btn-secondary shrink-0" @click="navigateToSession(session.id)">
              View
            </button>
          </div>

          <div class="mt-5 rounded-xl border border-slate-100 bg-slate-50/80 p-4">
            <div class="flex flex-col gap-1 sm:flex-row sm:items-center sm:justify-between">
              <p class="text-sm font-semibold text-gray-900">
                {{ formatDate(session.startDate) }} - {{ formatDate(session.endDate) }}
              </p>
              <p class="text-xs font-medium text-gray-500">{{ getDuration(session.startDate, session.endDate) }}</p>
            </div>
          </div>

          <div class="mt-4 grid grid-cols-2 gap-3 sm:grid-cols-4">
            <div class="rounded-xl border border-slate-100 bg-white p-3">
              <p class="text-xs font-medium text-gray-500">Students</p>
              <p class="mt-1 text-base font-bold text-gray-950">{{ session.activeStudents }}/{{ session.totalStudents }}</p>
            </div>
            <div class="rounded-xl border border-slate-100 bg-white p-3">
              <p class="text-xs font-medium text-gray-500">Classes</p>
              <p class="mt-1 text-base font-bold text-gray-950">{{ session.activeClasses }}/{{ session.totalClasses }}</p>
            </div>
            <div class="rounded-xl border border-slate-100 bg-white p-3">
              <p class="text-xs font-medium text-gray-500">Teachers</p>
              <p class="mt-1 text-base font-bold text-gray-950">{{ session.totalTeachers }}</p>
            </div>
            <div class="rounded-xl border border-slate-100 bg-white p-3">
              <p class="text-xs font-medium text-gray-500">Revenue</p>
              <p class="mt-1 text-base font-bold text-gray-950">Rs {{ session.totalRevenue.toLocaleString() }}</p>
            </div>
          </div>

          <div class="mt-5 flex flex-wrap gap-2 border-t border-slate-100 pt-4">
            <button
              v-if="!session.isActive && session.status !== 'Completed'"
              type="button"
              class="btn-primary"
              @click="activateSession(session.id)"
            >
              Activate
            </button>
            <button type="button" class="btn-secondary" @click="editSession(session)">
              Edit
            </button>
            <button
              v-if="session.status === 'Completed'"
              type="button"
              class="btn-secondary"
              @click="archiveSession(session.id)"
            >
              Archive
            </button>
          </div>
        </article>
      </div>

      <div v-else class="rounded-2xl border border-dashed border-slate-300 bg-white p-8 text-center shadow-card">
        <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
        </svg>
        <h3 class="mt-3 text-sm font-semibold text-gray-900">No sessions yet</h3>
        <p class="mt-1 text-sm text-gray-500">Create the first academic session to start organizing data.</p>
        <button type="button" class="btn-primary mt-5" @click="showCreateModal = true">
          Create Session
        </button>
      </div>
    </div>

    <div v-if="showCreateModal || showEditModal" class="fixed inset-0 z-50 overflow-y-auto">
      <div class="flex min-h-screen items-center justify-center px-4 py-6">
        <div class="fixed inset-0 bg-slate-900/60 backdrop-blur-sm" @click="closeModals"></div>

        <div class="relative w-full max-w-xl overflow-hidden rounded-2xl bg-white shadow-2xl">
          <form @submit.prevent="isEditing ? updateSession() : createSession()">
            <div class="border-b border-slate-100 px-6 py-5">
              <h3 class="text-lg font-bold text-gray-950">
                {{ isEditing ? 'Edit Session' : 'Create Session' }}
              </h3>
              <p class="mt-1 text-sm text-gray-500">
                {{ isEditing ? 'Update this academic year.' : 'Add a new academic year for session-specific data.' }}
              </p>
            </div>

            <div class="space-y-4 px-6 py-5">
              <div>
                <label class="block text-sm font-medium text-gray-700">Session Name *</label>
                <input
                  v-model="formData.name"
                  type="text"
                  required
                  class="mt-1 block w-full rounded-xl border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
                  placeholder="2026-27"
                />
              </div>

              <div>
                <label class="block text-sm font-medium text-gray-700">Display Name</label>
                <input
                  v-model="formData.displayName"
                  type="text"
                  class="mt-1 block w-full rounded-xl border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
                  placeholder="Academic Year 2026-27"
                />
              </div>

              <div>
                <label class="block text-sm font-medium text-gray-700">Description</label>
                <textarea
                  v-model="formData.description"
                  rows="3"
                  class="mt-1 block w-full rounded-xl border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
                  placeholder="Optional note for this session"
                ></textarea>
              </div>

              <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <div>
                  <label class="block text-sm font-medium text-gray-700">Start Date *</label>
                  <input
                    v-model="formData.startDate"
                    type="date"
                    required
                    class="mt-1 block w-full rounded-xl border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
                  />
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">End Date *</label>
                  <input
                    v-model="formData.endDate"
                    type="date"
                    required
                    class="mt-1 block w-full rounded-xl border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
                  />
                </div>
              </div>

              <div v-if="isEditing">
                <label class="block text-sm font-medium text-gray-700">Status</label>
                <select
                  v-model="formData.status"
                  class="mt-1 block w-full rounded-xl border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
                >
                  <option value="Planned">Planned</option>
                  <option value="Active">Active</option>
                  <option value="Completed">Completed</option>
                  <option value="Suspended">Suspended</option>
                  <option value="Cancelled">Cancelled</option>
                </select>
              </div>
            </div>

            <div class="flex flex-col-reverse gap-3 bg-slate-50 px-6 py-4 sm:flex-row sm:justify-end">
              <button type="button" class="btn-secondary" @click="closeModals">
                Cancel
              </button>
              <button type="submit" :disabled="loading" class="btn-primary disabled:opacity-50">
                {{ loading ? 'Saving...' : (isEditing ? 'Update Session' : 'Create Session') }}
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
import { differenceInDays, format, parseISO } from 'date-fns'

const router = useRouter()
const sessionStore = useSessionStore()

const showCreateModal = ref(false)
const showEditModal = ref(false)
const isEditing = ref(false)
const editingSession = ref<Session | null>(null)

const formData = ref({
  name: '',
  displayName: '',
  description: '',
  startDate: '',
  endDate: '',
  status: 'Planned'
})

const sessions = computed(() => sessionStore.sessions)
const selectedSession = computed(() => sessionStore.selectedSession)
const loading = computed(() => sessionStore.loading)

const createSession = async () => {
  try {
    await sessionStore.createSession({
      ...formData.value,
      displayName: formData.value.displayName || `Academic Year ${formData.value.name}`,
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
  if (!editingSession.value) return

  try {
    await sessionStore.updateSession(editingSession.value.id, {
      name: formData.value.name,
      displayName: formData.value.displayName || `Academic Year ${formData.value.name}`,
      description: formData.value.description,
      startDate: formData.value.startDate,
      endDate: formData.value.endDate,
      status: formData.value.status
    })
    closeModals()
  } catch (error) {
    console.error('Failed to update session:', error)
  }
}

const activateSession = async (sessionId: number) => {
  try {
    await sessionStore.activateSession(sessionId)
  } catch (error) {
    console.error('Failed to activate session:', error)
  }
}

const archiveSession = async (sessionId: number) => {
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
  if (isActive) return 'bg-green-100 text-green-800 ring-1 ring-green-500/20'

  const classes: Record<string, string> = {
    Planned: 'bg-blue-100 text-blue-800 ring-1 ring-blue-500/20',
    Active: 'bg-green-100 text-green-800 ring-1 ring-green-500/20',
    Completed: 'bg-gray-100 text-gray-800 ring-1 ring-gray-300',
    Suspended: 'bg-yellow-100 text-yellow-800 ring-1 ring-yellow-500/20',
    Cancelled: 'bg-red-100 text-red-800 ring-1 ring-red-500/20'
  }

  return classes[status] || 'bg-gray-100 text-gray-800 ring-1 ring-gray-300'
}

onMounted(() => {
  sessionStore.loadSessions()
})
</script>
