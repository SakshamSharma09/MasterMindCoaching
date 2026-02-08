<template>
  <div class="fixed inset-0 z-50 overflow-y-auto">
    <div class="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
      <div class="fixed inset-0 transition-opacity" @click="$emit('close')">
        <div class="absolute inset-0 bg-gray-900 opacity-50 backdrop-blur-sm"></div>
      </div>
      
      <div class="inline-block align-bottom bg-white rounded-2xl text-left overflow-hidden shadow-2xl transform transition-all sm:my-8 sm:align-middle sm:max-w-6xl sm:w-full">
        <!-- Header -->
        <div class="bg-gradient-to-r from-indigo-600 to-purple-600 px-6 py-4">
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-semibold text-white">Student-Class Mapping</h3>
            <button
              @click="$emit('close')"
              class="text-white/80 hover:text-white transition-colors"
            >
              <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
              </svg>
            </button>
          </div>
        </div>

        <!-- Content -->
        <div class="bg-white px-6 py-5">
          <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
            <!-- Classes Section -->
            <div>
              <h4 class="text-md font-semibold text-gray-900 mb-4">Select Class</h4>
              <div class="space-y-2 max-h-96 overflow-y-auto">
                <div
                  v-for="cls in classes"
                  :key="cls.id"
                  @click="selectClass(cls)"
                  :class="[
                    'p-3 rounded-lg border cursor-pointer transition-all duration-200',
                    selectedClass?.id === cls.id
                      ? 'border-indigo-500 bg-indigo-50 ring-2 ring-indigo-500/20'
                      : 'border-gray-200 hover:border-gray-300 hover:bg-gray-50'
                  ]"
                >
                  <div class="flex items-center justify-between">
                    <div>
                      <div class="font-medium text-gray-900">{{ cls.name }}</div>
                      <div class="text-sm text-gray-500">{{ cls.subject }} • {{ cls.board || 'N/A' }} • {{ cls.studentCount }} students</div>
                    </div>
                    <div
                      v-if="selectedClass?.id === cls.id"
                      class="h-5 w-5 bg-indigo-600 rounded-full flex items-center justify-center"
                    >
                      <svg class="h-3 w-3 text-white" fill="currentColor" viewBox="0 0 20 20">
                        <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
                      </svg>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Students Section -->
            <div>
              <div class="flex items-center justify-between mb-4">
                <h4 class="text-md font-semibold text-gray-900">Available Students</h4>
                <div class="text-sm text-gray-500">
                  {{ selectedStudents.length }} selected
                </div>
              </div>
              
              <!-- Search -->
              <div class="mb-4">
                <div class="relative">
                  <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
                    </svg>
                  </div>
                  <input
                    v-model="searchQuery"
                    type="text"
                    placeholder="Search students..."
                    class="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                  />
                </div>
              </div>

              <!-- Students List -->
              <div class="space-y-2 max-h-80 overflow-y-auto">
                <!-- Loading State -->
                <div v-if="isLoadingStudents" class="flex items-center justify-center py-8">
                  <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
                  <span class="ml-3 text-gray-600">Loading students...</span>
                </div>
                
                <!-- Empty State -->
                <div v-else-if="!selectedClass" class="text-center py-8 text-gray-500">
                  <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"></path>
                  </svg>
                  <p class="mt-2 text-sm">Select a class to view available students</p>
                </div>
                
                <!-- Students List -->
                <div v-else>
                  <div
                    v-for="student in filteredStudents"
                    :key="student.id"
                    @click="toggleStudentSelection(student)"
                    :class="[
                      'p-3 rounded-lg border cursor-pointer transition-all duration-200',
                      isStudentSelected(student.id)
                        ? 'border-green-500 bg-green-50 ring-2 ring-green-500/20'
                        : isStudentAssignedToOtherClass(student)
                        ? 'border-orange-200 bg-orange-50 hover:border-orange-300'
                        : 'border-gray-200 hover:border-gray-300 hover:bg-gray-50'
                    ]"
                  >
                  <div class="flex items-center justify-between">
                    <div class="flex items-center space-x-3">
                      <div class="h-8 w-8 bg-gradient-to-r from-indigo-500 to-purple-500 rounded-full flex items-center justify-center">
                        <span class="text-white font-medium text-xs">
                          {{ student.firstName.charAt(0) }}{{ student.lastName.charAt(0) }}
                        </span>
                      </div>
                      <div class="flex-1">
                        <div class="font-medium text-gray-900">
                          {{ student.firstName }} {{ student.lastName }}
                        </div>
                        <div class="text-sm text-gray-500">{{ student.studentMobile }}</div>
                        <div class="text-xs mt-1">
                          <span v-if="isStudentAssignedToOtherClass(student)" class="inline-flex items-center px-2 py-1 text-xs font-medium bg-orange-100 text-orange-800 rounded-full">
                            <svg class="w-3 h-3 mr-1" fill="currentColor" viewBox="0 0 20 20">
                              <path fill-rule="evenodd" d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" clip-rule="evenodd"></path>
                            </svg>
                            {{ getCurrentClassInfo(student) }}
                          </span>
                          <span v-else class="inline-flex items-center px-2 py-1 text-xs font-medium bg-green-100 text-green-800 rounded-full">
                            <svg class="w-3 h-3 mr-1" fill="currentColor" viewBox="0 0 20 20">
                              <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"></path>
                            </svg>
                            Available for assignment
                          </span>
                        </div>
                      </div>
                    </div>
                    <div
                      v-if="isStudentSelected(student.id)"
                      class="h-5 w-5 bg-green-600 rounded-full flex items-center justify-center"
                    >
                      <svg class="h-3 w-3 text-white" fill="currentColor" viewBox="0 0 20 20">
                        <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
                      </svg>
                    </div>
                  </div>
                </div>
                </div> <!-- Close v-else container for students list -->
              </div>
            </div>
          </div>

          <!-- Selected Students Summary -->
          <div v-if="selectedStudents.length > 0" class="mt-6 p-4 bg-gray-50 rounded-lg">
            <h4 class="text-sm font-semibold text-gray-900 mb-2">Selected Students ({{ selectedStudents.length }})</h4>
            <div class="flex flex-wrap gap-2">
              <span
                v-for="student in selectedStudents"
                :key="student.id"
                class="inline-flex items-center px-2 py-1 text-xs font-medium bg-indigo-100 text-indigo-800 rounded-full"
              >
                {{ student.firstName }} {{ student.lastName }}
                <button
                  @click="removeStudentSelection(student.id)"
                  class="ml-1 text-indigo-600 hover:text-indigo-800"
                >
                  <svg class="h-3 w-3" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd"></path>
                  </svg>
                </button>
              </span>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="bg-gray-50 px-6 py-4 sm:flex sm:flex-row-reverse sm:px-6">
          <button
            @click="saveMapping"
            :disabled="!selectedClass || selectedStudents.length === 0 || isSaving"
            class="w-full inline-flex justify-center rounded-lg border border-transparent shadow-sm px-4 py-2 bg-gradient-to-r from-indigo-600 to-purple-600 text-base font-medium text-white hover:from-indigo-700 hover:to-purple-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:ml-3 sm:w-auto sm:text-sm transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <div v-if="isSaving" class="flex items-center space-x-2">
              <svg class="animate-spin h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              <span>Mapping...</span>
            </div>
            <span v-else>Map {{ selectedStudents.length }} Students</span>
          </button>
          <button
            @click="$emit('close')"
            class="mt-3 w-full inline-flex justify-center rounded-lg border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm transition-colors duration-200"
          >
            Cancel
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { classesService } from '@/services/classesService'
import { studentsService, type Student } from '@/services/studentsService'

interface Class {
  id: number
  name: string
  subject: string
  board?: string
  studentCount: number
}

interface StudentWithClasses {
  id: number
  firstName: string
  lastName: string
  fullName: string
  admissionNumber?: string
  studentMobile?: string
  isActive: boolean
  studentClasses?: Array<{
    studentId: number
    classId: number
    isActive: boolean
    class?: {
      id: number
      name: string
      board?: string
    }
  }>
}

const emit = defineEmits<{
  close: []
  mappingComplete: []
}>()

// Reactive data
const classes = ref<Class[]>([])
const availableStudents = ref<StudentWithClasses[]>([])
const selectedClass = ref<Class | null>(null)
const selectedStudents = ref<StudentWithClasses[]>([])
const searchQuery = ref('')
const isLoadingStudents = ref(false)
const isSaving = ref(false)

// Computed filtered students
const filteredStudents = computed(() => {
  if (!searchQuery.value) return availableStudents.value
  
  const query = searchQuery.value.toLowerCase()
  return availableStudents.value.filter(student =>
    student.firstName.toLowerCase().includes(query) ||
    student.lastName.toLowerCase().includes(query) ||
    student.fullName.toLowerCase().includes(query) ||
    student.studentMobile?.includes(query)
  )
})

// Methods
const checkAuthStatus = () => {
  const token = localStorage.getItem('token')
  const user = localStorage.getItem('user')
  console.log('Authentication Status:', {
    hasToken: !!token,
    tokenLength: token?.length || 0,
    hasUser: !!user,
    userInfo: user ? JSON.parse(user)?.email || 'No email' : 'No user data'
  })
  return { token, user }
}

const selectClass = async (cls: Class) => {
  selectedClass.value = cls
  selectedStudents.value = []
  isLoadingStudents.value = true
  
  // Load available students for this class
  try {
    console.log(`Loading available students for class: ${cls.name} (ID: ${cls.id})`)
    availableStudents.value = await studentsService.getAvailableStudentsForMapping(cls.id)
    console.log(`Loaded ${availableStudents.value.length} available students`)
  } catch (error: any) {
    console.error('Error loading available students:', error)
    isLoadingStudents.value = false
    
    // Show the actual error message for debugging
    const errorMessage = error?.message || error?.toString() || 'Unknown error occurred'
    console.log('Actual error message:', errorMessage)
    
    // Handle authentication errors gracefully
    if (errorMessage.includes('401') || errorMessage.includes('Authentication required') || errorMessage.includes('session has expired')) {
      // Check if user is actually logged in
      const token = localStorage.getItem('token')
      const user = localStorage.getItem('user')
      
      if (!token && !user) {
        alert('You are not logged in. Please log in to access student mapping.')
      } else {
        alert('Your session has expired. Please refresh the page and log in again.')
      }
      return
    }
    
    // Handle permission errors
    if (errorMessage.includes('403') || errorMessage.includes('Permission denied') || errorMessage.includes('access to student mapping')) {
      alert('You do not have permission to access student mapping. Please contact your administrator.')
      return
    }
    
    // Handle network errors
    if (errorMessage.includes('Network error') || errorMessage.includes('fetch') || errorMessage.includes('Failed to fetch')) {
      alert('Network error: Unable to connect to the server. Please check if the API server is running.')
      return
    }
    
    // Handle CORS errors
    if (errorMessage.includes('CORS') || errorMessage.includes('cross-origin')) {
      alert('CORS error: Please check server configuration for cross-origin requests.')
      return
    }
    
    // Show detailed error for debugging
    alert(`Error loading students: ${errorMessage}`)
    availableStudents.value = []
  } finally {
    isLoadingStudents.value = false
  }
}

const toggleStudentSelection = (student: StudentWithClasses) => {
  const index = selectedStudents.value.findIndex(s => s.id === student.id)
  if (index > -1) {
    selectedStudents.value.splice(index, 1)
  } else {
    selectedStudents.value.push(student)
  }
}

const isStudentSelected = (studentId: number) => {
  return selectedStudents.value.some(s => s.id === studentId)
}

const isStudentAssignedToOtherClass = (student: StudentWithClasses) => {
  return student.studentClasses && 
         student.studentClasses.some((sc: any) => sc.isActive && sc.class)
}

const getCurrentClassInfo = (student: StudentWithClasses) => {
  if (!student.studentClasses) return 'No class assigned'
  
  const activeClass = student.studentClasses.find((sc: any) => sc.isActive && sc.class)
  if (!activeClass || !activeClass.class) return 'No class assigned'
  
  const classInfo = activeClass.class
  return classInfo.board ? `${classInfo.name} (${classInfo.board})` : classInfo.name
}

const removeStudentSelection = (studentId: number) => {
  selectedStudents.value = selectedStudents.value.filter(s => s.id !== studentId)
}

const saveMapping = async () => {
  if (!selectedClass.value || selectedStudents.value.length === 0) return
  
  isSaving.value = true
  
  try {
    console.log('Starting mapping for class:', selectedClass.value.name)
    console.log('Students to map:', selectedStudents.value.map(s => `${s.firstName} ${s.lastName}`))
    
    // Map all selected students to the class
    const mappingPromises = selectedStudents.value.map(async student => {
      console.log(`Mapping student ${student.firstName} ${student.lastName} (ID: ${student.id}) to class ${selectedClass.value!.name} (ID: ${selectedClass.value!.id})`)
      return await studentsService.mapStudentToClass(student.id, selectedClass.value!.id)
    })
    
    const results = await Promise.allSettled(mappingPromises)
    
    // Check if any mappings failed
    const failures = results.filter(result => result.status === 'rejected')
    if (failures.length > 0) {
      console.error('Some mappings failed:', failures)
      throw new Error(`${failures.length} out of ${selectedStudents.value.length} mappings failed`)
    }
    
    console.log('All mappings completed successfully')
    alert(`Successfully mapped ${selectedStudents.value.length} students to ${selectedClass.value.name}!`)
    
    emit('mappingComplete')
    emit('close')
  } catch (error) {
    console.error('Error saving mapping:', error)
    
    // Show a more user-friendly error message
    if (error instanceof Error) {
      if (error.message.includes('401') || error.message.includes('Unauthorized')) {
        alert('Authentication error: Please log in again to continue.')
      } else if (error.message.includes('403') || error.message.includes('Forbidden')) {
        alert('Permission denied: You do not have permission to map students to classes.')
      } else if (error.message.includes('404') || error.message.includes('not found')) {
        alert('Error: Student or class not found. Please refresh and try again.')
      } else {
        alert(`Error mapping students: ${error.message}`)
      }
    } else {
      alert('An unexpected error occurred while mapping students. Please try again.')
    }
  } finally {
    isSaving.value = false
  }
}

// Load classes on mount
onMounted(async () => {
  try {
    const classesData = await classesService.getClasses()
    classes.value = classesData.map((c: any) => ({
      id: c.id,
      name: c.name,
      subject: c.subjects && c.subjects.length > 0 ? c.subjects[0] : 'N/A',
      board: c.board || 'N/A',
      studentCount: c.studentCount || 0
    }))
  } catch (error) {
    console.error('Error loading classes:', error)
  }
})
</script>
