<template>
  <div class="min-h-screen bg-gradient-to-br from-surface-50 via-surface-100 to-surface-200">
    <!-- Session Selection Header -->
    <div class="glass-dark rounded-2xl shadow-glass-xl p-6 text-white animate-float">
      <div class="flex items-center justify-between">
        <div class="flex items-center space-x-6">
          <div>
            <label class="block text-sm font-medium text-purple-100 mb-2">Academic Session</label>
            <select 
              v-model="selectedSession" 
              @change="onSessionChange"
              class="glass-input border border-white/20 text-white rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-white/30 backdrop-blur-lg transition-all duration-300 hover:bg-white/10"
            >
              <option value="2025-26" class="text-gray-900">2025-26</option>
              <option value="2026-27" class="text-gray-900">2026-27</option>
              <option value="2027-28" class="text-gray-900">2027-28</option>
            </select>
          </div>
          <div class="h-12 w-px bg-white/30"></div>
          <div>
            <div class="text-sm text-purple-100">Current Session</div>
            <div class="text-lg font-semibold">{{ selectedSession }}</div>
          </div>
        </div>
        <div class="flex items-center space-x-3">
          <button class="glass-button hover:glass-button-hover p-2 rounded-lg transition-all duration-300 transform hover:scale-105">
            <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"></path>
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
            </svg>
          </button>
          <div class="relative">
            <button class="glass-button hover:glass-button-hover p-2 rounded-lg transition-all duration-300 transform hover:scale-105">
              <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-5 5v-5zM21 5a2 2 0 00-2-2H5a2 2 0 00-2 2v14l7-7h11z"></path>
                <span class="absolute top-1 right-1 h-2 w-2 bg-red-400 rounded-full animate-pulse"></span>
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Sidebar -->
    <div class="fixed inset-y-0 left-0 z-50 w-64 glass-sidebar shadow-glass-xl transform transition-all duration-500 ease-out lg:translate-x-0"
         :class="{ '-translate-x-full': !sidebarOpen, 'translate-x-0': sidebarOpen }">
      <div class="flex flex-col h-full">
        <!-- Logo -->
        <div class="flex items-center justify-center h-16 px-4 glass-dark shadow-glass-lg">
          <div class="flex items-center space-x-2">
            <div class="relative">
              <svg class="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.746 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"></path>
              </svg>
              <div class="absolute -bottom-1 -right-1 h-3 w-3 bg-green-400 rounded-full border-2 border-white"></div>
            </div>
            <span class="text-white font-bold text-lg">MasterMind</span>
          </div>
        </div>

        <!-- Navigation -->
        <nav class="flex-1 px-4 py-6 space-y-2">
          <router-link
            v-for="item in navigation"
            :key="item.name"
            :to="item.href"
            class="flex items-center px-4 py-3 text-sm font-medium rounded-xl transition-all duration-300 group animate-fade-in"
            :class="[
              isNavActive(item)
                ? 'glass-active text-white shadow-glass-lg transform scale-105'
                : 'text-surface-600 hover:glass-hover hover:text-surface-900 hover:translate-x-1'
            ]"
          >
            <component :is="item.icon" 
              :class="[
                isNavActive(item) ? 'text-white' : 'text-gray-400 group-hover:text-indigo-600'
              ]" 
              class="mr-3 h-5 w-5 transition-colors duration-200" />
            <span class="truncate">{{ item.name }}</span>
            <div v-if="isNavActive(item)" class="ml-auto">
              <div class="h-2 w-2 bg-white rounded-full animate-pulse"></div>
            </div>
          </router-link>
        </nav>

        <!-- User Menu -->
        <div class="border-t border-surface-200/50 p-4 glass-footer">
          <div class="flex items-center space-x-3 p-3 rounded-lg glass-card shadow-glass-sm hover:shadow-glass-md transition-all duration-300 hover:scale-102">
            <div class="flex-shrink-0">
              <div class="h-10 w-10 rounded-full bg-gradient-to-r from-indigo-500 to-purple-500 flex items-center justify-center shadow-md">
                <span class="text-white text-sm font-bold">
                  {{ userInitials }}
                </span>
              </div>
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-semibold text-gray-900 truncate">
                {{ authStore.userName }}
              </p>
              <p class="text-xs text-indigo-600 truncate font-medium">
                Administrator
              </p>
            </div>
            <button
              @click="logout"
              class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors duration-200"
              title="Logout"
            >
              <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"></path>
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Main Content -->
    <div class="lg:pl-64">
      <!-- Top Bar -->
      <div class="sticky top-0 z-40 glass-navbar shadow-glass-sm border-b border-surface-200/30 backdrop-blur-xl">
        <div class="flex items-center justify-between h-16 px-4 sm:px-6 lg:px-8">
          <button
            @click="sidebarOpen = !sidebarOpen"
            class="lg:hidden p-2 rounded-lg text-surface-400 hover:text-surface-600 hover:glass-button focus:outline-none focus:ring-2 focus:ring-inset focus:ring-primary-500 transition-all duration-300 transform hover:scale-105"
          >
            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"></path>
            </svg>
          </button>

          <div class="flex-1">
            <div class="max-w-lg">
              <div class="relative">
                <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
                  </svg>
                </div>
                <input
                  v-model="searchQuery"
                  type="text"
                  placeholder="Search students, classes, attendance..."
                  class="block w-full pl-10 pr-3 py-2 border border-surface-300/50 rounded-lg leading-5 glass-input placeholder-surface-500 focus:outline-none focus:placeholder-surface-400 focus:ring-2 focus:ring-primary-500 focus:border-primary-500 sm:text-sm transition-all duration-300 hover:shadow-glass-sm focus:shadow-glass-md"
                  @keyup.enter="handleSearch"
                />
              </div>
            </div>
          </div>

          <!-- Notifications -->
          <div class="flex items-center space-x-3">
            <button @click="toggleNotifications" class="relative p-2 text-surface-400 hover:text-surface-600 hover:glass-button rounded-lg transition-all duration-300 transform hover:scale-105">
              <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-5 5v-5zM21 5a2 2 0 00-2-2H5a2 2 0 00-2 2v14l7-7h11z"></path>
              </svg>
              <span class="absolute top-1 right-1 h-2 w-2 bg-red-500 rounded-full animate-pulse"></span>
            </button>
            <button class="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors duration-200">
              <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"></path>
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
              </svg>
            </button>
          </div>
        </div>
      </div>

      <!-- Page Content -->
      <main class="flex-1">
        <div class="py-8">
          <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <router-view />
          </div>
        </div>
      </main>
    </div>

    <!-- Mobile sidebar overlay -->
    <div
      v-if="sidebarOpen"
      class="fixed inset-0 z-40 lg:hidden"
      @click="sidebarOpen = false"
    >
      <div class="absolute inset-0 bg-gray-900 opacity-50 backdrop-blur-sm"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, h } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

// Icon components
const HomeIcon = () => h('svg', { class: 'h-5 w-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6' })
])

const UsersIcon = () => h('svg', { class: 'h-5 w-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z' })
])

const BookOpenIcon = () => h('svg', { class: 'h-5 w-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.746 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253' })
])

const ClipboardListIcon = () => h('svg', { class: 'h-5 w-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4' })
])

const CurrencyDollarIcon = () => h('svg', { class: 'h-5 w-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1' })
])

const UserGroupIcon = () => h('svg', { class: 'h-5 w-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z' })
])

const PhoneIcon = () => h('svg', { class: 'h-5 w-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z' })
])

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const sidebarOpen = ref(false)
const selectedSession = ref('2025-26')
const searchQuery = ref('')
const showNotifications = ref(false)
const showSettings = ref(false)

const navigation = [
  { name: 'Dashboard', href: '/admin', icon: HomeIcon },
  { name: 'Students', href: '/admin/students', icon: UsersIcon },
  { name: 'Classes', href: '/admin/classes', icon: BookOpenIcon },
  { name: 'Attendance', href: '/admin/attendance', icon: ClipboardListIcon },
  { name: 'Finance', href: '/admin/finance', icon: CurrencyDollarIcon },
  { name: 'Teachers', href: '/admin/teachers', icon: UserGroupIcon },
  { name: 'Leads', href: '/admin/leads', icon: PhoneIcon }
]

const isNavActive = (item: { name: string; href: string }) => {
  const path = route.path
  if (item.href === '/admin') {
    return path === '/admin' || path === '/admin/'
  }
  return path.startsWith(item.href)
}

const userInitials = computed(() => {
  const user = authStore.user
  if (!user) return 'U'
  return `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase()
})

const onSessionChange = () => {
  // Handle session change logic here
  console.log('Session changed to:', selectedSession.value)
  // You can reload data, update filters, etc.
}

const handleSearch = () => {
  // Handle search functionality
  console.log('Searching for:', searchQuery.value)
  // Implement search logic here
}

const toggleNotifications = () => {
  showNotifications.value = !showNotifications.value
  console.log('Notifications toggled')
}

const toggleSettings = () => {
  showSettings.value = !showSettings.value
  console.log('Settings toggled')
}

const logout = async () => {
  await authStore.logout()
  router.push('/login')
}

onMounted(() => {
  // Close sidebar on mobile when route changes
  router.afterEach(() => {
    sidebarOpen.value = false
  })
})
</script>

<style scoped>
/* Additional styles if needed */
</style>