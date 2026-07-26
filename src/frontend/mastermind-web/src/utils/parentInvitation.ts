export interface ParentInvitationMessageInput {
  parentName?: string
  studentName: string
  inviteUrl: string
}

export const buildParentInvitationWhatsAppMessage = ({
  parentName,
  studentName,
  inviteUrl
}: ParentInvitationMessageInput): string => [
  `Namaste ${parentName?.trim() || 'Parent'},`,
  '',
  `You are invited to join the MasterMind Coaching Classes parent app for ${studentName}.`,
  'Please open this private link to set your password and recovery email:',
  inviteUrl,
  '',
  'After setup, sign in using your primary mobile number and password.',
  'This single-use link expires in 72 hours.',
  '',
  '— MasterMind Coaching Classes'
].join('\n')
