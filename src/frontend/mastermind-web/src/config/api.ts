// API Configuration for THE MASTERMIND COACHING CLASSES
// Change this URL to connect to different backend environments

// For local development
export const API_BASE_URL = 'https://localhost:49627/api'

// For production deployment (uncomment when deploying)
// export const API_BASE_URL = 'https://your-production-domain.com/api'

// For staging environment (uncomment when needed)
// export const API_BASE_URL = 'https://staging.mastermindcoaching.com/api'

// API Endpoints
export const API_ENDPOINTS = {
  // Authentication
  AUTH: {
    LOGIN: '/auth/login',
    REGISTER: '/auth/register',
    REQUEST_OTP: '/auth/request-otp',
    VERIFY_OTP: '/auth/verify-otp',
    REFRESH_TOKEN: '/auth/refresh-token',
    LOGOUT: '/auth/logout',
    CURRENT_USER: '/auth/me'
  },
  
  // Dashboard
  DASHBOARD: {
    PARENT_STATS: '/dashboard/parent-stats',
    TEACHER_STATS: '/dashboard/teacher-stats',
    ADMIN_STATS: '/dashboard/admin-stats',
    RECENT_STUDENTS: '/dashboard/recent-students'
  },

  // Parent
  PARENT: {
    BASE: '/parent',
    CHILDREN: '/parent/children',
    DASHBOARD_STATS: '/parent/dashboard/stats'
  },
  
  // Students
  STUDENTS: {
    LIST: '/students',
    CREATE: '/students',
    UPDATE: (id: string) => `/students/${id}`,
    DELETE: (id: string) => `/students/${id}`,
    ATTENDANCE: '/students/attendance',
    PERFORMANCE: (id: string) => `/students/${id}/performance`,
    REMARKS: (id: string) => `/students/${id}/remarks`
  },
  
  // Teachers
  TEACHERS: {
    LIST: '/teachers',
    CREATE: '/teachers',
    UPDATE: (id: string) => `/teachers/${id}`,
    DELETE: (id: string) => `/teachers/${id}`,
    SCHEDULE: '/teachers/schedule',
    CLASSES: '/teachers/classes'
  },
  
  // Classes
  CLASSES: {
    LIST: '/classes',
    CREATE: '/classes',
    UPDATE: (id: string) => `/classes/${id}`,
    DELETE: (id: string) => `/classes/${id}`,
    ATTENDANCE: (id: string) => `/classes/${id}/attendance`,
    SUBJECTS: (id: string) => `/classes/${id}/subjects`
  },
  
  // Attendance
  ATTENDANCE: {
    MARK: '/attendance/mark',
    REPORT: '/attendance/report',
    SUMMARY: '/attendance/summary',
    HISTORY: '/attendance/history'
  },
  
  // Fees
  FEES: {
    STRUCTURE: '/fees/structure',
    PAYMENT: '/fees/payment',
    HISTORY: '/fees/history',
    PENDING: '/fees/pending',
    INVOICE: (id: string) => `/fees/invoice/${id}`
  },
  
  // Subjects
  SUBJECTS: {
    LIST: '/subjects',
    CREATE: '/subjects',
    UPDATE: (id: string) => `/subjects/${id}`,
    DELETE: (id: string) => `/subjects/${id}`,
    ASSIGN: '/subjects/assign'
  },
  
  // Reports
  REPORTS: {
    PERFORMANCE: '/reports/performance',
    ATTENDANCE: '/reports/attendance',
    FINANCIAL: '/reports/financial',
    ACADEMIC: '/reports/academic'
  },
  
  // Notifications
  NOTIFICATIONS: {
    LIST: '/notifications',
    SEND: '/notifications/send',
    MARK_READ: (id: string) => `/notifications/${id}/read`,
    PREFERENCES: '/notifications/preferences'
  }
}

// Default timeout for API requests (in milliseconds)
export const API_TIMEOUT = 30000

// Request headers configuration
export const DEFAULT_HEADERS = {
  'Content-Type': 'application/json',
  'Accept': 'application/json'
}

// Environment detection
export const isDevelopment = import.meta.env.DEV
export const isProduction = import.meta.env.PROD

// Helper function to get full API URL
export const getApiUrl = (endpoint: string): string => {
  return `${API_BASE_URL}${endpoint}`
}

// Helper function to check if URL is absolute
export const isAbsoluteUrl = (url: string): boolean => {
  return /^https?:\/\//.test(url)
}

// Helper function to get image URL
export const getImageUrl = (path: string): string => {
  if (isAbsoluteUrl(path)) return path
  return `${API_BASE_URL}/uploads/${path}`
}
