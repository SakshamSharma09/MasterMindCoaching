<template>
  <div class="space-y-6">
    <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h1 class="text-2xl font-bold text-surface-900">Syllabus & Timetable Tracker</h1>
        <p class="text-sm text-surface-600 mt-1">
          Manage Unit Test, Half-Yearly and Yearly syllabus/timetable per student or school.
        </p>
      </div>
      <button class="btn-premium px-4 py-2" @click="openCreate">Add Entry</button>
    </div>

    <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-4 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
      <input v-model="filters.schoolName" type="text" placeholder="Filter by school" class="w-full rounded-lg border-gray-300 text-sm" />
      <select v-model="filters.examType" class="w-full rounded-lg border-gray-300 text-sm">
        <option value="">All Exam Types</option>
        <option value="UnitTest">Unit Test</option>
        <option value="HalfYearly">Half-Yearly</option>
        <option value="Yearly">Yearly</option>
        <option value="Custom">Custom</option>
      </select>
      <select v-model="filters.studentId" class="w-full rounded-lg border-gray-300 text-sm">
        <option value="">All Students</option>
        <option v-for="s in students" :key="s.id" :value="String(s.id)">
          {{ s.firstName }} {{ s.lastName }}
        </option>
      </select>
      <button class="px-3 py-2 rounded-lg border border-gray-300 text-sm font-medium hover:bg-gray-50" @click="loadEntries">Apply Filters</button>
    </div>

    <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="min-w-[980px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Date</th>
              <th class="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Exam</th>
              <th class="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Subject</th>
              <th class="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">School</th>
              <th class="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Student</th>
              <th class="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Class</th>
              <th class="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Time</th>
              <th class="px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase">Syllabus</th>
              <th class="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100">
            <tr v-for="entry in entries" :key="entry.id">
              <td class="px-4 py-3 text-sm text-gray-700">{{ entry.examDate }}</td>
              <td class="px-4 py-3 text-sm font-medium text-gray-900">{{ prettyExamType(entry.examType) }}</td>
              <td class="px-4 py-3 text-sm text-gray-700">{{ entry.subject }}</td>
              <td class="px-4 py-3 text-sm text-gray-700">{{ entry.schoolName || '-' }}</td>
              <td class="px-4 py-3 text-sm text-gray-700">{{ entry.studentName || '-' }}</td>
              <td class="px-4 py-3 text-sm text-gray-700">{{ entry.className || '-' }}</td>
              <td class="px-4 py-3 text-sm text-gray-700">{{ timeRange(entry.startTime, entry.endTime) }}</td>
              <td class="px-4 py-3 text-sm text-gray-700 max-w-md">
                <div class="line-clamp-2">{{ entry.syllabus }}</div>
              </td>
              <td class="px-4 py-3 text-right text-sm">
                <button class="text-indigo-600 hover:text-indigo-800 mr-3" @click="openEdit(entry)">Edit</button>
                <button class="text-red-600 hover:text-red-800" @click="remove(entry.id)">Delete</button>
              </td>
            </tr>
            <tr v-if="entries.length === 0">
              <td colspan="9" class="px-4 py-8 text-center text-sm text-gray-500">No entries found.</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-if="showModal" class="fixed inset-0 z-50 bg-black/40 flex items-center justify-center p-4" role="dialog" aria-modal="true">
      <form class="bg-white rounded-xl shadow-xl w-full max-w-3xl p-6 max-h-[90vh] overflow-y-auto" @submit.prevent="save">
        <h3 class="text-lg font-semibold text-gray-900 mb-4">{{ editId ? 'Edit Entry' : 'New Entry' }}</h3>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="text-sm font-medium text-gray-700">Exam Type</label>
            <select v-model="form.examType" class="mt-1 w-full rounded-lg border-gray-300 text-sm" required>
              <option value="UnitTest">Unit Test</option>
              <option value="HalfYearly">Half-Yearly</option>
              <option value="Yearly">Yearly</option>
              <option value="Custom">Custom</option>
            </select>
          </div>
          <div>
            <label class="text-sm font-medium text-gray-700">Subject</label>
            <input v-model="form.subject" type="text" class="mt-1 w-full rounded-lg border-gray-300 text-sm" required />
          </div>
          <div>
            <label class="text-sm font-medium text-gray-700">Exam Date</label>
            <input v-model="form.examDate" type="date" class="mt-1 w-full rounded-lg border-gray-300 text-sm" required />
          </div>
          <div>
            <label class="text-sm font-medium text-gray-700">School Name</label>
            <input v-model="form.schoolName" type="text" class="mt-1 w-full rounded-lg border-gray-300 text-sm" />
          </div>
          <div>
            <label class="text-sm font-medium text-gray-700">Student (optional)</label>
            <select v-model="form.studentId" class="mt-1 w-full rounded-lg border-gray-300 text-sm">
              <option value="">None</option>
              <option v-for="s in students" :key="s.id" :value="String(s.id)">
                {{ s.firstName }} {{ s.lastName }}
              </option>
            </select>
          </div>
          <div>
            <label class="text-sm font-medium text-gray-700">Class (optional)</label>
            <select v-model="form.classId" class="mt-1 w-full rounded-lg border-gray-300 text-sm">
              <option value="">None</option>
              <option v-for="c in classes" :key="c.id" :value="String(c.id)">{{ c.name }}</option>
            </select>
          </div>
          <div>
            <label class="text-sm font-medium text-gray-700">Start Time</label>
            <input v-model="form.startTime" type="time" class="mt-1 w-full rounded-lg border-gray-300 text-sm" />
          </div>
          <div>
            <label class="text-sm font-medium text-gray-700">End Time</label>
            <input v-model="form.endTime" type="time" class="mt-1 w-full rounded-lg border-gray-300 text-sm" />
          </div>
        </div>
        <div class="mt-4">
          <label class="text-sm font-medium text-gray-700">Syllabus / Course</label>
          <textarea v-model="form.syllabus" rows="4" class="mt-1 w-full rounded-lg border-gray-300 text-sm" required></textarea>
        </div>
        <div class="mt-4">
          <label class="text-sm font-medium text-gray-700">Notes</label>
          <textarea v-model="form.notes" rows="2" class="mt-1 w-full rounded-lg border-gray-300 text-sm"></textarea>
        </div>
        <p v-if="errorMessage" class="mt-3 text-sm text-red-600">{{ errorMessage }}</p>
        <div class="mt-5 flex justify-end gap-2">
          <button type="button" class="px-3 py-2 text-sm bg-gray-100 rounded-lg" @click="closeModal">Cancel</button>
          <button type="submit" :disabled="isSaving" class="px-4 py-2 text-sm text-white bg-indigo-600 rounded-lg disabled:opacity-60">
            {{ isSaving ? 'Saving...' : 'Save' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { academicPlannerService, type AcademicPlannerEntry } from '@/services/academicPlannerService'
import { studentsService, type Student } from '@/services/studentsService'
import { classesService, type Class } from '@/services/classesService'

const entries = ref<AcademicPlannerEntry[]>([])
const students = ref<Student[]>([])
const classes = ref<Class[]>([])
const showModal = ref(false)
const editId = ref<number | null>(null)
const errorMessage = ref('')
const isSaving = ref(false)

const filters = ref({
  schoolName: '',
  examType: '',
  studentId: ''
})

const form = ref({
  examType: 'UnitTest',
  subject: '',
  examDate: '',
  schoolName: '',
  studentId: '',
  classId: '',
  startTime: '',
  endTime: '',
  syllabus: '',
  notes: ''
})

const loadEntries = async () => {
  entries.value = await academicPlannerService.getEntries({
    schoolName: filters.value.schoolName || undefined,
    examType: filters.value.examType || undefined,
    studentId: filters.value.studentId ? Number(filters.value.studentId) : undefined
  })
}

const loadMeta = async () => {
  const [studentRes, classRes] = await Promise.allSettled([
    studentsService.getStudents(1, 200),
    classesService.getClasses()
  ])

  if (studentRes.status === 'fulfilled') students.value = studentRes.value.data || []
  if (classRes.status === 'fulfilled') classes.value = classRes.value || []
}

const resetForm = () => {
  form.value = {
    examType: 'UnitTest',
    subject: '',
    examDate: new Date().toISOString().slice(0, 10),
    schoolName: '',
    studentId: '',
    classId: '',
    startTime: '',
    endTime: '',
    syllabus: '',
    notes: ''
  }
}

const openCreate = () => {
  editId.value = null
  errorMessage.value = ''
  resetForm()
  showModal.value = true
}

const openEdit = (entry: AcademicPlannerEntry) => {
  editId.value = entry.id
  errorMessage.value = ''
  form.value = {
    examType: entry.examType,
    subject: entry.subject,
    examDate: entry.examDate,
    schoolName: entry.schoolName || '',
    studentId: entry.studentId ? String(entry.studentId) : '',
    classId: entry.classId ? String(entry.classId) : '',
    startTime: entry.startTime || '',
    endTime: entry.endTime || '',
    syllabus: entry.syllabus,
    notes: entry.notes || ''
  }
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
}

const save = async () => {
  isSaving.value = true
  errorMessage.value = ''
  try {
    const payload = {
      examType: form.value.examType,
      subject: form.value.subject,
      examDate: form.value.examDate,
      schoolName: form.value.schoolName || undefined,
      studentId: form.value.studentId ? Number(form.value.studentId) : null,
      classId: form.value.classId ? Number(form.value.classId) : null,
      startTime: form.value.startTime || null,
      endTime: form.value.endTime || null,
      syllabus: form.value.syllabus,
      notes: form.value.notes || null
    }

    if (editId.value) {
      await academicPlannerService.updateEntry(editId.value, payload)
    } else {
      await academicPlannerService.createEntry(payload)
    }

    await loadEntries()
    showModal.value = false
  } catch (error: any) {
    errorMessage.value = error?.response?.data?.message || 'Unable to save entry.'
  } finally {
    isSaving.value = false
  }
}

const remove = async (id: number) => {
  if (!confirm('Delete this entry?')) return
  await academicPlannerService.deleteEntry(id)
  await loadEntries()
}

const prettyExamType = (type: string) => {
  if (type === 'UnitTest') return 'Unit Test'
  if (type === 'HalfYearly') return 'Half-Yearly'
  return type
}

const timeRange = (start?: string | null, end?: string | null) => {
  if (!start && !end) return '-'
  if (start && end) return `${start} - ${end}`
  return start || end || '-'
}

onMounted(async () => {
  await Promise.allSettled([loadEntries(), loadMeta()])
})
</script>

