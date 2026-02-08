import { apiService } from './apiService'
import { API_ENDPOINTS } from '@/config/api'

const USE_MOCK_API = import.meta.env.VITE_USE_MOCK_API === 'true' || false

// Mock data for development
const mockClasses: any[] = []

export interface Class {
  id: number
  name: string
  subjects: string[]
  medium: string
  board: string
  teachers: string[]
  studentCount: number
  maxStudents: number
  status: 'Active' | 'Inactive'
  createdAt: string
}

export interface CreateClassDto {
  name: string
  subjects: string[]
  medium: string
  board: string
  academicYear?: string
  status: 'Active' | 'Inactive'
}

export const classesService = {
  // Get all classes
  async getClasses(): Promise<Class[]> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting classes')
      await new Promise(resolve => setTimeout(resolve, 500))
      return mockClasses
    }

    const response = await apiService.get(API_ENDPOINTS.CLASSES.LIST)
    const apiData = response.data
    
    // Map API response to frontend interface
    return apiData.map((item: any) => ({
      id: item.id,
      name: item.name,
      subjects: item.subjects || [],
      medium: item.medium,
      board: item.board,
      teachers: item.teachers || [],
      studentCount: item.studentCount || 0,
      maxStudents: item.maxStudents || 30,
      status: item.isActive ? 'Active' : 'Inactive',
      createdAt: item.createdAt
    }))
  },

  // Get a single class by ID
  async getClass(id: number): Promise<Class> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting class with id:', id)
      await new Promise(resolve => setTimeout(resolve, 500))
      const mockClass = mockClasses.find(c => c.id === id)
      if (!mockClass) {
        throw new Error('Class not found')
      }
      return mockClass
    }

    const response = await apiService.get(API_ENDPOINTS.CLASSES.UPDATE(id.toString()))
    return response.data
  },

  // Create a new class
  async createClass(data: CreateClassDto): Promise<Class> {
    if (USE_MOCK_API) {
      console.log('Mock API: Creating class:', data)
      await new Promise(resolve => setTimeout(resolve, 800))
      
      const newClass: Class = {
        id: Date.now(),
        name: data.name,
        subjects: data.subjects,
        medium: data.medium,
        board: data.board,
        teachers: [],
        studentCount: 0,
        maxStudents: 30,
        status: data.status,
        createdAt: new Date().toISOString()
      }
      
      mockClasses.push(newClass)
      return newClass
    }

    const payload = {
      ...data,
      isActive: data.status === 'Active'
    }
    const response = await apiService.post(API_ENDPOINTS.CLASSES.CREATE, payload)
    return response.data
  },

  // Update an existing class
  async updateClass(id: number, data: Partial<CreateClassDto>): Promise<Class> {
    if (USE_MOCK_API) {
      console.log('Mock API: Updating class:', id, data)
      await new Promise(resolve => setTimeout(resolve, 800))
      
      const index = mockClasses.findIndex(c => c.id === id)
      if (index === -1) {
        throw new Error('Class not found')
      }
      
      mockClasses[index] = {
        ...mockClasses[index],
        ...data
      }
      
      return mockClasses[index]
    }

    const payload: any = { ...data }
    if (data.status) {
      payload.isActive = data.status === 'Active'
      delete payload.status
    }

    const response = await apiService.put(API_ENDPOINTS.CLASSES.UPDATE(id.toString()), payload)
    return response.data
  },

  // Delete a class
  async deleteClass(id: number): Promise<void> {
    if (USE_MOCK_API) {
      console.log('Mock API: Deleting class:', id)
      await new Promise(resolve => setTimeout(resolve, 500))
      
      const index = mockClasses.findIndex(c => c.id === id)
      if (index === -1) {
        throw new Error('Class not found')
      }
      
      mockClasses.splice(index, 1)
      return
    }

    await apiService.delete(API_ENDPOINTS.CLASSES.DELETE(id.toString()))
  }
}

export default classesService
