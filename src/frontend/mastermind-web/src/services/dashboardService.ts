import { apiService } from './apiService'

const USE_MOCK_API = import.meta.env.VITE_USE_MOCK_API === 'true' || false

export interface DashboardStats {
  totalStudents: number
  activeStudents: number
  totalTeachers: number
  totalClasses: number
  totalRevenue: number
  pendingFees: number
  todayAttendance: number
  attendancePercentage: number
  newLeads: number
  upcomingExams: number
}

export interface RecentActivity {
  id: number
  type: 'admission' | 'payment' | 'attendance' | 'lead' | 'exam'
  title: string
  description: string
  timestamp: string
  icon?: string
}

export interface AttendanceTrend {
  date: string
  present: number
  absent: number
  total: number
  percentage: number
}

export interface RevenueTrend {
  month: string
  revenue: number
  expenses: number
  profit: number
}

export interface ClassWiseStats {
  classId: number
  className: string
  totalStudents: number
  presentToday: number
  pendingFees: number
  averageAttendance: number
}

export const dashboardService = {
  async getStats(): Promise<DashboardStats> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting dashboard stats')
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        totalStudents: 150,
        activeStudents: 145,
        totalTeachers: 12,
        totalClasses: 10,
        totalRevenue: 500000,
        pendingFees: 75000,
        todayAttendance: 138,
        attendancePercentage: 95.2,
        newLeads: 8,
        upcomingExams: 3
      }
    }

    const response = await apiService.get('/dashboard/stats')
    return response.data
  },

  async getRecentActivities(limit: number = 10): Promise<RecentActivity[]> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting recent activities')
      await new Promise(resolve => setTimeout(resolve, 500))
      return []
    }

    const response = await apiService.get(`/dashboard/activities?limit=${limit}`)
    return response.data
  },

  async getAttendanceTrend(days: number = 7): Promise<AttendanceTrend[]> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting attendance trend')
      await new Promise(resolve => setTimeout(resolve, 500))
      return []
    }

    const response = await apiService.get(`/dashboard/attendance-trend?days=${days}`)
    return response.data
  },

  async getRevenueTrend(months: number = 6): Promise<RevenueTrend[]> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting revenue trend')
      await new Promise(resolve => setTimeout(resolve, 500))
      return []
    }

    const response = await apiService.get(`/dashboard/revenue-trend?months=${months}`)
    return response.data
  },

  async getClassWiseStats(): Promise<ClassWiseStats[]> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting class-wise stats')
      await new Promise(resolve => setTimeout(resolve, 500))
      return []
    }

    const response = await apiService.get('/dashboard/class-stats')
    return response.data
  },

  async getUpcomingEvents(): Promise<{
    exams: Array<{ id: number; name: string; date: string; className: string }>
    holidays: Array<{ date: string; name: string }>
    feeDeadlines: Array<{ date: string; className: string; count: number }>
  }> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting upcoming events')
      await new Promise(resolve => setTimeout(resolve, 500))
      return { exams: [], holidays: [], feeDeadlines: [] }
    }

    const response = await apiService.get('/dashboard/upcoming-events')
    return response.data
  },

  async getQuickStats(): Promise<{
    todayAdmissions: number
    todayPayments: number
    todayPaymentAmount: number
    pendingFollowups: number
    overdueFeesCount: number
    overdueFeesAmount: number
  }> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting quick stats')
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        todayAdmissions: 0,
        todayPayments: 0,
        todayPaymentAmount: 0,
        pendingFollowups: 0,
        overdueFeesCount: 0,
        overdueFeesAmount: 0
      }
    }

    const response = await apiService.get('/dashboard/quick-stats')
    return response.data
  }
}

export default dashboardService
