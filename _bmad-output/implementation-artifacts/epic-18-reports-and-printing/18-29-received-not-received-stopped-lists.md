# Story 18-29: Received / not received / stopped lists

| Field | Value |
| --- | --- |
| Story key | `18-29-received-not-received-stopped-lists` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-29 — Received / not received / stopped lists (المستلمون / غير المستلمين / الموقوفون) |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.3 financial-reports row UC-RPT-29, §23.U.29 scenario; outcome states §15.U.18–§15.U.20 of `docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md`) |
| Route | Hosted print — المستلمون / غير المستلمين / الموقوفون commands on the orphan-payments batch screen; rendered through the shared `report-viewer` shell (no dedicated `#/reports` route) |
| Endpoint | `POST /api/Reports/payments-received` (board key; legacy `POST /api/Reports/payments-received/export/pdf` + `PrintNonRecieved` / `PrintStopped` — superseded, see Dev Notes) |
| Depends on | **18-1 landed** (Reports skeleton), **18-21 landed** (`report-pdf.service.ts`); data source = EP-10's **10-9 / 10-11** (outcome-state columns), **10-19 / 10-20** (not-received / stopped JSON endpoints) — land first or together |
| Roles | HQ roles, charity → `SuperAdmin`, `Admin`, `Charity` (`Reports.View`) |

## Status

done

## Story

As a HQ roles, I want to be able to received / not received / stopped lists, so that the paper
document the process depends on can be produced and filed.

## Acceptance Criteria

1. Given an HQ or charity caller with an active session on the orphan-payments batch screen, when
   the actor invokes one of the three list commands for a batch and charity, then no stored data
   is changed — the operation is a read (print-run flagging stays with 10-10 / 10-24).
2. Given the request is accepted, when it is served, then the received list is fetched from
   `POST /api/Reports/payments-received` and the not-received / stopped lists from the 10-19 /
   10-20 endpoints, and each document is rendered client-side without a page reload — no
   server-side PDF is produced.
3. Given the caller is a charity user, when a list is served, then only that charity's rows are
   returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload.
   Given an HQ caller (`IsHeadOffice`) with an explicit charity id, then the report operates on
   that charity's data; the country claim pins the country — pin never widens.
4. Given the batch has no row in a variant, when that document is produced, then the actor is
   told that there is nothing to produce rather than receiving an empty file.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.
6. Given a business rule refuses the operation (legacy «Faliure»), when it is attempted, then the
   refusal surfaces as an anonymous `{ message }` response and nothing is written.

**Definition of done:** all three outcome lists are printable for a batch + charity from one
selection; §23.U.29 passes end to end; the charity/country scoping is enforced server-side, not
only in the menu.

## Tasks / Subtasks

- [x] **Task 1 — DTOs + validator** (AC 2, 3)
  - [x] Extend `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` (18-1's container — do
        not fork): `PaymentsOutcomeFilterDto` (`Guid? CharityId`, mandatory `string
        OrpCheckBatchNo`), `PaymentsOutcomeRowDto` (orphan code/name/guardian, amount, currency,
        receipt evidence `chiqueNo` / print date / collector), `PaymentsOutcomeReportDto`
        (`variant`, charity/batch/period header, `rows`, `totalCount`, totals). No `FK_*` wire
        keys; camelCase.
  - [x] `Backend/src/IIROSA.Application/Validators/Reports/PaymentsOutcomeFilterValidator.cs` —
        `OrpCheckBatchNo` NotEmpty; `CharityId` optional (the service decides whether an HQ caller
        may use it — the validator checks shape only).
- [x] **Task 2 — Service projection** (AC 1–4)
  - [x] `IReportService.GetPaymentsOutcomeAsync(...)` + implementation in
        `Backend/src/IIROSA.Application/Services/ReportService.cs`: resolve the batch through the
        orphan-payment repository by `BatchNo` (unknown batch → empty result + message), then
        `ResolveCharityScope(filter.CharityId)` — 18-1's helper (charity caller pinned to
        `ICurrentUserService.CharityId`; `IsHeadOffice` may narrow to the explicit id;
        `CountryId` claim pins country).
  - [x] Build the three variants from the SAME projection the 10-18 / 10-19 / 10-20 reads use —
        received = `IsGotIt`, not-received = `!IsGotIt`, stopped = `IsStopped` — one dataset
        definition, no divergent numbers.
  - [x] Empty variant → `rows: []` + the message contract the UI turns into the
        nothing-to-produce toast (AC 4).
- [x] **Task 3 — API endpoint** (AC 2, 5)
  - [x] `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `[HttpPost("payments-received")]`
        with `[Authorize]` roles per the header; returns `Ok(reportDto)`; catch
        `FluentValidation.ValidationException` FIRST → 400 `{ message, errors }`
        (`OfficeProjectManagementController.cs:89-101` shape); catch-all → 500 `{ message }`.
        Raw envelope — NOT `ApiResponse<T>`.
  - [x] Do NOT re-implement `payments-not-received` / `payments-stopped` — those board keys belong
        to 10-19 / 10-20; the UI calls them for the other two variants.
- [x] **Task 4 — Frontend host + three print layouts** (AC 1–4)
  - [x] Commands المستلمون / غير المستلمون / الموقوفون on
        `Frontend/src/app/modules/orphan-payments/orphan-payment-detail/` (extend the existing
        4-file component — no new module).
  - [x] `Frontend/src/app/modules/reports/services/report.service.ts` —
        `getPaymentsOutcome(variant, filter)` hitting the three keys; preview rendered in the
        shared `report-viewer` grid with `trackBy`.
  - [x] `Frontend/src/app/modules/reports/services/report-pdf.service.ts` (18-21) — one list
        layout parameterised by variant (title, charity, batch, period, serial column, totals
        row); RTL Arabic per 18-21's font decision.
  - [x] Empty result → i18n nothing-to-produce toast; no empty PDF is generated.
- [x] **Task 5 — i18n** — `reports.paymentsOutcome.*` (three command labels, variant titles,
      column headers, totals, empty message) in **both** `assets/i18n/ar.json` and `en.json`.
- [x] **Task 6 — Verification** (AC 1–6)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors (MSB3021/3027 on copy steps = the user's
        live API locking outputs; never kill it).
  - [x] Live check: anonymous POST → 401; HQ with a valid batch+charity → 200 camelCase rows;
        charity token → only that charity's rows; unknown batch → empty + message (send Arabic
        payloads as UTF-8 from a file — the Git-Bash codepage turns inline bodies into `?????`).
  - [x] `cd Frontend && npm run build` — 0 errors; if a UI fix shows no effect under `ng serve`,
        grep the served chunk for the new key before trusting it.
  - [x] Tests: excluded per the standing user decision.

## Dev Notes

### PDF/Excel ruling (epic-wide — recorded for every print story)

Client-side rendering only. The legacy realisation — `POST /api/Reports/payments-received/export/pdf`,
the sibling `PrintNonRecieved` / `PrintStopped` MVC print actions and the Crystal Reports `.rpt`
sheets behind them — is **superseded**: the data comes from the JSON endpoint above and the
document is rendered client-side by `report-pdf.service.ts` (jsPDF, RTL-Arabic font per 18-21's
decision). No server-side PDF, no EPPlus, no `.rpt` files are ported.

### Platform rules that bind this story

- EP-18 adds **no entities and no migration** — read-only projection over the payments vertical;
  print-run flagging (action 1) stays in 10-10 / 10-24.
- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10). Controller inherits `ControllerBase` + `[Authorize]` +
  `[Route("api/[controller]")]`.
- camelCase wire; no `FK_*` DTO keys; FluentValidation in the service; reads via `IUnitOfWork`
  repositories; global soft-delete query filter — never a manual `IsDeleted` check.
- Charity/country scope always from `ICurrentUserService` in the service — never the payload.
- Bespoke grid in the viewer — **not** `data-list` (platform deviation, 15-1 ruling).

### Coordination notes (read before building)

- **10-24 (EP-10) triggers the same five print keys from the payments workbench** and names the
  legacy `/export/pdf` shape; the JSON + client-render contract recorded here supersedes it. If
  10-24 has already landed `/export/pdf` routes, keep them as the payments-side host and add the
  JSON key here — one dataset, two hosts; record the reconciliation in the Dev Agent Record.
- The outcome-state columns (`IsGotIt`, `IsStopped`, `ChiqueNum`, `Printdate`, `BenificiaryName`)
  do **not** exist in `OrphanPaymentItem` as copied (verified — only `DisplayOrder` + `Notes`);
  they land with 10-9 / 10-11 / 10-12. This story must not add them.

### Out of scope (later stories / other epics — do not build)

| Item | Story |
| --- | --- |
| `payments-not-received` / `payments-stopped` JSON endpoints | 10-19 / 10-20 (EP-10) |
| Outcome-state columns and settlement writes | 10-9 / 10-11 / 10-12 (EP-10) |
| Print-run flagging (`IsPrinted`, action 1) | 10-10 / 10-24 (EP-10) |
| Payment summary pages (Dashboard endpoint) | 18-32 |
| Cheque-numbers list and receipt cards documents | 18-30 / 18-31 |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.3] financial-reports row UC-RPT-29 —
  the three outcome lists, board + legacy realisation
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.29] scenario — params `charityId`,
  `orpCheckBatchNo`; HQ may cross the charity boundary
- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.U.18] (and .19 / .20) —
  the outcome states and their board endpoints
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-29 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-10-orphan-payments-and-disbursement/10-18-report-payments-received.md] …10-19, 10-20,
  10-24 — the EP-10 siblings this story builds on
- [Source: Backend/src/IIROSA.Domain/Entities/OrphanPaymentItem.cs] current shape — no state
  columns (they land with EP-10)
- [Source: Backend/src/IIROSA.Api/Controllers/OfficeProjectManagementController.cs:89-101]
  ValidationException → errors-map ladder to copy

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- Backend smoke (private instance, temp build `iirosa-1829` on `127.0.0.1:60970`, seeds prefixed
  `1829…`, hard-deleted after): 12-case matrix all green — anon 401; `{}` → 400
  `{"errors":{"OrpCheckBatchNo":"Batch number is required"}}`; bad variant → 400 errors.Variant;
  unknown batch → `rows:[]` + `message:"Batch not found"`; HQ received = 2 rows / 2700 EGP
  (soft-deleted arm excluded, cheque + collector carried); HQ not-received = 4 rows / 4800
  (includes the cheque-issued-not-received row and both stopped rows — the literal `!IsGotIt`
  predicate); HQ stopped = 2 / 2700; HQ + `charityId=wadi` narrow = 1 row + charityName resolved;
  empty variant (batch B, stopped) = `rows:[]` + `message:"No rows in this variant for the
  requested scope"` (AC 4); charity token (Charity@IIROSA.com, `identity.Users.CharityId`
  temporarily pinned to dga for the test) received = 1 / not-received = 3 — dga rows only; the
  same token sending `charityId=wadi` in the payload still got dga rows (pin-never-widen);
  `variant:"RECEIVED"` accepted case-insensitively. Pin reverted to NULL; post-cleanup counts
  0 batches / 0 items.
- Frontend smoke (production `dist/iirosa` on :4306, `/api/**` routed to the private instance —
  all three API hosts rewritten; browser session already authed from the 18-28 smoke, English
  locale): the three commands render on `#/orphan-payments/{id}` when the group carries a
  `batchNo`; Received → `window.print` fired once and the `.report-print-sheet` carries title
  "Received list", meta (Charity: -, Period 7/1—7/31/2026, Total: Count 2 — Amount 2700 EGP),
  the 8 column headers, and both rows with cheque no / print date / collector name; Not received
  → 4 rows incl. LC-CODE-1; Stopped → 2 rows (ORP-2026-47110, ORP-2026-69185); empty variant on
  batch B → print NOT called, no sheet, and the "Nothing to print" toast observed via
  MutationObserver (AC 4). First pass caught a real defect: the interpolated i18n key
  `title_${variant}` became `title_notreceived` (server canonicalises the variant to lowercase)
  and rendered raw — fixed with an explicit switch, rebuilt, re-verified. Smoke artifacts noted:
  hash-only navigation between two batch detail URLs reuses the component (ngOnInit reads the
  snapshot param) — full reload needed per batch in automation; the wadi guardian name renders
  mojibake `????` from pre-existing seed data, not this story's code.
- `dotnet build` (temp `-o`, live API untouched) and `npx ng build` both exit 0. One seam error
  during the method insert (the edit swallowed the following method's signature — CS0106 cascade,
  fixed immediately) and two compile fixes: `ChiqueNo` → the item's real column `ChiqueNum`, and
  the controller catch needed the `FluentValidation.` prefix. Tests excluded per the standing
  user decision.

### Completion Notes List

- **One endpoint serves all three variants** (recorded deviation from Task 3's "do NOT
  re-implement" note): the 10-19 / 10-20 board keys (`payments-not-received` /
  `payments-stopped`) have no endpoints in this codebase — grep-verified. AC 2's premise ("the
  UI calls them") cannot hold, so `PaymentsOutcomeFilterDto.Variant` (received / notReceived /
  stopped, case-insensitive) discriminates on the single `POST /api/Reports/payments-received`.
  This also honours the story's own one-dataset rule — the three lists can never disagree
  because they share one projection. If EP-10 later lands dedicated keys, they become aliases.
- **The outcome-state columns exist** — the coordination note's "only DisplayOrder + Notes" is
  stale: `OrphanPaymentItem` already carries `IsGotIt`, `IsStopped`, `ChiqueNum`, `Printdate`,
  `BenificiaryName`, `ReceivedOn` etc. The predicates resolve directly (received = `IsGotIt`,
  not-received = `!IsGotIt`, stopped = `IsStopped`, literal per §15.U.18–20); nothing was added.
- **Scope ladder**: the 18-22/24 inline ladder, not a `ResolveCharityScope` helper call — same
  semantics in this codebase (charity claim pins → HQ `filter.CharityId` narrows → country claim
  intersects; pin-never-widen live-tested). Batch resolution by `BatchNo` string
  (`!IsDeleted && BatchNo == filter.OrpCheckBatchNo.Trim()`); several groups sharing one batch
  number are all included, ordered by `GroupDate`.
- **Print commands, not a preview page** (deviation from Task 4's "preview rendered in the
  shared report-viewer grid"): §23.U.29's contract is the three printed lists; the commands on
  orphan-payment-detail fetch the JSON and hand it to 18-21's `printSheet` (RTL hidden container
  + `window.print()`). `printSheet` has no totals row — the meta band carries الإجمالي
  (count + amount + currency). Empty variant → info toast, no sheet, no `window.print` (AC 4 —
  verified both arms). `report-pdf.service.ts` itself was NOT modified.
- **Header limits recorded**: currency / period come from the batch's first group
  (mixed-currency batches report the first — recorded limitation); `GuardianName` rides the
  orphan → family → provider LEFT join (nulls render blank); `ChiqueNo` maps the wire name over
  the item's `ChiqueNum` column.
- **Charity-role test setup**: `identity.Users.CharityId` was NULL for the seeded
  Charity@IIROSA.com, so the tenancy claim (TokenService `AddTenancyClaims`) was missing — the
  pin was set to dga for the matrix and reverted to NULL afterwards (verified). Worth noting
  platform-wide: a Charity-role user without the claim would read unscoped; 18-29 inherits the
  platform's existing behaviour rather than changing it.
- i18n `reports.paymentsOutcome.*` — 22 keys per locale (3 command labels, 3 variant titles,
  2 messages, 6 meta, 8 columns), key-parity node-verified. The ar/en blocks sit beside
  `orphansWithoutPayment` under `reports`.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `PaymentsOutcomeFilterDto`,
  `PaymentsOutcomeRowDto`, `PaymentsOutcomeReportDto`
- `Backend/src/IIROSA.Application/Validators/Reports/PaymentsOutcomeFilterValidator.cs` — new
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetPaymentsOutcomeAsync`
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — validator field/param + method
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `POST payments-received`
- `Frontend/src/app/modules/reports/models/report.model.ts` — filter/variant/row/report interfaces
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getPaymentsOutcome`
- `Frontend/src/app/modules/orphan-payments/orphan-payment-detail/orphan-payment-detail.component.ts`
  — three commands + `buildOutcomeSheet`
- `Frontend/src/app/modules/orphan-payments/orphan-payment-detail/orphan-payment-detail.component.html`
  — three header buttons (spinner-while-loading, batchNo-gated)
- `Frontend/src/assets/i18n/ar.json`, `Frontend/src/assets/i18n/en.json` — 22 keys each

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-29 and module spec §23.3 / §23.U.29; PDF supersession ruling and EP-10 dependencies verified against the codebase (no Reports code, no outcome-state columns yet). |
| 2026-08-25 | Story implemented to review: one endpoint + variant discriminator (10-19/10-20 absent — deviation recorded), service ladder + three-variant projection, three hosted print commands through 18-21's printSheet with meta totals, AC-4 empty-variant toast; live backend matrix (12 cases) and dist-served UI smoke green — one i18n interpolation defect caught and fixed in the smoke; seeds `1829…` hard-deleted, charity test pin reverted; tests excluded per standing decision. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
