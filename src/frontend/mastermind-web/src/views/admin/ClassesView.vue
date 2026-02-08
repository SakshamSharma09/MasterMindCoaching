<template>
  <div class="px-4 sm:px-6 lg:px-8">
    <div class="sm:flex sm:items-center">
      <div class="sm:flex-auto">
        <h1 class="text-2xl font-semibold text-gray-900">Classes Management</h1>
        <p class="mt-2 text-sm text-gray-700">
          Manage classes, subjects, and class assignments.
        </p>
      </div>
      <div class="mt-4 sm:mt-0 sm:ml-16 sm:flex-none">
        <button
          type="button"
          @click="showAddModal = true"
          class="inline-flex items-center justify-center rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
        >
          Add New Class
        </button>
      </div>
    </div>

    <!-- Search and Filter -->
    <div class="mt-6 grid grid-cols-1 md:grid-cols-2 gap-4">
      <div>
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search classes..."
          class="input-primary"
        />
      </div>
      <div>
        <select v-model="selectedSubject" class="input-primary">
          <option value="">All Subjects</option>
          <option v-for="subject in availableSubjects" :key="subject" :value="subject">
            {{ subject }}
          </option>
        </select>
      </div>
    </div>

    <div class="mt-8 flex flex-col">
      <div class="-my-2 -mx-4 overflow-x-auto sm:-mx-6 lg:-mx-8">
        <div class="inline-block min-w-full py-2 align-middle md:px-6 lg:px-8">
          <div class="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
            <table class="min-w-full divide-y divide-gray-300">
              <thead class="bg-gray-50">
                <tr>
                  <th scope="col" class="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">
                    Class Name
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Subjects
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Medium
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Board
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Teachers
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Status
                  </th>
                  <th scope="col" class="relative py-3.5 pl-3 pr-4 sm:pr-6">
                    <span class="sr-only">Actions</span>
                  </th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200 bg-white">
                <tr v-for="classItem in filteredClasses" :key="classItem.id">
                  <td class="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">
                    {{ classItem.name }}
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <div class="flex flex-wrap gap-1">
                      <span 
                        v-for="(subject, index) in classItem.subjects" 
                        :key="index"
                        class="inline-flex items-center px-2 py-1 text-xs font-medium bg-blue-100 text-blue-800 rounded-full"
                      >
                        {{ subject }}
                      </span>
                    </div>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <span class="inline-flex items-center px-2 py-1 text-xs font-medium bg-purple-100 text-purple-800 rounded-full">
                      {{ classItem.medium }}
                    </span>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <span class="inline-flex items-center px-2 py-1 text-xs font-medium bg-green-100 text-green-800 rounded-full">
                      {{ classItem.board }}
                    </span>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <span class="inline-flex items-center px-2 py-1 text-xs font-medium bg-orange-100 text-orange-800 rounded-full">
                      {{ classItem.teachers.join(', ') }}
                    </span>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <span
                      :class="[
                        classItem.status === 'Active' 
                          ? 'bg-green-100 text-green-800 ring-1 ring-green-500/20' 
                          : 'bg-red-100 text-red-800 ring-1 ring-red-500/20'
                      ]"
                      class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                    >
                      {{ classItem.status }}
                    </span>
                  </td>
                  <td class="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                    <button @click="editClass(classItem)" class="text-indigo-600 hover:text-indigo-900 mr-4">
                      Edit
                    </button>
                    <button @click="deleteClass(classItem.id)" class="text-red-600 hover:text-red-900">
                      Delete
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <!-- Add/Edit Class Modal -->
    <div v-if="showAddModal || showEditModal" class="fixed inset-0 z-50 overflow-y-auto">
      <div class="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
        <div class="fixed inset-0 transition-opacity" @click="closeModal">
          <div class="absolute inset-0 bg-gray-500 opacity-75"></div>
        </div>
        <div class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
          <form @submit.prevent="submitForm">
            <div class="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
              <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">
                {{ showEditModal ? 'Edit Class' : 'Add New Class' }}
              </h3>
              <div class="grid grid-cols-2 gap-4">
                <div class="col-span-2">
                  <label class="block text-sm font-medium text-gray-700">Class Name</label>
                  <input
                    v-model="form.name"
                    type="text"
                    required
                    placeholder="e.g., Class 10A"
                    class="mt-1 input-primary"
                  />
                </div>
              </div>
              <div class="mt-4">
                <SubjectInput
                  v-model="form.subjects"
                  :suggestions="availableSubjects"
                  label="Subjects"
                  placeholder="e.g., Mathematics, Science, etc."
                  @subject-added="handleSubjectAdded"
                  @subject-removed="handleSubjectRemoved"
                />
              </div>
              <div class="mt-4">
                <label class="block text-sm font-medium text-gray-700">Medium</label>
                <select v-model="form.medium" required class="mt-1 input-primary">
                  <option value="English">English</option>
                  <option value="Hindi">Hindi</option>
                  <option value="Regional">Regional</option>
                </select>
              </div>
              <div class="mt-4">
                <label class="block text-sm font-medium text-gray-700">Board of Study</label>
                <select v-model="form.board" required class="mt-1 input-primary">
                  <option value="CBSE">CBSE</option>
                  <option value="RBSE">RBSE</option>
                  <option value="ICSE">ICSE</option>
                  <option value="Competitive Exam">Competitive Exam</option>
                </select>
              </div>
              <div class="mt-4">
                <label class="block text-sm font-medium text-gray-700">Status</label>
                <select v-model="form.status" required class="mt-1 input-primary">
                  <option value="Active">Active</option>
                  <option value="Inactive">Inactive</option>
                </select>
              </div>
            </div>
            <div class="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
              <button
                type="submit"
                class="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:ml-3 sm:w-auto sm:text-sm"
              >
                {{ showEditModal ? 'Update' : 'Add' }} Class
              </button>
              <button
                type="button"
                @click="closeModal"
                class="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import SubjectInput from '@/components/common/SubjectInput.vue'
import classesService, { type Class, type CreateClassDto } from '@/services/classesService'
import { apiService } from '@/services/apiService'
import { API_ENDPOINTS } from '@/config/api'

// Reactive data
const classes = ref<Class[]>([])
const searchQuery = ref('')
const selectedSubject = ref('')
const availableSubjects = ref<string[]>([])

// Modal states
const showAddModal = ref(false)
const showEditModal = ref(false)
const editingClass = ref<Class | null>(null)

// Form data
const form = ref({
  name: '',
  subjects: [] as string[],
  medium: 'English',
  board: 'CBSE',
  status: 'Active' as 'Active' | 'Inactive'
})

// Computed filtered classes
const filteredClasses = computed(() => {
  return classes.value.filter(classItem => {
    const matchesSearch = classItem.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                          classItem.teachers.join(',').toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                          classItem.subjects.join(',').toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchesSubject = !selectedSubject.value || classItem.subjects.includes(selectedSubject.value)

    return matchesSearch && matchesSubject
  })
})

// API methods
const fetchClasses = async () => {
  try {
    classes.value = await classesService.getClasses()
    
    // Extract unique subjects from all classes
    const allSubjects = new Set<string>()
    classes.value.forEach(cls => {
      cls.subjects.forEach(subject => allSubjects.add(subject))
    })
    availableSubjects.value = Array.from(allSubjects).sort()
  } catch (error) {
    console.error('Error fetching classes:', error)
  }
}

const fetchSubjects = async () => {
  try {
    const result = await apiService.get(API_ENDPOINTS.SUBJECTS.LIST)
    const subjects = result || []
    availableSubjects.value = subjects.map((s: any) => s.name).sort()
  } catch (error) {
    console.error('Error fetching subjects:', error)
  }
}

// Event handlers
const handleSubjectAdded = (subject: string) => {
  console.log('Subject added:', subject)
  // Add to available subjects if not already present
  if (!availableSubjects.value.includes(subject)) {
    availableSubjects.value.push(subject)
    availableSubjects.value.sort()
  }
}

const handleSubjectRemoved = (subject: string) => {
  console.log('Subject removed:', subject)
}

// Form methods
const resetForm = () => {
  form.value = {
    name: '',
    subjects: [],
    medium: 'English',
    board: 'CBSE',
    status: 'Active'
  }
}

const closeModal = () => {
  showAddModal.value = false
  showEditModal.value = false
  editingClass.value = null
  resetForm()
}

const editClass = (classItem: Class) => {
  editingClass.value = classItem
  form.value = {
    name: classItem.name,
    subjects: [...classItem.subjects],
    medium: classItem.medium,
    board: classItem.board,
    status: classItem.status
  }
  showEditModal.value = true
}

const submitForm = async () => {
  try {
    const classData = {
      name: form.value.name,
      subjects: form.value.subjects,
      medium: form.value.medium,
      board: form.value.board,
      status: form.value.status
    }

    if (showEditModal.value && editingClass.value) {
      // Update existing class
      await classesService.updateClass(editingClass.value.id, classData)
    } else {
      // Add new class
      await classesService.createClass(classData)
    }

    await fetchClasses() // Refresh data
    closeModal()
  } catch (error: any) {
    console.error('Error submitting form:', error)
    alert(error.response?.data?.message || 'Failed to save class. Please try again.')
  }
}

const deleteClass = async (id: number) => {
  if (confirm('Are you sure you want to delete this class? This will also remove all associated students.')) {
    try {
      await classesService.deleteClass(id)
      await fetchClasses() // Refresh data
    } catch (error) {
      console.error('Error deleting class:', error)
    }
  }
}

onMounted(async () => {
  await fetchSubjects()
  await fetchClasses()
})

</script>

<style scoped>
/* Additional styles if needed */
</style>