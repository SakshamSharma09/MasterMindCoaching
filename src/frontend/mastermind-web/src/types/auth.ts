// Authentication related types

export interface User {
  id: number
  email: string
  mobile: string
  firstName: string
  lastName: string
  role: UserRole
  isActive: boolean
  isEmailVerified: boolean
  isMobileVerified: boolean
  lastLoginAt?: string
  profileImageUrl?: string
  createdAt: string
  updatedAt?: string
}

export type UserRole = 'Admin' | 'Teacher' | 'Parent'

export interface OtpRequest {
  identifier: string
  type: 'email' | 'mobile'
}

export interface OtpResponse {
  message: string
  expiresIn: number
}

export interface OtpVerifyRequest {
  identifier: string
  otp: string
  firstName?: string
  lastName?: string
  role?: UserRole
}

export interface AuthResponse {
  user: User
  accessToken: string
  refreshToken: string
  expiresIn: number
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface ApiResponse<T = any> {
  success: boolean
  data?: T
  message?: string
  errors?: string[]
}