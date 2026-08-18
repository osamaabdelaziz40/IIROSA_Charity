# WAR.IIROSA - Overview & Common Context


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Overview & Common Context |
| Contents | Chapters 1-5 and 26, plus Appendices A-G of the master document |
| Purpose | Shared reading for every module specification in this set |
| Version | 1.3 |
| Date | 18 August 2026 |
| Architecture | Chapter 2 and chapter 26 state the **approved** re-platform architecture (.NET 8 Clean Architecture + Angular 18). Authoritative source: `_bmad-output/planning-artifacts/architecture.md`, consolidating `Architecture/01`-`04`. |
| Terminology | The tenant organisation is a **Charity** (الجمعية). The terms "NGO" and "Association" are retired from this documentation set; legacy code identifiers keep their original `Ngo` spelling — see §2.7. |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## 1. Introduction

### 1.1 Purpose of this document

This document is the shared context for the **IIROSA Charities** re-platform. It serves two purposes at once:

1. **Functional scope.** It is the consolidated functional documentation of the WAR.IIROSA system, produced by a full static scan of that solution source code — Web API controllers, MVC print controllers, the AngularJS single-page front end, the business logic layer and the Entity Framework data model — and it catalogues every use case that must be delivered on the new platform.
2. **Target architecture.** Chapter 2 and chapter 26 state the **approved** architecture the new system is built to: .NET 8 Clean Architecture with Angular 18, from the approved design documents `Architecture/01`-`04`, consolidated in `_bmad-output/planning-artifacts/architecture.md`.

The legacy technical design is kept only where it carries traceability — §2.7 and appendices A-D, all marked *legacy reference*.

It is intended for:


| Audience | How they use this document |
| --- | --- |
| Business analysts / product owners | Authoritative inventory of existing behaviour, as the baseline for change requests. |
| Developers & maintainers | Map from a business function to the exact screen, controller and endpoint that implements it. |
| QA engineers | Test-case derivation — each use case is a testable unit with actors, pre-conditions and flows. |
| Operations / support | Understand role-based access, locking, and the payment and reporting cycles. |
| Migration / modernisation team | Complete functional scope to be preserved when re-platforming. |
| Delivery team | Chapter 2 for the approved architecture and the rules every module must follow; chapter 26 for the target non-functional characteristics. |

### 1.2 System overview

IIROSA Charities is a web-based, multi-tenant welfare administration platform operated by a head office (HQ) and used by a network of partner charities across several countries. Its core mission is the end-to-end management of the orphan sponsorship cycle and the related family-welfare programmes. It replaces the WAR.IIROSA system, preserving its functional scope on a new technical platform.

The system covers five business pillars:


| Pillar | Description |
| --- | --- |
| Beneficiary register سجل المستفيدين | Family files, guardians (parents/widows), orphans (children), housing-project families and refugee families — including national-ID validation, orphan coding and inter-charity transfer of files. |
| Periodic reporting التقارير الدورية | Each sponsored orphan requires a recurring status report (health, education, behaviour, prayer, memorisation, family circumstances) with photographic and documentary attachments. HQ accepts or refuses each report with reasons. |
| Financial disbursement الدفعات والحوالات | Payment batches per charity, per-orphan payment rows, bank transfer file generation, CSV reconciliation, cheque issuance and printing, receipt confirmation and HQ financial transfers. |
| Programme management المشاريع | Seasonal assistance projects for families, office development projects, missions (field visits) and technical-support tickets. |
| Correspondence & reporting الصادر والوارد والتقارير | Incoming/outgoing official letters linked to orphan reports, plus an extensive catalogue of operational and statistical reports with printable and exportable output. |

### 1.3 Scope of this documentation

In scope: all functionality reachable from the application UI or exposed by the public Web API surface, the actors that may invoke it, the business rules that govern it, the reports and printed documents produced, and the approved architecture the system is built to (chapter 2).

Out of scope: database physical schema and stored-procedure internals, infrastructure/deployment topology, and third-party library internals (Crystal Reports, EPPlus, Entity Framework).

### 1.4 Definitions and abbreviations


| Term | Arabic | Meaning |
| --- | --- | --- |
| HQ | المكتب / المقر الرئيسي | Head office of the organisation; supervises all charities. |
| Charity | الجمعية | Partner charity; a tenant of the system that owns its own families and orphans. Formerly written "NGO" or "Association". |
| Family | الأسرة | The beneficiary household file; the root record that owns guardians and orphans. |
| Guardian / Parent / Sponsor-of-record | العائل / المعيل | The adult (usually the widowed mother) responsible for the orphans in a family. |
| Orphan / Child | اليتيم | The sponsored beneficiary child. |
| Orphan code | كود اليتيم | The unique sponsorship number assigned to an orphan; a prerequisite for payments and reports. |
| Periodic report | التقرير الدوري | The recurring orphan status report submitted by the charity and approved by HQ. |
| Payment batch | الدفعة المالية | A disbursement run covering many orphans across one or more charities. |
| Batch number | رقم الدفعة | Identifier of a payment batch as seen by a charity. |
| Bank file | ملف البنك / كشف التحويلات | The Excel/CSV file handed to the bank to execute the transfers of a batch. |
| Meza card | كرت ميزة | National payment card registered against a guardian, used as the disbursement channel. |
| Exclusion | الاستبعاد | Removal of an orphan from the sponsorship programme. |
| Incoming / Outgoing | الوارد / الصادر | Official correspondence register. |
| Mission | المأمورية | A field visit / assignment carried out by staff. |
| SPA | — | Single-Page Application. In the target system this is the Angular 18 client under `Frontend/`; in the legacy system it was the AngularJS front end. |
| UoW | — | Unit of Work — the repository/transaction pattern used by the data layer. In the target system only the UoW calls `SaveChanges`. |
| DTO | — | Data Transfer Object. Entities never cross the API boundary; DTOs do. |
| HQ role claims | — | Role and permission claims carried in the JWT and enforced server-side on every endpoint. |

### 1.5 How to read the use cases

Each functional chapter contains one or more use-case tables. The columns are:


| Column | Meaning |
| --- | --- |
| ID | Stable identifier, format UC-<MODULE>-<nn>. Use this ID in change requests and test cases. |
| Use case | The goal the actor wants to achieve, stated as a verb phrase, with the Arabic UI wording where one exists. |
| Primary actor | The role that initiates it. See chapter 4 for role definitions. |
| Description & main flow | What the system does, including the business rules enforced and the significant alternate outcomes. |
| Realisation | The Angular route and component, and the API endpoint, controller and application service that serve it — the traceability link into the code. |

Chapter 25 then gives fully expanded specifications (pre-conditions, step-by-step main flow, alternate and exception flows, post-conditions, business rules) for the ten most critical end-to-end scenarios.

## 2. System Architecture

This chapter describes the **approved target architecture** of the re-platformed system — not the
architecture of the legacy WAR.IIROSA solution. The functional scope in chapters 6–24 is inherited
from the legacy system; the technical platform is not.

The architecture is defined by the four approved design documents under `Architecture/` and
consolidated — with every document-versus-code conflict resolved — in
`_bmad-output/planning-artifacts/architecture.md`. **That consolidated file is the authoritative
reference; where this chapter and it disagree, it wins.**


| Approved source document | Governs |
| --- | --- |
| Architecture/01_ProjectArchitecture.md | Clean Architecture layering, Repository / Service / Unit of Work, API response envelope, custom exceptions |
| Architecture/02_QuickStartGuide.md | The module creation recipe and its checklist (authoritative for *process*) |
| Architecture/03_FrameworkIntegration_Architecture.md | Base entities, database schemas, DbContext auto-discovery, convention-based DI — supersedes 01 where they differ |
| Architecture/04_Angular_Frontend_Architecture.md | Frontend structure, mandatory shared components, i18n, TinyDash RTL template |

The legacy three-tier system is kept in **§2.7** as a *functional reference only*. It is what the
`Realisation` column of every use-case table and the §<n>.A / §<n>.B annexes point at, so
traceability back into the WAR.IIROSA source still resolves. Its technical choices — .NET Framework
4.8, AngularJS, Forms Authentication, Crystal Reports, stored-procedure reporting, `JObject` request
binding — are explicitly replaced and must not be reproduced.

### 2.1 Architectural style

**Clean Architecture (4 layers) + Repository + Unit of Work + Service layer.**

Explicitly **not** CQRS and **not** MediatR. Controllers inject application services directly; there
is no `IRequest<T>`, no `Send()`, and no command/query separation.

```
IIROSA.Api  ──►  IIROSA.Application  ──►  IIROSA.Domain
                          │
                 IIROSA.Infrastructure  ──►  IIROSA.Domain

Framework.Core      ── referenced by all layers
Framework.Identity  ── referenced by Infrastructure + Api
```

**Dependency rule:** inner layers never reference outer layers. A `using IIROSA.Infrastructure`
inside `IIROSA.Domain` or `IIROSA.Application` is an architecture violation, not a style choice.

### 2.2 Solution structure

Solution file `Backend/IIROSA.sln`; every project targets **.NET 8** (`net8.0`). The Angular client
is a separate application under `Frontend/`.


| Project | Type | Responsibility |
| --- | --- | --- |
| Framework.Core | Shared class library | Base entities (`FullAuditedEntityBase`, `LookupEntityBase`), `BaseDbContext`, `RepositoryBase` / `UnitOfWorkBase`, `ApiResponse`, caching, notifications, globalization, background jobs, convention-based DI. Shared framework — not forked per project. |
| Framework.Identity | Shared class library | ASP.NET Identity: users, roles, claims, tokens, `AppIdentityDbContext`. The system has **no** local `User` or `Role` entity. |
| IIROSA.Domain | Class library | Business entities and lookups, entity configurations, `MappingDefaults`, domain contracts and per-aggregate repository interfaces. |
| IIROSA.Application | Class library | Service interfaces and implementations, DTOs, AutoMapper profiles, FluentValidation validators, enums, custom exceptions. All business rules live here. |
| IIROSA.Infrastructure | Class library | `ApplicationDbContext`, repository implementations, EF Core migrations, save-changes interceptors, infrastructure services, DI extensions. |
| IIROSA.Api | ASP.NET Core Web API | Controllers, middleware, SignalR hubs, attachment endpoints. API-only — the UI is the separate Angular application. |
| Frontend | Angular 18 SPA | The entire user interface, RTL-first, Arabic primary. |
| tests | Test projects | Unit tests for services, integration tests for controllers. |

```
Backend/
├── Framework/
│   ├── Framework.Core/          Data/ (EntityBase, BaseDbContext, Repositories/, Uow/, Mapping/, Paging/)
│   │                            ApiResponse.cs · AutoMapper/ · Caching/ · Notifications/
│   │                            Globalization/ · BackgroundJobs/ · SharedServices/ · DependencyManagement/
│   └── Framework.Identity/      Data/ (AppIdentityDbContext, services, DTOs) · Migrations/
├── src/
│   ├── IIROSA.Domain/           Entities/ (Base/, Lookups/, module folders) · Configurations/
│   │                            Contracts/ · Interfaces/
│   ├── IIROSA.Application/      Interfaces/ · Services/ · DTOs/<Entity>/ · Profiles/ · Validators/ · Enums/
│   ├── IIROSA.Infrastructure/   Data/ (ApplicationDbContext.cs, Interceptors/, Repository/, Migrations/)
│   │                            Services/ · Extensions/
│   └── IIROSA.Api/              Controllers/ · Middleware/ · Hubs/ · Attachments/ · Properties/
└── tests/
```

> **Naming note.** The approved documents call the presentation project `IIROSA.Web`. The codebase
> names it **`IIROSA.Api`**, and `IIROSA.Api` is the canonical name. The empty `src/Presentation/`
> folder is a leftover of the old naming.

### 2.3 Layer responsibilities


| Layer | Purpose | May depend on | Must not contain |
| --- | --- | --- | --- |
| IIROSA.Domain | Core business entities and domain contracts | Framework.Core only | EF Core query code, DTOs, service logic |
| IIROSA.Application | Business logic and orchestration | Domain, Framework.Core | Data-access code, `DbContext`, HTTP concerns |
| IIROSA.Infrastructure | Data access and external services | Domain, Application, Framework.* | Business rules |
| IIROSA.Api | HTTP surface — controllers, middleware, hubs | All layers | Business rules; controllers stay thin and delegate to services |

The rules that follow from this, applied in every module:


| Rule | Detail |
| --- | --- |
| Entity base class | `FullAuditedEntity` (Guid key) for business entities; `LookupEntity` (int key) for lookups. Audit fields (`CreatedOn`, `CreatedBy`, `UpdatedOn`, `UpdatedBy`, `DeletedOn`, `DeletedBy`, `IsDeleted`) are inherited and are never re-declared. There is no `IAuditable` interface. |
| Database schemas | Two only, declared as constants in `MappingDefaults`: `IIROSA_SCHEMA` for audited business entities and `LOOKUP_SCHEMA` for lookups. Never a hard-coded string. |
| DbContext | `ApplicationDbContext : BaseDbContext<ApplicationDbContext>` discovers entities from the `IIROSA.Domain` assembly. **No manual `DbSet<T>` properties.** |
| Persistence | Only the Unit of Work calls `SaveChanges`. Repositories never do. Multi-step writes share one UoW transaction. |
| Deletes | Soft delete via `IsDeleted`; deleted rows are always filtered out of reads. No hard `DELETE`. |
| Validation | FluentValidation, invoked in the **service layer** — not left to the controller. |
| Mapping | AutoMapper profiles in `IIROSA.Application/Profiles/`. DTOs, never entities, cross the API boundary. |
| API response | Every endpoint returns `Framework.Core.ApiResponse` / `ApiResponse<T>` — `Success`, `Message`, `Value`, `ModelStateErrors`, `Confirm`. No per-controller response shapes. |
| Controller base | Every controller inherits `ApiController`, which supplies `[ApiController]`, JWT `[Authorize]`, `api/[controller]` routing and `CurrentUserId` / `CurrentUserName` / `CurrentUserEmail`. |
| Exceptions | `NotFoundException` → 404, `ValidationException` → 400, `BusinessException` → 400 / 409, translated by API middleware. |
| Bilingual text | Every entity carrying user-facing text has `NameAr` / `NameEn`. |
| Tenancy | Every query is scoped to the caller's charity and country, enforced **server-side**. |
| Caching | `ICacheService` only — never `IMemoryCache` directly. |

### 2.4 Frontend structure

```
Frontend/src/app/
├── core/          interceptors/ · guards/ (auth, permission, module) · services/ · models/
├── shared/        components/ · pipes/ · directives/ (hasPermission) · validators/
├── layouts/       main-layout/ (sidebar, header, footer) · auth-layout/
├── modules/       one lazy-loaded folder per feature — components/ services/ models/
├── store/         actions/ reducers/ effects/ selectors/ state.ts
├── assets/        i18n/ (ar.json, en.json) · css/ (TinyDash) · js/ · images/
└── environments/  environment.ts · environment.prod.ts
```

Three shared components are **mandatory** — every module reuses them instead of re-implementing the
same UI:


| Component | Responsibility |
| --- | --- |
| shared/components/data-list | All listing screens: paging, sorting, filtering, row actions, export |
| shared/components/input-fields | Dynamic, metadata-driven form field rendering with validation and lookups |
| shared/components/attachment | All file upload / download / preview against the Framework attachment API |

Supporting shared components: `modal`, `toast`, `loading`, `confirm-dialog`, `page-header`,
`empty-state`, `chart`. Feature modules are lazy-loaded, use `OnPush` change detection and `trackBy`
on every `*ngFor`, and keep the 4-file component shape (`.ts`, `.html`, `.scss`, `.spec.ts`).
Permission-driven UI uses the `hasPermission` directive and `permission.guard` — **client-side hiding
is never an authorisation control**; the endpoint must authorise as well.

### 2.5 Request lifecycle


| Step | What happens |
| --- | --- |
| 1 | The Angular router resolves the route; `auth.guard` and `permission.guard` decide whether the lazy-loaded feature module may load. |
| 2 | The component calls a typed feature service, which calls `HttpClient`. The `auth` interceptor attaches the JWT bearer token, the `locale` interceptor the language header, and the `loading` interceptor drives the global spinner. |
| 3 | `IIROSA.Api` authenticates the JWT and authorises the endpoint. `ApiController` exposes the caller's identity as `CurrentUserId` / `CurrentUserName` / `CurrentUserEmail`. |
| 4 | The controller binds a **typed request DTO** and delegates to an application service. No business logic, no EF Core, no `DbContext` in the controller. |
| 5 | The service runs its FluentValidation validator, applies the business rules and the charity / country tenancy scope, and works through per-aggregate repositories. |
| 6 | The Unit of Work commits once. `AuditLogSaveChangesInterceptor` stamps the audit fields; soft-deleted rows are filtered out of reads. |
| 7 | The service maps entities to DTOs with AutoMapper and the controller returns `ApiResponse<T>`. A thrown `NotFoundException` / `ValidationException` / `BusinessException` is translated to the right status code by the exception middleware. |
| 8 | The `error` interceptor surfaces `Message` and `ModelStateErrors` to the user through SweetAlert2 / toasts. |
| 9 | Server-pushed events (notifications, dashboard counters) arrive over SignalR rather than by polling. |
| 10 | Printed and exported output is produced by the client — ExcelJS for Excel, jsPDF for PDF — or by a server report endpoint. There is no Crystal Reports dependency. |

### 2.6 Technology stack


| Concern | Technology |
| --- | --- |
| Runtime / language | .NET 8 (`net8.0`), C# |
| Web framework | ASP.NET Core Web API |
| ORM | Entity Framework Core, code-first with migrations; entity auto-discovery through `BaseDbContext` |
| Database | Microsoft SQL Server — schemas `IIROSA` and `Lookup` |
| Data access pattern | Repository + Unit of Work from `Framework.Core.Data` |
| Object mapping | AutoMapper (`IIROSA.Application/Profiles/`) |
| Validation | FluentValidation (`IIROSA.Application/Validators/`), invoked in the service layer |
| Authentication | JWT bearer tokens issued by `Framework.Identity`, with refresh-token rotation |
| Authorisation | Role and permission claims enforced server-side on every endpoint |
| Real-time | SignalR hubs in `IIROSA.Api/Hubs/`; `@microsoft/signalr` on the client |
| Front end | Angular 18, TypeScript 5.x, Bootstrap 5.3 + TinyDash dark RTL, Angular Material, ng-bootstrap, ngx-bootstrap, ng-select |
| State management | NgRx (`src/app/store/`) for cross-cutting state; Angular Signals for local component state |
| Forms | Reactive Forms |
| Charts | ApexCharts |
| Spreadsheets / PDF | ExcelJS (Excel), jsPDF (PDF) |
| Alerts | SweetAlert2 |
| Localisation | `@ngx-translate/core` with `assets/i18n/ar.json` and `en.json`; `Framework.Core.Globalization` on the server. Arabic is the primary language and the application is RTL-first. Hijri and Gregorian date entry are both supported. |
| API documentation | Swagger / OpenAPI |
| Caching | `ICacheService` (`Framework.Core.Caching`) |

### 2.7 Legacy baseline — WAR.IIROSA as built (functional reference only)

Everything in this section describes the **superseded** system, and is kept only so that the shape of
what is being replaced stays on record.

It is **not** a routing reference. The routing of this project is stated in §2.4 (client) and §2.3
(API), indexed in full in Appendix A (Angular routes), Appendix B (API controllers) and Appendix C
(report and export endpoints), and mapped use case by use case in `00-ROUTING-MAP.md`. Every
`Realisation` entry, every §<n>.A and §<n>.B annex and every screen block in §<n>.S names this
project's routes, controllers and services — the legacy AngularJS states, `/api/*` controllers,
`/Print/*` actions, BLL classes and `Ngo`-prefixed identifiers have all been replaced.

#### 2.7.1 Legacy solution structure

The solution WAR.IIROSA.sln contains four projects targeting .NET Framework 4.8, arranged as a classic three-tier architecture.


| Project | Type | Responsibility |
| --- | --- | --- |
| WAR.IIROSA.PresentationAndServices | ASP.NET MVC 5 + Web API 2 web application | Hosts the AngularJS SPA, the 66 Web API service controllers under /api/*, the MVC controllers used for Crystal Reports printing, authentication cookie handling and file upload/download. |
| WAR.IIROSA.BLL | Class library | Business logic layer — 20 static BLL classes holding all business rules, authorisation checks, report composition and the Crystal Reports .rpt definitions. |
| WAR.IIROSA.MODEL | Class library | Entity Framework 6 data model (151 entity/result classes), generated repositories and Unit of Work, 55 API contracts (DTOs) and 48 response objects. |
| WAR.IIROSA.TESTING | Console/test project | Developer harness project. |

#### 2.7.2 Legacy layer responsibilities


| Layer | Contents | Key artefacts |
| --- | --- | --- |
| Client (SPA) | AngularJS 1.x application with ui.router and ngCookies. Around 80 HTML views under /Components, grouped by feature folder, each with its own controller and, in some features, factories. Navigation and role-based menu visibility are defined in index.html. | Content/Scripts/app.js (state map + API base-URL constants), index.html (shell and menu), Components/** |
| Service (Web API) | 66 ApiController classes exposing REST-style endpoints under /api/{controller}. They are thin: they deserialise the request (frequently as a loosely typed JObject), delegate to the BLL, and return a typed response object carrying Status, Message, InternalMessage and the payload. | ApiService/*.cs, App_Start/WebApiConfig.cs |
| Printing (MVC) | PrintController renders Crystal Reports to PDF/stream for cheques, orphan report forms, family cards, tracking sheets and receipt cards. AccountController, HomeController, FamilyMVCController serve the shell views. | Controllers/PrintController.cs, Views/Print/*.cshtml |
| Business logic | Static classes such as BLL, FamilyBLL, ChildBLL, OrphanReportBLL, OrphanPaymentBLL, ProjectBLL, ChequeBLL, TransfersBLL, NGOsBLL, ReportsBLL. Each operation opens a UnitOfWork, applies role and tenancy rules, mutates entities and saves. All operations are wrapped in try/catch with log4net error logging. | WAR.IIROSA.BLL/*.cs, Helper/, Enum/ |
| Data access | Entity Framework 6 against SQL Server (connection IIROSAEntities), with generated repositories per entity and a UnitOfWork aggregate. Roughly 160 stored procedures are imported as function imports and surface as SP_*_Result types; most reporting is delegated to these procedures rather than LINQ. | WAR.IIROSA.MODEL/Repositories, Repos, SP_*_Result.cs |
| Membership | ASP.NET Membership (aspnet_Users, aspnet_Membership, aspnet_Roles) with Forms Authentication tickets stored in the Cookie1 cookie. Role identity is resolved from GUIDs configured in Web.config. | Web.config <appSettings>, BLL.Login |

#### 2.7.3 Legacy request lifecycle


| Step | What happens |
| --- | --- |
| 1 | The browser loads index.html; ui.router resolves the requested state and loads the corresponding view and controller from /Components. |
| 2 | The Angular controller reads the current user id and role from cookies and calls one or more /api/* endpoints, passing userId (and usually ngoId) as query-string parameters. |
| 3 | The Web API controller delegates to the matching BLL method. |
| 4 | The BLL resolves the caller's role via GetUserRole(), applies tenancy filtering (a charity only ever sees its own data), executes the operation through the Unit of Work and commits. |
| 5 | A response object is returned with a Status of Success or Fail; on failure the exception is logged through log4net and a generic message is returned to the caller. |
| 6 | For printed output the client navigates to a /Print/* MVC action, which binds a Crystal Reports .rpt definition to a dataset and streams a PDF back to the browser. |

#### 2.7.4 Legacy technology stack


| Concern | Technology |
| --- | --- |
| Runtime / language | .NET Framework 4.8, C# |
| Web framework | ASP.NET MVC 5.2.2, ASP.NET Web API 2 (5.2.2) |
| Front end | AngularJS with ui-router, jQuery 1.10, Bootstrap 3, DataTables, Hijri date-picker, Limitless admin theme |
| ORM | Entity Framework 6.2 (database-first), generated repositories + Unit of Work |
| Database | Microsoft SQL Server — ~160 stored procedures drive reporting |
| Reporting / printing | SAP Crystal Reports 13 (44 .rpt definitions), Microsoft Report Viewer 11 |
| Spreadsheets | EPPlus 5.4.2, Microsoft Excel Interop |
| Serialisation | Newtonsoft.Json (JObject-based request binding on many endpoints) |
| Logging | log4net 2.0.8 (log4net.config) |
| API documentation | Swashbuckle 5.6 (Swagger) |
| Auth | ASP.NET Membership + Forms Authentication (15-day ticket) |
| Localisation | Arabic (RTL) user interface; Hijri and Gregorian calendars |

#### 2.7.5 Legacy defects — specified as non-goals

These were observed in the legacy system and are recorded so they are not carried forward. Each is a
non-goal of the re-platform (see also `_bmad-output/planning-artifacts/prd.md` §7).

An AuthenticationFilter (AuthorizationFilterAttribute) exists under ApiService/ but is not applied to any controller; API authorisation is instead performed inside individual BLL methods by inspecting the userId supplied by the caller as a query parameter.

Role-based menu hiding on the client (adminRule, NotTransferRole, staff CSS classes) is a presentation concern only and is not by itself an access-control mechanism.

Many write endpoints accept an untyped JObject, so the request contract is defined by the BLL parsing code rather than by a DTO.

Several capabilities exist in duplicate "V2/V3/Two" variants (e.g. ChequeTwoController, FamilyTwoController, OrphanPaymentTwoController, PrintNewOrphanReportV2/V3), reflecting incremental evolution rather than distinct business functions.

A Docs/Security-Assessment-Remediation-Report.docx already exists in the repository and should be read alongside this document.

## 3. Business Context

### 3.1 The sponsorship lifecycle

The system's central process chain is the following. Every functional module in chapters 5–23 supports one or more of these stages.


| # | Stage | What the system does |
| --- | --- | --- |
| 1 | Charity onboarding | HQ registers a partner charity, creates its login, assigns it to a country/region/centre, and controls whether it may add or edit data. |
| 2 | Family registration | The charity opens a family file capturing housing, income, guardian and children data, validating national IDs against duplicates across the country. |
| 3 | Orphan coding | HQ (or the charity under supervision) assigns each eligible orphan a unique sponsorship code, which admits the orphan to the sponsorship programme. |
| 4 | Periodic reporting | The charity submits a status report per coded orphan with photographs and supporting documents. HQ reviews it and either accepts it or refuses it with recorded reasons. |
| 5 | Payment batch preparation | HQ creates a payment batch; eligible orphans are enrolled as payment rows. Orphans who are excluded, uncoded, or lacking an accepted report can be stopped. |
| 6 | Disbursement | A bank transfer file is generated, or cheques are issued and printed. Transfer numbers and exchange statuses are re-imported from the bank via CSV. |
| 7 | Receipt confirmation | The charity confirms per orphan that the money was received, records the cheque number, date and the beneficiary who collected it, and prints receipt cards. |
| 8 | Follow-up & audit | HQ monitors charity performance through tracking reports: missed payments, orphans lacking reports or files, reports awaiting approval, refused reports, guardian-change requests and exclusions. |
| 9 | Complementary programmes | Seasonal assistance projects, housing project, office development projects, missions and technical support run alongside the sponsorship cycle against the same family register. |

### 3.2 Tenancy and data ownership


| Rule | Effect |
| --- | --- |
| Charity scoping | Families, orphans, reports and payment rows all carry an owning charity (`CharityId`). A charity user's queries are constrained to its own records by the application service, from the charity claim in the JWT. An HQ role may pass an explicit `charityId` filter, and the service checks the role permits it. |
| Country scoping | Charities and employees belong to a country; regions and centres cascade from the country. National-ID uniqueness checks and maximum transfer amounts are evaluated per country. |
| File transfer | A family file can be reassigned to another charity, carrying its guardians and orphans with it. |
| Add/edit locking | HQ can independently switch off a charity's ability to add records and to edit records, and can lock the account out of the system entirely. |

## 4. Actors and Roles

### 4.1 Role definitions

Roles are ASP.NET Identity roles owned by `Framework.Identity`. The role travels as a claim in the JWT and is resolved by `IAuthService`; there are no role GUIDs in configuration and no numeric role code on the wire. The **Code** column is kept only so that older references to role 0-4 still resolve.


| Code | Role | Role name | Scope and typical responsibilities |
| --- | --- | --- | --- |
| 0 | General Director المدير العام | `GeneralDirector` | Full HQ administrator. Sees all charities and countries. Manages charities, employees, orphan coding, report approval, payment batches, projects, transfers and every report. Menu visibility follows from its permission claims through the `hasPermission` directive — which is presentation only, never the control. |
| 1 | Charity user مستخدم الجمعية | `Charity` | The partner charity's operational account. Registers families and orphans, submits periodic reports, confirms payment receipt, selects families for projects and prints its own documents. Restricted to its own data and subject to add/edit locks. |
| 2 | Guest زائر | `Guest` | Read-only account used for supervisory viewing. |
| 3 | Staff موظف | `Staff` | HQ back-office employee. Shares most administrative capabilities with the General Director, and additionally owns the correspondence register (incoming/outgoing). |
| 4 | Financial Director المدير المالي | `FinancialDirector` | Owns the financial pillar: HQ transfers, maximum transfer amounts per country, cheques, bank files and disbursement tracking. A transfer-only account sees the transfer features and nothing else, enforced by `PermissionGuard` on the client and by the endpoint on the server. |

Authorisation is enforced on the endpoint, from the role and permission claims in the JWT — never from a role code supplied by the caller. Application services apply the same claims when they scope a query to the caller's charity and country.

### 4.2 Secondary and system actors


| Actor | Interaction with the system |
| --- | --- |
| Bank | Receives the generated transfer file; returns transfer numbers and exchange (execution) statuses which are imported back through CSV upload. Also the source of cheque layouts and printing positions. |
| Sponsor / donor | Not a system user; represented in data as the sponsorship attached to an orphan. Reports identify orphans whose sponsorship ended, who are unsponsored, or who are sponsored by another body. |
| Guardian / beneficiary | Not a system user; signs receipt cards and is recorded as the collecting beneficiary on payment rows and cheques. |
| Report & export service | Renders the printed documents (orphan report forms, cheques, family cards, tracking sheets) through `api/Reports`, with jsPDF and ExcelJS on the client. |
| E-mail service | Delivers the password-reset link generated by the forgotten-password use case. |
| SQL Server stored procedures | Execute the heavy reporting queries; the application acts as a presentation shell over roughly 160 procedures. |

### 4.3 Role / module access matrix

Derived from BLL role checks and the menu visibility rules in index.html. F = full, O = own data only, R = read-only, – = no access.


| Module | Gen. Director | Staff | Fin. Director | Charity | Guest |
| --- | --- | --- | --- | --- | --- |
| Families / orphans register | F | F | – | O | R |
| Housing project | F | F | – | O | R |
| Orphan coding | F | F | – | – | – |
| Periodic reports (entry) | F | F | – | O | R |
| Periodic reports (approve / refuse) | F | F | – | – | – |
| Orphan payment batches | F | F | F | O (receipt only) | – |
| Bank files & CSV reconciliation | F | F | F | – | – |
| General cheques | F | F | F | – | – |
| HQ financial transfers | F | – | F | – | – |
| Seasonal assistance projects | F | F | – | O | R |
| Office development projects | F | F | – | – | – |
| Missions | F | F | – | – | – |
| Technical support tickets | F | F | – | – | – |
| Correspondence (incoming/outgoing) | F | F | – | – | – |
| Charity administration & locking | F | – | – | – | – |
| Employee administration | F | – | – | – | – |
| Operational reports | F | F | R | O | R |
| Own account (password change) | F | F | F | F | F |

## 5. Functional Module Map

The system decomposes into 19 functional modules. Each is documented in its own chapter with a complete use-case catalogue.


| Ch. | Prefix | Module | Use cases | Principal routes |
| --- | --- | --- | --- | --- |
| 6 | UC-AUT | Authentication & user account (الحساب) | 9 | `#/auth/login`, `#/auth/register`, `#/auth/forgot-password`, `#/auth/reset-password/:userId/:code`, `#/auth/change-password` |
| 7 | UC-DSH | Home dashboard (الرئيسية) | 3 | `#/dashboard` |
| 8 | UC-CHR | Charity administration (الجمعيات) | 9 | `#/charities`, `#/charities/create` |
| 9 | UC-EMP | Employee & user administration (الموظفين) | 6 | `#/employees`, `#/employees/:id` |
| 10 | UC-FAM | Family register (الأسر) | 14 | `#/families`, `#/families/:id/edit`, `#/families/:id/members`, `#/families/provider-requests` |
| 11 | UC-HOU | Housing project (مشروع الإسكان) | 8 | `#/housing-projects`, `#/housing-projects/:id/edit`, `#/housing-projects/:id/reports`, `#/housing-projects/:id/reports/:reportId` |
| 12 | UC-REF | Refugee families (الأسر اللاجئة) | 4 | `#/families/refugees`, `#/families/refugees/:id/edit` |
| 13 | UC-ORP | Orphan register & coding (الأيتام والتكويد) | 11 | `#/families/orphans/coding/worklist`, `#/families/orphans/coding` |
| 14 | UC-ORR | Orphan periodic reports (التقارير الدورية) | 17 | `#/periodic-orphan-reports`, `#/periodic-orphan-reports/:id/edit`, `#/periodic-orphan-reports/orphan-reports/search`, `#/periodic-orphan-reports/orphan-reports` |
| 15 | UC-PAY | Orphan payments & disbursement (دفعات الأيتام) | 24 | `#/orphan-payments`, `#/orphan-payments/:id/edit`, `#/orphan-payments/:id/cheques`, `#/orphan-payments/:id/bank-file` |
| 16 | UC-CHQ | General cheques (الشيكات العامة) | 10 | `#/general-checks`, `#/general-checks/edit/:id`, `#/general-checks/statement` |
| 17 | UC-PRJ | Seasonal assistance projects (المساعدات الموسمية) | 13 | `#/seasonal-aid`, `#/seasonal-aid/:id/edit`, `#/seasonal-aid/:id/beneficiaries`, `#/seasonal-aid/:id/report` |
| 18 | UC-OFP | Office development projects (المشاريع التنموية) | 6 | `#/office-development-projects`, `#/office-development-projects/:id/edit` |
| 19 | UC-CST | Technical support (الدعم الفني) | 6 | `#/technical-support`, `#/technical-support/:id/edit` |
| 20 | UC-MSN | Missions (المأموريات) | 9 | `#/missions`, `#/missions/:id/edit`, `#/missions/:id/register` |
| 21 | UC-COR | Correspondence — incoming & outgoing (الصادر والوارد) | 19 | `#/incoming-outgoing/incoming`, `#/incoming-outgoing/incoming/:id/edit`, `#/incoming-outgoing/outgoing`, `#/incoming-outgoing/outgoing/:id/edit`, `#/incoming-outgoing/export/outgoing`, `#/incoming-outgoing/export/incoming` |
| 22 | UC-TRF | HQ financial transfers (الحوالات المالية) | 8 | `#/hq-transfers`, `#/hq-transfers/:id/edit`, `#/hq-transfers/max-amounts`, `#/hq-transfers/:id/details` |
| 23 | UC-RPT | Reports & printing (التقارير والطباعة) | 41 | Report views, Print controller actions |
| — | UC-SYS | Cross-cutting: files, lookups, validation | 13 | (embedded in all screens) |

Total documented use cases: 230.

## 26. Non-Functional Characteristics

The **Target** column is binding — it is what the approved architecture requires. The **Legacy
as-built** column records how the superseded system behaved, so the delta is explicit.


| Characteristic | Target (approved architecture) | Legacy as-built |
| --- | --- | --- |
| Language & direction | Arabic primary, RTL-first throughout; every user-facing string resolved through `@ngx-translate/core` (`ar.json` / `en.json`) — no hard-coded strings in templates. Every entity with user-facing text carries `NameAr` / `NameEn`. Hijri and Gregorian date entry are both supported. | Arabic (RTL) interface with strings embedded in the AngularJS views; Hijri and Gregorian calendars. |
| Multi-tenancy | Every beneficiary record carries an owning charity. Scoping by charity and country is applied in the application service layer and enforced **server-side**; a client-supplied tenant id is never trusted. HQ roles cross the boundary through an authorised claim, not a query-string parameter. | Every record carried an owning charity; queries were scoped by charity and by country. HQ roles passed an explicit charity id as a query-string parameter to cross the boundary. |
| Authentication | JWT bearer tokens issued by `Framework.Identity`, short-lived, with refresh-token rotation. No cookie-based Forms Authentication. | ASP.NET Membership with Forms Authentication; a 15-day encrypted ticket in the Cookie1 cookie. |
| Authorisation | Enforced on **every** endpoint by role and permission claims. `ApiController` applies `[Authorize]` by default. Menu hiding and the `hasPermission` directive are presentation only and never the control. | Enforced inside BLL methods against the internal role code, from a `userId` supplied by the caller. The prepared AuthenticationFilter was never applied to any controller. |
| Request contracts | Typed request DTOs on every write endpoint, validated with FluentValidation in the service layer. No untyped `JObject` binding. | Many write endpoints accepted an untyped `JObject`; the contract was defined by the BLL parsing code. |
| Response contract | One envelope for the whole API: `Framework.Core.ApiResponse` / `ApiResponse<T>` — `Success`, `Message`, `Value`, `ModelStateErrors`, `Confirm`. | Per-controller response objects carrying `Status`, `Message`, `InternalMessage` and the payload. |
| Paging | Paging, sorting and filtering are handled uniformly by the `data-list` shared component against paged list endpoints built on `Framework.Core.Data.Paging`. | List endpoints accepted a page number; several accepted a flag to disable paging for export. |
| Deletes & history | Soft delete only (`IsDeleted`); deleted rows are filtered out of every read. Audit fields are stamped automatically by `AuditLogSaveChangesInterceptor` — never by hand. | Mixed; audit columns were maintained by the BLL where they existed. |
| Error handling | Domain failures are raised as `NotFoundException` / `ValidationException` / `BusinessException` and translated to 404 / 400 / 409 by API middleware; the client `error` interceptor renders `Message` and `ModelStateErrors`. | Every BLL operation caught, logged through log4net and returned a Fail response with a generic message, keeping the internal message separate. |
| Logging | ASP.NET Core structured logging with request logging in `ApiController` and correlation through middleware. | log4net configured in log4net.config. |
| Reporting performance | Reporting is expressed against EF Core over the code-first model, with paged and projected queries; heavy aggregations may be pushed to the database deliberately, not by default. | Roughly 160 SQL stored procedures carried the reporting load; the application was largely a shell over them. |
| Printing & export | Client-side generation: ExcelJS for spreadsheets, jsPDF for PDF, plus server report endpoints where the layout demands it. **No Crystal Reports dependency.** | SAP Crystal Reports 13 (44 `.rpt` definitions) rendered to PDF and streamed to the browser; EPPlus and Excel Interop for spreadsheets. |
| Real-time | SignalR hubs push notifications and dashboard counters; the client subscribes with `@microsoft/signalr` instead of polling. | None — screens re-queried on navigation. |
| Interoperability | CSV import from the bank retained as a functional requirement; the API surface is documented with Swagger / OpenAPI. | CSV import from the bank in three formats; Swashbuckle 5.6 for Swagger. |
| Availability of reference data | Lookup lists are served through `ICacheService` and cached on the client, not re-fetched per screen. | Lookup lists were fetched per screen from the API and not cached. |
| Duplicate capabilities | One implementation per capability. The V2 / V3 / "Two" variants are not carried forward. | Several capabilities existed in duplicate V2 / V3 / "Two" variants. |
| Testability | Unit tests for every application service and integration tests for every controller, per the module checklist in §2 of `Architecture/02_QuickStartGuide.md`. | A developer harness project only. |
## Appendix A — Angular Route Index

Every route of the client, with the lazy-loaded feature module and the component that renders it.
The router runs in hash mode (`useHash: true`), so the browser URL reads `…/#/charities`. Everything
except `#/auth/**` renders inside `MainLayoutComponent` behind `AuthGuard`; `PermissionGuard` decides
per feature. `#/` redirects to `#/dashboard`, and an unknown path redirects there too.

**Status** — *implemented* means the route exists today under `Frontend/src/app`; *planned* means it
is specified here and not built yet.


| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/auth/login` | `auth` | `LoginComponent` | implemented |
| `#/auth/register` | `auth` | `RegisterComponent` | planned |
| `#/auth/forgot-password` | `auth` | `ForgotPasswordComponent` | planned |
| `#/auth/reset-password/:userId/:code` | `auth` | `ResetPasswordComponent` | planned |
| `#/auth/change-password` | `auth` | `ChangePasswordComponent` | planned |
| `#/dashboard` | `dashboard` | `DashboardComponent` | implemented |
| `#/charities` | `charities` | `CharityListComponent` | implemented |
| `#/charities/create` | `charities` | `CharityFormComponent` | implemented |
| `#/employees` | `employees` | `EmployeeListComponent` | implemented |
| `#/employees/:id` | `employees` | `EmployeeDetailComponent` | implemented |
| `#/user-management/users` | `user-management` | `UserListComponent` | implemented |
| `#/user-management/users/:id` | `user-management` | `UserDetailComponent` | implemented |
| `#/families` | `families` | `FamilyListComponent` | implemented |
| `#/families/:id/edit` | `families` | `FamilyFormComponent` | implemented |
| `#/families/:id/members` | `families` | `FamilyMembersComponent` | planned |
| `#/families/provider-requests` | `families` | `ProviderRequestListComponent` | planned |
| `#/families/refugees` | `families` | `RefugeeFamilyListComponent` | planned |
| `#/families/refugees/:id/edit` | `families` | `RefugeeFamilyFormComponent` | planned |
| `#/families/orphans/coding` | `families` | `OrphanCodingComponent` | planned |
| `#/families/orphans/coding/worklist` | `families` | `OrphanCodingWorklistComponent` | planned |
| `#/housing-projects` | `housing-projects` | `HousingProjectListComponent` | implemented |
| `#/housing-projects/:id/edit` | `housing-projects` | `HousingProjectFormComponent` | implemented |
| `#/housing-projects/:id/reports` | `housing-projects` | `HousingReportListComponent` | planned |
| `#/housing-projects/:id/reports/:reportId` | `housing-projects` | `HousingReportFormComponent` | planned |
| `#/periodic-orphan-reports` | `periodic-orphan-reports` | `PeriodicReportsListComponent` | implemented |
| `#/periodic-orphan-reports/:id/edit` | `periodic-orphan-reports` | `PeriodicReportFormComponent` | implemented |
| `#/periodic-orphan-reports/orphan-reports` | `periodic-orphan-reports` | `OrphanReportsListComponent` | implemented |
| `#/periodic-orphan-reports/orphan-reports/search` | `periodic-orphan-reports` | `OrphanReportSearchComponent` | implemented |
| `#/orphan-payments` | `orphan-payments` | `OrphanPaymentListComponent` | implemented |
| `#/orphan-payments/:id/edit` | `orphan-payments` | `OrphanPaymentFormComponent` | implemented |
| `#/orphan-payments/:id/cheques` | `orphan-payments` | `OrphanPaymentChequesComponent` | planned |
| `#/orphan-payments/:id/bank-file` | `orphan-payments` | `BankFileComponent` | planned |
| `#/general-checks` | `general-checks` | `CheckListComponent` | implemented |
| `#/general-checks/edit/:id` | `general-checks` | `CheckFormComponent` | implemented |
| `#/general-checks/statement` | `general-checks` | `CheckStatementComponent` | planned |
| `#/seasonal-aid` | `seasonal-aid` | `CampaignListComponent` | implemented |
| `#/seasonal-aid/:id/edit` | `seasonal-aid` | `CampaignFormComponent` | implemented |
| `#/seasonal-aid/:id/beneficiaries` | `seasonal-aid` | `BeneficiarySelectionComponent` | implemented |
| `#/seasonal-aid/:id/eligible-families` | `seasonal-aid` | `EligibleFamiliesComponent` | planned |
| `#/seasonal-aid/:id/report` | `seasonal-aid` | `CampaignReportComponent` | planned |
| `#/office-development-projects` | `office-development-projects` | `ProjectListComponent` | implemented |
| `#/office-development-projects/:id/edit` | `office-development-projects` | `ProjectFormComponent` | implemented |
| `#/technical-support` | `technical-support` | `TicketListComponent` | implemented |
| `#/technical-support/:id/edit` | `technical-support` | `TicketFormComponent` | implemented |
| `#/missions` | `missions` | `MissionListComponent` | implemented |
| `#/missions/:id/edit` | `missions` | `MissionFormComponent` | implemented |
| `#/missions/:id/register` | `missions` | `MissionRegisterComponent` | planned |
| `#/incoming-outgoing/incoming` | `incoming-outgoing` | `IncomingLettersListComponent` | implemented |
| `#/incoming-outgoing/incoming/:id/edit` | `incoming-outgoing` | `IncomingLetterFormComponent` | implemented |
| `#/incoming-outgoing/outgoing` | `incoming-outgoing` | `OutgoingLettersListComponent` | implemented |
| `#/incoming-outgoing/outgoing/:id/edit` | `incoming-outgoing` | `OutgoingLetterFormComponent` | implemented |
| `#/incoming-outgoing/export/incoming` | `incoming-outgoing` | `ExportWizardComponent` | implemented |
| `#/incoming-outgoing/export/outgoing` | `incoming-outgoing` | `ExportWizardComponent` | implemented |
| `#/incoming-outgoing/export/outgoing-orphans` | `incoming-outgoing` | `ExportWizardComponent` | planned |
| `#/hq-transfers` | `hq-transfers` | `HqTransferListComponent` | planned |
| `#/hq-transfers/:id/edit` | `hq-transfers` | `HqTransferFormComponent` | planned |
| `#/hq-transfers/:id/details` | `hq-transfers` | `HqTransferDetailComponent` | planned |
| `#/hq-transfers/max-amounts` | `hq-transfers` | `MaxTransferAmountComponent` | planned |
| `#/reports/orphans` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/excluded-orphans` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/finished-sponsorship-orphans` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/unsponsored-orphans` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/registered-family-projects` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/meza-cards` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/widows-allowing-sponsorship` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/family-orphans` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/missed-payments` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/charity-payment-tracking` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/beneficiary-family-details` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/orphans-missing-reports` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/orphans-missing-files` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/reports-awaiting-approval` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/refused-reports` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/orphan-files` | `reports` | `ReportViewerComponent` | planned |
| `#/reports/provider-sponsor-changes` | `reports` | `ReportViewerComponent` | planned |
| `#/error` | `core` | `ErrorPageComponent` | planned |

Every CRUD feature module exposes the same four child routes — `''` (list), `create`, `:id` (detail)
and `:id/edit`. They are listed once here rather than repeated per module; the table above names the
route each use case enters through.

## Appendix B — API Controller Index

The HTTP surface of `IIROSA.Api`. Every controller inherits `ApiController`, which applies
`[ApiController]`, `[Authorize]` (JWT bearer), the route template `api/[controller]` and request
logging, and exposes `CurrentUserId` / `CurrentUserName` / `CurrentUserEmail`. Every action returns
`Framework.Core.ApiResponse` or `ApiResponse<T>`.


| Controller | Base route | Application service | Owns | Status |
| --- | --- | --- | --- | --- |
| `AuthController` | `api/Auth` | `IAuthService` | Login, registration, refresh-token rotation, logout, token validation, password reset | implemented |
| `DashboardController` | `api/Dashboard` | `IDashboardService` | Home counters, charts and payment summary | planned |
| `CharitiesController` | `api/Charities` | `ICharityService` | Charity register, profile, bank and location detail, lock / unlock, add- and edit-rights | implemented |
| `UserManagementController` | `api/UserManagement` | `IUserService` | Application users, activation, roles, claims, activity | implemented |
| `RoleManagementController` | `api/RoleManagement` | `IRoleService` | Roles and their permission sets | implemented |
| `EmployeeManagementController` | `api/EmployeeManagement` | `IEmployeeService` | Employee register, activation, role assignment, export | implemented |
| `FamiliesController` | `api/Families` | `IFamilyService` | Family file, father / mother / relatives / orphans, provider verification, transfer to another charity, attachments | implemented |
| `HousingProjectsController` | `api/HousingProjects` | `IHousingProjectService` | Housing projects, budget, beneficiary, progress, completion, reports | implemented |
| `PeriodicOrphanReportsController` | `api/PeriodicOrphanReports` | `IPeriodicOrphanReportService` | Periodic report entry, review (accept / refuse), lock, per-orphan history | implemented |
| `OrphanReportsController` | `api/OrphanReports` | `IOrphanReportService` | Report generation, export, history, comparison, scheduling, statistics | implemented |
| `OrphanPaymentsController` | `api/OrphanPayments` | `IOrphanPaymentService` | Payment batches, exchange rate, orphan enrolment, batch number, bank-file export, CSV import | implemented |
| `CheckManagementController` | `api/CheckManagement` | `ICheckService` | Cheque register, amount and date, clear / void, reconciliation, statement | implemented |
| `SeasonalAidController` | `api/SeasonalAid` | `ISeasonalAidService` | Campaigns, eligible families, beneficiaries, distributions, campaign reports | implemented |
| `OfficeProjectManagementController` | `api/OfficeProjectManagement` | `IOfficeProjectService` | Office development projects, budget, donor, beneficiaries, completion, export | implemented |
| `SupportTicketsController` | `api/SupportTickets` | `ISupportTicketService` | Support tickets, responses, assignment, status, ticket report | implemented |
| `MissionManagementController` | `api/MissionManagement` | `IMissionService` | Missions, assignment, completion, event record, mission type lookups | implemented |
| `IncomingOutgoingController` | `api/IncomingOutgoing` | `IIncomingService` · `IOutgoingService` | Incoming and outgoing correspondence, serials, categories, child dispatches, import / export | implemented |
| `HqTransfersController` | `api/HqTransfers` | `IHqTransferService` | HQ financial transfers, transfer detail, per-country maximum transfer amount | planned |
| `ReportsController` | `api/Reports` | `IReportService` | The operational and statistical report catalogue, and its PDF / Excel export | planned |
| `LookupManagementController` | `api/LookupManagement` | `ILookupService` | Countries, regions, centres, departments, banks and every other lookup table | implemented |
| `AttachmentsController` | `api/Attachments` | `IAttachmentService` | Upload, download, preview and metadata for every attached file | implemented |
| `NotificationController` | `api/Notification` | `INotificationService` | Broadcast and per-user notifications, pushed over SignalR | implemented |
| `AuditLogsController` | `api/AuditLogs` | `IAuditLogService` | Entity history, user activity, comparison and restore preview | implemented |
| `ImpersonationController` | `api/Impersonation` | `IImpersonationService` | Support impersonation sessions and their audit trail | implemented |

## Appendix C — Report & Export Endpoint Index

Printed and exported output is produced from these endpoints. A report is requested with a filter
body (`POST`) and is returned either as data for `ReportViewerComponent` or as a file, through
`…/export/pdf` and `…/export/excel`. There is **no Crystal Reports dependency** — PDF comes from
jsPDF on the client or from the server report endpoint, Excel from ExcelJS. See §2.6 and chapter 26.


| Report key | Endpoint | Route | Printed document it produces |
| --- | --- | --- | --- |
| `orphans` | `POST /api/Reports/orphans` | `#/reports/orphans` | Orphan register listing |
| `excluded-orphans` | `POST /api/Reports/excluded-orphans` | `#/reports/excluded-orphans` | Excluded orphans |
| `finished-sponsorship-orphans` | `POST /api/Reports/finished-sponsorship-orphans` | `#/reports/finished-sponsorship-orphans` | Orphans whose sponsorship ended |
| `unsponsored-orphans` | `POST /api/Reports/unsponsored-orphans` | `#/reports/unsponsored-orphans` | Unsponsored orphans |
| `orphans-other-sponsor` | `POST /api/Reports/orphans-other-sponsor` | `#/reports/orphans` | Orphans sponsored by another body |
| `orphans-without-payment` | `POST /api/Reports/orphans-without-payment` | `#/reports/missed-payments` | Orphans receiving no amount |
| `missed-payments` | `POST /api/Reports/missed-payments` | `#/reports/missed-payments` | Missed payments |
| `payments-received` | `POST /api/Reports/payments-received/export/pdf` | `#/reports/missed-payments` | Received-payment sheet |
| `payments-not-received` | `POST /api/Reports/payments-not-received/export/pdf` | `#/reports/missed-payments` | Not-received sheet |
| `payments-stopped` | `POST /api/Reports/payments-stopped/export/pdf` | `#/reports/missed-payments` | Stopped-payment sheet |
| `charity-payment-tracking` | `POST /api/Reports/charity-payment-tracking` | `#/reports/charity-payment-tracking` | Charity payment follow-up |
| `family-update-tracking` | `POST /api/Reports/family-update-tracking/export/pdf` | `#/reports/beneficiary-family-details` | Family update tracking sheet |
| `beneficiary-family-details` | `POST /api/Reports/beneficiary-family-details` | `#/reports/beneficiary-family-details` | Beneficiary family details |
| `family-cards` | `POST /api/Reports/family-cards/export/pdf` | `#/reports/family-orphans` | Family card |
| `family-card-primary` | `POST /api/Reports/family-card-primary/export/pdf` | `#/reports/family-orphans` | Family card — primary |
| `family-card-secondary` | `POST /api/Reports/family-card-secondary/export/pdf` | `#/reports/family-orphans` | Family card — secondary |
| `family-orphans` | `POST /api/Reports/family-orphans` | `#/reports/family-orphans` | Orphans of a family |
| `guardian-identification-sheets` | `POST /api/Reports/guardian-identification-sheets/export/pdf` | `#/reports/beneficiary-family-details` | Guardian identification sheet |
| `widow-identification-sheets` | `POST /api/Reports/widow-identification-sheets/export/pdf` | `#/reports/widows-allowing-sponsorship` | Widow identification sheet |
| `widows-allowing-sponsorship` | `POST /api/Reports/widows-allowing-sponsorship` | `#/reports/widows-allowing-sponsorship` | Widows admitting widow sponsorship |
| `meza-cards` | `POST /api/Reports/meza-cards` | `#/reports/meza-cards` | Meza card report |
| `orphan-report-form` | `POST /api/Reports/orphan-report-form/export/pdf` | `#/periodic-orphan-reports/:id/edit` | The periodic orphan report form |
| `orphans-missing-reports` | `POST /api/Reports/orphans-missing-reports` | `#/reports/orphans-missing-reports` | Orphans lacking a report |
| `orphans-missing-files` | `POST /api/Reports/orphans-missing-files` | `#/reports/orphans-missing-files` | Orphans lacking a file |
| `reports-awaiting-approval` | `POST /api/Reports/reports-awaiting-approval` | `#/reports/reports-awaiting-approval` | Reports not yet accepted |
| `refused-reports` | `POST /api/Reports/refused-reports` | `#/reports/refused-reports` | Refused reports |
| `non-renewed-reports` | `POST /api/Reports/non-renewed-reports` | `#/reports/reports-awaiting-approval` | Non-renewed reports |
| `provider-sponsor-changes` | `POST /api/Reports/provider-sponsor-changes` | `#/reports/provider-sponsor-changes` | Guardian / sponsor change list |
| `registered-family-projects` | `POST /api/Reports/registered-family-projects` | `#/reports/registered-family-projects` | Families registered on a campaign |
| `orphan-files` | `POST /api/Reports/orphan-files/export` | `#/reports/orphan-files` | Bulk export of orphan images and files |
| `certificate-files` | `POST /api/Reports/certificate-files/export` | `#/reports/orphan-files` | Bulk export of certificates |
| `receipt-cards` | `POST /api/Reports/receipt-cards/export/pdf` | `#/orphan-payments/:id/edit` | Receipt card |
| `payment-cheques` | `POST /api/Reports/payment-cheques/export/pdf` | `#/orphan-payments/:id/cheques` | Payment cheques |
| `payment-cheques-eg` | `POST /api/Reports/payment-cheques-eg/export/pdf` | `#/orphan-payments/:id/cheques` | Payment cheques — Egypt layout |
| `general-cheque` | `POST /api/Reports/general-cheque/export/pdf` | `#/general-checks/:id` | General cheque |
| `general-cheque-eg` | `POST /api/Reports/general-cheque-eg/export/pdf` | `#/general-checks/:id` | General cheque — Egypt layout |
| `cheque-numbers` | `POST /api/Reports/cheque-numbers/export/pdf` | `#/general-checks` | Cheque number sheet |
| `cheque-statement` | `POST /api/Reports/cheque-statement/export/pdf` | `#/general-checks/statement` | Cheque statement |

## Appendix D — Application Service Index

Business rules live in `IIROSA.Application/Services/` — one injectable service per aggregate,
resolved by convention through `Framework.Core.DependencyManagement`. Services validate with
FluentValidation, apply the charity / country tenancy scope, and persist through `IUnitOfWork`, which
is the only component that calls `SaveChanges`.


| Service | Aggregate | Responsibility |
| --- | --- | --- |
| `IAuthService` | Identity | Sign-in, JWT issue and refresh, password reset, role resolution |
| `IUserService` · `IRoleService` | Identity | Users, roles, claims and permission sets |
| `IEmployeeService` | `Employee` | Employee register and role assignment |
| `ICharityService` | `Charity` | Charity register, profile, lock and rights flags |
| `IFamilyService` | `Family` | Family file, guardians, relatives, orphans, provider changes, transfer between charities |
| `IOrphanService` | `Orphan` | Orphan record and sponsorship coding |
| `IPeriodicOrphanReportService` | `PeriodicOrphanReport` | Report entry, review and locking |
| `IOrphanReportService` | reporting | Report generation, comparison, scheduling and statistics |
| `IOrphanPaymentService` | `OrphanPayment` | Batches, enrolment, exchange rate, bank file and reconciliation |
| `ICheckService` | `Check` | Cheque issue, clearing, voiding and reconciliation |
| `IHousingProjectService` | `HousingProject` | Housing projects and their beneficiaries |
| `ISeasonalAidService` | `Campaign` | Seasonal campaigns, beneficiary selection and distribution |
| `IOfficeProjectService` | `OfficeProject` | Office development projects |
| `IMissionService` | `Mission` | Missions and their outcomes |
| `ISupportTicketService` | `SupportTicket` | Technical support tickets |
| `IIncomingService` · `IOutgoingService` | `Incoming` · `Outgoing` | Correspondence register, serials and dispatches |
| `IHqTransferService` | `HqTransfer` | HQ financial transfers and per-country ceilings |
| `IReportService` | reporting | The operational report catalogue and its exports |
| `ILookupService` | lookups | Every `LookupEntity` table |
| `IAttachmentService` | attachments | File upload, download and preview |
| `INotificationService` | notifications | Notification dispatch over SignalR |
| `IAuditLogService` | audit | Entity history and user activity |

## Appendix E — Domain Enumerations


| Enumeration | Values |
| --- | --- |
| ChildOrParentEnum | Child (1), Parent (2) — discriminates housing reports. |
| GenderTypeEnum | Male (1), Female (2), Empty (3). |
| EducationalLevelEnum | Young, Kindergarten, Primary, Medium, High school, Technical institute, Undergraduate, Postgraduate, Left study, Has certificate. |
| ClassRoomEnum | 30 values spanning nursery, primary 1–6, preparatory 1–3, secondary 1–4, university 1–7, technical 1–5, diploma, master, doctorate. |
| EducationalStatusEnum | Teaching, Certificated, Left study. |
| HealthStatusEnum, DisabilityEnum | Health condition; disability type — movement, visual, hearing, mental. |
| PrayerStatusEnum, HadeethStatusEnum, MoralEnum | Religious observance, Hadith memorisation and behaviour scales recorded on the periodic report. |
| HobbiesEnum, ProgramsOfferedEnum | Recorded interests and the programmes the orphan was offered. |
| RelativeRelationEnum | Relationship of the guardian to the orphan. |
| FileTypeEnum | ProjectFile (1), ProjectReportFile (2). |
| SearchCriteria | None, FatherName, MotherName, OrphanName, SponserName, NationalId, Code, Phone — the family search dimensions. |
| CharityLockType | Charity, CharityEdit, CharityAdd — the three charity control switches, applied through `POST /api/Charities/{id}/lock` and `PUT /api/Charities/{id}/update-rights`. |

## Appendix F — Traceability Summary

> **Prefix change (v1.2).** Module 8 was renamed from "Association (NGO) Administration" to
> **Charity Administration** and its identifier prefix from `UC-NGO` to **`UC-CHR`** (user stories
> `US-NGO-nn` → `US-CHR-nn`). The numbering is unchanged, so the mapping is one-to-one:
> `UC-NGO-01` ≡ `UC-CHR-01` … `UC-NGO-09` ≡ `UC-CHR-09`. Any older reference resolves by substituting
> the prefix. No other prefix changed.


| Prefix | Module | Count | Chapter |
| --- | --- | --- | --- |
| UC-AUT | Authentication & user account | 9 | 6 |
| UC-DSH | Home dashboard | 3 | 7 |
| UC-CHR | Charity administration | 9 | 8 |
| UC-EMP | Employee administration | 6 | 9 |
| UC-FAM | Family register | 14 | 10 |
| UC-HOU | Housing project | 8 | 11 |
| UC-REF | Refugee families | 4 | 12 |
| UC-ORP | Orphan register & coding | 11 | 13 |
| UC-ORR | Orphan periodic reports | 17 | 14 |
| UC-PAY | Orphan payments | 24 | 15 |
| UC-CHQ | General cheques | 10 | 16 |
| UC-PRJ | Seasonal assistance projects | 13 | 17 |
| UC-OFP | Office development projects | 6 | 18 |
| UC-CST | Technical support | 6 | 19 |
| UC-MSN | Missions | 9 | 20 |
| UC-COR | Correspondence | 19 | 21 |
| UC-TRF | HQ financial transfers | 8 | 22 |
| UC-RPT | Reports & printing | 41 | 23 |
| UC-SYS | Cross-cutting services | 13 | 24 |
| Total |  | 230 |  |

## Appendix G — Document Provenance


| Item | Detail |
| --- | --- |
| Method | Full static analysis of the repository: SPA route map and views, 66 Web API controllers, the MVC print controller, 20 BLL classes, the Entity Framework model, Web.config, and the Crystal Reports catalogue. |
| Primary sources | Content/Scripts/app.js, index.html, ApiService/*.cs, Controllers/PrintController.cs, WAR.IIROSA.BLL/*.cs, WAR.IIROSA.MODEL/**, WAR.IIROSA.BLL/Reports/*.rpt. |
| Scale observed | 4 projects; 66 API controllers; ~80 SPA views; 20 BLL classes; 151 entity/result classes; 55 contracts; 48 response objects; ~160 stored procedures; 44 Crystal report definitions. |
| Known limitations | Behaviour that lives only inside stored procedures is described by its business purpose rather than its internal logic. Screens whose menu entries are commented out (refugee families, HQ transfers menu group, incoming employee report) are documented because the routes and endpoints remain live. Amounts, entitlement formulas and country-specific rules are held as data and are not reproduced here. |
| Maintenance | When a use case is added or changed, update the relevant chapter, the count in Appendix F and the module map in chapter 5, keeping the UC identifiers stable. |
| Revision 1.2 (18 August 2026) | Two changes. (1) **Architecture approved and applied** — chapter 2 and chapter 26 now state the approved re-platform architecture (.NET 8 Clean Architecture + Angular 18) from `Architecture/01`-`04`, consolidated in `_bmad-output/planning-artifacts/architecture.md`; the legacy as-built description moved to §2.7 and appendices A-D are marked *legacy reference*. (2) **Terminology** — the tenant organisation is now called a **Charity** throughout; "NGO" and "Association" are retired and module 8 became `UC-CHR`. |
| Revision 1.3 (18 August 2026) | **Routing re-platformed.** Every `Realisation` entry, every §<n>.A / §<n>.B annex, every screen block in §<n>.S and every scenario flow now names this project's Angular 18 route and ASP.NET Core endpoint. The legacy AngularJS states, the 66 `/api/*` controllers, the `/Print/*` Crystal Reports actions, the static BLL classes, Forms Authentication and the `Ngo`-prefixed identifiers have all been replaced. Appendices A-D were rewritten as the route, controller, report-endpoint and service indexes of this project. New companion document: `00-ROUTING-MAP.md`. |
| Authoritative documents | Architecture: `_bmad-output/planning-artifacts/architecture.md`. Scope and backlog: `_bmad-output/planning-artifacts/epics.md` and `_bmad-output/implementation-artifacts/sprint-status.yaml`. The `.docx` exports in `docs/Modules/` predate revision 1.2 — **the `.md` files are authoritative**. |

