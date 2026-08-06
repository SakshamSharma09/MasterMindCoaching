<template>
  <div class="px-4 sm:px-6 lg:px-8">
    <div class="sm:flex sm:items-center">
      <div class="sm:flex-auto">
        <h1 class="text-2xl font-semibold text-gray-900">Teachers Management</h1>
        <p class="mt-2 text-sm text-gray-700">
          Manage teacher records and assignments.
        </p>
      </div>
      <div class="mt-4 sm:mt-0 sm:ml-16 sm:flex-none">
        <button
          @click="openAddModal"
          type="button"
          class="inline-flex items-center justify-center rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
        >
          Add New Teacher
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="mt-8 flex justify-center">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="mt-8 rounded-md bg-red-50 p-4">
      <div class="flex">
        <div class="ml-3">
          <h3 class="text-sm font-medium text-red-800">Error loading teachers</h3>
          <div class="mt-2 text-sm text-red-700">{{ error }}</div>
          <div class="mt-4">
            <button @click="fetchTeachers" class="bg-red-100 px-3 py-2 rounded-md text-sm font-medium text-red-800 hover:bg-red-200">
              Try Again
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Teachers Table -->
    <div v-else class="mt-8 flex flex-col">
      <div class="space-y-3 md:hidden">
        <article v-for="teacher in teachers" :key="`mobile-${teacher.id}`" class="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
          <div class="flex items-start justify-between gap-3">
            <div class="flex min-w-0 items-center gap-3">
              <img v-if="teacher.profileImageUrl" :src="teacher.profileImageUrl" :alt="teacher.fullName" class="h-12 w-12 shrink-0 rounded-full object-cover ring-2 ring-indigo-100" />
              <div v-else class="flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-indigo-500 to-teal-500 text-sm font-bold text-white">
                {{ teacherInitials(teacher) }}
              </div>
              <div class="min-w-0">
                <h2 class="break-words font-bold leading-5 text-slate-950">{{ teacher.fullName }}</h2>
                <p class="mt-1 text-sm text-slate-500">{{ teacher.mobile }}</p>
              </div>
            </div>
            <span class="rounded-full px-2.5 py-1 text-xs font-bold" :class="teacher.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'">{{ teacher.isActive ? 'Active' : 'Inactive' }}</span>
          </div>
          <p class="mt-3 text-sm text-slate-600">DOB: {{ formatDate(teacher.dateOfBirth) }}</p>
          <p class="mt-1 text-sm text-slate-600">{{ getTeacherClasses(teacher).join(', ') || 'No classes assigned' }}</p>
          <div class="mt-4 grid grid-cols-3 gap-2">
            <button type="button" class="min-h-11 rounded-xl bg-indigo-600 px-2 text-xs font-bold text-white" @click="shareTeacherInvite(teacher)">Invite</button>
            <button type="button" class="min-h-11 rounded-xl border border-slate-200 px-2 text-xs font-bold text-slate-700" @click="openEditModal(teacher)">Edit</button>
            <button type="button" class="min-h-11 rounded-xl border border-red-200 px-2 text-xs font-bold text-red-700" @click="deleteTeacher(teacher.id)">Delete</button>
          </div>
        </article>
      </div>
      <div class="-my-2 -mx-4 hidden overflow-x-auto sm:-mx-6 md:block lg:-mx-8">
        <div class="inline-block min-w-full py-2 align-middle md:px-6 lg:px-8">
          <div class="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
            <table class="min-w-full divide-y divide-gray-300">
              <thead class="bg-gray-50">
                <tr>
                  <th scope="col" class="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">
                    Name
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Specialization
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Classes
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Medium
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Board
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Phone
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Date of Birth
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Joining Date
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Monthly Salary
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
                <tr v-for="teacher in teachers" :key="teacher.id">
                  <td class="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">
                    <div class="flex items-center gap-3">
                      <img v-if="teacher.profileImageUrl" :src="teacher.profileImageUrl" :alt="teacher.fullName" class="h-10 w-10 rounded-full object-cover" />
                      <div v-else class="flex h-10 w-10 items-center justify-center rounded-full bg-gradient-to-br from-indigo-500 to-teal-500 text-xs font-bold text-white">{{ teacherInitials(teacher) }}</div>
                      <span>{{ teacher.fullName }}</span>
                    </div>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <span class="inline-flex items-center px-2 py-1 text-xs font-medium bg-blue-100 text-blue-800 rounded-full">
                      {{ teacher.specialization || 'Not specified' }}
                    </span>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <div class="flex flex-wrap gap-1">
                      <span 
                        v-for="(classItem, index) in getTeacherClasses(teacher)" 
                        :key="index"
                        class="inline-flex items-center px-2 py-1 text-xs font-medium bg-indigo-100 text-indigo-800 rounded-full"
                      >
                        {{ classItem }}
                      </span>
                    </div>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <div class="flex flex-wrap gap-1">
                      <span 
                        v-for="(medium, index) in getTeacherMediums(teacher)" 
                        :key="index"
                        class="inline-flex items-center px-2 py-1 text-xs font-medium bg-purple-100 text-purple-800 rounded-full"
                      >
                        {{ medium }}
                      </span>
                    </div>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <div class="flex flex-wrap gap-1">
                      <span 
                        v-for="(board, index) in getTeacherBoards(teacher)" 
                        :key="index"
                        class="inline-flex items-center px-2 py-1 text-xs font-medium bg-green-100 text-green-800 rounded-full"
                      >
                        {{ board }}
                      </span>
                    </div>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    {{ teacher.mobile }}
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    {{ formatDate(teacher.dateOfBirth) }}
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    {{ formatDate(teacher.joiningDate) }}
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm font-medium text-gray-900">
                    {{ formatSalary(teacher.monthlySalary) }}
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <span
                      :class="[
                        teacher.isActive 
                          ? 'bg-green-100 text-green-800 ring-1 ring-green-500/20' 
                          : 'bg-red-100 text-red-800 ring-1 ring-red-500/20'
                      ]"
                      class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                    >
                      {{ teacher.isActive ? 'Active' : 'Inactive' }}
                    </span>
                  </td>
                  <td class="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                    <button @click="shareTeacherInvite(teacher)" class="mr-4 text-emerald-700 hover:text-emerald-900">
                      Invite
                    </button>
                    <button @click="openEditModal(teacher)" class="text-indigo-600 hover:text-indigo-900 mr-4">
                      Edit
                    </button>
                    <button @click="deleteTeacher(teacher.id)" class="text-red-600 hover:text-red-900">
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

    <!-- Add/Edit Modal -->
    <div v-if="showModal" class="fixed inset-0 z-50 overflow-y-auto" aria-labelledby="modal-title" role="dialog" aria-modal="true">
      <div class="flex items-end justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
        <div class="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" aria-hidden="true"></div>
        <span class="hidden sm:inline-block sm:align-middle sm:h-screen" aria-hidden="true">&#8203;</span>
        <div class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-6xl sm:w-full">
          <form @submit.prevent="saveTeacher">
            <div class="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
              <div class="sm:flex sm:items-start">
                <div class="mt-3 text-center sm:mt-0 sm:text-left w-full">
                  <h3 class="text-lg leading-6 font-medium text-gray-900" id="modal-title">
                    {{ isEditing ? 'Edit Teacher' : 'Add New Teacher' }}
                  </h3>
                  <div class="mt-4">
                    <!-- Broad Horizontal Layout -->
                    <div class="grid grid-cols-1 xl:grid-cols-4 gap-4">
                      <!-- Personal Information -->
                      <div class="bg-gray-50 p-4 rounded-lg">
                        <h4 class="text-sm font-medium text-gray-900 mb-3">Personal Information</h4>
                        <div class="space-y-4">
                          <div>
                            <label for="firstName" class="block text-sm font-medium text-gray-700 mb-1">First Name *</label>
                            <input
                              v-model="teacherForm.firstName"
                              type="text"
                              id="firstName"
                              required
                              class="block w-full px-3 py-2 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors"
                              placeholder="Enter first name"
                            />
                          </div>
                          <div>
                            <label for="lastName" class="block text-sm font-medium text-gray-700 mb-1">Last Name *</label>
                            <input
                              v-model="teacherForm.lastName"
                              type="text"
                              id="lastName"
                              required
                              class="block w-full px-3 py-2 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors"
                              placeholder="Enter last name"
                            />
                          </div>
                          <div>
                            <label for="email" class="block text-sm font-medium text-gray-700 mb-1">Email (optional)</label>
                            <input
                              v-model="teacherForm.email"
                              type="email"
                              id="email"
                              class="block w-full px-3 py-2 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors"
                              placeholder="Teacher adds recovery email from invite"
                            />
                            <p class="mt-1 text-xs text-gray-500">The mobile number is primary. The teacher can add their own recovery email from the private invitation.</p>
                          </div>
                          <div>
                            <label for="mobile" class="block text-sm font-medium text-gray-700 mb-1">Mobile *</label>
                            <input
                              v-model="teacherForm.mobile"
                              type="tel"
                              id="mobile"
                              required
                              class="block w-full px-3 py-2 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors"
                              placeholder="+91 9876543210"
                            />
                          </div>
                          <div>
                            <label for="teacherDateOfBirth" class="block text-sm font-medium text-gray-700 mb-1">Date of Birth *</label>
                            <input
                              v-model="teacherForm.dateOfBirth"
                              type="date"
                              id="teacherDateOfBirth"
                              :max="getLocalDate()"
                              required
                              class="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors"
                            />
                          </div>
                          <div>
                            <label for="teacherPhoto" class="block text-sm font-medium text-gray-700 mb-1">Profile Photo</label>
                            <input
                              id="teacherPhoto"
                              type="file"
                              accept="image/jpeg,image/png,image/gif,image/webp"
                              class="block w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm file:mr-3 file:rounded-md file:border-0 file:bg-indigo-50 file:px-3 file:py-1.5 file:font-medium file:text-indigo-700"
                              @change="onTeacherPhotoSelected"
                            />
                            <p class="mt-1 text-xs text-gray-500">JPG, PNG, GIF or WebP up to 5 MB.</p>
                            <img
                              v-if="teacherPhotoPreviewUrl || teacherForm.profileImageUrl"
                              :src="teacherPhotoPreviewUrl || teacherForm.profileImageUrl"
                              alt="Teacher photo preview"
                              class="mt-3 h-20 w-20 rounded-full object-cover ring-2 ring-indigo-100"
                            />
                          </div>
                        </div>
                      </div>

                      <!-- Academic Information -->
                      <div class="bg-blue-50 p-4 rounded-lg">
                        <h4 class="text-sm font-medium text-gray-900 mb-3">Academic Information</h4>
                        <div class="space-y-4">
                          <div>
                            <label for="specialization" class="block text-sm font-medium text-gray-700 mb-1">Specialization</label>
                            <input
                              v-model="teacherForm.specialization"
                              type="text"
                              id="specialization"
                              class="block w-full px-3 py-2 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors"
                              placeholder="e.g., Mathematics, Science"
                            />
                          </div>
                          <div>
                            <label for="qualification" class="block text-sm font-medium text-gray-700 mb-1">Qualification</label>
                            <input
                              v-model="teacherForm.qualification"
                              type="text"
                              id="qualification"
                              class="block w-full px-3 py-2 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors"
                              placeholder="e.g., M.Sc. Mathematics, B.Ed."
                            />
                          </div>

                          <!-- Subjects Selection -->
                          <div>
                            <label class="block text-sm font-medium text-gray-700 mb-2">Subjects Taught *</label>
                            <div class="border border-gray-300 rounded-md p-3 max-h-32 overflow-y-auto bg-white">
                              <div class="grid grid-cols-2 gap-2">
                                <label v-for="subject in availableSubjects" :key="subject" class="flex items-center hover:bg-gray-50 px-2 py-1 rounded cursor-pointer transition-colors">
                                  <input
                                    v-model="teacherForm.subjects"
                                    :value="subject"
                                    type="checkbox"
                                    class="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                                  />
                                  <span class="ml-2 text-sm text-gray-700">{{ subject }}</span>
                                </label>
                              </div>
                            </div>
                            <p class="mt-2 text-sm text-indigo-600 font-medium">Selected: {{ teacherForm.subjects.length }} subjects</p>
                          </div>
                        </div>
                      </div>

                      <!-- Classes Selection -->
                      <div class="bg-green-50 p-4 rounded-lg">
                        <h4 class="text-sm font-medium text-gray-900 mb-3">Classes the Teacher Teaches *</h4>

                        <!-- Multi-Select Dropdown -->
                        <div class="relative">
                          <button
                            @click="toggleClassesDropdown"
                            type="button"
                            class="relative w-full bg-white border border-gray-300 rounded-md shadow-sm pl-3 pr-10 py-2 text-left cursor-default focus:outline-none focus:ring-1 focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                          >
                            <span class="block truncate">
                              <span v-if="teacherForm.classes.length === 0" class="text-gray-500">Select classes...</span>
                              <span v-else class="text-gray-900">{{ selectedClassesText }}</span>
                            </span>
                            <span class="absolute inset-y-0 right-0 flex items-center pr-2 pointer-events-none">
                              <svg class="h-5 w-5 text-gray-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                                <path fill-rule="evenodd" d="M10 3a1 1 0 01.707.293l3 3a1 1 0 01-1.414 1.414L10 5.414 7.707 7.707a1 1 0 01-1.414-1.414l3-3A1 1 0 0110 3zm-3.707 9.293a1 1 0 011.414 0L10 14.586l2.293-2.293a1 1 0 011.414 1.414l-3 3a1 1 0 01-1.414 0l-3-3a1 1 0 01.707-1.707z" clip-rule="evenodd" />
                              </svg>
                            </span>
                          </button>

                          <!-- Dropdown Options -->
                          <div v-if="showClassesDropdown" class="absolute z-10 mt-1 w-full bg-white shadow-lg max-h-60 rounded-md py-1 text-base ring-1 ring-black ring-opacity-5 overflow-auto focus:outline-none sm:text-sm">
                            <div v-if="loadingClasses" class="flex items-center justify-center py-4">
                              <div class="animate-spin rounded-full h-4 w-4 border-b-2 border-indigo-600"></div>
                              <span class="ml-2 text-sm text-gray-600">Loading classes...</span>
                            </div>
                            <div v-else-if="availableClasses.length === 0" class="py-2 px-3 text-sm text-gray-500">
                              No classes available
                            </div>
                            <label v-else v-for="classItem in availableClasses" :key="classItem.id" class="flex items-center hover:bg-gray-50 px-3 py-2 cursor-pointer">
                              <input
                                v-model="teacherForm.classes"
                                :value="classItem.id"
                                type="checkbox"
                                class="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                              />
                              <span class="ml-3 block font-normal truncate">{{ formatClassForDropdown(classItem) }}</span>
                            </label>
                          </div>
                        </div>

                        <!-- Selected Classes Display -->
                        <div v-if="teacherForm.classes.length > 0" class="mt-3">
                          <p class="text-xs text-gray-600 mb-2">Selected classes:</p>
                          <div class="flex flex-wrap gap-1">
                            <span
                              v-for="classId in teacherForm.classes"
                              :key="classId"
                              class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-indigo-100 text-indigo-800"
                            >
                              {{ getClassNameById(classId) }}
                              <button
                                @click="removeClass(classId)"
                                class="ml-1 inline-flex items-center p-0.5 rounded-full text-indigo-400 hover:bg-indigo-200 hover:text-indigo-500"
                              >
                                <svg class="h-3 w-3" fill="currentColor" viewBox="0 0 20 20">
                                  <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
                                </svg>
                              </button>
                            </span>
                          </div>
                        </div>
                      </div>

                      <!-- Professional Information -->
                      <div class="bg-yellow-50 p-4 rounded-lg">
                        <h4 class="text-sm font-medium text-gray-900 mb-3">Professional Information</h4>
                        <div class="space-y-4">
                          <div>
                            <label for="joiningDate" class="block text-sm font-medium text-gray-700 mb-1">Joining Date *</label>
                            <input
                              v-model="teacherForm.joiningDate"
                              type="date"
                              id="joiningDate"
                              required
                              class="block w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors"
                            />
                          </div>
                          <div>
                            <label for="experienceYears" class="block text-sm font-medium text-gray-700 mb-1">Experience (Years) *</label>
                            <input
                              v-model.number="teacherForm.experienceYears"
                              type="number"
                              id="experienceYears"
                              min="0"
                              required
                              class="block w-full px-3 py-2 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors"
                              placeholder="e.g., 5"
                            />
                          </div>
                          <div>
                            <label for="monthlySalary" class="block text-sm font-medium text-gray-700 mb-1">Monthly Salary (₹) *</label>
                            <input
                              v-model.number="teacherForm.monthlySalary"
                              type="number"
                              id="monthlySalary"
                              min="0"
                              step="100"
                              required
                              class="block w-full px-3 py-2 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors"
                              placeholder="e.g., 25000"
                            />
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
              <button
                type="submit"
                :disabled="saving"
                class="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50"
              >
                <span v-if="saving" class="inline-flex items-center">
                  <svg class="animate-spin -ml-1 mr-3 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  Saving...
                </span>
                <span v-else>{{ isEditing ? 'Update' : 'Create' }}</span>
              </button>
              <button
                @click="closeModal"
                type="button"
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
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { apiService } from '@/services/apiService'
import { API_ENDPOINTS } from '@/config/api'
import { formatClassForDropdown } from '@/utils/classUtils'
import { buildTeacherInvitationWhatsAppUrl } from '@/utils/teacherInvitation'

// Type definitions
interface Teacher {
  id: number
  firstName: string
  lastName: string
  fullName: string
  email: string
  mobile: string
  specialization?: string
  qualification?: string
  subjects?: string
  experienceYears?: number
  monthlySalary?: number
  dateOfBirth?: string
  joiningDate?: string
  profileImageUrl?: string
  isActive?: boolean
  teacherClasses?: any[]
}

// Reactive data
const teachers = ref<Teacher[]>([])
const loading = ref(false)
const error = ref('')
const showModal = ref(false)
const isEditing = ref(false)
const saving = ref(false)
const selectedTeacherPhotoFile = ref<File | null>(null)
const teacherPhotoPreviewUrl = ref('')
// Available subjects for multi-select
const availableSubjects = ref<string[]>([])

// Reactive data for classes
const availableClasses = ref<any[]>([])
const loadingClasses = ref(false)

// Classes dropdown
const showClassesDropdown = ref(false)

// Class assignment modal - removed as per user request
const getLocalDate = () => {
  const value = new Date()
  value.setMinutes(value.getMinutes() - value.getTimezoneOffset())
  return value.toISOString().slice(0, 10)
}

const teacherForm = ref({
  id: 0,
  firstName: '',
  lastName: '',
  email: '',
  mobile: '',
  specialization: '',
  qualification: '',
  subjects: [] as string[],
  classes: [] as number[], // Array of class IDs
  dateOfBirth: '',
  joiningDate: getLocalDate(),
  experienceYears: 0,
  monthlySalary: null as number | null,
  profileImageUrl: ''
})

// Fetch teachers from API
const fetchTeachers = async () => {
  loading.value = true
  error.value = ''

  try {
    const response = await apiService.get(API_ENDPOINTS.TEACHERS.LIST)
    // Handle both response formats: direct data or wrapped in data property
    teachers.value = response.data || response
    console.log('Teachers data loaded:', teachers.value)
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Failed to fetch teachers'
    console.error('Error fetching teachers:', err)
  } finally {
    loading.value = false
  }
}

const formatDate = (value?: string) => {
  if (!value) return 'Not set'
  return new Date(`${value.slice(0, 10)}T00:00:00`).toLocaleDateString('en-IN', {
    day: '2-digit',
    month: 'short',
    year: 'numeric'
  })
}

const formatSalary = (value?: number) => {
  if (value === null || value === undefined) return 'Not set'
  return `₹${value.toLocaleString('en-IN')}`
}

const isPlaceholderEmail = (value?: string) => !value || value.endsWith('@placeholder.mastermind.local')

const teacherInitials = (teacher: Teacher) =>
  `${teacher.firstName?.charAt(0) || ''}${teacher.lastName?.charAt(0) || ''}`.toUpperCase() || 'T'

const clearTeacherPhotoSelection = () => {
  if (teacherPhotoPreviewUrl.value.startsWith('blob:')) URL.revokeObjectURL(teacherPhotoPreviewUrl.value)
  selectedTeacherPhotoFile.value = null
  teacherPhotoPreviewUrl.value = ''
}

const onTeacherPhotoSelected = (event: Event) => {
  const file = (event.target as HTMLInputElement).files?.[0] || null
  clearTeacherPhotoSelection()
  if (!file) return
  if (file.size > 5 * 1024 * 1024) {
    alert('Teacher photo must be 5 MB or smaller.')
    ;(event.target as HTMLInputElement).value = ''
    return
  }
  selectedTeacherPhotoFile.value = file
  teacherPhotoPreviewUrl.value = URL.createObjectURL(file)
}

// Get teacher classes as array of class names
const getTeacherClasses = (teacher: any) => {
  if (!teacher.teacherClasses || teacher.teacherClasses.length === 0) {
    return []
  }
  return teacher.teacherClasses.map((tc: any) => tc.class?.name || 'Unknown Class')
}

// Get unique mediums from teacher's classes
const getTeacherMediums = (teacher: any) => {
  if (!teacher.teacherClasses || teacher.teacherClasses.length === 0) {
    return []
  }
  const mediums = new Set<string>()
  teacher.teacherClasses.forEach((tc: any) => {
    if (tc.class?.medium) {
      mediums.add(tc.class.medium)
    }
  })
  return Array.from(mediums)
}

// Get unique boards from teacher's classes
const getTeacherBoards = (teacher: any) => {
  if (!teacher.teacherClasses || teacher.teacherClasses.length === 0) {
    return []
  }
  const boards = new Set<string>()
  teacher.teacherClasses.forEach((tc: any) => {
    if (tc.class?.board) {
      boards.add(tc.class.board)
    }
  })
  return Array.from(boards)
}

// Fetch available classes
const fetchClasses = async () => {
  loadingClasses.value = true
  try {
    const response = await apiService.get(API_ENDPOINTS.CLASSES.LIST)
    // Handle both response formats: direct data or wrapped in data property
    availableClasses.value = response.data || response
    
    // Extract unique subjects from classes
    const allSubjects = new Set<string>()
    availableClasses.value.forEach((cls: any) => {
      if (cls.subjects && Array.isArray(cls.subjects)) {
        cls.subjects.forEach((subject: string) => allSubjects.add(subject))
      }
    })
    availableSubjects.value = Array.from(allSubjects).sort()
    console.log('Classes data loaded:', availableClasses.value)
  } catch (err) {
    console.error('Error fetching classes:', err)
  } finally {
    loadingClasses.value = false
  }
}

// Open add modal
const openAddModal = () => {
  isEditing.value = false
  clearTeacherPhotoSelection()
  teacherForm.value = {
    id: 0,
    firstName: '',
    lastName: '',
    email: '',
    mobile: '',
    specialization: '',
    qualification: '',
    subjects: [],
    classes: [],
    dateOfBirth: '',
    joiningDate: getLocalDate(),
    experienceYears: 0,
    monthlySalary: null,
    profileImageUrl: ''
  }
  showModal.value = true
}

// Open edit modal
const openEditModal = (teacher: any) => {
  isEditing.value = true
  clearTeacherPhotoSelection()
  teacherForm.value = {
    id: teacher.id,
    firstName: teacher.firstName,
    lastName: teacher.lastName,
    email: isPlaceholderEmail(teacher.email) ? '' : teacher.email,
    mobile: teacher.mobile,
    specialization: teacher.specialization || '',
    qualification: teacher.qualification || '',
    subjects: teacher.subjects ? teacher.subjects.split(',').map((s: string) => s.trim()) : [],
    classes: teacher.teacherClasses ? teacher.teacherClasses.map((tc: any) => tc.classId) : [],
    dateOfBirth: teacher.dateOfBirth ? teacher.dateOfBirth.slice(0, 10) : '',
    joiningDate: teacher.joiningDate ? teacher.joiningDate.slice(0, 10) : getLocalDate(),
    experienceYears: teacher.experienceYears ?? 0,
    monthlySalary: teacher.monthlySalary,
    profileImageUrl: teacher.profileImageUrl || ''
  }
  showModal.value = true
}

// Close modal
const closeModal = () => {
  showModal.value = false
  clearTeacherPhotoSelection()
  teacherForm.value = {
    id: 0,
    firstName: '',
    lastName: '',
    email: '',
    mobile: '',
    specialization: '',
    qualification: '',
    subjects: [],
    classes: [],
    dateOfBirth: '',
    joiningDate: getLocalDate(),
    experienceYears: 0,
    monthlySalary: null,
    profileImageUrl: ''
  }
}

// Save teacher (create or update)
const saveTeacher = async () => {
  saving.value = true

  try {
    const teacherData = {
      firstName: teacherForm.value.firstName,
      lastName: teacherForm.value.lastName,
      email: teacherForm.value.email,
      mobile: teacherForm.value.mobile,
      specialization: teacherForm.value.specialization,
      qualification: teacherForm.value.qualification,
      subjects: teacherForm.value.subjects.join(', '),
      experienceYears: teacherForm.value.experienceYears,
      monthlySalary: teacherForm.value.monthlySalary,
      dateOfBirth: teacherForm.value.dateOfBirth,
      joiningDate: teacherForm.value.joiningDate,
      isActive: true,
      classIds: teacherForm.value.classes // Send selected class IDs
    }

    const response: any = isEditing.value
      ? await apiService.put(API_ENDPOINTS.TEACHERS.UPDATE(teacherForm.value.id.toString()), teacherData)
      : await apiService.post(API_ENDPOINTS.TEACHERS.CREATE, teacherData)
    const savedTeacher = response.data || response
    const teacherId = isEditing.value ? teacherForm.value.id : savedTeacher?.id

    let photoUploadError = ''
    if (selectedTeacherPhotoFile.value && teacherId) {
      try {
        await apiService.upload(API_ENDPOINTS.TEACHERS.PHOTO(String(teacherId)), selectedTeacherPhotoFile.value)
      } catch (photoError: any) {
        photoUploadError = photoError.response?.data?.message || photoError.message || 'Photo upload failed'
      }
    }

    closeModal()
    await fetchTeachers() // Refresh the list
    if (photoUploadError) {
      alert(`Teacher details were saved, but the photo could not be uploaded: ${photoUploadError}`)
    }
  } catch (err: any) {
    alert(err.response?.data?.message || 'Failed to save teacher')
    console.error('Error saving teacher:', err)
  } finally {
    saving.value = false
  }
}

const shareTeacherInvite = async (teacher: Teacher) => {
  const inviteWindow = window.open('about:blank', '_blank')
  try {
    const response: any = await apiService.post(API_ENDPOINTS.TEACHERS.INVITATION(teacher.id.toString()))
    const invitation = response.data || response
    if (!invitation.inviteUrl) throw new Error('The invitation link was not returned.')
    const whatsappUrl = buildTeacherInvitationWhatsAppUrl(
      invitation.primaryMobile || teacher.mobile,
      teacher.fullName,
      invitation.inviteUrl,
      invitation.whatsAppMessage
    )
    if (inviteWindow) inviteWindow.location.href = whatsappUrl
    else {
      await navigator.clipboard.writeText(invitation.inviteUrl)
      alert('WhatsApp could not be opened. The teacher invitation link has been copied.')
    }
  } catch (err: any) {
    inviteWindow?.close()
    alert(err.response?.data?.message || err.message || 'Teacher invitation could not be created.')
  }
}

// Delete teacher
const deleteTeacher = async (id: number) => {
  if (!confirm('Are you sure you want to delete this teacher?')) {
    return
  }

  try {
    await apiService.delete(API_ENDPOINTS.TEACHERS.DELETE(id.toString()))
    await fetchTeachers() // Refresh the list
  } catch (err: any) {
    alert(err.response?.data?.message || 'Failed to delete teacher')
    console.error('Error deleting teacher:', err)
  }
}

// Classes dropdown functions
const toggleClassesDropdown = () => {
  showClassesDropdown.value = !showClassesDropdown.value
}

const selectedClassesText = computed(() => {
  if (teacherForm.value.classes.length === 0) return 'Select classes...'
  if (teacherForm.value.classes.length === 1) {
    return getClassNameById(teacherForm.value.classes[0])
  }
  return `${teacherForm.value.classes.length} classes selected`
})

const getClassNameById = (classId: number) => {
  const classItem = availableClasses.value.find(c => c.id === classId)
  return classItem ? formatClassForDropdown(classItem) : `Class ${classId}`
}

const removeClass = (classId: number) => {
  const index = teacherForm.value.classes.indexOf(classId)
  if (index > -1) {
    teacherForm.value.classes.splice(index, 1)
  }
}

// Close dropdown when clicking outside
const closeDropdownOnClickOutside = (event: Event) => {
  const target = event.target as HTMLElement
  if (!target.closest('.relative')) {
    showClassesDropdown.value = false
  }
}

// Add event listener for closing dropdown
onMounted(() => {
  document.addEventListener('click', closeDropdownOnClickOutside)
  fetchTeachers()
  fetchClasses()
})

onUnmounted(() => {
  document.removeEventListener('click', closeDropdownOnClickOutside)
  clearTeacherPhotoSelection()
})

// Initialize component - moved to the new onMounted below
</script>

<style scoped>
/* Additional styles if needed */
</style>
