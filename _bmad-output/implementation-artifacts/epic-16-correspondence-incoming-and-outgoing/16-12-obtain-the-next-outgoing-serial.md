# Story 16-12: Obtain the next outgoing serial

| Field | Value |
| --- | --- |
| Story key | `16-12-obtain-the-next-outgoing-serial` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-12 — رقم الصادر التالي |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.U.12 scenario; the serial appears on the §21.S.5 form) |
| Route | consumed by `#/incoming-outgoing/outgoing/create` (16-13 renders it read-only) |
| Endpoint | `GET /api/IncomingOutgoing/outgoing/next-serial` |
| Depends on | 16-10 (charity dimension + scope), 16-3 (pattern) |
| Roles | Staff → `Admin`, `SuperAdmin` |

## Status

review

## Story

As a head-office staff member, I want to be able to obtain the next outgoing serial رقم الصادر
التالي, so that the dispatch I am about to register is numbered in order.

## Acceptance Criteria

1. Given a head-office staff member with an active session in the module, when the actor opens
   the screen with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/IncomingOutgoing/outgoing/next-serial` and the response is rendered on the screen
   without a page reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the scenario of §21.U.12 passes end to end; the sequence is scoped per
charity and year; the scoping is enforced server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Repo | `OutgoingRepository.GetNextSerialNumberAsync` | Exists — `Max(Serial)+1` per year (no charity key) |
| API | `IncomingOutgoingController` `GET outgoing/next-serial` | Exists |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

Identical defect set to 16-3 on the outgoing side:

1. **Scope lacks the charity key** — `Max+1` per year only; with 16-10's charity dimension the
   sequence must be per charity+year, caller-derived.
2. **`OUT-`-style English serial text** if the service formats one (incoming had `INC-`); the
   رقم الصادر must be a neutral/Arabic-friendly zero-padded number.
3. **Same `Max+1` race window** — advisory endpoint; 16-13 re-derives inside the create save.
4. **No SPA consumer** — add `getNextOutgoingSerial(year?)` to `outgoing.service.ts` for 16-13.

## Tasks / Subtasks

- [x] **Task 1 — Sequence semantics** (AC 2–4): scope by charity+year from `ICurrentUserService`
        (HQ may pass explicit `charityId`); plain zero-padded serial text; response
        `{ serial, serialTxt }`
- [x] **Task 2 — SPA exposure** (AC 2): `outgoing.service.ts.getNextOutgoingSerial(year?)`
- [x] **Task 3 — Verification**: live — max+1 for the caller's charity+year; other charities'
        letters don't advance it; unauthenticated → 401; `dotnet build` + `npm run build` — 0
        errors; tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

Same as 16-3: serial is data not identity; advisory-only contract documented for 16-13; bare
value returns.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Rendering + persisting the serial on the outgoing form | 16-13 |
| Serial in the report query (سنه الخطاب / رقم الخطاب filters) | 16-19 |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.12] scenario
- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.S.5] رقم الصادر —
  mandatory · read-only
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-12 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-16-correspondence-incoming-and-outgoing/16-3-obtain-the-next-incoming-serial.md] mirrored
  defect classes + tasks

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): verified `OutgoingService.GetNextSerialAsync` (:237-246) — `effectiveCharity = _currentUser.CharityId ?? charityId` pin (added this pass), charity+year-scoped `Max+1`, plain `ToString("D4")` serial text. Advisory-only contract documented in the form's code comment; `CreateAsync` re-derives inside the save (:125-126).
- 2026-08-24 (verification): project builds clean; `ng build` clean for the module.

### Completion Notes List

- Exact mirror of 16-3's landed shape: sequence key charity+year, `{ serial, serialTxt }` bare response, HQ-only `charityId` param that a charity caller's claim overrides.
- SPA: `outgoing.service.ts` `getNextSerial(year?, charityId?)` → `GET /api/IncomingOutgoing/outgoing/next-serial`; the create form binds the read-only رقم الصادر via `nextSerialTxt` (16-13).
- Naming deviation (cosmetic): method is `getNextSerial`, not the story-suggested `getNextOutgoingSerial` — same contract.
- Task 3's live portion pending the user's `IIROSA.Api` restart; builds verified. Tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Application/Services/OutgoingService.cs` — serial derivation + charity pin
- `Backend/src/IIROSA.Infrastructure/Data/Repository/OutgoingRepository.cs` — charity+year `Max+1`
- `Frontend/src/app/modules/incoming-outgoing/services/outgoing.service.ts` — `getNextSerial`
- `Frontend/src/app/modules/incoming-outgoing/outgoing-letters/outgoing-letter-form.component.ts` — read-only serial binding

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-12 and module spec §21.U.12; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (charity pin on the advisory read verified; mirror of 16-3). Status → review; live check pending user's API restart. |
