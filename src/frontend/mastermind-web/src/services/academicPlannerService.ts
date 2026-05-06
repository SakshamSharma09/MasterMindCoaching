import { apiService } from './apiService'

export interface AcademicPlannerEntry {
  id: number
  sessionId?: number | null
  studentId?: number | null
  studentName?: string | null
  classId?: number | null
  className?: string | null
  schoolName: string
  examType: 'UnitTest' | 'HalfYearly' | 'Yearly' | 'Custom'
  subject: string
  syllabus: string
  examDate: string
  startTime?: string | null
  endTime?: string | null
  notes?: string | null
}

export interface CreateAcademicPlannerEntryRequest {
  sessionId?: number
  studentId?: number | null
  classId?: number | null
  schoolName?: string
  examType?: string
  subject: string
  syllabus: string
  examDate: string
  startTime?: string | null
  endTime?: string | null
  notes?: string | null
}

export interface UpdateAcademicPlannerEntryRequest {
  studentId?: number | null
  classId?: number | null
  schoolName?: string
  examType?: string
  subject?: string
  syllabus?: string
  examDate?: string
  startTime?: string | null
  endTime?: string | null
  notes?: string | null
}

export const academicPlannerService = {
  async getEntries(filters?: {
    sessionId?: number
    studentId?: number
    schoolName?: string
    examType?: string
  }): Promise<AcademicPlannerEntry[]> {
    const params = new URLSearchParams()
    if (filters?.sessionId) params.append('sessionId', filters.sessionId.toString())
    if (filters?.studentId) params.append('studentId', filters.studentId.toString())
    if (filters?.schoolName) params.append('schoolName', filters.schoolName)
    if (filters?.examType) params.append('examType', filters.examType)

    const query = params.toString()
    const endpoint = query ? `/academicplanner?${query}` : '/academicplanner'
    const response = await apiService.get(endpoint)
    return response.data || []
  },

  async createEntry(payload: CreateAcademicPlannerEntryRequest): Promise<AcademicPlannerEntry> {
    const response = await apiService.post('/academicplanner', payload)
    return response.data
  },

  async updateEntry(id: number, payload: UpdateAcademicPlannerEntryRequest): Promise<AcademicPlannerEntry> {
    const response = await apiService.put(`/academicplanner/${id}`, payload)
    return response.data
  },

  async deleteEntry(id: number): Promise<void> {
    await apiService.delete(`/academicplanner/${id}`)
  }
}

