# IIROSA Charities - Routing Map

The routing contract of **this** project: the Angular 18 route each screen lives at, and the
ASP.NET Core endpoint each use case calls. It is the bridge between the use-case documentation in
this folder and the code under `Frontend/src/app` and `Backend/src/IIROSA.Api`.


| Field | Value |
| --- | --- |
| Document title | IIROSA Charities - Routing Map |
| Purpose | The Angular 18 route and API endpoint behind every documented use case |
| Version | 1.1 |
| Date | 19 August 2026 |
| Applies to | The 19 module documents in `docs/Modules/`, `epics.md`, and `sprint-status.yaml` |
| Architecture | `_bmad-output/planning-artifacts/architecture.md` (authoritative) |
| Full indexes | `00-Overview-and-Common-Context.md` Appendix A (routes), B (controllers), C (reports), D (services) |

## 1. Conventions

### 1.1 Client routes

- The router runs in **hash mode** (`useHash: true` in `app-routing.module.ts`), so a route written
  here as `#/charities` is the URL `https://<host>/#/charities`.
- `#/auth/**` renders in `AuthLayoutComponent`. Everything else renders in `MainLayoutComponent`
  behind `AuthGuard`, with `PermissionGuard` deciding per feature.
- `#/` redirects to `#/dashboard`; an unknown path redirects there too.
- Every feature module is **lazy-loaded** and follows the same child-route shape:

  | Child route | Purpose | Component |
  | --- | --- | --- |
  | `''` | List / search | `<Entity>ListComponent`, built on the shared `data-list` component |
  | `create` | New record | `<Entity>FormComponent`, built on the shared `input-fields` component |
  | `:id` | Read-only detail | `<Entity>DetailComponent` |
  | `:id/edit` | Edit | `<Entity>FormComponent` |

  A module adds further child routes only where the use cases need them (for example
  `#/orphan-payments/:id/add-orphans`, `#/periodic-orphan-reports/:id/review`).

### 1.2 API endpoints

- Route template `api/[controller]`, from `ApiController`, which also applies `[Authorize]`
  (JWT bearer) and request logging.
- REST verbs carry the intent: `GET` reads, `POST` creates or runs an operation, `PUT` replaces,
  `PATCH` flips a flag, `DELETE` soft-deletes. The legacy habit of doing everything over `GET` and
  `POST` is not carried forward.
- Every response is `Framework.Core.ApiResponse` / `ApiResponse<T>`.
- **Identity is never a parameter.** The legacy API passed `userId` and `ngoId` on the query string
  and let the business layer decide what to do with them. Here the caller's user, role, charity and
  country come from the JWT, and tenancy is applied server-side inside the application service. That
  is why no endpoint in this document takes a `userId` or a caller-supplied charity id; an HQ role
  that needs to act across charities passes an explicit `charityId` **filter**, and the service
  checks the role permits it.
- Reports take a filter body, so they are `POST /api/Reports/<report-key>`, with
  `POST /api/Reports/<report-key>/export/pdf` and `…/export/excel` for file output.

### 1.3 Status

*implemented* — the route or controller exists today in the repository.
*planned* — specified here, not yet built. The delivery board
(`_bmad-output/implementation-artifacts/sprint-status.yaml`) holds all 232 stories at `backlog`, so
*implemented* means "the scaffolding is in the tree", not "the use case is done".

## 2. Module routing map

One row per module: where the module lives in the client, which controller serves it, and its
delivery epic.


| Ch. | Prefix | Module | Angular feature module | Base route | API controller | Epic |
| --- | --- | --- | --- | --- | --- | --- |
| 6 | `UC-AUT` | Authentication & User Account | `auth` | `#/auth` | `api/Auth` | EP-01 |
| 7 | `UC-DSH` | Home Dashboard | `dashboard` | `#/dashboard` | `api/Dashboard` *(planned)* | EP-02 |
| 8 | `UC-CHR` | Charity Administration | `charities` | `#/charities` | `api/Charities` | EP-03 |
| 9 | `UC-EMP` | Employee & User Administration | `employees` · `user-management` | `#/employees` · `#/user-management` | `api/EmployeeManagement` · `api/UserManagement` · `api/RoleManagement` | EP-04 |
| 10 | `UC-FAM` | Family Register | `families` | `#/families` | `api/Families` | EP-05 |
| 11 | `UC-HOU` | Housing Project | `housing-projects` | `#/housing-projects` | `api/HousingProjects` | EP-06 |
| 12 | `UC-REF` | Refugee Families | `families` | `#/families/refugees` *(planned)* | `api/Families` (`familyType=Refugee`) | EP-07 |
| 13 | `UC-ORP` | Orphan Register & Coding | `families` | `#/families/orphans` *(planned)* | `api/Families` (orphans sub-resource) | EP-08 |
| 14 | `UC-ORR` | Orphan Periodic Reports | `periodic-orphan-reports` | `#/periodic-orphan-reports` *(built, unregistered)* | `api/PeriodicOrphanReports` · `api/OrphanReports` | EP-09 |
| 15 | `UC-PAY` | Orphan Payments & Disbursement | `orphan-payments` | `#/orphan-payments` | `api/OrphanPayments` | EP-10 |
| 16 | `UC-CHQ` | General Cheques | `general-checks` | `#/general-checks` *(uses `edit/:id`)* | `api/CheckManagement` | EP-11 |
| 17 | `UC-PRJ` | Seasonal Assistance Projects | `seasonal-aid` | `#/seasonal-aid` | `api/SeasonalAid` | EP-12 |
| 18 | `UC-OFP` | Office Development Projects | `office-development-projects` | `#/office-development-projects` | `api/OfficeProjectManagement` | EP-13 |
| 19 | `UC-CST` | Technical Support | `technical-support` | `#/technical-support` | `api/SupportTickets` | EP-14 |
| 20 | `UC-MSN` | Missions | `missions` | `#/missions` | `api/MissionManagement` | EP-15 |
| 21 | `UC-COR` | Correspondence | `incoming-outgoing` | `#/incoming-outgoing` | `api/IncomingOutgoing` | EP-16 |
| 22 | `UC-TRF` | HQ Financial Transfers | `hq-transfers` *(planned)* | `#/hq-transfers` *(planned)* | `api/HqTransfers` *(planned)* | EP-17 |
| 23 | `UC-RPT` | Reports & Printing | `reports` *(planned)* | `#/reports` *(planned)* | `api/Reports` *(planned)* | EP-18 |
| 24 | `UC-SYS` | Cross-Cutting Services | `core` · `shared` | — (embedded) | `api/LookupManagement` · `api/Attachments` · `api/Notification` | EP-19 |

## 3. Gaps this map exposes

Recorded because they are real work, not documentation defects.


| # | Gap | Consequence |
| --- | --- | --- |
| 1 | `periodic-orphan-reports` has a routing module and components but **no entry in `app-routing.module.ts`** | The whole of chapter 14 is unreachable in the running client until the route is registered. |
| 2 | No `hq-transfers` feature module and no `HqTransfersController` | Chapter 22 (8 use cases) has no home yet. |
| 3 | No `reports` feature module and no `ReportsController` | Chapter 23 (41 use cases, the largest epic) has no home yet. `ReportViewerComponent` and the `report-key` scheme in Appendix C are the proposed shape. |
| 4 | No `DashboardController` | Chapter 7 reads counters that nothing serves yet. |
| 5 | Refugee families and orphan coding have no routes of their own | They are specified as child routes of `families`; chapters 12 and 13 depend on that decision holding. |
| 6 | `NGOType` lookup entity and the `NgoMapLocation` property still carry the old name in code | The documentation says *Charity type* and *map location*. Renaming the entity, the property and its migration closes the last `Ngo` in the codebase. |
| 7 | `general-checks` declares `reconcile` **after** `:id` | The router matches `:id` first, so `#/general-checks/reconcile` opens `CheckDetailComponent` with `id = "reconcile"`. `CheckReconcileComponent` is unreachable until the two are reordered. |
| 8 | `general-checks` uses `edit/:id` where every other module uses `:id/edit` | The only feature module that breaks the child-route shape in §1.1. Chapter 16 documents the deviation rather than the convention. |

## 4. How the legacy references were retired

The module documents used to carry the legacy realisation: an AngularJS state (`Ngos`, route
`/Ngos`), a Web API 2 endpoint (`GET /api/Ngo?userId`), a controller method and a static BLL method.
All four have been replaced by this project's equivalents:


| Legacy element | Replaced by |
| --- | --- |
| AngularJS SPA state + route | The Angular 18 route and the component that renders it |
| `/api/<LegacyController>/<Action>` | The REST endpoint on the owning `ApiController` |
| `/Print/<Action>` (Crystal Reports) | `POST /api/Reports/<report-key>/export/pdf` |
| `<X>Controller` (66 of them) | The 24 controllers in Appendix B |
| `<X>BLL` static class | The application service in Appendix D |
| `?userId` / `?ngoId` query parameters | JWT claims, applied server-side |
| `Ngo`, `Ngos`, `NgoId`, `FK_NgoId`, `LcokType.Ngo` | `Charity`, `Charities`, `CharityId`, `CharityLockType.Charity` |

Where a legacy call has **no** direct equivalent yet, this map gives the endpoint the module *will*
expose, derived from the conventions in §1, and marks it *planned*. Nothing was invented to fill a
gap silently — §3 lists every gap.

---

Shared context is in [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md); the module
specifications are indexed in [00-INDEX-Module-Documentation-Set](00-INDEX-Module-Documentation-Set.md).
