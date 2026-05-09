# Play Store Release Guide

Use this as the single entry point for Android Play Store release work.

## Documents

- Primary execution checklist: [PLAYSTORE_RELEASE_CHECKLIST.md](./PLAYSTORE_RELEASE_CHECKLIST.md)
- Root project context: [../README.md](../README.md)

## Current Mobile Baseline (Already Implemented)

- Capacitor Android project is configured in `src/frontend/mastermind-web/android`.
- App identifier: `com.mastermind.coaching`.
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
   - 512x512 Play icon
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
