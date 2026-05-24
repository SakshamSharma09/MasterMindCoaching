# Play Store Release Checklist (MasterMind Coaching)

This is the production checklist for publishing the Android app from the current web codebase.

## 0) One-Time Setup

- [ ] Play Console developer account approved.
- [ ] App created in Play Console (`MasterMind Coaching`).
- [ ] Package name confirmed: `com.mastermind.coaching`.
- [ ] Privacy policy URL published.
- [ ] Release keystore created and stored securely.

## 1) Local Build Preparation

Recommended Windows environment:

- `JAVA_HOME=C:\Program Files\Android\openjdk\jdk-21.0.8`
- `ANDROID_HOME=C:\Users\Saksham\AppData\Local\Android\Sdk`
- `ANDROID_SDK_ROOT=C:\Users\Saksham\AppData\Local\Android\Sdk`
- `android/local.properties` should point to `sdk.dir=C\:\\Users\\Saksham\\AppData\\Local\\Android\\Sdk`

The `npm run build:aab` and `npm run build:apk` commands now use `scripts/build-android-release.mjs`, which auto-detects the Android Studio JDK/SDK on this Windows machine and injects those values into the Gradle process. This prevents the common `JAVA_HOME is not set` failure even when a newly opened terminal has not loaded Android variables correctly.

From `src/frontend/mastermind-web`:

```bash
npm install
npm run build:mobile
```

This runs web build + Capacitor Android sync.

## 2) Configure Signing for Local Release Builds

In `src/frontend/mastermind-web/android/gradle.properties` (local machine only), add:

```properties
MYAPP_UPLOAD_STORE_FILE=../keystore/mastermind-upload.jks
MYAPP_UPLOAD_KEY_ALIAS=mastermind_upload
MYAPP_UPLOAD_STORE_PASSWORD=CHANGE_ME
MYAPP_UPLOAD_KEY_PASSWORD=CHANGE_ME
```

Notes:
- Keep keystore and passwords out of Git.
- Use a secure password manager.
- Losing keystore access can block future app updates if not using managed Play App Signing correctly.

## 3) Generate Android App Bundle (.aab)

CLI method (Windows):

```bash
npm run build:aab
```

Output file:

- `src/frontend/mastermind-web/android/app/build/outputs/bundle/release/app-release.aab`

If the command still reports SDK license/package errors, run Android Studio SDK Manager once or accept licenses with `sdkmanager --licenses` against the user SDK path above.

Android Studio method:

```bash
npm run cap:open
```

In Android Studio:

1. Build -> Generate Signed Bundle / APK  
2. Android App Bundle  
3. Select release keystore  
4. Build release `.aab`

## 4) Play Console Submission Flow

Recommended rollout path:

1. Internal Testing
2. Closed Testing
3. Production

For the release:

- [ ] Upload `.aab`
- [ ] Release notes added
- [ ] Country/region availability reviewed

## 5) App Content and Policy Declarations

Complete all required forms:

- [ ] Privacy policy URL
- [ ] Data Safety form
- [ ] App Access (reviewer login instructions)
- [ ] Ads declaration
- [ ] Target audience
- [ ] Permissions declaration (if applicable)

## 6) Test Credentials for Google Review

Provide clear reviewer steps:

- [ ] Admin test login steps
- [ ] OTP test login steps (teacher/parent via email OTP)
- [ ] If OTP is restricted, provide whitelisted reviewer account details

## 7) Release Readiness QA (Must Pass)

- [ ] Login works (admin password + email OTP flows)
- [ ] Dashboard loads for Admin/Teacher/Parent
- [ ] Student CRUD works
- [ ] Photo upload works
- [ ] Fee create/collection works
- [ ] Leads and Notes Tracker work
- [ ] Template Zone works and WhatsApp/share link works
- [ ] Session-specific data filtering works
- [ ] Logout/login persistence works after app restart

## 8) Post-Release Verification

After production rollout:

- [ ] Verify API calls from Android app hit production endpoints only
- [ ] Check App Service logs for exceptions (first 24 hours)
- [ ] Track crash and ANR metrics in Play Console
- [ ] Monitor OTP delivery latency and failures

## 9) Every Subsequent Release

- [ ] Increment `versionCode` (mandatory)
- [ ] Update `versionName`
- [ ] Repeat QA checklist
- [ ] Promote gradually (not 100% instantly)
