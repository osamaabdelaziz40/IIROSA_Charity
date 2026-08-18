# CLAUDE.md

Guidance for Claude Code when working in this repository.

## Project overview

**IIROSA Charities** — a multi-tenant welfare administration platform run by a head office (HQ) and
used by partner charities (الجمعيات) across several countries. It manages the orphan
sponsorship cycle end to end: beneficiary register, periodic orphan reports, financial disbursement,
programme management, and correspondence & reporting.

This is a **re-platform**. Functional scope comes from the documented WAR.IIROSA system
(19 modules · 230 use cases); the technical platform is .NET 8 + Angular 18.

**`_bmad-output/planning-artifacts/architecture.md` is the authoritative architecture reference.
When in doubt, defer to it.** It also records where the approved design documents and the shipped
code disagree (§10) — read that section before "fixing" code toward a document.

| Artifact | Path |
| --- | --- |
| Architecture (authoritative) | `_bmad-output/planning-artifacts/architecture.md` |
| PRD | `_bmad-output/planning-artifacts/prd.md` |
| Epics & user stories (232) | `_bmad-output/planning-artifacts/epics.md` |
| Delivery board | `_bmad-output/implementation-artifacts/sprint-status.yaml` |
| Module specifications (19) | `docs/Modules/` |
| Approved design documents | `Architecture/01`–`04` |

## Tech stack

- **.NET 8** (`net8.0`), ASP.NET Core, EF Core, SQL Server
- **Angular 18**, TypeScript 5.x, Bootstrap 5.3 + TinyDash dark RTL, Angular Material,
  ng-bootstrap, ng-select
- **AutoMapper**, **FluentValidation**, **SignalR**, **NgRx** + Signals
- **@ngx-translate/core** for i18n (Arabic primary, RTL-first), **ApexCharts**, **ExcelJS**,
  **SweetAlert2**
- In-house `Framework.Core` and `Framework.Identity` shared projects

## Build & test commands

```bash
# Backend
dotnet build Backend/IIROSA.sln
dotnet run    --project Backend/src/IIROSA.Api/IIROSA.Api.csproj
dotnet watch run --project Backend/src/IIROSA.Api/IIROSA.Api.csproj
dotnet test   Backend/IIROSA.sln

# EF Core migrations
dotnet ef migrations add <Name> \
  --project Backend/src/IIROSA.Infrastructure/IIROSA.Infrastructure.csproj \
  --startup-project Backend/src/IIROSA.Api/IIROSA.Api.csproj
dotnet ef database update \
  --project Backend/src/IIROSA.Infrastructure/IIROSA.Infrastructure.csproj \
  --startup-project Backend/src/IIROSA.Api/IIROSA.Api.csproj

# Frontend  (node_modules is not committed — restore first)
cd Frontend && npm install
npm start        # ng serve with src/proxy.conf.json
npm run build
npm test
```

## Solution structure

```
Backend/
  Framework/
    Framework.Core/          Base entities, ApiResponse, RepositoryBase, UnitOfWorkBase,
                             BaseDbContext, caching, notifications, globalization, DI conventions
    Framework.Identity/      ASP.NET Identity — users, roles, claims, tokens
  src/
    IIROSA.Domain/           Entities, Lookups, Configurations, Contracts, Interfaces
    IIROSA.Application/      Services, Interfaces, DTOs, Profiles, Validators, Enums
    IIROSA.Infrastructure/   Data/ (ApplicationDbContext, Repository/, Interceptors/, Migrations/),
                             Services, Extensions
    IIROSA.Api/              Controllers, Middleware, Hubs, Attachments
  tests/
Frontend/src/app/
  core/ shared/ layouts/ modules/ store/
docs/Modules/                19 module use case specifications
Architecture/                Approved design documents (01–04)
_bmad/                       BMAD Method v6.2.2 (core + bmm)
_bmad-output/                planning-artifacts/ · implementation-artifacts/
```

## Architecture: Clean Architecture + Repository/Service/UoW

```
IIROSA.Api  ──►  IIROSA.Application  ──►  IIROSA.Domain
                         │
                IIROSA.Infrastructure  ──►  IIROSA.Domain
Framework.Core  (referenced by all layers)
```

Inner layers never reference outer ones.

### Always do this

| Pattern | Rule |
| --- | --- |
| Entity base class | `FullAuditedEntity` (Guid) for business entities, `LookupEntity` (int) for lookups |
| Audit fields | Inherited — `CreatedOn`, `CreatedBy`, `UpdatedOn`, `UpdatedBy`, `DeletedOn`, `DeletedBy`, `IsDeleted` |
| DB schema | `MappingDefaults.IIROSA_SCHEMA` / `MappingDefaults.LOOKUP_SCHEMA` — never a literal string |
| API responses | `Framework.Core.ApiResponse` / `ApiResponse<T>` — `Success`, `Message`, `Value`, `ModelStateErrors`, `Confirm` |
| Controller base | Inherit `ApiController` (`IIROSA.Api/Controllers/ApiController.cs`) |
| Object mapping | AutoMapper profiles in `IIROSA.Application/Profiles/` |
| Validation | FluentValidation in `IIROSA.Application/Validators/`, invoked in the **service layer** |
| Persistence | Only `IUnitOfWork` saves — repositories never call `SaveChanges` |
| Deletes | Soft delete via `IsDeleted`; always filter deleted rows out of reads |
| Caching | `ICacheService` — never `IMemoryCache` directly |
| Current user | `ApiController.CurrentUserId` / `CurrentUserName` / `CurrentUserEmail` |
| Tenancy | Every query scoped to the caller's Charity/country, enforced **server-side** |
| Domain term | The tenant organisation is a **Charity**. "NGO" and "Association" are retired from docs and new code; legacy `Ngo`-prefixed identifiers quoted from WAR.IIROSA stay verbatim |
| Bilingual text | `NameAr` / `NameEn` on every entity with user-facing text |

### Never do this

- **No MediatR, no CQRS, no `IRequest<T>`.** Controllers inject services directly.
- **No manual `DbSet<T>`** on `ApplicationDbContext` — entities auto-discover from `IIROSA.Domain`.
- **No `IAuditable` interface** and no hand-declared audit fields — the base class provides them.
- **No local `User` or `Role` entity** — identity belongs to `Framework.Identity`.
- **No business logic in controllers or repositories** — it belongs in the Application layer.
- **No entities crossing the API boundary** — DTOs only.
- **No untyped `JObject` request binding** — this was a legacy defect (see `prd.md` §7).
- **No hard-coded UI strings** — everything goes through `ar.json` / `en.json`.
- **No client-side-only authorisation** — hiding a menu is not a control; the endpoint must authorise.

### Frontend rules

- Reuse the three mandatory shared components rather than re-implementing them:
  `shared/components/data-list` (all listings), `input-fields` (dynamic forms),
  `attachment` (all file handling).
- Lazy-load feature modules; `OnPush` change detection; `trackBy` on every `*ngFor`.
- Keep the 4-file component shape: `.ts`, `.html`, `.scss`, `.spec.ts`.
- RTL-first — Arabic is the primary language.

## Naming conventions

| Element | Convention | Example |
| --- | --- | --- |
| C# class | PascalCase | `CharityService` |
| Interface | `I` prefix | `ICharityService` |
| Private field | `_camelCase` | `_charityRepository` |
| Entity | Singular | `Charity`, `Orphan` |
| DTOs | `<Entity>Dto`, `Create<Entity>Dto`, `Update<Entity>Dto`, `<Entity>FilterDto`, `<Entity>ListDto` | |
| DB table | PascalCase singular | `Charity`, `Family` |
| API route | `api/[controller]` | `/api/Charities` |
| JSON property | camelCase | `nameAr`, `totalCount` |
| Angular file | kebab-case | `orphan-list.component.ts` |

## Adding a module

Follow the order in `architecture.md` §9: Domain → Infrastructure → Application → API → Frontend →
Tests → Docs. Then update `_bmad-output/implementation-artifacts/sprint-status.yaml`.

## BMAD Method

This project uses **BMAD Method v6.2.2**. Configuration: `_bmad/bmm/config.yaml` and
`_bmad/core/config.yaml`. Skills are installed under `.claude/skills/` — invoke them as
slash commands, e.g.:

| Skill | Use |
| --- | --- |
| `/bmad-sprint-status` | Current board state and recommended next action |
| `/bmad-create-story` | Draft the next story file from the backlog |
| `/bmad-dev-story` | Implement a ready story |
| `/bmad-code-review` | Review a story implementation |
| `/bmad-create-epics-and-stories` | Regenerate or extend the backlog |
| `/bmad-correct-course` | Handle a scope or requirement change |

Story files are written to `_bmad-output/implementation-artifacts/<story-key>.md`.

## Current state

The backend and frontend were copied from the previous IIROSA Charities implementation and provide
working Clean Architecture scaffolding plus partial module implementations (charities, families,
orphan payments, periodic reports, missions, housing projects, cheques, correspondence, technical
support, lookups, user/role management).

The delivery board imports with **all 232 stories at `backlog`** — the WAR.IIROSA scope has not been
delivered on this stack. Stories tagged `legacy: implemented` have a reference implementation in the
old system to read from; they are not done here.

### Known debt inherited with the copy

Clean these up before building on the affected areas — they are **not** approved architecture:

- Duplicate stray trees `Backend/Backend/**` and `Backend/Framework/src/**` shadow real files
  (notably the ImportExport module).
- `*.cs.bak.bak2` files under `IIROSA.Api/Controllers/`.
- Duplicate service pairs: `LookupManagementService` / `LookupManagementManagementService`,
  `EmployeeService` / `EmployeeAppService`.
- `Backend/src/Presentation/` and `Backend/src/IIROSA.Infrastructure/Repositories/` are empty
  leftovers — the real repositories live in `Infrastructure/Data/Repository/`.
- `node_modules/`, `bin/`, `obj/` were intentionally not copied — restore with `npm install` and
  `dotnet restore`.
