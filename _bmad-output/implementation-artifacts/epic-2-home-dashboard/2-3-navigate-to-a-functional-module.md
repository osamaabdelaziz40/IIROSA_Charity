# Story 2.3: Navigate to a functional module — التنقل بين الوحدات

| Field | Value |
| --- | --- |
| Story | US-DSH-03 (UC-DSH-03) |
| Epic | EP-02 — Home Dashboard (chapter 7 · §7.U.3) |
| Priority / size | Should · 2 points |
| Route | every sidebar route — the side menu of `MainLayoutComponent` (`#/dashboard`, `#/families`, `#/orphan-payments`, …) |
| Endpoint | none (client-side routing) — but every **target** endpoint must authorise server-side (the menu is UX, not a control) |
| Depends on | EP-01 platform auth (live: `AuthGuard`, `AuthService`, claims); benefits from every landed module route |
| Roles | All roles — menu entries the role may not use are not rendered |

Status: ready-for-dev

## Story

As a signed-in user,
I want to navigate to a functional module from the collapsible side menu,
so that the router activates the corresponding screen — and entries my role may not use are not rendered.

## Acceptance Criteria

1. Given a sidebar entry gated by `hasPermission('<key>')`, when the current user's roles are not in that key's `PERMISSION_ROLES` mapping, then the entry **is not rendered**. Today every such entry renders for everyone: `MainLayoutComponent.hasPermission` is a stub returning `true` (`main-layout.component.ts:320-323`) while the real gate sits unused in `AuthService.hasPermission` (`auth.service.ts:452`). This story wires the stub to the service.
2. Given a menu entry the role may not use, when it is hidden, then the corresponding **server endpoint still refuses that role** — hiding is UX polish, the endpoint is the control (board non-goal: client-side-only authorisation).
3. Given the rendered menu, when any entry is clicked, then it resolves to a **registered route** — no entry may silently fall through the `**` wildcard into `/dashboard`. A dead-link audit over every `routerLink` in `main-layout.component.html` against the registered routes (app-routing + the 16 lazy modules) removes or re-points the dead ones.
4. Given an unauthenticated visitor, when any protected route is requested, then `AuthGuard` redirects to `/auth/login`; given an unknown hash, then the router lands on `/dashboard` (`''` and `**` redirects stay; the `/error` route registered outside the layout at `app-routing.module.ts:117` keeps precedence over the wildcard — 19-13's surface).
5. Given a route whose `data.permission` is missing from `PERMISSION_ROLES`, when `PermissionGuard` evaluates it, then the documented warn-and-allow behaviour (`auth.service.ts:447-450`) applies — this story adds **no** blanket deny; it only documents the map.
6. Given the menu after this story, then a menu→route→permission map is recorded (in this story's Dev Agent Record) so later modules know the convention: gate via `hasPermission('<Module>.<Action>')` once the module owns a key; `hasAnyRole([...])` only while no key exists.

**Definition of done:** §7.U.3 passes end to end; every rendered menu link routes; role gating on the menu is live and matches `PERMISSION_ROLES`; no regression in the `/error` precedence or login redirect; builds green.

## What exists already

| Layer | State (working tree, 2026-08-26) |
| --- | --- |
| Menu | `layouts/main-layout/main-layout.component.html` — the TinyDash collapsible sidebar with ~20 dropdown groups. Entries gate via `hasPermission('Families.View' | 'Reports.View' | …)` **or** raw `hasAnyRole([...])` (office-projects, housing, seasonal-aid, payments, cheques, missions, correspondence groups). |
| **The defect** | `MainLayoutComponent.hasPermission()` (`main-layout.component.ts:320`) is `// TODO: Implement permission check` `return true;` — so **every** `hasPermission()`-gated entry currently renders for all roles. The real implementation exists on `AuthService` (`hasPermission`, `auth.service.ts:452`) with the `PERMISSION_ROLES` map (`:60`). |
| Guards | `AuthGuard` on the layout (`app-routing.module.ts:17`); `PermissionGuard` (`core/guards/permission.guard.ts`) reads `route.data['permission']`, redirects to `/dashboard?error=insufficient_permissions` on refusal. Unknown permission keys warn-and-allow by design (`auth.service.ts:447-450`). |
| Routes | 16 lazy modules registered + `/error` + `''`/`'**'` → `/dashboard` redirects. Known suspects for the dead-link audit (verify, don't assume): `/admin/impersonation/sessions` + `/admin/impersonation/history` (`main-layout.component.html:70,76` — **no `admin` path is registered anywhere**), `/user-management/users/create` (`:150`), and the `/create` links at `:176` (employees), `:202` (charities), `:228` (office-projects), `:254` (housing), `:281` (seasonal-aid), `:351` (orphan-payments), `:660` (missions), `:740` (technical-support) — several modules use `:id/edit` or dialogs instead of a `/create` child, so some of these silently bounce off the wildcard. |
| Permissions | `PERMISSION_ROLES` (`auth.service.ts:60`) holds keys for Charities, Families, OrphanCoding, Reports, PeriodicReports, HqTransfers and a few more. Dashboard itself needs **no** entry (all roles). |

## Tasks / Subtasks

- [ ] Task 1 — Wire the stub (AC: 1, 2): `MainLayoutComponent.hasPermission(permission)` delegates to `this.authService.hasPermission(permission)` (inject if missing); delete the TODO stub. No template change needed — every `hasPermission(...)` call site starts gating for real.
- [ ] Task 2 — Dead-link audit (AC: 3): extract every `routerLink` in `main-layout.component.html`; resolve each against the registered routes (app-routing + each lazy module's routing file); for each dead link either remove the entry (impersonation links — the feature is reachable via the quick-impersonation dialog, and no `/admin` route exists by design) or re-point it to the module's real create/edit route. Record the disposition of every audited link in the Dev Agent Record.
- [ ] Task 3 — Gating consistency pass (AC: 6): for menu groups currently on raw `hasAnyRole([...])` **where an equivalent `PERMISSION_ROLES` key already exists**, switch to `hasPermission('<key>')`. Groups with no key keep `hasAnyRole` — do NOT invent permission keys for other modules' endpoints here (each module owns its keys; inventing them here risks diverging from server policy).
- [ ] Task 4 — Redirect behaviour (AC: 4): verify (and only fix if broken) `''` → `/dashboard`, `**` → `/dashboard`, `/error` precedence outside the layout, `AuthGuard` → `/auth/login`; verify `PermissionGuard`'s insufficient-permissions landing still works after Task 1 (a hidden menu entry is unreachable, but a deep link must still bounce correctly).
- [ ] Task 5 — Verify: `npm run build` green; manual matrix — SuperAdmin sees everything, a Charity-role token loses the HQ-only groups (users, employees, charities admin, HQ transfers, cheques…), Accountant/FinancialOfficer keep the finance groups; click-through of every rendered entry lands on its screen (no wildcard bounces); deep-linking a forbidden route still redirects. Backend untouched — no dotnet gate beyond a sanity build if nothing backend changes.

## Dev Notes

### Platform rules that bind this story

- No client-side-only authorisation: this story improves menu UX; it must not be read as "the menu is the control". Endpoint `[Authorize]` policy is untouched and remains the enforcement (board legacy-defect note; PRD §7).
- No new i18n for existing entries (labels already keyed); any removed entry's keys stay in `ar.json`/`en.json` unless provably orphaned.
- `PERMISSION_ROLES` unknown-key warn-and-allow is **documented platform behaviour**, not a bug to fix in this story (`auth.service.ts:447-450`).
- 4-file component shape untouched for the layout (no new files unless the audit demands it); OnPush not imposed on the legacy layout in this story (risk/reward too thin — record if changed).

### Story-specific rulings

- The two `/admin/impersonation/*` dropdown items are **dead** (no `admin` route exists) — remove rather than register routes: impersonation is not in the 232-story scope and its dialog flow (`openQuickImpersonation`, guarded by `canImpersonate`) is the working surface.
- `hasAnyRole` entries that mirror server role sets (e.g. payments group `SuperAdmin,Admin,Accountant,FinancialOfficer,Charity` — §15 role matrix) are correct as-is until their module owns keys; normalising them wholesale is deferred to each module's own story, not batched here (scope discipline: 2 points).
- The `**` wildcard redirect to `/dashboard` is the UC-DSH-03 landing behaviour per §7.A (`#/** → #/dashboard`) — keep.

### Out of scope (do not build)

| Item | Story |
| --- | --- |
| Dashboard summary / charts | 2-1 / 2-2 |
| New `PERMISSION_ROLES` keys for modules that lack them | each owning module's stories |
| Server-side policy changes of any kind | — (endpoints already authorise) |
| Mobile/responsive sidebar redesign | — (TinyDash layout is the platform shell) |

### References

- [Source: docs/Modules/07-UC-DSH-Home-Dashboard.md#7.U.3] · [#7.A]
- [Source: Frontend/src/app/layouts/main-layout/main-layout.component.ts:320-323] (the stub)
- [Source: Frontend/src/app/core/services/auth.service.ts:60,447-468] (`PERMISSION_ROLES`, warn-and-allow, `hasPermission`)
- [Source: Frontend/src/app/core/guards/permission.guard.ts] · [auth.guard.ts]
- [Source: Frontend/src/app/app-routing.module.ts:113-126] (`/error` precedence, wildcard)
- [Source: _bmad-output/planning-artifacts/prd.md#7] (client-side-only authorisation non-goal)

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

### Change Log

- 2026-08-26 — created (EP-02 context pass) → ready-for-dev.
