let expiryTimer: ReturnType<typeof setTimeout> | undefined
let handlingExpiry = false

export const tokenExpiryTime = (token: string): number | null => {
  try {
    const payload = token.split('.')[1]
    if (!payload) return null
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/')
    const decoded = JSON.parse(atob(normalized))
    return typeof decoded.exp === 'number' ? decoded.exp * 1000 : null
  } catch {
    return null
  }
}

export const cancelSessionExpiry = (): void => {
  if (expiryTimer) clearTimeout(expiryTimer)
  expiryTimer = undefined
}

export const expireSession = (): void => {
  if (handlingExpiry || window.location.pathname === '/login') return
  handlingExpiry = true
  cancelSessionExpiry()
  localStorage.removeItem('mastermind-auth')
  window.alert('Your login session has expired. Please sign in again.')
  window.location.replace('/login?reason=session-expired')
}

export const scheduleSessionExpiry = (token: string | null): void => {
  cancelSessionExpiry()
  if (!token) return
  const expiresAt = tokenExpiryTime(token)
  if (!expiresAt) return
  const remaining = expiresAt - Date.now()
  if (remaining <= 0) {
    expireSession()
    return
  }
  expiryTimer = setTimeout(expireSession, Math.min(remaining, 2_147_483_647))
}
