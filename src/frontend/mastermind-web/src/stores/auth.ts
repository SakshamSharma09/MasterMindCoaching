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

  const loginWithPassword = async (email: string, password: string) => {
    isLoading.value = true
    error.value = null

    try {
      const response = await authService.loginWithPassword(email, password)
      setTokens(response.accessToken, response.refreshToken)
      setUser(response.user)
      return response
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Login failed'
      throw err
    } finally {
      isLoading.value = false
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
    // Pinia persistence plugin auto-restores state from localStorage
    // We just need to verify the token is still valid if we have one
    if (accessToken.value && user.value) {
      try {
        await getCurrentUser()
      } catch (err) {
        console.warn('[Auth] Token validation failed, clearing auth')
        clearAuth()
      }
    }
  }

  // Bypass authentication for test emails - now calls backend
  const bypassAuth = async (identifier: string) => {
    try {
      // Call the backend quick-login endpoint
      const response = await authService.quickLogin(identifier)
      
      if (response.accessToken) {
        setTokens(response.accessToken, response.refreshToken)
        setUser(response.user)
        
        console.log('[Auth Store] Quick login successful:', response.user)
        
        return {
          accessToken: response.accessToken,
          refreshToken: response.refreshToken,
          user: response.user
        }
      } else {
        throw new Error(response.message || 'Quick login failed')
      }
    } catch (error: any) {
      console.error('[Auth Store] Quick login failed:', error)
      const errorMessage = error.response?.data?.message || error.message || 'Quick login failed'
      throw new Error(errorMessage)
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
    loginWithPassword,
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