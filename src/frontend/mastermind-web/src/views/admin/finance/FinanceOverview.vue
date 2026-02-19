<template>
  <div class="space-y-6">
    <!-- Student Stats -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
        <div class="flex items-center gap-3">
          <div class="p-2 bg-blue-100 rounded-lg">
            <svg class="h-5 w-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z" />
            </svg>
          </div>
          <div>
            <p class="text-sm font-medium text-gray-500">Total Students</p>
            <p class="text-xl font-bold text-gray-900">{{ financialSummary.totalStudents }}</p>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
        <div class="flex items-center gap-3">
          <div class="p-2 bg-green-100 rounded-lg">
            <svg class="h-5 w-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div>
            <p class="text-sm font-medium text-gray-500">Paid Students</p>
            <p class="text-xl font-bold text-gray-900">{{ financialSummary.paidStudents }}</p>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
        <div class="flex items-center gap-3">
          <div class="p-2 bg-red-100 rounded-lg">
            <svg class="h-5 w-5 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div>
            <p class="text-sm font-medium text-gray-500">Overdue Students</p>
            <p class="text-xl font-bold text-gray-900">{{ financialSummary.overdueStudents }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Recent Transactions -->
    <div class="bg-white rounded-xl shadow-sm border border-gray-100">
      <div class="px-6 py-4 border-b border-gray-100">
        <div class="flex items-center justify-between">
          <div>
            <h3 class="text-lg font-semibold text-gray-900">Recent Transactions</h3>
            <p class="mt-1 text-sm text-gray-500">Latest financial transactions from students and expenses.</p>
          </div>
          <button
            @click="refreshData"
            :disabled="loading"
            class="inline-flex items-center gap-2 px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 transition-colors"
          >
            <svg :class="['h-4 w-4', loading ? 'animate-spin' : '']" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            Refresh
          </button>
        </div>
      </div>
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="py-3 pl-6 pr-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Type</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Student/Description</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Amount</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Date</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Status</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-200 bg-white">
            <tr v-for="tx in recentTransactions" :key="tx.id" class="hover:bg-gray-50 transition-colors">
              <td class="whitespace-nowrap py-4 pl-6 pr-3 text-sm">
                <span
                  :class="[
                    tx.type === 'fee' ? 'bg-green-100 text-green-800' :
                    tx.type === 'expense' ? 'bg-red-100 text-red-800' :
                    'bg-blue-100 text-blue-800'
                  ]"
                  class="inline-flex rounded-full px-2.5 py-0.5 text-xs font-semibold"
                >
                  {{ tx.type }}
                </span>
              </td>
              <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-700">{{ tx.studentName || tx.description }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm font-medium text-gray-900">₹{{ formatCurrency(tx.amount) }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ formatDate(tx.date) }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm">
                <span
                  :class="[
                    tx.status === 'Completed' ? 'bg-green-100 text-green-800' :
                    tx.status === 'Pending' ? 'bg-yellow-100 text-yellow-800' :
                    tx.status === 'Overdue' ? 'bg-red-100 text-red-800' :
                    'bg-gray-100 text-gray-800'
                  ]"
                  class="inline-flex rounded-full px-2.5 py-0.5 text-xs font-semibold"
                >
                  {{ tx.status }}
                </span>
              </td>
            </tr>
            <tr v-if="recentTransactions.length === 0">
              <td colspan="5" class="px-6 py-12 text-center text-sm text-gray-500">
                No recent transactions found.
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
import { financeService, type FinancialSummary, type Fee, type Expense } from '@/services/financeService'

interface Transaction {
  id: number
  type: 'fee' | 'expense' | 'payment'
  studentName?: string
  description?: string
  amount: number
  date: string
  status: 'Completed' | 'Pending' | 'Overdue'
}

const loading = ref(false)
const financialSummary = ref<FinancialSummary>({
  totalRevenue: 0, pendingPayments: 0, expenses: 0, netProfit: 0,
  totalStudents: 0, paidStudents: 0, pendingStudents: 0, overdueStudents: 0
})
const recentTransactions = ref<Transaction[]>([])

const formatCurrency = (amount: number): string => amount.toLocaleString('en-IN')

const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })
}

const loadData = async () => {
  loading.value = true
  try {
    const [summary, fees, expenses] = await Promise.allSettled([
      financeService.getFinancialSummary(),
      financeService.getFees(),
      financeService.getExpenses()
    ])

    if (summary.status === 'fulfilled') financialSummary.value = summary.value
    
    const feeTransactions: Transaction[] = (fees.status === 'fulfilled' ? fees.value : []).map((fee: Fee) => ({
      id: fee.id, type: 'fee' as const, studentName: fee.studentName, amount: fee.amount,
      date: fee.dueDate, status: fee.status === 'Paid' ? 'Completed' as const : fee.status as 'Pending' | 'Overdue'
    }))

    const expenseTransactions: Transaction[] = (expenses.status === 'fulfilled' ? expenses.value : []).map((exp: Expense) => ({
      id: exp.id, type: 'expense' as const, description: exp.description, amount: exp.amount,
      date: exp.date, status: 'Completed' as const
    }))

    recentTransactions.value = [...feeTransactions, ...expenseTransactions]
      .sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
      .slice(0, 10)
  } catch (error) {
    console.error('Error loading overview data:', error)
  } finally {
    loading.value = false
  }
}

const refreshData = () => loadData()

onMounted(() => { loadData() })
</script>
