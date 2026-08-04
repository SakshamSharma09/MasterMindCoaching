<template>
  <div class="mx-auto w-full max-w-7xl px-3 pb-8 sm:px-6 lg:px-8">
    <header class="overflow-hidden rounded-3xl bg-gradient-to-br from-slate-950 via-indigo-950 to-indigo-700 p-5 text-white shadow-xl sm:p-8">
      <p class="text-xs font-black uppercase tracking-[0.2em] text-indigo-200">Teacher dashboard</p>
      <div class="mt-3 gap-6 sm:flex sm:items-end sm:justify-between">
        <div>
          <h1 class="text-2xl font-black sm:text-4xl">Your classes, ready for today</h1>
          <p class="mt-3 max-w-2xl text-sm leading-6 text-indigo-100 sm:text-base">Open an assigned class, mark attendance, and share clear remarks with parents.</p>
        </div>
        <RouterLink to="/teacher/attendance" class="mt-5 inline-flex min-h-12 w-full items-center justify-center rounded-xl bg-white px-5 font-black text-indigo-800 shadow-sm sm:mt-0 sm:w-auto">
          Mark attendance
        </RouterLink>
      </div>
    </header>

    <div v-if="error" role="alert" class="mt-5 rounded-2xl border border-red-200 bg-red-50 p-4 text-sm font-semibold text-red-700">
      {{ error }}
      <button class="ml-2 underline" type="button" @click="loadTeacherDashboardData">Try again</button>
    </div>

    <section class="mt-5 grid grid-cols-2 gap-3 lg:grid-cols-4">
      <article v-for="item in statCards" :key="item.label" class="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5">
        <p class="text-xs font-bold uppercase tracking-wide text-slate-500">{{ item.label }}</p>
        <p class="mt-2 text-2xl font-black text-slate-950 sm:text-3xl">{{ item.value }}</p>
        <p class="mt-1 text-xs text-slate-500">{{ item.help }}</p>
      </article>
    </section>

    <section class="mt-5 rounded-3xl border border-slate-200 bg-white p-4 shadow-sm sm:p-6">
      <div class="flex items-center justify-between gap-3">
        <div>
          <h2 class="text-xl font-black text-slate-950">Assigned classes</h2>
          <p class="mt-1 text-sm text-slate-500">Attendance access is restricted to these classes.</p>
        </div>
        <RouterLink to="/teacher/students" class="hidden min-h-11 items-center rounded-xl border border-slate-200 px-4 text-sm font-bold text-slate-700 sm:inline-flex">View students</RouterLink>
      </div>

      <div v-if="loading" class="py-10 text-center text-sm text-slate-500">Loading your classes…</div>
      <div v-else-if="classes.length === 0" class="mt-5 rounded-2xl border border-dashed border-slate-300 p-8 text-center text-sm text-slate-500">No active class has been assigned. Please contact the administrator.</div>
      <div v-else class="mt-5 grid gap-3 lg:grid-cols-2">
        <article v-for="classItem in classes" :key="classItem.id" class="rounded-2xl border border-slate-200 bg-slate-50 p-4 sm:flex sm:items-center sm:justify-between">
          <div class="min-w-0">
            <p class="truncate font-black text-slate-950">{{ classItem.name }}</p>
            <p class="mt-1 text-sm text-slate-500">{{ classItem.board }} · {{ classItem.medium }} · {{ classItem.studentCount }} students</p>
          </div>
          <RouterLink :to="{ path: '/teacher/attendance', query: { classId: classItem.id } }" class="mt-3 inline-flex min-h-11 w-full items-center justify-center rounded-xl bg-indigo-600 px-4 text-sm font-black text-white sm:mt-0 sm:w-auto">
            Take attendance
          </RouterLink>
        </article>
      </div>
    </section>

    <section class="mt-5 rounded-3xl border border-slate-200 bg-white p-4 shadow-sm sm:p-6">
      <h2 class="text-xl font-black text-slate-950">Recent parent-visible remarks</h2>
      <div v-if="recentActivities.length === 0" class="mt-4 rounded-2xl bg-slate-50 p-6 text-center text-sm text-slate-500">No recent remarks yet.</div>
      <ul v-else class="mt-4 divide-y divide-slate-100">
        <li v-for="activity in recentActivities" :key="activity.id" class="py-4">
          <p class="font-bold text-slate-900">{{ activity.title }}</p>
          <p class="mt-1 text-sm text-slate-600">{{ activity.description }}</p>
          <p class="mt-1 text-xs text-slate-400">{{ activity.time }}</p>
        </li>
      </ul>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { apiService } from '@/services/apiService'
import { API_ENDPOINTS } from '@/config/api'
import { teacherPortalService } from '@/services/teacherPortalService'

interface TeacherStats { totalStudents: number; classesToday: number; attendanceMarked: number; remarksAdded: number }
interface ClassCard { id: number; name: string; board: string; medium: string; studentCount: number }
interface ActivityItem { id: number; title: string; description: string; time: string }

const loading = ref(false)
const error = ref('')
const stats = ref<TeacherStats>({ totalStudents: 0, classesToday: 0, attendanceMarked: 0, remarksAdded: 0 })
const classes = ref<ClassCard[]>([])
const recentActivities = ref<ActivityItem[]>([])

const statCards = computed(() => [
  { label: 'Students', value: stats.value.totalStudents, help: 'Across assigned classes' },
  { label: 'Classes', value: stats.value.classesToday, help: 'Active assignments' },
  { label: 'Attendance', value: `${stats.value.attendanceMarked}%`, help: 'Students marked today' },
  { label: 'Remarks', value: stats.value.remarksAdded, help: 'Remarks added by you' }
])

const loadTeacherDashboardData = async () => {
  loading.value = true
  error.value = ''
  try {
    const [statsData, classRows] = await Promise.all([
      apiService.get<TeacherStats>(API_ENDPOINTS.DASHBOARD.TEACHER_STATS),
      teacherPortalService.getMyClasses()
    ])
    const statsResponse: any = statsData
    stats.value = statsResponse.data || statsResponse
    const studentLists = await Promise.all(classRows.map(item => teacherPortalService.getClassStudents(item.id)))
    classes.value = classRows.map((item, index) => ({ ...item, studentCount: studentLists[index].length }))
    const remarks = classRows.length ? await teacherPortalService.getRemarks(classRows[0].id) : []
    recentActivities.value = remarks.slice(0, 6).map(remark => ({
      id: remark.id,
      title: `${remark.studentName} · ${remark.type}`,
      description: remark.remarks,
      time: remark.date
    }))
  } catch (err: any) {
    error.value = err?.response?.data?.message || err?.message || 'Teacher dashboard data could not be loaded.'
  } finally {
    loading.value = false
  }
}

onMounted(loadTeacherDashboardData)
</script>
