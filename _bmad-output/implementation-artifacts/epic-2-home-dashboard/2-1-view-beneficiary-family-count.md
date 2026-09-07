# Story 2.1: View beneficiary family count — عدد الأسر

| Field | Value |
| --- | --- |
| Story | US-DSH-01 (UC-DSH-01) |
| Epic | EP-02 — Home Dashboard (chapter 7 · §7.U.1) |
| Priority / size | Should · 2 points |
| Route | `#/dashboard` |
| Endpoint | `GET /api/Dashboard/summary` (**extends the existing** `DashboardController`) |
| Depends on | EP-01 platform auth (live: JWT + claims via `ICurrentUserService`); reuses `GET /api/Families/orphans` (epic 8) and `GET /api/Charities` (epic 3) |
| Roles | All roles — SuperAdmin, Admin, Charity, Accountant, FinancialOfficer (no role gate; claims scoping only) |

Status: ready-for-dev

## Story

As a signed-in user,
I want to view the beneficiary family count عدد الأسر on the home screen,
so that I immediately see the size of the beneficiary population visible to me — my charity's families for a charity account, the whole population for HQ.

## Acceptance Criteria

1. Given a signed-in user on `#/dashboard`, when the screen loads, then `GET /api/Dashboard/summary?charityId=` returns the counts **scoped server-side to the caller** — a charity claim pins to that charity, an HQ caller may narrow with an explicit `charityId`, a country claim intersects, and a claim-less non-HQ token is refused 403 (fail-closed). No stored data changes — read only.
2. Given the response, then it carries `{ familiesCount, orphansCount, charitiesCount?, charityName?, message? }` (raw envelope, no `ApiResponse<T>`) — `charitiesCount` is populated only for HQ callers viewing across charities; an empty scope returns zeroed counts plus a message, never an error.
3. Given a charity user, then the الجمعية dropdown is **not rendered** and the counts are its own charity's only (a `charityId` query param from a charity caller is ignored — pin never widens).
4. Given an HQ user, then the الجمعية dropdown (options from `GET /api/Charities`, plus كافة الجهات = no filter) re-fetches summary and grid without a page reload.
5. Given the screen contract §7.S.1, then the stat cards render the live counts (عدد الأسر is the headline card) and the demo's invented cards (activeMissions, recentActivities, upcomingEvents) are gone; every visible string is an i18n key (ar + en).
6. Given §7.S.1's grid, then a beneficiaries grid lists orphans in scope via the **existing** `GET /api/Families/orphans` (HQ passes its dropdown `charityId`), with the shared `data-list` component and the §7.S.1 columns it supports (م · الكود · الاسم · السن · رقم التليفون · الجمعية · الحاله — القريه/العنوان are not on `OrphanLookupDto`; see deviation), and تحميل التقرير exports the current grid page client-side via ExcelJS.
7. Given the session has expired or the token is invalid, then the request is rejected 401 and the SPA routes back to login (existing auth interceptor).

**Definition of done:** §7.S.1 fields with their lookups implemented; §7.U.1 passes end to end; the role and charity scoping is enforced server-side, not only in the UI; ar/en keys added; backend and frontend builds green.

## What exists already

| Layer | State (working tree, 2026-08-26) |
| --- | --- |
| Controller | `Backend/src/IIROSA.Api/Controllers/DashboardController.cs` — **live since 18-32** (`payment-summary` endpoint). `ControllerBase`, `[Authorize]`, raw envelope + anonymous `{ message }` errors, `UnauthorizedAccessException` → `Forbid()`. Epic 2 EXTENDS it — never re-found, never converted to `ApiController` (architecture ruling R2). |
| Service | `IDashboardService` / `DashboardService` (`Application/Services/`) — live since 18-32 with the claims scope ladder and `FailClosedWhenUnscoped()` (claim pin → HQ narrow → country pin → fail-closed, `DashboardService.cs:89-118`). DI by assembly convention — classes ending `Service` are auto-registered; **no manual DI line**. |
| Frontend module | `modules/dashboard/` is registered in `app-routing.module.ts` (`'' → /dashboard` redirect at :20, lazy route at :26, `**` wildcard → /dashboard at :122). `DashboardComponent` is a **static demo**: hardcoded `stats` (`totalCharities: 152` …), fake charts, fake activities — all to be replaced by this story (cards/grid) and 2-2 (charts). |
| Data | Everything needed is live: `Family` (scope on `FK_CharityId`, `!IsDeleted`), `Orphan` (`FK_CharityId`, counts), `Charity`; `GET /api/Families/orphans` (epic 8, `OrphanSearchFilterDto.CharityId` = HQ narrowing, paged envelope); `GET /api/Charities` (epic 3). |
| Shared components | `shared/components/data-list` (mandatory for listings), `ApexChartComponent` (`shared/components`), ExcelJS + file-saver already in `package.json` (`:33-34`). |
| i18n | `dashboard.*` block exists in `ar.json` (:112 — title/totalFamilies/totalOrphans/totalCharities…); verify/add the same keys in `en.json`. |

## Tasks / Subtasks

- [ ] Task 1 — `GetSummaryAsync` on the dashboard service (AC: 1, 2)
  - [ ] `DashboardSummaryDto` in `Application/DTOs/Reports/` (beside `PaymentSummaryDto` — the dashboard DTO home): `FamiliesCount`, `OrphansCount`, `CharitiesCount?`, `CharityName?`, `Message?`
  - [ ] `Task<DashboardSummaryDto> GetSummaryAsync(Guid? charityId = null, CancellationToken ct = default)` on `IDashboardService` + `DashboardService` — three `TableNoTracking` counts (`Family`/`Orphan` on `FK_CharityId`, `!IsDeleted`; charities count only for unscoped HQ callers)
  - [ ] Scope ladder **copied verbatim** from `GetPaymentSummaryAsync` (`DashboardService.cs:89-118`), including `FailClosedWhenUnscoped()` → `UnauthorizedAccessException` → controller maps to 403
- [ ] Task 2 — `GET /api/Dashboard/summary` endpoint (AC: 1, 2, 7)
  - [ ] Add to the **existing** `DashboardController`; `[HttpGet("summary")]`, `[Authorize]` (all roles — no `[Authorize(Roles=…)]`); `[FromQuery] Guid? charityId`
  - [ ] Error shape identical to the controller's existing action: log + `StatusCode(500, new { message })`; `catch (UnauthorizedAccessException) → Forbid()` — one error shape per controller (R1)
- [ ] Task 3 — Frontend summary load (AC: 3, 4, 5)
  - [ ] `modules/dashboard/dashboard.service.ts` — `getSummary(charityId?)`, `getOrphans(filter)` riding `HttpClient` (raw envelope, `responseType` json)
  - [ ] Replace the hardcoded `stats`/`recentActivities`/`upcomingEvents` demo fields with the live summary; `OnPush` + `ChangeDetectorRef` on the touched component; loading / empty / error states (loadFailed key)
  - [ ] HQ-only الجمعية dropdown fed by `GET /api/Charities` (reuse the families-list filter pattern); selecting كافة الجهات clears the param; charity callers see a static scope label instead
- [ ] Task 4 — §7.S.1 grid + export (AC: 6)
  - [ ] `data-list` over `GET /api/Families/orphans` (`Search` left empty, `CharityId` = HQ selection, paged envelope `Items/TotalCount`); columns م/الكود/الاسم/السن/رقم التليفون/الجمعية/الحاله from `OrphanLookupDto`
  - [ ] تحميل التقرير = client-side ExcelJS export of the loaded page (epic-9/16 convention — server never returns bytes for extracts)
- [ ] Task 5 — i18n ar + en (AC: 5): keep `dashboard.title/totalFamilies/totalOrphans/totalCharities`; add keys for charity filter (كافة الجهات all-charities), grid headers, export command, loadFailed; remove keys for the deleted demo blocks only if nothing else references them
- [ ] Task 6 — Verify (AC: 1-7): `dotnet build` 0 errors in touched projects (MSB3021/3027 copy-lock from the live API is expected, not a code error); `npm run build` green for the dashboard chunk; live smoke on a private `--no-build --urls 127.0.0.1:<spare>` instance: HQ unscoped / HQ narrowed / Charity pinned / country-claim intersect / claim-less token 403 / empty scope zeros

## Dev Notes

### Platform rules that bind this story

- Extend, don't found: `DashboardController` + `IDashboardService` exist (18-32). Adding a second dashboard controller/service, or moving `payment-summary`, is a regression.
- Raw envelope + anonymous `{ message }` errors here — NOT `ApiResponse<T>` (15-1 ruling as applied in 18-32; architecture R1/R2).
- Claims tenancy server-side via `ICurrentUserService` (`CharityId` / `IsHeadOffice` / `CountryId`); the `charityId` query param is an HQ-only narrow, ignored when a charity claim exists.
- Read-only story: `TableNoTracking` only; `IUnitOfWork` untouched; no validators needed (no write payload).
- Bilingual labels: any dimension/lookup text rendered from data uses `NameAr`/`NameEn`; UI strings only via `ar.json`/`en.json`. RTL-first.
- The scope column is `FK_CharityId` — `Family.CharityId` is a legacy twin written in tandem by 5-6; new reads bind `FK_CharityId` (the 18-32/epic-5 convention).

### Story-specific rulings

- The epics traceability row (`IFamilyService.GetFamiliesCount`) is a **legacy artifact** — on this stack dashboard reads live in `DashboardService` (18-32 precedent). Do not add count methods to `IFamilyService`.
- The demo's `activeMissions` card and the recentActivities/upcomingEvents lists are template inventions with no UC-DSH use case — deleted by this story, not preserved.
- §7.S.1 lists القريه/العنوان grid columns; `OrphanLookupDto` carries neither (FamilyCode is the only family reference). Render the columns the DTO has; the omission is a recorded deviation (extending the epic-8 contract for two dashboard columns is not worth the coupling).
- `GET /api/Charities` admits the Charity role too (scoped to its own record) — harmless for the dropdown; the dropdown itself is HQ-only regardless.

### Out of scope (do not build)

| Item | Story |
| --- | --- |
| Statistical charts (dimensions, donut grid) | 2-2 |
| Menu/permission gating fixes (`main-layout.hasPermission` stub, dead links) | 2-3 |
| `payment-summary` pagination / covers | 10-21 (reuse note recorded there) |
| SignalR dashboard hub (architecture §6.3 mention) | not in UC-DSH scope |

### References

- [Source: docs/Modules/07-UC-DSH-Home-Dashboard.md#7.U.1] · [#7.S.1]
- [Source: _bmad-output/planning-artifacts/epics.md#32-ep-02—home-dashboard] (AC 1-5)
- [Source: _bmad-output/planning-artifacts/architecture.md#rulings] (R1 raw envelope, R2 `DashboardController : ControllerBase`)
- [Source: Backend/src/IIROSA.Application/Services/DashboardService.cs:89-118] (scope ladder + fail-closed to copy)
- [Source: Backend/src/IIROSA.Application/DTOs/Family/OrphanCodingDtos.cs:8-65] (`OrphanSearchFilterDto` / `OrphanLookupDto`)

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-26 — created (EP-02 context pass) → ready-for-dev.
