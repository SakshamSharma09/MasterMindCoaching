<template>
  <div class="px-4 sm:px-6 lg:px-8 py-8">
    <!-- Header Section -->
    <div class="sm:flex sm:items-center sm:justify-between">
      <div class="sm:flex-auto">
        <h1 class="text-3xl font-bold text-gray-900 flex items-center">
          <svg class="w-8 h-8 mr-3 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path>
          </svg>
          Leads Management
        </h1>
        <p class="mt-2 text-sm text-gray-600">
          Manage potential student leads, track follow-ups, and convert them to students.
        </p>
      </div>
      <div class="mt-4 sm:mt-0 sm:flex-none">
        <button
          type="button"
          @click="openAddModal"
          :disabled="loading"
          class="inline-flex items-center justify-center rounded-lg border border-transparent bg-indigo-600 px-6 py-3 text-sm font-semibold text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200"
        >
          <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path>
          </svg>
          Add New Lead
        </button>
      </div>
    </div>

    <!-- Filters Section -->
    <div class="mt-8 bg-white rounded-lg shadow-sm border border-gray-200 p-6">
      <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <div>
          <label class="block text-sm font-semibold text-gray-700 mb-2">Search Leads</label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
              </svg>
            </div>
            <input
              v-model="filters.search"
              type="text"
              placeholder="Search by name, phone, or email"
              class="pl-10 block w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm border transition-colors"
            />
          </div>
        </div>
        <div>
          <label class="block text-sm font-semibold text-gray-700 mb-2">Status Filter</label>
          <select
            v-model="filters.status"
            class="block w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm border transition-colors"
          >
            <option value="">All Status</option>
            <option value="New">New</option>
            <option value="Contacted">Contacted</option>
            <option value="Interested">Interested</option>
            <option value="Converted">Converted</option>
            <option value="Lost">Lost</option>
          </select>
        </div>
        <div>
          <label class="block text-sm font-semibold text-gray-700 mb-2">Source Filter</label>
          <select
            v-model="filters.source"
            class="block w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm border transition-colors"
          >
            <option value="">All Sources</option>
            <option value="Website">Website</option>
            <option value="Phone">Phone</option>
            <option value="Referral">Referral</option>
            <option value="Social Media">Social Media</option>
            <option value="Advertisement">Advertisement</option>
            <option value="Other">Other</option>
          </select>
        </div>
        <div class="flex items-end">
          <button
            @click="resetFilters"
            class="w-full inline-flex justify-center rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 transition-colors"
          >
            <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"></path>
            </svg>
            Reset Filters
          </button>
        </div>
      </div>
    </div>

    <!-- Stats Cards -->
    <div class="mt-8 grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-5">
      <div class="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow">
        <div class="p-6">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <div class="h-12 w-12 rounded-full bg-indigo-100 flex items-center justify-center">
                <svg class="h-6 w-6 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path>
                </svg>
              </div>
            </div>
            <div class="ml-5">
              <div class="text-sm font-medium text-gray-500">Total Leads</div>
              <div class="text-2xl font-bold text-gray-900">{{ stats.total }}</div>
            </div>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow">
        <div class="p-6">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <div class="h-12 w-12 rounded-full bg-green-100 flex items-center justify-center">
                <svg class="h-6 w-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                </svg>
              </div>
            </div>
            <div class="ml-5">
              <div class="text-sm font-medium text-gray-500">Interested</div>
              <div class="text-2xl font-bold text-gray-900">{{ stats.interested }}</div>
            </div>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow">
        <div class="p-6">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <div class="h-12 w-12 rounded-full bg-yellow-100 flex items-center justify-center">
                <svg class="h-6 w-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"></path>
                </svg>
              </div>
            </div>
            <div class="ml-5">
              <div class="text-sm font-medium text-gray-500">New Leads</div>
              <div class="text-2xl font-bold text-gray-900">{{ stats.new }}</div>
            </div>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow">
        <div class="p-6">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <div class="h-12 w-12 rounded-full bg-blue-100 flex items-center justify-center">
                <svg class="h-6 w-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z"></path>
                </svg>
              </div>
            </div>
            <div class="ml-5">
              <div class="text-sm font-medium text-gray-500">Contacted</div>
              <div class="text-2xl font-bold text-gray-900">{{ stats.contacted }}</div>
            </div>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow">
        <div class="p-6">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <div class="h-12 w-12 rounded-full bg-purple-100 flex items-center justify-center">
                <svg class="h-6 w-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path>
                </svg>
              </div>
            </div>
            <div class="ml-5">
              <div class="text-sm font-medium text-gray-500">Converted</div>
              <div class="text-2xl font-bold text-gray-900">{{ stats.converted }}</div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="mt-8 flex justify-center">
      <div class="flex items-center space-x-3 text-gray-500">
        <svg class="animate-spin h-5 w-5" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>
        <span>Loading leads...</span>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="mt-8 bg-red-50 border border-red-200 rounded-lg p-6">
      <div class="flex items-center">
        <svg class="h-5 w-5 text-red-400 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
        </svg>
        <div>
          <h3 class="text-sm font-medium text-red-800">Error loading leads</h3>
          <p class="text-sm text-red-700 mt-1">{{ error }}</p>
          <button @click="loadLeads" class="mt-3 text-sm font-medium text-red-600 hover:text-red-500">
            Try again
          </button>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="filteredLeads.length === 0" class="mt-8 text-center">
      <div class="bg-white rounded-lg border border-gray-200 p-12">
        <svg class="mx-auto h-16 w-16 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path>
        </svg>
        <h3 class="mt-4 text-lg font-medium text-gray-900">No leads found</h3>
        <p class="mt-2 text-sm text-gray-500 max-w-sm mx-auto">
          {{ filters.search || filters.status || filters.source ? 'No leads match your current filters. Try adjusting your search criteria.' : 'Start building your pipeline by adding your first lead.' }}
        </p>
        <div class="mt-8">
          <button
            @click="openAddModal"
            class="inline-flex items-center px-6 py-3 border border-transparent text-base font-medium rounded-lg text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-colors shadow-sm"
          >
            <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path>
            </svg>
            Add Your First Lead
          </button>
        </div>
      </div>
    </div>

    <!-- Table Section -->
    <div v-else class="mt-8 flex flex-col">
      <div class="-my-2 -mx-4 overflow-x-auto sm:-mx-6 lg:-mx-8">
        <div class="inline-block min-w-full py-2 align-middle md:px-6 lg:px-8">
          <div class="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
            <table class="min-w-full divide-y divide-gray-300">
              <thead class="bg-gray-50">
                <tr>
                  <th scope="col" class="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">
                    Name
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Phone
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Email
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Status
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Source
                  </th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                    Last Follow-up
                  </th>
                  <th scope="col" class="relative py-3.5 pl-3 pr-4 sm:pr-6">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200 bg-white">
                <tr v-for="lead in filteredLeads" :key="lead.id" class="hover:bg-gray-50 transition-colors">
                  <td class="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">
                    <div class="flex items-center">
                      <div class="h-8 w-8 flex-shrink-0">
                        <div class="h-8 w-8 rounded-full bg-indigo-100 flex items-center justify-center">
                          <span class="text-sm font-medium text-indigo-600">{{ lead.name.charAt(0).toUpperCase() }}</span>
                        </div>
                      </div>
                      <div class="ml-4">
                        <div class="text-sm font-medium text-gray-900">{{ lead.name }}</div>
                        <div class="text-sm text-gray-500" v-if="lead.notes">{{ lead.notes.substring(0, 50) }}{{ lead.notes.length > 50 ? '...' : '' }}</div>
                      </div>
                    </div>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <div class="flex items-center">
                      <svg class="h-4 w-4 text-gray-400 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z"></path>
                      </svg>
                      {{ lead.phone }}
                    </div>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <div class="flex items-center">
                      <svg class="h-4 w-4 text-gray-400 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 4.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"></path>
                      </svg>
                      {{ lead.email }}
                    </div>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <span
                      :class="getStatusClass(lead.status)"
                      class="inline-flex rounded-full px-2 py-1 text-xs font-semibold leading-5"
                    >
                      {{ lead.status }}
                    </span>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <span class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                      {{ lead.source }}
                    </span>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <div class="flex items-center">
                      <svg class="h-4 w-4 text-gray-400 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"></path>
                      </svg>
                      {{ formatDate(lead.lastFollowup) }}
                    </div>
                  </td>
                  <td class="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                    <div class="flex items-center justify-end space-x-2">
                      <button
                        @click="editLead(lead)"
                        class="inline-flex items-center px-3 py-1.5 border border-gray-300 shadow-sm text-xs font-medium rounded text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                      >
                        <svg class="w-3 h-3 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path>
                        </svg>
                        Edit
                      </button>
                      <button
                        @click="followUpLead(lead)"
                        class="inline-flex items-center px-3 py-1.5 border border-green-300 shadow-sm text-xs font-medium rounded text-green-700 bg-white hover:bg-green-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500"
                      >
                        <svg class="w-3 h-3 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"></path>
                        </svg>
                        Follow Up
                      </button>
                      <button
                        @click="confirmDelete(lead)"
                        class="inline-flex items-center px-3 py-1.5 border border-red-300 shadow-sm text-xs font-medium rounded text-red-700 bg-white hover:bg-red-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
                      >
                        <svg class="w-3 h-3 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path>
                        </svg>
                        Delete
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <!-- Add/Edit Modal -->
    <div v-if="showAddModal || showEditModal" class="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
      <div class="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
        <div class="mt-3">
          <h3 class="text-lg leading-6 font-medium text-gray-900">
            {{ showAddModal ? 'Add New Lead' : 'Edit Lead' }}
          </h3>
          <form @submit.prevent="showAddModal ? addLead() : updateLead()" class="mt-4 space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700">Name</label>
              <input
                v-model="form.name"
                type="text"
                required
                class="mt-1 block w-full rounded-lg border-2 border-gray-300 bg-white text-gray-900 placeholder-gray-500 shadow-sm focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500 focus:bg-white sm:text-sm transition-colors"
                placeholder="Enter lead name"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Phone</label>
              <input
                v-model="form.phone"
                type="tel"
                required
                class="mt-1 block w-full rounded-lg border-2 border-gray-300 bg-white text-gray-900 placeholder-gray-500 shadow-sm focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500 focus:bg-white sm:text-sm transition-colors"
                placeholder="Enter phone number"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Email</label>
              <input
                v-model="form.email"
                type="email"
                required
                class="mt-1 block w-full rounded-lg border-2 border-gray-300 bg-white text-gray-900 placeholder-gray-500 shadow-sm focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500 focus:bg-white sm:text-sm transition-colors"
                placeholder="Enter email address"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Status</label>
              <select
                v-model="form.status"
                required
                class="mt-1 block w-full rounded-lg border-2 border-gray-300 bg-white text-gray-900 shadow-sm focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500 focus:bg-white sm:text-sm transition-colors"
              >
                <option value="New">New</option>
                <option value="Contacted">Contacted</option>
                <option value="Interested">Interested</option>
                <option value="Converted">Converted</option>
                <option value="Lost">Lost</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Source</label>
              <select
                v-model="form.source"
                required
                class="mt-1 block w-full rounded-lg border-2 border-gray-300 bg-white text-gray-900 shadow-sm focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500 focus:bg-white sm:text-sm transition-colors"
              >
                <option value="Website">Website</option>
                <option value="Phone">Phone</option>
                <option value="Referral">Referral</option>
                <option value="Social Media">Social Media</option>
                <option value="Advertisement">Advertisement</option>
                <option value="Other">Other</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Notes</label>
              <textarea
                v-model="form.notes"
                rows="3"
                class="mt-1 block w-full rounded-lg border-2 border-gray-300 bg-white text-gray-900 placeholder-gray-500 shadow-sm focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500 focus:bg-white sm:text-sm transition-colors resize-none"
                placeholder="Enter any additional notes"
              ></textarea>
            </div>
            <div class="flex justify-end space-x-3">
              <button
                type="button"
                @click="closeModal"
                class="inline-flex justify-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
              >
                Cancel
              </button>
              <button
                type="submit"
                class="inline-flex justify-center rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
              >
                {{ showAddModal ? 'Add' : 'Update' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Follow Up Modal -->
    <div v-if="showFollowUpModal" class="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
      <div class="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
        <div class="mt-3">
          <h3 class="text-lg leading-6 font-medium text-gray-900">
            Follow Up - {{ followUpForm.name }}
          </h3>
          <form @submit.prevent="saveFollowUp" class="mt-4 space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700">Follow-up Date</label>
              <input
                v-model="followUpForm.nextFollowup"
                type="date"
                required
                class="mt-1 block w-full rounded-lg border-2 border-gray-300 bg-white text-gray-900 shadow-sm focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500 focus:bg-white sm:text-sm transition-colors"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Status</label>
              <select
                v-model="followUpForm.status"
                required
                class="mt-1 block w-full rounded-lg border-2 border-gray-300 bg-white text-gray-900 shadow-sm focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500 focus:bg-white sm:text-sm transition-colors"
              >
                <option value="Contacted">Contacted</option>
                <option value="Interested">Interested</option>
                <option value="Converted">Converted</option>
                <option value="Lost">Lost</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Notes</label>
              <textarea
                v-model="followUpForm.notes"
                rows="3"
                class="mt-1 block w-full rounded-lg border-2 border-gray-300 bg-white text-gray-900 placeholder-gray-500 shadow-sm focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500 focus:bg-white sm:text-sm transition-colors resize-none"
                placeholder="Enter follow-up notes"
              ></textarea>
            </div>
            <div class="flex justify-end space-x-3">
              <button
                type="button"
                @click="showFollowUpModal = false"
                class="inline-flex justify-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
              >
                Cancel
              </button>
              <button
                type="submit"
                class="inline-flex justify-center rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
              >
                Save Follow Up
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Delete Modal -->
    <div v-if="showDeleteModal" class="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
      <div class="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
        <div class="mt-3">
          <h3 class="text-lg leading-6 font-medium text-gray-900">
            Delete Lead - {{ deletingLead?.name }}
          </h3>
          <p class="text-sm text-gray-500">
            Are you sure you want to delete this lead?
          </p>
          <div class="flex justify-end space-x-3">
            <button
              type="button"
              @click="showDeleteModal = false"
              class="inline-flex justify-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
            >
              Cancel
            </button>
            <button
              type="button"
              @click="deleteLead"
              class="inline-flex justify-center rounded-md border border-transparent bg-red-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2"
            >
              Delete
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import type { Lead, LeadFormData, FollowUpFormData, LeadFilters } from '@/types/lead'
import leadsService from '@/services/leadsService'

// Reactive state
const leads = ref<Lead[]>([])
const loading = ref(false)
const error = ref('')
const showAddModal = ref(false)
const showEditModal = ref(false)
const showFollowUpModal = ref(false)
const showDeleteModal = ref(false)
const editingLead = ref<Lead | null>(null)
const followingLead = ref<Lead | null>(null)
const deletingLead = ref<Lead | null>(null)

// Form data
const form = ref<LeadFormData>({
  name: '',
  phone: '',
  email: '',
  status: 'New',
  source: 'Website',
  notes: ''
})

const followUpForm = ref<FollowUpFormData>({
  name: '',
  nextFollowup: '',
  status: 'Contacted',
  notes: ''
})

// Filters
const filters = ref<LeadFilters>({
  search: '',
  status: '',
  source: ''
})

// Computed properties
const filteredLeads = computed(() => {
  return leads.value.filter(lead => {
    const matchesSearch = !filters.value.search || 
      lead.name.toLowerCase().includes(filters.value.search.toLowerCase()) ||
      lead.phone.includes(filters.value.search) ||
      lead.email.toLowerCase().includes(filters.value.search.toLowerCase())
    
    const matchesStatus = !filters.value.status || lead.status === filters.value.status
    const matchesSource = !filters.value.source || lead.source === filters.value.source
    
    return matchesSearch && matchesStatus && matchesSource
  })
})

const stats = computed(() => {
  const total = leads.value.length
  const interested = leads.value.filter(l => l.status === 'Interested').length
  const newLeads = leads.value.filter(l => l.status === 'New').length
  const contacted = leads.value.filter(l => l.status === 'Contacted').length
  const converted = leads.value.filter(l => l.status === 'Converted').length
  
  return { total, interested, new: newLeads, contacted, converted }
})

// Watch for filter changes
watch(filters, () => {
  loadLeads()
}, { deep: true })

// Methods
const getStatusClass = (status: string) => {
  switch (status) {
    case 'New':
      return 'bg-gray-100 text-gray-800'
    case 'Contacted':
      return 'bg-blue-100 text-blue-800'
    case 'Interested':
      return 'bg-green-100 text-green-800'
    case 'Converted':
      return 'bg-purple-100 text-purple-800'
    case 'Lost':
      return 'bg-red-100 text-red-800'
    default:
      return 'bg-gray-100 text-gray-800'
  }
}

const formatDate = (dateString: string) => {
  if (!dateString) return 'N/A'
  const date = new Date(dateString)
  return date.toLocaleDateString('en-US', { 
    year: 'numeric', 
    month: 'short', 
    day: 'numeric' 
  })
}

const resetFilters = () => {
  filters.value = {
    search: '',
    status: '',
    source: ''
  }
}

const closeModal = () => {
  showAddModal.value = false
  showEditModal.value = false
  showFollowUpModal.value = false
  showDeleteModal.value = false
  editingLead.value = null
  followingLead.value = null
  deletingLead.value = null
  resetForm()
}

const resetForm = () => {
  form.value = {
    name: '',
    phone: '',
    email: '',
    status: 'New',
    source: 'Website',
    notes: ''
  }
  followUpForm.value = {
    name: '',
    nextFollowup: '',
    status: 'Contacted',
    notes: ''
  }
}

const openAddModal = () => {
  resetForm()
  showAddModal.value = true
}

const loadLeads = async () => {
  try {
    loading.value = true
    error.value = ''
    leads.value = await leadsService.getLeads(filters.value)
  } catch (err) {
    console.error('Error loading leads:', err)
    // Don't show error for empty data, only for actual API failures
    if (err instanceof Error && err.message.includes('404')) {
      // 404 is ok - just no data
      leads.value = []
    } else {
      error.value = 'Unable to connect to the server. Please check your connection and try again.'
    }
  } finally {
    loading.value = false
  }
}

const addLead = async () => {
  try {
    loading.value = true
    const newLead = await leadsService.createLead(form.value)
    leads.value.push(newLead)
    closeModal()
  } catch (err) {
    console.error('Error adding lead:', err)
    error.value = 'Failed to add lead. Please try again.'
  } finally {
    loading.value = false
  }
}

const editLead = (lead: Lead) => {
  editingLead.value = lead
  form.value = {
    name: lead.name,
    phone: lead.phone,
    email: lead.email,
    status: lead.status,
    source: lead.source,
    notes: lead.notes || ''
  }
  showEditModal.value = true
}

const updateLead = async () => {
  if (!editingLead.value) return
  
  try {
    loading.value = true
    const updatedLead = await leadsService.updateLead(editingLead.value.id, form.value)
    const index = leads.value.findIndex(l => l.id === editingLead.value!.id)
    if (index !== -1) {
      leads.value[index] = updatedLead
    }
    closeModal()
  } catch (err) {
    console.error('Error updating lead:', err)
    error.value = 'Failed to update lead. Please try again.'
  } finally {
    loading.value = false
  }
}

const confirmDelete = (lead: Lead) => {
  deletingLead.value = lead
  showDeleteModal.value = true
}

const deleteLead = async () => {
  if (!deletingLead.value) return
  
  try {
    loading.value = true
    await leadsService.deleteLead(deletingLead.value.id)
    leads.value = leads.value.filter(lead => lead.id !== deletingLead.value!.id)
    closeModal()
  } catch (err) {
    console.error('Error deleting lead:', err)
    error.value = 'Failed to delete lead. Please try again.'
  } finally {
    loading.value = false
  }
}

const followUpLead = (lead: Lead) => {
  followingLead.value = lead
  followUpForm.value = {
    name: lead.name,
    nextFollowup: new Date().toISOString().split('T')[0],
    status: lead.status,
    notes: ''
  }
  showFollowUpModal.value = true
}

const saveFollowUp = async () => {
  if (!followingLead.value) return
  
  try {
    loading.value = true
    const updatedLead = await leadsService.addFollowUp(followingLead.value.id, followUpForm.value)
    const index = leads.value.findIndex(l => l.id === followingLead.value!.id)
    if (index !== -1) {
      leads.value[index] = updatedLead
    }
    showFollowUpModal.value = false
    followingLead.value = null
    resetForm()
  } catch (err) {
    console.error('Error saving follow-up:', err)
    error.value = 'Failed to save follow-up. Please try again.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadLeads()
})
</script>

<style scoped>
/* Additional styles if needed */
</style>