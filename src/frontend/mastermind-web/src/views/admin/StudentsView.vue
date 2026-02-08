<template>
  <div class="space-y-6">
    <!-- Header Section with Background -->
    <div class="bg-gradient-to-r from-blue-600 to-indigo-600 rounded-2xl shadow-xl p-8 text-white">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-3xl font-bold mb-2">Students Management</h1>
          <p class="text-blue-100 text-lg">Manage student records, enrollment, and academic progress</p>
          <div class="mt-4 flex items-center space-x-4">
            <div class="flex items-center space-x-2">
              <div class="h-8 w-8 bg-white/20 rounded-lg flex items-center justify-center">
                <svg class="h-5 w-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z"></path>
                </svg>
              </div>
              <span class="text-white font-medium">{{ students.length }} Total Students</span>
            </div>
            <div class="h-6 w-px bg-white/30"></div>
            <div class="flex items-center space-x-2">
              <div class="h-8 w-8 bg-white/20 rounded-lg flex items-center justify-center">
                <svg class="h-5 w-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                </svg>
              </div>
              <span class="text-white font-medium">{{ activeStudents }} Active</span>
            </div>
          </div>
        </div>
        <button
          @click="showAddModal = true"
          class="bg-white text-indigo-600 px-6 py-3 rounded-xl font-semibold hover:bg-blue-50 transition-all duration-200 shadow-lg hover:shadow-xl transform hover:scale-105 mr-3"
        >
          <div class="flex items-center space-x-2">
            <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"></path>
            </svg>
            <span>Add New Student</span>
          </div>
        </button>
        <button
          @click="showMappingModal = true"
          class="bg-gradient-to-r from-green-600 to-emerald-600 text-white px-6 py-3 rounded-xl font-semibold hover:from-green-700 hover:to-emerald-700 transition-all duration-200 shadow-lg hover:shadow-xl transform hover:scale-105"
        >
          <div class="flex items-center space-x-2">
            <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01"></path>
            </svg>
            <span>Map Students to Classes</span>
          </div>
        </button>
      </div>
    </div>

    <!-- Stats Cards -->
    <div class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
      <div class="bg-white rounded-xl shadow-lg p-6 border border-gray-100 hover:shadow-xl transition-shadow duration-300">
        <div class="flex items-center">
          <div class="flex-shrink-0">
            <div class="h-12 w-12 bg-gradient-to-r from-blue-500 to-blue-600 rounded-xl flex items-center justify-center">
              <svg class="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z"></path>
              </svg>
            </div>
          </div>
          <div class="ml-4">
            <div class="text-sm font-medium text-gray-500">Total Students</div>
            <div class="text-2xl font-bold text-gray-900">{{ students.length }}</div>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-xl shadow-lg p-6 border border-gray-100 hover:shadow-xl transition-shadow duration-300">
        <div class="flex items-center">
          <div class="flex-shrink-0">
            <div class="h-12 w-12 bg-gradient-to-r from-green-500 to-green-600 rounded-xl flex items-center justify-center">
              <svg class="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
            </div>
          </div>
          <div class="ml-4">
            <div class="text-sm font-medium text-gray-500">Active Students</div>
            <div class="text-2xl font-bold text-gray-900">{{ activeStudents }}</div>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-xl shadow-lg p-6 border border-gray-100 hover:shadow-xl transition-shadow duration-300">
        <div class="flex items-center">
          <div class="flex-shrink-0">
            <div class="h-12 w-12 bg-gradient-to-r from-yellow-500 to-yellow-600 rounded-xl flex items-center justify-center">
              <svg class="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
            </div>
          </div>
          <div class="ml-4">
            <div class="text-sm font-medium text-gray-500">New This Month</div>
            <div class="text-2xl font-bold text-gray-900">{{ newStudents }}</div>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-xl shadow-lg p-6 border border-gray-100 hover:shadow-xl transition-shadow duration-300">
        <div class="flex items-center">
          <div class="flex-shrink-0">
            <div class="h-12 w-12 bg-gradient-to-r from-purple-500 to-purple-600 rounded-xl flex items-center justify-center">
              <svg class="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"></path>
              </svg>
            </div>
          </div>
          <div class="ml-4">
            <div class="text-sm font-medium text-gray-500">Total Classes</div>
            <div class="text-2xl font-bold text-gray-900">{{ classes.length }}</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Filters Section -->
    <div class="bg-white rounded-xl shadow-lg p-6 border border-gray-100">
      <div class="flex items-center justify-between mb-4">
        <h2 class="text-lg font-semibold text-gray-900">Filters & Search</h2>
        <button
          @click="clearFilters"
          class="text-sm text-indigo-600 hover:text-indigo-800 font-medium"
        >
          Clear All
        </button>
      </div>
      <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">Search Students</label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
              </svg>
            </div>
            <input
              v-model="filters.search"
              type="text"
              placeholder="Search by name, email, phone..."
              class="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
            />
          </div>
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">Class</label>
          <select
            v-model="filters.class"
            class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
          >
            <option value="">All Classes</option>
            <option v-for="cls in classes" :key="cls.id" :value="cls.name">
              {{ cls.name }}
            </option>
          </select>
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">Status</label>
          <select
            v-model="filters.status"
            class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
          >
            <option value="">All Status</option>
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
            <option value="Graduated">Graduated</option>
          </select>
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">Sort By</label>
          <select
            v-model="filters.sortBy"
            class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
          >
            <option value="name">Name</option>
            <option value="enrollmentDate">Enrollment Date</option>
            <option value="class">Class</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Students Table -->
    <div class="bg-white rounded-xl shadow-lg border border-gray-100 overflow-hidden">
      <div class="px-6 py-4 border-b border-gray-200">
        <div class="flex items-center justify-between">
          <h2 class="text-lg font-semibold text-gray-900">Student Records</h2>
          <div class="text-sm text-gray-500">
            Showing {{ filteredStudents.length }} of {{ students.length }} students
          </div>
        </div>
      </div>
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Student
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Class
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Medium
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Board
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Contact
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Parents
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                School
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th scope="col" class="relative px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="student in filteredStudents" :key="student.id" class="hover:bg-gray-50 transition-colors duration-150">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center">
                  <div class="h-10 w-10 flex-shrink-0">
                    <img 
                      v-if="student.photo" 
                      :src="student.photo" 
                      :alt="`${student.firstName} ${student.lastName}`"
                      class="h-10 w-10 rounded-full object-cover"
                      @error="handleImageError"
                    />
                    <div 
                      v-else 
                      class="h-10 w-10 rounded-full bg-gradient-to-r from-indigo-500 to-purple-500 flex items-center justify-center"
                    >
                      <span class="text-white font-medium text-sm">
                        {{ student.firstName.charAt(0) }}{{ student.lastName.charAt(0) }}
                      </span>
                    </div>
                  </div>
                  <div class="ml-4">
                    <div class="text-sm font-medium text-gray-900">
                      {{ student.firstName }} {{ student.lastName }}
                    </div>
                    <div class="text-sm text-gray-500">
                      {{ student.email }}
                    </div>
                    <div class="text-xs text-gray-400 mt-1">{{ student.className || 'Not Assigned' }} • ID: {{ student.id.toString().padStart(4, '0') }}</div>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm text-gray-900">{{ student.className || 'Not Assigned' }}</div>
                <div class="text-xs text-gray-500">ID: {{ student.id.toString().padStart(4, '0') }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex flex-wrap gap-1">
                  <span 
                    v-for="(medium, index) in getStudentMediums(student)" 
                    :key="index"
                    class="inline-flex items-center px-2 py-1 text-xs font-medium bg-purple-100 text-purple-800 rounded-full"
                  >
                    {{ medium }}
                  </span>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex flex-wrap gap-1">
                  <span 
                    v-for="(board, index) in getStudentBoards(student)" 
                    :key="index"
                    class="inline-flex items-center px-2 py-1 text-xs font-medium bg-green-100 text-green-800 rounded-full"
                  >
                    {{ board }}
                  </span>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm text-gray-900">{{ student.phone }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex flex-col space-y-1">
                  <div class="text-sm font-medium text-gray-900">Parents</div>
                  <div class="text-xs text-gray-500">Mother: {{ student.motherName }}</div>
                  <div class="text-xs text-gray-500">Father: {{ student.fatherName }}</div>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm font-medium text-gray-900">{{ student.currentSchool }}</div>
                <div class="text-xs text-gray-400">Current School</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span
                  :class="[
                    student.status === 'Active' 
                      ? 'bg-green-100 text-green-800 ring-1 ring-green-500/20' 
                      : 'bg-red-100 text-red-800 ring-1 ring-red-500/20'
                  ]"
                  class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                >
                  <div class="h-1.5 w-1.5 rounded-full mr-1.5" :class="
                    student.status === 'Active' ? 'bg-green-400' : 'bg-red-400'
                  "></div>
                  {{ student.status }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                <button 
                  @click="editStudent(student)" 
                  class="text-indigo-600 hover:text-indigo-900 font-medium mr-3 transition-colors duration-150"
                >
                  Edit
                </button>
                <button 
                  @click="deleteStudent(student.id)" 
                  class="text-red-600 hover:text-red-900 font-medium transition-colors duration-150"
                >
                  Delete
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <!-- Empty State -->
      <div v-if="filteredStudents.length === 0" class="text-center py-12">
        <div class="mx-auto h-12 w-12 text-gray-400">
          <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z"></path>
          </svg>
        </div>
        <h3 class="mt-2 text-sm font-medium text-gray-900">No students found</h3>
        <p class="mt-1 text-sm text-gray-500">Try adjusting your search or filter criteria</p>
      </div>
    </div>

    <!-- Add/Edit Student Modal -->
    <div v-if="showAddModal || showEditModal" class="fixed inset-0 z-50 overflow-y-auto">
      <div class="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
        <div class="fixed inset-0 transition-opacity" @click="closeModal">
          <div class="absolute inset-0 bg-gray-900 opacity-50 backdrop-blur-sm"></div>
        </div>
        <div class="inline-block align-bottom bg-white rounded-2xl text-left overflow-hidden shadow-2xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
          <div class="bg-gradient-to-r from-indigo-600 to-purple-600 px-6 py-4">
            <h3 class="text-lg font-semibold text-white">
              {{ showEditModal ? 'Edit Student' : 'Add New Student' }}
            </h3>
          </div>
          <form @submit.prevent="submitForm">
            <div class="bg-white px-6 py-5">
              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">First Name</label>
                  <input
                    v-model="form.firstName"
                    type="text"
                    required
                    class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200"
                  />
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Last Name</label>
                  <input
                    v-model="form.lastName"
                    type="text"
                    required
                    class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200"
                  />
                </div>
              </div>
              <div class="mt-4">
                <label class="block text-sm font-medium text-gray-700 mb-2">Email</label>
                <input
                  v-model="form.email"
                  type="email"
                  required
                  class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200"
                />
              </div>
              <div class="mt-4">
                <label class="block text-sm font-medium text-gray-700 mb-2">Phone</label>
                <input
                  v-model="form.phone"
                  type="tel"
                  required
                  class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200"
                />
              </div>
              <div class="mt-4">
                <label class="block text-sm font-medium text-gray-700 mb-2">Class</label>
                <select v-model="form.className" required class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200">
                  <option value="">Select Class</option>
                  <option v-for="cls in classes" :key="cls.id" :value="cls.name">
                    {{ formatClassForDropdown(cls) }}
                  </option>
                </select>
              </div>
              <div class="mt-4">
                <label class="block text-sm font-medium text-gray-700 mb-2">Mother Name</label>
                <input
                  v-model="form.motherName"
                  type="text"
                  class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200"
                />
              </div>
              <div class="mt-4">
                <label class="block text-sm font-medium text-gray-700 mb-2">Father Name</label>
                <input
                  v-model="form.fatherName"
                  type="text"
                  class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200"
                />
              </div>
              <div class="mt-4">
                <label class="block text-sm font-medium text-gray-700 mb-2">Address</label>
                <textarea
                  v-model="form.address"
                  rows="2"
                  class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200"
                ></textarea>
              </div>
              <div class="mt-4">
                <label class="block text-sm font-medium text-gray-700 mb-2">Current School</label>
                <input
                  v-model="form.currentSchool"
                  type="text"
                  class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200"
                />
              </div>
              <div class="mt-4">
                <label class="block text-sm font-medium text-gray-700 mb-2">Photo</label>
                <input
                  v-model="form.photo"
                  type="text"
                  placeholder="Enter photo URL or upload file"
                  class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200"
                />
              </div>
              <div class="mt-4">
                <label class="block text-sm font-medium text-gray-700 mb-2">Status</label>
                <select v-model="form.status" required class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200">
                  <option value="Active">Active</option>
                  <option value="Inactive">Inactive</option>
                </select>
              </div>
            </div>
            <div class="bg-gray-50 px-6 py-4 sm:flex sm:flex-row-reverse sm:px-6">
              <button
                type="submit"
                class="w-full inline-flex justify-center rounded-lg border border-transparent shadow-sm px-4 py-2 bg-gradient-to-r from-indigo-600 to-purple-600 text-base font-medium text-white hover:from-indigo-700 hover:to-purple-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:ml-3 sm:w-auto sm:text-sm transition-all duration-200"
              >
                {{ showEditModal ? 'Update' : 'Add' }} Student
              </button>
              <button
                type="button"
                @click="closeModal"
                class="mt-3 w-full inline-flex justify-center rounded-lg border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm transition-colors duration-200"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Student-Class Mapping Modal -->
    <StudentClassMapping
      v-if="showMappingModal"
      @close="showMappingModal = false"
      @mapping-complete="handleMappingComplete"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { formatClassForDropdown } from '@/utils/classUtils'
import { classesService } from '@/services/classesService'
import { studentsService, type Student, type CreateStudentRequest } from '@/services/studentsService'
import { apiService } from '@/services/apiService'
import { API_ENDPOINTS } from '@/config/api'
import StudentClassMapping from '@/components/admin/StudentClassMapping.vue'

interface Class {
  id: number
  name: string
  subject: string
  teacher: string
  studentCount: number
}

interface ExtendedStudent extends Student {
  email: string
  phone: string
  className: string
  status: 'Active' | 'Inactive'
  createdAt: string
  motherName: string
  fatherName: string
  address: string
  currentSchool: string
  photo: string
  dateOfBirth: string
  gender: 'Male' | 'Female' | 'Other'
  whatsappNumber: string
  textNumber: string
  aadharNumber: string
  caste: string
  rollNumber: string
  standard: string
}

// Reactive data
const students = ref<ExtendedStudent[]>([])
const classes = ref<Class[]>([])

// Modal states
const showAddModal = ref(false)
const showEditModal = ref(false)
const showMappingModal = ref(false)
const editingStudent = ref<ExtendedStudent | null>(null)

// Form data
const form = ref<CreateStudentRequest>({
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  className: '',
  status: 'Active' as 'Active' | 'Inactive',
  motherName: '',
  fatherName: '',
  address: '',
  currentSchool: '',
  photo: '',
  dateOfBirth: '',
  gender: 'Male' as 'Male' | 'Female' | 'Other',
  whatsappNumber: '',
  textNumber: '',
  aadharNumber: '',
  caste: '',
  rollNumber: '',
  standard: ''
})

// Computed filtered students
const filteredStudents = computed(() => {
  return students.value.filter(student => {
    const matchesSearch = !filters.value.search || 
      `${student.firstName} ${student.lastName}`.toLowerCase().includes(filters.value.search.toLowerCase()) ||
      student.email.toLowerCase().includes(filters.value.search.toLowerCase()) ||
      student.phone.includes(filters.value.search)
    const matchesClass = !filters.value.class || student.className === filters.value.class
    const matchesStatus = !filters.value.status || student.status === filters.value.status
    
    return matchesSearch && matchesStatus && matchesClass
  }).sort((a, b) => {
    switch (filters.value.sortBy) {
      case 'name':
        return `${a.firstName} ${a.lastName}`.localeCompare(`${b.firstName} ${b.lastName}`)
      case 'enrollmentDate':
        return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
      case 'class':
        return (a.className || '').localeCompare(b.className || '')
      default:
        return 0
    }
  })
})

// Helper functions for student class information
const getStudentMediums = (student: any) => {
  // Extract medium from student's class information
  if (student.className) {
    // For now, return a default medium - this could be enhanced to extract from class data
    return ['English'] // Default medium
  }
  return []
}

const getStudentBoards = (student: any) => {
  // Extract board from student's class information
  if (student.className && student.className.includes('CBSE')) {
    return ['CBSE']
  } else if (student.className && student.className.includes('State')) {
    return ['State Board']
  }
  return ['CBSE'] // Default board
}

// Computed statistics
const activeStudents = computed(() => {
  return students.value.filter(student => student.status === 'Active').length
})

const newStudents = computed(() => {
  const currentMonth = new Date().getMonth()
  const currentYear = new Date().getFullYear()
  return students.value.filter(student => {
    const createdDate = new Date(student.createdAt)
    return createdDate.getMonth() === currentMonth && createdDate.getFullYear() === currentYear
  }).length
})

// Filters object
const filters = ref({
  search: '',
  class: '',
  status: '',
  sortBy: 'name'
})

// Clear filters method
const clearFilters = () => {
  filters.value = {
    search: '',
    class: '',
    status: '',
    sortBy: 'name'
  }
}

// Load initial data
const loadData = async () => {
  try {
    // Load students using centralized service
    const studentsResponse = await studentsService.getStudents(1, 50)
    students.value = studentsResponse.data.map((student: any) => ({
      id: student.id,
      firstName: student.firstName,
      lastName: student.lastName,
      email: student.studentEmail,
      phone: student.studentMobile,
      className: student.studentClasses?.find((sc: any) => sc.isActive)?.class?.name || 'Not Assigned',
      status: student.isActive ? 'Active' : 'Inactive',
      motherName: student.parentName?.split(' ')[0] || '',
      fatherName: student.parentName?.split(' ').slice(1).join(' ') || '',
      address: student.address,
      currentSchool: '',
      photo: student.profileImageUrl,
      dateOfBirth: student.dateOfBirth,
      gender: student.gender === 0 ? 'Male' : student.gender === 1 ? 'Female' : 'Other',
      whatsappNumber: student.parentMobile,
      textNumber: student.parentMobile,
      aadharNumber: '',
      caste: '',
      rollNumber: '',
      standard: '',
      createdAt: student.createdAt
    }))
  } catch (error) {
    console.error('Error loading students:', error)
  }

  // Load classes from API
  try {
    const classesData = await classesService.getClasses()
    classes.value = classesData.map((c: any) => ({
      id: c.id,
      name: c.name,
      subject: c.subjects && c.subjects.length > 0 ? c.subjects[0] : '',
      teacher: c.teachers && c.teachers.length > 0 ? c.teachers[0] : '',
      studentCount: c.studentCount
    }))
  } catch (error) {
    console.error('Error loading classes:', error)
  }
}

// Form methods
const resetForm = () => {
  form.value = {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    className: '',
    status: 'Active' as 'Active' | 'Inactive',
    motherName: '',
    fatherName: '',
    address: '',
    currentSchool: '',
    photo: '',
    dateOfBirth: '',
    gender: 'Male' as 'Male' | 'Female' | 'Other',
    whatsappNumber: '',
    textNumber: '',
    aadharNumber: '',
    caste: '',
    rollNumber: '',
    standard: ''
  }
}

const handleImageError = () => {
  console.log('Image loading error')
}

const closeModal = () => {
  showAddModal.value = false
  showEditModal.value = false
  editingStudent.value = null
  resetForm()
}

const editStudent = (student: ExtendedStudent) => {
  editingStudent.value = student
  form.value = {
    firstName: student.firstName,
    lastName: student.lastName,
    email: student.email,
    phone: student.phone,
    className: student.className,
    status: student.status,
    motherName: student.motherName,
    fatherName: student.fatherName,
    address: student.address,
    currentSchool: student.currentSchool,
    photo: student.photo,
    dateOfBirth: student.dateOfBirth,
    gender: student.gender as 'Male' | 'Female' | 'Other',
    whatsappNumber: student.whatsappNumber,
    textNumber: student.textNumber,
    aadharNumber: student.aadharNumber,
    caste: student.caste,
    rollNumber: student.rollNumber,
    standard: student.standard
  }
  showEditModal.value = true
}

const submitForm = async () => {
  try {
    if (showEditModal.value && editingStudent.value) {
      // Update existing student via API
      await studentsService.updateStudent(editingStudent.value.id, form.value)
      const index = students.value.findIndex(s => s.id === editingStudent.value!.id)
      if (index !== -1) {
        students.value[index] = {
          ...editingStudent.value,
          ...form.value
        }
      }
    } else {
      // Add new student via API
      const newStudent = await studentsService.createStudent(form.value)
      students.value.push({
        ...newStudent,
        id: newStudent.id || Date.now(),
        createdAt: newStudent.createdAt || new Date().toISOString()
      })
    }
    closeModal()
  } catch (error) {
    console.error('Error saving student:', error)
    alert('Failed to save student. Please try again.')
  }
}

const deleteStudent = async (id: number) => {
  if (confirm('Are you sure you want to delete this student?')) {
    try {
      await studentsService.deleteStudent(id)
      students.value = students.value.filter(s => s.id !== id)
    } catch (error) {
      console.error('Error deleting student:', error)
      alert('Failed to delete student. Please try again.')
    }
  }
}

const handleMappingComplete = async () => {
  // Reload students data to reflect the new mappings
  await loadData()
}

onMounted(() => {
  loadData()
})
</script>

<style scoped>
/* Additional styles if needed */
</style>