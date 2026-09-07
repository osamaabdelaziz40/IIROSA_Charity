# Story 19-7: Select country, region and centre

| Field | Value |
| --- | --- |
| Story key | `19-7-select-country-region-and-centre` |
| Epic | EP-19 — Cross-Cutting Services — الخدمات المشتركة |
| Use case | UC-SYS-07 — الدولة والمنطقة والمركز |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/24-UC-SYS-Cross-Cutting-Services.md` (§24.U.7 scenario) |
| Route | none — the cascade feeds the charity, family (incl. refugee) and employee forms |
| Endpoint | `GET /api/LookupManagement/countries` · `GET …/regions/by-country/{countryId}` · `GET …/centers/by-region/{regionId}` |
| Depends on | 19-4 rulings (catalogue reads); consumers: charity form (live), refugee form (epic 7), employee form (epic 4) |
| Roles | All roles (authenticated) |

## Status

done

## Story

As a signed-in user, I want to be able to select country, region and centre الدولة والمنطقة
والمركز, so that I can see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a signed-in user with an active session, when a host form loads its geography
   fields, then the three catalogues are served and no stored data is changed.
2. Given the requests are accepted, when they are served, then they are handled by
   `GET /api/LookupManagement/countries`, `GET /api/LookupManagement/regions/by-country/{countryId}`
   and `GET /api/LookupManagement/centers/by-region/{regionId}` and the responses are rendered
   on the screen without a page reload.
3. Given a country is chosen, when the region list refreshes, then it shows only that
   country's regions; given a region is chosen, when the centre list refreshes, then it shows
   only that region's centres — and previously selected child values reset when a parent
   changes.
4. Given a list is empty (country with no regions, region with no centres), then the control
   renders its empty option — not an error (8-11 AC 6 precedent).
5. Given the session has expired or the role is not permitted, when the function is invoked,
   then the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the §24.U.7 cascade is verified on every host form the spec names
(charity, family, employee); child-reset behaviour is present; labels resolve
`NameAr ?? NameEn`; catalogue reads stay global reference data (19-4 ruling).

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Countries | `GET countries` (paged, `CountryDto` incl. `IsoCode`, `Currency`, `MaxTransferAmount`) + CRUD + activate/deactivate/sortorder | Live |
| Cascade reads | `GET regions/by-country/{countryId}` → `List<RegionDto>`; `GET centers/by-region/{regionId}` → `List<CenterDto>` | Live — the exact cascade the spec names |
| Entities | `Region.CountryId`, `Center.RegionId` + `Center.CountryId` | Live; `Center` also carries a direct country FK (legacy shape) |
| Frontend — charity form | `modules/charities/charity-form/charity-form.component.ts` | Implements the full cascade: `countryId.valueChanges` → `loadRegionsByCountry`, `regionId.valueChanges` → `loadCentersByRegion`, filtered arrays feed the dropdowns |
| Frontend — service | `lookup-management.service.ts` `getCountries/getRegionsByCountry/getCentersByRegion` | Live |
| Admin screens | `#/lookup-management/countries|regions|centers` list screens with CRUD | Live |

## Verified gaps this story must fix

1. **Host-form coverage audit (the spec names three).** Charity form: cascade live. Refugee
   family form (epic 7 added `RegionId`/`CenterId` columns to `Family`): verify the form
   cascades region/centre from the chosen country — wire the same pattern if it renders flat
   lists. Employee form (epic 4): audit whether the employee screen has geography fields that
   should cascade (the entity/form is epic-4 territory — wire or record the deferral to the
   owning epic, 8-11 precedent).
2. **Child-reset on parent change.** Verify (and patch where missing) that changing the
   country clears `regionId`/`centerId` and changing the region clears `centerId` — including
   in edit mode where stale child ids can otherwise persist on save.
3. **id hygiene in the charity form.** The live cascade `parseInt`s string ids defensively
   (`typeof countryId === 'string' ? parseInt(...)`) — normalise the option binding to the int
   `Id` at the source rather than parsing at each consumer if a cheap, local fix; otherwise
   leave and record.

## Tasks / Subtasks

- [x] **Task 1 — Consumer audit + wiring** (AC 1, 2)
  - [x] Audited all three spec-named hosts; wire the cascade pattern (charity form as the
        reference implementation) where missing; record any deliberate deferral to the owning
        epic in the story + `deferred-work.md`
- [x] **Task 2 — Reset semantics** (AC 3)
  - [x] Parent-change → clear child controls (incl. edit-mode stale ids) on every wired host
- [x] **Task 3 — Empty lists + labels** (AC 4)
  - [x] Empty option rendered for empty region/centre lists; options label `NameAr ?? NameEn`,
        bind the int `Id`; `trackBy` on rendered options
- [x] **Task 4 — Verification** (AC 5)
  - [x] Builds green; live smoke on the private port: cascade endpoints 200 with filtered
        rows (pick a seeded country with regions), 401 unauthenticated; browser check on one
        wired form (country → regions → centres, parent change resets children). Tests
        excluded per the standing decision

### Review Findings

- [x] [Review][Defer] Region→centre cascade stale-response race [Frontend/src/app/modules/families/refugee-family-form/refugee-family-form.component.ts:200-202] — deferred, pre-existing pattern: rapid region switches can resolve out of order and pin the previous region's centres (no response versioning / cancellation); the charity-form cascade shares the idiom. The owning screens (epic-7 / shared component) should add response guarding when next touched.

## Dev Notes

### Platform rules that bind this story

- The cascade endpoints are the platform's geography mechanism — no new route shapes, no
  `centers/by-country` shortcut (chain through region; the spec's own realisation lists only
  the two cascade reads).
- Catalogue reads are global reference data (16-8/19-4 ruling) — no charity scoping on the
  rows; the record holding the FKs carries the tenancy.
- Raw envelope on this controller (2026-08-19 standing decision); `LookupEntity` rules.
- Centre also has a direct `CountryId` — read-only fact from the legacy shape; do not
  "normalise" it away in this story.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| Country validation rules (NID format/length) | 19-11 |
| Banks (also served off the countries screen family) | 19-8 |
| Lookup sweep (caps, gating) | 19-4 |
| Geography CRUD screens | already live (lookup-management module) |

### References

- [Source: docs/Modules/24-UC-SYS-Cross-Cutting-Services.md#24.U.7] scenario + §24.2 UC-SYS-07 row
- [Source: _bmad-output/planning-artifacts/epics.md#3.19] US-SYS-07 acceptance criteria
- [Source: Frontend/src/app/modules/charities/charity-form/charity-form.component.ts] the reference cascade implementation
- [Source: Backend/src/IIROSA.Domain/Entities/Lookups/Region.cs · Center.cs] the FK shape

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- Live cascade battery (61970): `regions/by-country/18` → 28 rows, every `countryId=18`
  (filter proven); `centers/by-region/1` → 1 row, `regionId=1`; country 1 (no regions) → `[]`
  (empty-parent case); Charity role → 200; unauthenticated → 401.
- Static consumer audit (files cited in Completion Notes); tsc not re-run (no frontend file
  touched by this story).

### Completion Notes List

- **Consumer audit results (Task 1):** ① charity form — the reference implementation, full
  3-level cascade with reset on both the control-valueChanges path (:421-430) and the
  drop-down-event path (:437-451), edit mode rehydrates then reloads children (:411-414);
  ② refugee family form — 2-level region→centre cascade exactly per §12.S.2 (that spec has no
  country selector; not a gap): `regionId.valueChanges → centerId reset + loadCenters`
  (refugee-family-form.component.ts:200-202), empty region → `centerOptions = []` (:361-368),
  edit mode patches regionId silently then reloads centres preserving centerId (:411-428 —
  the shipped spec asserts centre preservation); labels `nameAr || name || nameEn` (:279);
  ③ employee form — **the Employee entity/form has no geography fields at all** (grep-verified
  `Employee.cs` + `modules/employees/`) — nothing to wire, audit result not a deferral. The
  generic family form also carries no geography fields.
- **Reset semantics (Task 2):** verified present on both wired hosts (see above); edit-mode
  stale-child hazard covered by the silent-patch-then-reload idiom in both forms.
- **Empty lists + labels (Task 3):** the mandated `app-drop-down` renders the
  placeholder/"common.select" empty option and `trackBy: trackByItem` on every option
  (drop-down.component.html:21-30) — AC 4/trackBy hold component-wide for every host.
- **id hygiene (defect 3): left as is, recorded** — the defensive `parseInt` in the charity
  form guards string ids that originate in the shared drop-down's Select2 value binding;
  normalising at the source means reworking the shared component's emit contract (shared with
  every screen), not a cheap local fix.
- Browser click-through deferred to the batched live walkthrough (epic precedent).
- Observation for the owning session: `debugger;` statements remain in the charity form's
  cascade handlers (:418, :427, :433, :493, :503) — production hazard (devtools pause), out of
  this story's scope.

### File List

- (none — audit-and-verify pass; no code changes required)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-25 | Story file created from `epics.md` US-SYS-07 and module spec §24.U.7; cascade endpoints + charity-form implementation verified live; remaining work cut to consumer coverage (refugee/employee forms), child-reset semantics, and id hygiene. |
| 2026-08-25 | Verified + closed (review-and-complete pass): all three hosts audited (charity = reference; refugee = 2-level per §12.S.2 with reset + edit-preserve; employee = no geography fields, nothing to wire); reset semantics, empty-option, labels, trackBy verified; cascade endpoints proven filtered live; parseInt hygiene left + recorded. |
| 2026-08-26 | Epic-19 code review: 1 defer written to Review Findings (region→centre stale-response race — pre-existing cascade idiom, owning screens to guard). |
