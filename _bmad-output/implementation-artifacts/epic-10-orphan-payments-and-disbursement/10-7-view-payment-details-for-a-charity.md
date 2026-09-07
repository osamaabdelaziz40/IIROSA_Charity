# Story 10.7: View payment details for a charity — تفاصيل الدفعة للجمعية

| Field | Value |
| --- | --- |
| Story | US-PAY-07 (UC-PAY-07) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.S.3 grid, §15.U.7) |
| Priority / size | Must · 2 points |
| Route | `#/orphan-payments/:id` (detail) — charity-visible row view |
| Endpoint | `GET /api/OrphanPayments/{id}/details` (as-built, extended) |
| Depends on | 10-2 (Amount column), 10-6 (batch/charity selectors) |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer, Charity (charity sees only its own rows) |

Status: done

## Story

As a charity user,
I want to be able to view payment details for a charity تفاصيل الدفعة للجمعية,
so that I can see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a Charity-role caller opening a batch, when `{id}/details` is called **without** `orphanId`, then the batch header returns with items **filtered to orphans owned by the caller's charity** — this replaces today's blanket `Forbid()` for the no-orphanId case.
2. Given a Charity-role caller with `orphanId` supplied (UC-ORP-09 single-orphan mode), then behaviour is **byte-identical to today** — scope check, 403 on out-of-scope, single-row result (8-9 regression guard).
3. Given an HQ role with explicit `charityId`, then items filter to that charity; without it, all rows return.
4. Given the charity dimension, then every row renders amount + stop/print/receipt flags + cheque data **from the 10-2 columns** (blank when null — never fake values, per 8-9).
5. Given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login; «Faild Operation» refusals map to 400 with the localised message.

**Definition of done:** the working screen for disbursement (§15.U.7 summary) reads correctly for both caller kinds; §15.S.3 grid columns render; charity scoping is server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Endpoint | `GET /{id}/details` → `GetPaymentGroupDetails(id, orphanId?)` (`OrphanPaymentsController.cs:120`) — Charity + no orphanId → `Forbid()` at :124 (**this story relaxes that branch**) |
| Service | `GetPaymentGroupDetailsAsync(id, orphanId, userCharityId, userRole)` — loads header + `GetWithOrphansByGroupAsync` + per-charity/region counts (`OrphanPaymentService.cs:438-478`) |
| Row DTO | `OrphanPaymentItemDto` — orphan identity/education + CharityId(int?)/CharityName(empty) + Region/Center/Sponsor names (never populated) |
| Tenancy helper | `EnsureOrphanInCallerScopeAsync` (:413) — orphan's `FK_CharityId ?? Family.FK_CharityId` must equal caller charity |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Charity branch blanket-forbids batch-level reads** — the WAR UC needs charity-level details (item-filtered). Implement AC-1 while preserving AC-2 exactly (the 8-8/8-9 orphan-scoped contract is a shipped epic-8 surface).
2. `OrphanPaymentItemDto.CharityId` is `int?` vs Guid charities — fix the type and populate it + `CharityName`/`RegionName`/`CenterName` in `MapOrphanPaymentItemDto` (empty block at `OrphanPaymentService.cs:721-725` — resolve via the orphan's charity navigation, batch-loaded, no N+1).
3. Extend the item projection with the 10-2 disbursement columns (`Amount`, `IsStopped`, `IsPrinted`, `IsGotIt`, `ChiqueNum`, `Printdate`, `BenificiaryName`, `TransferNo`, `ExchangeStatus`) — read side only; behaviour comes in 10-9..10-13.
4. Roles: widen to include Accountant/FinancialOfficer (and keep Charity).
5. `GetOrphanCountByCharityAsync`/`ByRegion` dicts — verify they exclude soft-deleted items; region stub returns empty (`GetOrphanCountByRegionAsync` TODO) — leave the stub, render nothing when empty (region stats are not WAR scope).

## Tasks / Subtasks

- [x] Task 1 — Charity-level read (AC: 1, 2, 3)
  - [x] Replace the no-orphanId `Forbid()` with item filtering by caller charity (role=Charity); HQ `charityId` param filters the same way; orphan-scoped branch untouched
  - [x] Unknown batch → 404; charity with zero rows in batch → header + empty item list (not 403/404)
- [x] Task 2 — Row projection (AC: 4)
  - [x] Fix CharityId type; populate name joins batch-loaded; add disbursement columns to DTO + profile
- [x] Task 3 — Detail grid (AC: 4): render §15.S.3 columns (وقف الصرف · كود اليتيم · اسم اليتيم · اسم المعيل · المبلغ · حالة الاستلام/الطباعة · رقم الشيك · تاريخ الشيك · رقم الحوالة · حالة الصرف) with blank-when-null; `trackBy`; selectors from 10-6
- [x] Task 4 — Smoke: Charity filtered vs HQ full vs HQ+charityId; orphan-scoped regression (8-9); i18n ar + en

### Review Findings

_Code review 2026-08-26 — full detail in `review-artifacts/epic10-review-report.md`._

- [x] [Review][Patch] per-charity count dict drops orphans tenanted through the Family — `GetOrphanCountByCharityAsync` groups by `Orphan.FK_CharityId` only; totals disagree with the coalesced row filter on the same batch [OrphanPaymentRepository.cs:265-274]
- [x] [Review][Patch] the `orphanId` branch returns before the charity-name join — charity column always blank in UC-ORP-09 single-orphan mode [OrphanPaymentService.cs:1152-1161 vs 1176-1196]

## Dev Notes

### Platform rules that bind this story

- Tenancy from JWT claims; pin-never-widen. Soft-delete filtering must cover the item join. Thin controller, raw envelope; tests excluded per standing decision — smoke and record.
- **8-9 regression contract** (verbatim from that file): "the orphan-payments screens call `{id}/details` with no `orphanId` — that path must behave identically after the change". Your change alters the **Charity-role** no-orphanId branch deliberately; HQ no-orphanId must stay identical.

### Story-specific rulings

- Batch headers carry no CharityId — the charity dimension is derived through `OrphanPaymentItem → Orphan → FK_CharityId ?? Family.FK_CharityId`. Filtering happens on items; the header is shared. This is the standing tenancy model for this epic (also used by 10-8/10-14/10-18..10-22).
- This story is the READ side only — no flag writes here.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Orphan-level grid surface/paging | 10-8 |
| Row action writes (stop/print/receipt/cheque) | 10-9..10-13 |
| Received/not-received filtered views | 10-18, 10-19 |
| Cheques workbench + CheckManagement link | 10-22 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.U.7] · [#15.S.3 grid columns]
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:120-152] · [OrphanPaymentService.cs:413-505,695-735]
- [Source: _bmad-output/implementation-artifacts/epic-8-orphan-register-and-coding/8-8-view-an-orphans-payment-history.md:48-49] · [8-9-view-an-orphans-payment-details.md:50,66] (regression contracts)

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code harness).

### Debug Log References

- Private smoke instance `http://127.0.0.1:60970` (Release bin), 2026-08-24; user's live API on 60961 untouched. Instance stopped after the run (frees the Release bin).
- sqlcmd `IIROSA_Db_Dev` — throwaway Charity-role user (`smoke107@test.local`) re-pinned B→C between calls; removed after the run.

### Completion Notes List

- **AC-1 (charity-level read)** — the no-orphanId blanket `Forbid()` is gone. Controller keeps only the D4 fail-closed guard (Charity claim unparseable → `Forbid()`); an explicit `charityId` is ignored for Charity callers (`effectiveCharityId = User.IsInRole("Charity") ? null : charityId` — pin-never-widen). Service `else` branch filters rows by `scopeCharityId` derived `Orphan.FK_CharityId ?? Family.FK_CharityId`; the header stays shared (batches carry no CharityId — the standing epic ruling).
- **AC-2 (8-9 regression, byte-identical)** — the `orphanId` branch is untouched. Live: Charity(B)+own orphan → 200 single row; HQ+orphanId → 200 single row. **Out-of-scope orphan returns 404, not the 403 this story's AC text quotes** — verified as the *shipped* 8-9 contract (`EnsureOrphanInCallerScopeAsync` throws `KeyNotFoundException` → 404, not-leak semantics, pre-existing code); the AC's "403" was the story-time expectation. Behaviour preserved, not changed.
- **AC-3 (HQ narrowing)** — `?charityId=B` returns that charity's rows only; no param → all rows. Live-verified on a two-charity batch.
- **AC-4 (row ledger + grid)** — `OrphanPaymentItemDto.CharityId` int?→Guid?; `MapOrphanPaymentItemDto` populates it from the orphan's charity; `CharityName` joined batch-loaded (one charity query per grid, no N+1 — verified live: both rows carry names). Disbursement columns were already on the wire from 10-2/10-3; the detail grid now renders the §15.S.3 set with blank-when-null + `trackOrphan` trackBy + badges (stopped/received/printed; حالة الصرف maps the 10-2/10-17 contract 0=Pending/1=Executed/2=Failed via `exchangeStatusKey/Class`, null renders dash, never fake values). Grid keeps the الجمعية column beyond the story's list — it is this story's defect-2 join surface and the only place it renders.
- **AC-5** — no token → 401 (SPA routes to login); refusals map through the standard catch set (400 `{message}` / 404).
- **Epic-1 note** — `POST /api/UserManagement` now returns **201** with roles assigned; the 500-after-insert defect recorded in 10-6 appears fixed by a parallel session. Role pin re-asserted idempotently via SQL regardless.
- **Env note** — `npx tsc --noEmit` shows 2 syntax errors in `families/refugee-family-detail/*.spec.ts` — a parallel session's **untracked, actively-being-written** file (content changed between two reads; `??` in git). Zero errors in any EP-10 file; the long-running `ng build` may fail on the same foreign file.

**Smoke evidence (live, 2026-08-24):** batch `SMOKE-10-7` (id `8a0708a2-…1877c`) with one charity-A + one charity-B orphan. HQ full → 2 rows, both with `charityId`+`charityName` populated, ledger columns null-blank; HQ `?charityId=B` → 1 row; unknown batch → 404. Charity(B) no-orphanId → 200 shared header + own row only; Charity(B)+own orphanId → 200 single row; Charity(B)+A-orphan → 404 (not-leak, shipped 8-9 behaviour); Charity(C, zero rows) → 200 shared header + `rows: 0`; no token → 401; HQ+orphanId → 200 single row. Cleanup: batch soft-deleted (204), test user removed. Debug `IIROSA.sln` Release build 0 errors; FE `tsc --noEmit` green for EP-10 scope (see env note). Tests excluded per standing decision.

### File List

| Layer | File | Change |
| --- | --- | --- |
| BE | `IIROSA.Application/Services/OrphanPaymentService.cs` | charity-dimension filter branch + batch-loaded name join in `GetPaymentGroupDetailsAsync`; `MapOrphanPaymentItemDto` populates `CharityId` |
| BE | `IIROSA.Application/Interfaces/IOrphanPaymentService.cs` | `GetPaymentGroupDetailsAsync` `charityId` param |
| BE | `IIROSA.Application/DTOs/OrphanPayment/OrphanPaymentItemDto.cs` | `CharityId` int?→Guid? |
| BE | `IIROSA.Api/Controllers/OrphanPaymentsController.cs` | `{id}/details` roles widened (+Accountant, FinancialOfficer, Charity); `charityId` HQ-only param; blanket Charity Forbid → D4-only |
| FE | `orphan-payment-detail/…component.html` | §15.S.3 grid column set, blank-when-null, badges, updated empty-row colSpan |
| FE | `orphan-payment-detail/…component.ts` | `exchangeStatusKey`/`exchangeStatusClass` helpers (0/1/2 contract) |
| FE | `assets/i18n/ar.json`, `en.json` | 13 keys × 2 languages (stopSpend, orphanCode, guardianName, receiptPrintStatus, received, printed, chequeNumber, chequeDate, transferNumber, exchangeStatus, exchangePending/Executed/Failed) |

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
- 2026-08-24 — implemented: charity-level details read (pin-never-widen), Guid CharityId + batch-loaded name join, §15.S.3 detail grid, i18n; live smoke all-green → review.
- 2026-08-26 — code review remediation (P16/P19): per-charity counts coalesce Family tenancy, single-orphan (UC-ORP-09) mode now joins charity names. Build verified. → done.
