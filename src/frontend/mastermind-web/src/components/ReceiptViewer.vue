<template>
  <div class="receipt-viewer">
    <!-- Receipt Header -->
    <div class="text-center mb-6">
      <h1 class="text-2xl font-bold text-gray-900">MasterMind Coaching</h1>
      <p class="text-gray-600">Education Excellence Center</p>
      <p class="text-sm text-gray-500">123, Education Street, City - 123456</p>
      <p class="text-sm text-gray-500">Phone: +91-9876543210 | Email: info@mastermind.com</p>
    </div>

    <!-- Receipt Title -->
    <div class="border-t-2 border-b-2 border-gray-800 py-4 mb-6 text-center">
      <h2 class="text-xl font-bold text-gray-900">FEE RECEIPT</h2>
    </div>

    <!-- Receipt Details -->
    <div class="grid grid-cols-2 gap-6 mb-6">
      <div>
        <h3 class="font-semibold text-gray-900 mb-2">Receipt Details</h3>
        <div class="space-y-1 text-sm">
          <div><span class="text-gray-600">Receipt No:</span> <span class="font-medium">{{ receipt.receiptNumber }}</span></div>
          <div><span class="text-gray-600">Date:</span> <span class="font-medium">{{ formatDate(receipt.receiptDate) }}</span></div>
          <div><span class="text-gray-600">Payment Method:</span> <span class="font-medium">{{ receipt.paymentMethod }}</span></div>
        </div>
      </div>
      <div>
        <h3 class="font-semibold text-gray-900 mb-2">Student Details</h3>
        <div class="space-y-1 text-sm">
          <div><span class="text-gray-600">Name:</span> <span class="font-medium">{{ receipt.studentName }}</span></div>
          <div><span class="text-gray-600">Class:</span> <span class="font-medium">{{ receipt.studentClass }}</span></div>
          <div><span class="text-gray-600">Parent:</span> <span class="font-medium">{{ receipt.parentName }}</span></div>
        </div>
      </div>
    </div>

    <!-- Fee Details Table -->
    <div class="mb-6">
      <h3 class="font-semibold text-gray-900 mb-3">Fee Details</h3>
      <table class="w-full border-collapse">
        <thead>
          <tr class="bg-gray-100">
            <th class="border border-gray-300 px-4 py-2 text-left text-sm font-medium">Description</th>
            <th class="border border-gray-300 px-4 py-2 text-right text-sm font-medium">Amount</th>
            <th class="border border-gray-300 px-4 py-2 text-right text-sm font-medium">Discount</th>
            <th class="border border-gray-300 px-4 py-2 text-right text-sm font-medium">Paid</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(item, index) in receipt.receiptItems" :key="index">
            <td class="border border-gray-300 px-4 py-2 text-sm">{{ item.itemDescription }}</td>
            <td class="border border-gray-300 px-4 py-2 text-sm text-right">₹{{ formatCurrency(item.itemAmount) }}</td>
            <td class="border border-gray-300 px-4 py-2 text-sm text-right">₹{{ formatCurrency(item.discountAmount || 0) }}</td>
            <td class="border border-gray-300 px-4 py-2 text-sm text-right font-medium">₹{{ formatCurrency(item.finalAmount) }}</td>
          </tr>
        </tbody>
        <tfoot>
          <tr class="bg-gray-100 font-semibold">
            <td colspan="3" class="border border-gray-300 px-4 py-2 text-sm text-right">Total Amount:</td>
            <td class="border border-gray-300 px-4 py-2 text-sm text-right">₹{{ formatCurrency(receipt.totalAmount) }}</td>
          </tr>
          <tr class="bg-gray-50">
            <td colspan="3" class="border border-gray-300 px-4 py-2 text-sm text-right">Amount Paid:</td>
            <td class="border border-gray-300 px-4 py-2 text-sm text-right text-green-600 font-medium">₹{{ formatCurrency(receipt.paidAmount) }}</td>
          </tr>
          <tr v-if="receipt.balanceAmount > 0" class="bg-red-50">
            <td colspan="3" class="border border-gray-300 px-4 py-2 text-sm text-right">Balance Amount:</td>
            <td class="border border-gray-300 px-4 py-2 text-sm text-right text-red-600 font-medium">₹{{ formatCurrency(receipt.balanceAmount) }}</td>
          </tr>
        </tfoot>
      </table>
    </div>

    <!-- Contact Information -->
    <div class="mb-6">
      <h3 class="font-semibold text-gray-900 mb-2">Contact Information</h3>
      <div class="grid grid-cols-2 gap-4 text-sm">
        <div>
          <span class="text-gray-600">Email:</span> <span class="font-medium">{{ receipt.parentEmail }}</span>
        </div>
        <div>
          <span class="text-gray-600">Mobile:</span> <span class="font-medium">{{ receipt.parentMobile }}</span>
        </div>
      </div>
    </div>

    <!-- Footer -->
    <div class="border-t pt-4 text-center text-sm text-gray-600">
      <p>This is a computer-generated receipt and does not require signature.</p>
      <p>Thank you for choosing MasterMind Coaching!</p>
    </div>

    <!-- Action Buttons -->
    <div class="mt-6 flex gap-3 justify-center">
      <button
        @click="sendEmail"
        :disabled="isSendingEmail || receipt.isEmailSent"
        class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 transition-colors"
      >
        <i v-if="isSendingEmail" class="fas fa-spinner fa-spin mr-2"></i>
        <i v-else class="fas fa-envelope mr-2"></i>
        {{ receipt.isEmailSent ? 'Email Sent' : 'Send Email' }}
      </button>
      <button
        @click="printReceipt"
        class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
      >
        <i class="fas fa-print mr-2"></i>
        Print Receipt
      </button>
      <button
        @click="downloadPDF"
        class="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition-colors"
      >
        <i class="fas fa-download mr-2"></i>
        Download PDF
      </button>
      <button
        @click="shareReceipt"
        class="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors"
      >
        <i class="fas fa-share-alt mr-2"></i>
        Share
      </button>
    </div>

    <!-- Success Message -->
    <div v-if="showSuccessMessage" class="mt-4 p-4 bg-green-100 border border-green-400 text-green-700 rounded-lg">
      <i class="fas fa-check-circle mr-2"></i>
      {{ successMessage }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { financeService, type FeeReceipt } from '@/services/financeService'

const props = defineProps<{
  receipt: FeeReceipt
}>()

const emit = defineEmits<{
  emailSent: []
}>()

const isSendingEmail = ref(false)
const showSuccessMessage = ref(false)
const successMessage = ref('')

const sendEmail = async () => {
  isSendingEmail.value = true
  try {
    await financeService.sendReceiptEmail(props.receipt.id)
    emit('emailSent')
    showSuccess('Receipt sent successfully to ' + props.receipt.parentEmail)
  } catch (error) {
    console.error('Error sending email:', error)
    showSuccess('Failed to send email. Please try again.')
  } finally {
    isSendingEmail.value = false
  }
}

const printReceipt = () => {
  window.print()
  showSuccess('Print dialog opened. Please select your printer.')
}

const downloadPDF = () => {
  // In a real implementation, this would generate and download a PDF
  // For now, we'll create a simple text version
  const receiptContent = generateReceiptText()
  const blob = new Blob([receiptContent], { type: 'text/plain' })
  const url = window.URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `Receipt-${props.receipt.receiptNumber}.txt`
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  window.URL.revokeObjectURL(url)
  
  showSuccess('Receipt downloaded successfully!')
}

const shareReceipt = async () => {
  if (navigator.share) {
    try {
      await navigator.share({
        title: `Fee Receipt - ${props.receipt.receiptNumber}`,
        text: `Fee receipt for ${props.receipt.studentName} - Amount: ₹${formatCurrency(props.receipt.paidAmount)}`,
        url: window.location.href
      })
      showSuccess('Receipt shared successfully!')
    } catch (error) {
      console.error('Error sharing:', error)
    }
  } else {
    // Fallback - copy to clipboard
    const receiptText = `Fee Receipt - ${props.receipt.receiptNumber}\nStudent: ${props.receipt.studentName}\nAmount: ₹${formatCurrency(props.receipt.paidAmount)}`
    navigator.clipboard.writeText(receiptText).then(() => {
      showSuccess('Receipt details copied to clipboard!')
    })
  }
}

const generateReceiptText = () => {
  let text = `FEE RECEIPT\n`
  text += `================\n\n`
  text += `MasterMind Coaching\n`
  text += `Education Excellence Center\n\n`
  text += `Receipt No: ${props.receipt.receiptNumber}\n`
  text += `Date: ${formatDate(props.receipt.receiptDate)}\n`
  text += `Payment Method: ${props.receipt.paymentMethod}\n\n`
  text += `Student Details:\n`
  text += `Name: ${props.receipt.studentName}\n`
  text += `Class: ${props.receipt.studentClass}\n`
  text += `Parent: ${props.receipt.parentName}\n\n`
  text += `Fee Details:\n`
  text += `----------------\n`
  
  props.receipt.receiptItems.forEach(item => {
    text += `${item.itemDescription}: ₹${formatCurrency(item.finalAmount)}\n`
  })
  
  text += `----------------\n`
  text += `Total Amount: ₹${formatCurrency(props.receipt.totalAmount)}\n`
  text += `Amount Paid: ₹${formatCurrency(props.receipt.paidAmount)}\n`
  if (props.receipt.balanceAmount > 0) {
    text += `Balance: ₹${formatCurrency(props.receipt.balanceAmount)}\n`
  }
  
  text += `\nContact Information:\n`
  text += `Email: ${props.receipt.parentEmail}\n`
  text += `Mobile: ${props.receipt.parentMobile}\n\n`
  text += `Thank you for choosing MasterMind Coaching!\n`
  
  return text
}

const showSuccess = (message: string) => {
  successMessage.value = message
  showSuccessMessage.value = true
  setTimeout(() => {
    showSuccessMessage.value = false
  }, 3000)
}

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('en-IN').format(amount)
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-IN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>

<style scoped>
.receipt-viewer {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
  background: white;
  font-family: 'Courier New', monospace;
}

/* Print styles */
@media print {
  .receipt-viewer {
    padding: 1rem;
    box-shadow: none;
  }
  
  button {
    display: none !important;
  }
  
  .receipt-viewer > div:last-child {
    display: none !important;
  }
}

/* Table styles */
table {
  font-family: inherit;
}

table th,
table td {
  font-family: inherit;
}

/* Button hover effects */
button:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

button:disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

/* Success message animation */
@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.bg-green-100 {
  animation: slideIn 0.3s ease-out;
}
</style>
