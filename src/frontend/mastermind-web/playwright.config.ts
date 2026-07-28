import { defineConfig } from '@playwright/test'

export default defineConfig({
  testDir: './e2e',
  timeout: 45_000,
  fullyParallel: false,
  use: {
    baseURL: 'http://127.0.0.1:4173',
    screenshot: 'only-on-failure'
  },
  webServer: {
    command: 'npm run preview -- --host 127.0.0.1',
    port: 4173,
    reuseExistingServer: true,
    timeout: 120_000
  }
})
