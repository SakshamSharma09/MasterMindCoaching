<template>
  <div class="px-4 sm:px-6 lg:px-8">
    <div class="sm:flex sm:items-center">
      <div class="sm:flex-auto">
        <h1 class="text-2xl font-semibold text-gray-900">Attendance Management</h1>
        <p class="mt-2 text-sm text-gray-700">
          Manage student and teacher attendance records.
        </p>
      </div>
      <div class="mt-4 sm:mt-0 sm:ml-16 sm:flex-none">
        <button
          type="button"
          @click="openMarkModal()"
          class="inline-flex items-center justify-center rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
        >
          Mark Attendance
        </button>
      </div>
    </div>

    <!-- Tab Navigation -->
    <div class="mt-6 border-b border-gray-200">
      <nav class="-mb-px flex space-x-8">
        <button
          @click="activeTab = 'individual'"
          :class="[
            activeTab === 'individual'
              ? 'border-indigo-500 text-indigo-600'
              : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700',
            'whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm'
          ]"
        >
          Individual Attendance
        </button>
        <button
          @click="activeTab = 'bulk'"
          :class="[
            activeTab === 'bulk'
              ? 'border-indigo-500 text-indigo-600'
              : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700',
            'whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm'
          ]"
        >
          Bulk Attendance
        </button>
      </nav>
    </div>

    <!-- Individual Attendance Tab -->
    <div v-if="activeTab === 'individual'" class="mt-6">

    <!-- Filters -->
    <div class="mt-6 grid grid-cols-1 md:grid-cols-4 gap-4">
      <div>
        <label class="block text-sm font-medium text-gray-700">Date</label>
        <input
          v-model="selectedDate"
          type="date"
          class="mt-1 input-primary"
        />
      </div>
      <div>
        <label class="block text-sm font-medium text-gray-700">Class</label>
        <select v-model="selectedClass" class="mt-1 input-primary">
          <option value="">All Classes</option>
          <option v-for="classItem in classes" :key="classItem.id" :value="classItem.id">
            {{ classItem.name }}
          </option>
        </select>
      </div>
      <div>
        <label class="block text-sm font-medium text-gray-700">Status</label>
          <select v-model="selectedStatus" class="mt-1 input-primary">
            <option value="">All Status</option>
            <option value="present">Present</option>
            <option value="absent">Absent</option>
            <option value="late">Late</option>
          </select>
      </div>
      <div>
        <label class="block text-sm font-medium text-gray-700">Search</label>
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search students..."
          class="mt-1 input-primary"
        />
      </div>
    </div>

    <!-- Attendance Table -->
    <div class="mt-8 flex flex-col">
      <div class="-my-2 -mx-4 overflow-x-auto sm:-mx-6 lg:-mx-8">
        <div class="inline-block min-w-full py-2 align-middle md:px-6 lg:px-8">
          <div class="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
            <table class="min-w-full divide-y divide-gray-300">
              <thead class="bg-gray-50">
                <tr>
                  <th scope="col" class="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">
                    Student Name
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Class
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Medium
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Board
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Status
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Check-in Time
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Check-out Time
                  </th>
                  <th scope="col" class="relative py-3.5 pl-3 pr-4 sm:pr-6">
                    <span class="sr-only">Actions</span>
                  </th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200 bg-white">
                <tr v-for="record in filteredAttendance" :key="record.id">
                  <td class="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">
                    {{ record.studentName }}
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    {{ record.className }}
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <div class="flex flex-wrap gap-1">
                      <span 
                        v-for="(medium, index) in getAttendanceMediums(record)" 
                        :key="index"
                        class="inline-flex items-center px-2 py-1 text-xs font-medium bg-purple-100 text-purple-800 rounded-full"
                      >
                        {{ medium }}
                      </span>
                    </div>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <div class="flex flex-wrap gap-1">
                      <span 
                        v-for="(board, index) in getAttendanceBoards(record)" 
                        :key="index"
                        class="inline-flex items-center px-2 py-1 text-xs font-medium bg-green-100 text-green-800 rounded-full"
                      >
                        {{ board }}
                      </span>
                    </div>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <span
                      :class="[
                        record.status === 'present' 
                          ? 'bg-green-100 text-green-800 ring-1 ring-green-500/20' 
                          : record.status === 'absent'
                          ? 'bg-red-100 text-red-800 ring-1 ring-red-500/20'
                          : 'bg-yellow-100 text-yellow-800 ring-1 ring-yellow-500/20'
                      ]"
                      class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                    >
                      {{ record.status }}
                    </span>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    {{ record.checkInTime }}
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    {{ record.checkOutTime || '-' }}
                  </td>
                  <td class="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                    <button
                      @click="editAttendance(record)"
                      class="text-indigo-600 hover:text-indigo-900 mr-4"
                    >
                      Edit
                    </button>
                    <button
                      @click="deleteAttendance(record.id)"
                      class="text-red-600 hover:text-red-900"
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
    </div>

    <!-- Bulk Attendance Tab -->
    <div v-if="activeTab === 'bulk'" class="mt-6">
      <!-- Bulk Attendance Header -->
      <div class="bg-white shadow rounded-lg p-6">
        <div class="flex items-center justify-between mb-6">
          <div>
            <h2 class="text-lg font-medium text-gray-900">Bulk Attendance Marking</h2>
            <p class="mt-1 text-sm text-gray-500">
              Mark attendance for all students at once. Check the box to mark present, uncheck to mark absent.
            </p>
          </div>
          <div class="flex items-center space-x-4">
            <div>
              <label class="block text-sm font-medium text-gray-700">Select Date</label>
              <input
                v-model="bulkDate"
                type="date"
                class="mt-1 input-primary"
                @change="loadBulkAttendance"
              />
            </div>
            <button
              @click="loadAllStudents"
              :disabled="loadingBulk"
              class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50"
            >
              <svg v-if="loadingBulk" class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              {{ loadingBulk ? 'Loading...' : 'Load Students' }}
            </button>
            <button
              @click="saveBulkAttendance"
              :disabled="loadingBulk || bulkStudents.length === 0"
              class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500 disabled:opacity-50"
            >
              <svg v-if="savingBulk" class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              {{ savingBulk ? 'Saving...' : 'Save Attendance' }}
            </button>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="flex items-center space-x-4 mb-6 p-4 bg-gray-50 rounded-lg">
          <span class="text-sm font-medium text-gray-700">Quick Actions:</span>
          <button
            @click="markAllPresent"
            class="text-sm text-indigo-600 hover:text-indigo-800 font-medium"
          >
            Mark All Present
          </button>
          <button
            @click="markAllAbsent"
            class="text-sm text-red-600 hover:text-red-800 font-medium"
          >
            Mark All Absent
          </button>
          <button
            @click="clearAllSelections"
            class="text-sm text-gray-600 hover:text-gray-800 font-medium"
          >
            Clear All
          </button>
          <span class="text-sm text-gray-500">
            Selected: {{ bulkStudents.filter(s => s.isPresent).length }} present, {{ bulkStudents.filter(s => !s.isPresent).length }} absent
          </span>
        </div>

        <!-- Students Grouped by Classes -->
        <div v-if="bulkStudents.length > 0" class="space-y-6">
          <div v-for="classGroup in groupedBulkStudents" :key="classGroup.classId" class="border border-gray-200 rounded-lg">
            <div class="bg-gray-50 px-6 py-3 border-b border-gray-200">
              <h3 class="text-lg font-medium text-gray-900">
                {{ classGroup.className }}
                <span class="ml-2 text-sm font-normal text-gray-500">
                  ({{ classGroup.board }} - {{ classGroup.medium }})
                </span>
                <span class="ml-2 text-sm text-gray-500">
                  {{ classGroup.students.length }} students
                </span>
              </h3>
            </div>
            <div class="divide-y divide-gray-200">
              <div
                v-for="student in classGroup.students"
                :key="student.id"
                class="px-6 py-4 hover:bg-gray-50 transition-colors"
              >
                <div class="flex items-center">
                  <input
                    type="checkbox"
                    :checked="student.isPresent"
                    @change="toggleStudentAttendance(student)"
                    class="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                  />
                  <div class="ml-4 flex-1">
                    <div class="flex items-center">
                      <span class="text-sm font-medium text-gray-900">
                        {{ student.firstName }} {{ student.lastName }}
                      </span>
                      <span class="ml-2 text-sm text-gray-500">
                        ({{ student.admissionNumber }})
                      </span>
                    </div>
                    <div class="mt-1 text-sm text-gray-500">
                      Mobile: {{ student.studentMobile || 'N/A' }}
                    </div>
                  </div>
                  <div class="flex items-center space-x-4">
                    <span
                      :class="[
                        'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium',
                        student.isPresent
                          ? 'bg-green-100 text-green-800'
                          : 'bg-red-100 text-red-800'
                      ]"
                    >
                      {{ student.isPresent ? 'Present' : 'Absent' }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Empty State -->
        <div v-else-if="!loadingBulk" class="text-center py-12">
          <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"></path>
          </svg>
          <h3 class="mt-2 text-sm font-medium text-gray-900">No students loaded</h3>
          <p class="mt-1 text-sm text-gray-500">Click "Load Students" to see all students for bulk attendance marking.</p>
        </div>

        <!-- Loading State -->
        <div v-else class="text-center py-12">
          <div class="inline-flex items-center">
            <svg class="animate-spin -ml-1 mr-2 h-5 w-5 text-indigo-600" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            <span class="text-sm text-gray-600">Loading students...</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Mark/Edit Attendance Modal -->
    <div v-if="showMarkModal" class="fixed inset-0 z-50 overflow-y-auto">
      <div class="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
        <div class="fixed inset-0 transition-opacity" @click="closeMarkModal">
          <div class="absolute inset-0 bg-gray-500 opacity-75"></div>
        </div>
        <div class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
          <form @submit.prevent="submitMarkForm">
            <div class="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
              <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">
                {{ isEditing ? 'Edit Attendance' : 'Mark Attendance' }}
              </h3>
              
              <div class="grid grid-cols-1 gap-4">
                <!-- Date -->
                <div>
                  <label class="block text-sm font-medium text-gray-700">Date</label>
                  <input
                    v-model="markForm.date"
                    type="date"
                    required
                    class="mt-1 input-primary"
                    :disabled="isEditing"
                  />
                </div>

                <!-- Class Selection -->
                <div>
                  <label class="block text-sm font-medium text-gray-700">Class</label>
                  <select 
                    v-model="markForm.classId" 
                    required 
                    class="mt-1 input-primary"
                    :disabled="isEditing"
                    @change="fetchStudents"
                  >
                    <option value="">Select Class</option>
                    <option v-for="classItem in classes" :key="classItem.id" :value="classItem.id">
                      {{ formatClassForDropdown(classItem) }}
                    </option>
                  </select>
                </div>

                <!-- Student Selection -->
                <div>
                  <label class="block text-sm font-medium text-gray-700">Student</label>
                  <select 
                    v-model="markForm.studentId" 
                    required 
                    class="mt-1 input-primary"
                    :disabled="isEditing || !markForm.classId"
                  >
                    <option value="">Select Student</option>
                    <option v-for="student in students" :key="student.id" :value="student.id">
                      {{ student.firstName }} {{ student.lastName }} ({{ student.admissionNumber }})
                    </option>
                  </select>
                  <p v-if="!markForm.classId" class="mt-1 text-xs text-gray-500">Select a class first</p>
                </div>

                <!-- Status -->
                <div>
                  <label class="block text-sm font-medium text-gray-700">Status</label>
                  <select v-model="markForm.status" required class="mt-1 input-primary">
                    <option :value="0">Present</option>
                    <option :value="1">Absent</option>
                    <option :value="2">Late</option>
                    <option :value="3">Half Day</option>
                    <option :value="4">Holiday</option>
                    <option :value="5">Leave</option>
                  </select>
                </div>

                <!-- Times -->
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Check-in Time</label>
                    <input
                      v-model="markForm.checkInTime"
                      type="time"
                      class="mt-1 input-primary"
                    />
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Check-out Time</label>
                    <input
                      v-model="markForm.checkOutTime"
                      type="time"
                      class="mt-1 input-primary"
                    />
                  </div>
                </div>

                <!-- Remarks -->
                <div>
                  <label class="block text-sm font-medium text-gray-700">Remarks</label>
                  <textarea
                    v-model="markForm.remarks"
                    rows="2"
                    class="mt-1 input-primary"
                    placeholder="Optional remarks..."
                  ></textarea>
                </div>
              </div>
            </div>
            
            <div class="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
              <button
                type="submit"
                class="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:ml-3 sm:w-auto sm:text-sm"
              >
                {{ isEditing ? 'Update' : 'Mark' }}
              </button>
              <button
                type="button"
                @click="closeMarkModal"
                class="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { formatClassForDropdown } from '@/utils/classUtils'
import { attendanceService, type AttendanceRecord, ATTENDANCE_STATUS_MAP, ATTENDANCE_STATUS_REVERSE_MAP } from '@/services/attendanceService'
import { classesService } from '@/services/classesService'
import { studentsService, type Student } from '@/services/studentsService'

// Reactive data
const attendance = ref<AttendanceRecord[]>([])
const classes = ref<any[]>([])
const selectedDate = ref(new Date().toISOString().split('T')[0])
const selectedClass = ref('')
const selectedStatus = ref('')
const searchQuery = ref('')
const loading = ref(false)

// Modal State
const showMarkModal = ref(false)
const isEditing = ref(false)
const editingId = ref<number | null>(null)
const students = ref<Student[]>([])

const markForm = ref({
  classId: '',
  date: new Date().toISOString().split('T')[0],
  studentId: '',
  status: 0,
  checkInTime: '',
  checkOutTime: '',
  remarks: ''
})

// Bulk Attendance State
const activeTab = ref('individual')
const bulkDate = ref(new Date().toISOString().split('T')[0])
const bulkStudents = ref<any[]>([])
const loadingBulk = ref(false)
const savingBulk = ref(false)

// Computed filtered attendance
const filteredAttendance = computed(() => {
  return attendance.value.filter(record => {
    const matchesStatus = !selectedStatus.value || record.status === selectedStatus.value
    const matchesSearch = !searchQuery.value || 
      record.studentName.toLowerCase().includes(searchQuery.value.toLowerCase())
    
    return matchesStatus && matchesSearch
  })
})

// Computed property for grouped bulk students
const groupedBulkStudents = computed(() => {
  const grouped: { [key: number]: any } = {}
  
  bulkStudents.value.forEach(student => {
    if (!grouped[student.classId]) {
      const classInfo = classes.value.find(c => c.id === student.classId)
      grouped[student.classId] = {
        classId: student.classId,
        className: classInfo?.name || `Class ${student.classId}`,
        board: classInfo?.board || 'N/A',
        medium: classInfo?.medium || 'N/A',
        students: []
      }
    }
    grouped[student.classId].students.push(student)
  })
  
  return Object.values(grouped).sort((a, b) => a.className.localeCompare(b.className))
})

const loadData = async () => {
  loading.value = true
  try {
    const classId = selectedClass.value ? parseInt(selectedClass.value) : undefined
    attendance.value = await attendanceService.getAttendance(selectedDate.value, classId)
  } catch (error) {
    console.error('Error fetching attendance:', error)
  } finally {
    loading.value = false
  }
}

const loadClasses = async () => {
  try {
    classes.value = await classesService.getClasses()
  } catch (error) {
    console.error('Error fetching classes:', error)
  }
}

const fetchStudents = async () => {
  if (!markForm.value.classId) {
    students.value = []
    return
  }
  try {
    const result = await studentsService.getStudents(1, 100, parseInt(markForm.value.classId))
    students.value = result.data
  } catch (error) {
    console.error('Error fetching students:', error)
  }
}

// Helper functions for attendance class information
const getAttendanceMediums = (record: any) => {
  // Find the class information from the classes array
  const classInfo = classes.value.find(c => c.id === record.classId)
  return classInfo?.medium ? [classInfo.medium] : []
}

const getAttendanceBoards = (record: any) => {
  // Find the class information from the classes array
  const classInfo = classes.value.find(c => c.id === record.classId)
  return classInfo?.board ? [classInfo.board] : []
}

// API methods
const openMarkModal = () => {
  isEditing.value = false
  editingId.value = null
  markForm.value = {
    classId: selectedClass.value || '',
    date: selectedDate.value,
    studentId: '',
    status: 0,
    checkInTime: '',
    checkOutTime: '',
    remarks: ''
  }
  if (markForm.value.classId) {
    fetchStudents()
  }
  showMarkModal.value = true
}

const editAttendance = async (record: AttendanceRecord) => {
  isEditing.value = true
  editingId.value = record.id
  
  // Need to load students for the class of the record
  markForm.value.classId = record.classId.toString()
  await fetchStudents()

  markForm.value.studentId = record.studentId.toString()
  markForm.value.date = record.date
  markForm.value.status = ATTENDANCE_STATUS_MAP[record.status] ?? 0
  markForm.value.checkInTime = record.checkInTime || ''
  markForm.value.checkOutTime = record.checkOutTime || ''
  markForm.value.remarks = record.remarks || ''
  
  showMarkModal.value = true
}

const closeMarkModal = () => {
  showMarkModal.value = false
  isEditing.value = false
  editingId.value = null
}

const submitMarkForm = async () => {
  try {
    if (isEditing.value && editingId.value) {
      await attendanceService.updateAttendance(editingId.value, {
        status: markForm.value.status,
        checkInTime: markForm.value.checkInTime || undefined,
        checkOutTime: markForm.value.checkOutTime || undefined,
        remarks: markForm.value.remarks
      })
    } else {
      await attendanceService.markAttendance({
        studentId: parseInt(markForm.value.studentId),
        classId: parseInt(markForm.value.classId),
        date: markForm.value.date,
        status: markForm.value.status,
        checkInTime: markForm.value.checkInTime || undefined,
        checkOutTime: markForm.value.checkOutTime || undefined,
        remarks: markForm.value.remarks
      })
    }
    await loadData()
    closeMarkModal()
  } catch (error: any) {
    console.error('Error saving attendance:', error)
    alert(error.response?.data?.message || 'Error saving attendance')
  }
}

const deleteAttendance = async (id: number) => {
  if (confirm('Are you sure you want to delete this attendance record?')) {
    try {
      await attendanceService.deleteAttendance(id)
      await loadData()
    } catch (error) {
      console.error('Error deleting attendance:', error)
    }
  }
}

// Bulk Attendance Methods
const loadAllStudents = async () => {
  loadingBulk.value = true
  try {
    // Get all students with their class information
    const result = await studentsService.getStudents(1, 1000) // Get all students
    console.log('Raw student data from API:', result.data)
    
    bulkStudents.value = result.data.map(student => {
      // Extract classId from student's active class assignment
      const activeClassAssignment = student.studentClasses?.find((sc: any) => sc.isActive)
      const classId = activeClassAssignment?.classId || student.classId
      
      console.log(`Student ${student.firstName} ${student.lastName}:`, {
        studentId: student.id,
        classId: classId,
        className: activeClassAssignment?.class?.name || 'N/A',
        studentClasses: student.studentClasses
      })
      
      return {
        ...student,
        classId: classId, // Use the extracted classId
        isPresent: false // Default to absent
      }
    }).filter(student => student.classId) // Only include students with valid classId
    
    console.log('Loaded bulk students:', bulkStudents.value.length)
    console.log('Bulk students before attendance load:', bulkStudents.value.map(s => ({
      name: `${s.firstName} ${s.lastName}`,
      isPresent: s.isPresent
    })))
    
    // Load existing attendance for the selected date
    await loadBulkAttendance()
    
    console.log('Bulk students after attendance load:', bulkStudents.value.map(s => ({
      name: `${s.firstName} ${s.lastName}`,
      isPresent: s.isPresent
    })))
  } catch (error) {
    console.error('Error loading students:', error)
    alert('Error loading students for bulk attendance')
  } finally {
    loadingBulk.value = false
  }
}

const loadBulkAttendance = async () => {
  if (!bulkDate.value || bulkStudents.value.length === 0) return
  
  try {
    // Get existing attendance for the selected date
    const existingAttendance = await attendanceService.getAttendance(bulkDate.value)
    
    console.log('Existing attendance for date:', bulkDate.value, existingAttendance)
    
    // Update student attendance status based on existing records
    bulkStudents.value.forEach(student => {
      const existingRecord = existingAttendance.find(record => 
        record.studentId === student.id
      )
      if (existingRecord) {
        // Set isPresent based on status - present = true, absent = false
        student.isPresent = existingRecord.status === 'present'
        console.log(`Student ${student.firstName}: status=${existingRecord.status}, isPresent=${student.isPresent}`)
      } else {
        // Default to false (absent) if no record exists
        student.isPresent = false
        console.log(`Student ${student.firstName}: no existing record, defaulting to absent`)
      }
    })
  } catch (error) {
    console.error('Error loading existing attendance:', error)
  }
}

const toggleStudentAttendance = (student: any) => {
  student.isPresent = !student.isPresent
}

const markAllPresent = () => {
  bulkStudents.value.forEach(student => {
    student.isPresent = true
  })
}

const markAllAbsent = () => {
  bulkStudents.value.forEach(student => {
    student.isPresent = false
  })
}

const clearAllSelections = () => {
  bulkStudents.value.forEach(student => {
    student.isPresent = false
  })
}

const saveBulkAttendance = async () => {
  savingBulk.value = true
  try {
    // Validate that all students have valid classId
    const studentsWithoutClass = bulkStudents.value.filter(student => !student.classId)
    if (studentsWithoutClass.length > 0) {
      console.error('Students without class assignment:', studentsWithoutClass)
      alert(`Error: ${studentsWithoutClass.length} students are not assigned to any class. Please assign them to classes first.`)
      return
    }

    console.log('Saving bulk attendance for:', bulkStudents.value.length, 'students')
    
    const attendancePromises = bulkStudents.value.map(async (student) => {
      const status = student.isPresent ? 0 : 1 // 0 = present, 1 = absent
      
      // Validate classId before proceeding
      if (!student.classId) {
        throw new Error(`Student ${student.firstName} ${student.lastName} has no class assignment`)
      }
      
      const attendanceData = {
        studentId: student.id,
        classId: student.classId,
        date: bulkDate.value,
        status: status,
        checkInTime: student.isPresent ? '09:00' : undefined,
        checkOutTime: student.isPresent ? '15:00' : undefined,
        remarks: student.isPresent ? 'Present' : 'Absent'
      }
      
      console.log(`Saving attendance for ${student.firstName} ${student.lastName}:`, attendanceData)
      
      // Check if attendance already exists for this student on this date
      const existingAttendance = await attendanceService.getAttendance(bulkDate.value, student.classId, student.id)
      
      if (existingAttendance.length > 0) {
        // Update existing attendance
        const existingRecord = existingAttendance.find(record => record.studentId === student.id)
        if (existingRecord) {
          console.log(`Updating existing attendance for ${student.firstName}`)
          return await attendanceService.updateAttendance(existingRecord.id, {
            status: status,
            checkInTime: attendanceData.checkInTime,
            checkOutTime: attendanceData.checkOutTime,
            remarks: attendanceData.remarks
          })
        }
      } else {
        // Create new attendance
        console.log(`Creating new attendance for ${student.firstName}`)
        return await attendanceService.markAttendance(attendanceData)
      }
    })
    
    const results = await Promise.allSettled(attendancePromises)
    
    // Check for any failed operations
    const failed = results.filter(result => result.status === 'rejected')
    if (failed.length > 0) {
      console.error('Some attendance operations failed:', failed)
      alert(`Warning: ${failed.length} attendance records failed to save. Please check the console for details.`)
    } else {
      console.log('All attendance records saved successfully')
      alert('Bulk attendance saved successfully!')
    }
    
    await loadData() // Refresh individual attendance view
  } catch (error: any) {
    console.error('Error saving bulk attendance:', error)
    alert(error.response?.data?.message || error.message || 'Error saving bulk attendance')
  } finally {
    savingBulk.value = false
  }
}

// Watchers
watch([selectedDate, selectedClass], () => {
  loadData()
})

onMounted(async () => {
  await loadClasses()
  await loadData()
})
</script>

<style scoped>
/* Additional styles if needed */
</style>
