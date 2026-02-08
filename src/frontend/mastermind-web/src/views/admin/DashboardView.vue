<template>
  <div class="space-y-6">
    <!-- Page Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p class="text-gray-600">Welcome back, {{ authStore.userName }}!</p>
      </div>
      <div class="text-sm text-gray-500">
        {{ currentDate }}
      </div>
    </div>

    <!-- Stats Cards -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      <!-- Total Students -->
      <div class="card">
        <div class="flex items-center">
          <div class="flex-shrink-0">
            <div class="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center">
              <svg class="w-4 h-4 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z"></path>
              </svg>
            </div>
          </div>
          <div class="ml-4">
            <p class="text-sm font-medium text-gray-600">Total Students</p>
            <p class="text-2xl font-bold text-gray-900">{{ stats.totalStudents }}</p>
          </div>
        </div>
      </div>

      <!-- Total Teachers -->
      <div class="card">
        <div class="flex items-center">
          <div class="flex-shrink-0">
            <div class="w-8 h-8 bg-green-500 rounded-full flex items-center justify-center">
              <svg class="w-4 h-4 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path>
              </svg>
            </div>
          </div>
          <div class="ml-4">
            <p class="text-sm font-medium text-gray-600">Total Teachers</p>
            <p class="text-2xl font-bold text-gray-900">{{ stats.totalTeachers }}</p>
          </div>
        </div>
      </div>

      <!-- Today's Attendance -->
      <div class="card">
        <div class="flex items-center">
          <div class="flex-shrink-0">
            <div class="w-8 h-8 bg-yellow-500 rounded-full flex items-center justify-center">
              <svg class="w-4 h-4 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"></path>
              </svg>
            </div>
          </div>
          <div class="ml-4">
            <p class="text-sm font-medium text-gray-600">Today's Attendance</p>
            <p class="text-2xl font-bold text-gray-900">{{ stats.todayAttendance }}%</p>
          </div>
        </div>
      </div>

      <!-- Pending Fees -->
      <div class="card">
        <div class="flex items-center">
          <div class="flex-shrink-0">
            <div class="w-8 h-8 bg-red-500 rounded-full flex items-center justify-center">
              <svg class="w-4 h-4 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1"></path>
              </svg>
            </div>
          </div>
          <div class="ml-4">
            <p class="text-sm font-medium text-gray-600">Pending Fees</p>
            <p class="text-2xl font-bold text-gray-900">₹{{ stats.pendingFees }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Charts and Recent Activity -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Recent Students -->
      <div class="card">
        <div class="card-header">
          <h3 class="card-title">Recent Students</h3>
          <p class="card-description">Latest student enrollments</p>
        </div>
        <div class="space-y-4">
          <div v-for="student in recentStudents" :key="student.id" class="flex items-center space-x-4">
            <div class="flex-shrink-0">
              <div class="w-10 h-10 bg-gray-300 rounded-full flex items-center justify-center">
                <span class="text-sm font-medium text-gray-700">
                  {{ (student.firstName || '').charAt(0) }}{{ (student.lastName || '').charAt(0) }}
                </span>
              </div>
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-gray-900 truncate">
                {{ student.firstName || '' }} {{ student.lastName || '' }}
              </p>
              <p class="text-sm text-gray-500 truncate">
                Class: {{ student.className || 'Not Assigned' }}
              </p>
            </div>
            <div class="text-sm text-gray-500">
              {{ formatDate(student.createdAt) }}
            </div>
          </div>
          <div v-if="recentStudents.length === 0" class="text-center py-4 text-gray-500">
            No recent students
          </div>
        </div>
      </div>

      <!-- Recent Leads -->
      <div class="card">
        <div class="card-header">
          <h3 class="card-title">Recent Leads</h3>
          <p class="card-description">Latest inquiries and leads</p>
        </div>
        <div class="space-y-4">
          <div v-for="lead in recentLeads" :key="lead.id" class="flex items-center space-x-4">
            <div class="flex-shrink-0">
              <div class="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
                <svg class="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path>
                </svg>
              </div>
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-gray-900 truncate">
                {{ lead.name }}
              </p>
              <p class="text-sm text-gray-500 truncate">
                {{ lead.mobile }}
              </p>
            </div>
            <div class="text-sm">
              <span
                class="badge"
                :class="getLeadStatusClass(lead.status)"
              >
                {{ lead.status }}
              </span>
            </div>
          </div>
          <div v-if="recentLeads.length === 0" class="text-center py-4 text-gray-500">
            No recent leads
          </div>
        </div>
      </div>
    </div>

    <!-- Quick Actions -->
    <div class="card">
      <div class="card-header">
        <h3 class="card-title">Quick Actions</h3>
        <p class="card-description">Common administrative tasks</p>
      </div>
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <router-link
          to="/admin/students"
          class="flex flex-col items-center p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
        >
          <svg class="w-8 h-8 text-blue-600 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z"></path>
          </svg>
          <span class="text-sm font-medium text-gray-900">Add Student</span>
        </router-link>

        <router-link
          to="/admin/classes"
          class="flex flex-col items-center p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
        >
          <svg class="w-8 h-8 text-green-600 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.746 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"></path>
          </svg>
          <span class="text-sm font-medium text-gray-900">Manage Classes</span>
        </router-link>

        <router-link
          to="/admin/attendance"
          class="flex flex-col items-center p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
        >
          <svg class="w-8 h-8 text-yellow-600 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"></path>
          </svg>
          <span class="text-sm font-medium text-gray-900">Mark Attendance</span>
        </router-link>

        <router-link
          to="/admin/leads"
          class="flex flex-col items-center p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
        >
          <svg class="w-8 h-8 text-purple-600 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z"></path>
          </svg>
          <span class="text-sm font-medium text-gray-900">View Leads</span>
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { apiService } from '@/services/apiService'
import { API_ENDPOINTS } from '@/config/api'
import { format } from 'date-fns'

// Type definitions
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
}

const authStore = useAuthStore()

// Real data from API
const stats = ref<DashboardStats>({
  totalStudents: 0,
  totalTeachers: 0,
  todayAttendance: 0,
  pendingFees: 0
})

const recentStudents = ref<StudentItem[]>([])
const recentLeads = ref<LeadItem[]>([])

const currentDate = format(new Date(), 'EEEE, MMMM do, yyyy')

const getLeadStatusClass = (status: string) => {
  const classes = {
    'New': 'badge-info',
    'Contacted': 'badge-warning',
    'Interested': 'badge-success',
    'Converted': 'badge-success',
    'Lost': 'badge-danger'
  }
  return classes[status as keyof typeof classes] || 'badge-info'
}

const formatDate = (dateString: string) => {
  return format(new Date(dateString), 'MMM dd, yyyy')
}

// Load dashboard data
const loadDashboardData = async () => {
  try {
    // Get dashboard stats from API
    const statsData = await apiService.get(API_ENDPOINTS.DASHBOARD.ADMIN_STATS)
    stats.value = statsData

    // Get recent students from API
    const studentsData = await apiService.get('/dashboard/recent-students')
    recentStudents.value = studentsData

    // Mock recent leads (you can create a leads controller later)
    recentLeads.value = [
      {
        id: 1,
        name: 'Alice Johnson',
        mobile: '+91 9876543210',
        status: 'New'
      },
      {
        id: 2,
        name: 'Bob Wilson',
        mobile: '+91 9876543211',
        status: 'Contacted'
      }
    ]
  } catch (error) {
    console.error('Failed to load dashboard data:', error)
    // Fallback to mock data if API fails
    stats.value = {
      totalStudents: 45,
      totalTeachers: 5,
      todayAttendance: 85,
      pendingFees: 25000
    }
  }
}

onMounted(() => {
  loadDashboardData()
})
</script>

<style scoped>
/* Additional styles if needed */
</style>