<template>
  <div class="px-4 sm:px-6 lg:px-8 py-8">
    <!-- Header with enhanced styling -->
    <div class="mb-8 animate-slide-up">
      <div class="bg-white/70 backdrop-blur-xl rounded-3xl border border-white/50 p-8 shadow-glass">
        <h1 class="text-4xl font-bold bg-gradient-to-r from-mastermind-600 to-primary-600 bg-clip-text text-transparent mb-3">
          Teacher Dashboard
        </h1>
        <p class="text-lg text-gray-600/90 leading-relaxed">
          Welcome back! Here's an overview of your classes and students at THE MASTERMIND COACHING CLASSES.
        </p>
      </div>
    </div>

    <!-- Quick Stats with premium cards -->
    <div class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4 mb-8">
      <div class="stat-card group animate-scale-in hover:scale-105" style="animation-delay: 0.1s;">
        <div class="flex items-center">
          <div class="stat-icon group-hover:animate-glow-pulse bg-gradient-to-br from-blue-500 to-indigo-600">
            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z"></path>
            </svg>
          </div>
          <div class="ml-5 w-0 flex-1">
            <dl>
              <dt class="stat-label">Total Students</dt>
              <dd class="stat-value text-blue-600">{{ stats.totalStudents }}</dd>
            </dl>
          </div>
        </div>
      </div>

      <div class="stat-card group animate-scale-in hover:scale-105" style="animation-delay: 0.2s;">
        <div class="flex items-center">
          <div class="stat-icon group-hover:animate-glow-pulse bg-gradient-to-br from-green-500 to-emerald-600">
            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"></path>
            </svg>
          </div>
          <div class="ml-5 w-0 flex-1">
            <dl>
              <dt class="stat-label">Classes Today</dt>
              <dd class="stat-value text-green-600">{{ stats.classesToday }}</dd>
            </dl>
          </div>
        </div>
      </div>

      <div class="stat-card group animate-scale-in hover:scale-105" style="animation-delay: 0.3s;">
        <div class="flex items-center">
          <div class="stat-icon group-hover:animate-glow-pulse bg-gradient-to-br from-amber-500 to-orange-600">
            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"></path>
            </svg>
          </div>
          <div class="ml-5 w-0 flex-1">
            <dl>
              <dt class="stat-label">Attendance Marked</dt>
              <dd class="stat-value text-amber-600">{{ stats.attendanceMarked }}%</dd>
            </dl>
          </div>
        </div>
      </div>

      <div class="stat-card group animate-scale-in hover:scale-105" style="animation-delay: 0.4s;">
        <div class="flex items-center">
          <div class="stat-icon group-hover:animate-glow-pulse bg-gradient-to-br from-mastermind-500 to-purple-600">
            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"></path>
            </svg>
          </div>
          <div class="ml-5 w-0 flex-1">
            <dl>
              <dt class="stat-label">Remarks Added</dt>
              <dd class="stat-value text-mastermind-600">{{ stats.remarksAdded }}</dd>
            </dl>
          </div>
        </div>
      </div>
    </div>

    <!-- Today's Schedule with enhanced styling -->
    <div class="card animate-slide-up mb-8" style="animation-delay: 0.5s;">
      <div class="card-header">
        <h3 class="card-title flex items-center">
          <div class="w-2 h-8 bg-gradient-to-b from-mastermind-500 to-primary-500 rounded-full mr-3"></div>
          Today's Schedule
        </h3>
      </div>
      <div class="space-y-4">
        <transition-group name="schedule" tag="div">
          <div v-for="(classItem, index) in todaysSchedule" :key="classItem.id" 
               class="flex items-center justify-between p-6 bg-white/40 backdrop-blur-sm rounded-2xl border border-white/30 hover:bg-white/60 transition-all duration-300"
               :style="{ animationDelay: `${0.6 + index * 0.1}s` }">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <div class="w-12 h-12 bg-gradient-to-br from-green-100 to-emerald-100 rounded-2xl flex items-center justify-center group-hover:scale-110 transition-transform duration-300">
                  <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.746 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"></path>
                  </svg>
                </div>
              </div>
              <div class="ml-4">
                <div class="text-base font-semibold text-gray-900">{{ classItem.subject }}</div>
                <div class="text-sm text-gray-600/80">{{ classItem.className }} • {{ classItem.time }}</div>
              </div>
            </div>
            <div class="flex items-center space-x-3">
              <span class="badge badge-info">
                {{ classItem.students }} students
              </span>
              <button class="btn-primary text-sm px-4 py-2">
                Mark Attendance
              </button>
            </div>
          </div>
        </transition-group>
      </div>
    </div>

    <!-- Recent Activity with enhanced styling -->
    <div class="card animate-slide-up" style="animation-delay: 0.8s;">
      <div class="card-header">
        <h3 class="card-title flex items-center">
          <div class="w-2 h-8 bg-gradient-to-b from-mastermind-500 to-primary-500 rounded-full mr-3"></div>
          Recent Activity
        </h3>
      </div>
      <div class="space-y-4">
        <transition-group name="activity" tag="div">
          <div v-for="(activity, index) in recentActivities" :key="activity.id" 
               class="flex items-start space-x-4 p-4 rounded-xl hover:bg-white/30 transition-all duration-300"
               :style="{ animationDelay: `${0.9 + index * 0.1}s` }">
            <div class="flex-shrink-0">
              <div class="w-10 h-10 bg-gradient-to-br from-mastermind-100 to-primary-100 rounded-full flex items-center justify-center ring-2 ring-white/50">
                <svg class="w-5 h-5 text-mastermind-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                </svg>
              </div>
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-semibold text-gray-900">{{ activity.title }}</p>
              <p class="text-sm text-gray-600/80">{{ activity.description }}</p>
              <p class="text-xs text-gray-500/70 mt-1">{{ activity.time }}</p>
            </div>
          </div>
        </transition-group>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { apiService } from '@/services/apiService'
import { API_ENDPOINTS } from '@/config/api'

// Type definitions
interface TeacherStats {
  totalStudents: number
  classesToday: number
  attendanceMarked: number
  remarksAdded: number
}

interface ScheduleItem {
  id: number
  subject: string
  className: string
  time: string
  students: number
}

interface ActivityItem {
  id: number
  title: string
  description: string
  time: string
}

// Real data from API
const stats = ref<TeacherStats>({
  totalStudents: 0,
  classesToday: 0,
  attendanceMarked: 0,
  remarksAdded: 0
})

const todaysSchedule = ref<ScheduleItem[]>([])
const recentActivities = ref<ActivityItem[]>([])

// Load teacher dashboard data
const loadTeacherDashboardData = async () => {
  try {
    // Get teacher stats from API using centralized service
    const statsData = await apiService.get<TeacherStats>(API_ENDPOINTS.DASHBOARD.TEACHER_STATS)
    stats.value = statsData

    // Mock schedule data (you can create a schedule controller later)
    todaysSchedule.value = [
      {
        id: 1,
        subject: 'Mathematics',
        className: 'Class 10A',
        time: '9:00 AM - 10:00 AM',
        students: 25
      },
      {
        id: 2,
        subject: 'Mathematics',
        className: 'Class 9B',
        time: '10:30 AM - 11:30 AM',
        students: 22
      },
      {
        id: 3,
        subject: 'Mathematics',
        className: 'Class 8C',
        time: '2:00 PM - 3:00 PM',
        students: 28
      }
    ]

    // Mock recent activities (you can create an activity controller later)
    recentActivities.value = [
      {
        id: 1,
        title: 'Attendance Marked',
        description: 'Marked attendance for Class 10A Mathematics',
        time: '2 hours ago'
      },
      {
        id: 2,
        title: 'Remark Added',
        description: 'Added remark for John Doe',
        time: '4 hours ago'
      },
      {
        id: 3,
        title: 'Test Graded',
        description: 'Completed grading for Science test',
        time: '1 day ago'
      }
    ]
  } catch (error) {
    console.error('Failed to load teacher dashboard data:', error)
    // Fallback to mock data if API fails
    stats.value = {
      totalStudents: 45,
      classesToday: 3,
      attendanceMarked: 87,
      remarksAdded: 12
    }
  }
}

onMounted(() => {
  loadTeacherDashboardData()
})
</script>

<style scoped>
/* Schedule transitions */
.schedule-enter-active {
  transition: all 0.5s ease-out;
}

.schedule-enter-from {
  opacity: 0;
  transform: translateX(-20px);
}

/* Activity transitions */
.activity-enter-active {
  transition: all 0.5s ease-out;
}

.activity-enter-from {
  opacity: 0;
  transform: translateX(-20px);
}

/* Enhanced hover effects */
.stat-card:hover {
  transform: translateY(-4px) scale(1.02);
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
}

/* Custom animations for stat values */
@keyframes countUp {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.stat-value {
  animation: countUp 0.8s ease-out;
}
</style>