# Epic-18 review — chunk 5 (records & conformance) — auditor findings

## 1. File List accuracy

- **Two stories record `IReportService.cs` under the wrong project folder** — 18-11 and 18-16 record `Backend/src/IIROSA.Application/Services/IReportService.cs`; the interface lives only in `Interfaces/IReportService.cs`. Wrong-path recording.
- **18-10 records a nonexistent i18n path** — `Frontend/src/app/assets/i18n/ar.json` + `en.json`; the real files are at `Frontend/src/assets/i18n/` (no `app/` segment). Every other story records the correct path.
- **18-1 and 18-2 use non-rooted path schemes that defeat cross-checking** — 18-1's backend entries omit `Backend/src/`, frontend entries omit `Frontend/src/app/`; 18-2 uses module-relative paths. Direct cause of manifest false positives; violates the repo-rooted convention used by 18-3…18-41.
- **Unexpanded brace globs / compound extension paths in 8 File Lists** — 18-24 (`orphan-files.component.{ts,html,scss,spec.ts}`), 18-26 (`survey-questionnaire.component.{…}`), 18-25, 18-30/31/32 (`component.ts/.html`), 18-33 (four-way compound), 18-37 (`component.ts/.html`). None expand to real paths as written.
- **18-37 lists only .ts/.html of a screen whose .scss and .spec.ts also changed** — those two appear in the unrecorded sweep; File List incomplete.
- **`reports.module.ts` claimed in a task but recorded by no story** — 18-1 Task 4 checkbox "[x] reports.module.ts (lazy, NgModule style)" yet 18-1's File List omits it (it pre-existed from epic 5); no other story lists it despite epic-18-referencing edits. A checked task with no File List backing.
- **The orchestrator's manifest contained 7 false "unrecorded" positives** — `DashboardController.cs`, `IDashboardService.cs`, `DashboardService.cs` ARE recorded by 18-32; `ChequeNumbersFilterValidator.cs` by 18-30; `ReceiptCardsFilterValidator.cs` by 18-31 (all recorded without backticks, which the sweep failed to parse); `ReportProfile.cs` and `OrphanDataFilterValidator.cs` by 18-1 (non-rooted paths). The "which story should have recorded them" hypotheses are therefore NOT sustained for these 7 — records exist, matcher at fault.
- **Remaining backend "extras" are out-of-epic artifacts, not epic-18 gaps** — `ReportSheetDtos.cs`, `IReportSheetService.cs`, `ReportSheetService.cs`, `ReportSheetRequestValidator.cs` are epic-5 UC-FAM-14 files (own headers say so; 18-1's completion note attributes them to the parallel session; frontend calls the 18-36/37 endpoints, not ReportSheetService). `ArabicAmountInWords.cs` is consumed exclusively by `CheckService` (cheque epic). No epic-18 story should have recorded these five.
- **~20 screens recorded only as bare directories** (`follow-up-sheets/`, `report-viewer/report-preview/`, etc.) — covered in substance but unexpandable by tooling; recording-style weakness accounting for the bulk of the 95 manifest entries.

## 2. Status honesty

- **No status violations**: all 41 stories `Status: review`, all with complete Dev Agent Record sections; zero unchecked task boxes; no empty Dev Records.
- **18-30's implementation pre-existed the story session** — honestly recorded ("user-implemented with the epic-18 batch; code-wins"). Checked boxes describe work the session only verified.
- **18-33 relaxed an acceptance expectation inside its own record** — AC originally demanded 400 when bank omitted; consolidated smoke records "Bank omitted → 200, per this story's OWN audit ruling… superseded inside the story". An in-story AC weakening that review should ratify explicitly.
- **Two live-verification claims are data-limited (labelled, not hidden)** — 18-31 (zero disbursement batches in dev DB; card sheet never rendered live) and 18-32 (zero batches; figures cross-checked "at review" only). 18-2 batched its live smoke into a group run — weaker evidence, but disclosed.
- **18-40/18-41 records disclose the broken producer binding found and fixed by the 2026-08-25 smoke** — transparently recorded with the affected files.

## 3. CLAUDE.md module-wide conformance

- **OnPush omitted in 25 of 26 report screens** — only `family-follow-up-report` sets OnPush (and it then never calls markForCheck — see chunk 4). Per 18-1's recorded ruling "list screens omit OnPush (codebase precedent)". Violates the CLAUDE.md frontend rule.
- **ApiResponse wrapping absent on every epic-18 report endpoint** — all 29 epic-18 actions in ReportsController + DashboardController.payment-summary return raw envelopes, per the stories' recorded "15-1 ruling / architecture.md §10" deviation; the SAME ReportsController mixes conventions — its two inherited epic-5/epic-14 actions DO return `Framework.Core.ApiResponse`.
- **DashboardController does not inherit the mandated base class** (`ControllerBase`, 18-32 cites a "17-1 convention"); CLAUDE.md requires `ApiController`.
- **No FluentValidation on the dashboard endpoint** — no validator; `paymentId`/`charityId` raw `[FromQuery]` Guids without `ValidateAndThrowAsync`, unlike the 28 validators wired into ReportService.
- **AutoMapper bypassed for ~all report projections** — ReportService (3,192 lines) uses IMapper exactly once; 25+ DTOs hand-projected.
- **Bilingual NameAr/NameEn flattened to one label on all report DTOs** — single `CharityName` resolved server-side; recorded ruling, deviation from the letter of the bilingual rule.
- **Compliant (no defect)**: 4-file component shape in all 26 screen directories; `trackBy` on every `*ngFor` (2 screens have none); module lazy-loaded with AuthGuard+PermissionGuard and `data.permission` on all 26 routes; `[Authorize]` on all 31 ReportsController actions + dashboard action; zero `SaveChanges` in the three report services (read-only); explicit `IsDeleted` filtering throughout (115 occurrences; platform has no global filter); charity scope resolved server-side from ICurrentUserService (24 references); only DTOs cross the API boundary; no hardcoded Arabic UI strings in templates; i18n keys both locales (spot-verified; chunk-5 i18n audit fully confirms).

## 4. Build/test state (records only)
- All 41 Dev Records claim green builds (`dotnet build` 0 errors — MSB3021/3027 attributed to the live API's file lock; `npm run build` exit 0 — 4 stories record initial failures honestly fixed).
- **Zero test execution across the epic** — every story records "Tests: excluded per the standing user decision"; the 26 `.spec.ts` files are generated TestBed stubs. No unit/E2E evidence exists for any epic-18 behavior; all behavioral evidence is the recorded private-instance smoke matrices.

## Counts
- Stories audited: 41 (all review, complete records, 0 unchecked boxes)
- File List defects: 16 story-level (wrong paths 3; non-rooted 2; unexpanded globs 8; omissions 2; task-claimed-but-unlisted 1) + manifest-side 7 false positives, ~25 unexpandable directory entries, 5 out-of-epic extras
- Conformance defects: OnPush ×25 screens; ApiResponse ×30 endpoints (mixed convention inside one controller); controller base ×1; FluentValidation ×1 endpoint group; manual mapping ×25+; bilingual flattening module-wide; 4-file shape / trackBy / route guards / [Authorize] / soft-delete / tenancy / DTO boundary / i18n — 0 defects
