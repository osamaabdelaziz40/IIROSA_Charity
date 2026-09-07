# Story 9-7: Accept a periodic report

| Field | Value |
| --- | --- |
| Story key | `9-7-accept-a-periodic-report` |
| Epic | EP-09 — Orphan Periodic Reports (التقارير الدورية للأيتام) |
| Use case | UC-ORR-07 — اعتماد التقرير |
| Priority / size | Must · 8 points |
| Specification | `docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md` (§14.D/25.5 detailed scenario; §14.U.7; review fields of §14.S.2) |
| Route | `#/periodic-orphan-reports/:id/review` |
| Endpoint | `POST /api/PeriodicOrphanReports/{id}/review` (spec's `PUT /api/PeriodicOrphanReports` is the legacy realisation — see Dev Notes) |
| Depends on | **9-1** (scope/permissions) · **9-4** (the reviewer examines content + attachments before deciding) |
| Roles | Gen. Director, Staff → `SuperAdmin`, `Admin`, `Accountant`, `Employee` (existing set on the review endpoint — keep). BR-15: only HQ roles may review — `Charity` must get 403 here |

## Status

review

## Story

As a General Director, I want to be able to accept a periodic report اعتماد التقرير, so that head
office keeps control of what is accepted into the sponsorship cycle.

## Acceptance Criteria

1. Given a General Director on the review screen of a submitted report, when the actor accepts it,
   then the report carries `IsAccepted = true`, `IsRefused = false`, `Reviewed = true`, the deciding
   user and the decision date, and moves out of the pending queue (`ReviewStatus = "Approved"`).
2. Given the request is accepted, when it is served, then it is handled by `POST
   /api/PeriodicOrphanReports/{id}/review` and the response is rendered without a page reload.
3. Given acceptance, when the orphan's reporting prerequisite is later evaluated (BR-11), then this
   accepted report satisfies it — no payment-side work here, only the state that payments will read.
4. Given the report is already reviewed, when the actor accepts again, then the request is refused
   with a clear message and nothing is written (re-deciding goes through the charity's resubmission,
   9-5).
5. Given a charity-role token, when the endpoint is invoked, then the request is rejected (403) —
   review is HQ-only, enforced server-side.
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §14.D-25.5 accept flow (steps 1–4a, 5) passes end to end; BR-13 (accepting
clears refusal) is enforced in the service; the decision audit trail (who, when) is recorded by the
service from claims, never from the payload.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| DTO | `DTOs/PeriodicOrphanReport/ReviewPeriodicReportDto.cs` | Exists — `ReportId` (required), `IsApproved` (required), `RefuseReasonId`?, `RefuseReason`?, `ReviewComments`?; conditional `RequiredWhen` attribute already refuses without a reason |
| Service | `PeriodicOrphanReportService.ReviewReportAsync(dto)` | Exists: validates not already reviewed, sets `Reviewed/ReviewedDate/IsAccepted/IsRefused/RefuseReason` via UoW |
| API | `POST /{id}/review` | Exists; roles already HQ-only |
| Frontend | `periodic-report-review.component.*` | Exists; route already carries `AuthGuard + PermissionGuard` (`PeriodicReports.Review`) — the only route in the module that does |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Reviewer identity not recorded.** `ReviewerId` is never set — the deciding user must come from
   the caller's claims (`ICurrentUserService`), never from the DTO (the spec's legacy
   `[FromUri] string userId` is exactly the trust-the-client defect to avoid).
2. **`ReviewStatus` string not stamped** — the stored column stays null while the DTO computes it
   on the fly; queries that filter on the stored value (9-9) would miss. Stamp `"Approved"`.
3. **`IsAccepted`/`IsRefused` not forced mutually exclusive on the stored row** — BR-13 requires
   accept to force `IsRefused = false` (and clear any stale `RefuseReason`); verify and enforce.
4. **`ReportId` duplication.** The DTO carries `ReportId` while the route also carries `{id}` —
   bind from the route, ignore/validate-against the body value (mismatch → 400).
5. **`CanUserReviewReportsAsync` is a `return true` stub** — either implement it against the role
   claim or delete it and rely on `[Authorize(Roles = …)]`; do not leave a dead stub guarding
   nothing (record the choice).
6. **Review screen is a stub** — rebuild it as: read-only rendering of the 9-4 detail sections +
   attachments, then اعتماد command, optional تعليقات field; the refusal branch's reason selector
   is 9-8's.

## Tasks / Subtasks

- [x] **Task 1 — Service decision path** (AC 1, 4): route-id binding (defect 4); claim-derived
      `ReviewerId` + `ReviewedDate = now` (defect 1); stamp `ReviewStatus` (defect 2); BR-13
      mutual exclusion + stale-reason clear (defect 3); already-reviewed → 400 message (AC 4);
      ownership/country gate per 9-1 scope
      — defects 1–4 verified already in place from the groundwork (reviewer from
      `_currentUser.UserId`, `"Approved"`/`"Rejected"` stamped, accept clears `IsRefused` +
      reason, controller `dto.ReportId = id` route-overrides the body). This story added the one
      missing rule: already-reviewed → `BusinessException` 400 (§25.5 — re-deciding goes through
      9-5 resubmission). Defect 5: `CanUserReviewReportsAsync` verified implemented against the
      caller's role claims (not a stub); kept — it backs `GET /can-review`.
- [x] **Task 2 — Review screen** (AC 1, 2): `:id/review` shows the report (reuse the 9-4 section
      rendering — extract or embed it, do not fork a third layout), اعتماد button → POST
      `{isApproved: true}` → toast → navigate to the pending list; disable the button while
      in-flight; show the current state badge
      — the shipped screen already followed the 9-4 section grouping (not a third layout); this
      story added: OnPush + `markForCheck`, success/error toasts, in-flight spinner + disabled
      decision buttons, reviewed-state badge + no-re-decide notice, and fixed a live payload
      defect — the old code sent `refuseReasonId: ""` on **accept**, which fails `int?` model
      binding; the payload is now built conditionally (reason fields only on refuse, empty
      optionals omitted).
- [x] **Task 3 — Pending queue reachability**: the reviewer arrives from a list filtered to
      `reviewStatus: "Pending"` — add a حالة الاعتماد filter value set (Pending/Accepted/Refused/
      All) to the 9-1 list filter if not already present, and a "review" action on pending rows
      permission-gated to `PeriodicReports.Review`
      — backend `ApplyFilters` already maps `reviewStatus` (pending/approved/rejected,
      case-insensitive); added the حالة الاعتماد select (الكل/قيد الانتظار/معتمد/مرفوض) to the
      9-1 filter bar; the review action on pending rows (`!reviewed && PeriodicReports.Review`)
      was already wired in 9-1.
- [x] **Task 4 — i18n** — review labels/messages under `periodicReports.review.*` in **both**
      `ar.json` and `en.json`
      — `periodicReports.review.*` (4 keys) + 47 screen keys (sections, fields, actions,
      placeholders — field labels mirrored from the 9-3 `form.*` values) + `filters.status`/
      `filters.all` added to both locales.
- [x] **Task 5 — Verification** (AC 1–6): live check — HQ accept → 200 with
      `isAccepted: true, isRefused: false, reviewed: true, reviewerId` = caller, `reviewStatus:
      "Approved"`; second accept → 400; charity token → 403; refused-then-resubmitted (9-5) report
      is acceptable; unauthenticated → 401; `npm run build` green; tests excluded per the standing
      user decision
      — `dotnet build` Application project 0 errors; `npm run build` green; live endpoint checks
      pending the user restarting their own IIROSA.Api (never killed by policy).

## Dev Notes

### Endpoint decision (recorded):** §14.U.7's realisation line says `PUT /api/PeriodicOrphanReports`
carrying `IsAccepted` — the legacy form posted the whole report back with a checkbox flipped. The
copied platform already has the superior dedicated action `POST /{id}/review` with a typed
`ReviewPeriodicReportDto`, and the frontend service calls it. Keep the review endpoint; do not
fold review into the update path (that would let any update-capable role sneak a decision — the
role sets differ: PUT is Charity-writable, review is HQ-only).

### Platform rules that bind this story

- Only `IUnitOfWork` saves; audit via interceptor; claims-only identity.
- Raw envelope + `{ message }` errors; camelCase wire.
- `data.permission: 'PeriodicReports.Review'` (already on the route) + endpoint roles = the two
  layers of the control; the menu/list action merely points at them.
- Accept and refuse share one endpoint and one DTO (`IsApproved` discriminates) — build the accept
  branch fully here; 9-8 adds the reason catalogue and the refuse branch's UI. Do not split the
  endpoint across stories.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Refusal reasons catalogue + refuse UI (`RefuseReasonId` selection, BR-14) | 9-8 |
| Accepted-reports extract | 9-12 |
| Refused-reports correction worklist | 9-13 |
| Payment-prerequisite consumption of the accepted state (BR-11 read side) | EP-10 |

### References

- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#25.5] the joint review scenario
  (steps 1–4a, 5; BR-13/14/15)
- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.U.7] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.9] US-ORR-07 acceptance criteria
- [Source: Backend/src/IIROSA.Application/DTOs/PeriodicOrphanReport/ReviewPeriodicReportDto.cs]
  existing DTO incl. conditional reason requirement

## Dev Agent Record

### Agent Model Used

Claude Code (GLM-5), 2026-08-24.

### Debug Log References

- Backend compile verified via the Application project (0 errors) after adding the
  already-reviewed guard.
- The review screen's attachment section previously linked `*ImageUrl` fields, which
  `MapToDetailDtoAsync` nulls pending 9-16 — dead links. Replaced with the 9-4 id-keyed
  blob-download buttons (`AttachmentService.download`, JWT-safe).

### Completion Notes List

- **Endpoint decision honoured:** `POST /{id}/review` kept (not the spec's legacy
  `PUT /api/PeriodicOrphanReports` whole-form checkbox flip) — role separation between the
  Charity-writable update and the HQ-only review is the control; recorded in Dev Notes.
- Service path needed only the AC 4 guard: everything else (claims-derived reviewer, status
  stamping, BR-13 clearing, route-id binding, scope gate, refuse-reason lookup validation)
  was already implemented — the story-file defect list predated the groundwork and was
  re-verified item by item.
- **Payload fix worth noting:** the old `submitReview` always sent `refuseReasonId` and
  `refuseReason` — an empty string for a nullable int fails model binding (400 before any
  business rule runs). The payload is now assembled per branch with empty optionals omitted.
- Review screen still ships a hardcoded English refusal-reason array — that is 9-8's scope
  (live lookup catalogue + BR-14 UI); left untouched deliberately.
- Reviewer reachability: list now filters حاله الاعتماد via `reviewStatus` — backend mapping
  `pending → !Reviewed`, `approved → Reviewed && IsAccepted`, `rejected → Reviewed &&
  IsRefused` verified in `ApplyFilters`.

### File List

- `Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs` (already-reviewed guard on review)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-report-review/periodic-report-review.component.ts` (OnPush, toasts, payload fix, blocked-state, attachment slots)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-report-review/periodic-report-review.component.html` (reviewed notice + disabled decisions, id-keyed attachments)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-reports-list/periodic-reports-list.component.ts` (reviewStatus filter state)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-reports-list/periodic-reports-list.component.html` (حالة الاعتماد select)
- `Frontend/src/assets/i18n/ar.json`, `Frontend/src/assets/i18n/en.json` (53 keys each)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORR-07 and module spec §14.D-25.5 / §14.U.7; reviewer-identity and status-stamping defects recorded; shared-endpoint contract with 9-8 pinned. |
| 2026-08-24 | Implemented: already-reviewed guard, review screen hardening (OnPush, toasts, in-flight disable, reviewed-state notice, payload fix, id-keyed attachments), status filter on the register for the pending queue. Status → review. |


### Review Findings (epic review 2026-08-24)

> Review Outcome 2026-08-24 — all items patched & verified: P13 verified already-correct in code (accept path nulls refuseReason/refuseReasonId; resubmission resets reason fields) — no edit was needed.

- [x] [Review][Patch] P10 Charity self-accept not blocked (self-refuse is) — symmetric guard [review path]
- [x] [Review][Patch] P13 Empty refusal reason persists when accepting — clear reason fields on accept
- [x] [Review][Patch] P35 Review submit double-unwraps error — BR-14 translated mapping dead code; use `error ?? {}` fallback [periodic-report-review.component.ts:260]
- [x] [Review][Patch] P37c Load failure blank screen (review) [periodic-report-review.component.ts:171]
