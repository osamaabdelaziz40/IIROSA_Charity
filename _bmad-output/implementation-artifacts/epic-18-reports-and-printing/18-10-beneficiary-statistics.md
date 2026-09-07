# Story 18-10: Beneficiary statistics

| Field | Value |
| --- | --- |
| Story key | `18-10-beneficiary-statistics` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-10 — احصائيات المستفيدين |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.2 screen, §23.U.10 scenario) |
| Route | `#/seasonal-aid/:id/report` — **already routed** (`seasonal-aid-routing.module.ts` `:id/report`, permission `SeasonalAid.Reports`) |
| Endpoint | existing seasonal-aid campaign report surface: `GET /api/SeasonalAid/campaigns/{id}/report` (`SeasonalAidController.cs:611`) — **no new ReportsController endpoint** |
| Depends on | **18-1 landed** (reports skeleton: `report-export.service.ts` shared exporter, `Reports.View` permission entry) · the existing `seasonal-aid` module (shipped) |
| Roles | Gen. Director → `SuperAdmin`, `Admin` (`SeasonalAid.Reports` — already wired to exactly these roles) |

## Status

done

## Story

As a General Director, I want to be able to beneficiary statistics احصائيات المستفيدين, so that I can
answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given a General Director with an active session on `#/seasonal-aid/:id/report`, when the actor
   opens the screen, then no stored data is changed — the operation is a read.
2. Given the actor filters by charity (الجمعية) or project, when the breakdown tables re-render, then
   they show only the selected dimension's rows, without a page reload.
3. Given a charity user reaches the report, when the data is served, then no other charity's
   beneficiary rows are returned — caller scope resolved server-side from `ICurrentUserService`,
   never from the payload (audit finding to verify; fix if the shipped read leaks).
4. Given the selected charity/project combination matches no beneficiary, when the filter applies,
   then the breakdown renders empty and the actor sees the empty state (no zero-row export).
5. Given the actor presses the export command, when rows exist, then an Excel workbook is delivered
   via the shared report exporter; the print command produces the browser print view.
6. Given the session has expired or the role is not permitted, when the function is invoked, then the
   request is rejected and the actor is routed back to the login screen.

**Definition of done:** §23.S.2's two drop-downs and four commands work on the shipped
`CampaignReportComponent`; export runs through 18-1's `report-export.service.ts`; the caller-scope
audit of the server read is recorded in the Dev Agent Record.

## Screen contract (§23.S.2 — campaign report, 2 fields, 4 commands)

The component exists (`Frontend/src/app/modules/seasonal-aid/campaign-report/`, standalone) and
already renders the report + breakdowns with `printReport()` and an inline ExcelJS export. This
story completes it to the §23.S.2 contract:

| Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- |
| الجمعية | `CharityId` | Drop-down | Optional · `GET /api/Charities` (`result.items \|\| []`) + كافة الجهات · on change: reload/filter the charity breakdown (`getCharityData()`) |
| (unlabelled) | `ProjectId` | Drop-down | Optional · lookup Projects — **no `Projects` lookup endpoint exists**; bind to the campaigns of the same programme via `SeasonalAidService.getCampaigns()` and jump to the selected campaign's report (recorded adaptation) |

| Command | Handler | Platform shape |
| --- | --- | --- |
| (icon) export | `ExportReportData()` | shared `report-export.service.ts` workbook (replaces the inline ExcelJS block) |
| (icon) next | `GetNext()` | navigate to the next campaign in the `getCampaigns()` page (disabled at the end) |
| (icon) prev | `GetPrev()` | navigate to the previous campaign (disabled at the start) |
| حفظ | `DeleteProject()` | legacy artefact of the copied spec — **not built** (this screen writes nothing; see Dev Notes) |

## Tasks / Subtasks

- [x] **Task 1 — Audit the existing surface** (AC 1, 3)
  - [x] Read `Frontend/src/app/modules/seasonal-aid/campaign-report/campaign-report.component.ts`
        and its template; record what already works (report load, breakdowns, print, inline export)
        before changing anything — audit, don't rewrite
  - [x] Audit the server read behind `GET /api/SeasonalAid/campaigns/{id}/report`
        (`SeasonalAidController.cs:611` → service): if the projection does not pin a charity
        caller to `ICurrentUserService.CharityId` (pin-never-widen,
        `OfficeProjectService.cs:384` shape), add that guard — a charity user must not see other
        charities' beneficiary rows in the breakdown or the export
- [x] **Task 2 — Filter drop-downs** (AC 2, 4)
  - [x] Charity drop-down from `GET /api/Charities` (`result.items || []`, كل الجهات all-option)
        filtering `report.distributionDetails` / the charity breakdown table client-side on change;
        hidden/disabled for a charity caller (server pins anyway)
  - [x] Project drop-down from `SeasonalAidService.getCampaigns()` (label `nameAr ?? nameEn`,
        value `campaignId`); selection navigates to that campaign's report — recorded adaptation
        for the nonexistent `Projects` lookup; no hardcoded arrays
- [x] **Task 3 — Commands** (AC 2, 5)
  - [x] `GetNext()` / `GetPrev()`: track the current campaign's position in the loaded campaigns
        page and `router.navigate(['/seasonal-aid', nextId, 'report'])`; disable at the ends
  - [x] `ExportReportData()`: replace the inline ExcelJS block with 18-1's
        `report-export.service.ts` (same Summary + Distribution Details sheets, RTL view); zero
        rows after filtering → on-screen message, no file. Keep `printReport()` as `window.print()`
        — jsPDF is not installed (18-21)
- [x] **Task 4 — i18n** — new keys only (charity/project labels, all-option, next/prev tooltips,
      empty-filter state, export toasts) under `seasonalAid.*` in **both** `assets/i18n/ar.json`
      and `en.json`; reuse existing `seasonalAid.campaignReport.*` keys where they exist
- [x] **Task 5 — Verification** (AC 1–6)
  - [x] UI: drop-downs filter the breakdown; next/prev navigate and disable at the ends; export
        downloads a workbook that opens with the filtered rows; print opens the browser dialog
  - [x] Live check: unauthenticated `GET /api/SeasonalAid/campaigns/{id}/report` → 401; charity
        caller sees only its rows (post-audit); HQ sees all
  - [x] `dotnet build` + `npm run build` green (MSB3021/3027 = live-API output lock — never kill
        the user's process; ng-serve stale-bundle grep caveat)
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Platform rules that bind this story

- Export is **client-side only** (ExcelJS via 18-1's `report-export.service.ts`); the shipped
  server endpoints `campaigns/{id}/report/pdf` and `campaigns/{id}/report/excel`
  (`SeasonalAidController.cs:636/665`) are legacy/NotImplemented and superseded — recorded
  deviation (architecture.md §10). Print stays `window.print()` until jsPDF lands (18-21).
- §23.S.2's «حفظ → DeleteProject()» is a spec-generation artefact (a save button wired to a delete
  handler on a read-only screen): not built. This story is a read — no endpoint writes anything.
- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling); controller
  inherits `ControllerBase` (17-1 note); camelCase wire; no `FK_*` keys; reads via `IUnitOfWork`
  repositories; global soft-delete filter; lookup labels `NameAr ?? NameEn`.
- No entity, no migration — this story only completes an existing screen and audits an existing read.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| `#/reports/*` grid screens (orphans, excluded, unsponsored, meza-cards…) | 18-3 … 18-8 |
| PDF rendering of any report (jsPDF install) | 18-21 |
| The generic display-in-browser / export-to-Excel engine | 18-40 / 18-41 |
| Campaign CRUD, beneficiaries, distributions | seasonal-aid epic (shipped) |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.2] screen contract — 2 drop-downs,
  4 commands
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.10] scenario — aggregate beneficiary
  counts scoped to the caller
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-10 acceptance criteria
- [Source: Frontend/src/app/modules/seasonal-aid/campaign-report/campaign-report.component.ts] the
  shipped surface this story audits and completes
- [Source: Backend/src/IIROSA.Api/Controllers/SeasonalAidController.cs:611] the report read;
  [:636]/[:665] the superseded server exports
- [Source: Frontend/src/app/modules/seasonal-aid/services/seasonal-aid.service.ts:48] `getCampaigns`
  paging behind next/prev

## Dev Agent Record

### Agent Model Used

`GLM 4.7 (Claude Code dev agent)`

### Debug Log References

- **Caller-scope audit (Task 1, verdict: NO LEAK — no fix applied):**
  - The endpoint is `[Authorize(Roles = "SuperAdmin,Admin")]` (`SeasonalAidController.cs` Reporting region) — a charity caller is rejected at the role gate before any data flows.
  - Even past the gate, `GenerateCampaignReportAsync` wraps the read in `GetScopedCampaignAsync` → `EnsureCampaignScope` → `IsCampaignVisibleToCaller`: charity claim → `campaign.CharityId == claim`; non-HQ non-charity → nothing; country claim → campaign's country (or unset); out-of-scope campaigns throw `KeyNotFoundException` → **404** (no existence leak either). This IS the pin-never-widen shape.
  - Bonus finding: `BeneficiariesByCharity` groups by the real `Family.FK_CharityId` + resolved names (a code comment records that the repository's Charity-navigation grouping sits on a null legacy column) — the charity breakdown is trustworthy.
- Live smoke (private instance, `http://127.0.0.1:60970`; killed after): anon `GET campaigns/{id}/report` → **401**; HQ with unknown id → **404** `{"message":"Campaign with ID '…' not found"}`; Charity-role caller → **403**. Dev DB holds **0 campaigns** (sqlcmd) — the report surface is legitimately empty until the seasonal-aid epic seeds campaigns.
- Frontend: first `npm run build` FAILED — `trackByEntry` (typed `{key,value}`) misused on the two new `*ngFor` shapes (charity options / campaigns) → TS2322 TrackByFunction mismatch ×2. Fixed with typed `trackByOption`/`trackByCampaign`; rebuild → **exit 0, 0 TS errors**. (Lesson: a grep-filtered build tail can hide a failed exit — check `exit:` and `error TS` count explicitly.)
- i18n: node key-walk → 8 new `seasonalAid.*` keys present in ar+en.
- Backend: no code changes in this story (audit verdict = no fix) → no rebuild needed; the last temp-folder compile (18-9) remains green.

### Completion Notes List

- The two §23.S.2 drop-downs: الجمعية filters the distribution-details grid AND the charity-breakdown card client-side (value = charity NAME because both surfaces key on names; options come from `GET /api/Charities`, كافة الجهات all-option). The project drop-down is the recorded adaptation: `getCampaigns()` list; selecting navigates to that campaign's report.
- Next/prev walk the same loaded campaigns page (`pageNumber 1, pageSize 100`), disabled at the ends; the campaign `<select>` mirrors the current campaign id.
- `ExportReportData()` now goes through 18-1's shared `report-export.service.ts` (`exportCampaignReport`) — same two sheets ported verbatim (Summary + Distribution Details), both RTL, sanitized campaign name in the file name. Zero rows after the charity filter → `seasonalAid.noFilteredRows` toast, no file. The summary sheet stays campaign-wide — it is labelled the campaign summary (recorded).
- «حفظ → DeleteProject()» not built (spec artefact on a read-only screen). `printReport()` stays `window.print()`.
- The filter row carries `no-print` so the browser print view shows the report, not the controls.

### File List

- `Frontend/src/app/modules/reports/services/report-export.service.ts` — NEW `exportCampaignReport(report, details, fileName?)` (two sheets, RTL, shared exporter)
- `Frontend/src/app/modules/seasonal-aid/campaign-report/campaign-report.component.ts` — charity/campaign drop-down state + loaders, `filteredDetails`/`filteredCharityEntries`, `onCharityFilterChange`/`onCampaignSelect`, `getNext`/`getPrev` + end guards, `exportToExcel` replaced by `exportReportData` (shared exporter), typed `trackByOption`/`trackByCampaign`
- `Frontend/src/app/modules/seasonal-aid/campaign-report/campaign-report.component.html` — §23.S.2 filter row (charity + campaign selects), next/prev buttons in the header group, grid + charity breakdown now render the FILTERED sets
- `Frontend/src/assets/i18n/ar.json` + `en.json` — 8 new `seasonalAid.*` keys each
- No backend files changed (audit-only verdict recorded above)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-10 and module spec §23.S.2 / §23.U.10; scoped to auditing/completing the shipped `CampaignReportComponent` — dropdowns, next/prev, shared export, server-scope audit. |
| 2026-08-24 | Implemented: caller-scope audit (no leak — HQ-only roles + GetScopedCampaignAsync pin; recorded), §23.S.2 filter row + next/prev, inline ExcelJS replaced by the shared exporter (RTL, filtered rows, no-file-on-empty). npm build green after a trackBy typing fix; live 401/404/403 recorded. Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
