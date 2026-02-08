import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { User } from '@/types/auth'
import { authService } from '@/services/authService'

export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref<User | null>(null)
  const accessToken = ref<string | null>(null)
  const refreshToken = ref<string | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const isAuthenticated = computed(() => !!accessToken.value && !!user.value)
  const userRole = computed(() => user.value?.role || null)
  const userName = computed(() => user.value ? `${user.value.firstName} ${user.value.lastName}` : '')

  // Actions
  const setTokens = (access: string, refresh: string) => {
    accessToken.value = access
    refreshToken.value = refresh
  }

  const setUser = (userData: User) => {
    user.value = userData
  }

  const clearAuth = () => {
    user.value = null
    accessToken.value = null
    refreshToken.value = null
    error.value = null
  }

  const requestOtp = async (identifier: string, type: 'email' | 'mobile') => {
    isLoading.value = true
    error.value = null

    try {
      const response = await authService.requestOtp(identifier, type)
      return response
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Failed to send OTP'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  const verifyOtp = async (identifier: string, otp: string, userData?: any) => {
    isLoading.value = true
    error.value = null

    try {
      const response = await authService.verifyOtp(identifier, otp, userData)
      setTokens(response.accessToken, response.refreshToken)
      setUser(response.user)
      return response
    } catch (err: any) {
      error.value = err.response?.data?.message || 'OTP verification failed'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  const refreshAccessToken = async () => {
    if (!refreshToken.value) {
      throw new Error('No refresh token available')
    }

    try {
      const response = await authService.refreshToken(refreshToken.value)
      setTokens(response.accessToken, response.refreshToken)
      return response
    } catch (err: any) {
      clearAuth()
      throw err
    }
  }

  const logout = async () => {
    try {
      await authService.logout()
    } catch (err) {
      console.error('Logout error:', err)
    } finally {
      clearAuth()
    }
  }

  const getCurrentUser = async () => {
    if (!accessToken.value) return null

    try {
      const userData = await authService.getCurrentUser()
      setUser(userData)
      return userData
    } catch (err: any) {
      if (err.response?.status === 401) {
        clearAuth()
      }
      throw err
    }
  }

  const initializeAuth = async () => {
    // Check if we have stored tokens
    const storedAccessToken = localStorage.getItem('accessToken')
    const storedRefreshToken = localStorage.getItem('refreshToken')
    const storedUser = localStorage.getItem('user')

    if (storedAccessToken && storedRefreshToken && storedUser) {
      accessToken.value = storedAccessToken
      refreshToken.value = storedRefreshToken

      try {
        user.value = JSON.parse(storedUser)
        // Verify token is still valid
        await getCurrentUser()
      } catch (err) {
        clearAuth()
      }
    }
  }

  // Bypass authentication for test emails
  const bypassAuth = (identifier: string) => {
    const testAccounts = {
      'admin@mastermind.com': {
        id: 1,
        email: 'admin@mastermind.com',
        mobile: '+1234567890',
        firstName: 'Test',
        lastName: 'Administrator',
        role: 'Admin' as const,
        isActive: true,
        isEmailVerified: true,
        isMobileVerified: true,
        createdAt: new Date().toISOString()
      },
      'teacher@mastermind.com': {
        id: 2,
        email: 'teacher@mastermind.com',
        mobile: '+1234567891',
        firstName: 'Test',
        lastName: 'Teacher',
        role: 'Teacher' as const,
        isActive: true,
        isEmailVerified: true,
        isMobileVerified: true,
        createdAt: new Date().toISOString()
      },
      'parent@mastermind.com': {
        id: 3,
        email: 'parent@mastermind.com',
        mobile: '+1234567892',
        firstName: 'Test',
        lastName: 'Parent',
        role: 'Parent' as const,
        isActive: true,
        isEmailVerified: true,
        isMobileVerified: true,
        createdAt: new Date().toISOString()
      }
    }

    const testUser = testAccounts[identifier as keyof typeof testAccounts]
    if (!testUser) {
      throw new Error('Invalid test email')
    }

    // Set mock tokens and user data
    const mockAccessToken = 'mock-access-token-' + Date.now()
    const mockRefreshToken = 'mock-refresh-token-' + Date.now()
    
    setTokens(mockAccessToken, mockRefreshToken)
    setUser(testUser)
    
    return {
      accessToken: mockAccessToken,
      refreshToken: mockRefreshToken,
      user: testUser
    }
  }

  return {
    // State
    user,
    accessToken,
    refreshToken,
    isLoading,
    error,

    // Getters
    isAuthenticated,
    userRole,
    userName,

    // Actions
    setTokens,
    setUser,
    clearAuth,
    requestOtp,
    verifyOtp,
    refreshAccessToken,
    logout,
    getCurrentUser,
    initializeAuth,
    bypassAuth
  }
}, {
  persist: {
    key: 'mastermind-auth',
    storage: localStorage,
    paths: ['user', 'accessToken', 'refreshToken']
  }
})