import { apiService } from './apiService'

const USE_MOCK_API = import.meta.env.VITE_USE_MOCK_API === 'true' || false

export interface Session {
  id: number
  name: string
  displayName: string
  description?: string
  startDate: string
  endDate: string
  academicYear: string
  isActive: boolean
  status: string
  totalStudents: number
  activeStudents: number
  totalClasses: number
  activeClasses: number
  totalTeachers: number
  totalRevenue: number
  totalExpenses: number
}

export interface CreateSessionDto {
  name: string
  displayName?: string
  description?: string
  startDate: string
  endDate: string
  academicYear: string
}

export interface UpdateSessionDto {
  name?: string
  displayName?: string
  description?: string
  startDate?: string
  endDate?: string
  isActive?: boolean
  status?: string
}

export const sessionsService = {
  async getSessions(): Promise<Session[]> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting sessions')
      await new Promise(resolve => setTimeout(resolve, 500))
      return []
    }

    const response = await apiService.get('/sessions')
    return response.data
  },

  async getSession(id: number): Promise<Session> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting session:', id)
      await new Promise(resolve => setTimeout(resolve, 500))
      throw new Error('Session not found')
    }

    const response = await apiService.get(`/sessions/${id}`)
    return response.data
  },

  async getActiveSession(): Promise<Session | null> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting active session')
      await new Promise(resolve => setTimeout(resolve, 500))
      return null
    }

    const response = await apiService.get('/sessions/active')
    return response.data
  },

  async createSession(data: CreateSessionDto): Promise<Session> {
    if (USE_MOCK_API) {
      console.log('Mock API: Creating session:', data)
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        id: Date.now(),
        ...data,
        displayName: data.displayName || data.name,
        isActive: false,
        status: 'Planned',
        totalStudents: 0,
        activeStudents: 0,
        totalClasses: 0,
        activeClasses: 0,
        totalTeachers: 0,
        totalRevenue: 0,
        totalExpenses: 0
      }
    }

    const response = await apiService.post('/sessions', data)
    return response.data
  },

  async updateSession(id: number, data: UpdateSessionDto): Promise<Session> {
    if (USE_MOCK_API) {
      console.log('Mock API: Updating session:', id, data)
      await new Promise(resolve => setTimeout(resolve, 500))
      throw new Error('Session not found')
    }

    const response = await apiService.put(`/sessions/${id}`, data)
    return response.data
  },

  async deleteSession(id: number): Promise<void> {
    if (USE_MOCK_API) {
      console.log('Mock API: Deleting session:', id)
      await new Promise(resolve => setTimeout(resolve, 500))
      return
    }

    await apiService.delete(`/sessions/${id}`)
  },

  async activateSession(id: number): Promise<Session> {
    if (USE_MOCK_API) {
      console.log('Mock API: Activating session:', id)
      await new Promise(resolve => setTimeout(resolve, 500))
      throw new Error('Session not found')
    }

    const response = await apiService.post(`/sessions/${id}/activate`, {})
    return response.data
  },

  async deactivateSession(id: number): Promise<Session> {
    if (USE_MOCK_API) {
      console.log('Mock API: Deactivating session:', id)
      await new Promise(resolve => setTimeout(resolve, 500))
      throw new Error('Session not found')
    }

    const response = await apiService.post(`/sessions/${id}/deactivate`, {})
    return response.data
  },

  async getSessionStatistics(id: number): Promise<{
    totalStudents: number
    activeStudents: number
    totalClasses: number
    totalTeachers: number
    totalRevenue: number
    totalExpenses: number
    attendanceRate: number
    feeCollectionRate: number
  }> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting session statistics:', id)
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        totalStudents: 0,
        activeStudents: 0,
        totalClasses: 0,
        totalTeachers: 0,
        totalRevenue: 0,
        totalExpenses: 0,
        attendanceRate: 0,
        feeCollectionRate: 0
      }
    }

    const response = await apiService.get(`/sessions/${id}/statistics`)
    return response.data
  }
}

export default sessionsService
