<template>
  <div class="px-4 sm:px-6 lg:px-8">
    <div class="sm:flex sm:items-center">
      <div class="sm:flex-auto">
        <h1 class="text-2xl font-semibold text-gray-900">Mark Attendance</h1>
        <p class="mt-2 text-sm text-gray-700">
          Mark attendance for your classes and view attendance records.
        </p>
      </div>
      <div class="mt-4 sm:mt-0 sm:ml-16 sm:flex-none">
        <button
          type="button"
          class="inline-flex items-center justify-center rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
        >
          Save Attendance
        </button>
      </div>
    </div>

    <!-- Class Selection -->
    <div class="mb-6">
      <label for="class-select" class="block text-sm font-medium text-gray-700">Select Class</label>
      <select
        id="class-select"
        v-model="selectedClass"
        class="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-green-500 focus:border-green-500 sm:text-sm rounded-md"
      >
        <option v-for="classItem in classes" :key="classItem.id" :value="classItem.id">
          {{ classItem.name }} - {{ classItem.board }}-{{ classItem.medium }}
        </option>
      </select>
    </div>

    <!-- Attendance Table -->
    <div class="bg-white shadow overflow-hidden sm:rounded-md">
      <ul role="list" class="divide-y divide-gray-200">
        <li v-for="student in students" :key="student.id">
          <div class="px-4 py-4 sm:px-6">
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <div class="flex-shrink-0">
                  <div class="w-10 h-10 bg-gray-200 rounded-full flex items-center justify-center">
                    <span class="text-sm font-medium text-gray-700">
                      {{ student.initials }}
                    </span>
                  </div>
                </div>
                <div class="ml-4">
                  <div class="text-sm font-medium text-gray-900">{{ student.name }}</div>
                  <div class="text-sm text-gray-500">Roll No: {{ student.rollNo }}</div>
                </div>
              </div>
              <div class="flex items-center space-x-4">
                <div class="flex items-center space-x-2">
                  <label class="inline-flex items-center">
                    <input
                      type="radio"
                      :name="`attendance-${student.id}`"
                      value="present"
                      v-model="student.status"
                      class="form-radio h-4 w-4 text-green-600"
                    >
                    <span class="ml-2 text-sm text-gray-700">Present</span>
                  </label>
                  <label class="inline-flex items-center">
                    <input
                      type="radio"
                      :name="`attendance-${student.id}`"
                      value="absent"
                      v-model="student.status"
                      class="form-radio h-4 w-4 text-red-600"
                    >
                    <span class="ml-2 text-sm text-gray-700">Absent</span>
                  </label>
                  <label class="inline-flex items-center">
                    <input
                      type="radio"
                      :name="`attendance-${student.id}`"
                      value="late"
                      v-model="student.status"
                      class="form-radio h-4 w-4 text-yellow-600"
                    >
                    <span class="ml-2 text-sm text-gray-700">Late</span>
                  </label>
                </div>
              </div>
            </div>
          </div>
        </li>
      </ul>
    </div>

    <!-- Attendance Summary -->
    <div class="mt-8 bg-white shadow rounded-lg">
      <div class="px-4 py-5 sm:p-6">
        <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">Attendance Summary</h3>
        <div class="grid grid-cols-1 gap-5 sm:grid-cols-3">
          <div class="text-center">
            <div class="text-2xl font-bold text-green-600">{{ attendanceSummary.present }}</div>
            <div class="text-sm text-gray-500">Present</div>
          </div>
          <div class="text-center">
            <div class="text-2xl font-bold text-red-600">{{ attendanceSummary.absent }}</div>
            <div class="text-sm text-gray-500">Absent</div>
          </div>
          <div class="text-center">
            <div class="text-2xl font-bold text-yellow-600">{{ attendanceSummary.late }}</div>
            <div class="text-sm text-gray-500">Late</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'

// Sample data - replace with actual API calls
const classes = ref([
  { id: 1, name: 'Class 10A', subject: 'Mathematics', board: 'CBSE', medium: 'English' },
  { id: 2, name: 'Class 9B', subject: 'Mathematics', board: 'CBSE', medium: 'Hindi' },
  { id: 3, name: 'Class 8C', subject: 'Mathematics', board: 'RBSE', medium: 'English' }
])

const selectedClass = ref(1)

const studentsData = ref({
  1: [
    { id: 1, name: 'John Doe', rollNo: '001', initials: 'JD', status: 'present' },
    { id: 2, name: 'Jane Smith', rollNo: '002', initials: 'JS', status: 'present' },
    { id: 3, name: 'Bob Johnson', rollNo: '003', initials: 'BJ', status: 'absent' },
    { id: 4, name: 'Alice Brown', rollNo: '004', initials: 'AB', status: 'late' }
  ],
  2: [
    { id: 5, name: 'Charlie Wilson', rollNo: '001', initials: 'CW', status: 'present' },
    { id: 6, name: 'Diana Prince', rollNo: '002', initials: 'DP', status: 'present' },
    { id: 7, name: 'Eve Adams', rollNo: '003', initials: 'EA', status: 'present' }
  ],
  3: [
    { id: 8, name: 'Frank Miller', rollNo: '001', initials: 'FM', status: 'present' },
    { id: 9, name: 'Grace Lee', rollNo: '002', initials: 'GL', status: 'absent' }
  ]
})

const students = computed(() => studentsData.value[selectedClass.value as keyof typeof studentsData.value] || [])

const attendanceSummary = computed(() => {
  const present = students.value.filter(s => s.status === 'present').length
  const absent = students.value.filter(s => s.status === 'absent').length
  const late = students.value.filter(s => s.status === 'late').length
  return { present, absent, late }
})

// Reset attendance when class changes
watch(selectedClass, () => {
  // In a real app, you might want to load saved attendance data
})
</script>

<style scoped>
/* Additional styles if needed */
</style>