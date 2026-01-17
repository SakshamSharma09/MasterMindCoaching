import axios from 'axios'
import type {
  OtpRequest,
  OtpResponse,
  OtpVerifyRequest,
  AuthResponse,
  RefreshTokenRequest,
  User
} from '@/types/auth'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  }
})

// Request interceptor to add auth token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Response interceptor to handle token refresh
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      try {
        const refreshToken = localStorage.getItem('refreshToken')
        if (refreshToken) {
          const response = await api.post('/auth/token/refresh', {
            refreshToken
          })

          const { accessToken } = response.data.data
          localStorage.setItem('accessToken', accessToken)

          originalRequest.headers.Authorization = `Bearer ${accessToken}`
          return api(originalRequest)
        }
      } catch (refreshError) {
        // Refresh failed, redirect to login
        localStorage.removeItem('accessToken')
        localStorage.removeItem('refreshToken')
        localStorage.removeItem('user')
        window.location.href = '/login'
      }
    }

    return Promise.reject(error)
  }
)

export const authService = {
  // Request OTP
  async requestOtp(identifier: string, type: 'email' | 'mobile'): Promise<OtpResponse> {
    const response = await api.post('/auth/otp/request', {
      identifier,
      type
    })
    return response.data.data
  },

  // Verify OTP and authenticate
  async verifyOtp(identifier: string, otp: string, userData?: any): Promise<AuthResponse> {
    const payload: OtpVerifyRequest = {
      identifier,
      otp,
      ...userData
    }

    const response = await api.post('/auth/otp/verify', payload)
    return response.data.data
  },

  // Refresh access token
  async refreshToken(refreshToken: string): Promise<AuthResponse> {
    const response = await api.post('/auth/token/refresh', {
      refreshToken
    } as RefreshTokenRequest)
    return response.data.data
  },

  // Logout
  async logout(): Promise<void> {
    await api.post('/auth/logout')
  },

  // Get current user
  async getCurrentUser(): Promise<User> {
    const response = await api.get('/auth/me')
    return response.data.data
  },

  // Check authentication status
  async checkAuth(): Promise<boolean> {
    try {
      await this.getCurrentUser()
      return true
    } catch {
      return false
    }
  }
}

export default authService