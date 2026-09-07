# Story 2.2: View statistical breakdown charts — الرسوم البيانية

| Field | Value |
| --- | --- |
| Story | US-DSH-02 (UC-DSH-02) |
| Epic | EP-02 — Home Dashboard (chapter 7 · §7.U.2) |
| Priority / size | Could · 3 points |
| Route | `#/dashboard` (charts section of the screen 2-1 re-cut) |
| Endpoint | `GET /api/Dashboard/charts?dimension=&charityId=` (**extends the existing** `DashboardController`) |
| Depends on | 2-1 (owns the screen re-cut, the summary service method and the HQ charity filter state this story rides) |
| Roles | All roles — SuperAdmin, Admin, Charity, Accountant, FinancialOfficer (no role gate; claims scoping only) |

Status: ready-for-dev

## Story

As a signed-in user,
I want to view statistical breakdown charts الرسوم البيانية of the beneficiary population,
so that I can see its composition at a glance within my own scope.

## Acceptance Criteria

1. Given a signed-in user on `#/dashboard`, when the charts load, then `GET /api/Dashboard/charts?dimension=&charityId=` returns `{ dimension, items: [{ label, labelEn?, value }], message? }` for the requested dimension, **scoped server-side exactly like the summary** (charity claim pins, HQ `charityId` narrows, country claim intersects, claim-less non-HQ token → 403). Read only.
2. Given an unknown/unsupported `dimension`, then 400 with a field-map error naming the valid dimensions (FluentValidation-style `{ errors }` shape the platform uses for field maps) — never a 500.
3. Given a valid dimension with no rows in scope, then `items: []` plus a message — the chart area renders an empty state, not an error.
4. Given the supported dimensions, then the server registry is exactly: `familyType` (families by نوع الأسرة), `gender` (orphans by النوع), `sponsorshipStatus` (orphans by حالة الكفالة), `educationLevel` (orphans by المؤهل الدراسي, labels from the lookup's NameAr/NameEn), `healthStatus` (orphans by الحالة الصحية, same), and `charity` (families by الجمعية — **HQ-only**; a charity-role caller requesting it gets 400, since a one-slice pie is meaningless).
5. Given the screen, then the five invented demo charts (Monthly Statistics / Financial Overview / Family Growth / Budget Allocation / Performance — all fake English-labelled series) are **deleted** and replaced by donut charts driven by the dimension registry, rendered through the existing `ApexChartComponent`, with chart titles from i18n (ar + en) and Arabic labels displayed RTL.
6. Given the HQ charity dropdown from 2-1, then changing it re-fetches the visible charts without a page reload; a charity user's charts are its own data only.
7. Given the session has expired or the token is invalid, then 401 → SPA routes back to login.

**Definition of done:** §7.U.2 passes end to end; scoping enforced server-side; no hardcoded UI strings; apexcharts renders from server data only (no client-side invented numbers remain on the screen); ar/en keys added; builds green.

## What exists already

| Layer | State (working tree, 2026-08-26) |
| --- | --- |
| Controller / Service | `DashboardController` + `DashboardService` live since 18-32 (see 2-1's table). This story adds `GetChartDataAsync` beside `GetSummaryAsync` (2-1). |
| Chart infra | `apexcharts ^4.7.0` installed; `ApexChartComponent` wrapper live in `shared/components` and already used by `DashboardComponent` (donut demo options exist at `dashboard.component.ts:144-170` as a style reference). The component's own comment (`:43-45`) records the pitfall: hand `apx-chart` a **stable options object** — a fresh object per change-detection pass forces an ApexCharts re-render each cycle. Build options once per data load. |
| Data | `Family.FamilyType` enum (wire = string per 6-1), `Orphan.Gender` (string), `Orphan.SponsorshipStatus` (string), `Orphan.EducationLevelId`/`HealthStatusId` (int lookups — education-levels + health-statuses lookup stacks are live from epic 8, `LookupEntity` with NameAr/NameEn), `Family.FK_CharityId` → `Charity.Name` for the HQ dimension. |
| i18n | `dashboard.*` block in `ar.json`/`en.json` — add chart/dimension keys beside 2-1's. |
| To delete | The five demo chart option blocks + their hardcoded `<apx-chart>` cards in `dashboard.component.html/ts` (2-1 removes the cards/lists; this story removes the charts). |

## Tasks / Subtasks

- [ ] Task 1 — Dimension registry + `GetChartDataAsync` (AC: 1-4)
  - [ ] `DashboardChartDto { Dimension, Items: List<ChartItemDto>, Message? }`, `ChartItemDto { Label, LabelEn?, Value }` beside `DashboardSummaryDto`
  - [ ] A private dimension registry in `DashboardService`: dimension key → (entity set, grouping selector, label resolver). Grouped counts over `TableNoTracking`, `!IsDeleted`, scoped by the same ladder as `GetSummaryAsync` (claim pin → HQ narrow → country pin → `FailClosedWhenUnscoped`)
  - [ ] Lookup dimensions resolve labels via a single lookup read (`EducationLevels` / `HealthStatuses`), mapping id → `NameAr`/`NameEn`; unmapped ids are skipped (soft-deleted lookup rows), not rendered as raw ids
  - [ ] `charity` dimension: HQ callers only — charity-role request → throw the platform's 400 field-map exception (`ValidationException` pattern) naming the valid dimensions
  - [ ] Unknown dimension → the same 400 field map (AC 2)
- [ ] Task 2 — `GET /api/Dashboard/charts` endpoint (AC: 1, 2, 7)
  - [ ] On the existing `DashboardController`: `[HttpGet("charts")]`, `[Authorize]`, `[FromQuery] string dimension, [FromQuery] Guid? charityId`
  - [ ] Controller error handling mirrors the existing actions: `UnauthorizedAccessException` → `Forbid()`, validation field map → 400, everything else → logged 500 `{ message }`
- [ ] Task 3 — Frontend charts re-cut (AC: 5, 6)
  - [ ] Delete the five demo `*ChartOptions` blocks and their cards; render a responsive grid of donut charts (default dimensions: `familyType`, `sponsorshipStatus`, `gender`, `educationLevel`; `healthStatus` and `charity` join via a dimension selector visible to the right roles — `charity` offered to HQ only)
  - [ ] `dashboard.service.ts` gains `getCharts(dimension, charityId?)`; options rebuilt **only on data arrival** (stable reference between CD passes — see the component's own comment)
  - [ ] Donut options follow the existing dark-theme styling (`theme.mode: 'dark'`, transparent background, bottom legend) with Arabic labels; empty state per AC 3
- [ ] Task 4 — i18n ar + en: chart titles per dimension (`dashboard.charts.familyType` …), selector label, empty-state key
- [ ] Task 5 — Verify (AC: 1-7): `dotnet build` + `npm run build` green; live smoke on a private spare-port instance: each dimension 200 in HQ scope / HQ narrowed by charity / Charity pinned scope; unknown dimension 400; `charity` dimension as Charity role 400; empty scope → `items: []` + message; claim-less token 403

## Dev Notes

### Platform rules that bind this story

- Same as 2-1: extend the existing controller/service (R2, 18-32), raw envelope + `{ message }` errors, claims tenancy server-side, `TableNoTracking` reads only, no manual DI line (assembly convention registers `DashboardService`).
- Lookup labels are bilingual (`NameAr`/`NameEn`) — send both in `ChartItemDto` and let the client pick by current language; enum-ish strings (`familyType`, `gender`, `sponsorshipStatus`) return their stored wire strings, and the **client maps them to i18n keys** (never display raw English enum values in the Arabic UI).
- One EF query per dimension (grouped server-side); no client-side aggregation of entity rows; no N+1 lookup reads — batch the label resolution.

### Story-specific rulings

- Legacy realisation (`IFamilyService.GetDataForBieChart` — "Bie" sic) is a legacy artifact; this stack's home is `DashboardService` (18-32 precedent).
- The legacy screen rendered **pie** charts — donut is the accepted modern rendering of the same contract (ApexCharts pie/donut family); bar/line/area/radar from the demo template have no UC-DSH basis and are deleted, not kept.
- Free-string dimensions (القريه `Family.CityVillage`, العنوان) are **excluded** — grouping on unnormalised free text produces garbage slices; recorded as out of scope, not silently dropped.
- Country dimension (`Family.CountryId`) is not in the registry: the population is effectively single-country per deployment; revisit only on demand.

### Out of scope (do not build)

| Item | Story |
| --- | --- |
| Summary counts, cards, grid, export | 2-1 |
| Menu/permission fixes | 2-3 |
| Chart image export / printing | not in UC-DSH scope (epic-18 owns print) |
| Payment figures on the dashboard | 10-21 / `payment-summary` (exists) |

### References

- [Source: docs/Modules/07-UC-DSH-Home-Dashboard.md#7.U.2]
- [Source: _bmad-output/planning-artifacts/epics.md#32-ep-02—home-dashboard] (AC 1-5)
- [Source: Frontend/src/app/modules/dashboard/dashboard.component.ts:43-45,144-170] (stable-options pitfall; donut style reference)
- [Source: Backend/src/IIROSA.Application/Services/DashboardService.cs:89-118] (scope ladder to copy)
- [Source: Backend/src/IIROSA.Domain/Entities/Orphan.cs] · [Family.cs] (dimension columns)

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-26 — created (EP-02 context pass) → ready-for-dev.
