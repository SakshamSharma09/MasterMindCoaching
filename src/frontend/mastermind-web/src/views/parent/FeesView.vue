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

    <div class="mb-6">
      <label for="child-select" class="block text-sm font-medium text-gray-700">Select Child</label>
      <select
        id="child-select"
        v-model.number="selectedChild"
        class="mt-1 block w-full rounded-md border-gray-300 py-2 pl-3 pr-10 text-base focus:border-blue-500 focus:outline-none focus:ring-blue-500 sm:text-sm"
      >
        <option v-for="child in children" :key="child.id" :value="child.id">
          {{ child.firstName }} {{ child.lastName }} - {{ child.className }}
        </option>
      </select>
    </div>

    <div v-if="error" class="mb-4 rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
      {{ error }}
    </div>

    <div class="mb-8 grid grid-cols-1 gap-5 sm:grid-cols-3">
      <div class="overflow-hidden rounded-lg bg-white shadow">
        <div class="p-5">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <svg class="h-6 w-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1"></path>
              </svg>
            </div>
            <div class="ml-5 w-0 flex-1">
              <dl>
                <dt class="truncate text-sm font-medium text-gray-500">Total Fees</dt>
                <dd class="text-lg font-medium text-gray-900">₹{{ feeStats.totalFees }}</dd>
              </dl>
            </div>
          </div>
        </div>
      </div>

      <div class="overflow-hidden rounded-lg bg-white shadow">
        <div class="p-5">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <svg class="h-6 w-6 text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
            </div>
            <div class="ml-5 w-0 flex-1">
              <dl>
                <dt class="truncate text-sm font-medium text-gray-500">Paid</dt>
                <dd class="text-lg font-medium text-gray-900">₹{{ feeStats.paidFees }}</dd>
              </dl>
            </div>
          </div>
        </div>
      </div>

      <div class="overflow-hidden rounded-lg bg-white shadow">
        <div class="p-5">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <svg class="h-6 w-6 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"></path>
              </svg>
            </div>
            <div class="ml-5 w-0 flex-1">
              <dl>
                <dt class="truncate text-sm font-medium text-gray-500">Pending</dt>
                <dd class="text-lg font-medium text-gray-900">₹{{ feeStats.pendingFees }}</dd>
              </dl>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="overflow-hidden rounded-md bg-white shadow">
      <div class="px-4 py-5 sm:px-6">
        <h3 class="text-lg font-medium leading-6 text-gray-900">Payment History</h3>
      </div>
      <ul role="list" class="divide-y divide-gray-200">
        <li v-for="payment in paymentHistory" :key="`${payment.date}-${payment.amount}-${payment.method}`">
          <div class="px-4 py-4 sm:px-6">
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <div class="flex-shrink-0">
                  <div
                    :class="[payment.status.toLowerCase() === 'completed' ? 'bg-green-100' : 'bg-yellow-100']"
                    class="flex h-8 w-8 items-center justify-center rounded-full"
                  >
                    <svg
                      :class="[payment.status.toLowerCase() === 'completed' ? 'text-green-600' : 'text-yellow-600']"
                      class="h-4 w-4"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path v-if="payment.status.toLowerCase() === 'completed'" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                      <path v-else stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                    </svg>
                  </div>
                </div>
                <div class="ml-4">
                  <div class="text-sm font-medium text-gray-900">{{ payment.method }}</div>
                  <div class="text-sm text-gray-500">{{ payment.date }}</div>
                </div>
              </div>
              <div class="flex items-center">
                <span class="text-sm font-medium text-gray-900">₹{{ payment.amount }}</span>
                <span
                  :class="[payment.status.toLowerCase() === 'completed' ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800']"
                  class="ml-2 inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium"
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
import { computed, onMounted, ref, watch } from 'vue'
import { parentService, type ChildFees, type ParentChild } from '@/services/parentService'

const children = ref<ParentChild[]>([])
const selectedChild = ref<number | null>(null)
const feeStats = ref<ChildFees>({
  totalFees: 0,
  paidFees: 0,
  pendingFees: 0,
  nextDueDate: '',
  paymentHistory: []
})
const error = ref('')

const paymentHistory = computed(() => feeStats.value.paymentHistory || [])

const loadFees = async () => {
  if (!selectedChild.value) return
  error.value = ''
  try {
    feeStats.value = await parentService.getChildFees(selectedChild.value)
  } catch (err: any) {
    error.value = err?.response?.data?.message || err?.message || 'Failed to load fee details'
    feeStats.value = { totalFees: 0, paidFees: 0, pendingFees: 0, nextDueDate: '', paymentHistory: [] }
  }
}

const loadChildren = async () => {
  try {
    children.value = await parentService.getChildren()
    selectedChild.value = children.value.length > 0 ? children.value[0].id : null
    await loadFees()
  } catch (err: any) {
    error.value = err?.response?.data?.message || err?.message || 'Failed to load children'
  }
}

watch(selectedChild, async (value, oldValue) => {
  if (value && value !== oldValue) {
    await loadFees()
  }
})

onMounted(async () => {
  await loadChildren()
})
</script>
