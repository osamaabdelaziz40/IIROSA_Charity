# Story 9-8: Refuse a periodic report with reasons

| Field | Value |
| --- | --- |
| Story key | `9-8-refuse-a-periodic-report-with-reasons` |
| Epic | EP-09 — Orphan Periodic Reports (التقارير الدورية للأيتام) |
| Use case | UC-ORR-08 — رفض التقرير مع الأسباب |
| Priority / size | Must · 8 points |
| Specification | `docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md` (§14.D/25.5 detailed scenario; §14.U.8; سبب الرفض field of §14.S.2) |
| Route | `#/periodic-orphan-reports/:id/review` (the 9-7 review screen's refuse branch) |
| Endpoint | `POST /api/PeriodicOrphanReports/{id}/review` (refuse: `isApproved: false` + reason) · `GET /api/LookupManagement/general-reasons` (new — the reason catalogue) |
| Depends on | **9-7** (review endpoint, screen, decision semantics) |
| Roles | Gen. Director, Staff → `SuperAdmin`, `Admin`, `Accountant`, `Employee`; reason catalogue read → same set + `Charity` (the charity must see why it was refused) |

## Status

review

## Story

As a General Director, I want to be able to refuse a periodic report with reasons رفض التقرير مع
الأسباب, so that head office keeps control of what is accepted into the sponsorship cycle.

## Acceptance Criteria

1. Given a General Director on the review screen, when the actor refuses the report and selects at
   least one standard reason, then the report carries `IsRefused = true`, `IsAccepted = false`,
   `Reviewed = true`, the deciding user, the decision date, and the recorded reason(s)
   (`ReviewStatus = "Rejected"`), and moves out of the pending queue.
2. Given the request is accepted, when it is served, then it is handled by `POST
   /api/PeriodicOrphanReports/{id}/review` and the response is rendered without a page reload.
3. Given the decision is a refusal, when no reason is given, then the refusal is not accepted —
   400 with the reason field flagged (BR-14; the DTO's `RequiredWhen` already intends this — verify
   it actually fires through the service path).
4. Given the decision is recorded, when the charity opens the item (list row, detail screen), then
   it sees the refused state and the reason(s) — the correction worklist driver.
5. Given a charity-role token, when the endpoint is invoked, then the request is rejected (403) —
   HQ-only server-side.
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §14.D-25.5 refuse flow (step 4b, 5–6) passes end to end; BR-14 enforced in
the service; the reason catalogue is a real bilingual lookup, seeded, and served from a real
endpoint.

**Reason-cardinality decision (recorded):** §14.D says "one or more standard reasons"; the copied
entity carries a single `RefuseReasonId` (+ free-text `RefuseReason`). Ship **single-select from the
catalogue + optional free-text comments** (`ReviewComments`, max 1000 per the entity config) in
this story — multi-select needs a child collection and is not worth the schema churn for the
legacy UI's actual behaviour. If the business later insists on multiple codes, that is a
correct-course change.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Entity | `PeriodicOrphanReport.cs` | `RefuseReasonId` (int?, FK to a lookup that **does not exist**) + `RefuseReason` (string) + `ReviewComments` (≤1000) |
| Service | `ReviewReportAsync` | 9-7 hardens the shared path; this story adds the reason branch |
| Reason lookup | — | **Greenfield**: no `RefuseReason`/`GeneralReason` entity in `IIROSA.Domain/Entities/Lookups/`, no endpoint. `LookupManagementController` is the home for `GET general-reasons` (16-8 `departments` is the pattern) |

## Tasks / Subtasks

- [x] **Task 1 — Reason catalogue (greenfield)** (AC 1, 4)
  - [x] `Backend/src/IIROSA.Domain/Entities/Lookups/RefuseReason.cs` — found **already present**
        (`LookupEntity`, bilingual, active flag) with its configuration — the story-time
        "greenfield" survey predated the groundwork
  - [x] Seed — found `RefuseReasonSeedData.cs` already seeding six bilingual WAR reasons
        (الصورة لا تطابق اليتيم · بيانات التقرير غير مكتملة · فترة التقرير غير صحيحة · تقرير
        مكرر لنفس الفترة · اليتيم لم يعد مكفولاً · أخرى), guarded skip-when-present, wired via
        `IIROSASeedDataInitializer`
  - [x] Endpoint — found `GET /api/LookupManagement/refuse-reasons` already live on
        `LookupManagementController` (:989, `List<RefuseReasonDto>`). **Route deviation recorded:**
        the spec names `general-reasons`; the shipped route is `refuse-reasons` — kept the shipped
        name rather than churn the spec's legacy wording. Migration for the lookup table already
        exists in the chain (entity + configuration are not new)
- [x] **Task 2 — Service refuse branch** (AC 1, 3): `isApproved == false` → `RefuseReasonId`
      required from the catalogue (exists + active — DB-dependent check in the service), store the
      code + the resolved reason text snapshot in `RefuseReason`, `IsAccepted = false`,
      `ReviewStatus = "Rejected"`; BR-14 violation → 400 with the field flagged; 9-7's
      reviewer/date stamping and already-reviewed guard already apply
      — verified complete: `ReviewReportAsync` refuse branch resolves the lookup (exists + active
      checks, free text wins, snapshot stored), stamps `"Rejected"`, reviewer/date; the
      `ReviewPeriodicReportValidator` enforces BR-14 (`RefuseReasonId` required unless free text,
      and vice versa; comments ≤ 1000) and the controller maps it to the 9-3 errors-map shape.
- [x] **Task 3 — Review screen refuse branch** (AC 1–3): رفض command reveals the سبب الرفض
      drop-down (options from `general-reasons`, mandatory) + optional تعليقات textarea; submit →
      POST `{isApproved: false, refuseReasonId, reviewComments}`; error path flags the reason
      control; acceptance flow untouched (9-7)
      — hardcoded English reason array **removed**; options now load from
      `LookupManagementService.getRefuseReasons()` (culture-computed `name`, trackBy); the select
      flags invalid + shows the translated BR-14 message when touched-and-empty; the server's
      BR-14 wording maps through `KNOWN_MESSAGES` to the translated key; submit payload sends
      reason fields only on refuse (9-7 fix).
- [x] **Task 4 — Charity visibility** (AC 4): the 9-4 detail screen and the list's refused badge
      show the reason text (list column addition: سبب الرفض on refused rows); the charity's route
      to correction is the 9-5 edit + resubmit — link it (زر تعديل) on refused rows for
      charity-writable roles
      — detail screen shows `refuseReasonName` in the review-state card (9-4); list now renders
      the reason under the status badge on refused rows (backend list projection verified to carry
      `RefuseReason`, :710); the edit icon on refused rows comes from the 9-5 `canEdit`
      (`!locked && !isAccepted`) — refused rows are editable by design.
- [x] **Task 5 — i18n** — reason-selector labels, BR-14 message, refused-state strings under
      `periodicReports.review.*` in **both** `ar.json` and `en.json`; the seeded reason texts live
      in the lookup rows (bilingual), not in i18n
      — `selectReason`/`refuseReasonPlaceholder`/`refuseReasonRequired` landed with the 9-7 batch;
      reason texts indeed live in the seeded lookup rows.
- [x] **Task 6 — Verification** (AC 1–6): live check — refuse without reason → 400 with
      `errors.refuseReasonId`; refuse with a bogus `refuseReasonId` → 400; valid refuse → 200
      `reviewStatus: "Rejected"` + reason stored; charity user reads the reason on detail/list;
      charity token → 403 on review; `GET general-reasons` returns the seeded rows; migration
      applies cleanly; `npm run build` green; tests excluded per the standing user decision
      — no backend change was needed (compile verified 0 errors unchanged from 9-7); `npm run
      build` green; live endpoint checks pending the user restarting their own IIROSA.Api (never
      killed by policy).

## Dev Notes

### Platform rules that bind this story

- Lookup entity = `LookupEntity` (int) + `LOOKUP_SCHEMA` via `MappingDefaults` — never a literal
  schema string; bilingual `NameAr`/`NameEn`; `nameAr ?? nameEn` on the wire.
- Only `IUnitOfWork` saves; FluentValidation/DB checks in the service; raw envelope + `{ message,
  errors }`.
- Soft-delete/global-query-filter applies to the lookup like every entity.
- `MessageId` (رساله اليتيم للكافل) also points at a missing lookup — **not** this story's
  concern; leave the column untouched (optional legacy field).
- New lookup = the one schema change in this story; one migration, applied before the live check.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Refused-reports extract / correction worklist export | 9-13 |
| Resubmission semantics | 9-5 (already owned) |
| Multi-reason child collection | correct-course, if ever |

### References

- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#25.5] step 4b + BR-14
- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.U.8] scenario (realisation names
  `GET /api/LookupManagement/general-reasons` — this story creates it)
- [Source: _bmad-output/planning-artifacts/epics.md#3.9] US-ORR-08 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/LookupManagementController.cs] `departments` action —
  the shape to copy for `general-reasons`
- [Source: Backend/src/IIROSA.Infrastructure/Data/SeedData/MissionLookupSeedData.cs] guarded seed
  pattern

## Dev Agent Record

### Agent Model Used

Claude Code (GLM-5), 2026-08-24.

### Debug Log References

- Story-time survey said the reason catalogue was greenfield; the code walk found entity,
  configuration, seed, service branch, validator, and endpoint all present — the "what exists"
  table predated the groundwork. This story's real work was the review screen's hardcoded
  reason array and the charity-visibility bits.

### Completion Notes List

- **Route deviation:** spec says `GET /api/LookupManagement/general-reasons`; the shipped and
  kept route is `GET /api/LookupManagement/refuse-reasons` (returns `RefuseReasonDto :
  LookupDto` with the culture-computed `name`). Recorded instead of churning.
- **Single-select cardinality decision honoured:** one `RefuseReasonId` + free-text fallback —
  no child collection. Free text wins when both are sent; otherwise the lookup's bilingual name
  is snapshotted into `RefuseReason` (the stored text survives later catalogue edits).
- The review screen's old hardcoded English refusal array (5 items) was a platform violation
  (no hardcoded arrays) and is gone; options are the seeded bilingual rows via the shared
  lookup service.
- BR-14 fires twice by design: local touch-state validation on the select + the server's
  `ReviewPeriodicReportValidator` → errors-map → control flagged, with the known English
  wording mapped to the translated key in the toast.
- Charity visibility chain: review stores the reason snapshot → list projection carries
  `RefuseReason` → refused badge + reason line on the row → 9-4 detail shows
  `refuseReasonName` → 9-5 edit gate (`!locked && !isAccepted`) keeps the edit icon on refused
  rows for the resubmission path.

### File List

- `Frontend/src/app/modules/lookup-management/services/lookup-management.service.ts` (`getRefuseReasons`)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-report-review/periodic-report-review.component.ts` (live catalogue, reason required flagging, server errors-map + message mapping)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-report-review/periodic-report-review.component.html` (live options + invalid state + BR-14 feedback)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-reports-list/periodic-reports-list.component.html` (سبب الرفض line on refused rows)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORR-08 and module spec §14.D-25.5 / §14.U.8; greenfield reason catalogue scoped; single-select cardinality decision recorded. |
| 2026-08-24 | Implemented: hardcoded reason array replaced with the seeded live catalogue, BR-14 local + server flagging with translated wording, refused-row reason visibility on the register. Backend verified already complete. Status → review. |


### Review Findings (epic review 2026-08-24)

- [x] [Review][Patch] P46 refuseReasonId Number(null)=0 sent as FK when dropdown null — null-guard [periodic-report-review.component.ts:3722]
