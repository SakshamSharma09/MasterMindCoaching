import { chromium } from '@playwright/test'
import { mkdir } from 'node:fs/promises'
import { resolve } from 'node:path'

const output = resolve('../../../docs/playstore/assets/screenshots-1.0.9')
await mkdir(output, { recursive: true })

const browser = await chromium.launch({ headless: true })
const context = await browser.newContext({
  viewport: { width: 393, height: 698 },
  deviceScaleFactor: 2.75,
  isMobile: true,
  hasTouch: true
})

await context.route('**/api/**', async route => {
  const url = route.request().url()
  const ok = data => route.fulfill({
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify({ success: true, message: 'Success', data })
  })

  if (url.includes('/auth/me')) {
    return ok({
      id: 9001,
      email: 'review-parent@example.com',
      mobile: '9000000000',
      firstName: 'Sample',
      lastName: 'Parent',
      role: 'Parent',
      roles: ['Parent'],
      isActive: true
    })
  }
  if (url.includes('/parent/children/') && url.includes('/attendance')) {
    return ok({ percentage: 92, records: [] })
  }
  if (url.includes('/parent/children/') && url.includes('/fees')) {
    return ok({ pendingFees: 1500, paymentHistory: [] })
  }
  if (url.includes('/parent/children/') && url.includes('/performance')) {
    return ok({ averageGrade: 'A', recentTests: [], recentRemarks: [] })
  }
  if (url.includes('/parent/children')) {
    return ok([{ id: 7001, firstName: 'Sample', lastName: 'Student', className: 'Class 10' }])
  }
  return ok([])
})

const page = await context.newPage()
await page.goto('http://127.0.0.1:4173/login', { waitUntil: 'networkidle' })
await page.screenshot({ path: resolve(output, '01-secure-login.png') })

await page.goto('http://127.0.0.1:4173/data-deletion', { waitUntil: 'networkidle' })
await page.screenshot({ path: resolve(output, '02-data-deletion.png') })

await page.goto('http://127.0.0.1:4173/privacy-policy', { waitUntil: 'networkidle' })
await page.screenshot({ path: resolve(output, '03-privacy-policy.png') })

await browser.close()
console.log(output)
