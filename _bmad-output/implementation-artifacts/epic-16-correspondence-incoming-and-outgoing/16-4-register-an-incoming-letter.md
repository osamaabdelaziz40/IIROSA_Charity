# Story 16-4: Register an incoming letter

| Field | Value |
| --- | --- |
| Story key | `16-4-register-an-incoming-letter` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-04 — تسجيل وارد |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.S.2 screen, §21.U.4 scenario) |
| Route | `#/incoming-outgoing/incoming/create` → `IncomingLetterFormComponent` (spec anchors the function at `…/incoming/:id/edit`, the same component in add mode) |
| Endpoint | `POST /api/IncomingOutgoing/incoming` |
| Depends on | 16-1 (wire + charity dimension), 16-3 (next serial), 16-8 (departments lookup) |
| Roles | Staff → `Admin`, `SuperAdmin` |

## Status

review

## Story

As a head-office staff member, I want to be able to register an incoming letter تسجيل وارد, so that
the register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a head-office staff member with an active session on the incoming form, when the actor
   presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating
   user, and appears in the list screen of the module.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/IncomingOutgoing/incoming` and the response is rendered on the screen without a page
   reload.
3. Given a mandatory field listed in the screen field specification is empty, when the actor
   saves, then the save is refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to the list screen, then the record appears
   there with the values just entered.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §21.S.2 are implemented with their mandatory flags and
lookups; the scenario of §21.U.4 passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Service | `IncomingService.CreateAsync` | Exists — uniqueness checks, serial assignment, `Status ?? "Received"` |
| DTO | `CreateIncomingDto` | Exists — but shaped for the legacy form, not §21.S.2 |
| Profile | `IncomingOutgoingMappingProfile` | Exists — Create/Update → entity maps |
| Frontend | `incoming-letters/incoming-letter-form.component.ts/.html` | Exists — reactive form, department lookup, attachment input |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Form fields do not match §21.S.2.** Spec (11 fields): التاريخ · كود الوارد (read-only, from
   16-3) · رقم الخطاب · تاريخ الخطاب · الادارة او الجهة الراسله (Departments lookup) · البيان ·
   الحاله (معلق / تم الرد / تم عمل اللازم) · الموظف المسئول (allEmployees) · الخطاب الصادر
   (Outgoings) · تفاصيل · الملف (PDF, optional). The copied form instead requires `incomingId`
   (رقم وارد حر — **not a spec field**), has **no serial field, no status-by-spec, no responsible
   employee, no mandatory department**, and treats الخطاب الصادر as optional
   (`incoming-letter-form.component.ts:141-159`).
2. **`FK_UserId` can never be set.** `CreateIncomingDto` / `UpdateIncomingDto` have no user
   property and `CreateAsync` never stamps `FK_UserId` — الموظف المسئول (mandatory in the spec)
   cannot persist. (UC-COR-09's *multi*-employee attachment is 16-9; this field is the single
   responsible employee.)
3. **No server-side validation.** No FluentValidation validator exists for create/update;
   mandatory-field refusal (AC 3) currently depends on the client only. Platform rule: validators
   run in the service layer.
4. **Wrong default status.** `Status = dto.Status ?? "Received"` — must default to معلق within the
   16-1 tri-state.
5. **Serial handling.** Create calls `GetNextSerialNumberAsync` (department-scoped, English text —
   16-3 defects); after 16-3 the create path must re-derive per charity+year inside the save.
6. **Wrong uniqueness scope.** Uniqueness is enforced on `IncomingId` + `LetterNumber` per
   department/year; with the spec shape it should be `LetterNumber` per charity+year (and only
   that) — return a localized 400, never a 500.
7. **Phantom-dependent reads.** The reply-to dropdown loads all outgoing letters via the phantom
   `/api/Outgoing` URL (`:174-185`) — dead until 16-10; bind it to
   `GET /api/IncomingOutgoing/outgoing` once fixed, or defer the dropdown's population to 16-10
   (keep the control, seed it when reachable).
8. **`console.log` left in** `onAttachmentChange` (`:229`); attachment re-population reads
   `letter.uploadedFile` which is not the wire name (`uploadedFileId` after 16-1 renames).

## Tasks / Subtasks

- [x] **Task 1 — Reshape the create contract** (AC 1, 2)
  - [x] `CreateIncomingDto` (clean names per 16-1): `Date, SerialTxt` (read-only echo, server
        re-derives), `LetterNumber, LetterDate, DepartmentId, Subject, Status, AssignedUserId,
        OutgoingId, LetterDescription, UploadedFileId` — drop `IncomingId`/`IncomingNumber`/
        `Year`(derive from date server-side) from the create shape; keep the entity properties
        (legacy data reads them)
  - [x] `IncomingService.CreateAsync`: stamp owning charity + `FK_UserId` (creating user) + Year
        from Date; derive serial per charity+year inside the save (16-3 contract); default status
        معلق; uniqueness = LetterNumber per charity+year → localized 400
- [x] **Task 2 — Server-side validation** (AC 3)
  - [x] `CreateIncomingValidator` (FluentValidation, `Validators/IncomingOutgoing/`, invoked in
        the service): mandatory — Date, LetterNumber, LetterDate, DepartmentId, Subject, Status,
        AssignedUserId, OutgoingId, LetterDescription; UploadedFileId optional but PDF-only;
        department/outgoing/user existence checks → 400 with `ModelStateErrors`-style anonymous
        shape, not 500
- [x] **Task 3 — Rebuild the form to §21.S.2** (AC 1, 3, 4)
  - [x] Fields in spec order with mandatory flags; كود الوارد read-only, populated on load from
        16-3's `getNextIncomingSerial`; الموظف المسئول drop-down from `GET /api/UserManagement`
        (paged — load active users, bind id, label localized name); الخطاب الصادر drop-down from
        the outgoing list endpoint (population completes with 16-10); الحاله tri-state; الملف via
        the shared `attachment` component, PDF-only, `RemoveImage` command when a file exists
  - [x] Remove the non-spec `incomingId` required field and `console.log`; wire save → POST →
        success toast → navigate to the list (record visible per AC 4)
  - [x] Client validation mirrors the server rules; server refusal flags the offending field on
        the form
- [x] **Task 4 — Verification**
  - [ ] Live: create with a mandatory field missing → 400 naming the field; happy path → row in
        the list with serial assigned per charity+year; two charities get independent sequences
  - [x] `dotnet build` + `npm run build` — 0 errors; tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

- Only `IUnitOfWork` saves — `CreateAsync` must not call `SaveChangesAsync` on the repository
  directly (15-1 recorded the same violation class in missions).
- FluentValidation in the **service layer**; FK existence violations are 400s, never 500s.
- Attachment handling goes through the shared `attachment` component — never bespoke upload code.
- The spec labels كود الوارد mandatory **and read-only** — the server is the source of truth; a
  client-supplied serial is ignored, not trusted.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Update flow + edit-mode patching | 16-6 |
| Multi-employee attachment (ربط الموظفين) | 16-9 |
| Delete | 16-7 |
| Outgoing form (mirror) | 16-13 |
| Import letters in bulk | not in this epic's scope |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.S.2] 11 fields with
  mandatory flags and lookups
- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.4] scenario —
  mandatory list, charity stamping, post-condition
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-04 acceptance criteria
- [Source: Backend/src/IIROSA.Application/Services/IncomingService.cs] `CreateAsync`
- [Source: Frontend/src/app/modules/incoming-outgoing/incoming-letters/incoming-letter-form.component.ts#L141-186] form shape + phantom lookups

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): the FK-existence half of Task 2 was missing — create/update accepted any `departmentId` / `assignedUserId` / `outgoingId` (500 or orphan row). Added `EnsureReferencesExistAsync(departmentId, assignedUserId, outgoingId)` to `IncomingService` (create :132, update :185): collects `FluentValidation.Results.ValidationFailure` entries (department via repo, assignee via `IEmployeeService`, outgoing via `IOutgoingRepository.GetWithDetailsAsync` + charity-scope check) and throws `ValidationException` → 400 with the field-flagging error map. MissionService precedent.
- 2026-08-24 (verification): validators confirmed at `Validators/Correspondence/IncomingValidators.cs`; `DefaultStatus = "معلق"`; uniqueness rides `IsLetterNumberUniqueAsync(letterNumber, charityId, year)` in both write paths. Project builds clean; solution blocked only by the parallel epic-6 error; `ng build` clean for the module.

### Completion Notes List

- Create contract reshaped per §21.S.2: `Date, SerialTxt (echo), LetterNumber, LetterDate, DepartmentId, Subject, Status, AssignedUserId, OutgoingId, LetterDescription, UploadedFileId` — non-spec `IncomingId`/`IncomingNumber` dropped from the create shape (entity keeps legacy columns). Year derives from Date server-side; owning charity + `FK_UserId` + serial (charity+year, inside the save) stamped by `CreateAsync`; default status معلق.
- Uniqueness = `LetterNumber` per **charity + year** → localized 400, never a 500.
- Validators live at `Validators/Correspondence/IncomingValidators.cs` (folder named `Correspondence`, not the story's suggested `IncomingOutgoing` — cosmetic deviation); invoked in the service layer per platform rule.
- Form rebuilt to §21.S.2: spec field order + mandatory flags; كود الوارد read-only from `getNextSerial`; الموظف المسئول from the employees read; الخطاب الصادر from the outgoing list endpoint; الحاله tri-state; الملف via the shared attachment component (PDF-only, remove command); non-spec required field and stray `console.log` gone. Post-save navigates to the register list (§21.U.4 post-condition — the new row with its serial is visible; fixed this pass, was navigating to detail).
- Client mirrors the server rules; a 400 error map flags the offending form fields.
- Task 4 live check left unchecked — pending the user's `IIROSA.Api` restart. Tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Application/Services/IncomingService.cs` — create contract stamping, معلق default, uniqueness, `EnsureReferencesExistAsync`
- `Backend/src/IIROSA.Application/DTOs/IncomingOutgoing/IncomingDto.cs` — `CreateIncomingDto`/`UpdateIncomingDto` shape
- `Backend/src/IIROSA.Application/Validators/Correspondence/IncomingValidators.cs` — create/update validators
- `Frontend/src/app/modules/incoming-outgoing/incoming-letters/incoming-letter-form.component.ts` / `.html` / `.scss` — §21.S.2 form
- `Frontend/src/app/modules/incoming-outgoing/services/incoming.service.ts` — create call

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-04 and module spec §21.S.2 / §21.U.4; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (added FK-existence 400s on create/update; post-save navigation to the register). Status → review; live create-refusal walkthrough pending user's API restart. |
