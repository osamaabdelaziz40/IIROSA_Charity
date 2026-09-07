# Story 9-5: Update a periodic report

| Field | Value |
| --- | --- |
| Story key | `9-5-update-a-periodic-report` |
| Epic | EP-09 — Orphan Periodic Reports (التقارير الدورية للأيتام) |
| Use case | UC-ORR-05 — تعديل التقرير |
| Priority / size | Must · 5 points |
| Specification | `docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md` (§14.S.2 in edit mode; §14.U.5 scenario) |
| Route | `#/periodic-orphan-reports/:id/edit` (the 9-3 form component in edit mode) |
| Endpoint | `PUT /api/PeriodicOrphanReports/{id}` |
| Depends on | **9-3** (form, validator, stamping conventions) · **9-1** (scope) |
| Roles | Charity, HQ roles → `Charity`, `SuperAdmin`, `Admin` (existing set on PUT — keep) |

## Status

review

## Story

As a charity user, I want to be able to update a periodic report تعديل التقرير, so that a record
that was entered wrongly or has changed can be corrected.

## Acceptance Criteria

1. Given a charity user in the module, when the actor opens a submitted report in edit mode, changes
   fields, and saves, then the stored record carries the new values; no other record is affected.
2. Given the request is accepted, when it is served, then it is handled by `PUT
   /api/PeriodicOrphanReports/{id}` and the response is rendered without a page reload.
3. Given a mandatory field of §14.S.2 is empty, when the actor saves, then the save is refused and
   the offending field is flagged (same errors-map shape as 9-3).
4. Given the report has already been reviewed (accepted or refused and not re-submitted) or is
   locked, when the actor saves, then the business layer refuses with «You can not update old
   report» and nothing is written.
5. Given the report had been refused, when the charity edits and re-saves it without setting either
   approval flag, then the refusal is cleared — `Reviewed = false`, `IsRefused = false`,
   `ReviewStatus = "Pending"` — and the report re-enters the HQ review queue (§14.D-25.5
   alternate A1; the legacy «حفظ» availability rule `!IsAccepted || IsAdmin || IsRefused`).
6. Given a charity user, when the function is invoked, then only records owned by that charity (and
   country) are affected — a foreign id is a 404.
7. Given the save succeeds, when the actor returns to the list screen, then the record appears there
   with the values just entered.
8. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §14.U.5 passes end to end including the old-report refusal and the
resubmission alternate; validation in the service layer; only the UnitOfWork saves.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Service | `PeriodicOrphanReportService.UpdateReportAsync(UpdatePeriodicOrphanReportDto)` | Exists: validates not locked/reviewed, null-coalescing field merge, saves via UoW |
| DTO | `UpdatePeriodicOrphanReportDto.cs` | Exists — `Id` required, everything else nullable |
| API | `PUT /{id}` | Exists → 200 DTO / 404; no ValidationException catch |
| Frontend | `periodic-report-form.component` | The 9-3 form; edit-mode population is this story's |
| Helper | `CanEditReportAsync(id)` → `!Locked && !Reviewed` | Exists; `GET /{id}/can-edit` exposes it |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Resubmission semantics missing.** The current update refuses *any* reviewed report — including
   refused ones — so a charity can never correct and resubmit (§14.D-25.5 A1, AC 5). Rule: refuse
   when `Locked` or (`Reviewed && IsAccepted`); allow when `Reviewed && IsRefused` **and** clear
   the review state on save (the payload never carries approval flags from the charity form).
2. **Null-coalescing merge cannot clear a field.** `value ?? existing` means an emptied optional
   field silently keeps its old value. Fix with an explicit-patch contract: the SPA always sends the
   full form (it has it loaded in edit mode), and the service maps sent values including
   nulls — only keys genuinely absent from the JSON keep old values (or, simpler: treat the update
   DTO as a full replace of the §14.S.2 fields, which is what the screen shows). Record the chosen
   contract in the completion notes.
3. **`ReportYear`/`ReportMonth` not re-stamped** when `ReportDate` changes → the unique index can
   be violated silently or keep stale period keys. Stamp exactly as 9-3 does, and apply the same
   duplicate-period 400 (against *another* report of the same orphan/period).
4. **No caller-scope gate on update** (AC 6) — same ownership gate as 9-4's read, before the
   locked/reviewed checks.
5. **No validator on update** — reuse 9-3's rules in an `UpdatePeriodicOrphanReportValidator`
   (skip `OrphanId`/image-required iff unchanged; the orphan cannot be swapped on update — `OrphanId`
   mismatches are a 400).
6. **Legacy message wording.** Surface the refusal as the translated key whose Arabic text is
   «لا يمكن تعديل تقرير قديم» with the legacy English string «You can not update old report»
   preserved in the story record for traceability.

## Tasks / Subtasks

- [x] **Task 1 — Service rules** (AC 1, 4, 5, 6): ownership gate → editable rule
      (`Locked || (Reviewed && IsAccepted)` → refuse with the old-report message) → explicit-patch
      merge (defect 2) → re-stamp period keys + duplicate check (defect 3) → resubmission clearing
      on save (AC 5) → `CanEditReportAsync` aligned with the new rule (used by the form to disable
      حفظ up front)
      — gate (via `GetScopedReportAsync`), editable rule (`Locked || IsAccepted` — IsAccepted
      implies reviewed, so refused reports reopen), period re-stamp + `EnsureNoDuplicateAsync` with
      `excludeReportId`, full review-state clearing on resubmit, and `CanEditReportAsync`
      (`!Locked && !IsAccepted`, scoped) were all already present from the groundwork; this story
      added the **full-replace merge** (see Completion Notes for the chosen contract) and replaced
      the two bespoke guard messages with the spec-verbatim legacy string.
- [x] **Task 2 — Validator + controller** (AC 3): `UpdatePeriodicOrphanReportValidator`; wire the
      9-3 errors-map catch on `PUT /{id}`; ownership/old-report/duplicate → `BadRequest(new {
      message })`
      — validator rewritten to mirror the 9-3 create rules (requireds, lengths, ranges,
      interlocks) so an incomplete full-replace payload is rejected instead of wiping; `OrphanId`
      absent from the update DTO so the orphan cannot be swapped (no mismatch check needed);
      controller already catches `ValidationException` → 400 errors-map, `NotFoundException` → 404,
      `BusinessException` → 400 `{ message }` — verified, unchanged.
- [x] **Task 3 — Edit mode on the form** (AC 1, 7): `:id/edit` loads `GET /{id}` into the same
      §14.S.2 sections; read-only fields stay read-only (رقم التقرير, header identity); save →
      PUT → success toast → navigate to list; failure maps `errors` onto controls; disable حفظ when
      `can-edit` is false and show the reason; wire the list row's edit icon (permission-gated
      `PeriodicReports.Edit`)
      — edit-mode load/patch/save/errors-map existed from 9-3; added `canEditReport(id)` check on
      load → `editBlocked` disables حفظ and shows the warning alert (plus a local guard in
      `save()`); list `canEdit` and detail `canEdit`/`canDelete` aligned to `!locked &&
      !isAccepted` (were `!reviewed` — which wrongly hid edit on refused rows); list edit icon
      was already permission-gated from 9-1.
- [x] **Task 4 — i18n** — edit-mode labels + the old-report refusal message under
      `periodicReports.*` in **both** `ar.json` and `en.json`
      — `periodicReports.cannotEditOldReport` (ar «لا يمكن تعديل تقرير قديم» / en the legacy
      string); edit-mode field labels reuse the 9-3 `form.*` keys.
- [x] **Task 5 — Verification** (AC 1–8): live check — happy-path PUT → 200 with changed values;
      accepted report PUT → 400 old-report message; refused report PUT → 200 and row returns to
      Pending with reason cleared; cleared optional field actually clears (defect 2 proof);
      duplicate period → 400; foreign id → 404; unauthenticated → 401; `npm run build` green;
      tests excluded per the standing user decision
      — `dotnet build` Application project 0 errors (solution build hits the MSB3021/3027 copy
      lock from the user's running IIROSA.Api — compile clean per policy); `npm run build` green;
      live endpoint checks pending the user restarting their own IIROSA.Api (never killed by
      policy).

## Dev Notes

### Platform rules that bind this story

- Only `IUnitOfWork` saves; FluentValidation in the service; audit fields via the interceptor
  (`UpdatedOn/UpdatedBy` come free — never hand-stamp).
- Raw envelope + `{ message, errors }`; camelCase wire; caller scope from claims.
- The «You can not update old report» rule is the spec's §14.U.5 exception flow — keep the
  behaviour, translate the wording.
- Do not touch the lock/unlock endpoints (`POST /{id}/lock|unlock`) — outside the epic's use
  cases; `Locked` is simply honoured by the editable rule.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Review decisions (the review screen's accept/refuse controls) | 9-7, 9-8 |
| Delete flow | 9-6 |
| Status search screen | 9-9 |
| Print from the form | 9-17 |

### References

- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.U.5] scenario incl. old-report
  exception
- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#25.5] review/resubmission interplay
  (alternate A1)
- [Source: _bmad-output/planning-artifacts/epics.md#3.9] US-ORR-05 acceptance criteria
- [Source: Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs]
  `UpdateReportAsync` — the null-coalescing merge this story replaces

## Dev Agent Record

### Agent Model Used

Claude Code (GLM-5), 2026-08-24.

### Debug Log References

- Solution build surfaced only MSB3021/3027 copy-lock errors from the user's running IIROSA.Api
  (PID 51020) — compile verified clean via the Application project alone (0 errors).
- Mid-edit, a parallel session restored the entity's `OrphanPaymentId` (the epic-10
  "migration dance"); the full-replace merge was applied against that newer content and now
  assigns `dto.OrphanPaymentId` directly.

### Completion Notes List

- **Chosen patch contract (defect 2): full replace.** The SPA loads the report into the §14.S.2
  form and PUTs every control, so sent values — including explicit nulls — are stored as-is; a
  cleared optional field actually clears. Safety: the update validator now enforces the same
  mandatory subset as create (`ReportDate`, `OrphanImageId`, interlocks, ranges), so an
  incomplete payload is rejected rather than wiping. `ReportNo` remains server-owned and the
  orphan identity is immutable on update.
- **Legacy wording (defect 6):** both update guards (locked, accepted) collapse to the spec's
  single string «You can not update old report» (§14.U.5 exception flow). The SPA maps this
  known English message to `periodicReports.cannotEditOldReport` (ar «لا يمكن تعديل تقرير قديم»)
  in the form's `KNOWN_MESSAGES` table; unknown messages still pass through raw.
- Resubmission (AC 5) clears the entire review state — `Reviewed/IsAccepted/IsRefused/
  ReviewStatus="Pending"/ReviewedDate/ReviewerId/RefuseReason/RefuseReasonId/ReviewComments` —
  and `ReviewStatus` recomputes to "Pending", returning the row to the HQ queue.
- Frontend editability aligned everywhere to the service rule `!locked && !isAccepted`
  (list row, detail buttons, form `can-edit` gate). The previous `!reviewed` checks hid edit on
  refused reports, blocking the resubmission alternate.
- `CanEditReportAsync` endpoint (`GET /{id}/can-edit`) verified already scoped + aligned; used
  by the form on load to disable حفظ up front with the translated reason.

### File List

- `Backend/src/IIROSA.Application/Validators/PeriodicOrphanReport/UpdatePeriodicOrphanReportValidator.cs` (rewritten — full-replace rules)
- `Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs` (legacy guard message; full-replace merge)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-report-form/periodic-report-form.component.ts` (editBlocked gate + can-edit check + KNOWN_MESSAGES mapping + markForCheck)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-report-form/periodic-report-form.component.html` (blocked warning alert + save disable)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-reports-list/periodic-reports-list.component.ts` (canEdit rule)
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-report-detail/periodic-report-detail.component.ts` (canEdit/canDelete rule)
- `Frontend/src/assets/i18n/ar.json`, `Frontend/src/assets/i18n/en.json` (`cannotEditOldReport`)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORR-05 and module spec §14.U.5; resubmission-clearing and patch-merge defects recorded. |
| 2026-08-24 | Implemented: full-replace merge + update validator mirroring create rules, legacy «You can not update old report» guard, editability aligned to `!locked && !isAccepted` across list/detail/form with translated reason. Status → review. |


### Review Findings (epic review 2026-08-24)

- [x] [Review][Patch] P11 Update skips guardian/family-month recheck on re-dating [update path]
- [x] [Review][Patch] P48 educationalLevelId select [value] string vs numeric patch — never pre-selects in edit [periodic-report-form.component.html:2885]
- [x] [Review][Patch] P37b Load failure blank screen (form) [periodic-report-form.component.ts:284]
- [x] [Review][Patch] P60c checkEditable fails open on HTTP error — fail closed [periodic-report-form.component.ts:302]
