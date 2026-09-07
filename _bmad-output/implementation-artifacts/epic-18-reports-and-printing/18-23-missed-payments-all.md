# Story 18-23: Missed payments — all

| Field | Value |
| --- | --- |
| Story key | `18-23-missed-payments-all` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-23 — جميع الدفعات الفائتة |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.11 screen — toggle command, §23.U.23 scenario) |
| Route | hosted on `#/reports/missed-payments` (the screen landed by 18-22 — no new route) |
| Endpoint | `POST /api/Reports/missed-payments` (all-scope variant — request flag `AllOrphans`) |
| Depends on | **18-22 landed** (screen, DTO, service query, validator) |
| Roles | Gen. Director → `SuperAdmin`, `Admin` (permission `Reports.View`) |

## Status

done

## Story

As a General Director, I want to be able to missed payments — all جميع الدفعات الفائتة, so that I
can see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a General Director with an active session on `#/reports/missed-payments`, when the actor
   presses the all-orphans toggle (the §23.S.11 icon command), then no stored data is changed — the
   operation is a read-only projection across charities.
2. Given the toggle is on, when the report is served, then the same endpoint
   `POST /api/Reports/missed-payments` runs with `allOrphans: true` and returns the
   organisation-wide arrears view — every charity the caller's country claim permits — rendered on
   the screen without a page reload.
3. Given the caller is a charity user, when the widened scope is requested (toggle UI or a forged
   payload), then the request is refused server-side with a `message` — the pinned
   `ICurrentUserService.CharityId` scope is never widened. The client toggle is never trusted.
4. Given an HQ caller passes an explicit charity id together with the toggle, when the report is
   served, then the explicit charity id wins — the toggle widens only the no-filter case.
5. Given no orphan qualifies, when the report runs, then the grid renders its empty state and the
   paging control reports zero pages.
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.

**Definition of done:** the §23.S.11 icon command `GetDataByAllOrphanCheckBox()` is wired and the
scenario of §23.U.23 passes end to end; the widened scope is HQ-only and enforced server-side, not
only by hiding the toggle.

## Tasks / Subtasks

- [x] **Task 1 — DTO flag** (AC 2)
  - [x] Add `bool AllOrphans = false` to `MissedPaymentsReportFilterDto` in
        `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` (18-22's DTO — one endpoint, two
        scope variants; no new DTO type)
  - [x] `MissedPaymentsReportValidator` — no new mandatory rules; the flag is a plain bool
- [x] **Task 2 — Service scope branch** (AC 2, 3, 4)
  - [x] In `ReportService.GetMissedPaymentsAsync` (`Backend/src/IIROSA.Application/Services/ReportService.cs`),
        before the query: if `AllOrphans` and the caller is NOT `IsHeadOffice`, throw
        `UnauthorizedAccessException` (scope refused — pin-never-widen); if `AllOrphans` and HQ,
        skip the charity pin but keep the `CountryId` claim pin; if an explicit `CharityId` is
        present it wins over the flag
  - [x] Read path unchanged otherwise — 18-22's projection, still no writes, still through
        `IUnitOfWork` repositories
- [x] **Task 3 — Controller refusal ladder** (AC 3, 6)
  - [x] `catch (UnauthorizedAccessException)` → `Forbid()` in `ReportsController`'s
        `missed-payments` action (the exact `OrphanPaymentsController.cs:136` shape), ahead of the
        catch-all → 500 `{ message }`. No `ApiResponse<T>`
- [x] **Task 4 — Toggle command** (AC 1, 2, 5)
  - [x] Wire the §23.S.11 icon-only command on 18-22's
        `missed-payments.component.{ts,html,scss}`: toggles `allOrphans`, re-fetches, and reflects
        the state on the icon (active/inactive). Rendered only for HQ callers via the
        `auth.service.ts` role check — convenience only; the endpoint's refusal is the control
  - [x] Charity callers never see the toggle and their requests never widen; empty state and
        shared `Pagination` behave as in 18-22
- [x] **Task 5 — i18n** — toggle tooltip/title, active/inactive labels, refusal message under
      `reports.missedPayments.*` in **both** `assets/i18n/ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–6)
  - [x] Live check: charity token with `{"allOrphans":true}` → 403; HQ with the flag →
        cross-charity rows (subject to the country claim); HQ + `charityId` + flag → that charity
        only; toggle off → 18-22 behaviour unchanged; anonymous → 401. Arabic payloads from UTF-8
        files
  - [x] `dotnet build Backend/IIROSA.sln` + `cd Frontend && npm run build` green (MSB3021/3027
        live-API lock caveat — never kill the user's process; ng-serve stale-bundle grep caveat)
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Platform rules that bind this story

- Never trust the client: scope widens only for `IsHeadOffice`, decided in the service from
  `ICurrentUserService` — never from the payload or the UI toggle.
- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10); controllers inherit `ControllerBase` + `[Authorize]`.
- camelCase wire; no `FK_*` DTO keys; FluentValidation in the service; repositories never save;
  global soft-delete query filter.
- Bespoke grid + shared `Pagination` (NOT `data-list`); list screens omit OnPush (codebase
  precedent); EP-18 has no new entities and no EF migration.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| صور الأيتام / صور الشهادات export screen `#/reports/orphan-files` | 18-24 / 18-25 |
| PDF rendering (`services/report-pdf.service.ts`) | 18-21 |
| Generic Excel-export engine beyond the shell's row dump | 18-41 |
| Any per-batch narrowing beyond the batch columns 18-22 already renders | — (not in epic scope) |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.11] the `GetDataByAllOrphanCheckBox()`
  icon command this story wires
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.23] scenario — organisation-wide
  arrears view, HQ-only
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-23 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:136]
  `UnauthorizedAccessException` → `Forbid()` ladder to copy
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-22-missed-payments-current-user-scope.md] the
  screen, DTO and query this story widens

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- `dotnet build` (Api csproj, temp `-o C:/Users/oabdelaziz/AppData/Local/Temp/iirosa-1823`) — **0 errors**.
- `npx ng build` — NG_EXIT=0, `error TS` count 0.
- i18n `reports.missedPayments.*` grew to 17 keys each locale (+allOrphansTooltip/Active/Inactive), key sets identical, JSON parses.
- Live smoke, private instance `127.0.0.1:60970`, seeds `18230000-…` (batches SB-1823-A/B; 5 items — 2 dga arrears + 1 wadi arrears):
  - anonymous POST → **401**
  - charity token + `{"allOrphans":true}` → **403** (AC 3 — forged/toggled payload refused server-side, `UnauthorizedAccessException` → `Forbid()` ladder)
  - HQ + flag → 200 **cross-charity**: 3 rows — 2 dga + 1 وادي النطرون, per-batch states intact
  - HQ + flag + explicit وادي النطرون → **1 row** (AC 4 — the explicit id wins over the flag)
  - HQ without the flag → same 3 rows (HQ's no-filter view was already cross-charity within the country pin — see Completion Notes); charity without the flag → unchanged 18-22 behaviour
  - seeds hard-deleted (0/0); smoke instance killed by PID (24524).

### Completion Notes List

- **What the flag actually changes (recorded):** for HQ, 18-22's no-filter view is *already* cross-charity within the country pin — the `AllOrphans` flag's operative effects are (a) the **non-HQ refusal** (toggle UI or forged payload → 403, never a widened read) and (b) the explicit-`CharityId`-wins rule on the wire. The toggle is UI state + the server-side guard, not a different query.
- Refusal ladder: `UnauthorizedAccessException` in the service (before the scope ladder) → `Forbid()` in the controller (the `OrphanPaymentsController` shape), ahead of the catch-all 500.
- The toggle renders HQ-only (`hasAnyRole(['SuperAdmin','Admin'])` — the same gate as the charity dropdown); its click toggles state, resets to page 1, re-fetches, and reflects state on the icon (people-fill vs people). Client-side hiding is convenience — the 403 is the control (proven live).
- Tests excluded per the standing user decision.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `MissedPaymentsReportFilterDto.AllOrphans` (one endpoint, two scope variants; no new DTO)
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — the refusal branch ahead of 18-22's scope ladder
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `catch (UnauthorizedAccessException)` → `Forbid()` in the missed-payments action
- `Frontend/src/app/modules/reports/models/report.model.ts` — `MissedPaymentsFilter.allOrphans?`
- `Frontend/src/app/modules/reports/missed-payments/missed-payments.component.ts` — `allOrphans` state, `toggleAllOrphans()`, filter wiring
- `Frontend/src/app/modules/reports/missed-payments/missed-payments.component.html` — the §23.S.11 icon command (HQ-only)
- `Frontend/src/assets/i18n/ar.json` / `en.json` — `reports.missedPayments.*` +3 keys each

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-23 and module spec §23.S.11 / §23.U.23; HQ-only widened-scope refusal ruled server-side (client toggle never trusted); shares 18-22's screen and endpoint by design. |
| 2026-08-24 | Implemented and verified: `AllOrphans` flag with the service-side refusal (non-HQ → 403), explicit-charity-wins rule, HQ-only icon toggle on 18-22's screen, i18n both locales. Matrix proven live (401, 403 refusal, cross-charity rows, explicit narrow over flag, unchanged default). Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
