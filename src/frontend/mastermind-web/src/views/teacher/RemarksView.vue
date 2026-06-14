<template>
  <div class="px-4 sm:px-6 lg:px-8">
    <div class="sm:flex sm:items-center">
      <div class="sm:flex-auto">
        <h1 class="text-2xl font-semibold text-gray-900">Student Remarks</h1>
        <p class="mt-2 text-sm text-gray-700">
          Add and manage remarks for your students.
        </p>
      </div>
      <div class="mt-4 sm:ml-16 sm:mt-0 sm:flex-none">
        <button
          type="button"
          class="inline-flex items-center justify-center rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700"
          @click="showAddRemark = true"
          :disabled="students.length === 0"
        >
          Add New Remark
        </button>
      </div>
    </div>

    <div class="mb-6">
      <label for="class-select" class="block text-sm font-medium text-gray-700">Select Class</label>
      <select
        id="class-select"
        v-model="selectedClass"
        class="mt-1 block w-full rounded-md border-gray-300 py-2 pl-3 pr-10 text-base focus:border-green-500 focus:outline-none focus:ring-green-500 sm:text-sm"
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
      Loading remarks...
    </div>

    <div v-else-if="remarks.length === 0" class="rounded-md border border-gray-200 bg-white px-4 py-8 text-center text-sm text-gray-500">
      No remarks found for this class.
    </div>

    <div v-else class="overflow-hidden rounded-md bg-white shadow">
      <ul role="list" class="divide-y divide-gray-200">
        <li v-for="remark in remarks" :key="remark.id">
          <div class="px-4 py-4 sm:px-6">
            <div class="flex items-start justify-between">
              <div class="flex items-start">
                <div class="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-full bg-gray-200">
                  <span class="text-sm font-medium text-gray-700">{{ getInitials(remark.studentName) }}</span>
                </div>
                <div class="ml-4 flex-1">
                  <div class="flex items-center justify-between">
                    <div>
                      <div class="text-sm font-medium text-gray-900">{{ remark.studentName }}</div>
                      <div class="text-sm text-gray-500">{{ remark.date }}</div>
                    </div>
                    <span class="inline-flex items-center rounded-full bg-indigo-100 px-2.5 py-0.5 text-xs font-medium text-indigo-800">
                      {{ remark.type }}
                    </span>
                  </div>
                  <div class="mt-2 text-sm text-gray-700">{{ remark.remarks }}</div>
                  <div class="mt-2 text-xs text-gray-500">
                    <span v-if="remark.subject">Subject: {{ remark.subject }}</span>
                    <span v-if="remark.chapterName" class="ml-3">Chapter: {{ remark.chapterName }}</span>
                    <span class="ml-3">Visible to parent: {{ remark.isVisibleToParent ? 'Yes' : 'No' }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </li>
      </ul>
    </div>

    <div v-if="showAddRemark" class="fixed inset-0 z-50 overflow-y-auto">
      <div class="flex min-h-screen items-end justify-center px-4 pb-20 pt-4 text-center sm:block sm:p-0">
        <div class="fixed inset-0 transition-opacity" @click="showAddRemark = false">
          <div class="absolute inset-0 bg-gray-500 opacity-75"></div>
        </div>
        <div class="inline-block w-full transform overflow-hidden rounded-lg bg-white px-4 pb-4 pt-5 text-left align-bottom shadow-xl transition-all sm:my-8 sm:max-w-lg sm:p-6 sm:align-middle">
          <h3 class="mb-4 text-lg font-medium leading-6 text-gray-900">Add Remark</h3>
          <form @submit.prevent="addRemark">
            <div class="mb-4">
              <label for="student" class="block text-sm font-medium text-gray-700">Student</label>
              <select
                id="student"
                v-model.number="newRemark.studentId"
                class="mt-1 block w-full rounded-md border-gray-300 py-2 pl-3 pr-10 text-base focus:border-green-500 focus:outline-none focus:ring-green-500 sm:text-sm"
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
                v-model.number="newRemark.type"
                class="mt-1 block w-full rounded-md border-gray-300 py-2 pl-3 pr-10 text-base focus:border-green-500 focus:outline-none focus:ring-green-500 sm:text-sm"
                required
              >
                <option :value="0">General</option>
                <option :value="1">Academic</option>
                <option :value="2">Behavior</option>
                <option :value="3">Attendance</option>
                <option :value="4">Improvement</option>
                <option :value="5">Achievement</option>
                <option :value="6">Concern</option>
              </select>
            </div>
            <div class="mb-4">
              <label for="subject" class="block text-sm font-medium text-gray-700">Subject (optional)</label>
              <input
                id="subject"
                v-model="newRemark.subject"
                class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-green-500 focus:ring-green-500 sm:text-sm"
                type="text"
              />
            </div>
            <div class="mb-4">
              <label for="chapter" class="block text-sm font-medium text-gray-700">Chapter (optional)</label>
              <input
                id="chapter"
                v-model="newRemark.chapterName"
                class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-green-500 focus:ring-green-500 sm:text-sm"
                type="text"
              />
            </div>
            <div class="mb-4">
              <label for="content" class="block text-sm font-medium text-gray-700">Remark</label>
              <textarea
                id="content"
                v-model="newRemark.remarks"
                rows="3"
                class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-green-500 focus:ring-green-500 sm:text-sm"
                required
              ></textarea>
            </div>
            <div class="mb-4 flex items-center">
              <input id="parent-visible" v-model="newRemark.isVisibleToParent" type="checkbox" class="h-4 w-4 rounded border-gray-300 text-indigo-600">
              <label for="parent-visible" class="ml-2 block text-sm text-gray-700">Visible to parent</label>
            </div>
            <div class="flex justify-end space-x-3">
              <button
                type="button"
                @click="showAddRemark = false"
                class="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                type="submit"
                class="inline-flex justify-center rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700"
                :disabled="savingRemark"
              >
                {{ savingRemark ? 'Saving...' : 'Add Remark' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { teacherPortalService, type StudentRemarkItem, type TeacherClassContext, type TeacherStudentItem } from '@/services/teacherPortalService'

const route = useRoute()
const authStore = useAuthStore()

const loading = ref(false)
const savingRemark = ref(false)
const error = ref('')

const classes = ref<TeacherClassContext[]>([])
const selectedClass = ref<number | null>(null)
const students = ref<TeacherStudentItem[]>([])
const remarks = ref<StudentRemarkItem[]>([])
const showAddRemark = ref(false)

const newRemark = ref({
  studentId: 0,
  type: 0,
  subject: '',
  chapterName: '',
  remarks: '',
  isVisibleToParent: true
})

const getInitials = (name: string) =>
  name.split(' ').map(part => part.charAt(0)).join('').slice(0, 2).toUpperCase() || 'NA'

const loadClassData = async () => {
  if (!selectedClass.value) {
    students.value = []
    remarks.value = []
    return
  }

  loading.value = true
  error.value = ''
  try {
    const [classStudents, classRemarks] = await Promise.all([
      teacherPortalService.getClassStudents(selectedClass.value),
      teacherPortalService.getRemarks(selectedClass.value)
    ])
    students.value = classStudents
    remarks.value = classRemarks

    if (students.value.length > 0 && !students.value.some(s => s.id === newRemark.value.studentId)) {
      newRemark.value.studentId = students.value[0].id
    }
  } catch (err: any) {
    students.value = []
    remarks.value = []
    error.value = err?.response?.data?.message || err?.message || 'Failed to load class data'
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
  error.value = ''
  try {
    classes.value = await teacherPortalService.getMyClasses()
    const classFromQuery = Number(route.query.classId)
    if (classFromQuery && classes.value.some(c => c.id === classFromQuery)) {
      selectedClass.value = classFromQuery
    } else {
      selectedClass.value = classes.value.length > 0 ? classes.value[0].id : null
    }

    await loadClassData()

    const studentFromQuery = Number(route.query.studentId)
    if (studentFromQuery && students.value.some(s => s.id === studentFromQuery)) {
      newRemark.value.studentId = studentFromQuery
      showAddRemark.value = true
    }
  } catch (err: any) {
    classes.value = []
    error.value = err?.response?.data?.message || err?.message || 'Failed to load teacher context'
  } finally {
    loading.value = false
  }
}

const addRemark = async () => {
  if (!selectedClass.value || !newRemark.value.studentId || !newRemark.value.remarks.trim()) return

  savingRemark.value = true
  error.value = ''
  try {
    await teacherPortalService.createRemark({
      studentId: newRemark.value.studentId,
      classId: selectedClass.value,
      subject: newRemark.value.subject || undefined,
      chapterName: newRemark.value.chapterName || undefined,
      remarks: newRemark.value.remarks,
      type: newRemark.value.type,
      isVisibleToParent: newRemark.value.isVisibleToParent
    })

    showAddRemark.value = false
    newRemark.value = {
      studentId: students.value[0]?.id || 0,
      type: 0,
      subject: '',
      chapterName: '',
      remarks: '',
      isVisibleToParent: true
    }
    await loadClassData()
  } catch (err: any) {
    error.value = err?.response?.data?.message || err?.message || 'Failed to add remark'
  } finally {
    savingRemark.value = false
  }
}

watch(selectedClass, async (value, oldValue) => {
  if (value && value !== oldValue) {
    await loadClassData()
  }
})

onMounted(async () => {
  await loadTeacherContext()
})
</script>
