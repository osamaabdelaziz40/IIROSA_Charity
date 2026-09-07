# Epic-18 review — chunk 1 (backend core) — Acceptance Auditor findings

Diff audited: `chunk1-backend.diff` (8,329 lines, all-new files). Line refs are diff line numbers.

## A. CLAUDE.md / platform-contract violations

- **[high] `DashboardController` inherits `ControllerBase`, not `ApiController`** — violates CLAUDE.md "Controller base: Inherit `ApiController`" and §23.B/§7.B. Evidence: `DashboardController.cs:23` `public class DashboardController : ControllerBase`, inline comment at :19 citing the "15-1 ruling; ControllerBase per the 17-1 convention" — a story-level ruling contradicting the binding CLAUDE.md table. Internal contradiction: `ReportsController : ApiController` (diff:92).

- **[high] Raw envelope instead of `ApiResponse`/`ApiResponse<T>` on ~25 of 27 actions** — violates CLAUDE.md "API responses" and §23.B/§7.B. Evidence: `ReportsController.cs` returns `Ok(result)` plus anonymous `new { message, errors }` on ValidationException and `new { message }` on 500 throughout (orphans-missing-reports :460/:465, detail :497/:502, missing-files :521+, family-orphans :1134/:1144/:1149); `DashboardController.GetPaymentSummary` :23–80. Sanctioned by each story's "15-1 ruling (architecture.md §10 'code wins')" — a recorded ruling contradicting the binding table.

- **[medium] Response contract internally inconsistent within one controller** — `ExportOrphanReportForm` (:1160–1187) returns `ApiResponse { Success, Message }`; `ExportSheetPdf` (:1193–1242) builds `ApiResponse` with `ModelStateErrors` (:1222–1231); the other ~25 actions use raw/anonymous envelopes. Same file, two envelope shapes.

- **[high] Business logic in controllers (four instances)** — violates CLAUDE.md "No business logic in controllers": (1) Meza variant discrimination `filter.ReportNo is > 0 ? …Export… : …` (:306); (2) page clamping `Math.Max(1, request.Page)` (:486) patching an unwired validator; (3) manual null-check of `dto.ReportId` in `ExportOrphanReportForm` (:1169–1172); (4) `ExportSheetPdf` report-key dispatch switch (:1207–1216).

- **[high] Tenancy of the family-sheet path trusts the payload with no `IsHeadOffice` gate** — `ReportSheetService.ScopeFamilies(dto, userCharityId, userRole)` pins only when `userRole == "Charity"` (string compare); any other role adopts `dto.CharityId` from the payload or runs unscoped — no `IsHeadOffice` check anywhere in the class. A non-HQ, non-Charity role (Accountant/Employee, admitted on the sheets route :1194) posting without payload sees the whole cross-charity register. No recorded ruling sanctions this asymmetry. [NOTE from orchestrator: ReportSheetService is epic-5 UC-FAM-14 code riding the same tree — defect is real in shipped code; attribution differs.]

- **[high] `GetUserRole()` uses the known-broken `User.IsInRole` API** — violates platform defect record ("Identity IsInRoleAsync broken — surrogate Id PK on ApplicationUserRoles"). `ReportsController.GetUserRole()` :1260 `User.IsInRole("Charity")`, and its output is the sole tenancy pin for the three `ReportSheetService` sheet builders, so a false negative converts a charity-scoped caller into an unscoped one. Also claims parsing in controller vs 18-8's own "scope only from ICurrentUserService" rule.

- **[medium] FluentValidation bypassed on UC-RPT-15 detail endpoint** — `GetOrphansMissingReportsDetailAsync(Guid, int, int)` takes scalars, no DTO/validator; controller compensates with its own clamp (:486).

- **[medium] Charity names via plain `c.Name`, not `NameAr ?? NameEn`** — every charity-name helper projects `c.Name` (:7103, `ResolveCharityNamesAsync`); refuse-reason labels do use `NameAr ?? NameEn` (:6773). `Charity` entity lacks bilingual columns (entity-level debt surfaced into report output).

- **[low] AutoMapper effectively unused** — `ReportProfile` has one map; ~25 report projections hand-rolled `.Select(...)`. Recorded as accepted; standing deviation.

- **[low] Story-text falsehood: 18-8 Dev Notes assert a global soft-delete filter that does not exist** — `Framework.Core`'s `SetGlobalQueryFilters` is commented out; code complies via explicit `!IsDeleted`. Flagged so the board doesn't cite 18-8 as precedent.

## B. UC / spec-intent deviations

- **[medium] Five report UCs ship as always-empty stubs** (UC-RPT-03/04/07/08/11: excluded orphans, ended sponsorship, meza-cards ×2, registered family projects) — no exclusion column / no "ended" state / no Meza entity / no project-FamilyId link exist domain-wide. All carry recorded "empty-set + recorded-gap" rulings (2026-08-24). Scope debt to surface, not fake implementation.

- **[medium] UC-RPT-12 (guardian change): old/new guardian pairing undeliverable** — three prior-guardian columns constant null (no Provider audit trail); تاريخ التعديل synthesized from UpdatedOn>CreatedOn. Recorded ruling; endpoint + live rows exist, change-pairing semantics don't.

- **[medium] UC-RPT-20 (charity follow-up): printed/disbursed/confirmed columns omitted** — "omitted, not faked" (no persisted flags); `reportsEntered` counts reports in any state.

- **[low] UC-RPT-22: سبب طلب الاستعداد (Reason) always null** — no source column; arrears semantics per §14.U.14 correct; non-HQ AllOrphans correctly refused 403.

- **[low] UC-RPT-38 v1 rule diverges from legacy semantics** — reports only letters with ZERO OutgoingOrphanReport rows (legacy expected-list table absent); `AttachmentCount` hardcoded 0. A letter missing some but holding one attachment is silently clean in v1.

- **[medium] All print/PDF/Excel client-side; §23.U.x server streaming superseded** — recorded deviation (18-21/18-40/18-41, architecture.md §10); backend honors it consistently (no server document generation anywhere).

- **[low] UC-RPT-01: `IsFinishedSponsorship` accepted-not-applied; age filter (`DateDiffYear`) vs display age (birthday-aware) boundary mismatch.**

- **[low] Two different widow discriminators in one epic** — `ReportService.GetGuardianIdentificationSheetsAsync` (mother alive + father death date) vs `ReportSheetService.BuildWidowIdentificationSheetAsync` (`Mother.IsProvider == true`): same concept, different row sets per endpoint.

- **[low] UC-RPT-29 recorded limitations** — not-received is literal `!IsGotIt` (stopped counts as not-received); mixed-currency batch reports first item's currency.

## C. Cross-story contradictions, roles, scope

- **[high] Generic `POST /api/Reports/{reportKey}/export/pdf` exists although 18-40 records it superseded with "no new backend endpoint"** — route at :1193 (roles SuperAdmin,Admin,Charity), created by the parallel epic-5 session for UC-FAM-14 (comment :84–92). Returns JSON, not PDF bytes — misleading wire contract. 18-37's Dev Notes say "do not build or name any `…/export/pdf` route"; two identification-sheet implementations now coexist.

- **[medium] Role sets diverge from spec matrices and story headers** — UC-RPT-19 story header "Gen. Director → SuperAdmin,Admin" but action authorizes SuperAdmin,Admin,Accountant,Employee,Charity (widest in file); "All roles" UCs (01,03,06,09,12,18,24,25,40,41) map to SuperAdmin,Admin,Charity although Accountant/Employee exist as live roles; UC-RPT-15 detail widened mid-story to SuperAdmin,Admin,Charity (:474); UC-RPT-39 family-orphans SuperAdmin,Admin only (:1124) while host screen admits Charity.

- **[low] `ArabicAmountInWords.cs` ships with zero epic-18 provenance** — full tafqit implementation (UC-CHQ-07 territory), no 18-x story references it. [Records auditor (chunk 5) confirms: consumed exclusively by CheckService (cheque epic).] Attribute to a cheque story or split out.

- **[low] UC-DSH-01/02 (Dashboard summary/charts) unimplemented** — DashboardController hosts only payment-summary. Out of epic-18 scope; scope note for the parent. payment-summary semantics follow §15.U.18–20 verbatim.

## Verified conforming (for synthesis — not findings)

- Tenancy in `ReportService` sound: pin-never-widen ladder (`ApplyCharityScopeAsync` :7046–7086) applied consistently; payload charityId honored only for HQ; fail-closed default (:7079–7085); awaiting-approval/refused ladders clamp correctly; missed-payments refuses non-HQ AllOrphans; 18-15 detail intersects pinned scope. Only tenancy exceptions: the ReportSheetService findings above.
- Valid reuse: 18-33 reuses `GET /api/CheckManagement/report`; 18-27 reuses `GET /api/OrphanPayments/{id}/details`; 18-26 blank client-side document.
- BR-11 window semantics correctly shared by 18-15/18-19/18-36 via `BuildAcceptedReportIdsQuery` (:6525–6537); sheet row caps (5000, Take(cap+1) probe) and empty-selection BusinessException → 400 consistently applied in ReportSheetService.
