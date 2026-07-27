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
      <div class="grid grid-cols-1 md:grid-cols-5 gap-4">
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
          <label class="block text-sm font-medium text-gray-700 mb-1">Due Period</label>
          <select v-model="feeFilters.duePeriod" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
            <option value="">All Due Dates</option>
            <option value="overdue">Overdue</option>
            <option value="thisMonth">Due This Month</option>
            <option value="nextMonth">Due Next Month</option>
          </select>
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Month</label>
          <input v-model="feeFilters.month" type="month" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
        </div>
        <div class="flex items-end">
          <button @click="feeFilters = { classId: '', status: '', duePeriod: '', month: '' }" class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors">
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
                  <select v-model="feeForm.studentId" :disabled="isEditingFee" required class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm disabled:bg-gray-100">
                    <option value="">Select Student</option>
                    <option v-for="student in students" :key="student.id" :value="student.id">{{ student.firstName }} {{ student.lastName }}</option>
                  </select>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Fee Type</label>
                  <select v-model="feeForm.feeStructureId" :disabled="isEditingFee" :required="!isEditingFee" @change="applyFeeStructureDefaults" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm disabled:bg-gray-100">
                    <option value="">Select Fee Type</option>
                    <option v-for="structure in feeStructures" :key="structure.id" :value="String(structure.id)">
                      {{ structure.name }}{{ structure.frequency ? ` · ${structure.frequency}` : '' }}
                    </option>
                  </select>
                  <p v-if="feeStructures.length === 0" class="mt-1 text-xs text-amber-700">
                    No active fee types are available. Add a fee structure before assigning a fee.
                  </p>
                  <p v-else-if="selectedFeeStructure && !isEditingFee" class="mt-1 text-xs text-gray-500">
                    {{ selectedFeeStructure.frequency }}{{ selectedFeeStructure.academicYear ? ` · ${selectedFeeStructure.academicYear}` : '' }}
                  </p>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Amount</label>
                  <input v-model="feeForm.amount" :disabled="isEditingFee" type="number" required min="0.01" step="0.01" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm disabled:bg-gray-100">
                </div>
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">Start Date</label>
                    <input v-model="feeForm.startDate" :disabled="isEditingFee" type="date" :required="!isEditingFee" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm disabled:bg-gray-100">
                  </div>
                  <div v-if="!isEditingFee">
                    <label class="block text-sm font-medium text-gray-700 mb-1">Schedule End Date</label>
                    <input v-model="feeForm.endDate" type="date" required class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
                  </div>
                  <div v-else>
                    <label class="block text-sm font-medium text-gray-700 mb-1">Due Date</label>
                    <input v-model="feeForm.dueDate" type="date" required class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
                  </div>
                </div>
                <p v-if="!isEditingFee && feeForm.feeCategory === 'Monthly'" class="text-xs text-gray-500">
                  Monthly installments become due on the 1st. Future months stay hidden until their due date. The schedule stops at this date or when the student becomes inactive.
                </p>

                <button
                  type="button"
                  class="flex w-full items-center justify-between rounded-lg border border-gray-200 bg-gray-50 px-4 py-3 text-left text-sm font-medium text-gray-700 hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  :aria-expanded="showAdvancedFeeOptions"
                  aria-controls="advanced-fee-options"
                  @click="showAdvancedFeeOptions = !showAdvancedFeeOptions"
                >
                  <span>Advanced options</span>
                  <span aria-hidden="true">{{ showAdvancedFeeOptions ? '−' : '+' }}</span>
                </button>
                <div v-if="showAdvancedFeeOptions" id="advanced-fee-options" class="space-y-4 rounded-lg border border-gray-200 p-4">
                  <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div>
                      <label class="block text-sm font-medium text-gray-700 mb-1">Discount Amount</label>
                      <input v-model="feeForm.discountAmount" type="number" min="0" step="0.01" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm">
                    </div>
                    <div>
                      <label class="block text-sm font-medium text-gray-700 mb-1">Late Fee Per Day</label>
                      <input v-model="feeForm.lateFeePerDay" :disabled="isEditingFee" type="number" min="0" step="0.01" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm disabled:bg-gray-100">
                    </div>
                    <div>
                      <label class="block text-sm font-medium text-gray-700 mb-1">Grace Period (Days)</label>
                      <input v-model="feeForm.gracePeriodDays" :disabled="isEditingFee" type="number" min="0" step="1" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm disabled:bg-gray-100">
                    </div>
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">Remarks</label>
                    <textarea v-model="feeForm.remarks" rows="2" class="w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm"></textarea>
                  </div>
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
import { useSessionStore } from '@/stores/session'
import { matchesDuePeriod, type DuePeriod } from '@/utils/datePeriod'

const toast = useToast()
const sessionStore = useSessionStore()

interface StudentItem {
  id: number
  firstName: string
  lastName: string
}

interface FeeStructureItem {
  id: number
  name: string
  type?: string
  amount?: number
  frequency?: string
  academicYear?: string
  lateFeePerDay?: number
}

const loading = ref(false)
const fees = ref<Fee[]>([])
const students = ref<StudentItem[]>([])
const classes = ref<Class[]>([])
const feeStructures = ref<FeeStructureItem[]>([])
const showFeeModal = ref(false)
const isEditingFee = ref(false)
const showAdvancedFeeOptions = ref(false)

const feeFilters = ref<{ classId: string; status: string; duePeriod: DuePeriod; month: string }>({
  classId: '',
  status: '',
  duePeriod: '',
  month: ''
})

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
  lateFeePerDay: '',
  gracePeriodDays: '0',
  academicYear: '',
  remarks: ''
})

const selectedFeeStructure = computed(() =>
  feeStructures.value.find(structure => String(structure.id) === feeForm.value.feeStructureId)
)

const today = () => {
  const value = new Date()
  value.setMinutes(value.getMinutes() - value.getTimezoneOffset())
  return value.toISOString().slice(0, 10)
}

const inferFeeCategory = (structure?: FeeStructureItem) => {
  const frequency = structure?.frequency?.toLowerCase()
  if (frequency === 'monthly') return 'Monthly'
  const type = structure?.type?.toLowerCase()
  if (type && type !== 'tuition') return 'Additional'
  return 'FullCourse'
}

const applyFeeStructureDefaults = () => {
  const structure = selectedFeeStructure.value
  if (!structure) return
  feeForm.value.amount = structure.amount?.toString() || ''
  feeForm.value.feeCategory = inferFeeCategory(structure)
  feeForm.value.academicYear = structure.academicYear || ''
  feeForm.value.lateFeePerDay = structure.lateFeePerDay?.toString() || ''
}

const filteredFees = computed(() => {
  let filtered = fees.value
  if (feeFilters.value.classId) {
    filtered = filtered.filter(fee => fee.classId === parseInt(feeFilters.value.classId))
  }
  if (feeFilters.value.status) {
    filtered = filtered.filter(fee => fee.status === feeFilters.value.status)
  }
  if (feeFilters.value.duePeriod) {
    filtered = filtered.filter(fee =>
      matchesDuePeriod(fee.dueDate, feeFilters.value.duePeriod, fee.status)
    )
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
    const result = await studentsService.getStudents(1, 100, undefined, sessionStore.selectedSessionId ?? undefined)
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

const loadFeeStructures = async () => {
  try {
    feeStructures.value = await financeService.getFeeStructures()
  } catch (error) {
    console.error('Error loading fee structures:', error)
    feeStructures.value = []
  }
}

const openAddFeeModal = () => {
  isEditingFee.value = false
  feeForm.value = {
    id: 0, studentId: '', feeStructureId: '', feeCategory: 'Monthly',
    amount: '', discountAmount: '', startDate: today(),
    endDate: sessionStore.selectedSession?.endDate?.slice(0, 10) || '',
    dueDate: today(),
    lateFeePerDay: '', gracePeriodDays: '0', academicYear: '', remarks: ''
  }
  if (feeStructures.value.length > 0) {
    feeForm.value.feeStructureId = String(feeStructures.value[0].id)
    applyFeeStructureDefaults()
  }
  showAdvancedFeeOptions.value = false
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
    lateFeePerDay: '',
    gracePeriodDays: '0',
    academicYear: '',
    remarks: fee.description || ''
  }
  showAdvancedFeeOptions.value = Boolean(fee.description)
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
      if (!feeForm.value.feeStructureId) {
        toast.error('Fee type is required', 'Please select a fee type.')
        return
      }

      const feeData = {
        studentId: parseInt(feeForm.value.studentId),
        feeStructureId: parseInt(feeForm.value.feeStructureId),
        feeCategory: feeForm.value.feeCategory,
        amount: parseFloat(feeForm.value.amount),
        discountAmount: feeForm.value.discountAmount ? parseFloat(feeForm.value.discountAmount) : null,
        startDate: feeForm.value.startDate || null,
        endDate: feeForm.value.endDate || null,
        dueDate: feeForm.value.dueDate || null,
        lateFeePerDay: feeForm.value.lateFeePerDay ? parseFloat(feeForm.value.lateFeePerDay) : null,
        gracePeriodDays: feeForm.value.gracePeriodDays ? parseInt(feeForm.value.gracePeriodDays) : 0,
        academicYear: feeForm.value.academicYear,
        remarks: feeForm.value.remarks || null
      }
      await financeService.createFee(feeData)
    }
    await loadFees()
    closeFeeModal()
    toast.success(isEditingFee.value ? 'Fee updated' : 'Fee created', 'Fee saved successfully.')
  } catch (error: any) {
    console.error('Error saving fee:', error)
    const apiMessage = error?.response?.data?.message
    toast.error('Failed to save fee', apiMessage || 'Please try again.')
  } finally {
    loading.value = false
  }
}

const deleteFee = async (feeId: number) => {
  if (!confirm('Are you sure you want to delete this fee?')) return
  try {
    const message = await financeService.deleteFee(feeId)
    await loadFees()
    toast.success('Fee deleted', message)
  } catch (error: any) {
    console.error('Error deleting fee:', error)
    const apiMessage = error?.response?.data?.message
    toast.error('Fee could not be deleted', apiMessage || 'Fees with recorded payments must be retained.')
  }
}

onMounted(async () => {
  await Promise.allSettled([loadFees(), loadStudents(), loadClasses(), loadFeeStructures()])
})
</script>
