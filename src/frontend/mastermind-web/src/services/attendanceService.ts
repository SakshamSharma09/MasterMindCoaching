import { apiService } from './apiService'
import { API_ENDPOINTS } from '@/config/api'

const USE_MOCK_API = import.meta.env.VITE_USE_MOCK_API === 'true' || false

export interface AttendanceRecord {
  id: number
  studentId: number
  studentName: string
  classId: number
  className: string
  status: 'present' | 'absent' | 'late' | 'halfday' | 'holiday' | 'leave'
  checkInTime: string
  checkOutTime?: string
  date: string
  remarks?: string
}

export interface CreateAttendanceDto {
  studentId: number
  classId: number
  date: string
  status: number // Enum value: 0=Present, 1=Absent, 2=Late, etc.
  checkInTime?: string
  checkOutTime?: string
  remarks?: string
}

export interface UpdateAttendanceDto {
  status: number
  checkInTime?: string
  checkOutTime?: string
  remarks?: string
}

export const ATTENDANCE_STATUS_MAP: Record<string, number> = {
  'present': 0,
  'absent': 1,
  'late': 2,
  'halfday': 3,
  'holiday': 4,
  'leave': 5
}

export const ATTENDANCE_STATUS_REVERSE_MAP: Record<number, string> = {
  0: 'present',
  1: 'absent',
  2: 'late',
  3: 'halfday',
  4: 'holiday',
  5: 'leave'
}

export const attendanceService = {
  // Get all attendance records
  async getAttendance(date?: string, classId?: number, studentId?: number): Promise<AttendanceRecord[]> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting attendance')
      await new Promise(resolve => setTimeout(resolve, 500))
      return [] // Return empty for mock or implement mock logic if needed
    }

    const params = new URLSearchParams()
    if (date) params.append('date', date)
    if (classId) params.append('classId', classId.toString())
    if (studentId) params.append('studentId', studentId.toString())

    const response = await apiService.get(`/attendance?${params.toString()}`)
    // Map backend status to frontend string
    const mappedData = response.data.map((record: any) => {
      let status = record.status
      
      // If status is already a string, use it directly
      if (typeof status === 'string') {
        // Normalize status to lowercase
        status = status.toLowerCase()
        // Validate it's a known status
        const validStatuses = ['present', 'absent', 'late', 'halfday', 'holiday', 'leave']
        status = validStatuses.includes(status) ? status : 'present'
      } 
      // If status is a number, map it using the reverse map
      else if (typeof status === 'number') {
        status = ATTENDANCE_STATUS_REVERSE_MAP[status] || 'present'
      }
      // Default fallback
      else {
        status = 'present'
      }
      
      return {
        ...record,
        status: status
      }
    })
    return mappedData
  },

  // Mark attendance for a student
  async markAttendance(data: CreateAttendanceDto): Promise<AttendanceRecord> {
    if (USE_MOCK_API) {
      console.log('Mock API: Marking attendance', data)
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
          id: Date.now(),
          studentId: data.studentId,
          studentName: 'Mock Student',
          classId: data.classId,
          className: 'Mock Class',
          status: 'present',
          checkInTime: data.checkInTime || '',
          checkOutTime: data.checkOutTime,
          date: data.date,
          remarks: data.remarks
      }
    }

    const response = await apiService.post('/attendance', data)
    return response.data
  },

  // Update attendance
  async updateAttendance(id: number, data: UpdateAttendanceDto): Promise<AttendanceRecord> {
    if (USE_MOCK_API) {
      console.log('Mock API: Updating attendance', id, data)
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
          id: id,
          studentId: 1,
          studentName: 'Mock Student',
          classId: 1,
          className: 'Mock Class',
          status: 'present',
          checkInTime: data.checkInTime || '',
          checkOutTime: data.checkOutTime,
          date: new Date().toISOString().split('T')[0],
          remarks: data.remarks
      }
    }

    const response = await apiService.put(`/attendance/${id}`, data)
    return response.data
  },

  // Delete attendance
  async deleteAttendance(id: number): Promise<boolean> {
    if (USE_MOCK_API) {
      console.log('Mock API: Deleting attendance', id)
      await new Promise(resolve => setTimeout(resolve, 500))
      return true
    }

    const response = await apiService.delete(`/attendance/${id}`)
    return response.data
  }
}
