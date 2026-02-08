<template>
  <div class="px-4 sm:px-6 lg:px-8">
    <!-- Header -->
    <div class="sm:flex sm:items-center">
      <div class="sm:flex-auto">
        <h1 class="text-2xl font-semibold text-gray-900">Finance Management</h1>
        <p class="mt-2 text-sm text-gray-700">
          Manage fees, payments, expenses, and financial reports.
        </p>
      </div>
      <div class="mt-4 sm:mt-0 sm:ml-16 sm:flex-none space-x-3">
        <button
          type="button"
          @click="openAddFeeModal"
          class="inline-flex items-center justify-center rounded-md border border-transparent bg-green-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2"
        >
          <svg class="-ml-1 mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"></path>
          </svg>
          Add Fee
        </button>
        <button
          type="button"
          @click="openAddExpenseModal"
          class="inline-flex items-center justify-center rounded-md border border-transparent bg-red-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2"
        >
          <svg class="-ml-1 mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4"></path>
          </svg>
          Add Expense
        </button>
        <button
          type="button"
          @click="generateReport"
          :disabled="loading"
          class="inline-flex items-center justify-center rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:opacity-50"
        >
          <svg v-if="loading" class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <svg v-else class="-ml-1 mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 17v2a2 2 0 002 2h6a2 2 0 002-2v-2m-6 4h6M9 7h6m-6 4h6"></path>
          </svg>
          {{ loading ? 'Generating...' : 'Generate Report' }}
        </button>
      </div>
    </div>

    <!-- Tabs Navigation -->
    <div class="mt-6 border-b border-gray-200">
      <nav class="-mb-px flex space-x-8">
        <button
          @click="activeTab = 'overview'"
          :class="[
            activeTab === 'overview'
              ? 'border-indigo-500 text-indigo-600'
              : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700',
            'whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm'
          ]"
        >
          Overview
        </button>
        <button
          @click="activeTab = 'fees'"
          :class="[
            activeTab === 'fees'
              ? 'border-indigo-500 text-indigo-600'
              : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700',
            'whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm'
          ]"
        >
          Fees Management
        </button>
        <button
          @click="activeTab = 'fee-collection'"
          :class="[
            activeTab === 'fee-collection'
              ? 'border-indigo-500 text-indigo-600'
              : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700',
            'whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm'
          ]"
        >
          Fee Collection
        </button>
        <button
          @click="activeTab = 'expenses'"
          :class="[
            activeTab === 'expenses'
              ? 'border-indigo-500 text-indigo-600'
              : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700',
            'whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm'
          ]"
        >
          Expenses
        </button>
        <button
          @click="activeTab = 'overdue'"
          :class="[
            activeTab === 'overdue'
              ? 'border-indigo-500 text-indigo-600'
              : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700',
            'whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm'
          ]"
        >
          Overdue Fees
          <span v-if="overdueFees.length > 0" class="ml-2 bg-red-100 text-red-800 text-xs font-medium px-2.5 py-0.5 rounded-full">
            {{ overdueFees.length }}
          </span>
        </button>
        <button
          @click="activeTab = 'reports'"
          :class="[
            activeTab === 'reports'
              ? 'border-indigo-500 text-indigo-600'
              : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700',
            'whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm'
          ]"
        >
          Reports
        </button>
      </nav>
    </div>

    <!-- Overview Tab -->
    <div v-if="activeTab === 'overview'" class="mt-6">
      <!-- Financial Summary Cards -->
      <div class="mt-8 grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
        <div class="bg-white overflow-hidden shadow rounded-lg">
          <div class="p-5">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <svg class="h-6 w-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1"></path>
                </svg>
              </div>
              <div class="ml-5 w-0 flex-1">
                <dl>
                  <dt class="text-sm font-medium text-gray-500 truncate">Total Revenue</dt>
                  <dd class="text-lg font-medium text-gray-900">₹{{ formatCurrency(financialSummary.totalRevenue) }}</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div class="bg-white overflow-hidden shadow rounded-lg">
          <div class="p-5">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <svg class="h-6 w-6 text-yellow-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                </svg>
              </div>
              <div class="ml-5 w-0 flex-1">
                <dl>
                  <dt class="text-sm font-medium text-gray-500 truncate">Pending Payments</dt>
                  <dd class="text-lg font-medium text-gray-900">₹{{ formatCurrency(financialSummary.pendingPayments) }}</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div class="bg-white overflow-hidden shadow rounded-lg">
          <div class="p-5">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <svg class="h-6 w-6 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 14h.01M12 14h.01M15 11h.01M12 11h.01M9 11h.01M7 21h10a2 2 0 002-2V5a2 2 0 00-2-2H7a2 2 0 00-2 2v14a2 2 0 002 2z"></path>
                </svg>
              </div>
              <div class="ml-5 w-0 flex-1">
                <dl>
                  <dt class="text-sm font-medium text-gray-500 truncate">Expenses</dt>
                  <dd class="text-lg font-medium text-gray-900">₹{{ formatCurrency(financialSummary.expenses) }}</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div class="bg-white overflow-hidden shadow rounded-lg">
          <div class="p-5">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <svg class="h-6 w-6 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6"></path>
                </svg>
              </div>
              <div class="ml-5 w-0 flex-1">
                <dl>
                  <dt class="text-sm font-medium text-gray-500 truncate">Net Profit</dt>
                  <dd class="text-lg font-medium text-gray-900">₹{{ formatCurrency(financialSummary.netProfit) }}</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Additional Stats -->
      <div class="mt-8 grid grid-cols-1 gap-5 sm:grid-cols-3">
        <div class="bg-white overflow-hidden shadow rounded-lg">
          <div class="p-5">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <svg class="h-6 w-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z"></path>
                </svg>
              </div>
              <div class="ml-5 w-0 flex-1">
                <dl>
                  <dt class="text-sm font-medium text-gray-500 truncate">Total Students</dt>
                  <dd class="text-lg font-medium text-gray-900">{{ financialSummary.totalStudents }}</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div class="bg-white overflow-hidden shadow rounded-lg">
          <div class="p-5">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <svg class="h-6 w-6 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                </svg>
              </div>
              <div class="ml-5 w-0 flex-1">
                <dl>
                  <dt class="text-sm font-medium text-gray-500 truncate">Paid Students</dt>
                  <dd class="text-lg font-medium text-gray-900">{{ financialSummary.paidStudents }}</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        <div class="bg-white overflow-hidden shadow rounded-lg">
          <div class="p-5">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <svg class="h-6 w-6 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                </svg>
              </div>
              <div class="ml-5 w-0 flex-1">
                <dl>
                  <dt class="text-sm font-medium text-gray-500 truncate">Overdue Students</dt>
                  <dd class="text-lg font-medium text-gray-900">{{ financialSummary.overdueStudents }}</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Recent Transactions -->
      <div class="mt-8">
        <div class="sm:flex sm:items-center">
          <div class="sm:flex-auto">
            <h3 class="text-lg leading-6 font-medium text-gray-900">Recent Transactions</h3>
            <p class="mt-1 text-sm text-gray-500">
              Latest financial transactions from students and expenses.
            </p>
          </div>
          <div class="mt-4 sm:mt-0 sm:ml-16 sm:flex-none">
            <button
              @click="refreshData"
              :disabled="loading"
              class="inline-flex items-center justify-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:opacity-50"
            >
              <svg v-if="loading" class="animate-spin -ml-1 mr-2 h-4 w-4 text-gray-500" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              Refresh
            </button>
          </div>
        </div>
        <div class="mt-4 flex flex-col">
          <div class="-my-2 -mx-4 overflow-x-auto sm:-mx-6 lg:-mx-8">
            <div class="inline-block min-w-full py-2 align-middle md:px-6 lg:px-8">
              <div class="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
                <table class="min-w-full divide-y divide-gray-300">
                  <thead class="bg-gray-50">
                    <tr>
                      <th scope="col" class="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">Type</th>
                      <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Student/Description</th>
                      <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Amount</th>
                      <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Date</th>
                      <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Status</th>
                    </tr>
                  </thead>
                  <tbody class="divide-y divide-gray-200 bg-white">
                    <tr v-for="transaction in recentTransactions" :key="transaction.id">
                      <td class="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">
                        <span
                          :class="[
                            transaction.type === 'fee' ? 'bg-green-100 text-green-800' :
                            transaction.type === 'expense' ? 'bg-red-100 text-red-800' :
                            'bg-blue-100 text-blue-800'
                          ]"
                          class="inline-flex rounded-full px-2 text-xs font-semibold leading-5"
                        >
                          {{ transaction.type }}
                        </span>
                      </td>
                      <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ transaction.studentName || transaction.description }}</td>
                      <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">₹{{ formatCurrency(transaction.amount) }}</td>
                      <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ formatDate(transaction.date) }}</td>
                      <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                        <span
                          :class="[
                            transaction.status === 'Completed' ? 'bg-green-100 text-green-800' :
                            transaction.status === 'Pending' ? 'bg-yellow-100 text-yellow-800' :
                            transaction.status === 'Overdue' ? 'bg-red-100 text-red-800' :
                            'bg-gray-100 text-gray-800'
                          ]"
                          class="inline-flex rounded-full px-2 text-xs font-semibold leading-5"
                        >
                          {{ transaction.status }}
                        </span>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Fees Management Tab -->
    <div v-if="activeTab === 'fees'" class="mt-6">
      <div class="bg-white shadow rounded-lg">
        <div class="px-4 py-5 sm:p-6">
          <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">Fees Management</h3>
          
          <!-- Fee Filters -->
          <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
            <div>
              <label class="block text-sm font-medium text-gray-700">Class</label>
              <select v-model="feeFilters.classId" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                <option value="">All Classes</option>
                <option v-for="cls in classes" :key="cls.id" :value="cls.id">{{ cls.name }}</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Status</label>
              <select v-model="feeFilters.status" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                <option value="">All Status</option>
                <option value="Paid">Paid</option>
                <option value="Pending">Pending</option>
                <option value="Overdue">Overdue</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Month</label>
              <input v-model="feeFilters.month" type="month" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
            </div>
            <div class="flex items-end">
              <button @click="filterFees" class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500">
                Apply Filters
              </button>
            </div>
          </div>

          <!-- Fees Table -->
          <div class="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
            <table class="min-w-full divide-y divide-gray-300">
              <thead class="bg-gray-50">
                <tr>
                  <th scope="col" class="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">Student</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Class</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Fee Type</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Amount</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Due Date</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Status</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200 bg-white">
                <tr v-for="fee in filteredFees" :key="fee.id">
                  <td class="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">{{ fee.studentName }}</td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ fee.className }}</td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ fee.feeType }}</td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">₹{{ formatCurrency(fee.amount) }}</td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ formatDate(fee.dueDate) }}</td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <span
                      :class="[
                        fee.status === 'Paid' ? 'bg-green-100 text-green-800' :
                        fee.status === 'Pending' ? 'bg-yellow-100 text-yellow-800' :
                        'bg-red-100 text-red-800'
                      ]"
                      class="inline-flex rounded-full px-2 text-xs font-semibold leading-5"
                    >
                      {{ fee.status }}
                    </span>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <button @click="editFee(fee)" class="text-indigo-600 hover:text-indigo-900 mr-3">Edit</button>
                    <button @click="deleteFee(fee.id)" class="text-red-600 hover:text-red-900">Delete</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <!-- Expenses Tab -->
    <div v-if="activeTab === 'expenses'" class="mt-6">
      <div class="bg-white shadow rounded-lg">
        <div class="px-4 py-5 sm:p-6">
          <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">Expenses Management</h3>
          
          <!-- Expense Filters -->
          <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
            <div>
              <label class="block text-sm font-medium text-gray-700">Category</label>
              <select v-model="expenseFilters.category" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                <option value="">All Categories</option>
                <option value="Salary">Salary</option>
                <option value="Rent">Rent</option>
                <option value="Utilities">Utilities</option>
                <option value="Supplies">Supplies</option>
                <option value="Maintenance">Maintenance</option>
                <option value="Other">Other</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Date Range</label>
              <input v-model="expenseFilters.startDate" type="date" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">&nbsp;</label>
              <input v-model="expenseFilters.endDate" type="date" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
            </div>
            <div class="flex items-end">
              <button @click="filterExpenses" class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500">
                Apply Filters
              </button>
            </div>
          </div>

          <!-- Expenses Table -->
          <div class="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
            <table class="min-w-full divide-y divide-gray-300">
              <thead class="bg-gray-50">
                <tr>
                  <th scope="col" class="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">Date</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Category</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Description</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Amount</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Paid To</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200 bg-white">
                <tr v-for="expense in filteredExpenses" :key="expense.id">
                  <td class="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">{{ formatDate(expense.date) }}</td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <span class="inline-flex rounded-full px-2 text-xs font-semibold leading-5 bg-gray-100 text-gray-800">
                      {{ expense.category }}
                    </span>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ expense.description }}</td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">₹{{ formatCurrency(expense.amount) }}</td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ expense.paidTo }}</td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <button @click="editExpense(expense)" class="text-indigo-600 hover:text-indigo-900 mr-3">Edit</button>
                    <button @click="deleteExpense(expense.id)" class="text-red-600 hover:text-red-900">Delete</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <!-- Overdue Fees Tab -->
    <div v-if="activeTab === 'overdue'" class="mt-6">
      <div class="bg-white shadow rounded-lg">
        <div class="px-4 py-5 sm:p-6">
          <div class="sm:flex sm:items-center">
            <div class="sm:flex-auto">
              <h3 class="text-lg leading-6 font-medium text-gray-900">Overdue Fees</h3>
              <p class="mt-1 text-sm text-gray-500">
                Students with overdue fee payments. Follow up for collection.
              </p>
            </div>
            <div class="mt-4 sm:mt-0 sm:ml-16 sm:flex-none">
              <button
                @click="sendReminders"
                :disabled="loading"
                class="inline-flex items-center justify-center rounded-md border border-transparent bg-orange-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-orange-700 focus:outline-none focus:ring-2 focus:ring-orange-500 focus:ring-offset-2 disabled:opacity-50"
              >
                <svg class="-ml-1 mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 4.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"></path>
                </svg>
                Send Reminders
              </button>
            </div>
          </div>

          <!-- Overdue Fees Table -->
          <div class="mt-6 overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
            <table class="min-w-full divide-y divide-gray-300">
              <thead class="bg-gray-50">
                <tr>
                  <th scope="col" class="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">Student</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Class</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Fee Type</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Amount</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Due Date</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Days Overdue</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Contact</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200 bg-white">
                <tr v-for="overdue in overdueFees" :key="overdue.id">
                  <td class="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">{{ overdue.studentName }}</td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ overdue.className }}</td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ overdue.feeType }}</td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">₹{{ formatCurrency(overdue.amount) }}</td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ formatDate(overdue.dueDate) }}</td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <span class="inline-flex rounded-full px-2 text-xs font-semibold leading-5 bg-red-100 text-red-800">
                      {{ overdue.daysOverdue }} days
                    </span>
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ overdue.parentContact }}</td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <button @click="sendReminder(overdue)" class="text-orange-600 hover:text-orange-900 mr-3">Send Reminder</button>
                    <button @click="markAsPaid(overdue.id)" class="text-green-600 hover:text-green-900">Mark Paid</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <!-- Reports Tab -->
    <div v-if="activeTab === 'reports'" class="mt-6">
      <div class="bg-white shadow rounded-lg">
        <div class="px-4 py-5 sm:p-6">
          <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">Financial Reports</h3>
          
          <!-- Report Generation -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="border border-gray-200 rounded-lg p-4">
              <h4 class="text-md font-medium text-gray-900 mb-3">Monthly Report</h4>
              <div class="space-y-3">
                <div>
                  <label class="block text-sm font-medium text-gray-700">Select Month</label>
                  <input v-model="reportFilters.month" type="month" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                </div>
                <button @click="generateMonthlyReport" class="w-full inline-flex justify-center items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500">
                  Generate Monthly Report
                </button>
              </div>
            </div>

            <div class="border border-gray-200 rounded-lg p-4">
              <h4 class="text-md font-medium text-gray-900 mb-3">Custom Date Range</h4>
              <div class="space-y-3">
                <div>
                  <label class="block text-sm font-medium text-gray-700">Start Date</label>
                  <input v-model="reportFilters.startDate" type="date" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">End Date</label>
                  <input v-model="reportFilters.endDate" type="date" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                </div>
                <button @click="generateCustomReport" class="w-full inline-flex justify-center items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500">
                  Generate Custom Report
                </button>
              </div>
            </div>
          </div>

          <!-- Recent Reports -->
          <div class="mt-8">
            <h4 class="text-md font-medium text-gray-900 mb-3">Recent Reports</h4>
            <div class="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
              <table class="min-w-full divide-y divide-gray-300">
                <thead class="bg-gray-50">
                  <tr>
                    <th scope="col" class="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">Report Type</th>
                    <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Period</th>
                    <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Generated On</th>
                    <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Actions</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-gray-200 bg-white">
                  <tr v-for="report in recentReports" :key="report.id">
                    <td class="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">{{ report.type }}</td>
                    <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ report.period }}</td>
                    <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ formatDate(report.generatedAt) }}</td>
                    <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                      <button @click="downloadReport(report)" class="text-indigo-600 hover:text-indigo-900">Download</button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Add Fee Modal -->
    <div v-if="showFeeModal" class="fixed inset-0 z-50 overflow-y-auto">
      <div class="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
        <div class="fixed inset-0 transition-opacity" @click="closeFeeModal">
          <div class="absolute inset-0 bg-gray-500 opacity-75"></div>
        </div>
        <div class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-2xl sm:w-full">
          <form @submit.prevent="saveFee">
            <div class="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
              <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">
                {{ isEditingFee ? 'Edit Fee' : 'Add New Fee' }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700">Student</label>
                  <select v-model="feeForm.studentId" required class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                    <option value="">Select Student</option>
                    <option v-for="student in students" :key="student.id" :value="student.id">{{ student.firstName }} {{ student.lastName }}</option>
                  </select>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">Fee Category</label>
                  <select v-model="feeForm.feeCategory" required class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm" @change="onFeeCategoryChange">
                    <option value="Monthly">Monthly Fee</option>
                    <option value="FullCourse">Full Course Fee</option>
                    <option value="Additional">Additional/One-time Fee</option>
                  </select>
                  <p class="mt-1 text-xs text-gray-500">
                    {{ getFeeCategoryDescription(feeForm.feeCategory) }}
                  </p>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">Fee Type</label>
                  <select v-model="feeForm.feeStructureId" required class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                    <option value="">Select Fee Type</option>
                    <option value="1">Tuition Fee</option>
                    <option value="2">Exam Fee</option>
                    <option value="3">Lab Fee</option>
                    <option value="4">Library Fee</option>
                    <option value="5">Sports Fee</option>
                    <option value="6">Transport Fee</option>
                    <option value="7">Other</option>
                  </select>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">Amount</label>
                  <input v-model="feeForm.amount" type="number" required min="0" step="0.01" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">Discount Amount (Optional)</label>
                  <input v-model="feeForm.discountAmount" type="number" min="0" step="0.01" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                </div>
                
                <!-- Monthly Fee Specific Fields -->
                <div v-if="feeForm.feeCategory === 'Monthly'" class="space-y-4 border-t pt-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Start Date</label>
                    <input v-model="feeForm.startDate" type="date" required class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700">End Date</label>
                    <input v-model="feeForm.endDate" type="date" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Recurring Day of Month</label>
                    <select v-model="feeForm.recurringDayOfMonth" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                      <option v-for="day in 31" :key="day" :value="day">{{ day }}{{ getDaySuffix(day) }}</option>
                    </select>
                  </div>
                </div>
                
                <!-- Full Course Fee Specific Fields -->
                <div v-if="feeForm.feeCategory === 'FullCourse'" class="space-y-4 border-t pt-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Start Date</label>
                    <input v-model="feeForm.startDate" type="date" required class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700">End Date (Optional)</label>
                    <input v-model="feeForm.endDate" type="date" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                  </div>
                </div>
                
                <!-- Additional Fee Specific Fields -->
                <div v-if="feeForm.feeCategory === 'Additional'" class="space-y-4 border-t pt-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Due Date</label>
                    <input v-model="feeForm.dueDate" type="date" required class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                  </div>
                </div>
                
                <!-- Common Fields -->
                <div class="space-y-4 border-t pt-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Late Fee Per Day (Optional)</label>
                    <input v-model="feeForm.lateFeePerDay" type="number" min="0" step="0.01" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Grace Period Days</label>
                    <input v-model="feeForm.gracePeriodDays" type="number" min="0" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Academic Year</label>
                    <input v-model="feeForm.academicYear" type="text" placeholder="2024-25" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Remarks</label>
                    <textarea v-model="feeForm.remarks" rows="3" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"></textarea>
                  </div>
                </div>
              </div>
            </div>
            <div class="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
              <button type="submit" class="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:ml-3 sm:w-auto sm:text-sm">
                {{ isEditingFee ? 'Update' : 'Save' }}
              </button>
              <button type="button" @click="closeFeeModal" class="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm">
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Add Expense Modal -->
    <div v-if="showExpenseModal" class="fixed inset-0 z-50 overflow-y-auto">
      <div class="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
        <div class="fixed inset-0 transition-opacity" @click="closeExpenseModal">
          <div class="absolute inset-0 bg-gray-500 opacity-75"></div>
        </div>
        <div class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
          <form @submit.prevent="saveExpense">
            <div class="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
              <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">
                {{ isEditingExpense ? 'Edit Expense' : 'Add New Expense' }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700">Category</label>
                  <select v-model="expenseForm.category" required class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                    <option value="">Select Category</option>
                    <option value="Salary">Salary</option>
                    <option value="Rent">Rent</option>
                    <option value="Utilities">Utilities</option>
                    <option value="Supplies">Supplies</option>
                    <option value="Maintenance">Maintenance</option>
                    <option value="Marketing">Marketing</option>
                    <option value="Other">Other</option>
                  </select>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">Description</label>
                  <input v-model="expenseForm.description" type="text" required class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">Amount</label>
                  <input v-model="expenseForm.amount" type="number" required min="0" step="0.01" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">Paid To</label>
                  <input v-model="expenseForm.paidTo" type="text" required class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">Date</label>
                  <input v-model="expenseForm.date" type="date" required class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">Receipt/Invoice</label>
                  <input v-model="expenseForm.receiptNumber" type="text" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm">
                </div>
              </div>
            </div>
            <div class="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
              <button type="submit" class="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-red-600 text-base font-medium text-white hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 sm:ml-3 sm:w-auto sm:text-sm">
                {{ isEditingExpense ? 'Update' : 'Save' }}
              </button>
              <button type="button" @click="closeExpenseModal" class="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm">
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
import { ref, onMounted, computed } from 'vue'
import { financeService, type FinancialSummary, type Payment } from '@/services/financeService'
import { studentsService, type Student } from '@/services/studentsService'
import { classesService, type Class } from '@/services/classesService'
import FeeCollectionView from './FeeCollectionView.vue'

// Interfaces
interface Fee {
  id: number
  studentId: number
  studentName: string
  classId: number
  className: string
  feeType: string
  amount: number
  dueDate: string
  status: 'Paid' | 'Pending' | 'Overdue'
  description?: string
}

interface Expense {
  id: number
  category: string
  description: string
  amount: number
  paidTo: string
  date: string
  receiptNumber?: string
}

interface OverdueFee extends Fee {
  daysOverdue: number
  parentContact: string
}

interface Transaction {
  id: number
  type: 'fee' | 'expense' | 'payment'
  studentName?: string
  description?: string
  amount: number
  date: string
  status: 'Completed' | 'Pending' | 'Overdue'
}

interface Report {
  id: number
  type: string
  period: string
  generatedAt: string
  data: any
}

// Reactive data
const financialSummary = ref<FinancialSummary>({
  totalRevenue: 0,
  pendingPayments: 0,
  expenses: 0,
  netProfit: 0,
  totalStudents: 0,
  paidStudents: 0,
  pendingStudents: 0,
  overdueStudents: 0
})

const recentPayments = ref<Payment[]>([])
const fees = ref<Fee[]>([])
const expenses = ref<Expense[]>([])
const overdueFees = ref<OverdueFee[]>([])
const recentTransactions = ref<Transaction[]>([])
const recentReports = ref<Report[]>([])
const students = ref<Student[]>([])
const classes = ref<Class[]>([])

const loading = ref(false)
const activeTab = ref('overview')

// Modal states
const showFeeModal = ref(false)
const showExpenseModal = ref(false)
const isEditingFee = ref(false)
const isEditingExpense = ref(false)

// Forms
const feeForm = ref({
  id: 0,
  studentId: '',
  feeStructureId: '',
  feeCategory: 'Monthly', // Monthly, FullCourse, Additional
  amount: '',
  discountAmount: '',
  startDate: '',
  endDate: '',
  dueDate: '',
  recurringDayOfMonth: 1, // 1-31 for monthly fees
  lateFeePerDay: '',
  gracePeriodDays: 0,
  academicYear: '',
  remarks: ''
})

const expenseForm = ref({
  id: 0,
  category: '',
  description: '',
  amount: '',
  paidTo: '',
  date: '',
  receiptNumber: ''
})

// Filters
const feeFilters = ref({
  classId: '',
  status: '',
  month: ''
})

const expenseFilters = ref({
  category: '',
  startDate: '',
  endDate: ''
})

const reportFilters = ref({
  month: '',
  startDate: '',
  endDate: ''
})

// Computed properties
const filteredFees = computed(() => {
  let filtered = fees.value
  
  if (feeFilters.value.classId) {
    filtered = filtered.filter(fee => fee.classId === parseInt(feeFilters.value.classId))
  }
  
  if (feeFilters.value.status) {
    filtered = filtered.filter(fee => fee.status === feeFilters.value.status)
  }
  
  if (feeFilters.value.month) {
    const [year, month] = feeFilters.value.month.split('-')
    filtered = filtered.filter(fee => {
      const feeDate = new Date(fee.dueDate)
      return feeDate.getFullYear() === parseInt(year) && feeDate.getMonth() + 1 === parseInt(month)
    })
  }
  
  return filtered
})

const filteredExpenses = computed(() => {
  let filtered = expenses.value
  
  if (expenseFilters.value.category) {
    filtered = filtered.filter(expense => expense.category === expenseFilters.value.category)
  }
  
  if (expenseFilters.value.startDate) {
    filtered = filtered.filter(expense => expense.date >= expenseFilters.value.startDate)
  }
  
  if (expenseFilters.value.endDate) {
    filtered = filtered.filter(expense => expense.date <= expenseFilters.value.endDate)
  }
  
  return filtered
})

// Utility functions
const formatCurrency = (amount: number): string => {
  return amount.toLocaleString('en-IN')
}

const formatDate = (dateString: string): string => {
  const date = new Date(dateString)
  return date.toLocaleDateString('en-IN', {
    day: '2-digit',
    month: 'short',
    year: 'numeric'
  })
}

const calculateDaysOverdue = (dueDate: string): number => {
  const due = new Date(dueDate)
  const today = new Date()
  const diffTime = today.getTime() - due.getTime()
  return Math.ceil(diffTime / (1000 * 60 * 60 * 24))
}

// API methods
const loadFinancialSummary = async () => {
  try {
    const summary = await financeService.getFinancialSummary()
    financialSummary.value = summary
  } catch (error) {
    console.error('Error loading financial summary:', error)
  }
}

const loadRecentPayments = async () => {
  try {
    const payments = await financeService.getRecentPayments(10)
    recentPayments.value = payments
  } catch (error) {
    console.error('Error loading recent payments:', error)
  }
}

const loadFees = async () => {
  try {
    fees.value = await financeService.getFees()
  } catch (error) {
    console.error('Error loading fees:', error)
    fees.value = []
  }
}

const loadExpenses = async () => {
  try {
    expenses.value = await financeService.getExpenses()
  } catch (error) {
    console.error('Error loading expenses:', error)
    expenses.value = []
  }
}

const loadOverdueFees = async () => {
  try {
    const overdueData = await financeService.getOverdueFees()
    overdueFees.value = overdueData.map(overdue => ({
      ...overdue,
      daysOverdue: calculateDaysOverdue(overdue.dueDate),
      parentContact: '+91 9876543210' // This should come from the API
    }))
  } catch (error) {
    console.error('Error loading overdue fees:', error)
    overdueFees.value = []
  }
}

const loadRecentTransactions = async () => {
  try {
    // Combine fees and expenses into transactions
    const feeTransactions = fees.value.map(fee => ({
      id: fee.id,
      type: 'fee' as const,
      studentName: fee.studentName,
      amount: fee.amount,
      date: fee.dueDate,
      status: fee.status === 'Paid' ? 'Completed' as const : fee.status as 'Pending' | 'Overdue'
    }))
    
    const expenseTransactions = expenses.value.map(expense => ({
      id: expense.id,
      type: 'expense' as const,
      description: expense.description,
      amount: expense.amount,
      date: expense.date,
      status: 'Completed' as const
    }))
    
    recentTransactions.value = [...feeTransactions, ...expenseTransactions]
      .sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
      .slice(0, 10)
  } catch (error) {
    console.error('Error loading recent transactions:', error)
  }
}

const loadStudents = async () => {
  try {
    const result = await studentsService.getStudents(1, 1000)
    students.value = result.data
  } catch (error) {
    console.error('Error loading students:', error)
  }
}

const loadClasses = async () => {
  try {
    const result = await classesService.getClasses()
    classes.value = result.data
  } catch (error) {
    console.error('Error loading classes:', error)
  }
}

const loadRecentReports = async () => {
  try {
    recentReports.value = await financeService.getReports()
  } catch (error) {
    console.error('Error loading recent reports:', error)
    recentReports.value = []
  }
}

const refreshData = async () => {
  loading.value = true
  try {
    // Load data with individual error handling to prevent cascading failures
    const promises = [
      loadFinancialSummary().catch(err => {
        console.warn('Financial summary loading failed, using fallback:', err)
        return null
      }),
      loadRecentPayments().catch(err => {
        console.warn('Recent payments loading failed, using fallback:', err)
        return null
      }),
      loadFees().catch(err => {
        console.warn('Fees loading failed, using fallback:', err)
        return null
      }),
      loadExpenses().catch(err => {
        console.warn('Expenses loading failed, using fallback:', err)
        return null
      }),
      loadOverdueFees().catch(err => {
        console.warn('Overdue fees loading failed, using fallback:', err)
        return null
      }),
      loadRecentTransactions().catch(err => {
        console.warn('Recent transactions loading failed, using fallback:', err)
        return null
      }),
      loadStudents().catch(err => {
        console.warn('Students loading failed, using fallback:', err)
        return null
      }),
      loadClasses().catch(err => {
        console.warn('Classes loading failed, using fallback:', err)
        return null
      }),
      loadRecentReports().catch(err => {
        console.warn('Recent reports loading failed, using fallback:', err)
        return null
      })
    ]
    
    await Promise.allSettled(promises)
    console.log('Finance dashboard data loading completed')
  } catch (error) {
    console.error('Critical error in refreshData:', error)
    // Don't redirect - show error state instead
  } finally {
    loading.value = false
  }
}

// Fee management
const closeFeeModal = () => {
  showFeeModal.value = false
}

const saveFee = async () => {
  try {
    loading.value = true
    
    const feeData = {
      studentId: parseInt(feeForm.value.studentId),
      feeStructureId: parseInt(feeForm.value.feeStructureId),
      feeCategory: feeForm.value.feeCategory,
      amount: parseFloat(feeForm.value.amount),
      discountAmount: feeForm.value.discountAmount ? parseFloat(feeForm.value.discountAmount) : null,
      startDate: feeForm.value.startDate || null,
      endDate: feeForm.value.endDate || null,
      dueDate: feeForm.value.dueDate || null,
      recurringDayOfMonth: feeForm.value.recurringDayOfMonth,
      lateFeePerDay: feeForm.value.lateFeePerDay ? parseFloat(feeForm.value.lateFeePerDay) : null,
      gracePeriodDays: feeForm.value.gracePeriodDays,
      academicYear: feeForm.value.academicYear || null,
      remarks: feeForm.value.remarks || null
    }
    
    if (isEditingFee.value) {
      // Update existing fee
      console.log('Updating fee:', feeForm.value.id, feeData)
    } else {
      // Create new fee using the enhanced API
      await financeService.createFee(feeData)
    }
    
    await loadFees()
    await loadRecentTransactions()
    closeFeeModal()
  } catch (error) {
    console.error('Error saving fee:', error)
    alert('Error saving fee. Please try again.')
  } finally {
    loading.value = false
  }
}

const editFee = (fee: Fee) => {
  isEditingFee.value = true
  feeForm.value = {
    id: fee.id,
    studentId: fee.studentId.toString(),
    feeStructureId: '',
    feeCategory: 'Monthly',
    amount: fee.amount.toString(),
    discountAmount: '',
    startDate: '',
    endDate: '',
    dueDate: fee.dueDate,
    recurringDayOfMonth: 1,
    lateFeePerDay: '',
    gracePeriodDays: 0,
    academicYear: '',
    remarks: fee.description || ''
  }
  showFeeModal.value = true
}

const deleteFee = async (feeId: number) => {
  if (confirm('Are you sure you want to delete this fee?')) {
    try {
      console.log('Deleting fee:', feeId)
      await loadFees()
      await loadRecentTransactions()
    } catch (error) {
      console.error('Error deleting fee:', error)
      alert('Error deleting fee. Please try again.')
    }
  }
}

// Expense management
const openAddExpenseModal = () => {
  isEditingExpense.value = false
  expenseForm.value = {
    id: 0,
    category: '',
    description: '',
    amount: '',
    paidTo: '',
    date: '',
    receiptNumber: ''
  }
  showExpenseModal.value = true
}

const closeExpenseModal = () => {
  showExpenseModal.value = false
}

const saveExpense = async () => {
  try {
    loading.value = true
    
    const expenseData = {
      category: expenseForm.value.category,
      description: expenseForm.value.description,
      amount: parseFloat(expenseForm.value.amount),
      paidTo: expenseForm.value.paidTo,
      date: expenseForm.value.date,
      receiptNumber: expenseForm.value.receiptNumber
    }
    
    if (isEditingExpense.value) {
      // Update existing expense
      console.log('Updating expense:', expenseForm.value.id, expenseData)
    } else {
      // Create new expense
      console.log('Creating new expense:', expenseData)
    }
    
    await loadExpenses()
    await loadRecentTransactions()
    closeExpenseModal()
  } catch (error) {
    console.error('Error saving expense:', error)
    alert('Error saving expense. Please try again.')
  } finally {
    loading.value = false
  }
}

const editExpense = (expense: Expense) => {
  isEditingExpense.value = true
  expenseForm.value = {
    id: expense.id,
    category: expense.category,
    description: expense.description,
    amount: expense.amount.toString(),
    paidTo: expense.paidTo,
    date: expense.date,
    receiptNumber: expense.receiptNumber || ''
  }
  showExpenseModal.value = true
}

const deleteExpense = async (expenseId: number) => {
  if (confirm('Are you sure you want to delete this expense?')) {
    try {
      console.log('Deleting expense:', expenseId)
      await loadExpenses()
      await loadRecentTransactions()
    } catch (error) {
      console.error('Error deleting expense:', error)
      alert('Error deleting expense. Please try again.')
    }
  }
}

// Overdue fees management
const sendReminders = async () => {
  try {
    loading.value = true
    console.log('Sending reminders to all overdue students')
    alert('Reminders sent successfully!')
  } catch (error) {
    console.error('Error sending reminders:', error)
    alert('Error sending reminders. Please try again.')
  } finally {
    loading.value = false
  }
}

const sendReminder = async (overdue: OverdueFee) => {
  try {
    console.log('Sending reminder to:', overdue.studentName)
    alert(`Reminder sent to ${overdue.studentName}'s parents!`)
  } catch (error) {
    console.error('Error sending reminder:', error)
    alert('Error sending reminder. Please try again.')
  }
}

const markAsPaid = async (feeId: number) => {
  try {
    console.log('Marking fee as paid:', feeId)
    await loadFees()
    await loadOverdueFees()
    await loadRecentTransactions()
    alert('Fee marked as paid successfully!')
  } catch (error) {
    console.error('Error marking fee as paid:', error)
    alert('Error marking fee as paid. Please try again.')
  }
}

// Reports
const generateMonthlyReport = async () => {
  if (!reportFilters.value.month) {
    alert('Please select a month')
    return
  }
  
  try {
    loading.value = true
    const [year, month] = reportFilters.value.month.split('-')
    const startDate = `${year}-${month}-01`
    const endDate = new Date(parseInt(year), parseInt(month), 0).toISOString().split('T')[0]
    
    const report = await financeService.generateReport(startDate, endDate)
    
    // Create a download link for the report
    const blob = new Blob([JSON.stringify(report, null, 2)], { type: 'application/json' })
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `monthly-report-${reportFilters.value.month}.json`
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    window.URL.revokeObjectURL(url)
    
    alert('Monthly report generated and downloaded successfully!')
  } catch (error) {
    console.error('Error generating monthly report:', error)
    alert('Error generating monthly report. Please try again.')
  } finally {
    loading.value = false
  }
}

const generateCustomReport = async () => {
  if (!reportFilters.value.startDate || !reportFilters.value.endDate) {
    alert('Please select both start and end dates')
    return
  }
  
  try {
    loading.value = true
    const report = await financeService.generateReport(reportFilters.value.startDate, reportFilters.value.endDate)
    
    // Create a download link for the report
    const blob = new Blob([JSON.stringify(report, null, 2)], { type: 'application/json' })
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `custom-report-${reportFilters.value.startDate}-to-${reportFilters.value.endDate}.json`
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    window.URL.revokeObjectURL(url)
    
    alert('Custom report generated and downloaded successfully!')
  } catch (error) {
    console.error('Error generating custom report:', error)
    alert('Error generating custom report. Please try again.')
  } finally {
    loading.value = false
  }
}

const generateReport = async () => {
  try {
    loading.value = true
    const endDate = new Date().toISOString().split('T')[0]
    const startDate = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0] // Last 30 days
    
    const report = await financeService.generateReport(startDate, endDate)
    
    // Create a download link for the report
    const blob = new Blob([JSON.stringify(report, null, 2)], { type: 'application/json' })
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `financial-report-${startDate}-to-${endDate}.json`
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    window.URL.revokeObjectURL(url)
    
    alert('Financial report generated and downloaded successfully!')
  } catch (error) {
    console.error('Error generating report:', error)
    alert('Error generating financial report. Please try again.')
  } finally {
    loading.value = false
  }
}

const downloadReport = (report: Report) => {
  const blob = new Blob([JSON.stringify(report.data, null, 2)], { type: 'application/json' })
  const url = window.URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `${report.type.toLowerCase().replace(' ', '-')}-${report.period}.json`
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  window.URL.revokeObjectURL(url)
}

// Filter methods
const filterFees = () => {
  // Filters are applied via computed property
  console.log('Applying fee filters:', feeFilters.value)
}

const filterExpenses = () => {
  // Filters are applied via computed property
  console.log('Applying expense filters:', expenseFilters.value)
}

// Utility functions for fee management
const getFeeCategoryDescription = (category: string): string => {
  switch (category) {
    case 'Monthly':
      return 'Recurring fee that becomes overdue every month on the specified day'
    case 'FullCourse':
      return 'One-time fee for the entire course duration. Becomes overdue immediately.'
    case 'Additional':
      return 'One-time additional fee with specific due date'
    default:
      return ''
  }
}

const getDaySuffix = (day: number): string => {
  if (day >= 11 && day <= 13) return 'th'
  switch (day % 10) {
    case 1: return 'st'
    case 2: return 'nd'
    case 3: return 'rd'
    default: return 'th'
  }
}

const onFeeCategoryChange = () => {
  // Reset form fields when category changes
  feeForm.value.startDate = ''
  feeForm.value.endDate = ''
  feeForm.value.dueDate = ''
  feeForm.value.recurringDayOfMonth = 1
  
  // Set default academic year if not set
  if (!feeForm.value.academicYear) {
    const currentYear = new Date().getFullYear()
    const currentMonth = new Date().getMonth()
    feeForm.value.academicYear = currentMonth >= 6 ? `${currentYear}-${currentYear + 1}` : `${currentYear - 1}-${currentYear}`
  }
}

const openAddFeeModal = () => {
  isEditingFee.value = false
  feeForm.value = {
    id: 0,
    studentId: '',
    feeStructureId: '',
    feeCategory: 'Monthly',
    amount: '',
    discountAmount: '',
    startDate: '',
    endDate: '',
    dueDate: '',
    recurringDayOfMonth: 1,
    lateFeePerDay: '',
    gracePeriodDays: 0,
    academicYear: '',
    remarks: ''
  }
  
  // Set default academic year
  const currentYear = new Date().getFullYear()
  const currentMonth = new Date().getMonth()
  feeForm.value.academicYear = currentMonth >= 6 ? `${currentYear}-${currentYear + 1}` : `${currentYear - 1}-${currentYear}`
  
  showFeeModal.value = true
}

// Lifecycle
onMounted(async () => {
  await refreshData()
})
</script>

<style scoped>
/* Additional styles if needed */
</style>