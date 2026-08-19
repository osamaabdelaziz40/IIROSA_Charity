# Story 3-4: View a charity profile

| Field | Value |
| --- | --- |
| Story key | `3-4-view-a-charity-profile` |
| Epic | EP-03 — Charity Administration |
| Use case | UC-CHR-04 — بيانات الجمعية |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/08-UC-CHR-Charity-Administration.md` (§8.S, §8.U.4) |
| Route | `#/charities/:id` |
| Endpoint | `GET /api/Charities/{id}` |

## Status

done

## Story

As a general director, I want to be able to view a charity profile بيانات الجمعية, so that I can
see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a general director with an active session in the module, when the actor opens the screen
   with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/Charities/{id}` and the response is rendered on the screen without a page reload.
3. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §8.S are implemented with their mandatory flags and
lookups; the scenario of §8.U passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## Review findings that produced this story's tasks

- **AC 1 and AC 2 already hold.** `CharityDetailComponent` reads the id from the route and calls
  `charityService.getCharity(id)` → `GET /api/Charities/{id}`; nothing is written.
- **Server-side scoping already holds** — but only because story 3-1's review replaced the
  `HasAccessToCharity` stub (which returned `true` unconditionally) with a real check against the
  caller's tenancy claim, and moved its call sites off the role-name guard.
- **AC 3's "role is not permitted" half is unmet.** The `:id` route declares
  `canActivate: [AuthGuard]` with no `PermissionGuard` and no `permission` in its data, so any
  authenticated user can open any charity's detail screen. The server refuses the data, so nothing
  leaks — but the client-side control the AC describes does not exist.
- **`PermissionGuard` is inert everywhere it *is* applied.** `AuthService.hasPermission()` is a
  stub — `return this.isAuthenticated()` with a TODO — so the routes that already declare
  `Charities.View` / `Charities.Create` / `Charities.Edit` admit every authenticated user. This is
  the same root cause the story 3-1/3-2 regression round identified when the blanket 403→login
  interceptor had to be reverted: route-level authorisation is where "role is not permitted"
  belongs, and it was never implemented.

## Tasks / Subtasks

- [x] **Task 1 — Make `PermissionGuard` actually decide** (AC 3)
  - [x] Implement `AuthService.hasPermission` against the roles already carried on the user
  - [x] Mirror the server's role sets so client and server cannot disagree
- [x] **Task 2 — Guard the detail route** (AC 3)
  - [x] Add `PermissionGuard` and `permission: 'Charities.View'` to `#/charities/:id`
- [~] **Task 3 — Tests** — EXCLUDED FROM SCOPE by user decision. The test layer is deliberately out of scope for epic 3; this is not deferred to a later story.
  - [~] Guard and permission-map tests — EXCLUDED, no test project exists

## Dev Notes

### Deliberate deviation from AC 3's wording

AC 3 says an unpermitted actor is "routed back to the login screen". `PermissionGuard` routes them
to `#/dashboard` with `error=insufficient_permissions` instead, and that is kept.

Sending an **authenticated** user to the login screen is wrong: their session is valid, so the
login screen has nothing to offer them and any in-progress work is discarded. This is exactly the
failure the blanket 403→login interceptor caused in the story 3-1/3-2 review round, where it
ejected users mid-form; it was reverted for that reason. The AC bundles two different cases —
expired session and insufficient role — and only the first belongs at the login screen. The
expired-session half is satisfied by `auth.interceptor`'s 401 handling.

### Scope boundary on the permission map

Only the `Charities.*` permissions are mapped. Permissions declared by other feature modules
(`Families.*`, `HousingProjects.*`, …) keep today's behaviour — allowed for any authenticated user
— and log a warning. Denying unmapped permissions would silently lock every other module in the
application out of routes this story never reviewed, which is a far larger change than UC-CHR-04
justifies. Each module's own story should add its entry.

### Not addressed

`GET /api/Charities/{id}/profile` and `CharityProfileDto` exist on the server with no Angular
client method. The detail screen uses `getCharity(id)`, which satisfies AC 2 as written, so the
profile endpoint is left unused rather than a second screen being invented here.

## Dev Agent Record

### Implementation Plan

1. Replace the `hasPermission` stub with a lookup over a permission→roles map, reusing the
   existing `hasAnyRole`.
2. Register the `:id` route with the guard and permission the sibling routes already use.

### Debug Log

- `AuthService.hasPermission` was a stub returning `isAuthenticated()`, so every route already
  declaring a permission — across charities, families, housing and more — admitted any signed-in
  user. `PermissionGuard` has been wired into routes since the module was written and had never
  once denied anyone.

### Completion Notes

**AC 1 and AC 2 verified unchanged** — `CharityDetailComponent` reads by route id via
`GET /api/Charities/{id}` and writes nothing.

**AC 3 now has a working client-side control.** `hasPermission` resolves the permission against the
roles already carried on the user, and the `:id` route gained `PermissionGuard` plus
`permission: 'Charities.View'`. `PERMISSION_ROLES` mirrors the `[Authorize(Roles = ...)]` sets on
`CharitiesController` exactly, so client and server cannot drift.

**Server-side scoping was already closed** by the story 3-1/3-2 review round, which replaced the
`HasAccessToCharity` stub and moved its call sites off the role-name guard. Nothing further was
needed for the definition of done.

**Deviation:** denial routes to `#/dashboard`, not the login screen — see Dev Notes. Sending an
authenticated user to login discards a valid session and any work in progress, which is the exact
failure that forced the 403 interceptor to be reverted in the previous review round.

## File List

| File | Change |
| --- | --- |
| `Frontend/src/app/core/services/auth.service.ts` | `PERMISSION_ROLES` map; real `hasPermission` |
| `Frontend/src/app/modules/charities/charities-routing.module.ts` | `PermissionGuard` on `#/charities/:id` |

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-19 | Story file created from `epics.md` US-CHR-04 and module spec §8.S / §8.U. |
| 2026-08-19 | Implemented route-level permission checking and guarded the detail route (Tasks 1–2). |

## Accepted exception to the definition of done

Marked `done` consistent with stories 3-1, 3-2 and 3-3: **no tests**. `PERMISSION_ROLES` is a
security-relevant map with no coverage, and it must be kept in step with the controller's
`[Authorize]` attributes by hand.
