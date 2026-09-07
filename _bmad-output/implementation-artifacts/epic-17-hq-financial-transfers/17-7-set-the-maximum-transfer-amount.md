# Story 17-7: Set the maximum transfer amount

| Field | Value |
| --- | --- |
| Story key | `17-7-set-the-maximum-transfer-amount` |
| Epic | EP-17 — HQ Financial Transfers (الحوالات المالية للادارة المالية) |
| Use case | UC-TRF-07 — تعيين الحد الأعلى للحوالة |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md` (§22.S.4 screen, §22.U.7 scenario) |
| Route | `#/hq-transfers/max-amounts` |
| Endpoint | `PUT /api/HqTransfers/max-amount` |
| Depends on | **17-6 landed** (the `Country.MaxTransferAmount` column + read endpoint); 17-1's module shell |
| Roles | Fin. Director → **`SuperAdmin` only** for the write (`HqTransfers.ManageLimits`); the screen's read side is `Admin`-visible (see Dev Notes) |

## Status

done

## Story

As a Financial Director, I want to be able to set the maximum transfer amount تعيين الحد الأعلى
للحوالة, so that the per-country ceiling that constrains all subsequent transfers to that country
stays correct.

## Acceptance Criteria

1. Given a Financial Director on `#/hq-transfers/max-amounts`, when the actor edits a country's
   قيمة الحوالة and presses that row's «حفظ», then the stored country carries the new ceiling and
   no other country is affected.
2. Given the request is accepted, when it is served, then it is handled by
   `PUT /api/HqTransfers/max-amount` with a typed request DTO and the response is rendered
   without a page reload.
3. Given a negative or zero amount, when the actor saves, then the save is refused and the field
   is flagged. Clearing the value (empty) saves NULL — meaning unlimited — and is legal.
4. Given the save succeeds, when the ceiling is next read (form guard from 17-6 or this screen),
   then it shows the new value, and the next transfer exceeding it is refused.
5. Given the session has expired or the role is not `SuperAdmin`, when the write is invoked, then
   the request is rejected (403) and the actor is routed back to the login screen.

**Definition of done:** §22.S.4's grid is implemented (الرقم · البلد · قيمة الحوالة · حفظ) with
per-row save; §22.U.7 passes end to end; the write authorises server-side.

## Screen contract (§22.S.4 — MaxTransferAmountComponent)

- Grid over `country in Countries`: **الرقم** (serial) · **البلد** (country name) ·
  **قيمة الحوالة** (editable `country.MaxTransferAmount`) · **حفظ** (per-row save command).
- One data-entry field per row (the amount, optional per the legacy binding — semantics: empty =
  unlimited).
- Command: «حفظ» `UpdateCountry(country)` per row.

## Tasks / Subtasks

- [x] **Task 1 — DTO + validator** (AC 2, 3)
  - [x] `UpdateCountryMaxTransferDto`: `CountryId` (int, mandatory), `MaxTransferAmount`
        (decimal?, null legal)
  - [x] `Application/Validators/HqTransfers/UpdateCountryMaxTransferValidator.cs`: `CountryId`
        NotEmpty; when `MaxTransferAmount.HasValue` → `GreaterThan(0)`. NULL passes (unlimited)
- [x] **Task 2 — Service write** (AC 1, 4)
  - [x] `IHqTransferService.UpdateCountryMaxTransferAmountAsync(dto)`: fetch the country →
        `NotFoundException` if absent; set `MaxTransferAmount`; save **through `IUnitOfWork`
        only**; return the updated `{ CountryId, CountryName, MaxTransferAmount }` (same DTO
        shape 17-6's read returns)
  - [x] No audit hand-stamping (interceptor owns it); the Country row is a lookup — editing this
        one column must not disturb `NameAr/NameEn/IsActive`
- [x] **Task 3 — API endpoint** (AC 2, 3, 5)
  - [x] `[HttpPut("max-amount")] UpdateMaxTransferAmount([FromBody] UpdateCountryMaxTransferDto)`
        in `HqTransfersController` — **`[Authorize(Roles = "SuperAdmin")]` on this action**
        (tighter than the controller default; the 13-5 SuperAdmin-only-delete precedent);
        `FluentValidation.ValidationException` catch before the catch-all →
        `BadRequest(new { message, errors })` (OfficeProject shape); `NotFoundException` → 404;
        catch-all → 500. Raw envelope, no `ApiResponse<T>`
- [x] **Task 4 — Screen** (AC 1, 3, 4)
  - [ ] `max-transfer-amount/` 4-file component; route `#/hq-transfers/max-amounts` guarded
        `AuthGuard + PermissionGuard` with `data.permission: 'HqTransfers.ManageLimits'`;
        registered in `hq-transfers-routing.module.ts`
  - [x] Load countries via `GET /api/LookupManagement/countries` (page size large enough for the
        full catalogue — audit what 17-2 used); rows show serial (`(page-1)*size + i + 1`),
        `nameAr ?? nameEn`, and an editable numeric box per row (empty ⇒ unlimited)
  - [x] Per-row «حفظ» → `PUT /api/HqTransfers/max-amount`; success toast + row refresh; failure
        maps `error.error.errors` onto that row's field; SweetAlert2 confirmation is NOT needed
        for a single-row save (no destructive effect)
  - [x] Non-SuperAdmin (Admin) opening the screen sees values read-only with the save buttons
        hidden — `hasPermission('HqTransfers.ManageLimits')` directive; hiding is UX, the 403 is
        the control
  - [x] `OnPush`, `trackBy: trackByCountryId` on the grid
  - [x] Entry point: a ضبط الحدود action on the `#/hq-transfers` list screen header (gated by
        the same permission) — the board requires the route reachable
- [x] **Task 5 — Permissions + i18n** (AC 5)
  - [x] `auth.service.ts` `PERMISSION_ROLES`: `HqTransfers.ManageLimits` → `['SuperAdmin']`
  - [x] Labels/messages under `hqTransfers.*` in **both** `ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–5)
  - [x] Live: as SuperAdmin — PUT sets 100000 → read reflects it → creating a transfer (17-2
        path) over 100000 is refused; PUT null → unlimited accepted; PUT 0/-5 → 400 flagged
        field; unknown countryId → 404. As Admin — PUT → **403**; screen shows read-only — all
        green 2026-08-24 (Debug Log); Admin's read-only VIEW is `hasPermission` UX (code-level),
        no browser pass
  - [x] `dotnet build` + `npm run build` green (lock caveats); tests excluded per the standing
        decision — backend 0 errors; frontend 0 in this module

### Review Findings

_From the epic-17 backend code review (chunk 1, 2026-08-24)._

- [x] [Review][Patch] The ceiling write bypasses the caller-scope pin: a **pinned SuperAdmin**
      (`TokenService.cs:186-189` emits the country claim for any user with a `CountryId`,
      SuperAdmin included) can overwrite another country's ceiling with 200 — contradicting the
      pin-never-widen contract every other path in the module enforces
      [`HqTransferService.cs:299-326`] — apply the 404-shaped scope guard (the read half is
      recorded under 17-6)
- [x] [Review][Patch] (mirror of 17-2's finding) `ScalePrecision(2, 18)` missing on
      `MaxTransferAmount` [`UpdateCountryMaxTransferValidator.cs`]

_From the epic-17 full-epic review (2026-08-24 — blind/edge/acceptance layers over all 8 stories).
The unchecked [Patch] items above were re-verified against the working tree — still unapplied._

- [x] [Review][Patch] Empty-state row hard-codes `colspan="4"` but the grid renders 3 columns
      when `!canManage` (the 4th `th`/`td` pair is edit-gated) [`max-transfer-amount.component.html`
      empty-state row vs column definitions]

## Dev Notes

### Permission decision (recorded)

The legacy system gated execution-editing behind `TransfersAdmin` — a tighter write role than the
module's read roles. The platform analogue for "consequential, few should do it" is
SuperAdmin-only (Missions.Delete precedent, 15-1). Reading the ceilings stays open to Admin so the
17-6 form guard works for every transfer writer; only the setting tightens.

### Platform rules that bind this story

- Only `IUnitOfWork` saves; FluentValidation in the service layer; typed DTOs only.
- camelCase wire; `NameAr ?? NameEn`; raw `{ message, errors }` envelope; no `ApiResponse<T>`.
- Per-action `[Authorize]` tightening is legal and preferred over a second controller.
- Editing a lookup row through the transfers service is the sanctioned path here — do NOT add a
  generic lookup-write dependency or reuse the lookup-management CRUD screens.

### Out of scope

| Item | Owner |
| --- | --- |
| Ceiling enforcement on create/update (already landed) | 17-6 |
| Bulk "save all rows" — the spec's command is per-row `UpdateCountry(country)` | — |
| Any other Country field's editability | lookup-management backlog |

### References

- [Source: docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md#22.S.4] the grid + per-row save
  contract
- [Source: docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md#22.U.7] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.17] US-TRF-07 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/Lookups/Country.cs] the row being edited (column
  from 17-6)
- [Source: _bmad-output/implementation-artifacts/epic-13-office-development-projects/13-5-delete-development-project.md] the
  SuperAdmin-only action precedent
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-6-view-the-maximum-transfer-amount-for-a-country.md]
  the column + read this story builds on

## Dev Agent Record

### Agent Model Used

Claude (Claude Code CLI, glm-5) — 2026-08-24.

### Debug Log References

- **Live battery run 2026-08-24** on a private `:5199` instance (this session's own process),
  same session as 17-4/5/6/8. The ceiling column was already applied by then (17-6 Debug Log):
  - SuperAdmin `PUT max-amount {countryId:2, maxTransferAmount:100000}` → **200** echo; read-back
    → **200** `100000.00`
  - Over-ceiling transfer create (17-2 path, 150000 > 100000) → **400** — the ceiling this story
    set is enforced (AC 1's end-to-end loop)
  - `PUT {maxTransferAmount:0}` and `{-5}` → **400** errors map flagging `MaxTransferAmount`
  - Unknown countryId → **404** `{message}`
  - `PUT null` → **200** (unlimited restored), read-back null
  - Admin (`Admin@IIROSA.com`) `PUT` → **403**; Admin `GET max-amount` → **200** — the exact
    write-tight/read-open split of Note 1
  - Cleanup: ceiling cleared, battery mutations reverted
- Backend `dotnet build` 0 errors; frontend `npm run build` 0 errors in this module.
- Tests: excluded per the standing user decision (spec.ts is the module's should-create stub).

### Completion Notes List

1. **Route permission deviates from the story line — deliberately** (unticked Task 4 sub-box).
   The story asked for `data.permission: 'HqTransfers.ManageLimits'` on the route AND an Admin
   read-only view of the same screen; a SuperAdmin-only route guard 403s Admin at navigation,
   making the read-only requirement unreachable. Resolved: route guards `HqTransfers.View`
   (like every module screen), the save UI/input hides behind
   `hasPermission('HqTransfers.ManageLimits')`, and the PUT endpoint's SuperAdmin-only
   `[Authorize]` is the control (AC 5's 403). Hiding is UX, exactly as the story itself states.
2. **Save button disabled until dirty** — per-row «حفظ» enables only when
   `editValue !== savedValue`; empty ⇒ NULL ⇒ unlimited is a legal save (AC 3), and clearing a
   value back to empty IS a change (null ≠ 100000), so the dirty check compares against null
   correctly.
3. **Error mapping per row** — 400 `errors.MaxTransferAmount` lands on that row's inline
   feedback (`row.error`); 403 toasts `noPermission`; other failures toast the server message.
   The read-only column always shows the PERSISTED value plus a لا يوجد حد مضبوط badge for
   null ceilings.
4. **Grid source** — `GET /api/LookupManagement/countries?isActive=true&pageSize=1000` (same
   catalogue call the §22.S.2 form uses); `CountryDto` carries `maxTransferAmount` from 17-6,
   so no second read endpoint was needed for the grid.
5. **OnPush omitted** — module-consistent (the 17-1/17-2 deviation, missions precedent); the
   per-row mutable state map would fight OnPush without an refactor of row identity; `trackBy`
   is implemented (`trackRow` by country id). Recorded, not silently dropped.
6. Serial column is plain `i + 1` — the grid loads the FULL catalogue (pageSize 1000), so page
   arithmetic never applies.
7. **Frontend `CountryDto` carry landed late** — this session's build caught `TS2339` at the
   grid's row mapping (`max-transfer-amount.component.ts:89-90`): `lookup.model.ts`'s
   `CountryDto` never gained the field when the screen was written. Added as
   `maxTransferAmount?: number | null`; also listed in 17-8's File List.

### File List

**Backend:**
- `Backend/src/IIROSA.Application/DTOs/HqTransfers/HqTransfers.cs` — + `UpdateCountryMaxTransferDto`
- `Backend/src/IIROSA.Application/Validators/HqTransfers/UpdateCountryMaxTransferValidator.cs` —
  new (auto-registered via AddValidatorsFromAssembly)
- `Backend/src/IIROSA.Application/Interfaces/IHqTransferService.cs` — +
  `UpdateCountryMaxTransferAmountAsync`
- `Backend/src/IIROSA.Application/Services/HqTransferService.cs` — + maxTransferValidator
  injection + the write (column-only touch, UoW-only save)
- `Backend/src/IIROSA.Api/Controllers/HqTransfersController.cs` — + `[HttpPut("max-amount")]`
  with `[Authorize(Roles = "SuperAdmin")]` and the full catch ladder

**Frontend:**
- `Frontend/src/app/modules/hq-transfers/max-transfer-amount/` — NEW 4-file component
  (`.ts`, `.html`, `.scss`, `.spec.ts`) — §22.S.4 grid with per-row save
- `Frontend/src/app/modules/hq-transfers/models/hq-transfer.model.ts` — +
  `UpdateCountryMaxTransferRequest`
- `Frontend/src/app/modules/hq-transfers/services/hq-transfer.service.ts` — +
  `updateMaxTransferAmount`
- `Frontend/src/app/modules/hq-transfers/hq-transfers-routing.module.ts` — + `max-amounts`
  route (static, before `:id` routes)
- `Frontend/src/app/modules/hq-transfers/hq-transfer-list/hq-transfer-list.component.ts` — +
  AuthService + permission-gated ضبط الحدود header action
- `Frontend/src/app/core/services/auth.service.ts` — + `HqTransfers.ManageLimits:
  ['SuperAdmin']`
- `Frontend/src/assets/i18n/{ar,en}.json` — + `hqTransfers.maxAmounts.*` block (13 keys each)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-TRF-07 and module spec §22.S.4 / §22.U.7; SuperAdmin-only write decision recorded. |
| 2026-08-24 | Implemented end to end (DTO/validator/service write/SuperAdmin-only PUT/§22.S.4 grid with per-row save/route/header action/permission/i18n). Route-guard deviation per Note 1. Live battery deferred on the user's WIP + 17-6's unapplied migration; compile-clean in all story files. Status in-progress until the battery runs. |
| 2026-08-24 | Live battery green (SuperAdmin set/read-back/0/-5/null/404; Admin 403-on-write 200-on-read; enforcement loop with 17-6); `CountryDto.maxTransferAmount` frontend carry added (Note 7); status → review. |
