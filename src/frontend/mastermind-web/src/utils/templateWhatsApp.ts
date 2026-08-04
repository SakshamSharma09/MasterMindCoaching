const SIGNATURE = '— The Master Mind Coaching Classes'

export const buildWelcomeTemplateMessage = (details: {
  studentName: string
  className: string
  joiningDate: string
  websiteUrl: string
}) => [
  'Namaste,',
  `We are delighted to welcome ${details.studentName} to The Master Mind Coaching Classes.`,
  `Class: ${details.className}`,
  `Admission date: ${details.joiningDate}`,
  `After completing the private account invitation, sign in with the registered primary mobile number and password at ${details.websiteUrl}.`,
  SIGNATURE
].join('\n')

export const buildBirthdayTemplateMessage = (details: { studentName: string }) => [
  `Happy Birthday, ${details.studentName}!`,
  'The Master Mind Coaching Classes wishes you a joyful year filled with confidence, learning, and success.',
  'Keep learning, growing, and shining.',
  SIGNATURE
].join('\n')

export const buildFeeReminderTemplateMessage = (details: {
  studentName: string
  className: string
  feePeriod: string
  amount: string
  dueDate: string
}) => [
  'Namaste,',
  `This is a fee reminder for ${details.studentName}.`,
  `Class: ${details.className}`,
  `Fee period: ${details.feePeriod}`,
  `Outstanding amount: ${details.amount}`,
  `Due date: ${details.dueDate}`,
  'Please complete the payment or contact us if this record needs correction.',
  SIGNATURE
].join('\n')

export const buildReceiptTemplateMessage = (details: {
  receiptNumber: string
  studentName: string
  feePeriod: string
  amount: string
  receiptDate: string
  paymentMethod: string
}) => [
  'Namaste,',
  `Payment received for ${details.studentName}.`,
  `Receipt: ${details.receiptNumber}`,
  `Fee period: ${details.feePeriod}`,
  `Amount received: ${details.amount}`,
  `Payment date: ${details.receiptDate}`,
  `Payment method: ${details.paymentMethod}`,
  'Thank you. Your payment has been recorded successfully.',
  SIGNATURE
].join('\n')
