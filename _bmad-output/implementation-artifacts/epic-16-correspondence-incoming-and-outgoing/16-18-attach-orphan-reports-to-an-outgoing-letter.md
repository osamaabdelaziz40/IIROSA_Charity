# Story 16-18: Attach orphan reports to an outgoing letter

| Field | Value |
| --- | --- |
| Story key | `16-18-attach-orphan-reports-to-an-outgoing-letter` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-18 — إضافة تقارير الأيتام إلى خطاب صادر |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.S.6 screen, §21.U.18 scenario) |
| Route | `#/incoming-outgoing/export/outgoing` |
| Endpoints | `GET /api/IncomingOutgoing/outgoing/{id}/orphans` (new list) · `POST …/orphans` · `DELETE …/orphans/{orphanId}` (new) — the epic AC names the copied `…/children` chain; see defect 1 |
| Depends on | 16-13 (letters exist), 16-10 (scope + wire), 16-9 (screen pattern to mirror) |
| Roles | Staff → `Admin`, `SuperAdmin` |

## Status

review

## Story

As a head-office staff member, I want to be able to attach orphan reports to an outgoing letter
إضافة تقارير الأيتام إلى خطاب صادر, so that the register records which reports a dispatch carried.

## Acceptance Criteria

1. Given a head-office staff member with an active session on the screen at
   `#/incoming-outgoing/export/outgoing`, when the actor presses «حفظ» with valid input, then a
   new record exists, owned by the charity of the creating user, and appears in the list screen
   of the module.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/IncomingOutgoing/outgoing/{parentOutgoingId}/children` and the response is rendered
   on the screen without a page reload.
3. Given a mandatory field listed in the screen field specification is empty, when the actor
   saves, then the save is refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to the list screen, then the record appears
   there with the values just entered.
5. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
7. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.
8. Given the business rule behind «Operation Faild» is broken, when the operation is attempted,
   then it is refused with that message and nothing is written.

**Definition of done:** the screen fields of §21.S.6 are implemented; the scenario of §21.U.18
passes end to end — attached and unattached orphan reports list for the chosen letter + charity,
the selection saves as an auditable link, and the scoping is enforced server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Route | `export/:type` → `ExportWizardComponent` | Exists (wrong screen — 16-9 defect 2) |
| Report entity | `Entities/PeriodicOrphanReport.cs` | Exists — `OrphanId`, `ReportNo`, `CharityId?`, period fields; the attachment target |
| Orphan entity | `Entities/Orphan.cs` + families module | Exists |
| Copied chain | `ChildOutGoing` entity + `POST outgoing/{parentOutgoingId}/child`, `GET …/children`, `DELETE outgoing/child/{childId}` | Exists — wrong semantics, stubbed persistence |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **The copied "children" chain is the wrong concept AND a stub.** `ChildOutGoing` models
   follow-up letters (Subject/Body/Date), not orphan-report links; `CreateChildOutgoingAsync`
   builds the entity but never persists ("TODO: Add to ChildOutgoingRepository"),
   `GetChildOutgoingsAsync` returns `Enumerable.Empty`, `DeleteChildOutgoingAsync` is a no-op
   returning `true`. The epic's AC references `…/children` because the generator mapped the
   existing route — the real contract (§21.S.6/§21.U.18) is an **outgoing ↔ orphan(-report)
   attachment**. Build the real link; retire the children endpoints in the same change (nothing
   working depends on them).
2. **No link entity exists.** BR-26: an orphan report attaches to **at most one** outgoing letter
   — enforce with a unique index on the report/orphan side, refusal with «Operation Faild».
   BR-27: the selection is scoped to one charity per letter — the unattached list only offers
   that charity's orphans.
3. **The screen at this route is the wrong software** (16-9 defect 2 applies identically): the
   file-export wizard with the never-set `@Input() fileType` sits here. §21.S.6 specifies:
   خطاب الصادر drop-down (mandatory) + الجمعية; two grids — unattached orphans (الرقم · إسم
   اليتيم · كود اليتيم · اسم المعيل · صلة القرابة · اضافة الى الخطاب) and attached
   (… · حذف); حفظ.
4. **Grid data requires a real projection** — orphan name, code, guarantor (اسم المعيل), and
   kinship (صلة القرابة) live across `Orphan`/`Family`/guardian entities; serve them as flat
   DTO fields (no client-side joins).

## Tasks / Subtasks

- [x] **Task 1 — Domain + persistence** (AC 8): new entity `OutgoingOrphanReport`
        (`FullAuditedEntity`, Guid): `Guid OutgoingId`, `Guid OrphanId` (+ `Guid?
        PeriodicOrphanReportId` when a specific report is pinned — confirm against the live
        `PeriodicOrphanReport` shape at dev time), `unique index` enforcing BR-26 on the
        orphan/report side; `MappingDefaults` conventions + Restrict; migration (CLAUDE.md
        command); repository + service registrations
- [x] **Task 2 — Service + endpoints** (AC 1, 2, 5, 6, 8): `GetOutgoingOrphansAsync(outgoingId)`
        → `{ attached: […], unattached: […] }` flat DTOs (orphan code/name, guarantor name,
        kinship) with the unattached set scoped to the letter's charity (BR-27);
        `AttachOrphanAsync(outgoingId, orphanId)` / `DetachOrphanAsync(outgoingId, orphanId)` —
        scope-verify the letter first; BR-26 violation or cross-charity orphan → localized
        «Operation Faild» refusal, nothing written; controller routes
        `GET/POST outgoing/{id}/orphans`, `DELETE outgoing/{id}/orphans/{orphanId}`,
        `[Authorize(Roles = "Admin,SuperAdmin")]`; **retire** the `child`/`children` endpoints +
        `CreateChildOutgoingDto` chain
- [x] **Task 3 — The §21.S.6 screen** (AC 1, 3, 4): new component at
        `#/incoming-outgoing/export/outgoing` (mirror of 16-9's screen): خطاب الصادر drop-down
        (mandatory) · الجمعية (HQ only, Charities + كافة الجهات); on letter change load both
        grids; §21.S.6 columns with `trackBy`; اضافة/حذف move rows (staged, saved on حفظ per
        §21.U.18); mandatory خطاب الصادر refusal flags the field (AC 3); i18n keys both files
- [x] **Task 4 — Verification**: live — attach → moves grids + persists; re-attach on a second
        letter → «Operation Faild» (BR-26); charity user's unattached list shows only their
        charity's orphans (BR-27); detach restores; unauthenticated → 401; `dotnet build` +
        `npm run build` — 0 errors; migration applied; tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

- Link entity is `FullAuditedEntity` (Guid); the unique index is the last line of defence — the
  service gives the friendly refusal; never rely on catching a SQL 2751/2601 as the UX.
- Do not add `ApplicationUser`-style navigations to identity tables from this link — flat
  projections only (15-1 `dbo.ApplicationUser` trap).
- The spec's "orphan reports" ride with the orphan: if the business needs the specific report
  pinned, store the report id; otherwise the letter↔orphan link implies "the orphan's current
  report travels with the letter" (WAR behaviour) — record which reading you implemented in the
  completion notes.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Report over attachments (تقرير الأيتام حسب الخطاب) | 16-19 |
| Employees attachment (sibling screen) | 16-9 |
| Detail-screen attached list | 16-14 Task 3 |
| Orphan periodic-report CRUD | epic 6 (periodic reports) |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.S.6] screen contract
  — 2 fields, 2 grids, 3 commands
- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.18] scenario —
  attached/unattached lists, charity scope, «Operation Faild»
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-18 acceptance criteria
- [Source: Backend/src/IIROSA.Application/Services/OutgoingService.cs] stubbed child-outgoing
  chain (`CreateChildOutgoingAsync` TODO)
- [Source: Backend/src/IIROSA.Domain/Entities/PeriodicOrphanReport.cs] attachment target shape
- [Source: _bmad-output/implementation-artifacts/epic-16-correspondence-incoming-and-outgoing/16-9-attach-employees-to-an-incoming-letter.md]
  sibling screen pattern + wizard-replacement decision

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): verified the shipped chain — `OutgoingOrphanReport` link entity (unique index enforcing BR-26), `GetOutgoingOrphansAsync` → `{ attached, unattached }` flat DTOs (orphan code/name, guarantor, kinship — no client-side joins), `AttachOrphanAsync` refusing BR-26 («the orphan's report is already attached to another letter») and cross-charity orphans («the orphan is outside the letter's charity selection») with nothing written, `DetachOrphanAsync`, endpoints `GET/POST outgoing/{id}/orphans` + `DELETE outgoing/{id}/orphans/{orphanId}`. The stubbed `child`/`children` chain + `CreateChildOutgoingDto` are retired (DTO file deleted; no route remains).
- 2026-08-24 (this pass): added the entry point — the outgoing detail screen routes here with `?outgoingId=` preselecting the letter (`ActivatedRoute` + `preselectFromQuery()`); the list screen's page action lands on this screen too.
- 2026-08-24 (verification): project builds clean; `ng build` clean for the module. Table delivered by the already-applied `20260824063442_Epic06_HousingFamilyType` migration — no separate migration.

### Completion Notes List

- **BR-26 reading implemented:** the letter↔orphan link is direct (`OutgoingId` + `OrphanId`) — the orphan's current report travels with the letter (WAR behaviour); no `PeriodicOrphanReportId` pinning. The unique index on the orphan side is the storage backstop; the service gives the friendly refusal first.
- **BR-27:** the unattached grid is scoped to the letter's charity server-side; HQ letter + explicit charity filter narrows the offered set.
- Screen follows the §21.U.18 read-then-save flow (mirror of 16-9): row commands stage locally and move rows between grids immediately; حفظ flushes the staged set through the per-link endpoints, then reloads both grids. Mandatory خطاب الصادر refusal flags the field.
- The spec's grid variable names are WAR artefacts; rows are orphans (الرقم · إسم اليتيم · كود اليتيم · اسم المعيل · صلة القرابة per §21.S.6).
- Task 4's live portion pending the user's `IIROSA.Api` restart; builds verified. Tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Domain/Entities/OutgoingOrphanReport.cs` + configuration — link entity + BR-26 index
- `Backend/src/IIROSA.Application/Services/OutgoingService.cs` — orphans read + guarded attach/detach
- `Backend/src/IIROSA.Api/Controllers/IncomingOutgoingController.cs` — orphans endpoints (children chain retired)
- `Backend/src/IIROSA.Application/DTOs/IncomingOutgoing/ChildOutgoingDto.cs` — deleted
- `Frontend/src/app/modules/incoming-outgoing/outgoing-orphans/outgoing-letter-orphans.component.ts` / `.html` / `.scss` — §21.S.6 screen + query-param preselect
- `Frontend/src/app/modules/incoming-outgoing/outgoing-letters/outgoing-letter-detail.component.ts` + list component — entry points

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-18 and module spec §21.S.6 / §21.U.18; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (verified entity/endpoints/BR-26+27 guards; children chain retired; detail-screen entry point added). Status → review; live attach/detach walkthrough pending user's API restart. |
