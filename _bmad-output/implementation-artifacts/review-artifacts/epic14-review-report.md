# Epic-14 Code Review Report — 2026-08-23

| Field | Value |
| --- | --- |
| Scope | All uncommitted epic-14 changes (21 files; diff: `epic14-review.diff`) |
| Review mode | `full` (specs: story files 14-4/14-6, `docs/Modules/19-UC-CST-Technical-Support.md`, CLAUDE.md) |
| Layers | Blind Hunter (diff-only) · Edge Case Hunter (diff + project) · Acceptance Auditor (diff + specs) |
| Outcome | **2 decision-needed · 17 patch · 1 defer · 14 dismissed** |

## Verified strengths (audit trail)

- 14-4 AC 1–5 and 14-6 AC 1–4 genuinely met; Dev Agent Record accurate (Acceptance Auditor).
- Module conventions honored: bare DTO returns, `ValidationException` → 400 with per-field errors,
  UoW-only saves, server-side role checks on every mutating endpoint, `{ items, totalCount }` wire
  shape, `ticketsResolvedWithinSLA` casing.
- Route-order defect genuinely fixed (`reports` before `:id`); phantom API surface fully removed;
  `deferred-work.md` epic-14 section matches the code.
- Validator auto-registration confirmed (`IIROSA.Application/ServiceCollectionExtensions.cs:91`).
- All 102 module translate keys resolve in both locales.

## Decision-needed (2)

- **D1 — Co-mingled i18n hunks.** `ar.json`/`en.json` working-tree diffs mix epic-14's additions
  with unrelated in-flight work (cheques `checkCreated`, `seasonalAid.*` ~55 keys,
  `generalChecks.*` ~150 lines, `userManagement.roles` removal). Cannot be attributed to epic 14
  from the diff; must be separated before commit or consciously combined. *Sources: blind+auditor+edge.*
- **D2 — Create-form attachment picker silently discards files.** No backend endpoint exists
  (`AttachFileToTicketAsync` has no route); user attaches a screenshot, gets success, file never
  arrives. Recorded deferral kept the picker — review recommends hiding it. *Sources: blind+edge.*

## Patches (17)

| # | Finding | Location |
| --- | --- | --- |
| P1 | **Admins get 403 viewing tickets they didn't create** — controller admits admins, then `GetTicketDetailsAsync` re-checks access without admin knowledge and throws (→ Forbid). All-Tickets view and admin edit are broken for others' tickets. Fix: plumb `isAdmin` through the service. | `SupportTicketsController.cs:99`, `SupportTicketService.cs:296-298` |
| P2 | **Internal notes exposed on the wire to non-admin creators** — `Responses` always carries all responses + `InternalNotes`; the frontend only filters visually. Fix via the same `isAdmin` plumbing: blank internal data for non-admins. | `SupportTicketService.cs:309-311` |
| P3 | **List row actions test `statusId` that `SupportTicketListDto` never returns** → `undefined !== closedStatusId` is always true: Close shows on closed rows, Solve on closed-unsolved rows. Fix: add `StatusId` to the list DTO (auto-maps). | `SupportTicketListDto.cs`, `ticket-list.component.ts:137,143` |
| P4 | **Edit silently reverts status from a stale snapshot; `?? 0` fallback yields cryptic 400.** Fix: make `StatusId` optional in `UpdateSupportTicketDto` (null = leave unchanged); frontend omits instead of sending 0. | `SupportTicketService.cs:111`, `ticket-form.component.ts:179` |
| P5 | **Create endpoint overwrites form-collected context** — `BrowserInfo`/`PageUrl`/`UserAction` unconditionally replaced (UserAction becomes constant "Create Ticket"), defeating UC-CST-01's reproduction-context capture. Fix: only fill when empty. | `SupportTicketsController.cs:54-57` |
| P6 | **No FK-existence validation on create/update** — invalid category/priority/status ids reach EF (`Restrict` FKs) → 500 with the SQL constraint error in the body instead of a 400 field error. Fix: existence checks in validators/service. | `UpdateSupportTicketValidator.cs`, `SupportTicketService.cs:101-114` |
| P7 | **500 bodies leak `ex.Message`** across all catch blocks (includes SQL errors, per P6). Fix: drop `error = ex.Message`; details stay in logs. | `SupportTicketsController.cs` (all catches) |
| P8 | **All 8 error handlers are silent** (`error: () => { … }` with no toast) — failed saves/status changes/responses give zero feedback; also the `closedStatusId == null` path no-ops silently. Fix: translated `notify.error`. | `modules/technical-support/*.component.ts` |
| P9 | **en.json `validation` block lost keys still in active use** — `email`, `pattern`, `requiredField`, `nameTaken`, `looksGood`, `selectAnOption`, `min`, `max`, `minValue`, `dateInvalid`, `phoneInvalid`, `nationalIdInvalid` deleted (present in ar); `minLength`/`maxLength` lost `{{minLength}}`/`{{maxLength}}` interpolation. Consumers include shared `text-input`/`drop-down` components — English users see raw keys app-wide. Fix: restore keys + interpolation (additive, safe). | `assets/i18n/en.json` |
| P10 | **`technicalSupport.title` now means "Title"/"العنوان"** — the module name key was hijacked as a field label (sidebar menu, page headers, breadcrumb, route title all show "Title"). Fix: restore module name; add `technicalSupport.ticketTitle` for the field label. | `assets/i18n/{ar,en}.json`, `ticket-form.component.html` |
| P11 | **Search box and sortable headers are silent no-ops** — service calls the 8-param overload which has no `SearchTerm`/sort; the fully functional filter-DTO overload (`SupportTicketRepository.cs:54-120`) is never called. Also: `ApplySorting` only recognizes `"ascending"` while the frontend sends `'asc'`; `GetAllTicketsAsync` passes `filter.AssignedTo` into the `createdByUserId` slot. Fix: call the filter overload (+ set `CreatedByUserId` for my-tickets), accept `asc`/`desc`. Resolves the standing "sorting no-op" deferred-work entry. | `SupportTicketService.cs:141-174`, `SupportTicketRepository.cs:318-339` |
| P12 | **Report status tiles are global all-time counts, not period-scoped** — `GetTicketsByStatusCountAsync(1..4)` ignores the date range and uses hardcoded seed ids; tiles can contradict `totalTickets`. Fix: compute from the period-scoped `tickets` collection. | `SupportTicketService.cs:388-391` |
| P13 | **`my-tickets/count` always returns 0** — `GetMyTicketsCountAsync(userId)` ignores its parameter and queries `CreatedByUserId == ""`. One-word fix. Dormant (no consumer yet) but wired in the frontend service. | `SupportTicketService.cs:445` |
| P14 | **Detail-page badges never get lookup colours** — `SupportTicketDetailDto` has no `PriorityColor`/`StatusColor` (list DTO only); every detail badge renders the grey fallback. Fix: add + map both. | `SupportTicketDetailDto.cs`, `SupportTicketMappingProfile.cs:33-34` |
| P15 | **CSV export lacks formula-injection neutralization** — leading `=`/`+`/`-`/`@` in user-controlled titles/messages execute as formulas in Excel. Plus `getPercentage` returns NaN when `totalTickets` is 0. | `support-report.component.ts` |
| P16 | **`trackBy` missing on remaining loops** — form selects, list filter selects, detail status select, all five report loops (CLAUDE.md: trackBy on every `*ngFor`). Also verify the `fe-chevrons-up-down` icon exists in the icon set. | `ticket-form/ticket-list/ticket-detail/support-report .html` |
| P17 | **Ticket delete is a hard delete** (`Repository.Remove`, responses cascade) — violates the CLAUDE.md soft-delete rule (`IsDeleted`). Fix: soft-delete the ticket. | `SupportTicketService.cs:414-426` |

## Deferred (1)

- **W1 — Requester names render GUIDs.** `CreatedByUserName`/`CreatedByEmail` map straight from
  `CreatedByUserId`, `AssignedToName` from `AssignedTo`, and `TicketsByCreator` groups by the raw
  user id — list/detail/report show 36-char GUIDs. Needs a Framework.Identity user-profile
  lookup/join; pre-existing backend limitation surfaced by the epic-14 wiring.
  *(edge+auditor; recorded in `deferred-work.md`.)*

## Dismissed (14) — with reasons

1. **[blind] "Lookup → LookupDto AutoMapper maps missing → GET lookups 500s"** — refuted: `SupportTicketMappingProfile.cs:116-135` maps all three lookup entities.
2. **[blind] "`IValidator<UpdateSupportTicketDto>` registration unverified"** — refuted: `AddValidatorsFromAssembly` at `ServiceCollectionExtensions.cs:91`.
3. **[blind] "`GET lookups` lacks `[Authorize]`"** — refuted: class-level `[Authorize(AuthenticationSchemes = JwtBearer…)]` at `SupportTicketsController.cs:16`.
4. **[edge] CRITICAL "API serializes PascalCase — every read yields undefined"** — refuted: `.AddNewtonsoftJson()` with no explicit resolver applies the framework's `CamelCasePropertyNamesContractResolver` (`MvcNewtonsoftJsonOptionsSetup` — applied by the ASP.NET Core package, hence invisible to a project-source grep). Empirically, the entire pre-existing app binds camelCase (`nameAr`, `totalCount`, …) through this exact pipeline; PascalCase would blank every module platform-wide. The `ticketsResolvedWithinSLA` casing note stands.
5. **[blind] "viewMode route data not visible in diff"** — false positive: routes carry `data.viewMode` (hunk context simply omitted it).
6. **[blind] "mass service-method removal may orphan callers"** — refuted by builds: backend 0 errors, frontend success.
7. **[blind] "detail edit button not admin-gated → owner gets 403"** — refuted: edit action is admin-gated, consistent with the PUT's role attribute.
8. **[blind] "RouterModule import is dead code"** — false: it activates the report template's previously broken breadcrumb links.
9. **[blind] "unused enums"** — retained wire models; harmless.
10. **[blind] "`ticketId` sent but absent from request interface"** — required by design: the controller enforces `id == dto.TicketId`.
11. **[blind] "`userAction` dropped from edit payload"** — by design: `UpdateSupportTicketDto` has no user-context fields; UC-CST-04 edits title/message/category/priority only.
12. **[auditor] "`''` route lacks viewMode → admins land on my-tickets"** — defensible UX; admin tabs switch views; no spec breach.
13. **[auditor] "list is hand-rolled, not shared `data-list`"** — recorded approved deferral ("do not retrofit").
14. **[blind] Arabic locale defects in contaminated hunks (`"isDone": "CheckDone"`, "إيذون")** — real, but authored by the co-mingled in-flight work, outside epic-14's changes; left to that effort (see D1).
