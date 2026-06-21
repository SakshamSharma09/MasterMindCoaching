<template>
  <div class="space-y-6">
    <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h2 class="text-xl font-semibold text-surface-900">Template Zone</h2>
        <p class="text-sm text-gray-500">Create student-ready images and WhatsApp messages for every important touchpoint.</p>
      </div>
      <button @click="openNewTemplate" class="btn-premium px-4 py-2">New Template</button>
    </div>

    <div class="card-premium">
      <div class="grid grid-cols-1 gap-3 md:grid-cols-4">
        <select v-model="selectedType" class="rounded-lg border-gray-300 text-sm">
          <option value="">All Types</option>
          <option value="1">Birthday</option>
          <option value="2">Fee Reminder</option>
          <option value="3">Fee Receipt</option>
          <option value="4">Welcome</option>
        </select>
        <input v-model="monthFilter" type="month" class="rounded-lg border-gray-300 text-sm" />
        <input v-model.number="birthdayDaysAhead" type="number" min="0" max="60" class="rounded-lg border-gray-300 text-sm" placeholder="Birthday days ahead" />
        <button @click="refreshData" class="rounded-lg bg-gray-100 px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200">Refresh</button>
      </div>
    </div>

    <div class="grid grid-cols-1 gap-6 xl:grid-cols-[420px_minmax(0,1fr)]">
      <section class="overflow-hidden rounded-xl border border-gray-100 bg-white shadow-sm">
        <div class="border-b px-5 py-3">
          <h3 class="font-medium text-gray-900">Saved Templates</h3>
          <p class="text-xs text-gray-500">
            Use placeholders like <code v-pre>{{StudentName}}</code>, <code v-pre>{{ClassName}}</code>, <code v-pre>{{FeeAmount}}</code>.
          </p>
        </div>
        <div class="divide-y">
          <div v-for="template in filteredTemplates" :key="template.id" class="p-4">
            <div class="flex items-center justify-between gap-3">
              <div>
                <p class="font-medium text-gray-900">{{ template.name }}</p>
                <p class="text-xs text-gray-500">{{ templateTypeName(template.type) }} - {{ template.frequency || 'Adhoc' }}</p>
              </div>
              <div class="flex shrink-0 items-center gap-2">
                <button @click="runPreview(template)" class="text-sm text-sky-600">Preview</button>
                <button @click="editTemplate(template)" class="text-sm text-indigo-600">Edit</button>
                <button @click="removeTemplate(template.id)" class="text-sm text-red-600">Delete</button>
              </div>
            </div>
          </div>
          <div v-if="filteredTemplates.length === 0" class="p-6 text-sm text-gray-500">No templates found.</div>
        </div>
      </section>

      <section class="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <QueuePanel title="Welcome Queue" empty-text="No active students found for welcome cards.">
          <TemplateQueueItem
            v-for="item in welcomeStudents"
            :key="item.id"
            :title="`${item.studentName} (${item.className})`"
            :meta-lines="[
              `Joined: ${formatDisplayDate(item.joiningDate)}`,
              `Login: ${item.loginEmail || 'Add email in student profile'}`,
              `Parent: ${item.parentName || 'N/A'} - ${item.parentMobile || 'No WhatsApp number'}`
            ]"
            @download="downloadTemplateCard(buildWelcomeCard(item))"
            @whatsapp="openWhatsApp(buildWelcomeCard(item))"
          />
          <template #empty>
            <div v-if="welcomeStudents.length === 0" class="p-4 text-sm text-gray-500">No active students found for welcome cards.</div>
          </template>
        </QueuePanel>

        <QueuePanel title="Birthday Queue" empty-text="No upcoming birthdays.">
          <TemplateQueueItem
            v-for="item in birthdayReminders"
            :key="item.id"
            :title="item.studentName"
            :meta-lines="[
              `Birthday: ${formatDisplayDate(item.nextBirthday)} - In ${item.daysLeft} days`,
              `Parent: ${item.parentName || 'N/A'} - ${item.parentMobile || 'No WhatsApp number'}`
            ]"
            @download="downloadTemplateCard(buildBirthdayCard(item))"
            @whatsapp="openWhatsApp(buildBirthdayCard(item))"
          />
          <template #empty>
            <div v-if="birthdayReminders.length === 0" class="p-4 text-sm text-gray-500">No upcoming birthdays.</div>
          </template>
        </QueuePanel>

        <QueuePanel title="Fee Reminder Queue" empty-text="No fee reminders for selected month.">
          <TemplateQueueItem
            v-for="item in feeReminders"
            :key="item.id"
            :title="`${item.studentName} (${item.className})`"
            :meta-lines="[
              `Parent: ${item.parentName || 'N/A'} - Amount: Rs. ${formatCurrency(item.feeAmount)}`,
              `Due: ${formatDisplayDate(item.dueDate)} - Frequency: ${item.frequency}`
            ]"
            @download="downloadTemplateCard(buildFeeReminderCard(item))"
            @whatsapp="openWhatsApp(buildFeeReminderCard(item))"
          />
          <template #empty>
            <div v-if="feeReminders.length === 0" class="p-4 text-sm text-gray-500">No fee reminders for selected month.</div>
          </template>
        </QueuePanel>

        <QueuePanel title="Fee Receipt Logs" empty-text="No receipt logs found.">
          <TemplateQueueItem
            v-for="item in receiptLogs"
            :key="item.id"
            :title="`${item.receiptNumber || 'Receipt'} - ${item.studentName}`"
            :meta-lines="[
              `Received: Rs. ${formatCurrency(item.paidAmount)} - ${item.paymentMethod}`,
              `Date: ${formatDisplayDate(item.receiptDate)} - Parent: ${item.parentName || 'N/A'}`
            ]"
            @download="downloadTemplateCard(buildReceiptCard(item))"
            @whatsapp="openWhatsApp(buildReceiptCard(item))"
          />
          <template #empty>
            <div v-if="receiptLogs.length === 0" class="p-4 text-sm text-gray-500">No receipt logs found.</div>
          </template>
        </QueuePanel>
      </section>
    </div>

    <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" role="dialog" aria-modal="true" aria-label="Template Editor">
      <div class="max-h-[90vh] w-full max-w-2xl overflow-auto rounded-xl bg-white p-6 shadow-xl">
        <h3 class="mb-4 text-lg font-semibold">{{ editingId ? 'Edit Template' : 'New Template' }}</h3>
        <div class="grid grid-cols-1 gap-3 md:grid-cols-2">
          <input v-model="form.name" placeholder="Template Name" class="rounded-lg border-gray-300 text-sm" />
          <select v-model.number="form.type" class="rounded-lg border-gray-300 text-sm">
            <option :value="1">Birthday Wish</option>
            <option :value="2">Fee Reminder</option>
            <option :value="3">Fee Receipt</option>
            <option :value="4">Welcome</option>
          </select>
          <input v-model="form.frequency" placeholder="Frequency (e.g. Monthly)" class="rounded-lg border-gray-300 text-sm" />
          <input v-model.number="form.autoReminderDaysBefore" type="number" min="0" max="30" class="rounded-lg border-gray-300 text-sm" placeholder="Auto reminder days before" />
        </div>
        <input v-model="form.subject" placeholder="Subject" class="mt-3 w-full rounded-lg border-gray-300 text-sm" />
        <input v-model="form.imageUrl" placeholder="Template Image URL (optional)" class="mt-3 w-full rounded-lg border-gray-300 text-sm" />
        <textarea v-model="form.body" rows="6" class="mt-3 w-full rounded-lg border-gray-300 text-sm" placeholder="Use placeholders like {{StudentName}}, {{ParentName}}, {{FeeAmount}}, {{ReceiptNumber}}"></textarea>
        <div class="mt-4 flex justify-end gap-2">
          <button @click="closeModal" class="rounded-lg bg-gray-100 px-3 py-2 text-sm">Cancel</button>
          <button @click="saveTemplate" class="rounded-lg bg-indigo-600 px-3 py-2 text-sm text-white">Save</button>
        </div>
      </div>
    </div>

    <div v-if="previewOpen" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" role="dialog" aria-modal="true" aria-label="Template Preview">
      <div class="w-full max-w-2xl rounded-xl bg-white p-6 shadow-xl">
        <div class="mb-3 flex items-center justify-between">
          <h3 class="text-lg font-semibold">Template Preview</h3>
          <button @click="previewOpen = false" class="text-gray-500">Close</button>
        </div>
        <div class="rounded-lg border border-gray-200 bg-gray-50 p-4">
          <div v-if="previewImageUrl" class="mb-4 overflow-hidden rounded-lg border border-gray-200 bg-white">
            <img :src="previewImageUrl" alt="Template Preview" class="max-h-64 w-full object-cover" />
            <div class="border-t border-gray-100 p-3">
              <p class="text-sm font-medium text-gray-900">{{ previewData?.renderedSubject || '-' }}</p>
              <p class="mt-2 whitespace-pre-wrap text-sm text-gray-700">{{ previewData?.renderedBody || '-' }}</p>
            </div>
          </div>
          <p class="mb-2 text-xs text-gray-500">Subject</p>
          <p class="font-medium text-gray-900">{{ previewData?.renderedSubject || '-' }}</p>
          <p class="mb-2 mt-4 text-xs text-gray-500">Body</p>
          <p class="whitespace-pre-wrap text-gray-800">{{ previewData?.renderedBody || '-' }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, defineComponent, h, onMounted, ref } from 'vue'
import { templateZoneService, type BirthdayReminder, type FeeReceiptLog, type FeeReminder, type MessageTemplate, type TemplatePreviewResponse } from '@/services/templateZoneService'
import { studentsService } from '@/services/studentsService'

const WEBSITE_URL = 'https://victorious-glacier-0e6507000.6.azurestaticapps.net'
const PASSWORD_SETUP_PATH = `${WEBSITE_URL}/change-password`

type TemplateKind = 'welcome' | 'birthday' | 'feeReminder' | 'receipt'

interface WelcomeStudent {
  id: number
  studentName: string
  className: string
  joiningDate: string
  loginEmail: string
  parentName: string
  parentMobile: string
  profileImageUrl?: string
}

interface TemplateCardData {
  kind: TemplateKind
  fileName: string
  whatsappMobile?: string
  whatsappText: string
  title: string
  headline: string
  subhead: string
  studentName: string
  className?: string
  parentName?: string
  amount?: string
  dateLabel: string
  dateValue: string
  photoUrl?: string
  bodyLines: string[]
  fields: Array<{ label: string; value: string }>
  accent: string
}

const QueuePanel = defineComponent({
  name: 'QueuePanel',
  props: {
    title: { type: String, required: true },
    emptyText: { type: String, required: true }
  },
  setup(props, { slots }) {
    return () => h('div', { class: 'overflow-hidden rounded-xl border border-gray-100 bg-white shadow-sm' }, [
      h('div', { class: 'border-b px-5 py-3' }, [
        h('h3', { class: 'font-medium text-gray-900' }, props.title),
        h('p', { class: 'text-xs text-gray-500' }, 'Download a personalized image or open WhatsApp with the prepared message.')
      ]),
      h('div', { class: 'max-h-80 divide-y overflow-auto' }, [
        slots.default?.(),
        slots.empty?.()
      ])
    ])
  }
})

const TemplateQueueItem = defineComponent({
  name: 'TemplateQueueItem',
  props: {
    title: { type: String, required: true },
    metaLines: { type: Array as () => string[], required: true }
  },
  emits: ['download', 'whatsapp'],
  setup(props, { emit }) {
    return () => h('div', { class: 'p-4 text-sm' }, [
      h('p', { class: 'font-medium text-gray-900' }, props.title),
      ...props.metaLines.map(line => h('p', { class: 'mt-1 text-gray-500' }, line)),
      h('div', { class: 'mt-3 flex flex-wrap gap-2' }, [
        h('button', {
          class: 'rounded-md bg-indigo-50 px-3 py-1.5 text-xs font-medium text-indigo-700 hover:bg-indigo-100',
          onClick: () => emit('download')
        }, 'Download image'),
        h('button', {
          class: 'rounded-md bg-emerald-50 px-3 py-1.5 text-xs font-medium text-emerald-700 hover:bg-emerald-100',
          onClick: () => emit('whatsapp')
        }, 'Open WhatsApp')
      ])
    ])
  }
})

const templates = ref<MessageTemplate[]>([])
const birthdayReminders = ref<BirthdayReminder[]>([])
const feeReminders = ref<FeeReminder[]>([])
const receiptLogs = ref<FeeReceiptLog[]>([])
const welcomeStudents = ref<WelcomeStudent[]>([])

const selectedType = ref('')
const monthFilter = ref(new Date().toISOString().slice(0, 7))
const birthdayDaysAhead = ref(7)

const showModal = ref(false)
const editingId = ref<number | null>(null)
type TemplateEditorForm = Partial<MessageTemplate> & { imageUrl?: string }
const form = ref<TemplateEditorForm>({ name: '', type: 1, subject: '', body: '', isActive: true, frequency: '', autoReminderDaysBefore: 0, imageUrl: '' })
const previewOpen = ref(false)
const previewData = ref<TemplatePreviewResponse | null>(null)
const previewImageUrl = ref('')

const filteredTemplates = computed(() =>
  selectedType.value ? templates.value.filter(template => String(template.type) === selectedType.value) : templates.value
)

const templateTypeName = (type: number) => {
  if (type === 1) return 'Birthday Wish'
  if (type === 2) return 'Fee Reminder'
  if (type === 3) return 'Fee Receipt'
  return 'Welcome'
}

const formatCurrency = (amount?: number) => new Intl.NumberFormat('en-IN').format(amount || 0)

const formatDisplayDate = (value?: string) => {
  if (!value) return 'N/A'
  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) return value
  return parsed.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })
}

const cleanFilePart = (value: string) => value.replace(/[^a-z0-9]+/gi, '-').replace(/^-|-$/g, '').toLowerCase() || 'template'

const sanitizeMobile = (mobile?: string) => (mobile || '').replace(/[^0-9]/g, '')

const normalizeWhatsAppMobile = (mobile?: string) => {
  const number = sanitizeMobile(mobile)
  if (!number) return ''
  return number.length === 10 ? `91${number}` : number
}

const loadWelcomeStudents = async () => {
  const result = await studentsService.getStudents(1, 200)
  welcomeStudents.value = result.data.map((student: any) => {
    const activeClass = student.studentClasses?.find((studentClass: any) => studentClass.isActive)
      || student.studentClasses?.[0]

    return {
      id: student.id,
      studentName: student.fullName || `${student.firstName || ''} ${student.lastName || ''}`.trim() || `Student ${student.id}`,
      className: student.className || activeClass?.class?.name || 'Not Assigned',
      joiningDate: (student.admissionDate || student.createdAt || new Date().toISOString()).slice(0, 10),
      loginEmail: student.studentEmail || student.email || '',
      parentName: student.parentName || '',
      parentMobile: student.parentMobile || student.studentMobile || '',
      profileImageUrl: student.profileImageUrl || student.photo || ''
    }
  })
}

const refreshData = async () => {
  templates.value = await templateZoneService.getTemplates()
  birthdayReminders.value = await templateZoneService.getBirthdayReminders(birthdayDaysAhead.value)
  feeReminders.value = await templateZoneService.getFeeReminders(monthFilter.value)
  receiptLogs.value = await templateZoneService.getFeeReceiptLogs(50)
  await loadWelcomeStudents()
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
  const sampleWelcome = welcomeStudents.value[0]

  const payload: { templateId: number; studentId?: number; studentFeeId?: number; feeReceiptId?: number } = { templateId: template.id }
  if ((template.type === 1 || template.type === 4) && (sampleBirthday?.id || sampleWelcome?.id)) payload.studentId = sampleBirthday?.id || sampleWelcome?.id
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

const buildWelcomeCard = (student: WelcomeStudent): TemplateCardData => ({
  kind: 'welcome',
  fileName: `welcome-${cleanFilePart(student.studentName)}.png`,
  whatsappMobile: student.parentMobile,
  whatsappText: [
    `Welcome to The Master Mind Coaching Classes, ${student.studentName}!`,
    `Class: ${student.className}`,
    `Joining date: ${formatDisplayDate(student.joiningDate)}`,
    `Login email: ${student.loginEmail || 'Please contact admin to update login email.'}`,
    `Website/App: ${WEBSITE_URL}`,
    `Create mobile password: Login once with email OTP, then open ${PASSWORD_SETUP_PATH}`,
    'Please attach the downloaded welcome image before sending this message.'
  ].join('\n'),
  title: 'Welcome',
  headline: 'Welcome to our learning family',
  subhead: 'Your journey toward knowledge, growth, and success begins here.',
  studentName: student.studentName,
  className: student.className,
  parentName: student.parentName,
  dateLabel: 'Joining date',
  dateValue: formatDisplayDate(student.joiningDate),
  photoUrl: student.profileImageUrl,
  bodyLines: [
    `We are delighted to have ${student.studentName} join The Master Mind Coaching Classes.`,
    'We are committed to guiding and supporting every step of the way.',
    student.loginEmail ? `Parent login email: ${student.loginEmail}` : 'Ask admin to add parent email for app login.'
  ],
  fields: [
    { label: 'Student', value: student.studentName },
    { label: 'Class', value: student.className },
    { label: 'Login email', value: student.loginEmail || 'Update in profile' }
  ],
  accent: '#d59b2d'
})

const buildBirthdayCard = (item: BirthdayReminder): TemplateCardData => ({
  kind: 'birthday',
  fileName: `birthday-${cleanFilePart(item.studentName)}.png`,
  whatsappMobile: item.parentMobile,
  whatsappText: [
    `Happy Birthday ${item.studentName}!`,
    'The Master Mind Coaching Classes wishes you a bright year full of learning and joy.',
    `Website/App: ${WEBSITE_URL}`,
    'Please attach the downloaded birthday image before sending this message.'
  ].join('\n'),
  title: 'Birthday Wishes',
  headline: `Happy Birthday, ${item.studentName}!`,
  subhead: 'May this year bring confidence, curiosity, and success.',
  studentName: item.studentName,
  parentName: item.parentName,
  dateLabel: 'Birthday',
  dateValue: formatDisplayDate(item.nextBirthday),
  photoUrl: item.profileImageUrl,
  bodyLines: [
    'From everyone at The Master Mind Coaching Classes, we wish you a joyful birthday.',
    'Keep learning, keep growing, and keep shining.'
  ],
  fields: [
    { label: 'Student', value: item.studentName },
    { label: 'Birthday', value: formatDisplayDate(item.nextBirthday) },
    { label: 'Days left', value: `${item.daysLeft}` }
  ],
  accent: '#ec4899'
})

const buildFeeReminderCard = (item: FeeReminder): TemplateCardData => ({
  kind: 'feeReminder',
  fileName: `fee-reminder-${cleanFilePart(item.studentName)}.png`,
  whatsappMobile: item.parentMobile,
  whatsappText: [
    'Fee reminder from The Master Mind Coaching Classes.',
    `Student: ${item.studentName}`,
    `Class: ${item.className}`,
    `Pending amount: Rs. ${formatCurrency(item.feeAmount)}`,
    `Due date: ${formatDisplayDate(item.dueDate)}`,
    `Website/App: ${WEBSITE_URL}`,
    'Please attach the downloaded reminder image before sending this message.'
  ].join('\n'),
  title: 'Fee Reminder',
  headline: 'Fee payment reminder',
  subhead: 'Kindly complete the pending fee by the due date.',
  studentName: item.studentName,
  className: item.className,
  parentName: item.parentName,
  amount: `Rs. ${formatCurrency(item.feeAmount)}`,
  dateLabel: 'Due date',
  dateValue: formatDisplayDate(item.dueDate),
  bodyLines: [
    `This is a gentle reminder for ${item.studentName}'s pending fee.`,
    'Thank you for supporting timely academic planning and continuity.'
  ],
  fields: [
    { label: 'Student', value: item.studentName },
    { label: 'Class', value: item.className },
    { label: 'Amount', value: `Rs. ${formatCurrency(item.feeAmount)}` }
  ],
  accent: '#f59e0b'
})

const buildReceiptCard = (item: FeeReceiptLog): TemplateCardData => ({
  kind: 'receipt',
  fileName: `fee-receipt-${cleanFilePart(item.receiptNumber || item.studentName)}.png`,
  whatsappMobile: item.parentMobile,
  whatsappText: [
    'Fee receipt confirmation from The Master Mind Coaching Classes.',
    `Receipt: ${item.receiptNumber || 'Generated'}`,
    `Student: ${item.studentName}`,
    `Amount received: Rs. ${formatCurrency(item.paidAmount)}`,
    `Date: ${formatDisplayDate(item.receiptDate)}`,
    `Website/App: ${WEBSITE_URL}`,
    'Please attach the downloaded receipt image before sending this message.'
  ].join('\n'),
  title: 'Fee Receipt',
  headline: 'Payment received',
  subhead: 'Thank you. Your payment has been recorded successfully.',
  studentName: item.studentName,
  parentName: item.parentName,
  amount: `Rs. ${formatCurrency(item.paidAmount)}`,
  dateLabel: 'Receipt date',
  dateValue: formatDisplayDate(item.receiptDate),
  bodyLines: [
    `Receipt ${item.receiptNumber || ''} has been generated for ${item.studentName}.`,
    `Payment method: ${item.paymentMethod || 'Recorded'}`
  ],
  fields: [
    { label: 'Receipt', value: item.receiptNumber || 'Generated' },
    { label: 'Student', value: item.studentName },
    { label: 'Received', value: `Rs. ${formatCurrency(item.paidAmount)}` }
  ],
  accent: '#10b981'
})

const openWhatsApp = (card: TemplateCardData) => {
  const number = normalizeWhatsAppMobile(card.whatsappMobile)
  if (!number) {
    alert('Parent WhatsApp number is missing. Please update the student profile first.')
    return
  }
  window.open(`https://wa.me/${number}?text=${encodeURIComponent(card.whatsappText)}`, '_blank', 'noopener,noreferrer')
}

const drawRoundedRect = (ctx: CanvasRenderingContext2D, x: number, y: number, width: number, height: number, radius: number) => {
  ctx.beginPath()
  ctx.moveTo(x + radius, y)
  ctx.arcTo(x + width, y, x + width, y + height, radius)
  ctx.arcTo(x + width, y + height, x, y + height, radius)
  ctx.arcTo(x, y + height, x, y, radius)
  ctx.arcTo(x, y, x + width, y, radius)
  ctx.closePath()
}

const drawWrappedText = (ctx: CanvasRenderingContext2D, text: string, x: number, y: number, maxWidth: number, lineHeight: number, maxLines = 4, align: CanvasTextAlign = 'left') => {
  const words = text.split(/\s+/)
  const lines: string[] = []
  let line = ''
  let truncated = false
  for (const word of words) {
    const next = line ? `${line} ${word}` : word
    if (ctx.measureText(next).width > maxWidth && line) {
      lines.push(line)
      line = word
      if (lines.length === maxLines - 1) {
        truncated = words.indexOf(word) < words.length - 1
        break
      }
    } else {
      line = next
    }
  }
  if (line && lines.length < maxLines) lines.push(line)

  ctx.textAlign = align
  lines.forEach((row, index) => {
    const output = truncated && index === maxLines - 1 ? `${row.replace(/\s+\S*$/, '')}...` : row
    ctx.fillText(output, x, y + index * lineHeight)
  })
  return lines.length * lineHeight
}

const drawFieldValue = (ctx: CanvasRenderingContext2D, value: string, x: number, y: number, maxWidth: number) => {
  if (!value.includes(' ')) {
    let fontSize = 34
    while (fontSize > 18) {
      ctx.font = `700 ${fontSize}px Arial`
      if (ctx.measureText(value).width <= maxWidth) break
      fontSize -= 2
    }
    ctx.fillText(value, x, y)
    return
  }

  ctx.font = value.length > 18 ? '700 27px Arial' : '700 34px Arial'
  drawWrappedText(ctx, value, x, y, maxWidth, 34, 2, 'center')
}

const loadImage = (src?: string): Promise<HTMLImageElement | null> => {
  return new Promise(resolve => {
    if (!src) {
      resolve(null)
      return
    }
    const image = new Image()
    image.crossOrigin = 'anonymous'
    image.onload = () => resolve(image)
    image.onerror = () => resolve(null)
    image.src = src
  })
}

const drawPhoto = async (ctx: CanvasRenderingContext2D, card: TemplateCardData) => {
  const image = await loadImage(card.photoUrl)
  const cx = 805
  const cy = 470
  const radius = 135

  ctx.save()
  ctx.beginPath()
  ctx.arc(cx, cy, radius, 0, Math.PI * 2)
  ctx.clip()
  if (image) {
    const size = Math.min(image.width, image.height)
    const sx = (image.width - size) / 2
    const sy = (image.height - size) / 2
    ctx.drawImage(image, sx, sy, size, size, cx - radius, cy - radius, radius * 2, radius * 2)
  } else {
    ctx.fillStyle = '#e8eef7'
    ctx.fillRect(cx - radius, cy - radius, radius * 2, radius * 2)
    ctx.fillStyle = '#071a3d'
    ctx.font = '700 84px Arial'
    ctx.textAlign = 'center'
    ctx.textBaseline = 'middle'
    const initials = card.studentName.split(' ').map(part => part[0]).join('').slice(0, 2).toUpperCase()
    ctx.fillText(initials || 'MM', cx, cy)
  }
  ctx.restore()

  ctx.strokeStyle = '#ffffff'
  ctx.lineWidth = 14
  ctx.beginPath()
  ctx.arc(cx, cy, radius + 8, 0, Math.PI * 2)
  ctx.stroke()
  ctx.strokeStyle = card.accent
  ctx.lineWidth = 12
  ctx.beginPath()
  ctx.arc(cx, cy, radius + 22, 0, Math.PI * 2)
  ctx.stroke()
}

const drawTemplateCard = async (card: TemplateCardData) => {
  const canvas = document.createElement('canvas')
  canvas.width = 1080
  canvas.height = 1500
  const ctx = canvas.getContext('2d')
  if (!ctx) return null

  ctx.fillStyle = '#fffdf8'
  ctx.fillRect(0, 0, canvas.width, canvas.height)

  ctx.fillStyle = '#071a3d'
  ctx.fillRect(980, 0, 100, 1500)
  ctx.fillStyle = card.accent
  ctx.fillRect(960, 0, 20, 1500)
  ctx.globalAlpha = 0.08
  ctx.fillStyle = '#071a3d'
  ctx.beginPath()
  ctx.arc(945, 820, 430, 0, Math.PI * 2)
  ctx.fill()
  ctx.globalAlpha = 1

  ctx.fillStyle = '#071a3d'
  ctx.font = '700 62px Arial'
  ctx.textAlign = 'center'
  ctx.textBaseline = 'alphabetic'
  ctx.fillText('The Master Mind', 440, 105)
  ctx.fillStyle = '#b7831c'
  ctx.font = 'italic 46px Georgia'
  ctx.fillText('Coaching Classes', 440, 162)
  ctx.fillStyle = '#071a3d'
  ctx.font = '700 23px Arial'
  ctx.fillText('A DREAM INSTITUTE FOR NURSERY TO XII', 440, 220)

  ctx.fillStyle = card.accent
  drawRoundedRect(ctx, 92, 275, 300, 56, 20)
  ctx.fill()
  ctx.fillStyle = '#ffffff'
  ctx.font = '700 24px Arial'
  ctx.textAlign = 'center'
  ctx.fillText(card.title.toUpperCase(), 242, 312)

  ctx.fillStyle = '#071a3d'
  ctx.font = 'italic 72px Georgia'
  ctx.textAlign = 'left'
  drawWrappedText(ctx, card.headline, 92, 405, 540, 72, 3)

  ctx.fillStyle = '#394761'
  ctx.font = '30px Arial'
  drawWrappedText(ctx, card.subhead, 96, 555, 560, 42, 3)

  await drawPhoto(ctx, card)

  ctx.fillStyle = '#071a3d'
  ctx.font = '30px Arial'
  let bodyY = 720
  for (const line of card.bodyLines) {
    bodyY += drawWrappedText(ctx, line, 96, bodyY, 660, 42, 3) + 12
  }

  const fieldTop = 1010
  card.fields.slice(0, 3).forEach((field, index) => {
    const x = 70 + index * 310
    ctx.strokeStyle = '#071a3d'
    ctx.lineWidth = 3
    drawRoundedRect(ctx, x, fieldTop, 270, 148, 24)
    ctx.stroke()
    ctx.fillStyle = '#071a3d'
    ctx.font = '700 22px Arial'
    ctx.textAlign = 'center'
    ctx.fillText(field.label.toUpperCase(), x + 135, fieldTop - 18)
    drawFieldValue(ctx, field.value, x + 135, fieldTop + 62, 230)
  })

  ctx.fillStyle = '#071a3d'
  ctx.font = 'italic 36px Georgia'
  ctx.textAlign = 'center'
  ctx.fillText('Your Success, Our Mission.', 540, 1255)

  ctx.fillStyle = '#071a3d'
  drawRoundedRect(ctx, 170, 1310, 740, 64, 28)
  ctx.fill()
  ctx.fillStyle = '#ffffff'
  ctx.font = '700 25px Arial'
  ctx.fillText('9887258679  |  Road No. 5, VKIA Jaipur', 540, 1351)
  ctx.fillStyle = '#071a3d'
  ctx.font = '700 23px Arial'
  ctx.fillText('EXCELLENCE  -  DEDICATION  -  SUCCESS', 540, 1422)

  return canvas
}

const downloadTemplateCard = async (card: TemplateCardData) => {
  const canvas = await drawTemplateCard(card)
  if (!canvas) return

  const safeFileName = card.fileName.endsWith('.png') ? card.fileName : `${card.fileName}.png`

  try {
    const blob = await new Promise<Blob | null>(resolve => canvas.toBlob(resolve, 'image/png', 0.96))
    if (!blob) throw new Error('Template image could not be created')

    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.download = safeFileName
    link.href = url
    link.rel = 'noopener'
    link.style.display = 'none'
    document.body.appendChild(link)
    link.click()
    window.setTimeout(() => {
      URL.revokeObjectURL(url)
      link.remove()
    }, 1200)
  } catch (error) {
    console.error('Template download failed:', error)
    const previewWindow = window.open(canvas.toDataURL('image/png'), '_blank', 'noopener,noreferrer')
    if (!previewWindow) {
      alert('Your browser blocked the image download. Please allow pop-ups for this site and try again.')
    }
  }
}

onMounted(refreshData)
</script>
