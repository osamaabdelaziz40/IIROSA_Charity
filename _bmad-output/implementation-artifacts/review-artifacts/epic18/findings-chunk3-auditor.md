# Epic-18 review — chunk 3 (screens 18-3…18-19 slice) — Acceptance Auditor findings

**Coverage note:** this chunk contained implementations only for 18-7/18-8 (meza-cards), 18-10 (campaign-report), 18-13 (backend + client service) and 18-14 (backend + orphan-reports-list). Screens for 18-3…18-6, 18-9, 18-11/12, 18-15…18-19 were reviewed in chunk 4 (their reads live in ReportsController, chunk 1). Verified compliant here (no finding): 18-13 AC2/AC3/AC5, 18-8 date-range rule, 18-18 AC3 refusal-reason requirement, 18-14 AC2/AC3 + AC5, 18-10 AC3 (`[Authorize(Roles="SuperAdmin,Admin")]`, no charity leak).

### Findings

- **18-14 — shipped `OrphanReportsListComponent` fully rewritten instead of "audited, not rewritten"** — violates 18-14 DoD ("the status-grouped counts render on the shipped OrphanReportsListComponent (audited, not rewritten)"). The diff deletes the Quick Actions cards (`navigateToGenerate/History/Schedule/Compare`), the Recent Reports card + `app-empty-state`, and the page-header add button; the `.ts` deletes `recentReports`, `loadRecentReports()` and all four `navigateTo*()` methods. The component is repurposed to UC-ORR-10 §14.S.3 statistics, which no 18-14 AC asks for. Target routes still exist but the screen's only in-app entry points to generate/history/schedule/compare were removed.

- **18-14 AC4 unsatisfiable — "the recent-reports list renders its empty state" has no implementation** — with the recent-reports list deleted, an empty scope renders the statistics-grid empty state instead of `orphanReports.noReports`; nothing loads `GET /api/OrphanReports/history` anymore.

- **18-8 — bank-file extract silently truncated at 200 rows** — deviates from 18-8 AC1/AC2 (the code's own contract comment: extract "returns the FULL selection (the bank file is never page 1)"). `exportPageSize = 200` posted as `pageSize`; server validator caps at 200; post-fetch check only catches zero rows — a selection with `totalCount > 200` produces a partial bank file with no warning even though totalCount is in hand.

- **18-10 — charity filter bound to charity NAME, not `CharityId`** — deviates from the 18-10 screen contract (الجمعية "Bound to `CharityId`"). `loadCharities()` maps options to `{ id: c.name, name: c.name }`; `filteredDetails` filters `d.charityName === this.selectedCharityName`. Name-keyed matching breaks on duplicate or localized charity names.

- **18-10 AC2 — region and family-type breakdown tables not narrowed by the charity filter** — `beneficiariesByRegion` / `beneficiariesByFamilyType` bind directly with no filtering; only `filteredDetails` and `filteredCharityEntries` honour the selection. (Contract letter only requires the charity breakdown; severity low, but AC2's wording covers all breakdown tables.)

- **18-10 — `campaign-report` component has no `.spec.ts`** — violates CLAUDE.md 4-file component shape; directory contains only `{html,scss,ts}`.

- **18-13 — new `GET /api/Families/{id}/follow-up` returns raw DTO and anonymous error bodies, not `ApiResponse`** — violates CLAUDE.md API responses rule; inconsistent with sibling `GetFollowUpActivity` in the same diff whose 400s build `ApiResponse`. Also `GetFollowUpActivity` returns `Ok(new { Items, TotalCount })`, dropping `page/pageSize/totalPages` from the paged envelope. FamiliesController remains `: ControllerBase` (pre-existing; new endpoints perpetuate it).

- **CLAUDE.md — two new components missing `ChangeDetectionStrategy.OnPush`** — `meza-cards-report.component.ts` and `campaign-report.component.ts` set no `changeDetection`, while `orphan-reports-list.component.ts` in the same diff correctly sets OnPush.

### Scope note
The `FamiliesController.cs` portion carries substantial non-epic-18 additions (UC-FAM-09/10 guardian change requests, UC-FAM-11 follow-up route, UC-ORP orphan-coding endpoints, transfers, member control, provider GET/PUT, RemoveProviderSponsorLink) and `PeriodicOrphanReportService.cs` is a full rewrite shared with epic 9. These belong to other epics' AC sets — not audited here beyond the verified 18-relevant paths; they should be covered by the chunks that own those stories.

17 of 17 stories audited, 8 AC issues.
