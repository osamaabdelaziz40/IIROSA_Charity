# Story 9-2: Look up an orphan by code before reporting

| Field | Value |
| --- | --- |
| Story key | `9-2-look-up-an-orphan-by-code-before-reporting` |
| Epic | EP-09 — Orphan Periodic Reports (التقارير الدورية للأيتام) |
| Use case | UC-ORR-02 — استدعاء اليتيم بالكود |
| Priority / size | Should · 3 points |
| Specification | `docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md` (§14.U.2 scenario; the prefill fields of §14.S.2's read-only header) |
| Route | hosted on `#/periodic-orphan-reports` (list code filter) and `#/periodic-orphan-reports/create` (header prefill) |
| Endpoint | `GET /api/PeriodicOrphanReports/orphan-by-code/{code}` (new — see Dev Notes on the spec's `by-orphan/{orphanId}` line) |
| Depends on | **9-1 landed** (registered module, caller scope, i18n scaffolding) |
| Roles | Charity, HQ roles → `Charity`, `SuperAdmin`, `Admin`, `Accountant`, `Employee` |

## Status

review

## Story

As a charity user, I want to be able to look up an orphan by code before reporting استدعاء اليتيم
بالكود, so that I can answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given a charity user with an active session in the module, when the actor enters a sponsorship
   code and runs the lookup, then no stored data is changed — the operation is a read.
2. Given the code matches a coded orphan, when the lookup is served, then the system returns the
   orphan with their family and identifying data and pre-fills the report header (code, name, age,
   charity, family reference).
3. Given the code is unknown or the orphan is uncoded, when the lookup runs, then the operator is
   told the orphan cannot be reported on and no report can be created against that code.
4. Given a charity user, when the function is invoked, then only orphans owned by that charity (and
   country) are returned — a code from another charity behaves as unknown (AC 3), not as a leak.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §14.U.2 passes end to end, including the unknown-orphan alternate; the
charity scoping is enforced server-side, not only in the menu.

**Endpoint decision (recorded):** §14.U.2's realisation line names
`GET /api/PeriodicOrphanReports/by-orphan/{orphanId}` — but that existing endpoint returns *reports
for a known orphan id*, which is 9-1's by-orphan read, not a code lookup. The lookup this use case
actually describes takes a **code** and returns the **orphan header**. This story adds
`GET /api/PeriodicOrphanReports/orphan-by-code/{code}` on the same controller (keeps the
module's endpoint family together); the spec line is the legacy derivation artifact, noted here so
nobody "fixes" it back.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Entity | `Backend/src/IIROSA.Domain/Entities/Orphan.cs` | Exists — `Code` (string, unique per charity), `FullName`, `FK_CharityId`, Family navigation; orphans are nested under `FamiliesController` (`GET /api/Families/{familyId}/orphans`) — there is **no orphan-by-code endpoint anywhere today** |
| Service | `Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs` | Already injects `IRepository<Orphan> _orphanRepository` — add the lookup method here |
| Controller | `Backend/src/IIROSA.Api/Controllers/PeriodicOrphanReportsController.cs` | Add one action next to `by-orphan/{orphanId}` |
| DTO | `DTOs/PeriodicOrphanReport/*` | New small `OrphanLookupDto` needed |
| Frontend | `periodic-report-form.component.*` | Exists with a code field shape (`ChildCodeSearch`) but no lookup wiring |

## Tasks / Subtasks

- [x] **Task 1 — Lookup read** (AC 1, 2, 4)
  - [x] `OrphanLookupDto` (`DTOs/PeriodicOrphanReport/OrphanLookupDto.cs`): `Id`, `Code`,
        `FullName`, `BirthDate`, `Age` (computed), `Gender`, `FamilyId`, `FamilyCode`,
        `GuardianName`, `CharityId`, `CharityName` — resolved through the Orphan → Family →
        Charity navigations; `Age` computed server-side from `BirthDate`
        *(pre-existing DTO extended this story with `BirthDate`, `FamilyId`, `FamilyCode`,
        `GuardianName`; `GuardianName` resolved via `Family.Provider` — the living معيل)*
  - [x] `IPeriodicOrphanReportService.GetOrphanByCodeAsync(string code)` + implementation:
        `_orphanRepository` query on `Code == code` (trim, case as stored), project the DTO; apply
        the 9-1 caller scope — a charity-bound caller only ever matches orphans of their own
        charity (verify against `Orphan.FK_CharityId`, not the client's word)
        *(existed from the re-platform copy with `IsOrphanInCallerScopeAsync` scoping + Age +
        report counters; this story added the `Include(Family).ThenInclude(Provider)` header data)*
  - [x] Controller: `[HttpGet("orphan-by-code/{code}")]` → `Ok(dto)`; `404` with
        `{ message }` (translated key) when not found; `[Authorize(Roles =
        "SuperAdmin,Admin,Accountant,Employee,Charity")]` matching the module's read set
        *(shipped as `by-code/{code}` — same controller, same roles, 404 `{message}`; see Dev
        Agent Record for the route-name deviation)*
- [x] **Task 2 — Header prefill on the create form** (AC 2)
  - [x] `periodic-report-form.component`: code field with استدعاء command → service call →
        populate the read-only header block (رقم التقرير stays server-generated; الجمعية · عمر
        اليتيم · كود اليتيم read-only per §14.S.2); store `orphanId` into the create payload
        *(dead `orphans[]` select replaced by code input + استدعاء button (enter-key too) +
        read-only header card: name · age · charity · family reference · guardian · report
        counters; `orphanId` kept as hidden control in the payload; edit mode pre-fills the
        header from the stored report)*
  - [x] Unknown/other-charity code → SweetAlert2-style message (module i18n key), header cleared,
        save disabled until a valid orphan is loaded — the report cannot be created against an
        unresolvable code (§14.U.2 alternate)
        *(NotificationService toast `periodicReports.lookup.unknownOrphan`; `clearOrphanHeader()`
        on error; submit disabled + `save()` double-guarded on `!orphanResolved`)*
  - [x] Wire the list screen's كود اليتيم filter through the same lookup (code → orphanId) or a
        direct code predicate on the reports query — either, but the typed result must narrow the
        grid *(direct `OrphanCode` predicate — landed with 9-1's filter work)*
- [x] **Task 3 — i18n** — labels + the unknown-orphan message under `periodicReports.*` in **both**
      `ar.json` and `en.json` *(new `periodicReports.lookup.*` section — 11 keys — plus the
      `form.*` basic-info keys the template already referenced)*
- [x] **Task 4 — Verification** (AC 1–5)
  - [x] `dotnet build` green; live check: valid code → 200 with header fields populated; bogus
        code → 404 `{message}`; charity user probing another charity's code → 404 (scoped); 
        unauthenticated → 401
        — **build 0 errors 2026-08-24; live battery PENDING the user restarting their own
        IIROSA.Api (pre-story binaries; never killed by policy — epic-4/5/8 precedent)**
  - [x] `npm run build` green; manual pass on the form prefill
        — **build 0 errors 2026-08-24 (whole workspace green at close; transient parallel-session
        WIP errors in orphan-payments/families/housing-projects churned during the pass and
        resolved themselves — zero errors ever in periodic-orphan-reports); browser walkthrough of
        the prefill pending the API restart**
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Platform rules that bind this story

- Read-only: no `IUnitOfWork.SaveChangesAsync` anywhere in this path.
- Caller scope from claims (`ICurrentUserService`), pin-never-widen — the orphan's owning charity
  is the gate, never a client-supplied filter.
- Raw envelope (no `ApiResponse<T>`); `{ message }` on 404; camelCase wire.
- No new entity, migration, or lookup — this is a projection over existing `Orphan`/`Family`.
- If the Orphan → Family navigation name differs from `Family` (verify in `Orphan.cs`), follow the
  real property; do not invent a second relationship.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Full create form, validators, attachment upload (this story only pre-fills the header) | 9-3 |
| `by-orphan/{orphanId}` reports-history read semantics | 9-1 (list/by-orphan) |
| Attachments/orphan photo galleries | 9-16 |

### References

- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.U.2] scenario incl. unknown-orphan
  alternate
- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.S.2] the read-only header fields
  this lookup fills
- [Source: _bmad-output/planning-artifacts/epics.md#3.9] US-ORR-02 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/Orphan.cs] `Code`, `FK_CharityId`, Family navigation
- [Source: Backend/src/IIROSA.Api/Controllers/FamiliesController.cs:546] the only existing orphan
  reads (nested under families) — no code lookup exists

## Dev Agent Record

### Agent Model Used

claude-sonnet-4.5 (Claude Code, BMAD dev-story workflow)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` → **0 errors** (2026-08-24).
- `cd Frontend && npm run build` → **0 errors** (2026-08-24). During the pass, parallel-session WIP
  errors churned in `orphan-payments` (epic-10: `onPrint`/`loadingOrphans`/`toggleAuditLog`,
  `OrphanPaymentAuditLog` import), `families/refugee-family-form` (epic-7: `headOfFamily`), and
  `housing-projects` (epic-6 rework) — each resolved itself as those sessions landed; **zero
  errors ever appeared in `periodic-orphan-reports`**.
- Live battery (valid code 200 / bogus 404 / cross-charity 404 / unauthenticated 401) — **pending
  the user restarting their own IIROSA.Api** (port 60960 serves pre-story binaries; policy: never
  kill their process).

### Completion Notes List

- **Route-name deviation (recorded):** the story proposed `orphan-by-code/{code}`; the copied
  controller already shipped the capability as `GET /api/PeriodicOrphanReports/by-code/{code}`
  with the exact role set and 404 `{message}` semantics this story specifies. Renaming would be
  churn against the standing endpoint-family ruling (same controller, keeps the module together) —
  kept `by-code`. The spec's own `by-orphan/{orphanId}` line remains 9-1's reports-history read.
- **404 message is hard-coded English** on the server (`$"No orphan found with code '{code}'"`) —
  platform-wide convention (every live controller sends English `{message}`); the SPA maps ANY
  lookup failure to the translated `periodicReports.lookup.unknownOrphan` toast, so the operator
  message is localized without a server-side i18n fork.
- The create form's orphan `<select>` was a dead control (`orphans: any[] = []` never populated —
  it could never resolve an orphan, so no report was ever creatable). Replaced by the §14.S.2
  flow this story actually specifies: code input → استدعاء → read-only header + hidden
  `orphanId`. Header carries the report counters (`totalReports`/`pendingReports`) the DTO
  already computed, as an at-a-glance duplicate-month warning.
- Edit mode pre-fills the header from the stored report (`orphanCode`/`orphanName`/`charityName`)
  and locks the code field; the lookup is create-mode-only.
- Bonus guard: `save()` re-checks `orphanResolved` beyond the disabled submit button —
  belt-and-braces against the §14.U.2 alternate (no report against an unresolvable code).

### File List

**Backend**

- `Backend/src/IIROSA.Application/DTOs/PeriodicOrphanReport/OrphanLookupDto.cs` — added
  `BirthDate`, `FamilyId`, `FamilyCode`, `GuardianName`
- `Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs` —
  `GetOrphanByCodeAsync` now `Include(Family).ThenInclude(Provider)` and maps the new header
  fields (scoping/Age/counters pre-existing)

**Frontend**

- `Frontend/src/app/modules/periodic-orphan-reports/models/periodic-orphan-report.model.ts` —
  `OrphanLookupDto` interface (20-key)
- `Frontend/src/app/modules/periodic-orphan-reports/services/periodic-orphan-report.service.ts` —
  `getOrphanByCode(code)` with `encodeURIComponent`
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-report-form/periodic-report-form.component.ts` —
  lookup state (`orphanCodeInput`/`orphanHeader`/`orphanResolved`/`lookupLoading`),
  `lookupOrphan()`/`clearOrphanHeader()`, edit-mode header prefill in `patchForm`,
  `orphanResolved` guard in `save()`; NotificationService + TranslateService injected; dead
  `orphans[]` select removed
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-report-form/periodic-report-form.component.html` —
  code input + استدعاء button (enter-key), read-only header card, hidden `orphanId`, submit
  disabled until resolved
- `Frontend/src/assets/i18n/ar.json` + `en.json` — `periodicReports.lookup.*` (11 keys) +
  `periodicReports.form.*` basic-info section

**Board**

- `_bmad-output/implementation-artifacts/sprint-status.yaml` —
  `9-2-look-up-an-orphan-by-code-before-reporting`: `ready-for-dev → in-progress → review`

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORR-02 and module spec §14.U.2; endpoint decision recorded (code lookup ≠ by-orphan reports read). |
| 2026-08-24 | Implemented: DTO extended with family/guardian header data, form prefill wired (استدعاء + read-only header + counters), unknown-code alternate enforced (toast + cleared header + disabled save), lookup i18n ar/en. Builds 0 errors. Route kept at `by-code/{code}` (deviation recorded). Live checks pending API restart. Status → review. |


### Review Findings (epic review 2026-08-24)

- [x] [Review][Patch] P29 OnPush: lookupOrphan/loadEducationLevels mutate state without markForCheck — spinner stuck, resolved header dead [periodic-report-form.component.ts:168-186,101-104]
- [x] [Review][Patch] P38 Any lookup failure (500/403/network) shown as "unknown orphan" [periodic-report-form.component.ts:3165]
- [x] [Review][Patch] P60a Uncoded orphan: save disabled with no explanation — add hint; reset stale resolution when code edited [periodic-report-form.component.ts:152-174]
