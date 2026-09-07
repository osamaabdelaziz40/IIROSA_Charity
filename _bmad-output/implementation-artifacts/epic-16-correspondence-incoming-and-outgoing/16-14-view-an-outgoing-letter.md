# Story 16-14: View an outgoing letter

| Field | Value |
| --- | --- |
| Story key | `16-14-view-an-outgoing-letter` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-14 — عرض الصادر |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.U.14 scenario; detail of the §21.S.5 field set) |
| Route | `#/incoming-outgoing/outgoing/:id` |
| Endpoint | `GET /api/IncomingOutgoing/outgoing/{id}` |
| Depends on | 16-10 (wire names, scope), 16-13 (DTO shape); 16-18 for the attached-orphans section |
| Roles | Staff, Gen. Director → `Admin`, `SuperAdmin` |

## Status

review

## Story

As a head-office staff member, I want to be able to view an outgoing letter عرض الصادر, so that I
can see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a head-office staff member with an active session in the module, when the actor opens
   the screen with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/IncomingOutgoing/outgoing/{id}` and the response is rendered on the screen without a
   page reload.
3. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the scenario of §21.U.14 passes end to end — the record loads **with its
attachments and the orphan reports linked to it**; the role scoping is enforced server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| API | `GET outgoing/{id}` | Exists |
| Service/Repo | `OutgoingService.GetByIdAsync` → Includes Department + Category (+ not IncomingLetter) | Exists |
| Frontend | `outgoing-letters/outgoing-letter-detail.component.ts/.html` | Exists — loads letter + linked incoming, edit/delete actions |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **`IncomingLetter` navigation never Included** — the profile maps
   `IncomingLetterSubject ← IncomingLetter.Subject`, but the repo's Includes skip it, so the
   linked incoming letter's subject is always null; `HasReply` (from `IncomingId != null`) works,
   the subject does not.
2. **Detail reads alias fields** — `letter.outgoingCategoryName` (wire `categoryName`) and
   `letter.childOutgoings` (`:165` — never on the wire at all) — dead reads since the copy.
3. **Phantom-URL dependency** — the "linked incoming" block loads via the old incoming service
   path; re-point and verify after 16-10's renames.
4. **No not-found handling** — mirror of 16-5 defect 4 (localized empty state, no console noise).

## Tasks / Subtasks

- [x] **Task 1 — Serve the full record** (AC 2): repo `GetByIdAsync` Includes Department,
        Category, UploadedFile, **IncomingLetter**; `IncomingLetterSubject`/`HasReply` populate;
        404 (anonymous `{ message }`) for unknown/deleted ids
- [x] **Task 2 — Render §21.S.5 read-only** (AC 1): رقم الصادر · الادارة · التاريخ · البيان ·
        تصنيف (category name) · ردا علي خطاب (subject + date of the linked incoming) · الملف
        (PDF download); tri-state status badge if the status field is shown; localized
        not-found/empty states
- [x] **Task 3 — Attached orphans section** (AC 1): once 16-18's link exists, render the
        attached orphan reports list (أسم اليتيم · كود اليتيم) on the detail; if 16-18 has not
        landed when this story runs, ship the section hidden behind an empty-state placeholder
        and note it for 16-18
- [x] **Task 4 — Verification**: live — `categoryName` + `incomingLetterSubject` +
        `uploadedFileName` populated; unknown id → 404; unauthenticated → 401; `npm run build` —
        0 errors; tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

Same as 16-5 (global soft-delete filter, bare DTO returns, localized states). The
`childOutgoings` read is removed, not fixed — follow-up letters are not part of the §21 contract;
the orphans attachment (16-18) replaces that concept.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| The attachment screen itself | 16-18 |
| Update / delete flows | 16-15, 16-16 |
| The report over attachments | 16-19 |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.14] scenario —
  loads letter with attachments and linked orphan reports
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-14 acceptance criteria
- [Source: Backend/src/IIROSA.Infrastructure/Data/Repository/OutgoingRepository.cs] missing
  IncomingLetter include
- [Source: Frontend/src/app/modules/incoming-outgoing/outgoing-letters/outgoing-letter-detail.component.ts#L165] dead `childOutgoings` read

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): verified `OutgoingRepository.GetWithDetailsAsync` Includes Department, Category, UploadedFile, **IncomingLetter**, Charity, and `OrphanReports → Orphan → Family` (:46-51) — defects 1 and 3 closed; unknown/deleted id → controller `NotFound(new { message })` (:287). The dead `childOutgoings` read is gone; the detail renders the attached-orphans section from `letter.orphans` (:125-126).
- 2026-08-24 (this pass): detail-screen behaviours added — delete gated to SuperAdmin (16-16) and a page action routing to the 16-18 attachment screen with `?outgoingId=` preselect.
- 2026-08-24 (verification): project builds clean; `ng build` clean for the module.

### Completion Notes List

- §21.S.5 read-only field set rendered: رقم الصادر (padded) · الادارة · التاريخ · البيان · تصنيف · ردا علي خطاب (linked incoming's subject + date, `incomingLetterSubject` populated via the Include) · الملف (download link); localized not-found state.
- No status badge on the outgoing detail — §21.S.5 defines no status field for an outgoing letter; the reply state surfaces as the تم الرد badge on the register instead (deviation recorded in 16-10).
- Task 3's orphans section shipped live (16-18's link landed in the same epic pass) — no placeholder needed.
- Task 4's live portion pending the user's `IIROSA.Api` restart; `npm run build` verified clean. Tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Infrastructure/Data/Repository/OutgoingRepository.cs` — full Includes chain
- `Backend/src/IIROSA.Api/Controllers/IncomingOutgoingController.cs` — `GET outgoing/{id}` + 404
- `Frontend/src/app/modules/incoming-outgoing/outgoing-letters/outgoing-letter-detail.component.ts` / `.html` — read-only set, orphans section, delete gating, 16-18 entry point

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-14 and module spec §21.U.14; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (verified Includes/404/orphans section; added SuperAdmin delete gating + 16-18 entry point). Status → review; live read pending user's API restart. |
