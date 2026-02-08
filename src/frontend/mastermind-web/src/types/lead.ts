export interface Lead {
  id: number
  name: string
  phone: string
  email: string
  status: 'New' | 'Contacted' | 'Interested' | 'Converted' | 'Lost'
  source: 'Website' | 'Phone' | 'Referral' | 'Social Media' | 'Advertisement' | 'Other'
  lastFollowup: string
  notes?: string
  createdAt?: string
  updatedAt?: string
}

export interface LeadFormData {
  name: string
  phone: string
  email: string
  status: Lead['status']
  source: Lead['source']
  notes: string
}

export interface FollowUpFormData {
  name: string
  nextFollowup: string
  status: Lead['status']
  notes: string
}

export interface LeadFilters {
  search: string
  status: string
  source?: string
}

export interface LeadStats {
  total: number
  interested: number
  new: number
  contacted: number
  converted: number
  lost: number
}
