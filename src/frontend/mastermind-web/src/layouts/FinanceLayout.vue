<template>
  <div class="finance-layout">
    <!-- Finance Header -->
    <div class="mb-6">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <div class="flex items-center gap-2 text-sm text-gray-500 mb-1">
            <router-link to="/admin" class="hover:text-indigo-600 transition-colors">Admin</router-link>
            <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
            </svg>
            <span class="text-gray-700 font-medium">Finance</span>
            <template v-if="currentSectionName">
              <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
              <span class="text-indigo-600 font-medium">{{ currentSectionName }}</span>
            </template>
          </div>
          <h1 class="text-2xl font-bold text-gray-900">Finance Management</h1>
          <p class="mt-1 text-sm text-gray-500">Manage fees, payments, expenses, and financial reports.</p>
        </div>
        <div class="flex items-center gap-3">
          <button
            @click="refreshSummary"
            :disabled="loadingSummary"
            class="inline-flex items-center gap-2 px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:opacity-50 transition-all duration-200"
          >
            <svg :class="['h-4 w-4', loadingSummary ? 'animate-spin' : '']" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            Refresh
          </button>
        </div>
      </div>
    </div>

    <!-- Financial Summary Bar -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 hover:shadow-md transition-shadow duration-300">
        <div class="flex items-center gap-3">
          <div class="flex-shrink-0 p-2 bg-green-100 rounded-lg">
            <svg class="h-5 w-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
            </svg>
          </div>
          <div class="min-w-0">
            <p class="text-xs font-medium text-gray-500 truncate">Total Revenue</p>
            <p class="text-lg font-bold text-gray-900">₹{{ formatCurrency(summary.totalRevenue) }}</p>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 hover:shadow-md transition-shadow duration-300">
        <div class="flex items-center gap-3">
          <div class="flex-shrink-0 p-2 bg-yellow-100 rounded-lg">
            <svg class="h-5 w-5 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div class="min-w-0">
            <p class="text-xs font-medium text-gray-500 truncate">Pending</p>
            <p class="text-lg font-bold text-yellow-600">₹{{ formatCurrency(summary.pendingPayments) }}</p>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 hover:shadow-md transition-shadow duration-300">
        <div class="flex items-center gap-3">
          <div class="flex-shrink-0 p-2 bg-red-100 rounded-lg">
            <svg class="h-5 w-5 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 14h.01M12 14h.01M15 11h.01M12 11h.01M9 11h.01M7 21h10a2 2 0 002-2V5a2 2 0 00-2-2H7a2 2 0 00-2 2v14a2 2 0 002 2z" />
            </svg>
          </div>
          <div class="min-w-0">
            <p class="text-xs font-medium text-gray-500 truncate">Expenses</p>
            <p class="text-lg font-bold text-red-600">₹{{ formatCurrency(summary.expenses) }}</p>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 hover:shadow-md transition-shadow duration-300">
        <div class="flex items-center gap-3">
          <div class="flex-shrink-0 p-2 bg-indigo-100 rounded-lg">
            <svg class="h-5 w-5 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" />
            </svg>
          </div>
          <div class="min-w-0">
            <p class="text-xs font-medium text-gray-500 truncate">Net Profit</p>
            <p class="text-lg font-bold text-indigo-600">₹{{ formatCurrency(summary.netProfit) }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Tab Navigation -->
    <div class="bg-white rounded-xl shadow-sm border border-gray-100 mb-6">
      <nav class="flex overflow-x-auto scrollbar-hide" aria-label="Finance sections">
        <router-link
          v-for="tab in tabs"
          :key="tab.route"
          :to="tab.route"
          :class="[
            isActiveRoute(tab.route)
              ? 'border-indigo-500 text-indigo-600 bg-indigo-50/50'
              : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 hover:bg-gray-50/50',
            'group flex items-center gap-2 px-5 py-3.5 border-b-2 text-sm font-medium whitespace-nowrap transition-all duration-200'
          ]"
        >
          <component
            :is="tab.icon"
            :class="[
              isActiveRoute(tab.route) ? 'text-indigo-500' : 'text-gray-400 group-hover:text-gray-500',
              'h-4 w-4 transition-colors duration-200'
            ]"
          />
          {{ tab.name }}
          <span
            v-if="tab.badge"
            :class="[
              isActiveRoute(tab.route) ? 'bg-indigo-100 text-indigo-700' : 'bg-gray-100 text-gray-600',
              'ml-1 px-2 py-0.5 text-xs font-medium rounded-full transition-colors duration-200'
            ]"
          >
            {{ tab.badge }}
          </span>
        </router-link>
      </nav>
    </div>

    <!-- Child View Content -->
    <div class="animate-fade-in">
      <router-view />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, h } from 'vue'
import { useRoute } from 'vue-router'
import { financeService, type FinancialSummary } from '@/services/financeService'

const route = useRoute()

const loadingSummary = ref(false)
const overdueFeeCount = ref(0)

const summary = ref<FinancialSummary>({
  totalRevenue: 0,
  pendingPayments: 0,
  expenses: 0,
  netProfit: 0,
  totalStudents: 0,
  paidStudents: 0,
  pendingStudents: 0,
  overdueStudents: 0
})

// Icon components
const ChartBarIcon = () => h('svg', { class: 'h-4 w-4', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z' })
])

const CurrencyIcon = () => h('svg', { class: 'h-4 w-4', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1' })
])

const CreditCardIcon = () => h('svg', { class: 'h-4 w-4', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z' })
])

const ReceiptIcon = () => h('svg', { class: 'h-4 w-4', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 14h.01M12 14h.01M15 11h.01M12 11h.01M9 11h.01M7 21h10a2 2 0 002-2V5a2 2 0 00-2-2H7a2 2 0 00-2 2v14a2 2 0 002 2z' })
])

const ExclamationIcon = () => h('svg', { class: 'h-4 w-4', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z' })
])

const DocumentIcon = () => h('svg', { class: 'h-4 w-4', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z' })
])

const tabs = computed(() => [
  { name: 'Overview', route: '/admin/finance', icon: ChartBarIcon, badge: null },
  { name: 'Fees Management', route: '/admin/finance/fees', icon: CurrencyIcon, badge: null },
  { name: 'Fee Collection', route: '/admin/finance/fee-collection', icon: CreditCardIcon, badge: null },
  { name: 'Expenses', route: '/admin/finance/expenses', icon: ReceiptIcon, badge: null },
  { name: 'Overdue Fees', route: '/admin/finance/overdue', icon: ExclamationIcon, badge: overdueFeeCount.value > 0 ? overdueFeeCount.value.toString() : null },
  { name: 'Reports', route: '/admin/finance/reports', icon: DocumentIcon, badge: null }
])

const currentSectionName = computed(() => {
  const tab = tabs.value.find(t => isActiveRoute(t.route))
  if (tab && tab.route !== '/admin/finance') return tab.name
  return ''
})

const isActiveRoute = (tabRoute: string) => {
  if (tabRoute === '/admin/finance') {
    return route.path === '/admin/finance' || route.path === '/admin/finance/'
  }
  return route.path.startsWith(tabRoute)
}

const formatCurrency = (amount: number): string => {
  return amount.toLocaleString('en-IN')
}

const loadSummary = async () => {
  loadingSummary.value = true
  try {
    summary.value = await financeService.getFinancialSummary()
  } catch (error) {
    console.error('Error loading financial summary:', error)
  } finally {
    loadingSummary.value = false
  }
}

const loadOverdueCount = async () => {
  try {
    const overdueData = await financeService.getOverdueFees()
    overdueFeeCount.value = overdueData.length
  } catch (error) {
    console.error('Error loading overdue count:', error)
  }
}

const refreshSummary = async () => {
  await Promise.all([loadSummary(), loadOverdueCount()])
}

onMounted(() => {
  refreshSummary()
})


</script>

<style scoped>
.scrollbar-hide {
  -ms-overflow-style: none;
  scrollbar-width: none;
}
.scrollbar-hide::-webkit-scrollbar {
  display: none;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: translateY(0); }
}

.animate-fade-in {
  animation: fadeIn 0.3s ease-out;
}
</style>
