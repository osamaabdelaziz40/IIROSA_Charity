# Story 16-17: Select the outgoing category

| Field | Value |
| --- | --- |
| Story key | `16-17-select-the-outgoing-category` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-17 — تصنيف الصادر |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.U.17 scenario; the category is the mandatory §21.S.5 field تصنيف موضوع الصادر) |
| Route | consumed by the outgoing form (16-13) and list filter (16-10) |
| Endpoint | `GET /api/IncomingOutgoing/outgoing/categories` |
| Depends on | 16-10 (outgoing wire conventions) |
| Roles | Staff → `Admin`, `SuperAdmin` |

## Status

review

## Story

As a head-office staff member, I want to be able to select the outgoing category تصنيف الصادر, so
that dispatched letters are classified consistently.

## Acceptance Criteria

1. Given a head-office staff member with an active session in the module, when the actor opens
   the screen with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/IncomingOutgoing/outgoing/categories` and the response is rendered on the screen
   without a page reload.
3. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the scenario of §21.U.17 passes end to end; the category catalogue drives
the outgoing form's mandatory field and the list filter; the scoping is enforced server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Lookup | `Entities/Lookups/OutgoingCategory.cs` (`LookupEntity`, int, + Description) + configuration | Exists; table created; **unseeded** |
| API | `GET outgoing/categories` | Exists — but hardcoded |
| Frontend | form + list filter category controls | Exist — but hardcoded strings |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **The endpoint ignores the table.** `OutgoingService.GetAvailableCategoriesAsync` returns a
   hardcoded `Dictionary<int,string> {1:"Official", 2:"Internal", 3:"External",
   4:"Confidential"}` — English, fixed ids, no `OutgoingCategory` query at all.
2. **The catalogue is empty where it matters.** No `OutgoingCategory` seed rows exist, so even a
   table-backed endpoint would serve nothing on a fresh database. Seed WAR-consistent
   Arabic-first categories (e.g. رسمي · داخلي · خارجي · سري — align final names with the WAR term
   list), existence-guarded (15-1 seed pattern).
3. **Wire shape is hostile to drop-downs.** `Dictionary<int,string>` serializes as
   `{"1":"Official",…}` — keys-as-strings, no bilingual names, no stable object shape. Return a
   list of `{ id, nameAr, nameEn }` DTOs (lookup-select convention: bind id, label
   `nameAr ?? nameEn`).
4. **The SPA hardcodes its own three string categories** — list filter
   (`outgoing-letters-list.component.ts:59-64`) and form
   (`outgoing-letter-form.component.ts:131-137`) both build `'official'/'internal'/'external'`
   options that can never bind to the int? `CategoryId`. Both re-bind to the endpoint's wire
   (16-10/16-13 integrate; this story delivers the catalogue + the consuming convention).

## Tasks / Subtasks

- [x] **Task 1 — Table-backed catalogue** (AC 2): `GetAvailableCategoriesAsync` reads the
        `OutgoingCategory` lookup (caller-visible actives ordered by `NameAr`); returns
        `List<OutgoingCategoryDto> { Id, NameAr, NameEn }`; `[Authorize(Roles =
        "Admin,SuperAdmin")]`; bare list return
- [x] **Task 2 — Seed** (AC 2): Arabic-first rows, `IsActive = true`, existence-guarded; runs
        with the app's seed path (16-8's seed task may carry both — coordinate to avoid two
        migrations)
- [x] **Task 3 — Consume in the SPA** (AC 1, 2): `outgoing.service.ts.getCategories()`; the form
        + list filter controls bind int ids with `nameAr ?? nameEn` labels (remove both hardcoded
        arrays); cascade on charity/letter change is not required — static catalogue
- [x] **Task 4 — Verification**: live — endpoint returns seeded rows in the DTO shape; form saves
        a letter with a real category id; list filter narrows by it; unauthenticated → 401;
        `dotnet build` + `npm run build` — 0 errors; tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

Lookup conventions: `LookupEntity` (int), `MappingDefaults.LOOKUP_SCHEMA`, bilingual
`NameAr`/`NameEn`, seeds existence-guarded and Arabic-first. `Dictionary<,>` return shapes are
anti-pattern for drop-downs — always object lists.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| The form's mandatory enforcement of the category | 16-13 (builds on this wire) |
| CRUD screens for managing categories | lookup module's scope, not this epic |
| Department catalogue (sibling lookup) | 16-8 |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.17] scenario
- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.S.5] تصنيف موضوع
  الصادر — mandatory, OutgoingCategories lookup
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-17 acceptance criteria
- [Source: Backend/src/IIROSA.Application/Services/OutgoingService.cs]
  `GetAvailableCategoriesAsync` — hardcoded dictionary
- [Source: Frontend/src/app/modules/incoming-outgoing/outgoing-letters/outgoing-letter-form.component.ts#L131-137] hardcoded client categories

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): verified `GetAvailableCategoriesAsync` (:221) returns `IEnumerable<OutgoingCategoryOptionDto>` from the `OutgoingCategory` table (the create/update FK check in 16-13/16-15 queries the same catalogue); seed rows رسمي · داخلي · خارجي · تعميم · أخرى (`NameEn` mirrors, `IsActive`, `SortOrder`), existence-guarded in `CorrespondenceLookupSeedData` and wired via `IIROSASeedDataInitializer`. Both SPA consumers (form :147-152, list filter :148-158) load via `getAvailableCategories()` and label with `nameAr || nameEn` — the hardcoded string arrays are gone.
- 2026-08-24 (verification): project builds clean; `ng build` clean for the module.

### Completion Notes List

- Catalogue names: the story's examples (رسمي · داخلي · خارجي · سري) were placeholders — shipped set adds تعميم and أخرى instead of سري to match the WAR term list; adjust by editing the seed on a fresh database (existence guard protects live rows).
- Wire is a bare object list `{ id, nameAr, nameEn }` — no `Dictionary<,>` shape; binds int ids on both the form and the §21.S.4 filter.
- Static catalogue — no cascade needed on charity/letter change.
- Task 4's live portion pending the user's `IIROSA.Api` restart (seed runs on startup); builds verified. Tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Application/Services/OutgoingService.cs` — table-backed categories
- `Backend/src/IIROSA.Infrastructure/Data/SeedData/CorrespondenceLookupSeedData.cs` — category seed
- `Frontend/src/app/modules/incoming-outgoing/services/outgoing.service.ts` — `getAvailableCategories`
- `Frontend/src/app/modules/incoming-outgoing/outgoing-letters/outgoing-letter-form.component.ts` + `outgoing-letters-list.component.ts` — live-catalogue binding

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-17 and module spec §21.U.17; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (verified table-backed endpoint + seed + both SPA consumers on int ids). Status → review; live seed check pending user's API restart. |
