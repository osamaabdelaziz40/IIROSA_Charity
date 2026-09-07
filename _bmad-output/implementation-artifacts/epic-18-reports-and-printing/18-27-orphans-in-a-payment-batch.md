# Story 18-27: Orphans in a payment batch أيتام الدفعة

| Field | Value |
| --- | --- |
| Story key | `18-27-orphans-in-a-payment-batch` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-27 — أيتام الدفعة |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.U.27 scenario — no dedicated §23.S screen) |
| Route | hosted on the orphan-payments module's batch view — no new route |
| Endpoint | `GET /api/OrphanPayments/{id}/details` — **EXISTS** (`OrphanPaymentsController.cs:116`); this story adds NO backend endpoint |
| Depends on | orphan-payments module (EP-10 vertical) present; 18-1's reports module NOT required |
| Roles | HQ roles + charity → `SuperAdmin`, `Admin`, `Charity` (`OrphanPayments.View`) |

## Status

done

## Story

As a HQ role, I want to be able to orphans in a payment batch أيتام الدفعة, so that I can see the
full detail of a single record before acting on it.

## Acceptance Criteria

1. Given an HQ or charity user with an active session on a payment batch in the orphan-payments
   module, when the actor opens the batch's orphan list, then every orphan included in that batch
   for the charity is listed with its amount and payment state. No stored data is changed.
2. Given the list is served, when it renders, then the data comes from the EXISTING
   `GET /api/OrphanPayments/{id}/details` (`GetPaymentGroupDetails`) — no new report endpoint, no
   fork of the payment service.
3. Given the caller is a charity user without an `orphanId`, when the request is served, then the
   endpoint refuses the widened read exactly as it does today (charity role requires `orphanId`) —
   this story does NOT relax the 10-7/8-9 contract; the batch-wide list is HQ-visible, and the
   charity sees its own scoped views per the existing endpoint behaviour.
4. Given the batch has no orphan items, when the list renders, then the empty state shows and no
   error is raised.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.

**Definition of done:** §23.U.27's question — "all orphans in a batch for a charity, with amounts
and payment state" — is answerable from the orphan-payments batch screen through the existing
endpoint; the endpoint contract is verified against the live controller and any UI gap is closed
WITHOUT backend changes (gaps recorded, not patched by a parallel endpoint).

## Data contract (verified against the live controller)

`OrphanPaymentsController.GetPaymentGroupDetails(Guid id, [FromQuery] Guid? orphanId)` —
`[Authorize(Roles = "SuperAdmin,Admin,Charity")]`, charity forbidden without `orphanId`; returns
the payment-group DTO with its orphan rows (orphan identity + amount + flags as EP-10's columns
carry them: `Amount`, `IsStopped`, `IsPrinted`, `IsGotIt`/`ReceivedOn`, cheque number). The
§23.U.27 text crediting `PeriodicOrphanReportsController` is a template artefact — the realisation
line in the same table names `OrphanPaymentsController` correctly.

## Tasks / Subtasks

- [x] **Task 1 — Contract audit** (AC 2, 3)
  - [x] Read `Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:110-150` and the
        `OrphanPaymentDto`/item DTOs the endpoint returns; confirm the orphan rows carry: orphan
        code/name, amount, payment state (stopped/printed/received flags), cheque number where
        applicable — the §23.U.27 "amounts and payment state" contract
  - [x] If a column the scenario names is genuinely absent from the DTO (not just unpainted in
        the UI), RECORD the gap in the Dev Agent Record — do not add a parallel endpoint in this
        story; a follow-up belongs to the EP-10 vertical
- [x] **Task 2 — Batch-screen surfacing** (AC 1, 4)
  - [x] In the orphan-payments module's batch view component (the screen that loads
        `GET /api/OrphanPayments/{id}`), surface/render the orphan rows from the SAME service call
        the module already makes (`{id}/details` is in `orphan-payment.service.ts` — verify and
        reuse); present them as the batch's orphan list with amount + state columns (badge/chip
        for stopped/printed/received), `trackBy`, empty state, bespoke grid — NOT `data-list`
  - [x] If the module already renders these rows (EP-10 stories may have landed the batch view),
        this story collapses to: verify the §23.U.27 contract is fully answerable + close any
        column/label gap in the template — RECORD what was already there; do not rebuild
- [x] **Task 3 — Charity-mode note** (AC 3) — where the charity-scoped mode requires `orphanId`,
      the batch-wide list is simply not offered to that caller (hide, don't 403-spam): the
      endpoint's existing authorisation remains the control — client hiding is convenience only
- [x] **Task 4 — i18n** — any new labels land under the orphan-payments module's existing i18n
      namespace in **both** `ar.json` and `en.json`; no `reports.*` keys (no reports screen here)
- [x] **Task 5 — Verification** (AC 1–5)
  - [x] Live check: HQ token `GET {id}/details` → 200 with orphan rows; charity token without
        `orphanId` → 403 (existing behaviour, unchanged); anonymous → 401; empty batch → empty
        state in the UI
  - [x] `dotnet build` (expected: untouched) + `npm run build` green (MSB3021/3027 live-API lock
        caveat; ng-serve stale-bundle grep); tests excluded per the standing user decision

## Dev Notes

### Reuse-over-reinvention (the whole point of this story)

UC-RPT-27's data surface is a pure reuse of the EP-10 batch-details endpoint. The risk this story
guards against is a dev building `POST /api/Reports/orphans-in-batch` beside a live endpoint that
already answers it. If the audit (Task 1) finds the batch screen already answers §23.U.27
completely, close the story with that verification recorded — an honest "already satisfied" beats
invented work.

### Platform rules that bind this story

- No new backend endpoint, no DTO changes, no migration — verification + UI surfacing only.
- The endpoint's `[Authorize(Roles = …)]` is the control; client hiding is convenience.
- Bespoke grid (NOT `data-list`); `trackBy`; empty state; OnPush omitted (list-screen precedent);
  i18n both languages.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Orphans receiving nothing (zero-disbursed subset of a batch) | 18-28 |
| Payment summary/cover pages for a batch | 18-32 |
| Received/not-received/stopped printed lists | 18-29 (landed scope) |
| Any change to the `{id}/details` authorisation or shape | EP-10 vertical |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.27] scenario — batch orphan list
  with amounts and payment state
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-27 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:110-150] the live
  endpoint this story reuses (`GetPaymentGroupDetails`, charity-requires-orphanId)
- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md] the batch/outcome-state
  vocabulary the list columns render

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- **Contract audit (Task 1)** — `OrphanPaymentsController.GetPaymentGroupDetails`
  (`{id}/details`, roles `SuperAdmin,Admin,Accountant,FinancialOfficer,Charity`) returns
  `OrphanPaymentDto` with `orphans: OrphanPaymentItemDto[]` carrying the FULL §23.U.27
  contract: `orphanCode`, `orphanFullName`, `orphanFamilyName`, `charityName`, `amount`,
  `isStopped`, `isPrinted`, `isGotIt`, `receivedOn`, `chiqueNum`, `transferNo`,
  `exchangeStatus`. No column the scenario names is absent — no gap to record, nothing to add.
- **Screen audit (Task 2)** — EP-10's 10-3 batch view
  (`orphan-payment-detail.component.html`) ALREADY renders these rows from the SAME
  `{id}/details` call (one payload, header + rows): stop badge + stop/resume action, orphan
  code, orphan name, guardian, charity, amount, received/printed badges, cheque number + date,
  transfer number, exchange status — sortable, `trackBy`, `noOrphansInGroup` /
  `noOrphansMatch` empty states, shared pager. §23.U.27 is fully answerable as landed; this
  story collapses to verification exactly as its Dev Notes allow ("an honest already-satisfied
  beats invented work"). No frontend file changed → the 18-26 green `ng build` tree is
  unchanged (nothing to recompile; no new i18n keys).
- `dotnet build` (Api → temp smoke dir) — **0 errors** (backend untouched by this story; the
  build doubles as the smoke instance).
- Live matrix, private instance `127.0.0.1:60970`, seed batch `1827-SMOKE` with 2 rows
  (LC-CODE-1 amount 500 plain; ORP-2026-23731 amount 750 stopped+printed+received, cheque
  `1827-CHQ-2`):
  - anonymous `GET {id}/details` → **401** (AC 5)
  - HQ token → **200**, `groupName 1827-SMOKE`, `orphanCount 2`, both rows with code/name/
    amount/stopped/printed/gotIt/cheque exactly as the DTO promises (AC 1, 2)
  - HQ + `?orphanId=` → **200 with 1 row** (single-orphan mode unchanged)
  - Charity token (no charity claim) → **403** — the endpoint's CURRENT fail-closed D4
    behaviour (see completion notes)
  - empty state: template-verified (`noOrphansInGroup` row renders when `filteredOrphans`
    is empty) — every live batch in the dev DB was soft-deleted EP-10 seed leftovers, so the
    empty-batch leg is structural, not seeded
  - seeds hard-deleted (0/0), instance killed by PID (38176), temp dir removed

### Completion Notes List

- **AC 3 delta — the endpoint has evolved past the story's recorded contract.** The story file
  recorded "charity role requires `orphanId` (10-7/8-9 contract)". The live controller now
  implements the later §15.U.7 semantics (a 10-x story landed it): a Charity caller WITH a
  charity claim gets the shared header with rows filtered to its own orphans (zero rows is an
  empty list, not a refusal); a Charity caller WITHOUT a claim is refused fail-closed (`403`,
  D4 — can never be scoped, refuse rather than leak the unfiltered batch); `charityId` is an
  HQ-only narrowing filter. This story does NOT touch that authorisation (verified live, not
  patched); the recorded AC-3 text is the older contract — current behaviour documented here.
- Client-side, the batch screen is role-shared and the endpoint is the control: a claim-less
  charity caller gets the 403 toast (fail-closed by design), which is the "no 403-spam"
  concern only in the claim-less edge — the normal charity user (claim present) reads its
  scoped rows directly on the same screen. No hiding logic added; the endpoint remains the
  control per the platform rule.
- **Zero new surface:** no endpoint, no DTO change, no migration, no frontend edit, no i18n
  key. The whole story is the audit + the live proof.
- Tests excluded per the standing user decision.

### File List

- *(none — pure-reuse verification story; the existing 10-3 batch screen and the existing
  `{id}/details` endpoint answer §23.U.27 completely as landed)*

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-27 and module spec §23.U.27; cut to a pure-reuse story over the existing `{id}/details` endpoint with a contract-audit-first task and the controller-attribution artefact recorded. |
| 2026-08-24 | Verified and closed as already-satisfied: DTO carries every §23.U.27 column (code/name/amount/stopped/printed/received/cheque); 10-3's batch view renders them from the same `{id}/details` payload with badges, empty state, trackBy, pager. Live matrix on a seeded batch: 401 anon, HQ 200 both rows, orphanId narrow 1 row, claim-less Charity 403 (endpoint's evolved §15.U.7/D4 semantics recorded as the AC-3 delta). No code changed. Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
