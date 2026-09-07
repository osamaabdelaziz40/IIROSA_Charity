# Story 19-11: Load country validation rules

| Field | Value |
| --- | --- |
| Story key | `19-11-load-country-validation-rules` |
| Epic | EP-19 — Cross-Cutting Services — الخدمات المشتركة |
| Use case | UC-SYS-11 — قواعد التحقق للدولة |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/24-UC-SYS-Cross-Cutting-Services.md` (§24.U.11 scenario) |
| Route | none — the rules ride on the country catalogue and are applied by NID inputs on every host form |
| Endpoint | `GET /api/LookupManagement/countries` + `GET …/countries/{id}` (live; extended shape) |
| Depends on | 19-7 (geography cascade shares the country read); consumers: 19-12 NID uniqueness check, family/orphan forms (EP-05/EP-08) |
| Roles | All roles (authenticated) |

## Status

done

## Story

As a signed-in user, I want to be able to load the country's validation rules قواعد التحقق
للدولة, so that national-id and country-specific fields are validated against the right
format before they reach the server.

## Acceptance Criteria

1. Given a signed-in user with an active session, when a form with a national-id field loads,
   then the selected country's validation rules are available from the country read — no
   stored data is changed.
2. Given the request is accepted, when it is served, then it is handled by the
   `countries` reads on `LookupManagementController` and the rules reach the screen without a
   page reload.
3. Given the country defines an NID pattern and/or length, when the user types a national id,
   then the input is validated client-side against that pattern/length and an invalid value
   shows the localised `validation.nationalIdInvalid` message; countries without rules keep
   the field free-form.
4. Given the country provides no rules, when the field is left unvalidated, then nothing
   breaks — the rules are optional per country (nullable columns).
5. Given the session has expired or the role is not permitted, when the function is invoked,
   then the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the Country entity + `CountryDto` carry nullable
`NationalIdPattern` / `NationalIdLength`; one clean migration is applied; at least one NID
input (family/orphan form) applies the rules client-side with the existing i18n key; the
19-12 check endpoint can consume the same columns.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- |
| Country reads | `GET countries` (paged) + `GET countries/{id}` + CRUD set | Live |
| Entity | `IIROSA.Domain/Entities/Lookups/Country.cs` — `IsoCode`, `DialingCode`, `Currency`, `FlagIcon`, `MaxTransferAmount` | Live; **no NID format/length fields** — this story adds them |
| i18n | `validation.nationalIdInvalid` key exists in ar/en JSON | Live — use it, do not add a new key |
| Consumer seed | 19-12's uniqueness check + the family/orphan NID fields (`nationalId` on orphan forms, 8-1's dedup span) | Live inputs awaiting rules |

## Verified gaps this story must fix

1. **No rule columns on Country.** Add nullable `NationalIdPattern` (regex string) and
   `NationalIdLength` (int?) to `Country` + `CountryDto` (+ create/update DTOs so HQ can
   maintain them through the existing countries CRUD screens).
2. **No client-side application.** The SPA has no NID format validation today (the i18n key
   ships unused). Wire the rules into at least the primary NID input: on the family/orphan
   form, when the selected country provides pattern/length, apply `Validators.pattern` /
   `Validators.minLength/maxLength` (or a composed validator) and surface
   `validation.nationalIdInvalid`.
3. **Seed values for operating countries.** Seed real patterns for the charities' operating
   countries where they are known (existence-guarded; e.g. Saudi 🇸🇦 10-digit numeric,
   Egyptian 🇪🇬 14-digit numeric — verify against current official formats before committing
   them; leave unknown countries null rather than guessing wrong).

## Tasks / Subtasks

- [x] **Task 1 — Entity + DTO + migration** (AC 1, 2, 4)
  - [x] `Country`: add nullable `NationalIdPattern` (string) + `NationalIdLength` (int?);
        `CountryDto` + create/update DTOs + AutoMapper mapping (existing profile)
  - [x] **One Epic-19 migration containing ONLY these two columns.** First check the last
        applied migration touching `Country` (`MaxTransferAmount` rode
        `20260824105001_Epic06_RetireConstructionHousing` — anything later in the chain wins);
        hand-prune any spurious `DropTable("ApplicationUser")` the stale snapshot emits
        (epic-15 recorded hazard). Never bundle unrelated model deltas
  - [x] `dotnet ef database update` against the dev database; verify with a targeted
        `SELECT` (sqlcmd filtered-index note: use `-I` for DML where relevant)
- [x] **Task 2 — Client-side application** (AC 3)
  - [x] Primary NID input (family/orphan form): when the form's selected country (or the
        charity's country, per the form's existing country source) provides rules, apply
        pattern/length validation + `validation.nationalIdInvalid`; no rules → field stays
        free-form (AC 4). Reuse the loaded country row — no extra endpoint call where the
        form already has the country
  - [x] Note for later hosts: keep the validator-attachment logic local + copyable (a small
        shared helper is fine if cheap; do not build a framework)
- [x] **Task 3 — Seed** (AC 3)
  - [x] Existence-guarded patterns/lengths for operating countries (verify formats; unknown
        → null)
- [x] **Task 4 — Verification** (AC 5)
  - [x] Builds green; live smoke on the private port: `countries/{id}` → 200 with the new
        nullable fields (null on unseeded rows); unauthenticated → 401; browser check on the
        wired form (invalid NID → localised message; country without rules → no validation).
        Tests excluded per the standing decision

### Review Findings

- [x] [Review][Patch] Unguarded `new RegExp(pattern)` crashes the form [Frontend/src/app/modules/families/refugee-family-form/refugee-family-form.component.ts] — an HQ-admin-entered invalid `NationalIdPattern` (trailing backslash, stray bracket…) throws inside `applyNationalIdRules` on `nationalityCountryId.valueChanges`, breaking the whole refugee form at runtime. Wrap the construction in try/catch and degrade to required-only (the AC 4 posture). **Fixed 2026-08-26** — `nationalIdRuleValidator` guards the construction; invalid patterns degrade to length-only.
- [x] [Review][Patch] Hard-coded `Validators.maxLength(14)` contradicts AC 4 free-form [Frontend/src/app/modules/families/refugee-family-form/refugee-family-form.component.ts:496,514,537,564] — four NID controls keep a static 14-char cap; Pakistan's CNIC rule is deliberately length-null (dashed 15-char form), so a valid CNIC can never be typed on a country with no length rule. Drop the static cap — the country rules govern length when present. **Fixed 2026-08-26** — static cap removed from all four controls (required stays).
- [x] [Review][Patch] Dev Agent Record garbled + unchecked evidenced sub-items [19-11 story file, Dev Agent Record; also 19-8/19-9/19-10 sub-items] — the record below "Debug Log References" is interleaved with mis-spliced debug-log content and triplicated `### Completion Notes List` / `### File List` / `## Change Log` blocks. Repair to one clean record; the checkbox sweep across 19-8/9/10/11 (evidenced-but-unchecked sub-items) rides along. **Fixed 2026-08-26** — record reconstructed cleanly; sub-items checked in all four stories against their Dev Agent Records.

## Dev Notes

### Platform rules that bind this story

- Lookups are `LookupEntity` + LOOKUP_SCHEMA; the new columns are **nullable** — no data
  backfill, no NOT-NULL trap on existing rows.
- Raw envelope on this controller (2026-08-19 standing decision); reads are global reference
  data (16-8/19-4 ruling).
- Client-side validation is UX only — **the server-side guards (19-12 uniqueness, service
  validators) remain the control**; no security decision is made in the browser (CLAUDE.md
  no-client-side-only-authorisation rule).
- Never kill the user's running API; smoke on the private port; migration hazards per the
  platform conventions above.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| NID uniqueness check (server) | 19-12 |
| Geography cascade consumers | 19-7 |
| Wiring rules into every host form | owning epics (EP-05/EP-08) — record in `deferred-work.md` |
| Lookup sweep | 19-4 |

### References

- [Source: docs/Modules/24-UC-SYS-Cross-Cutting-Services.md#24.U.11] scenario + §24.2 UC-SYS-11 row
- [Source: _bmad-output/planning-artifacts/epics.md#3.19] US-SYS-11 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/Lookups/Country.cs] current shape — no NID fields
- [Source: Frontend/src/assets/i18n/ar.json · en.json] `validation.nationalIdInvalid` (exists, unused)

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- Migration 20260825102847_Epic19_CountryNidRules applied via the Release EF route (same as 19-9); contains ONLY the two nullable Lookup.Country columns, symmetric Down.
- Live battery (61970, Development restart so the seeder runs): GET countries/18 (Egypt, +20) →
  nationalIdPattern `^\d{14}` + nationalIdLength 14; countries/14 (Pakistan, +92) →
  `^\d{5}-\d{7}-\d`, length null; countries/1 (no-rule country) → null/null; unauthenticated →
  401.
- Seed discovery: the 25-row country set has NO Saudi row (+966 absent) — the story's SA example
  does not apply to this database; DialingCode is the stable match key (the legacy IsoCode column
  holds non-ISO numeric codes: 818=Egypt, 586=Pakistan).
- Frontend tsc clean (non-spec); backend builds green (Release + bin/Smoke).

### Completion Notes List

- **Task 1 (entity + DTO + migration):** Country += nullable NationalIdPattern/NationalIdLength (UC-SYS-11 doc comments: UX-only, server controls stay authoritative); CountryDto/CreateCountryDto/UpdateCountryDto += same fields — convention maps in LookupProfile cover both directions. Isolated migration applied.
- **Task 2 (client-side application):** host-form audit first — the family form has four NID controls but NO country selector; the refugee family form (epic 7, §12.S.2) already has providerForm.nationalityCountryId (جنسية العائل, required) beside nationalId — the genuine country source. Wired there: raw CountryDto rows retained beside the options list; nationalityCountryId.valueChanges + post-load re-apply (covers edit-mode patch ordering) drive applyNationalIdRules(): ruled country -> required + nationalIdRuleValidator(pattern, length); no-rule country -> required only (AC 4). The validator raises the dedicated nationalIdInvalid error key; the shared text input renders it as validation.nationalIdInvalid (existing i18n key). Validator exported from the component file — local + copyable.
- **Task 3 (seed):** CountryNidRuleSeedData (Step 11) fills blanks only — matched by DialingCode, never overwrites HQ edits: Egypt 14-digit numeric, Pakistan CNIC 5-7-1 dashed (length deliberately null: with/without-dash counting is ambiguous). Sudan/Somalia/others left null — no stable official NID format to guess. Saudi row absent from the 25-country set.
- AC 4 shape proven live (no-rule country -> null fields -> validator-free control).
- Browser click-through on the refugee form deferred to the batched live walkthrough (epic precedent).

### File List

- Backend/src/IIROSA.Domain/Entities/Lookups/Country.cs (NationalIdPattern/NationalIdLength)
- Backend/src/IIROSA.Application/DTOs/LookupManagement/LookupDtos.cs (Country DTO trio + fields)
- Backend/src/IIROSA.Infrastructure/Data/SeedData/CountryNidRuleSeedData.cs (new — blank-filling rule seeder)
- Backend/src/IIROSA.Infrastructure/Data/SeedData/IIROSASeedDataInitializer.cs (Step 11)
- Backend/src/IIROSA.Infrastructure/Data/Migrations/20260825102847_Epic19_CountryNidRules.cs (+Designer, snapshot)
- Frontend/src/app/modules/lookup-management/models/lookup.model.ts (CountryDto fields)
- Frontend/src/app/modules/families/refugee-family-form/refugee-family-form.component.ts (rules wiring + nationalIdRuleValidator)
- Frontend/src/app/shared/components/text-input/text-input.component.ts (nationalIdInvalid error branch)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-25 | Story file created from `epics.md` US-SYS-11 and module spec §24.U.11; resolved to nullable NID pattern/length columns on Country + client-side application on the primary NID input + existence-guarded seed; migration isolated to the two columns. |
| 2026-08-25 | Implemented + verified (review-and-complete pass): nullable rule columns + isolated migration applied; rules wired client-side on the refugee provider form (nationality country source) with the dedicated i18n key; Egypt/Pakistan seeded by DialingCode (Saudi absent from the country set), others left null; live battery green. |
| 2026-08-26 | Epic-19 code review: 3 patch findings written to Review Findings (unguarded `new RegExp` crash, hard-coded maxLength(14) vs AC 4, record repair). Dev Agent Record de-garbled — the debug-log continuation was spliced through the record with duplicated sections; restored to one clean record. |
| 2026-08-26 | Review patches applied (RegExp crash guard, maxLength cap dropped — country rules govern); tsc clean on touched files. Status → done. |
