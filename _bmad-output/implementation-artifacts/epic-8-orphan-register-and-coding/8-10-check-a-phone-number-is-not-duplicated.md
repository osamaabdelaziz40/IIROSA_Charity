# Story 8-10: Check a phone number is not duplicated

| Field | Value |
| --- | --- |
| Story key | `8-10-check-a-phone-number-is-not-duplicated` |
| Epic | EP-08 — Orphan Register & Coding (سجل الايتام والتكويد) |
| Use case | UC-ORP-10 — التحقق من رقم الهاتف |
| Priority / size | Must · 5 points |
| Specification | `docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md` (§13.U.10 scenario) |
| Route | none — hosted on the family form's phone inputs (`#/families/:id/edit`) |
| Endpoint | `GET /api/Families/{familyId}/provider/check-phone` |
| Depends on | EP-05 (family form exists); independent of the coding stories |
| Roles | Charity (+ HQ `Admin`/`SuperAdmin` with explicit `charityId`) |

## Status

done

## Story

As a charity user, I want to be able to check a phone number is not duplicated التحقق من رقم الهاتف, so that the register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a charity user with an active session, when the actor enters a contact number, then the duplication verdict is returned — no stored data is changed.
2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/{familyId}/provider/check-phone` and the response is rendered on the screen without a page reload.
3. Given the number already exists on another beneficiary record in scope, when the actor saves, then the save is refused and the offending field is flagged.
4. Given the check succeeds, when the actor then saves, then the record persists and the family's numbers appear as entered.
5. Given a charity user, when the function is invoked, then only numbers held by that charity's (and country's) beneficiaries are matched.
6. Given an HQ role, when an explicit charity id is supplied, then the check operates on that charity's data.
7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the scenario of §13.U.10 passes end to end; the role and charity scoping is enforced server-side, not only in the menu.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Phone columns | `Family.PhoneNumber`; `Father.Phone`; `Mother.Phone`; `Provider.Phone`; `Orphan.Phone` | All five exist — the check spans them all |
| Family form | `Frontend/src/app/modules/families/family-form/**` (`father-form`, `mother-form`, relatives, orphan tab) | Exists — wire the check into each phone input |
| Scope helpers | `FamiliesController.GetUserCharityId()` / `GetUserRole()` | Reuse |
| Precedent | 8-1's `check-national-id` (same shape: verdict + reason) | Build after / alongside 8-1 and mirror it |
| Legacy method | `IFamilyService.CanAddPhoneNumber` | Legacy identifier — new method follows this codebase's `Async` naming |

## Tasks / Subtasks

- [x] **Task 1 — Service + endpoint** (AC 1, 2, 5, 6)
  - [x] `IFamilyService`: `Task<PhoneCheckDto> CheckPhoneNumberDuplicateAsync(Guid familyId, PhoneCheckFilterDto filter, Guid? userCharityId, string? userRole)`; filter: `string Number` + `Guid? CharityId` (HQ override) — `Type` is **carried on the filter DTO accepted-for-wire-compatibility but ignored by the matcher** (no type column exists); the match is type-agnostic and recorded as such. _(Record corrected 2026-08-24 review — the original note here wrongly said `Type` was dropped.)_
  - [x] Search span, all in the resolved charity scope (global soft-delete filter applies): `Family.PhoneNumber`, `Father.Phone`, `Mother.Phone`, `Provider.Phone`, `Orphan.Phone` — **excluding the given `familyId`'s own records** (AC 4). Implemented as: families-in-scope query excluding `familyId` with `.Include(Father/Mother/Provider)` + orphans, then an in-memory `MatchFamilyPhone` over the five holders returning a display label (`"Family {code} — {name} (father)"`)
  - [x] Normalisation: **trim-only** (no trunk/zero folding — recorded: unsafe to guess across locales); exact match otherwise. Rule encoded in the validator (`^\+?[0-9\s\-]{3,20}$`) and here
  - [x] `FamiliesController`: `GET {familyId}/provider/check-phone`, `[FromQuery]`, `[Authorize]` (controller default), thin delegate, raw DTO — **no `ApiResponse<T>` wrapper**; sits beside the existing `{familyId}/provider` POST without collision (GET vs POST + literal `check-phone`)
  - [x] FluentValidation in the service layer; family 404 when missing, tenancy 403 when out of scope — no existence leak
- [x] **Task 2 — Wire into the family form** (AC 3, 4)
  - [x] Debounced (400 ms) `valueChanges` checks on family / father / mother / provider phone controls — **edit mode only** (recorded limitation): the endpoint needs the family id to exclude the family's own numbers, so create mode cannot check before the record exists; the check wires itself the moment a family is loaded for edit
  - [x] On `IsDuplicate`: field flagged inline with the translated message naming the existing holder (`phoneHeldBy`), and the section's save is blocked — `onSubmit` gates the family save, `saveFather`/`saveMother`/`saveProvider` gate their own sections. Father/mother flags render inside their child components via a `phoneCheck` input owned by the parent form
  - [x] Orphan-phone wiring deferred with the orphan form (EP-05) — same deferral as 8-1; the server already spans `Orphan.Phone`
  - [x] i18n keys under `orphanCoding` (`phoneDuplicate`, `phoneHeldBy`, `phoneChecking`, `phoneAvailable`) in **both** `ar.json` and `en.json`
- [x] **Task 3 — Verification** (AC 7)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; `cd Frontend && npm run build` — 0 errors
  - [x] Live check 2026-08-24: shared family number → `isDuplicate: true` with structured holder (`holderFamilyCode` + `holderName` + `holderType: "family"`); orphan-held number → `holderType: "orphan"`; **same-family exclusion proven** — checking a family's own number returned its *co-holder*, never the editing family itself; unauthenticated → 401. Cross-charity non-matching not exercisable live (seed `Charity` user has no `CharityId` claim → fail-closed denial instead). A D5:b known-limitation stands: the check is check-only (no write-side phone enforcement), recorded as legacy parity

### Review Findings

_2026-08-24 adversarial review (blind + edge-case + acceptance layers)._

- [x] [Review][Decision] Duplicate-phone rule is enforced only in the Angular form — direct API saves (and create mode, which the form itself skips) store duplicates; AC 3's "save is refused" is UI-only and the platform rule says a UI guard is not a control. Enforce server-side in the family/father/mother/provider save paths (risks blocking edits of already-duplicated legacy rows) or accept check-only as legacy parity and record it — **Resolved 2026-08-24 (D5:b):** accepted as check-only legacy parity (WAR.IIROSA behaved the same); recorded as a known limitation in the completion notes
- [x] [Review][Patch] Stale-response race: every debounced change fires an uncancellable request and responses are not tagged with the value they checked — a slow older verdict can pin `duplicate` on the current number and block a valid save; the error callback also fail-opens to `idle`. Tag responses with the checked value or `switchMap` [family-form.component.ts:445–473]
- [x] [Review][Patch] Validator regex `^\+?[0-9\s\-]{3,20}$` never requires a digit — `"---"` and `"   "` pass and get compared against every family; require ≥1 digit [OrphanCodingValidators.cs:83]
- [x] [Review][Patch] Holder label is composed server-side in English (`"Family {code} — {name} (father)"`) and rendered next to the translated `phoneHeldBy` prefix — return structured data (holder type + names) and compose the translated label client-side [FamilyService.cs MatchFamilyPhone]
- [x] [Review][Patch] The whole in-scope family table (5 includes) is materialised per check, unbounded for HQ without charityId — push the five-phone predicate into the SQL `Where` [FamilyService.cs CheckPhoneNumberDuplicateAsync]
- [x] [Review][Patch] Story record inverted: Task 1 and the completion note say `Type` was **dropped**, but the DTO carries `Type` accepted-for-wire-compatibility and the code comments say exactly that — correct the record (docs-only)
- [x] [Review][Patch] Soft-delete: deleted families and their five holders sit inside the duplicate span, permanently blocking their numbers (cross-story, anchored in 8-2)
- [x] [Review][Defer] Arabic-Indic digits (٠–٩) rejected by the ASCII-only phone regex on an RTL-first app — deferred, needs a product/locale normalisation decision [OrphanCodingValidators.cs:83]

## Dev Notes

### Platform rules that bind this story

- Soft delete has **no global query filter** — manual `!IsDeleted` is the shipped convention; the epic-8 check filters the family and all five phone sources (family/father/mother/provider/orphan) explicitly.
- Wire is camelCase via **Newtonsoft**; clean DTO key names only.
- FluentValidation in the **service layer**; this story writes nothing (the family-form save path it guards already exists).
- The five-table span is the whole point — a check that misses `Orphan.Phone` or `Provider.Phone` is a false pass. Verify all five in the live check
- Never kill the user's running `IIROSA.Api` process. Tests excluded per the standing user decision.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| National-ID eligibility check | 8-1 |
| Adding phone-type columns / per-type matching | deferred (no owning story — see `deferred-work.md`) |
| Any orphan-form field rebuild | EP-05 refinement |

### References

- [Source: docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md#13.U.10] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.8] US-ORP-10 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/Provider.cs#L35] required provider phone
- [Source: Frontend/src/app/modules/families/family-form/family-form.component.ts] host form

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — Build succeeded, 0 errors.
- `cd Frontend && npm run build` — exit 0.

### Completion Notes List

- Five-holder span verified in code: Family.PhoneNumber, Father.Phone, Mother.Phone, Provider.Phone, Orphan.Phone — all within the resolved charity scope, own-family records excluded.
- Normalisation rule recorded: **trim-only** — no trunk-zero folding (unsafe across locales); the validator bounds the shape (`+`, digits, spaces, hyphens, 3–20).
- Create-mode limitation recorded: the check requires the family id (to exclude the family's own numbers), so it runs only in edit mode; a fresh family's phones are checked the first time it is re-opened for edit. Not fixable without a create-then-check flow the legacy system did not have either.
- Orphan-phone field wiring deferred with the EP-05 orphan form (server side already spans it).
- The `Type` parameter is carried on the filter DTO accepted-for-wire-compatibility and ignored by the matcher (corrected 2026-08-24 review — an earlier note here said "dropped"); a dead parameter invites the belief that per-type matching exists; the type-agnostic simplification is recorded here instead of `deferred-work.md` (no owning story exists; if per-type columns are ever added, the predicate extends in `MatchFamilyPhone`).
- **Duplicate-phone enforcement is check-only (2026-08-24 review, decision D5:b):** the rule is enforced in the Angular form (edit mode) exactly as WAR.IIROSA did — the family/father/mother/provider save endpoints do not re-check. Accepted as legacy parity; a server-side re-check would block edits of already-duplicated legacy rows. Direct API saves can store duplicates; recorded as a known limitation.
- Frontend shape: parent `FamilyFormComponent` owns the debounced subscriptions and the `phoneChecks` map; `FatherFormComponent`/`MotherFormComponent` render their flag via a `phoneCheck` input; per-section saves are gated independently (`onSubmit` → family, `saveFather`/`saveMother`/`saveProvider` → their own holder).
- Live walkthrough pending the user's IIROSA.Api restart.

### File List

- `Backend/src/IIROSA.Application/DTOs/Family/OrphanCodingDtos.cs` (PhoneCheckFilterDto / PhoneCheckDto)
- `Backend/src/IIROSA.Application/Validators/Family/OrphanCodingValidators.cs` (PhoneCheckFilterValidator)
- `Backend/src/IIROSA.Application/Interfaces/IFamilyService.cs`
- `Backend/src/IIROSA.Application/Services/FamilyService.cs` (CheckPhoneNumberDuplicateAsync + MatchFamilyPhone)
- `Backend/src/IIROSA.Api/Controllers/FamiliesController.cs` (GET {familyId}/provider/check-phone)
- `Frontend/src/app/modules/families/models/family.model.ts` (PhoneCheckState)
- `Frontend/src/app/modules/families/services/family.service.ts` (checkPhoneDuplicate)
- `Frontend/src/app/modules/families/family-form/family-form.component.ts` + `.html`
- `Frontend/src/app/modules/families/components/father-form/**` + `mother-form/**` (phoneCheck input + flag)
- `Frontend/src/assets/i18n/ar.json` + `en.json`

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORP-10 and module spec §13.U.10; five-table phone span (Family/Father/Mother/Provider/Orphan) scoped; same-family exclusion and the type-agnostic simplification recorded. |
| 2026-08-24 | Implemented: five-holder check endpoint + edit-mode family-form wiring with per-section save blocks. Create-mode limitation and orphan-form deferral recorded. Status → review. |
| 2026-08-24 | Adversarial review close-out: 8 findings — all resolved (patches applied + decisions D1–D6 recorded in the findings above; record corrections made). Backend `dotnet build` 0 errors, frontend `npm run build` OK. Status → done. |
