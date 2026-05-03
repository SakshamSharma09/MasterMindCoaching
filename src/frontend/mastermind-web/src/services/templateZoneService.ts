import { apiService } from './apiService'
import type { ApiEnvelope } from './apiResponse'
import { unwrapData } from './apiResponse'

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
  variablesJson?: string
}

export interface BirthdayReminder {
  id: number
  studentName: string
  parentName: string
  parentMobile: string
  profileImageUrl?: string
  dateOfBirth: string
  nextBirthday: string
  daysLeft: number
}

export interface FeeReminder {
  id: number
  studentId: number
  studentName: string
  parentName: string
  parentMobile: string
  className: string
  feeAmount: number
  month: string
  dueDate: string
  joiningDate: string
  frequency: string
}

export interface FeeReceiptLog {
  id: number
  receiptNumber: string
  studentName: string
  parentName: string
  parentMobile: string
  feePeriod: string
  paidAmount: number
  totalAmount: number
  paymentMethod: string
  receiptDate: string
}

export interface TemplatePreviewResponse {
  id: number
  name: string
  renderedSubject: string
  renderedBody: string
  placeholders: Record<string, string>
}

export const templateZoneService = {
  async getTemplates(): Promise<MessageTemplate[]> {
    const response = await apiService.get<ApiEnvelope<MessageTemplate[]>>('/templatezone/templates')
    return unwrapData(response)
  },

  async createTemplate(payload: Partial<MessageTemplate>): Promise<MessageTemplate> {
    const response = await apiService.post<ApiEnvelope<MessageTemplate>>('/templatezone/templates', payload)
    return unwrapData(response)
  },

  async updateTemplate(id: number, payload: Partial<MessageTemplate>): Promise<MessageTemplate> {
    const response = await apiService.put<ApiEnvelope<MessageTemplate>>(`/templatezone/templates/${id}`, payload)
    return unwrapData(response)
  },

  async deleteTemplate(id: number): Promise<void> {
    await apiService.delete(`/templatezone/templates/${id}`)
  },

  async getBirthdayReminders(daysAhead = 7): Promise<BirthdayReminder[]> {
    const response = await apiService.get<ApiEnvelope<BirthdayReminder[]>>(`/templatezone/birthday-reminders?daysAhead=${daysAhead}`)
    return unwrapData(response)
  },

  async getFeeReminders(month: string): Promise<FeeReminder[]> {
    const response = await apiService.get<ApiEnvelope<FeeReminder[]>>(`/templatezone/fee-reminders?month=${month}`)
    return unwrapData(response)
  },

  async getFeeReceiptLogs(take = 100): Promise<FeeReceiptLog[]> {
    const response = await apiService.get<ApiEnvelope<FeeReceiptLog[]>>(`/templatezone/fee-receipt-logs?take=${take}`)
    return unwrapData(response)
  },

  async previewTemplate(payload: { templateId: number; studentId?: number; studentFeeId?: number; feeReceiptId?: number }): Promise<TemplatePreviewResponse> {
    const response = await apiService.post<ApiEnvelope<TemplatePreviewResponse>>('/templatezone/preview', payload)
    return unwrapData(response)
  }
}
