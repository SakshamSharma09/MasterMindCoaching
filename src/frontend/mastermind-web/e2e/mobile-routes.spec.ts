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
    const data = url.includes('/notifications')
      ? { totalCount: 0, items: [] }
      : url.includes('/students')
        ? { data: [], totalCount: 0, totalPages: 0, currentPage: 1 }
        : []
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
