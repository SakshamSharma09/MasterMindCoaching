import { apiService } from './apiService'
import { API_ENDPOINTS } from '@/config/api'

const USE_MOCK_API = import.meta.env.VITE_USE_MOCK_API === 'true' || false

export interface Student {
  id: number
  firstName: string
  lastName: string
  fullName: string
  admissionNumber?: string
  studentMobile?: string
  isActive: boolean
  classId?: number
  studentClasses?: StudentClass[]
}

export interface StudentClass {
  id: number
  studentId: number
  classId: number
  isActive: boolean
  class?: {
    id: number
    name: string
    board?: string
    medium?: string
  }
}

export interface CreateStudentRequest {
  firstName: string
  lastName: string
  email: string
  phone: string
  className?: string
  status: 'Active' | 'Inactive'
  motherName?: string
  fatherName?: string
  address?: string
  currentSchool?: string
  photo?: string
  dateOfBirth?: string
  gender?: 'Male' | 'Female' | 'Other'
  whatsappNumber?: string
  textNumber?: string
  aadharNumber?: string
  caste?: string
  rollNumber?: string
  standard?: string
}

export const studentsService = {
  async getStudents(page = 1, pageSize = 50, classId?: number): Promise<{ data: Student[], totalCount: number }> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting students')
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        data: [],
        totalCount: 0
      }
    }

    const params = new URLSearchParams()
    params.append('page', page.toString())
    params.append('pageSize', pageSize.toString())
    if (classId) params.append('classId', classId.toString())

    const response = await apiService.get(`${API_ENDPOINTS.STUDENTS.LIST}?${params.toString()}`)
    return {
        data: response.data,
        totalCount: response.totalCount
    }
  },

  async getAvailableStudentsForMapping(classId?: number): Promise<Student[]> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting available students for mapping')
      await new Promise(resolve => setTimeout(resolve, 500))
      return []
    }

    const params = new URLSearchParams()
    if (classId) params.append('classId', classId.toString())

    try {
      // Use apiService with bypass redirect option - let the interceptor handle the token
      const response = await apiService.getWithoutRedirect(`${API_ENDPOINTS.STUDENTS.LIST}/available-for-mapping?${params.toString()}`)
      console.log('API Response structure:', response)
      console.log('Response data:', response.data)
      return response.data || []
    } catch (error: any) {
      console.error('Error in getAvailableStudentsForMapping:', error)
      console.error('Error details:', {
        message: error?.message,
        response: error?.response,
        status: error?.response?.status,
        config: error?.config
      })
      
      // Handle authentication errors gracefully to prevent redirect
      if (error.message && (error.message.includes('401') || error.message.includes('Unauthorized'))) {
        throw new Error('Your session has expired. Please log in again to access student mapping.')
      }
      
      if (error.message && (error.message.includes('403') || error.message.includes('Forbidden'))) {
        throw new Error('Permission denied: You do not have access to student mapping functionality.')
      }
      
      // Handle CORS or network errors
      if (error.message && error.message.includes('Network Error')) {
        throw new Error('Network error: Unable to connect to the server. Please check if the API server is running.')
      }
      
      // Handle axios errors with response data
      if (error.response) {
        throw new Error(`Server error: ${error.response.status} - ${error.response.statusText}`)
      }
      
      // Handle other errors
      throw new Error(`Unexpected error: ${error.message || 'Unknown error'}`)
    }
  },

  async mapStudentToClass(studentId: number, classId: number): Promise<void> {
    if (USE_MOCK_API) {
      console.log('Mock API: Mapping student to class')
      await new Promise(resolve => setTimeout(resolve, 500))
      return
    }

    await apiService.post(`${API_ENDPOINTS.STUDENTS.LIST}/${studentId}/classes/${classId}`)
  },

  async unmapStudentFromClass(studentId: number, classId: number): Promise<void> {
    if (USE_MOCK_API) {
      console.log('Mock API: Unmapping student from class')
      await new Promise(resolve => setTimeout(resolve, 500))
      return
    }

    await apiService.delete(`${API_ENDPOINTS.STUDENTS.LIST}/${studentId}/classes/${classId}`)
  },

  async createStudent(studentData: CreateStudentRequest): Promise<Student> {
    if (USE_MOCK_API) {
      console.log('Mock API: Creating student')
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        id: Date.now(),
        firstName: studentData.firstName,
        lastName: studentData.lastName,
        fullName: `${studentData.firstName} ${studentData.lastName}`,
        admissionNumber: studentData.email,
        studentMobile: studentData.phone,
        isActive: studentData.status === 'Active'
      }
    }

    const response = await apiService.post(API_ENDPOINTS.STUDENTS.CREATE, studentData)
    return response.data
  },

  async updateStudent(id: number, studentData: Partial<CreateStudentRequest>): Promise<Student> {
    if (USE_MOCK_API) {
      console.log('Mock API: Updating student')
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        id,
        firstName: studentData.firstName || '',
        lastName: studentData.lastName || '',
        fullName: `${studentData.firstName || ''} ${studentData.lastName || ''}`,
        admissionNumber: studentData.email,
        studentMobile: studentData.phone,
        isActive: studentData.status === 'Active'
      }
    }

    const response = await apiService.put(API_ENDPOINTS.STUDENTS.UPDATE(id.toString()), studentData)
    return response.data
  },

  async deleteStudent(id: number): Promise<void> {
    if (USE_MOCK_API) {
      console.log('Mock API: Deleting student')
      await new Promise(resolve => setTimeout(resolve, 500))
      return
    }

    await apiService.delete(API_ENDPOINTS.STUDENTS.DELETE(id.toString()))
  }
}
