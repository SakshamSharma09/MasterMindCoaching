<template>
  <div class="min-h-screen admin-shell">
    <!-- Premium Background -->
    <div class="fixed inset-0 bg-premium-mesh pointer-events-none -z-10"></div>
    
    <!-- Mobile Header -->
    <header class="lg:hidden fixed top-0 left-0 right-0 z-50 glass-navbar h-16 px-4 flex items-center justify-between">
      <button 
        @click="sidebarOpen = true"
        class="p-2 rounded-xl text-surface-600 hover:bg-surface-100 transition-colors"
      >
        <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"/>
        </svg>
      </button>
      
      <div class="flex items-center gap-2">
        <div class="w-8 h-8 rounded-xl bg-gradient-to-br from-primary-500 to-mastermind-600 flex items-center justify-center">
          <svg class="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.746 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"/>
          </svg>
        </div>
        <span class="font-display font-bold text-surface-900">MasterMind</span>
      </div>
      
      <button class="p-2 rounded-xl text-surface-600 hover:bg-surface-100 transition-colors relative">
        <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/>
        </svg>
        <span class="absolute top-1.5 right-1.5 w-2 h-2 bg-accent-500 rounded-full"></span>
      </button>
    </header>

    <!-- Sidebar Overlay (Mobile) -->
    <Transition name="fade">
      <div 
        v-if="sidebarOpen" 
        class="fixed inset-0 bg-surface-900/50 backdrop-blur-sm z-40 lg:hidden"
        @click="sidebarOpen = false"
      ></div>
    </Transition>

    <!-- Sidebar -->
    <aside 
      :class="[
        'fixed top-0 left-0 z-50 h-full w-72 glass-sidebar shadow-sidebar transition-transform duration-300 ease-premium',
        sidebarOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
      ]"
    >
      <div class="flex flex-col h-full">
        <!-- Logo Section -->
        <div class="h-20 px-6 flex items-center border-b border-surface-200/50">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-primary-500 via-primary-600 to-mastermind-600 flex items-center justify-center shadow-glow-primary">
              <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.746 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"/>
              </svg>
            </div>
            <div>
              <h1 class="font-display font-bold text-lg text-surface-900">MasterMind</h1>
              <p class="text-xs text-surface-500 font-medium">Coaching System</p>
            </div>
          </div>
          
          <!-- Close button (mobile) -->
          <button 
            @click="sidebarOpen = false"
            class="lg:hidden ml-auto p-2 rounded-lg text-surface-400 hover:text-surface-600 hover:bg-surface-100 transition-colors"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>

        <!-- Session Selector -->
        <div class="px-4 py-4 border-b border-surface-200/50">
          <SessionSelector />
        </div>

        <!-- Navigation -->
        <nav class="flex-1 px-3 py-4 space-y-1 overflow-y-auto scrollbar-premium">
          <div class="mb-3">
            <p class="px-3 text-[10px] font-bold text-surface-400 uppercase tracking-wider">Main Menu</p>
          </div>
          
          <router-link
            v-for="item in mainNavigation"
            :key="item.name"
            :to="item.href"
            @click="sidebarOpen = false"
            class="nav-item group"
            :class="{ 'active': isNavActive(item) }"
          >
            <span class="icon" :class="item.iconColor">
              <component :is="item.icon" />
            </span>
            <span class="flex-1">{{ item.name }}</span>
            <span v-if="item.badge" class="badge-premium text-[10px] px-2 py-0.5">
              {{ item.badge }}
            </span>
          </router-link>

          <div class="pt-4 mb-3">
            <p class="px-3 text-[10px] font-bold text-surface-400 uppercase tracking-wider">Management</p>
          </div>
          
          <router-link
            v-for="item in managementNavigation"
            :key="item.name"
            :to="item.href"
            @click="sidebarOpen = false"
            class="nav-item group"
            :class="{ 'active': isNavActive(item) }"
          >
            <span class="icon" :class="item.iconColor">
              <component :is="item.icon" />
            </span>
            <span class="flex-1">{{ item.name }}</span>
            <span v-if="item.badge" class="badge-premium text-[10px] px-2 py-0.5">
              {{ item.badge }}
            </span>
          </router-link>
        </nav>

        <!-- User Section -->
        <div class="p-4 border-t border-surface-200/50">
          <div class="flex items-center gap-3 p-3 rounded-xl bg-surface-100/50 hover:bg-surface-100 transition-colors cursor-pointer group">
            <div class="avatar avatar-md avatar-gradient avatar-status">
              {{ userInitials }}
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-semibold text-surface-900 truncate">{{ authStore.userName }}</p>
              <p class="text-xs text-primary-600 font-medium">Administrator</p>
            </div>
            <button
              @click="logout"
              class="p-2 rounded-lg text-surface-400 hover:text-error-600 hover:bg-error-50 transition-colors opacity-0 group-hover:opacity-100"
              title="Logout"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
              </svg>
            </button>
          </div>
        </div>
      </div>
    </aside>

    <!-- Main Content Area -->
    <div class="lg:pl-72 min-h-screen">
      <!-- Top Navigation Bar -->
      <header class="sticky top-0 z-30 glass-navbar h-16 hidden lg:flex items-center justify-between px-6 border-b border-surface-200/30">
        <!-- Search -->
        <div class="flex-1 max-w-xl">
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
              <svg class="w-5 h-5 text-surface-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
              </svg>
            </div>
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Search students, classes, attendance..."
              class="input-search w-full pl-12 pr-4 py-2.5 rounded-xl"
              @keyup.enter="handleSearch"
            />
            <div class="absolute inset-y-0 right-3 flex items-center">
              <kbd class="hidden sm:inline-flex items-center px-2 py-1 text-xs font-medium text-surface-400 bg-surface-100 rounded border border-surface-200">
                ⌘K
              </kbd>
            </div>
          </div>
        </div>

        <!-- Right Actions -->
        <div class="flex items-center gap-2 ml-4">
          <!-- Quick Actions -->
          <button class="btn-ghost p-2.5 rounded-xl">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
            </svg>
          </button>
          
          <!-- Notifications -->
          <button class="btn-ghost p-2.5 rounded-xl relative">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/>
            </svg>
            <span class="absolute top-2 right-2 w-2 h-2 bg-accent-500 rounded-full animate-pulse"></span>
          </button>
          
          <!-- Settings -->
          <button class="btn-ghost p-2.5 rounded-xl" @click="router.push({ name: 'AdminChangePassword' })" title="Change Password">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"/>
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
            </svg>
          </button>

          <!-- Divider -->
          <div class="w-px h-8 bg-surface-200 mx-2"></div>

          <!-- User Menu -->
          <div class="flex items-center gap-3 pl-2">
            <div class="avatar avatar-md avatar-gradient">
              {{ userInitials }}
            </div>
            <div class="hidden xl:block">
              <p class="text-sm font-semibold text-surface-900">{{ authStore.userName }}</p>
              <p class="text-xs text-surface-500">Admin</p>
            </div>
          </div>
        </div>
      </header>

      <!-- Page Content -->
      <main class="admin-content p-4 sm:p-6 lg:p-8 pt-20 lg:pt-8">
        <div class="max-w-7xl mx-auto">
          <router-view v-slot="{ Component }">
            <Transition name="page" mode="out-in">
              <component :is="Component" />
            </Transition>
          </router-view>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, h, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import SessionSelector from '@/components/admin/SessionSelector.vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const sidebarOpen = ref(false)
const searchQuery = ref('')

// Icon Components
const DashboardIcon = () => h('svg', { class: 'w-5 h-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M4 5a1 1 0 011-1h14a1 1 0 011 1v2a1 1 0 01-1 1H5a1 1 0 01-1-1V5zM4 13a1 1 0 011-1h6a1 1 0 011 1v6a1 1 0 01-1 1H5a1 1 0 01-1-1v-6zM16 13a1 1 0 011-1h2a1 1 0 011 1v6a1 1 0 01-1 1h-2a1 1 0 01-1-1v-6z' })
])

const SessionsIcon = () => h('svg', { class: 'w-5 h-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z' })
])

const StudentsIcon = () => h('svg', { class: 'w-5 h-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z' })
])

const ClassesIcon = () => h('svg', { class: 'w-5 h-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10' })
])

const AttendanceIcon = () => h('svg', { class: 'w-5 h-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4' })
])

const FinanceIcon = () => h('svg', { class: 'w-5 h-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z' })
])

const TeachersIcon = () => h('svg', { class: 'w-5 h-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z' })
])

const LeadsIcon = () => h('svg', { class: 'w-5 h-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z' })
])

const NotesIcon = () => h('svg', { class: 'w-5 h-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l3.414 3.414a1 1 0 01.293.707V19a2 2 0 01-2 2z' })
])

const TemplateIcon = () => h('svg', { class: 'w-5 h-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M7 8h10M7 12h6m-9 8h16a2 2 0 002-2V6a2 2 0 00-2-2H8l-4 4v10a2 2 0 002 2z' })
])

// Navigation items
const mainNavigation = [
  { name: 'Dashboard', href: '/admin', icon: DashboardIcon, iconColor: 'text-primary-500' },
  { name: 'Sessions', href: '/admin/sessions', icon: SessionsIcon, iconColor: 'text-mastermind-500' },
  { name: 'Students', href: '/admin/students', icon: StudentsIcon, iconColor: 'text-success-500' },
  { name: 'Classes', href: '/admin/classes', icon: ClassesIcon, iconColor: 'text-warning-500' },
  { name: 'Attendance', href: '/admin/attendance', icon: AttendanceIcon, iconColor: 'text-accent-500' },
]

const managementNavigation = [
  { name: 'Finance', href: '/admin/finance', icon: FinanceIcon, iconColor: 'text-success-500' },
  { name: 'Template Zone', href: '/admin/template-zone', icon: TemplateIcon, iconColor: 'text-warning-500', badge: 'New' },
  { name: 'Notes Tracker', href: '/admin/notes-tracker', icon: NotesIcon, iconColor: 'text-accent-500', badge: 'New' },
  { name: 'Teachers', href: '/admin/teachers', icon: TeachersIcon, iconColor: 'text-primary-500' },
  { name: 'Leads', href: '/admin/leads', icon: LeadsIcon, iconColor: 'text-mastermind-500' },
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
  return `${user.firstName?.charAt(0) || ''}${user.lastName?.charAt(0) || ''}`.toUpperCase() || 'U'
})

const handleSearch = () => {
  console.log('Searching for:', searchQuery.value)
}

const logout = async () => {
  await authStore.logout()
  router.push('/login')
}

// Close sidebar on route change (mobile)
onMounted(() => {
  router.afterEach(() => {
    sidebarOpen.value = false
  })
})

// Handle escape key
const handleEscape = (e: KeyboardEvent) => {
  if (e.key === 'Escape') {
    sidebarOpen.value = false
  }
}

onMounted(() => {
  document.addEventListener('keydown', handleEscape)
})

onUnmounted(() => {
  document.removeEventListener('keydown', handleEscape)
})
</script>

<style scoped>
/* Page Transitions */
.page-enter-active,
.page-leave-active {
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.page-enter-from {
  opacity: 0;
  transform: translateY(10px);
}

.page-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

/* Fade Transition */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* Premium Background Mesh */
.bg-premium-mesh {
  background: 
    radial-gradient(at 40% 20%, rgba(99, 102, 241, 0.08) 0px, transparent 50%),
    radial-gradient(at 80% 0%, rgba(168, 85, 247, 0.06) 0px, transparent 50%),
    radial-gradient(at 0% 50%, rgba(244, 63, 94, 0.04) 0px, transparent 50%),
    radial-gradient(at 80% 50%, rgba(16, 185, 129, 0.04) 0px, transparent 50%),
    radial-gradient(at 0% 100%, rgba(99, 102, 241, 0.06) 0px, transparent 50%),
    linear-gradient(180deg, #fafafa 0%, #f4f4f5 100%);
}
</style>
