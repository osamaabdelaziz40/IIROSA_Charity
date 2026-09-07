# Epic 7 Code Review Report — all stories (7-1 … 7-4)

**Date:** 2026-08-24 · **Mode:** full (spec + stories + context) · **Scope:** union of the four story File Lists vs HEAD
**Method:** BMAD three-layer adversarial review, chunked by layer — Group A backend core (5,478 diff lines, 34 files) + A2 configs addendum (295 lines, orchestrator-reviewed), Group B frontend refugee screens (3,001 lines, 15 files), Group C i18n + shared wiring (3,966 lines, 7 files). Nine subagent reviews (3 per group); one transient API failure (Group A Edge Case Hunter) recovered by resume. All Critical/High findings independently re-verified against the live code before classification.

**Totals:** 4 `decision-needed` · 29 `patch` · 27 `defer` (cross-epic / pre-existing) · 9 `dismissed` (refuted or noise) — from 118 raw findings.

Raw layer output: `findings-{A,B,C}-{blind-hunter.md|edge-case-hunter.json|acceptance-auditor.md}` in this directory.

---

## Decision needed (4) — resolved with the user before patching

| # | Finding | Choice |
| --- | --- | --- |
| N1 | **[7-4] §12.S.2 labels the معيل/أبناء ID field «رقم جواز السفر» (passport); the implementation uses «الرقم القومي» (national ID)** — and the duplicate-معيل rule keys on `NationalId`. For a refugee register the spec's passport semantics may be deliberate. | passport semantics vs keep national-ID + record deviation |
| N2 | **[7-3] Four mandatory name-part fields (الاسم الاول/الثانى/الثالث/الرباعي) collapsed into one `fullName` input** (server: ≥2 words). Father-form precedent is also a single input; storage is one column. | 4 joined inputs (spec-faithful) vs keep single (record deviation) |
| N3 | **[7-3] Optional §12.S.2 fields dropped unrecorded: ابن الصورة photo upload** (shared attachment component; 7-3's binding promised it) **and معيل إسم الأسرة** (`Provider` has no FamilyName column). | implement now vs follow-up story vs record deviation |
| N4 | **[7-3/7-4] 8 Arabic label deviations from §12.S.2's verbatim labels** (e.g. «المدينة/القرية» vs «القرية / الحي», «حالة السكن» vs «حالة محتويات السكن», «متوفى» checkbox vs «على قيد الحياة»). | align verbatim vs record accepted |

## Patch (29) — unambiguous fixes, verified in code

### 7-1 list
- **P1 [High]** `charityName` is null for every family created via `POST /api/Families` — create stamps only `FK_CharityId`, never the `CharityId` mirror the `Charity` navigation binds to (`FamilyService.cs:177` vs `FamilyConfiguration.cs:87`); the §12.S.1 الجمعية column and detail resolved name can never populate. Fix: stamp both columns at create (the transfer method already models the correct dual write, `FamilyService.cs:1289-1290`). [A-blind#1 + A-auditor#1 + A-edge#8]
- **P11 [Low]** `IX_Family_FamilyType` index (7-1 Task 1 deliverable, deferred to 7-3's migration, which was absorbed un-minted) never landed. Additive migration — check the last APPLIED migration first (parallel-session chain).
- **P19 [Medium]** List loads: no `takeUntil` on `getFamilies`/`getCharities` subscriptions, no request sequencing (stale-response race), failed reload leaves previous rows rendered. `refugee-family-list.component.ts:139-183`. [B-blind#3 + B-edge#12/13/14/15]
- **P23a [Low]** Hard-coded English `'Failed to load refugee families'` in the error path. [B-blind#12 + B-auditor#8]

### 7-2 search
- **P26 [Medium]** Typed code search does not trim — `" 1234"` returns empty where the orphan exists (`FamilyService.cs:1085-1088`). Trim the exact-code term.
- **P25 [Low]** `family.model.ts` `searchType` doc comment lists 6 values; the shipped contract (and server switch) is 7 + `all` incl. `provider`. Update the comment.

### 7-3 register/create
- **P2 [Medium]** `Enum.TryParse` result discarded on create — an unparseable `familyType` persists discriminator **0**, vanishing from every register filter and leaking `"0"` on the wire (`FamilyService.cs:126-130,181`). Guard like the filter path does. [A-auditor#2 + A-edge#1]
- **P5 [Medium]** The معيل block is not mandatory on refugee create — every provider rule sits inside `When(Provider != null)`; a guardian-less refugee family persists with `HeadOfFamily` unset. Add `RuleFor(x => x.Provider).NotNull()` (validator is refugee-only). [A-auditor#3]
- **P6 [Low]** `CreateRefugeeFamilyValidator.cs:55` — `Must(name => name.Split(...))` NREs (500) when FullName is null: FluentValidation's default cascade runs both rules. Null-safe the predicate. [A-edge#23]
- **P7a [High]** Create is fail-open for a Charity-role token whose charity claim is null/unparseable: `FamiliesController.CreateFamily` pins `dto.CharityId` only `if (userCharityId.HasValue)` (`:315`) — arbitrary charity minted. Add the D4 guard used by the five orphan endpoints (`IsInRole("Charity") && GetUserCharityId() == null → Forbid()`). [A-blind#4 + A-auditor#6]
- **P10 [Low]** `IValidator<CreateFamilyDto>` is DI-ambiguous: `CreateHousingFamilyValidator` is also auto-registered by assembly scan; "refugee wins" is registration-order luck. Make the registration deterministic (RemoveAll + AddTransient in `ServiceCollectionExtensions`). [A-blind#19 + A-auditor#10]
- **P12 [High]** Mandatory الحالة الصحية missing from child AND companion rows (§12.S.2 both mandatory). `Orphan.HealthStatusId` and `Relative.HealthStatusId` both exist — pure frontend + i18n (control, `getHealthStatuses()`, payloads, detail columns, keys). [B-auditor#1 + C-auditor#1]
- **P13a [Medium]** Child `nationalId` maxLength-only, companion `nationalId` unvalidated — §12.S.2 marks both mandatory, max 14. Add `required` + `maxLength(14)` (create + edit row builders). [B-auditor#2]
- **P14 [Medium]** Mandatory provider العلاقة control (الاب/الام/علاقة أخرى) absent — `Provider.MainRelation` column exists unused. Add the select → `MainRelation`; «علاقة أخرى» reveals نوعها (spec `OtherRelations()`). [B-auditor#4]
- **P15 [Medium]** Read-only §12.S.2 displays الدخل الكلى / نصيب الفرد / عدد الأبناء rendered nowhere (`perMemberShare` added to the model but unbound). Add to detail + i18n (epic-6 housing form already ships the same trio). [B-auditor#5 + C-auditor#2]
- **P20 [Low]** `save()` create subscribe lacks `takeUntil(destroy$)` (`:580`) — late handlers on a destroyed component. [B-blind#4 + B-edge#8]
- **P21 [Medium]** Catalogue GET failures silently empty dropdowns; if `getRelations()` failed, edit re-serialises نوعها as `undefined` — wiping stored data. Add error surfacing + `pendingRelationName` fallback in `optionName`. [B-blind#5 + B-edge#4/5]
- **P22 [Low]** `trackByIndex` on removable FormArray rows — mid-row delete reshuffles identities; track by the hidden `id` control. [B-blind#13]
- **P24 [Low]** «إضافة أسرة لاجئة» menu entry carries no `Families.Create` gate (inherits `Families.View`). [C-auditor#4]

### 7-4 view/update/detail
- **P3 [High]** `UpdateFamilyAsync` restamps the discriminator at `:861-865` BEFORE the refugee effective-state validation at `:882` — a PUT with `familyType: "Regular"` escapes the §12.S.2 mandatory contract, and cross-register mutation contradicts the immutability invariant. Make the discriminator immutable on the generic PUT (verified safe: the refugee edit screen re-sends the stored value). [A-blind#5 + A-auditor#4 + A-edge#10]
- **P4 [Medium]** `UpdateProviderAsync` runs no validator and its `!= null` copies accept empty strings — `nationalId: ""` blanks the duplicate-rule key with a 200. Whitespace-guard the string copies / reject blank on mandatory provider fields. [A-auditor#5 + A-edge#9]
- **P7b [High]** Provider GET (`[Authorize]` only, `:1101`) and PUT lack the D4 null-claim guard, and the service gate (`userRole == "Charity" && userCharityId.HasValue && …`) fails OPEN when the claim is null. Fail closed at both layers. [A-blind#1/#4 + A-edge#5/#19]
- **P8 [Medium]** Provider GET/PUT anchor on unfiltered `GetByIdAsync` — a soft-deleted family's guardian stays readable/editable. Add the `IsDeleted` check (sibling methods already model it). [A-blind#20 + A-auditor#12 + A-edge#6/7]
- **P9 [Low]** HeadOfFamily mirror on provider rename fires for every register — guard with `family.FamilyType == Refugee`. [A-blind#22 residual]
- **P16 [Medium]** No `familyType` guard on detail/edit screens — a Regular family renders on the refugee screens / can be PUT through the refugee path. Redirect non-Refugee ids. [B-edge#2/#11]
- **P17 [Medium]** Detail `forkJoin` is all-or-nothing: one failed read (e.g. provider 404) blanks the page below the header with no retry. Per-stream `catchError` fallbacks + on-screen error state. [B-blind#7 + B-edge#1]
- **P18 [Low]** Raw `nationalityCountryId` rendered as the provider's الجنسية — resolve via `countryOptions`. [B-blind#6 + B-auditor#9 + B-edge#3]
- **P23b [Low]** Edit-load failures surface as `refugeeFormSaveFailed` — add a load-failed message. [B-blind#10]

## Defer (27) — real but owned elsewhere / pre-existing

Full list with owners in `deferred-work.md`. Highlights: seasonal-aid received-flag tenancy (epic 18); housing create-via-generic-endpoint allocation bypass, children mass-soft-delete on omitted `orphans` node, sponsored-child soft-delete, counter drift, missing `Provider`/`HousingBuilding`/`HousingFlat` includes (epic 6); member-control audit/concurrency gaps, orphan-coding eligibility HQ-null scope, phone-check counting soft-deleted orphans, BR-07 check-then-save race, transfer `Guid.Empty` sentinel (epic 5); `ex.Message` in 500s + mixed response envelopes (pre-existing house pattern / recorded 15-1 deviation); lookup `PageSize` 1000 caps (cross). **Frontend/i18n regressions introduced by concurrent sessions** (missions/officeDev menu role drift, orphan-payments dead menu, deleted `missionDetails` key still referenced by 3 templates + route title, `noRolesAvailable` Arabic gap, `technicalSupport.title` rename behavioural change, login silent no-token, employees ar/en asymmetry, assorted typos) — flagged for those sessions, not epic 7.

## Dismissed (9) — refuted or noise

Lookup auth downgrade (auditor verified all 16 write actions SuperAdminOnly); "no EF migrations" (minted & applied in prior sessions, live-verified); "types never defined" (all exist; solution compiles); "routing imports missing components" (untracked dirs exist); "i18n keys missing" (90/90 parity verified both languages); "`IIROSA.*` menu keys missing" (present in both); §12.S.1 father/mother "dead columns" (the spec itself specifies them — line 66); Bootstrap-4 utility classes (legacy-majority house style, theme supports); `saveEdit` teardown-as-defect (symmetric teardown is intended; the real asymmetry is P20).
