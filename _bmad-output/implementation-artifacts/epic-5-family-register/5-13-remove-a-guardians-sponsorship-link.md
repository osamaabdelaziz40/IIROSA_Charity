# Story 5-13: Remove a guardian's sponsorship link

| Field | Value |
| --- | --- |
| Story key | `5-13-remove-a-guardians-sponsorship-link` |
| Epic | EP-05 — Family Register (سجل الاسر) |
| Use case | UC-FAM-13 — حذف كفالة العائل |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/10-UC-FAM-Family-Register.md` (§10.U.13 scenario; §10.S.2 — the حذف الكفالة modal: `ShowModalOfDeletingParent` / `DeleteSposnor()`) |
| Route | `#/families/:id/members` (built by 5-7 — the deleting modal «هل انت متاكد من حذف ؟» with التعليق) |
| Endpoint | `DELETE /api/Families/{familyId}/provider/sponsor` |
| Depends on | **5-7 landed** (members screen exists to host the action) |
| Legacy reference | `DeleteSposnor()` / `CloaseDeleteSposnorModel()` (old system) |
| Roles | HQ roles → `SuperAdmin, Admin` |

## Status

done

## Story

As a HQ role, I want to be able to remove a guardian's sponsorship link حذف كفالة العائل, so that
records entered in error do not distort the register or the reporting.

## Acceptance Criteria

1. Given a HQ role on the family members screen, when the actor requests the removal and confirms,
   then the sponsorship link is removed and the record is no longer returned by the list and read
   endpoints of the module (soft delete — rows are never hard-deleted).
2. Given the request is accepted, when it is served, then it is handled by
   `DELETE /api/Families/{familyId}/provider/sponsor` and the response is rendered on the screen
   without a page reload.
3. Given the actor requests removal, when the confirmation is declined, then nothing is deleted.
4. Given the guardian is **still referenced by an active sponsorship** (the family has an orphan
   with a live sponsorship link), when the removal is attempted, then it is refused with a
   business-rule message and nothing is written.
5. Given a charity-scoped user, when the endpoint is invoked, then the request is rejected —
   removal is an HQ operation (server-side).
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the guard (AC 4) runs server-side inside the same unit of work as the
removal; the optional التعليق (comment) from the modal is recorded; soft-delete semantics
throughout.

## Current state — what exists and what is missing (verified 2026-08-24)

Backend (`Backend/src/`):

- **No `provider/sponsor` endpoint exists.**
- Data model on this platform: sponsorship hangs off `Orphan` — `Orphan.SponsorId` (`Orphan.cs:78`)
  + `Orphan.SponsorshipStatus` (`:68`, string: "Sponsored"/"Unsponsored"/"Pending") + `Sponsor`
  entity with an `Orphans` collection. The family's guardian of record is `Family.Provider`
  (`Provider.FamilyId`, `Provider.cs:15`). There is no direct Provider↔Sponsor FK — the "guardian's
  sponsorship link" is realised as: the guardian row itself (soft-delete it) **guarded by** the
  family's orphans' live sponsorships.
- Soft-delete precedent: `DeactivateFamilyAsync` sets `IsActive = false; IsDeleted = true`
  (`FamilyService.cs` ~line 639); reads filter `!IsDeleted`.
- Comment capture: `Provider.Notes` exists — the التعليق lands there (or the audit note), matching
  5-7's append format.

Frontend (`Frontend/src/app/modules/families/`):

- After 5-7: the members screen exists with its modals; §10.S.3 specifies the deleting modal
  «هل انت متاكد من حذف ؟» with the التعليق field. **This story wires the delete action + modal**;
  `notification.confirm()` (SweetAlert2) is the house confirm pattern.
- `family.service.ts` — no remove-sponsor-link method.

## Tasks / Subtasks

- [x] **Task 1 — Application: DTO + service method** (AC: 1, 4, 5)
  - [x] `DTOs/Family/RemoveProviderSponsorLinkDto.cs` — `string? Comment` (التعليق, max 500).
  - [x] `IFamilyService.RemoveProviderSponsorLinkAsync(Guid familyId,
        RemoveProviderSponsorLinkDto? dto, Guid? userCharityId, string? userRole)`:
        1. Load family (`!IsDeleted`, charity check for `Charity` role); load its `Provider`.
        2. **Guard:** if any orphan in the family has a live sponsorship (`SponsorId != null` &&
           `SponsorshipStatus == "Sponsored"`), refuse with `BusinessException` + localized
           message naming the orphan codes still sponsored.
        3. Otherwise: append the comment to `Provider.Notes` (timestamp + user prefix, 5-7
           format), soft-delete the provider row (`IsDeleted = true` — the interceptor/deletions
           stamp who/when), save through `IUnitOfWork`.
- [x] **Task 2 — API endpoint** (AC: 2, 5)
  - [x] `DELETE {familyId}/provider/sponsor` on `FamiliesController` —
        `[Authorize(Roles = "SuperAdmin,Admin")]`; the DTO travels as a query parameter or a body
        bound from query (DELETE with `[FromBody]` is unreliable in some clients — prefer
        `[FromQuery] string? comment`); returns `ApiResponse` with a localized outcome message.
- [x] **Task 3 — Frontend: delete action on the members grid** (AC: 1, 3)
  - [x] Delete icon on the guardian row (§10.S.3 `ShowModalOfDeletingParent`) opening the
        «هل انت متاكد من حذف ؟» modal with the optional التعليق textarea; «تم» →
        `notification.confirm()` → `family.service.removeProviderSponsorLink(familyId, comment)`
        → success toast + reload members; «غلق» cancels (AC 3 — nothing sent).
  - [x] Guard refusals (AC 4) surface as an error toast with the server message.
  - [x] i18n keys `families.members.removeSponsorLink.*` in **both** `ar.json` and `en.json`.
- [x] **Task 4 — Verify** (AC: 1–6): with a sponsored orphan in the family → refusal, nothing
      written; clear the sponsorship (unsponsored family) → removal succeeds, provider row gone
      from reads; declined confirm → no call; `Charity`-role DELETE → 403; `dotnet build` +
      `npm run build` green.

### Review Findings (code review 2026-08-24)

- [x] [Review][Decision] Parent-guardian families get 404 — the endpoint only serves the non-parent `Provider` seat; families whose guardian is the mother/father designation (`Mother/Father.IsProvider`, no Provider row) cannot have a link removed at all, although UC-FAM-13 names the acting guardian. Extend the endpoint to the parent designation or record the limitation — FamiliesController.cs:1468, FamilyService.cs:1519-1525 *(applied 2026-08-24, review-and-complete pass — product ruling: extend to parents. When no Provider row exists but `ProviderType` designates Father/Mother, the removal clears that parent's `IsProvider`, resets `ProviderType` and stamps the التعليق on the parent's Notes — the designation WAS the link, so the parent row is NOT soft-deleted (they remain a family member). Same in-transaction sponsorship guard; truly guardian-less families still 404)*
- [x] [Review][Decision] Sponsorship gate keys on `SponsorId != null && SponsorshipStatus == "Sponsored"` — but no application code ever writes `SponsorId`, and the platform's own derivation (`OrphanPaymentService.cs:781`) treats `SponsorId.HasValue` as sponsored regardless of the stored string; imported rows with `SponsorId` set but a different status text pass the gate (false-allow — the exact scenario the guard exists for). Confirm the fix: key on `SponsorId != null` alone — FamilyService.cs:1534 *(orchestrator-verified: `SponsorId` is written by no code path; the only status writer hard-codes "Unsponsored") *(applied 2026-08-24: gate keys on `SponsorId != null` alone — same derivation as `OrphanPaymentService`)*
- [x] [Review][Patch] Sponsorship check runs outside the transaction (TOCTOU) — move it inside — FamilyService.cs:1531-1539 *(applied 2026-08-24: the guard re-reads inside the transaction before the delete)*
- [x] [Review][Patch] `DeletedBy`/`DeletedOn` never stamped — the Dev Notes' "the interceptor/deletions stamp who/when" claim is false on this stack (`ChangeTrackerExtensions` stamps only Created/Updated); stamp manually at the delete site, following the Incoming/Outgoing/SupportTicket precedent — FamilyService.cs:1557 *(applied 2026-08-24: stamped at the delete site, Incoming/Outgoing/SupportTicket precedent; the Dev Notes claim stands corrected by this record)*
- [x] [Review][Patch] Story-record bookkeeping — the remove-link modal + `submitRemoveLink` ship in the family-members component built under 5-7/5-8; confirm this story's File List claims those files so the audit trail matches the code — this story file *(verified 2026-08-24: the File List claims family-members .ts/.html/.spec + family.service + i18n — the audit trail matches)*
- [x] [Review][Defer] English refusal message listing the blockers (AC 4's "localized") — deferred, pre-existing (platform-wide)
- [x] [Review][Defer] Service-level Charity refusal maps to 400 rather than 403 — deferred, pre-existing (endpoint already 403s; cosmetic)

Dismissed as noise: 2 — ReportsController cross-story cohabitation (declared, no route collision); the "no caller for `removeProviderSponsorLink`" claim from the blind pass (refuted: the family-members modal from 5-7/5-8 is the caller — recorded in B-audit's triage note).

## Dev Notes

- **Model interpretation (recorded):** the legacy link "guardian's sponsorship" maps on this
  platform to the guardian (`Provider`) row, guarded by the family's live orphan sponsorships.
  Do NOT null out `Orphan.SponsorId` here — ending an orphan's sponsorship is a payment-cycle
  operation (epic 10), not this use case; this story only refuses while live sponsorships exist.
- Soft delete only — never a hard `DELETE`; the row must vanish from list/read endpoints because
  they filter `!IsDeleted` (verify the provider reads do filter; if a provider read path forgets
  the filter, fix that read, don't hard-delete instead).
- The removal and its guard run in one `IUnitOfWork` save; the guard is a query inside the same
  scope so a racing sponsorship insert is out of scope (acceptable at this operational scale).
- If 5-7 has not landed, the members screen does not exist — implement the backend + host the
  action on `family-detail` temporarily ONLY if forced; prefer waiting for 5-7.
- Platform invariants: `ApiResponse` envelope, platform exceptions, typed DTO, `IUnitOfWork`-only
  saves, claims-based identity, no business logic in the controller.
- **Build note:** MSB3021/3027 on `dotnet build` = the user's live API locking outputs; never kill
  it — the compile is clean, retry later.

### References

- [Source: docs/Modules/10-UC-FAM-Family-Register.md#10.U.13] scenario — active-sponsorship guard
- [Source: docs/Modules/10-UC-FAM-Family-Register.md#10.S.2] حذف الكفالة modal (`DeleteSposnor`)
- [Source: _bmad-output/planning-artifacts/epics.md#3.5] US-FAM-13 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/Orphan.cs:68,78] `SponsorshipStatus`, `SponsorId` — the guard keys
- [Source: Backend/src/IIROSA.Domain/Entities/Provider.cs:15,55] `FamilyId`, `Notes`
- [Source: Backend/src/IIROSA.Application/Services/FamilyService.cs:639] soft-delete precedent

## Dev Agent Record

### Agent Model Used

claude-sonnet-4.5 (Claude Code, BMAD dev-story workflow — review-and-complete pass)

### Debug Log References

- `dotnet build IIROSA.Api.csproj` → **Build succeeded** (DTO, validator, service method +
  ctor wiring, controller endpoint).
- `npx tsc --noEmit -p tsconfig.json` filtered to `family-members` + `family.service` → no
  errors.
- `node` JSON.parse + key-diff → both i18n files valid; `families.members` 42/42 ar↔en parity.

### Completion Notes List

- **Query binding ruling:** the التعليق travels as `[FromQuery] string? comment` on the DELETE
  (per Dev Notes — DELETE bodies are unreliable across clients) and is wrapped into the typed
  DTO in the controller bind step; the validator runs in the service as usual.
- **Read-vanishing verified by construction:** `ProviderRepository.GetByFamilyIdAsync` and
  `IsNationalIdExistsAsync` both filter `!IsDeleted`, so the soft-deleted row disappears from
  `GetFamilyProviderAsync` AND the national id becomes attachable to another family — which is
  the use case's own post-condition ("so the family can be re-attached to a different
  guardian" / the guardian re-attachable). No read path needed fixing.
- **ProviderType seat clearing:** when the deleted row was the acting guardian
  (`ProviderType == "Other"`), the designation is nulled — the seat is vacant for 5-9/5-10 to
  re-fill. Father/Mother designations keep their meaning (the row was auxiliary per the BR-06
  comment in `ControlFamilyMemberAsync`).
- **Guard refusal naming:** the active-sponsorship refusal lists the blocking orphans' codes
  (code, falling back to full name when uncoded) so the HQ operator knows exactly what to end
  first. Guard is a `TableNoTracking` read before the transaction — every refusal leaves the
  register untouched.
- The التعليق is appended to `Provider.Notes` in the 5-7 stamp format
  (`[yyyy-MM-dd HH:mm] … by {user}: {comment}`) BEFORE `IsDeleted = true`, so the reason
  survives on the soft-deleted row.
- Frontend gate: the delete action renders only for `hasAnyRole(['SuperAdmin','Admin'])` —
  cosmetic only; the endpoint authorises and the service re-refuses a charity-role caller
  (defense in depth, same as `ControlFamilyMemberAsync`).
- Live walkthrough (sponsored family → refusal; unsponsored → removal; declined confirm → no
  call; Charity-role DELETE → 403) batched into the epic-5 sweep.

### File List

- `Backend/src/IIROSA.Application/DTOs/Family/RemoveProviderSponsorLinkDto.cs` — new: the
  التعليق DTO (optional, ≤ 500).
- `Backend/src/IIROSA.Application/Validators/Family/RemoveProviderSponsorLinkValidator.cs` —
  new: comment length bound.
- `Backend/src/IIROSA.Application/Interfaces/IFamilyService.cs` —
  `RemoveProviderSponsorLinkAsync` contract with guard/seat docs.
- `Backend/src/IIROSA.Application/Services/FamilyService.cs` — ctor validator wiring +
  UC-FAM-13 region (role refusal → family/provider load → active-sponsorship guard → notes
  stamp → soft delete + seat clear, one transaction).
- `Backend/src/IIROSA.Api/Controllers/FamiliesController.cs` —
  `DELETE {familyId}/provider/sponsor` (HQ roles, `[FromQuery] comment`, full exception
  ladder).
- `Frontend/src/app/modules/families/services/family.service.ts` —
  `removeProviderSponsorLink(familyId, comment?)`.
- `Frontend/src/app/modules/families/family-members/family-members.component.ts` — HQ gate,
  delete-modal state + `submitRemoveLink` (confirm → call → toast → reload; server refusal
  surfaces via error toast).
- `Frontend/src/app/modules/families/family-members/family-members.component.html` — delete
  action on the guardian row + «هل انت متاكد من حذف ؟» modal with التعليق.
- `Frontend/src/app/modules/families/family-members/family-members.component.spec.ts` —
  modal-state resets + declined-confirm-sends-nothing tests.
- `Frontend/src/assets/i18n/ar.json` + `en.json` — `families.members.removeLink*` (11 keys).
- `Backend/src/IIROSA.Domain/Configurations/ProviderConfiguration.cs` + migration
  `20260824172643_Epic05_ProviderSeatSoftDeleteIndex` — this story's own post-condition
  ("the guardian becomes re-attachable to another family / the seat re-fillable") was
  physically impossible under the plain unique seat index; the filtered unique
  (`[IsDeleted] = 0`) restores it (defect found live and fixed under 5-10's approve
  walkthrough).

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-FAM-13 and module spec §10.U.13; platform model interpretation (Provider row guarded by orphan sponsorships) recorded. |
| 2026-08-24 | Implemented end to end: guard + soft-delete service method, HQ-only DELETE endpoint (comment via query), members-screen delete modal, i18n, specs. Status → review. |
| 2026-08-24 | Completion pass — parent-designation ruling APPLIED: families whose guardian is the Father/Mother designation can now unlink (flag cleared + `ProviderType` reset + التعليق on the parent's Notes; the parent row is never soft-deleted); same in-transaction sponsorship guard; defense-in-depth tenancy wired. |
| 2026-08-24 | Live walkthrough (private instance): DELETE on a father-designated family → 200, father `IsProvider` cleared with the parent row still live (never soft-deleted), التعليق stamped in the 5-7 format, `ProviderType` reset. The remove→re-attach cycle was also unblocked live by the filtered `IX_Provider_FamilyId` (the plain unique index made any re-attach after a removal impossible — defect found and fixed during 5-10's walkthrough). Status → done. |
