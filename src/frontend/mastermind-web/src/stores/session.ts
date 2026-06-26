import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { apiService } from '@/services/apiService'
import { API_ENDPOINTS } from '@/config/api'

export interface Session {
  id: number
  name: string
  displayName: string
  description?: string
  academicYear: string
  startDate: string
  endDate: string
  status: string
  isActive: boolean
  totalStudents: number
  activeStudents: number
  totalClasses: number
  activeClasses: number
  totalTeachers: number
  totalRevenue: number
  totalExpenses: number
  settings?: string
  createdAt: string
  updatedAt: string
}

const normalizeStatus = (status: unknown) => {
  if (typeof status === 'number') {
    return ['Planned', 'Active', 'Completed', 'Suspended', 'Cancelled'][status - 1] || 'Planned'
  }

  const value = String(status || 'Planned').toLowerCase()
  const statuses: Record<string, string> = {
    planned: 'Planned',
    active: 'Active',
    completed: 'Completed',
    suspended: 'Suspended',
    cancelled: 'Cancelled'
  }

  return statuses[value] || 'Planned'
}

const normalizeSession = (session: any): Session => ({
  id: Number(session?.id || 0),
  name: session?.name || '',
  displayName: session?.displayName || session?.name || '',
  description: session?.description || '',
  academicYear: session?.academicYear || session?.name || '',
  startDate: session?.startDate || '',
  endDate: session?.endDate || '',
  status: normalizeStatus(session?.status),
  isActive: Boolean(session?.isActive),
  totalStudents: Number(session?.totalStudents || 0),
  activeStudents: Number(session?.activeStudents || 0),
  totalClasses: Number(session?.totalClasses || 0),
  activeClasses: Number(session?.activeClasses || 0),
  totalTeachers: Number(session?.totalTeachers || 0),
  totalRevenue: Number(session?.totalRevenue || 0),
  totalExpenses: Number(session?.totalExpenses || 0),
  settings: session?.settings,
  createdAt: session?.createdAt || '',
  updatedAt: session?.updatedAt || ''
})

export const useSessionStore = defineStore('session', () => {
  // State
  const sessions = ref<Session[]>([])
  const selectedSessionId = ref<number | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const selectedSession = computed(() => {
    if (!selectedSessionId.value) return null
    return sessions.value.find(s => s.id === selectedSessionId.value) || null
  })

  const activeSession = computed(() => {
    return sessions.value.find(s => s.isActive) || null
  })

  const hasMultipleSessions = computed(() => {
    return sessions.value.length > 1
  })

  // Actions
  const loadSessions = async () => {
    loading.value = true
    error.value = null
    
    try {
      const response = await apiService.get(API_ENDPOINTS.SESSIONS.LIST)
      // Handle ApiResponse wrapper - response is the ApiResponse object directly
      sessions.value = (response.data || []).map(normalizeSession)
      
      const selectedSessionExists = selectedSessionId.value
        ? sessions.value.some(s => s.id === selectedSessionId.value)
        : false

      // If no valid session is selected, try to select the active one
      if ((!selectedSessionId.value || !selectedSessionExists) && sessions.value.length > 0) {
        const active = sessions.value.find(s => s.isActive)
        if (active) {
          selectedSessionId.value = active.id
        } else {
          // If no active session, select the first one
          selectedSessionId.value = sessions.value[0].id
        }
      }
      
      // Save to localStorage
      if (selectedSessionId.value) {
        localStorage.setItem('selectedSessionId', selectedSessionId.value.toString())
      }
    } catch (err) {
      console.error('Failed to load sessions:', err)
      error.value = 'Failed to load sessions'
    } finally {
      loading.value = false
    }
  }

  const selectSession = (sessionId: number) => {
    selectedSessionId.value = sessionId
    localStorage.setItem('selectedSessionId', sessionId.toString())
  }

  const createSession = async (sessionData: Partial<Session>) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await apiService.post(API_ENDPOINTS.SESSIONS.CREATE, sessionData)
      const newSession = normalizeSession(response.data)
      
      sessions.value.push(newSession)
      
      // Auto-select the new session
      selectedSessionId.value = newSession.id
      localStorage.setItem('selectedSessionId', newSession.id.toString())
      
      return newSession
    } catch (err) {
      console.error('Failed to create session:', err)
      error.value = 'Failed to create session'
      throw err
    } finally {
      loading.value = false
    }
  }

  const updateSession = async (sessionId: number, sessionData: Partial<Session>) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await apiService.put(`${API_ENDPOINTS.SESSIONS.LIST}/${sessionId}`, sessionData)
      const updatedSession = normalizeSession(response.data)
      
      // Update local state
      const index = sessions.value.findIndex(s => s.id === sessionId)
      if (index !== -1) {
        sessions.value[index] = { ...sessions.value[index], ...updatedSession }
      }
      
      return updatedSession
    } catch (err) {
      console.error('Failed to update session:', err)
      error.value = 'Failed to update session'
      throw err
    } finally {
      loading.value = false
    }
  }

  const activateSession = async (sessionId: number) => {
    loading.value = true
    error.value = null
    
    try {
      await apiService.put(`${API_ENDPOINTS.SESSIONS.LIST}/${sessionId}/activate`)
      
      // Update local state
      sessions.value.forEach(s => {
        s.isActive = s.id === sessionId
        if (s.id === sessionId) {
          s.status = 'Active'
        }
      })
      
      // Select the activated session
      selectSession(sessionId)
    } catch (err) {
      console.error('Failed to activate session:', err)
      error.value = 'Failed to activate session'
      throw err
    } finally {
      loading.value = false
    }
  }

  const initializeFromStorage = () => {
    const stored = localStorage.getItem('selectedSessionId')
    if (stored) {
      selectedSessionId.value = parseInt(stored, 10)
    }
  }

  // Initialize on store creation
  initializeFromStorage()

  return {
    // State
    sessions,
    selectedSessionId,
    loading,
    error,
    
    // Getters
    selectedSession,
    activeSession,
    hasMultipleSessions,
    
    // Actions
    loadSessions,
    selectSession,
    createSession,
    updateSession,
    activateSession,
    initializeFromStorage
  }
})
