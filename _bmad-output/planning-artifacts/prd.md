---
title: IIROSA Charities — Product Requirements Document
project: IIROSA Charities
version: 1.0
date: 2026-08-18
status: approved
stepsCompleted: [imported-from-war-iirosa-documentation]
---

# IIROSA Charities — Product Requirements Document

## 1. Product summary

IIROSA Charities is a **multi-tenant welfare administration platform** operated by a head office
(HQ) and used by a network of partner charities across several countries. Its
core mission is the end-to-end management of the **orphan sponsorship cycle** and the related
family-welfare programmes.

This project is a **re-platform**. The functional scope is the complete, documented behaviour of the
legacy WAR.IIROSA system; the technical platform is the modern .NET 8 / Angular 18 stack defined in
`architecture.md`. Nothing in the legacy functional surface is dropped without an explicit decision.

| Field | Value |
| --- | --- |
| Functional baseline | WAR.IIROSA — 19 modules, 230 documented use cases, 661 screen fields |
| Specification | `docs/Modules/` (one document per module) |
| Backlog | `_bmad-output/planning-artifacts/epics.md` — 19 epics, 230 stories, 838 pts |
| Delivery board | `_bmad-output/implementation-artifacts/sprint-status.yaml` |
| Architecture | `_bmad-output/planning-artifacts/architecture.md` |
| Languages | Arabic (primary, RTL) and English |

## 2. Business pillars

| Pillar | Description | Epics |
| --- | --- | --- |
| **Beneficiary register** سجل المستفيدين | Family files, guardians (parents/widows), orphans, housing-project families and refugee families — national-ID validation, orphan coding, inter-charity file transfer | EP-05 … EP-08 |
| **Periodic reporting** التقارير الدورية | Recurring per-orphan status reports (health, education, behaviour, prayer, memorisation, family circumstances) with photo and document attachments; HQ accepts or refuses with reasons | EP-09 |
| **Financial disbursement** الدفعات والحوالات | Payment batches per charity, per-orphan payment rows, bank transfer file generation, CSV reconciliation, cheque issuance and printing, receipt confirmation, HQ transfers | EP-10, EP-11, EP-17 |
| **Programme management** المشاريع | Seasonal assistance projects, office development projects, missions (field visits), technical-support tickets | EP-12 … EP-15 |
| **Correspondence & reporting** الصادر والوارد والتقارير | Incoming/outgoing official letters linked to orphan reports, plus the operational and statistical report catalogue | EP-16, EP-18 |

## 3. Actors

| Actor | Arabic | Scope |
| --- | --- | --- |
| HQ administrator | المكتب / المقر الرئيسي | Supervises all charities across all countries; approves reports, runs payment batches, issues transfers |
| Charity user | الجمعية | Tenant user — sees and manages only their own families, orphans, reports and payments |
| Employee / staff | الموظف | Role-scoped operational user (missions, correspondence, support) |
| Prospective charity | — | Self-registers a charity account, pending HQ approval |

## 4. Epics

| Epic | Module | Prefix | Stories | Points | Spec |
| --- | --- | --- | --- | --- | --- |
| EP-01 | Authentication & User Account | `UC-AUT` | 9 (+2 new) | 21 (+21) | `06-UC-AUT-*` |
| EP-02 | Home Dashboard | `UC-DSH` | 3 | 7 | `07-UC-DSH-*` |
| EP-03 | Charity Administration | `UC-CHR` | 9 | 36 | `08-UC-CHR-*` |
| EP-04 | Employee & User Administration | `UC-EMP` | 6 | 18 | `09-UC-EMP-*` |
| EP-05 | Family Register | `UC-FAM` | 14 | 68 | `10-UC-FAM-*` |
| EP-06 | Housing Project | `UC-HOU` | 8 | 36 | `11-UC-HOU-*` |
| EP-07 | Refugee Families | `UC-REF` | 4 | 19 | `12-UC-REF-*` |
| EP-08 | Orphan Register & Coding | `UC-ORP` | 11 | 31 | `13-UC-ORP-*` |
| EP-09 | Orphan Periodic Reports | `UC-ORR` | 17 | 80 | `14-UC-ORR-*` |
| EP-10 | Orphan Payments & Disbursement | `UC-PAY` | 24 | 95 | `15-UC-PAY-*` |
| EP-11 | General Cheques | `UC-CHQ` | 10 | 35 | `16-UC-CHQ-*` |
| EP-12 | Seasonal Assistance Projects | `UC-PRJ` | 13 | 43 | `17-UC-PRJ-*` |
| EP-13 | Office Development Projects | `UC-OFP` | 6 | 22 | `18-UC-OFP-*` |
| EP-14 | Technical Support | `UC-CST` | 6 | 19 | `19-UC-CST-*` |
| EP-15 | Missions | `UC-MSN` | 9 | 30 | `20-UC-MSN-*` |
| EP-16 | Correspondence — Incoming & Outgoing | `UC-COR` | 19 | 54 | `21-UC-COR-*` |
| EP-17 | HQ Financial Transfers | `UC-TRF` | 8 | 28 | `22-UC-TRF-*` |
| EP-18 | Reports & Printing | `UC-RPT` | 41 | 151 | `23-UC-RPT-*` |
| EP-19 | Cross-Cutting Services | `UC-SYS` | 13 | 45 | `24-UC-SYS-*` |
| | | **Total** | **232** | **859** | |

Full story text, MoSCoW priority and acceptance criteria: `epics.md`.

## 5. Delivery sequencing

Dependency order — EP-01 gates everything, and the register gates the money.

```
EP-01 Authentication  ──►  everything

EP-03 Charity Admin ──►  EP-05 Family Register ──►  EP-08 Orphan Register & Coding
                                                      │
                                    ┌─────────────────┼─────────────────┐
                                    ▼                 ▼                 ▼
                          EP-09 Periodic Reports  EP-10 Payments   EP-06/07 Housing, Refugees
                                    │                 │
                                    └────► EP-16 Correspondence
                                                      ▼
                                          EP-11 Cheques, EP-17 Transfers

EP-18 Reports & Printing  ──►  depends on whatever data modules have shipped
EP-19 Cross-Cutting  ──►  built alongside, not last
```

**Recommended order:** EP-01 → EP-19 (cross-cutting foundations) → EP-03 → EP-04 → EP-05 → EP-08 →
EP-09 → EP-10 → EP-11/EP-17 → EP-06/EP-07 → EP-12/EP-13 → EP-15/EP-16 → EP-14 → EP-02 → EP-18.

## 6. Non-functional requirements

| # | Requirement |
| --- | --- |
| NFR-1 | **Tenancy isolation.** A charity user must never read or write another charity's data. Enforced server-side in every query — client-side filtering does not count. |
| NFR-2 | **Authorisation is server-side.** Role-based menu hiding in the client is a usability feature, never a control. Every endpoint authorises independently. |
| NFR-3 | **Bilingual, RTL-first.** Every user-facing string is translatable; Arabic is the primary language. Entities carrying user-facing text have `NameAr` / `NameEn`. |
| NFR-4 | **Full audit trail.** Every business entity records created/updated/deleted actor and timestamp automatically. |
| NFR-5 | **Soft delete.** No hard deletes on business data; deleted rows are excluded from reads. |
| NFR-6 | **Typed API contracts.** Every endpoint has typed request and response DTOs and returns `ApiResponse<T>`. No untyped `JObject` binding — this was a defect of the legacy system. |
| NFR-7 | **Token-based authentication.** Short-lived signed JWT plus rotating refresh token, replacing the legacy 15-day forms-authentication cookie. |
| NFR-8 | **Testability.** Service-layer unit tests and controller integration tests for every module. |
| NFR-9 | **Performance.** Server-side paging, filtering and sorting on all list endpoints; lazy-loaded frontend modules; `OnPush` change detection. |

## 7. Defects in the legacy baseline — do not reproduce

Carried forward from the documented baseline as explicit non-goals:

- Dead client routes (`Users`, `user`) pointing at views that do not exist.
- 13 of the 230 use cases have **no server endpoint** — client-only behaviour, including role-based
  menu hiding treated as authorisation.
- Duplicate `V2` / `V3` / `Two` capability variants (`ChequeTwoController`, `FamilyTwo`, …) — the new
  system delivers one implementation per capability.
- Write endpoints accepting untyped `JObject`, leaving the request contract implicit in the method
  body.
- Reporting logic buried in ~160 stored procedures rather than in the application layer.

## 8. Out of scope

- Migration of legacy physical database schema and stored-procedure internals (a separate data
  migration workstream).
- Crystal Reports `.rpt` definitions — reporting is re-implemented on the new stack (EP-18).
- Infrastructure and deployment topology.

## 9. Source documents

| Document | Location |
| --- | --- |
| Module specifications (19) | `docs/Modules/06-UC-AUT-*` … `24-UC-SYS-*` |
| Overview, actors, appendices | `docs/Modules/00-Overview-and-Common-Context.md` |
| Module set index | `docs/Modules/00-INDEX-Module-Documentation-Set.md` |
| Epics & user stories | `docs/Modules/00-EPICS-and-User-Stories.md` → `epics.md` |
| Master project documentation | `docs/WAR.IIROSA-Project-Documentation.docx` |
| Security assessment | `docs/Security-Assessment-Remediation-Report.docx` |
| Approved architecture | `Architecture/01`–`04`, consolidated in `architecture.md` |
