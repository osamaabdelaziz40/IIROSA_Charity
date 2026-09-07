# Story 10.4: Update a payment batch — تعديل الدفعة

| Field | Value |
| --- | --- |
| Story | US-PAY-04 (UC-PAY-04) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.S.2, §15.U.4) |
| Priority / size | Must · 5 points |
| Route | `#/orphan-payments/:id/edit` |
| Endpoint | `PUT /api/OrphanPayments/{id}` (+ as-built `PUT {id}/exchange-rate`, `POST {id}/lock-exchange-rate`) |
| Depends on | 10-2 (columns, validator pattern), 10-3 |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer |

Status: done

## Story

As a General Director,
I want to be able to update a payment batch تعديل الدفعة,
so that a record that was entered wrongly or has changed can be corrected.

## Acceptance Criteria

1. Given an HQ-Fin role in edit mode, when «حفظ» is pressed with valid input, then `PUT /api/OrphanPayments/{id}` persists the new values and only that record changes; the list (10-1) shows the new values.
2. Given a mandatory field (§15.S.2 set incl. `PaymentDate`) is empty or periods are inverted, then the save is refused (400) and the field is flagged.
3. Given the exchange rate is edited while `DontRemoveRate` is set, then the change is refused with the business message (as-built lock semantics, سعر الصرف + عدم خصم النسبه).
4. Given the charity write-lock (UC-CHR-09) is active, then update is refused (`EnsureCanUpdateAsync`).
5. Given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login.

**Definition of done:** edit round trip works incl. exchange-rate set/lock; the form's PATCH-verb calls are fixed; §15.U.4 passes.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Endpoint | `PUT /{id}` → `UpdatePaymentGroup(UpdateOrphanPaymentDto)` (`OrphanPaymentsController.cs:185`, Id overwritten from route :189) · `PUT /{id}/exchange-rate` (:249, `SetExchangeRateDto`) · `POST /{id}/lock-exchange-rate` (:279, raw `bool` body) |
| Service | `UpdatePaymentGroupAsync` (real: write-guard, date validation, lock check) · `SetExchangeRateAsync` (rejects when `DontRemoveRate`) · `LockExchangeRateAsync` (`OrphanPaymentService.cs:102-148,237-288`) |
| Frontend | `orphan-payment-form/` edit mode + service methods — but `updateExchangeRate`/`lockExchangeRate`/`unlockExchangeRate` use **PATCH** (405 against the POST/PUT backend) |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. Frontend verb mismatches: `PATCH {id}/exchange-rate` → `PUT`; `PATCH {id}/lock-exchange-rate` → `POST`; `PATCH {id}/unlock-exchange-rate` → `POST` with `false` (or collapse to one toggle call).
2. No `UpdateOrphanPaymentValidator` (FluentValidation in service) — add; include `PaymentPeriodFrom <= To`, `PaymentDate` mandatory.
3. Roles: widen update/exchange endpoints to the HQ-Fin set.
4. Raw `bool` body on `lock-exchange-rate` is an untyped binding smell — change to a typed `{ lockRate: bool }` DTO (AddOrphans-style) and update the client in the same change.
5. 500 leak pattern on these actions — apply the generic-message fix if not already done in 10-1/10-3.

## Tasks / Subtasks

- [x] Task 1 — Validator + backend (AC: 1, 2, 3, 4)
  - [x] `UpdateOrphanPaymentValidator` invoked in service; widen roles; typed lock DTO; generic 500 message
- [x] Task 2 — Edit form wiring (AC: 1)
  - [x] Fix service verbs; load current values via `GET {id}`; save → PUT; field-error mapping from 400 payload
  - [x] Exchange-rate + DontRemoveRate controls wired to the real endpoints incl. lock refusal message (localised)
- [x] Task 3 — Regression: create mode (10-2) still works from the same component; i18n keys in ar + en

### Review Findings

_Code review 2026-08-26 — full detail in `review-artifacts/epic10-review-report.md`._

- [x] [Review][Patch] update writes `BatchNo` with no uniqueness check — the edit picker lists every group's number; picking another's → 2601 → raw 500 (create/assign paths check; add `IsBatchNoUniqueAsync` here) [OrphanPaymentService.cs:394]
- [x] [Review][Patch] `LockExchangeRateDto` `{}` silently UNlocks with 200 — make `LockRate` `bool?` + required [LockExchangeRateDto.cs]
- [x] [Review][Patch] `takeUntil(destroy$)` on the form's create/update subscriptions [orphan-payment-form.component.ts:216-260]

## Dev Notes

### Platform rules that bind this story

- `ICharityWriteGuard` stays the write-lock gate; business refusals keep the service's existing exception shapes (`InvalidOperationException` → 400).
- Thin controller, raw envelope, camelCase; tests excluded per standing decision — smoke and record.

### Story-specific rulings

- Board endpoint `PUT /api/OrphanPayments` is realised as the as-built `PUT /{id}` (id in route beats id in body; the epic's body-only shape is a legacy artifact — same ruling style as epic 9's endpoint column).
- Row-level updates do NOT go through this endpoint (see 10-13 ruling) — this PUT is header-only.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Delete | 10-5 |
| Row flags / settlement | 10-9..10-13 |
| mark-uploaded / IsBatchUploaded wiring | 10-16 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.U.4] · [#15.S.2]
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:185-305]
- [Source: Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs:102-311]

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code CLI).

### Debug Log References

- Live smoke vs a private instance of the NEW build on http://127.0.0.1:60970 (the user's
  API on 60961 kept running untouched), seeded SuperAdmin, 2026-08-24:
  `POST` create (SAR, rate 0.2) → 201; `PUT /{id}` valid rename + period/paymentDate
  change → **200, all values persisted** (AC 1); `PUT` inverted periods → **400
  `{"errors":{"PaymentPeriodFrom":[…]}}`** field map (AC 2, validator via
  ValidationException catch); `POST /{id}/lock-exchange-rate {"lockRate":true}` → 200
  "locked" (typed DTO — would 400 on the old raw-bool binding); `PUT` rate 0.2→0.9 while
  locked → **400 "Exchange rate is locked and cannot be modified"** (AC 3); unlock → 200;
  cleanup `DELETE` → 204.
- AC 4 (charity write-lock): `EnsureCanUpdateAsync` pre-exists at the top of
  `UpdatePaymentGroupAsync` — verified by inspection, not re-smoked (needs a locked
  charity fixture; same coverage as the service's other writes).
- `dotnet build Backend/IIROSA.sln` → 0 errors. One mid-story build failed on
  `IncomingService.cs`/`OutgoingService.cs` CS1501 — the parallel session's in-flight
  edit (files modified seconds earlier); no errors in this story's files; resolved by
  their follow-up edit, final build clean.
- `npx tsc --noEmit -p tsconfig.app.json` → 0 errors; `ar.json`/`en.json` JSON.parse OK.

### Completion Notes List

- FE verb fixes (all previously 405'd): `updateExchangeRate` PATCH→**PUT**
  `{id}/exchange-rate`; `lockExchangeRate`/`unlockExchangeRate` PATCH→**POST**
  `{id}/lock-exchange-rate` with `{ lockRate: true|false }` (no separate unlock route);
  drive-by same-class fixes: `markAsUploaded`/`unmarkAsUploaded` PATCH→**POST**
  `{id}/mark-uploaded` with `{ isUploaded }` (10-3's detail buttons were 405ing — keeps
  that story's "only live endpoints" DoD true), `updateBatchNumber` PATCH→**PUT**.
- Locked-batch edit bug found + fixed: `patchForm` disables rate/currency while
  `DontRemoveRate` is set, and disabled controls drop out of `form.value` — the update
  DTO would send `exchangeRate: undefined` and the server read the omission as a rate
  change, refusing ANY edit of a locked batch. Update path now reads
  `form.getRawValue()`.
- Business refusals (plain 400 `{ message }`, no field map) previously vanished silently;
  the form now shows them via `serverMessage` — known messages map to i18n
  (`exchangeRateLockedError` ar/en), unknown ones display verbatim.
- `UpdateOrphanPaymentValidator` mirrors the create validator (GroupName/PaymentDate/
  periods mandatory, period order, length caps); the service's hand date-range check was
  removed in its favour. Convention registration covers both validators
  (`AddValidatorsFromAssembly`).
- Roles widened on `PUT {id}`, `PUT {id}/exchange-rate`, `POST {id}/lock-exchange-rate`
  to SuperAdmin,Admin,Accountant,FinancialOfficer. `PUT {id}/batch-number` role widening
  deliberately NOT done here (owned by the batch-number story).
- Task 3 regression: create mode shares the component — create path re-verified in the
  same smoke (201 before the PUT flow); tsc covers the shared template.

### File List

- Backend/src/IIROSA.Application/DTOs/OrphanPayment/LockExchangeRateDto.cs — NEW typed
  `{ LockRate }` body for POST {id}/lock-exchange-rate (replaces raw bool binding)
- Backend/src/IIROSA.Application/Validators/OrphanPayment/UpdateOrphanPaymentValidator.cs —
  NEW §15.S.2 update field set
- Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs — ctor +
  `_updateValidator`; `UpdatePaymentGroupAsync` invokes it (hand date check removed)
- Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs — PUT {id} roles +
  ValidationException 400 field map; exchange-rate PUT + lock POST roles widened; lock
  action takes `LockExchangeRateDto`
- Frontend/src/app/modules/orphan-payments/services/orphan-payment.service.ts — 6 verb
  fixes (PUT/POST alignments incl. mark-uploaded + batch-number)
- Frontend/src/app/modules/orphan-payments/orphan-payment-form/orphan-payment-form.component.ts —
  getRawValue on update; serverMessage + resolveServerMessage (known→i18n); cleared on submit
- Frontend/src/app/modules/orphan-payments/orphan-payment-form/orphan-payment-form.component.html —
  serverMessage alert
- Frontend/src/assets/i18n/ar.json / en.json — `exchangeRateLockedError` (ar + en)

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
- 2026-08-24 — implemented (Tasks 1–3): update validator + roles + typed lock DTO, FE
  verb fixes, locked-batch raw-value fix, localised refusal message; build + tsc + live
  smoke (all ACs) clean → review.
- 2026-08-26 — code review remediation (P7/P25/P21): update-path BatchNo uniqueness + trim normalisation, LockRate bool? + required guard, takeUntil on create/update. Build verified. → done.
