# Epic 10 Review — Edge Case Hunter Findings (Reviewer B)

Scope: stories 10-1..10-9 (Orphan Payments & Disbursement). All locations are real repo files/lines, verified against the working tree (not the diff alone). Raw JSON envelope for this vertical is an accepted ruling and is not flagged.

---

## 1. Charity token without a charity claim bypasses tenancy on the stop/resume WRITE endpoint

**Severity:** Critical

**Location:**
- `Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:443-487` (`UpdateOrphanPaymentItem` — no fail-closed guard)
- `Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs:288-293` (scope guard conditional on `userCharityId.HasValue`)
- `Backend/src/IIROSA.Application/Services/TokenService.cs:180-189` (`AddTenancyClaims` omits the claim when `charityId` is null)

**Failure scenario:**
1. A user holds role `Charity` but their user row has `CharityId = NULL` (the seeded dev account `Charity@IIROSA.com` is exactly this state) — or the FK is lost later.
2. Login: `AddTenancyClaims` only emits the `charityId` claim `if (charityId.HasValue)` → the JWT has role Charity and **no** charity claim.
3. The user calls `POST /api/OrphanPayments/orphan-items` with `{ itemId: <another charity's row id>, action: 0, flag: true }`.
4. Controller `GetUserCharityId()` → `Guid.TryParse` fails → `null`. The endpoint has no D4 guard (unlike `GetPaymentGroups` at lines 47-50 and `{id}/details` at 138-141, which fail closed).
5. Service guard `if (userRole == "Charity" && userCharityId.HasValue && orphanCharityId != userCharityId.Value)` — `userCharityId.HasValue` is false → the whole check short-circuits to false → **no scope check at all**.
6. The foreign charity's row is stopped/resumed/marked-printed and persisted.

**Evidence:**
```csharp
// OrphanPaymentService.cs:288-293
if (userRole == "Charity" && userCharityId.HasValue && orphanCharityId != userCharityId.Value)
    throw new UnauthorizedAccessException(...);
```
```csharp
// TokenService.cs:180-189 — claim only when charityId.HasValue
if (charityId.HasValue) { identity.AddClaim(new Claim(IiroSaClaimTypes.CharityId, ...)); }
```

**Verdict:** Confirmed

---

## 2. Re-adding a removed orphan always 500s — soft delete + unfiltered unique index on (OrphanPaymentId, OrphanId)

**Severity:** Critical

**Location:**
- `Backend/src/IIROSA.Domain/Configurations/OrphanPaymentItemConfiguration.cs:73` — `builder.HasIndex(x => new { x.OrphanPaymentId, x.OrphanId }).IsUnique();` (no `HasFilter` on IsDeleted)
- `Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs:244-260` (`RemoveOrphanFromGroupAsync` sets `IsDeleted = true`, row remains)
- `Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs:166-238` (`AddOrphansToGroupAsync`; duplicate check reads only `!IsDeleted` rows)

**Failure scenario:**
1. Admin adds orphan O to batch B, then wrongly — removes O (`RemoveOrphanFromGroupAsync` soft-deletes the `OrphanPaymentItem` row; the DB row stays).
2. Later (same session or months later) they re-add O to B.
3. The existing-membership check filters `!opi.IsDeleted` → soft-deleted row invisible → O passes as "not in group".
4. INSERT executes → SQL Server raises 2601 on `IX_OrphanPaymentItem(OrphanPaymentId, OrphanId)` which has **no** `IsDeleted` filter → raw 500.
5. O can never be re-enrolled in B through any code path until someone hard-deletes the row in the database. The FE "add orphans" dialog reports a server error, and retrying always fails.

**Evidence:**
```csharp
// OrphanPaymentItemConfiguration.cs:73
builder.HasIndex(x => new { x.OrphanPaymentId, x.OrphanId }).IsUnique();
```
```csharp
// RemoveOrphanFromGroupAsync — soft delete only
orphanPaymentItem.IsDeleted = true;
```
The snapshot agrees (`ApplicationDbContextModelSnapshot.cs:3386-3387` — `.IsUnique()` with no filter), so the deployed index has the same defect.

**Verdict:** Confirmed

---

## 3. Soft-deleted batches remain fully writable — `FindAsync` bypasses IsDeleted, no global query filter

**Severity:** High

**Location:**
- `Framework/Framework.Core/Data/Repositories/RepositoryBase.cs:354-357` — `GetByIdAsync` = `DbSet.FindAsync` (ignores IsDeleted)
- `Framework/Framework.Core/Data/ModelBuilderExtensions.cs` — `#region Global Query Filters` block is commented out (ends `*/`); no global filter exists
- `Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs:361-406` (`UpdatePaymentGroupAsync`), `412-429` (`MarkAsUploadedAsync`), `166-170` (`AddOrphansToGroupAsync`), plus `AssignBatchNumberAsync` / `SetExchangeRateAsync` / `LockExchangeRateAsync` — all load via repository `GetByIdAsync`

**Failure scenario:**
1. HQ deletes batch B (`DeletePaymentGroupAsync` → `IsDeleted = true`).
2. Client calls `PUT /api/OrphanPayments/{B}` (stale screen, replayed request, or direct API call).
3. `repo.GetByIdAsync(B)` = `FindAsync` returns the **deleted** entity → `BatchNo`/`GroupName`/`ExchangeRate` mutated and `SaveChanges` succeeds.
4. The service then re-reads via the *filtered* `GetPaymentGroupByIdAsync` → returns null → throws `"Failed to retrieve updated payment group"` → user gets a 400-style failure **after the mutation was already persisted**. DB and UI now disagree.
5. Same pattern: `MarkAsUploadedAsync`, `AddOrphansToGroupAsync`, `AssignBatchNumberAsync`, `SetExchangeRateAsync`, `LockExchangeRateAsync` all succeed silently on deleted batches.
6. Also: `RemoveOrphanFromGroupAsync` on a soft-deleted item (loaded via unfiltered item read path at the group level) returns 200 instead of 404.

**Evidence:**
```csharp
// RepositoryBase.cs:354-357
public virtual async Task<TEntity?> GetByIdAsync(TKey id) =>
    await DbSet.FindAsync(id);
```
```csharp
// OrphanPaymentService.cs:394 (runs on the deleted row before the filtered re-read)
paymentGroup.BatchNo = dto.BatchNo;
```

**Verdict:** Confirmed

---

## 4. Role mismatch: FE admits Accountant/FinancialOfficer to the list screen the BE rejects — and the failure is swallowed into a blank screen

**Severity:** High

**Location:**
- `Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:38` — `GET /` = `[Authorize(Roles = "SuperAdmin,Admin,Charity")]`
- `Frontend/src/app/core/services/auth.service.ts` — `PERMISSION_ROLES['OrphanPayments.View']` includes Accountant + FinancialOfficer (5 roles)
- `Frontend/src/app/modules/orphan-payments/orphan-payment-list/orphan-payment-list.component.ts:240-243` — `error: () => { this.loading = false; }`

**Failure scenario:**
1. Accountant signs in. Sidebar shows مدفوعات الأيتام because `hasPermission('OrphanPayments.View')` passes (FE map is wider than BE), and the route guard passes for the same reason.
2. List component calls `GET /api/OrphanPayments` → 403 (role not authorized).
3. Error handler is empty → `loading = false`, no toast, no message → **permanently blank grid** with no explanation.
4. Charity users hit a different 403: the endpoint admits `Charity`, but the D4 guard on `GetPaymentGroups` throws when the charity claim is absent — and the FE list never sends `orphanId` (the only path that scopes a Charity token).

**Evidence:** Controller `[Authorize(Roles = "SuperAdmin,Admin,Charity")]` vs FE map listing `['SuperAdmin','Admin','Accountant','FinancialOfficer','Charity']`; empty `error:` callback at line 240.

**Verdict:** Confirmed

---

## 5. `UpdatePaymentGroupAsync` writes BatchNo with no uniqueness check — the edit screen can trivially produce a raw 500

**Severity:** High

**Location:** `Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs:394`

**Failure scenario:**
1. The edit dialog's batch-number picker is fed by `GET /batch-numbers`, which lists **every group's** batch number.
2. User opens group A for edit and picks a number that belongs to group B (the picker offers it).
3. `paymentGroup.BatchNo = dto.BatchNo;` — no `IsBatchNoUniqueAsync` call on this path (the create path and `AssignBatchNumberAsync` do check).
4. `SaveChanges` → 2601 on `IX_OrphanPayment_BatchNo` → raw 500 with no field-level guidance.
5. Additionally `dto.BatchNo` may be null → silently clears the batch number; and any value up to 50 chars is accepted (spaces, `/`, `%`, `#`) — see findings 9 and 13.

**Evidence:**
```csharp
// Create path validates; update path does not
paymentGroup.BatchNo = dto.BatchNo;   // service:394 — no uniqueness check
```

**Verdict:** Confirmed

---

## 6. `Epic10_PaymentDisbursement.Down()` recreates the BatchNo index with a PostgreSQL predicate — rollback fails on SQL Server

**Severity:** Medium

**Location:** `Backend/src/IIROSA.Infrastructure/Data/Migrations/20260824111613_Epic10_PaymentDisbursement.cs:204` (Up at line 122 is correct)

**Failure scenario:**
1. `dotnet ef database update` to a target before this migration (rollback path).
2. Down drops the index, then recreates it with `filter: "\"BatchNo\" IS NOT NULL AND \"IsDeleted\" = false"`.
3. That is PostgreSQL syntax/boolean literal — SQL Server rejects it (quoting + `false`) → the rollback aborts mid-migration, leaving the index dropped and `__EFMigrationsHistory` inconsistent.
4. Up (line 122) uses the valid `"[BatchNo] IS NOT NULL AND [IsDeleted] = 0"` — only the reverse direction is broken.

**Evidence:**
```csharp
// line 122 (Up — valid)
filter: "[BatchNo] IS NOT NULL AND [IsDeleted] = 0");
// line 204 (Down — invalid on SQL Server)
filter: "\"BatchNo\" IS NOT NULL AND \"IsDeleted\" = false");
```

**Verdict:** Confirmed

---

## 7. Batch-number auto-generation breaks on manually-entered numbers sharing the prefix

**Severity:** Medium

**Location:** `Backend/src/IIROSA.Infrastructure/Data/Repository/OrphanPaymentRepository.cs:45-68` (`GetNextBatchNumberAsync`)

**Failure scenario:**
1. Generated numbers are D4-padded (`BP-202608-0001`), so pure-auto ordering is lexicographically correct — but manual entry (finding 5) can store anything ≤50 chars.
2. Someone stores `BP-202608-9` (unpadded) or `BP-202608-` (bare prefix) or `BP-202608-Z` on any group via update/assign.
3. `OrderByDescending(op => op.BatchNo)` is a **string** sort: `"BP-202608-9"` / `"BP-202608-Z"` sort above `"BP-202608-0009"` → that row becomes `lastBatch`.
4. `int.TryParse` on `"9"`→ returns 100 (jump, may collide → 500); on `"Z"` or `""` → fails → `nextSequence` stays 1 → returns `BP-202608-0001`, which already exists → 2601 on create → **every** new-batch create 500s for the remainder of the month.
5. No concurrent-creation guard either: two HQ users creating simultaneously both read the same `lastBatch` → both get the same number → one 500s.

**Evidence:**
```csharp
.OrderByDescending(op => op.BatchNo)   // string ordering
...
if (int.TryParse(lastSequenceStr, out var lastSequence)) { nextSequence = lastSequence + 1; }
// parse failure silently falls back to 1
```

**Verdict:** Confirmed (trigger requires a manual prefix-sharing value; auto-only usage is safe up to 9999)

---

## 8. Charity fail-open also on `GET /batch-numbers` and `GET /by-batch-no/{batchNo}` (read paths)

**Severity:** Medium

**Location:**
- `Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:77-92` (`GetBatchNumbers` — no D4 guard)
- `Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:609-632` (`GetByBatchNo` — no D4 guard)
- `Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs:650-675` and `808-823` (participation check skipped when `userCharityId` null)

**Failure scenario:**
1. Same claim-less Charity token as finding 1.
2. `GET /api/OrphanPayments/batch-numbers` → service runs unscoped → returns the batch numbers of **all charities** (cross-tenant enumeration).
3. `GET /api/OrphanPayments/by-batch-no/BP-202608-0001` → participation check `if (userCharityId.HasValue && ...)` skipped → any batch's header (dates, totals, orphan counts) is returned.

**Evidence:**
```csharp
// GetByBatchNoAsync:816-820 — guard conditioned on HasValue
if (userCharityId.HasValue && batchCharityId != userCharityId.Value) throw ...
```

**Verdict:** Confirmed

---

## 9. Trim inconsistency: picker groups by `Trim()`, lookups exact-match

**Severity:** Medium

**Location:**
- `Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs:667-670` — `GroupBy(p => p.BatchNo!.Trim())`
- `Backend/src/IIROSA.Infrastructure/Data/Repository/OrphanPaymentRepository.cs:27-31` (`GetByBatchNoAsync` — `op.BatchNo == batchNo`) and `33-43` (`IsBatchNoUniqueAsync` — exact)

**Failure scenario:**
1. Manual entry stores `" BP-1"` (leading/trailing space — charset unvalidated).
2. Picker trims → displays `BP-1`; user clicks it → `GET by-batch-no/BP-1` → exact match finds nothing → 404 for a number the UI just offered.
3. Uniqueness also exact → `" BP-1"` and `"BP-1"` coexist as two visually identical batches.

**Verdict:** Confirmed

---

## 10. `GetFilteredPaginatedAsync` charity filter is an empty no-op

**Severity:** Medium

**Location:** `Backend/src/IIROSA.Infrastructure/Data/Repository/OrphanPaymentRepository.cs:130-133`

**Failure scenario:**
1. HQ user filters the list by a charity (the FE passes `charityId`).
2. The repository branch is `if (charityId.HasValue) { /* intentionally not applied yet */ }` → filter silently ignored.
3. Grid shows unfiltered rows and `totalCount` from the unfiltered count → user believes they are looking at one charity's batches.

**Evidence:**
```csharp
if (charityId.HasValue)
{
    // Join activation owned by 10-7 — intentionally not applied yet.
}
```

**Verdict:** Confirmed

---

## 11. Charity attribution inconsistent across stats vs row mapping (family-fallback divergence)

**Severity:** Medium

**Location:**
- `Backend/src/IIROSA.Infrastructure/Data/Repository/OrphanPaymentRepository.cs:265-274` (`GetOrphanCountByCharityAsync` — groups by `Orphan.FK_CharityId` only, `.Where(g => g.CharityId.HasValue)` drops nulls)
- Contrast: `OrphanPaymentService.cs:925` (`MapOrphanPaymentItemDto` falls back `Orphan.FK_CharityId ?? Orphan.Family?.FK_CharityId`) and `OrphanPaymentItemRepository.cs:83-94` (`GetGroupIdsByCharityAsync` checks both)
- `Backend/src/IIROSA.Infrastructure/Data/Repository/OrphanPaymentRepository.cs:276-283` — `GetOrphanCountByRegionAsync` returns an **empty dictionary always**

**Failure scenario:**
1. Orphan O has `FK_CharityId = null`, charity derived from Family (the exact case the row mapper handles).
2. O's rows count toward the charity's scope checks and appear in the charity's detail views — but `GetOrphanCountByCharityAsync` drops them (`CharityId.HasValue` filter) → per-charity stats don't sum to the batch's total; some charities receive no entry at all.
3. Any consumer of region counts gets an always-empty result.

**Verdict:** Confirmed

---

## 12. Available-orphans search: charity filter lacks the family fallback; region/center filters are silent no-ops

**Severity:** Medium

**Location:** `Backend/src/IIROSA.Infrastructure/Data/Repository/OrphanRepository.cs` — `SearchFilteredAsync` (charity predicate `o.FK_CharityId == charityId.Value` only; `regionId`/`centerId` `if` blocks with **empty bodies**)

**Failure scenario:**
1. Charity user opens "add orphans": family-derived orphans (their charity lives on the Family) are **not offered** even though they are enrollable and counted as that charity's elsewhere (finding 11 contrast).
2. The FE sidebar sends `regionId`/`centerId`; the repository's `if` blocks are empty → filters silently ignored → user sees all regions/centers and believes the filter applied.

**Verdict:** Confirmed

---

## 13. BatchNo charset unvalidated — URL-hostile values make `by-batch-no` unreachable

**Severity:** Low

**Location:** DTO/validator (BatchNo only `MaxLength(50)` + optional pattern on create; manual paths accept anything) and route `GET by-batch-no/{batchNo}` (`OrphanPaymentsController.cs:609`)

**Failure scenario:**
1. User stores `BP/2026%08` via update (finding 5 path).
2. `GET /api/OrphanPayments/by-batch-no/BP/2026%08` — the `/` terminates the route segment → 404 from routing; `%08` decoding further mangles the value. The batch becomes unopenable by number from the FE.

**Verdict:** Confirmed

---

## 14. Row stop/resume ignores the parent batch's state (deleted or uploaded)

**Severity:** Low

**Location:** `Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs:295-331` — action switch never checks `item.OrphanPayment.IsDeleted` or `IsBatchUploaded`

**Failure scenario:**
1. Batch is soft-deleted, or already uploaded/exported.
2. Direct `POST /orphan-items` with action 0 on one of its rows → stop/resume succeeds and mutates a deleted/uploaded batch. The FE gates this via `canModify()` (`orphan-payment-detail.component.ts:541`) but the server does not — hiding a button is not a control.

**Verdict:** Confirmed

---

## 15. Soft-deleted orphans still render, count, and are actionable inside batch details

**Severity:** Low

**Location:** `Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs` — detail nav includes (`IncludeOrphans`/`IncludeNavigationProperties`) do not filter `Orphan.IsDeleted`; `EnsureOrphanInCallerScopeAsync:545-565` 404s deleted orphans on other paths

**Failure scenario:**
1. An orphan row is soft-deleted (Family module) while enrolled in a live batch.
2. Batch detail still lists the orphan (item rows are live), counts it in totals, and allows stop/print actions on it — while the history endpoint 404s the same orphan. Inconsistent treatment of the same record.

**Verdict:** Confirmed

---

## 16. HQ-stop lock edges: empty NameIdentifier claim defeats the lock; every resume loads all HQ users

**Severity:** Low

**Location:**
- `Backend/src/IIROSA.Application/Services/TokenService.cs:46` — `new Claim(ClaimTypes.NameIdentifier, userDto.Id?.ToString() ?? string.Empty)`
- `Backend/src/IIROSA.Application/Services/OrphanPaymentService.cs:308-313` (lock check) and `351-355` (`IsHeadOfficeUserAsync` → `GetUsersInRoles(HeadOfficeRoles)`)

**Failure scenario:**
1. An HQ user whose `UserDto.Id` is null gets an **empty-string** NameIdentifier claim → `CurrentUserId` null → `StoppedByUserId` stored as null.
2. The resume lock (`StoppedByUserId == currentUserId || isHeadOffice`) then passes for anyone with the Charity role — the "only the stopper or HQ may resume" rule silently never engages for such stops.
3. Separately, every resume attempt loads the full membership of four HQ roles (with duplicates for multi-role users) — per-request cost that grows with HQ headcount.

**Verdict:** Confirmed

---

## 17. Payment-summary band 403s for the roles the detail screen admits

**Severity:** Low

**Location:** `Backend/src/IIROSA.Api/Controllers/DashboardController.cs:38-39` — `payment-summary` = `[Authorize(Roles = "SuperAdmin,Admin")]` while the detail route/permission admits Accountant/FinancialOfficer/Charity

**Failure scenario:**
1. Accountant opens a batch detail page → summary band requests `payment-summary` → 403 → error swallowed (`getPaymentSummary` handler) → band silently absent with no explanation.

**Verdict:** Confirmed

---

## 18. FE lifecycle/UX edges: missing `takeUntil` on list loads, silent error handler, capped pickers, double-click race

**Severity:** Low

**Location:** `Frontend/src/app/modules/orphan-payments/orphan-payment-list/orphan-payment-list.component.ts:240-243` (empty `error:`; subscriptions without `takeUntil`), add-orphans dialog (`pageSize: 200`), charities dropdown (`pageSize: 500`), `onToggleStop` (no in-flight guard)

**Failure scenarios:**
1. Navigating away mid-load leaks the subscription (no `takeUntil`, unlike the detail component which now uses it).
2. Orphan picker caps at 200 rows with no total indicator — orphans 201+ are unenrollable from the UI; charities beyond 500 invisible in HQ dropdowns.
3. Double-clicking stop/resume fires two requests; responses can arrive out of order and the last `this.orphans[idx] = row` wins — UI can show `isStopped = true` while the DB is false.

**Verdict:** Confirmed

---

## Verified NOT broken (checked, no finding)

- **Dual migrations folders are one lineage, not corruption.** `Backend/src/IIROSA.Infrastructure/Migrations/20260421205436_InitialCreate.cs` is the real base (creates Charities, Families, Orphans, …); `Data/Migrations/20260424161500_InitialApplicationCreate.cs` is an **empty** Up/Down pair; `Data/Migrations/20260428052011_UpdateApplicationSchema.cs` continues the stray folder's tables (e.g. drops `PhoneCode` on `Charities`) — a single applied chain spans both folders. EF discovers migrations by `[Migration]`/`[DbContext]` attribute scan regardless of folder/namespace, and the only snapshot (`Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs`) **is current** — it contains every epic-10 column (PaymentDate, IsStopped, StoppedOn, StoppedByUserId, ChiqueNum, ChildOrParent, FK_HousingFamilyId) and the filtered BatchNo index, so the next `migrations add` diffs correctly and a fresh DB replays both folders in id order reproducing the live schema. Organizational anomaly only; the one real defect there is finding 6 (Down() filter).
- **i18n key parity is clean.** `orphanPayments` blocks in `ar.json`/`en.json` have identical 161-key sets in both languages, no duplicate keys inside any object (object-scoped parser scan), all diff-template keys (`{{added}}`, `{{skipped}}`, etc.) present in both, and the 10-9 mass-duplicate sed incident is fully reversed (`orphansAddedResult` correct in `ar`).
- **OrphanPaymentItemRepository** filters `!IsDeleted` on every read/count/join; `OrphanRepository.GetPagedAsync` skips deleted orphans during enrolment.
- **List/history/detail core scoping** pins charity from the token (never from the request), 404s rather than leaking existence, and handles zero rows as 200 + empty.
- **Row-action validation** (0..4 inclusive, Flag required for 0) refuses actions 3/4 loudly with a localized message; `IsInRoleAsync` is correctly avoided in favour of `IUserAppService.GetUsersInRoles`.
- **FE grid math** (filter → sort → clamp → slice), null-amount ordering, Arabic `localeCompare`, `OnPush` + `markForCheck` — traced, no defect found.
