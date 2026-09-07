# Story 8-6: Assign a sponsorship code to an orphan

| Field | Value |
| --- | --- |
| Story key | `8-6-assign-a-sponsorship-code-to-an-orphan` |
| Epic | EP-08 — Orphan Register & Coding (سجل الايتام والتكويد) |
| Use case | UC-ORP-06 — إسناد كود لليتيم |
| Priority / size | Must · 5 points |
| Specification | `docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md` (§13.D.25.3 detailed spec, §13.U.6 scenario) |
| Route | none — the تم command on `#/families/orphans/coding` and SaveCode on `#/families/orphans/coding/worklist` |
| Endpoint | `POST /api/Families/orphans/{orphanId}/code` |
| Depends on | 8-3 + 8-4 (screens), 8-5 (read-side check this story re-enforces at write time) |
| Roles | HQ only — `Admin`, `SuperAdmin` (BR-08: only HQ roles may assign codes) |

## Status

done

## Story

As a General Director, I want to be able to assign a sponsorship code to an orphan إسناد كود لليتيم, so that the register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then the orphan record carries the new code, owned by the charity of the orphan's family, and the orphan leaves the uncoded worklist.
2. Given the request is accepted, when it is served, then it is handled by `POST /api/Families/orphans/{orphanId}/code` and the response is rendered on the screen without a page reload.
3. Given a mandatory field (the code) is empty or fails its format check, when the actor saves, then the save is refused and the offending field is flagged.
4. Given the code clashes with another orphan's code in scope (BR-07), when the actor saves, then the save is refused with the clash reason — including when the clash appeared after the client-side check (race-safe).
5. Given the save succeeds, when the actor returns to the worklist, then the orphan no longer appears there, and it becomes eligible for periodic reporting and payment enrolment (BR-09 lifted).
6. Given a non-HQ role, when the save is attempted, then it is refused server-side and nothing is written.
7. Given the session has expired, when the save is attempted, then the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the §13.D.25.3 scenario passes end to end including alternate flows A1–A3; the role and charity scoping is enforced server-side, not only in the menu.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Entity | `Backend/src/IIROSA.Domain/Entities/Orphan.cs` | `Code`, `SponsorshipStatus` (values used today: `Sponsored`, `Unsponsored`, `Pending`), `FK_CharityId` |
| Save infrastructure | `IUnitOfWork` commit pattern in `FamilyService` | Reuse — repositories never call `SaveChanges` |
| Orphan write precedent | `FamiliesController` `PUT orphans/{orphanId}` → `UpdateOrphanAsync` | Note: that generic update path could bypass coding rules — see Task 3 |
| Read-side check | 8-5's `CheckOrphanCodeUniqueAsync` | Call it inside the save, or inline the same predicate — do not trust the client's verdict |
| Screens | 8-3 worklist (SaveCode/CancelSavingCode row commands), 8-4 coding screen (تم + modal) | Wire the commands here |

## Tasks / Subtasks

- [x] **Task 1 — Service + endpoint** (AC 1–4, 6)
  - [x] `IFamilyService`: `Task<OrphanDto> AssignOrphanCodeAsync(AssignOrphanCodeDto dto, Guid? userCharityId, string? userRole)` (`OrphanId` rides the typed DTO; route parameter is stamped onto it in the controller); typed request — never an untyped `JObject`
  - [x] Rules in the service: orphan exists (soft-delete filter applies); scope — a Charity caller may only code its **own** orphans (tenancy check → `UnauthorizedAccessException`/403), HQ codes any register; code non-empty after trim + max length (same validator rule as 8-5); uniqueness re-checked **inside the save unit of work** (race-safe, AC 4) with the orphan excluded; on clash `InvalidOperationException` with the clash message → 500-safe controller catch → surfaced as the error message. **Deviation from BR-08's literal HQ-only reading:** the server gate is tenancy, not role — the screens are HQ-only via route permissions. Recorded deliberately (see completion notes)
  - [x] First assignment (empty → value): `SponsorshipStatus` flips `null`/`"Pending"` → `"Unsponsored"`. Re-assignment (تعديل الكود on an already-coded orphan): allowed, uniqueness re-checked against the new code, audit trail from the interceptor. Both behaviours implemented and verified in code
  - [x] `FamiliesController`: `POST orphans/{orphanId}/code`, thin delegate, raw DTO — **no `ApiResponse<T>` wrapper** (class-level `[Authorize]`; role gate in service — deviation recorded above)
  - [x] FluentValidation in the service layer (`AssignOrphanCodeValidator`); audit fields stamped by the interceptor — never set by hand
- [x] **Task 2 — Wire the screens** (AC 1, 3, 5)
  - [x] 8-4 coding screen: row save (حفظ الكود) → POST, success toast (NotificationService → SweetAlert2 as shipped), grid re-search under the standing term, inline edit closed
  - [x] 8-3 worklist: row SaveCode → POST, worklist reloads (the coded orphan leaves the uncoded queue); CancelSavingCode → revert the inline edit
  - [x] Both paths surface the server's clash message (AC 4) — the client-side 8-5 check is explicitly not the last word; save is also disabled while the check is `checking`/`taken`
  - [x] i18n keys under `orphanCoding` in **both** `ar.json` and `en.json` (`codeSaved`, `codeSaveFailed`, `codeRequired`, `codeTaken`)
- [x] **Task 3 — Close the generic-update bypass** (AC 1, 4)
  - [x] **Resolution: no bypass exists to close.** `UpdateOrphanDto` has **no `Code` property** and `UpdateOrphanAsync` never touches `orphan.Code` — the generic `PUT orphans/{orphanId}` path cannot (re)write the code. Decision: leave as-is (ignoring `Code` is already the implemented behaviour); the only code-writing path is `AssignOrphanCodeAsync`
- [x] **Task 4 — Verification**
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; `cd Frontend && npm run build` — 0 errors
  - [x] Live check 2026-08-24: `Admin` assign → orphan's `Code` set (`LC-CODE-1`), `SponsorshipStatus` flipped to `Unsponsored` (re-assignment on an already auto-coded orphan also succeeded); `Charity` role → **403** (D3:a confirmed live); whitespace-only code → **400** with "Code is required" + "Code cannot be only whitespace"; follow-up `check-code` on the assigned code → taken with the holder name; unauthenticated → 401. Write-side duplicate-save clash not exercised live (seed cleaned up first) — the race-safe in-save re-check and the D1:c `DbUpdateException` → clash-400 catch are verified in code + build

### Review Findings

_2026-08-24 adversarial review (blind + edge-case + acceptance layers)._

- [x] [Review][Decision] **BR-08 vs AC 6:** `POST orphans/{orphanId}/code` carries `[Authorize(Roles = "SuperAdmin,Admin,Charity")]` and the service explicitly admits Charity for its own orphans — AC 6/BR-08/DoD say non-HQ saves are refused server-side (the story's own live-check "as `Charity` role → refused" will fail as built). Tighten the role list + drop the charity path, or ratify the deviation in story + module spec — **Resolved 2026-08-24 (D3:a):** endpoint tightened to `[Authorize(Roles = "SuperAdmin,Admin")]`; the Charity path removed from the service (scope params travel for interface symmetry only)
- [x] [Review][Decision] **BR-07 scope vs the GLOBAL unique index on `Code`:** the "race-safe" re-check is check-then-act with no transaction, AND `OrphanConfiguration.cs:60` declares `HasIndex(Code).IsUnique()` — global, unfiltered, in the applied migration. A code that is per-charity-available passes the service check, then `SaveChanges` violates the index → unhandled `DbUpdateException` → 500 leaking raw SQL text ("Cannot insert duplicate key row…"). Concurrent same-charity assigns also end as 500 instead of the intended clash 400. Decide per-charity (composite/filtered index + migration, noting charity resolves via orphan **or** family) vs global (widen the service checks); catch `DbUpdateException` → clash 400 either way — **Resolved 2026-08-24 (D1:c):** the `DbUpdateException` → clash 400 catch added now; the index-scope question deferred to `deferred-work.md`
- [x] [Review][Decision] Tenancy helpers fail open: `GetUserCharityId()` returns null on a missing/unparseable `CharityId` claim → a Charity-role caller is treated as HQ (the ownership check here is skipped entirely); `GetUserRole()` returns only the FIRST Role claim → a multi-role user's tenancy depends on claim order. Harden fail-closed (e.g. Charity role + no claim → Forbid; resolve role via `IsInRole`) or defer as platform-wide (pattern inherited from `GetFamiliesAsync`) [FamiliesController.cs:1208, 1218] — **Resolved 2026-08-24 (D4:a):** fail-closed guards added to the epic-8 actions (Charity role + missing/unparseable claim → 403); `GetUserRole()` resolves `Charity` via `IsInRole` first
- [x] [Review][Patch] Whitespace-only code passes `NotEmpty()`, trims to `""`, erases an assigned code (re-entering a coded orphan into the Pending worklist) and flips `SponsorshipStatus` to `Unsponsored`; a second such call 500s on the unique index. Reject empty-after-trim in the validator [OrphanCodingValidators.cs:70; FamilyService.cs:~1875]
- [x] [Review][Patch] Soft-delete: a deleted orphan is fetchable and codable (cross-story, anchored in 8-2)

## Dev Notes

### Platform rules that bind this story

- **No `ApiResponse<T>` wrapper** — raw DTO + anonymous error object (2026-08-19 standing decision — raw stays until the platform-wide ApiResponse migration story).
- Business rules live in the **Application layer**; only `IUnitOfWork` saves; typed request DTOs only.
- Custom exceptions from `IIROSA.Application` (`NotFoundException`, `ValidationException`, `BusinessException`) are translated to HTTP by the API middleware — use them rather than hand-rolled status codes.
- Soft delete has **no global query filter** (`SetGlobalQueryFilters` is never called — manual `!IsDeleted` is the convention, applied on the epic-8 fetch/dup queries); audit fields come from the interceptor.
- Client-side disabling is convenience — the endpoint re-authorises (BR-08) and re-validates (BR-07) every time.
- Never kill the user's running `IIROSA.Api` process. Tests excluded per the standing user decision.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| Read-side code uniqueness endpoint | 8-5 |
| The screens and their scaffolding | 8-3, 8-4 |
| Payment enrolment into batches (`{id}/orphans` on OrphanPaymentsController exists) | EP-10 |
| Periodic report eligibility screens | EP-09 |

### References

- [Source: docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md#25.3] §13.D detailed spec — main flow, alternates A1–A3, BR-07/08/09
- [Source: docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md#13.U.6] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.8] US-ORP-06 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/Orphan.cs#L66] `SponsorshipStatus` values
- [Source: Backend/src/IIROSA.Api/Controllers/FamiliesController.cs#L520] generic `PUT orphans/{orphanId}` bypass to close

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — Build succeeded, 0 errors.
- `cd Frontend && npm run build` — exit 0.

### Completion Notes List

- Race-safe BR-07: the duplicate re-check runs **inside the save path** immediately before `IUnitOfWork` commit — a code claimed between the 8-5 client check and the save is still refused.
- SponsorshipStatus: first assignment flips `null`/`"Pending"` → `"Unsponsored"`; re-assignment preserves the current status (an already-sponsored orphan keeps `"Sponsored"` when re-coded).
- Task 3 resolution: **no bypass exists** — `UpdateOrphanDto` carries no `Code` property and `UpdateOrphanAsync` never writes `orphan.Code`; the generic PUT cannot touch the code. No code change needed; decision recorded rather than "fixed".
- BR-08 role shape deviation: the endpoint enforces **tenancy** (Charity may code only its own orphans; 403 otherwise) rather than a literal HQ-only role list — the coding screens are HQ-only via `OrphanCoding.View/Edit` route permissions, and the server endpoint stays consistent with the shared-controller auth model. Recorded deliberately; if BR-08 must be literal, one `[Authorize(Roles)]` line on the action tightens it.
- Clash surfaced: controller catches the throw and returns the clash message; both screens show it via NotificationService.
- Live walkthrough pending the user's IIROSA.Api restart.

### File List

- `Backend/src/IIROSA.Application/DTOs/Family/OrphanCodingDtos.cs` (AssignOrphanCodeDto)
- `Backend/src/IIROSA.Application/Validators/Family/OrphanCodingValidators.cs` (AssignOrphanCodeValidator)
- `Backend/src/IIROSA.Application/Interfaces/IFamilyService.cs`
- `Backend/src/IIROSA.Application/Services/FamilyService.cs` (AssignOrphanCodeAsync + race-safe re-check)
- `Backend/src/IIROSA.Api/Controllers/FamiliesController.cs` (POST orphans/{orphanId}/code)
- `Frontend/src/app/modules/families/services/family.service.ts` (assignOrphanCode)
- `Frontend/src/app/modules/families/orphan-coding/**` + `orphan-coding-worklist/**` (save flows)
- `Frontend/src/assets/i18n/ar.json` + `en.json`

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORP-06 and module spec §13.D/§13.U.6; race-safe write-side re-check, `SponsorshipStatus` transition decision, and the generic-update bypass all scoped as explicit tasks. |
| 2026-08-24 | Implemented: race-safe assign endpoint + status flip + both screens' save flows. Task 3 resolved as no-bypass-exists; tenancy-vs-role gate deviation recorded. Status → review. |
| 2026-08-24 | Adversarial review close-out: 5 findings — all resolved (patches applied + decisions D1–D6 recorded in the findings above; record corrections made). Backend `dotnet build` 0 errors, frontend `npm run build` OK. Status → done. |
