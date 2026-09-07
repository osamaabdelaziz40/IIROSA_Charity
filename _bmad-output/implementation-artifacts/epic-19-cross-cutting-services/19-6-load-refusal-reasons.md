# Story 19-6: Load refusal reasons

| Field | Value |
| --- | --- |
| Story key | `19-6-load-refusal-reasons` |
| Epic | EP-19 — Cross-Cutting Services — الخدمات المشتركة |
| Use case | UC-SYS-06 — أسباب الرفض |
| Priority / size | Should · 8 points |
| Specification | `docs/Modules/24-UC-SYS-Cross-Cutting-Services.md` (§24.U.6 scenario) |
| Route | consumed by the periodic-report review flow (`#/periodic-orphan-reports/...`, 9-7/9-8) |
| Endpoint | `GET /api/LookupManagement/refuse-reasons` (board shorthand `general-reasons` — ruling below) |
| Depends on | 9-8 (built the catalogue + endpoint + seeding); 9-7/9-8 review flow is the consumer |
| Roles | Gen. Director / Staff → `SuperAdmin`, `Admin` (+ `Accountant` per `PeriodicReports.Review`) |

## Status

done

## Story

As a General Director, I want to be able to load refusal reasons أسباب الرفض, so that head
office keeps control of what is accepted into the sponsorship cycle.

## Acceptance Criteria

1. Given a reviewer with an active session in the review flow, when a report is refused, then
   the refusal carries the chosen reason, the deciding user and the decision date, and the item
   moves out of the pending queue.
2. Given the request is accepted, when it is served, then it is handled by the reason
   catalogue read on `GET /api/LookupManagement/refuse-reasons` and the response is rendered on
   the screen without a page reload.
3. Given the decision is a refusal, when no reason is given, then the refusal is not accepted —
   the reason is mandatory on the refuse path (validator-level, server-side).
4. Given the decision is recorded, when the charity opens the item, then it sees the new state
   and, on refusal, the reason.
5. Given the session has expired or the role is not permitted, when the function is invoked,
   then the request is rejected and the actor is routed back to the login screen.

> **Endpoint ruling (recorded deviation):** the board names
> `GET /api/LookupManagement/general-reasons` with a reason-Type filter. The live platform
> contract is **`GET /api/LookupManagement/refuse-reasons`** (entity `RefuseReason`, shipped by
> 9-8 with seeding and consumed by the 9-7/9-8 review flow). Adding a parallel
> `general-reasons` route would be a duplicate capability (PRD §7 non-goal) — keep
> `refuse-reasons`. The `RefuseReason` entity carries **no Type discriminator**; the legacy
> "reason type" filter is superseded by the single refusal catalogue (only the periodic-report
> refusal flow consumes reasons on this stack). Record both rulings; correct-course only if a
> second reason family (e.g. orphan-exclusion reasons, epic-8's `childExcludeResons` note)
   later needs the discriminator.

**Definition of done:** the §24.U.6 scenario passes end to end on the live 9-7/9-8 flow; the
reason is provably mandatory on refusal; the frontend service section mis-filings are fixed;
the endpoint rulings are recorded.

## What exists already (built by epic 9 — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Entity | `IIROSA.Domain/Entities/Lookups/RefuseReason.cs` — `LookupEntity`, LOOKUP_SCHEMA, seeded by 9-8 | Live |
| Endpoint | `LookupManagementController` `GET refuse-reasons` → `_refuseReasonService.GetLookupItemsAsync(filter)` → `List<RefuseReasonDto>` | Live |
| Review flow | 9-7/9-8: `POST /api/PeriodicOrphanReports/{id}/review` stamps Approved / Refused + reason, deciding user + date | Live (epic-9 board: review) |
| Frontend | `lookup-management.service.ts` `getRefuseReasons()` → `/refuse-reasons`; review screen reason dropdown | Live — with two recorded defects below |

## Verified defects this story must fix (from the epic-7 review's shared-file findings + audit)

1. **Frontend service hygiene (epic-7 review, deferred to this epic):**
   `lookup-management.service.ts` — `getRefuseReasons` is filed under the UC-HOU-03 section
   header (mis-filed); `getHousingFlats(NaN)` throws synchronously when
   `buildingId` is undefined, bypassing `catchError`. Fix both: move the method under the
   refusal/review section; guard `getHousingFlats` (return `of([])` / throw through the
   observable on a non-numeric id).
2. **Reason-mandatory verification (AC 3).** Audit the refuse path in
   `PeriodicOrphanReportService` (9-8's validator): a Refused decision with an empty reason
   must 400 with a field error naming the reason control. If the validator already enforces
   it, record the evidence; if not, patch the validator (server-side, service layer).
3. **Charity visibility of the reason (AC 4).** Verify the charity-facing report read carries
   the refusal reason (9-13's reason column exists per epic-9 review note F7 — confirm the
   list/detail DTO includes it for Charity-role callers within scope).

## Tasks / Subtasks

- [x] **Task 1 — Service hygiene** (AC 2)
  - [x] `lookup-management.service.ts`: re-filed `getRefuseReasons`; guard `getHousingFlats`
        against non-numeric input (observable error, not a synchronous throw)
- [x] **Task 2 — Reason-mandatory audit/patch** (AC 1, 3)
  - [x] Read the 9-8 refuse validator + review endpoint; enforce/verify reason-required on
        Refused; record the validator path in the story notes
- [x] **Task 3 — Charity-side reason surfacing** (AC 4)
  - [x] Verified — no patch needed exposes the refusal reason to in-scope Charity
        callers; patch the DTO/projection only if missing
- [x] **Task 4 — Verification** (AC 5)
  - [x] Builds green; live smoke on the private port: `refuse-reasons` → 200 seeded rows,
        401 unauthenticated; refuse-without-reason → 400 naming the field (if a review record
        is available to refuse). Tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

- **No `general-reasons` fork** — `refuse-reasons` is the single reason catalogue (ruling
  above); no Type column is added speculatively.
- Raw envelope on this controller (2026-08-19 standing decision); `LookupEntity` rules;
  `NameAr ?? NameEn` labels; reads are global reference data.
- The 8-point size reflects the legacy implementation's breadth; on this stack the heavy lift
  already shipped with 9-8 — this story is the audit-and-close pass. Do not inflate scope.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| The review decision flow itself | 9-7/9-8 (epic 9, review) |
| Lookup sweep (caps, export gating) | 19-4 |
| Orphan-exclusion reason family | deferred (epic-8 note) — correct-course if the business asks |

### References

- [Source: docs/Modules/24-UC-SYS-Cross-Cutting-Services.md#24.U.6] scenario + §24.2 UC-SYS-06 row
- [Source: _bmad-output/planning-artifacts/epics.md#3.19] US-SYS-06 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/LookupManagementController.cs] `refuse-reasons` action
- [Source: _bmad-output/implementation-artifacts/deferred-work.md] epic-7 review: `getRefuseReasons` mis-filed + `getHousingFlats(NaN)` synchronous throw
- [Source: _bmad-output/implementation-artifacts/sprint-status.yaml] epic-9 context note — 9-8 shipped RefuseReason + endpoint + seeding

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- Live proof (61970): `POST /api/PeriodicOrphanReports/{id}/review` with `{"isApproved":false}`
  and no reason → **400** `{"errors":{"RefuseReason":["Refuse reason is required when
  rejecting"]}}` (+ProblemDetails framing) — fired at model binding by the DTO's
  `[RequiredWhen]` annotation, before existence checks. Same call with `isApproved:true` → 404
  not-found (acceptance needs no reason; flow reaches the existence check).
- `refuse-reasons` endpoint live-verified during the 19-4 battery (200, 6 seeded rows,
  first الصورة لا تطابق اليتيم); class-level JWT → 401 unauth.

### Completion Notes List

- **Task 2 evidence (reason mandatory, server-side, two layers):** ①
  `ReviewPeriodicReportDto.cs:22` `[RequiredWhen(IsApproved, false)]` — model-binding 400 naming
  `RefuseReason` (the layer the live proof hit); ②
  `ReviewPeriodicReportValidator.cs:19-26` — service-layer backstop: `RefuseReasonId` required
  when refusing without text, `RefuseReason` (≤500) when without id, invoked
  `ValidateAndThrowAsync` at `PeriodicOrphanReportService.cs:519`; the controller's
  FluentValidation catch additionally maps it to `{ message, errors }`. Belt-and-braces — no
  patch needed.
- **Task 3 evidence (charity sees the reason):** `PeriodicOrphanReportDto` carries
  `RefuseReason`/`RefuseReasonId`/`RefuseReasonName` (:139-141); the list projection maps both
  fields (:856-867); `ApplyCallerScope` pins rows to `r.CharityId` (:933-934) — scoping filters
  rows, never columns; charity-facing screens render it (refused-state extract column, detail
  `refuseReasonName || refuseReason`). 9-13's reason column confirmed in the templates.
- **Task 1:** `getRefuseReasons` re-filed under a new "PERIODIC-REPORT REVIEW (UC-ORR-08 /
  UC-SYS-06)" section (no longer under the UC-HOU-03 header); `getHousingFlats` now guards
  non-numeric/undefined `buildingId` with `throwError(() => …)` through the observable —
  `catchError`/subscribers see it instead of a synchronous NaN throw. tsc clean.

### File List

- Frontend/src/app/modules/lookup-management/services/lookup-management.service.ts (getRefuseReasons re-filed; getHousingFlats guard)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-25 | Story file created from `epics.md` US-SYS-06 and module spec §24.U.6; resolved to an audit-and-close pass over 9-8's live catalogue (endpoint + Type-filter rulings recorded); assigned the epic-7 frontend service-hygiene deferrals. |
| 2026-08-25 | Implemented + verified (review-and-complete pass): service hygiene fixed (re-file + observable guard); reason-mandatory proven live as a two-layer server-side control (DTO annotation + service validator); charity-side reason visibility verified through DTO/projection/scoping/UI evidence — no backend patch required. |
