<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
      <div>
        <h2 class="text-xl font-semibold text-gray-900">Overdue Fees</h2>
        <p class="mt-1 text-sm text-gray-500">Review unpaid dues, open WhatsApp reminders, and close payments after collection.</p>
      </div>
      <button
        @click="sendAllReminders"
        :disabled="loading || overdueFees.length === 0"
        class="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-orange-600 rounded-lg shadow-sm hover:bg-orange-700 focus:outline-none focus:ring-2 focus:ring-orange-500 focus:ring-offset-2 disabled:opacity-50 transition-colors"
      >
        <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 4.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
        </svg>
        Log All Reminders
      </button>
    </div>

    <div class="space-y-4">
      <section v-for="month in groupedOverdueFees" :key="month.key" class="overflow-hidden rounded-2xl border border-red-100 bg-white shadow-sm">
        <div class="flex items-center justify-between bg-red-50 px-4 py-3">
          <div><h3 class="font-bold text-slate-950">{{ month.label }} overdue</h3><p class="text-xs text-red-700">{{ month.households.length }} household{{ month.households.length === 1 ? '' : 's' }}</p></div>
          <p class="font-bold tabular-nums text-red-700">₹{{ formatCurrency(month.balance) }}</p>
        </div>
        <div class="space-y-2 p-3">
          <details v-for="household in month.households" :key="household.key" class="overflow-hidden rounded-xl border border-slate-200 bg-white">
            <summary class="flex min-h-14 cursor-pointer list-none items-center justify-between gap-3 px-3 py-2 marker:hidden">
              <div class="min-w-0"><p class="truncate text-sm font-semibold text-slate-900">{{ household.parentName || 'Parent household' }}</p><p class="text-xs text-slate-500">{{ displayMobile(household.mobile) }} · {{ household.studentCount }} student{{ household.studentCount === 1 ? '' : 's' }}</p></div>
              <div class="text-right"><p class="text-sm font-bold tabular-nums text-red-700">₹{{ formatCurrency(household.balance) }}</p><p class="text-[11px] text-slate-500">{{ household.fees.length }} due</p></div>
            </summary>
            <div class="border-t border-slate-100 bg-slate-50/70 p-3">
              <div v-for="fee in household.fees" :key="fee.id" class="mb-2 flex items-center justify-between gap-3 rounded-lg bg-white p-3 last:mb-0">
                <div class="min-w-0"><p class="truncate text-sm font-semibold text-slate-900">{{ fee.studentName }}</p><p class="truncate text-xs text-slate-500">{{ fee.className }} · due {{ formatDate(fee.dueDate) }}</p></div>
                <p class="shrink-0 text-sm font-bold tabular-nums">₹{{ formatCurrency(fee.balanceAmount || fee.amount) }}</p>
              </div>
              <div class="mt-3 grid grid-cols-2 gap-2">
                <button type="button" class="min-h-11 rounded-lg bg-emerald-600 px-3 text-sm font-semibold text-white" @click="sendHouseholdReminder(household)">WhatsApp reminder</button>
                <button type="button" class="min-h-11 rounded-lg bg-indigo-600 px-3 text-sm font-semibold text-white" @click="collectHouseholdFee(household)">Collect payment</button>
              </div>
            </div>
          </details>
        </div>
      </section>
      <div v-if="groupedOverdueFees.length === 0" class="rounded-2xl border border-emerald-200 bg-emerald-50 px-6 py-12 text-center"><p class="font-semibold text-emerald-900">No overdue fees</p><p class="mt-1 text-sm text-emerald-700">All configured households are up to date.</p></div>
    </div>

    <!-- Legacy flat table retained for compatibility but hidden in the household ledger. -->
    <div class="hidden bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="py-3 pl-6 pr-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Student</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Class</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Fee Type</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Amount</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Due Date</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Days Overdue</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Parent Contact</th>
              <th class="px-3 py-3 text-right text-xs font-semibold text-gray-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-200 bg-white">
            <tr v-for="overdue in overdueFees" :key="overdue.id" class="hover:bg-gray-50 transition-colors">
              <td class="whitespace-nowrap py-4 pl-6 pr-3 text-sm font-medium text-gray-900">{{ overdue.studentName }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ overdue.className }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ overdue.feeType }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm font-medium text-gray-900">₹{{ formatCurrency(overdue.amount) }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ formatDate(overdue.dueDate) }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm">
                <span class="inline-flex rounded-full px-2.5 py-0.5 text-xs font-semibold bg-red-100 text-red-800">
                  {{ overdue.daysOverdue }} days
                </span>
              </td>
              <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ overdue.parentContact || 'Not available' }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-right text-sm">
                <button @click="sendReminder(overdue)" class="text-orange-600 hover:text-orange-900 mr-3 font-medium">WhatsApp</button>
                <button @click="markAsPaid(overdue.id)" class="text-green-600 hover:text-green-900 font-medium">Mark Paid</button>
              </td>
            </tr>
            <tr v-if="overdueFees.length === 0">
              <td colspan="8" class="px-6 py-12 text-center">
                <div class="flex flex-col items-center">
                  <svg class="h-12 w-12 text-green-400 mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <p class="text-sm font-medium text-gray-900">No overdue fees!</p>
                  <p class="text-sm text-gray-500">All students are up to date with their payments.</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { financeService, type Fee } from '@/services/financeService'
import { useToast } from '@/composables/useToast'
import { householdKey, normalizeHouseholdMobile, overdueMonthKey } from '@/utils/financeHouseholds'

const toast = useToast()
const router = useRouter()

interface OverdueFee extends Fee {
  daysOverdue: number
  parentContact: string
}

const loading = ref(false)
const overdueFees = ref<OverdueFee[]>([])

const formatCurrency = (amount: number): string => amount.toLocaleString('en-IN')
const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })
}

const calculateDaysOverdue = (dueDate: string): number => {
  const due = new Date(dueDate)
  const today = new Date()
  return Math.max(0, Math.ceil((today.getTime() - due.getTime()) / (1000 * 60 * 60 * 24)))
}

const normalizePhone = (phone?: string): string => {
  const digits = (phone || '').replace(/\D/g, '')
  if (!digits) return ''
  return digits.length === 10 ? `91${digits}` : digits
}

const displayMobile = (phone?: string) => normalizeHouseholdMobile(phone) || 'Mobile not added'

interface OverdueHousehold {
  key: string
  mobile: string
  parentName: string
  fees: OverdueFee[]
  balance: number
  studentCount: number
}

const groupedOverdueFees = computed(() => {
  const months = new Map<string, Map<string, Omit<OverdueHousehold, 'balance' | 'studentCount'>>>()
  overdueFees.value.forEach(fee => {
    const monthKey = overdueMonthKey(fee.dueDate)
    const householdId = householdKey(fee.studentId, fee.parentMobile || fee.parentContact)
    if (!months.has(monthKey)) months.set(monthKey, new Map())
    const households = months.get(monthKey)!
    const household = households.get(householdId) || { key: householdId, mobile: fee.parentMobile || fee.parentContact || '', parentName: fee.parentName || '', fees: [] }
    household.fees.push(fee)
    households.set(householdId, household)
  })
  return Array.from(months.entries()).sort(([a], [b]) => a.localeCompare(b)).map(([key, households]) => {
    const grouped = Array.from(households.values()).map(household => ({
      ...household,
      balance: household.fees.reduce((sum, fee) => sum + (fee.balanceAmount || fee.amount), 0),
      studentCount: new Set(household.fees.map(fee => fee.studentId)).size
    }))
    return {
      key,
      label: new Date(`${key}-01T00:00:00`).toLocaleDateString('en-IN', { month: 'long', year: 'numeric' }),
      households: grouped,
      balance: grouped.reduce((sum, household) => sum + household.balance, 0)
    }
  })
})

const buildReminderMessage = (overdue: OverdueFee): string => {
  return [
    'Namaste, this is a fee reminder from The Master Mind Coaching Classes.',
    `Student: ${overdue.studentName}`,
    `Class: ${overdue.className || 'Not Assigned'}`,
    `Pending amount: Rs. ${formatCurrency(overdue.balanceAmount || overdue.amount)}`,
    `Due date: ${formatDate(overdue.dueDate)}`,
    'Please complete the payment at your earliest convenience.'
  ].join('\n')
}

const openWhatsAppReminder = (overdue: OverdueFee) => {
  const phone = normalizePhone(overdue.parentContact || overdue.parentMobile)
  if (!phone) {
    toast.error('WhatsApp number missing', 'Add the parent mobile number in the student profile first.')
    return
  }

  const url = `https://wa.me/${phone}?text=${encodeURIComponent(buildReminderMessage(overdue))}`
  window.open(url, '_blank', 'noopener,noreferrer')
}

const sendHouseholdReminder = async (household: OverdueHousehold) => {
  const phone = normalizePhone(household.mobile)
  if (!phone) {
    toast.error('WhatsApp number missing', 'Add the primary parent mobile number first.')
    return
  }
  try {
    await financeService.sendReminders(household.fees.map(fee => fee.id))
    const lines = household.fees.map(fee => `• ${fee.studentName}: Rs. ${formatCurrency(fee.balanceAmount || fee.amount)} due ${formatDate(fee.dueDate)}`)
    const message = ['Namaste, this is a fee reminder from The Master Mind Coaching Classes.', '', ...lines, '', 'Please complete the pending payment at your earliest convenience.'].join('\n')
    window.open(`https://wa.me/${phone}?text=${encodeURIComponent(message)}`, '_blank', 'noopener,noreferrer')
  } catch (error) {
    console.error('Error preparing household reminder:', error)
    toast.error('Failed to prepare reminder', 'Please try again.')
  }
}

const collectHouseholdFee = (household: OverdueHousehold) => {
  const firstFee = household.fees[0]
  if (!firstFee) return
  router.push({ path: '/admin/finance/fee-collection', query: { studentId: String(firstFee.studentId), feeId: String(firstFee.id) } })
}

const loadOverdueFees = async () => {
  loading.value = true
  try {
    const overdueData = await financeService.getOverdueFees()
    overdueFees.value = overdueData.map(fee => ({
      ...fee,
      daysOverdue: calculateDaysOverdue(fee.dueDate),
      parentContact: fee.parentContact || fee.parentMobile || ''
    }))
  } catch (error) {
    console.error('Error loading overdue fees:', error)
    overdueFees.value = []
  } finally {
    loading.value = false
  }
}

const sendAllReminders = async () => {
  loading.value = true
  try {
    const feeIds = overdueFees.value.map(f => f.id)
    await financeService.sendReminders(feeIds)
    toast.success('Reminders logged', 'Use each row WhatsApp action to send the parent-ready message.')
  } catch (error) {
    console.error('Error sending reminders:', error)
    toast.error('Failed to send reminders', 'Please try again.')
  } finally {
    loading.value = false
  }
}

const sendReminder = async (overdue: OverdueFee) => {
  try {
    await financeService.sendReminders([overdue.id])
    openWhatsAppReminder(overdue)
    toast.success('Reminder ready', `Opened WhatsApp reminder for ${overdue.studentName}.`)
  } catch (error) {
    console.error('Error sending reminder:', error)
    toast.error('Failed to send reminder', 'Please try again.')
  }
}

const markAsPaid = async (feeId: number) => {
  const fee = overdueFees.value.find(item => item.id === feeId)
  if (fee) router.push({ path: '/admin/finance/fee-collection', query: { studentId: String(fee.studentId), feeId: String(fee.id) } })
}

onMounted(() => { loadOverdueFees() })
</script>
