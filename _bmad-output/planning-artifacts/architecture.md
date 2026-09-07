---
title: IIROSA Charities — Approved Architecture
project: IIROSA Charities
version: 1.0
date: 2026-08-18
status: approved
source: Architecture/01_ProjectArchitecture.md, 02_QuickStartGuide.md, 03_FrameworkIntegration_Architecture.md, 04_Angular_Frontend_Architecture.md
stepsCompleted: [imported-from-approved-architecture]
---

# IIROSA Charities — Approved Architecture

This document is the **authoritative architecture reference** for the project. When any other
document, comment or habit conflicts with this file, this file wins.

It consolidates the four approved architecture documents under `Architecture/` and reconciles them
against the code as it actually stands in `Backend/` and `Frontend/`. Where the approved documents
and the code disagree, the reconciliation is recorded explicitly in §10 — nothing is silently
dropped.

| Approved source document | Governs | Status |
| --- | --- | --- |
| `Architecture/03_FrameworkIntegration_Architecture.md` | Backend structure, base entities, schemas, DI | **Authoritative** — supersedes 01 where they differ |
| `Architecture/01_ProjectArchitecture.md` | Repository/Service/UoW pattern, layer rules, API response, exceptions | Authoritative except where 03 corrects it |
| `Architecture/02_QuickStartGuide.md` | Module creation recipe and checklist | Authoritative for *process* |
| `Architecture/04_Angular_Frontend_Architecture.md` | Frontend structure, shared components, i18n, TinyDash | **Authoritative** for the frontend |

---

## 1. Architectural style

**Clean Architecture (4 layers) + Repository + Unit of Work + Service layer.**

Explicitly **not** CQRS and **not** MediatR. Controllers inject application services directly.
There is no `IRequest<T>`, no `Send()`, no command/query separation.

```
IIROSA.Api  ──►  IIROSA.Application  ──►  IIROSA.Domain
                          │
                 IIROSA.Infrastructure  ──►  IIROSA.Domain

Framework.Core      ── referenced by all layers
Framework.Identity  ── referenced by Infrastructure + Api
```

**Dependency rule:** inner layers never reference outer layers. A `using IIROSA.Infrastructure`
inside `IIROSA.Domain` or `IIROSA.Application` is an architecture violation, not a style choice.

### 1.1 Layer responsibilities

| Layer | Purpose | Contains | May depend on | Must not contain |
| --- | --- | --- | --- | --- |
| **IIROSA.Domain** | Core business entities and domain contracts | Entities, lookups, domain interfaces, entity configurations, contracts | Framework.Core only | EF Core query code, DTOs, service logic |
| **IIROSA.Application** | Business logic and orchestration | Service interfaces + implementations, DTOs, AutoMapper profiles, FluentValidation validators, enums | Domain, Framework.Core | Data-access code, `DbContext`, HTTP concerns |
| **IIROSA.Infrastructure** | Data access and external services | `ApplicationDbContext`, repositories, migrations, interceptors, infrastructure services, DI extensions | Domain, Application, Framework.* | Business rules |
| **IIROSA.Api** | HTTP surface | Controllers, middleware, SignalR hubs, filters, DI composition | All layers | Business rules — controllers stay thin and delegate to services |

---

## 2. Backend solution structure

Target framework: **.NET 8** (`net8.0`) across every project.
Solution file: `Backend/IIROSA.sln`.

```
Backend/
├── Framework/
│   ├── Framework.Core/                  # Shared framework — do not fork per project
│   │   ├── Data/
│   │   │   ├── EntityBase.cs             # Root base for all entities
│   │   │   ├── BaseDbContext.cs          # Auto-discovery DbContext base
│   │   │   ├── Repositories/             # IRepositoryBase, RepositoryBase
│   │   │   ├── Uow/                      # IUnitOfWorkBase, UnitOfWorkBase
│   │   │   ├── Mapping/                  # EntityTypeConfiguration conventions
│   │   │   └── Paging/                   # Paging primitives
│   │   ├── ApiResponse.cs                # ApiResponse, ApiResponse<T>, Item
│   │   ├── AutoMapper/                   # Mapping infrastructure
│   │   ├── Caching/                      # ICacheService
│   │   ├── Notifications/                # Notification services
│   │   ├── Globalization/                # Localization support
│   │   ├── BackgroundJobs/
│   │   ├── SharedServices/               # Seed, shared cross-cutting services
│   │   └── DependencyManagement/         # Convention-based DI registration
│   │
│   └── Framework.Identity/              # ASP.NET Identity — users, roles, claims, tokens
│       ├── Data/                         # AppIdentityDbContext, services, DTOs
│       └── Migrations/
│
├── src/
│   ├── IIROSA.Domain/
│   │   ├── Entities/                     # Business entities (Charity, Family, Orphan, …)
│   │   │   ├── Base/                     # FullAuditedEntity, LookupEntity, LookupEntity<TKey>
│   │   │   ├── Lookups/                  # Lookup entities
│   │   │   └── TechnicalSupport/         # Module-scoped entity folders
│   │   ├── Configurations/               # MappingDefaults + entity configurations
│   │   ├── Contracts/                    # IAppDbContext and domain contracts
│   │   └── Interfaces/                   # Repository interfaces, one per aggregate
│   │
│   ├── IIROSA.Application/
│   │   ├── Interfaces/                   # I<Entity>Service
│   │   ├── Services/                     # <Entity>Service
│   │   ├── DTOs/<Entity>/                # Dto, CreateDto, UpdateDto, FilterDto, ListDto
│   │   ├── Profiles/                     # AutoMapper profiles
│   │   ├── Validators/                   # FluentValidation validators
│   │   └── Enums/
│   │
│   ├── IIROSA.Infrastructure/
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs   # Auto-discovers entities — no manual DbSets
│   │   │   ├── Interceptors/             # AuditLogSaveChangesInterceptor
│   │   │   ├── Repository/               # <Entity>Repository + generic Repository.cs
│   │   │   └── Migrations/
│   │   ├── Services/                     # Infrastructure service implementations
│   │   ├── Extensions/                   # ServiceCollection extensions
│   │   └── Migrations/
│   │
│   └── IIROSA.Api/
│       ├── Controllers/                  # ApiController base + <Entity>Controller
│       ├── Middleware/                   # Exception handling, correlation, etc.
│       ├── Hubs/                         # SignalR hubs
│       ├── Attachments/
│       └── Properties/
│
└── tests/
```

> **Naming note.** The approved documents call the presentation project `IIROSA.Web`. The codebase
> names it **`IIROSA.Api`**, and it is API-only (the UI is the separate Angular app). `IIROSA.Api`
> is the canonical name. See §10.

---

## 3. Domain model rules

### 3.1 Base classes — every entity inherits one of these

| Base class | Key | Use for | Provides |
| --- | --- | --- | --- |
| `FullAuditedEntity` (→ `Framework.Core.Data.FullAuditedEntityBase<Guid>`) | `Guid` | **Main business entities** — Charity, Family, Orphan, Mission, Check, … | `Id` (assigned in ctor), `CreatedOn`, `CreatedBy`, `UpdatedOn`, `UpdatedBy`, `DeletedOn`, `DeletedBy`, `IsDeleted` |
| `LookupEntity` (→ `Framework.Core.Data.LookupEntityBase`) | `int` | **Lookup / reference tables** — Country, Region, Center, Bank, … | `Id`, `Name`, `NameAr`, `NameEn`, `IsActive` (defaults `true`), audit fields |
| `LookupEntity<TKey>` | `TKey : struct` | Lookups that need a non-`int` key | As above with a generic key |

Rules:

- **Never re-declare audit fields** on an entity. They are inherited. Adding your own `CreatedDate`
  or `IsDeleted` is a defect.
- **Never write an `IAuditable` interface.** It was removed by the corrected architecture — the base
  class replaces it.
- Business entities live in `IIROSA.Domain/Entities/`; lookups live in `Entities/Lookups/` (or a
  module-scoped `Lookups/` subfolder).
- Entities are POCOs with public getters and setters and navigation properties. This project does
  **not** use private setters / behaviour-only aggregates.

### 3.2 Database schemas

Two schemas, and only two. Both are declared as constants in
`IIROSA.Domain/Configurations/MappingDefaults.cs` — never hard-code the string.

| Constant | Schema | Holds |
| --- | --- | --- |
| `MappingDefaults.IIROSA_SCHEMA` | `IIROSA` | Every entity deriving from `FullAuditedEntity` |
| `MappingDefaults.LOOKUP_SCHEMA` | `Lookup` | Every entity deriving from `LookupEntity` |

Identity tables are owned by `Framework.Identity` (`AppIdentityDbContext`); framework tables
(attachments, settings, notification templates) are owned by `Framework.Core` (`CommonsDbContext`).
`ApplicationDbContext` owns **business entities only**.

### 3.3 DbContext — entity auto-discovery

`ApplicationDbContext : BaseDbContext<ApplicationDbContext>, IAppDbContext`.

- **No manual `DbSet<T>` properties.** Entities are discovered from the `IIROSA.Domain` assembly by
  `BaseDbContext`. Declaring `DbSet<Charity>` is a violation.
  *(The only sanctioned exception in the code today is the small set of Technical-Support lookup
  `DbSet`s required by seeding.)*
- Entity configuration goes in `EntityTypeConfiguration<T>` classes, applied by convention.
- Auditing is applied by `AuditLogSaveChangesInterceptor`, not by hand in services.

---

## 4. Data access — Repository + Unit of Work

- `IRepositoryBase` / `RepositoryBase` and `IUnitOfWorkBase` / `UnitOfWorkBase` come from
  `Framework.Core.Data`. Do not write a competing generic repository.
- Per-aggregate repository **interfaces** live in `IIROSA.Domain/Interfaces/`; their
  **implementations** live in `IIROSA.Infrastructure/Data/Repository/`, alongside the generic
  `Repository.cs`. *(`IIROSA.Infrastructure/Repositories/` exists but is empty — do not use it.)*
- **Repositories never call `SaveChanges`.** Persistence happens once, through the Unit of Work.
- Multi-step writes that must succeed or fail together are wrapped in a single UoW transaction.
- Soft delete is the default: set `IsDeleted`, never issue a hard `DELETE`, and always filter
  deleted rows out of reads.

---

## 5. Application layer

- One service interface + implementation per aggregate: `ICharityService` / `CharityService`.
- Services are registered by convention through `Framework.Core.DependencyManagement`; new services
  do not require hand-edited DI unless they fall outside the convention.
- **DTOs, never entities, cross the API boundary.** Per aggregate, in `DTOs/<Entity>/`:
  `<Entity>Dto`, `Create<Entity>Dto`, `Update<Entity>Dto`, `<Entity>FilterDto`, `<Entity>ListDto`.
- Mapping is AutoMapper, configured in `IIROSA.Application/Profiles/`.
- Validation is FluentValidation, in `IIROSA.Application/Validators/`. **Validate in the service
  layer** — do not rely on the controller alone.
- Business rules live here, not in controllers and not in repositories.

### 5.1 Custom exceptions

Defined in the Application layer and translated to HTTP by the API middleware:

| Exception | Meaning | Maps to |
| --- | --- | --- |
| `NotFoundException(entityType, entityId)` | Requested record does not exist | 404 |
| `ValidationException(List<string> errors)` | Input failed validation | 400 |
| `BusinessException(message)` | A business rule refused the operation | 400 / 409 |

---

## 6. API layer

### 6.1 Controllers

- Every controller inherits **`ApiController`** (`IIROSA.Api/Controllers/ApiController.cs`), which
  supplies `[ApiController]`, `[Authorize(JwtBearer)]`, `[Route("api/[controller]")]`, request
  logging, and `CurrentUserId` / `CurrentUserName` / `CurrentUserEmail`.
- Controllers are **thin**: bind → delegate to a service → return. No business logic, no EF Core, no
  `DbContext`.
- Read the caller's identity from the `ApiController` members — not from `HttpContext.User` scattered
  through services.

### 6.2 Response envelope

Every endpoint returns `Framework.Core.ApiResponse` or `ApiResponse<T>`. Do not invent per-controller
response shapes.

| Member | Type | Meaning |
| --- | --- | --- |
| `Success` | `bool` | Defaults to `true`; set `false` on failure |
| `Message` | `string` | Human-readable outcome message (localised) |
| `Value` | `object` / `T` | The payload |
| `ModelStateErrors` | `ICollection<Item>` | Field-level errors — `Item(Name, Value, ErrorCode)` |
| `Confirm` | `bool` | The message needs user confirmation rather than being a plain alert |

> The `ApiResponse` sketched in `01_ProjectArchitecture.md` (with `Data`, `Errors`, `Timestamp`) was
> a proposal. The shipped `Framework.Core.ApiResponse` above is the real contract, and the Angular
> client is written against it. See §10.

### 6.3 Real-time

SignalR hubs are hosted by `IIROSA.Api/Hubs/` (notifications and dashboard). The client connects via
`@microsoft/signalr`.

---

## 7. Frontend architecture

**Angular 18**, standalone-capable, TypeScript 5.x.

| Concern | Choice |
| --- | --- |
| UI framework | Bootstrap 5.3 + **TinyDash dark RTL** template |
| Components | Angular Material + ng-bootstrap + ngx-bootstrap + ng-select |
| i18n | **`@ngx-translate/core`** — `assets/i18n/{ar,en}.json`. Arabic is the primary language; the app is **RTL-first** |
| State | NgRx (`src/app/store/`) for cross-cutting state; Signals for local component state |
| HTTP | `HttpClient` + interceptors: `auth`, `error`, `loading`, `locale` |
| Forms | Reactive Forms |
| Charts | ApexCharts |
| Real-time | `@microsoft/signalr` |
| Export | ExcelJS (Excel), jsPDF (PDF) |
| Alerts | SweetAlert2 |

### 7.1 Structure (`Frontend/src/app/`)

```
core/          interceptors/ · guards/ (auth, permission, module) · services/ · models/
shared/        components/ · pipes/ · directives/ (hasPermission) · validators/
layouts/       main-layout/ (sidebar, header, footer) · auth-layout/
modules/       one folder per feature — components/ services/ models/
store/         actions/ reducers/ effects/ selectors/ state.ts
assets/        i18n/ · css/ (TinyDash) · js/ · images/
environments/  environment.ts · environment.prod.ts
```

### 7.2 Mandatory shared components

These three exist to stop every module re-inventing the same UI. **Use them; do not fork them.**

| Component | Responsibility |
| --- | --- |
| `shared/components/attachment` | All file upload / download / preview against the Framework attachment API |
| `shared/components/input-fields` | Dynamic, metadata-driven form field rendering with validation and lookups |
| `shared/components/data-list` | All listing screens — paging, sorting, filtering, row actions, export |

Supporting shared components: `modal`, `toast`, `loading`, `confirm-dialog`, `page-header`,
`empty-state`, `chart`.

### 7.3 Frontend rules

- Feature modules are **lazy-loaded**.
- `OnPush` change detection; `trackBy` on every `*ngFor`; virtual scrolling for long lists.
- Permission-driven UI via the `hasPermission` directive and `permission.guard`.
  **Client-side hiding is not an authorisation control** — the server must enforce it too.
- Every user-facing string goes through the translation files. No hard-coded Arabic or English in
  templates.
- Keep the 4-file component shape (`.ts`, `.html`, `.scss`, `.spec.ts`).

---

## 8. Cross-cutting concerns

| Concern | Owner | Rule |
| --- | --- | --- |
| Identity, roles, claims | `Framework.Identity` | **Never** write a local User or Role entity |
| Notifications | `Framework.Core.Notifications` | Reuse; do not re-implement |
| Attachments | Framework attachment service | Reuse |
| Settings | Framework settings service | Reuse |
| Audit log | `AuditLogSaveChangesInterceptor` | Automatic — do not stamp audit fields by hand |
| Caching | `ICacheService` | Never inject `IMemoryCache` directly |
| Localization | `Framework.Core.Globalization` + ngx-translate | Every entity with user-facing text carries `NameAr` / `NameEn` |
| Multi-tenancy | Application services | Every query is scoped by the caller's charity/country. Tenancy is enforced **server-side**. |

---

## 9. Adding a module — required order

From `Architecture/02_QuickStartGuide.md`, corrected for the framework integration:

1. **Domain** — entity inheriting `FullAuditedEntity` (or `LookupEntity`); navigation properties;
   entity configuration using `MappingDefaults`; repository interface in `Domain/Interfaces/`.
2. **Infrastructure** — repository implementation in `Infrastructure/Data/Repository/`; EF migration.
   *(No `DbSet` to add — auto-discovery handles it.)*
3. **Application** — DTOs (`Dto`, `Create`, `Update`, `Filter`, `List`); service interface + service;
   AutoMapper profile; FluentValidation validator.
4. **API** — controller inheriting `ApiController`, returning `ApiResponse<T>`.
5. **Frontend** — lazy-loaded feature module reusing `data-list`, `input-fields`, `attachment`;
   `ar.json` / `en.json` entries.
6. **Tests** — unit tests for the service, integration tests for the controller.
7. **Docs** — update the module use case document and the sprint status board.

---

## 10. Reconciliation: approved documents vs. shipped code

Recorded deliberately so nobody "fixes" the code toward a stale document.

| # | Approved document says | Code does | Ruling |
| --- | --- | --- | --- |
| 1 | Presentation project `IIROSA.Web` | `IIROSA.Api` (API-only; UI is the separate Angular app) | **Code wins** — `IIROSA.Api` |
| 2 | `ApiResponse` with `Data` / `Errors` / `Timestamp` (doc 01) | `Framework.Core.ApiResponse` with `Value` / `ModelStateErrors` / `Confirm` | **Code wins** — the Angular client is built on it |
| 3 | AutoMapper profiles in `Mappers/` | `IIROSA.Application/Profiles/` | **Code wins** — `Profiles/` |
| 4 | Repositories in `Infrastructure/Data/Repository/` | Same — `Data/Repository/` (27 repositories) | **Agreed** — `Data/Repository/`. An empty `Infrastructure/Repositories/` folder also exists; ignore it |
| 5 | Audit fields `CreatedDate` / `ModifiedDate` (doc 03 sketch) | `CreatedOn` / `UpdatedOn` / `DeletedOn` | **Code wins** |
| 6 | Entities implement `IAuditable`; add `DbSet` + `OnModelCreating` (doc 02 checklist) | Base class + auto-discovery | **Doc 03 wins** — doc 02's checklist predates the framework integration |
| 7 | Controllers inherit `BaseController<TDto, TEntity>` (doc 01) | Controllers inherit `ApiController` | **Code wins** — `ApiController` |
| 8 | Frontend i18n includes `fr.json` | `ar.json` / `en.json` only | Arabic + English are in scope; French is not |

### Known debt carried over from the copied codebase

These were inherited with the copy and are **not** approved architecture:

- Duplicate stray trees: `Backend/Backend/**` and `Backend/Framework/src/**` shadow real files
  (notably the ImportExport module). Consolidate into `Backend/src/**` before building on them.
- `*.cs.bak.bak2` files under `IIROSA.Api/Controllers/`.
- Duplicate service pairs (`LookupManagementService` / `LookupManagementManagementService`,
  `EmployeeService` / `EmployeeAppService`) — pick one per aggregate.
- `src/Presentation/` is an empty leftover of the `IIROSA.Web` naming.
- `src/IIROSA.Infrastructure/Repositories/` is an empty folder that shadows the real
  `Data/Repository/`. Delete it, or it will attract misplaced files.

### Ratified platform rulings — epic-18 review, 2026-08-26 (§10 D1)

The epic-18 delivery recorded a set of "code wins" story rulings that sit against the binding
convention table. The 2026-08-26 code review escalated them as D1; the platform owner ratified
the set platform-wide. Where the CLAUDE.md table and the rulings below disagree, **the ruling
below wins until formally overturned**:

| # | Ruling | Scope note |
| --- | --- | --- |
| R1 | Reports endpoints return **raw paged/JSON envelopes**, not `ApiResponse<T>`, where a story recorded that shape | Applies to epic-18 report reads. Mixed error shapes *inside one controller* were normalized in the review patch — new endpoints pick one error shape per controller |
| R2 | `DashboardController : ControllerBase` | The 17-1 convention; do not "fix" toward `ApiController` on touch |
| R3 | Report screens may omit `OnPush` | Perf posture, not a control; new screens prefer OnPush but its absence is not a defect |
| R4 | Report screens bypass `data-list` / `input-fields` | Bespoke grids are accepted for report projections; CRUD modules still use the shared components |
| R5 | Hand projections over AutoMapper in report services | The 25+ hand projections stand; AutoMapper remains the mapper for entity↔DTO elsewhere |
| R6 | `NameAr`/`NameEn` flattened server-side into single display strings | Report DTO contract; bilingual charity names remain blocked by DF4 (entity lacks the columns) |
| R7 | Auth interceptor leaves 403 unhandled | As designed (18-40/18-41 AC5 as written is superseded); the endpoint `[Authorize]` is the control |
| R8 | **Client-side print + ExcelJS** supersede a server PDF/Excel pipeline | Epic-wide recorded path; `POST {reportKey}/export/pdf` returns a print payload (JSON) and has a live consumer — kept, not renamed |
| R9 | 18-33 bank field omitted from the bank-file contract | In-story AC relaxation stands (recorded) |

Not ratified: hard-coded English server strings (P16) — deferred to a platform message-catalog
decision; the tenancy fail-closed ladder (P1) and the review's security fixes are **binding
direction**, not deviations.

---

## 11. Target scope

The functional scope this architecture must deliver is the WAR.IIROSA use case catalogue:
**19 epics · 230 user stories · 838 points**, specified in `docs/Modules/` and tracked in
`_bmad-output/implementation-artifacts/sprint-status.yaml`.

The legacy WAR.IIROSA system (.NET Framework 4.8, AngularJS, Web Forms authentication, Crystal
Reports) is the **functional** reference only. Its technical choices are explicitly replaced by the
architecture above. Two capabilities in the backlog are new and have no legacy equivalent:
JWT access tokens (`1-10`) and refresh-token rotation (`1-11`).
