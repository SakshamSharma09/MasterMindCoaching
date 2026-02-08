<template>
  <div class="px-4 sm:px-6 lg:px-8">
    <div class="sm:flex sm:items-center">
      <div class="sm:flex-auto">
        <h1 class="text-2xl font-semibold text-gray-900">Attendance Records</h1>
        <p class="mt-2 text-sm text-gray-700">
          View your child's attendance history and statistics.
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

    <!-- Attendance Summary -->
    <div class="grid grid-cols-1 gap-5 sm:grid-cols-3 mb-8">
      <div class="bg-white overflow-hidden shadow rounded-lg">
        <div class="p-5">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <svg class="h-6 w-6 text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
              </svg>
            </div>
            <div class="ml-5 w-0 flex-1">
              <dl>
                <dt class="text-sm font-medium text-gray-500 truncate">Present Days</dt>
                <dd class="text-lg font-medium text-gray-900">{{ attendanceStats.present }}</dd>
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
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
              </svg>
            </div>
            <div class="ml-5 w-0 flex-1">
              <dl>
                <dt class="text-sm font-medium text-gray-500 truncate">Absent Days</dt>
                <dd class="text-lg font-medium text-gray-900">{{ attendanceStats.absent }}</dd>
              </dl>
            </div>
          </div>
        </div>
      </div>

      <div class="bg-white overflow-hidden shadow rounded-lg">
        <div class="p-5">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <svg class="h-6 w-6 text-yellow-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
            </div>
            <div class="ml-5 w-0 flex-1">
              <dl>
                <dt class="text-sm font-medium text-gray-500 truncate">Late Days</dt>
                <dd class="text-lg font-medium text-gray-900">{{ attendanceStats.late }}</dd>
              </dl>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Attendance Records Table -->
    <div class="bg-white shadow overflow-hidden sm:rounded-md">
      <ul role="list" class="divide-y divide-gray-200">
        <li v-for="record in attendanceRecords" :key="record.id">
          <div class="px-4 py-4 sm:px-6">
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <div class="flex-shrink-0">
                  <div
                    :class="[
                      record.status === 'Present' ? 'bg-green-100' :
                      record.status === 'Absent' ? 'bg-red-100' :
                      'bg-yellow-100'
                    ]"
                    class="w-8 h-8 rounded-full flex items-center justify-center"
                  >
                    <svg
                      :class="[
                        record.status === 'Present' ? 'text-green-600' :
                        record.status === 'Absent' ? 'text-red-600' :
                        'text-yellow-600'
                      ]"
                      class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"
                    >
                      <path v-if="record.status === 'Present'" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                      <path v-else-if="record.status === 'Absent'" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                      <path v-else stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                    </svg>
                  </div>
                </div>
                <div class="ml-4">
                  <div class="text-sm font-medium text-gray-900">{{ record.date }}</div>
                  <div class="text-sm text-gray-500">{{ record.subject }}</div>
                </div>
              </div>
              <div class="flex items-center">
                <span
                  :class="[
                    record.status === 'Present' ? 'bg-green-100 text-green-800' :
                    record.status === 'Absent' ? 'bg-red-100 text-red-800' :
                    'bg-yellow-100 text-yellow-800'
                  ]"
                  class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                >
                  {{ record.status }}
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

const attendanceData = ref({
  1: {
    present: 18,
    absent: 2,
    late: 1,
    records: [
      { id: 1, date: '2024-01-15', subject: 'Mathematics', status: 'Present' },
      { id: 2, date: '2024-01-15', subject: 'Science', status: 'Present' },
      { id: 3, date: '2024-01-14', subject: 'English', status: 'Late' },
      { id: 4, date: '2024-01-14', subject: 'History', status: 'Present' },
      { id: 5, date: '2024-01-13', subject: 'Mathematics', status: 'Absent' }
    ]
  },
  2: {
    present: 15,
    absent: 1,
    late: 0,
    records: [
      { id: 1, date: '2024-01-15', subject: 'Science', status: 'Present' },
      { id: 2, date: '2024-01-15', subject: 'Mathematics', status: 'Present' },
      { id: 3, date: '2024-01-14', subject: 'English', status: 'Present' },
      { id: 4, date: '2024-01-13', subject: 'Science', status: 'Absent' }
    ]
  }
})

const attendanceStats = computed(() => attendanceData.value[selectedChild.value as keyof typeof attendanceData.value])
const attendanceRecords = computed(() => attendanceStats.value.records)
</script>

<style scoped>
/* Additional styles if needed */
</style>