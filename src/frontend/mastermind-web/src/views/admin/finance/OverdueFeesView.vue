<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-lg font-semibold text-gray-900">Overdue Fees</h2>
        <p class="mt-1 text-sm text-gray-500">Students with overdue fee payments. Follow up for collection.</p>
      </div>
      <button
        @click="sendAllReminders"
        :disabled="loading || overdueFees.length === 0"
        class="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-orange-600 rounded-lg shadow-sm hover:bg-orange-700 focus:outline-none focus:ring-2 focus:ring-orange-500 focus:ring-offset-2 disabled:opacity-50 transition-colors"
      >
        <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 4.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
        </svg>
        Send All Reminders
      </button>
    </div>

    <!-- Overdue Fees Table -->
    <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
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
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Contact</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Actions</th>
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
              <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ overdue.parentContact }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm">
                <button @click="sendReminder(overdue)" class="text-orange-600 hover:text-orange-900 mr-3 font-medium">Remind</button>
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
import { ref, onMounted } from 'vue'
import { financeService, type Fee } from '@/services/financeService'
import { useToast } from '@/composables/useToast'

const toast = useToast()

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

const loadOverdueFees = async () => {
  loading.value = true
  try {
    const overdueData = await financeService.getOverdueFees()
    overdueFees.value = overdueData.map(fee => ({
      ...fee,
      daysOverdue: calculateDaysOverdue(fee.dueDate),
      parentContact: '+91 9876543210'
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
    toast.success('Reminders sent', 'All overdue students have been notified.')
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
    toast.success('Reminder sent', `Notified ${overdue.studentName}'s parents.`)
  } catch (error) {
    console.error('Error sending reminder:', error)
    toast.error('Failed to send reminder', 'Please try again.')
  }
}

const markAsPaid = async (feeId: number) => {
  if (!confirm('Mark this fee as paid?')) return
  try {
    await financeService.markFeeAsPaid(feeId)
    await loadOverdueFees()
    toast.success('Fee updated', 'Fee has been marked as paid.')
  } catch (error) {
    console.error('Error marking fee as paid:', error)
    toast.error('Failed to update fee', 'Please try again.')
  }
}

onMounted(() => { loadOverdueFees() })
</script>
