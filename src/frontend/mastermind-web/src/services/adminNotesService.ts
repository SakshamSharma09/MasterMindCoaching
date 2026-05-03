import { apiService } from './apiService'

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
    const res = await apiService.get('/adminnotes')
    return res?.data || []
  },
  async create(payload: Partial<AdminNote>): Promise<AdminNote> {
    const res = await apiService.post('/adminnotes', payload)
    return res.data
  },
  async update(id: number, payload: Partial<AdminNote>): Promise<AdminNote> {
    const res = await apiService.put(`/adminnotes/${id}`, payload)
    return res.data
  },
  async remove(id: number): Promise<void> {
    await apiService.delete(`/adminnotes/${id}`)
  }
}

