import type { CapacitorConfig } from '@capacitor/cli'

const config: CapacitorConfig = {
  appId: 'com.mastermind.coaching',
  appName: 'MasterMind Coaching',
  webDir: 'dist',
  server: {
    androidScheme: 'https'
  }
}

export default config
