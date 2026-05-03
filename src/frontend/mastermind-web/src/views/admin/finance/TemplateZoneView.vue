<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between">
      <h2 class="text-xl font-semibold text-surface-900">Template Zone</h2>
      <button @click="openNewTemplate" class="btn-premium px-4 py-2">New Template</button>
    </div>

    <div class="card-premium">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-3">
        <select v-model="selectedType" class="rounded-lg border-gray-300 text-sm">
          <option value="">All Types</option>
          <option value="1">Birthday</option>
          <option value="2">Fee Reminder</option>
          <option value="3">Fee Receipt</option>
        </select>
        <input v-model="monthFilter" type="month" class="rounded-lg border-gray-300 text-sm" />
        <input v-model.number="birthdayDaysAhead" type="number" min="0" max="60" class="rounded-lg border-gray-300 text-sm" placeholder="Birthday days ahead" />
        <button @click="refreshData" class="px-3 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200">Refresh</button>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div class="px-5 py-3 border-b font-medium">Templates</div>
        <div class="divide-y">
          <div v-for="t in filteredTemplates" :key="t.id" class="p-4">
            <div class="flex items-center justify-between gap-3">
              <div>
                <p class="font-medium text-gray-900">{{ t.name }}</p>
                <p class="text-xs text-gray-500">{{ templateTypeName(t.type) }} • {{ t.frequency || 'Adhoc' }}</p>
              </div>
              <div class="flex items-center gap-2">
                <button @click="runPreview(t)" class="text-sky-600 text-sm">Preview</button>
                <button @click="editTemplate(t)" class="text-indigo-600 text-sm">Edit</button>
                <button @click="removeTemplate(t.id)" class="text-red-600 text-sm">Delete</button>
              </div>
            </div>
          </div>
          <div v-if="filteredTemplates.length === 0" class="p-6 text-sm text-gray-500">No templates found.</div>
        </div>
      </div>

      <div class="space-y-6">
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
          <div class="px-5 py-3 border-b font-medium">Birthday Queue</div>
          <div class="max-h-56 overflow-auto divide-y">
            <div v-for="item in birthdayReminders" :key="item.id" class="p-3 text-sm">
              <p class="font-medium">{{ item.studentName }}</p>
              <p class="text-gray-500">Birthday: {{ item.nextBirthday }} • In {{ item.daysLeft }} days</p>
              <p class="text-gray-500">Parent: {{ item.parentName || 'N/A' }}</p>
              <div class="mt-2">
                <a v-if="getWhatsAppLink(item.parentMobile, item.studentName, 'birthday')" :href="getWhatsAppLink(item.parentMobile, item.studentName, 'birthday')" target="_blank" rel="noopener" class="text-green-600 text-xs font-medium">Send via WhatsApp</a>
              </div>
            </div>
            <div v-if="birthdayReminders.length === 0" class="p-3 text-sm text-gray-500">No upcoming birthdays.</div>
          </div>
        </div>

        <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
          <div class="px-5 py-3 border-b font-medium">Monthly Fee Reminder Queue</div>
          <div class="max-h-56 overflow-auto divide-y">
            <div v-for="item in feeReminders" :key="item.id" class="p-3 text-sm">
              <p class="font-medium">{{ item.studentName }} ({{ item.className }})</p>
              <p class="text-gray-500">Parent: {{ item.parentName }} • Amount: ₹{{ item.feeAmount }} • Due: {{ item.dueDate }}</p>
              <p class="text-gray-500">Joining: {{ item.joiningDate }} • Frequency: {{ item.frequency }}</p>
              <div class="mt-2">
                <a v-if="getWhatsAppLink(item.parentMobile, item.studentName, 'feeReminder', item.feeAmount)" :href="getWhatsAppLink(item.parentMobile, item.studentName, 'feeReminder', item.feeAmount)" target="_blank" rel="noopener" class="text-green-600 text-xs font-medium">Send via WhatsApp</a>
              </div>
            </div>
            <div v-if="feeReminders.length === 0" class="p-3 text-sm text-gray-500">No fee reminders for selected month.</div>
          </div>
        </div>

        <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
          <div class="px-5 py-3 border-b font-medium">Fee Receipt Logs</div>
          <div class="max-h-56 overflow-auto divide-y">
            <div v-for="item in receiptLogs" :key="item.id" class="p-3 text-sm">
              <p class="font-medium">{{ item.receiptNumber }} • {{ item.studentName }}</p>
              <p class="text-gray-500">₹{{ item.paidAmount }} • {{ item.receiptDate }} • {{ item.paymentMethod }}</p>
              <div class="mt-2">
                <a v-if="getWhatsAppLink(item.parentMobile, item.studentName, 'receipt', item.paidAmount)" :href="getWhatsAppLink(item.parentMobile, item.studentName, 'receipt', item.paidAmount)" target="_blank" rel="noopener" class="text-green-600 text-xs font-medium">Share Receipt on WhatsApp</a>
              </div>
            </div>
            <div v-if="receiptLogs.length === 0" class="p-3 text-sm text-gray-500">No receipt logs found.</div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
      <div class="bg-white rounded-xl shadow-xl max-w-2xl w-full p-6">
        <h3 class="text-lg font-semibold mb-4">{{ editingId ? 'Edit Template' : 'New Template' }}</h3>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
          <input v-model="form.name" placeholder="Template Name" class="rounded-lg border-gray-300 text-sm" />
          <select v-model.number="form.type" class="rounded-lg border-gray-300 text-sm">
            <option :value="1">Birthday Wish</option>
            <option :value="2">Fee Reminder</option>
            <option :value="3">Fee Receipt</option>
          </select>
          <input v-model="form.frequency" placeholder="Frequency (e.g. Monthly)" class="rounded-lg border-gray-300 text-sm" />
          <input v-model.number="form.autoReminderDaysBefore" type="number" min="0" max="30" class="rounded-lg border-gray-300 text-sm" placeholder="Auto reminder days before" />
        </div>
        <input v-model="form.subject" placeholder="Subject" class="w-full mt-3 rounded-lg border-gray-300 text-sm" />
        <input v-model="form.imageUrl" placeholder="Template Image URL (optional)" class="w-full mt-3 rounded-lg border-gray-300 text-sm" />
        <textarea v-model="form.body" rows="6" class="w-full mt-3 rounded-lg border-gray-300 text-sm" placeholder="Use placeholders like {{StudentName}}, {{ParentName}}, {{FeeAmount}}, {{ReceiptNumber}}"></textarea>
        <div class="mt-4 flex justify-end gap-2">
          <button @click="closeModal" class="px-3 py-2 text-sm bg-gray-100 rounded-lg">Cancel</button>
          <button @click="saveTemplate" class="px-3 py-2 text-sm text-white bg-indigo-600 rounded-lg">Save</button>
        </div>
      </div>
    </div>

    <div v-if="previewOpen" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
      <div class="bg-white rounded-xl shadow-xl max-w-2xl w-full p-6">
        <div class="flex items-center justify-between mb-3">
          <h3 class="text-lg font-semibold">Template Preview</h3>
          <button @click="previewOpen = false" class="text-gray-500">Close</button>
        </div>
        <div class="rounded-lg border border-gray-200 p-4 bg-gray-50">
          <div v-if="previewImageUrl" class="mb-4 rounded-lg overflow-hidden border border-gray-200 bg-white">
            <img :src="previewImageUrl" alt="Template Preview" class="w-full max-h-64 object-cover" />
            <div class="p-3 border-t border-gray-100">
              <p class="text-sm font-medium text-gray-900">{{ previewData?.renderedSubject || '-' }}</p>
              <p class="text-sm text-gray-700 whitespace-pre-wrap mt-2">{{ previewData?.renderedBody || '-' }}</p>
            </div>
          </div>
          <p class="text-xs text-gray-500 mb-2">Subject</p>
          <p class="font-medium text-gray-900">{{ previewData?.renderedSubject || '-' }}</p>
          <p class="text-xs text-gray-500 mt-4 mb-2">Body</p>
          <p class="text-gray-800 whitespace-pre-wrap">{{ previewData?.renderedBody || '-' }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { templateZoneService, type MessageTemplate } from '@/services/templateZoneService'

const templates = ref<MessageTemplate[]>([])
const birthdayReminders = ref<any[]>([])
const feeReminders = ref<any[]>([])
const receiptLogs = ref<any[]>([])

const selectedType = ref('')
const monthFilter = ref(new Date().toISOString().slice(0, 7))
const birthdayDaysAhead = ref(7)

const showModal = ref(false)
const editingId = ref<number | null>(null)
const form = ref<any>({ name: '', type: 1, subject: '', body: '', isActive: true, frequency: '', autoReminderDaysBefore: 0 })
const previewOpen = ref(false)
const previewData = ref<any>(null)
const previewImageUrl = ref('')

const filteredTemplates = computed(() =>
  selectedType.value ? templates.value.filter(t => String(t.type) === selectedType.value) : templates.value
)

const templateTypeName = (type: number) => {
  if (type === 1) return 'Birthday Wish'
  if (type === 2) return 'Fee Reminder'
  return 'Fee Receipt'
}

const refreshData = async () => {
  templates.value = await templateZoneService.getTemplates()
  birthdayReminders.value = await templateZoneService.getBirthdayReminders(birthdayDaysAhead.value)
  feeReminders.value = await templateZoneService.getFeeReminders(monthFilter.value)
  receiptLogs.value = await templateZoneService.getFeeReceiptLogs(50)
}

const openNewTemplate = () => {
  editingId.value = null
  form.value = { name: '', type: 1, subject: '', body: '', isActive: true, frequency: '', autoReminderDaysBefore: 0, imageUrl: '' }
  showModal.value = true
}

const editTemplate = (template: MessageTemplate) => {
  editingId.value = template.id
  let imageUrl = ''
  try {
    if ((template as any).variablesJson) {
      const parsed = JSON.parse((template as any).variablesJson)
      imageUrl = parsed?.imageUrl || ''
    }
  } catch {
    imageUrl = ''
  }
  form.value = { ...template, imageUrl }
  showModal.value = true
}

const saveTemplate = async () => {
  const variablesJson = JSON.stringify({ imageUrl: form.value.imageUrl || '' })
  const payload = { ...form.value, variablesJson }
  if (editingId.value) {
    await templateZoneService.updateTemplate(editingId.value, payload)
  } else {
    await templateZoneService.createTemplate(payload)
  }
  closeModal()
  await refreshData()
}

const removeTemplate = async (id: number) => {
  await templateZoneService.deleteTemplate(id)
  await refreshData()
}

const closeModal = () => {
  showModal.value = false
}

const runPreview = async (template: MessageTemplate) => {
  const sampleBirthday = birthdayReminders.value[0]
  const sampleFee = feeReminders.value[0]
  const sampleReceipt = receiptLogs.value[0]

  const payload: any = { templateId: template.id }
  if (template.type === 1 && sampleBirthday?.id) payload.studentId = sampleBirthday.id
  if (template.type === 2 && sampleFee?.id) payload.studentFeeId = sampleFee.id
  if (template.type === 3 && sampleReceipt?.id) payload.feeReceiptId = sampleReceipt.id

  previewData.value = await templateZoneService.previewTemplate(payload)
  try {
    previewImageUrl.value = (template as any).variablesJson ? (JSON.parse((template as any).variablesJson)?.imageUrl || '') : ''
  } catch {
    previewImageUrl.value = ''
  }
  previewOpen.value = true
}

const sanitizeMobile = (mobile?: string) => (mobile || '').replace(/[^0-9]/g, '')

const getWhatsAppLink = (mobile?: string, studentName?: string, type?: 'birthday' | 'feeReminder' | 'receipt', amount?: number) => {
  const number = sanitizeMobile(mobile)
  if (!number) return ''
  let text = `Hello, this is MasterMind Coaching regarding ${studentName || 'your child'}.`
  if (type === 'birthday') text = `Happy Birthday to ${studentName || 'your child'} from MasterMind Coaching.`
  if (type === 'feeReminder') text = `Fee reminder for ${studentName || 'student'}: pending amount is INR ${amount ?? 0}.`
  if (type === 'receipt') text = `Fee receipt confirmation for ${studentName || 'student'}: received INR ${amount ?? 0}.`
  return `https://wa.me/${number}?text=${encodeURIComponent(text)}`
}

onMounted(refreshData)
</script>
