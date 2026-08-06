export const normalizeHouseholdMobile = (mobile?: string): string => {
  const digits = (mobile || '').replace(/\D/g, '')
  if (digits.length > 10 && digits.startsWith('91')) return digits.slice(-10)
  return digits
}

export const householdKey = (studentId: number, mobile?: string): string =>
  normalizeHouseholdMobile(mobile) || `student-${studentId}`

export const overdueMonthKey = (dueDate: string): string => dueDate.slice(0, 7)
