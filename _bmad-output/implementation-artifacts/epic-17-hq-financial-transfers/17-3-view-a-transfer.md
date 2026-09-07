# Story 17-3: View a transfer

| Field | Value |
| --- | --- |
| Story key | `17-3-view-a-transfer` |
| Epic | EP-17 — HQ Financial Transfers (الحوالات المالية للادارة المالية) |
| Use case | UC-TRF-03 — عرض الحوالة |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md` (§22.U.3 scenario) |
| Route | `#/hq-transfers/:id/view` — the form component in read-only mode (see Dev Notes: route decision) |
| Endpoint | `GET /api/HqTransfers/{id}` |
| Depends on | **17-1 landed** (vertical skeleton); 17-2's form component reused |
| Roles | Fin. Director, Gen. Director → `SuperAdmin`, `Admin` (`HqTransfers.View`) |

## Status

done

## Story

As a Financial Director, I want to be able to view a transfer عرض الحوالة, so that I can see the
full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a Financial Director with an active session in the module, when the actor opens a
   transfer, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/HqTransfers/{id}` and the response is rendered on the screen without a page reload.
3. Given the caller's JWT carries a country claim, when the record's destination country differs
   from it, then the request is refused (a charity-country user cannot read another country's
   transfer); an HQ caller without a claim reads any.
4. Given the id does not exist (or is soft-deleted), when the request is served, then the API
   answers 404 with a `message` and the SPA shows the not-found state.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §22.U.3 passes end to end — the record loads with its lookup names
resolved (country, department) and every §22.S.2 field displayed; the read is scoped server-side.

## Tasks / Subtasks

- [x] **Task 1 — Detail DTO + mapping** (AC 2)
  - [x] `HqTransferDetailDto` in `DTOs/HqTransfers/HqTransfers.cs`: `Id` + the 12 §22.S.2 fields
        with clean names + `CountryName` / `DepartmentName` (`NameAr ?? NameEn`) + audit display
        fields (`createdOn`, `createdBy`) — no `FK_*` wire keys (camelCase/FK defect class)
  - [x] `HqTransferProfile`: entity → detail map with explicit `ForMember` for the lookup names;
        verify the repository include brings `Country` and `Department` (it does — 17-1's
        `IncludeNavigationProperties()`; if a detail-specific include path is needed, extend that
        method, don't `Include` ad hoc in the service)
- [x] **Task 2 — Service read** (AC 2, 3, 4)
  - [x] `IHqTransferService.GetHqTransferByIdAsync(Guid id)` → `NotFoundException` when absent
        (soft-deleted rows are already filtered by the global query filter — never check
        `IsDeleted` by hand)
  - [x] Country-scope guard in the service after the fetch: if the caller's `ICurrentUserService`
        country claim is set and differs from the record's `FK_CountryId`, throw
        `NotFoundException` (read as "not yours → not found"; do not leak existence across the
        country boundary)
- [x] **Task 3 — API endpoint** (AC 2, 4)
  - [x] `[HttpGet("{id:guid}")] GetHqTransferById(Guid id)` in `HqTransfersController` →
        `Ok(detailDto)`; `catch (NotFoundException)` → `NotFound(new { message = ex.Message })`;
        catch-all → 500 `{ message }`. No `ApiResponse<T>` wrapper (platform deviation)
  - [x] Keep the `{id:guid}` constraint — it also keeps the literal routes (`max-amount`,
        `max-amounts` siblings) unambiguous when 17-6/17-7 land
- [x] **Task 4 — Read-only view** (AC 1, 2)
  - [x] Route `#/hq-transfers/:id/view` in `hq-transfers-routing.module.ts` pointing at the SAME
        `HqTransferFormComponent` with `data: { mode: 'view' }`; guarded
        `AuthGuard + PermissionGuard`, `data.permission: 'HqTransfers.View'`
  - [x] View mode: load via `GET /api/HqTransfers/{id}`, populate the same 12 controls, disable
        every control, hide the حفظ button, show a زرار رجوع back to `#/hq-transfers`; display
        the resolved names for country/department, not raw ids
  - [x] Wire the list screen's View icon (rendered disabled in 17-1) to navigate here
  - [x] `OnPush`; unsubscribe patterns consistent with the module's existing components —
        **OnPush omitted (mission-form precedent, 17-2 Note 3)**
- [x] **Task 5 — i18n** — view-mode title, back label, not-found message under `hqTransfers.*` in
      **both** `ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–5)
  - [x] Live check: authenticated `GET /api/HqTransfers/{id}` → 200 with `countryName` /
        `departmentName` resolved; random Guid → 404 `{ message }`; unauthenticated → 401;
        caller with a country claim different from the record's → 404 — **HQ path live-verified;
        pinned-caller path code-reviewed only (no country-claim account available — Note 3)**
  - [x] UI: view icon opens the populated read-only screen; back returns to the list
  - [x] `dotnet build` + `npm run build` green (live-API lock caveat MSB3021/3027; ng-serve
        stale-bundle caveat)
  - [x] Tests: excluded per the standing user decision

### Review Findings

_From the epic-17 backend code review (chunk 1, 2026-08-24)._

- [x] [Review][Defer] AC 4's "or is soft-deleted" promise is unenforceable today: no global
      soft-delete query filter exists at runtime (`SetGlobalQueryForSoftDelete` is never invoked;
      zero `HasQueryFilter` in the model snapshot) — a soft-deleted row would return 200, not
      404. Latent (no delete endpoint ships in epic 17) — deferred, pre-existing (platform
      thread)

_From the epic-17 full-epic review (2026-08-24 — blind/edge/acceptance layers over all 8 stories)._

- [x] [Review][Patch] View mode resolves country/department through active-only catalogue selects:
      a deactivated lookup on a stored transfer renders as an empty select instead of the resolved
      name — the server-resolved `countryName`/`departmentName` (resolved regardless of active
      state) are never displayed anywhere in view mode, breaking Task 4's "display the resolved
      names, not raw ids" [`hq-transfer-form.component.ts:3183-3196` id-patching + active-only
      selects at template `:2624`/`:2643`] — render the resolved names in view mode (or fall back
      to them when the id is missing from the active-only options)

## Dev Notes

### Route decision (recorded deviation)

The module doc's route annex (§22.A) lists `#/hq-transfers/:id` for `HqTransferDetailComponent` —
but that screen is the **execution-tracking / detail-lines** screen of UC-TRF-08, and the delivery
board (authoritative for this project's routes) assigns it `#/hq-transfers/:id/details` (story
17-8). To keep every board route unique and reachable, view mode gets `#/hq-transfers/:id/view`
and REUSES the form component disabled — no new component for a read-only render. Reuse over
reinvention: one form component serves create (17-2), view (here) and edit (17-4).

### Platform rules that bind this story

- Soft delete via the global query filter — deleted rows vanish from reads automatically.
- camelCase wire; lookup names `NameAr ?? NameEn`; raw envelope, no `ApiResponse<T>`.
- Caller scope from `ICurrentUserService` in the service, never from the payload.
- Cross-tenant reads return 404, not 403 — do not confirm existence across the boundary.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Edit mode population + `PUT /api/HqTransfers` | 17-4 |
| Detail lines / execution-state fields (EstimatedTransferDate etc.) — NOT part of this read | 17-8 |
| Max-amount display on the form | 17-6 |

### References

- [Source: docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md#22.U.3] scenario — single-record read
  with child collections (none yet; detail lines arrive in 17-8)
- [Source: _bmad-output/planning-artifacts/epics.md#3.17] US-TRF-03 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-2-create-a-transfer.md] the form component
  and DTO container this story reuses
- [Source: Backend/src/IIROSA.Application/Services/OfficeProjectService.cs] `GetProjectByIdAsync`
  — the reviewed single-read reference (NotFoundException + includes)

## Dev Agent Record

### Agent Model Used

Claude (Claude Code CLI, glm-5) — 2026-08-24.

### Debug Log References

- Backend build: `dotnet build Backend/IIROSA.sln` → **Build succeeded, 0 errors**.
- Live verification against `https://localhost:60960` (fresh binaries):
  - `GET /api/HqTransfers/{id}` (record created in 17-2) → **200**
    `{"id":"13fa8dce-…","countryName":"السعودية","departmentName":"وكيل الشؤون التنفيذية",…}`
  - `GET /api/HqTransfers/00000000-0000-0000-0000-000000000000` → **404**
    `{"message":"HqTransfer with id '…' was not found"}`
  - anonymous → **401**
  - POST (regression) → **201 + `Location: https://localhost:60960/api/HqTransfers/{id}`** —
    the 17-2 → CreatedAtAction upgrade works
- Frontend build: `hq-transfers` module **0 errors**; the 3 remaining workspace errors are the
  user's in-flight `employees` edits (untouched by this story).

### Completion Notes List

1. **Task 1 was already landed by 17-2** — `HqTransferDetailDto`, the Entity→DetailDto map, and
   the repository's `GetByIdWithDetailsAsync` include were built for the create echo; this story
   consumed them as-is.
2. **Pinned-caller 404 guard implemented but not live-tested** — the only seeded account
   (`OsamaSuper@IIROSA.com`) is HQ with no country claim, so the cross-country branch could not
   be exercised end to end. The code path is `IsWithinCallerScope` (same helper the 17-1 list
   uses) feeding the same `NotFoundException` the random-guid test did hit live. First
   country-claim account should re-verify AC 3.
3. **NotFoundException chosen over the shipped `KeyNotFoundException` precedent** —
   HousingProjectService throws `KeyNotFoundException`, but the story and
   `Application/Exceptions/NotFoundException.cs` (architecture.md §5.1 mapping) both say
   `NotFoundException`; the existing class is now in use instead of dead code.
4. OnPush omitted per module precedent (see 17-2 Note 3).
5. View mode reuses the create form's selects for country/department — the resolved NAME is what
   renders (option labels come from the catalogues; the patched numeric value selects the right
   option), satisfying "display names, not raw ids" without a separate read-only template.

### File List

**Backend (all in `Backend/src/`):**
- `IIROSA.Application/Interfaces/IHqTransferService.cs` — + `GetHqTransferByIdAsync`
- `IIROSA.Application/Services/HqTransferService.cs` — + `GetHqTransferByIdAsync` with the
  country-scope 404 guard; + `using IIROSA.Application.Exceptions`
- `IIROSA.Api/Controllers/HqTransfersController.cs` — + `[HttpGet("{id:guid}")]` with
  NotFoundException→404 ladder; POST upgraded from `StatusCode(201,…)` to
  `CreatedAtAction(nameof(GetHqTransferById),…)` (17-2 Note 1 honoured); + exceptions using

**Frontend (all in `Frontend/src/app/modules/hq-transfers/`):**
- `services/hq-transfer.service.ts` — + `getTransferById`
- `hq-transfer-form/hq-transfer-form.component.ts` — view mode: `isViewMode`/`transferId`/
  `notFound` from route, `loadTransfer` (patch + `disable()`), mode-aware title/breadcrumb/page
  action; create path unchanged
- `hq-transfer-form/hq-transfer-form.component.html` — not-found state block, `*ngIf="!isViewMode"`
  on the actions row, dynamic `[title]`
- `hq-transfers-routing.module.ts` — + `:id/view` route (`mode: 'view'`, `HqTransfers.View`)
- `hq-transfer-list/hq-transfer-list.component.{ts,html}` — view icon wired → `viewTransfer(id)`
- `Frontend/src/assets/i18n/{ar,en}.json` — + `backToList`, `transferNotFound` both languages

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-TRF-03 and module spec §22.U.3; view-route decision recorded against the board's route set. |
| 2026-08-24 | Implemented: service read + country-scope 404 guard, GET `{id:guid}` endpoint, POST upgraded to CreatedAtAction (201 + Location verified), form view mode + not-found state, `:id/view` route, list view icon, i18n ×2 keys both languages. Live: 200 names / 404 message / 401 / Location header. Status → review. |
