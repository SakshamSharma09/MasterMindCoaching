import { apiService } from './apiService'
import { API_ENDPOINTS } from '@/config/api'
import type { Lead, LeadFormData, FollowUpFormData, LeadFilters } from '@/types/lead'

const USE_MOCK_API = import.meta.env.VITE_USE_MOCK_API === 'true' || false

// Mock data for development
const mockLeads: Lead[] = []

export const leadsService = {
  // Get all leads with optional filtering
  async getLeads(filters?: LeadFilters): Promise<Lead[]> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting leads with filters:', filters)
      await new Promise(resolve => setTimeout(resolve, 800))
      
      let filteredLeads = [...mockLeads]
      
      if (filters?.search) {
        const search = filters.search.toLowerCase()
        filteredLeads = filteredLeads.filter(lead => 
          lead.name.toLowerCase().includes(search) ||
          lead.phone.includes(filters.search) ||
          lead.email.toLowerCase().includes(search)
        )
      }
      
      if (filters?.status) {
        filteredLeads = filteredLeads.filter(lead => lead.status === filters.status)
      }
      
      if (filters?.source) {
        filteredLeads = filteredLeads.filter(lead => lead.source === filters.source)
      }
      
      return filteredLeads
    }

    const params = new URLSearchParams()
    if (filters?.search) params.append('search', filters.search)
    if (filters?.status) params.append('status', filters.status)
    if (filters?.source) params.append('source', filters.source)

    const response = await apiService.get(`/leads?${params.toString()}`)
    return response.data
  },

  // Get a single lead by ID
  async getLead(id: number): Promise<Lead> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting lead with id:', id)
      await new Promise(resolve => setTimeout(resolve, 500))
      
      const lead = mockLeads.find(l => l.id === id)
      if (!lead) {
        throw new Error('Lead not found')
      }
      return lead
    }

    const response = await apiService.get(`/leads/${id}`)
    return response.data
  },

  // Create a new lead
  async createLead(data: LeadFormData): Promise<Lead> {
    if (USE_MOCK_API) {
      console.log('Mock API: Creating lead:', data)
      await new Promise(resolve => setTimeout(resolve, 1000))
      
      const newLead: Lead = {
        id: Date.now(),
        ...data,
        lastFollowup: new Date().toISOString().split('T')[0],
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      }
      
      mockLeads.push(newLead)
      return newLead
    }

    const response = await apiService.post('/leads', data)
    return response.data
  },

  // Update an existing lead
  async updateLead(id: number, data: Partial<LeadFormData>): Promise<Lead> {
    if (USE_MOCK_API) {
      console.log('Mock API: Updating lead:', id, data)
      await new Promise(resolve => setTimeout(resolve, 800))
      
      const index = mockLeads.findIndex(l => l.id === id)
      if (index === -1) {
        throw new Error('Lead not found')
      }
      
      mockLeads[index] = {
        ...mockLeads[index],
        ...data,
        updatedAt: new Date().toISOString()
      }
      
      return mockLeads[index]
    }

    const response = await apiService.put(`/leads/${id}`, data)
    return response.data
  },

  // Delete a lead
  async deleteLead(id: number): Promise<void> {
    if (USE_MOCK_API) {
      console.log('Mock API: Deleting lead:', id)
      await new Promise(resolve => setTimeout(resolve, 500))
      
      const index = mockLeads.findIndex(l => l.id === id)
      if (index === -1) {
        throw new Error('Lead not found')
      }
      
      mockLeads.splice(index, 1)
      return
    }

    await apiService.delete(`/leads/${id}`)
  },

  // Add follow-up to a lead
  async addFollowUp(id: number, data: FollowUpFormData): Promise<Lead> {
    if (USE_MOCK_API) {
      console.log('Mock API: Adding follow-up to lead:', id, data)
      await new Promise(resolve => setTimeout(resolve, 800))
      
      const index = mockLeads.findIndex(l => l.id === id)
      if (index === -1) {
        throw new Error('Lead not found')
      }
      
      mockLeads[index] = {
        ...mockLeads[index],
        status: data.status,
        lastFollowup: data.nextFollowup,
        notes: data.notes,
        updatedAt: new Date().toISOString()
      }
      
      return mockLeads[index]
    }

    const response = await apiService.post(`/leads/${id}/followup`, data)
    return response.data
  },

  // Get lead statistics
  async getLeadStats(): Promise<{
    total: number
    interested: number
    new: number
    contacted: number
    converted: number
    lost: number
  }> {
    if (USE_MOCK_API) {
      console.log('Mock API: Getting lead stats')
      await new Promise(resolve => setTimeout(resolve, 500))
      
      const total = mockLeads.length
      const interested = mockLeads.filter(l => l.status === 'Interested').length
      const newLeads = mockLeads.filter(l => l.status === 'New').length
      const contacted = mockLeads.filter(l => l.status === 'Contacted').length
      const converted = mockLeads.filter(l => l.status === 'Converted').length
      const lost = mockLeads.filter(l => l.status === 'Lost').length
      
      return { total, interested, new: newLeads, contacted, converted, lost }
    }

    const response = await apiService.get('/leads/stats')
    return response.data
  }
}

export default leadsService
