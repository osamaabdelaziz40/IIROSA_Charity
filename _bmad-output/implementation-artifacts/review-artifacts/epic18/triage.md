# Epic-18 review — step-03 triage

- **Review mode:** full (all 41 stories, 18-1…18-41, status `review`)
- **Layers run:** 15/15 completed (5 chunks × blind/edge/auditor + i18n + records). Two agents (chunk-3 blind, chunk-3 auditor) hit API 429 mid-run and were resumed to completion — **{failed_layers} is empty**; no layer is missing.
- **Raw findings:** 22+20+~25 (chunk 1) · 19+14+13 (chunk 2) · 18+16+8 (chunk 3) · 24+18+3 (chunk 4) · 2+~20 (chunk 5) ≈ **230 raw → 43 deduped → 40 after dismissals**
- **Verification:** the 4 highest-severity single/dual-source findings were spot-checked against the working tree before classification — all confirmed (see ✅ marks).

Classification buckets: **decision_needed 6 · patch 27 · defer 7 · dismiss 3**

---

## DECISION_NEEDED (6)

| ID | Sources | Finding | Location |
| --- | --- | --- | --- |
| D1 | auditor1+2, records5, blind1 | **Ratify or overturn the recorded deviation set vs the binding CLAUDE.md table.** Raw envelopes on ~30 epic-18 endpoints with 4 error-body shapes mixed *inside one controller*; `DashboardController : ControllerBase` ("17-1 convention"); OnPush omitted in 25/26 screens; data-list/input-fields bypass; AutoMapper effectively unused (1 map, 25+ hand projections); NameAr/NameEn flattened server-side; 403 deliberately unhandled by the auth interceptor (contradicts 18-40/18-41 AC5 as written); client-side print/ExcelJS supersessions; 18-33's in-story AC relaxation (bank omitted → 200). Each is a *recorded story ruling* citing "code wins" — but rulings contradict the binding table, and architecture.md §10 exists precisely to arbitrate this. One decision: ratify platform-wide (and amend CLAUDE.md/architecture §10) or schedule the normalization patch. | ReportsController.cs, DashboardController.cs, 26 screens |
| D2 | auditor3 | **18-14 shipped a full rewrite of `OrphanReportsListComponent` against its own DoD ("audited, not rewritten").** Quick Actions (generate/history/schedule/compare), recent-reports card and its AC4 empty state were deleted; the screen was repurposed to UC-ORR-10 statistics no 18-14 AC asks for; `/api/OrphanReports/history` now has no consumer. Reinstate the entry points or ratify the repurposed screen. | orphan-reports-list.component.{ts,html} |
| D3 | auditor1 | **`POST /api/Reports/{reportKey}/export/pdf` exists although 18-40 recorded "no new backend endpoint"** — built by the parallel epic-5 session for UC-FAM-14; returns JSON, not PDF bytes; two identification-sheet implementations now coexist (ReportService vs ReportSheetService). Keep (epic-5 consumer) or remove/rename? | ReportsController.cs:1193 |
| D4 | blind3, blind1+edge1 | **Cross-epic defects riding the same uncommitted tree — fix now under this review or hand to the owning epics?** (a) `PeriodicOrphanReportService`: `isHqCaller = !IsInRole("Charity")` lets a no-role token self-approve at create; `ReviewReportAsync` has no HQ gate at all (epic 9). (b) `FamilyService` bundle: HQ gates as "Charity" blacklist (null/unknown role = head office); absence=soft-delete children sync destroys live orphans on racing GET→PUT; phone-duplicate check ignores orphan `IsDeleted`; HQ can mint charity-less Regular families invisible to every scoped read; eligibility/code-unique scopes steered by client input; audit actor defaults to literal "HQ"; null-forgiving `!` on designed-null reads; report-number race surfaced as user-retry 400; member-move → sponsor-link-unlink hole (epics 4/8/9). (c) `ArabicAmountInWords` crashes ≥ 10^12 (`Scales[4]`) and overflows > long.MaxValue — cheque-epic file, zero tests (cheque epic). | PeriodicOrphanReportService.cs, FamilyService.cs, ArabicAmountInWords.cs:153 |
| D5 | blind1+edge1 | **One concept, five wire names:** `BatchNumber` / `BatchId` (string) / `BatchNo` / `PaymentId` (Guid) / `OrpCheckBatchNo` — plus trim-inconsistent matching. Canonicalising breaks/aligns both sides of the wire; needs a naming decision. | DTOs/Reports/*.cs |
| D6 | blind1+blind4+auditor1 | **Five permanently-empty UCs ship full UIs around endpoints that can never return rows** (UC-RPT-03/04/07/08/11 — no exclusion column / ended state / Meza entity / project link exists domain-wide; recorded gap rulings 2026-08-24). Ship empty-with-recorded-gap, or pull screens/menus until the domain links land? | ReportService.cs, 5 screens |

## PATCH (27)

| ID | Sources | Finding | Location |
| --- | --- | --- | --- |
| P1 ✅ | blind1+edge1 | **Tenancy ladder fails OPEN in ~18 hand-inlined copies** while the shared `ApplyCharityScopeAsync` is fail-closed: orphan/mother/provider/beneficiary/outgoing/payment-item roots, awaiting-approval & refused clamps, and `DashboardService` — a claim-less non-HQ token reads every charity's data. Add the fail-closed throw to every ladder (consolidate to one helper per root). | ReportService.cs:1089-1112, 2694-2764, 2904-3033; DashboardService.cs:82-105 |
| P2 | blind1 | The one fail-closed helper throws raw `InvalidOperationException` → blanket catch → **500 instead of 403** for claim-less users on `Accountant,Employee`-admitted endpoints. | ReportService.cs:7046-7086, ReportsController.GetNonRenewedReports |
| P3 | auditor1+blind1 | `ReportSheetService.ScopeFamilies` trusts controller-supplied `userCharityId`/`userRole` (string compare, no `IsHeadOffice`); its only pin is `GetUserRole()` → **broken `User.IsInRole`** + first-role-claim-only. Resolve scope internally from `ICurrentUserService`. (File is epic-5's; defect is real in shipped code — fix rides this tree.) | ReportSheetService.cs, ReportsController.cs:1260 |
| P4 | blind1+auditor1 | `AllOrphans` authorised/refused/documented but **never applied** (HQ widened query byte-identical to default); `IsFinishedSponsorship`, `NotExcluded` accepted-and-ignored; contradictory `Excluded`+`AllOrphans` silently returns empty instead of 400. | ReportService.cs GetMissedPayments/GetOrphanData |
| P5 | blind1+edge1 | **Three age formulas in one epic**: filter uses `DateDiffYear` (birthday-blind), display uses `ComputeAge` (DayOfYear — also wrong across leap boundaries), historical sheets use `TotalDays/365.2425`; future DOB / post-as-at births render negative ages. Unify on one formula; null for impossible ages. | ReportService.cs, ReportProfile.cs:102-105 |
| P6 ✅ | blind1+edge1 | `ReportPagedResult<T>.TotalPages` divides by `PageSize` unguarded (sibling DTO guards; comments admit the smoke crash). Guard like `:798`. | DTOs/Reports/Reports.cs:140 |
| P7 | edge1 | `(Page-1)*PageSize` int overflow → negative `Skip` → 500 on huge pages; `DateTo=9999-12-31` + `AddDays(1)` overflows `DateTime.MaxValue` (×5 sites). | ReportService.cs:234-238, 330, 851-853, 1405, 1527, 1825 |
| P8 | blind1+edge1 | Cheque-presence inconsistency: `ChiqueNum == ""` counts as "no cheque" on the dashboard but "cheque issued" in UC-RPT-28 — the row vanishes from both reports. Standardise on `IsNullOrEmpty` everywhere. | DashboardService.cs:70-77, ReportService.cs:997-1003 |
| P9 | blind1 | Variant canonicalised via `ToLowerInvariant()` that can't round-trip the case-insensitive validator; `"notreceived"` falls into the wrong `switch` arm for any caller that skips validation. | PaymentsOutcome path |
| P10 | blind1 | `GroupBy(_=>1)` + nested `Distinct().Count()` in `GetPaymentSummaryAsync` is an EF translation gamble → 500 on SQL Server. Restructure/`AsEnumerable`. | DashboardService.cs |
| P11 | edge1 | Drill-down treats `Guid.Empty` payload CharityId as a real charity narrow → silently empty chase list instead of 400. | ReportService.cs:2510 |
| P12 | edge1 | `batchGroups[0]` indexed after a second no-tracking read — soft-delete race → `IndexOutOfRangeException` transient 500. | DashboardService.cs:70-77 |
| P13 | blind1+edge3 | Unbounded `ToListAsync()` before in-memory group/paging: `GetMissedPayments` (whole national payment table for unconstrained HQ), `GetBeneficiaryFamilies`, `GetFollowUpActivity` (18-13's own addition) — DoS-shaped. Server-side paging. | ReportService.cs, FamilyService.cs:2846-2866 |
| P14 | blind1+edge1 | Guardian/widow identification sheets return the **whole HQ register with no row cap** (PII: national IDs + phones) — the same epic's `ReportSheetService` caps at 5000 with a Truncated flag. Apply the cap. | ReportService.cs:1804-1813 |
| P15 | blind1 | `ResolveCharityNameAsync` uses tracked `GetByIdAsync` without `!IsDeleted` — soft-deleted charity still names itself on sheets (every sibling helper is no-tracking + filtered). | ReportSheetService.cs |
| P16 | blind1 | Hard-coded English user-facing strings (batch-not-found, validator messages, sheet titles, `Relationship="Mother"`) on an Arabic-first platform. | ReportService/ReportSheetService |
| P17 | auditor1+3, blind3 | Controller business logic ×4 (meza variant dispatch, `Math.Max` page clamp, null-check, report-key switch); UC-RPT-15 detail endpoint bypasses FluentValidation; `[ProducesResponseType]` declarations that don't match returned envelopes; 18-13 `GetFamilyFollowUp` untyped `Task<object>` + raw envelope; `GetFamilyFollowUpAsync` dead `orphansQuery` never executed. Move to service, type the returns. | ReportsController.cs, FamiliesController.cs |
| P18 ✅ | blind4+edge4 | **Injected print stylesheet is never removed** — after any report print, `@media print { body > *:not(.report-print-*) { display:none } }` permanently blanks every other print in the app (Ctrl+P anywhere → empty pages); cleanup restores title/sheet only; a `window.print()` throw strands the sheet too. Remove the style in cleanup; try/catch. | report-pdf.service.ts:121-136, 456-465 |
| P19 | blind4+edge4+auditor4 | **Variant switch keeps stale rows** (18-35/18-36): `selectVariant()` swaps the column set without clearing `rows` — print/preview composes the previous variant's projection under the new columns (orphans under widow headers). Clear `rows`/`hasRun` (or re-run) on cross-projection switch. | new-beneficiaries-report, follow-up-sheets |
| P20 | blind4+auditor4 | `family-follow-up-report`: OnPush with **no `markForCheck`** (async results never render) + no `destroy$` (leaks); its spec tests removed APIs (`familyCode`, `printIdentificationSheets`) — compile-dead. Add CDR/takeUntil; rewrite spec to shipped API. | family-follow-up-report.component.{ts,spec.ts} |
| P21 | blind2+4, edge2+4, auditor2 | **Export-engine correctness bundle**: 14 screens compute `totalPages` from the last search's `totalCount` while each page re-reads the live filter (edit-after-search → silently truncated/padded workbook); filters not locked during the walk; single-page exit when `totalCount` missing; "nothing to export" on an empty page with positive count; mid-walk failure discards collected pages; `spread` RangeError; sheet titles >31 chars corrupt the workbook; whitespace numerics export as 0. Snapshot the filter, lock the form, harden the walk. | report-export.service.ts + 14 screens, report-viewer |
| P22 | blind4+edge4 | Print/preview builds documents from the **loaded grid page only** while the meta band prints full `totalCount` — misleading partial documents, no truncation warning (4 print screens; `charity-payment-tracking` same defect in data form: >500 updates silently vanish). Warn or fetch-all before print. | follow-up-sheets, new-beneficiaries-report, family-orphans-by-date, cheque-statement, charity-payment-tracking |
| P23 | blind4+edge4 | `family-orphans-by-date`: export feeds the engine flattened orphan `items` with the **family** `totalCount` (walk math incoherent); zero-orphan families are omitted from printed/exported sheets though the grid shows them. | family-orphans-by-date-report |
| P24 ✅ | blind3 | **Double-`/api` in production**: `report.service.ts` builds `${environment.apiUrl}/api/Reports` verbatim while prod `apiUrl` already ends in `/api` — the sibling periodic service strips it for exactly this reason. Every EP-18 call 404s in prod. | report.service.ts:18, 209 |
| P25 | blind3+edge3+auditor3 | Meza bank extract silently truncates at 200 rows while its own contract says "the FULL selection" (totalCount in hand, no warning); meza page-change race lets a slow older page overwrite the newer one. | meza-cards-report.component.ts:139-216 |
| P26 | blind3+4, edge3+4, auditor4 | **Silent filter-option caps** across ~20 screens: charities 500, campaigns 100 (next/prev dead-ends; campaign 101+ unreachable), batches 100, SingleFamily families 500 (18-37 AC7: family #501 unselectable — a required input). Warn on truncation; page or lazy-search. | ~20 screens incl. widows-sponsorship (12k charities), orphan-reports-list, campaign-report |
| P27 | auditor3+blind4+edge2+4 | **Screen-level defect bundle** (unambiguous fixes): 18-10 charity filter bound to charity *name* + unfiltered breakdown tables; `charity-payment-tracking` HQ-only rule ignored (dropdown for everyone, dead `authService`); `orphan-payment-detail` swallows non-404 to a blank screen / no `takeUntil` / write buttons bypass the `canModify` lock / null-amount sort / unformatted money; `missed-payments` lexicographic batch sort + page-scoped column set; BS4 classes (`mr-1`, `badge-*`) in BS 5.3 + dead `[description]` binding; synchronous `printing` flags that can never paint (×5); `orphan-files` 20 parallel thumb fetches with swallowed failures + receipt-card unhandled rejection + hyperlink fallback that 401s by design + silent 40-image budget; UTC-vs-local date stamps (filenames, footer); `cheque-statement` 200-cap with no pager + `as any` casts; `family-orphans-entries` page 2 always fetches page 1; review-jump button ungated in `reports-awaiting-approval`; hard-coded '✓'/'—' glyphs + decorative `notExcluded` checkbox + age label bound to `isFinishedSponsorship`; `setErrors` clobbering server reasons; query-param pre-selection race; `orphan-reports-list` subscription leak; preview modal hardening (print-before-load, empty-source state, blob-URL leak on destroy, filename chars, sync producer throw); menu/route permission mismatches (role-gated menu vs permission-only routes ×4, `family-orphans` nested under `Reports.View`) + 3 routes with no menu entry (cheque-statement, new-beneficiaries, family-orphans-by-date); dead-disabled عرض button vs promised `*ngIf`; double serial column; boolean columns inventing facts from null (`chequeType`, `changeKind`); variant sets sharing one sheetKey (indistinguishable files); registry collision assert; orphan-status bare i18n keys; 18-40 AC4 server message swallowed by generic toast; missing campaign-report spec; 6 dead `reports.print.*` keys + duplicate `exportFailed` naming; story File-List corrections (16 defects: wrong paths ×3, non-rooted ×2, unexpanded globs ×8, omissions ×2, task-claimed-but-unlisted ×1; 18-8 story-text falsehood about a global soft-delete filter). | see chunk 2-5 findings files |

## DEFER (7)

| ID | Sources | Finding | Rationale |
| --- | --- | --- | --- |
| DF1 | blind1+auditor1 | Guardian-change report semantics (prior-guardian columns always null; ChangeDate synthesized from `UpdatedOn`; two widow discriminators). | Recorded ruling; needs a Provider audit-trail feature — domain work, not a patch. |
| DF2 | auditor1 | Recorded UC limitations bundle: UC-RPT-20 omitted columns; UC-RPT-22 reason null; UC-RPT-38 zero-attachment-only v1 semantics; UC-RPT-29 stopped-counts-as-not-received + mixed-currency batch. | Recorded scope debt; each needs new persisted state or spec revision. |
| DF3 | blind1+blind2+auditor2 | Structural debt: `AttachmentService` concrete injection; 40-parameter `ReportService` constructor (the mechanism behind silent validator non-wiring); four coexisting export paths; cross-module type imports contradicting the shim rule; ~15 drifted scope-ladder copies (root cause of P1 — consolidate when touching it). | Refactoring debt without behavioral defect; bundle with P1/D1 execution. |
| DF4 | auditor1 | Charity entity lacks NameAr/NameEn — report output can't be bilingual at the charity axis. | Schema-level debt; needs entity + migration + data fill. |
| DF5 | auditor1 | UC-DSH-01/02 (dashboard summary/charts) unimplemented — DashboardController hosts only payment-summary. | Out of epic-18 scope; scope note for the parent epic. |
| DF6 | records5+blind3/4 | Zero test execution across the epic (standing user decision; all 26 specs are should-create stubs; the one real spec is compile-dead → P20). | Standing decision — resurfaced for the record, not re-litigated here. |
| DF7 | records5 | 18-31/18-32 live-verification claims are data-limited (zero disbursement batches in dev DB); 18-2 batched its smoke into a group run. | Disclosed in-story; needs a populated-DB re-smoke when data exists. |

## DISMISS (3)

1. chunk3-blind #1 "promised screens largely missing from the diff" — **chunking artifact**; the screens exist and were reviewed in chunk 4 (orchestrator note in the findings file).
2. chunk2-blind "no translation resource changes ship" — **chunking artifact**; i18n was scoped to chunk 5 and audited clean (650/650 keys both locales, 0 referenced-but-missing, 0 hard-coded strings).
3. chunk2-blind "function-typed inputs invite broken `this` bindings" — the concrete defect was **found and fixed in-story** (all 12 producers converted to arrow properties, verified by the chunk-4 auditor); residual type-signature laxity is a design note, not a live defect.

---

## Clean areas (for the summary)

i18n fully clean (parity, usage, placeholders, no hard-coded strings) · story status honesty clean (41/41 complete records, 0 unchecked boxes, deviations disclosed) · 4-file shape, `trackBy`, lazy routes + AuthGuard + PermissionGuard + `data.permission`, `[Authorize]` on all 31 actions, explicit soft-delete filtering, DTO-only boundary, server-side tenancy in the main `ApplyCharityScopeAsync` ladder, zero `SaveChanges` in report services · BR-11 window semantics shared correctly across 18-15/18-19/18-36 · 18-33/18-27/18-26 reuse instead of rebuild · all 12 export producers arrow-bound.

---

# Step-04 execution record — 2026-08-26

Executed under the user's batch authorization ("proceed review epic 18"): recommended resolutions for
all 6 decision items + all 27 patches, in 4 waves. Fix sites carry `// Review P<x>/D4 2026-08-26:`
comments.

## Decisions as executed

| ID | Resolution | As executed |
| --- | --- | --- |
| D1 | Ratify platform-wide | `architecture.md` §10 gains the rulings table **"Ratified platform rulings — epic-18 review, 2026-08-26 (§10 D1)"** (R1–R9): raw envelopes (mixed shapes normalized in patch), `DashboardController : ControllerBase`, OnPush omission, data-list/input-fields bypass, hand projections, NameAr/NameEn flattening, 403 interceptor unhandled, client-side print/ExcelJS, 18-33's AC relaxation. Explicitly NOT ratified: English server strings (P16, deferred), and the tenancy ladder stays binding direction. |
| D2 | **REFUTED — ratify the consolidated screen** | The announced "reinstate Quick Actions" premise did not survive verify-before-change: the repo has a single baseline commit (no pre-rewrite state exists to reinstate), all four Quick Actions have live screens elsewhere, and `/api/OrphanReports/history` has a live consumer (`orphan-report-history.component.ts:78`). Resolution honestly flipped from the announced option to **ratify 18-14's consolidated screen + record the refutation**. No build work. |
| D3 | Keep the endpoint | `POST /api/Reports/{reportKey}/export/pdf` (`ReportsController.cs:1193`, epic-5's UC-FAM-14 path) kept — live caller found; coexistence with 18-37's JSON endpoint recorded. |
| D4 | Fix security now, hand semantics to owners | **Fixed in this tree:** `ReviewReportAsync` HQ gate (Charity-role caller → `UnauthorizedAccessException`) + submitter-cannot-review-own-submission block + the controller `Forbid` catch (`PeriodicOrphanReportService.cs:534`, `PeriodicOrphanReportsController.cs`); `FamiliesController.GetProvider` bare `[Authorize]` → `[Authorize(Roles = "SuperAdmin,Admin,Charity")]` (the one fail-open action); phone-duplicate check now filters soft-deleted HOLDERS (`!Father/Mother/Provider.IsDeleted`, `Orphans.Any(!IsDeleted …)`). **Handed off** (`deferred-work.md`): no-Blacklist-entity gate; FamilyService data-semantics bundle (epics 4/8/9); `ArabicAmountInWords` ≥10^12 (cheque epic). **Residual, recorded:** service-side gates block the `Charity` role — full fail-closed for null/unknown roles rides the epics-4/8/9 handoff; today's control is the `[Authorize(Roles=…)]` attribute on every action (all present after the GetProvider fix). `ApplyCreateReviewFlagsAsync`'s `!IsInRole("Charity")` negation stands with its recorded 2026-08-24 semantics. |
| D5 | Keep the five batch-key wire names | Trim-only matching patch applied; `Task<object>` re-typing rides D5 (wire change needs the frontend co-change). |
| D6 | Keep the five empty-UC screens | Ship empty-with-recorded-gap; the 2026-08-24 gap rulings stand. |

## Patches as executed (27 triaged → 22 applied · 2 deferred · 3 applied-with-refutations)

**Wave 1 — backend security (P1, P2, P3, P4, P8, P11, P12):** fail-closed throw added to every
hand-inlined tenancy ladder + `DashboardService` (P1, ✅-confirmed); fail-closed path now surfaces
403 not 500 (P2); `ReportSheetService.ScopeFamilies` resolves scope internally from
`ICurrentUserService` (P3); `AllOrphans`/`IsFinishedSponsorship`/`NotExcluded` filters applied,
contradictory `Excluded`+`AllOrphans` → 400 (P4); cheque-presence standardized on `IsNullOrEmpty`
(P8); `Guid.Empty` payload CharityId → 400 (P11); post-race `batchGroups[0]` index guarded (P12).

**Wave 2 — backend correctness (P5–P7, P9, P10, P14, P15, P17-partial):** one age formula + null
for impossible ages (P5); `TotalPages` divide guarded (P6, ✅); `Skip`/`DateTime` overflow guards
×5 sites (P7); variant canonicalisation round-trips the validator (P9); `GroupBy(_=>1)`
restructured off the EF translation cliff (P10); guardian/widow ID sheets capped at the row
ceiling with a `Truncated` flag on `GuardianIdentificationSheetDto` (P14, `Reports.cs:1479`);
soft-deleted charity no longer names itself (P15). **Reclassified to defer:** P13, P16 (see
`deferred-work.md`). **P17:** UC-RPT-15 detail endpoint re-typed to a DTO + new
`OrphansMissingReportsDetailValidator`, controller logic (Math.Max clamp, null-check, report-key
dispatch) moved to the service; **2 sub-findings REFUTED** (`[ProducesResponseType]` declarations
match the actual raw envelopes; `GetFamilyFollowUpAsync`'s `orphansQuery` is live — the totals mode
uses it); `Task<object>` re-typing deferred (D5).

**Wave 3 — frontend engine/print (P18–P26):** print stylesheet removed on cleanup + `window.print`
try/catch (P18, ✅); variant switch clears stale rows (P19); family-follow-up OnPush fixed
(`destroy$`, `takeUntil`, `markForCheck` ×6) + spec rewritten to the shipped API — 9 tests
(P20); export filter snapshot + engine walk hardening (footer `totalCount || collected.length`,
empty-page-with-advertised-total = truncated signal, maxRows/page caps) (P21); truncation warnings
before print/preview — family-follow-up here, 5 more screens in wave 4 (P22); family-orphans-by-date
stops carrying the family-domain totalCount on flattened orphan rows (`totalCount: 0` →
walk-until-empty) + zero-orphan families kept as family-only rows (P23); double-`/api` in prod
(P24, ✅); meza page-change guard + extract truncation warning (P25); capped-lookup sweep —
**27 sites in 24 files** warn via `reports.lookup.truncated` (number-free wording, both locales);
5 plain-array loaders correctly skipped (P26).

**Wave 4 — screens + records (P22-remainder, P27) — background agent + records pass:**
P22-remainder in 5 screens (follow-up-sheets, new-beneficiaries-report, family-orphans-by-date,
cheque-statement, charity-payment-tracking's data form). **P27: 22 sub-items FIXED across 31
files** (`// Review P22/P27 2026-08-26` tags) — 18-10 charity filter binds Guid ids + recomputed
breakdowns; charity-payment-tracking HQ gate; orphan-payment-detail failure toasts + `takeUntil` ×7
+ `canModify` on write buttons + null-amount sort + money formatting + BS4 sweep; missed-payments
numeric batch sort + accumulating column set; BS4→BS5 sweep (7 files); synchronous print flags
deferred via `setTimeout` (5 screens); orphan-files batched thumbnails (4 at a time) + absolute-URL
hyperlink rule + embed-budget warning; report-preview hardening (print-before-load, empty
re-preview, blob-URL revoke, filename sanitize); report-viewer عرض `*ngIf` + producer error
surfacing + `runExport` try/catch; menu/route alignment (Families.FollowUp, 4 redundant role gates
dropped, 3 missing menu entries); report-columns registry rebuilt from a `REPORT_SETS` array that
throws on collision + variant sheetKeys + null-guarded booleans + i18n-key factory; 18-40 AC4
server messages on `setErrors`; `notExcluded` checkbox wired; `نعم/لا` for booleans;
`destroy$` on orphan-reports-list; page params sent by family-orphans-entries; query-param
pre-selection race fixed; review-jump `canReview` gate. **3 agent REFUTATIONS:** receipt-card
producer rejection — the shell's `openPreview` already chains `.catch()`; orphan-reports-list
`[description]` — a live `EmptyStateComponent` `@Input`; orphan-payment-detail takeUntil-as-stated
— loads already had it (only writes lacked it; now fixed). **3 agent deferrals:** campaign-report
familyType breakdown (no family type on the wire — cannot recompute); `getStatusBadgeClass` BS4
classes (shared service outside epic-18); build/test (run centrally below instead).

## Refutations (first-class outcomes, verify-before-change)

1. **D2 reinstatement premise** — see table.
2. **P17 ×2** — `[ProducesResponseType]` declarations match; `orphansQuery` is live.
3. **P21 "14 screens"** — 7 engine closures exist, 6 already snapshot their filter; only
   family-follow-up's `exportToExcel` re-read live state (fixed). The engine-level defects were
   real and fixed centrally.
4. **P26 "~20 screens"** — executed: 27 capped sites / 24 files warned, 5 plain-array loaders not
   applicable.
5. **Phone-duplicate family-level claim** — `FamilyService.cs:2894` already filtered family-level
   `IsDeleted`; the real gap was holder-level (fixed).
6. **18-10 "wrong i18n path"** — the story's File List already carried the correct path; only task
   prose was module-relative.
7. **P27 agent ×3** — see wave 4.

## Records

- `architecture.md` §10 D1 rulings table (R1–R9 + two not-ratified items).
- `deferred-work.md` — 6 entries: P13, P16, P17-remainder (D5), D4-Blacklist, D4-epics-4/8/9,
  D4-cheque.
- **14 story files**: 13 File-List corrections (both `Services/`→`Interfaces/IReportService.cs`
  mis-paths, both fully non-rooted File Lists re-rooted, 10 compound/glob entries expanded to real
  files, omitted files added — 18-37's `.scss`/`.spec.ts`, 18-1's task-claimed
  `reports.module.ts`) + Change Log rows in each; 18-8's platform-rule line corrected (the
  Framework's global soft-delete filter is commented out in this codebase — manual `!IsDeleted`
  checks ARE the convention, the 18-36 finding; the line previously asserted the global filter).
- i18n: `reports.lookup.truncated` + `reports.orphanFiles.hyperlinkFallback` added (both locales);
  `reports.orphanData.flagFinishedSponsorship` value fixed ("العمر"→"منتهية الكفالة"); the 6 dead
  `reports.print.*` keys (`printGuardians`/`printWidows`/`familyCode`/`familyCodePlaceholder`/
  `titleGuardians`/`titleWidows`) verified dead, symmetrical across locales, **left in place per
  18-37's recorded "no key deleted" ruling**; per-namespace `exportFailed` duplication accepted
  (every instance consumed — per-namespace i18n scoping is the pattern, not a defect). Node
  JSON.parse verified after every edit.

## Verification

- **Backend:** `dotnet build Backend/IIROSA.sln` green after all wave-1/2/D4 changes.
- **Frontend:** first post-P27 `npm run build` **caught 2 TS2339** — `GuardianIdentificationSheet`
  lacked the `truncated` field (wave-2's P14 backend flag vs wave-3's P22 guard; an integration
  miss between waves). Fixed in `report.model.ts`; rebuild → **exit 0** (`PIPESTATUS_EXIT=0`
  verified, not the pipe's tail code), pre-existing warnings only (NG8107 report-numbers from a
  parallel session's WIP, SCSS budget, CommonJS notices).
- **Tests:** excluded per the standing user decision (DF6).
- Live re-smoke of patched endpoints: not re-run (the 2026-08-25 consolidated smoke covered the
  pre-patch behaviour; the patches are code-verified + build-verified). DF7's populated-DB re-smoke
  still stands.
