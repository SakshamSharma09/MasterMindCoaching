<template>
  <div class="px-4 sm:px-6 lg:px-8 py-8">
    <!-- Header with enhanced styling -->
    <div class="mb-8 animate-slide-up">
      <div class="bg-white/70 backdrop-blur-xl rounded-3xl border border-white/50 p-8 shadow-glass">
        <h1 class="text-4xl font-bold bg-gradient-to-r from-mastermind-600 to-primary-600 bg-clip-text text-transparent mb-3">
          Academic Performance
        </h1>
        <p class="text-lg text-gray-600/90 leading-relaxed">
          View your child's grades, test results, and academic progress at THE MASTERMIND COACHING CLASSES.
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
            {{ child.name }} - {{ child.className }}
          </option>
        </select>
      </div>
    </div>

    <!-- Performance Overview with premium cards -->
    <div class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4 mb-8">
      <div class="stat-card group animate-scale-in hover:scale-105" style="animation-delay: 0.2s;">
        <div class="flex items-center">
          <div class="stat-icon group-hover:animate-glow-pulse bg-gradient-to-br from-blue-500 to-indigo-600">
            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path>
            </svg>
          </div>
          <div class="ml-5 w-0 flex-1">
            <dl>
              <dt class="stat-label">Overall Grade</dt>
              <dd class="stat-value text-blue-600">{{ performanceStats.overallGrade }}</dd>
            </dl>
          </div>
        </div>
      </div>

      <div class="stat-card group animate-scale-in hover:scale-105" style="animation-delay: 0.3s;">
        <div class="flex items-center">
          <div class="stat-icon group-hover:animate-glow-pulse bg-gradient-to-br from-amber-500 to-orange-600">
            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"></path>
            </svg>
          </div>
          <div class="ml-5 w-0 flex-1">
            <dl>
              <dt class="stat-label">Average Score</dt>
              <dd class="stat-value text-amber-600">{{ performanceStats.averageScore }}%</dd>
            </dl>
          </div>
        </div>
      </div>

      <div class="stat-card group animate-scale-in hover:scale-105" style="animation-delay: 0.4s;">
        <div class="flex items-center">
          <div class="stat-icon group-hover:animate-glow-pulse bg-gradient-to-br from-green-500 to-emerald-600">
            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
            </svg>
          </div>
          <div class="ml-5 w-0 flex-1">
            <dl>
              <dt class="stat-label">Tests Passed</dt>
              <dd class="stat-value text-green-600">{{ performanceStats.testsPassed }}/{{ performanceStats.totalTests }}</dd>
            </dl>
          </div>
        </div>
      </div>

      <div class="stat-card group animate-scale-in hover:scale-105" style="animation-delay: 0.5s;">
        <div class="flex items-center">
          <div class="stat-icon group-hover:animate-glow-pulse bg-gradient-to-br from-mastermind-500 to-purple-600">
            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6"></path>
            </svg>
          </div>
          <div class="ml-5 w-0 flex-1">
            <dl>
              <dt class="stat-label">Improvement</dt>
              <dd class="stat-value text-mastermind-600">+{{ performanceStats.improvement }}%</dd>
            </dl>
          </div>
        </div>
      </div>
    </div>

    <!-- Subject-wise Performance with enhanced styling -->
    <div class="card animate-slide-up mb-8" style="animation-delay: 0.6s;">
      <div class="card-header">
        <h3 class="card-title flex items-center">
          <div class="w-2 h-8 bg-gradient-to-b from-mastermind-500 to-primary-500 rounded-full mr-3"></div>
          Subject-wise Performance
        </h3>
      </div>
      <div class="space-y-4">
        <transition-group name="subject" tag="div">
          <div v-for="(subject, index) in subjectPerformance" :key="subject.name" 
               class="p-6 bg-white/40 backdrop-blur-sm rounded-2xl border border-white/30 hover:bg-white/60 transition-all duration-300"
               :style="{ animationDelay: `${0.7 + index * 0.1}s` }">
            <div class="flex items-center justify-between mb-3">
              <div class="flex items-center flex-1">
                <div class="flex-1">
                  <div class="text-base font-semibold text-gray-900 mb-2">{{ subject.name }}</div>
                  <div class="relative">
                    <div class="w-full bg-gray-200/50 rounded-full h-3 overflow-hidden">
                      <div
                        :class="[
                          'h-3 rounded-full transition-all duration-1000 ease-out relative overflow-hidden',
                          subject.grade === 'A' ? 'bg-gradient-to-r from-green-400 to-green-600' :
                          subject.grade === 'B' ? 'bg-gradient-to-r from-blue-400 to-blue-600' :
                          subject.grade === 'C' ? 'bg-gradient-to-r from-amber-400 to-amber-600' :
                          'bg-gradient-to-r from-red-400 to-red-600'
                        ]"
                        :style="{ width: subject.percentage + '%' }"
                      >
                        <div class="absolute inset-0 bg-white/20 animate-shimmer"></div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div class="flex items-center space-x-4 ml-6">
                <span class="text-sm font-semibold text-gray-700">{{ subject.percentage }}%</span>
                <span
                  :class="[
                    'badge',
                    subject.grade === 'A' ? 'badge-success' :
                    subject.grade === 'B' ? 'badge-info' :
                    subject.grade === 'C' ? 'badge-warning' :
                    'badge-danger'
                  ]"
                >
                  Grade {{ subject.grade }}
                </span>
              </div>
            </div>
          </div>
        </transition-group>
      </div>
    </div>

    <!-- Recent Test Results with enhanced styling -->
    <div class="card animate-slide-up" style="animation-delay: 0.9s;">
      <div class="card-header">
        <h3 class="card-title flex items-center">
          <div class="w-2 h-8 bg-gradient-to-b from-mastermind-500 to-primary-500 rounded-full mr-3"></div>
          Recent Test Results
        </h3>
      </div>
      <div class="space-y-4">
        <transition-group name="test" tag="div">
          <div v-for="(test, index) in recentTests" :key="test.id" 
               class="flex items-center justify-between p-6 bg-white/40 backdrop-blur-sm rounded-2xl border border-white/30 hover:bg-white/60 transition-all duration-300 group"
               :style="{ animationDelay: `${1.0 + index * 0.1}s` }">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <div
                  :class="[
                    'w-12 h-12 rounded-2xl flex items-center justify-center font-bold text-sm transition-all duration-300 group-hover:scale-110',
                    test.score >= 90 ? 'bg-gradient-to-br from-green-100 to-green-200 text-green-700 ring-2 ring-green-300' :
                    test.score >= 75 ? 'bg-gradient-to-br from-blue-100 to-blue-200 text-blue-700 ring-2 ring-blue-300' :
                    test.score >= 60 ? 'bg-gradient-to-br from-amber-100 to-amber-200 text-amber-700 ring-2 ring-amber-300' :
                    'bg-gradient-to-br from-red-100 to-red-200 text-red-700 ring-2 ring-red-300'
                  ]"
                >
                  {{ test.score }}
                </div>
              </div>
              <div class="ml-4">
                <div class="text-base font-semibold text-gray-900">{{ test.subject }} Test</div>
                <div class="text-sm text-gray-600/80">{{ test.date }}</div>
              </div>
            </div>
            <div class="flex items-center">
              <span class="text-lg font-bold text-gray-900 mr-3">{{ test.score }}/{{ test.total }}</span>
              <div class="text-xs text-gray-500">
                {{ Math.round((test.score / test.total) * 100) }}%
              </div>
            </div>
          </div>
        </transition-group>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

// Sample data - replace with actual API calls
const children = ref([
  { id: 1, name: 'John Doe', className: 'Class 10A' },
  { id: 2, name: 'Jane Doe', className: 'Class 8B' }
])

const selectedChild = ref(1)

const performanceData = ref({
  1: {
    overallGrade: 'A-',
    averageScore: 87,
    testsPassed: 8,
    totalTests: 10,
    improvement: 12,
    subjects: [
      { name: 'Mathematics', percentage: 92, grade: 'A' },
      { name: 'Science', percentage: 85, grade: 'B' },
      { name: 'English', percentage: 88, grade: 'B' },
      { name: 'History', percentage: 82, grade: 'B' },
      { name: 'Geography', percentage: 90, grade: 'A' }
    ],
    tests: [
      { id: 1, subject: 'Mathematics', score: 92, total: 100, date: '2024-01-15' },
      { id: 2, subject: 'Science', score: 85, total: 100, date: '2024-01-12' },
      { id: 3, subject: 'English', score: 88, total: 100, date: '2024-01-10' }
    ]
  },
  2: {
    overallGrade: 'B+',
    averageScore: 82,
    testsPassed: 6,
    totalTests: 8,
    improvement: 8,
    subjects: [
      { name: 'Mathematics', percentage: 78, grade: 'C' },
      { name: 'Science', percentage: 85, grade: 'B' },
      { name: 'English', percentage: 88, grade: 'B' },
      { name: 'Social Studies', percentage: 80, grade: 'B' }
    ],
    tests: [
      { id: 1, subject: 'Science', score: 85, total: 100, date: '2024-01-15' },
      { id: 2, subject: 'English', score: 88, total: 100, date: '2024-01-12' },
      { id: 3, subject: 'Mathematics', score: 78, total: 100, date: '2024-01-10' }
    ]
  }
})

const performanceStats = computed(() => performanceData.value[selectedChild.value as keyof typeof performanceData.value])
const subjectPerformance = computed(() => performanceStats.value.subjects)
const recentTests = computed(() => performanceStats.value.tests)
</script>

<style scoped>
/* Subject transitions */
.subject-enter-active {
  transition: all 0.6s ease-out;
}

.subject-enter-from {
  opacity: 0;
  transform: translateX(-30px);
}

/* Test transitions */
.test-enter-active {
  transition: all 0.6s ease-out;
}

.test-enter-from {
  opacity: 0;
  transform: translateY(20px);
}

/* Enhanced hover effects */
.stat-card:hover {
  transform: translateY(-4px) scale(1.02);
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
}

/* Progress bar animation */
@keyframes progressFill {
  from { width: 0; }
}

/* Custom animations for stat values */
@keyframes countUp {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.stat-value {
  animation: countUp 0.8s ease-out;
}

/* Shimmer effect for progress bars */
@keyframes shimmer {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(100%); }
}

.animate-shimmer {
  animation: shimmer 2s infinite;
}
</style>