<template>
  <div class="px-4 sm:px-6 lg:px-8">
    <div class="sm:flex sm:items-center">
      <div class="sm:flex-auto">
        <h1 class="text-2xl font-semibold text-gray-900">Fee Management</h1>
        <p class="mt-2 text-sm text-gray-700">
          View fee structure, payment history, and outstanding balances.
        </p>
      </div>
    </div>

    <!-- Child Selection -->
    <div class="mb-6">
      <label for="child-select" class="block text-sm font-medium text-gray-700">Select Child</label>
      <select
        id="child-select"
        v-model="selectedChild"
        class="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm rounded-md"
      >
        <option v-for="child in children" :key="child.id" :value="child.id">
          {{ child.name }} - {{ child.className }}
        </option>
      </select>
    </div>

    <!-- Fee Summary -->
    <div class="grid grid-cols-1 gap-5 sm:grid-cols-3 mb-8">
      <div class="bg-white overflow-hidden shadow rounded-lg">
        <div class="p-5">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <svg class="h-6 w-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1"></path>
              </svg>
            </div>
            <div class="ml-5 w-0 flex-1">
              <dl>
                <dt class="text-sm font-medium text-gray-500 truncate">Total Fees</dt>
                <dd class="text-lg font-medium text-gray-900">₹{{ feeStats.totalFees }}</dd>
              </dl>
            </div>
          </div>
        </div>
      </div>

      <div class="bg-white overflow-hidden shadow rounded-lg">
        <div class="p-5">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <svg class="h-6 w-6 text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
            </div>
            <div class="ml-5 w-0 flex-1">
              <dl>
                <dt class="text-sm font-medium text-gray-500 truncate">Paid</dt>
                <dd class="text-lg font-medium text-gray-900">₹{{ feeStats.paid }}</dd>
              </dl>
            </div>
          </div>
        </div>
      </div>

      <div class="bg-white overflow-hidden shadow rounded-lg">
        <div class="p-5">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <svg class="h-6 w-6 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"></path>
              </svg>
            </div>
            <div class="ml-5 w-0 flex-1">
              <dl>
                <dt class="text-sm font-medium text-gray-500 truncate">Pending</dt>
                <dd class="text-lg font-medium text-gray-900">₹{{ feeStats.pending }}</dd>
              </dl>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Payment History -->
    <div class="bg-white shadow overflow-hidden sm:rounded-md">
      <div class="px-4 py-5 sm:px-6">
        <h3 class="text-lg leading-6 font-medium text-gray-900">Payment History</h3>
      </div>
      <ul role="list" class="divide-y divide-gray-200">
        <li v-for="payment in paymentHistory" :key="payment.id">
          <div class="px-4 py-4 sm:px-6">
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <div class="flex-shrink-0">
                  <div
                    :class="[
                      payment.status === 'Completed' ? 'bg-green-100' : 'bg-yellow-100'
                    ]"
                    class="w-8 h-8 rounded-full flex items-center justify-center"
                  >
                    <svg
                      :class="[
                        payment.status === 'Completed' ? 'text-green-600' : 'text-yellow-600'
                      ]"
                      class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"
                    >
                      <path v-if="payment.status === 'Completed'" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                      <path v-else stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                    </svg>
                  </div>
                </div>
                <div class="ml-4">
                  <div class="text-sm font-medium text-gray-900">{{ payment.description }}</div>
                  <div class="text-sm text-gray-500">{{ payment.date }}</div>
                </div>
              </div>
              <div class="flex items-center">
                <span class="text-sm font-medium text-gray-900">₹{{ payment.amount }}</span>
                <span
                  :class="[
                    payment.status === 'Completed' ? 'bg-green-100 text-green-800' :
                    'bg-yellow-100 text-yellow-800'
                  ]"
                  class="ml-2 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                >
                  {{ payment.status }}
                </span>
              </div>
            </div>
          </div>
        </li>
      </ul>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

// Sample data - replace with actual API calls
const children = ref([
  { id: 1, name: 'John Doe', className: 'Class 10A' },
  { id: 2, name: 'Jane Doe', className: 'Class 8B' }
])

const selectedChild = ref(1)

const feeData = ref({
  1: {
    totalFees: 25000,
    paid: 20000,
    pending: 5000,
    history: [
      { id: 1, description: 'January Tuition Fee', amount: '5000', date: '2024-01-15', status: 'Completed' },
      { id: 2, description: 'December Tuition Fee', amount: '5000', date: '2024-01-01', status: 'Completed' },
      { id: 3, description: 'November Tuition Fee', amount: '5000', date: '2023-12-01', status: 'Completed' },
      { id: 4, description: 'October Tuition Fee', amount: '5000', date: '2023-11-01', status: 'Pending' }
    ]
  },
  2: {
    totalFees: 20000,
    paid: 15000,
    pending: 5000,
    history: [
      { id: 1, description: 'January Tuition Fee', amount: '4000', date: '2024-01-15', status: 'Completed' },
      { id: 2, description: 'December Tuition Fee', amount: '4000', date: '2024-01-01', status: 'Completed' },
      { id: 3, description: 'November Tuition Fee', amount: '4000', date: '2023-12-01', status: 'Pending' }
    ]
  }
})

const feeStats = computed(() => feeData.value[selectedChild.value as keyof typeof feeData.value])
const paymentHistory = computed(() => feeStats.value.history)
</script>

<style scoped>
/* Additional styles if needed */
</style>