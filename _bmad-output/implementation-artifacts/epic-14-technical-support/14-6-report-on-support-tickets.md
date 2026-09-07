# Story 14-6: Report on support tickets

| Field | Value |
| --- | --- |
| Story key | `14-6-report-on-support-tickets` |
| Epic | EP-14 — Technical Support |
| Use case | UC-CST-06 — تقارير الدعم الفني |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/19-UC-CST-Technical-Support.md` |
| Route | `#/technical-support/reports` (`SupportReportComponent`) |
| Endpoint | `POST /api/SupportTickets/report` (board note says GET — actual controller action is POST with `{startDate, endDate}`) |
| Depends on | 14-1 (list — report links back), backend report action (exists) |

## Status

done

> Review completed 2026-08-23 — both scoped patches applied (period-scoped status tiles,
> CSV formula-injection neutralization; the NaN guard already existed). Frontend build green.

## Story

As a General Director, I want a report on support tickets over a period, so that I can see volumes,
resolution performance and per-status/category/creator breakdowns.

## Acceptance Criteria

1. Given an admin selects a date range and generates, when the report is served, then summary
   statistics (total / solved / unsolved / per-status counts), SLA metrics, and the
   by-status / by-priority / by-category / by-creator breakdowns render from
   `POST /api/SupportTickets/report`.
2. Given the generated report, when the actor exports, then a CSV of the ticket list downloads
   (client-side — the API has no export endpoint) and the browser print dialog serves the print
   action.
3. Given a non-admin session, when the report endpoint is invoked, then the request is rejected
   server-side (`[Authorize(Roles = "SuperAdmin,Admin")]`).
4. All screen strings resolve in both `ar.json` and `en.json` (no hard-coded UI text).

## What this story fixed (2026-08-23 review-and-complete pass)

The copied screen targeted a **phantom report shape**: it bound `report.summaryStatistics.*`,
`report.performanceMetrics.*`, `report.userStatistics[]` and sent `groupBy` / `includeCategories` /
`includeUsers` options the backend DTO (`SupportReportRequestDto`) never had. The real
`SupportTicketReportDto` is flat: `totalTickets`, `openTickets`, `inProgressTickets`,
`resolvedTickets`, `closedTickets`, `solvedTickets`, `unsolvedTickets`, dictionaries
`ticketsByStatus/Priority/Category/Creator`, `averageResolutionTimeHours`,
`ticketsResolvedWithinSLA`, `ticketsBreachedSLA`, `startDate`, `endDate`, `tickets[]`.

Changes:

- `support-report.component.ts` — request reduced to `{startDate, endDate}`; CSV export
  implemented client-side from `report.tickets`; `printReport` kept; dictionary helpers for the
  breakdown tables. `RouterModule` import added (the template's breadcrumb links never worked).
- `support-report.component.html` — all bindings rewritten to the flat wire DTO; group-by and
  include-* controls removed (no backend counterpart); sections added: by-status, by-priority,
  by-creator, per-ticket table.
- Route order fixed in `technical-support-routing.module.ts`: `'reports'` was registered **after**
  `':id'`, so `#/technical-support/reports` rendered the detail screen with id `"reports"`.

## Wire-name note (serializer)

`Program.cs` calls **`.AddNewtonsoftJson()`**, which replaces the System.Text.Json formatter.
Two consequences this pass corrected elsewhere in the module and that hold here:

- Newtonsoft's camelCase keeps a trailing acronym uppercase: the DTO properties
  `TicketsResolvedWithinSLA` / `TicketsBreachedSLA` serialize as `ticketsResolvedWithinSLA` /
  `ticketsBreachedSLA` — **not** `…WithinSla`.
- The frontend model must mirror the flat DTO exactly (`technical-support.model.ts` → `SupportReport`).

## Dev Agent Record

### Agent Model Used

claude-code (glm-5)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — 0 errors (backend untouched by this story; verified during the
  same pass as 14-4)
- `npm run build` (Frontend) — see 14-4 story record for the same run; this story's changes are
  frontend-only

### Completion Notes List

- Story file created **and** implemented in the same epic-14 review-and-complete pass
  (user instruction 2026-08-23: complete 14-1 through 14-6); recorded here rather than as a
  separate dev cycle.
- Tests EXCLUDED by standing user decision (no `Backend/tests` project; same as epics 3, 13, 14-4).
- The board note "GET /api/SupportTickets/report" is wrong — the action is `HttpPost("report")`.
  Board comment corrected at finalization.

### File List

- `Frontend/src/app/modules/technical-support/support-report/support-report.component.ts` (rewritten)
- `Frontend/src/app/modules/technical-support/support-report/support-report.component.html` (rewritten)
- `Frontend/src/app/modules/technical-support/technical-support-routing.module.ts` (route order)
- `Frontend/src/app/core/models/technical-support.model.ts` (wire `SupportReport`; shared with 14-4)
- `Frontend/src/app/modules/technical-support/services/technical-support.service.ts` (`generateReport`; shared)
- `Frontend/src/assets/i18n/ar.json`, `en.json` (reports.* + validation/common additions; shared)

### Change Log

| Date | Change |
| --- | --- |
| 2026-08-23 | Story file created + implemented in the epic-14 review-and-complete pass; phantom report shape replaced with the real `SupportTicketReportDto` wire; route-order defect fixed. |

### Review Findings

_Code review 2026-08-23 — scope: all epic-14 changes. This story's scoped findings below;
module-wide findings (2 decision · 15 other patches · 1 defer · 14 dismissed) are recorded in
`14-4-update-a-support-ticket.md` and `review-artifacts/epic14-review-report.md`._

- [x] [Review][Patch] Report status tiles are global all-time counts, not period-scoped — compute from the period tickets [SupportTicketService.cs:388-391]
- [x] [Review][Patch] CSV export lacks formula-injection neutralization (=, +, -, @); getPercentage returns NaN when totalTickets is 0 [support-report.component.ts]
