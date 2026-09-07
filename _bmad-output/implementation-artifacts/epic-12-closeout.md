# Epic 12 — Seasonal Assistance Projects: closeout

All 13 stories (UC-PRJ-01..13) `done` as of 2026-08-19, after a review-and-complete pass over the
copied implementation. Backend compiles with 0 errors; `npm run build` succeeds (two advisory
warnings: distribution-record SCSS budget, exceljs CommonJS). Only story 12-7 has a story file —
this note plus `12-7-register-families-for-a-project.md` are the epic record. Nothing committed.

Spec: `docs/Modules/17-UC-PRJ-Seasonal-Assistance-Projects.md` (§17.S screens, §17.U scenarios,
§17.D 25.9 BR-23/24/25).

## What the review found and fixed, by story

Only the deltas from the copied code are listed; everything else was verified as already correct.

- **12-1..12-5 (campaign CRUD)** — the frontend models/service spoke invented wire keys that
  matched no backend DTO (`beneficiaryIds` vs `familyIds`, tuple unwrapping, status/budget fields),
  so every screen bound nothing. Models rewritten 1:1 against the real DTOs; list/form/detail
  reworked onto server-side paging/filtering, cascading country→region→center lookups, wire
  validators, i18n-only strings, ExcelJS export of the list.
- **12-6 / 12-9 (selection screen reads)** — beneficiary grid now reads the real
  `GET campaigns/{id}/beneficiaries` with `{items,totalCount}` unwrap, distributed/pending filter,
  pagination, `trackBy`.
- **12-7 (register families)** — see the story file. New `PUT campaigns/{campaignId}/beneficiaries`
  full-sync endpoint + service + validator; BR-23 now a filtered unique index
  (`IX_SeasonalAidBeneficiary_CampaignId_FamilyId WHERE IsDeleted = 0`, migration
  `20260819201729_FixSeasonalAidBeneficiaries`, applied); the re-declared
  `SeasonalAidCampaign.IsDeleted = true` (every campaign born soft-deleted) removed with a data
  repair; charity ownership (BR-24) and quota (A1) enforced server-side.
- **12-8 (confirm family received)** — new `PUT /api/Families/{id}/received-flag` +
  `SetFamilyReceivedFlagAsync`; toggle on the registered grid; acting user resolved from the token
  server-side, never the client.
- **12-10 (list non-registered families)** — new `eligible-families` component + route on
  `GET campaigns/{id}/eligible-families` (the old client called an endpoint that never existed).
- **12-11 / 12-12 (summary + report)** — new `campaign-report` component on
  `GET campaigns/{id}/report`: stat cards, budget utilisation, by-region/charity/family-type
  breakdowns, distribution details, `window.print()` view and ExcelJS export. The controller's
  pdf/excel endpoints remain NotImplemented and are not called.
- **12-13 (print distribution documents)** — printable RTL Arabic receipt (إيذون استلام) per
  distribution from the distribution-record screen: campaign, family, date, amount, recipient +
  signature lines, via a print window. jsPDF is not a dependency; server pdf export is not
  implemented — recorded, not silently faked.

Cross-cutting: controller authorisation restructured per-action (family-facing surface admits
`Charity`; management/report stay `SuperAdmin,Admin`); `ICurrentUserService` scoping added;
`PERMISSION_ROLES` + route `data.permission` + sidebar menu aligned to those sets; ~55 new
`seasonalAid.*` i18n keys in both languages; `tsconfig.app.json` `"types": ["node"]` so exceljs
typings resolve.

## Deliberately not modelled (recorded decisions)

- **Primary/secondary distribution lists** (الكشف الأساسي / الكشف الإضافي, §17.S.3) — the
  re-platform entity has a single beneficiary list with `IsRegistered`/`IsDistributed`; no AC
  references the two lists. If a future story needs them, add a list-type column then.
- **Server-side pdf/excel export endpoints** stay NotImplemented; the client prints/exports itself.
- Server error strings stay English (platform-wide, `deferred-work.md`); the frontend maps failures
  to i18n keys.
- Tests excluded by standing user decision (epics 3 & 13 precedent).

## Outstanding before release

1. **Live Charity-user walkthrough** — the one verification gate not run this session (no Charity
   credentials; the API was running under the user's VS debug session). Script is in the 12-7 story
   file, Task 8.
2. **Pre-existing model drift** — the EF diff still contains ~50 columns of unrelated drift
   (PeriodicOrphanReport rename guesses that would corrupt data, ApplicationUser duplicate-table
   adds). The seasonal-aid migration was hand-trimmed to exclude it; whoever migrates next must
   trim likewise or fix the model first. Recorded in the migration's doc comment.
3. **Identity migrations pending** (see `epic-3-closeout.md` — unchanged by this epic).
4. Seeded `Charity@IIROSA.com` tenancy and `ngx-bootstrap`/`--legacy-peer-deps` — unchanged,
   still open items from epic 3.
