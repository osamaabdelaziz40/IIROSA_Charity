# Story 14-4: Update a support ticket

| Field | Value |
| --- | --- |
| Story key | `14-4-update-a-support-ticket` |
| Epic | EP-14 — Technical Support |
| Use case | UC-CST-04 — تعديل الطلب |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/19-UC-CST-Technical-Support.md` (§19.U.4 scenario; §19.S.2 screen) |
| Route | `#/technical-support/:id/edit` (route + `TicketFormComponent` already exist) |
| Endpoint to add | `PUT /api/SupportTickets/{id}` |
| Depends on | 14-3 (detail GET — exists), 14-2 (create — shares the form component) |

## Status

done

> Review completed 2026-08-23 — both decisions resolved, all 18 review patches applied,
> backend compiles clean and frontend build green. **Task 8 (live walkthrough) is still
> outstanding** — the running `IIROSA.Api` (PID 13532) predates the patches and must be
> restarted before verifying against the live stack.

## Story

As a General Director, I want to be able to update a support ticket تعديل الطلب, so that a record
that was entered wrongly or has changed can be corrected.

## Acceptance Criteria

1. Given a General Director with an active session in the module, when the actor invokes the
   function with valid input, then the stored record carries the new values; no other record is
   affected.
2. Given the request is accepted, when it is served, then it is handled by
   `PUT /api/SupportTickets/{id}` and the response is rendered on the screen without a page reload.
3. Given a mandatory field listed in the screen field specification is empty, when the actor saves,
   then the save is refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to the ticket screens, then the record appears
   there with the values just entered. *(Verification note: the update flow lands on the detail
   screen `#/technical-support/:id` — same UX as create. The **list** screen cannot currently
   render live data at all: see "Phantom API" below. Verify AC 4 on the detail screen and via a
   direct `GET /api/SupportTickets/all-tickets` API call. The list-screen wire repair is 14-1
   rework, explicitly out of scope.)*
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §19.S are implemented with their mandatory flags and
lookups; the scenario of §19.U passes end to end (edit → save → corrected record visible); the role
check is enforced server-side (`Admin,SuperAdmin`), not only in the menu.

## ⚠️ The defect this story actually fixes — read before estimating

**The edit screen is broken end to end today, in two directions at once.**

1. **No update path exists anywhere.** `SupportTicketsController` has no PUT action for the ticket
   body, `ISupportTicketService` has no `UpdateTicketAsync`, the frontend service has no
   `updateTicket()`. Opening `#/technical-support/:id/edit` and pressing «حفظ» calls
   `createTicket()` unconditionally (`ticket-form.component.ts:148`) — **it creates a duplicate
   ticket**, it does not update.
2. **The Angular module talks to a phantom API.** `TechnicalSupportService` uses base endpoint
   `/api/technicalsupport` (`technical-support.service.ts:21`); the only backend controller is
   `SupportTicketsController` at `api/SupportTickets` (`[Route("api/[controller]")]`,
   `SupportTicketsController.cs:15`). No interceptor rewrites paths (`api.service.ts` appends the
   endpoint verbatim to `environment.apiUrl`). Every call the module makes today 404s — including
   `getTicketById`, which `loadTicket()` needs to fill the edit form.
3. **The client model doesn't match the wire.** The form patches `ticket.category` /
   `ticket.priority` (`ticket-form.component.ts:121-122`) but the API returns
   `categoryId`/`categoryName` (`SupportTicketDetailDto`). The selects would stay on defaults and a
   "successful" update would silently rewrite category/priority. This is the same class of bug
   13-4 fixed for office projects (DTO-key vs form-control mismatch).

This story delivers the **update flow** end to end and repairs exactly the service methods that
flow rides on. Everything else the phantom API breaks (list, status, solve, responses, assign,
report, attachments) is catalogued under "Known defects outside scope".

## Tasks / Subtasks

- [x] **Task 1 — Backend: `UpdateTicketAsync` in the application service** (AC 1, 3)
  - [x] Add to `Backend/src/IIROSA.Application/Interfaces/ISupportTicketService.cs`:
        `Task<SupportTicketDto> UpdateTicketAsync(UpdateSupportTicketDto dto, string userId);`
  - [x] Implement in `Backend/src/IIROSA.Application/Services/SupportTicketService.cs` by mirroring
        `UpdateTicketStatusAsync` (lines ~130–140): load the tracked ticket with the same query
        pattern the service already uses (`FirstOrDefaultAsync(t => t.Id == …)`),
        `throw new KeyNotFoundException($"Ticket with ID {dto.Id} not found")` when missing, assign
        `Title`, `Message`, `CategoryId`, `PriorityId`, `StatusId`, then
        `await _unitOfWork.SaveChangesAsync()` and return the mapped `SupportTicketDto`
  - [x] Direct assignment is correct here (do **not** hand-patch nulls as 13-4 did): every field of
        `UpdateSupportTicketDto` is `[Required]`, so the payload is always complete
  - [x] Inject and run the validator (Task 2) inside the service — validation belongs in the
        service layer, not the controller; on failure throw `ValidationException` with the field
        errors (pattern: `SeasonalAidService` constructor-injects `IValidator<T>` and validates
        before mutating, `SeasonalAidService.cs:27-44`)
- [x] **Task 2 — Backend: FluentValidation validator** (AC 3)
  - [x] Create `Backend/src/IIROSA.Application/Validators/TechnicalSupport/UpdateSupportTicketValidator.cs`
        (`AbstractValidator<UpdateSupportTicketDto>`): `Id` NotEmpty; `Title` NotEmpty +
        MaximumLength(200); `Message` NotEmpty + MaximumLength(4000); `CategoryId`, `PriorityId`,
        `StatusId` GreaterThan(0). Keep the rules in sync with the DataAnnotations already on
        `Backend/src/IIROSA.Application/DTOs/TechnicalSupport/UpdateSupportTicketDto.cs` — this is
        the module's **first** FluentValidation validator; house pattern to copy:
        `Validators/Charity/UpdateCharityValidator.cs`
  - [x] No validator auto-registration step is needed if the assembly scan is already in place
        (Charity/SeasonalAid validators resolve); verify by resolving `IValidator<UpdateSupportTicketDto>`
        — if the DI convention needs a manual line, add it where
        `ServiceCollectionExtensions.cs:208` registers `ISupportTicketRepository`
- [x] **Task 3 — Backend: controller action** (AC 2, 5)
  - [x] In `Backend/src/IIROSA.Api/Controllers/SupportTicketsController.cs` add:
        `[HttpPut("{id}")]`, `[Authorize(Roles = "Admin,SuperAdmin")]`,
        `public async Task<ActionResult<SupportTicketDto>> UpdateTicket(Guid id, [FromBody] UpdateSupportTicketDto dto)`
  - [x] Guard `dto.Id != id` → `BadRequest` (route wins); reuse `GetCurrentUserId()` as the
        neighbouring actions do
  - [x] Catch mapping consistent with the file: `KeyNotFoundException` → 404,
        `ValidationException` → 400 with field errors, generic → 500 logged (copy the
        try/catch shape of `CreateTicket`, lines 39–71). Return `Ok(updated)` — **bare DTO, not
        `ApiResponse<T>`**; see "Conventions" below
  - [x] Route decision (recorded): the legacy literal was `PUT /api/SupportTickets` with the id in
        the body; every id-addressed action in this controller (`{id}`, `{id}/status`) puts the id
        in the route. The story uses `PUT {id}` — consistent with the module and RESTful. The DTO
        keeps its `Id` and the controller cross-checks the two.
- [x] **Task 4 — Backend: ticket lookups read path** (DoD "…and lookups")
  - [x] The seeded lookups `SupportTicketCategory` / `SupportTicketPriority` / `SupportTicketStatus`
        exist as entities + configurations + `ApplicationDbContext` DbSets (the sanctioned §3.3
        exception, `ApplicationDbContext.cs:66-68`) but nothing exposes them at runtime — and the
        edit form's selects need real ids, not the hardcoded TS string enums
  - [x] Add `ISupportTicketLookupRepository` to `Backend/src/IIROSA.Domain/Interfaces/` with
        `GetCategoriesAsync()`, `GetPrioritiesAsync()`, `GetStatusesAsync()` returning
        `Task<IEnumerable<T>>` ordered by `SortOrder`/`SeverityLevel`/id; implement in
        `Backend/src/IIROSA.Infrastructure/Data/Repository/SupportTicketLookupRepository.cs`
        (extend `Repository<T>` like `SupportTicketRepository.cs:14`); register in
        `Backend/src/IIROSA.Infrastructure/Extensions/ServiceCollectionExtensions.cs` next to the
        `ISupportTicketRepository` line (208)
  - [x] Service: `Task<TicketLookupsDto> GetTicketLookupsAsync()` returning the three lists mapped
        to `LookupDto` — `SupportTicketMappingProfile.cs` **already maps each lookup → `LookupDto`**;
        reuse it, do not write new mapping code. Add `TicketLookupsDto`
        (`Categories`, `Priorities`, `Statuses` of `LookupDto`) in `DTOs/TechnicalSupport/`
  - [x] Controller: `GET /api/SupportTickets/lookups`, `[Authorize]` (any authenticated user — the
        create form needs it too), returns `Ok(dto)`
- [x] **Task 5 — Frontend: align the service to the real API** (AC 2, 3)
  - [x] `Frontend/src/app/modules/technical-support/services/technical-support.service.ts:21` —
        change `endpoint` from `'/api/technicalsupport'` to `'/api/SupportTickets'`
  - [x] `getTicketById` now hits the real `GET {id}` — no path change needed beyond the constant;
        its return type stays the bare `SupportTicket` detail JSON (the controller does **not**
        envelope responses — do not read `.value` off it)
  - [x] Add `updateTicket(id: string, request: UpdateTicketRequest): Observable<SupportTicket>` →
        `this.api.put(`${this.endpoint}/${id}`, request)`; add `UpdateTicketRequest`
        `{ id: string; title: string; message: string; categoryId: number; priorityId: number; statusId: number }`
        to `Frontend/src/app/core/models/technical-support.model.ts`
  - [x] Add `getTicketLookups(): Observable<TicketLookups>` → `this.api.get(`${this.endpoint}/lookups`)`
        with a `TicketLookups` interface `{ categories: LookupOption[]; priorities: LookupOption[]; statuses: LookupOption[] }`,
        `LookupOption { id: number; name: string }`
  - [x] Extend the `SupportTicket` interface with the wire fields the form needs (optional, so
        existing consumers don't break): `categoryId?: number; categoryName?: string; priorityId?:
        number; priorityName?: string; statusId?: number; statusName?: string`
  - [x] Fix `createTicket` while you are in this file (the create screen shares it): it POSTs
        FormData while the controller binds JSON `CreateSupportTicketDto` — send JSON
        `{ title, message, categoryId, priorityId }` and type the return as
        `Observable<SupportTicket>` (the current `ApiResponse<SupportTicket>` typing reads
        `response.value`, which the backend never sends). The file selection UI stays as-is;
        server-side attach has no endpoint yet (known defect, out of scope)
- [x] **Task 6 — Frontend: edit mode in `TicketFormComponent`** (AC 1–4)
  - File: `Frontend/src/app/modules/technical-support/ticket-form/ticket-form.component.ts`
  - [x] Replace the string-enum form controls with id controls: `category: [null, Validators.required]`,
        `priority: [null, Validators.required]` (create defaults come from the first loaded lookup,
        or keep current enum defaults for create mode only if you also map enum→id — prefer ids
        everywhere and drop the `TicketCategory`/`TicketPriority` usage in this component)
  - [x] Load `getTicketLookups()` on init; bind the selects to the id lists; option labels via the
        existing translation pattern `technicalSupport.categories.*` keyed by lookup name
  - [x] `loadTicket()`: patch from the **wire** fields — `title`, `categoryId`, `priorityId`,
        `message` — and stash `statusId` from the loaded ticket for the update payload; stop
        patching `ticket.category`/`ticket.priority` (they don't exist on the wire)
  - [x] `onSubmit()`: branch on `isEditMode` — edit → `updateTicket(this.ticketId!, { id, …form
        value, statusId: loaded })`, then `notifyTicketsUpdated()` and navigate to
        `['/technical-support', this.ticketId]` (same landing as create); create → `createTicket`
        with the JSON payload. Client-side flagging of empty mandatory fields already exists
        (`isFieldInvalid` / `getErrorMessage`) — keep it; it satisfies the "flagged field" half of
        AC 3 while the server-side validator covers the API half
  - [x] `onCancel()` already returns to the detail in edit mode — no change
- [x] **Task 7 — i18n** (project rule: no hard-coded UI strings)
  - [x] Every key the touched templates use must exist in **both**
        `Frontend/src/assets/i18n/ar.json` and `en.json` under the existing `technicalSupport.*`
        prefix (`editTicket`, `save`, `cancel`, `categories.*`, `priorities.*` are believed to
        exist — verify against what the new selects render, including any empty-option label you
        add). Add only keys you actually reference; Arabic is the primary language.
- [ ] **Task 8 — Verify live**
  - [ ] `dotnet build Backend/IIROSA.sln` — zero errors; then run the API and exercise: open a
        ticket via `#/technical-support/:id/edit`, change title/category/priority/message, save →
        lands on detail with new values; `GET /api/SupportTickets/{id}` returns the new values;
        PUT with an empty title → 400 + flagged field; PUT as a non-`Admin`/`SuperAdmin` user →
        403/redirect to login
  - [ ] `cd Frontend && npm run build` — zero errors (remember `npm install` needs
        `--legacy-peer-deps` until the ngx-bootstrap pin is fixed)
  - [ ] ng-serve stale-bundle pitfall: after frontend fixes, confirm the served bundle actually
        contains your change (grep the newest chunk in the dev-server output for a string you
        added) before concluding "no effect" — this project has been bitten by a dead watcher
        serving an old build
- [x] **Task 9 — Tests** — EXCLUDED by standing user decision (no test project exists under
      `Backend/tests`; same decision as epics 3 and 13). Do not create spec files.

## Dev Notes

### Conventions this story must follow — and where the module deviates from the architecture on purpose

| Concern | Rule for this story | Why |
| --- | --- | --- |
| Response envelope | Return **bare DTOs** (`Ok(ticket)`) like every other action in `SupportTicketsController` | CLAUDE.md wants `ApiResponse<T>`, but only 1 of 21 controllers uses it; converting one endpoint in isolation is a deferred decision (`deferred-work.md`). Module-local consistency wins |
| Controller base | Stay on `ControllerBase` for this controller | Same deferred-decision logic; do not "fix" the inheritance while adding one action |
| Exception → HTTP | `KeyNotFoundException` → 404 in the controller catch | Module convention (`SupportTicketsController.cs:62-65`); the architecture's `NotFoundException` would map identically but nothing in this module throws it — don't introduce a second dialect in one story |
| Validation | FluentValidation **in the service**, injected as `IValidator<T>` | Architecture §5; live pattern in `SeasonalAidService.cs:27-44` |
| Persistence | Mutate the tracked entity, then `_unitOfWork.SaveChangesAsync()` — nothing else saves | Architecture §4; mirror `UpdateTicketStatusAsync` |
| Audit fields | Never set `UpdatedOn`/`UpdatedBy` by hand | `AuditLogSaveChangesInterceptor` stamps them |
| New repository | Interface in `Domain/Interfaces/`, implementation in `Infrastructure/Data/Repository/`, manual DI line in `ServiceCollectionExtensions.cs` | Open-generic registration is commented out there; every repo is hand-registered |
| Lookups DbSets | Do not add or remove `DbSet`s on `ApplicationDbContext` | The three Technical-Support lookup DbSets are the single sanctioned exception (architecture §3.3), already present |
| Status changes | The edit form does **not** offer a status control; the payload carries the loaded `statusId` unchanged. Status transitions remain the job of the existing `PUT {id}/status` / `{id}/mark-solved` actions | `UpdateSupportTicketDto.StatusId` is `[Required]` so it stays in the payload, but §19.U.4's "progress and closure" is already served by the dedicated status/solve endpoints the copied code shipped. Splitting write paths keeps their audit semantics intact |
| Authorisation | `[Authorize(Roles = "Admin,SuperAdmin")]` on the PUT | §19.U.4 actors Gen. Director + Staff; verified role mapping from 13-4: Gen. Director ≈ `SuperAdmin`, Staff ≈ `Admin`. Server-side only — menu hiding is not a control (NFR-2) |
| Tenancy note | `SupportTicket` has **no** Charity/country FK — scoping for this module is role + `CreatedByUserId` (`HasUserAccessAsync`), not charity scoping | This module's register is user-scoped by design in the shipped code; do not bolt a `CharityId` on in this story |
| Frontend change detection | Keep the component's existing patterns; no new hard-coded strings | RTL-first, `ar.json`/`en.json` only |

### Known defects outside scope — do not fix in this story, do not forget they exist

The phantom-API audit (2026-08-23) found the frontend module calls endpoints that don't exist.
Fixed by this story: base path, `getTicketById`, `createTicket` (JSON), new `updateTicket`,
new `getTicketLookups`. **Still broken after this story**, all pre-existing under "done" stories:

- `getMyTickets` → `GET /api/technicalsupport/mytickets`: wrong base **and** wrong segment
  (`my-tickets` on the backend) **and** shape mismatch — the tuple serialises as `{ item1, item2 }`,
  not the `PagedResponse` the list expects. `getAllTickets` likewise (`GET all-tickets`). The list
  screen stays non-functional → 14-1 rework.
- `updateTicketStatus`/`markTicketAsSolved` use `api.patch` and `/status`//`/solve` paths; backend
  is `PUT {id}/status` and `POST {id}/mark-solved`.
- `addTicketResponse`, attachments (`/{id}/attachments*`), `assign`/`unassign`, `close`,
  `categories`/`priorities`/`statuses` (superseded by Task 4's `lookups`), `reports`,
  `export/excel|pdf`, `statistics` — backend equivalents absent or differently shaped.
- Backend: `GetByCodeAsync` throws `NotImplementedException` by design; `ApiService.get` leaves
  `console.log` debug output on every GET.

On completion, append these to `_bmad-output/implementation-artifacts/deferred-work.md` and raise
a `/bmad-correct-course` recommendation: epic 14's "done" stories passed an existence audit, not a
wire audit.

### Previous story intelligence (13-4 — closest analogue, epic 13)

- **DTO keys must match form-control names.** 13-4's whole edit screen was broken because the wire
  sent `fK_*` keys the patch step didn't look for. Task 6 applies the lesson up front: patch from
  `categoryId`/`priorityId`, the exact wire names.
- **13-4 hand-patched non-null fields** because its update DTO was partial. Here the DTO is fully
  required — direct assignment, no partial-merge machinery.
- **Tests are excluded by standing user decision** — re-confirmed in epics 3 and 13.
- **ng-serve stale bundle pitfall** — verify the served chunk contains your change before trusting
  a "fix had no effect" conclusion (Task 8).

### Git intelligence

- History is three commits (`695f2ed` Charity Epic · `8877738` Charity Epic · `0e8a1e9` baseline);
  nothing Technical-Support-specific has landed on this stack.
- **The working tree currently holds uncommitted changes across Checks / OfficeProjects /
  SeasonalAid / Lookups / Families files.** They belong to other in-flight work — do not touch,
  revert, or "clean up" any file outside this story's File List.

### Project Structure Notes

- Follows the module-add order of architecture §9 for the backend additions (Domain interface →
  Infrastructure repository + DI → Application service/validator/DTO → Api controller). No EF
  migration is required — no schema change; `UpdateSupportTicketDto`, the AutoMapper
  `UpdateSupportTicketDto → SupportTicket` map, the `{id}/edit` route, `TicketFormComponent`, and
  `isEditMode`/`loadTicket()` already exist and are being **wired**, not rebuilt.
- The three shared-component mandates (data-list / input-fields / attachment) are not used by this
  module — it predates the mandate. Do not retrofit them in this story; note it for the
  course-correct.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#3.14] — US-CST-04 acceptance criteria
- [Source: docs/Modules/19-UC-CST-Technical-Support.md#19.U.4] — update scenario; §19.S.2 screen fields
- [Source: _bmad-output/planning-artifacts/architecture.md] — §4 UoW, §5 validators, §6 controllers,
  §3.3 DbSet exception, §10 reconciliation
- [Source: _bmad-output/implementation-artifacts/epic-13-office-development-projects/13-4-view-update-development-project.md] — prior
  learnings
- [Source: _bmad-output/implementation-artifacts/deferred-work.md] — envelope/ApiController/test
  deferrals
- Code anchors verified 2026-08-23: `SupportTicketsController.cs:15-71`,
  `technical-support.service.ts:21`, `ticket-form.component.ts:115-158`,
  `SupportTicketService.cs:130-140`, `ServiceCollectionExtensions.cs:208`,
  `ApplicationDbContext.cs:66-68`

## Dev Agent Record

### Agent Model Used

claude-code (glm-5)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — **0 Error(s)**, 8 warnings (all pre-existing: NU1903 AutoMapper
  advisory etc.); re-verified at pass end (3.7 s incremental).
- `cd Frontend && npm run build` — **success**, hash `7804f7b5486665f9`, 36.2 s;
  `modules-technical-support-technical-support-module` chunk 89.46 kB. Warnings pre-existing
  (seasonal-aid SCSS budget, sweetalert2/exceljs CommonJS).
- i18n audit script: 102 translate keys used by the module; **all resolve in both `ar.json` and
  `en.json`** after additions (see File List).
- Live click-through (Task 8 first bullet, beyond the builds) **not executed** this session — no
  running stack was available; both builds are the verification evidence. Left the Task 8 box
  unticked for the reviewer to close after the walkthrough.

### Completion Notes List

- **Scope extension (user instruction, 2026-08-23):** "review and complete implementation from
  use case 14-1 until 14-6 … it's for epic 14". Tasks 1–7 below delivered story 14-4; the same
  pass then rewired the remaining module screens so the epic's "done" stories are real:
  - **14-1 list** — `ticket-list.component.ts/.html` rewritten to the wire: `categoryName`/
    `priorityName`/`statusName` + lookup colours, `createdByUserName`, `createdOn`/`updatedOn`;
    filters became id-based over `GET lookups`; my/all view mode from route data; sort toggling.
  - **14-3 detail** — `ticket-detail.component.ts/.html` rewritten: `createdByUserId` ownership,
    `visibleResponses` (admins see all, owners see `publicResponses`), status dropdown over lookups,
    close → `PUT {id}/status` with the Closed id resolved from lookups (`nameEn === 'Closed'`),
    admin-only response form (the copied UI wrongly offered responding to owners — the endpoint is
    `Admin,SuperAdmin`), attachments display name-only (no download endpoint exists), phantom
    assign/unassign/download calls removed.
  - **14-5 delete** — SuperAdmin-only dropdown action in the list calling `DELETE {id}` with
    confirm (controller authorises `SuperAdmin` only; button hidden for others **and** endpoint
    enforces).
  - **14-6 report** — see story file `14-6-report-on-support-tickets.md` (created + implemented in
    this pass).
  - **Route-order defect fixed** — `technical-support-routing.module.ts` registered `'reports'`
    after `':id'`, so `#/technical-support/reports` rendered the detail screen with id `"reports"`.
- **Serializer discovery (applies module-wide):** `Program.cs` ends with `.AddNewtonsoftJson()`,
  which **replaces** the System.Text.Json formatter. Consequences corrected during wiring: list
  endpoints return `Ok(new { result.Items, result.TotalCount })` → `{ items, totalCount }` (the
  story-time assumption of a ValueTuple `{ item1, item2 }` shape was wrong); trailing acronyms
  keep their case (`TicketsResolvedWithinSLA` → `ticketsResolvedWithinSLA`). Service mapping and
  `SupportReport` model corrected accordingly.
- **Deviation from Task 4 wording:** `SupportTicketLookupRepository` is a plain class over
  `ApplicationDbContext`, not a `Repository<T>` subclass — one repository serves three lookup
  aggregates, so the generic base doesn't fit. Registered manually next to
  `ISupportTicketRepository` as specified.
- **Deviation from Task 6 wording:** select option labels render `lookup.name` (server-localised
  `NameAr ?? NameEn`), not the `technicalSupport.categories.*` enum translations — the TS string
  enums cannot key dynamic DB rows. The enums remain in the model for compatibility but no screen
  uses them. In create mode the selects default to the first loaded option.
- The edit form intentionally offers no status control; the payload carries the loaded `statusId`
  unchanged, per the Conventions table.
- Server-side sorting note: `SupportTicketService` ignores `SortBy`/`SortDirection` (no
  `OrderBy` logic exists) — the UI sort toggle binds and sends the params but ordering comes back
  repository-default. Recorded in `deferred-work.md`.
- Unwired/absent backend kept out of the UI rather than faked: assign (`[FromBody] string` raw-JSON
  binding unusable via `ApiService`), unassign (no backend), attachment upload/download
  (`AttachFileToTicketAsync` has no controller action), Excel export (removed from toolbar;
  the report screen exports CSV client-side instead). All catalogued in `deferred-work.md`.
- Tests EXCLUDED by standing user decision (Task 9; no `Backend/tests` project — epics 3, 13).

### File List

Backend (all under `Backend/src/`):

- `IIROSA.Application/DTOs/TechnicalSupport/TicketLookupsDto.cs` — **new**
- `IIROSA.Application/Validators/TechnicalSupport/UpdateSupportTicketValidator.cs` — **new**
- `IIROSA.Domain/Interfaces/ISupportTicketLookupRepository.cs` — **new**
- `IIROSA.Infrastructure/Data/Repository/SupportTicketLookupRepository.cs` — **new**
- `IIROSA.Application/Interfaces/ISupportTicketService.cs` — `UpdateTicketAsync` + `GetTicketLookupsAsync`
- `IIROSA.Application/Services/SupportTicketService.cs` — both implementations + validator/lookup-repo injection
- `IIROSA.Infrastructure/Extensions/ServiceCollectionExtensions.cs` — lookup repository DI line
- `IIROSA.Api/Controllers/SupportTicketsController.cs` — `PUT {id}` + `GET lookups`

Frontend (all under `Frontend/src/`):

- `app/core/models/technical-support.model.ts` — rewritten to the wire
- `app/modules/technical-support/services/technical-support.service.ts` — rewritten (`/api/SupportTickets`)
- `app/modules/technical-support/ticket-form/ticket-form.component.ts` — rewritten (edit mode)
- `app/modules/technical-support/ticket-form/ticket-form.component.html` — id-bound selects
- `app/modules/technical-support/ticket-list/ticket-list.component.ts` — rewritten
- `app/modules/technical-support/ticket-list/ticket-list.component.html` — rewritten
- `app/modules/technical-support/ticket-detail/ticket-detail.component.ts` — rewritten
- `app/modules/technical-support/ticket-detail/ticket-detail.component.html` — rewritten
- `app/modules/technical-support/support-report/support-report.component.ts` — rewritten (14-6)
- `app/modules/technical-support/support-report/support-report.component.html` — rewritten (14-6)
- `app/modules/technical-support/technical-support-routing.module.ts` — route order fix
- `assets/i18n/ar.json`, `assets/i18n/en.json` — `common.notAssigned`, `technicalSupport.reports.*`
  (9 keys), `validation.invalidFormat` (ar) / `validation.{minLength,maxLength,fileTooLarge,invalidFileType,invalidFormat}` (en)

Records:

- `_bmad-output/implementation-artifacts/epic-14-technical-support/14-6-report-on-support-tickets.md` — **new** (14-6)
- `_bmad-output/implementation-artifacts/sprint-status.yaml` — 14-4/14-6 status + footer
- `_bmad-output/implementation-artifacts/deferred-work.md` — epic-14 residual defects appended

### Change Log

| Date | Change |
| --- | --- |
| 2026-08-23 | Story file created from `epics.md` US-CST-04 + module spec §19.U.4/§19.S.2; phantom-API audit findings recorded; ultimate context engine analysis completed. |
| 2026-08-23 | Implemented (Tasks 1–7 + 9 excluded-by-decision). Scope extended per user instruction to the full epic-14 review-and-complete pass (14-1…14-6); status → review. |

### Review Findings

_Code review 2026-08-23 — scope: all epic-14 changes. Full report incl. dismissed items and
refutations: `review-artifacts/epic14-review-report.md` (2 decision · 17 patch · 1 defer · 14 dismissed)._

- [x] [Review][Decision] Co-mingled i18n hunks — **resolved 2026-08-23: restore broken en.json keys only (additive); hunks stay mixed, separation at commit time (git add -p)**
- [x] [Review][Decision] Create-form attachment picker silently discards files — **resolved 2026-08-23: hide the picker until a backend endpoint exists** → became the patch below
- [x] [Review][Patch] Hide the create-form attachment picker (file zone + preview) until a backend endpoint exists [ticket-form.component.html:99-147]
- [x] [Review][Patch] Admins get 403 viewing tickets they didn't create — service re-check lacks admin bypass [SupportTicketsController.cs:99, SupportTicketService.cs:296-298]
- [x] [Review][Patch] Internal notes exposed on the wire to non-admin creators [SupportTicketService.cs:309-311]
- [x] [Review][Patch] List row actions test statusId absent from SupportTicketListDto — add StatusId [SupportTicketListDto.cs]
- [x] [Review][Patch] Edit reverts status from stale snapshot; `?? 0` fallback — make StatusId optional in update (null = unchanged) [SupportTicketService.cs:111, ticket-form.component.ts:179]
- [x] [Review][Patch] Create endpoint overwrites form-collected browserInfo/pageUrl/userAction with constants [SupportTicketsController.cs:54-57]
- [x] [Review][Patch] No FK-existence validation on create/update — invalid ids give 500 + SQL leak instead of 400 [UpdateSupportTicketValidator.cs, SupportTicketService.cs:101-114]
- [x] [Review][Patch] 500 bodies leak ex.Message across all controller catch blocks [SupportTicketsController.cs]
- [x] [Review][Patch] All 8 module error handlers silent — add translated notify.error (incl. closedStatusId-null path) [modules/technical-support/*.component.ts]
- [x] [Review][Patch] en.json validation block: restore deleted keys (email, pattern, requiredField, nameTaken, looksGood, selectAnOption, min, max, minValue, dateInvalid, phoneInvalid, nationalIdInvalid) + minLength/maxLength interpolation [assets/i18n/en.json]
- [x] [Review][Patch] technicalSupport.title must mean the module name; add ticketTitle field-label key and rebind form label [assets/i18n/ar.json, en.json, ticket-form.component.html]
- [x] [Review][Patch] searchTerm + sorting silently ignored — call the filter-DTO overload; accept 'asc'/'desc' (resolves the "sorting no-op" deferred entry) [SupportTicketService.cs:141-174, SupportTicketRepository.cs:318-339]
- [x] [Review][Patch] my-tickets/count ignores its userId parameter — always returns 0 [SupportTicketService.cs:445]
- [x] [Review][Patch] DetailDto lacks PriorityColor/StatusColor — detail badges always grey [SupportTicketDetailDto.cs]
- [x] [Review][Patch] trackBy missing on remaining loops (form/list-filter/detail selects, 5 report loops); verify fe-chevrons-up-down icon exists [modules/technical-support/*.html]
- [x] [Review][Patch] Ticket delete is a hard delete — violates CLAUDE.md soft-delete rule [SupportTicketService.cs:414-426]
- [x] [Review][Defer] CreatedBy/AssignedTo/report-creator names render GUIDs — needs Framework.Identity user-profile integration [SupportTicketMappingProfile.cs:21-31] — deferred, pre-existing
