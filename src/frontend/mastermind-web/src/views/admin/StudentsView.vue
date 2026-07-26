<template>
  <div class="space-y-6">
    <!-- Header Section with Background -->
    <div class="bg-gradient-to-r from-blue-600 to-indigo-600 rounded-2xl shadow-xl p-5 sm:p-8 text-white">
      <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
        <div>
          <h1 class="text-2xl sm:text-3xl font-bold mb-2">Students Management</h1>
          <p class="text-blue-100 text-base sm:text-lg">Manage student records, enrollment, and academic progress</p>
          <div class="mt-4 flex flex-wrap items-center gap-3 sm:gap-4">
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
        <div class="flex flex-col sm:flex-row gap-3">
          <button
            @click="downloadAllStudents"
            class="rounded-xl border border-white/40 bg-white/15 px-5 py-3 font-semibold text-white transition hover:bg-white/25"
          >
            Download All Students (Excel)
          </button>
          <button
            @click="showAddModal = true"
            class="bg-white text-indigo-600 px-5 py-3 rounded-xl font-semibold hover:bg-blue-50 transition-all duration-200 shadow-lg hover:shadow-xl transform hover:scale-105"
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
            class="bg-gradient-to-r from-green-600 to-emerald-600 text-white px-5 py-3 rounded-xl font-semibold hover:from-green-700 hover:to-emerald-700 transition-all duration-200 shadow-lg hover:shadow-xl transform hover:scale-105"
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
      <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-6">
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
          <label class="block text-sm font-medium text-gray-700 mb-2">School</label>
          <select
            v-model="filters.school"
            class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
          >
            <option value="">All Schools</option>
            <option v-for="school in schoolOptions" :key="school.key" :value="school.key">
              {{ school.label }} ({{ school.count }})
            </option>
          </select>
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">View</label>
          <select
            v-model="filters.groupBy"
            class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
          >
            <option value="">Student List</option>
            <option value="school">Group by School</option>
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
      <div ref="studentTableScroller" class="max-h-[70vh] overflow-auto">
        <table class="min-w-[940px] w-full divide-y divide-gray-200">
          <thead class="sticky top-0 z-20 bg-gray-50">
            <tr>
              <th scope="col" class="sticky left-0 z-30 bg-gray-50 px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Student
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Class
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Contact & Parents
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                School
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Enrollment
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th scope="col" class="sticky right-0 z-30 bg-gray-50 px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <template v-for="group in groupedStudents" :key="group.key">
            <tr v-if="filters.groupBy === 'school'" class="bg-indigo-50">
              <td colspan="7" class="px-6 py-2 text-sm font-semibold text-indigo-900">
                {{ group.label }} · {{ group.students.length }} student{{ group.students.length === 1 ? '' : 's' }}
              </td>
            </tr>
            <tr v-for="student in group.students" :key="student.id" class="hover:bg-gray-50 transition-colors duration-150">
              <td class="sticky left-0 z-10 bg-white px-6 py-4 whitespace-nowrap">
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
                <div class="mt-1 flex flex-wrap gap-1">
                  <span 
                    v-for="(medium, index) in getStudentMediums(student)" 
                    :key="index"
                    class="inline-flex items-center px-2 py-1 text-xs font-medium bg-purple-100 text-purple-800 rounded-full"
                  >
                    {{ medium }}
                  </span>
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
                <div class="text-xs text-gray-500">Primary: {{ student.parentMobile || 'No parent mobile' }}</div>
                <div v-if="student.secondaryParentMobile" class="text-xs text-gray-400">Secondary: {{ student.secondaryParentMobile }}</div>
                <div class="mt-2 flex flex-col space-y-1 border-t border-gray-100 pt-2">
                  <div v-if="student.motherName" class="text-xs text-gray-500">Mother: {{ student.motherName }}</div>
                  <div v-if="student.fatherName" class="text-xs text-gray-500">Father: {{ student.fatherName }}</div>
                  <div v-if="!student.motherName && !student.fatherName" class="text-xs text-gray-500">
                    Parent: {{ student.parentName || 'Not provided' }}
                  </div>
                  <div class="text-xs text-gray-400">{{ student.parentEmail }}</div>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm font-medium text-gray-900">{{ student.currentSchool }}</div>
                <div class="text-xs text-gray-400">Current School</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm font-medium text-gray-900">{{ formatDate(student.admissionDate) }}</div>
                <div class="text-xs text-gray-400">Admission Date</div>
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
              <td class="sticky right-0 z-10 bg-white px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                <button
                  @click="sendInvitation(student)"
                  :disabled="student.parentOnboarded"
                  class="mr-3 font-medium text-emerald-600 transition-colors duration-150 hover:text-emerald-900 disabled:cursor-not-allowed disabled:text-gray-400"
                >
                  {{ student.parentOnboarded ? 'Parent Joined' : 'Send Invite' }}
                </button>
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
            </template>
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
        <div class="inline-block align-bottom bg-white rounded-2xl text-left overflow-hidden shadow-2xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full w-full">
          <div class="bg-gradient-to-r from-indigo-600 to-purple-600 px-6 py-4">
            <h3 class="text-lg font-semibold text-white">
              {{ showEditModal ? 'Edit Student' : 'Add New Student' }}
            </h3>
          </div>
          <form @submit.prevent="submitForm">
            <div class="bg-white px-6 py-5">
              <p class="mb-4 text-xs text-gray-500"><span class="text-red-600">*</span> Required fields</p>
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">First Name <span class="text-red-600" aria-hidden="true">*</span></label>
                  <input
                    v-model="form.firstName"
                    type="text"
                    required
                    class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200"
                  />
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Last Name <span class="text-red-600" aria-hidden="true">*</span></label>
                  <input
                    v-model="form.lastName"
                    type="text"
                    required
                    class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200"
                  />
                </div>
              </div>
              <div class="mt-4">
                <label class="block text-sm font-medium text-gray-700 mb-2">Student Mobile</label>
                <input
                  v-model="form.phone"
                  type="tel"
                  class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200"
                />
              </div>
              <div class="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Primary Parent Mobile <span class="text-red-600" aria-hidden="true">*</span></label>
                  <input
                    v-model="form.parentMobile"
                    type="tel"
                    required
                    inputmode="numeric"
                    maxlength="14"
                    class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200"
                  />
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Secondary Parent Mobile</label>
                  <input
                    v-model="form.secondaryParentMobile"
                    type="tel"
                    inputmode="numeric"
                    maxlength="14"
                    class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200"
                  />
                </div>
              </div>
              <p class="mt-2 text-xs leading-5 text-gray-500">The primary number is the parent's login identity. The parent adds their own recovery email from the invitation link.</p>
              <div class="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
                <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">Admission Date <span class="text-red-600" aria-hidden="true">*</span></label>
                <input
                  v-model="form.admissionDate"
                  type="date"
                  required
                  class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200"
                />
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Date of Birth <span class="text-red-600" aria-hidden="true">*</span></label>
                  <input
                    v-model="form.dateOfBirth"
                    type="date"
                    required
                    :max="todayDateInput()"
                    class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200"
                  />
                </div>
              </div>
              <div class="mt-4">
                <label class="block text-sm font-medium text-gray-700 mb-2">Class <span class="text-red-600" aria-hidden="true">*</span></label>
                <select v-model.number="form.classId" required class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200">
                  <option value="">Select Class</option>
                  <option v-for="cls in classes" :key="cls.id" :value="cls.id">
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
                  type="file"
                  accept="image/*"
                  @change="onPhotoSelected"
                  class="block w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors duration-200"
                />
                <p class="mt-1 text-xs text-gray-500">Max 5MB. Allowed: jpg, jpeg, png, gif, webp.</p>
                <div v-if="selectedPhotoPreviewUrl || form.photo" class="mt-3">
                  <img
                    :src="selectedPhotoPreviewUrl || form.photo"
                    alt="Student photo preview"
                    class="h-16 w-16 rounded-lg object-cover border border-gray-200"
                  />
                </div>
              </div>
              <div class="mt-4">
                <label class="block text-sm font-medium text-gray-700 mb-2">Status <span class="text-red-600" aria-hidden="true">*</span></label>
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
import { ref, computed, onMounted, watch } from 'vue'
import { formatClassForDropdown } from '@/utils/classUtils'
import { classesService } from '@/services/classesService'
import { studentsService, normalizeSchoolKey, resolveParentNames, type Student, type CreateStudentRequest } from '@/services/studentsService'
import { normalizeIndianMobile } from '@/utils/phoneUtils'
import { buildParentInvitationWhatsAppMessage } from '@/utils/parentInvitation'
import { apiService } from '@/services/apiService'
import { API_ENDPOINTS } from '@/config/api'
import { useSessionStore } from '@/stores/session'
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
  parentName: string
  parentEmail: string
  parentMobile: string
  secondaryParentMobile: string
  admissionDate: string
  classId?: number | null
  className: string
  classMedium: string
  classBoard: string
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
  parentOnboarded: boolean
}

// Reactive data
const sessionStore = useSessionStore()
const students = ref<ExtendedStudent[]>([])
const classes = ref<Class[]>([])

// Modal states
const showAddModal = ref(false)
const showEditModal = ref(false)
const showMappingModal = ref(false)
const editingStudent = ref<ExtendedStudent | null>(null)
const selectedPhotoFile = ref<File | null>(null)
const selectedPhotoPreviewUrl = ref('')
const todayDateInput = () => {
  const today = new Date()
  const offset = today.getTimezoneOffset()
  return new Date(today.getTime() - offset * 60_000).toISOString().slice(0, 10)
}

// Form data
const form = ref<CreateStudentRequest>({
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  parentEmail: '',
  parentMobile: '',
  secondaryParentMobile: '',
  admissionDate: todayDateInput(),
  classId: null,
  className: '',
  status: 'Active' as 'Active' | 'Inactive',
  parentName: '',
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
    const matchesSchool = !filters.value.school ||
      normalizeSchoolKey(student.currentSchool) === filters.value.school
    
    return matchesSearch && matchesStatus && matchesClass && matchesSchool
  }).sort((a, b) => {
    switch (filters.value.sortBy) {
      case 'name':
        return `${a.firstName} ${a.lastName}`.localeCompare(`${b.firstName} ${b.lastName}`)
      case 'enrollmentDate':
        return new Date(b.admissionDate).getTime() - new Date(a.admissionDate).getTime()
      case 'class':
        return (a.className || '').localeCompare(b.className || '')
      default:
        return 0
    }
  })
})

const schoolOptions = computed(() => {
  const schools = new Map<string, { key: string; label: string; count: number }>()
  for (const student of students.value) {
    const key = normalizeSchoolKey(student.currentSchool)
    if (!key) continue
    const existing = schools.get(key)
    if (existing) existing.count += 1
    else schools.set(key, { key, label: student.currentSchool.trim(), count: 1 })
  }
  return [...schools.values()].sort((a, b) => a.label.localeCompare(b.label))
})

const groupedStudents = computed(() => {
  if (filters.value.groupBy !== 'school') {
    return [{ key: 'all', label: 'All Students', students: filteredStudents.value }]
  }

  const groups = new Map<string, { key: string; label: string; students: ExtendedStudent[] }>()
  for (const student of filteredStudents.value) {
    const key = normalizeSchoolKey(student.currentSchool) || '__unspecified'
    const label = student.currentSchool.trim() || 'School not specified'
    const existing = groups.get(key)
    if (existing) existing.students.push(student)
    else groups.set(key, { key, label, students: [student] })
  }
  return [...groups.values()].sort((a, b) => a.label.localeCompare(b.label))
})

// Helper functions for student class information
const getStudentMediums = (student: any) => {
  return student.classMedium ? [student.classMedium] : []
}

const getStudentBoards = (student: any) => {
  return student.classBoard ? [student.classBoard] : []
}

// Computed statistics
const activeStudents = computed(() => {
  return students.value.filter(student => student.status === 'Active').length
})

const newStudents = computed(() => {
  const currentMonth = new Date().getMonth()
  const currentYear = new Date().getFullYear()
  return students.value.filter(student => {
    const admissionDate = new Date(student.admissionDate)
    return admissionDate.getMonth() === currentMonth && admissionDate.getFullYear() === currentYear
  }).length
})

const formatDate = (value?: string) => {
  if (!value) return 'Not set'
  const date = new Date(`${value.slice(0, 10)}T00:00:00`)
  return Number.isNaN(date.getTime())
    ? 'Not set'
    : new Intl.DateTimeFormat('en-IN', { day: '2-digit', month: 'short', year: 'numeric' }).format(date)
}

// Filters object
const filters = ref({
  search: '',
  class: '',
  status: '',
  school: '',
  groupBy: '',
  sortBy: 'name'
})

// Clear filters method
const clearFilters = () => {
  filters.value = {
    search: '',
    class: '',
    status: '',
    school: '',
    groupBy: '',
    sortBy: 'name'
  }
}

// Load initial data
const loadData = async () => {
  const selectedSessionId = sessionStore.selectedSessionId
  if (!selectedSessionId) {
    students.value = []
    classes.value = []
    return
  }

  try {
    // Load students using centralized service
    const studentsResponse = await studentsService.getStudents(1, 50, undefined, selectedSessionId)
    students.value = studentsResponse.data.map((student: any) => {
      const activeClass = student.studentClasses?.find((sc: any) => sc.isActive)
      const parentNames = resolveParentNames(student.parentName, student.motherName, student.fatherName)

      return {
        id: student.id,
        firstName: student.firstName,
        lastName: student.lastName,
        email: student.studentEmail || '',
        phone: student.studentMobile || '',
        parentName: student.parentName || '',
        parentEmail: student.parentEmail || '',
        parentMobile: student.parentMobile || '',
        secondaryParentMobile: student.secondaryParentMobile || '',
        admissionDate: student.admissionDate?.slice(0, 10) || '',
        classId: activeClass?.classId ?? activeClass?.class?.id ?? null,
        className: activeClass?.class?.name || 'Not Assigned',
        classMedium: activeClass?.class?.medium || '',
        classBoard: activeClass?.class?.board || '',
        status: student.isActive ? 'Active' : 'Inactive',
        motherName: parentNames.motherName,
        fatherName: parentNames.fatherName,
        address: student.address,
        currentSchool: student.currentSchool || '',
        photo: student.profileImageUrl,
        dateOfBirth: student.dateOfBirth?.slice(0, 10) || '',
        gender: student.gender === 0 ? 'Male' : student.gender === 1 ? 'Female' : 'Other',
        whatsappNumber: student.parentMobile,
        textNumber: student.parentMobile,
        aadharNumber: '',
        caste: '',
        rollNumber: '',
        standard: '',
        parentOnboarded: Boolean(student.parentOnboarded),
        createdAt: student.createdAt
      }
    })
  } catch (error) {
    console.error('Error loading students:', error)
  }

  // Load classes from API
  try {
    const classesData = await classesService.getClasses(selectedSessionId)
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
    parentEmail: '',
    parentMobile: '',
    secondaryParentMobile: '',
    admissionDate: todayDateInput(),
    classId: null,
    className: '',
    status: 'Active' as 'Active' | 'Inactive',
    parentName: '',
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
  selectedPhotoFile.value = null
  selectedPhotoPreviewUrl.value = ''
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

const onPhotoSelected = (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0] ?? null
  selectedPhotoFile.value = file

  if (!file) {
    selectedPhotoPreviewUrl.value = ''
    return
  }

  selectedPhotoPreviewUrl.value = URL.createObjectURL(file)
}

const editStudent = (student: ExtendedStudent) => {
  editingStudent.value = student
  form.value = {
    firstName: student.firstName,
    lastName: student.lastName,
    email: student.email,
    phone: student.phone,
    parentEmail: student.parentEmail,
    parentMobile: student.parentMobile,
    secondaryParentMobile: student.secondaryParentMobile,
    admissionDate: student.admissionDate,
    classId: student.classId ?? null,
    className: student.className,
    status: student.status,
    parentName: student.parentName,
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
  selectedPhotoFile.value = null
  selectedPhotoPreviewUrl.value = student.photo || ''
  showEditModal.value = true
}

const submitForm = async () => {
  try {
    if (showEditModal.value && editingStudent.value) {
      // Update existing student via API
      const updatedStudent = await studentsService.updateStudent(editingStudent.value.id, form.value)
      await syncStudentClassMapping(updatedStudent.id, editingStudent.value.classId ?? null)
      let photoUrl = form.value.photo || editingStudent.value.photo
      if (selectedPhotoFile.value) {
        const uploadResult = await studentsService.uploadStudentPhoto(editingStudent.value.id, selectedPhotoFile.value)
        photoUrl = uploadResult.url
      }
      if (photoUrl) form.value.photo = photoUrl
    } else {
      // Add new student via API
      const newStudent = await studentsService.createStudent(form.value, sessionStore.selectedSessionId ?? undefined)
      await syncStudentClassMapping(newStudent.id, null)
      let photoUrl = form.value.photo || ''
      if (selectedPhotoFile.value && newStudent?.id) {
        const uploadResult = await studentsService.uploadStudentPhoto(newStudent.id, selectedPhotoFile.value)
        photoUrl = uploadResult.url
      }
      if (photoUrl) form.value.photo = photoUrl
    }
    await loadData()
    closeModal()
  } catch (error) {
    console.error('Error saving student:', error)
    const message = (error as any)?.response?.data?.message || (error as any)?.message || 'Failed to save student. Please try again.'
    alert(message)
  }
}

const syncStudentClassMapping = async (studentId: number, previousClassId: number | null) => {
  const selectedClassId = form.value.classId ? Number(form.value.classId) : null

  if (selectedClassId && selectedClassId !== previousClassId) {
    try {
      await studentsService.mapStudentToClass(studentId, selectedClassId)
    } catch (error: any) {
      const message = error?.response?.data?.message || error?.message || ''
      if (!message.toLowerCase().includes('already mapped')) {
        throw error
      }
    }
  }

  if (previousClassId && previousClassId !== selectedClassId) {
    await studentsService.unmapStudentFromClass(studentId, previousClassId)
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

const sendInvitation = async (student: ExtendedStudent) => {
  const inviteWindow = window.open('about:blank', '_blank')
  try {
    const invitation = await studentsService.createParentInvitation(student.id)
    if (!invitation.inviteUrl) throw new Error('The invite link was not returned.')
    const phone = normalizeIndianMobile(invitation.primaryMobile || student.parentMobile)
    if (!phone) throw new Error('A valid primary parent mobile number is required.')
    const parentName = student.motherName || student.fatherName || 'Parent'
    const message = buildParentInvitationWhatsAppMessage({
      parentName,
      studentName: `${student.firstName} ${student.lastName}`,
      inviteUrl: invitation.inviteUrl
    })
    const whatsappUrl = `https://wa.me/${phone}?text=${encodeURIComponent(message)}`
    if (inviteWindow) inviteWindow.location.href = whatsappUrl
    else {
      await navigator.clipboard.writeText(invitation.inviteUrl)
      alert('WhatsApp could not be opened. The invitation link has been copied.')
    }
  } catch (error: any) {
    inviteWindow?.close()
    alert(error.response?.data?.message || error.message || 'Parent invitation could not be created. Please try again.')
    await loadData()
  }
}

const downloadAllStudents = async () => {
  try {
    await studentsService.downloadAllStudents()
  } catch (error: any) {
    alert(error.response?.data?.message || error.message || 'Student Excel export could not be downloaded.')
  }
}

const handleMappingComplete = async () => {
  // Reload students data to reflect the new mappings
  await loadData()
}

onMounted(async () => {
  if (sessionStore.sessions.length === 0) {
    await sessionStore.loadSessions()
  }
  await loadData()
})

watch(() => sessionStore.selectedSessionId, () => {
  loadData()
})
</script>

<style scoped>
/* Additional styles if needed */
</style>
