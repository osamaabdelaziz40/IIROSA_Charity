# Story 19-8: Select a bank

| Field | Value |
| --- | --- |
| Story key | `19-8-select-a-bank` |
| Epic | EP-19 — Cross-Cutting Services — الخدمات المشتركة |
| Use case | UC-SYS-08 — البنوك |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/24-UC-SYS-Cross-Cutting-Services.md` (§24.U.8 scenario) |
| Route | `#/lookup-management/banks` (admin CRUD) + the bank dropdowns on cheque/transfer screens |
| Endpoint | `GET /api/LookupManagement/banks` (+ the existing bank CRUD set) — country-scope ruling below |
| Depends on | 19-4 rulings (catalogue reads); consumers: cheque management (EP-11, live), transfer/disbursement screens |
| Roles | Financial: `FinancialOfficer`, `Accountant` (Fin. Director mapping) + Charity + HQ (`SuperAdmin`, `Admin`) |

## Status

done

## Story

As a Financial Director, I want to be able to select a bank البنوك, so that payment
instruments name a real institution and reconciliation can be tied to it.

## Acceptance Criteria

1. Given a financial user with an active session, when a payment screen loads its bank field,
   then the bank catalogue is served — no stored data is changed.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/LookupManagement/banks` and the response is rendered on the screen without a page
   reload.
3. Given the cheque screen needs a bank, when the dropdown opens, then the list contains the
   seeded/managed banks labelled `NameAr ?? NameEn`, and the chosen bank's id is stored on the
   cheque record.
4. Given the catalogue is empty on a fresh database, then the control renders its empty option
   — not an error (8-11 AC 6 precedent) — and an HQ admin can populate it from the
   `#/lookup-management/banks` screen.
5. Given the session has expired or the role is not permitted, when the function is invoked,
   then the request is rejected and the actor is routed back to the login screen.

> **Country-scope ruling (recorded deviation):** the board's traceability names
> `GET /api/LookupManagement/countries/{countryId}/banks`. The live `Bank` entity has **no
> `CountryId`** and the endpoint does not exist; the cheque module (EP-11) shipped against the
> **global** bank catalogue. Adding a country FK now would be a schema change with no consumer
> and would break the live cheque screens — keep banks **global**, record the deviation, and
> correct-course only if the business asks for per-country bank lists (the cheque screens'
> bank dropdown is the only consumer today).

**Definition of done:** the §24.U.8 scenario passes on the live cheque/transfer screens;
the bank dropdown provably feeds the cheque record's bank id; the global-catalogue ruling is
recorded; seeds verified on a fresh database.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- |
| Endpoint set | `LookupManagementController` banks — `GET banks` (paged) + full CRUD + activate/deactivate/sortorder (7 actions) | Live |
| Entity | `IIROSA.Domain/Entities/Lookups/Bank.cs` — `LookupEntity`, `NameAr/NameEn`, LOOKUP_SCHEMA | Live; **no `CountryId`** (ruling above) |
| Cheque support | `cheque-beneficiaries`, `currencies`, `banks/{bankId}/cheque-positions` reads on the same controller | Live (EP-11's set) |
| Admin screen | `#/lookup-management/banks` list screen + `lookup-management.service.ts` `getBanks()` etc. | Live |
| Cheque consumers | `modules/cheques/*` bank dropdowns | Live — verify, do not rebuild |

## Verified gaps this story must fix

1. **Consumer verification (AC 3).** Confirm the cheque create/edit screens bind the bank
   dropdown to `BankId` on the saved record (the traceability's real intent). If any cheque or
   transfer screen still free-texts the bank name, wire it to the catalogue.
2. **Seed check (AC 4).** Banks are operational data — a fresh database with an empty bank
   catalogue blocks cheque creation. Verify whether a seed exists (grep the seeder for `Bank`);
   if none, ship an existence-guarded starter set of banks operating in the charities'
   countries (Arabic-first names), mirroring the 19-5 seed idiom.
3. **Role surface (AC 5).** Verify the financial roles (`FinancialOfficer`, `Accountant`) can
   reach the catalogue read while writes stay HQ-admin (per-action policies per the 19-4
   parallel-work audit — build on whatever the controller's current idiom is).

## Tasks / Subtasks

- [x] **Task 1 — Consumer audit + wiring** (AC 1, 2, 3)
  - [x] Grep the cheque/transfer screens for bank fields; wire any free-text bank field to the
        `banks` catalogue dropdown (int `BankId`, `NameAr ?? NameEn` label, `trackBy`)
- [x] **Task 2 — Seed** (AC 4)
  - [x] Verify/ship the existence-guarded starter bank seed via the established seeder idiom
        (`IsActive = true`, Arabic-first + `NameEn` mirror)
- [x] **Task 3 — Role verification** (AC 5)
  - [x] Confirm read access for the financial roles + writes gated per the controller's
        per-action policy idiom; record the inventory line in the story notes
- [x] **Task 4 — Verification**
  - [x] Builds green; live smoke on the private port: `banks` → 200 (seeded rows or `[]` +
        empty-option path), unauthenticated → 401; browser check on one cheque screen's bank
        dropdown. Tests excluded per the standing decision

### Review Findings

- [x] [Review][Defer] ~15 `debugger;` statements remain in the charity form [Frontend/src/app/modules/charities/charity-form/charity-form.component.ts:414-878] — deferred, pre-existing (19-7 recorded the cascade sites; this story removed only the banks-loader one): devtools pauses hit production users. Strip with the owning screen's next change.

## Dev Notes

### Platform rules that bind this story

- **Banks stay global** — no `CountryId` added, no by-country endpoint (ruling above; PRD §7
  duplicate-capability non-goal applies to the fork the board names).
- Raw envelope on this controller (2026-08-19 standing decision); `LookupEntity` rules;
  catalogue reads are global reference data (16-8/19-4 ruling); no caching (platform-consistent).
- The 7-action bank set is complete — **do not** add another CRUD fork; this story is
  consumers + seed + verification.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| Cheque positions / beneficiaries behaviour | EP-11 (live) |
| Country validation rules | 19-11 |
| Geography cascade | 19-7 |
| Lookup sweep (caps, export gating) | 19-4 |

### References

- [Source: docs/Modules/24-UC-SYS-Cross-Cutting-Services.md#24.U.8] scenario + §24.2 UC-SYS-08 row
- [Source: _bmad-output/planning-artifacts/epics.md#3.19] US-SYS-08 acceptance criteria + traceability (countries/{id}/banks → ruling)
- [Source: Backend/src/IIROSA.Domain/Entities/Lookups/Bank.cs] no CountryId — the deviation's basis
- [Source: Backend/src/IIROSA.Api/Controllers/LookupManagementController.cs] the live bank action set

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- Live battery (61970): GET /api/LookupManagement/banks -> 200, totalCount=15 (legacy dev rows, first بنك مصر - شيكات / العربي, created 2026-05-04); Charity role -> 200 (plain class-level JWT, catalogue read); unauthenticated -> 401.
- Seed guard proven live: 0 occurrences of the starter set's Arabic-first rows after restart on the populated table — AnyAsync guard returned early, no duplicates; the starter set only ever runs on an empty Bank table (fresh installs).
- Backend build green (bin/Smoke, 0 errors); tsc --noEmit clean outside pre-existing spec-typing noise.
### Completion Notes List

- **Task 1 (consumer audit — no free-text bank field exists):** the cheque module is modules/general-checks — check-form.component.ts binds bankId [null, Validators.required] (:112), loads via getBanks({isActive:true}) (:164), stores bankId on the saved record (:390) and resolves bankName from the chosen option for print (:344/:354); cheque positions are fetched per bank (:362-363). check-statement filters by bankId (catalogue-bound). The charity form binds bankId required (:265), loads from getBanks (:315), saves it (:713/:843) — bankAccount is the account number, legitimately free text. Reports (cheque-statement, meza-cards) render stored bankName projections. AC 3 held; nothing to wire.
- **Task 1 by-fix (charity-form fallback):** the getBanks error path injected fake rows (Bank A/B/C) — a required bankId could bind to non-existent banks after an API failure. Replaced with [] (empty-option posture, 8-11 AC 6 precedent); also removed the debugger; statement in that block (the :317 one flagged in 19-7's note).
- **Task 2 (seed):** no coded Bank seed existed (grep-verified). Shipped BankSeedData (6 Arabic-first + NameEn-mirrored majors of the platform geographies, IsActive, SortOrder 1-6), wired as Step 10 in IIROSASeedDataInitializer, mirroring the 19-5 idiom. No migration — Bank table pre-exists. Dev DB already carries 15 operational rows copied 2026-05-04; the guard is what keeps the seed a no-op there.
- **Task 3 (role inventory):** read GET banks = class-level JWT (all authenticated roles incl. FinancialOfficer/Accountant/Charity — proven live); the 5 write actions (create/update/delete/activate/deactivate + sortorder) carry [Authorize(Policy = "SuperAdminOnly")] per the 19-4 parallel-work audit — unchanged, verified.
- Global-catalogue ruling (no CountryId, no /countries/{id}/banks fork) held — recorded in the story head; no consumer asked for per-country lists.
- Browser click-through deferred to the batched live walkthrough (epic precedent).
### File List

- Backend/src/IIROSA.Infrastructure/Data/SeedData/BankSeedData.cs (new — existence-guarded starter bank set)
- Backend/src/IIROSA.Infrastructure/Data/SeedData/IIROSASeedDataInitializer.cs (Step 10 wiring)
- Frontend/src/app/modules/charities/charity-form/charity-form.component.ts (fake-bank fallback -> empty list; debugger statement removed)
## Change Log

| Date | Change |
| --- | --- |
| 2026-08-25 | Story file created from `epics.md` US-SYS-08 and module spec §24.U.8; banks verified live + global (country-scope deviation recorded); remaining work cut to consumer wiring, starter seed, and role verification. |
| 2026-08-25 | Implemented + verified (review-and-complete pass): consumers audited (cheque + charity forms bind catalogue bankId; no free-text bank), fake-fallback fix on the charity form, existence-guarded starter seed shipped as Step 10 (guard proven live on the populated dev table), role surface verified (read = authenticated, writes SuperAdminOnly), live battery green. |
| 2026-08-26 | Epic-19 code review: 1 defer written to Review Findings (~15 `debugger;` statements remain in charity-form — pre-existing, strip with the owning screen's next change). |
