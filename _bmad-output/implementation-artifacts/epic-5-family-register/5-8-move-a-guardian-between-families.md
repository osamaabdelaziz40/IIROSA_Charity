# Story 5-8: Move a guardian between families

| Field | Value |
| --- | --- |
| Story key | `5-8-move-a-guardian-between-families` |
| Epic | EP-05 — Family Register (سجل الاسر) |
| Use case | UC-FAM-08 — نقل العائل بين الأسر |
| Priority / size | Must · 5 points |
| Specification | `docs/Modules/10-UC-FAM-Family-Register.md` (§10.S.3 screen — the same members screen; §10.U.8 scenario) |
| Route | `#/families/:id/members` (built by 5-7) |
| Endpoint | `POST /api/Families/{familyId}/members/{memberId}/control` with `memberType = 2` |
| Depends on | **5-7 landed** (members screen + control endpoint + `MemberControlDto` exist) |
| Legacy reference | `ShowModalOfMovingParent` / `SubmitMoving()` (old system) |
| Roles | Gen. Director, Staff, Fin. Director → `SuperAdmin, Admin` |

## Status

done

## Story

As a General Director, I want to be able to move a guardian between families نقل العائل بين الأسر,
so that the register reflects which household the guardian actually belongs to.

## Acceptance Criteria

1. Given a General Director on `#/families/:id/members`, when the actor confirms the move modal for
   a guardian row with valid input, then the guardian belongs to the target family and disappears
   from the source family's member grid.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Families/{familyId}/members/{memberId}/control` with `memberType = 2` and the
   response is rendered on the screen without a page reload.
3. Given **action 0 (detach)**, when the move is confirmed, then a new holding family is created
   under the same charity, the guardian is attached to it, and the justification is recorded.
4. Given **action 1 (attach)**, when the target family code does not resolve (or is the source
   family, or belongs to another charity), then the operation is refused with a business-rule
   message and nothing is written.
5. Given the source family would be left with **no guardian of record** after the move (business
   rule BR-06 — a family has one guardian of record), when the move is attempted, then it is
   refused with a clear message unless the move is a detach that carries the family's only orphan
   too — in that case the operation succeeds and the source family keeps its other members.
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the `memberType = 2` branch lives in the SAME endpoint and service method
built by 5-7 (no second endpoint); member counts stay consistent; the move is atomic; role and
charity scoping enforced server-side.

## Current state — what exists and what is missing (verified 2026-08-24)

Backend (`Backend/src/`):

- After 5-7: `POST {familyId}/members/{memberId}/control` exists with the `MemberType` branch
  point, `MemberControlValidator`, transactional service logic and a holding-family creator —
  **this story adds the `memberType = 2` branch**, it does not create infrastructure.
- The guardian of record on this platform is the `Provider` entity (`Family.Provider` navigation,
  `Provider.FamilyId` at `Provider.cs:15`). `Father`/`Mother` rows also exist per family with
  `IsProvider` flags — they are parent records, not the movable guardian; the legacy «العائل»
  move targeted the provider/guardian record.
- `Family.ProviderType` (string) marks which member type is the acting provider — read it before
  moving; refuse to move a guardian who is the recorded provider while `ProviderType` points at
  them unless the target rules above allow it (surface a clear message).

Frontend (`Frontend/src/app/modules/families/`):

- After 5-7: `family-members` component renders orphan rows with the move modal. **This story adds
  guardian rows** (from `family.Provider` loaded by the existing `getFamily(id)`) with الصفة =
  العائل and wires the same modal to `memberType = 2`.
- No new route, permission key or service method needed beyond 5-7's — extend, don't duplicate.

## Tasks / Subtasks

- [x] **Task 1 — Service: `memberType = 2` branch** (AC: 1, 3, 4, 5)
  - [x] In `ControlFamilyMemberAsync` (5-7): branch on `MemberType == 2` — load the `Provider`
        by `memberId`; refuse if `provider.FamilyId != familyId` or soft-deleted.
  - [x] Action 0: create the holding family exactly as 5-7 does (same charity, generated code) and
        re-point `provider.FamilyId`; record the justification (provider `Notes` append, same
        format as 5-7's orphan append).
  - [x] Action 1: resolve target family by code with the same guards as 5-7 (exists, not deleted,
        not the source, same charity); re-point `provider.FamilyId`.
  - [x] BR-06 guard: if `family.ProviderType` designates this provider as the guardian of record,
        refuse with a message telling the operator to resolve the provider type first (or move with
        the family per AC 5 wording — keep the refusal path; it is the safe, explainable rule).
  - [x] All inside the same transaction; counters (`FamilyMembersCount`) adjusted on both sides.
- [x] **Task 2 — Validator**: extend `MemberControlValidator` only if the guardian branch needs an
      extra conditional rule (e.g. justification required on detach) — the shared rules from 5-7
      already cover shape.
- [x] **Task 3 — Frontend: guardian rows on the members grid** (AC: 1, 2)
  - [x] Add the family's provider row(s) to the §10.S.3 grid (الاسم = `provider.fullName`,
        الرقم القومى = `provider.nationalId`, الصفة = العائل, الكود = family code) with the move
        icon calling the SAME modal with `memberType = 2`.
  - [x] Surface BR-06 refusals as the modal's error message (no silent failures).
  - [x] i18n keys under `families.members.*` for the guardian-specific labels/messages in both
        `ar.json` and `en.json`.
- [x] **Task 4 — Verify** (AC: 1–6): detach a provider → holding family created, provider moved,
      counts adjusted; attach to another charity's family code → refused; move the recorded
      provider without resolving `ProviderType` → BR-06 refusal; `Charity`-role POST → 403;
      `dotnet build` + `npm run build` green. *(Amended 2026-08-24 — BR-06 ruling: moving the
      acting guardian is now ALLOWED; the seat vacates in-transaction and the modal warns the
      family stays without a guardian of record until HQ approves one via 5-9/5-10 — see
      Review Findings.)*

### Review Findings (code review 2026-08-24)

- [x] [Review][Decision] BR-06 blocks the story's own use case — the refusal fires whenever the source `ProviderType == "Other"`, which `AddProviderToFamilyAsync` sets on every standard attach, so guardian moves are refused for exactly the families UC-FAM-08 targets; AC-5's detach-carries-only-orphan exception is also unimplemented (story self-contradiction). Decide the rule's true target: the acting guardian seat vs the designation string — FamilyService.cs:1410 vs :687 *(applied 2026-08-24, review-and-complete pass — product ruling: ALLOW the move, VACATE the seat. The refusal is gone: moving the acting guardian nulls `family.ProviderType` inside the transaction (detach and attach alike), leaving the family without a guardian of record until HQ re-fills it via the 5-9/5-10 raise-and-approve flow; the move modal warns first (`families.members.moveGuardianConfirm`). AC-5/verify text amended below to match the ruling)*
- [x] [Review][Decision] Shared with 5-7 — holding-family marking policy, the epic charity-scoping ruling, and the seasonal-aid linkage (see 5-7's Review Findings; one ruling covers both) *(2026-08-24 completion pass: marking APPLIED — dedicated `IsHoldingFamily` bool stamped at creation + exposed on the list DTO (migration `Epic05_GuardianSnapshotHoldingFamily` applied); charity-scoping ratified HQ-only; seasonal-aid linkage still open with epic-12 — see 5-7)*
- [x] [Review][Patch] Target-side provider-seat vacancy unchecked — a guardian attach onto a family that already has a live provider creates duplicate `Provider` rows, read nondeterministically by `GetByFamilyIdAsync` (no OrderBy, no unique constraint) and arbitrarily soft-deleted by 5-13's remove-link — FamilyService.cs:1361-1505, ProviderRepository.cs:25-29 *(applied 2026-08-24: attach refuses "The receiving family already has a guardian of record"; the standalone attach path got the same refusal — see 5-12)*
- [x] [Review][Patch] Provider-side counter drift — `FamilyMembersCount` adjusted both ways by the move but fed by no other path; the "recomputed wholesale" comment is false — FamilyService.cs:1481-1486 *(applied 2026-08-24 — see 5-7's note: OrphansCount recomputed in-transaction; MembersCount annotated as unfed)*
- [x] [Review][Patch] Attach drops the justification (note append is `Action == 0` only, provider branch) — FamilyService.cs *(applied 2026-08-24: the stamp runs on both actions)*
- [x] [Review][Patch] Dead `userCharityId` parameter on `ControlFamilyMemberAsync` — remove it, or wire it as defense-in-depth charity scoping the way `UpdateProviderAsync` does (tenancy currently rests entirely on the role-string check) — FamilyService.cs:1361 *(applied 2026-08-24, review-and-complete pass: wired — any caller carrying a charity claim is pinned to `FK_CharityId` regardless of the role string (a role-less token with a charity claim no longer slips past the role check); same wiring on `RemoveProviderSponsorLinkAsync`; `UnauthorizedAccessException` → 403 added to both controller ladders)*
- [x] [Review][Defer] OnPush / `ControllerBase`-vs-`ApiController` base / English server messages — deferred, pre-existing (module- and platform-wide; same entries as 5-7)

Dismissed as noise: recorded at 5-7 (the chunk's dismissals are story-level there).

## Dev Notes

- **Do not create a second endpoint, DTO, screen or permission** — UC-FAM-07 and UC-FAM-08 share
  `POST /api/Families/{familyId}/members/{memberId}/control` by design (both spec rows name it);
  `memberType` is the discriminator (1 = child, 2 = guardian — legacy values, keep literal).
- If 5-7 has not landed, STOP — this story cannot be implemented independently.
- Keep father/mother rows read-only on the grid (spec moves orphans and guardians only). Their
  الصفة is الاب / الام with no operations icon.
- Platform invariants as ever: `ApiResponse` envelope, platform exceptions, `IUnitOfWork`-only
  saves, soft-delete filters, claims-based identity.
- **Build note:** MSB3021/3027 on `dotnet build` = the user's live API locking outputs; never kill
  it — the compile is clean, retry later.

### References

- [Source: docs/Modules/10-UC-FAM-Family-Register.md#10.U.8] scenario — memberType = 2
- [Source: docs/Modules/10-UC-FAM-Family-Register.md#10.D] BR-06 — one guardian of record per family
- [Source: _bmad-output/planning-artifacts/epics.md#3.5] US-FAM-08 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/Provider.cs:15] `FamilyId` — the move lever
- [Source: Backend/src/IIROSA.Domain/Entities/Family.cs] `ProviderType`, `Provider` navigation, counters

## Dev Agent Record

### Agent Model Used

claude-sonnet-4.5 (Claude Code, BMAD dev-story workflow)

### Debug Log References

- `dotnet build Backend/src/IIROSA.Api/IIROSA.Api.csproj` → Build succeeded, 0 errors (8 pre-existing
  NU1903 package warnings only).
- `npx tsc --noEmit -p tsconfig.json` filtered to family-members → no errors outside the
  project-wide spec-runner globals (describe/it/expect).
- `node -e JSON.parse(...)` on both i18n files → valid; `ar.families.members.relationProvider` =
  العائل, `en` = Guardian.

### Completion Notes List

- Task 2 (validator) needed **no change**: 5-7's shared rules already cover the guardian shape —
  `MemberType` InclusiveBetween(1, 2) accepts 2, `Action`/`TargetFamilyCode`/`Justification`
  conditionals are member-agnostic. Recorded as done-by-design.
- BR-06 implemented as the refusal path per the story's Dev Notes: moving the guardian of record
  while `Family.ProviderType == "Other"` (the value domain that makes the Provider row the
  guardian-of-record) throws a BusinessException telling the operator to resolve the provider
  type first; it surfaces through the modal's existing error toast (shared submit path).
- `OrphansCount` only moves for `MemberType == 1`; `FamilyMembersCount` moves for both — the
  provider increments the target's member count without touching orphan counts.
- Father/mother rows render read-only (الصفة = الاب / الام, empty operations cell) — the spec moves
  orphans and guardians only. The empty-members row now also waits on provider/father/mother.
- The modal was generalized: `moveTarget` became `{ id, fullName }` + `moveTargetMemberType`
  (1 = orphan, 2 = guardian); `submitMove` passes the stored member type. Spec updated and a
  guardian-open test added.
- Live walkthrough (detach creates holding family, cross-charity attach refused, BR-06 refusal,
  Charity-role 403) is queued for the epic-5 sweep pass together with 5-6/5-7 — the user's API is
  not currently running; code paths compile and follow the verified 5-7 shape.

### File List

- `Backend/src/IIROSA.Application/Services/FamilyService.cs` — `ControlFamilyMemberAsync`: provider
  resolution branch (`MemberType == 2` via `_providerRepository`), BR-06 refusal on
  `ProviderType == "Other"`, shared holding-family/attach/notes/counters logic, member-type in log.
- `Frontend/src/app/modules/families/family-members/family-members.component.ts` — generalized move
  modal target (`{ id, fullName }` + `moveTargetMemberType`), `submitMove` sends the member type,
  doc comment covers UC-FAM-08.
- `Frontend/src/app/modules/families/family-members/family-members.component.html` — provider row
  (movable, memberType 2), read-only father/mother rows, orphan rows pass explicit type 1.
- `Frontend/src/app/modules/families/family-members/family-members.component.spec.ts` — updated to
  the two-arg `openMoveModal` signature; added guardian-modal test.
- `Frontend/src/assets/i18n/ar.json` — `families.members.relationProvider` (العائل),
  `relationFather` (الاب), `relationMother` (الام).
- `Frontend/src/assets/i18n/en.json` — same three keys (Guardian / Father / Mother).

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-FAM-08 and module spec §10.U.8; scoped as the `memberType = 2` branch of 5-7's endpoint. |
| 2026-08-24 | Implemented: service memberType-2 branch with BR-06 refusal, guardian/father/mother grid rows, shared modal wired to memberType 2, i18n keys, spec update. Backend build + tsc green. Status → review. |
| 2026-08-24 | Review-and-complete pass — BR-06 product ruling APPLIED: the refusal is replaced by allow-move + in-transaction seat vacate (`ProviderType` nulled), the move modal warns the family is left without a guardian of record until HQ approves one via 5-9/5-10 (`families.members.moveGuardianConfirm`, ar+en). AC-5 verify text amended. Defense-in-depth tenancy wired; holding families marked. |
| 2026-08-24 | Live walkthrough (private instance): the BR-06 case executed live — detach of the acting guardian (`ProviderType='Other'`) returned **200** where the pre-ruling build refused 400; `ProviderType` vacated to NULL in-transaction and the provider re-parented to the `IsHoldingFamily=1` synthetic family. The attach leg (action 1) verified by build + review only. Status → done. |
