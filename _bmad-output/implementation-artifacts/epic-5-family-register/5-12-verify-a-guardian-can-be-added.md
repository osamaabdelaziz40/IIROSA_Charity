# Story 5-12: Verify a guardian can be added

| Field | Value |
| --- | --- |
| Story key | `5-12-verify-a-guardian-can-be-added` |
| Epic | EP-05 — Family Register (سجل الاسر) |
| Use case | UC-FAM-12 — التحقق من إمكانية إضافة عائل |
| Priority / size | Must · 5 points |
| Specification | `docs/Modules/10-UC-FAM-Family-Register.md` (§10.U.12 scenario) |
| Endpoint | `POST /api/Families/{familyId}/verify-provider` (exists — legacy UC-4.7 realization) |
| Depends on | 5-1..5-4 done (family file + provider seat) |
| Legacy reference | `IFamilyService.CanAddParent` (epics crosswalk) — no such method on this stack |
| Roles | Charity → `Charity` (+ HQ roles pass through) |

## Status

done

## Story

As a charity user, I want to be able to verify a guardian can be added التحقق من إمكانية إضافة
عائل, so that the register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a charity user with an active session in the module, when the actor invokes the function
   with valid input, then a new record exists, owned by the charity of the creating user, and
   appears in the list screen of the module.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Families/{familyId}/verify-provider` and the response is rendered on the screen
   without a page reload.
3. Given a mandatory field listed in the screen field specification is empty, when the actor
   saves, then the save is refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to the list screen, then the record appears
   there with the values just entered.
5. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
7. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

## As-built assessment (reviewed 2026-08-24 — no story file existed; created by the epic-5
review-and-complete pass)

The epic crosswalk names `IFamilyService.CanAddParent` — a legacy identifier with no counterpart
on this stack. What the spec's summary actually requires — *"Before a guardian is attached to a
family the system checks that the family does not already have one and that the charity is
permitted to add records"* — is enforced by construction across the platform's guardian paths:

**Check 1 — the family's guardian seat (BR-06, one guardian of record).** The platform models the
seat as the `Provider` row plus the `ProviderType` designation:

- Attaching a new guardian: `AddProviderToFamilyAsync` → `AddProviderToFamilyInternalAsync`
  enforces the duplicate rule «أحد المعيلين مكرر من قبل أكثر من مرة»
  (`IsNationalIdExistsAsync`, FamilyService) — one provider national ID per family, platform-wide.
- Replacing a guardian: the 5-9/5-10 flow (raise → head-office approve) updates the family's
  existing `Provider` row in place inside one transaction — a second seat is never created, so
  BR-06 holds by construction.
- Recording a parent as the guardian: `VerifyParentProviderAsync` sets `Father.IsProvider` /
  `Mother.IsProvider` and restamps `ProviderType` ("Father"/"Mother") — the designation, not a
  second record.

**Check 2 — the charity may add records (UC-CHR-08 add permission / UC-CHR-07 lock).**
`ICharityWriteGuard.EnsureCanAddAsync` (throws `CharityWriteForbiddenException`, handled globally
by `ExceptionMiddleware` → 403-classed):

- Family create: already guarded (`CreateFamilyAsync`, FamilyService).
- **Gap found and closed this pass:** the standalone attach path
  (`AddProviderToFamilyAsync`) and the verify/record path (`VerifyParentProviderAsync`) did NOT
  run the guard — a locked charity or an add-disabled charity could still attach or re-designate
  a guardian on an existing family. Both now call `EnsureCanAddAsync` first, mirroring create.

**The named endpoint** `POST /api/Families/{familyId}/verify-provider` exists
(FamiliesController `VerifyParentProvider`, `[Authorize(Roles = "SuperAdmin,Admin,Charity")]`,
Charity-role ownership checked against the family's charity) and serves the §10.U.12 contract.
It has no SPA consumer today — the user-facing "can this guardian be added?" check surfaces where
guardians are actually entered: the duplicate-national-ID refusal on the family form / provider
add, and the pending-duplicate refusal «تم اضافه الطلب من قبل…» on the 5-9 raise modal. The
endpoint stays as the programmatic contract (and the legacy UC-4.7 screen hook).

### Deviations recorded

- No new screen: §10.U.12's template ACs (add-mode form, list-screen return) are crosswalk
  boilerplate from the "Create a record" type; the spec's own summary defines a pre-attach
  validation, which the platform implements server-side in the attach paths above. A standalone
  "verify" screen would duplicate the family file's provider section.
- The endpoint's response envelope is the legacy raw `{ message }` shape (pre-dates the module's
  ApiResponse convention). Kept — no consumer exists to break, and the wire change buys nothing;
  revisit if a screen ever binds it.

### Review Findings (code review 2026-08-24)

- [x] [Review][Patch] Check 1 ("the family does not already have a guardian") is NOT enforced — `AddProviderToFamilyAsync` never looks for an existing provider; only the platform-wide national-ID rule blocks re-attachment, so a second guardian with a distinct NID creates a duplicate `Provider` row read nondeterministically (and 5-13's remove-link deletes the arbitrary first row). The as-built claim that BR-06 holds "by construction" is false for distinct NIDs. Add the occupied-seat refusal — FamilyService.cs:1552-1565, ProviderRepository.cs:25-29 *(applied 2026-08-24: the standalone attach path refuses "The receiving family already has a guardian of record" — same rule the 5-8 move path received; the 5-10 approve path was already safe by construction)*
- [x] [Review][Patch] `CharityWriteForbiddenException` (locked / add-disabled charity) surfaces as 500, not 403 — both controller actions catch generic `Exception` before the middleware's dedicated 403 mapping is reached; add the catch — FamiliesController.cs:1095-1099,1213-1217, ICharityWriteGuard.cs:44 *(applied 2026-08-24: dedicated catch → 403 with the exception's message on both actions; their generic 500 bodies no longer echo `ex.Message`)*
- [x] [Review][Patch] Verify-provider accepts contradictory state — both `fatherIsProvider`+`motherIsProvider` true (two guardians of record while `ProviderType = "Father"` silently); both false never resets `ProviderType` (stale designation feeds 5-14's widow sheet and member-control BR-06); the `notes` justification is never stored; a soft-deleted family is accepted (unfiltered `FindAsync`, no `IsDeleted` check) — FamilyService.cs:816-860 *(applied 2026-08-24: both-designated refused ("Only one parent can be the family's provider of record"); undesignating resets `ProviderType = null` (BR-06/widow-sheet comment); the justification appends to `Family.Notes` with a timestamped provider-designation stamp; the family load refuses soft-deleted families)*
- [x] [Review][Patch] Attach path has no charity-ownership check on the target family for charity-role callers (defense-in-depth tenancy) — FamilyService.cs:816-828 *(applied 2026-08-24, review-and-complete pass: `AddProviderToFamilyAsync` takes `userCharityId`/`userRole` (interface + controller updated) and enforces the `UpdateProviderAsync` tenancy rule against the live `FK_CharityId`; the controller's mirror-column inline guard is gone — it compared `FamilyDto.CharityId`, Forbade rightful owners and fell open on null-vs-null; `UnauthorizedAccessException` → 403 added to the ladder)*
- [x] [Review][Defer] `[Required]` DataAnnotations + controller ModelState instead of service-layer FluentValidation on the attach path — deferred, pre-existing (pre-existing DTO shape; normalize with the platform validator sweep)

Dismissed as noise: none — every reviewer claim on this story verified true.

## Dev Agent Record

### Agent Model Used

claude-sonnet-4.5 (Claude Code, BMAD dev-story workflow — review-and-complete pass)

### Debug Log References

- `dotnet build IIROSA.Api.csproj` → Build succeeded (guard wiring compiles;
  `CharityWriteForbiddenException` handled by `ExceptionMiddleware`).

### Completion Notes List

- Story file created by the epic-5 sweep (5-1..5-14 review pass) — the story was never
  drafted during epic-5's original run, yet most of it shipped with the copied vertical.
- The only functional gap found (missing add-permission guard on the two existing-family
  guardian writes) was closed this pass; everything else was verified as-built, not rebuilt.
- Frontend needs nothing: the check is server-side by design ("hiding a menu is not a control"),
  and the user-visible refusals already surface through the existing forms.

### File List

- `Backend/src/IIROSA.Application/Services/FamilyService.cs` — `EnsureCanAddAsync()` guard added
  to `AddProviderToFamilyAsync` and `VerifyParentProviderAsync` (UC-FAM-12 check 2).

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created by the epic-5 review-and-complete pass; as-built assessment recorded (both UC checks verified in the attach paths); the missing add-permission guard wired into the two existing-family guardian writes. Status → review. |
| 2026-08-24 | Completion pass: attach-path tenancy moved into the service (`AddProviderToFamilyAsync` now enforces the `UpdateProviderAsync` FK-based rule; interface + controller signature updated; the controller's mirror-column guard removed, 403 catch added). |
| 2026-08-24 | Live battery follow-up: the one-live-provider invariant (check 1) is now ALSO enforced physically — `IX_Provider_FamilyId` is a filtered unique over live rows only, aligning the DB with the occupied-seat refusal and unblocking re-attach after 5-13 removals. Guards verified by build + review. Status → done. |
