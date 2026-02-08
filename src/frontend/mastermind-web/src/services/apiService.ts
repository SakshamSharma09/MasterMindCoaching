import axios from 'axios'
import type { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios'
import { API_BASE_URL, API_TIMEOUT, DEFAULT_HEADERS, getApiUrl } from '@/config/api'

// Extended interface for custom config options
interface ExtendedAxiosRequestConfig extends AxiosRequestConfig {
  bypassRedirect?: boolean
}

// Create axios instance with default configuration
const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: API_TIMEOUT,
  headers: DEFAULT_HEADERS,
})

// Request interceptor to add auth token
apiClient.interceptors.request.use(
  (config) => {
    // Get token from localStorage (auth store uses 'mastermind-auth' key with 'accessToken' field)
    const authData = localStorage.getItem('mastermind-auth')
    let token = null
    
    if (authData) {
      try {
        const parsedAuth = JSON.parse(authData)
        token = parsedAuth.accessToken
      } catch (error) {
        console.error('Error parsing auth data:', error)
      }
    }
    
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    
    // Log request in development
    if (import.meta.env.DEV) {
      console.log(`🚀 API Request: ${config.method?.toUpperCase()} ${config.url}`)
    }
    
    return config
  },
  (error) => {
    console.error('Request Error:', error)
    return Promise.reject(error)
  }
)

// Response interceptor to handle common responses
apiClient.interceptors.response.use(
  (response) => {
    // Log response in development
    if (import.meta.env.DEV) {
      console.log(`✅ API Response: ${response.status} ${response.config.url}`)
    }
    
    return response
  },
  (error) => {
    // Check if this call should bypass automatic redirect
    const bypassRedirect = (error.config as ExtendedAxiosRequestConfig)?.bypassRedirect
    
    // Handle common errors
    if (error.response) {
      const { status, data } = error.response
      
      switch (status) {
        case 401:
          // Unauthorized - clear auth data and redirect to login
          localStorage.removeItem('mastermind-auth')
          
          // Only redirect if not bypassed
          if (!bypassRedirect) {
            window.location.href = '/login'
          }
          break
          
        case 403:
          // Forbidden - show access denied message
          console.error('Access denied: You do not have permission to access this resource')
          break
          
        case 404:
          // Not found
          console.error('Resource not found')
          break
          
        case 500:
          // Server error
          console.error('Server error. Please try again later.')
          break
      }
      
      if (import.meta.env.DEV) {
        console.error(`❌ API Error: ${status} ${error.config?.url}`, data)
      }
    } else if (error.request) {
      // Network error
      console.error('Network error. Please check your connection.')
      if (import.meta.env.DEV) {
        console.error('Network Error:', error.request)
      }
    } else {
      // Other error
      console.error('Request setup error:', error.message)
    }
    
    return Promise.reject(error)
  }
)

// Generic API methods
export const apiService = {
  // GET request
  async get<T = any>(endpoint: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await apiClient.get<T>(endpoint, config)
    return response.data
  },

  // POST request
  async post<T = any>(endpoint: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await apiClient.post<T>(endpoint, data, config)
    return response.data
  },

  // PUT request
  async put<T = any>(endpoint: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await apiClient.put<T>(endpoint, data, config)
    return response.data
  },

  // PATCH request
  async patch<T = any>(endpoint: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await apiClient.patch<T>(endpoint, data, config)
    return response.data
  },

  // DELETE request
  async delete<T = any>(endpoint: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await apiClient.delete<T>(endpoint, config)
    return response.data
  },

  // Upload file
  async upload<T = any>(endpoint: string, file: File, onProgress?: (progress: number) => void): Promise<T> {
    const formData = new FormData()
    formData.append('file', file)

    const config: AxiosRequestConfig = {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
      onUploadProgress: (progressEvent) => {
        if (onProgress && progressEvent.total) {
          const progress = Math.round((progressEvent.loaded * 100) / progressEvent.total)
          onProgress(progress)
        }
      },
    }

    const response = await apiClient.post<T>(endpoint, formData, config)
    return response.data
  },

  // Download file
  async download(endpoint: string, filename?: string): Promise<void> {
    const response = await apiClient.get(endpoint, {
      responseType: 'blob',
    })

    // Create download link
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    
    // Set filename if provided, otherwise get from Content-Disposition header
    if (filename) {
      link.setAttribute('download', filename)
    } else {
      const contentDisposition = response.headers['content-disposition']
      if (contentDisposition) {
        const filenameMatch = contentDisposition.match(/filename="(.+)"/)
        if (filenameMatch) {
          link.setAttribute('download', filenameMatch[1])
        }
      }
    }
    
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
  },

  // GET request without automatic redirect on 401
  async getWithoutRedirect(endpoint: string, params?: any): Promise<any> {
    const config: ExtendedAxiosRequestConfig = {
      params,
      bypassRedirect: true // Custom flag to bypass redirect
    }
    const response = await apiClient.get(endpoint, config)
    return response.data
  },
}

// Export the axios instance for custom requests
export { apiClient }

// Export helper function to get full API URL
export { getApiUrl }

// Types for API responses
export interface ApiResponse<T = any> {
  success: boolean
  data: T
  message?: string
  errors?: Record<string, string[]>
}

export interface PaginatedResponse<T = any> {
  success: boolean
  data: T[]
  pagination: {
    page: number
    limit: number
    total: number
    totalPages: number
  }
  message?: string
}
