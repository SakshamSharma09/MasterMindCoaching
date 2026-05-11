import { apiService } from './apiService'
import { classesService, type Class } from './classesService'
import { studentsService, type Student } from './studentsService'
import { teachersService, type Teacher } from './teachersService'

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

const normalizeName = (firstName?: string, lastName?: string) =>
  `${firstName || ''} ${lastName || ''}`.trim()

const normalizeInitials = (firstName?: string, lastName?: string) =>
  `${(firstName || '').charAt(0)}${(lastName || '').charAt(0)}`.toUpperCase() || 'NA'

class TeacherPortalService {
  async getTeacherByEmail(email: string): Promise<Teacher | null> {
    const response = await teachersService.getTeachers(1, 500)
    const match = response.data.find((teacher: any) =>
      (teacher.email || '').trim().toLowerCase() === email.trim().toLowerCase()
    )
    return match || null
  }

  async getMyClasses(email: string): Promise<TeacherClassContext[]> {
    const teacher = await this.getTeacherByEmail(email)
    if (!teacher) return []

    const rawTeacherClasses = ((teacher as any).teacherClasses || []) as any[]
    const classIds = rawTeacherClasses
      .filter(tc => tc.isActive !== false)
      .map(tc => tc.classId)
      .filter((id: any) => typeof id === 'number')

    if (classIds.length === 0) return []

    const allClasses = await classesService.getClasses()
    const classMap = new Map<number, Class>(allClasses.map(c => [c.id, c]))

    return [...new Set(classIds)]
      .map(classId => classMap.get(classId))
      .filter((c): c is Class => !!c)
      .map(c => ({
        id: c.id,
        name: c.name,
        board: c.board,
        medium: c.medium
      }))
  }

  async getClassStudents(classId: number): Promise<TeacherStudentItem[]> {
    const studentsResponse = await studentsService.getStudents(1, 200, classId)
    return (studentsResponse.data || []).map((student: Student) => ({
      id: student.id,
      name: normalizeName(student.firstName, student.lastName),
      initials: normalizeInitials(student.firstName, student.lastName),
      rollNo: student.admissionNumber || `STD-${student.id}`,
      classId,
      attendance: '--',
      averageGrade: 'N/A'
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
