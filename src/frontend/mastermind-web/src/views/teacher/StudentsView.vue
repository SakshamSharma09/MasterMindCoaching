<template>
  <div class="px-4 sm:px-6 lg:px-8">
    <div class="sm:flex sm:items-center">
      <div class="sm:flex-auto">
        <h1 class="text-2xl font-semibold text-gray-900">My Students</h1>
        <p class="mt-2 text-sm text-gray-700">
          View and manage students in your classes.
        </p>
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

    <!-- Students Grid -->
    <div class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
      <div
        v-for="student in students"
        :key="student.id"
        class="bg-white overflow-hidden shadow rounded-lg"
      >
        <div class="p-6">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <div class="w-12 h-12 bg-gray-200 rounded-full flex items-center justify-center">
                <span class="text-lg font-medium text-gray-700">
                  {{ student.initials }}
                </span>
              </div>
            </div>
            <div class="ml-4">
              <h3 class="text-lg font-medium text-gray-900">{{ student.name }}</h3>
              <p class="text-sm text-gray-500">Roll No: {{ student.rollNo }}</p>
            </div>
          </div>
          <div class="mt-4">
            <div class="flex items-center justify-between text-sm">
              <span class="text-gray-500">Attendance:</span>
              <span class="font-medium">{{ student.attendance }}%</span>
            </div>
            <div class="flex items-center justify-between text-sm mt-1">
              <span class="text-gray-500">Average Grade:</span>
              <span class="font-medium">{{ student.averageGrade }}</span>
            </div>
          </div>
          <div class="mt-4 flex space-x-2">
            <button class="flex-1 bg-indigo-600 text-white px-3 py-2 rounded-md text-sm font-medium hover:bg-indigo-700">
              View Details
            </button>
            <button class="flex-1 bg-green-600 text-white px-3 py-2 rounded-md text-sm font-medium hover:bg-green-700">
              Add Remark
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Class Summary -->
    <div class="mt-8 bg-white shadow rounded-lg">
      <div class="px-4 py-5 sm:p-6">
        <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">Class Summary</h3>
        <div class="grid grid-cols-1 gap-5 sm:grid-cols-4">
          <div class="text-center">
            <div class="text-2xl font-bold text-gray-900">{{ classSummary.totalStudents }}</div>
            <div class="text-sm text-gray-500">Total Students</div>
          </div>
          <div class="text-center">
            <div class="text-2xl font-bold text-green-600">{{ classSummary.averageAttendance }}%</div>
            <div class="text-sm text-gray-500">Avg Attendance</div>
          </div>
          <div class="text-center">
            <div class="text-2xl font-bold text-blue-600">{{ classSummary.averageGrade }}</div>
            <div class="text-sm text-gray-500">Avg Grade</div>
          </div>
          <div class="text-center">
            <div class="text-2xl font-bold text-yellow-600">{{ classSummary.recentRemarks }}</div>
            <div class="text-sm text-gray-500">Recent Remarks</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

// Sample data - replace with actual API calls
const classes = ref([
  { id: 1, name: 'Class 10A', subject: 'Mathematics', board: 'CBSE', medium: 'English' },
  { id: 2, name: 'Class 9B', subject: 'Mathematics', board: 'CBSE', medium: 'Hindi' },
  { id: 3, name: 'Class 8C', subject: 'Mathematics', board: 'RBSE', medium: 'English' }
])

const selectedClass = ref(1)

const studentsData = ref({
  1: [
    {
      id: 1,
      name: 'John Doe',
      rollNo: '001',
      initials: 'JD',
      attendance: 95,
      averageGrade: 'A-'
    },
    {
      id: 2,
      name: 'Jane Smith',
      rollNo: '002',
      initials: 'JS',
      attendance: 88,
      averageGrade: 'B+'
    },
    {
      id: 3,
      name: 'Bob Johnson',
      rollNo: '003',
      initials: 'BJ',
      attendance: 92,
      averageGrade: 'A'
    },
    {
      id: 4,
      name: 'Alice Brown',
      rollNo: '004',
      initials: 'AB',
      attendance: 85,
      averageGrade: 'B'
    }
  ],
  2: [
    {
      id: 5,
      name: 'Charlie Wilson',
      rollNo: '001',
      initials: 'CW',
      attendance: 90,
      averageGrade: 'A-'
    },
    {
      id: 6,
      name: 'Diana Prince',
      rollNo: '002',
      initials: 'DP',
      attendance: 87,
      averageGrade: 'B+'
    }
  ],
  3: [
    {
      id: 7,
      name: 'Eve Adams',
      rollNo: '001',
      initials: 'EA',
      attendance: 93,
      averageGrade: 'A'
    },
    {
      id: 8,
      name: 'Frank Miller',
      rollNo: '002',
      initials: 'FM',
      attendance: 89,
      averageGrade: 'B+'
    }
  ]
})

const classSummaryData = ref({
  1: {
    totalStudents: 4,
    averageAttendance: 90,
    averageGrade: 'A-',
    recentRemarks: 3
  },
  2: {
    totalStudents: 2,
    averageAttendance: 88,
    averageGrade: 'B+',
    recentRemarks: 1
  },
  3: {
    totalStudents: 2,
    averageAttendance: 91,
    averageGrade: 'A-',
    recentRemarks: 2
  }
})

const students = computed(() => studentsData.value[selectedClass.value as keyof typeof studentsData.value] || [])
const classSummary = computed(() => classSummaryData.value[selectedClass.value as keyof typeof classSummaryData.value])
</script>

<style scoped>
/* Additional styles if needed */
</style>