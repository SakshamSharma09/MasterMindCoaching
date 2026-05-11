<template>
  <div class="px-4 sm:px-6 lg:px-8">
    <div class="sm:flex sm:items-center">
      <div class="sm:flex-auto">
        <h1 class="text-2xl font-semibold text-gray-900">My Students</h1>
        <p class="mt-2 text-sm text-gray-700">
          View students mapped to your classes.
        </p>
      </div>
    </div>

    <div class="mb-6">
      <label for="class-select" class="block text-sm font-medium text-gray-700">Select Class</label>
      <select
        id="class-select"
        v-model="selectedClass"
        class="mt-1 block w-full rounded-md border-gray-300 py-2 pl-3 pr-10 text-base focus:border-green-500 focus:outline-none focus:ring-green-500 sm:text-sm"
        :disabled="loading || classes.length === 0"
      >
        <option v-for="classItem in classes" :key="classItem.id" :value="classItem.id">
          {{ classItem.name }} - {{ classItem.board }}-{{ classItem.medium }}
        </option>
      </select>
    </div>

    <div v-if="error" class="mb-4 rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
      {{ error }}
    </div>

    <div v-if="loading" class="rounded-md border border-gray-200 bg-white px-4 py-8 text-center text-sm text-gray-500">
      Loading students...
    </div>

    <div v-else-if="students.length === 0" class="rounded-md border border-gray-200 bg-white px-4 py-8 text-center text-sm text-gray-500">
      No students found for this class.
    </div>

    <div v-else class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
      <div
        v-for="student in students"
        :key="student.id"
        class="overflow-hidden rounded-lg bg-white shadow"
      >
        <div class="p-6">
          <div class="flex items-center">
            <div class="flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-full bg-gray-200">
              <span class="text-lg font-medium text-gray-700">{{ student.initials }}</span>
            </div>
            <div class="ml-4">
              <h3 class="text-lg font-medium text-gray-900">{{ student.name }}</h3>
              <p class="text-sm text-gray-500">Roll No: {{ student.rollNo }}</p>
            </div>
          </div>
          <div class="mt-4">
            <div class="flex items-center justify-between text-sm">
              <span class="text-gray-500">Attendance:</span>
              <span class="font-medium">{{ student.attendance }}</span>
            </div>
            <div class="mt-1 flex items-center justify-between text-sm">
              <span class="text-gray-500">Average Grade:</span>
              <span class="font-medium">{{ student.averageGrade }}</span>
            </div>
          </div>
          <div class="mt-4 flex space-x-2">
            <button class="flex-1 rounded-md bg-indigo-600 px-3 py-2 text-sm font-medium text-white hover:bg-indigo-700">
              View Details
            </button>
            <button
              class="flex-1 rounded-md bg-green-600 px-3 py-2 text-sm font-medium text-white hover:bg-green-700"
              @click="openRemark(student.id)"
            >
              Add Remark
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="mt-8 rounded-lg bg-white shadow">
      <div class="px-4 py-5 sm:p-6">
        <h3 class="mb-4 text-lg font-medium leading-6 text-gray-900">Class Summary</h3>
        <div class="grid grid-cols-1 gap-5 sm:grid-cols-4">
          <div class="text-center">
            <div class="text-2xl font-bold text-gray-900">{{ classSummary.totalStudents }}</div>
            <div class="text-sm text-gray-500">Total Students</div>
          </div>
          <div class="text-center">
            <div class="text-2xl font-bold text-green-600">{{ classSummary.averageAttendance }}</div>
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
import { computed, onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { teacherPortalService, type TeacherClassContext, type TeacherStudentItem } from '@/services/teacherPortalService'

const router = useRouter()
const authStore = useAuthStore()

const loading = ref(false)
const error = ref('')
const classes = ref<TeacherClassContext[]>([])
const students = ref<TeacherStudentItem[]>([])
const selectedClass = ref<number | null>(null)

const classSummary = computed(() => ({
  totalStudents: students.value.length,
  averageAttendance: '--',
  averageGrade: 'N/A',
  recentRemarks: 0
}))

const loadStudents = async () => {
  if (!selectedClass.value) {
    students.value = []
    return
  }

  loading.value = true
  error.value = ''

  try {
    students.value = await teacherPortalService.getClassStudents(selectedClass.value)
  } catch (err: any) {
    students.value = []
    error.value = err?.response?.data?.message || err?.message || 'Failed to load students'
  } finally {
    loading.value = false
  }
}

const loadTeacherClasses = async () => {
  const email = authStore.user?.email || ''
  if (!email) {
    error.value = 'Teacher account email not found. Please re-login.'
    return
  }

  loading.value = true
  error.value = ''

  try {
    classes.value = await teacherPortalService.getMyClasses(email)
    selectedClass.value = classes.value.length > 0 ? classes.value[0].id : null
    await loadStudents()
  } catch (err: any) {
    classes.value = []
    students.value = []
    error.value = err?.response?.data?.message || err?.message || 'Failed to load teacher classes'
  } finally {
    loading.value = false
  }
}

const openRemark = (studentId: number) => {
  router.push({
    name: 'TeacherRemarks',
    query: {
      classId: selectedClass.value?.toString() || '',
      studentId: studentId.toString()
    }
  })
}

watch(selectedClass, async (newValue, oldValue) => {
  if (newValue && newValue !== oldValue) {
    await loadStudents()
  }
})

onMounted(async () => {
  await loadTeacherClasses()
})
</script>
