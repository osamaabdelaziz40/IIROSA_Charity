# IIROSA Charities - Module Documentation Set

The consolidated project documentation is split into one document per functional module.
Each module document carries: the chapter verbatim from the master document, the screen field
specifications, one expanded scenario per use case, and annexes listing the legacy screens and Web
API controllers of that module.

This set defines **what** the system must do. **How** it is built is fixed by the approved
architecture — read `00-Overview-and-Common-Context` §2 first, and
`_bmad-output/planning-artifacts/architecture.md` for the authoritative version.

> **Terminology.** The tenant organisation is a **Charity** (الجمعية). "NGO" and "Association" are
> retired from this set; module 8 changed prefix from `UC-NGO` to `UC-CHR` with numbering unchanged
> (`UC-NGO-01` ≡ `UC-CHR-01`).

> **Routing.** Every route and endpoint in this set is this project's — the Angular 18 route and the
> ASP.NET Core endpoint. The legacy AngularJS states, `/api/*` controllers, `/Print/*` actions and BLL
> classes have been replaced throughout; see [00-ROUTING-MAP](00-ROUTING-MAP.md).

> **File formats.** The `.md` files are authoritative and current. The `.docx` exports in this folder
> predate revision 1.2 (the architecture update and the Charity rename) and are stale.


| Field | Value |
| --- | --- |
| Document title | IIROSA Charities - Module Documentation Set (Index) |
| Documents in set | 23 |
| Total use cases | 230 |
| Screen field specifications | 661 fields across 19 chapters |
| Use case scenarios | 230 |
| Version | 1.3 |
| Date | 18 August 2026 |
| Target architecture | .NET 8 Clean Architecture + Angular 18 — approved in `Architecture/01`-`04`, consolidated in `_bmad-output/planning-artifacts/architecture.md` (authoritative) |
| Delivery backlog | `_bmad-output/planning-artifacts/epics.md` · board: `_bmad-output/implementation-artifacts/sprint-status.yaml` |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Documents

| # | Document | Ch. | Prefix | Module | Use cases |
| --- | --- | --- | --- | --- | --- |
| — | [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md) | 1-5, 26, App. A-G | — | Overview, architecture, actors, appendices | — |
| — | [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md) | — | — | Epic & user story backlog for all modules | 230 |
| — | [00-ROUTING-MAP](00-ROUTING-MAP.md) | — | — | Angular 18 route and API endpoint behind every use case | — |
| 1 | [06-UC-AUT-Authentication-and-User-Account](06-UC-AUT-Authentication-and-User-Account.md) | 6 | `UC-AUT` | Authentication & User Account | 9 |
| 2 | [07-UC-DSH-Home-Dashboard](07-UC-DSH-Home-Dashboard.md) | 7 | `UC-DSH` | Home Dashboard | 3 |
| 3 | [08-UC-CHR-Charity-Administration](08-UC-CHR-Charity-Administration.md) | 8 | `UC-CHR` | Charity Administration | 9 |
| 4 | [09-UC-EMP-Employee-and-User-Administration](09-UC-EMP-Employee-and-User-Administration.md) | 9 | `UC-EMP` | Employee & User Administration | 6 |
| 5 | [10-UC-FAM-Family-Register](10-UC-FAM-Family-Register.md) | 10 | `UC-FAM` | Family Register | 14 |
| 6 | [11-UC-HOU-Housing-Project](11-UC-HOU-Housing-Project.md) | 11 | `UC-HOU` | Housing Project | 8 |
| 7 | [12-UC-REF-Refugee-Families](12-UC-REF-Refugee-Families.md) | 12 | `UC-REF` | Refugee Families | 4 |
| 8 | [13-UC-ORP-Orphan-Register-and-Coding](13-UC-ORP-Orphan-Register-and-Coding.md) | 13 | `UC-ORP` | Orphan Register & Coding | 11 |
| 9 | [14-UC-ORR-Orphan-Periodic-Reports](14-UC-ORR-Orphan-Periodic-Reports.md) | 14 | `UC-ORR` | Orphan Periodic Reports | 17 |
| 10 | [15-UC-PAY-Orphan-Payments-and-Disbursement](15-UC-PAY-Orphan-Payments-and-Disbursement.md) | 15 | `UC-PAY` | Orphan Payments & Disbursement | 24 |
| 11 | [16-UC-CHQ-General-Cheques](16-UC-CHQ-General-Cheques.md) | 16 | `UC-CHQ` | General Cheques | 10 |
| 12 | [17-UC-PRJ-Seasonal-Assistance-Projects](17-UC-PRJ-Seasonal-Assistance-Projects.md) | 17 | `UC-PRJ` | Seasonal Assistance Projects | 13 |
| 13 | [18-UC-OFP-Office-Development-Projects](18-UC-OFP-Office-Development-Projects.md) | 18 | `UC-OFP` | Office Development Projects | 6 |
| 14 | [19-UC-CST-Technical-Support](19-UC-CST-Technical-Support.md) | 19 | `UC-CST` | Technical Support | 6 |
| 15 | [20-UC-MSN-Missions](20-UC-MSN-Missions.md) | 20 | `UC-MSN` | Missions | 9 |
| 16 | [21-UC-COR-Correspondence-Incoming-and-Outgoing](21-UC-COR-Correspondence-Incoming-and-Outgoing.md) | 21 | `UC-COR` | Correspondence — Incoming & Outgoing | 19 |
| 17 | [22-UC-TRF-HQ-Financial-Transfers](22-UC-TRF-HQ-Financial-Transfers.md) | 22 | `UC-TRF` | HQ Financial Transfers | 8 |
| 18 | [23-UC-RPT-Reports-and-Printing](23-UC-RPT-Reports-and-Printing.md) | 23 | `UC-RPT` | Reports & Printing | 41 |
| 19 | [24-UC-SYS-Cross-Cutting-Services](24-UC-SYS-Cross-Cutting-Services.md) | 24 | `UC-SYS` | Cross-Cutting Services | 13 |

## What each module document contains


| Section | Content | Source |
| --- | --- | --- |
| Chapter <n> | Module purpose and the use-case catalogue table | Master document, verbatim |
| §<n>.D | Fully expanded specifications for the module’s critical paths | Master document chapter 25, verbatim |
| §<n>.S | Screen field specifications: every field, its binding, control type, mandatory flag, lookup and rules — the contract the Angular screen must implement | Derived from the legacy AngularJS views under Components/*/Views |
| §<n>.U | One expanded scenario per use case: trigger, pre-conditions, main flow, alternates, exceptions, post-conditions, realisation | The use case catalogue, mapped onto this project's routes, endpoints and services |
| §<n>.A | Screens of the module — **legacy reference**, mapping to the target Angular feature module | Master document Appendix A, filtered |
| §<n>.B | Web API controllers of the module — **legacy reference**, mapping to the target `ApiController` per aggregate | Master document Appendix B, filtered |

## How to use this set

- Read **00-Overview-and-Common-Context** once: the approved architecture (§2), the target non-functional characteristics (§26), the actor/role model, the sponsorship lifecycle, the role/module access matrix and all appendices.
- Before writing code, read `_bmad-output/planning-artifacts/architecture.md` — it is authoritative and records where the approved documents and the shipped code disagree (§10).
- Take the single module document you need. Chapter and use-case numbering is unchanged from the master document, so every `UC-xxx-nn` reference still resolves — substituting `UC-CHR` for `UC-NGO` where module 8 is concerned.
- Use **00-EPICS-and-User-Stories** for delivery planning: one epic per module, one user story per use case, with acceptance criteria. Track state in `_bmad-output/implementation-artifacts/sprint-status.yaml`.
- Follow the module build order in `00-Overview-and-Common-Context` §2 and `Architecture/02_QuickStartGuide.md`: Domain → Infrastructure → Application → API → Frontend → Tests → Docs.
- **Routing** — every `Realisation` entry, every §<n>.A / §<n>.B annex and every screen block in §<n>.S names this project's Angular 18 route and ASP.NET Core endpoint. The conventions, the full crosswalk and the known gaps are in [00-ROUTING-MAP](00-ROUTING-MAP.md).

## Traceability


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

