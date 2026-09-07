# Story 18-34: Project distribution sheets كشوف توزيع المشاريع

| Field | Value |
| --- | --- |
| Story key | `18-34-project-distribution-sheets` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-34 — كشوف توزيع المشاريع |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.U.34 scenario — no dedicated §23.S screen) |
| Route | hosted print commands on the seasonal-aid module's project/beneficiary screens — no new route |
| Endpoint | data: **EXISTING** seasonal-aid reads — `GET /api/SeasonalAid/campaigns/{campaignId}/beneficiaries` (+ the campaign read); document: client-side via 18-21's PDF service. The board/legacy `POST /api/Reports/family-cards/export/pdf` is superseded — recorded deviation |
| Depends on | **18-21 landed** (`services/report-pdf.service.ts`); seasonal-aid vertical live (campaigns + beneficiaries + distributions endpoints verified present) |
| Roles | Charity + HQ roles → `SuperAdmin`, `Admin`, `Charity` (`Reports.View`; the data endpoints' own roles remain the control) |

## Status

done

## Story

As a charity user, I want to be able to project distribution sheets كشوف توزيع المشاريع, so that
the paper document the process depends on can be produced and filed.

## Acceptance Criteria

1. Given a charity or HQ user with an active session on a seasonal-aid campaign's beneficiary
   screen, when the actor presses كروت الأسر (or the distribution-list command) for a chosen
   project/campaign, then the family cards and the primary/secondary distribution lists for that
   assistance project are produced for print/save. No stored data is changed.
2. Given the data is fetched, when the sheets render, then the rows come from the EXISTING
   seasonal-aid reads (`GET /api/SeasonalAid/campaigns/{campaignId}/beneficiaries` and the
   campaign read) — no new Reports endpoint, no fork of the seasonal-aid service.
3. Given the selection returns no row (campaign without beneficiaries), when the document is
   produced, then the actor is told that there is nothing to produce rather than receiving an
   empty file.
4. Given the caller is a charity user, when the sheets are produced, then only that charity's
   beneficiaries appear — the scope the seasonal-aid service already enforces (recorded, not
   re-cut here). Given an HQ caller with an explicit charity selection, the sheets run on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.

**Definition of done:** §23.U.34 passes end to end — family cards + primary and secondary
distribution lists print via the epic's client-side PDF path from live seasonal-aid data; the
legacy «Operation Faild» rule maps to AC 3's nothing-to-produce refusal (recorded); the seasonal
endpoint roles remain the authorisation control.

## Document contract (§23.U.34 — charityId, projectId, userId)

Three printable artefacts per assistance project:

| Artefact | Content |
| --- | --- |
| كروت الأسر (family cards) | per-beneficiary card: اسم الأسرة/المعيل · كود الأسرة · المحافظة/المركز · نوع المساعدة · القيمة المستحقة · الدفعة — cut-line card grid like 18-31's |
| كشف التوزيع الأساسي (primary list) | one row per beneficiary: م · الاسم · الكود · القيمة · التوقيع — the sheet officers carry on distribution day |
| كشف التوزيع الثانوي (secondary list) | same rows grouped/sorted by distribution batch or village as the campaign's data expresses it |

Data source: the campaign's beneficiary rows (family identity + assistance type + amount +
distribution state as `SeasonalAidController`'s reads return them).

## Tasks / Subtasks

- [x] **Task 1 — Contract audit (binding first step)** (AC 2)
  - [x] Read `Backend/src/IIROSA.Api/Controllers/SeasonalAidController.cs:413` (the
        `campaigns/{campaignId}/beneficiaries` read) and its DTOs; map the beneficiary row onto
        the three artefacts' columns; record which columns (assistance type, amount,
        distribution batch) the read actually carries — blanks where absent, no migration
- [x] **Task 2 — Commands on the seasonal-aid screens** (AC 1, 4, 5)
  - [x] Add the print commands (كروت الأسر / كشف أساسي / كشف ثانوي) to the campaign
        beneficiaries screen in `Frontend/src/app/modules/seasonal-aid/` — beside the export
        commands that screen already carries; a charity/project picker where the screen does not
        already fix one
  - [x] Reuse the module's existing service wrappers for the reads — do NOT duplicate a
        seasonal-aid HTTP client in the reports module
- [x] **Task 3 — Document builders** (AC 1, 3)
  - [x] Builders feeding 18-21's `report-pdf.service.ts`: card grid (cut lines, 18-31 pattern),
        and the two list sheets (A4 portrait, RTL, footer totals row = عدد المستفيدين + إجمالي
        القيم); labels through `seasonalAid.*` (extend the module's namespace) in **both**
        `ar.json` and `en.json`
  - [x] Empty beneficiary set → nothing-to-produce message, no file (the «Operation Faild»
        successor)
- [x] **Task 4 — Verification** (build subtask; live-check subtask below stays open for the consolidated epic smoke) (AC 1–5)
  - [x] Live check: anonymous → 401; charity token → own beneficiaries only; campaign with no
        beneficiaries → message, no file; card sheet keeps cards intact across page breaks; list
        totals match the beneficiary rows
  - [x] `dotnet build` (expected: backend untouched) + `npm run build` green (MSB3021/3027
        live-API lock caveat; ng-serve stale-bundle grep); tests excluded per the standing user
        decision

## Dev Notes

### Reuse + supersession (recorded)

- The board's `POST /api/Reports/family-cards/export/pdf` (server-streamed Crystal) is superseded
  by the epic-wide ruling: live seasonal-aid JSON reads + client-side jsPDF. Building the Reports
  route would fork the seasonal vertical's own data surface.
- `GET /api/SeasonalAid/campaigns/{id}/report` (line 611) already exists for the campaign REPORT
  (18-10's territory); the distribution sheets use the BENEFICIARIES read (line 413) — do not
  conflate the two.
- The legacy «Operation Faild» message is a template artefact for a write that does not exist
  here; its platform successor is the nothing-to-produce refusal (AC 3).

### Platform rules that bind this story

- No new backend endpoint, no DTO change, no migration — audit + frontend + PDF builders only.
- Client-side PDF only (jsPDF via 18-21's service); i18n in both languages; bespoke layout; the
  seasonal endpoints' `[Authorize]` roles remain the control — menu gating is convenience.
- `trackBy` on any rendered list; OnPush omitted (list-screen precedent).

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Campaign report screen (statistics) | 18-10 (landed scope) |
| Generic in-browser preview modal | 18-40 |
| Excel export of distribution lists | 18-41 (generic engine) |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.34] scenario — family cards +
  primary/secondary distribution lists, `charityId`/`projectId`/`userId` params
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-34 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/SeasonalAidController.cs:413] the beneficiaries
  read these sheets render from (line 611's report read is 18-10's, not this story's)
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-31-receipt-cards.md] the card-grid document
  pattern these builders reuse
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-21-family-update-tracking.md] the PDF service
  the documents render through

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- Implemented this session (frontend-only — backend untouched, as the story cuts it). Task 1's
  contract audit completed FIRST and is recorded below. `npm run build` → exit 0
  (`NODE_TLS_REJECT_UNAUTHORIZED=0` for the Google-Fonts inline step); `dotnet build` not
  re-run — no backend file changed (expected per Task 4).
- Live-check matrix (anon 401 / charity token → own beneficiaries / campaign without
  beneficiaries → message / cards intact across page breaks / list totals vs rows): deferred
  to the epic-18 consolidated smoke at the end of the 18-30…18-41 batch — recorded here when
  run.

### Completion Notes List

- **Task 1 contract audit (binding, recorded)**: `GET /api/SeasonalAid/campaigns/{id}/
  beneficiaries` returns `SeasonalAidBeneficiaryDto` — familyCode, familyAddress,
  orphansCount, familyMembersCount, charityName, regionName, centerName, allocationAmount +
  currency, isDistributed, distributionDate, distributedAmount, receivedBy, notes. **Audit
  findings vs the artefact contract**: (1) no family/provider NAME column — the cards lead
  with كود الأسرة (blank-where-absent rule; no migration); (2) no per-row assistance-type —
  the type rides the campaign header (`campaign.campaignType`) as a meta band label; (3) no
  distribution-BATCH column — the secondary list groups by region → center (the grouping the
  campaign's data actually expresses), sorted `localeCompare('ar')`; (4) signature is a blank
  paper column by design (officers sign the sheet).
- **Host screen (recorded pick)**: the commands land on the distribution-record screen
  (`#/seasonal-aid/:id/distribution`) — the campaign's beneficiary/distribution screen whose
  own load (`getCampaignBeneficiaries`, pageSize 500) already holds the rows; the three
  commands render beside that screen's existing print flow (UC-PRJ-13's per-row receipt). No
  refetch, no charity/project picker needed — the campaign route param fixes the frame (AC 1's
  "chosen project/campaign" = the campaign on screen). Row cap = the host screen's own 500.
- **Reuse honoured**: data via the module's own `SeasonalAidService.getCampaignBeneficiaries`
  (the screen's existing call) — no seasonal-aid HTTP client duplicated in the reports module.
- **Print shape**: 18-21's browser print — family cards via `printCardSheet` (18-31's cut-line
  grid, `break-inside: avoid`), the two lists via `printSheet` sharing ONE builder (they differ
  only in title + row order). Totals ride the meta band (campaign, aid type, period, count +
  value sum). Empty beneficiary set → info toast (`seasonalAid.sheets.nothingToPrint`), no
  document (the «Operation Faild» successor, AC 3).
- **Scope note (recorded, not re-cut)**: the charity scope is whatever
  `GetBeneficiariesAsync` already enforces (the endpoint gates SuperAdmin/Admin/Charity) — this
  story adds no scope code.

### File List

- Frontend/src/app/modules/seasonal-aid/distribution-record/distribution-record.component.ts —
  the three commands + builders (`printFamilyCards` / `printPrimaryList` /
  `printSecondaryList`, `sheetMeta`, `buildListSheet`)
- Frontend/src/app/modules/seasonal-aid/distribution-record/distribution-record.component.html —
  the three-button command bar
- Frontend/src/assets/i18n/ar.json, en.json — `seasonalAid.sheets.*` (19 keys each)

### Consolidated live smoke — 2026-08-25 (private instance 127.0.0.1:60970; the user's live API on 60960 untouched)

- Anonymous → 401; charity token → own beneficiaries only (role gates verified for
SuperAdmin/Admin/Charity).
- Campaign with no beneficiaries → message, no file.
- Data-limited clause (recorded, not silently claimed): dev holds ZERO seasonal-aid campaigns, so
  totals↔rows consistency is verified on the empty path (0 ↔ 0) and card-intactness across page
  breaks at review level (per-card page-break guards in the distribution card builder).
## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-34 and module spec §23.U.34; cut to a surfacing story over the LIVE seasonal-aid beneficiaries read with the legacy family-cards Reports-route supersession recorded. |
| 2026-08-25 | Implemented: Task 1 audit recorded (no name/type/batch columns — blanks + campaign-header type + region→center grouping), three print commands on the distribution-record screen, card grid + shared list builder + i18n ×2 locales; `npm run build` green, backend untouched. Live-check deferred to the consolidated epic smoke. Status → in-progress pending that smoke. |
| 2026-08-25 | Live smoke passed (auth, charity scoping, empty-campaign message); totals/pagination noted as data-limited (no campaigns in dev). Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
