import { apiService } from './apiService'

export type PaperDocumentStatus = 'Uploaded' | 'Processing' | 'Extracted' | 'Failed' | 'Expired'
export type PaperGenerationStatus = 'Queued' | 'Uploaded' | 'Ocr' | 'QuestionExtraction' | 'AiGeneration' | 'PdfGeneration' | 'Completed' | 'Failed'
export type PaperQuestionType = 'Mcq' | 'FillBlank' | 'OneMark' | 'TwoMark' | 'FiveMark' | 'CaseStudy' | 'ShortAnswer'
export type PaperQuestionDifficulty = 'Easy' | 'Medium' | 'Hard'

export interface PaperDocument {
  id: number
  sessionId?: number | null
  fileName: string
  sizeBytes: number
  status: PaperDocumentStatus
  pageCount?: number | null
  errorMessage?: string | null
  uploadedAt: string
  retainUntil?: string | null
}

export interface PaperQuestion {
  id: number
  sessionId?: number | null
  sourceDocumentId?: number | null
  subject: string
  className: string
  chapter?: string | null
  marks: number
  questionType: PaperQuestionType
  difficulty: PaperQuestionDifficulty
  questionText: string
  answerText?: string | null
  sourceMode: string
}

export interface PaperGenerationJob {
  id: number
  sessionId?: number | null
  status: PaperGenerationStatus
  statusMessage: string
  className: string
  subject: string
  chapter?: string | null
  totalMarks: number
  durationMinutes: number
  relevancePercentage: number
  aiModelUsed?: string | null
  errorMessage?: string | null
  hasPaper: boolean
  hasAnswerKey: boolean
  createdAt: string
  startedAt?: string | null
  completedAt?: string | null
  retainUntil?: string | null
  documents: PaperDocument[]
}

export interface CreatePaperGenerationJobRequest {
  sessionId?: number | null
  className: string
  subject: string
  chapter?: string | null
  totalMarks: number
  durationMinutes: number
  timeDurationLabel?: string
  mcqCount: number
  fibCount?: number
  trueFalseCount?: number
  oneMarkCount: number
  twoMarkCount: number
  threeMarkCount?: number
  fourMarkCount?: number
  fiveMarkCount: number
  caseStudyCount: number
  rtcCount?: number
  exactPercent?: number
  easyPercentage: number
  mediumPercentage: number
  hardPercentage: number
  relevancePercentage: number
  selectedDocumentIds: number[]
}

export interface PaperGenerationJobSummary {
  job: PaperGenerationJob
  questionCount: number
  previewQuestions: PaperQuestion[]
}

const unwrap = <T>(response: any): T => response?.data ?? response

export const paperGeneratorService = {
  async uploadDocuments(files: File[], sessionId?: number | null): Promise<PaperDocument[]> {
    const formData = new FormData()
    files.forEach(file => formData.append('files', file))
    if (sessionId) formData.append('sessionId', sessionId.toString())

    const response = await apiService.post('/paper-generator/documents', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      timeout: 120000
    })
    return unwrap<PaperDocument[]>(response)
  },

  async getDocuments(): Promise<PaperDocument[]> {
    const response = await apiService.get('/paper-generator/documents')
    return unwrap<PaperDocument[]>(response)
  },

  async createJob(payload: CreatePaperGenerationJobRequest): Promise<PaperGenerationJobSummary> {
    const response = await apiService.post('/paper-generator/jobs', payload, { timeout: 180000 })
    return unwrap<PaperGenerationJobSummary>(response)
  },

  async getJobs(): Promise<PaperGenerationJob[]> {
    const response = await apiService.get('/paper-generator/jobs')
    return unwrap<PaperGenerationJob[]>(response)
  },

  async getJob(id: number): Promise<PaperGenerationJob> {
    const response = await apiService.get(`/paper-generator/jobs/${id}`)
    return unwrap<PaperGenerationJob>(response)
  },

  async getQuestions(filters?: { subject?: string; className?: string; chapter?: string }): Promise<PaperQuestion[]> {
    const params = new URLSearchParams()
    if (filters?.subject) params.set('subject', filters.subject)
    if (filters?.className) params.set('className', filters.className)
    if (filters?.chapter) params.set('chapter', filters.chapter)
    const endpoint = params.toString() ? `/paper-generator/questions?${params}` : '/paper-generator/questions'
    const response = await apiService.get(endpoint)
    return unwrap<PaperQuestion[]>(response)
  },

  async downloadPaper(jobId: number): Promise<void> {
    await apiService.download(`/paper-generator/jobs/${jobId}/paper`)
  },

  async downloadAnswerKey(jobId: number): Promise<void> {
    await apiService.download(`/paper-generator/jobs/${jobId}/answer-key`)
  }
}
