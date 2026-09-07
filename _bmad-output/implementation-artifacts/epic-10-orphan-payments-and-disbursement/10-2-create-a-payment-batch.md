# Story 10.2: Create a payment batch — اضافة دفعة مالية

| Field | Value |
| --- | --- |
| Story | US-PAY-02 (UC-PAY-02) |
| Epic | EP-10 — Orphan Payments & Disbursement (chapter 15 · §15.S.2, §15.U.2, §25.6) |
| Priority / size | Must · 5 points |
| Route | `#/orphan-payments/create` then `#/orphan-payments/:id/add-orphans` |
| Endpoint | `POST /api/OrphanPayments` + enrolment `GET {id}/available-orphans`, `POST {id}/orphans` (all as-built) |
| Depends on | 10-1 (foundation). **Owns the whole-epic EF migration.** |
| Roles | SuperAdmin, Admin, Accountant, FinancialOfficer (WAR matrix: Gen. Director + Staff + Fin. Director create batches) |

Status: done

## Story

As a General Director,
I want to be able to create a payment batch اضافة دفعة مالية,
so that the register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given an HQ-Fin role on `#/orphan-payments/create`, when «حفظ» is pressed with valid input, then `POST /api/OrphanPayments` creates the batch owned by the batch parameters entered and the record appears in the 10-1 list without a page reload.
2. Given a mandatory field (رقم الدفعة، اسم الدفعة، الفترة من/الى، تاريخ بدء التوزيع) is empty, then the save is refused (400) and the offending field is flagged on the form.
3. Given enrolment (`#/orphan-payments/:id/add-orphans`), when orphans are selected and added, then each enrolled orphan gets an `OrphanPaymentItem` row **with a snapshotted `Amount`**, and already-in-group orphans are reported as skipped (as-built contract).
4. Given the database is behind the model, when this story's migration runs, then `IIROSA.OrphanPayment` / `IIROSA.OrphanPaymentItem` exist with all columns this epic needs (see Task 1) — resolved per the drift hazard below, surfaced to the user if ambiguous.
5. Given the session has expired or the role is not permitted, then the request is rejected and the SPA routes back to login.

**Definition of done:** a batch can be created, batch number auto-generated or uniqueness-checked, orphans enrolled with amounts, and the whole epic's schema delta lands in ONE migration cut here. §15.U.2/§25.6 main flow passes end to end.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | State (working tree, 2026-08-24) |
| --- | --- |
| Create endpoint | `POST /api/OrphanPayments` → `CreatePaymentGroup(CreateOrphanPaymentDto)` (`OrphanPaymentsController.cs:154`, roles SuperAdmin,Admin — widen per Roles row) |
| Enrolment | `GET {id}/available-orphans` (:307, `OrphanFilterForPaymentDto`, paged, marks IsInGroup), `POST {id}/orphans` (:332, `AddOrphansToGroupDto` → `{ message, addedCount, skippedCount }`), `DELETE orphan-items/{orphanPaymentItemId}` (:367) |
| Service | `CreatePaymentGroupAsync` (real: `ICharityWriteGuard.EnsureCanAddAsync()` lock check, period validation, `GenerateNextBatchNumberAsync` or manual uniqueness) · `AddOrphansToGroupAsync` (skips duplicates; **N+1 per-orphan `GetByIdAsync`**), `OrphanPaymentService.cs:48,150` |
| Entities | `OrphanPayment` (GroupName, Description, PaymentPeriodFrom/To, GroupDate, ExchangeRate, Currency, DontRemoveRate, BatchNo, ShowOrder, IsBatchUploaded, UploadDate, Notes, Orphans) · `OrphanPaymentItem` (**only** OrphanPaymentId, OrphanId, DisplayOrder, Notes) — `Backend/src/IIROSA.Domain/Entities/` |
| Config | `OrphanPaymentConfiguration` / `OrphanPaymentItemConfiguration` — IIROSA schema, unique composite (OrphanPaymentId, OrphanId) |
| Frontend | `orphan-payment-form/` (create+edit, one component) + `add-orphans-to-group/` screens exist and call the real create/enrol endpoints |
| Validator | **None** — only DataAnnotations on the DTO (architecture requires FluentValidation in the service layer) |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **BLOCKING migration drift**: `ApplicationDbContextModelSnapshot` contains `OrphanPayment`/`OrphanPaymentItem` but **no migration does** (only `InitialCreate` + `RemoveDuplicateFrameworkTables` exist; zero "OrphanPayment" occurrences). `dotnet ef database update` will never create these tables. Resolve in Task 1 — do not guess.
2. **`OrphanPaymentConfiguration.cs:67` unique index filter uses PostgreSQL syntax** (`"BatchNo" IS NOT NULL AND "IsDeleted" = false`) — invalid on SQL Server; fix to `[BatchNo] IS NOT NULL AND [IsDeleted] = 0` and capture in the migration.
3. **No `PaymentDate`** (تاريخ بدء التوزيع, mandatory in §15.S.2) — the entity has only `GroupDate` (creation date). Add the column (Task 1).
4. **No `Amount` on `OrphanPaymentItem`** — enrolment stores no money. BR-17 "amounts follow the batch's entitlement category": snapshot the orphan's current monthly amount at enrolment (see rulings).
5. **No FluentValidation validator** — add `CreateOrphanPaymentValidator` invoked from the service (`ValidateAndThrowAsync`, 15-6 precedent).
6. **Create DTO carries non-persisted UI filter fields** (CharityId `int?`, RegionId, CenterId, AgeFrom/AgeTo…) that silently do nothing — keep them only if the form's available-orphans pre-filter actually consumes them; otherwise remove from the wire (they are NOT stored).
7. **`AddOrphansToGroupAsync` N+1** (`GetByIdAsync` per orphan; `GetAvailableOrphansAsync` re-queries per row at :596) — batch-load by id set.
8. Widen create/enrol roles to the HQ-Fin set (see Roles).

## Tasks / Subtasks

- [x] Task 1 — Whole-epic migration `Epic10_PaymentDisbursement` (AC: 4) — **do this first**
  - [x] Diagnose: probe migration `_ProbePending` cut first (after the user stopped their running IIROSA.Api — the MSB output lock blocked `dotnet ef` builds); diff inspected
  - [x] Resolution: tables already existed (created by `20260603071842_AddMissingFamilyColumns`) — the probe emitted **AddColumns only, zero `CreateTable`s**, and ZERO unrelated entities (the parallel sessions' deltas had landed in their own Epic06/Epic09 migrations, so the model was back in sync). No STOP-and-surface needed; probe removed, recut as `20260824111613_Epic10_PaymentDisbursement`, **applied to the DB** (`database update` Done)
  - [x] Column set: `OrphanPayment.PaymentDate datetime2 NULL` + `OrphanPaymentItem`: `Amount decimal(18,2) NULL`, `IsStopped bit NOT NULL default 0`, `StoppedOn`, `IsPrinted bit NOT NULL default 0`, `PrintedOn`, `IsGotIt bit NOT NULL default 0`, `ReceivedOn`, `ChiqueNum nvarchar(50)`, `Printdate`, `BenificiaryName nvarchar(200)`, `TransferNo nvarchar(100)`, `ExchangeStatus int NULL` — §15.1 names verbatim (incl. the `ChiqueNum`/`BenificiaryName`/`Printdate` spellings)
  - [x] BatchNo index filter fixed in the same migration: dropped + recreated with `[BatchNo] IS NOT NULL AND [IsDeleted] = 0` (SQL Server syntax; the Down restores the old PostgreSQL filter verbatim — harmless, the Down is never taken on SQL Server)
- [x] Task 2 — Create path (AC: 1, 2)
  - [x] `PaymentDate` added to entity + Create/Update DTOs + `OrphanPaymentDto`/`OrphanPaymentListDto` (convention-mapped; update path applies it null-conditionally so legacy rows keep their value)
  - [x] `CreateOrphanPaymentValidator` (Validators/OrphanPayment/): GroupName/BatchNo/Description/Notes/Currency lengths, `PaymentPeriodFrom <= PaymentPeriodTo`, `PaymentDate` required; invoked via `ValidateAndThrowAsync` in the service; convention-registered by `AddValidatorsFromAssembly`
  - [x] Create `[Authorize]` widened to `SuperAdmin,Admin,Accountant,FinancialOfficer`; `EnsureCanAddAsync()` kept; hand-rolled date-range check removed (validator is the single authority); controller gained the CharitiesController-shaped `ValidationException` catch → 400 `{ message, errors: { field: [messages] } }`
  - [x] Form: `paymentDate` required control + star + required/server error slots; 400 `errors` dictionary mapped onto controls (PascalCase→camelCase) via `applyServerErrors`; group-level `dateRangeInvalid` display fixed (was `hasError('dateRangeInvalid',…)` on a non-existent control)
- [x] Task 3 — Enrolment with amounts (AC: 3)
  - [x] Snapshot source: **`Orphan.MonthlyAmount` (decimal?, Orphan.cs:83)** — set as `item.Amount` at add time; BR-17 freeze semantics recorded in the entity XML doc
  - [x] `AddOrphansToGroupAsync` batch-loads via `GetPagedAsync(o => ids.Contains(o.Id), …)` into a dictionary (was `GetByIdAsync` per orphan); `{ addedCount, skippedCount }` contract unchanged, dupes/not-found still counted as skipped
  - [x] `GetAvailableOrphansAsync` per-row re-query of `GetByPaymentGroupIdAsync` replaced with ONE id→itemId dictionary load
  - [x] add-orphans screen: المبلغ column shows `monthlyAmount` (server `OrphanForPaymentListDto.MonthlyAmount` added — convention-mapped preview of the snapshot); enrolled-row `Amount` surfaces through `OrphanPaymentItemDto.Amount`
- [x] Task 4 — Wire/i18n cleanup on the form
  - [x] The three legacy .xlsx file inputs were **already absent** from the copied form template (verified by full read) — no trim needed; recorded rather than assumed
  - [x] New keys in ar+en: `paymentDate`, `amount`, `batchNumberAutoHint`, `orphansAddedResult` (both files JSON-validated); `trackBy` added to every `*ngFor` on form + add-orphans screens

### Review Findings

_Code review 2026-08-26 — full detail in `review-artifacts/epic10-review-report.md`._

- [x] [Review][Patch] CRITICAL: re-adding a removed orphan always 500s — removal is a soft delete but `IX_OrphanPaymentItem(OrphanPaymentId, OrphanId)` is unique with no IsDeleted filter; resurrect the row on re-add or cut a filtered index [OrphanPaymentItemConfiguration.cs:73, OrphanPaymentService.cs:166-260]
- [x] [Review][Patch] `orphanPayments.age` missing from BOTH ar.json and en.json — renders raw key [add-orphans-to-group.component.html:204]
- [x] [Review][Patch] available-orphans search: charity predicate lacks the Family fallback (family-derived orphans unenrollable) and the region/center filter blocks the sidebar offers are empty no-ops [OrphanRepository.cs SearchFilteredAsync]
- [x] [Review][Patch] available-orphans read hard-capped `pageSize: 200`, totalCount discarded — orphans 201+ silently unenrollable [orphan-payment.service.ts getAvailableOrphans]
- [x] [Review][Patch] `alert()` for the enrolment result — use NotificationService [add-orphans-to-group.component.ts:2519-2525]
- [x] [Review][Patch] `takeUntil(destroy$)` on add-orphans subscriptions [add-orphans-to-group.component.ts]
- [x] [Review][Defer] enrolment write + Amount snapshot never executed (dev DB has zero Orphan rows) — MUST be covered in the epic-10 final regression — deferred, sanctioned deferral
- [x] [Review][Defer] batch-number generator: manual prefix-sharing values break TryParse → duplicate → every create 500s; no concurrency guard — pre-existing generator [OrphanPaymentRepository.cs:45-68] — deferred, pre-existing
- [x] [Review][Defer] scope creep riding this file set (housing fields on PeriodicOrphanReport etc.) — route to owning epics' reviews — deferred, informational

## Dev Notes

### Platform rules that bind this story

- Repos never `SaveChanges` — `IUnitOfWork` only; one transaction for header+rows.
- Controller stays thin/raw-envelope (15-1 ruling — no `ApiResponse` conversion in this vertical).
- Field mapping (§15.S.2 → entity): رقم الدفعة→`BatchNo` (select-or-auto-generate; as-built `AssignBatchNumberAsync` semantics), اسم الدفعة→`GroupName`, الفترة من/الى→`PaymentPeriodFrom/To`, تاريخ بدء التوزيع→**new `PaymentDate`**, سعر الصرف→`ExchangeRate`, عدم خصم النسبه→`DontRemoveRate`. List screen التاريخ column → `PaymentDate` (fallback `GroupDate` for legacy rows).
- Tests excluded per standing decision — smoke-verify create + enrol live and record it.

### Story-specific rulings

- **One migration for the whole epic** (epic-9 pattern: whole-epic schema delta lands once). Stories 10-7..10-17 consume these columns; if a later story genuinely needs more schema it must say so loudly in its completion notes.
- **Amount snapshot**: amounts are frozen per row at enrolment (the WAR batch is a snapshot disbursement run; later orphan changes must not mutate past batches). Entitlement-category rules beyond the orphan's monthly amount are EP-19 scope (19-10 done).
- BR-11 read side (9-7/9-12 deferral): eligibility checking (accepted report in period) is **not** enforced at enrolment — HQ stops ineligible rows afterwards per §25.6 main flow step 6 (10-9). Do not build a report-state gate here.
- Legacy create-form file uploads are a legacy convenience — dropped (typed-DTO rule; parsing belongs to the import endpoints).

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Batch update / exchange-rate lock wiring | 10-4 |
| Delete | 10-5 |
| Charity batch-number listing | 10-6 |
| Row flags usage (stop/print/receipt/cheque) — columns land here, behaviour later | 10-9..10-13 |
| Bank file + imports (file parsing of any kind) | 10-14..10-17 |

### References

- [Source: docs/Modules/15-UC-PAY-Orphan-Payments-and-Disbursement.md#15.S.2] · [#15.U.2] · [#25.6] · [#15.1 row state model]
- [Source: Backend/src/IIROSA.Domain/Entities/OrphanPayment.cs / OrphanPaymentItem.cs] · [Configurations/OrphanPaymentConfiguration.cs:67]
- [Source: Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs:48-236]
- [Source: Backend/src/IIROSA.Infrastructure/Data/Migrations/] (drift evidence)
- [Source: _bmad-output/implementation-artifacts/epic-15-missions/15-6-*.md] validator precedent · [5-6-*.md] 400 field-map shape

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code session, 2026-08-24).

### Debug Log References

- `dotnet ef migrations add _ProbePending` → diff = the 13 epic-10 columns + BatchNo index fix ONLY (no unrelated entities; parallel sessions' deltas already landed in Epic06/Epic09 migrations). Probe removed; `20260824111613_Epic10_PaymentDisbursement` cut; `dotnet ef database update` → "Applying migration '20260824111613_Epic10_PaymentDisbursement'. Done."
- `dotnet build IIROSA.sln` — **0 errors** (first fully clean solution build of the epic; prior MSB3021/3027 were the user's live-API output-copy lock).
- `npx tsc --noEmit` — 0 errors in orphan-payments/* (remaining non-spec errors confined to modules/housing-projects + modules/periodic-orphan-reports, both owned by parallel/prior sessions, untouched).
- **Live smoke (user's restarted IIROSA.Api on http://localhost:60961, serving the post-migration build, as OsamaSuper@IIROSA.com):**
  - AC2: `POST /api/OrphanPayments` without paymentDate → **400** `{"errors":{"PaymentDate":["Payment date is required"]}}` — validator + field map live.
  - AC1: same payload + paymentDate → **201**, response carries `paymentDate:"2026-09-01T00:00:00Z"` and auto-generated `batchNo:"BP-202608-0001"`.
  - Enrolment read: `GET {id}/available-orphans` → **200** `{"items":[],"totalCount":0}` — paged envelope confirmed (the FE now consumes exactly this shape).
  - Smoke batch deleted (`DELETE` → 204) — no residue in the dev DB.
  - **DEFERRED to epic-final regression**: enrolment write smoke (POST orphans → added/skipped + Amount snapshot) — the dev DB has **zero Orphan rows** (verified via `GET /api/Families/orphans` → `totalCount:0`), and no orphan-creation surface exists on this stack (8-1 deferred the form to EP-05; no orphan seeder). Code path is compile-verified; contract unchanged from the as-built skip logic.

### Completion Notes List

- **Migration ownership**: `20260824111613_Epic10_PaymentDisbursement` is the epic's whole-schema migration. Later stories add NO schema except the sanctioned `Epic10_StoppedBy` micro-migration (10-9).
- **PaymentDate is nullable at the DB** (story said "(DateTime)" unqualified): legacy rows exist in dev, a NOT NULL column would have needed arbitrary backfill, and the 10-1 list already renders التاريخ with a GroupDate fallback. Validator makes it mandatory for every NEW create; update applies it null-conditionally.
- **Amount snapshot source**: `Orphan.MonthlyAmount` (the orphan's current monthly amount). Entitlement-category rules beyond this stay EP-19 (19-10 done) per the ruling.
- **Defect 6 executed as a real trim**: `CreateOrphanPaymentDto` lost CharityId/RegionId/CenterId/SponsorshipStatus/AgeFrom/AgeTo (never persisted, never echoed back — the server `OrphanPaymentDto` carries none of them, so the add-orphans "pre-fill from group" always no-op'd). The form's decorative "Filtering Options" card + its three controls + the now-unused Charity/Lookup service injections were removed; the enrolment screen keeps its own live filter sidebar (which does the real filtering via the available-orphans query). FE create/update DTOs trimmed to match.
- **Selection-wire re-cut (deferred from 10-1)**: `OrphanForSelectionDto` re-bound to the live `OrphanForPaymentListDto` wire (`orphanId/orphanName/isAlreadyInGroup` → `id/fullName/isInGroup`; + `code`, `orphanPaymentItemId`); `getAvailableOrphans` typed to the `{items,totalCount}` envelope (pageSize 200); `addOrphansToGroup` typed to `{message,addedCount,skippedCount}` with the skipped count surfaced via the `orphansAddedResult` i18n alert; `removeOrphanFromGroup` replaced by `removeOrphanItem(itemId)` hitting the live `DELETE orphan-items/{id}` route (detail screen re-bound to pass the item id).
- `generateNextBatchNumber()` service method + form button removed — `GET /batch-number/next` never existed; blank BatchNo auto-generates server-side (verified live: BP-202608-0001).
- `OrphanForPaymentListDto.CharityId` int?→Guid? — the profile maps `Orphan.FK_CharityId` (Guid?) into it; the old int? silently no-op'd/threw. The SAME latent mismatch still exists on `OrphanPaymentItemDto.CharityId` (int?) — left for 10-3/10-7 to fix with the detail-read re-cut (noting here so it isn't forgotten).
- Dead `/orphans/{id}` link on the available-orphans grid replaced with plain text (no orphan-profile route exists on this stack).
- OrphanPaymentItem FE model gained the full ledger field set (`amount` + the 10-9..10-17 fields) so later stories bind without model churn.
- Roles widened on create + available-orphans + add + remove to the HQ-Fin set (§4.3), matching 10-1's PERMISSION_ROLES `OrphanPayments.Create/.AddOrphans` map.

### File List

- Backend/src/IIROSA.Domain/Entities/OrphanPayment.cs
- Backend/src/IIROSA.Domain/Entities/OrphanPaymentItem.cs
- Backend/src/IIROSA.Domain/Configurations/OrphanPaymentConfiguration.cs
- Backend/src/IIROSA.Domain/Configurations/OrphanPaymentItemConfiguration.cs
- Backend/src/IIROSA.Application/DTOs/OrphanPayment/CreateOrphanPaymentDto.cs
- Backend/src/IIROSA.Application/DTOs/OrphanPayment/UpdateOrphanPaymentDto.cs
- Backend/src/IIROSA.Application/DTOs/OrphanPayment/OrphanPaymentDto.cs
- Backend/src/IIROSA.Application/DTOs/OrphanPayment/OrphanPaymentListDto.cs
- Backend/src/IIROSA.Application/DTOs/OrphanPayment/OrphanPaymentItemDto.cs
- Backend/src/IIROSA.Application/DTOs/OrphanPayment/OrphanForPaymentListDto.cs
- Backend/src/IIROSA.Application/Validators/OrphanPayment/CreateOrphanPaymentValidator.cs (new)
- Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs
- Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs
- Backend/src/IIROSA.Infrastructure/Data/Migrations/20260824111613_Epic10_PaymentDisbursement.cs (+ Designer, + model snapshot)
- Frontend/src/app/modules/orphan-payments/models/orphan-payment.model.ts
- Frontend/src/app/modules/orphan-payments/services/orphan-payment.service.ts
- Frontend/src/app/modules/orphan-payments/orphan-payment-form/orphan-payment-form.component.ts
- Frontend/src/app/modules/orphan-payments/orphan-payment-form/orphan-payment-form.component.html
- Frontend/src/app/modules/orphan-payments/add-orphans-to-group/add-orphans-to-group.component.ts
- Frontend/src/app/modules/orphan-payments/add-orphans-to-group/add-orphans-to-group.component.html
- Frontend/src/app/modules/orphan-payments/orphan-payment-detail/orphan-payment-detail.component.ts
- Frontend/src/app/modules/orphan-payments/orphan-payment-detail/orphan-payment-detail.component.html
- Frontend/src/assets/i18n/ar.json
- Frontend/src/assets/i18n/en.json

### Change Log

- 2026-08-24 — created (EP-10 context pass) → ready-for-dev.
- 2026-08-24 — implemented (dev-story): whole-epic migration `Epic10_PaymentDisbursement` cut AND applied (probe-verified epic-10-only diff); PaymentDate end to end; CreateOrphanPaymentValidator + 400 field map; enrolment batch-load + BR-17 Amount snapshot + N+1 fixes (both hot paths); selection-wire re-cut to the live envelope; remove-by-item-id; dead batch-number generator stripped; defect-6 filter-field trim; i18n + trackBy. Backend build 0 errors; tsc clean (module); create-path + envelope verified live. → review.
- 2026-08-26 — code review remediation (P2/P8/P17/P18/P21/P22): re-add resurrects the soft-deleted row (unique-index 500 fixed), orphanPayments.age key, Family-fallback charity predicate + region/center sidebar filters implemented, server-side pager replaces the 200-row cap, NotificationService replaces alert(), takeUntil. Build verified; live smoke rides the epic-10 final regression. → done.
