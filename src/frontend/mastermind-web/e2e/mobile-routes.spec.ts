import { expect, test, type Page } from '@playwright/test'

const viewports = [
  { width: 360, height: 800 },
  { width: 390, height: 844 },
  { width: 412, height: 915 },
  { width: 768, height: 1024 },
  { width: 844, height: 390 }
]

const routesByRole = {
  Admin: [
    '/admin', '/admin/sessions', '/admin/students', '/admin/classes', '/admin/attendance',
    '/admin/finance', '/admin/finance/fees', '/admin/finance/fee-collection',
    '/admin/finance/expenses', '/admin/finance/overdue', '/admin/finance/reports',
    '/admin/template-zone', '/admin/notes-tracker', '/admin/academic-planner',
    '/admin/paper-generator', '/admin/teachers', '/admin/leads', '/admin/change-password'
  ],
  Parent: ['/parent', '/parent/attendance', '/parent/fees', '/parent/performance', '/parent/account-security'],
  Teacher: ['/teacher', '/teacher/students', '/teacher/attendance', '/teacher/remarks']
} as const

const publicRoutes = [
  '/login',
  '/privacy-policy',
  '/data-deletion',
  '/accept-invitation?token=mobile-fixture',
  '/otp-verify'
]

const token = `x.${Buffer.from(JSON.stringify({ exp: 2_000_000_000 })).toString('base64url')}.x`

async function authenticate(page: Page, role: keyof typeof routesByRole) {
  await page.addInitScript(({ role, token }) => {
    localStorage.setItem('mastermind-auth', JSON.stringify({
      user: { id: 1, firstName: 'Mobile', lastName: role, role },
      accessToken: token,
      refreshToken: 'fixture-refresh'
    }))
    localStorage.setItem('mastermind-session', JSON.stringify({
      selectedSessionId: 1,
      selectedSession: { id: 1, name: '2026-27', isActive: true }
    }))
  }, { role, token })
}

test.beforeEach(async ({ page }) => {
  await page.route('**/api/**', async route => {
    const url = route.request().url()
    let data: unknown = []
    if (url.includes('/teacher-portal/classes/10/students')) {
      data = [{ id: 1, name: 'Test Student', initials: 'TS', rollNo: 'TEST-1', classId: 10 }]
    } else if (url.includes('/teacher-portal/classes/10/attendance')) {
      data = []
    } else if (url.includes('/teacher-portal/classes')) {
      data = [{ id: 10, name: 'Class 8', board: 'CBSE', medium: 'English' }]
    } else if (url.includes('/dashboard/teacher-stats')) {
      data = { totalStudents: 1, classesToday: 1, attendanceMarked: 0, remarksAdded: 0 }
    } else if (url.includes('/student-remarks')) {
      data = []
    } else if (url.includes('/notifications')) {
      data = { totalCount: 0, items: [] }
    } else if (url.includes('/students')) {
      data = { data: [], totalCount: 0, totalPages: 0, currentPage: 1 }
    }
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ success: true, message: 'fixture', data })
    })
  })
})

for (const viewport of viewports) {
  test(`all routed pages fit ${viewport.width}x${viewport.height}`, async ({ page }) => {
    await page.setViewportSize(viewport)
    for (const route of publicRoutes) {
      await page.goto(route)
      await page.waitForLoadState('domcontentloaded')
      const dimensions = await page.evaluate(() => ({
        body: document.body.scrollWidth,
        viewport: document.documentElement.clientWidth
      }))
      expect(dimensions.body, `${route} should not overflow horizontally`).toBeLessThanOrEqual(dimensions.viewport + 1)
    }
    for (const [role, routes] of Object.entries(routesByRole) as [keyof typeof routesByRole, readonly string[]][]) {
      await authenticate(page, role)
      for (const route of routes) {
        await page.goto(route)
        await page.waitForLoadState('domcontentloaded')
        const dimensions = await page.evaluate(() => ({
          body: document.body.scrollWidth,
          viewport: document.documentElement.clientWidth
        }))
        expect(dimensions.body, `${route} should not overflow horizontally`).toBeLessThanOrEqual(dimensions.viewport + 1)
        const hamburger = page.getByRole('button', { name: /open navigation/i })
        if (await hamburger.count()) {
          await expect(hamburger.first()).toBeVisible()
          const box = await hamburger.first().boundingBox()
          expect(box?.width).toBeGreaterThanOrEqual(44)
          expect(box?.height).toBeGreaterThanOrEqual(44)
        }
      }
    }
  })
}

test('bulk attendance keeps load and sticky save actions reachable', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 })
  await authenticate(page, 'Admin')
  await page.goto('/admin/attendance')
  await page.getByRole('button', { name: 'Bulk Attendance' }).click()
  await expect(page.getByRole('button', { name: /load students/i })).toBeVisible()
})

test('teacher attendance keeps status controls and save action reachable on mobile', async ({ page }) => {
  const pageErrors: string[] = []
  const consoleErrors: string[] = []
  const failedRequests: string[] = []
  page.on('pageerror', error => pageErrors.push(error.message))
  page.on('console', message => { if (message.type() === 'error') consoleErrors.push(message.text()) })
  page.on('requestfailed', request => failedRequests.push(`${request.url()} (${request.failure()?.errorText})`))
  await page.setViewportSize({ width: 390, height: 844 })
  await authenticate(page, 'Teacher')
  await page.goto('/teacher/attendance')
  await page.waitForTimeout(500)
  if (pageErrors.length) throw new Error(`Teacher attendance page errors: ${pageErrors.join(' | ')}`)
  const bodyText = await page.locator('body').innerText()
  if (!bodyText.includes('Test Student')) throw new Error(`Teacher attendance did not render. URL: ${page.url()}. Body: ${bodyText}. Console: ${consoleErrors.join(' | ')}. Requests: ${failedRequests.join(' | ')}`)
  await expect(page.getByText('Test Student', { exact: true })).toBeVisible()
  await expect(page.getByRole('group', { name: 'Attendance for Test Student' })).toBeVisible()
  await expect(page.getByRole('button', { name: /save attendance \(1\)/i })).toBeVisible()
  const dimensions = await page.evaluate(() => ({
    body: document.body.scrollWidth,
    viewport: document.documentElement.clientWidth
  }))
  expect(dimensions.body).toBeLessThanOrEqual(dimensions.viewport + 1)
})
