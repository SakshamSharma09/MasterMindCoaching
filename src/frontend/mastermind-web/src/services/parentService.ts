import { apiService } from './apiService'
import { API_ENDPOINTS } from '@/config/api'

export interface ParentChild {
  id: number
  firstName: string
  lastName: string
  admissionNumber?: string
  studentMobile?: string
  studentEmail?: string
  className: string
  classId?: number
  isActive: boolean
}

export interface ParentDashboardStats {
  totalChildren: number
  activeChildren: number
  averageAttendance: number
  totalPendingFees: number
  totalRemarks: number
}

export interface ChildAttendance {
  totalClasses: number
  present: number
  absent: number
  late: number
  percentage: number
  records: Array<{
    date: string
    subject: string
    status: string
  }>
}

export interface ChildFees {
  totalFees: number
  paidFees: number
  pendingFees: number
  nextDueDate: string
  paymentHistory: Array<{
    date: string
    amount: number
    status: string
    method: string
  }>
}

export interface ChildPerformance {
  averageGrade: string
  overallPercentage: number
  subjectPerformance: Array<{
    subject: string
    grade: string
    percentage: number
  }>
  recentTests: Array<{
    date: string
    subject: string
    topic: string
    score: number
    totalMarks: number
  }>
}

class ParentService {
  async getChildren(): Promise<ParentChild[]> {
    const response = await apiService.get(API_ENDPOINTS.PARENT.CHILDREN)
    return response.data
  }

  async getDashboardStats(): Promise<ParentDashboardStats> {
    const response = await apiService.get(API_ENDPOINTS.PARENT.DASHBOARD_STATS)
    return response.data
  }

  async getChildAttendance(childId: number): Promise<ChildAttendance> {
    const response = await apiService.get(`${API_ENDPOINTS.PARENT.BASE}/children/${childId}/attendance`)
    return response.data
  }

  async getChildFees(childId: number): Promise<ChildFees> {
    const response = await apiService.get(`${API_ENDPOINTS.PARENT.BASE}/children/${childId}/fees`)
    return response.data
  }

  async getChildPerformance(childId: number): Promise<ChildPerformance> {
    const response = await apiService.get(`${API_ENDPOINTS.PARENT.BASE}/children/${childId}/performance`)
    return response.data
  }
}

export const parentService = new ParentService()
