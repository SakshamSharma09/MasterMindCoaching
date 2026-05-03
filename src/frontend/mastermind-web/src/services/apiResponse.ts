export interface ApiEnvelope<T> {
  success: boolean
  message: string
  data: T
  timestamp?: string
}

export const unwrapData = <T>(response: ApiEnvelope<T>): T => {
  return response?.data as T
}
