# Epic 10 acceptance audit — Acceptance Auditor (C)

Reviewer scope: stories 10-1..10-9 of EP-10 (Orphan Payments & Disbursement).
Input artifact: `D:\Osama\Projects\IIROSA Charities New Version\_bmad-output\implementation-artifacts\review-artifacts\epic10-review-input.diff` (~5,646 lines, read in full) plus working-tree verification of everything the diff does not carry (i18n JSON, migrations, snapshot, repositories, templates).
Context: `docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md`, `docs/Modules/00-Overview-and-Common-Context.md` §4.1/§4.3, `_bmad-output/planning-artifacts/architecture.md` §10 (incl. D1 ratified rulings), epic-8 8-8/8-9 regression contracts.
Accepted rulings NOT re-litigated: raw envelope (15-1), no tests / live-smoke evidence, client-side paging for 10-8, PaymentDate nullable-at-DB with validator-mandatory-on-create, out-of-scope 404 where the story records the shipped 8-9 contract.

Findings: 10 total — 1 High, 3 Medium, 6 Low.

---

## 10-1 — List payment batches

**Finding C10-1-1 — §15.S.1 grid's الرقم column is absent from the list screen**
- **Violates:** 10-1 AC-3 ("rows show الرقم · رقم الدفعة · اسم الدفعة · الفترة · التاريخ · سعر الصرف (§15.S.1 grid)"); §15.S.1 grid row (spec line 149: `الرقم · رقم الدفعة · اسم الدفعة · الفترة · التاريخ · سعر الصرف · تعديل`)
- **Severity:** Medium
- **Evidence:** `Frontend/src/app/modules/orphan-payments/orphan-payment-list/orphan-payment-list.component.html:79-89` — header row renders batchNumber, groupName, paymentPeriod, orphanCount, exchangeRate, currency, isUploaded, groupDate, actions. No serial/الرقم column exists anywhere in the grid; §15.S.1 lists it first.
- **Gap type:** AC-violated (partial — five of the six enumerated columns render; الرقم does not)

Defect-list audit (all five items verified fixed): (1) `OrphanPayments.*` PERMISSION_ROLES block present in `auth.service.ts` matching Task 2 exactly; (2) `GetPaymentGroups` roles widened to `SuperAdmin,Admin,Accountant,FinancialOfficer,Charity`; (3) `CharityId` int?→Guid? swept across DTO+repo chain, charity WHERE reactivated; (4) `ex.Message` stripped from every 500 in the controller; (5) dead endpoint calls stripped from list/form/add-orphans/detail with live-lookup rewiring. AC-2's Charity-without-orphanId `Forbid()` retained and extended with the D4 fail-closed guard (controller lines 47-50). AC-4 empty state and pager present (template lines 123-142).

## 10-2 — Create a payment batch

**Finding C10-2-1 — "List التاريخ column → PaymentDate (fallback GroupDate)" is narrated but not implemented**
- **Violates:** 10-2 Dev Notes field mapping (story line 81) and Completion Notes line 130 ("the 10-1 list already renders التاريخ with a GroupDate fallback"); §15.S.2 → entity mapping for تاريخ بدء التوزيع
- **Severity:** Medium
- **Evidence:** Story 10-2 lines 81 and 130 make the claim. The list template renders only `groupDate`: `orphan-payment-list.component.html:87` (`'orphanPayments.groupDate'`) and `:108` (`{{ formatDate(group.groupDate) }}`). `OrphanPaymentListDto.PaymentDate` is on the wire but never bound by any template. Consequence: the user-entered تاريخ بدء التوزيع (PaymentDate, mandatory on create) never surfaces anywhere; التاريخ permanently shows the batch creation date.
- **Gap type:** claim-unbacked + missing-behavior

**Finding C10-2-2 — Enrolment write (add-orphans + Amount snapshot) has zero execution evidence**
- **Violates:** 10-2 AC-4 (enrol orphans into the batch, BR-17 amount snapshot at enrolment)
- **Severity:** Low
- **Evidence:** Dev Agent Record line 125: smoke DEFERRED to epic-final regression — dev DB has zero Orphan rows and no orphan-creation surface exists. Code is present and compile-verified (`AddOrphansToGroupAsync` batch-loads orphans and sets `Amount = orphan.MonthlyAmount` in the diff), but the add/skip path and snapshot have never been executed. The deferral is recorded, so this flags the epic-final regression as a must-cover, not a hidden skip.
- **Gap type:** claim-unbacked (sanctioned deferral — evidence still owed)

Defect-list audit (verified fixed): available-orphans paged-envelope mismatch, `removeOrphanFromGroup` DELETE shape, dead `/batch-number/next` generation (server auto-generates, live-verified `BP-202608-0001`), roles widened on create/available-orphans/add/remove, `OrphanForPaymentListDto.CharityId` Guid?.

## 10-3 — View a payment batch

No story-specific findings. Details read verified: unknown id → 404 on null (controller), dead statistics path removed from detail, wire contract repaired, grid renders §15.S.3 set with blank-when-null. Cross-story items C10-X-1 (missing `orphanPayments.export` key on this screen) and C10-X-3 (epic-18 print UI riding in this file) are filed under Cross-story.

## 10-4 — Update a payment batch

No findings. `EnsureCanUpdateAsync` charity write-guard present (`Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs:364`); exchange-rate lock enforced on update (lines 379-382, `InvalidOperationException` → 400); typed `LockExchangeRateDto` body; update validators null-conditional (PaymentDate preserved via `?? `, line 390); roles HQ-Fin per §4.3.

## 10-5 — Delete a payment batch

No findings. Dual delete guard verified in the service: disbursed rows (`IsGotIt`/`TransferNo`/`ChiqueNum`) block deletion, and any `PeriodicOrphanReport.OrphanPaymentId` reference blocks deletion (§25.6 A1 pre-disbursement guard + report-link protection); soft-delete rewrite via `IsDeleted`; FE confirm is SweetAlert2 with decline-safe no-op; roles HQ-Fin.

## 10-6 — List a charity's batch numbers

No story-specific findings. Roles aligned on both endpoints (HQ-Fin set + Charity); `GetByBatchNoAsync` participation check → 404 not-leak for non-participating charity; `GetGroupIdsByCharityAsync` verified to filter `!IsDeleted`; form رقم الدفعة selector fed by `batch-numbers` + HQ-only الجمعية filter; the 3 new keys exist in both ar.json and en.json. The §15.S.1 list-screen charity filter gap is filed as C10-X-4.

## 10-7 — View payment details for a charity

**Finding C10-7-1 — Per-charity count dict drops orphans tenanted through the family**
- **Violates:** 10-7's standing tenancy ruling (story line 67: charity dimension derives `OrphanPaymentItem → Orphan → FK_CharityId ?? Family.FK_CharityId` — "the standing tenancy model for this epic"); defect 5's intent (verify the count dicts)
- **Severity:** Low
- **Evidence:** `Backend/src/IIROSA.Infrastructure/Data/Repository/OrphanPaymentRepository.cs:265-273` — `GetOrphanCountByCharityAsync` groups only by `opi.Orphan.FK_CharityId` and then `.Where(g => g.CharityId.HasValue)`. An orphan with null `FK_CharityId` whose tenancy derives via `Family.FK_CharityId` is silently absent from the per-charity counts (soft-delete filtering is correct; the derivation is not). Row filtering elsewhere in the epic uses the full coalesce, so counts can disagree with the filtered rows on the same batch.
- **Gap type:** spec-deviation

Otherwise verified: AC-1 blanket Charity Forbid replaced by item filtering with D4-only refusal and pin-never-widen (`effectiveCharityId` HQ-only); AC-2 byte-identical orphan-scoped branch (out-of-scope 404 = shipped 8-9 contract, accepted); AC-3 HQ narrowing live-verified; AC-4 Guid CharityId + batch-loaded CharityName join (no N+1) + §15.S.3 grid; all 13 i18n keys in both languages.

## 10-8 — List orphans in a batch

No findings. Client-side search/sort/page via `applyGridState` (accepted ruling), `trackBy` present, i18n keys (`searchOrphanHint`, `noOrphansMatch`, etc.) present in both files.

## 10-9 — Stop or resume an orphan's payment

**Finding C10-9-1 — Row-action endpoint has no D4 fail-closed guard; the service scope check no-ops on a null charity claim**
- **Violates:** 10-9 AC-2 (charity acts on own rows only); the epic's own D4 pattern implemented on `GetPaymentGroups` and `GetPaymentGroupDetails`; `architecture.md` §10 D1 ("the tenancy fail-closed ladder (P1) … binding direction, not deviations")
- **Severity:** High
- **Evidence:** `Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:443-456` — `POST orphan-items` passes `GetUserCharityId(), GetUserRole()` straight to the service with no `User.IsInRole("Charity") && GetUserCharityId() == null → Forbid()` guard (contrast the list guard at :47-50 and the details guard at :138-141). `Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs:290` — `if (userRole == "Charity" && userCharityId.HasValue && orphanCharityId != userCharityId.Value)`: when the Charity token carries no parseable charity claim, `userCharityId` is null, `HasValue` is false, and the check is skipped entirely — the caller falls through to the unscoped branch and can stop/resume (and mark printed, action 1) ANY row in ANY charity. Such users demonstrably exist: 10-6's env note records `Charity@IIROSA.com` with `CharityId = NULL`.
- **Gap type:** AC-violated + missing-behavior (tenancy hole)

**Finding C10-9-2 — Validator per-action clauses cover action 3 only, not 3/4**
- **Violates:** 10-9 defect item 2 (story line 44: "per-action required fields (3/4 ⇒ ChiqueNum + BenificiaryName)")
- **Severity:** Low
- **Evidence:** `Backend/src/IIROSA.Application/Validators/OrphanPayment/UpdateOrphanPaymentItemValidator.cs:28-36` — ChiqueNum/BenificiaryName clauses are `.When(x => x.Action == 3)`; no `Action == 4` clause exists. Mitigation: the service default case refuses actions 2..4 loudly with 400 (`OrphanPaymentService.cs:328-330`), so nothing unvalidated can be written until 10-12 fills action 4 — but the story's letter is not delivered.
- **Gap type:** defect-list-not-fixed

**Finding C10-9-3 — Refusal toasts localised for the HQ-stop message only**
- **Violates:** 10-9 defect item 5 / Task 3 ("refusal toasts localised ar/en")
- **Severity:** Low
- **Evidence:** `Frontend/src/app/modules/orphan-payments/orphan-payment-detail/orphan-payment-detail.component.ts:441-444` — error handler regex-matches `/stopped by head office/i` to pick `hqStopResumeRefused`; every other refusal (scope 403 "Orphan payment item is outside the caller's charity scope", validation 400) renders the raw English server message. Self-recorded in the completion note ("other errors show raw").
- **Gap type:** defect-list-not-fixed (partial)

Verified clean: HQ-stop lock via `IUserAppService.GetUsersInRoles` (correct workaround for the broken `IsInRoleAsync` surrogate-PK platform defect), unknown stopper → fail closed; `Epic10_StoppedBy` is exactly one AddColumn (the single sanctioned schema addition); BR-11 deferral and the BR-16 exclusion-field simplification are both sanctioned by the story's own Dev Notes rulings and correctly recorded. Note (no finding): action 1 (mark printed) is fully implemented in the reviewed artifact and wired to `onMarkPrinted` — that is 10-10's delivery, so 10-9's "actions 1..4 refuse loudly" smoke line is stale relative to the diff; judge action 1 under 10-10, not here.

---

## Cross-story

**Finding C10-X-1 — Two i18n keys referenced by shipped templates exist in neither ar.json nor en.json**
- **Violates:** 10-1 Task 4 DoD ("verify every rendered key exists in ar.json AND en.json"), 10-2 Task 4, 10-3 DoD (i18n resolves in ar + en); project rule "no hard-coded/raw UI strings"
- **Severity:** Medium
- **Evidence:** working-tree JSON walk (parse both files, compare all 167 keys referenced under `src/app/modules/orphan-payments/**`): `orphanPayments.age` — referenced `add-orphans-to-group/add-orphans-to-group.component.html:204`, present in neither file; `orphanPayments.export` — referenced `orphan-payment-detail/orphan-payment-detail.component.html:49` and `:443`, present in neither file. Both render as raw keys for users. ar and en each carry 161 `orphanPayments` keys and every story-mandated key checked (paymentDate, batchNumberAutoHint, orphansAddedResult, exchangeRateLockedError, delete* family, stop/resume family, hqStopResumeRefused, full paymentSummary.* block, 10-7's 13 keys, 10-8's search keys) is present in both — the miss is confined to these two legacy references on surfaces this epic rewired.
- **Gap type:** DoD-violated / missing-behavior

**Finding C10-X-2 — Regression contracts verified honored (no defect — recorded for the board)**
- 8-8/8-9 contract: Charity-without-orphanId on `GET /api/OrphanPayments` → 403 retained (controller :47-50); `{id}/details` with orphanId behaves as shipped 8-9 — out-of-scope orphan → 404 (not-leak), zero rows → 404, single-row result; HQ no-orphanId branch unchanged. 10-7's AC-2 "403" text vs shipped 404 is the recorded accepted divergence, not re-litigated.
- One-migration ruling: the diff and tree contain exactly the three sanctioned migrations — `Epic10_PaymentDisbursement`, `Epic10_ReportBatchLink`, `Epic10_StoppedBy`. `PeriodicOrphanReport.FK_HousingFamilyId`/`ChildOrparent` in the snapshot are schema-covered by the parallel `20260824115014_Epic06_HousingReportBeneficiary` migration — no drift attributable to epic 10.
- Board endpoints: `POST /api/OrphanPayments/orphan-items` is the single §15.1 action endpoint (actions 0/1 live, 2..4 refuse loudly); `batch-numbers` and `by-batch-no/{batchNo}` both in 10-6 scope per its board-endpoint ruling.
- Routing: all five orphan-payments routes carry `permission:` values that resolve in PERMISSION_ROLES (View/Create/Edit/AddOrphans) with PermissionGuard — DoD met.

**Finding C10-X-3 — Scope creep riding in the epic-10 artifact (changes belonging to no 10-1..10-9 story)**
- **Violates:** story boundary discipline — these hunks are owned by other epics and were never acceptance-checked here
- **Severity:** Low
- **Evidence (diff):** (a) `PeriodicOrphanReport` gains `ChildOrparent`, `FK_HousingFamilyId`, `HousingFamily` navigation — epic-6/11 housing beneficiary scope (migration covered by Epic06, so schema-consistent, but unreviewed by this epic); (b) detail screen grows 18-29/18-30/18-32-style print buttons, a PaymentSummary band and `getPaymentSummary` — epic-18 / 10-21/10-24 surfaces; (c) `OrphanRepository.IncludeNavigationProperties` adds `.Include(o => o.SocialStatus)` — refugee register §12.S.2 scope; (d) `auth.service.ts` login token-undefined guard and PERMISSION_ROLES/main-layout blocks for other epics' modules — known shared-file adjacency; the `OrphanPayments.*` block itself was verified to match 10-1 Task 2 exactly.
- **Gap type:** scope-creep (informational — route to the owning epics' reviews)

**Finding C10-X-4 — §15.S.1 list-screen الجمعية filter remains unwired and unowned**
- **Violates:** §15.S.1 field table (spec line 143: الجمعية `CharityId` dropdown with كافة الجهات, `getCharityData()` on change)
- **Severity:** Low
- **Evidence:** the list screen's filter form offers search/periods/upload-status only (`orphan-payment-list.component.html:13-55`); no charity dropdown. 10-6 defect 3 wired "at least the form's رقم الدفعة selector" (sanctioned partial) and deferred list/detail/cheques selectors to 10-7/10-22; 10-7 delivered no list filter and no story in 10-1..10-9 claims it.
- **Gap type:** silent non-delivery (epic-slice level — needs an owner story or an explicit board ruling)

---

## Verdict table

| Story | Verdict | Findings |
| --- | --- | --- |
| 10-1 List payment batches | ACs 1,2,4,5 met; AC-3 partial (الرقم column missing); all 5 defects fixed | 1 |
| 10-2 Create a payment batch | ACs met in code; التاريخ mapping claim unimplemented; enrolment-write smoke still owed | 2 |
| 10-3 View a payment batch | All ACs met; defects fixed | 0 |
| 10-4 Update a payment batch | All ACs met; defects fixed | 0 |
| 10-5 Delete a payment batch | All ACs met; defects fixed | 0 |
| 10-6 List a charity's batch numbers | All ACs met; defects fixed | 0 |
| 10-7 View payment details for a charity | All ACs met; 8-9 regression honored; counts derivation gap | 1 |
| 10-8 List orphans in a batch | All ACs met (client paging per accepted ruling) | 0 |
| 10-9 Stop or resume an orphan's payment | AC-1/3/4 met; AC-2 tenancy hole (High); AC-5 partial toast localisation; defect 2 partial | 3 |
| Cross-story | Migration + regression + routing rulings verified; i18n gap; scope creep; unowned list filter | 4 |

Totals: 1 High (C10-9-1), 3 Medium (C10-1-1, C10-2-1, C10-X-1), 6 Low.
