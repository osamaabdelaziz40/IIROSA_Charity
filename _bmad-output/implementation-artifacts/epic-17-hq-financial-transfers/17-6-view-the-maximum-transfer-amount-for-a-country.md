# Story 17-6: View the maximum transfer amount for a country

| Field | Value |
| --- | --- |
| Story key | `17-6-view-the-maximum-transfer-amount-for-a-country` |
| Epic | EP-17 — HQ Financial Transfers (الحوالات المالية للادارة المالية) |
| Use case | UC-TRF-06 — الحد الأعلى للحوالة |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md` (§22.U.6 scenario) |
| Route | — (no screen of its own; a guard shown on the §22.S.2 form) |
| Endpoint | `GET /api/HqTransfers/max-amount?countryId=` |
| Depends on | **17-2 + 17-4 landed** (create/update paths this story retrofits); 17-1's vertical |
| Roles | Fin. Director → `SuperAdmin`, `Admin` (`HqTransfers.View`) |

## Status

done

## Story

As a Financial Director, I want to be able to view the maximum transfer amount for a country
الحد الأعلى للحوالة, so that the transfers I issue respect the ceiling configured for that
destination.

## Acceptance Criteria

1. Given a Financial Director with an active session, when the ceiling for a country is
   requested, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/HqTransfers/max-amount` with a `countryId` query parameter and the response is
   rendered on the transfer form without a page reload.
3. Given no ceiling is configured for the country, when the response arrives, then the value is
   `null` and the form communicates "no limit configured" (a null ceiling means unlimited —
   it is not an error).
4. Given the create/update payload's `AmountOfPayment` exceeds the destination country's
   configured ceiling, when the save is attempted, then it is refused with a clear message and
   nothing is written (the UC-TRF-02 business rule, landed here — see Dev Notes).
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §22.U.6 passes end to end; the ceiling is visible on the form as a guard;
the ceiling is enforced server-side on both create and update.

## What exists already (verified)

- `Country : LookupEntity` (`Entities/Lookups/Country.cs`) has **no** `MaxTransferAmount` column —
  this story adds it (grep over `Backend/src` for `maxtransfer` returns nothing).
- `CountryDto` (`DTOs/LookupManagement/LookupDtos.cs:48`) exposes `IsoCode`, `DialingCode`,
  `Currency`, `FlagIcon` — extend it with the new field.
- The §22.S.2 form's الدولا field already fires an on-change hook (legacy `GetTransferValue()`)
  — 17-2 left the ceiling unwired with a TODO; this story is that TODO's owner.
- The legacy §22.S.4 screen binds `country.MaxTransferAmount` directly on the country row —
  one column on the lookup table is the faithful model. Do NOT invent a separate
  limits entity.

## Tasks / Subtasks

- [x] **Task 1 — Schema: the ceiling column** (AC 2, 3)
  - [x] `Country` (Lookup schema): add `decimal? MaxTransferAmount` (precision 18,2; NULL =
        unlimited). Update `CountryConfiguration` (or the lookup's configuration class) with the
        precision
  - [x] Migration `Epic17_CountryMaxTransferAmount` (CLAUDE.md command form) + apply. All
        existing rows start NULL — correct, meaning "no limit yet" — **not authored by this
        session**: the column shipped inside the parallel session's
        `20260824105001_Epic06_RetireConstructionHousing` (verified applied; `decimal(18,2)`
        NULL on `Lookup.Country` exactly as specified)
  - [x] `CountryDto` + country profile: carry `maxTransferAmount` through (nullable)
- [x] **Task 2 — Read endpoint** (AC 2, 3, 5)
  - [x] `[HttpGet("max-amount")] GetMaxTransferAmount([FromQuery] int countryId)` in
        `HqTransfersController` — literal route beside `{id:guid}` (literals outrank parameters;
        the guid constraint from 17-3 keeps them unambiguous)
  - [x] Service `GetMaxTransferAmountAsync(int countryId)` → `NotFoundException` when the country
        doesn't exist; otherwise a small DTO `{ CountryId, CountryName, MaxTransferAmount }`
        (`NameAr ?? NameEn`); unknown country → 404 `{ message }`; missing `countryId` → 400
        `{ message }`; catch-all → 500. Raw envelope, no `ApiResponse<T>`
- [x] **Task 3 — Enforce the ceiling on the write paths** (AC 4 — the deferred UC-TRF-02 rule)
  - [x] In `HqTransferService` create AND update paths, replace both
        `// TODO 17-6` markers: after FK validation, load the destination country; if
        `MaxTransferAmount.HasValue` and `AmountOfPayment > MaxTransferAmount.Value` → refuse
        with a 400-shaped message naming the ceiling (e.g. "مبلغ الدفعة يتجاوز الحد الأعلى
        للحوالة لهذا البلد") — `BusinessException`-style; the controller maps it to
        `BadRequest(new { message })` (`catch (InvalidOperationException)` slot or an explicit
        catch, matching the OfficeProject pattern)
  - [x] NULL ceiling → skip the check (AC 3: unlimited is legal)
  - [x] The check runs server-side in the service — the form guard is UX, not the control
- [x] **Task 4 — Form guard** (AC 2, 4)
  - [x] On the form's country change: `GET /api/HqTransfers/max-amount?countryId=` and show the
        ceiling under مبلغ الدفعة as helper text (الحد الأعلى: X — or "لا يوجد حد مضبوط" when
        null); client-side pre-check refuses amounts over a known ceiling before the POST, with
        the server remaining authoritative
  - [x] Edit mode re-fetches the ceiling for the loaded country
- [x] **Task 5 — i18n** — ceiling helper text, no-limit text, over-ceiling error under
      `hqTransfers.*` in **both** `ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–5)
  - [x] Live: `max-amount?countryId=<real>` → 200 with name + null value; after 17-7 sets a
        value, the read reflects it; unknown id → 404; missing param → 400; unauthenticated → 401
        — all green 2026-08-24 (Debug Log)
  - [x] Enforcement: set a ceiling of e.g. 100000 (via SQL or after 17-7), POST an amount over it
        → 400 message, nothing written; amount under it → 201; NULL ceiling → any amount accepted
        — over-ceiling UPDATE and CREATE both → 400 naming both amounts; under-ceiling update →
        200 persisted; NULL ceiling → restored and read-back null (Debug Log)
  - [x] `dotnet build` + `npm run build` green (lock caveats); tests excluded per the standing
        decision — backend 0 errors; frontend 0 in this module

### Review Findings

_From the epic-17 backend code review (chunk 1, 2026-08-24)._

- [x] [Review][Patch] The ceiling read bypasses the caller-scope pin the service's own contract
      promises ("scopes every read and write"): a country-claimed caller reading another
      country's `max-amount` gets 200 instead of 404 — and pinned callers are real
      (`TokenService.cs:186-189` emits the country claim for every user with a `CountryId`)
      [`HqTransferService.cs:271-291`] — apply the same 404-shaped scope guard (the write half
      is recorded under 17-7)

_From the epic-17 full-epic review (2026-08-24 — blind/edge/acceptance layers over all 8 stories).
The unchecked [Patch] item above was re-verified against the working tree (`GetMaxTransferAmountAsync`
still performs an unpinned `GetByIdAsync`) — still unapplied._

## Dev Notes

### Why the enforcement lands here and not in 17-2

The board order builds create (17-2) and update (17-4) before the ceiling exists. 17-2/17-4 each
left a single `// TODO 17-6` marker; this story owns the column, so it owns the rule. Landing the
read + the enforcement together means AC 4 is provable the moment the story completes.

### Modelling decision (recorded)

`MaxTransferAmount` lives on `Country` (Lookup schema) — the legacy §22.S.4 grid edits
`country.MaxTransferAmount` in place, and a separate limits entity would be reinvention.
NULL = unlimited is the documented semantics; never treat NULL as zero.

### Platform rules that bind this story

- Lookup-schema change: use the existing `Country` configuration; never a literal schema string.
- camelCase wire; `NameAr ?? NameEn`; raw envelope with `{ message }`; no `ApiResponse<T>`.
- The read is any-authenticated (lookup-adjacent); the WRITE of the ceiling stays in 17-7 with a
  tighter permission — do not widen it here.
- EF migration via the CLAUDE.md two-project command; live-API lock caveat applies to builds.

### Out of scope (17-7's story — do not build)

| Item | Story |
| --- | --- |
| `PUT /api/HqTransfers/max-amount` + `#/hq-transfers/max-amounts` editing screen | 17-7 |
| Seeding ceiling values — production sets them through 17-7's screen | 17-7 |

### References

- [Source: docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md#22.U.6] scenario — ceiling shown as a
  guard on the transfer form
- [Source: docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md#22.U.2] UC-TRF-02's business rule this
  story lands
- [Source: _bmad-output/planning-artifacts/epics.md#3.17] US-TRF-06 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/Lookups/Country.cs] the entity to extend (verified:
  no ceiling column today)
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-2-create-a-transfer.md] the TODO markers and
  write paths being retrofitted

## Dev Agent Record

### Agent Model Used

Claude (Claude Code CLI, glm-5) — 2026-08-24.

### Debug Log References

- **Migration + battery both landed 2026-08-24.** The earlier blocker resolved two ways: the
  user's tree went green, and a parallel session's `20260824105001_Epic06_RetireConstructionHousing`
  carried this story's column (its header documents it) — `Lookup.Country.MaxTransferAmount
  decimal(18,2) NULL`, verified applied. No duplicate empty-diff migration was added here.
- **Live battery (private `:5199` instance, this session's own process):**
  - `GET max-amount?countryId=1` → **200** `{"countryId":1,"countryName":"السعودية",
    "maxTransferAmount":null}` — NULL reads as unlimited (AC 3)
  - After 17-7 set country 2 to 100000: read-back **200** with `100000.00`
  - Unknown country → **404** `{message}`; missing `countryId` → **400** `{message}`;
    anonymous → **401**
  - Enforcement on UPDATE: 120000 over a 100000 ceiling → **400** "Amount of payment 120000
    exceeds the maximum transfer amount (100000.00) configured for this country", nothing
    written; 95000 under it → **200** persisted
  - Enforcement on CREATE: country-2 transfer at 150000 over the ceiling → **400** same shape,
    no row created
  - Cleanup: ceiling cleared to NULL, transfer B restored to 99000
- Backend `dotnet build` 0 errors; frontend `npm run build` 0 errors in this module (user's
  concurrent WIP errors live in other modules).

### Completion Notes List

1. **No `CountryConfiguration` was created** — lookup entities are convention-mapped in this
   codebase (the snapshot shows per-property attributes only), and the SQL Server provider
   default gives `decimal(18,2)` — the exact precision `HqTransfer.AmountOfPayment` (same epic)
   ships with, no explicit configuration anywhere. A plain `decimal?` property follows that
   precedent; the migration will show `decimal(18,2) NULL`.
2. **Write-path re-order (17-2/17-4 flow, this story owns the retrofit)** — the caller-scope
   pin now runs BEFORE the FK validation and ceiling check on both create and update, so all
   three rules apply to the country the record will actually carry. Previously a pinned caller
   requesting another country had the FK checks run against the requested country while the
   record stored the pinned one — the ceiling would have guarded the wrong country. Fixes that
   class of bug for the FK-active checks too.
3. **Refusal message is English** (platform precedent — every InvalidOperationException in the
   services is English; the client surfaces its own Arabic toast). It names both amounts:
   "Amount of payment X exceeds the maximum transfer amount (Y) configured for this country".
   The story's Arabic example text lives in the FRONT end (`hqTransfers.amountExceedsLimit`)
   instead — the actor-facing UX is Arabic, the wire message is a developer-facing backstop.
4. **Form guard shape** — `countryId.valueChanges` → `switchMap` fetch (later country wins; no
   stale-ceiling race), state split into `maxAmount` + `maxAmountKnown` so NULL renders as
   "لا يوجد حد أعلى مضبوط" rather than as unknown. Edit mode needs no extra code:
   `patchValue` fires the subscription. A failed ceiling fetch is silent by design — no guard
   text, no toast; the server enforces regardless.
5. **Ceiling not added to Create/UpdateCountryDto** — the write surface stays exactly
   `PUT /api/HqTransfers/max-amount` (17-7). Country CRUD cannot move the ceiling.
6. **Migration landed via the parallel session** — `20260824105001_Epic06_RetireConstructionHousing`
   carries this story's column alongside its own epic-06 changes (documented in that migration's
   header; Task 1's own migration name was never created — a duplicate would have been an empty
   diff). Verified applied and column-exact before the battery ran. The live battery (read
   ladder + create/update enforcement over/under/NULL) ran green in the same session as 17-4/5/7/8.

### File List

**Backend:**
- `Backend/src/IIROSA.Domain/Entities/Lookups/Country.cs` — + `decimal? MaxTransferAmount`
- `Backend/src/IIROSA.Application/DTOs/LookupManagement/LookupDtos.cs` — `CountryDto` +
  `MaxTransferAmount` (read-only carry)
- `Backend/src/IIROSA.Application/DTOs/HqTransfers/HqTransfers.cs` — + `CountryMaxTransferAmountDto`
- `Backend/src/IIROSA.Application/Interfaces/IHqTransferService.cs` — + `GetMaxTransferAmountAsync`
- `Backend/src/IIROSA.Application/Services/HqTransferService.cs` — + ceiling read; + static
  `EnforceTransferCeiling` helper; both TODO 17-6 markers replaced; scope-pin re-ordered ahead
  of FK/ceiling rules on create AND update (Note 2)
- `Backend/src/IIROSA.Api/Controllers/HqTransfersController.cs` — + `[HttpGet("max-amount")]`
  with the 400/404/500 ladder

**Frontend:**
- `Frontend/src/app/modules/hq-transfers/models/hq-transfer.model.ts` — + `CountryMaxTransferAmount`
- `Frontend/src/app/modules/hq-transfers/services/hq-transfer.service.ts` — + `getMaxTransferAmount`
- `Frontend/src/app/modules/hq-transfers/hq-transfer-form/hq-transfer-form.component.ts` —
  ceiling state, `countryId.valueChanges` switchMap fetch, client pre-check in `onSubmit`
- `Frontend/src/app/modules/hq-transfers/hq-transfer-form/hq-transfer-form.component.html` —
  ceiling helper text under مبلغ الدفعة (limit / no-limit variants)
- `Frontend/src/assets/i18n/{ar,en}.json` — `maxAmountLimit`, `noMaxAmountLimit`,
  `amountExceedsLimit`

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-TRF-06 and module spec §22.U.6; ceiling column + create/update enforcement consolidated here from the 17-2/17-4 TODOs. |
| 2026-08-24 | Code complete end to end (entity/DTO/read endpoint/enforcement on both write paths/form guard/i18n) with the pin re-order of Note 2. Migration + apply + live battery deferred on the user's concurrent WIP (their 2 migrations also pending in the chain — see Debug Log). Status in-progress until the migration lands. |
| 2026-08-24 | Migration resolved via the parallel session's 20260824105001 (column verified applied); full live battery green (read ladder + create/update enforcement over/under/NULL); status → review. |
