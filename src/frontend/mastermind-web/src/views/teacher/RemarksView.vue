<template>
  <div class="px-4 sm:px-6 lg:px-8">
    <div class="sm:flex sm:items-center">
      <div class="sm:flex-auto">
        <h1 class="text-2xl font-semibold text-gray-900">Student Remarks</h1>
        <p class="mt-2 text-sm text-gray-700">
          Add remarks and feedback for your students.
        </p>
      </div>
      <div class="mt-4 sm:mt-0 sm:ml-16 sm:flex-none">
        <button
          type="button"
          class="inline-flex items-center justify-center rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
        >
          Add New Remark
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
          {{ classItem.name }} - {{ classItem.subject }}
        </option>
      </select>
    </div>

    <!-- Remarks List -->
    <div class="bg-white shadow overflow-hidden sm:rounded-md">
      <ul role="list" class="divide-y divide-gray-200">
        <li v-for="remark in remarks" :key="remark.id">
          <div class="px-4 py-4 sm:px-6">
            <div class="flex items-start justify-between">
              <div class="flex items-start">
                <div class="flex-shrink-0">
                  <div class="w-10 h-10 bg-gray-200 rounded-full flex items-center justify-center">
                    <span class="text-sm font-medium text-gray-700">
                      {{ remark.studentInitials }}
                    </span>
                  </div>
                </div>
                <div class="ml-4 flex-1">
                  <div class="flex items-center justify-between">
                    <div>
                      <div class="text-sm font-medium text-gray-900">{{ remark.studentName }}</div>
                      <div class="text-sm text-gray-500">{{ remark.date }}</div>
                    </div>
                    <span
                      :class="[
                        remark.type === 'Positive' ? 'bg-green-100 text-green-800' :
                        remark.type === 'Improvement' ? 'bg-yellow-100 text-yellow-800' :
                        'bg-red-100 text-red-800'
                      ]"
                      class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                    >
                      {{ remark.type }}
                    </span>
                  </div>
                  <div class="mt-2 text-sm text-gray-700">
                    {{ remark.content }}
                  </div>
                  <div class="mt-2 flex items-center space-x-4">
                    <button class="text-indigo-600 hover:text-indigo-900 text-sm">
                      Edit
                    </button>
                    <button class="text-red-600 hover:text-red-900 text-sm">
                      Delete
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </li>
      </ul>
    </div>

    <!-- Add Remark Modal/Form (simplified) -->
    <div v-if="showAddRemark" class="fixed inset-0 z-50 overflow-y-auto">
      <div class="flex items-end justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
        <div class="fixed inset-0 transition-opacity" @click="showAddRemark = false">
          <div class="absolute inset-0 bg-gray-500 opacity-75"></div>
        </div>
        <div class="inline-block align-bottom bg-white rounded-lg px-4 pt-5 pb-4 text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
          <div>
            <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">Add Remark</h3>
            <form @submit.prevent="addRemark">
              <div class="mb-4">
                <label for="student" class="block text-sm font-medium text-gray-700">Student</label>
                <select
                  id="student"
                  v-model="newRemark.studentId"
                  class="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-green-500 focus:border-green-500 sm:text-sm rounded-md"
                  required
                >
                  <option v-for="student in students" :key="student.id" :value="student.id">
                    {{ student.name }}
                  </option>
                </select>
              </div>
              <div class="mb-4">
                <label for="type" class="block text-sm font-medium text-gray-700">Type</label>
                <select
                  id="type"
                  v-model="newRemark.type"
                  class="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-green-500 focus:border-green-500 sm:text-sm rounded-md"
                  required
                >
                  <option value="Positive">Positive</option>
                  <option value="Improvement">Needs Improvement</option>
                  <option value="Concern">Concern</option>
                </select>
              </div>
              <div class="mb-4">
                <label for="content" class="block text-sm font-medium text-gray-700">Remark</label>
                <textarea
                  id="content"
                  v-model="newRemark.content"
                  rows="3"
                  class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-green-500 focus:border-green-500 sm:text-sm"
                  placeholder="Enter your remark..."
                  required
                ></textarea>
              </div>
              <div class="flex justify-end space-x-3">
                <button
                  type="button"
                  @click="showAddRemark = false"
                  class="bg-white py-2 px-4 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  class="inline-flex justify-center py-2 px-4 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                >
                  Add Remark
                </button>
              </div>
            </form>
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
  { id: 1, name: 'Class 10A', subject: 'Mathematics' },
  { id: 2, name: 'Class 9B', subject: 'Mathematics' }
])

const selectedClass = ref(1)

const students = ref([
  { id: 1, name: 'John Doe' },
  { id: 2, name: 'Jane Smith' },
  { id: 3, name: 'Bob Johnson' }
])

const remarks = ref([
  {
    id: 1,
    studentName: 'John Doe',
    studentInitials: 'JD',
    date: '2024-01-15',
    type: 'Positive',
    content: 'Excellent performance in the recent test. Shows great understanding of algebraic concepts.'
  },
  {
    id: 2,
    studentName: 'Jane Smith',
    studentInitials: 'JS',
    date: '2024-01-14',
    type: 'Improvement',
    content: 'Needs to work on problem-solving speed. Practice more exercises regularly.'
  },
  {
    id: 3,
    studentName: 'Bob Johnson',
    studentInitials: 'BJ',
    date: '2024-01-13',
    type: 'Concern',
    content: 'Has been consistently absent. Please ensure regular attendance.'
  }
])

const showAddRemark = ref(false)
const newRemark = ref({
  studentId: '',
  type: 'Positive',
  content: ''
})

const addRemark = () => {
  // In a real app, this would make an API call
  console.log('Adding remark:', newRemark.value)
  showAddRemark.value = false
  newRemark.value = { studentId: '', type: 'Positive', content: '' }
}
</script>

<style scoped>
/* Additional styles if needed */
</style>