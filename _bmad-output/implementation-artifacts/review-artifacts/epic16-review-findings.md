# Epic 16 Code Review — Trialed Findings (2026-08-24)

Review of all 19 stories (UC-COR-01…19, Correspondence — Incoming & Outgoing).
Input: `epic16-review-input.diff` (12,126 lines) + working-tree verification.
Layers: Blind Hunter (24 findings) + Edge Case Hunter (15, JSON) + Acceptance Auditor (10 + per-story
verdicts). Raw 49 findings → 33 unified after dedupe. Every load-bearing claim re-verified directly in
the repo during triage.

> **Known input gap (owned in triage):** the diff artifact omitted `ar.json` / `en.json` /
> `auth.service.ts` (construction grep miss). Verified directly instead: all new i18n keys present in
> both files; `PERMISSION_ROLES` carries the `IncomingOutgoing.*` set with `Delete: ['SuperAdmin']`.

## Root cause (dominates everything else)

**There is no soft-delete query filter in this platform.** The only `HasQueryFilter` lives at
`Backend/Framework/Framework.Core/Data/ModelBuilderExtensions.cs:83` — inside a block comment opened at
line 5 (dead code). Story Dev Notes claiming "global query filter hides flagged rows" were written
against a guarantee that does not exist. Every epic-16 read/count/guard sees `IsDeleted = 1` rows.

## Decision-needed (1) — RESOLVED 2026-08-24

### D1 — NULL-charity HQ letters sit outside every scope and defeat BR-27
`blind+auditor` — `IncomingService.cs:135`, `OutgoingService.cs:123/139`

HQ callers without a `CharityId` claim create letters with `FK_CharityId = NULL`: invisible to every
charity-pinned read; `IsLetterNumberUniqueAsync(null)` spans ALL charities;
`AttachOrphanAsync` calls `GetUnattachedOrphansAsync(null)` → all charities' orphans offered → BR-27
membership check passes for any orphan. The create forms have **no** الجمعية field (§21.S.2/§21.S.5
field lists), so WAR behaviour implies HQ-owned letters are a real concept (the department seed even
includes رئاسة المكتب) — but 16-4/16-13 AC 1 says "owned by the charity of the creating user".

**User decision: allow HQ-owned NULL-charity letters (WAR-faithful)** → converted to patch **P14**
with explicit semantics:
- HQ (no charity claim) keeps creating NULL-charity letters; they are HQ's own correspondence
  (رئاسة المكتب) — reads stay as-is (HQ unpinned sees all incl. NULL; charity users never see them;
  HQ + explicit charity filter narrows to that charity).
- `IsLetterNumberUniqueAsync` scoped to the effective charity — a NULL-charity letter is checked only
  against other NULL-charity letters, never against every charity's numbers.
- BR-27 made explicit: charity-pinned letters may attach only their charity's orphans (refusal
  unchanged); NULL-charity (HQ) letters may attach any charity's orphan — deliberate HQ
  cross-charity dispatch, documented in code.

## Patch (14)

| # | Finding | Location |
| --- | --- | --- |
| P1 | **Soft delete filters nothing** — add `!IsDeleted` predicates to all four correspondence repositories: list/detail reads, `CountOutgoingRepliesAsync`/`CountIncomingRepliesAsync` (else letters become permanently undeletable), `IsLetterNumberUniqueAsync`, `IncomingEmployeeRepository` (Exists/linked-ids — else a detached employee can never re-attach), `OutgoingOrphanReportRepository` (IsOrphanAttached/GetByOutgoing/GetUnattached — else BR-26 blocks forever after a detach), and the 16-19 report counts. Contained to epic-16 repos; do NOT activate a global filter while parallel epic sessions run. | 4 repos under `Infrastructure/Data/Repository/` |
| P2 | **Employee candidates not charity-scoped** — `GetAvailableEmployeesAsync` filters only `IsActive && !linked`; add the letter's charity criterion via `ApplicationUser.CharityId` (§21.U.9 "for the chosen charity and letter"; 16-9 Task 2). | `IncomingEmployeeRepository.cs:47-48` |
| P3 | **Incoming mandatory fields unenforced** — رقم الخطاب (LetterNumber), تاريخ الخطاب (LetterDate), الحاله (Status) have no `NotEmpty` rules (only conditional max-length / LessThanOrEqual); service silently defaults Status. §21.S.2 mandatory list + 16-4 Task 2. | `Validators/Correspondence/IncomingValidators.cs:37-44,80-87` |
| P4 | **Chicken-and-egg deadlock** — incoming create requires `OutgoingId` NotEmpty AND outgoing create requires `IncomingId` NotEmpty → at an empty database neither first letter can exist. Spec resolves it: §21.S.5 line 263 offers the outgoing reply-to **with `(+ -)` empty option** while §21.S.2 line 154 keeps incoming's mandatory (no empty option). Drop `NotEmpty` from the outgoing validators' `IncomingId` (create+update) and skip reply-to checks when null; frontend `-` option already exists. | `Validators/Correspondence/OutgoingValidators.cs:32-33,63-64` |
| P5 | **"Offending field is flagged" not implemented** — `getErrorMessage` returns `''` on both branches; zero `is-invalid`/`invalid-feedback` bindings in either form template; toasts read transport junk (`error.message`) instead of `error.error.message`/`errors`. Violates AC 3 of 16-4/16-6/16-13/16-15. Wire control error state + map server 400 `errors` keys to form controls. | `incoming-letter-form.component.ts:311-315`, both form templates |
| P6 | **Serial race, no unique backstop** — `(FK_CharityId, Year, Serial)` indexes are non-unique; `GetNextSerialAsync` is unlocked Max+1 → two concurrent creates persist the same serial. Add filtered unique index `WHERE [IsDeleted] = 0` (pattern the epic itself used for the link tables) + catch the duplicate → friendly re-derive/refuse. **Needs a new migration — check the parallel-session chain (last applied migration) before adding.** | `IncomingConfiguration.cs` / `OutgoingConfiguration.cs` |
| P7 | **Country dimension of tenancy missing** — `ApplyCallerScope` pins `CharityId` only; CLAUDE.md and §21 pre-condition 3 demand charity **and** country; `MissionService.ApplyCallerScope` is the platform pattern. Mirror it (country via `Charity.CountryId`) in both services + the report filter. | `IncomingService.cs:376-392`, `OutgoingService.cs` |
| P8 | **اليتيم مضاف للتقرير is a constant** — 16-19 report rows are pre-filtered to letters already carrying the orphan, so `OrphanAttached = true` always renders ✓. Row set should include matching letters regardless; compute the real flag. | `OutgoingOrphanReportRepository.cs:~10499` region (`GetOrphanReportAsync`) |
| P9 | **Null-forgiving operators → 500 on legacy rows** — `dto.Date!.Value.Year` / `incoming.Year!.Value` throw `InvalidOperationException` when copied legacy rows carry NULL. Guard and fail gracefully. | update paths, both services |
| P10 | **Inactive category bricks letter updates** — FK check uses `GetActiveAsync()`, so editing a letter whose category was later deactivated always 400s ("does not exist or is inactive") until reactivation. Existence check should accept any row (department check already uses `GetByIdAsync`); dropdowns stay active-only. | `OutgoingService.cs:394` |
| P11 | **ExcelJS failures are silent** — dynamic `import()` / `writeBuffer` rejections unhandled in both list extracts + 16-19 `extractAll`. Wrap and toast. | 3 export paths |
| P12 | **Edit-mode `patchValue` races options load** — reply-to summary block/selection can silently miss when patch fires before `allIncomingLetters` resolves. Await loads before patching (or re-patch on load). | `outgoing-letter-form.component.ts` edit path |
| P13 | **Advisory serial stale on year change** — create forms don't re-fetch the next serial when the year control changes. | both create forms |
| P14 | **HQ-owned NULL-charity letters made explicit** (D1 resolution): uniqueness scoped to the effective charity (NULL checked against NULL only); BR-27 explicit — charity letters restricted to their charity's orphans, NULL-charity (HQ) letters may attach any orphan by design; document HQ-letter semantics in code. | `IncomingService.cs`, `OutgoingService.cs` |

## Deferred (10) — appended to `deferred-work.md`

1. Year immutability vs date-year divergence on update (documented design; revisit if business wants
   serial re-anchoring).
2. Employee keyspace split — form's الموظف المسئول validates against `Employee`, 16-9 attachment uses
   `ApplicationUser`; each internally consistent with its spec text, but a product-level unification
   question. [`IncomingService.cs:420` vs `IncomingEmployeeRepository.cs:47`]
3. Attachment download via plain `<a href>` vs JWT Authorization header — platform-wide shared
   attachment pattern, not an epic-16 regression.
4. Stale permission identifiers (`IncomingOutgoing.Import/Export/ViewHistory`) retired with the wizard.
5. Reply-to dropdowns cap at `pageSize: 1000` — typeahead needed at scale / after legacy data import.
6. Unattached grid materializes all link ids in memory; extract-all `pageSize = totalCount` unbounded —
   perf hardening.
7. Four competing serial-padding implementations — cleanup to one helper.
8. Three new components (`incoming-employees`, `outgoing-orphans`, `outgoing-orphans-report`) missing
   `.spec.ts` and `OnPush` — tests excluded per the standing decision; OnPush is follow-up.
9. Unbounded `pageNumber`/`pageSize` clamp — interacts with the by-design extract-all; needs an exempt
   export read.
10. Outgoing Excel extract drops some grid columns — fidelity polish.

## Dismissed (8, with evidence)

1. "No migration in diff" — `20260824063442_Epic06_HousingFamilyType` (already applied) covers
   `IncomingEmployee`, `OutgoingOrphanReport`, both `FK_CharityId` columns + indexes. Adding another
   would double-create on fresh DBs.
2. "Zero i18n hunks" — diff-artifact omission (construction grep); keys verified present in both
   `ar.json`/`en.json` working-tree.
3. «Operation Faild» English misspelling — spec-mandated verbatim wording (§21 ACs demand exactly it).
4. Anonymous envelopes vs `ApiResponse<T>` — documented platform-wide divergence (20/22 controllers;
   architecture.md §10 row 7 is the stale side).
5. Seed saves via direct `DbContext.SaveChangesAsync` — platform seed convention
   (`IIROSASeedDataInitializer` precedent).
6. Advisory serial readable cross-charity by HQ — by design (16-1/16-3 AC 4).
7. `export/…` routes no longer host an export wizard — documented deviation (16-9/16-18/16-19).
8. Serial display `('0000'+serial).slice(-4)` truncates at ≥10000 — unreachable per-charity-per-year
   volume; backend `D4` never truncates.

## Per-story verdicts (Acceptance Auditor)

| Story | Verdict | Driver |
| --- | --- | --- |
| 16-1 | SATISFIED | documented AssignedUserId deviation; country gap P7 |
| 16-2 | SATISFIED | — |
| 16-3 | SATISFIED | serial race → P6 |
| 16-4 | VIOLATION | P3, P4-mirror, P5, D1 |
| 16-5 | SATISFIED | deleted-id-404 false → P1 |
| 16-6 | VIOLATION | P3, P5 |
| 16-7 | VIOLATION | P1 (AC 1 fails: row still returned after delete) |
| 16-8 | SATISFIED | — |
| 16-9 | VIOLATION | P1, P2 |
| 16-10 | DEVIATES (documented) | الحاله→hasReply reading |
| 16-11 | DEVIATES (documented) | same reading |
| 16-12 | SATISFIED | serial race → P6 |
| 16-13 | VIOLATION | P4, P5, D1 |
| 16-14 | SATISFIED | detached links visible → P1 |
| 16-15 | VIOLATION | P5 |
| 16-16 | VIOLATION | P1 (AC 1 fails; guard never releases after detach) |
| 16-17 | SATISFIED | — |
| 16-18 | VIOLATION | P1 (detach unrecoverable), D1 (BR-27 bypass) |
| 16-19 | DEVIATES (documented) + VIOLATION | P8 flag column; P1 counts |

Verified-clean across all layers: role gates incl. both SuperAdmin DELETE endpoints and matching
`PERMISSION_ROLES`; charity pin-never-widen on every read incl. advisory serials and the report filter;
tri-state statuses with معلق default; FK pre-checks → 400 `errors` map (never 500) incl. cross-charity
reply-to refusals; bilingual categories + Arabic-first department seeds; BR-26 filtered unique index;
DI registrations; the four documented deviations implemented exactly as recorded; UC-12 import/export
retired cleanly.

## Patch outcome (2026-08-24, applied same day)

User decision: fix automatically. All 14 patches applied; D1 (NULL-charity HQ letters) resolved as
"Allow HQ-owned NULL-charity letters" and encoded as P14.

| Patch | Applied | Verification |
| --- | --- | --- |
| P1 !IsDeleted predicates | ✅ 4 repositories, every read path | Domain+Application compile clean |
| P2 employee candidates charity-scoped | ✅ `GetAvailableEmployeesAsync(incomingId, charityId)`; NULL charity = all actives (D1) | compile |
| P3 incoming mandatory validators | ✅ LetterNumber/LetterDate/Status NotEmpty (+client mirror, P5) | compile + ng build |
| P4 outgoing reply-to optional | ✅ server validators + client `incomingId` un-required; `(+ -)` comment | compile + ng build |
| P5 SPA field flagging | ✅ `handleSaveError` (details map → `setErrors({server})` + markAsTouched) both forms; required+maxlength on letterNumber/letterDate; dead `getErrorMessage` removed; toasts via error path | ng build exit 0 |
| P6 serial unique backstop | ✅ filtered unique index + `20260824114509_Epic16_SerialUniqueBackstop` (index-ops only; epic-6 drift stripped from Designer+snapshot) + DbUpdateException retry in both services | Infrastructure 0 errors |
| P7 country dimension | ✅ `CountryId` on criteria + repository subquery filter + `IsWithinCallerScope` country branch + report param | compile |
| P8 real OrphanAttached flag | ✅ live-link intersection in `GetOrphanReportAsync` | compile |
| P9 legacy NULL Date/Year | ✅ update uniqueness `incoming.Year ?? dto.Date.Year` | compile |
| P10 inactive category FK | ✅ existence (not activity) check | compile |
| P11 ExcelJS failures | ✅ `.catch` on dynamic import + writeBuffer ×3 sites; new `incomingOutgoing.exportFailed` key (ar/en) | ng build exit 0 |
| P12 edit-mode patchValue race | ✅ re-resolve `selectedIncoming` after incoming register loads | ng build exit 0 |
| P13 serial follows year | ✅ date valueChanges → `getNextSerial(year)` both forms (create mode) | ng build exit 0 |
| P14 HQ NULL-charity semantics | ✅ NULL-aware scoping/uniqueness, documented in code (Incoming/Outgoing service + repos) | compile |

Build verification: Domain 0 errors · Application — all 5 csc errors are the epic-10/17 session's
in-flight `OrphanPaymentId` refactor (foreign files, untouched); every epic-16 file compiled clean ·
Infrastructure 0 errors (`-p:BuildProjectReferences=false` against the 14:49 good Application DLL) ·
Frontend `ng build` exit 0. Migration `20260824114509` applies at the next API restart;
`ef migrations list` sanity check deferred until the parallel session's Application break clears.

Board: all 19 epic-16 stories `review` → `done` (2026-08-24).
