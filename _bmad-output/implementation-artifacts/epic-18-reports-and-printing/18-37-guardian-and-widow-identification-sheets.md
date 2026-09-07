# Story 18-37: Guardian and widow identification sheets

| Field | Value |
| --- | --- |
| Story key | `18-37-guardian-and-widow-identification-sheets` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-37 — كشوف تعريف العائل والأرامل |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.U.37 scenario, §23.S.10 screen — the command طباعة الاستبانة من تاريخ محدد) |
| Route | `#/reports/family-orphans` — the §23.S.10 screen (18-13's); this story adds its print command |
| Endpoint | `POST /api/Reports/guardian-identification-sheets` (board: legacy `POST /api/Reports/guardian-identification-sheets/export/pdf` + `ForWidows` / `ForWidows_Family` variants — superseded, see Dev Notes) |
| Depends on | **18-1 landed** (skeleton); **18-13 landed** (`#/reports/family-orphans` screen — its §23.S.10 print command was left deferred to this story); **18-21 landed** (client PDF service) |
| Roles | HQ roles, charity → `SuperAdmin`, `Admin`, `Charity` (`Reports.View`) |

## Status

done

## Story

As a HQ roles, I want to be able to guardian and widow identification sheets كشوف تعريف العائل
والأرامل, so that the paper document the process depends on can be produced and filed.

## Acceptance Criteria

1. Given an HQ or charity user with an active session on `#/reports/family-orphans`, when the
   actor presses طباعة الاستبانة من تاريخ محدد with valid input, then a printable document has
   been produced (client-rendered PDF) and no stored data is changed.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Reports/guardian-identification-sheets` with a typed request DTO (never untyped) and
   the result is rendered without a page reload.
3. Given the actor selects one of the three variants — all guardians (تعريف العائل), widows only
   (الأرامل), single family (أسرة محددة) — when the command runs, then the document lists that
   variant's rows.
4. Given the selection returns no row, when the document is produced, then the actor is told that
   there is nothing to produce rather than receiving an empty file.
5. Given the caller is a charity user, when the command is invoked, then only that charity's rows
   are returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload.
6. Given an HQ caller (`IsHeadOffice`) with an explicit charity id, when the command is invoked,
   then it operates on that charity's data.
7. Given the single-family variant is chosen without a family, when the command runs, then the
   request is refused with a `message` and no document is produced (§23.U.37 exception flow — a
   required parameter is missing).
8. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §23.U.37 passes end to end for all three variants; the §23.S.10 command
deferred by 18-13 is wired; scoping is enforced server-side; the caller identity used for the
print comes from the token, not the payload.

## Variant design (binding — read-only projection, no new entity, no migration)

`GuardianIdentificationSheetFilterDto` in `DTOs/Reports/Reports.cs`: `Variant` (enum
`AllGuardians | WidowsOnly | SingleFamily`), `CharityId?` (HQ-only passthrough), `Date?` (start
date — limits rows to register entries created on/after it, per §23.S.10's تاريخ بدء التقرير),
`FamilyId?` (required when `Variant = SingleFamily`). The legacy `userId` parameter is **resolved
server-side from `ICurrentUserService.UserId`** — it is never accepted from the payload.

| Variant | Rows | Key columns (sources) |
| --- | --- | --- |
| `AllGuardians` | `Provider` rows of the charity's families | `GuardianName` (`Provider.FullName`), `NationalId`, `Phone`, `RelationshipToFamily`, `Job`, `FamilyCode` (`Family.Code`), `CharityName` (`NameAr ?? NameEn`) |
| `WidowsOnly` | `Mother` rows of the charity's families (alive mothers — verify the widow discriminator against the register at dev time) | `WidowName` (`Mother.FullName`), `NationalId`, `Phone`, `DateOfBirth`, `FamilyCode`, `CharityName` |
| `SingleFamily` | guardians + widows of ONE family (`FamilyId`) | the union of the above, grouped under the family |

## Tasks / Subtasks

- [x] **Task 1 — DTO + validator** (AC 2, 7)
  - [x] Filter + row DTOs in `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — clean
        names, no `FK_*` wire keys; `Application/Validators/Reports/GuardianIdentificationFilterValidator.cs`:
        `Variant` `IsInEnum()`, `FamilyId` `NotNull()` when variant is `SingleFamily`, `Date` shape
- [x] **Task 2 — Service projections** (AC 3–6)
  - [x] `IReportService.GetGuardianIdentificationSheetsAsync(filter)` + implementation in
        `ReportService.cs`: `ResolveCharityScope(filter.CharityId)` first (pin-never-widen), then
        one query per variant over `IUnitOfWork` repositories (`Provider`/`Mother` joined to
        `Family`), `Date` applied as `CreatedOn >= date` when supplied, ordered by family code
  - [x] Stamp the producing user from `ICurrentUserService.UserId` in the returned result header
        data (never from the payload); soft-delete scoping stays with the global query filter
- [x] **Task 3 — API endpoint** (AC 2, 7, 8)
  - [x] `[HttpPost("guardian-identification-sheets")]` in `ReportsController.cs`;
        `[Authorize(Roles = "SuperAdmin,Admin,Charity")]`; ValidationException → 400
        `{ message, errors }` ladder (OfficeProjectManagementController.cs:89-101 shape);
        catch-all → 500 `{ message }` — no `ApiResponse<T>`
- [x] **Task 4 — Wire the deferred §23.S.10 command** (AC 1, 3, 4)
  - [x] On `#/reports/family-orphans` (18-13's component): add the command button
        طباعة الاستبانة من تاريخ محدد with a variant selector, family dropdown for the
        single-family variant (`GET /api/Families`, label `nameAr ?? nameEn` or family code),
        charity dropdown for HQ callers (`GET /api/Charities`) and the date picker 18-13's filter
        bar already carries
  - [x] `getGuardianIdentificationSheets()` in `modules/reports/services/report.service.ts`;
        POST → rows → `report-pdf.service.ts` (18-21) renders the identification sheet; zero rows
        → "nothing to produce" message, no file
- [x] **Task 5 — i18n** — `reports.guardianIdentification.*` block (command label, variant labels,
      all sheet columns, nothing-to-produce, missing-family message) in **both**
      `Frontend/src/assets/i18n/ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–8)
  - [x] Live check: anonymous → 401; HQ with explicit `charityId` → that charity's rows only;
        charity user → own rows regardless of payload; `SingleFamily` without `familyId` → 400
        `message`; no matching rows → empty result + UI message (Arabic payloads via UTF-8 files)
  - [x] `dotnet build` + `npm run build` green (live-API MSB3021/3027 lock caveat; ng-serve
        stale-bundle grep caveat); no migration; tests excluded per standing decision

## Dev Notes

### Cross-reference and supersession (recorded)

18-13 left the §23.S.10 command طباعة الاستبانة من تاريخ محدد (`PrintCharityIdentifications`)
deferred to exactly this story — wiring it is in scope here and nowhere else. Epic-wide decision:
the legacy server-side `POST …/export/pdf` (and its `ForWidows` / `ForWidows_Family` variant
endpoints) are **superseded** by one JSON endpoint + client-side jsPDF; do not build or name any
`…/export/pdf` route.

### Platform rules that bind this story

- Read-only projection — **no new entity, no EF migration, no write path**; the "flagged as
  printed" post-condition of the generic template has no recorded field in this module, so none is
  stamped (§23.U.37 says "where the module records printing" — it does not).
- Raw envelope + anonymous `{ message }` / `{ message, errors }` — NOT `ApiResponse<T>` (15-1
  ruling, architecture.md §10); `ControllerBase` + `[Authorize]` + `[Route("api/[controller]")]`.
- camelCase wire; no `FK_*` DTO keys; lookup labels `NameAr ?? NameEn`; FluentValidation in the
  service; caller identity from `ICurrentUserService`, never from the payload.
- No client-side-only authorisation — the route guard is convenience; the endpoint's
  `[Authorize]` is the control.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Generic browser preview modal (review the PDF before saving) | 18-40 |
| Excel export of the identification sheets | 18-41 |
| The other §23.S.10 commands (`GetFamilyOrphans`, `GetFamilyOrphansDetails`) | 18-13 |
| The family-orphan list by date print (different use case on the same screen) | 18-39 |
| Widows-requiring-sponsorship analytical report | 18-06 (US-RPT-06) |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.37] scenario — params
  `charityId, date?, userId`, HQ cross-charity read, missing-parameter exception
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.10] the host screen and the
  deferred command طباعة الاستبانة من تاريخ محدد
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-37 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/Provider.cs] guardian row source (`FullName`,
  `NationalId`, `RelationshipToFamily`)
- [Source: Backend/src/IIROSA.Domain/Entities/Mother.cs] widow row source (`FullName`,
  `IsAlive`, `DateOfBirth`)
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-2-create-a-transfer.md] reviewed print-adjacent
  story — ValidationException ladder and typed-DTO discipline

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code harness)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` → 0 Error(s) (pre-existing warnings only).
- `npm run build` → exit 0, 0 `Error:` lines (first pass).

### Completion Notes List

- **Supersession executed on the host screen (recorded):** the §23.S.10 screen's legacy
  UC-FAM-14 identification buttons (`exportSheet('guardian-identification-sheets' |
  'widow-identification-sheets')` + free-text family code) were REPLACED by the single 18-37
  command — variant selector (تعريف العائل / الأرامل / أسرة محددة), family dropdown for
  SingleFamily, the bar's existing charity dropdown + date picker. The screen no longer names
  any `/export/pdf` route (per Dev Notes); the tracking-sheet print (UC-FAM-14) is untouched,
  and its private printer's now-dead identification branches were trimmed.
- **Widow discriminator (recorded, per "verify at dev time"):** the 18-6/18-35 definition —
  mother alive + family registered + husband (Father) death date present.
- **Start-date semantics (recorded):** `Date` filters the ROW ANCHOR's `CreatedOn`
  (Provider rows for AllGuardians, Mother rows for WidowsOnly); SingleFamily takes the whole
  family — the explicit family choice is the frame, the date does not cut its members.
- **SingleFamily = the union:** the widows query + the providers query over the one family,
  merged and ordered familyCode → name (the sheet renders the union under the family).
- **ProducedBy:** `ICurrentUserService` exposes no UserName/Email — the producing user is
  stamped as `UserId.ToString()` from the token (the legacy `userId` payload key is ruled out).
- **Manual `!IsDeleted` checks everywhere** — the Framework's global soft-delete query filter is
  commented out (the 18-36 finding); this file's explicit-check convention IS the scoping.
- **Charity scope:** provider-rooted inline ladder (18-22/24 form); widow/mother branch reuses
  18-6's `ApplyMotherCharityScopeAsync` — a charity caller naming another charity's family is
  clamped (pin-never-widen).
- **Family dropdown:** loaded lazily on first switch to SingleFamily
  (`GET /api/Families` pageSize 500, label = family code) — the whole register is not fetched
  for the two list variants.
- **AC 7 client mirror:** أسرة محددة without a family is refused with the missing-family toast
  before any request; the server 400s again regardless (validator `NotNull().When(SingleFamily)`).
- The legacy `reports.print.printGuardians/printWidows/familyCode` i18n keys are now unused by
  this screen — left in place (harmless; no key deleted).
- **Task 6 live check deferred** to the epic's consolidated private-instance smoke
  (127.0.0.1:60970 pattern) — the live-check box stays open until then; tests excluded per the
  standing user decision.

### File List

Backend:
- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `GuardianIdentificationSheetVariant`
  enum, `GuardianIdentificationSheetFilterDto`, `GuardianIdentificationRowDto`,
  `GuardianIdentificationSheetDto` (whole-selection print payload + server-stamped producer).
- `Backend/src/IIROSA.Application/Validators/Reports/GuardianIdentificationFilterValidator.cs` — NEW.
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetGuardianIdentificationSheetsAsync`.
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — implementation (variant fork,
  mother+provider queries, ladders, union ordering, `ResolveIdentificationCharityNamesAsync`)
  + validator DI.
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `POST guardian-identification-sheets`
  (HQ + Charity roles; raw envelope; 400 errors-map / 500 `{ message }` ladder).

Frontend:
- `Frontend/src/app/modules/reports/models/report.model.ts` — `GuardianIdentificationVariant`,
  `GuardianIdentificationFilter`, `GuardianIdentificationRow`, `GuardianIdentificationSheet`.
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getGuardianIdentificationSheets`.
- `Frontend/src/app/modules/reports/family-follow-up-report/family-follow-up-report.component.ts` · `.html` · `.scss` · `.spec.ts`
  — the deferred §23.S.10 command wired (variant selector + family dropdown + print builder via
  18-21's printSheet); legacy identification path removed; tracking printer trimmed.
- `Frontend/src/assets/i18n/ar.json`, `en.json` — `reports.guardianIdentification` (22 keys ×2).

### Consolidated live smoke — 2026-08-25 (private instance 127.0.0.1:60970; the user's live API on 60960 untouched)

- Anonymous → 401; HQ + explicit `charityId` → that charity's rows only.
- Charity user → own rows regardless of payload.
- `SingleFamily` without `familyId` → 400 `message`.
- No matching rows → empty result + UI message (Arabic payloads via UTF-8 files).
## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-37 and module spec §23.U.37 / §23.S.10; three-variant projection designed, legacy `/export/pdf` family recorded as superseded, deferred 18-13 command cross-referenced. |
| 2026-08-25 | Tasks 1–5 + Task 6's build subtask implemented and checked; legacy host-screen identification path migrated onto the variant-keyed endpoint; widow-discriminator / date-semantics / ProducedBy rulings recorded; Dev Agent Record written; Status → in-progress (live check pending the epic's consolidated smoke). |
| 2026-08-25 | Live smoke passed (auth, scoping both directions, SingleFamily guard, empty path). Status → review. |
| 2026-08-26 | Code-review records pass: File List path corrections (repo-rooted paths, brace/compound globs expanded to the real files) — record hygiene only, no code change. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
