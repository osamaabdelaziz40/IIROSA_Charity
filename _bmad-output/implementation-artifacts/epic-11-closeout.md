# Epic 11 — General Cheques: closeout

All 10 stories (UC-CHQ-01..10) `done` as of 2026-08-20, after a review-and-complete pass over the
copied implementation. Backend compiles with 0 errors; `npm run build` succeeds (only the two
advisory warnings inherited from epic 12: distribution-record SCSS budget, exceljs/sweetalert2
CommonJS). No story files existed; this note is the epic record. Nothing committed.

Spec: `docs/Modules/16-UC-CHQ-General-Cheques.md` (§16.S screens, §16.U scenarios).

## What the review found and fixed, by story

The copied implementation was written against **invented** use cases — a "cheque workflow" with
Pending/Issued/Cleared/Void statuses, approval, voiding, reconciliation and bank-statement import
that appear nowhere in chapter 16 — while the frontend called a `/api/GeneralChecks` controller
that has never existed on this backend, so every screen was dead. The pass re-cut both sides to
the WAR.IIROSA scope. Only the deltas are listed.

- **11-1 (list cheques)** — `CheckListComponent` rewritten onto `GET /api/CheckManagement` with
  server-side paging; §16.S.1 exact columns (الرقم، رقم الشيك، تاريخ الشيك، اسم المستفيد، المبلغ،
  البنك، تعديل); the charity dropdown renders for head office only — everyone else is scoped by
  the service from the token.
- **11-2 (issue a cheque)** — form rewritten to the §16.S.2 field set: the six mandatory fields
  (بنك، اسم المستفيد، تاريخ الشيك، رقم الشيك، عملة، مبلغ), تعليقات, and the four flags
  (شيك تالف / تم رد الشيك / تم الصرف / CheckDone). The old form's invented mandatory fields
  (due date, payment reason, status) are gone. Beneficiary type-ahead, live تفقيط, and the
  طباعة / طباعة مصري / حفظ commands wired (below). Entity gained `FK_CharityId` + `ChequeType`
  ("Orphans"/"Individuals", default Individuals) + the four flags; uniqueness is now
  number+bank+charity among live rows (filtered unique index), replacing the old global
  number-only index that could never hold two charities' cheque books.
- **11-3 (view a cheque)** — detail screen reads the real `GET /{id}` with Guid ids; shows the
  cheque face data, flags and audit; print + edit entry points.
- **11-4 (update a cheque)** — `PUT /api/CheckManagement` with the id in the body (the legacy
  contract the board records). Full-field update, charity ownership never moves, تفقيط
  regenerated when the amount/currency changes and the client echoed the old words untouched.
- **11-5 (select a beneficiary)** — new `GET /api/LookupManagement/cheque-beneficiaries?term=`
  over the reusable `ChequeBeneficiary` lookup (active rows, Arabic/English match, take ≤ 50);
  debounced type-ahead in the form fills the name + snapshot fields and links
  `FK_ChequeBeneficiaryId`.
- **11-6 (select the currency)** — new `GET /api/LookupManagement/currencies` (distinct
  `Country.Currency`); feeds the currency dropdown, 3-letter codes, upper-cased server-side.
- **11-7 (amount to Arabic words تفقيط)** — new `ArabicAmountInWords` (Application layer):
  proper Arabic grammar (one/dual/plural/tanween/singular agreement, scale words to billions,
  2-dp fractions, currency-specific unit and fraction nouns for EGP/SAR/USD/AED/EUR/YER + a
  generic fallback) wrapped in «فقط … لا غير». Served by `GET /api/CheckManagement/amount-in-words`
  and stamped server-side on create/update when the client doesn't send words. Replaces the old
  `"{amount} {currency}"` placeholder.
- **11-8 (bank print positions)** — `Bank` gained eight nullable mm offsets (ChequeDateX/Y,
  PayeeX/Y, AmountX/Y, AmountWordsX/Y) served by
  `GET /api/LookupManagement/banks/{id}/cheque-positions` with a `configured` flag; the Egyptian
  print layout consumes them (CSS mm units map 1:1), falling back to a default layout.
- **11-9 (cheque statement)** — `GET /api/CheckManagement/report` returns items + per-currency
  totals computed over the whole filtered set. New `check-statement` component (replaces the
  invented `check-reconcile` screen, deleted): §16.S.3 fields — charity (HQ), bank, date range,
  the شيكات إيتام / شيكات أفراد radio — plus بحث and طباعة. The documented routing bug
  (`reconcile` shadowed by `:id`) is fixed by construction: `statement` is declared before `:id`.
- **11-10 (print)** — client-side printing per the epic-12 precedent (jsPDF is not a dependency;
  the server pdf endpoint the board note names was never implemented): طباعة = formatted cheque
  card, طباعة مصري = stationery-positioned leaf via 11-8, statement prints via `window.print()`.
  No print-flag column — the spec's conditional "where the module records printing" doesn't apply.

Cross-cutting: `CheckService` rebuilt on the SeasonalAid pattern — FluentValidation in the service,
`ICurrentUserService` tenancy (charity user pinned to their charity, HQ optional explicit charity,
HQ country users constrained to their country's charities, unscopeable callers see nothing), only
`IUnitOfWork` saves, soft-delete relied on the global query filter. Controller re-cut to six
endpoints with per-action roles; `PERMISSION_ROLES` (`GeneralChecks.View/Create/Edit`) + route
`data.permission` + sidebar aligned to the same sets. The `generalChecks` i18n block rewritten in
both languages — invented workflow keys (statuses, void, clearance, reconciliation) removed,
spec keys added; no hardcoded strings. Migration `20260820063719_Epic11_GeneralCheques` (Check +
Bank only — no drift to trim this time) created and applied to `IIROSA_Db_Dev`.

## Deliberately not modelled (recorded decisions)

- **Cheque workflow statuses / approval / voiding / reconciliation / bank-statement import** —
  not in chapter 16; the invented columns (CheckStatus, IssueDate, DueDate, ClearanceDate,
  BankReference, VoidDate/Reason/Notes, RequiresApproval, ApprovedBy, ApprovalDate,
  PaymentReason/PaymentDescription, FK_CheckImageId) were dropped, not preserved.
- **Roles** — reads `SuperAdmin,Admin,Accountant,FinancialOfficer` (Financial Director + General
  Director); writes `SuperAdmin,Accountant,FinancialOfficer`. `Charity` holds no cheque screen in
  the spec, so it is not admitted — but the service still scopes rows by the token's charity claim
  (defence in depth, and the module stays ready if a charity cheque surface is ever requested).
- **"Not more than 1 month in the past" check-date rule** (in the copied form) — not in the spec
  and it blocks legitimate back-dating; removed.
- **Server error strings stay English** (platform-wide, `deferred-work.md`); the form maps 400
  `errors` maps onto controls, everything else surfaces i18n keys.
- **Tests excluded** by standing user decision (epics 3/13/12 precedent).

## Outstanding before release

1. **Live walkthrough** — API + UI run under my session for build/migration only; a logged-in
   walkthrough (SuperAdmin issuing a cheque, Egyptian print against a bank with offsets
   configured, HQ statement across charities) has not been performed with real credentials.
2. **Bank print offsets have no admin UI** — the eight Bank columns are settable only by SQL;
   the bank CRUD (epic 14 lookup screens) doesn't expose them yet. The Egyptian print falls back
   to the default layout until they're filled.
3. **Identity migrations pending** (see `epic-3-closeout.md` — unchanged by this epic).
4. `deferred-work.md` ApiResponse<T> migration — this epic's controller follows the reviewed
   ControllerBase + `{ message }` pattern like the other 20, recorded there.
