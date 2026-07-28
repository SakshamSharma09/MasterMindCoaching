export type BillingFrequency = 'Monthly' | 'Quarterly' | 'HalfYearly' | 'Yearly' | 'OneTime'

export const billingIntervalMonths = (frequency: BillingFrequency): number => ({
  Monthly: 1,
  Quarterly: 3,
  HalfYearly: 6,
  Yearly: 12,
  OneTime: 0
})[frequency]

export const nextCycleDueDate = (startDate: string, frequency: BillingFrequency): string => {
  if (!startDate) return ''
  const [year, month, day] = startDate.split('-').map(Number)
  const target = new Date(Date.UTC(year, month - 1, day))
  target.setUTCMonth(target.getUTCMonth() + billingIntervalMonths(frequency))
  return target.toISOString().slice(0, 10)
}
