import { describe, expect, it } from 'vitest'
import { normalizeIndianMobile } from './phoneUtils'
import { fileNameFromContentDisposition } from './fileDownload'
import { buildStudentQueryParams, mapStudentPayload, normalizeSchoolKey, resolveParentNames } from '@/services/studentsService'
import { buildAbsentWhatsAppMessage, resolveAttendanceParentGreeting } from './attendanceMessaging'
import { buildParentInvitationWhatsAppMessage } from './parentInvitation'
import { matchesDuePeriod } from './datePeriod'
import { tokenExpiryTime } from './sessionExpiry'
import { billingIntervalMonths, nextCycleDueDate } from './financeSchedule'

describe('student and communication operational fixes', () => {
  it('uses next-cycle fee due dates for every supported recurrence', () => {
    expect(nextCycleDueDate('2026-04-01', 'Monthly')).toBe('2026-05-01')
    expect(nextCycleDueDate('2026-04-01', 'Quarterly')).toBe('2026-07-01')
    expect(nextCycleDueDate('2026-04-01', 'HalfYearly')).toBe('2026-10-01')
    expect(nextCycleDueDate('2026-04-01', 'Yearly')).toBe('2027-04-01')
    expect(billingIntervalMonths('HalfYearly')).toBe(6)
  })
  it('filters overdue, current-month, and next-month due dates', () => {
    const now = new Date(2026, 6, 27)
    expect(matchesDuePeriod('2026-07-15', 'thisMonth', 'Pending', now)).toBe(true)
    expect(matchesDuePeriod('2026-08-15', 'nextMonth', 'Pending', now)).toBe(true)
    expect(matchesDuePeriod('2026-06-15', 'overdue', 'Overdue', now)).toBe(true)
  })

  it('reads JWT expiry for proactive session logout', () => {
    const payload = btoa(JSON.stringify({ exp: 2_000_000_000 }))
      .replace(/\+/g, '-')
      .replace(/\//g, '_')
      .replace(/=+$/, '')
    expect(tokenExpiryTime(`header.${payload}.signature`)).toBe(2_000_000_000_000)
  })

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
      currentSchool: 'Sample Public School',
      dateOfBirth: '2011-08-14'
    })

    expect(payload.motherName).toBe('Sunita Sharma')
    expect(payload.fatherName).toBe('Rajesh Sharma')
    expect(payload.currentSchool).toBe('Sample Public School')
    expect(payload.parentEmail).toBe('parent@example.com')
    expect(payload.admissionDate.startsWith('2026-07-01')).toBe(true)
    expect(payload.dateOfBirth.startsWith('2011-08-14')).toBe(true)
  })

  it('splits a legacy combined parent name into a two-word mother name and remaining father name', () => {
    expect(resolveParentNames('Nisha Agarwal Vinod Agarwal')).toEqual({
      motherName: 'Nisha Agarwal',
      fatherName: 'Vinod Agarwal'
    })
    expect(resolveParentNames('Legacy Name', 'Explicit Mother', 'Explicit Father')).toEqual({
      motherName: 'Explicit Mother',
      fatherName: 'Explicit Father'
    })
  })

  it('greets only the mother and creates a complete absence message', () => {
    const record = {
      id: 1,
      studentId: 1,
      studentName: 'Tanush Agarwal',
      classId: 10,
      className: 'Class 10 CBSE',
      status: 'absent' as const,
      checkInTime: '',
      date: '2026-07-26',
      parentName: 'Nisha Agarwal Vinod Agarwal',
      motherName: 'Nisha Agarwal'
    }
    expect(resolveAttendanceParentGreeting(record)).toBe('Nisha Agarwal')
    const message = buildAbsentWhatsAppMessage(record, '26 July 2026')
    expect(message).toContain('Namaste Nisha Agarwal,')
    expect(message).not.toContain('Vinod Agarwal')
    expect(message).not.toContain('{{')
    expect(message).toContain('3:00 PM to 6:00 PM')
  })

  it('groups equivalent school names regardless of spaces, punctuation, or case', () => {
    expect(normalizeSchoolKey('Lotus')).toBe('lotus')
    expect(normalizeSchoolKey('Lot us')).toBe('lotus')
    expect(normalizeSchoolKey('LOT-US')).toBe('lotus')
  })

  it('builds a WhatsApp-first parent setup invitation', () => {
    const message = buildParentInvitationWhatsAppMessage({
      parentName: 'Nisha Agarwal',
      studentName: 'Tanush Agarwal',
      inviteUrl: 'https://example.test/accept-invitation?token=opaque'
    })
    expect(message).toContain('Namaste Nisha Agarwal,')
    expect(message).toContain('set your password and recovery email')
    expect(message).toContain('primary mobile number and password')
  })

  it('adds India country code once', () => {
    expect(normalizeIndianMobile('98872 58679')).toBe('919887258679')
    expect(normalizeIndianMobile('+91 98872 58679')).toBe('919887258679')
  })

  it('includes the selected academic session in student list requests', () => {
    const params = buildStudentQueryParams(1, 50, undefined, 2)
    expect(params.get('sessionId')).toBe('2')
  })

  it('supports mobile-first parent onboarding without an admin-entered email', () => {
    const payload = mapStudentPayload({
      firstName: 'Asha',
      lastName: 'Sharma',
      parentMobile: '9887258679',
      secondaryParentMobile: '9876543210',
      parentEmail: '',
      admissionDate: '2026-07-26',
      status: 'Active'
    })

    expect(payload.parentMobile).toBe('9887258679')
    expect(payload.secondaryParentMobile).toBe('9876543210')
    expect(payload.parentEmail).toBe('')
  })

  it('extracts UTF-8 download filenames', () => {
    expect(fileNameFromContentDisposition("attachment; filename*=UTF-8''Fee%20Receipt.pdf"))
      .toBe('Fee Receipt.pdf')
  })
})
