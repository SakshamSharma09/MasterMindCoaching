<template>
  <div class="space-y-6">
    <!-- Header Actions -->
    <div class="flex items-center justify-between">
      <h2 class="text-lg font-semibold text-gray-900">Expenses Management</h2>
      <button
        @click="openAddExpenseModal"
        class="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg shadow-sm hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 transition-colors"
      >
        <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
        </svg>
        Add Expense
      </button>
    </div>

    <!-- Expense Filters -->
    <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Category</label>
          <select v-model="expenseFilters.category" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
            <option value="">All Categories</option>
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
          <label class="block text-sm font-medium text-gray-700 mb-1">Start Date</label>
          <input v-model="expenseFilters.startDate" type="date" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">End Date</label>
          <input v-model="expenseFilters.endDate" type="date" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
        </div>
        <div class="flex items-end">
          <button @click="expenseFilters = { category: '', startDate: '', endDate: '' }" class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors">
            Clear Filters
          </button>
        </div>
      </div>
    </div>

    <!-- Expenses Table -->
    <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="py-3 pl-6 pr-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Date</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Category</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Description</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Amount</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Paid To</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-200 bg-white">
            <tr v-for="expense in filteredExpenses" :key="expense.id" class="hover:bg-gray-50 transition-colors">
              <td class="whitespace-nowrap py-4 pl-6 pr-3 text-sm text-gray-900">{{ formatDate(expense.date) }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm">
                <span class="inline-flex rounded-full px-2.5 py-0.5 text-xs font-semibold bg-gray-100 text-gray-800">
                  {{ expense.category }}
                </span>
              </td>
              <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-700">{{ expense.description }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm font-medium text-gray-900">₹{{ formatCurrency(expense.amount) }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ expense.paidTo }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm">
                <button @click="editExpense(expense)" class="text-indigo-600 hover:text-indigo-900 mr-3 font-medium">Edit</button>
                <button @click="deleteExpense(expense.id)" class="text-red-600 hover:text-red-900 font-medium">Delete</button>
              </td>
            </tr>
            <tr v-if="filteredExpenses.length === 0">
              <td colspan="6" class="px-6 py-12 text-center text-sm text-gray-500">
                No expenses found matching your filters.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Add/Edit Expense Modal -->
    <div v-if="showExpenseModal" class="fixed inset-0 z-50 overflow-y-auto">
      <div class="flex items-center justify-center min-h-screen px-4">
        <div class="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" @click="closeExpenseModal"></div>
        <div class="relative bg-white rounded-xl shadow-xl max-w-lg w-full z-10">
          <form @submit.prevent="saveExpense">
            <div class="px-6 pt-6 pb-4">
              <h3 class="text-lg font-semibold text-gray-900 mb-4">
                {{ isEditingExpense ? 'Edit Expense' : 'Add New Expense' }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Category</label>
                  <select v-model="expenseForm.category" required class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
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
                  <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
                  <input v-model="expenseForm.description" type="text" required class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
                </div>
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">Amount</label>
                    <input v-model="expenseForm.amount" type="number" required min="0" step="0.01" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">Date</label>
                    <input v-model="expenseForm.date" type="date" required class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
                  </div>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Paid To</label>
                  <input v-model="expenseForm.paidTo" type="text" required class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Receipt/Invoice Number</label>
                  <input v-model="expenseForm.receiptNumber" type="text" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
                </div>
              </div>
            </div>
            <div class="bg-gray-50 px-6 py-4 flex justify-end gap-3 rounded-b-xl">
              <button type="button" @click="closeExpenseModal" class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
                Cancel
              </button>
              <button type="submit" :disabled="loading" class="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 disabled:opacity-50 transition-colors">
                {{ isEditingExpense ? 'Update' : 'Save' }}
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
import { financeService, type Expense, type CreateExpenseDto, type UpdateExpenseDto } from '@/services/financeService'
import { useToast } from '@/composables/useToast'

const toast = useToast()

const loading = ref(false)
const expenses = ref<Expense[]>([])
const showExpenseModal = ref(false)
const isEditingExpense = ref(false)

const expenseFilters = ref({ category: '', startDate: '', endDate: '' })

const expenseForm = ref({
  id: 0,
  category: '',
  description: '',
  amount: '',
  paidTo: '',
  date: '',
  receiptNumber: ''
})

const filteredExpenses = computed(() => {
  let filtered = expenses.value
  if (expenseFilters.value.category) {
    filtered = filtered.filter(e => e.category === expenseFilters.value.category)
  }
  if (expenseFilters.value.startDate) {
    filtered = filtered.filter(e => e.date >= expenseFilters.value.startDate)
  }
  if (expenseFilters.value.endDate) {
    filtered = filtered.filter(e => e.date <= expenseFilters.value.endDate)
  }
  return filtered
})

const formatCurrency = (amount: number): string => amount.toLocaleString('en-IN')
const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })
}

const loadExpenses = async () => {
  try {
    expenses.value = await financeService.getExpenses()
  } catch (error) {
    console.error('Error loading expenses:', error)
    expenses.value = []
  }
}

const openAddExpenseModal = () => {
  isEditingExpense.value = false
  expenseForm.value = { id: 0, category: '', description: '', amount: '', paidTo: '', date: '', receiptNumber: '' }
  showExpenseModal.value = true
}

const closeExpenseModal = () => { showExpenseModal.value = false }

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

const saveExpense = async () => {
  loading.value = true
  try {
    const expenseData: CreateExpenseDto = {
      category: expenseForm.value.category,
      description: expenseForm.value.description,
      amount: parseFloat(expenseForm.value.amount),
      paidTo: expenseForm.value.paidTo,
      date: expenseForm.value.date,
      receiptNumber: expenseForm.value.receiptNumber || undefined
    }

    if (isEditingExpense.value) {
      await financeService.updateExpense(expenseForm.value.id, expenseData as UpdateExpenseDto)
    } else {
      await financeService.createExpense(expenseData)
    }
    await loadExpenses()
    closeExpenseModal()
    toast.success(isEditingExpense.value ? 'Expense updated' : 'Expense added', 'Expense saved successfully.')
  } catch (error) {
    console.error('Error saving expense:', error)
    toast.error('Failed to save expense', 'Please try again.')
  } finally {
    loading.value = false
  }
}

const deleteExpense = async (expenseId: number) => {
  if (!confirm('Are you sure you want to delete this expense?')) return
  try {
    await financeService.deleteExpense(expenseId)
    await loadExpenses()
    toast.success('Expense deleted', 'Expense has been removed.')
  } catch (error) {
    console.error('Error deleting expense:', error)
    toast.error('Failed to delete expense', 'Please try again.')
  }
}

onMounted(() => { loadExpenses() })
</script>
