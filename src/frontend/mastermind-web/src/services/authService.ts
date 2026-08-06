import { apiService } from './apiService'
import { API_BASE_URL, API_ENDPOINTS } from '@/config/api'
import type {
  OtpRequest,
  OtpResponse,
  OtpVerifyRequest,
  AuthResponse,
  RefreshTokenRequest,
  User
} from '@/types/auth'

const USE_MOCK_API = import.meta.env.VITE_USE_MOCK_API === 'true' || false
export const AUTH_REQUEST_DEADLINE_MS = 9500
const AUTH_PRIMARY_ATTEMPT_MS = 6500
const AUTH_WARMUP_BUDGET_MS = 1200

const sleep = (ms: number) => new Promise(resolve => setTimeout(resolve, ms))

const getHealthUrl = () => {
  if (API_BASE_URL.endsWith('/api')) return API_BASE_URL.slice(0, -4) + '/health/ready'
  if (API_BASE_URL.endsWith('/api/')) return API_BASE_URL.slice(0, -5) + '/health/ready'
  if (API_BASE_URL === '/api') return '/health/ready'
  return '/health/ready'
}

let warmUpPromise: Promise<void> | null = null

const warmUpApi = async (timeoutMs = AUTH_REQUEST_DEADLINE_MS) => {
  if (warmUpPromise) return warmUpPromise

  const controller = new AbortController()
  const timer = globalThis.setTimeout(() => controller.abort(), timeoutMs)
  warmUpPromise = (async () => {
    try {
      await fetch(getHealthUrl(), {
        method: 'GET',
        cache: 'no-store',
        signal: controller.signal
      })
    } catch {
      // Login still gets its own bounded attempt if readiness is unavailable.
    } finally {
      globalThis.clearTimeout(timer)
      warmUpPromise = null
    }
  })()

  return warmUpPromise
}

const isColdStartLikeError = (error: any) =>
  error?.code === 'ECONNABORTED' ||
  /timeout/i.test(error?.message || '') ||
  !error?.response

const createAuthTimeoutError = () => {
  const error = new Error('Login service did not respond within 10 seconds. Please try again.')
  error.name = 'AuthTimeoutError'
  return error
}

const withAuthRetry = async <T>(operation: (timeoutMs: number) => Promise<T>): Promise<T> => {
  const deadline = Date.now() + AUTH_REQUEST_DEADLINE_MS
  try {
    return await operation(AUTH_PRIMARY_ATTEMPT_MS)
  } catch (error: any) {
    if (!isColdStartLikeError(error)) throw error

    const warmupBudget = Math.min(AUTH_WARMUP_BUDGET_MS, Math.max(0, deadline - Date.now() - 1000))
    if (warmupBudget > 0) {
      await Promise.race([warmUpApi(warmupBudget), sleep(warmupBudget)])
    }

    const remaining = deadline - Date.now()
    if (remaining < 750) throw createAuthTimeoutError()

    try {
      return await operation(remaining)
    } catch (secondError: any) {
      if (!isColdStartLikeError(secondError)) throw secondError
      throw createAuthTimeoutError()
    }
  }
}

export const authService = {
  // Request OTP
  async requestOtp(identifier: string, type: 'email' = 'email'): Promise<OtpResponse> {
    if (USE_MOCK_API) {
      console.log('Mock API: Requesting OTP for', identifier)
      await new Promise(resolve => setTimeout(resolve, 1000)) // Simulate network delay
      return {
        message: 'OTP sent successfully',
        expiresIn: 300
      }
    }

    const response = await withAuthRetry((timeout) => apiService.post<OtpResponse>(API_ENDPOINTS.AUTH.REQUEST_OTP, {
      identifier,
      type
    }, { timeout }))
    return response
  },

  // Verify OTP and authenticate
  async verifyOtp(identifier: string, otp: string, userData?: any): Promise<AuthResponse> {
    if (USE_MOCK_API) {
      console.log('Mock API: Verifying OTP for', identifier)
      await new Promise(resolve => setTimeout(resolve, 1500)) // Simulate network delay

      // Mock successful verification
      const mockUser: User = {
        id: 1,
        email: identifier.includes('@') ? identifier : '',
        mobile: identifier.includes('@') ? '' : identifier,
        firstName: userData?.firstName || 'Demo',
        lastName: userData?.lastName || 'User',
        role: userData?.role || 'Admin',
        isActive: true,
        isEmailVerified: true,
        isMobileVerified: true,
        createdAt: new Date().toISOString()
      }

      return {
        user: mockUser,
        accessToken: 'mock-jwt-token-' + Date.now(),
        refreshToken: 'mock-refresh-token-' + Date.now(),
        expiresIn: 3600
      }
    }

    const payload: OtpVerifyRequest = {
      identifier,
      otp,
      ...userData
    }

    const response = await withAuthRetry((timeout) => apiService.post<AuthResponse>(API_ENDPOINTS.AUTH.VERIFY_OTP, payload, { timeout }))
    return response
  },

  // Password login for admin email or provisioned parent/teacher mobile
  async loginWithPassword(identifier: string, password: string): Promise<AuthResponse> {
    const trimmedIdentifier = identifier.trim()
    const payload = trimmedIdentifier.includes('@')
      ? { email: trimmedIdentifier, identifier: trimmedIdentifier, password }
      : { mobile: trimmedIdentifier, identifier: trimmedIdentifier, password }

    const response = await withAuthRetry((timeout) => apiService.post<AuthResponse>(API_ENDPOINTS.AUTH.LOGIN, {
      ...payload
    }, { timeout }))
    return response
  },

  prepareLogin(): Promise<void> {
    return warmUpApi(AUTH_REQUEST_DEADLINE_MS)
  },

  // Quick login for demo accounts
  async quickLogin(email: string): Promise<AuthResponse> {
    const response = await apiService.post<AuthResponse>('/auth/quick-login', {
      email
    })
    return response
  },

  // Set admin password
  async setPassword(password: string, confirmPassword: string): Promise<any> {
    const response = await apiService.post('/auth/set-password', {
      password,
      confirmPassword
    })
    return response
  },

  async validateInvitation(token: string): Promise<any> {
    return apiService.get(`/auth/invitations/${encodeURIComponent(token)}`)
  },

  async acceptInvitation(token: string, email: string, password: string): Promise<any> {
    return apiService.post('/auth/invitations/accept', { token, email, password })
  },

  async getAccountSecurity(): Promise<any> {
    return apiService.get('/account/security')
  },

  async updateRecoveryEmail(email: string): Promise<any> {
    return apiService.put('/account/security/email', { email })
  },

  async requestAccountDeletion(reason = ''): Promise<any> {
    return apiService.post('/account/deletion-request', { reason })
  },

  async requestPublicAccountDeletion(emailOrMobile: string, reason = ''): Promise<any> {
    return apiService.post('/account/public-deletion-request', { emailOrMobile, reason })
  },

  // Refresh access token
  async refreshToken(refreshToken: string): Promise<AuthResponse> {
    if (USE_MOCK_API) {
      console.log('Mock API: Refreshing token')
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        user: {} as User, // Would be populated in real implementation
        accessToken: 'mock-refreshed-jwt-token-' + Date.now(),
        refreshToken: 'mock-new-refresh-token-' + Date.now(),
        expiresIn: 3600
      }
    }

    const response = await apiService.post<AuthResponse>(API_ENDPOINTS.AUTH.REFRESH_TOKEN, {
      refreshToken
    } as RefreshTokenRequest)
    return response
  },

  // Logout
  async logout(): Promise<void> {
    if (USE_MOCK_API) {
      console.log('Mock API: Logging out')
      await new Promise(resolve => setTimeout(resolve, 300))
      return
    }

    await apiService.post(API_ENDPOINTS.AUTH.LOGOUT)
  },

  // Get current user
  async getCurrentUser(): Promise<User> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting current user')
      await new Promise(resolve => setTimeout(resolve, 500))
      return {
        id: 1,
        email: 'demo@mastermind.com',
        mobile: '',
        firstName: 'Demo',
        lastName: 'User',
        role: 'Admin',
        isActive: true,
        isEmailVerified: true,
        isMobileVerified: true,
        createdAt: new Date().toISOString()
      }
    }

    const response = await apiService.get<User>(API_ENDPOINTS.AUTH.CURRENT_USER)
    return response
  },

  // Check authentication status
  async checkAuth(): Promise<boolean> {
    if (USE_MOCK_API) {
      console.log('Mock API: Checking auth status')
      await new Promise(resolve => setTimeout(resolve, 300))
      return true
    }

    try {
      await this.getCurrentUser()
      return true
    } catch {
      return false
    }
  }
}

export default authService
