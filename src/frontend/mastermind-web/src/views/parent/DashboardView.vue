<template>
  <div class="px-4 sm:px-6 lg:px-8 py-8">
    <!-- Header with enhanced styling -->
    <div class="mb-8 animate-slide-up">
      <div class="bg-white/70 backdrop-blur-xl rounded-3xl border border-white/50 p-8 shadow-glass">
        <h1 class="text-4xl font-bold bg-gradient-to-r from-mastermind-600 to-primary-600 bg-clip-text text-transparent mb-3">
          Parent Dashboard
        </h1>
        <p class="text-lg text-gray-600/90 leading-relaxed">
          Welcome back! Here's an overview of your child's progress at THE MASTERMIND COACHING CLASSES.
        </p>
      </div>
    </div>

    <!-- Child Selection with enhanced styling -->
    <div class="mb-8 animate-slide-up" style="animation-delay: 0.1s;">
      <div class="bg-white/60 backdrop-blur-lg rounded-2xl border border-white/40 p-6 shadow-glass">
        <label for="child-select" class="block text-sm font-semibold text-gray-700 mb-3">Select Child</label>
        <select
          id="child-select"
          v-model="selectedChild"
          class="input-primary"
        >
          <option v-for="child in children" :key="child.id" :value="child.id">
            {{ child.firstName }} {{ child.lastName }} - {{ child.className }}
          </option>
        </select>
      </div>
    </div>

    <!-- Quick Stats with premium cards -->
    <div class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4 mb-8">
      <div class="stat-card group animate-scale-in hover:scale-105" style="animation-delay: 0.2s;">
        <div class="flex items-center">
          <div class="stat-icon group-hover:animate-glow-pulse bg-gradient-to-br from-green-500 to-emerald-600">
            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"></path>
            </svg>
          </div>
          <div class="ml-5 w-0 flex-1">
            <dl>
              <dt class="stat-label">Attendance</dt>
              <dd class="stat-value text-green-600">{{ currentChildStats.attendance }}%</dd>
            </dl>
          </div>
        </div>
      </div>

      <div class="stat-card group animate-scale-in hover:scale-105" style="animation-delay: 0.3s;">
        <div class="flex items-center">
          <div class="stat-icon group-hover:animate-glow-pulse bg-gradient-to-br from-blue-500 to-indigo-600">
            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path>
            </svg>
          </div>
          <div class="ml-5 w-0 flex-1">
            <dl>
              <dt class="stat-label">Average Grade</dt>
              <dd class="stat-value text-blue-600">{{ currentChildStats.averageGrade }}</dd>
            </dl>
          </div>
        </div>
      </div>

      <div class="stat-card group animate-scale-in hover:scale-105" style="animation-delay: 0.4s;">
        <div class="flex items-center">
          <div class="stat-icon group-hover:animate-glow-pulse bg-gradient-to-br from-amber-500 to-orange-600">
            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1"></path>
            </svg>
          </div>
          <div class="ml-5 w-0 flex-1">
            <dl>
              <dt class="stat-label">Pending Fees</dt>
              <dd class="stat-value text-amber-600">₹{{ currentChildStats.pendingFees }}</dd>
            </dl>
          </div>
        </div>
      </div>

      <div class="stat-card group animate-scale-in hover:scale-105" style="animation-delay: 0.5s;">
        <div class="flex items-center">
          <div class="stat-icon group-hover:animate-glow-pulse bg-gradient-to-br from-mastermind-500 to-purple-600">
            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"></path>
            </svg>
          </div>
          <div class="ml-5 w-0 flex-1">
            <dl>
              <dt class="stat-label">Remarks</dt>
              <dd class="stat-value text-mastermind-600">{{ currentChildStats.remarksCount }}</dd>
            </dl>
          </div>
        </div>
      </div>
    </div>

    <!-- Recent Activity with enhanced styling -->
    <div class="card animate-slide-up" style="animation-delay: 0.6s;">
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
               :style="{ animationDelay: `${0.7 + index * 0.1}s` }">
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
              <p class="text-xs text-gray-500/70 mt-1">{{ activity.date }}</p>
            </div>
          </div>
        </transition-group>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { parentService, type ParentChild, type ParentDashboardStats } from '@/services/parentService'

// Reactive data
const children = ref<ParentChild[]>([])
const selectedChild = ref<number | null>(null)
const childStats = ref<Record<number, any>>({})
const loading = ref(true)
const error = ref('')

// Current child stats
const currentChildStats = computed(() => {
  if (!selectedChild.value) return { attendance: 0, averageGrade: 'N/A', pendingFees: 0, remarksCount: 0 }
  return childStats.value[selectedChild.value] || { attendance: 0, averageGrade: 'N/A', pendingFees: 0, remarksCount: 0 }
})

// Sample recent activities - replace with actual API call
const recentActivities = ref([
  {
    id: 1,
    title: 'Mathematics Test',
    description: 'Scored 85% in Mathematics test',
    date: '2024-01-15'
  },
  {
    id: 2,
    title: 'Attendance Marked',
    description: 'Present for Science class',
    date: '2024-01-15'
  },
  {
    id: 3,
    title: 'Fee Payment',
    description: 'Monthly fee payment received',
    date: '2024-01-10'
  }
])

// Load data
const loadData = async () => {
  try {
    loading.value = true
    error.value = ''
    
    // Load children
    children.value = await parentService.getChildren()
    
    if (children.value.length > 0) {
      selectedChild.value = children.value[0].id
      
      // Load stats for each child
      for (const child of children.value) {
        try {
          const [attendanceData, feesData, performanceData] = await Promise.all([
            parentService.getChildAttendance(child.id),
            parentService.getChildFees(child.id),
            parentService.getChildPerformance(child.id)
          ])
          
          childStats.value[child.id] = {
            attendance: attendanceData.percentage,
            averageGrade: performanceData.averageGrade,
            pendingFees: feesData.pendingFees,
            remarksCount: 3 // Mock - implement actual remarks count
          }
        } catch (childError) {
          console.error(`Error loading data for child ${child.id}:`, childError)
          childStats.value[child.id] = {
            attendance: 0,
            averageGrade: 'N/A',
            pendingFees: 0,
            remarksCount: 0
          }
        }
      }
    }
  } catch (err) {
    console.error('Error loading parent dashboard data:', err)
    error.value = 'Failed to load dashboard data. Please try again.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadData()
})
</script>

<style scoped>
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