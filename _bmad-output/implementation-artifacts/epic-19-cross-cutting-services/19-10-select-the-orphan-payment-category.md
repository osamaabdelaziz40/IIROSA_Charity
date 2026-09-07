# Story 19-10: Select the orphan payment category

| Field | Value |
| --- | --- |
| Story key | `19-10-select-the-orphan-payment-category` |
| Epic | EP-19 — Cross-Cutting Services — الخدمات المشتركة |
| Use case | UC-SYS-10 — فئة دفع الأيتام |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/24-UC-SYS-Cross-Cutting-Services.md` (§24.U.10 scenario) |
| Route | the payment-composition screens (`#/orphan-payments`, EP-08 batch flow) |
| Endpoint | `GET /api/OrphanPayments/{orphanId?}` (live) — interpretation ruling below |
| Depends on | 8-8 (built the payment-groups read); consumers: batch composition screens (EP-08) |
| Roles | HQ financial: `SuperAdmin`, `Admin`, `FinancialOfficer`, `Accountant`; Charity admitted **only with an explicit `orphanId`** |

## Status

done

## Story

As a Head Office financial user, I want to be able to select the orphan payment category فئة
دفع الأيتام, so that a payment batch is composed from the right set of orphans.

## Acceptance Criteria

1. Given an HQ financial user with an active session, when composing a payment batch, then
   the orphan's payment history/categories are served by `GET /api/OrphanPayments/{orphanId?}`
   — no stored data is changed.
2. Given the request is accepted, when it is served, then the response is rendered on the
   screen without a page reload and the orphan's payment rows can be selected into the batch
   composition.
3. Given a Charity-role caller, when the endpoint is invoked without an `orphanId`, then the
   request is refused — the broad listing is HQ-only; a charity sees payment data only for an
   explicitly named orphan within its scope.
4. Given the orphan has no payment history, then the screen renders its empty state — not an
   error (8-11 AC 6 precedent).
5. Given the session has expired or the role is not permitted, when the function is invoked,
   then the request is rejected and the actor is routed back to the login screen.

> **Interpretation ruling (recorded):** the board names `GET
> /api/OrphanPayments/payment-categories` — a legacy WAR shorthand. On this stack there is **no
> separate "payment category" catalogue** (orphan payment categorisation lives on the payment
> rows themselves); the shipped mechanism is 8-8's `GET /api/OrphanPayments/{orphanId?}`
> payment-groups read (an orphan's payment history grouped for batch composition). Building a
> parallel category endpoint would be a duplicate capability (PRD §7 non-goal). This story
> verifies + documents the live read as the platform's realisation of UC-SYS-10; correct-course
> only if the business later defines an explicit category taxonomy.

**Definition of done:** the §24.U.10 scenario is verified on the live payment-composition
flow; the Charity-scope rule (orphanId required) is proven; the interpretation ruling is
recorded; no new endpoint exists.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- |
| Endpoint | `OrphanPaymentsController` `GET /{orphanId?}` = `GetPaymentGroups` (8-8): payment rows grouped per orphan for batch composition; Charity admitted only with `orphanId` | Live |
| Scoping | Server-side: HQ roles see the broad listing; a charity claim narrows to its orphans (orphanId required) | Live — verify, do not rebuild |
| Frontend | `modules/orphan-payments/*` composition screens | Live (EP-08 set) |
| Payment entity | Payment rows carry the amount/period/case linkage the "category" selection composes from | Live |

## Verified gaps this story must fix

1. **Charity-scope proof (AC 3).** Exercise the live read as a Charity-role token both ways —
   with and without `orphanId` — and record the responses. If the guard is missing anywhere in
   the path, patch it server-side in the service (never controller-level only).
2. **Consumer verification (AC 2).** Confirm the batch-composition screen actually consumes
   the grouped read (selecting an orphan's payment rows into a batch); fix the wiring if the
   screen fetches something else.
3. **Empty state (AC 4).** Orphans without payment history render the screen's empty state.

## Tasks / Subtasks

- [x] **Task 1 — Scope proof** (AC 3, 5)
  - [x] Live smoke on the private port: HQ token → 200 broad listing; Charity token without
        orphanId → refused; Charity token with own-scope orphanId → 200; with an out-of-scope
        orphanId → refused. Record the four results in the story notes
- [x] **Task 2 — Consumer wiring check** (AC 1, 2)
  - [x] Trace the composition screen's data source to the grouped read; patch only if broken
- [x] **Task 3 — Empty state** (AC 4)
  - [x] Verify the no-history orphan renders the empty state; patch only if missing
- [x] **Task 4 — Ruling hygiene**
  - [x] Record the interpretation ruling + the four scope-proof results here; add a one-line
        pointer in `deferred-work.md` if any consumer was found fetching a non-existent
        category endpoint

## Dev Notes

### Platform rules that bind this story

- **No new endpoint, no category catalogue** — the grouped payment read is the realisation
  (ruling above).
- Tenancy: every query scoped to the caller's Charity/country, enforced server-side (AC 3 is
  this rule made concrete).
- `ApiResponse` envelope rules apply on this controller as shipped by 8-8 — do not re-shape.
- If any guard must change: service layer, not controller.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| Batch creation / disbursement flow | EP-08 (live set) |
| Payment category taxonomy (if ever defined) | correct-course story |
| Lookup sweep | 19-4 |

### References

- [Source: docs/Modules/24-UC-SYS-Cross-Cutting-Services.md#24.U.10] scenario + §24.2 UC-SYS-10 row
- [Source: _bmad-output/planning-artifacts/epics.md#3.19] US-SYS-10 acceptance criteria + traceability (payment-categories → ruling)
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs] `GetPaymentGroups` — the live read
- [Source: _bmad-output/implementation-artifacts/sprint-status.yaml] epic-8 context — 8-8 shipped the grouped read

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- Four-case scope proof (61970), BEFORE patch: HQ no-orphanId -> 200; Charity no-orphanId -> 403 (D4 fail-closed: claim-less token); Charity own-scope orphanId -> 200; Charity out-of-scope orphanId -> **500** (defect). Unauth -> 401.
- Four-case proof AFTER patch (rebuild + restart): same first three; out-of-scope -> **404** with { message } body. All green.
- Scope-fixture note: the seeded Charity@IIROSA.com user had a NULL CharityId (token carried no charityId claim -> permanently fail-closed). Linked it in dev DB to charity A99AC790 (owns 8 orphans; EF78FFD2 owns the other 2, used for the out-of-scope id) and re-logged-in to mint a claim-bearing token. Left linked — it makes every charity-scope path testable; it is a seeded dev test user.
- Backend build green (bin/Smoke, 0 errors); no frontend file touched.
### Completion Notes List

- **Task 1 (scope proof + patch):** the guard chain is real — controller refuses Charity-without-orphanId outright (OrphanPaymentsController:47-50, D4 fail-closed on missing charity claim too); the SERVICE enforces scope via EnsureOrphanInCallerScopeAsync (OrphanPaymentService:545-565, soft-delete-aware, throws KeyNotFoundException for missing AND out-of-scope — the P12 no-existence-leak design). Defect found + fixed: GetPaymentGroups' local catch (Exception) swallowed the KeyNotFoundException and returned 500 — added the controller-idiom catch (KeyNotFoundException) -> NotFound({ message }) clause (the same shape its 10 sibling actions already use). Guard stayed in the service layer; the controller patch is only status translation.
- **Task 2 (consumer wiring):** orphan-payment-list feeds from loadPaymentGroups() -> orphanPaymentService.getOrphanPayments(searchRequest) -> GET /api/OrphanPayments (the grouped read, filter carries orphanId) — wired correctly, nothing to patch.
- **Task 3 (empty state):** the list renders an empty-state row (*ngIf empty) with i18n key orphanPayments.noPaymentsFound (orphan-payment-list.component.html:125-131) — present.
- **Task 4 (ruling hygiene):** interpretation ruling stands (no payment-categories endpoint, no catalogue — grouped read is the realisation); frontend grep for 'payment-categories' -> zero hits, so no deferred-work pointer needed. Ruling already recorded in the story head.
- Browser click-through deferred to the batched live walkthrough (epic precedent).
### File List

- Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs (GetPaymentGroups: KeyNotFoundException -> 404 catch clause; was 500)
## Change Log

| Date | Change |
| --- | --- |
| 2026-08-25 | Story file created from `epics.md` US-SYS-10 and module spec §24.U.10; resolved to a verify-and-document pass over 8-8's live grouped read (category-endpoint ruling recorded); Charity-scope proof is the core task. |
| 2026-08-25 | Verified + patched (review-and-complete pass): four-case scope proof run live (200/403/200/404 after fix); out-of-scope 500->404 translation defect found and fixed at the controller catch ladder; consumer wiring + empty state verified present; ruling recorded, no phantom consumers. |
