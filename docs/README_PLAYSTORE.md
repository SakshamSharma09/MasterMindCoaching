# Play Store Release Guide

Use this as the single entry point for Android Play Store release work.

## Documents

- Primary execution checklist: [PLAYSTORE_RELEASE_CHECKLIST.md](./PLAYSTORE_RELEASE_CHECKLIST.md)
- Root project context: [../README.md](../README.md)

## Current Mobile Baseline (Already Implemented)

- Capacitor Android project is configured in `src/frontend/mastermind-web/android`.
- App identifier: `com.mastermind.coaching`.
- App name: `MasterMind Coaching`.
- Branded launcher icon is integrated into the Android project.
- Play Console high-resolution icon is available at `docs/playstore/assets/mastermind-play-icon-512.png`.
- Target/compile SDK: 35.
- Release build hardening:
  - release minification and resource shrinking enabled
  - cleartext traffic blocked
  - Android backup/data extraction disabled for security
- Auth UX aligned for production:
  - email-only OTP flow for non-admin users
  - no quick demo login panel in primary login flow

## What You Need To Provide

1. Play Console app setup completion (after account verification).
2. Final brand assets:
   - 512x512 Play icon: `docs/playstore/assets/mastermind-play-icon-512.png`
   - feature graphic (1024x500)
   - screenshots (phone; optional tablet)
3. Privacy policy URL (public).
4. App signing details:
   - Keystore file (local only, never commit)
   - Keystore alias/passwords
5. Reviewer test instructions and test accounts.

## What We Can Do In Code (Any Time)

1. App icon + splash integration in Android project.
2. Version bump (`versionCode` / `versionName`) for each release.
3. Mobile UX polish fixes based on internal testing feedback.
4. Release validation pass before each upload.

## Fastest Way To Get `.aab` (Windows)

From `src/frontend/mastermind-web`:

```bash
npm run build:aab
```

Generated file:

- `android/app/build/outputs/bundle/release/app-release.aab`

## Internal Testing Notes

After an internal release is marked available, testers usually need the internal testing opt-in link from Play Console. Adding emails to the tester list controls who can join the test, but sharing the opt-in link is the reliable way to get them into the release. Ask testers to open the link with the same Google account that was added to the tester list, accept the invitation, then install from Google Play.

Every uploaded AAB must use a new Android `versionCode`. The current project version is:

- `versionCode`: `7`
- `versionName`: `1.0.6`

If you prefer Android Studio:

1. `npm run build:mobile`
2. `npm run cap:open`
3. Build -> Generate Signed Bundle / APK -> Android App Bundle
