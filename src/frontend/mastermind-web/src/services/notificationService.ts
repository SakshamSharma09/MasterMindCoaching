import { apiService } from './apiService'

export interface PortalNotification {
  id: string
  type: string
  priority: 'High' | 'Medium' | 'Low' | string
  title: string
  message: string
  actionUrl: string
  dueDate?: string | null
  createdAt: string
  meta?: Record<string, unknown> | null
}

interface NotificationResponse {
  totalCount: number
  items: PortalNotification[]
  Items?: PortalNotification[]
}

const unwrap = <T>(response: any): T => response?.data ?? response

export const notificationService = {
  async getNotifications(): Promise<PortalNotification[]> {
    const response = await apiService.get('/notifications')
    const payload = unwrap<NotificationResponse>(response)
    return payload.items || payload.Items || []
  },

  async showDeviceNotification(title: string, message: string) {
    if (!('Notification' in window)) return

    if (Notification.permission === 'default') {
      await Notification.requestPermission()
    }

    if (Notification.permission === 'granted') {
      new Notification(title, {
        body: message
      })
    }
  }
}
