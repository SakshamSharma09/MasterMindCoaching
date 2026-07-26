import { describe, expect, it } from 'vitest'
import { normalizeIndianMobile } from './phoneUtils'
import { fileNameFromContentDisposition } from './fileDownload'
import { mapStudentPayload } from '@/services/studentsService'

describe('student and communication operational fixes', () => {
  it('keeps mother and father names independent and persists school/date/contact fields', () => {
    const payload = mapStudentPayload({
      firstName: 'Asha',
      lastName: 'Sharma',
      email: 'student@example.com',
      phone: '9000000000',
      parentEmail: 'parent@example.com',
      parentMobile: '9887258679',
      admissionDate: '2026-07-01',
      status: 'Active',
      motherName: 'Sunita Sharma',
      fatherName: 'Rajesh Sharma',
      currentSchool: 'Sample Public School'
    })

    expect(payload.motherName).toBe('Sunita Sharma')
    expect(payload.fatherName).toBe('Rajesh Sharma')
    expect(payload.currentSchool).toBe('Sample Public School')
    expect(payload.parentEmail).toBe('parent@example.com')
    expect(payload.admissionDate.startsWith('2026-07-01')).toBe(true)
  })

  it('adds India country code once', () => {
    expect(normalizeIndianMobile('98872 58679')).toBe('919887258679')
    expect(normalizeIndianMobile('+91 98872 58679')).toBe('919887258679')
  })

  it('extracts UTF-8 download filenames', () => {
    expect(fileNameFromContentDisposition("attachment; filename*=UTF-8''Fee%20Receipt.pdf"))
      .toBe('Fee Receipt.pdf')
  })
})
