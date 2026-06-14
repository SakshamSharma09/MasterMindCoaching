<template>
  <div class="px-4 sm:px-6 lg:px-8">
    <div class="sm:flex sm:items-center">
      <div class="sm:flex-auto">
        <h1 class="text-2xl font-semibold text-gray-900">Mark Attendance</h1>
        <p class="mt-2 text-sm text-gray-700">
          Mark attendance for your class students.
        </p>
      </div>
      <div class="mt-4 sm:ml-16 sm:mt-0 sm:flex-none">
        <button
          type="button"
          class="inline-flex items-center justify-center rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 disabled:opacity-50"
          @click="saveAttendance"
          :disabled="saving || students.length === 0 || !selectedClass"
        >
          {{ saving ? 'Saving...' : 'Save Attendance' }}
        </button>
      </div>
    </div>

    <div class="mb-4 mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
      <div>
        <label for="class-select" class="block text-sm font-medium text-gray-700">Select Class</label>
        <select
          id="class-select"
          v-model.number="selectedClass"
          class="mt-1 block w-full rounded-md border-gray-300 py-2 pl-3 pr-10 text-base focus:border-green-500 focus:outline-none focus:ring-green-500 sm:text-sm"
        >
          <option v-for="classItem in classes" :key="classItem.id" :value="classItem.id">
            {{ classItem.name }} - {{ classItem.board }}-{{ classItem.medium }}
          </option>
        </select>
      </div>
      <div>
        <label for="attendance-date" class="block text-sm font-medium text-gray-700">Date</label>
        <input
          id="attendance-date"
          v-model="attendanceDate"
          type="date"
          class="mt-1 block w-full rounded-md border-gray-300 py-2 px-3 text-base focus:border-green-500 focus:outline-none focus:ring-green-500 sm:text-sm"
        />
      </div>
    </div>

    <div v-if="error" class="mb-4 rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
      {{ error }}
    </div>
    <div v-if="successMessage" class="mb-4 rounded-md border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
      {{ successMessage }}
    </div>

    <div v-if="loading" class="rounded-md border border-gray-200 bg-white px-4 py-8 text-center text-sm text-gray-500">
      Loading students...
    </div>

    <div v-else-if="students.length === 0" class="rounded-md border border-gray-200 bg-white px-4 py-8 text-center text-sm text-gray-500">
      No students found in this class.
    </div>

    <div v-else class="overflow-hidden rounded-md bg-white shadow">
      <ul role="list" class="divide-y divide-gray-200">
        <li v-for="student in students" :key="student.id">
          <div class="px-4 py-4 sm:px-6">
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <div class="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-full bg-gray-200">
                  <span class="text-sm font-medium text-gray-700">{{ student.initials }}</span>
                </div>
                <div class="ml-4">
                  <div class="text-sm font-medium text-gray-900">{{ student.name }}</div>
                  <div class="text-sm text-gray-500">Roll No: {{ student.rollNo }}</div>
                </div>
              </div>
              <div class="flex items-center space-x-4">
                <label class="inline-flex items-center">
                  <input type="radio" :name="`attendance-${student.id}`" value="present" v-model="student.status" class="h-4 w-4 text-green-600">
                  <span class="ml-2 text-sm text-gray-700">Present</span>
                </label>
                <label class="inline-flex items-center">
                  <input type="radio" :name="`attendance-${student.id}`" value="absent" v-model="student.status" class="h-4 w-4 text-red-600">
                  <span class="ml-2 text-sm text-gray-700">Absent</span>
                </label>
                <label class="inline-flex items-center">
                  <input type="radio" :name="`attendance-${student.id}`" value="late" v-model="student.status" class="h-4 w-4 text-yellow-600">
                  <span class="ml-2 text-sm text-gray-700">Late</span>
                </label>
              </div>
            </div>
          </div>
        </li>
      </ul>
    </div>

    <div class="mt-8 rounded-lg bg-white shadow">
      <div class="px-4 py-5 sm:p-6">
        <h3 class="mb-4 text-lg font-medium leading-6 text-gray-900">Attendance Summary</h3>
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
import { computed, onMounted, ref, watch } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { ATTENDANCE_STATUS_MAP, attendanceService, type AttendanceRecord } from '@/services/attendanceService'
import { teacherPortalService, type TeacherClassContext } from '@/services/teacherPortalService'

interface StudentAttendanceRow {
  id: number
  name: string
  initials: string
  rollNo: string
  status: 'present' | 'absent' | 'late'
  attendanceId?: number
}

const authStore = useAuthStore()

const loading = ref(false)
const saving = ref(false)
const error = ref('')
const successMessage = ref('')
const classes = ref<TeacherClassContext[]>([])
const selectedClass = ref<number | null>(null)
const students = ref<StudentAttendanceRow[]>([])
const attendanceDate = ref(new Date().toISOString().slice(0, 10))

const attendanceSummary = computed(() => ({
  present: students.value.filter(s => s.status === 'present').length,
  absent: students.value.filter(s => s.status === 'absent').length,
  late: students.value.filter(s => s.status === 'late').length
}))

const loadStudentsAndAttendance = async () => {
  if (!selectedClass.value) {
    students.value = []
    return
  }

  loading.value = true
  error.value = ''
  successMessage.value = ''
  try {
    const [classStudents, attendanceRecords] = await Promise.all([
      teacherPortalService.getClassStudents(selectedClass.value),
      attendanceService.getAttendance(attendanceDate.value, selectedClass.value)
    ])

    const attendanceMap = new Map<number, AttendanceRecord>(
      (attendanceRecords || []).map(record => [record.studentId, record])
    )

    students.value = classStudents.map(student => {
      const existing = attendanceMap.get(student.id)
      return {
        id: student.id,
        name: student.name,
        initials: student.initials,
        rollNo: student.rollNo,
        status: ((existing?.status as 'present' | 'absent' | 'late') || 'present'),
        attendanceId: existing?.id
      }
    })
  } catch (err: any) {
    students.value = []
    error.value = err?.response?.data?.message || err?.message || 'Failed to load attendance data'
  } finally {
    loading.value = false
  }
}

const loadTeacherContext = async () => {
  const email = authStore.user?.email || ''
  if (!email) {
    error.value = 'Teacher account email not found. Please re-login.'
    return
  }

  loading.value = true
  try {
    classes.value = await teacherPortalService.getMyClasses()
    selectedClass.value = classes.value.length > 0 ? classes.value[0].id : null
    await loadStudentsAndAttendance()
  } catch (err: any) {
    error.value = err?.response?.data?.message || err?.message || 'Failed to load teacher classes'
  } finally {
    loading.value = false
  }
}

const saveAttendance = async () => {
  if (!selectedClass.value || students.value.length === 0) return

  saving.value = true
  error.value = ''
  successMessage.value = ''
  try {
    for (const student of students.value) {
      const payload = {
        studentId: student.id,
        classId: selectedClass.value,
        date: attendanceDate.value,
        status: ATTENDANCE_STATUS_MAP[student.status]
      }

      if (student.attendanceId) {
        await attendanceService.updateAttendance(student.attendanceId, {
          status: ATTENDANCE_STATUS_MAP[student.status]
        })
      } else {
        const created = await attendanceService.markAttendance(payload)
        student.attendanceId = created.id
      }
    }

    successMessage.value = 'Attendance saved successfully.'
  } catch (err: any) {
    error.value = err?.response?.data?.message || err?.message || 'Failed to save attendance'
  } finally {
    saving.value = false
  }
}

watch([selectedClass, attendanceDate], async ([newClass, newDate], [oldClass, oldDate]) => {
  if (newClass && (newClass !== oldClass || newDate !== oldDate)) {
    await loadStudentsAndAttendance()
  }
})

onMounted(async () => {
  await loadTeacherContext()
})
</script>
