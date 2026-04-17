export type LeadStatus = 'New' | 'Contacted' | 'Interested' | 'FollowUp' | 'Negotiation' | 'Converted' | 'Lost' | 'NotInterested'
export type LeadSource = 'Website' | 'PhoneCall' | 'Referral' | 'SocialMedia' | 'Advertisement' | 'WalkIn' | 'Event' | 'Other'
export type LeadPriority = 'Low' | 'Medium' | 'High' | 'Urgent'

export interface Lead {
  id: number
  name: string
  phone: string
  email: string
  parentName?: string
  parentMobile?: string
  address?: string
  city?: string
  source: string
  sourceDetails?: string
  status: string
  priority: string
  interestedClass?: string
  interestedSubject?: string
  nextFollowup?: string
  lastFollowup: string
  notes?: string
  assignedTo?: string
  convertedStudentId?: number
  convertedAt?: string
  createdAt?: string
  updatedAt?: string
  followupCount: number
}

export interface LeadFormData {
  name: string
  phone: string
  email?: string
  parentName?: string
  parentMobile?: string
  address?: string
  city?: string
  source?: string
  sourceDetails?: string
  interestedClass?: string
  interestedSubject?: string
  priority?: string
  nextFollowup?: string
  notes?: string
}

export interface FollowUpFormData {
  type?: string
  notes?: string
  status?: string
  nextFollowup?: string
  nextFollowupNotes?: string
}

export interface LeadFilters {
  search: string
  status: string
  source?: string
  page?: number
  pageSize?: number
}

export interface LeadStats {
  total: number
  new: number
  contacted: number
  interested: number
  followUp: number
  negotiation: number
  converted: number
  lost: number
}
