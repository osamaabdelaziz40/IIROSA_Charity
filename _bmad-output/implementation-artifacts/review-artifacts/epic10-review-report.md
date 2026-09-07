# Epic 10 Code Review — Stories 10-1 … 10-9 (Orphan Payments & Disbursement)

| Field | Value |
| --- | --- |
| Date | 2026-08-26 |
| Mode | Story-range review, uncommitted working tree, diff scoped to the nine stories' File Lists |
| Diff artifact | `review-artifacts/epic10-review-input.diff` (5,645 lines; 34 tracked files + 9 new files) |
| Layers | Blind Hunter (28 raw) · Edge Case Hunter (18 raw) · Acceptance Auditor (10 raw) — all completed, none failed |
| Outcome | 1 decision-needed · 29 patch · 7 defer · 12 raw dismissed (11 groups) |

Layer findings: `epic10-findings-B-edge-case-hunter.md`, `epic10-findings-C-acceptance-auditor.md`
(Blind Hunter's copy landed in its isolated worktree; its findings are reproduced in full below and in this report.)

## Contradiction resolutions (verified in-repo by the triager)

| # | Conflict | Resolution |
| --- | --- | --- |
| R1 | Auditor "i18n fully clean" (ECH) vs auditor "`age`/`export` missing" | **Auditor right** — `orphanPayments.age` and `orphanPayments.export` exist in neither `ar.json` nor `en.json`; referenced at `add-orphans-to-group.component.html:204` and `orphan-payment-detail.component.html:49,443`. |
| R2 | Auditor "10-1 defect 2 (role widening) verified fixed" vs ECH "list still `SuperAdmin,Admin,Charity`" | **ECH right** — `OrphanPaymentsController.cs:38` is `[Authorize(Roles = "SuperAdmin,Admin,Charity")]`. The widening 10-1 Task 1 prescribed never landed (or was reverted). Auditor's clean call overturned. |
| R3 | Blind "`errors` vs `err?.details` wire break" | **False alarm** — `orphan-payment.service.ts:320-330` `handleError` maps `error.error.errors` → `details`, `error.error.message` → `message` before rethrowing. |
| R4 | Blind "unknown stopper fails OPEN" vs story/auditor "fail closed" | **Blind right** — `OrphanPaymentService.cs:354` `return hqUsers.Any(u => u.Id == userId)` is `false` for an unknown/deleted user; the doc comment at :349 claims fail-closed. Code contradicts comment. |
| R5 | Blind "PeriodicOrphanReport columns lack a migration — Critical" | **False alarm** — columns are covered by the parallel epic-6 migration `20260824115014_Epic06_HousingReportBeneficiary`; ECH additionally verified the dual `Migrations/` + `Data/Migrations/` folders are ONE lineage (the stray folder holds the real base `InitialCreate`; `Data/Migrations/InitialApplicationCreate` is an empty Up/Down) and the sole snapshot is current with every epic-10 column. |
| R6 | Blind "`dateRangeInvalid` alert can never render" | **False alarm** — group validator exists (`orphan-payment-form.component.ts:126` returns `{ dateRangeInvalid: true }`). |

## Decision-needed

| ID | Title | Detail |
| --- | --- | --- |
| D1 | §15.S.1 list-screen الجمعية (charity) filter has no owner | `OrphanPaymentFilterDto.CharityId` is a dead no-op (`OrphanPaymentRepository.cs:130-133` empty if-body, sanctioned by 10-1's "10-7 owns the join" ruling — but 10-7 delivered the details read only). §15.S.1 specifies a charity dropdown with كافة الجهات on the list. No story in 10-1..10-9 claims it. Board ruling needed: owning story (10-18..10-22 reporting surfaces? a new slice?) or explicit de-scope; until then wire the join or remove the dead param. |

## Patch findings

### Critical

| ID | Source | Title | Location |
| --- | --- | --- | --- |
| P1 | blind+edge+auditor | Charity token without a charity claim bypasses tenancy on `POST /api/OrphanPayments/orphan-items` — stop/resume/mark-printed on ANY row of ANY charity. `TokenService.AddTenancyClaims` omits the claim when `CharityId` is null (seeded `Charity@IIROSA.com` is exactly this); the controller has no D4 guard (contrast `:47-50`, `:138-141`) and the service check `userRole == "Charity" && userCharityId.HasValue && …` short-circuits false. | `OrphanPaymentsController.cs:443-487`, `OrphanPaymentService.cs:288-293` |
| P2 | edge | Re-adding a removed orphan always 500s — remove is now a soft delete (10-5) but `IX_OrphanPaymentItem(OrphanPaymentId, OrphanId)` is unique with no `IsDeleted` filter; the add-path duplicate check sees only live rows → SqlException 2601 → raw 500. Orphan can never be re-enrolled in that batch. | `OrphanPaymentItemConfiguration.cs:73`, `OrphanPaymentService.cs:166-260` |

### High

| ID | Source | Title | Location |
| --- | --- | --- | --- |
| P3 | edge (R2) | List role set `SuperAdmin,Admin,Charity` rejects the Accountant/FinancialOfficer the FE admits (PERMISSION_ROLES `OrphanPayments.View` = 5 roles) — 10-1 Task 1's widening not in effect; and the list error handler swallows the 403 into a permanently blank grid. | `OrphanPaymentsController.cs:38`, `orphan-payment-list.component.ts:240-243` |
| P4 | edge+blind | Soft-deleted batches remain fully writable — `RepositoryBase.FindAsync` bypasses `IsDeleted` (no global filter); update / mark-uploaded / add-orphans / assign-batch-number / set-exchange-rate / lock all mutate deleted batches. 10-5 fixed three READ paths; the WRITE paths still resurrect. Update even persists the mutation, then throws "Failed to retrieve updated payment group". | `OrphanPaymentService.cs:361-429,166-170` et al. |
| P5 | blind+edge (R4) | HQ-stop lock fails OPEN — `IsHeadOfficeUserAsync` returns false for an unknown/de-role'd stopper (comment claims fail-closed), and a stop made without a resolvable NameIdentifier stores `StoppedByUserId = null`, so the lock condition (`StoppedByUserId.HasValue && …`) never engages. | `OrphanPaymentService.cs:308-313,351-355`, `TokenService.cs:46` |
| P6 | edge | `by-batch-no` / `batch-numbers` reads lack the D4 fail-closed guard — claim-less Charity token enumerates every charity's batch numbers and resolves any batch header. | `OrphanPaymentsController.cs:77-92,609-632`, `OrphanPaymentService.cs:650-675,808-823` |
| P7 | edge | `UpdatePaymentGroupAsync` writes `BatchNo` with no uniqueness check — the edit dialog's picker lists every group's number; picking another group's number → 2601 → raw 500 (create and assign paths DO check). | `OrphanPaymentService.cs:394` |

### Medium

| ID | Source | Title | Location |
| --- | --- | --- | --- |
| P8 | auditor (R1) | `orphanPayments.age` missing from BOTH ar.json and en.json (renders raw key on the add-orphans grid header). | `add-orphans-to-group.component.html:204` |
| P9 | auditor (R1) | `orphanPayments.export` missing from BOTH files (export button + modal submit render raw key). | `orphan-payment-detail.component.html:49,443` |
| P10 | auditor | §15.S.1 الرقم (serial) column absent from the list grid — AC-3's first enumerated column. | `orphan-payment-list.component.html:79-89` |
| P11 | auditor | PaymentDate never surfaces on the list — التاريخ binds `groupDate` only; 10-2's "PaymentDate with GroupDate fallback" claim unimplemented; the mandatory تاريخ بدء التوزيع is invisible anywhere. | `orphan-payment-list.component.html:87,108` |
| P12 | blind+edge | Row removal (`DELETE orphan-items/{id}`) bypasses the disbursement guard the group delete enforces — settled rows (`IsGotIt`/`TransferNo`/`ChiqueNum`) deletable one row at a time. | `OrphanPaymentService.cs:875-883` vs `:1322-1329` |
| P13 | blind+edge | Row actions ignore the parent batch's state — stop/resume/mark-printed succeed on rows of a soft-deleted or uploaded batch (FE hides buttons via `canModify()`; server has no guard — hiding a button is not a control). | `OrphanPaymentService.cs:295-331` |
| P14 | edge | `GetByBatchNoAsync` participation check is header-level only — a participating Charity caller receives the FULL DTO with every charity's rows and batch-wide counts via unscoped `GetByIdAsync`. | `OrphanPaymentService.cs:1289-1305` |
| P15 | edge | Trim inconsistency — picker groups by `Trim()` while uniqueness and `by-batch-no` resolve exact-match: `" BP-1"` renders as `BP-1`, clicks to 404; visually identical duplicates coexist. | `OrphanPaymentService.cs:667-670` vs `OrphanPaymentRepository.cs:27-43` |
| P16 | edge+auditor | Per-charity count dict drops orphans tenanted through the Family (`FK_CharityId ?? Family.FK_CharityId` everywhere else) — totals disagree with the row filter on the same batch. | `OrphanPaymentRepository.cs:265-274` |
| P17 | edge | Available-orphans search: charity predicate lacks the family fallback (family-derived orphans unenrollable though counted elsewhere), and the region/center filter blocks the FE sidebar offers are empty no-ops. | `OrphanRepository.cs` `SearchFilteredAsync` |
| P18 | blind+edge | Available-orphans read hard-capped `pageSize: 200` with `totalAvailable` fetched and discarded — orphans 201+ silently unenrollable, no pager, no indicator. | `orphan-payment.service.ts` `getAvailableOrphans`, `add-orphans-to-group.component.ts` |

### Low

| ID | Source | Title | Location |
| --- | --- | --- | --- |
| P19 | blind | Single-orphan (`orphanId`) detail mode returns before the charity-name join — charity column always blank in UC-ORP-09 mode. | `OrphanPaymentService.cs:1152-1161` vs `:1176-1196` |
| P20 | blind | Entity XML docs contradict the frozen §15.1 action numbering (`IsStopped` "(action 1)"… vs DTO 0=stop/resume, 1=printed, 2=receipt, 3=cheque, 4=clear) — trap for 10-11..10-13. | `OrphanPaymentItem.cs` doc comments vs `UpdateOrphanPaymentItemDto.cs` |
| P21 | blind+edge | Missing `takeUntil(destroy$)` on subscriptions in add-orphans, form (create+update), and list `deleteGroup`; `onToggleStop` has no in-flight guard (double-click → out-of-order 200s → UI flag can disagree with DB). | `add-orphans-to-group.component.ts`, `orphan-payment-form.component.ts:216-260`, `orphan-payment-list.component.ts`, `orphan-payment-detail.component.ts` |
| P22 | blind | `alert()` for the enrolment result — module standard is NotificationService. | `add-orphans-to-group.component.ts:2519-2525` |
| P23 | auditor | Validator per-action clause covers action 3 only, story letter says 3/4 (mitigated: service refuses 2..4 loudly until 10-12). | `UpdateOrphanPaymentItemValidator.cs:28-36` |
| P24 | auditor | Refusal toasts localised for the HQ-stop message only — scope 403 / validation 400 render raw English. | `orphan-payment-detail.component.ts:441-444` |
| P25 | blind | `LockExchangeRateDto.LockRate` non-nullable — posting `{}` silently UNlocks with 200. | `LockExchangeRateDto.cs`, controller `:280-287` |
| P26 | edge | BatchNo charset unvalidated — `/`, `%` values make `by-batch-no/{batchNo}` unreachable (route-segment / decoding). | Create/Update validators, route `:609` |

## Deferred

| ID | Title | Reason / owner |
| --- | --- | --- |
| W1 | `hasPermission` fails OPEN for unmapped permission keys (platform-wide; ~60 new keys now ride on it) | Pre-existing platform behavior; harden in core, not this epic |
| W2 | Enrolment write (add + Amount snapshot) has zero execution evidence — dev DB has no Orphan rows | Sanctioned deferral; MUST be covered in the epic-10 final regression |
| W3 | Payment-summary band 403s for Accountant/FinancialOfficer/Charity the detail screen admits (`DashboardController` = SuperAdmin,Admin only) | Band belongs to 10-21/epic-18 scope creep; role alignment rides that story |
| W4 | N+1 orphan-count loop (`GetOrphanCountAsync` per group) on BOTH list `:465` and history `:532` branches | Pre-existing loops untouched by this epic |
| W5 | Batch-number generator: manual prefix-sharing values (`BP-202608-9`) break `int.TryParse` → duplicate → every create 500s for the month; no concurrency guard | Pre-existing generator; harden with P7's uniqueness work if convenient |
| W6 | Orphan-history sort ignores `filter.SortBy`, always `GroupDate` | 8-8 shipped surface, pre-existing |
| W7 | Scope creep riding epic-10 files: `PeriodicOrphanReport` housing fields (epic-6), detail print buttons + summary band (10-21/10-24), `OrphanRepository` `SocialStatus` include (§12), auth login token-guard hunk (epic-1/parallel) | Route to owning epics' reviews; recorded so it isn't lost |

## Dismissed (with grounds)

| Raw finding(s) | Grounds |
| --- | --- |
| Blind #3 (Critical: PeriodicOrphanReport unmigrated columns) | Refuted — R5: epic-6 migration covers; one lineage; snapshot current |
| Blind #4 (`errors` vs `details` wire break) | Refuted — R3: service `handleError` maps the shape |
| Blind #7 + ECH #6 (Down restores PostgreSQL filter) | Recorded story ruling (10-2): "Down is never taken on SQL Server" — accepted risk, noted |
| Blind #9, #10 (OfficeDevelopmentProjects/Missions menu gates; correspondence links) | Foreign sessions' changes in shared `main-layout.component.html` — out of epic-10 scope (see W7 pattern) |
| Blind #14a (history item soft-delete unfiltered) | Refuted — OrphanPaymentItemRepository filters `IsDeleted` everywhere (ECH trace) |
| Blind #16 (validators not DI-registered) | Refuted — convention registration live; live-smoke 400s prove the validators fire |
| Blind #17 (failed-login flows into success branch) | Foreign hunk (epic-1/parallel session, auditor C10-X-3d); out of scope here |
| Blind #18 (error-message extraction inconsistent) | Refuted as defect — consistent via `handleError`; dead first fallback arm is cosmetic |
| Blind #21 second half (missing body NRE) | `[ApiController]` model binding rejects empty bodies with 400; `{}` half kept as P25 |
| Blind #23 (charity dropdown 500 cap) | Platform families-idiom established pattern (10-1 rewire) |
| Blind #26 (`dateRangeInvalid` dead) | Refuted — R6: group validator exists |
| Blind #28 (unverifiable cross-references bundle) | Superseded by repo-verified layers (Dashboard endpoint, models, components, enums all confirmed) |

## Per-story verdicts

| Story | Verdict | Patch IDs |
| --- | --- | --- |
| 10-1 List payment batches | AC-3 partial; Task-1 role widening NOT in effect | P3, P10, P11 (list surface), P21 (list) |
| 10-2 Create a payment batch | ACs met in code; enrolment-write evidence owed (W2) | P2, P8, P17, P18, P21 (add-orphans), P22 |
| 10-3 View a payment batch | Clean per its own ACs | P9 |
| 10-4 Update a payment batch | Clean per its own ACs | P7, P21 (form), P25 |
| 10-5 Delete a payment batch | Guards solid; write-path resurrection gap | P4, P12 |
| 10-6 List a charity's batch numbers | ACs met; claim-less token gap + scoping | P6, P14, P15, P26 |
| 10-7 View payment details | ACs met; derivation gaps | P16, P19 |
| 10-8 List orphans in a batch | **Clean — no findings** | — |
| 10-9 Stop/resume a payment | AC-2 tenancy hole (P1); lock fail-open (P5) | P1, P5, P13, P20, P21 (toggle), P23, P24 |
