import { apiService } from './apiService'

export interface PhotoUploadResponse {
  success: boolean
  message: string
  data?: {
    blobName: string
    url: string
  }
}

export const photoService = {
  async uploadPhoto(file: File): Promise<PhotoUploadResponse> {
    const formData = new FormData()
    formData.append('file', file)

    const response = await apiService.post('/photo/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
    return response
  },

  async deletePhoto(blobName: string): Promise<{ success: boolean; message: string }> {
    return apiService.delete(`/photo/${blobName}`)
  },

  getPhotoUrl(blobName: string): string {
    if (!blobName) return ''
    // If it's already a full URL, return as-is
    if (blobName.startsWith('http')) return blobName
    // Otherwise construct the URL
    return `${import.meta.env.VITE_API_BASE_URL || ''}/api/photo/url/${blobName}`
  }
}
