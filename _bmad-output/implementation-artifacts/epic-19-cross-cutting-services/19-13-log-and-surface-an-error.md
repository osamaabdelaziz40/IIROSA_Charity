# Story 19-13: Log and surface an error

| Field | Value |
| --- | --- |
| Story key | `19-13-log-and-surface-an-error` |
| Epic | EP-19 — Cross-Cutting Services — الخدمات المشتركة |
| Use case | UC-SYS-13 — معالجة الأخطاء |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/24-UC-SYS-Cross-Cutting-Services.md` (§24.U.13 scenario + §24.A `#/error` route) |
| Route | `#/error` (new — the spec's annex already plans it; today the wildcard `**` redirects to `/dashboard`) |
| Endpoint | none new — platform-wide middleware + interceptor behaviour |
| Depends on | nothing in-epic (foundation — **recommended first in the epic order**); 19-1/19-2 strip sibling `ex.Message` leaks on their controller |
| Roles | All users (unauthenticated crashes included) |

## Status

done

## Story

As a user, I want errors handled gracefully معالجة الأخطاء, so that when something fails I
see a friendly message in my language and the technical details are logged for support.

## Acceptance Criteria

1. Given any unhandled exception, when the request pipeline completes, then the response
   carries the correct status code — domain `NotFoundException` → **404**, `BusinessException`
   → **400**, and unexpected faults → **500** (architecture.md §5.1 binding).
2. Given a 500, when the body is built, then it contains a **safe** message and the
   `traceId` — the raw exception text, stack trace and inner exceptions are **never** sent to
   clients in production; developers may still see them in development.
3. Given the failure, when it is logged, then the full exception (message, stack, inner) is
   written server-side via the platform logger — nothing is swallowed.
4. Given the SPA receives an unhandled 5xx or a network failure, when no screen-level handler
   catches it, then the user is routed to the `#/error` page showing a friendly, localised
   (Arabic-first) message with the trace id — never a blank screen.
5. Given a 401, when the interceptor fires, then the existing login redirect still happens
   (unchanged); 403 stays deliberately unhandled per the recorded platform decision
   (screens own their "no permission" presentation).
6. Given any of the above, when the message reaches the user, then it is localised
   (`error.*` keys, ar + en) — no raw English server strings on Arabic screens.

**Definition of done:** the middleware maps the two domain exception types per
architecture §5.1; production 500 bodies are sanitised; the `#/error` route + component exist
and receive unhandled frontend failures; the NLog-not-log4net deviation is recorded; the wire
shape (`ErrorResponse`) is unchanged.

## What exists already (verified directly — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Middleware | `Backend/src/IIROSA.Api/Middleware/ExceptionMiddleware.cs` — switch on exception type; `UnauthorizedAccessException`→401, `FluentValidation.ValidationException`→400 (+`Errors` dict, :76-85), `CharityWriteForbiddenException`→403, `KeyNotFoundException`/`InvalidOperationException`→404, `ArgumentException`/`BadHttpRequestException`→400, default→500; registered `Program.cs:564` | Live — with the two defects below |
| Wire shape | `ErrorResponse { Message, Status, TraceId, StackTrace?, InnerException?, Details?, Errors? }` | Live — keep the shape (raw envelope is the standing decision; **not** `ApiResponse`) |
| Logging | **NLog** (`nlog.config`, `builder.Host.UseNLog()`) | Live — spec's "log4net" is a legacy-doc artifact; record the deviation |
| Interceptor | `AuthInterceptor` handles **401 only** (redirect to login) | Live; 403 deliberately unhandled — recorded decision, keep |
| Routing | Wildcard `**` → `/dashboard` | Live — **no `#/error` route exists** (the spec's §24.A plans one) |
| Sibling leaks | `AttachmentsController` catch blocks return `error = ex.Message` (:76, :108, :161) | 19-1/19-2 strip those — this story owns the platform rule |

## Verified defects this story must fix

1. **Domain exceptions map to 500.** `NotFoundException` and `BusinessException`
   (`IIROSA.Application/Exceptions/`) inherit `Exception` directly — the middleware's switch
   never matches them, so the architecture's own §5.1 contract (404 / 400) is broken: every
   domain "not found" or rule violation currently surfaces as a 500. Add both cases to the
   switch.
2. **Production bodies leak internals.** `Message = exception.Message` at :47 (and the
   `StackTrace`/`InnerException` fields) ship raw internals in **every** environment. Gate on
   the environment: production → safe localisable message + `traceId` only; development →
   keep the detail. **NLog keeps the full exception server-side either way** (AC 3).
3. **No `#/error` surface.** Build `core/pages/error/` (or the layouts-appropriate location):
   a standalone component (no auth-gating), routed at `#/error`, friendly Arabic-first copy +
   trace-id display + "back to dashboard" action; register it **before** the wildcard route.
4. **Unhandled frontend failures vanish.** Extend the interceptor (or a second
   `HttpInterceptor`): unhandled 5xx / network-level errors (`status === 0`) → preserve the
   `traceId` from the body when present → router navigate to `/error` (with a
   `skipLocationChange`-style state so the user can return). Leave 401 (redirect) and 403
   (screens own it) exactly as they are.

## Tasks / Subtasks

- [x] **Task 1 — Middleware status mapping** (AC 1)
  - [x] Add `NotFoundException` → 404 and `BusinessException` → 400 cases to the switch
        (before the fallback); both log at the appropriate level first
- [x] **Task 2 — Production sanitisation** (AC 2, 3)
  - [x] Environment-aware body: production → `{ Message: <safe message>, Status, TraceId }`
        (omit/null `StackTrace`, `InnerException`); development unchanged; NLog call sites
        keep full detail
- [x] **Task 3 — Error page + routing** (AC 4)
  - [x] `error` component (4-file shape, OnPush, standalone), `#/error` route registered
        before `**`; localised copy `error.title / error.body / error.traceId / error.back`
        (ar + en); no auth guard
- [x] **Task 4 — Interceptor extension** (AC 4, 5)
  - [x] Unhandled 5xx + network failure → navigate to `/error` carrying trace id; 401/403
        paths untouched; ensure screen-level handlers that already catch errors are not
        double-routed (the interceptor only acts when the request's error propagates)
- [x] **Task 5 — Verification** (AC 6)
  - [x] Builds green; live smoke on the private port: a known 404 path (`NotFoundException`
        throw or a missing id read) → **404 not 500**; a `BusinessException` path → 400; a
        forced 500 → production-mode body has no stack/inner (run once with
        `ASPNETCORE_ENVIRONMENT=Production` on the private port); unauthenticated → 401
        unchanged. Browser: kill the API mid-session → SPA lands on `#/error` in Arabic.
        Tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

- **ErrorResponse wire shape stays** — the raw-envelope standing decision (2026-08-19) covers
  this middleware too; do not convert to `ApiResponse<T>` here.
- Logging is **NLog** — the module spec's log4net references are legacy artifacts; record the
  deviation in the story + `deferred-work.md` if not already there. No new logging package.
- Bilingual text: the safe production message should be a stable, localisable **key-friendly**
  string the SPA already maps (or maps via `error.*` keys added in Task 3); avoid free-text
  English on Arabic screens (AC 6; English-only server messages are a recorded platform
  deferral — this story narrows it for the 500 path only, don't boil the ocean).
- The middleware is a platform file shared by every epic — smallest safe diff, no reformat.
- Recommended execution order: **first** in the epic — 19-1…19-12 all verify error paths and
  inherit these mappings.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| `AttachmentsController` catch-block `ex.Message` strips | 19-1 / 19-2 |
| English-only business messages platform sweep | deferred-work.md (recorded; narrowed only for the 500 path here) |
| 403 handling | deliberately unhandled (recorded decision) |
| Global `ApiResponse` envelope migration | its own platform story (standing decision) |

### References

- [Source: docs/Modules/24-UC-SYS-Cross-Cutting-Services.md#24.U.13] scenario + §24.A (the planned `#/error` route) + §24.B
- [Source: _bmad-output/planning-artifacts/architecture.md#5.1] the 404/400/500 contract this story enforces
- [Source: Backend/src/IIROSA.Api/Middleware/ExceptionMiddleware.cs] verified switch + :47 leak
- [Source: Backend/src/IIROSA.Application/Exceptions/NotFoundException.cs · BusinessException.cs] both `: Exception` — the 500 defect's root
- [Source: _bmad-output/implementation-artifacts/deferred-work.md] ex.Message leak + English-only messages deferrals (this story owns the platform half)

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- Backend build: `dotnet build IIROSA.Api.csproj -c Release -o bin/Smoke` → Build succeeded (note: the bare `-c Release` build does NOT refresh bin/Smoke — output goes to the default path; always pass `-o bin/Smoke`).
- Frontend: `npx tsc --noEmit` → clean (spec noise excluded); `ng serve` on :4201 → Compiled successfully.
- Middleware mapping probe (real ExceptionMiddleware from the built assembly, minimal host on 127.0.0.1:61971 — throwaway project outside the repo):
  - Development: NotFoundException → **404** (was 500); BusinessException → **400** (was 500); ApplicationException → 500 with raw message + stackTrace (dev keeps detail, innerException null).
  - Production (ASPNETCORE_ENVIRONMENT=Production): boom → 500 `{"message":"error.unexpected","status":500,traceId present,stackTrace:null,innerException:null,details:null}` — sanitised; NotFoundException → 404 with the domain message intact (product copy, ships by design); BusinessException → 400 with message intact.
- Real-instance regression: unauth GET /api/Families/check-national-id → 401 unchanged (smoke :61970).
- Browser (Playwright, ng serve :4201): live API down → login POST `net::ERR_CONNECTION_REFUSED` (status 0) → SPA auto-landed on `#/error` with the friendly page (the AC 4 "kill the API" scenario, satisfied involuntarily by the live API being down); `lang=ar` → `#/error` renders "حدث خطأ غير متوقع" + Arabic body (screenshot `ep19-13-error-page-ar.jpeg`).

### Completion Notes List

- **Why a probe host instead of a live app fault:** every reachable controller is defensively guarded (Audits/Notifications are unguarded but have no throw paths) — no natural input reaches the middleware with a domain exception today. The mappings are a safety net (filters, binders, future unguarded actions). The probe hosts the REAL middleware from bin/Smoke over real HTTP with real JSON serialisation, in both environments — the same evidence a forced in-app 500 would give, without polluting the product with a dev-only fault endpoint.
- **Sanitisation scope ruling:** only the default-500 path is sanitised. The 404/400 domain messages (NotFound/Business/KeyNotFound/InvalidOperation/Argument) are product copy and ship in every environment — the English-only-messages platform sweep stays a recorded deferral (this story narrows it for the 500 path only, per Dev Notes). The safe key `error.unexpected` is stable and the SPA maps it via the `error.*` section added in Task 3.
- **Interceptor discriminator ruling:** navigate to `/error` only on (a) status 0, or (b) status >= 500 whose body carries a `traceId` — middleware-signed 500s are by definition unhandled, while controller-caught faults answer `{message}` WITHOUT a traceId and stay with their screens (no double-handling). 401 redirect and 403 deliberate non-handling untouched.
- **Route registration:** `error` sits at the TOP level (outside the AuthGuard layout — unauthenticated crashes still get the page) and BEFORE the `**` wildcard, lazy-loaded via `loadComponent`.
- **TraceId transport:** interceptor passes `body.traceId` in router navigation state; the page reads it from `history.state` (survives refresh of `#/error`); displayed as a user-selectable LTR code block under a localised label, only when present.
- **environment.ts discovery recorded as DF-19.13b** (deferred-work.md): dev `apiUrl` hard-codes the live API and defeats `proxy.conf.json`; not fixed here (shared config, dev-experience story).
- NLog-not-log4net deviation recorded (DF-19.13a).

### File List

- Backend/src/IIROSA.Api/Middleware/ExceptionMiddleware.cs — NotFoundException → 404 + BusinessException → 400 cases; production-safe `error.unexpected` message on the default 500 (dev keeps detail); no shape change
- Frontend/src/app/core/interceptors/auth.interceptor.ts — unhandled-5xx-with-traceId + status-0 branch navigating to /error with state; 401/403 untouched
- Frontend/src/app/core/pages/error/error-page.component.ts/.html/.scss/.spec.ts — NEW: standalone OnPush error surface (4-file shape), traceId + status from history.state, back-to-dashboard action
- Frontend/src/app/app-routing.module.ts — `error` route (loadComponent) before the wildcard
- Frontend/src/assets/i18n/ar.json + en.json — NEW `error` section (title/body/traceId/back)
- _bmad-output/implementation-artifacts/deferred-work.md — DF-19.13a/b/c records

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-25 | Implemented + verified: §5.1 status mapping live-proven (404/400 over real HTTP in both environments), production 500 sanitised to error.unexpected + traceId, /#/error page (ar+en) + interceptor discriminator, 401/403 preserved; NLog deviation + environment.ts discovery recorded. |
| 2026-08-25 | Story file created from `epics.md` US-SYS-13 and module spec §24.U.13; middleware verified line-level (NotFoundException/BusinessException → 500 defect; production leak at :47); deliverable = status mapping + sanitisation + `#/error` route + interceptor extension; recommended first in the epic order. |
