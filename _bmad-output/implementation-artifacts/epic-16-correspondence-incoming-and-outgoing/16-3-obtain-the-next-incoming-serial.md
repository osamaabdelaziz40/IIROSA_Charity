# Story 16-3: Obtain the next incoming serial

| Field | Value |
| --- | --- |
| Story key | `16-3-obtain-the-next-incoming-serial` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-03 — رقم الوارد التالي |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.U.3 scenario; the serial appears on the §21.S.2 form) |
| Route | consumed by `#/incoming-outgoing/incoming/create` (16-4 renders it read-only) |
| Endpoint | `GET /api/IncomingOutgoing/incoming/next-serial` |
| Depends on | 16-1 (charity dimension + controller hygiene) |
| Roles | Staff → `Admin`, `SuperAdmin` |

## Status

review

## Story

As a head-office staff member, I want to be able to obtain the next incoming serial رقم الوارد
التالي, so that the letter I am about to register is filed in order.

## Acceptance Criteria

1. Given a head-office staff member with an active session in the module, when the actor opens the
   screen with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/IncomingOutgoing/incoming/next-serial` and the response is rendered on the screen
   without a page reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the scenario of §21.U.3 passes end to end; the serial sequence is scoped
per charity and year; the role and charity scoping is enforced server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Repo | `IncomingRepository.GetNextSerialNumberAsync` | Exists — `Max(Serial) + 1` scoped by department + year |
| Service | `IncomingService.GenerateSerialTextAsync` | Exists — formats `"INC-{year}-{serial:D4}"` |
| API | `IncomingOutgoingController` | Exists — `GET incoming/next-serial?departmentId=&year=` |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Scope is wrong.** The next serial is computed per **department** + year; the spec's register
   is a per-charity, per-year sequence (§21.U.3 pre-condition 3: scope to the charity that owns
   the user). Two letters routed to different departments would collide on the same serial today.
2. **Serial text is English.** `GenerateSerialTextAsync` produces `INC-2026-0001`; the register is
   Arabic-primary — كود الوارد must be a neutral/Arabic-friendly code (plain zero-padded number or
   an Arabic scheme), not an `INC-` prefix.
3. **Race window.** `Max+1` computed at GET time can hand the same number to two concurrent
   registrations. Mitigate within the read story: final assignment is re-derived **inside the
   create transaction** in 16-4; this endpoint is advisory (display). Document that contract.
4. **No charity scope on the endpoint** — like every other read (16-1 defect 4), the query ignores
   the caller entirely.
5. **The SPA never calls it.** No frontend usage exists — the create form (16-4) will consume it;
   add the service method `getNextIncomingSerial(year?)` so 16-4 has a real call to make.

## Tasks / Subtasks

- [x] **Task 1 — Fix the sequence semantics** (AC 2, 3, 4)
  - [x] `GetNextSerialNumberAsync`: scope `Max(Serial)+1` by **charity + year** (department
        drops out of the key; it stays a letter attribute, not a sequence attribute); the caller's
        charity comes from `ICurrentUserService` via the service — the endpoint takes `year` and an
        HQ-only `charityId`, never a client-asserted one
  - [x] `GenerateSerialTextAsync`: replace `INC-{year}-{serial:D4}` with a plain zero-padded
        `"{serial:D4}"`-style كود الوارد (Arabic register order); no English prefix
  - [x] Response: return `{ serial, serialTxt }` (clean camelCase names, 16-1 convention)
- [x] **Task 2 — Make it advisory-only, safely** (AC 1)
  - [x] Document (code comment + story note) that the returned number is reserved only when 16-4's
        create persists it; create re-derives inside the save so a stale read cannot duplicate
- [x] **Task 3 — Expose to the SPA** (AC 2)
  - [x] `incoming.service.ts`: add `getNextIncomingSerial(year?: number)` calling the fixed
        endpoint; 16-4 binds it to the read-only كود الوارد field
- [x] **Task 4 — Verification**
  - [ ] Live: with letters seeded for a charity+year, the endpoint returns max+1; a different
        charity's letters do not advance the counter; unauthenticated call → 401
  - [x] `dotnet build` + `npm run build` — 0 errors; tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

- Same platform rules as 16-1. The serial is **data, not identity** — never make it the primary
  key; the Guid Id stays the identifier on the wire.
- Bare value returns are the module norm (no `ApiResponse<T>` wrapper).

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Rendering the read-only serial on the form + persisting it on save | 16-4 |
| Outgoing serial (mirror) | 16-12 |
| Serial appearing in list grid | 16-1 |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.3] scenario —
  reserve the next sequential number, charity-scoped
- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.S.2] كود الوارد —
  mandatory · read-only
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-03 acceptance criteria
- [Source: Backend/src/IIROSA.Infrastructure/Data/Repository/IncomingRepository.cs]
  `GetNextSerialNumberAsync`
- [Source: Backend/src/IIROSA.Application/Services/IncomingService.cs] `GenerateSerialTextAsync`

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): verified the shipped chain — `IncomingService.GetNextSerialAsync` (:271) pins `effectiveCharity = _currentUser.CharityId ?? charityId` (advisory read is pin-never-widen too, added this pass), `IncomingRepository.GetNextSerialAsync(charityId, year)` scopes `Max(Serial)+1`, `SerialTxt` is plain `ToString("D4")` — no English prefix.
- 2026-08-24 (verification): project builds clean; solution build blocked only by the parallel epic-6 error; `ng build` clean for the module.

### Completion Notes List

- Sequence key is **charity + year** (department dropped out — stays a letter attribute); the caller's charity comes from `ICurrentUserService`, `charityId` param is HQ-only and never client-asserted for charity callers (a charity caller's own claim wins).
- Response is the bare `{ serial, serialTxt }` pair (module norm — no wrapper).
- Advisory-only is documented in code on both forms (comment: "the server allocates the definitive serial per charity + year inside the create transaction") — `CreateAsync` re-derives the serial inside the save, so a stale read cannot duplicate.
- SPA: `incoming.service.ts` `getNextSerial(year?, charityId?)` → `GET /api/IncomingOutgoing/incoming/next-serial`; the create form binds the read-only كود الوارد field via `nextSerialTxt` (16-4). Outgoing mirror shipped as `outgoing.service.ts` `getNextSerial` (16-12).
- Task 4 live check left unchecked — pending the user's `IIROSA.Api` restart. Tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Application/Services/IncomingService.cs` — `GetNextSerialAsync` charity pin, `D4` text
- `Backend/src/IIROSA.Infrastructure/Data/Repository/IncomingRepository.cs` — charity+year-scoped `Max+1`
- `Backend/src/IIROSA.Api/Controllers/IncomingOutgoingController.cs` — `GET incoming/next-serial`
- `Frontend/src/app/modules/incoming-outgoing/services/incoming.service.ts` — `getNextSerial`
- `Frontend/src/app/modules/incoming-outgoing/incoming-letters/incoming-letter-form.component.ts` — read-only serial binding

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-03 and module spec §21.U.3; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (charity+year key, plain `D4` serial, advisory-only contract, charity pin on the advisory read). Status → review; live check pending user's API restart. |
