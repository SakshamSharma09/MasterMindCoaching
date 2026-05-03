import { apiService } from './apiService'
import type { ApiEnvelope } from './apiResponse'
import { unwrapData } from './apiResponse'

export interface AdminNote {
  id: number
  title: string
  content: string
  noteDate: string
  createdAt: string
  updatedAt?: string
}

export const adminNotesService = {
  async getAll(): Promise<AdminNote[]> {
    const res = await apiService.get<ApiEnvelope<AdminNote[]>>('/adminnotes')
    return unwrapData(res) || []
  },
  async create(payload: Partial<AdminNote>): Promise<AdminNote> {
    const res = await apiService.post<ApiEnvelope<AdminNote>>('/adminnotes', payload)
    return unwrapData(res)
  },
  async update(id: number, payload: Partial<AdminNote>): Promise<AdminNote> {
    const res = await apiService.put<ApiEnvelope<AdminNote>>(`/adminnotes/${id}`, payload)
    return unwrapData(res)
  },
  async remove(id: number): Promise<void> {
    await apiService.delete(`/adminnotes/${id}`)
  }
}
