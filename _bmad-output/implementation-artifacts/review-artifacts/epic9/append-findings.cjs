// Appends "### Review Findings (epic review 2026-08-24)" to each epic-9 story file
// with only that story's findings. Idempotent: skips files already carrying the marker.
const fs = require('fs');
const path = require('path');
const dir = path.resolve(__dirname, '../../../implementation-artifacts');
const DATE = '2026-08-24';

// tag: D = decision-needed, P = patch, F = defer (checked)
const M = {
  '9-1-list-an-orphans-periodic-reports.md': [
    ['P', 'P12 NULL-dropping list predicates — ChildOrparent/!Reviewed strict compares drop NULL legacy rows [PeriodicOrphanReportService.cs]'],
    ['P', 'P49 statusClass returns BS4 badge-* on Bootstrap 5.3 [periodic-reports-list.component.ts:4350]'],
    ['P', 'P30 HQ charity dropdown async without markForCheck (list screen) [periodic-reports-list.component.ts:87]'],
    ['P', 'P63a Record pager deviation: AC6 "reports zero pages" met by hiding pager (unrecorded)'],
    ['P', 'P64 Reconcile recorded ControllerBase ruling with ApiController re-base (CLAUDE.md side wins)'],
  ],
  '9-2-look-up-an-orphan-by-code-before-reporting.md': [
    ['P', 'P29 OnPush: lookupOrphan/loadEducationLevels mutate state without markForCheck — spinner stuck, resolved header dead [periodic-report-form.component.ts:168-186,101-104]'],
    ['P', 'P38 Any lookup failure (500/403/network) shown as "unknown orphan" [periodic-report-form.component.ts:3165]'],
    ['P', 'P60a Uncoded orphan: save disabled with no explanation — add hint; reset stale resolution when code edited [periodic-report-form.component.ts:152-174]'],
  ],
  '9-3-create-a-periodic-report.md': [
    ['P', 'P4 Null-CharityId stamp at create — report invisible to own charity + 201 null body [PeriodicOrphanReportService.cs create]'],
    ['P', 'P7 Soft-delete squats (orphan,year,month) unique slot — filtered unique index (WHERE IsDeleted=0) [InitialCreate IX + create guard]'],
    ['P', 'P8 Guardian one-per-family-month: plain IX, not filtered-unique (story claim false) [migration 20260824115014]'],
    ['P', 'P9 Duplicate race — raw 500 not friendly 400 [create path]'],
    ['P', 'P14 Coded-orphan pre-condition never enforced server-side on create'],
    ['P', 'P47b Form default reportDate = UTC yesterday for Riyadh 00:00-03:00 [periodic-report-form.component.ts:200]'],
  ],
  '9-4-view-a-periodic-report.md': [
    ['P', 'P37a Load failure = silent blank screen (detail) — toast/redirect like search [periodic-report-detail.component.ts:77]'],
    ['P', 'P36b Delete-failure message double-unwrap (detail) [periodic-report-detail.component.ts:112]'],
    ['P', 'P26c i18n: history/compare companion keys missing both locales (detail links) [ar.json/en.json]'],
  ],
  '9-5-update-a-periodic-report.md': [
    ['P', 'P11 Update skips guardian/family-month recheck on re-dating [update path]'],
    ['P', 'P48 educationalLevelId select [value] string vs numeric patch — never pre-selects in edit [periodic-report-form.component.html:2885]'],
    ['P', 'P37b Load failure blank screen (form) [periodic-report-form.component.ts:284]'],
    ['P', 'P60c checkEditable fails open on HTTP error — fail closed [periodic-report-form.component.ts:302]'],
  ],
  '9-6-delete-a-periodic-report.md': [
    ['P', 'P7b Deleted report permanently squats its slot — combine with P7 filtered unique index [create guard + IX]'],
    ['P', 'P36a Delete-failure message double-unwrap — server reason never surfaces [periodic-reports-list.component.ts:158]'],
    ['P', 'P61 Story claim "framework does soft delete" is FALSE — global filter is commented out; deletes are hand-stamped. Correct Change Log.'],
  ],
  '9-7-accept-a-periodic-report.md': [
    ['P', 'P10 Charity self-accept not blocked (self-refuse is) — symmetric guard [review path]'],
    ['P', 'P13 Empty refusal reason persists when accepting — clear reason fields on accept'],
    ['P', 'P35 Review submit double-unwraps error — BR-14 translated mapping dead code; use `error ?? {}` fallback [periodic-report-review.component.ts:260]'],
    ['P', 'P37c Load failure blank screen (review) [periodic-report-review.component.ts:171]'],
  ],
  '9-8-refuse-a-periodic-report-with-reasons.md': [
    ['P', 'P46 refuseReasonId Number(null)=0 sent as FK when dropdown null — null-guard [periodic-report-review.component.ts:3722]'],
  ],
  '9-9-filter-reports-by-status.md': [
    ['P', 'P5 Unknown ReviewStatus token silently ignored — unfiltered register; validate, 400 [filter binding]'],
    ['P', 'P47a toISOString off-by-one day for UTC+3 in fmtDate + Excel [orphan-report-search.component.ts:292]'],
    ['P', 'P34 Excel export recursion no cancel — takeUntil [orphan-report-search.component.ts:1835]'],
    ['P', 'P23 history + schedule companion routes lack PermissionGuard [routing:176,200]'],
    ['P', 'P24 OrphanReports.Compare absent from PERMISSION_ROLES — hasPermission fails open [auth.service.ts:447]'],
    ['P', 'P26 i18n: orphanReports.history/compare/schedule + breadcrumb.* routing keys + 59 template keys + common.exportExcel missing from BOTH locales [ar.json:3206/en.json:3207]'],
    ['P', 'P27 Hard-coded English "Report ${n}" fallback [orphan-report-comparison.component.ts:989]'],
    ['P', 'P28 toLocaleDateString without locale [orphan-report-comparison.component.ts:985]'],
    ['P', 'P57b Dead code: viewReport + unused Router [orphan-report-search.component.ts:1795]'],
    ['P', 'P63b Record pager deviation: AC4 zero-pages met by hiding pager (unrecorded)'],
    ['P', 'P63c Record manual !IsDeleted deviation vs platform rule (necessary; no global filter)'],
  ],
  '9-10-view-report-statistics-by-group.md': [
    ['D', 'D1 Role/endpoint mismatch: map admits Accountant/Employee but /api/OrphanReports/statistics|generate authorize SuperAdmin/Admin/Charity only — dead screen with generic 403 toast. Widen endpoint roles OR narrow route map [auth.service.ts:124 vs OrphanReportsController.cs:38,321]'],
    ['P', 'P22 CRITICAL route shadowing: :id (line 64) declared before orphan-reports (line 114) — screen unreachable via sidebar [periodic-orphan-reports-routing.module.ts]'],
    ['P', 'P1 Soft-delete leak: grouped stats include deleted reports — add !r.IsDeleted [OrphanReportService.cs:250]'],
    ['P', 'P2 Tenancy pin: CharityId ?? filter.CharityId fail-open, country-blind — mirror ApplyCharityScopeAsync [OrphanReportService.cs:69,427]'],
    ['P', 'P3 Scope fails open when caller has neither charity nor country claim — fail closed [ApplyCallerScope]'],
    ['P', 'P15 Grouped stats count reports, not orphans — totals disagree with tiles; distinct-orphan per spec'],
    ['P', 'P20 1000-cap truncation non-deterministic — add OrderBy'],
    ['P', 'P21 SummaryDto.OrphanId int vs list Guid — align wire contract'],
    ['P', 'P30b HQ charity dropdown markForCheck (statistics screen) [orphan-reports-list.component.ts:72]'],
    ['P', 'P31 Rapid charity-switch race — switchMap [orphan-reports-list.component.ts:84]'],
    ['P', 'P32 Failed re-run leaves previous results under new criteria [orphan-reports-list.component.ts:104]'],
    ['P', 'P42a Pagination boundary page 0 / last+1 blanks grid — clamp [orphan-reports-list]'],
    ['P', 'P43a exporting flag never set — double-click duplicates export [orphan-reports-list.component.ts:47]'],
    ['P', 'P50 (change) reads stale ngModel on charity select — (ngModelChange) [orphan-reports-list.component.html:1032]'],
  ],
  '9-11-extract-detailed-report-data.md': [
    ['D', 'D1 (also applies here) — see 9-10'],
    ['P', 'P1b Soft-delete leak: detailed extract includes deleted reports [OrphanReportService.cs:454]'],
    ['P', 'P41 Grid headers misaligned: orphanCode has no th; codesOnly mode 2-header vs 3-cell body [orphan-reports-generate.component.html:85-136]'],
    ['P', 'P30c HQ charity dropdown markForCheck [orphan-reports-generate.component.ts:72]'],
    ['P', 'P42b Pagination clamp [orphan-reports-generate.component.ts:124]'],
    ['P', 'P43b exporting flag never set [orphan-reports-generate.component.ts:44]'],
    ['P', 'P51b Page-number *ngFor no trackBy [orphan-reports-generate.component.html:525]'],
    ['P', 'P52 reportDate cell lacks "—" fallback [orphan-reports-generate.component.html:490]'],
    ['P', 'P58 const filter: any — restore typed DTO [orphan-reports-generate.component.ts:787]'],
  ],
  '9-12-extract-accepted-reports.md': [
    ['P', 'P39 Silent 2000-row cap: totalCount computed from collected rows, server totalCount discarded — capped indicator can never show; use server total [orphan-report-state-extract.component.ts:133,148]'],
    ['P', 'P42c Pagination clamp [orphan-report-state-extract.component.ts:163]'],
    ['P', 'P30d HQ charity dropdown markForCheck [orphan-report-state-extract.component.ts:88]'],
    ['P', 'P62 Story "capped indicator" claim FALSE — correct Completion Notes after P39 fix'],
  ],
  '9-13-extract-refused-reports.md': [
    ['P', 'P39b (shared with 9-12) silent 2000-row cap [orphan-report-state-extract.component.ts]'],
    ['P', 'P25 Any :state other than refused silently renders accepted extract — guard unknown state [orphan-report-state-extract.component.ts:82]'],
    ['P', 'AA3 Route pageTitle hardcodes accepted title for both states — refused tab mislabeled [periodic-orphan-reports-routing.module.ts:142]'],
    ['P', 'F7 Reason column "prominent, red" not styled (AC met; record overstates) — add class or correct record'],
  ],
  '9-14-list-orphans-with-no-renewed-report.md': [
    ['D', 'D2 Co-owned-file defects (main-layout menu, auth.service login, ReportService family incl. ApplyCharityScopeAsync fallthrough) — patch under epic-9 or leave to owning sessions?'],
    ['P', 'P18 ReportsController catch-alls never log — _logger.LogError [ReportsController.cs]'],
    ['P', 'P19 Page clamp — 11 validators injected, 1 shipped; Page=0 negative skip 500'],
    ['P', 'P17 MezaCards ReportNo=0 → 400 instead of grid — treat 0 as unnumbered [ReportsController]'],
    ['P', 'P40 Non-renewed fetches only page 1 (100 rows) — count>100 silently truncated; export ships partial as complete [non-renewed-reports.component.ts:106-118]'],
    ['P', 'P42d Pagination clamp [non-renewed-reports.component.ts:133]'],
    ['P', 'P30e HQ charity dropdown markForCheck [non-renewed-reports.component.ts:73]'],
  ],
  '9-15-extract-report-numbers-added-in-a-period.md': [
    ['D', 'D1 (also applies here) — see 9-10'],
    ['P', 'P1c Soft-delete leak: numbers/counts include deleted reports [OrphanReportService.cs:510]'],
    ['P', 'P16 IncludeReportNumbers window semantics — silent no-op without window; partial indistinguishable from empty'],
    ['P', 'P42e Pagination clamp [report-numbers.component.ts:127]'],
    ['P', 'P30f HQ charity dropdown markForCheck [report-numbers.component.ts:71]'],
  ],
  '9-16-view-report-attachments.md': [
    ['P', 'P33 Gallery blob subscriptions outlive component — unrevoked URLs + destroyed-view writes [report-attachment-gallery.component.ts:55-79,129]'],
    ['P', 'P55 Gallery download filename lacks extension [report-attachment-gallery.component.ts:1685]'],
  ],
  '9-17-print-the-periodic-report-form.md': [
    ['P', 'P44 Auto-print never fires if any image fails — decrement pending in error handler too [periodic-report-print.component.ts:144-159]'],
    ['P', 'P45 Auto-print setTimeout never cancelled on destroy — prints destination page [periodic-report-print.component.ts:2316]'],
    ['P', 'P53 snapshot.paramMap — stale report on same-route id change [periodic-report-print.component.ts:2216]'],
    ['P', 'P54 annualFeeForStudy hidden when 0; ragged row when optionals absent [periodic-report-print.component.html:1856]'],
    ['P', 'P56 charityName printed twice in form header [periodic-report-print.component.html:1747,1772]'],
    ['P', 'P59 getPrintForm URL builds from environment.apiUrl — /api doubling risk [periodic-orphan-report.service.ts]'],
    ['P', 'P57c Dead code: RouterLink/auth unused [periodic-report-print.component.ts]'],
  ],
};

const deferred = [
  'F1 Concurrent reviews last-write-wins (no RowVersion) — schema + concurrency work beyond review scope [PeriodicOrphanReportService]',
  'F2 Detail extract runs on every GenerateReport — perf refactor, unmeasured impact [OrphanReportService]',
  'F3 Same-date supersede semantics in non-renewed window — minor spec ambiguity [ReportService]',
  'F4 API-caller inverted date range silently empty (UI already validates) [OrphanReportService/ReportService]',
  'F5 HQ charity dropdowns cap at 500 silently — tenant-scale dependent [orphan-reports-list/generate]',
  'F6 POR ReportNo nvarchar(max), count+1 race, D4 rollover — numbering scheme redesign',
  'F7 9-13 reason column red styling — cosmetic (AC met)',
  'F8 andOr always sent on wire — harmless no-op without status predicates [orphan-report-search]',
];

let touched = 0, skipped = 0;
for (const [file, items] of Object.entries(M)) {
  const p = path.join(dir, file);
  if (!fs.existsSync(p)) { console.log('MISSING:', file); continue; }
  let txt = fs.readFileSync(p, 'utf8');
  if (txt.includes('### Review Findings (epic review')) { console.log('SKIP (marker):', file); skipped++; continue; }
  const order = { D: 0, P: 1, F: 2 };
  items.sort((a, b) => order[a[0]] - order[b[0]]);
  let sec = `\n\n### Review Findings (epic review ${DATE})\n\n`;
  for (const [tag, text] of items) {
    if (tag === 'D') sec += `- [ ] [Review][Decision] ${text}\n`;
    else if (tag === 'P') sec += `- [ ] [Review][Patch] ${text}\n`;
    else sec += `- [x] [Review][Defer] ${text} — deferred, pre-existing\n`;
  }
  fs.writeFileSync(p, txt + sec);
  touched++;
}
// deferred-work.md
const dw = path.join(dir, 'deferred-work.md');
let dwtxt = fs.existsSync(dw) ? fs.readFileSync(dw, 'utf8') : '# Deferred Work\n';
if (!dwtxt.includes('## Deferred from: code review of epic 9')) {
  dwtxt += `\n## Deferred from: code review of epic 9 (${DATE})\n\n`;
  for (const d of deferred) dwtxt += `- ${d}\n`;
  fs.writeFileSync(dw, dwtxt);
  console.log('deferred-work.md updated');
} else console.log('deferred-work.md SKIP (marker)');
console.log(`story files appended: ${touched}, skipped: ${skipped}`);
