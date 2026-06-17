import { existsSync, mkdirSync, readdirSync, writeFileSync } from 'node:fs'
import { join, resolve } from 'node:path'
import { spawnSync } from 'node:child_process'
import { fileURLToPath } from 'node:url'

const rootDir = resolve(fileURLToPath(new URL('..', import.meta.url)))
const androidDir = join(rootDir, 'android')
const target = process.argv[2] || 'bundleRelease'
const allowedTargets = new Set(['bundleRelease', 'assembleRelease'])

const isWindows = process.platform === 'win32'

if (!allowedTargets.has(target)) {
  console.error(`Unsupported Android build target: ${target}`)
  process.exit(1)
}

function windowsPath(path) {
  return isWindows ? path.replaceAll('/', '\\') : path
}

function command(name) {
  return isWindows ? `${name}.cmd` : name
}

function findJavaHome() {
  const configured = process.env.JAVA_HOME
  if (configured && existsSync(join(configured, 'bin', isWindows ? 'java.exe' : 'java'))) {
    return configured
  }

  if (!isWindows) {
    return configured
  }

  const openJdkRoot = 'C:\\Program Files\\Android\\openjdk'
  if (!existsSync(openJdkRoot)) {
    return configured
  }

  const candidates = readdirSync(openJdkRoot, { withFileTypes: true })
    .filter((entry) => entry.isDirectory())
    .map((entry) => join(openJdkRoot, entry.name))
    .filter((path) => existsSync(join(path, 'bin', 'java.exe')))
    .sort()
    .reverse()

  return candidates[0] || configured
}

function findAndroidSdk() {
  const configured = process.env.ANDROID_HOME || process.env.ANDROID_SDK_ROOT
  if (configured && existsSync(configured)) {
    return configured
  }

  if (!isWindows) {
    return configured
  }

  const candidates = [
    join(process.env.LOCALAPPDATA || 'C:\\Users\\Saksham\\AppData\\Local', 'Android', 'Sdk'),
    'C:\\Program Files (x86)\\Android\\android-sdk'
  ]

  return candidates.find((path) => existsSync(path)) || configured
}

function run(name, args, options = {}) {
  console.log(`Running: ${name} ${args.join(' ')}`)

  const result = isWindows
    ? spawnSync([name, ...args].join(' '), {
        cwd: options.cwd || rootDir,
        env,
        stdio: 'inherit',
        shell: true
      })
    : spawnSync(name, args, {
        cwd: options.cwd || rootDir,
        env,
        stdio: 'inherit',
        shell: false
      })

  if (result.error) {
    console.error(result.error.message)
    process.exit(1)
  }

  if (result.status !== 0) {
    process.exit(result.status || 1)
  }
}

const javaHome = findJavaHome()
const androidSdk = findAndroidSdk()

if (!javaHome) {
  console.error('JAVA_HOME could not be resolved. Install Android Studio or set JAVA_HOME manually.')
  process.exit(1)
}

if (!androidSdk) {
  console.error('Android SDK could not be resolved. Install Android SDK or set ANDROID_HOME manually.')
  process.exit(1)
}

const env = {
  ...process.env,
  JAVA_HOME: javaHome,
  ANDROID_HOME: androidSdk,
  ANDROID_SDK_ROOT: androidSdk,
  VITE_API_BASE_URL: process.env.VITE_API_BASE_URL || 'https://mastermind-api-2404-eadxgpe5f7dch9f6.centralindia-01.azurewebsites.net/api',
  VITE_NODE_ENV: 'production',
  VITE_USE_MOCK_API: 'false'
}

mkdirSync(androidDir, { recursive: true })
writeFileSync(
  join(androidDir, 'local.properties'),
  `sdk.dir=${windowsPath(androidSdk).replaceAll('\\', '\\\\')}\n`
)

console.log(`Using JAVA_HOME=${javaHome}`)
console.log(`Using ANDROID_HOME=${androidSdk}`)

run(command('npm'), ['run', 'build'])
run(command('npx'), ['cap', 'sync', 'android'])
run(isWindows ? 'gradlew.bat' : './gradlew', [target], { cwd: androidDir })
