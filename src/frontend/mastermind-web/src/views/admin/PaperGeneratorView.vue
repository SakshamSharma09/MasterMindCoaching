<template>
  <div class="space-y-6">
    <section class="relative overflow-hidden rounded-[1.75rem] border border-white/70 bg-white/85 p-6 shadow-soft backdrop-blur">
      <div class="absolute inset-y-0 right-0 w-1/2 bg-gradient-to-l from-primary-50 via-accent-50/60 to-transparent"></div>
      <div class="relative flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
        <div>
          <p class="text-sm font-semibold uppercase tracking-wide text-primary-600">Admin tools</p>
          <h1 class="mt-2 text-3xl font-bold text-surface-950">Paper Generator</h1>
          <p class="mt-2 max-w-3xl text-surface-600">
            Upload chapter PDFs, choose the blueprint, and generate a branded question paper with an answer key.
          </p>
        </div>
        <button class="btn-secondary" type="button" :disabled="loading" @click="refreshAll">Refresh</button>
      </div>
    </section>

    <div class="grid gap-6 xl:grid-cols-[0.95fr_1.05fr]">
      <section class="rounded-2xl border border-surface-200 bg-white p-5 shadow-sm">
        <div class="flex items-start justify-between gap-4">
          <div>
            <h2 class="text-lg font-semibold text-surface-950">1. Source PDFs</h2>
            <p class="mt-1 text-sm text-surface-500">Up to 5 PDF files, 25 MB each.</p>
          </div>
          <span class="rounded-full bg-primary-50 px-3 py-1 text-xs font-semibold text-primary-700">
            {{ selectedDocumentIds.length }} selected
          </span>
        </div>

        <label
          class="mt-5 flex min-h-44 cursor-pointer flex-col items-center justify-center rounded-2xl border-2 border-dashed border-primary-200 bg-primary-50/50 p-5 text-center transition hover:border-primary-400 hover:bg-primary-50"
          @dragover.prevent
          @drop.prevent="handleDrop"
        >
          <input class="sr-only" type="file" accept="application/pdf" multiple @change="handleFileChange" />
          <span class="grid h-12 w-12 place-items-center rounded-2xl bg-white text-2xl shadow-sm">PDF</span>
          <span class="mt-3 text-sm font-semibold text-surface-900">Drop PDFs here or click to upload</span>
          <span class="mt-1 text-xs text-surface-500">Private Blob Storage. Source files expire after 7 days.</span>
        </label>

        <div v-if="uploadError" class="mt-3 rounded-xl border border-error-100 bg-error-50 px-4 py-3 text-sm text-error-700">
          {{ uploadError }}
        </div>

        <button class="btn-primary mt-4 w-full" type="button" :disabled="uploading || stagedFiles.length === 0" @click="uploadFiles">
          {{ uploading ? 'Uploading...' : `Upload ${stagedFiles.length || ''} PDF${stagedFiles.length === 1 ? '' : 's'}` }}
        </button>

        <div v-if="stagedFiles.length" class="mt-4 space-y-2">
          <p class="text-xs font-semibold uppercase tracking-wide text-surface-400">Ready to upload</p>
          <div v-for="file in stagedFiles" :key="file.name" class="flex items-center justify-between rounded-xl bg-surface-50 px-3 py-2 text-sm">
            <span class="truncate pr-3 text-surface-800">{{ file.name }}</span>
            <span class="shrink-0 text-xs text-surface-500">{{ formatBytes(file.size) }}</span>
          </div>
        </div>

        <div class="mt-6">
          <div class="mb-3 flex items-center justify-between">
            <h3 class="text-sm font-semibold text-surface-900">Recent documents</h3>
            <button class="text-xs font-semibold text-primary-600" type="button" @click="loadDocuments">Reload</button>
          </div>
          <div v-if="documents.length === 0" class="rounded-xl border border-surface-200 bg-surface-50 p-4 text-sm text-surface-500">
            No PDFs uploaded for this session yet.
          </div>
          <label
            v-for="doc in documents"
            :key="doc.id"
            class="mb-2 flex cursor-pointer items-start gap-3 rounded-xl border p-3 transition"
            :class="selectedDocumentIds.includes(doc.id) ? 'border-primary-300 bg-primary-50' : 'border-surface-200 bg-white hover:bg-surface-50'"
          >
            <input v-model="selectedDocumentIds" type="checkbox" class="mt-1" :value="doc.id" />
            <div class="min-w-0 flex-1">
              <p class="truncate text-sm font-semibold text-surface-900">{{ doc.fileName }}</p>
              <p class="mt-1 text-xs text-surface-500">{{ formatBytes(doc.sizeBytes) }} | {{ doc.status }} | expires {{ formatDate(doc.retainUntil) }}</p>
            </div>
          </label>
        </div>
      </section>

      <section class="rounded-2xl border border-surface-200 bg-white p-5 shadow-sm">
        <h2 class="text-lg font-semibold text-surface-950">2. Paper settings</h2>
        <div class="mt-5 grid gap-4 sm:grid-cols-2">
          <label class="field-label">
            Class
            <input v-model.trim="form.className" class="input-field" placeholder="Class 9" />
          </label>
          <label class="field-label">
            Subject
            <input v-model.trim="form.subject" class="input-field" placeholder="Mathematics" />
          </label>
          <label class="field-label sm:col-span-2">
            Chapter
            <input v-model.trim="form.chapter" class="input-field" placeholder="Optional chapter or unit" />
          </label>
          <label class="field-label">
            Total marks
            <input v-model.number="form.totalMarks" class="input-field" type="number" min="1" max="500" />
          </label>
          <label class="field-label">
            Duration minutes
            <input v-model.number="form.durationMinutes" class="input-field" type="number" min="1" max="360" />
          </label>
        </div>

        <div class="mt-6 rounded-2xl bg-surface-50 p-4">
          <h3 class="text-sm font-semibold text-surface-900">Question blueprint</h3>
          <div class="mt-4 grid gap-3 sm:grid-cols-5">
            <label v-for="item in questionFields" :key="item.key" class="field-label text-xs">
              {{ item.label }}
              <input v-model.number="form[item.key]" class="input-field" type="number" min="0" />
            </label>
          </div>
        </div>

        <div class="mt-6 grid gap-4 sm:grid-cols-3">
          <label class="field-label">
            Easy %
            <input v-model.number="form.easyPercentage" class="input-field" type="number" min="0" max="100" />
          </label>
          <label class="field-label">
            Medium %
            <input v-model.number="form.mediumPercentage" class="input-field" type="number" min="0" max="100" />
          </label>
          <label class="field-label">
            Hard %
            <input v-model.number="form.hardPercentage" class="input-field" type="number" min="0" max="100" />
          </label>
        </div>
        <p class="mt-2 text-xs" :class="difficultyTotal === 100 ? 'text-success-600' : 'text-error-600'">
          Difficulty total: {{ difficultyTotal }}%
        </p>

        <div class="mt-6 rounded-2xl border border-primary-100 bg-primary-50 p-4">
          <div class="flex items-center justify-between">
            <h3 class="text-sm font-semibold text-surface-900">Uploaded source relevance</h3>
            <span class="text-sm font-bold text-primary-700">{{ form.relevancePercentage }}%</span>
          </div>
          <input v-model.number="form.relevancePercentage" class="mt-4 w-full accent-primary-600" type="range" min="0" max="100" step="10" />
          <p class="mt-2 text-xs text-surface-500">Higher values reuse extracted/source questions first; lower values create similar structured questions.</p>
        </div>

        <div v-if="formError" class="mt-4 rounded-xl border border-error-100 bg-error-50 px-4 py-3 text-sm text-error-700">
          {{ formError }}
        </div>

        <div class="mt-5 grid gap-3 sm:grid-cols-2">
          <button class="btn-primary w-full" type="button" :disabled="generating || difficultyTotal !== 100" @click="generatePaper">
            {{ generating ? 'Generating paper...' : 'Generate Paper + Answer Key' }}
          </button>
          <button class="btn-secondary w-full" type="button" :disabled="difficultyTotal !== 100" @click="generatePrompt">
            Generate Prompt
          </button>
        </div>

        <div v-if="generatedPrompt" class="mt-5 rounded-2xl border border-primary-100 bg-white p-4 shadow-sm">
          <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h3 class="text-sm font-semibold text-surface-950">Ready-made prompt</h3>
              <p class="mt-1 text-xs text-surface-500">Built from the selected class, blueprint, difficulty, and source documents.</p>
            </div>
            <button class="btn-secondary text-sm" type="button" @click="copyPrompt">
              {{ promptCopied ? 'Copied' : 'Copy prompt' }}
            </button>
          </div>
          <textarea
            class="mt-4 min-h-64 w-full resize-y rounded-xl border border-surface-200 bg-surface-50 p-3 font-mono text-xs leading-5 text-surface-800 outline-none focus:border-primary-300 focus:ring-4 focus:ring-primary-100"
            :value="generatedPrompt"
            readonly
          ></textarea>
        </div>
      </section>
    </div>

    <section v-if="activeJob" class="rounded-2xl border border-surface-200 bg-white p-5 shadow-sm">
      <div class="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <h2 class="text-lg font-semibold text-surface-950">Generation status</h2>
          <p class="mt-1 text-sm text-surface-500">{{ activeJob.statusMessage }}</p>
        </div>
        <div class="flex gap-2">
          <button class="btn-secondary" type="button" :disabled="!activeJob.hasPaper" @click="downloadPaper(activeJob.id)">Download paper</button>
          <button class="btn-secondary" type="button" :disabled="!activeJob.hasAnswerKey" @click="downloadAnswerKey(activeJob.id)">Download answer key</button>
        </div>
      </div>
      <div class="mt-5 grid gap-3 md:grid-cols-6">
        <div v-for="step in progressSteps" :key="step.status" class="rounded-xl border p-3 text-center text-xs font-semibold" :class="stepClass(step.status)">
          {{ step.label }}
        </div>
      </div>
      <div v-if="activeJob.errorMessage" class="mt-4 rounded-xl border border-error-100 bg-error-50 px-4 py-3 text-sm text-error-700">
        {{ activeJob.errorMessage }}
      </div>
    </section>

    <section class="rounded-2xl border border-surface-200 bg-white p-5 shadow-sm">
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-lg font-semibold text-surface-950">Recent generated papers</h2>
          <p class="mt-1 text-sm text-surface-500">Session-specific history. Generated files expire after 30 days.</p>
        </div>
        <button class="btn-secondary" type="button" @click="loadJobs">Refresh</button>
      </div>
      <div v-if="jobs.length === 0" class="mt-5 rounded-xl border border-surface-200 bg-surface-50 p-5 text-sm text-surface-500">
        No papers generated yet.
      </div>
      <div v-else class="mt-5 overflow-hidden rounded-xl border border-surface-200">
        <div v-for="job in jobs" :key="job.id" class="grid gap-3 border-b border-surface-100 p-4 last:border-b-0 lg:grid-cols-[1.4fr_0.8fr_0.8fr_auto] lg:items-center">
          <div>
            <p class="font-semibold text-surface-950">{{ job.subject }} | {{ job.className }}</p>
            <p class="text-sm text-surface-500">{{ job.chapter || 'Full syllabus' }} | {{ job.totalMarks }} marks | {{ formatDate(job.createdAt) }}</p>
          </div>
          <span class="rounded-full px-3 py-1 text-xs font-semibold" :class="statusClass(job.status)">{{ job.status }}</span>
          <p class="text-sm text-surface-500">{{ job.statusMessage }}</p>
          <div class="flex gap-2">
            <button class="btn-secondary text-sm" type="button" :disabled="!job.hasPaper" @click="downloadPaper(job.id)">Paper</button>
            <button class="btn-secondary text-sm" type="button" :disabled="!job.hasAnswerKey" @click="downloadAnswerKey(job.id)">Key</button>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useSessionStore } from '@/stores/session'
import paperPromptTemplate from '@/prompts/paper-generator-template.txt?raw'
import {
  paperGeneratorService,
  type CreatePaperGenerationJobRequest,
  type PaperDocument,
  type PaperGenerationJob,
  type PaperGenerationStatus
} from '@/services/paperGeneratorService'

const sessionStore = useSessionStore()
const documents = ref<PaperDocument[]>([])
const jobs = ref<PaperGenerationJob[]>([])
const activeJob = ref<PaperGenerationJob | null>(null)
const stagedFiles = ref<File[]>([])
const selectedDocumentIds = ref<number[]>([])
const loading = ref(false)
const uploading = ref(false)
const generating = ref(false)
const uploadError = ref('')
const formError = ref('')
const generatedPrompt = ref('')
const promptCopied = ref(false)

const form = reactive<CreatePaperGenerationJobRequest>({
  sessionId: null,
  className: '',
  subject: '',
  chapter: '',
  totalMarks: 80,
  durationMinutes: 180,
  mcqCount: 10,
  oneMarkCount: 10,
  twoMarkCount: 5,
  fiveMarkCount: 4,
  caseStudyCount: 1,
  easyPercentage: 20,
  mediumPercentage: 50,
  hardPercentage: 30,
  relevancePercentage: 80,
  selectedDocumentIds: []
})

type QuestionCountKey = 'mcqCount' | 'oneMarkCount' | 'twoMarkCount' | 'fiveMarkCount' | 'caseStudyCount'

const questionFields: Array<{ key: QuestionCountKey; label: string }> = [
  { key: 'mcqCount', label: 'MCQ' },
  { key: 'oneMarkCount', label: '1 mark' },
  { key: 'twoMarkCount', label: '2 marks' },
  { key: 'fiveMarkCount', label: '5 marks' },
  { key: 'caseStudyCount', label: 'Case' }
]

const progressSteps: Array<{ status: PaperGenerationStatus; label: string }> = [
  { status: 'Uploaded', label: 'Uploaded' },
  { status: 'Ocr', label: 'OCR' },
  { status: 'QuestionExtraction', label: 'Extraction' },
  { status: 'AiGeneration', label: 'AI' },
  { status: 'PdfGeneration', label: 'PDF' },
  { status: 'Completed', label: 'Ready' }
]

const difficultyTotal = computed(() => Number(form.easyPercentage || 0) + Number(form.mediumPercentage || 0) + Number(form.hardPercentage || 0))

const handleFileChange = (event: Event) => {
  const input = event.target as HTMLInputElement
  stageFiles(Array.from(input.files || []))
  input.value = ''
}

const handleDrop = (event: DragEvent) => {
  stageFiles(Array.from(event.dataTransfer?.files || []))
}

const stageFiles = (files: File[]) => {
  uploadError.value = ''
  const pdfs = files.filter(file => file.type === 'application/pdf' || file.name.toLowerCase().endsWith('.pdf'))
  if (pdfs.length !== files.length) {
    uploadError.value = 'Only PDF files are allowed.'
  }
  stagedFiles.value = pdfs.slice(0, 5)
  if (pdfs.length > 5) {
    uploadError.value = 'Only the first 5 PDFs were selected.'
  }
}

const uploadFiles = async () => {
  uploadError.value = ''
  uploading.value = true
  try {
    const uploaded = await paperGeneratorService.uploadDocuments(stagedFiles.value, sessionStore.selectedSessionId)
    documents.value = [...uploaded, ...documents.value]
    selectedDocumentIds.value = uploaded.map(doc => doc.id)
    stagedFiles.value = []
  } catch (error: any) {
    uploadError.value = error.response?.data?.message || error.message || 'Upload failed.'
  } finally {
    uploading.value = false
  }
}

const validateForm = () => {
  if (!form.className.trim()) return 'Class is required.'
  if (!form.subject.trim()) return 'Subject is required.'
  if (difficultyTotal.value !== 100) return 'Difficulty split must total 100%.'
  if (selectedDocumentIds.value.length > 5) return 'Select at most 5 source PDFs.'
  return ''
}

const buildPromptVariables = () => {
  const selectedDocuments = documents.value.filter(document => selectedDocumentIds.value.includes(document.id))

  return {
    className: form.className || 'Not selected',
    subject: form.subject || 'Not selected',
    chapter: form.chapter || 'Full syllabus',
    totalMarks: String(form.totalMarks || 0),
    durationMinutes: String(form.durationMinutes || 0),
    mcqCount: String(form.mcqCount || 0),
    oneMarkCount: String(form.oneMarkCount || 0),
    twoMarkCount: String(form.twoMarkCount || 0),
    fiveMarkCount: String(form.fiveMarkCount || 0),
    caseStudyCount: String(form.caseStudyCount || 0),
    easyPercentage: String(form.easyPercentage || 0),
    mediumPercentage: String(form.mediumPercentage || 0),
    hardPercentage: String(form.hardPercentage || 0),
    relevancePercentage: String(form.relevancePercentage || 0),
    selectedDocumentCount: String(selectedDocuments.length),
    selectedDocumentNames: selectedDocuments.length
      ? selectedDocuments.map(document => document.fileName).join(', ')
      : 'No uploaded documents selected'
  }
}

const renderPromptTemplate = () => {
  const variables = buildPromptVariables()
  return Object.entries(variables).reduce((prompt, [key, value]) => {
    return prompt.replaceAll(`{{${key}}}`, value)
  }, paperPromptTemplate)
}

const generatePrompt = () => {
  formError.value = validateForm()
  if (formError.value) return

  promptCopied.value = false
  generatedPrompt.value = renderPromptTemplate()
}

const copyPrompt = async () => {
  if (!generatedPrompt.value) return
  await navigator.clipboard.writeText(generatedPrompt.value)
  promptCopied.value = true
  window.setTimeout(() => {
    promptCopied.value = false
  }, 1600)
}

const generatePaper = async () => {
  formError.value = validateForm()
  if (formError.value) return

  generating.value = true
  try {
    const summary = await paperGeneratorService.createJob({
      ...form,
      sessionId: sessionStore.selectedSessionId,
      selectedDocumentIds: selectedDocumentIds.value
    })
    activeJob.value = summary.job
    await loadJobs()
  } catch (error: any) {
    formError.value = error.response?.data?.message || error.message || 'Paper generation failed.'
  } finally {
    generating.value = false
  }
}

const loadDocuments = async () => {
  documents.value = await paperGeneratorService.getDocuments()
}

const loadJobs = async () => {
  jobs.value = await paperGeneratorService.getJobs()
  if (!activeJob.value && jobs.value.length > 0) activeJob.value = jobs.value[0]
}

const refreshAll = async () => {
  loading.value = true
  try {
    await Promise.all([loadDocuments(), loadJobs()])
  } finally {
    loading.value = false
  }
}

const downloadPaper = (jobId: number) => paperGeneratorService.downloadPaper(jobId)
const downloadAnswerKey = (jobId: number) => paperGeneratorService.downloadAnswerKey(jobId)

const formatBytes = (bytes: number) => {
  if (!bytes) return '0 B'
  const units = ['B', 'KB', 'MB', 'GB']
  const index = Math.min(Math.floor(Math.log(bytes) / Math.log(1024)), units.length - 1)
  return `${(bytes / Math.pow(1024, index)).toFixed(index === 0 ? 0 : 1)} ${units[index]}`
}

const formatDate = (value?: string | null) => {
  if (!value) return 'not set'
  return new Date(value).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })
}

const statusOrder: PaperGenerationStatus[] = ['Queued', 'Uploaded', 'Ocr', 'QuestionExtraction', 'AiGeneration', 'PdfGeneration', 'Completed']

const stepClass = (status: PaperGenerationStatus) => {
  const active = activeJob.value?.status || 'Queued'
  const complete = statusOrder.indexOf(active) >= statusOrder.indexOf(status)
  if (active === 'Failed') return 'border-error-200 bg-error-50 text-error-700'
  return complete ? 'border-primary-200 bg-primary-50 text-primary-700' : 'border-surface-200 bg-surface-50 text-surface-400'
}

const statusClass = (status: PaperGenerationStatus) => {
  if (status === 'Completed') return 'bg-success-50 text-success-700'
  if (status === 'Failed') return 'bg-error-50 text-error-700'
  return 'bg-primary-50 text-primary-700'
}

onMounted(refreshAll)
</script>

<style scoped>
.field-label {
  display: flex;
  flex-direction: column;
  gap: 0.45rem;
  font-size: 0.875rem;
  font-weight: 700;
  color: rgb(31 41 55);
}

.input-field {
  width: 100%;
  border-radius: 0.9rem;
  border: 1px solid rgb(226 232 240);
  background: white;
  padding: 0.7rem 0.85rem;
  font-size: 0.95rem;
  font-weight: 600;
  color: rgb(15 23 42);
  outline: none;
  transition: border-color 160ms ease, box-shadow 160ms ease;
}

.input-field:focus {
  border-color: rgb(99 102 241);
  box-shadow: 0 0 0 4px rgba(99, 102, 241, 0.12);
}
</style>
