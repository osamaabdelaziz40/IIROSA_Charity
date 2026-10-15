# WAR.IIROSA - Orphan Report Cycles

دورات التقارير الدورية للأيتام | use case prefix `UC-ORC` | new-platform capability (no WAR.IIROSA chapter)


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Orphan Report Cycles |
| Module | Orphan Report Cycles — دورات التقارير الدورية للأيتام |
| Use case prefix | UC-ORC |
| Chapter in master document | none — new capability requested for the re-platform |
| Documented use cases | 9 |
| Principal routes | `#/orphan-report-cycles`, `#/orphan-report-cycles/create`, `#/orphan-report-cycles/:id/edit`, `#/reports/cycle-reports` |
| Version | 1.1 |
| Status | Implemented (backend + frontend) — 14 October 2026 |
| Date | 14 October 2026 |
| Parent document | architecture.md (authoritative) |

## Contents of this document

1. Module purpose, cycle concept and status lifecycle
2. Use-case catalogue
3. §26.S — screen field specifications
4. §26.U — one expanded scenario per use case
5. §26.R — business rules
6. §26.A — data model, background process, API and frontend realisation annex

## 26. Orphan Report Cycles

دورات التقارير الدورية للأيتام — HQ (Admin / SuperAdmin) defines reporting **cycles**: named
periods during which every charity must submit one periodic report per sponsored orphan. The cycle
is the pacing mechanism for chapter 14: outside a cycle there is nothing to submit from the orphan
screens; inside a cycle the «إضافة تقرير دوري» (Add Periodic Report) action appears on the orphan's
page and in the orphan search results table, and the charity submits, then corrects, its reports
while the cycle runs. When two cycles run at the same time the user chooses which one to report
into. Cycle status is never set by hand — a background service moves every cycle
**Not Started → In Progress → Completed** as its dates pass, and completion locks charity-side
editing.

### 26.1 The cycle and its lifecycle


| Aspect | Detail |
| --- | --- |
| Cycle identity | Title (Arabic primary, English optional) and free-text description, both bilingual per the platform rule. |
| Duration | Mandatory start and end dates (end strictly after start). A cycle may span any length, and **overlapping cycles are permitted** — two or more cycles In Progress at once is a first-class scenario, not an error. |
| Status | `NotStarted` (created, start date in the future) → `InProgress` (now within `[StartDate, EndDate]`) → `Completed` (EndDate passed). Stored on the row, advanced only by the background service (UC-ORC-05); read-only on every screen. |
| Scope | A cycle is HQ-defined and platform-wide: every charity reports its own orphans within the same cycle. Tenancy applies to *submissions*, never to the cycle definition. |
| Relationship to chapter 14 | A cycle-bound report is an ordinary `PeriodicOrphanReport` (UC-ORR-03/05) carrying a `CycleId`. The HQ accept/refuse review queue (UC-ORR-07/08) is unchanged and may act on a report whatever the cycle's status. Housing-register reports (UC-HOU-06/08) are **not** cycle-bound. |
| Submission window | Create and charity-side edit are allowed only while the cycle is In Progress. `NotStarted` = too early; `Completed` = locked. |

### 26.2 Use cases


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-ORC-01 | List orphan report cycles إدارة الدورات | Admin, SuperAdmin | The cycle register: paged, newest first, with title, description, period, status badge, bound-report count and progress. Filter by status and free text. | Route `#/orphan-report-cycles` → GET /api/OrphanReportCycles |
| UC-ORC-02 | Create a cycle إضافة دورة | Admin, SuperAdmin | Title (Ar/En), description (Ar/En), start and end dates. On save the status is computed from the dates (`NotStarted`, or `InProgress` if already running). | Route `#/orphan-report-cycles/create` → POST /api/OrphanReportCycles |
| UC-ORC-03 | Update a cycle تعديل الدورة | Admin, SuperAdmin | Amend master data while the cycle has not completed. Dates may be moved (including extending a running cycle); the status recomputes on save. Completed cycles are read-only. | Route `#/orphan-report-cycles/:id/edit` → PUT /api/OrphanReportCycles/{id} |
| UC-ORC-04 | Delete a cycle حذف الدورة | Admin, SuperAdmin | Soft-deletes a cycle that never started and has no bound reports. Cycles with submissions — and completed cycles, which are the audit record of a reporting round — are refused. | DELETE /api/OrphanReportCycles/{id} |
| UC-ORC-05 | Run the automatic status lifecycle تحديث حالة الدورات | System (background service) | Periodically advances every cycle: `StartDate` reached → In Progress; `EndDate` passed → Completed, stamping the transition time. No user action ever sets status. | `OrphanReportCycleStatusService` (hosted service) |
| UC-ORC-06 | Choose the active cycle from an orphan context اختيار الدورة | Charity | On the orphan's page and in the orphan search results table, the «إضافة تقرير دوري» action appears while ≥ 1 cycle is In Progress. One active cycle → straight to the report form; two or more → a chooser listing each cycle with its dates and this orphan's submission state; an already-reported cycle offers Edit instead. | `GET /api/OrphanReportCycles/orphan/{orphanId}/report-status` → route `#/periodic-orphan-reports/create` with `orphanId` + `cycleId` |
| UC-ORC-07 | Submit a periodic report within a cycle رفع تقرير داخل دورة | Charity | The chapter-14 report form (UC-ORR-03), opened with the orphan and cycle pre-bound. One report per orphan per cycle; the cycle header is shown read-only on the form. | Route `#/periodic-orphan-reports/create` → POST /api/PeriodicOrphanReports (payload gains `cycleId`) |
| UC-ORC-08 | Edit a report while its cycle runs تعديل التقرير أثناء الدورة | Charity | The charity may amend its report data (UC-ORR-05) any number of times while the cycle is In Progress. On completion the report becomes read-only for the charity; HQ review is unaffected. | Route `#/periodic-orphan-reports/:id/edit` → PUT /api/PeriodicOrphanReports |
| UC-ORC-09 | View cycle report coverage تقارير الدورة | Charity, HQ | In the reports module, for a chosen cycle: the orphan population split into submitted (with report date and review state) and not submitted — the chase list of the cycle. HQ filters by charity; a charity sees only its own orphans. Export to PDF/Excel. | Route `#/reports/cycle-reports` → POST /api/Reports/cycle-reports |

### 26.S  Screen field specifications

Screens are RTL-first; every label below resolves through `ar.json` / `en.json`.

#### 26.S.1  Screen `#/orphan-report-cycles` — cycle register


| Property | Value |
| --- | --- |
| Feature module | `orphan-report-cycles` (lazy-loaded, HQ-only behind `PermissionGuard`) |
| Component | `CycleListComponent` on the shared `data-list` component |
| Filters | بحث (free text over title/description) · الحالة (status: الكل / لم تبدأ / جارية / مكتملة) |
| Grid columns | العنوان (Ar, En) · الوصف · من تاريخ · إلى تاريخ · الحالة (badge) · عدد التقارير · نسبة الاكتمال (submitted ÷ coded orphans) |
| Commands | إضافة دورة · تعديل · عرض · حذف — all HQ-only |

#### 26.S.2  Screen `#/orphan-report-cycles/create` and `#/orphan-report-cycles/:id/edit` — cycle form


| Field | Bound to | Control | Mandatory | Rule |
| --- | --- | --- | --- | --- |
| العنوان بالعربية | `titleAr` | text | yes | ≤ 250 chars |
| العنوان بالإنجليزية | `titleEn` | text | no | ≤ 250 chars |
| الوصف بالعربية | `descriptionAr` | textarea | no | ≤ 2000 chars |
| الوصف بالإنجليزية | `descriptionEn` | textarea | no | ≤ 2000 chars |
| من تاريخ | `startDate` | date picker | yes | — |
| إلى تاريخ | `endDate` | date picker | yes | strictly after `startDate` |
| الحالة | `status` | read-only badge | — | system-computed; never an input |

Commands: حفظ, إلغاء. In edit mode: dates editable only while the cycle has not completed.

#### 26.S.3  Cycle chooser (modal on the orphan screens)


| Property | Value |
| --- | --- |
| Opens from | «إضافة تقرير دوري» on `#/families/:id/members` (the orphan's page) and in the orphan search results table `#/periodic-orphan-reports/orphan-search` — only when ≥ 1 cycle is In Progress |
| Shown when | exactly one active cycle → skipped entirely, the form opens directly |
| Rows | one per In Progress cycle: العنوان · الفترة (من – إلى) · حالة التقرير لهذا اليتيم (لم يتم / تم الرفع — مع حالة المراجعة) |
| Row action | not submitted → «رفع تقرير» routes to `#/periodic-orphan-reports/create?orphanId=…&cycleId=…`; already submitted → «تعديل التقرير» routes to that report's `:id/edit` |
| Cancel | closes with no navigation |

#### 26.S.4  Screen `#/reports/cycle-reports` — cycle report coverage


| Property | Value |
| --- | --- |
| Feature module | `reports` (existing, lazy-loaded) |
| Component | `CycleReportsComponent` on the shared `data-list` component |
| Filters | الدورة (mandatory, cycles of any status; default = latest) · الجمعية (HQ roles only) |
| Categories | two tabs — تم رفع تقرير (submitted) / لم يتم رفع تقرير (not submitted) — with live counts |
| Submitted grid | كود اليتيم · اسم اليتيم · الجمعية · تاريخ التقرير · حالة المراجعة (بانتظار / معتمد / مرفوض) · أمر فتح التقرير |
| Not-submitted grid | كود اليتيم · اسم اليتيم · الجمعية · تاريخ آخر تقرير خارج الدورة (إن وجد) |
| Commands | استخراج PDF · استخراج Excel |

### 26.U  Expanded scenarios

**UC-ORC-02 — Create a cycle.** Trigger: HQ actor opens إضافة دورة. Pre-conditions: authenticated
as Admin/SuperAdmin — enforced by `[Authorize(Roles = "SuperAdmin,Admin")]` on the endpoint, not by
hiding the menu. Main flow: the form collects bilingual title/description and the period; the
FluentValidation validator rejects an end date on or before the start date and empty `titleAr`;
POST /api/OrphanReportCycles persists the cycle through `IUnitOfWork` with status computed from the
dates — `NotStarted` for a future period, `InProgress` if the period already runs (no error; HQ may
legitimately open a cycle today). Post-condition: the register shows the row; nothing else changes —
creating a cycle touches no orphan data. Alternate: overlapping an existing cycle is explicitly
allowed and draws no warning beyond the visible calendar overlap in the register.

**UC-ORC-03 — Update a cycle.** Main flow: HQ edits master data or dates; on save the service
recomputes status from the new dates against now (so correcting a typo in `startDate` before it
passes keeps the cycle NotStarted). Guard rules: a Completed cycle is refused outright; shortening
`endDate` to before today on a running cycle is refused (it would strand in-window submissions);
extending a running cycle's end date is allowed. Alternate: the edit only touches display fields —
status still recomputes. Exceptions: validation failure returns the per-field error map and nothing
is written.

**UC-ORC-04 — Delete a cycle.** Main flow: HQ confirms the delete dialog; the service verifies
`Status == NotStarted` **and** zero bound reports, then soft-deletes (`IsDeleted`). Post-condition:
the cycle disappears from the register and from every chooser. Exceptions: any bound report or any
status other than NotStarted → refusal with a message naming the blocking reason (report count);
completed cycles are never deletable — they are the audit record of a reporting round.

**UC-ORC-05 — Automatic status lifecycle.** Actor: the system. Trigger:
`OrphanReportCycleStatusService`, a .NET `BackgroundService` hosted in IIROSA.Api, fires on a
configurable interval (appsettings `OrphanReportCycles:StatusCheckIntervalMinutes`, default 15).
Main flow: in one transactional pass over non-completed cycles — every `NotStarted` cycle whose
`StartDate <= now` becomes `InProgress`; every cycle whose `EndDate < now` becomes `Completed`,
stamping `StatusChangedOn`; changes commit through `IUnitOfWork` with the system identity in the
audit fields. Completion has no cascade side effect: reports are locked by comparing their cycle's
status at read/write time, not by a data sweep. Compensating control: the cycle service performs
the same date check lazily whenever cycles are read, so a delayed or crashed job degrades to
at-most-15-minutes-stale badges and still-correct gating on the submission path (which re-validates
server-side, BR-ORC-11). Post-condition: statuses converge to the calendar without any manual
intervention.

**UC-ORC-06 — Choose the active cycle.** Trigger: a Charity user views the orphan's page
(`#/families/:id/members`) or the orphan search results (`#/periodic-orphan-reports/orphan-search`).
Main flow: the screen loads the cycle report-status for the orphan context —
`GET /api/OrphanReportCycles/orphan/{orphanId}/report-status` returns every In Progress cycle with
this orphan's submission state (report id + review state when present) and the orphan must be coded
(the report form resolves its subject by code, UC-ORR-02). No active cycle → the action is absent
(hiding is UX only; the endpoints still enforce BR-ORC-11). One active cycle → «إضافة تقرير دوري»
routes straight to the form with `orphanId` + `cycleId`. Two or more → the chooser modal lists the
cycles; the user picks one and is routed identically; a cycle already reported in shows
«تعديل التقرير» instead. Post-condition: the report form opens pre-bound; nothing is persisted by
the chooser itself. Exceptions: orphan uncoded → the action is not offered for that row.

**UC-ORC-07 — Submit a periodic report within a cycle.** This is UC-ORR-03 with the cycle
pre-bound. Main flow: the chapter-14 form loads with a read-only cycle header (title + period);
the user completes the assessment and attachments and saves; POST /api/PeriodicOrphanReports
carries `cycleId`; the service validates server-side that the cycle exists, is In Progress, the
orphan is coded and belongs to the caller's charity, and that no live report already binds this
(orphan, cycle) — then persists with `CharityId` stamped from the caller, never from the payload.
The report enters the unchanged HQ review queue. Exceptions: duplicate submission → refusal pointing
at the existing report (the UI would have offered Edit); cycle completed between opening the form
and saving → refusal «انتهت الدورة» (the race the server-side check exists for). Post-condition:
exactly one live report per (orphan, cycle), awaiting review.

**UC-ORC-08 — Edit while the cycle runs.** Trigger: the charity reopens its submitted report from
the chooser, the orphan's report history, or the coverage screen. Main flow: the form loads
(UC-ORR-05); the service admits the update because the bound cycle is still In Progress; the
refusal-clearing and re-queue semantics of chapter 14 are unchanged. Exceptions: the cycle has
completed → the charity gets a read-only view (the server refuses the write with «انتهت الدورة، لا
يمكن التعديل»); HQ review decisions (UC-ORR-07/08) remain possible regardless of cycle status —
completion locks the charity's data entry, not HQ's review.

**UC-ORC-09 — View cycle report coverage.** Trigger: Charity or HQ user opens تقارير الدورة under
the reports module. Main flow: the user picks a cycle (any status — completed cycles are the
archive); the charity filter is offered to HQ roles only; POST /api/Reports/cycle-reports with the
filter body returns the orphan population of the caller's scope — server-side tenancy, a charity
never passes its own id — each row flagged submitted (with report date and review state) or not.
The two tabs render with counts; PDF/Excel export follows the standard
`POST /api/Reports/cycle-reports/export/…` convention. Post-condition: none — a read. Alternates:
cycle with zero submissions renders a fully-populated not-submitted tab; the submitted tab renders
empty with its zero count.

### 26.R  Business rules


| Rule | Statement |
| --- | --- |
| BR-ORC-01 | Cycles are HQ-defined and platform-wide; only Admin/SuperAdmin manage them, enforced by endpoint authorization. |
| BR-ORC-02 | Status is system-computed from the dates, stored on the row, and never an input on any screen or endpoint. |
| BR-ORC-03 | `EndDate > StartDate` is mandatory; overlapping cycles are permitted, including several In Progress at once. |
| BR-ORC-04 | A cycle-bound report requires: the cycle In Progress, a coded orphan, and the orphan belonging to the caller's charity. |
| BR-ORC-05 | At most one live report per (orphan, cycle) — service check backed by a filtered unique index; the existing report converts the action to Edit. |
| BR-ORC-06 | Charity-side create/edit is admitted only while the cycle is In Progress; completion locks the report against the charity. HQ accept/refuse (UC-ORR-07/08) is unaffected by cycle status. |
| BR-ORC-07 | Deletion: only a NotStarted cycle with zero bound reports; completed cycles are never deleted. |
| BR-ORC-08 | Updates: Completed cycles read-only; a running cycle's `EndDate` may be extended but not pulled before today. |
| BR-ORC-09 | Uniqueness reconciliation: the legacy monthly rule — one Child report per orphan per `(ReportYear, ReportMonth)` — continues to govern cycle-less reports and its filtered index gains `AND [CycleId] IS NULL`; cycle-bound Child reports are governed instead by the (orphan, cycle) rule of BR-ORC-05, so two overlapping cycles in the same month cannot collide. Housing reports (UC-HOU-06/08) are never cycle-bound. |
| BR-ORC-10 | `CycleId` on `PeriodicOrphanReport` is nullable forever: pre-cycle rows, ad-hoc chapter-14 reports and housing reports stay cycle-less. |
| BR-ORC-11 | Every submission/edit request re-validates the cycle window server-side at write time — the hidden button is UX, not the control. |
| BR-ORC-12 | All texts bilingual (`Ar`/`En`) on the cycle, all labels through the i18n files; API responses follow `ApiResponse<T>`. |

### 26.A  Realisation annex

**Domain (IIROSA.Domain).** `Entities/OrphanReportCycle.cs : FullAuditedEntity` — `TitleAr`
(250, required), `TitleEn` (250), `DescriptionAr`/`DescriptionEn` (2000), `StartDate`, `EndDate`,
`Status` (`Enums/OrphanReportCycleStatus.cs`: `NotStarted = 0, InProgress = 1, Completed = 2`),
`StatusChangedOn` (nullable); audit fields inherited. `Configurations/OrphanReportCycleConfiguration.cs`
— table `OrphanReportCycle` in `MappingDefaults.IIROSA_SCHEMA`, indexes on `Status` and
`(StartDate, EndDate)`. `PeriodicOrphanReport` gains `CycleId (Guid?)` + navigation
`OrphanReportCycle`; `PeriodicOrphanReportConfiguration` — FK with `DeleteBehavior.Restrict`
(a live report blocks cycle deletion, mirroring BR-ORC-07), the legacy monthly unique index filter
extended with `AND [CycleId] IS NULL` (BR-ORC-09), and a new filtered unique index on
`(OrphanId, CycleId)` with `[CycleId] IS NOT NULL AND [ChildOrParent] = 1 AND [IsDeleted] = 0`
(BR-ORC-05).

**Application (IIROSA.Application).** `IOrphanReportCycleService` / `OrphanReportCycleService` —
CRUD, `GetSelectorAsync()` (the picker read), `GetOrphanCycleReportStatusAsync(orphanId)` (the
chooser), `RefreshDueStatusesAsync(ct)` (the shared status rule) and `GetCoverageAsync(filter)`;
validators in `Validators/OrphanReportCycles/`; DTOs in `DTOs/OrphanReportCycles/`
(`OrphanReportCycleDto`, `Create…`, `Update…`, `…FilterDto`, `OrphanReportCyclePagedResult`,
`OrphanReportCycleSelectorDto`, `OrphanCycleReportStatusDto`, `CycleReportCoverage…`); AutoMapper
profile in `Profiles/`. The existing `IPeriodicOrphanReportService` create/update paths carry the
BR-ORC-04/05/06/11 checks (`ResolveOpenCycleAsync`, `EnsureCycleEditWindowAsync`).

**Background process (IIROSA.Api).** `Services/OrphanReportCycleStatusService : BackgroundService`
— the project's first hosted service, registered `builder.Services.AddHostedService<…>()` in
`Program.cs`, interval from `OrphanReportCycles:StatusCheckIntervalMinutes` (default 15, clamped
to [1, 1440]), one scoped `IServiceScopeFactory` pass per tick (startup sweep first), transitions
per UC-ORC-05. A failed sweep logs and waits for the next tick; the lazy re-check on
chooser/coverage reads keeps windows correct regardless.

**API (IIROSA.Api).** `Controllers/OrphanReportCyclesController : ApiController` — management
endpoints `[Authorize(Roles = "SuperAdmin,Admin")]`; `GET /selector` and
`GET /orphan/{orphanId}/report-status` authenticated for any signed-in user (the button gating
and chooser run on charity screens); `POST /api/Reports/cycle-reports`
`[Authorize(Roles = "SuperAdmin,Admin,Charity")]` on the existing `ReportsController`. No endpoint
takes a `userId` or caller-supplied tenant id; PDF/Excel exports of the coverage are not
implemented (deferred with the module-18 export rulings).

**Frontend (Frontend/src/app).** New lazy module `modules/orphan-report-cycles`
(`cycle-list`, `cycle-form`, standalone `cycle-report-chooser` imported directly by its hosts,
`services/`, `models/`; 4-file component shape, `OnPush` on data-driven screens, `trackBy` on
every `*ngFor`); the chooser wired into `families/family-members` (the orphan's page) and
`periodic-orphan-reports/orphan-search` (results table) behind the `hasRunningCycle` gate — one
`GET /selector` read per screen, the per-orphan decision only on click; `cycleId` rides the
create deep-link into `periodic-report-form`'s payload (UC-ORC-07); `reports/cycle-report-coverage`
component on route `#/reports/cycle-reports` with Submitted/NotSubmitted/All tabs; permission keys
`OrphanReportCycles.View/Create/Edit/Delete` in `PERMISSION_ROLES`; labels in
`assets/i18n/ar.json` + `en.json`; a dedicated HQ sidebar section for cycle management and the
coverage entry under the reports section.

**Delivery.** New scope beyond the WAR.IIROSA 232-story backlog: add a dedicated epic (cycle
entity + management UI → background service → chooser + submission binding → edit window →
coverage report), and follow the module build order Domain → Infrastructure → Application → API →
Frontend → Tests → Docs, updating `sprint-status.yaml` as stories land.

---

Shared context — system architecture, actors and roles, the sponsorship lifecycle and appendices —
is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md).
The report this cycle delivers is chapter 14's [14-UC-ORR-Orphan-Periodic-Reports](14-UC-ORR-Orphan-Periodic-Reports.md);
routing conventions are in [00-ROUTING-MAP](00-ROUTING-MAP.md).
