<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between">
      <h2 class="text-lg font-semibold text-gray-900">Template Zone</h2>
      <button @click="openNewTemplate" class="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700">New Template</button>
    </div>

    <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
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
            <div class="flex items-center justify-between">
              <div>
                <p class="font-medium text-gray-900">{{ t.name }}</p>
                <p class="text-xs text-gray-500">{{ templateTypeName(t.type) }} • {{ t.frequency || 'Adhoc' }}</p>
              </div>
              <div class="flex items-center gap-2">
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
            </div>
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
        <textarea v-model="form.body" rows="6" class="w-full mt-3 rounded-lg border-gray-300 text-sm" placeholder="Use placeholders like {{StudentName}}, {{ParentName}}, {{FeeAmount}}, {{ReceiptNumber}}"></textarea>
        <div class="mt-4 flex justify-end gap-2">
          <button @click="closeModal" class="px-3 py-2 text-sm bg-gray-100 rounded-lg">Cancel</button>
          <button @click="saveTemplate" class="px-3 py-2 text-sm text-white bg-indigo-600 rounded-lg">Save</button>
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
  form.value = { name: '', type: 1, subject: '', body: '', isActive: true, frequency: '', autoReminderDaysBefore: 0 }
  showModal.value = true
}

const editTemplate = (template: MessageTemplate) => {
  editingId.value = template.id
  form.value = { ...template }
  showModal.value = true
}

const saveTemplate = async () => {
  if (editingId.value) {
    await templateZoneService.updateTemplate(editingId.value, form.value)
  } else {
    await templateZoneService.createTemplate(form.value)
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

onMounted(refreshData)
</script>
