<template>
  <div class="fee-collection-view">
    <!-- Header -->
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-2xl font-bold text-gray-900">Fee Collection</h1>
      <div class="flex gap-3">
        <button
          @click="showSetupModal = true"
          class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          <i class="fas fa-plus mr-2"></i>
          Setup Student Fee
        </button>
        <button
          @click="refreshData"
          class="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors"
        >
          <i class="fas fa-sync-alt mr-2"></i>
          Refresh
        </button>
      </div>
    </div>

    <!-- Main Content -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <!-- Left Panel - Student Selection & Fee Details -->
      <div class="lg:col-span-1 space-y-6">
        <!-- Student Selection -->
        <div class="bg-white rounded-lg shadow p-6">
          <h3 class="text-lg font-semibold mb-4">Select Student</h3>
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">Search Student</label>
              <input
                v-model="studentSearchQuery"
                type="text"
                placeholder="Search by name or ID..."
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                @input="searchStudents"
              />
            </div>
            <div v-if="filteredStudents.length > 0" class="max-h-60 overflow-y-auto">
              <div
                v-for="student in filteredStudents"
                :key="student.id"
                @click="selectStudent(student)"
                :class="[
                  'p-3 border rounded-lg cursor-pointer transition-colors',
                  selectedStudent?.id === student.id
                    ? 'border-blue-500 bg-blue-50'
                    : 'border-gray-200 hover:bg-gray-50'
                ]"
              >
                <div class="font-medium">{{ student.name }}</div>
                <div class="text-sm text-gray-600">{{ student.class }} - ID: {{ student.id }}</div>
              </div>
            </div>
          </div>
        </div>

        <!-- Student Details -->
        <div v-if="selectedStudent" class="bg-white rounded-lg shadow p-6">
          <h3 class="text-lg font-semibold mb-4">Student Details</h3>
          <div class="space-y-3">
            <div>
              <span class="text-sm text-gray-600">Name:</span>
              <span class="ml-2 font-medium">{{ selectedStudent.name }}</span>
            </div>
            <div>
              <span class="text-sm text-gray-600">Class:</span>
              <span class="ml-2 font-medium">{{ selectedStudent.class }}</span>
            </div>
            <div>
              <span class="text-sm text-gray-600">Parent:</span>
              <span class="ml-2 font-medium">{{ selectedStudent.parentName || 'N/A' }}</span>
            </div>
            <div>
              <span class="text-sm text-gray-600">Email:</span>
              <span class="ml-2 font-medium">{{ selectedStudent.email || 'N/A' }}</span>
            </div>
            <div>
              <span class="text-sm text-gray-600">Mobile:</span>
              <span class="ml-2 font-medium">{{ selectedStudent.mobile || 'N/A' }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Right Panel - Fee Collection -->
      <div class="lg:col-span-2 space-y-6">
        <!-- Pending Fees -->
        <div v-if="studentFeeDetails" class="bg-white rounded-lg shadow p-6">
          <div class="flex justify-between items-center mb-4">
            <h3 class="text-lg font-semibold">Pending Fees</h3>
            <span class="text-sm text-gray-600">
              Total Pending: ₹{{ formatCurrency(studentFeeDetails.pendingFees.reduce((sum, fee) => sum + fee.balanceAmount, 0)) }}
            </span>
          </div>
          
          <div v-if="studentFeeDetails.pendingFees.length > 0" class="space-y-3">
            <div
              v-for="fee in studentFeeDetails.pendingFees"
              :key="fee.studentFeeId"
              class="border rounded-lg p-4"
            >
              <div class="flex items-start justify-between">
                <div class="flex-1">
                  <div class="flex items-center gap-2">
                    <input
                      type="checkbox"
                      :id="`fee-${fee.studentFeeId}`"
                      v-model="selectedFees"
                      :value="fee.studentFeeId"
                      @change="updateSelectedFeeItems"
                      class="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                    />
                    <label :for="`fee-${fee.studentFeeId}`" class="font-medium cursor-pointer">
                      {{ fee.feeType }}
                    </label>
                    <span
                      v-if="fee.isOverdue"
                      class="px-2 py-1 text-xs font-medium text-red-700 bg-red-100 rounded-full"
                    >
                      Overdue
                    </span>
                    <span class="px-2 py-1 text-xs font-medium text-gray-700 bg-gray-100 rounded-full">
                      {{ fee.feeCategory }}
                    </span>
                  </div>
                  <div class="mt-2 ml-6 text-sm text-gray-600">
                    <div>Period: {{ fee.month || fee.dueDate }}</div>
                    <div>Due Date: {{ formatDate(fee.dueDate) }}</div>
                    <div>Amount: ₹{{ formatCurrency(fee.amount) }}</div>
                    <div>Paid: ₹{{ formatCurrency(fee.paidAmount) }}</div>
                    <div class="font-medium text-red-600">Balance: ₹{{ formatCurrency(fee.balanceAmount) }}</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div v-else class="text-center py-8 text-gray-500">
            No pending fees found for this student.
          </div>
        </div>

        <!-- Payment Form -->
        <div v-if="selectedFees.length > 0" class="bg-white rounded-lg shadow p-6">
          <h3 class="text-lg font-semibold mb-4">Payment Details</h3>
          <form @submit.prevent="collectPayment" class="space-y-4">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">Payment Method</label>
                <select
                  v-model="paymentForm.paymentMethod"
                  required
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="">Select Payment Method</option>
                  <option value="Cash">Cash</option>
                  <option value="UPI">UPI</option>
                  <option value="BankTransfer">Bank Transfer</option>
                  <option value="CreditCard">Credit Card</option>
                  <option value="DebitCard">Debit Card</option>
                  <option value="Cheque">Cheque</option>
                  <option value="Online">Online</option>
                  <option value="Other">Other</option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">Transaction ID</label>
                <input
                  v-model="paymentForm.transactionId"
                  type="text"
                  placeholder="Enter transaction ID"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">Remarks</label>
              <textarea
                v-model="paymentForm.remarks"
                rows="3"
                placeholder="Enter payment remarks..."
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              ></textarea>
            </div>

            <!-- Payment Summary -->
            <div class="border-t pt-4">
              <div class="space-y-2">
                <div class="flex justify-between">
                  <span class="text-gray-600">Total Amount:</span>
                  <span class="font-medium">₹{{ formatCurrency(selectedFeesTotalAmount) }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-gray-600">To Pay:</span>
                  <span class="font-medium text-green-600">₹{{ formatCurrency(selectedFeesTotalBalance) }}</span>
                </div>
              </div>
            </div>

            <div class="flex gap-3">
              <button
                type="submit"
                :disabled="isProcessing"
                class="flex-1 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:bg-gray-400 transition-colors"
              >
                <i v-if="isProcessing" class="fas fa-spinner fa-spin mr-2"></i>
                <i v-else class="fas fa-money-bill-wave mr-2"></i>
                {{ isProcessing ? 'Processing...' : 'Collect Payment & Generate Receipt' }}
              </button>
              <button
                type="button"
                @click="resetPaymentForm"
                class="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors"
              >
                Reset
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Setup Fee Modal -->
    <div v-if="showSetupModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg shadow-xl max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
        <div class="p-6">
          <div class="flex justify-between items-center mb-4">
            <h2 class="text-xl font-bold">Setup Student Fee</h2>
            <button
              @click="showSetupModal = false"
              class="text-gray-400 hover:text-gray-600"
            >
              <i class="fas fa-times"></i>
            </button>
          </div>

          <FeeSetupForm @success="onFeeSetupSuccess" @cancel="showSetupModal = false" />
        </div>
      </div>
    </div>

    <!-- Receipt Modal -->
    <div v-if="showReceiptModal && currentReceipt" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg shadow-xl max-w-4xl w-full mx-4 max-h-[90vh] overflow-y-auto">
        <div class="p-6">
          <div class="flex justify-between items-center mb-4">
            <h2 class="text-xl font-bold">Payment Receipt</h2>
            <button
              @click="showReceiptModal = false"
              class="text-gray-400 hover:text-gray-600"
            >
              <i class="fas fa-times"></i>
            </button>
          </div>

          <ReceiptViewer :receipt="currentReceipt" @email-sent="onEmailSent" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { financeService, type Student, type StudentFeeDetails, type FeeReceipt, type CollectPaymentRequest, type PaymentFeeItem } from '@/services/financeService'
import FeeSetupForm from '@/components/FeeSetupForm.vue'
import ReceiptViewer from '@/components/ReceiptViewer.vue'

// Reactive data
const students = ref<Student[]>([])
const filteredStudents = ref<Student[]>([])
const selectedStudent = ref<Student | null>(null)
const studentFeeDetails = ref<StudentFeeDetails | null>(null)
const selectedFees = ref<number[]>([])
const selectedFeeItems = ref<PaymentFeeItem[]>([])
const isProcessing = ref(false)
const showSetupModal = ref(false)
const showReceiptModal = ref(false)
const currentReceipt = ref<FeeReceipt | null>(null)
const studentSearchQuery = ref('')

// Payment form
const paymentForm = ref({
  paymentMethod: '',
  transactionId: '',
  remarks: ''
})

// Computed properties
const selectedFeesTotalAmount = computed(() => {
  return selectedFeeItems.value.reduce((sum, item) => sum + item.itemAmount, 0)
})

const selectedFeesTotalBalance = computed(() => {
  return selectedFeeItems.value.reduce((sum, item) => sum + item.amount, 0)
})

// Methods
const loadStudents = async () => {
  try {
    // Mock students data - in real app, this would come from an API
    students.value = [
      { id: 1, name: 'John Doe', class: 'Class 10', email: 'john@example.com', mobile: '9876543210', parentName: 'Parent Name' },
      { id: 2, name: 'Jane Smith', class: 'Class 9', email: 'jane@example.com', mobile: '9876543211', parentName: 'Parent Name' },
      { id: 3, name: 'Bob Johnson', class: 'Class 11', email: 'bob@example.com', mobile: '9876543212', parentName: 'Parent Name' }
    ]
    filteredStudents.value = students.value
  } catch (error) {
    console.error('Error loading students:', error)
  }
}

const searchStudents = () => {
  const query = studentSearchQuery.value.toLowerCase()
  if (!query) {
    filteredStudents.value = students.value
  } else {
    filteredStudents.value = students.value.filter((student: Student) =>
      student.name.toLowerCase().includes(query) ||
      student.id.toString().includes(query) ||
      student.class.toLowerCase().includes(query)
    )
  }
}

const selectStudent = async (student: Student) => {
  selectedStudent.value = student
  await loadStudentFeeDetails(student.id)
}

const loadStudentFeeDetails = async (studentId: number) => {
  try {
    studentFeeDetails.value = await financeService.getStudentFeeDetails(studentId)
    selectedFees.value = []
    selectedFeeItems.value = []
  } catch (error) {
    console.error('Error loading student fee details:', error)
  }
}

const updateSelectedFeeItems = () => {
  if (!studentFeeDetails.value) return
  
  selectedFeeItems.value = studentFeeDetails.value.pendingFees
    .filter(fee => selectedFees.value.includes(fee.studentFeeId))
    .map(fee => ({
      studentFeeId: fee.studentFeeId,
      description: `${fee.feeType} - ${fee.month || 'Full Course'}`,
      itemAmount: fee.amount,
      amount: fee.balanceAmount,
      period: fee.month || fee.dueDate
    }))
}

const collectPayment = async () => {
  if (!selectedStudent.value || selectedFeeItems.value.length === 0) return

  isProcessing.value = true
  try {
    const paymentRequest: CollectPaymentRequest = {
      studentId: selectedStudent.value.id,
      paymentMethod: paymentForm.value.paymentMethod as any,
      transactionId: paymentForm.value.transactionId,
      remarks: paymentForm.value.remarks,
      feeItems: selectedFeeItems.value
    }

    const receipt = await financeService.collectPayment(paymentRequest)
    currentReceipt.value = receipt
    showReceiptModal.value = true
    
    // Reset form
    resetPaymentForm()
    await loadStudentFeeDetails(selectedStudent.value.id)
  } catch (error) {
    console.error('Error collecting payment:', error)
    alert('Error collecting payment. Please try again.')
  } finally {
    isProcessing.value = false
  }
}

const resetPaymentForm = () => {
  selectedFees.value = []
  selectedFeeItems.value = []
  paymentForm.value = {
    paymentMethod: '',
    transactionId: '',
    remarks: ''
  }
}

const refreshData = async () => {
  if (selectedStudent.value) {
    await loadStudentFeeDetails(selectedStudent.value.id)
  }
}

const onFeeSetupSuccess = () => {
  showSetupModal.value = false
  refreshData()
}

const onEmailSent = () => {
  if (currentReceipt.value) {
    currentReceipt.value.isEmailSent = true
  }
}

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('en-IN').format(amount)
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-IN')
}

// Lifecycle
onMounted(() => {
  loadStudents()
})
</script>

<style scoped>
.fee-collection-view {
  padding: 1.5rem;
}

/* Custom scrollbar */
.overflow-y-auto::-webkit-scrollbar {
  width: 6px;
}

.overflow-y-auto::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.overflow-y-auto::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.overflow-y-auto::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}
</style>
