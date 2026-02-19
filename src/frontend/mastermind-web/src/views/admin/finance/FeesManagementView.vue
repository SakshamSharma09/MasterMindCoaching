<template>
  <div class="space-y-6">
    <!-- Header Actions -->
    <div class="flex items-center justify-between">
      <h2 class="text-lg font-semibold text-gray-900">Fees Management</h2>
      <button
        @click="openAddFeeModal"
        class="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-green-600 rounded-lg shadow-sm hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2 transition-colors"
      >
        <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
        </svg>
        Add Fee
      </button>
    </div>

    <!-- Fee Filters -->
    <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Class</label>
          <select v-model="feeFilters.classId" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
            <option value="">All Classes</option>
            <option v-for="cls in classes" :key="cls.id" :value="cls.id">{{ cls.name }}</option>
          </select>
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Status</label>
          <select v-model="feeFilters.status" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
            <option value="">All Status</option>
            <option value="Paid">Paid</option>
            <option value="Pending">Pending</option>
            <option value="Overdue">Overdue</option>
          </select>
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Month</label>
          <input v-model="feeFilters.month" type="month" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
        </div>
        <div class="flex items-end">
          <button @click="feeFilters = { classId: '', status: '', month: '' }" class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors">
            Clear Filters
          </button>
        </div>
      </div>
    </div>

    <!-- Fees Table -->
    <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="py-3 pl-6 pr-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Student</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Class</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Fee Type</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Amount</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Due Date</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Status</th>
              <th class="px-3 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-200 bg-white">
            <tr v-for="fee in filteredFees" :key="fee.id" class="hover:bg-gray-50 transition-colors">
              <td class="whitespace-nowrap py-4 pl-6 pr-3 text-sm font-medium text-gray-900">{{ fee.studentName }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ fee.className }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ fee.feeType }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm font-medium text-gray-900">₹{{ formatCurrency(fee.amount) }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{{ formatDate(fee.dueDate) }}</td>
              <td class="whitespace-nowrap px-3 py-4 text-sm">
                <span
                  :class="[
                    fee.status === 'Paid' ? 'bg-green-100 text-green-800' :
                    fee.status === 'Pending' ? 'bg-yellow-100 text-yellow-800' :
                    'bg-red-100 text-red-800'
                  ]"
                  class="inline-flex rounded-full px-2.5 py-0.5 text-xs font-semibold"
                >
                  {{ fee.status }}
                </span>
              </td>
              <td class="whitespace-nowrap px-3 py-4 text-sm">
                <button @click="editFee(fee)" class="text-indigo-600 hover:text-indigo-900 mr-3 font-medium">Edit</button>
                <button @click="deleteFee(fee.id)" class="text-red-600 hover:text-red-900 font-medium">Delete</button>
              </td>
            </tr>
            <tr v-if="filteredFees.length === 0">
              <td colspan="7" class="px-6 py-12 text-center text-sm text-gray-500">
                No fees found matching your filters.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Add/Edit Fee Modal -->
    <div v-if="showFeeModal" class="fixed inset-0 z-50 overflow-y-auto">
      <div class="flex items-center justify-center min-h-screen px-4">
        <div class="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" @click="closeFeeModal"></div>
        <div class="relative bg-white rounded-xl shadow-xl max-w-2xl w-full z-10">
          <form @submit.prevent="saveFee">
            <div class="px-6 pt-6 pb-4">
              <h3 class="text-lg font-semibold text-gray-900 mb-4">
                {{ isEditingFee ? 'Edit Fee' : 'Add New Fee' }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Student</label>
                  <select v-model="feeForm.studentId" required class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
                    <option value="">Select Student</option>
                    <option v-for="student in students" :key="student.id" :value="student.id">{{ student.firstName }} {{ student.lastName }}</option>
                  </select>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Fee Category</label>
                  <select v-model="feeForm.feeCategory" required class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
                    <option value="Monthly">Monthly Fee</option>
                    <option value="FullCourse">Full Course Fee</option>
                    <option value="Additional">Additional/One-time Fee</option>
                  </select>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Fee Type</label>
                  <select v-model="feeForm.feeStructureId" required class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
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
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">Amount</label>
                    <input v-model="feeForm.amount" type="number" required min="0" step="0.01" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">Discount Amount</label>
                    <input v-model="feeForm.discountAmount" type="number" min="0" step="0.01" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
                  </div>
                </div>
                <div v-if="feeForm.feeCategory === 'Monthly'" class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">Start Date</label>
                    <input v-model="feeForm.startDate" type="date" required class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">End Date</label>
                    <input v-model="feeForm.endDate" type="date" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
                  </div>
                </div>
                <div v-if="feeForm.feeCategory === 'Additional' || feeForm.feeCategory === 'FullCourse'">
                  <label class="block text-sm font-medium text-gray-700 mb-1">Due Date</label>
                  <input v-model="feeForm.dueDate" type="date" required class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Remarks</label>
                  <textarea v-model="feeForm.remarks" rows="2" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm"></textarea>
                </div>
              </div>
            </div>
            <div class="bg-gray-50 px-6 py-4 flex justify-end gap-3 rounded-b-xl">
              <button type="button" @click="closeFeeModal" class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
                Cancel
              </button>
              <button type="submit" :disabled="loading" class="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition-colors">
                {{ isEditingFee ? 'Update' : 'Save' }}
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
import { financeService, type Fee, type UpdateFeeRequest } from '@/services/financeService'
import { studentsService } from '@/services/studentsService'
import { classesService, type Class } from '@/services/classesService'
import { useToast } from '@/composables/useToast'

const toast = useToast()

interface StudentItem {
  id: number
  firstName: string
  lastName: string
}

const loading = ref(false)
const fees = ref<Fee[]>([])
const students = ref<StudentItem[]>([])
const classes = ref<Class[]>([])
const showFeeModal = ref(false)
const isEditingFee = ref(false)

const feeFilters = ref({ classId: '', status: '', month: '' })

const feeForm = ref({
  id: 0,
  studentId: '',
  feeStructureId: '',
  feeCategory: 'Monthly',
  amount: '',
  discountAmount: '',
  startDate: '',
  endDate: '',
  dueDate: '',
  remarks: ''
})

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

const formatCurrency = (amount: number): string => amount.toLocaleString('en-IN')
const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })
}

const loadFees = async () => {
  try {
    fees.value = await financeService.getFees()
  } catch (error) {
    console.error('Error loading fees:', error)
    fees.value = []
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
    classes.value = await classesService.getClasses()
  } catch (error) {
    console.error('Error loading classes:', error)
  }
}

const openAddFeeModal = () => {
  isEditingFee.value = false
  feeForm.value = {
    id: 0, studentId: '', feeStructureId: '', feeCategory: 'Monthly',
    amount: '', discountAmount: '', startDate: '', endDate: '', dueDate: '', remarks: ''
  }
  showFeeModal.value = true
}

const closeFeeModal = () => { showFeeModal.value = false }

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
    remarks: fee.description || ''
  }
  showFeeModal.value = true
}

const saveFee = async () => {
  loading.value = true
  try {
    if (isEditingFee.value) {
      const updateData: UpdateFeeRequest = {
        dueDate: feeForm.value.dueDate || undefined,
        discountAmount: feeForm.value.discountAmount ? parseFloat(feeForm.value.discountAmount) : undefined,
        description: feeForm.value.remarks || undefined
      }
      await financeService.updateFee(feeForm.value.id, updateData)
    } else {
      const feeData = {
        studentId: parseInt(feeForm.value.studentId),
        feeStructureId: parseInt(feeForm.value.feeStructureId),
        feeCategory: feeForm.value.feeCategory,
        amount: parseFloat(feeForm.value.amount),
        discountAmount: feeForm.value.discountAmount ? parseFloat(feeForm.value.discountAmount) : null,
        startDate: feeForm.value.startDate || null,
        endDate: feeForm.value.endDate || null,
        dueDate: feeForm.value.dueDate || null,
        remarks: feeForm.value.remarks || null
      }
      await financeService.createFee(feeData)
    }
    await loadFees()
    closeFeeModal()
    toast.success(isEditingFee.value ? 'Fee updated' : 'Fee created', 'Fee saved successfully.')
  } catch (error) {
    console.error('Error saving fee:', error)
    toast.error('Failed to save fee', 'Please try again.')
  } finally {
    loading.value = false
  }
}

const deleteFee = async (feeId: number) => {
  if (!confirm('Are you sure you want to delete this fee?')) return
  try {
    await financeService.deleteFee(feeId)
    await loadFees()
    toast.success('Fee deleted', 'Fee has been removed.')
  } catch (error) {
    console.error('Error deleting fee:', error)
    toast.error('Failed to delete fee', 'Please try again.')
  }
}

onMounted(async () => {
  await Promise.allSettled([loadFees(), loadStudents(), loadClasses()])
})
</script>
