import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    // Public routes
    {
      path: '/',
      redirect: '/login'
    },
    {
      path: '/login',
      name: 'Login',
      component: () => import('@/views/auth/LoginView.vue'),
      meta: { requiresAuth: false }
    },
    {
      path: '/otp-verify',
      name: 'OtpVerify',
      component: () => import('@/views/auth/OtpVerifyView.vue'),
      meta: { requiresAuth: false }
    },

    // Admin routes
    {
      path: '/admin',
      component: () => import('@/layouts/AdminLayout.vue'),
      meta: { requiresAuth: true, roles: ['Admin'] },
      children: [
        {
          path: '',
          name: 'AdminDashboard',
          component: () => import('@/views/admin/DashboardView.vue')
        },
        {
          path: 'students',
          name: 'AdminStudents',
          component: () => import('@/views/admin/StudentsView.vue')
        },
        {
          path: 'classes',
          name: 'AdminClasses',
          component: () => import('@/views/admin/ClassesView.vue')
        },
        {
          path: 'attendance',
          name: 'AdminAttendance',
          component: () => import('@/views/admin/AttendanceView.vue')
        },
        {
          path: 'finance',
          component: () => import('@/layouts/FinanceLayout.vue'),
          children: [
            {
              path: '',
              name: 'FinanceOverview',
              component: () => import('@/views/admin/finance/FinanceOverview.vue')
            },
            {
              path: 'fees',
              name: 'FeesManagement',
              component: () => import('@/views/admin/finance/FeesManagementView.vue')
            },
            {
              path: 'fee-collection',
              name: 'FeeCollection',
              component: () => import('@/views/admin/FeeCollectionView.vue')
            },
            {
              path: 'expenses',
              name: 'ExpensesManagement',
              component: () => import('@/views/admin/finance/ExpensesView.vue')
            },
            {
              path: 'overdue',
              name: 'OverdueFees',
              component: () => import('@/views/admin/finance/OverdueFeesView.vue')
            },
            {
              path: 'reports',
              name: 'FinanceReports',
              component: () => import('@/views/admin/finance/FinanceReportsView.vue')
            }
          ]
        },
        {
          path: 'teachers',
          name: 'AdminTeachers',
          component: () => import('@/views/admin/TeachersView.vue')
        },
        {
          path: 'leads',
          name: 'AdminLeads',
          component: () => import('@/views/admin/LeadsView.vue')
        }
      ]
    },

    // Teacher routes
    {
      path: '/teacher',
      component: () => import('@/layouts/TeacherLayout.vue'),
      meta: { requiresAuth: true, roles: ['Teacher'] },
      children: [
        {
          path: '',
          name: 'TeacherDashboard',
          component: () => import('@/views/teacher/DashboardView.vue')
        },
        {
          path: 'students',
          name: 'TeacherStudents',
          component: () => import('@/views/teacher/StudentsView.vue')
        },
        {
          path: 'attendance',
          name: 'TeacherAttendance',
          component: () => import('@/views/teacher/AttendanceView.vue')
        },
        {
          path: 'remarks',
          name: 'TeacherRemarks',
          component: () => import('@/views/teacher/RemarksView.vue')
        }
      ]
    },

    // Parent routes
    {
      path: '/parent',
      component: () => import('@/layouts/ParentLayout.vue'),
      meta: { requiresAuth: true, roles: ['Parent'] },
      children: [
        {
          path: '',
          name: 'ParentDashboard',
          component: () => import('@/views/parent/DashboardView.vue')
        },
        {
          path: 'attendance',
          name: 'ParentAttendance',
          component: () => import('@/views/parent/AttendanceView.vue')
        },
        {
          path: 'fees',
          name: 'ParentFees',
          component: () => import('@/views/parent/FeesView.vue')
        },
        {
          path: 'performance',
          name: 'ParentPerformance',
          component: () => import('@/views/parent/PerformanceView.vue')
        }
      ]
    },

    // Catch all route
    {
      path: '/:pathMatch(.*)*',
      name: 'NotFound',
      component: () => import('@/views/NotFoundView.vue')
    }
  ]
})

// Navigation guards
router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()

  // Check if route requires authentication
  if (to.meta.requiresAuth) {
    // Check if user is authenticated
    if (!authStore.isAuthenticated) {
      next({ name: 'Login' })
      return
    }

    // Check if user has required role
    if (to.meta.roles && !to.meta.roles.includes(authStore.user?.role || '')) {
      // Redirect to appropriate dashboard based on role
      const role = authStore.user?.role
      if (role === 'Admin') {
        next({ name: 'AdminDashboard' })
      } else if (role === 'Teacher') {
        next({ name: 'TeacherDashboard' })
      } else if (role === 'Parent') {
        next({ name: 'ParentDashboard' })
      } else {
        next({ name: 'Login' })
      }
      return
    }
  }

  // If user is authenticated and trying to access login page, redirect to dashboard
  if (authStore.isAuthenticated && to.name === 'Login') {
    const role = authStore.user?.role
    if (role === 'Admin') {
      next({ name: 'AdminDashboard' })
    } else if (role === 'Teacher') {
      next({ name: 'TeacherDashboard' })
    } else if (role === 'Parent') {
      next({ name: 'ParentDashboard' })
    } else {
      next()
    }
    return
  }

  next()
})

export default router