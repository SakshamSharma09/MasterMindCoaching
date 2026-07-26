import type { AttendanceRecord } from '@/services/attendanceService'

export const resolveAttendanceParentGreeting = (
  record: Pick<AttendanceRecord, 'motherName' | 'fatherName' | 'parentName'>
): string => {
  const explicitName = record.motherName?.trim() || record.fatherName?.trim()
  if (explicitName) return explicitName

  const legacyParts = (record.parentName || '').trim().split(/\s+/).filter(Boolean)
  return legacyParts.length >= 3 ? legacyParts.slice(0, 2).join(' ') : 'Parent'
}

export const buildAbsentWhatsAppMessage = (
  record: AttendanceRecord,
  formattedDate: string
): string => [
  `Namaste ${resolveAttendanceParentGreeting(record)},`,
  '',
  `This is to inform you that ${record.studentName} was absent from ${record.className} on ${formattedDate}.`,
  'Class timing: 3:00 PM to 6:00 PM.',
  'Please reply with the reason for absence or contact MasterMind Coaching Classes if this record needs correction.',
  '',
  '— MasterMind Coaching Classes'
].join('\n')
