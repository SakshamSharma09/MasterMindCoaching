import { apiService } from './apiService'

export interface TeacherClassContext {
  id: number
  name: string
  board: string
  medium: string
}

export interface TeacherStudentItem {
  id: number
  name: string
  initials: string
  rollNo: string
  classId?: number
  attendance: string
  averageGrade: string
}

export interface StudentRemarkItem {
  id: number
  studentId: number
  studentName: string
  classId?: number
  className?: string
  subject?: string
  chapterName?: string
  topicName?: string
  remarks: string
  type: string
  rating?: number
  date: string
  isVisibleToParent: boolean
}

export interface CreateStudentRemarkPayload {
  studentId: number
  classId?: number
  subject?: string
  chapterName?: string
  topicName?: string
  remarks: string
  type?: number
  rating?: number
  isVisibleToParent?: boolean
}

class TeacherPortalService {
  async getMyClasses(): Promise<TeacherClassContext[]> {
    const response = await apiService.get('/teacher-portal/classes')
    const records = response.data || []
    return records.map((item: any) => ({
      id: item.id,
      name: item.name,
      board: item.board,
      medium: item.medium
    }))
  }

  async getClassStudents(classId: number): Promise<TeacherStudentItem[]> {
    const response = await apiService.get(`/teacher-portal/classes/${classId}/students`)
    const records = response.data || []
    return records.map((student: any) => ({
      id: student.id,
      name: student.name,
      initials: student.initials,
      rollNo: student.rollNo,
      classId: student.classId,
      attendance: student.attendance || '--',
      averageGrade: student.averageGrade || 'N/A'
    }))
  }

  async getRemarks(classId?: number): Promise<StudentRemarkItem[]> {
    const query = classId ? `?classId=${classId}` : ''
    const response = await apiService.get(`/student-remarks${query}`)
    const records = response.data || []
    return records.map((item: any) => ({
      id: item.id,
      studentId: item.studentId,
      studentName: item.studentName,
      classId: item.classId,
      className: item.className,
      subject: item.subject,
      chapterName: item.chapterName,
      topicName: item.topicName,
      remarks: item.remarks,
      type: item.type,
      rating: item.rating,
      date: item.date,
      isVisibleToParent: item.isVisibleToParent
    }))
  }

  async createRemark(payload: CreateStudentRemarkPayload): Promise<void> {
    await apiService.post('/student-remarks', {
      ...payload,
      type: payload.type ?? 0,
      isVisibleToParent: payload.isVisibleToParent ?? true
    })
  }
}

export const teacherPortalService = new TeacherPortalService()
