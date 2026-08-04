import { normalizeIndianMobile } from './phoneUtils'

export const buildTeacherInvitationWhatsAppUrl = (
  mobile: string,
  teacherName: string,
  inviteUrl: string,
  message?: string
) => {
  const phone = normalizeIndianMobile(mobile)
  if (!phone) throw new Error('A valid teacher mobile number is required.')
  const body = message || `Namaste ${teacherName}, use this private link to set your MasterMind teacher app password and recovery email: ${inviteUrl} This link expires in 72 hours.`
  return `https://wa.me/${phone}?text=${encodeURIComponent(body)}`
}
