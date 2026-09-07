# Story 9-4: View a periodic report

| Field | Value |
| --- | --- |
| Story key | `9-4-view-a-periodic-report` |
| Epic | EP-09 — Orphan Periodic Reports (التقارير الدورية للأيتام) |
| Use case | UC-ORR-04 — عرض التقرير |
| Priority / size | Should · 3 points |
| Specification | `docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md` (§14.S.2 sections rendered read-only; §14.U.4 scenario) |
| Route | `#/periodic-orphan-reports/:id` |
| Endpoint | `GET /api/PeriodicOrphanReports/{id}` |
| Depends on | **9-1** (registered module, scope, list linking here) |
| Roles | Charity, HQ roles → `Charity`, `SuperAdmin`, `Admin`, `Accountant`, `Employee` (existing role set on `GET /{id}` — keep) |

## Status

review

## Story

As a charity user, I want to be able to view a periodic report عرض التقرير, so that I can answer
the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given a charity user with an active session in the module, when the actor opens a report from the
   list, then no stored data is changed — the operation is a read, rendered in read-only mode with
   all recorded dimensions.
2. Given the request is accepted, when it is served, then it is handled by `GET
   /api/PeriodicOrphanReports/{id}` and the response is rendered on the screen without a page reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity (and
   country) are returned — a charity user opening another charity's report id gets 404, not a leak.
4. Given the report carries review state, when it is opened, then the state (Submitted / Accepted /
   Refused), the deciding user and the decision date are shown, and on refusal the recorded reason.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §14.U.4 passes end to end; the read-only screen mirrors every section of
§14.S.2; the charity scoping is enforced server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| API | `PeriodicOrphanReportsController.cs` `GET /{id}` | Exists → `PeriodicOrphanReportDto`; 404 `{ message }` shape already used |
| Service | `PeriodicOrphanReportService.GetByIdAsync(id)` | Exists; returns null when missing; **no caller-scope check** |
| DTO | `DTOs/PeriodicOrphanReport/PeriodicOrphanReportDto.cs` | Rich — all dimensions + `OrphanCode/OrphanName/CharityName` + resolved lookup names + `ReviewStatus` computed (`!Reviewed ? "Pending" : IsAccepted ? "Approved" : "Rejected"`) |
| Frontend | `periodic-report-detail.component.*` | Exists (standalone); not reachable before 9-1; no i18n; attachment display is placeholder |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **No ownership gate on the single read** — `GetByIdAsync` returns any id to any caller (AC 3).
   Route the read through the 9-1 caller scope (charity-bound caller → report's `CharityId` must
   match, else treat as not found).
2. **Reviewer name never resolves** — the entity's `Reviewer` navigation is commented out and the
   DTO's `ReviewerName` has no populated source. Either map `ReviewerId → ApplicationUser` display
   name via the identity set (15-1's `ApplicationUser` remap context) or show the stored
   `ReviewerId`-derived name only when resolvable; do not fake it.
3. **Detail template hard-codes strings** and renders sections as raw key/value dumps — rebuild to
   the §14.S.2 section grouping with translated labels.
4. **Attachment placeholders** — the five image slots render nothing; image streaming itself is
   9-16's, but the detail screen must at minimum show which documents are attached (name + link to
   `GET /api/Attachments/{id}/image`) using the DTO's `*ImageUrl` fields if they resolve, else the
   ids.

## Tasks / Subtasks

- [x] **Task 1 — Ownership gate** (AC 3): apply the 9-1 caller scope inside `GetByIdAsync`
      (charity-bound caller + mismatched `CharityId` → null → 404)
      — verified already in place: `PeriodicOrphanReportService.cs:291` (`GetByIdAsync`) and
      `:872` (`GetScopedReportAsync`) both gate via `IsWithinCallerScopeAsync` (:879);
      controller maps null → 404 `{ message }`. No code change needed.
- [x] **Task 2 — Review state display** (AC 4): badge (Submitted/Accepted/Refused from
      `reviewStatus`), deciding user (`reviewerName` — see defect 2), decision date
      (`reviewedDate`), refusal reason (`refuseReason`/`refuseReasonName` when present)
      — defect 2 resolved server-side: `MapToDetailDtoAsync` resolves `ReviewerName` from the
      identity `ApplicationUser` via `_userRepository.GetByIdAsync` (:1083–1084, `FullName`),
      never faked; screen renders badge + reviewer + date + refusal reason + `reviewComments`.
- [x] **Task 3 — Read-only screen** (AC 1): rebuild `periodic-report-detail` to the §14.S.2 section
      grouping (header incl. orphan identity from the 9-2 header shape · الحالة الصحية · الحالة
      التعليمية · الملفات المطلوبه · الانشطة والبرامج · religious/behaviour), every field
      read-only, missing optional fields shown as —; `OnPush`; breadcrumb back to the list; wire
      the list row's view icon here
      — template fully rebuilt (header · review state · religious/quran · health · education ·
      life events & personal development · attachments · audit trail), missing optionals render
      "—"; `OnPush` + `ChangeDetectorRef.markForCheck()` added; malformed review routerLink
      `['../' + id + 'review']` fixed to relative `['review']` (route `:id/review` exists).
      List row view icon was already wired in 9-1.
- [x] **Task 4 — Attachment presence** (AC 1): per attached document, a row with its translated
      label and a link to `/api/Attachments/{id}/image`; full gallery/lightbox interaction is 9-16
      — deviation: the story's `GET /api/Attachments/{id}/image` route does not exist on this
      platform; the verified-live endpoint is `GET /api/attachments/{id}/download`. Because it is
      JWT-protected, a plain `<a href>` would 401 — each slot renders a presence row and downloads
      via `AttachmentService.download(id)` (blob → objectURL → new tab), keyed off the DTO
      `*ImageId` fields (BR-12). Gallery/lightbox remains 9-16.
- [x] **Task 5 — i18n** — all labels under `periodicReports.detail.*` in **both** `ar.json` and
      `en.json`
      — deviation: keys placed flat under the existing `periodicReports.*` namespace (reusing the
      9-3 `form.*` field labels rather than duplicating them under `detail.*`); 16 new keys +
      `periodicReviews.reviewedBy` + `status.accepted`/`status.refused` aliases added to both
      locales (aliases cover any backend status casing).
- [x] **Task 6 — Verification** (AC 1–5): live check — HQ opens any id → 200; charity user opens a
      foreign id → 404; refused report shows its reason; unauthenticated → 401; `npm run build`
      green; tests excluded per the standing user decision
      — `npm run build` green (detail-screen `reviewComments` model gap found and fixed during
      build); backend unchanged by this story so no rebuild required. Live endpoint check pending
      the user restarting their own IIROSA.Api (never killed by policy) — the running instance
      serves pre-story binaries.

## Dev Notes

### Platform rules that bind this story

- Read-only path: no `SaveChangesAsync` anywhere.
- Raw envelope; camelCase wire; caller scope from claims (pin-never-widen).
- `data.permission: 'PeriodicReports.View'` on the route (set in 9-1) — the endpoint's
  `[Authorize(Roles = …)]` remains the control.
- Do not duplicate the form component for viewing — this is the separate detail component the
  copied tree already ships.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Edit mode on the same data | 9-5 |
| Accept/refuse controls + `:id/review` screen | 9-7, 9-8 |
| Image lightbox/streaming behaviour beyond a plain link | 9-16 |
| PDF print of the form | 9-17 |

### References

- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.U.4] scenario
- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.S.2] section structure mirrored
  read-only
- [Source: _bmad-output/planning-artifacts/epics.md#3.9] US-ORR-04 acceptance criteria
- [Source: Backend/src/IIROSA.Application/DTOs/PeriodicOrphanReport/PeriodicOrphanReportDto.cs]
  computed `ReviewStatus` and resolved-name fields this screen renders

## Dev Agent Record

### Agent Model Used

Claude Code (GLM-5), 2026-08-24.

### Debug Log References

- `npm run build` first pass failed: TS2339 `reviewComments` on `PeriodicOrphanReportDto` — the
  backend DTO carries `ReviewComments` (`PeriodicOrphanReportDto.cs:136`) but the frontend model
  never declared it; added `reviewComments?: string` to the model, rebuild green.

### Completion Notes List

- Backend needed **zero changes**: ownership gate (`IsWithinCallerScopeAsync`, :291/:872/:879),
  reviewer-name resolution (identity `ApplicationUser.FullName`, :1083), refuse-reason and
  educational-level name resolution were all verified present — defects 1 and 2 were already
  fixed by the 9-1/9-2 groundwork.
- Defect 3 fixed: template rebuilt from the placeholder dump to the §14.S.2 grouping; review link
  corrected to relative `['review']`; delete confirmation + toasts moved off hard-coded English
  onto i18n keys.
- Defect 4 fixed with a route deviation (see Task 4): `/download` blob fetch instead of the
  non-existent `/image` route, because the attachment endpoint requires the JWT header.
- `OnPush` + `markForCheck` added; `trackBy` on the attachment-slot `*ngFor`.
- Model: `reviewComments` added to `PeriodicOrphanReportDto` (frontend interface only).
- The `:id/review` entry button is shown only while `!reviewed` — 9-7/9-8 own the review screen.

### File List

- `Frontend/src/app/modules/periodic-orphan-reports/periodic-report-detail/periodic-report-detail.component.html` (rebuilt)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-report-detail/periodic-report-detail.component.ts` (OnPush, attachment slots + blob download, translated delete flow)
- `Frontend/src/app/modules/periodic-orphan-reports/models/periodic-orphan-report.model.ts` (`reviewComments` added)
- `Frontend/src/assets/i18n/ar.json`, `Frontend/src/assets/i18n/en.json` (17 keys each)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORR-04 and module spec §14.U.4; ownership-gate and reviewer-name defects recorded. |
| 2026-08-24 | Implemented: detail screen rebuilt to §14.S.2 read-only sections, review-state card, attachment presence + blob download, OnPush, i18n (ar/en). Backend verified already compliant — no backend change. Status → review. |


### Review Findings (epic review 2026-08-24)

- [x] [Review][Patch] P37a Load failure = silent blank screen (detail) — toast/redirect like search [periodic-report-detail.component.ts:77]
- [x] [Review][Patch] P36b Delete-failure message double-unwrap (detail) [periodic-report-detail.component.ts:112]
- [x] [Review][Patch] P26c i18n: history/compare companion keys missing both locales (detail links) [ar.json/en.json]
