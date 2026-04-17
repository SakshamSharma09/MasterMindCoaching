import { apiService } from './apiService'

const USE_MOCK_API = import.meta.env.VITE_USE_MOCK_API === 'true' || false

export interface Exam {
  id: number
  name: string
  description?: string
  classId: number
  className: string
  subjectId?: number
  subjectName?: string
  type: string
  examDate: string
  startTime?: string
  endTime?: string
  durationMinutes: number
  maxMarks: number
  passingMarks: number
  status: string
  academicYear: string
  resultCount: number
}

export interface ExamDetail extends Exam {
  syllabus?: string
  instructions?: string
  results: ExamResult[]
}

export interface ExamResult {
  id: number
  examId: number
  studentId: number
  studentName: string
  marksObtained?: number
  grade?: string
  percentage?: number
  isPassed: boolean
  rank?: number
  status: string
  remarks?: string
  teacherComments?: string
  evaluatedBy?: string
  evaluatedAt?: string
}

export interface CreateExamDto {
  name: string
  description?: string
  classId: number
  subjectId?: number
  type?: string
  examDate: string
  startTime?: string
  endTime?: string
  durationMinutes?: number
  maxMarks?: number
  passingMarks?: number
  syllabus?: string
  instructions?: string
  academicYear?: string
  sessionId?: number
}

export interface UpdateExamDto {
  name?: string
  description?: string
  classId?: number
  subjectId?: number
  type?: string
  examDate?: string
  startTime?: string
  endTime?: string
  durationMinutes?: number
  maxMarks?: number
  passingMarks?: number
  syllabus?: string
  instructions?: string
  status?: string
}

export interface AddResultDto {
  studentId: number
  marksObtained: number
  remarks?: string
  teacherComments?: string
}

export interface BulkResultItem {
  studentId: number
  marksObtained: number
}

export interface ExamStatistics {
  examId: number
  examName: string
  totalStudents: number
  passed: number
  failed: number
  absent: number
  highestMarks: number
  lowestMarks: number
  averageMarks: number
  passPercentage: number
  maxMarks: number
  passingMarks: number
}

export interface ExamFilters {
  classId?: number
  subjectId?: number
  status?: string
  sessionId?: number
  page?: number
  pageSize?: number
}

export const examsService = {
  async getExams(filters?: ExamFilters): Promise<Exam[]> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting exams with filters:', filters)
      await new Promise(resolve => setTimeout(resolve, 500))
      return []
    }

    const params = new URLSearchParams()
    if (filters?.classId) params.append('classId', filters.classId.toString())
    if (filters?.subjectId) params.append('subjectId', filters.subjectId.toString())
    if (filters?.status) params.append('status', filters.status)
    if (filters?.sessionId) params.append('sessionId', filters.sessionId.toString())
    if (filters?.page) params.append('page', filters.page.toString())
    if (filters?.pageSize) params.append('pageSize', filters.pageSize.toString())

    const response = await apiService.get(`/exams?${params.toString()}`)
    return response.data
  },

  async getExam(id: number): Promise<ExamDetail> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting exam:', id)
      await new Promise(resolve => setTimeout(resolve, 500))
      throw new Error('Exam not found')
    }

    const response = await apiService.get(`/exams/${id}`)
    return response.data
  },

  async createExam(data: CreateExamDto): Promise<Exam> {
    if (USE_MOCK_API) {
      console.log('Mock API: Creating exam:', data)
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        id: Date.now(),
        ...data,
        className: 'Mock Class',
        type: data.type || 'Written',
        durationMinutes: data.durationMinutes || 180,
        maxMarks: data.maxMarks || 100,
        passingMarks: data.passingMarks || 33,
        status: 'Scheduled',
        academicYear: data.academicYear || '2025-26',
        resultCount: 0
      } as Exam
    }

    const response = await apiService.post('/exams', data)
    return response.data
  },

  async updateExam(id: number, data: UpdateExamDto): Promise<Exam> {
    if (USE_MOCK_API) {
      console.log('Mock API: Updating exam:', id, data)
      await new Promise(resolve => setTimeout(resolve, 500))
      throw new Error('Exam not found')
    }

    const response = await apiService.put(`/exams/${id}`, data)
    return response.data
  },

  async deleteExam(id: number): Promise<void> {
    if (USE_MOCK_API) {
      console.log('Mock API: Deleting exam:', id)
      await new Promise(resolve => setTimeout(resolve, 500))
      return
    }

    await apiService.delete(`/exams/${id}`)
  },

  async getExamResults(examId: number): Promise<ExamResult[]> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting exam results:', examId)
      await new Promise(resolve => setTimeout(resolve, 500))
      return []
    }

    const response = await apiService.get(`/exams/${examId}/results`)
    return response.data
  },

  async addResult(examId: number, data: AddResultDto): Promise<ExamResult> {
    if (USE_MOCK_API) {
      console.log('Mock API: Adding result:', examId, data)
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        id: Date.now(),
        examId,
        studentId: data.studentId,
        studentName: 'Mock Student',
        marksObtained: data.marksObtained,
        grade: 'A',
        percentage: 85,
        isPassed: true,
        status: 'Evaluated'
      }
    }

    const response = await apiService.post(`/exams/${examId}/results`, data)
    return response.data
  },

  async bulkAddResults(examId: number, results: BulkResultItem[]): Promise<{ successCount: number; failedCount: number; errors: string[] }> {
    if (USE_MOCK_API) {
      console.log('Mock API: Bulk adding results:', examId, results)
      await new Promise(resolve => setTimeout(resolve, 500))
      return { successCount: results.length, failedCount: 0, errors: [] }
    }

    const response = await apiService.post(`/exams/${examId}/results/bulk`, { results })
    return response.data
  },

  async publishResults(examId: number): Promise<void> {
    if (USE_MOCK_API) {
      console.log('Mock API: Publishing results:', examId)
      await new Promise(resolve => setTimeout(resolve, 500))
      return
    }

    await apiService.post(`/exams/${examId}/publish`, {})
  },

  async getExamStatistics(examId: number): Promise<ExamStatistics> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting exam statistics:', examId)
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        examId,
        examName: 'Mock Exam',
        totalStudents: 0,
        passed: 0,
        failed: 0,
        absent: 0,
        highestMarks: 0,
        lowestMarks: 0,
        averageMarks: 0,
        passPercentage: 0,
        maxMarks: 100,
        passingMarks: 33
      }
    }

    const response = await apiService.get(`/exams/${examId}/statistics`)
    return response.data
  }
}

export default examsService
