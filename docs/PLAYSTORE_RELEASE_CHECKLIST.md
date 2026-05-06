# Play Store Release Checklist (MasterMind Coaching)

This document lists the exact steps for building and submitting the Android app from the existing web codebase.

## 1) Build and Sync Mobile App

From:

`src/frontend/mastermind-web`

Run:

```bash
npm install
npm run build:mobile
npm run cap:open
```

## 2) Android Studio Setup

1. Open the generated Android project in Android Studio.
2. Wait for Gradle sync to complete.
3. Set app package id if needed:
   - Current: `com.mastermind.coaching`
4. Set app name, icon, and splash assets.

## 3) Create Signed App Bundle (.aab)

In Android Studio:

1. `Build` -> `Generate Signed Bundle / APK`
2. Select `Android App Bundle`
3. Create or select release keystore
4. Build release `.aab`

## 4) Play Console Submission Steps

1. Create app in Google Play Console
2. Fill store listing (title, short/long description, screenshots, icon, feature graphic)
3. Upload `.aab` to **Internal Testing** first
4. Complete policy sections:
   - App content
   - Data safety
   - Privacy policy URL
   - Ads declaration
   - Target audience
5. Fix warnings, then promote to closed/open/production track

## 5) Mandatory Approval TODOs

- [ ] Privacy Policy page publicly hosted (required)
- [ ] Data Safety form completed accurately
- [ ] App Access instructions provided (admin + OTP flows)
- [ ] Test account credentials documented for Google review
- [ ] Permission usage reviewed (camera/storage/network only if used)
- [ ] 64-bit support verified (default with modern Android builds)
- [ ] Target SDK updated to current Play requirement
- [ ] Version code/version name incremented before each upload
- [ ] Crash-free smoke test on real Android device
- [ ] Internal testing feedback pass completed

## 6) Post-Release Operational Checklist

- [ ] Verify API base URL points to production
- [ ] Verify login, OTP, student photo upload, fee creation, leads, template zone
- [ ] Verify session-specific data behavior
- [ ] Monitor App Service logs for first 24 hours

