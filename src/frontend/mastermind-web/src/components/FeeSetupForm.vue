<template>
  <form @submit.prevent="submitForm" class="space-y-6">
    <!-- Student Selection -->
    <div>
      <label class="block text-sm font-medium text-gray-700 mb-2">Select Student *</label>
      <select
        v-model="form.studentId"
        required
        class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        @change="loadStudentDetails"
      >
        <option value="">Select a student</option>
        <option v-for="student in students" :key="student.id" :value="student.id">
          {{ student.name }} - {{ student.class }}
        </option>
      </select>
    </div>

    <!-- Fee Type Selection -->
    <div>
      <label class="block text-sm font-medium text-gray-700 mb-2">Fee Type *</label>
      <div class="grid grid-cols-3 gap-4">
        <label class="relative">
          <input
            type="radio"
            v-model="form.feeType"
            value="monthly"
            class="peer sr-only"
            @change="onFeeTypeChange"
          />
          <div class="p-4 border-2 rounded-lg cursor-pointer transition-all peer-checked:border-blue-500 peer-checked:bg-blue-50 hover:bg-gray-50">
            <div class="text-center">
              <i class="fas fa-calendar-alt text-2xl text-blue-600 mb-2"></i>
              <div class="font-medium">Monthly Fee</div>
              <div class="text-sm text-gray-600">Recurring monthly payments</div>
            </div>
          </div>
        </label>
        <label class="relative">
          <input
            type="radio"
            v-model="form.feeType"
            value="fullcourse"
            class="peer sr-only"
            @change="onFeeTypeChange"
          />
          <div class="p-4 border-2 rounded-lg cursor-pointer transition-all peer-checked:border-blue-500 peer-checked:bg-blue-50 hover:bg-gray-50">
            <div class="text-center">
              <i class="fas fa-graduation-cap text-2xl text-green-600 mb-2"></i>
              <div class="font-medium">Full Course</div>
              <div class="text-sm text-gray-600">One-time payment</div>
            </div>
          </div>
        </label>
        <label class="relative">
          <input
            type="radio"
            v-model="form.feeType"
            value="additional"
            class="peer sr-only"
            @change="onFeeTypeChange"
          />
          <div class="p-4 border-2 rounded-lg cursor-pointer transition-all peer-checked:border-blue-500 peer-checked:bg-blue-50 hover:bg-gray-50">
            <div class="text-center">
              <i class="fas fa-plus-circle text-2xl text-purple-600 mb-2"></i>
              <div class="font-medium">Additional</div>
              <div class="text-sm text-gray-600">Extra fees/materials</div>
            </div>
          </div>
        </label>
      </div>
    </div>

    <!-- Fee Structure Selection -->
    <div>
      <label class="block text-sm font-medium text-gray-700 mb-2">Fee Structure *</label>
      <select
        v-model="form.feeStructureId"
        required
        :disabled="!form.feeType"
        class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent disabled:bg-gray-100"
        @change="onFeeStructureChange"
      >
        <option value="">Select fee structure</option>
        <option v-for="structure in filteredFeeStructures" :key="structure.id" :value="structure.id">
          {{ structure.name }} - ₹{{ formatCurrency(structure.amount) }}
        </option>
      </select>
    </div>

    <!-- Fee Structure Details -->
    <div v-if="selectedFeeStructure" class="bg-gray-50 p-4 rounded-lg">
      <h4 class="font-medium mb-2">Fee Structure Details</h4>
      <div class="grid grid-cols-2 gap-4 text-sm">
        <div>
          <span class="text-gray-600">Amount:</span>
          <span class="ml-2 font-medium">₹{{ formatCurrency(selectedFeeStructure.amount) }}</span>
        </div>
        <div>
          <span class="text-gray-600">Frequency:</span>
          <span class="ml-2 font-medium">{{ selectedFeeStructure.frequency }}</span>
        </div>
        <div>
          <span class="text-gray-600">Category:</span>
          <span class="ml-2 font-medium">{{ selectedFeeStructure.category }}</span>
        </div>
        <div>
          <span class="text-gray-600">Academic Year:</span>
          <span class="ml-2 font-medium">{{ selectedFeeStructure.academicYear }}</span>
        </div>
      </div>
      <div v-if="selectedFeeStructure.description" class="mt-2 text-sm text-gray-600">
        {{ selectedFeeStructure.description }}
      </div>
    </div>

    <!-- Monthly Fee Options -->
    <div v-if="form.feeType === 'monthly'" class="space-y-4">
      <div class="grid grid-cols-2 gap-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">Start Date *</label>
          <input
            v-model="form.startDate"
            type="date"
            required
            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">Number of Months *</label>
          <input
            v-model="form.numberOfMonths"
            type="number"
            min="1"
            max="12"
            required
            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            @input="calculateEndDate"
          />
        </div>
      </div>
      <div v-if="form.endDate">
        <label class="block text-sm font-medium text-gray-700 mb-2">End Date</label>
        <div class="px-3 py-2 bg-gray-100 border border-gray-300 rounded-lg">
          {{ formatDate(form.endDate) }}
        </div>
      </div>
    </div>

    <!-- Full Course Fee Options -->
    <div v-if="form.feeType === 'fullcourse'" class="space-y-4">
      <div class="grid grid-cols-2 gap-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">Start Date *</label>
          <input
            v-model="form.startDate"
            type="date"
            required
            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">Due Date *</label>
          <input
            v-model="form.dueDate"
            type="date"
            required
            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
        </div>
      </div>
    </div>

    <!-- Academic Year -->
    <div>
      <label class="block text-sm font-medium text-gray-700 mb-2">Academic Year *</label>
      <select
        v-model="form.academicYear"
        required
        class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
      >
        <option value="">Select academic year</option>
        <option value="2023-24">2023-24</option>
        <option value="2024-25">2024-25</option>
        <option value="2025-26">2025-26</option>
        <option value="2026-27">2026-27</option>
      </select>
    </div>

    <!-- Summary -->
    <div v-if="form.feeType && selectedFeeStructure" class="bg-blue-50 p-4 rounded-lg">
      <h4 class="font-medium mb-2">Setup Summary</h4>
      <div class="space-y-1 text-sm">
        <div v-if="form.feeType === 'monthly'">
          <div><strong>Monthly Amount:</strong> ₹{{ formatCurrency(selectedFeeStructure.amount) }}</div>
          <div><strong>Duration:</strong> {{ form.numberOfMonths }} months</div>
          <div><strong>Total Amount:</strong> ₹{{ formatCurrency(selectedFeeStructure.amount * (form.numberOfMonths || 0)) }}</div>
        </div>
        <div v-else-if="form.feeType === 'fullcourse'">
          <div><strong>Course Fee:</strong> ₹{{ formatCurrency(selectedFeeStructure.amount) }}</div>
          <div><strong>Due Date:</strong> {{ formatDate(form.dueDate) }}</div>
        </div>
        <div v-else>
          <div><strong>Fee Amount:</strong> ₹{{ formatCurrency(selectedFeeStructure.amount) }}</div>
        </div>
      </div>
    </div>

    <!-- Form Actions -->
    <div class="flex gap-3 pt-4">
      <button
        type="submit"
        :disabled="isSubmitting || !isFormValid"
        class="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 transition-colors"
      >
        <i v-if="isSubmitting" class="fas fa-spinner fa-spin mr-2"></i>
        <i v-else class="fas fa-save mr-2"></i>
        {{ isSubmitting ? 'Setting up...' : 'Setup Fee' }}
      </button>
      <button
        type="button"
        @click="$emit('cancel')"
        class="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors"
      >
        Cancel
      </button>
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { financeService, type FeeStructure, type Student, type SetupStudentFeeRequest, type StudentFeeSetup } from '@/services/financeService'

const emit = defineEmits<{
  success: []
  cancel: []
}>()

// Reactive data
const students = ref<Student[]>([])
const feeStructures = ref<FeeStructure[]>([])
const isSubmitting = ref(false)

// Form data
const form = ref({
  studentId: '',
  feeType: '',
  feeStructureId: '',
  startDate: '',
  endDate: '',
  dueDate: '',
  numberOfMonths: 12,
  academicYear: new Date().getFullYear() + '-' + (new Date().getFullYear() + 1).toString().slice(-2)
})

// Computed properties
const filteredFeeStructures = computed(() => {
  if (!form.value.feeType) return []
  return feeStructures.value.filter(structure => 
    structure.category.toLowerCase() === form.value.feeType.toLowerCase()
  )
})

const selectedFeeStructure = computed(() => {
  return feeStructures.value.find(s => s.id === parseInt(form.value.feeStructureId))
})

const isFormValid = computed(() => {
  return form.value.studentId &&
         form.value.feeType &&
         form.value.feeStructureId &&
         form.value.startDate &&
         form.value.academicYear &&
         (form.value.feeType === 'monthly' ? form.value.numberOfMonths : form.value.dueDate)
})

// Methods
const loadStudents = async () => {
  try {
    // Mock data - in real app, this would come from students API
    students.value = [
      { id: 1, name: 'John Doe', class: 'Class 10', email: 'john@example.com', mobile: '9876543210', parentName: 'Parent Name' },
      { id: 2, name: 'Jane Smith', class: 'Class 9', email: 'jane@example.com', mobile: '9876543211', parentName: 'Parent Name' },
      { id: 3, name: 'Bob Johnson', class: 'Class 11', email: 'bob@example.com', mobile: '9876543212', parentName: 'Parent Name' }
    ]
  } catch (error) {
    console.error('Error loading students:', error)
  }
}

const loadFeeStructures = async () => {
  try {
    feeStructures.value = await financeService.getFeeStructures()
  } catch (error) {
    console.error('Error loading fee structures:', error)
  }
}

const onFeeTypeChange = () => {
  form.value.feeStructureId = ''
  form.value.endDate = ''
  form.value.dueDate = ''
  form.value.numberOfMonths = 12
}

const onFeeStructureChange = () => {
  // Update form based on selected fee structure
  if (selectedFeeStructure.value) {
    // Set default values based on fee structure
    if (form.value.feeType === 'monthly') {
      calculateEndDate()
    }
  }
}

const calculateEndDate = () => {
  if (form.value.startDate && form.value.numberOfMonths) {
    const start = new Date(form.value.startDate)
    const end = new Date(start)
    end.setMonth(end.getMonth() + parseInt(form.value.numberOfMonths.toString()))
    form.value.endDate = end.toISOString().split('T')[0]
  }
}

const loadStudentDetails = () => {
  // Load student-specific details if needed
  console.log('Loading details for student:', form.value.studentId)
}

const submitForm = async () => {
  if (!isFormValid.value) return

  isSubmitting.value = true
  try {
    const requestData: SetupStudentFeeRequest = {
      studentId: parseInt(form.value.studentId),
      feeStructureId: parseInt(form.value.feeStructureId),
      startDate: form.value.startDate,
      endDate: form.value.endDate || undefined,
      dueDate: form.value.dueDate || form.value.endDate,
      numberOfMonths: form.value.feeType === 'monthly' ? parseInt(form.value.numberOfMonths.toString()) : undefined,
      academicYear: form.value.academicYear
    }

    const result = await financeService.setupStudentFee(requestData)
    
    // Show success message
    alert(`Fee setup completed successfully!\n\nStudent: ${result.studentName}\nFee Type: ${result.feeType}\nTotal Amount: ₹${formatCurrency(result.totalAmount)}`)
    
    emit('success')
  } catch (error) {
    console.error('Error setting up fee:', error)
    alert('Error setting up fee. Please try again.')
  } finally {
    isSubmitting.value = false
  }
}

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('en-IN').format(amount)
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-IN')
}

// Watch for changes
watch(() => form.value.numberOfMonths, calculateEndDate)
watch(() => form.value.startDate, calculateEndDate)

// Lifecycle
onMounted(() => {
  loadStudents()
  loadFeeStructures()
})
</script>

<style scoped>
/* Custom radio button styles */
input[type="radio"]:checked + div {
  border-color: #3b82f6;
  background-color: #eff6ff;
}

input[type="radio"]:hover + div {
  border-color: #9ca3af;
}

input[type="radio"]:checked:hover + div {
  border-color: #3b82f6;
}
</style>
