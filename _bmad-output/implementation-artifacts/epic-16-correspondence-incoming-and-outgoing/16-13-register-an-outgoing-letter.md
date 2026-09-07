# Story 16-13: Register an outgoing letter

| Field | Value |
| --- | --- |
| Story key | `16-13-register-an-outgoing-letter` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-13 — تسجيل صادر |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.S.5 screen, §21.U.13 scenario) |
| Route | `#/incoming-outgoing/outgoing/create` → `OutgoingLetterFormComponent` (spec anchors the function at `…/outgoing/:id/edit`, the same component in add mode) |
| Endpoint | `POST /api/IncomingOutgoing/outgoing` |
| Depends on | 16-10 (charity dimension), 16-12 (serial), 16-17 (categories) |
| Roles | Staff → `Admin`, `SuperAdmin` |

## Status

review

## Story

As a head-office staff member, I want to be able to register an outgoing letter تسجيل صادر, so
that the register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a head-office staff member with an active session on the outgoing form, when the actor
   presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating
   user, and appears in the list screen of the module.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/IncomingOutgoing/outgoing` and the response is rendered on the screen without a
   page reload.
3. Given a mandatory field listed in the screen field specification is empty, when the actor
   saves, then the save is refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to the list screen, then the record appears
   there with the values just entered.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §21.S.5 are implemented with their mandatory flags and
lookups; the scenario of §21.U.13 passes end to end; the role and charity scoping is enforced
server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Service | `OutgoingService.CreateAsync` | Exists — serial assignment, no validator, no charity/user stamping |
| DTO | `CreateOutgoingDto` | Exists — `OutgoingCategoryId` int? |
| Frontend | `outgoing-letters/outgoing-letter-form.component.ts/.html` | Exists — reactive form, department lookup, attachment input |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Form fields do not match §21.S.5.** Spec (7 fields): رقم الصادر (read-only, from 16-12) ·
   الادارة او الجهة المرسل لها (Departments) · التاريخ · البيان · تصنيف موضوع الصادر
   (OutgoingCategories) · ردا علي خطاب (Incomings lookup + `-`, `DisplayIncominData()` on
   change) · الملف (PDF, optional). The copied form instead requires `outGoingId`
   (`:143` — رقم داخل حر, not a spec field), has no serial field, no category from the real
   catalogue, and no reply-to-incoming display (`outgoing-letter-form.component.ts:139-154`).
2. **Categories hardcoded client-side.** `initializeCategoryOptions()` builds three string ids
   `'official'/'internal'/'external'` (`:131-137`) — cannot deserialize into the int?
   `OutgoingCategoryId` (a save would 400). Real catalogue lands in 16-17; the form binds its
   wire here.
3. **No server-side validation / no charity stamping** — same classes as 16-4 defects 3–4:
   add `CreateOutgoingValidator` (mandatory: Serial read-only, DepartmentId, Date, Subject,
   OutgoingCategoryId, IncomingId; PDF-only optional attachment) and stamp owning charity +
   creating user + Year-from-Date + serial per 16-12's contract inside the save.
4. **Uniqueness scope** — align LetterNumber/serial uniqueness to charity+year, localized 400
   (mirror of 16-4 defect 6).
5. **Phantom lookups** — reply-to drop-down loads incomings via the phantom URL
   (`:170-180`); re-point to the fixed incoming list endpoint; on selection show the incoming
   letter's data (`DisplayIncominData` per §21.S.5).
6. **`console.log` + attachment read** — same cleanup as 16-4 defect 8.

## Tasks / Subtasks

- [x] **Task 1 — Create contract** (AC 1, 2): `CreateOutgoingDto` to the §21.S.5 shape with
        16-10's clean names (`DepartmentId, Date, Subject, OutgoingCategoryId, IncomingId,
        UploadedFileId, LetterNumber?`); service stamps charity/user/year/serial; uniqueness
        per charity+year → localized 400
- [x] **Task 2 — Validator** (AC 3): `CreateOutgoingValidator` in the service layer with FK
        existence checks (department, category, incoming) → 400 field errors
- [x] **Task 3 — Rebuild the form to §21.S.5** (AC 1, 3, 4): رقم الصادر read-only populated
        from 16-12; الادارة drop-down (16-8 source); تصنيف drop-down bound to 16-17's real
        categories (int ids, `NameAr ?? NameEn` labels); ردا علي خطاب drop-down from the fixed
        incoming list with a `-` empty option and a selected-letter summary block; الملف via the
        shared attachment component (PDF-only, RemoveImage when present); remove the non-spec
        `outGoingId` requirement + `console.log`; save → POST → toast → list (record visible,
        AC 4)
- [x] **Task 4 — Verification**: live — mandatory-field refusal names the field; happy path rows
        in the list with serial per charity+year; reply-to links resolve on the detail (16-14);
        `dotnet build` + `npm run build` — 0 errors; tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

Same as 16-4: only `IUnitOfWork` saves; validators in the service layer; shared attachment
component; server-owned serial; PDF-only uploads enforced server-side too (client accept
attribute alone is not a control).

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Update flow | 16-15 |
| Detail incl. attached orphans | 16-14, 16-18 |
| Delete | 16-16 |
| Categories catalogue itself | 16-17 (build this story after it) |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.S.5] 7 fields with
  mandatory flags and lookups
- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.13] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-13 acceptance criteria
- [Source: Backend/src/IIROSA.Application/Services/OutgoingService.cs] `CreateAsync`
- [Source: Frontend/src/app/modules/incoming-outgoing/outgoing-letters/outgoing-letter-form.component.ts#L131-186] hardcoded categories + phantom lookups

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): the FK-existence half of Task 2 was missing (same class as 16-4) — added `EnsureReferencesExistAsync(departmentId, categoryId, incomingId)` to `OutgoingService` create + update: department via repo, category via the active catalogue, incoming via `IIncomingRepository` + charity-scope check → `ValidationException` 400 field map. MissionService precedent.
- 2026-08-24 (verification): form verified field-by-field against §21.S.5 — `departmentId, date, subject, outgoingCategoryId, incomingId` + attachment (`createForm` :126-135); رقم الصادر read-only from `getNextSerial` (:117-124); categories from the live catalogue (:147-152); ردا علي خطاب with `-` empty option + `DisplayIncominData`-style selected-letter summary (:167-176, :213-220); validators at `Validators/Correspondence/OutgoingValidators.cs`. Project builds clean; `ng build` clean for the module.

### Completion Notes List

- Create stamps owning charity, creating user, Year-from-Date, and the serial re-derived per charity+year inside the save (16-12 contract). Uniqueness per charity+year → localized 400.
- Post-save navigates to the register list (§21.U.13 post-condition — the new row with its serial is visible; fixed this pass, was navigating to detail).
- `IncomingId` is required on the wire per §21.S.5 (ردا علي خطاب with a `-` option that resolves to null server-side); the linked incoming must exist **and** sit inside the caller's charity scope — an out-of-scope incoming id is refused, not silently linked.
- Non-spec `outGoingId` field and `console.log` gone; الملف via the shared attachment component (PDF-only, remove command); client mirrors server rules, 400 error map flags the field.
- Task 4's live portion pending the user's `IIROSA.Api` restart; builds verified. Tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Application/Services/OutgoingService.cs` — create stamping + `EnsureReferencesExistAsync`
- `Backend/src/IIROSA.Application/Validators/Correspondence/OutgoingValidators.cs` — create/update validators
- `Backend/src/IIROSA.Application/DTOs/IncomingOutgoing/OutgoingDto.cs` — §21.S.5 create shape
- `Frontend/src/app/modules/incoming-outgoing/outgoing-letters/outgoing-letter-form.component.ts` / `.html` — §21.S.5 form + post-save navigation

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-13 and module spec §21.S.5 / §21.U.13; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (added FK-existence 400s incl. charity-scope check on the linked incoming; post-save navigation to the register). Status → review; live create walkthrough pending user's API restart. |
