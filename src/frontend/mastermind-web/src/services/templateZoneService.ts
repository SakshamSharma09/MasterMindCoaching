import { apiService } from './apiService'

export type TemplateType = 'BirthdayWish' | 'FeeReminder' | 'FeeReceipt'

export interface MessageTemplate {
  id: number
  name: string
  type: number
  subject: string
  body: string
  isActive: boolean
  autoReminderDaysBefore: number
  frequency?: string
}

export const templateZoneService = {
  async getTemplates(): Promise<MessageTemplate[]> {
    const response = await apiService.get('/templatezone/templates')
    return response.data
  },

  async createTemplate(payload: Partial<MessageTemplate>): Promise<MessageTemplate> {
    const response = await apiService.post('/templatezone/templates', payload)
    return response.data
  },

  async updateTemplate(id: number, payload: Partial<MessageTemplate>): Promise<MessageTemplate> {
    const response = await apiService.put(`/templatezone/templates/${id}`, payload)
    return response.data
  },

  async deleteTemplate(id: number): Promise<void> {
    await apiService.delete(`/templatezone/templates/${id}`)
  },

  async getBirthdayReminders(daysAhead = 7): Promise<any[]> {
    const response = await apiService.get(`/templatezone/birthday-reminders?daysAhead=${daysAhead}`)
    return response.data
  },

  async getFeeReminders(month: string): Promise<any[]> {
    const response = await apiService.get(`/templatezone/fee-reminders?month=${month}`)
    return response.data
  },

  async getFeeReceiptLogs(take = 100): Promise<any[]> {
    const response = await apiService.get(`/templatezone/fee-receipt-logs?take=${take}`)
    return response.data
  },

  async previewTemplate(payload: { templateId: number; studentId?: number; studentFeeId?: number; feeReceiptId?: number }): Promise<any> {
    const response = await apiService.post('/templatezone/preview', payload)
    return response.data
  }
}
