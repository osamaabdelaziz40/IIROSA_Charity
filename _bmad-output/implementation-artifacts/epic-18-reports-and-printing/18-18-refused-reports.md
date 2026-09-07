# Story 18-18: Refused reports

| Field | Value |
| --- | --- |
| Story key | `18-18-refused-reports` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-18 — تقارير تم رفضها |
| Priority / size | Should · 8 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.17 screen, §23.U.18 scenario) |
| Route | `#/reports/refused-reports` |
| Endpoint | `POST /api/Reports/refused-reports` (read). The decision write path is the existing `POST /api/PeriodicOrphanReports/{id}/review` — **this story wires the jump to it, it does not rebuild it** |
| Depends on | **18-1 landed** (reports skeleton); **18-17 landed or in parallel** (this component forks 18-17's shared grid pattern). The jump needs the `#/periodic-orphan-reports` route — 18-2/18-14's job; until then the command degrades to a disabled placeholder |
| Roles | All roles → `SuperAdmin`, `Admin`, `Charity` (`Reports.View` — the charity reads its own refused worklist; scoping pins it server-side). The decision itself stays HQ-only: `{id}/review` authorises `SuperAdmin,Admin,Accountant,Employee` |

## Status

done

## Story

As a signed-in user, I want to be able to refused reports تقارير تم رفضها, so that head office keeps control of what is accepted into the sponsorship cycle.

## Acceptance Criteria

1. Given an HQ reviewer on `#/reports/refused-reports`, when the actor opens a refused item and records the decision through the wired review command, then the item carries its new state, the deciding user and the decision date, and moves out of the pending queue — via `POST /api/PeriodicOrphanReports/{id}/review`, the existing write path.
2. Given the report listing is requested, when it is served, then it is handled by `POST /api/Reports/refused-reports` and the response is rendered on the screen without a page reload.
3. Given the decision is a refusal, when no reason is given, then the refusal is not accepted — the review endpoint rejects it and the screen flags the reason field.
4. Given the decision is recorded, when the charity opens the item, then it sees the new state and, on refusal, the reason — the charity renders this same screen scoped to itself, with سبب الرفض visible.
5. Given the caller is a charity user, when the report is served, then only that charity's rows are returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload; the `CountryId` claim pins country the same way (pin-never-widen).
6. Given an HQ caller (`IsHeadOffice`) with an explicit charity id, when the report runs, then it operates on that charity's data.
7. Given the refused queue is empty for the scope, when the report is served, then the grid renders empty and the paging control reports zero pages.
8. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the §23.S.17 grid renders refused reports with their reasons resolved from the `RefuseReason` lookup; the `EditOrpReport` jump reaches the periodic-report review screen (or degrades cleanly until 18-2/18-14 register the route); a refusal without a reason is refused by the wired path; a charity user sees its own refused items with state and reason; the scenario of §23.U.18 passes end to end; scoping is enforced server-side, not only in the menu.

## Screen contract (§23.S.17 — تقارير تم رفضها)

| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | `CharityId` | Drop-down list | Optional · lookup Charities (+ كل الجهات all-option, **HQ only** — a charity user gets a pinned, disabled single option) |

Grid (row source `field in All_Data`) — identical shape to §23.S.16 (shared pattern, 18-17):

| Column | DTO key | Source |
| --- | --- | --- |
| الجمعيه | `charityName` | `PeriodicOrphanReport.Charity` → `NameAr ?? NameEn` |
| اسم اليتيم | `orphanName` | `.Orphan.FullName` |
| تاريخ التقرير | `reportDate` | `.ReportDate` |
| كود اليتيم | `orphanCode` | `.Orphan.Code` |
| سبب الرفض | `refuseReason` | `RefuseReasonId` → `RefuseReason` lookup label; falls back to free-text `RefuseReason`; renders `—` only when both are empty |

Commands:

| Command (legacy handler) | Platform realisation |
| --- | --- |
| EditOrpReport(field.ReportId, field.ChildId) | jump to the periodic-report review screen (`#/periodic-orphan-reports`) carrying the report id — the decision is recorded **there** through `POST /api/PeriodicOrphanReports/{id}/review`; disabled placeholder until the route exists |
| ExportReportData() | استخراج البيانات — ExcelJS via 18-1's `report-export.service.ts` |
| GetNext() / GetPrev() | shared `Pagination` component |
| حفظ (DeleteOutgoing()) | **template artefact** — dropped (18-15 ruling); the real save lives on the review screen this grid jumps to |

## Tasks / Subtasks

- [x] **Task 1 — DTOs + validator** (AC 2)
  - [x] `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs`: `RefusedReportsFilterDto` (`Page = 1`, `PageSize = 20`, `Guid? CharityId`), `RefusedReportsListDto` (`ReportId`, `OrphanId`, `CharityId`, `CharityName`, `OrphanName`, `ReportDate`, `OrphanCode`, `RefuseReason`, `ReviewedDate`, `ReviewerId` nullable) — no `FK_*` wire keys
  - [x] `Backend/src/IIROSA.Application/Validators/Reports/RefusedReportsValidator.cs`: page bounds only
- [x] **Task 2 — Service projection with reason resolution** (AC 2, 4, 5, 6, 7)
  - [x] `IReportService.GetRefusedReportsAsync(filter)` in `ReportService`: scope through `ResolveCharityScope`, then filter `PeriodicOrphanReport` to the refused state — **refused = `IsRefused == true`** (set exclusively by the `{id}/review` path) — projected with `Orphan`, `Charity` and the `RefuseReason` lookup includes
  - [x] Reason resolution (decided): prefer `RefuseReasonId` → `Entities/Lookups/RefuseReason.cs` label (`NameAr ?? NameEn`); fall back to the free-text `RefuseReason` column; empty only when the refusing reviewer left neither (legacy rows) — render `—`, never throw
  - [x] Order by `ReviewedDate DESC` (newest refusal first); page through 18-1's `ReportPagedResult<T>`; global soft-delete filter applies
- [x] **Task 3 — API endpoint** (AC 2, 8)
  - [x] In 18-1's `ReportsController`: `[HttpPost("refused-reports")]` → `Ok(paged)`; ValidationException → 400 `{ message, errors }` first, catch-all → 500 `{ message }`; no `ApiResponse<T>` (15-1 ruling)
  - [x] **No new write endpoint** — the decision path is `PeriodicOrphanReportsController.ReviewReport` (`{id}/review`, `[Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee")]`); this story only jumps to its screen
- [x] **Task 4 — Frontend thin component** (AC 2, 4, 7)
  - [x] `Frontend/src/app/modules/reports/refused-reports/` 4-file component — fork of 18-17's component per the shared-pattern ruling; route `#/reports/refused-reports` guarded `AuthGuard + PermissionGuard`, `data.permission: 'Reports.View'` (18-1 already maps it to all three roles)
  - [x] Filter panel: الجمعيات dropdown from `GET /api/Charities`; **HQ only** sees كل الجهات — a charity user's dropdown is pinned and disabled (the server pins regardless; the UI just stops lying)
  - [x] Grid: the 5 §23.S.17 columns with سبب الرفض resolved; shared `Pagination`; `trackBy: reportId`; row serial formula; empty state at `totalCount === 0`; **not `data-list`**; OnPush omitted (list-screen precedent)
  - [x] Charity visibility (AC 4): no role-gating on the reason column — a charity user sees سبب الرفض, `reviewedDate` and the state badge; that is the point of the worklist. Only the decision **command** is HQ-gated
  - [x] `EditOrpReport(reportId, orphanId)` jump: shown only when `authService` says the caller may review (`SuperAdmin`/`Admin`/`Accountant`/`Employee` — mirror the endpoint's roles, no client-only authorisation: the endpoint re-checks); navigates to the periodic-report review screen; disabled placeholder with tooltip while the route is unregistered (18-2/18-14)
  - [x] استخراج البيانات through `report-export.service.ts`; empty grid → nothing-to-produce message, no file
- [x] **Task 5 — Refusal-reason-mandatory wiring (AC 3)** — verify, do not rebuild
  - [x] Read `ReviewPeriodicReportDto` + its validator: the review path must reject a refusal with an empty reason. If the validator already enforces it, record the proof; if it does not, add the rule **to the periodic-reports validator/service** (the owning module), not to this report's code — and note it in the Dev Agent Record as a cross-module fix
  - [x] On the review screen's refusal branch (reached via the jump), an empty reason flags the field client-side too — convenience, not the control
- [x] **Task 6 — i18n** — title, filter label + all-option, 5 column headers, state badge (مرفوض), reason fallback dash, jump tooltip + HQ-only note, empty state, export messages under `reports.*` in **both** `assets/i18n/ar.json` and `en.json`
- [x] **Task 7 — Verification** (AC 1–8)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; **no EF migration**. MSB3021/3027 = live-API output lock; never kill the user's process
  - [x] Live check: anonymous POST → 401; HQ POST `{}` → 200 paged camelCase, rows cross-checked against `GET /api/PeriodicOrphanReports/rejected` counts; charity token → only its rows even when the payload names another charity; reason column resolves lookup labels on seeded data and falls back to free text
  - [x] Decision round-trip (AC 1, 3, 4): via the wired jump, refuse a pending report without a reason → rejected with the field flagged; with a reason → the item leaves `reports-awaiting-approval`, appears here with the reason, and the charity login sees state + reason
  - [x] `cd Frontend && npm run build` — 0 errors; ng-serve stale-bundle caveat (grep the served chunk); Arabic payloads from UTF-8 files when curling
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Why this is 8 points

The grid itself is 18-17 forked. The weight is the **decision integration**: proving the refusal-reason rule on the existing review path, wiring the jump with correct role gating on both sides, and the charity-visibility semantics (read the reason as a charity, but not decide). Cross-module touch points make this the epic's riskiest 8 points.

### Decision flow boundary (recorded)

US-RPT-18 is typed «Review decision», but the write path already exists and is owned by the periodic-reports module: `POST /api/PeriodicOrphanReports/{id}/review` (roles `SuperAdmin,Admin,Accountant,Employee`). This report screen is the **worklist + jump**, not a second decision endpoint. Rebuilding the decision here would fork the state machine — forbidden. The only permitted change inside the periodic-reports module is the reason-mandatory rule if it is missing (Task 5).

### Charity visibility (recorded)

`Reports.View` already maps to `['SuperAdmin','Admin','Charity']` from 18-1. A charity user opening `#/reports/refused-reports` sees exactly its own refused items with reasons — the correction worklist of §23.U.18. The decision command is hidden for charity users and the endpoint refuses them anyway; hiding is convenience, the `[Authorize]` on `{id}/review` is the control.

### Platform rules that bind this story

- Wire is camelCase; no `FK_*` DTO keys (Newtonsoft emits `fK_…`).
- Raw paged envelope + anonymous `{ message, errors }` — NOT `ApiResponse<T>` (15-1 ruling, architecture.md §10).
- Controllers inherit `ControllerBase` + `[Authorize]` + `[Route("api/[controller]")]` (17-1 note).
- FluentValidation in the owning module's service; this story's report validator covers page bounds only.
- Reads via `IUnitOfWork` repositories; repositories never save; the only save in the flow belongs to the periodic-reports service.
- Soft delete via the global query filter — never hand-check `IsDeleted`.
- Lookup labels `NameAr ?? NameEn` — including `RefuseReason`.
- Bespoke grid + shared `Pagination` (NOT `data-list` — recorded deviation); OnPush omitted on list screens (codebase precedent).
- Caller scope from `ICurrentUserService` — pin-never-widen (`OfficeProjectService.cs:384` shape); a charity caller cannot read another charity's refusals even by naming it in the payload.
- No client-side-only authorisation — the jump hides for charity users, but `{id}/review`'s `[Authorize]` is the control.
- EP-18 adds no entities, no EF migration.
- Tests excluded per the standing user decision.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Reports skeleton | 18-1 |
| Registering the `#/periodic-orphan-reports` routes (jump target) | 18-2 / 18-14 |
| The pending-queue twin of this grid | 18-17 |
| Bulk re-review, mass refusal, or any new write endpoint on `ReportsController` | none — never |
| Any other report vertical | 18-3 … 18-16, 18-19 … |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.17] screen contract — 1 filter field, 5-column grid, 5 commands
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.18] scenario — decision flow, refusal reason mandatory, charity visibility
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-18 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/PeriodicOrphanReport.cs] `IsRefused`, `RefuseReason`, `RefuseReasonId`, `ReviewerId`, `ReviewedDate` — the refusal projection
- [Source: Backend/src/IIROSA.Domain/Entities/Lookups/RefuseReason.cs] the reason lookup this screen resolves
- [Source: Backend/src/IIROSA.Api/Controllers/PeriodicOrphanReportsController.cs:159] `{id}/review` — the existing decision write path this story wires to
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-17-reports-awaiting-approval.md] the shared grid pattern this component forks

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- `dotnet build` (Api csproj, temp `-o`) — **0 errors**. (18-17's parallel-session `DeletedBy` idiom fix carried; no other foreign deltas.)
- `npx ng build` — EXIT=0, `error TS` count 0; i18n `reports.refusedReports.*` 16 keys in each locale, JSON parses.
- Live smoke, private instance `127.0.0.1:60970` (login field `email`), seeds `11111118-…` (2 refused across dga + وادي النطرون, 1 pending, 1 accepted — **with `CharityId` and the no-default columns `Active/Deleted/Locked/IsAccepted/IsRefused` set**; sqlcmd needs `-I` for this table's filtered indexes):
  - anonymous POST `/api/Reports/refused-reports` → **401**
  - HQ `{}` → **200, 2 rows, newest refusal first** (ReviewedDate 2026-08-10 then 2026-06-05); pending + accepted seeds **absent**
  - **reason resolution both paths**: catalogue row (`RefuseReasonId=2`, free text NULL) → `refuseReason: "بيانات التقرير غير مكتملة"` (the lookup's `NameAr`); free-text row (`RefuseReasonId=NULL`) → `"Incomplete documents"` as written
  - dga narrow → 1; وادي النطرون narrow → 1; `{"page":0}` → **400**
  - **AC 3 live**: `POST /api/PeriodicOrphanReports/{id}/review` with `isApproved:false`, no reason → **400** `errors.RefuseReason: ["Refuse reason is required when rejecting"]`
  - **AC 1 live**: same call with `refuseReason:"Smoke refusal"` → **200**; the row left `reports-awaiting-approval` (totalCount 0) and appeared in `refused-reports` as newest with `reviewedDate` = now and `reviewerId` = the deciding Admin (`b2222222-…`)
  - **AC 4 live**: charity login (`Charity@IIROSA.com`) reads the refused worklist with reason + reviewedDate + reviewer visible; only the decision command is gated (hidden for non-review roles; `{id}/review`'s `[Authorize]` re-checks)
  - seeds hard-deleted (table back to 0); smoke instance killed by PID.

### Completion Notes List

- **Task 5 proof — reason-mandatory already enforced, no cross-module fix made:** the owning module's `ReviewPeriodicReportValidator` requires `RefuseReasonId` OR non-empty `RefuseReason` when `!IsApproved` (catalogue-or-free-text, stronger than the DTO's null-only `[RequiredWhen]`, which is what fired in the live 400). Recorded, nothing rebuilt.
- **Story premise stale (same as 18-17):** the "degrade until 18-2/18-14 register the route" jump dependency no longer exists — `#/periodic-orphan-reports/:id/review` is registered; the jump is live and role-gated client-side to `{id}/review`'s roles (`SuperAdmin,Admin,Accountant,Employee`) as convenience only.
- **Refused predicate:** `IsRefused == true` (set exclusively by `{id}/review`); explicit `!IsDeleted` (no global filter on this platform — standing story-text correction). Scope ladder identical to 18-17's report-rooted ladder (claim pin → country pin → requested-charity intersection).
- **Reason resolution in-memory per page** (18-12 attach pattern): the lookup's culture-dependent `Name` is `[NotMapped]`, so `NameAr ?? NameEn` is read directly; `RefuseReasonId` rides the wire as the resolution input (18-16 `FamilyId` precedent), not rendered.
- **Seed lesson (standing for any future PeriodicOrphanReport seed):** the table's required-no-default columns are `Active`, `Deleted`, `Locked`, `IsAccepted`, `IsRefused` (plus the obvious Id/OrphanId/dates/ReviewStatus/CreatedOn) — and DML needs `sqlcmd -I` (filtered indexes, Msg 1934).
- Endpoint roles: `SuperAdmin,Admin,Charity` per the story's Roles row (the charity reads its own correction worklist; the decision stays with `{id}/review`).
- Export: client-side ExcelJS over the WHOLE worklist paged at 200, RTL sheet; state (مرفوض) + تاريخ المراجعة columns ride the sheet to match the grid; empty worklist → nothing-to-produce toast, no file.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `RefusedReportsFilterDto`, `RefusedReportsListDto`
- `Backend/src/IIROSA.Application/Validators/Reports/RefusedReportsValidator.cs` — new (page bounds)
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetRefusedReportsAsync` declared
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — implementation + `AttachRefusedReasonLabelsAsync` (+ `IRepository<Lookups.RefuseReason>` ctor wiring)
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `POST refused-reports` action
- `Frontend/src/app/modules/reports/models/report.model.ts` — `RefusedReportsFilter`, `RefusedReportsRow`
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getRefusedReports()`
- `Frontend/src/app/modules/reports/services/report-export.service.ts` — `exportRefusedReports()`
- `Frontend/src/app/modules/reports/refused-reports/` — 4-file component (new; 18-17 fork)
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — import + `refused-reports` route (`Reports.View`)
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry
- `Frontend/src/assets/i18n/ar.json` / `en.json` — `reports.refusedReports.*` (16 keys each)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-18 and module spec §23.S.17 / §23.U.18; decision flow bounded to the existing `{id}/review` path, reason resolution fixed on the `RefuseReason` lookup, charity visibility and reason-mandatory rules carried as ACs 3–4. |
| 2026-08-24 | Implemented and verified: endpoint + service + validator + DTOs with dual-path reason resolution, 18-17-forked grid with state badge + review date + role-gated jump, ExcelJS export, i18n both locales. Full decision round-trip proven live (reason-less refusal 400 → reasoned refusal 200 → queue move → charity visibility). Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
