<template>
  <div class="space-y-8 animate-fade-in-up">
    <!-- Welcome Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <h1 class="text-2xl sm:text-3xl font-display font-bold text-surface-900">
          Welcome back, <span class="bg-gradient-to-r from-primary-600 to-mastermind-600 bg-clip-text text-transparent">{{ firstName }}</span>
        </h1>
        <p class="mt-1 text-surface-500">Here's what's happening with your coaching institute today.</p>
      </div>
      <div class="flex items-center gap-3">
        <div class="px-4 py-2 rounded-xl bg-surface-100 text-surface-600 text-sm font-medium">
          <span class="hidden sm:inline">{{ currentDate }}</span>
          <span class="sm:hidden">{{ shortDate }}</span>
        </div>
        <button class="btn-premium px-4 py-2.5">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
          </svg>
          <span class="hidden sm:inline">New Student</span>
        </button>
      </div>
    </div>

    <!-- Stats Grid -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6">
      <!-- Total Students -->
      <div class="card-stat group">
        <div class="flex items-start justify-between">
          <div>
            <p class="text-xs sm:text-sm font-medium text-surface-500 uppercase tracking-wider">Students</p>
            <p class="mt-2 text-2xl sm:text-3xl font-bold text-surface-900">{{ stats.totalStudents }}</p>
            <div class="mt-2 flex items-center gap-1 text-success-600">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6"/>
              </svg>
              <span class="text-xs font-semibold">+12%</span>
            </div>
          </div>
          <div class="w-12 h-12 rounded-2xl bg-gradient-to-br from-primary-500 to-primary-600 flex items-center justify-center shadow-glow-primary group-hover:scale-110 transition-transform duration-300">
            <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"/>
            </svg>
          </div>
        </div>
      </div>

      <!-- Total Teachers -->
      <div class="card-stat group">
        <div class="flex items-start justify-between">
          <div>
            <p class="text-xs sm:text-sm font-medium text-surface-500 uppercase tracking-wider">Teachers</p>
            <p class="mt-2 text-2xl sm:text-3xl font-bold text-surface-900">{{ stats.totalTeachers }}</p>
            <div class="mt-2 flex items-center gap-1 text-surface-400">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 12h14"/>
              </svg>
              <span class="text-xs font-semibold">Stable</span>
            </div>
          </div>
          <div class="w-12 h-12 rounded-2xl bg-gradient-to-br from-success-500 to-success-600 flex items-center justify-center shadow-glow-success group-hover:scale-110 transition-transform duration-300">
            <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"/>
            </svg>
          </div>
        </div>
      </div>

      <!-- Today's Attendance -->
      <div class="card-stat group">
        <div class="flex items-start justify-between">
          <div>
            <p class="text-xs sm:text-sm font-medium text-surface-500 uppercase tracking-wider">Attendance</p>
            <p class="mt-2 text-2xl sm:text-3xl font-bold text-surface-900">{{ stats.todayAttendance }}%</p>
            <div class="mt-2 flex items-center gap-1 text-warning-600">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 17h8m0 0V9m0 8l-8-8-4 4-6-6"/>
              </svg>
              <span class="text-xs font-semibold">-3%</span>
            </div>
          </div>
          <div class="w-12 h-12 rounded-2xl bg-gradient-to-br from-warning-500 to-warning-600 flex items-center justify-center group-hover:scale-110 transition-transform duration-300">
            <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"/>
            </svg>
          </div>
        </div>
      </div>

      <!-- Pending Fees -->
      <div class="card-stat group">
        <div class="flex items-start justify-between">
          <div>
            <p class="text-xs sm:text-sm font-medium text-surface-500 uppercase tracking-wider">Pending</p>
            <p class="mt-2 text-2xl sm:text-3xl font-bold text-surface-900">₹{{ formatCurrency(stats.pendingFees) }}</p>
            <div class="mt-2 flex items-center gap-1 text-error-600">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
              </svg>
              <span class="text-xs font-semibold">5 overdue</span>
            </div>
          </div>
          <div class="w-12 h-12 rounded-2xl bg-gradient-to-br from-accent-500 to-accent-600 flex items-center justify-center shadow-glow-accent group-hover:scale-110 transition-transform duration-300">
            <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
            </svg>
          </div>
        </div>
      </div>
    </div>

    <!-- Main Content Grid -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-6">
      <!-- Recent Students - Takes 2 columns -->
      <div class="xl:col-span-2 card-premium">
        <div class="flex items-center justify-between mb-6">
          <div>
            <h2 class="text-lg font-semibold text-surface-900">Recent Students</h2>
            <p class="text-sm text-surface-500">Latest enrollments this month</p>
          </div>
          <router-link to="/admin/students" class="btn-ghost text-sm">
            View All
            <svg class="w-4 h-4 ml-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
            </svg>
          </router-link>
        </div>
        
        <div class="space-y-3">
          <div 
            v-for="(student, index) in recentStudents" 
            :key="student.id"
            class="flex items-center gap-4 p-3 rounded-xl hover:bg-surface-50 transition-colors group"
            :style="{ animationDelay: `${index * 100}ms` }"
          >
            <div class="avatar avatar-md" :class="getAvatarColor(index)">
              {{ getInitials(student.firstName, student.lastName) }}
            </div>
            <div class="flex-1 min-w-0">
              <p class="font-medium text-surface-900 truncate">
                {{ student.firstName }} {{ student.lastName }}
              </p>
              <p class="text-sm text-surface-500 truncate">
                {{ student.className || 'Not Assigned' }}
              </p>
            </div>
            <div class="text-right hidden sm:block">
              <p class="text-sm text-surface-600">{{ formatDate(student.createdAt) }}</p>
              <p class="text-xs text-surface-400">Enrolled</p>
            </div>
            <button class="p-2 rounded-lg text-surface-400 hover:text-primary-600 hover:bg-primary-50 opacity-0 group-hover:opacity-100 transition-all">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
              </svg>
            </button>
          </div>
          
          <div v-if="recentStudents.length === 0" class="text-center py-8">
            <div class="w-16 h-16 mx-auto rounded-full bg-surface-100 flex items-center justify-center mb-4">
              <svg class="w-8 h-8 text-surface-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"/>
              </svg>
            </div>
            <p class="text-surface-500">No recent students</p>
            <button class="mt-4 btn-outline text-sm">Add First Student</button>
          </div>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="card-premium">
        <div class="mb-6">
          <h2 class="text-lg font-semibold text-surface-900">Quick Actions</h2>
          <p class="text-sm text-surface-500">Common tasks</p>
        </div>
        
        <div class="space-y-3">
          <router-link 
            to="/admin/students" 
            class="flex items-center gap-4 p-4 rounded-xl border border-surface-200 hover:border-primary-300 hover:bg-primary-50/50 transition-all group"
          >
            <div class="w-10 h-10 rounded-xl bg-primary-100 text-primary-600 flex items-center justify-center group-hover:bg-primary-500 group-hover:text-white transition-colors">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z"/>
              </svg>
            </div>
            <div class="flex-1">
              <p class="font-medium text-surface-900">Add Student</p>
              <p class="text-sm text-surface-500">Enroll new student</p>
            </div>
            <svg class="w-5 h-5 text-surface-400 group-hover:text-primary-600 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
            </svg>
          </router-link>

          <router-link 
            to="/admin/attendance" 
            class="flex items-center gap-4 p-4 rounded-xl border border-surface-200 hover:border-success-300 hover:bg-success-50/50 transition-all group"
          >
            <div class="w-10 h-10 rounded-xl bg-success-100 text-success-600 flex items-center justify-center group-hover:bg-success-500 group-hover:text-white transition-colors">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"/>
              </svg>
            </div>
            <div class="flex-1">
              <p class="font-medium text-surface-900">Mark Attendance</p>
              <p class="text-sm text-surface-500">Today's attendance</p>
            </div>
            <svg class="w-5 h-5 text-surface-400 group-hover:text-success-600 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
            </svg>
          </router-link>

          <router-link 
            to="/admin/finance" 
            class="flex items-center gap-4 p-4 rounded-xl border border-surface-200 hover:border-warning-300 hover:bg-warning-50/50 transition-all group"
          >
            <div class="w-10 h-10 rounded-xl bg-warning-100 text-warning-600 flex items-center justify-center group-hover:bg-warning-500 group-hover:text-white transition-colors">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 9V7a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2m2 4h10a2 2 0 002-2v-6a2 2 0 00-2-2H9a2 2 0 00-2 2v6a2 2 0 002 2zm7-5a2 2 0 11-4 0 2 2 0 014 0z"/>
              </svg>
            </div>
            <div class="flex-1">
              <p class="font-medium text-surface-900">Collect Fees</p>
              <p class="text-sm text-surface-500">Record payment</p>
            </div>
            <svg class="w-5 h-5 text-surface-400 group-hover:text-warning-600 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
            </svg>
          </router-link>

          <router-link 
            to="/admin/leads" 
            class="flex items-center gap-4 p-4 rounded-xl border border-surface-200 hover:border-mastermind-300 hover:bg-mastermind-50/50 transition-all group"
          >
            <div class="w-10 h-10 rounded-xl bg-mastermind-100 text-mastermind-600 flex items-center justify-center group-hover:bg-mastermind-500 group-hover:text-white transition-colors">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z"/>
              </svg>
            </div>
            <div class="flex-1">
              <p class="font-medium text-surface-900">View Leads</p>
              <p class="text-sm text-surface-500">Manage inquiries</p>
            </div>
            <svg class="w-5 h-5 text-surface-400 group-hover:text-mastermind-600 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
            </svg>
          </router-link>
        </div>
      </div>
    </div>

    <!-- Recent Leads Section -->
    <div class="card-premium">
      <div class="flex items-center justify-between mb-6">
        <div>
          <h2 class="text-lg font-semibold text-surface-900">Recent Leads</h2>
          <p class="text-sm text-surface-500">Latest inquiries and follow-ups</p>
        </div>
        <router-link to="/admin/leads" class="btn-ghost text-sm">
          View All
          <svg class="w-4 h-4 ml-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
          </svg>
        </router-link>
      </div>
      
      <div class="overflow-x-auto -mx-6">
        <table class="w-full min-w-[600px]">
          <thead>
            <tr class="border-b border-surface-200">
              <th class="px-6 py-3 text-left text-xs font-semibold text-surface-500 uppercase tracking-wider">Name</th>
              <th class="px-6 py-3 text-left text-xs font-semibold text-surface-500 uppercase tracking-wider">Contact</th>
              <th class="px-6 py-3 text-left text-xs font-semibold text-surface-500 uppercase tracking-wider">Interest</th>
              <th class="px-6 py-3 text-left text-xs font-semibold text-surface-500 uppercase tracking-wider">Status</th>
              <th class="px-6 py-3 text-right text-xs font-semibold text-surface-500 uppercase tracking-wider">Action</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-surface-100">
            <tr v-for="lead in recentLeads" :key="lead.id" class="hover:bg-surface-50 transition-colors">
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                  <div class="w-9 h-9 rounded-full bg-gradient-to-br from-mastermind-400 to-mastermind-600 flex items-center justify-center text-white text-sm font-medium">
                    {{ lead.name.charAt(0) }}
                  </div>
                  <span class="font-medium text-surface-900">{{ lead.name }}</span>
                </div>
              </td>
              <td class="px-6 py-4 text-surface-600">{{ lead.mobile }}</td>
              <td class="px-6 py-4 text-surface-600">{{ lead.interest || 'General' }}</td>
              <td class="px-6 py-4">
                <span :class="getLeadStatusClass(lead.status)" class="badge">
                  {{ lead.status }}
                </span>
              </td>
              <td class="px-6 py-4 text-right">
                <button class="btn-ghost p-2 rounded-lg">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z"/>
                  </svg>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <div v-if="recentLeads.length === 0" class="text-center py-8">
        <p class="text-surface-500">No recent leads</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useSessionStore } from '@/stores/session'
import { apiService } from '@/services/apiService'
import { API_ENDPOINTS } from '@/config/api'
import { format } from 'date-fns'

const authStore = useAuthStore()
const sessionStore = useSessionStore()

interface DashboardStats {
  totalStudents: number
  totalTeachers: number
  todayAttendance: number
  pendingFees: number
}

interface StudentItem {
  id: number
  firstName: string
  lastName: string
  className: string
  createdAt: string
}

interface LeadItem {
  id: number
  name: string
  mobile: string
  status: string
  interest?: string
}

const stats = ref<DashboardStats>({
  totalStudents: 0,
  totalTeachers: 0,
  todayAttendance: 0,
  pendingFees: 0
})

const recentStudents = ref<StudentItem[]>([])
const recentLeads = ref<LeadItem[]>([])

const firstName = computed(() => authStore.user?.firstName || 'Admin')
const currentDate = computed(() => format(new Date(), 'EEEE, MMMM do, yyyy'))
const shortDate = computed(() => format(new Date(), 'MMM do'))

const avatarColors = [
  'bg-gradient-to-br from-primary-400 to-primary-600 text-white',
  'bg-gradient-to-br from-success-400 to-success-600 text-white',
  'bg-gradient-to-br from-warning-400 to-warning-600 text-white',
  'bg-gradient-to-br from-accent-400 to-accent-600 text-white',
  'bg-gradient-to-br from-mastermind-400 to-mastermind-600 text-white',
]

const getAvatarColor = (index: number) => avatarColors[index % avatarColors.length]

const getInitials = (firstName: string, lastName: string) => {
  return `${firstName?.charAt(0) || ''}${lastName?.charAt(0) || ''}`.toUpperCase() || 'U'
}

const formatDate = (dateString: string) => {
  try {
    return format(new Date(dateString), 'MMM dd')
  } catch {
    return 'N/A'
  }
}

const formatCurrency = (amount: number) => {
  if (amount >= 100000) {
    return `${(amount / 100000).toFixed(1)}L`
  } else if (amount >= 1000) {
    return `${(amount / 1000).toFixed(1)}K`
  }
  return amount.toString()
}

const getLeadStatusClass = (status: string) => {
  const classes: Record<string, string> = {
    'New': 'badge-primary',
    'Contacted': 'badge-warning',
    'Interested': 'badge-success',
    'Converted': 'badge-success',
    'Lost': 'badge-error'
  }
  return classes[status] || 'badge-primary'
}

const loadDashboardData = async () => {
  try {
    if (!sessionStore.selectedSessionId) {
      return
    }

    const statsData = await apiService.get(API_ENDPOINTS.DASHBOARD.ADMIN_STATS)
    stats.value = statsData

    const studentsData = await apiService.get('/dashboard/recent-students')
    recentStudents.value = studentsData

    recentLeads.value = [
      { id: 1, name: 'Alice Johnson', mobile: '+91 9876543210', status: 'New', interest: 'Class 10' },
      { id: 2, name: 'Bob Wilson', mobile: '+91 9876543211', status: 'Contacted', interest: 'Class 12' },
      { id: 3, name: 'Carol Smith', mobile: '+91 9876543212', status: 'Interested', interest: 'JEE Prep' },
    ]
  } catch (error) {
    console.error('Failed to load dashboard data:', error)
    stats.value = {
      totalStudents: 45,
      totalTeachers: 5,
      todayAttendance: 85,
      pendingFees: 25000
    }
  }
}

onMounted(() => {
  if (sessionStore.sessions.length === 0) {
    sessionStore.loadSessions().then(() => loadDashboardData())
  } else {
    loadDashboardData()
  }
  
  sessionStore.$subscribe((mutation) => {
    if (Array.isArray(mutation.events)) {
      const hasSessionChange = mutation.events.some(e => 'key' in e && e.key === 'selectedSessionId')
      if (hasSessionChange) loadDashboardData()
    } else if ('key' in mutation.events && mutation.events.key === 'selectedSessionId') {
      loadDashboardData()
    }
  })
})
</script>

<style scoped>
.animate-fade-in-up {
  animation: fadeInUp 0.5s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
