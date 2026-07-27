export type DuePeriod = '' | 'overdue' | 'thisMonth' | 'nextMonth'

const localDate = (value: string): Date => {
  const [year, month, day] = value.slice(0, 10).split('-').map(Number)
  return new Date(year, month - 1, day)
}

export const matchesDuePeriod = (
  value: string,
  period: DuePeriod,
  status = '',
  now = new Date()
): boolean => {
  if (!period) return true
  if (period === 'overdue') return status.toLowerCase() === 'overdue'

  const date = localDate(value)
  const monthOffset = period === 'nextMonth' ? 1 : 0
  const target = new Date(now.getFullYear(), now.getMonth() + monthOffset, 1)
  return date.getFullYear() === target.getFullYear() &&
    date.getMonth() === target.getMonth()
}
