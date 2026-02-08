// Device and authentication types
export interface Device {
  deviceId: string
  deviceName: string
  deviceType: string
  isTrusted: boolean
  lastUsedAt?: string
  createdAt?: string
}

export interface DeviceInfo {
  deviceId: string
  deviceName: string
  deviceType: string
  isTrusted: boolean
  canTrustDevice: boolean
  lastUsedAt?: string
  createdAt?: string
}

export interface TrustDeviceRequest {
  deviceId: string
}

export interface RevokeDeviceRequest {
  deviceId: string
}
