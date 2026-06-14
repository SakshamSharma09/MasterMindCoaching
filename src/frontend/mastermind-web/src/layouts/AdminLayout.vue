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
      
      <div class="flex items-center gap-1">
        <button
          class="p-2 rounded-xl text-surface-600 hover:bg-surface-100 transition-colors relative"
          type="button"
          aria-label="Notifications"
          @click="toggleNotifications"
        >
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/>
          </svg>
          <span v-if="notificationCount > 0" class="absolute -top-1 -right-1 min-w-5 h-5 px-1 rounded-full bg-accent-500 text-white text-[10px] font-bold flex items-center justify-center">{{ notificationCount }}</span>
        </button>
        <button
          class="p-2 rounded-xl text-surface-600 hover:bg-surface-100 transition-colors"
          type="button"
          aria-label="Change Password"
          @click="router.push({ name: 'AdminChangePassword' })"
        >
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 11c1.657 0 3-1.567 3-3.5S13.657 4 12 4s-3 1.567-3 3.5S10.343 11 12 11z"/>
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 20v-1a7 7 0 0114 0v1"/>
          </svg>
        </button>
      </div>
    </header>

    <div v-if="notificationsOpen" class="lg:hidden fixed right-4 top-16 z-50 w-[calc(100vw-2rem)] rounded-2xl border border-surface-200 bg-white shadow-xl overflow-hidden">
      <div class="flex items-center justify-between px-4 py-3 border-b border-surface-100">
        <div>
          <p class="text-sm font-semibold text-surface-900">Admin Reminders</p>
          <p class="text-xs text-surface-500">{{ notificationSummary }}</p>
        </div>
        <button class="text-xs font-semibold text-primary-600 hover:text-primary-700" type="button" @click="loadNotifications">Refresh</button>
      </div>
      <div v-if="notificationsLoading" class="px-4 py-5 text-sm text-surface-500">Loading reminders...</div>
      <div v-else-if="notifications.length === 0" class="px-4 py-5 text-sm text-surface-500">No pending reminders right now.</div>
      <div v-else class="max-h-96 overflow-y-auto divide-y divide-surface-100">
        <button
          v-for="item in notifications"
          :key="`mobile-${item.type}-${item.studentId || item.leadId || item.dueDate}-${item.title}`"
          type="button"
          class="w-full px-4 py-3 text-left hover:bg-surface-50 focus:outline-none focus:bg-surface-50"
          @click="goToNotification(item)"
        >
          <div class="flex items-start justify-between gap-3">
            <div class="flex min-w-0 gap-3">
              <span class="mt-1 h-8 w-1.5 shrink-0 rounded-full" :class="notificationTone(item.type)"></span>
              <div class="min-w-0">
              <p class="text-sm font-semibold text-surface-900">{{ item.title }}</p>
              <p class="mt-1 text-xs text-surface-500">{{ item.message }}</p>
              <p class="mt-1 text-xs text-surface-400">Due: {{ item.dueDate }}</p>
              </div>
            </div>
            <span class="shrink-0 rounded-full px-2 py-0.5 text-[10px] font-bold" :class="notificationPriorityClass(item.priority)">
              {{ item.priority }}
            </span>
          </div>
        </button>
      </div>
    </div>

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
              class="p-2 rounded-lg text-surface-400 hover:text-error-600 hover:bg-error-50 transition-colors opacity-100 lg:opacity-0 lg:group-hover:opacity-100"
              title="Logout"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
              </svg>
            </button>
          </div>
          <button
            type="button"
            @click="router.push({ name: 'AdminChangePassword' }); sidebarOpen = false"
            class="mt-3 w-full inline-flex items-center justify-center rounded-xl border border-surface-200 bg-white px-3 py-2 text-sm font-medium text-surface-700 hover:bg-surface-50"
          >
            Change Password
          </button>
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
          <div class="relative">
            <button class="btn-ghost p-2.5 rounded-xl relative" type="button" aria-label="Notifications" @click="toggleNotifications">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/>
              </svg>
              <span v-if="notificationCount > 0" class="absolute -top-1 -right-1 min-w-5 h-5 px-1 rounded-full bg-accent-500 text-white text-[10px] font-bold flex items-center justify-center">{{ notificationCount }}</span>
            </button>
            <div v-if="notificationsOpen" class="absolute right-0 mt-3 w-96 max-w-[calc(100vw-2rem)] rounded-2xl border border-surface-200 bg-white shadow-xl overflow-hidden">
              <div class="flex items-center justify-between px-4 py-3 border-b border-surface-100">
                <div>
                  <p class="text-sm font-semibold text-surface-900">Admin Reminders</p>
                  <p class="text-xs text-surface-500">{{ notificationSummary }}</p>
                </div>
                <button class="text-xs font-semibold text-primary-600 hover:text-primary-700" type="button" @click="loadNotifications">Refresh</button>
              </div>
              <div v-if="notificationsLoading" class="px-4 py-5 text-sm text-surface-500">Loading reminders...</div>
              <div v-else-if="notifications.length === 0" class="px-4 py-5 text-sm text-surface-500">No pending reminders right now.</div>
              <div v-else class="max-h-96 overflow-y-auto divide-y divide-surface-100">
                <button
                  v-for="item in notifications"
                  :key="`${item.type}-${item.studentId || item.leadId || item.dueDate}-${item.title}`"
                  type="button"
                  class="w-full px-4 py-3 text-left hover:bg-surface-50 focus:outline-none focus:bg-surface-50"
                  @click="goToNotification(item)"
                >
                  <div class="flex items-start justify-between gap-3">
                    <div class="flex min-w-0 gap-3">
                      <span class="mt-1 h-8 w-1.5 shrink-0 rounded-full" :class="notificationTone(item.type)"></span>
                      <div class="min-w-0">
                        <p class="text-sm font-semibold text-surface-900">{{ item.title }}</p>
                        <p class="mt-1 text-xs text-surface-500">{{ item.message }}</p>
                        <p class="mt-1 text-xs text-surface-400">Due: {{ item.dueDate }}</p>
                      </div>
                    </div>
                    <span class="shrink-0 rounded-full px-2 py-0.5 text-[10px] font-bold" :class="notificationPriorityClass(item.priority)">
                      {{ item.priority }}
                    </span>
                  </div>
                </button>
              </div>
            </div>
          </div>
          
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
import { apiService } from '@/services/apiService'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const sidebarOpen = ref(false)
const searchQuery = ref('')
const notificationsOpen = ref(false)
const notificationsLoading = ref(false)
const notifications = ref<AdminNotification[]>([])

interface AdminNotification {
  type: string
  priority: 'High' | 'Medium' | 'Low'
  title: string
  message: string
  dueDate: string
  actionUrl?: string
  studentId?: number
  leadId?: number
}

const notificationCount = computed(() => Math.min(notifications.value.length, 99))
const notificationSummary = computed(() => {
  if (notificationsLoading.value) return 'Checking birthdays, fees, and follow-ups'
  if (notifications.value.length === 0) return 'Everything is clear for now'

  const counts = notifications.value.reduce<Record<string, number>>((summary, item) => {
    const key = item.type || 'Reminder'
    summary[key] = (summary[key] || 0) + 1
    return summary
  }, {})

  return Object.entries(counts)
    .slice(0, 3)
    .map(([type, count]) => `${count} ${type}`)
    .join(' | ')
})

const notificationTone = (type: string) => {
  const normalized = type.toLowerCase()
  if (normalized.includes('birthday')) return 'bg-gradient-to-b from-rose-400 to-amber-400'
  if (normalized.includes('fee')) return 'bg-gradient-to-b from-amber-400 to-emerald-500'
  if (normalized.includes('lead')) return 'bg-gradient-to-b from-sky-400 to-blue-600'
  return 'bg-gradient-to-b from-indigo-400 to-violet-500'
}

const notificationPriorityClass = (priority: AdminNotification['priority']) => {
  if (priority === 'High') return 'bg-error-50 text-error-700 ring-1 ring-error-100'
  if (priority === 'Medium') return 'bg-warning-50 text-warning-700 ring-1 ring-warning-100'
  return 'bg-success-50 text-success-700 ring-1 ring-success-100'
}

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

const PlannerIcon = () => h('svg', { class: 'w-5 h-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2zm4-5h6' })
])

const TemplateIcon = () => h('svg', { class: 'w-5 h-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M7 8h10M7 12h6m-9 8h16a2 2 0 002-2V6a2 2 0 00-2-2H8l-4 4v10a2 2 0 002 2z' })
])

const SecurityIcon = () => h('svg', { class: 'w-5 h-5', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M12 11c1.657 0 3-1.567 3-3.5S13.657 4 12 4s-3 1.567-3 3.5S10.343 11 12 11z' }),
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2', d: 'M5 20v-1a7 7 0 0114 0v1' })
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
  { name: 'Account Security', href: '/admin/change-password', icon: SecurityIcon, iconColor: 'text-indigo-500' },
  { name: 'Syllabus & Timetable', href: '/admin/academic-planner', icon: PlannerIcon, iconColor: 'text-indigo-500' },
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

const loadNotifications = async () => {
  notificationsLoading.value = true
  try {
    const response = await apiService.get('/admin-notifications')
    notifications.value = response.data?.items || response.data?.Items || []
  } catch (error) {
    console.error('Failed to load admin notifications:', error)
    notifications.value = []
  } finally {
    notificationsLoading.value = false
  }
}

const toggleNotifications = async () => {
  notificationsOpen.value = !notificationsOpen.value
  if (notificationsOpen.value) {
    await loadNotifications()
  }
}

const goToNotification = (item: AdminNotification) => {
  notificationsOpen.value = false
  if (item.actionUrl) {
    router.push(item.actionUrl)
  }
}

const logout = async () => {
  await authStore.logout()
  router.push('/login')
}

// Close sidebar on route change (mobile)
onMounted(() => {
  router.afterEach(() => {
    sidebarOpen.value = false
    notificationsOpen.value = false
  })
  loadNotifications()
})

// Handle escape key
const handleEscape = (e: KeyboardEvent) => {
  if (e.key === 'Escape') {
    sidebarOpen.value = false
    notificationsOpen.value = false
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
    linear-gradient(120deg, rgba(37, 99, 235, 0.08), rgba(16, 185, 129, 0.06) 34%, rgba(244, 63, 94, 0.05) 68%, rgba(245, 158, 11, 0.06)),
    linear-gradient(rgba(15, 23, 42, 0.035) 1px, transparent 1px),
    linear-gradient(90deg, rgba(15, 23, 42, 0.025) 1px, transparent 1px),
    linear-gradient(180deg, #f8fafc 0%, #f1f5f9 100%);
  background-size: auto, 36px 36px, 36px 36px, auto;
}
</style>
