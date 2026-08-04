import { describe, expect, it } from 'vitest'
import {
  buildBirthdayTemplateMessage,
  buildFeeReminderTemplateMessage,
  buildReceiptTemplateMessage,
  buildWelcomeTemplateMessage
} from './templateWhatsApp'

describe('template WhatsApp messages', () => {
  const messages = [
    buildWelcomeTemplateMessage({ studentName: 'Test Student', className: 'Class 8', joiningDate: '04 Aug 2026', websiteUrl: 'https://example.test' }),
    buildBirthdayTemplateMessage({ studentName: 'Test Student' }),
    buildFeeReminderTemplateMessage({ studentName: 'Test Student', className: 'Class 8', feePeriod: '2026-08', amount: 'Rs. 2,000', dueDate: '01 Sep 2026' }),
    buildReceiptTemplateMessage({ receiptNumber: 'MM-101', studentName: 'Test Student', feePeriod: '2026-08', amount: 'Rs. 2,000', receiptDate: '04 Aug 2026', paymentMethod: 'UPI' })
  ]

  it('keeps sender-only attachment instructions out of every parent message', () => {
    for (const message of messages) {
      expect(message.toLowerCase()).not.toContain('attach')
      expect(message.toLowerCase()).not.toContain('downloaded')
      expect(message).toContain('The Master Mind Coaching Classes')
    }
  })

  it('includes the complete fee period and payment context', () => {
    expect(messages[2]).toContain('Fee period: 2026-08')
    expect(messages[2]).toContain('Outstanding amount: Rs. 2,000')
    expect(messages[3]).toContain('Receipt: MM-101')
    expect(messages[3]).toContain('Payment method: UPI')
  })
})
