import { apiService } from './apiService'

const USE_MOCK_API = import.meta.env.VITE_USE_MOCK_API === 'true' || false

export interface Teacher {
  id: number
  firstName: string
  lastName: string
  email: string
  mobile: string
  specialization?: string
  qualification?: string
  subjects?: string
  joiningDate?: string
  salary?: number
  isActive: boolean
  classes?: TeacherClass[]
}

export interface TeacherClass {
  classId: number
  className: string
  subjectId?: number
  subjectName?: string
}

export interface CreateTeacherDto {
  firstName: string
  lastName: string
  email: string
  mobile: string
  specialization?: string
  qualification?: string
  subjects?: string
  joiningDate?: string
  salary?: number
}

export interface UpdateTeacherDto {
  firstName?: string
  lastName?: string
  email?: string
  mobile?: string
  specialization?: string
  qualification?: string
  subjects?: string
  salary?: number
  isActive?: boolean
}

export interface TeacherSalary {
  id: number
  teacherId: number
  teacherName: string
  month: string
  basicSalary: number
  allowances: number
  deductions: number
  netSalary: number
  status: string
  paidDate?: string
}

export interface TeacherAttendance {
  id: number
  teacherId: number
  teacherName: string
  date: string
  status: string
  checkInTime?: string
  checkOutTime?: string
  remarks?: string
}

export const teachersService = {
  async getTeachers(page: number = 1, pageSize: number = 50): Promise<{ data: Teacher[]; totalCount: number }> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting teachers')
      await new Promise(resolve => setTimeout(resolve, 500))
      return { data: [], totalCount: 0 }
    }

    const response = await apiService.get(`/teachers?page=${page}&pageSize=${pageSize}`)
    return {
      data: response.data || [],
      totalCount: response.totalCount || response.data?.length || 0
    }
  },

  async getTeacher(id: number): Promise<Teacher> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting teacher:', id)
      await new Promise(resolve => setTimeout(resolve, 500))
      throw new Error('Teacher not found')
    }

    const response = await apiService.get(`/teachers/${id}`)
    return response.data
  },

  async createTeacher(data: CreateTeacherDto): Promise<Teacher> {
    if (USE_MOCK_API) {
      console.log('Mock API: Creating teacher:', data)
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        id: Date.now(),
        ...data,
        isActive: true
      } as Teacher
    }

    const response = await apiService.post('/teachers', data)
    return response.data
  },

  async updateTeacher(id: number, data: UpdateTeacherDto): Promise<Teacher> {
    if (USE_MOCK_API) {
      console.log('Mock API: Updating teacher:', id, data)
      await new Promise(resolve => setTimeout(resolve, 500))
      throw new Error('Teacher not found')
    }

    const response = await apiService.put(`/teachers/${id}`, data)
    return response.data
  },

  async deleteTeacher(id: number): Promise<void> {
    if (USE_MOCK_API) {
      console.log('Mock API: Deleting teacher:', id)
      await new Promise(resolve => setTimeout(resolve, 500))
      return
    }

    await apiService.delete(`/teachers/${id}`)
  },

  async assignToClass(teacherId: number, classId: number, subjectId?: number): Promise<void> {
    if (USE_MOCK_API) {
      console.log('Mock API: Assigning teacher to class:', teacherId, classId)
      await new Promise(resolve => setTimeout(resolve, 500))
      return
    }

    await apiService.post(`/teachers/${teacherId}/classes/${classId}`, { subjectId })
  },

  async removeFromClass(teacherId: number, classId: number): Promise<void> {
    if (USE_MOCK_API) {
      console.log('Mock API: Removing teacher from class:', teacherId, classId)
      await new Promise(resolve => setTimeout(resolve, 500))
      return
    }

    await apiService.delete(`/teachers/${teacherId}/classes/${classId}`)
  },

  async getTeacherSalaries(teacherId?: number, month?: string): Promise<TeacherSalary[]> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting teacher salaries')
      await new Promise(resolve => setTimeout(resolve, 500))
      return []
    }

    const params = new URLSearchParams()
    if (teacherId) params.append('teacherId', teacherId.toString())
    if (month) params.append('month', month)

    const response = await apiService.get(`/teachers/salaries?${params.toString()}`)
    return response.data
  },

  async processSalary(teacherId: number, month: string, data: { basicSalary: number; allowances?: number; deductions?: number }): Promise<TeacherSalary> {
    if (USE_MOCK_API) {
      console.log('Mock API: Processing salary:', teacherId, month, data)
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        id: Date.now(),
        teacherId,
        teacherName: 'Mock Teacher',
        month,
        basicSalary: data.basicSalary,
        allowances: data.allowances || 0,
        deductions: data.deductions || 0,
        netSalary: data.basicSalary + (data.allowances || 0) - (data.deductions || 0),
        status: 'Pending'
      }
    }

    const response = await apiService.post(`/teachers/${teacherId}/salaries`, { month, ...data })
    return response.data
  },

  async getTeacherAttendance(teacherId?: number, date?: string): Promise<TeacherAttendance[]> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting teacher attendance')
      await new Promise(resolve => setTimeout(resolve, 500))
      return []
    }

    const params = new URLSearchParams()
    if (teacherId) params.append('teacherId', teacherId.toString())
    if (date) params.append('date', date)

    const response = await apiService.get(`/teachers/attendance?${params.toString()}`)
    return response.data
  },

  async markTeacherAttendance(teacherId: number, data: { date: string; status: string; checkInTime?: string; checkOutTime?: string; remarks?: string }): Promise<TeacherAttendance> {
    if (USE_MOCK_API) {
      console.log('Mock API: Marking teacher attendance:', teacherId, data)
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        id: Date.now(),
        teacherId,
        teacherName: 'Mock Teacher',
        ...data
      }
    }

    const response = await apiService.post(`/teachers/${teacherId}/attendance`, data)
    return response.data
  }
}

export default teachersService
