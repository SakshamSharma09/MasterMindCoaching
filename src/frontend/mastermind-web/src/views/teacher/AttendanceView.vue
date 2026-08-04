<template>
  <div class="mx-auto w-full max-w-6xl px-3 pb-28 sm:px-6 sm:pb-8 lg:px-8">
    <header class="mb-5 rounded-3xl border border-slate-200 bg-white p-5 shadow-sm sm:flex sm:items-center sm:justify-between sm:p-7">
      <div>
        <p class="text-xs font-black uppercase tracking-[0.18em] text-indigo-600">Teacher workspace</p>
        <h1 class="mt-2 text-2xl font-black text-slate-950 sm:text-3xl">Mark Attendance</h1>
        <p class="mt-2 text-sm text-slate-600">Only students from your assigned classes are shown.</p>
      </div>
      <button
        type="button"
        class="mt-4 hidden min-h-11 rounded-xl bg-indigo-600 px-5 py-3 text-sm font-bold text-white shadow-sm disabled:opacity-50 sm:inline-flex sm:items-center sm:justify-center"
        :disabled="!canSave"
        @click="saveAttendance"
      >
        {{ saving ? 'Saving…' : 'Save Attendance' }}
      </button>
    </header>

    <section class="mb-5 grid gap-4 rounded-2xl border border-slate-200 bg-white p-4 sm:grid-cols-2 sm:p-5">
      <div>
        <label for="class-select" class="mb-2 block text-sm font-bold text-slate-700">Class *</label>
        <select id="class-select" v-model.number="selectedClass" class="min-h-12 w-full rounded-xl border-slate-300 px-3 text-base focus:border-indigo-500 focus:ring-indigo-500">
          <option :value="null" disabled>Select an assigned class</option>
          <option v-for="classItem in classes" :key="classItem.id" :value="classItem.id">
            {{ classItem.name }} · {{ classItem.board }} · {{ classItem.medium }}
          </option>
        </select>
      </div>
      <div>
        <label for="attendance-date" class="mb-2 block text-sm font-bold text-slate-700">Date *</label>
        <input id="attendance-date" v-model="attendanceDate" type="date" class="min-h-12 w-full rounded-xl border-slate-300 px-3 text-base focus:border-indigo-500 focus:ring-indigo-500">
      </div>
    </section>

    <div v-if="error" role="alert" class="mb-4 rounded-2xl border border-red-200 bg-red-50 p-4 text-sm font-semibold text-red-700">{{ error }}</div>
    <div v-if="successMessage" role="status" class="mb-4 rounded-2xl border border-emerald-200 bg-emerald-50 p-4 text-sm font-semibold text-emerald-700">{{ successMessage }}</div>

    <section v-if="students.length" class="mb-4 rounded-2xl border border-slate-200 bg-white p-3 sm:p-4">
      <div class="grid grid-cols-3 gap-2 text-center sm:grid-cols-5">
        <button v-for="option in statusOptions" :key="option.value" type="button" class="min-h-11 rounded-xl border px-2 py-2 text-xs font-bold sm:text-sm" :class="option.quickClass" @click="markAll(option.value)">
          All {{ option.label }}
        </button>
      </div>
      <div class="mt-4 grid grid-cols-3 gap-2 sm:grid-cols-5">
        <div v-for="option in statusOptions" :key="`count-${option.value}`" class="rounded-xl bg-slate-50 p-2 text-center">
          <div class="text-lg font-black text-slate-950">{{ countStatus(option.value) }}</div>
          <div class="text-[11px] font-semibold text-slate-500">{{ option.label }}</div>
        </div>
      </div>
    </section>

    <div v-if="loading" class="rounded-2xl border border-slate-200 bg-white p-10 text-center text-sm text-slate-500">Loading assigned students…</div>
    <div v-else-if="!selectedClass" class="rounded-2xl border border-dashed border-slate-300 bg-white p-10 text-center text-sm text-slate-500">Select one of your assigned classes.</div>
    <div v-else-if="students.length === 0" class="rounded-2xl border border-dashed border-slate-300 bg-white p-10 text-center text-sm text-slate-500">No active students are assigned to this class.</div>

    <ul v-else class="space-y-3" aria-label="Student attendance">
      <li v-for="student in students" :key="student.id" class="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
        <div class="flex min-w-0 items-center gap-3">
          <div class="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-indigo-50 text-sm font-black text-indigo-700">{{ student.initials }}</div>
          <div class="min-w-0">
            <p class="truncate font-bold text-slate-950">{{ student.name }}</p>
            <p class="text-xs text-slate-500">Admission no. {{ student.rollNo }}</p>
          </div>
        </div>
        <fieldset class="mt-4 grid grid-cols-3 gap-2 sm:grid-cols-5">
          <legend class="sr-only">Attendance for {{ student.name }}</legend>
          <label v-for="option in statusOptions" :key="option.value" class="flex min-h-11 cursor-pointer items-center justify-center rounded-xl border px-2 text-xs font-bold transition" :class="student.status === option.value ? option.selectedClass : 'border-slate-200 bg-white text-slate-600'">
            <input v-model="student.status" class="sr-only" type="radio" :name="`attendance-${student.id}`" :value="option.value">
            {{ option.label }}
          </label>
        </fieldset>
      </li>
    </ul>

    <div class="fixed inset-x-0 bottom-0 z-30 border-t border-slate-200 bg-white/95 p-3 pb-[max(0.75rem,env(safe-area-inset-bottom))] shadow-[0_-8px_30px_rgba(15,23,42,0.12)] backdrop-blur sm:hidden">
      <button type="button" class="min-h-12 w-full rounded-xl bg-indigo-600 px-5 font-black text-white disabled:opacity-50" :disabled="!canSave" @click="saveAttendance">
        {{ saving ? 'Saving attendance…' : `Save attendance (${students.length})` }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { teacherPortalService, type TeacherClassContext } from '@/services/teacherPortalService'

type AttendanceValue = 'Present' | 'Absent' | 'Late' | 'HalfDay' | 'Leave'

interface StudentAttendanceRow {
  id: number
  name: string
  initials: string
  rollNo: string
  status: AttendanceValue
}

const route = useRoute()
const loading = ref(false)
const saving = ref(false)
const error = ref('')
const successMessage = ref('')
const classes = ref<TeacherClassContext[]>([])
const selectedClass = ref<number | null>(null)
const students = ref<StudentAttendanceRow[]>([])
const attendanceDate = ref(new Date().toISOString().slice(0, 10))

const statusOptions: Array<{ value: AttendanceValue; label: string; selectedClass: string; quickClass: string }> = [
  { value: 'Present', label: 'Present', selectedClass: 'border-emerald-500 bg-emerald-50 text-emerald-700', quickClass: 'border-emerald-200 bg-emerald-50 text-emerald-700' },
  { value: 'Absent', label: 'Absent', selectedClass: 'border-red-500 bg-red-50 text-red-700', quickClass: 'border-red-200 bg-red-50 text-red-700' },
  { value: 'Late', label: 'Late', selectedClass: 'border-amber-500 bg-amber-50 text-amber-700', quickClass: 'border-amber-200 bg-amber-50 text-amber-700' },
  { value: 'HalfDay', label: 'Half Day', selectedClass: 'border-blue-500 bg-blue-50 text-blue-700', quickClass: 'border-blue-200 bg-blue-50 text-blue-700' },
  { value: 'Leave', label: 'Leave', selectedClass: 'border-violet-500 bg-violet-50 text-violet-700', quickClass: 'border-violet-200 bg-violet-50 text-violet-700' }
]

const canSave = computed(() => !saving.value && !loading.value && !!selectedClass.value && students.value.length > 0)
const countStatus = (status: AttendanceValue) => students.value.filter(student => student.status === status).length
const markAll = (status: AttendanceValue) => students.value.forEach(student => { student.status = status })

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
      teacherPortalService.getClassAttendance(selectedClass.value, attendanceDate.value)
    ])
    const attendanceMap = new Map(attendanceRecords.map(record => [record.studentId, record.status as AttendanceValue]))
    students.value = classStudents.map(student => ({
      id: student.id,
      name: student.name,
      initials: student.initials,
      rollNo: student.rollNo,
      status: attendanceMap.get(student.id) || 'Present'
    }))
  } catch (err: any) {
    students.value = []
    error.value = err?.response?.data?.message || err?.message || 'Attendance could not be loaded.'
  } finally {
    loading.value = false
  }
}

const saveAttendance = async () => {
  if (!selectedClass.value || !students.value.length) return
  saving.value = true
  error.value = ''
  successMessage.value = ''
  try {
    await teacherPortalService.saveClassAttendance(selectedClass.value, attendanceDate.value, students.value.map(student => ({ studentId: student.id, status: student.status })))
    successMessage.value = `Attendance saved for ${students.value.length} students. Class time is recorded as 3:00 PM to 6:00 PM.`
  } catch (err: any) {
    error.value = err?.response?.data?.message || err?.message || 'Attendance could not be saved.'
  } finally {
    saving.value = false
  }
}

watch([selectedClass, attendanceDate], loadStudentsAndAttendance)

onMounted(async () => {
  loading.value = true
  try {
    classes.value = await teacherPortalService.getMyClasses()
    const requestedClass = Number(route.query.classId)
    selectedClass.value = classes.value.some(item => item.id === requestedClass) ? requestedClass : (classes.value[0]?.id || null)
    if (!selectedClass.value) students.value = []
  } catch (err: any) {
    error.value = err?.response?.data?.message || err?.message || 'Assigned classes could not be loaded.'
  } finally {
    loading.value = false
  }
})
</script>
