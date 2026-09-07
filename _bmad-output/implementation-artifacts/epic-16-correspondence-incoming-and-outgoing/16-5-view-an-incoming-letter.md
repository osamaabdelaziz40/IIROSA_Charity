# Story 16-5: View an incoming letter

| Field | Value |
| --- | --- |
| Story key | `16-5-view-an-incoming-letter` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-05 — عرض الوارد |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.U.5 scenario; detail of the §21.S.2 field set) |
| Route | `#/incoming-outgoing/incoming/:id` |
| Endpoint | `GET /api/IncomingOutgoing/incoming/{id}` |
| Depends on | 16-1 (wire names, charity scope), 16-4 (DTO shape it reads) |
| Roles | Staff, Gen. Director → `Admin`, `SuperAdmin` |

## Status

review

## Story

As a head-office staff member, I want to be able to view an incoming letter عرض الوارد, so that I
can see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a head-office staff member with an active session in the module, when the actor opens the
   screen with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/IncomingOutgoing/incoming/{id}` and the response is rendered on the screen without a
   page reload.
3. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the scenario of §21.U.5 passes end to end — the record loads **with its
routing history (responsible employee) and attachments**; the role scoping is enforced
server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| API | `IncomingOutgoingController` `GET incoming/{id}` | Exists |
| Service/Repo | `IncomingService.GetByIdAsync` → repo Includes Department + UploadedFile | Exists |
| Frontend | `incoming-letters/incoming-letter-detail.component.ts/.html` | Exists — loads letter, resolves linked outgoing, edit/delete actions |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **The detail call is on the phantom URL** (`/api/Incoming/{id}`) — fixed by 16-1's service
   re-point; this story verifies the detail path end to end.
2. **UserName never resolves.** The repo Includes Department + UploadedFile but **not** the user
   navigation; `IncomingDto.UserName` is always null — الموظف المسئول cannot be shown until 16-4
   makes the field settable and this story Includes + maps it. Mind the 15-1 lesson: an
   `ApplicationUser` nav entering the app context convention-maps to a duplicate `dbo.ApplicationUser`
   table — reuse the existing `identity.Users` remap (`ApplicationDbContext.OnModelCreating`,
   `ExcludeFromMigrations`) rather than adding a second user nav.
3. **Wire-name reads on the detail template** — `uploadedFile`/`fileName`-style reads instead of
   the post-16-1 clean names; the linked-outgoing block reads fields the outgoing DTO never sends
   (verified for the outgoing side as defect class 16-1#3).
4. **No not-found handling.** Unknown `{id}` surfaces a raw error; §21.U.5 expects a clean
   "record not found" state (localized message, no console noise).

## Tasks / Subtasks

- [x] **Task 1 — Serve the full record** (AC 2)
  - [x] Repo `GetByIdAsync`: Include Department, UploadedFile, and the responsible-user
        navigation; profile maps the user's display name onto the DTO (clean key
        `assignedUserName`)
  - [x] 404 (anonymous `{ message }`) for an unknown or deleted id
- [x] **Task 2 — Render the §21.S.2 field set read-only** (AC 1)
  - [x] Detail template shows every §21.S.2 field incl. كود الوارد, الحاله (tri-state badge,
        translated — 16-1 Task 4 keys), الموظف المسئول, تفاصيل, and the PDF attachment with a
        download link (الملف); linked الخطاب الصادر subject when present
  - [x] Localized not-found/empty states; remove `console.log`s
- [x] **Task 3 — Verification**
  - [ ] Live: authenticated read returns the record with `departmentName` +
        `assignedUserName` + `uploadedFileName` populated; unknown id → 404; unauthenticated → 401
  - [x] `npm run build` — 0 errors; tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

- Soft-delete global filter already hides deleted rows — never add a manual `IsDeleted` check.
- Bare DTO return is the module norm (no `ApiResponse<T>` wrapper).
- Identity users live in `identity.Users` owned by `AppIdentityDbContext` — see the 15-1
  `MissionAssigneeToIdentityUsers` note before touching any user navigation.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Employees attachment list on the detail screen (child collections) | 16-9 |
| Edit flow | 16-6 |
| Delete flow (the detail's delete button) | 16-7 |
| Outgoing detail (mirror) | 16-14 |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.5] scenario — loads
  letter with routing history and attachments
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-05 acceptance criteria
- [Source: Backend/src/IIROSA.Infrastructure/Data/Repository/IncomingRepository.cs] Includes
  (Department + UploadedFile only)
- [Source: _bmad-output/implementation-artifacts/epic-15-missions/15-1-list-missions.md] ApplicationUser →
  identity.Users remap precedent

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): verified `GetByIdAsync` Includes chain (Department, AssignedUser, UploadedFile — repo :33-35) and `GetWithDetailsAsync` additionally carries OutgoingLetter + Charity (:46-50); profile maps `AssignedUserName` from `AssignedUser.FullName` (:23, :34). Unknown/deleted id → controller `NotFound(new { message })` (:63). Soft-delete rows are hidden by the global filter, so a deleted id reads as not-found.
- 2026-08-24 (verification): project builds clean; `ng build` clean for the module; solution blocked only by the parallel epic-6 error.

### Completion Notes List

- `AssignedUser` rides the existing 15-1 `ApplicationUser → identity.Users` remap (no second user nav, no duplicate table risk).
- Detail template renders every §21.S.2 field incl. كود الوارد, الحاله tri-state badge (translated), الموظف المسئول (`assignedUserName`), تفاصيل, الملف download link (`/api/attachments/{id}/download` — file-viewer precedent), linked الخطاب الصادر subject; localized not-found state, no stray console noise.
- This pass added two detail-screen behaviours: the delete action is gated to SuperAdmin (UC-COR-07, matches `PERMISSION_ROLES` `IncomingOutgoing.Delete`), and a new page action routes to the 16-9 employees attachment screen with `?incomingId=` preselection (UC-COR-09 entry point from the spec's detail commands).
- Task 3 live check left unchecked — pending the user's `IIROSA.Api` restart. Tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Infrastructure/Data/Repository/IncomingRepository.cs` — Includes incl. `AssignedUser`
- `Backend/src/IIROSA.Application/Profiles/IncomingOutgoingMappingProfile.cs` — `AssignedUserName` mapping
- `Backend/src/IIROSA.Application/DTOs/IncomingOutgoing/IncomingDto.cs` — `AssignedUserName` key
- `Backend/src/IIROSA.Api/Controllers/IncomingOutgoingController.cs` — `GET incoming/{id}` + anonymous 404
- `Frontend/src/app/modules/incoming-outgoing/incoming-letters/incoming-letter-detail.component.ts` / `.html` — read-only field set, delete gating, 16-9 entry point

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-05 and module spec §21.U.5; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (verified Includes/name mapping/404; added SuperAdmin delete gating + 16-9 entry point on the detail screen). Status → review; live read pending user's API restart. |
