# Epic-9 review — findings ledger (chunks C1–C4)

Running ledger for `/bmad-code-review` (all 17 stories, 4 sequential chunks × 3 adversarial layers).
Status legend: `open` | `confirmed` (second layer verified) | `refuted` (verified false — drop at triage) | `arbitrate` (layers contradict — verify at step-03).

## C1 — backend register core (34 findings)

### Blind Hunter (16)
| # | Sev | Finding | Loc | Status |
|---|-----|---------|-----|--------|
| 1 | critical | Repair migration omits columns the service reads (ReportYear/ReportMonth/ReviewStatus/IsDeleted) → SqlException on first use | migration 20260824153802 | arbitrate — contradicts ECH#10 (snapshot says columns exist, nullable vs NOT NULL drift); model snapshot arbitrates |
| 2 | high | Duplicate guards check-then-act; no unique index in diff | PeriodicOrphanReportService create | open — ECH#2 says unique IX exists at InitialCreate:514, refutes "no index" half |
| 3 | high | `ChildOrParent == Child` / `!Reviewed` strict predicates drop NULL legacy rows | service predicates | open |
| 4 | high | Soft-delete permanently squats (orphan,year,month) unique slot | create/update guard | confirmed by ECH#3 |
| 5 | medium | Charity self-REFUSE blocked at create but self-accept not | create guard | open |
| 6 | medium | DropColumn SubmittedBy/SubmissionDate/ReviewedBy/ReviewDate — unguarded data loss | repair migration | open |
| 7 | medium | Update coalesce→full-replace: nulls in payload erase omitted fields | update path | open (9-5 full-replace contract recorded — check story ruling) |
| 8 | medium | 4 IValidator<T> deps seemingly unregistered | service ctor | refuted — validators auto-registered at Application/ServiceCollectionExtensions.cs:92 (verified) |
| 9 | medium | `dto.HousingBeneficiaryId!.Value` NRE path | service | open |
| 10 | medium | Bare `/api/Attachments/{id}/image` URLs likely unauthenticated media route | controller/print payload | open |
| 11 | medium | Export returns `Array.Empty<byte>()` as xlsx → 200 + corrupt file | export endpoint | confirmed by ECH#15 (zero-byte xlsx) |
| 12 | low | Update skips guardian one-per-family-month recheck | update path | confirmed by ECH#7 (re-dating skips rule) |
| 13 | low | Empty refusal reason persists | review path | open |
| 14 | low | GetPrintFormAsync dead code | service | open (C2-ECH cleared: it IS used by 9-17 print) |
| 15 | low | GetApproved/Rejected forced flags contradict ReviewStatus filter | service | open |
| 16 | low | Dual Deleted/IsDeleted flags confusion | entities/migrations | open |
| 17 | low | Null-forgiving `(await GetByIdAsync(...))!` | service | open |
| 18 | low | SummaryDto.OrphanId int→Guid wire break | DTO | open |

### Edge Case Hunter (14)
| # | Sev | Finding | Loc | Status |
|---|-----|---------|-----|--------|
| 1 | high | Null-CharityId stamp → report invisible to own charity (201 null body) | create + current-user claims | open — related C2-ECH#9 (TokenService omits claim → scope fails open) |
| 2 | high | Guardian one-per-family-month "filtered unique index" claim FALSE — plain non-unique IX at migration 20260824115014:37 | migration + story claim | open |
| 3 | high | Soft-delete burns (orphan,year,month) slot — InitialCreate:514 unique index unfiltered | InitialCreate index | confirmed (BH#4) |
| 4 | medium | Unknown ReviewStatus token silently ignored → unfiltered register | filter binding | open |
| 5 | medium | Duplicate race → raw 500 not friendly 400 | create | open |
| 6 | medium | POR-YYYY-NNNN count+1 numbering, no unique constraint (nvarchar(max)), D4 rollover | number generation | open |
| 7 | medium | Re-dating guardian report skips family-month rule | update | confirmed (BH#12) |
| 8 | medium | Concurrent reviews last-write-wins (no RowVersion) | review | open |
| 9 | medium | Scope fails open when caller has neither charity nor country claim (TokenService omits claim) | ApplyCallerScope | open — feeds BH-C2#1 fail-open |
| 10 | medium | 6 bool columns nullable in migration vs NOT NULL in snapshot — drift | repair migration vs snapshot | arbitrate vs BH#1 |
| 11–16 | low | summary counters vs list mismatch; snapshot drift (dup of 10); attachment ids only NotEmpty; inverted date range silent empty; Charity self-refuse (dup BH#5); zero-byte xlsx (dup BH#11) | — | open |

### Acceptance Auditor (4)
| # | Sev | Finding | Status |
|---|-----|---------|--------|
| 1 | medium | 9-6 story claims "framework does soft delete" — FALSE (Framework.Core filter commented out; hand-stamped; outcome still meets AC) | open — story-file correction |
| 2 | medium | Manual `!IsDeleted` predicates contradict recorded platform rule but are factually necessary (undocumented deviation) | open — record deviation in story |
| 3 | low | Coded-orphan pre-condition never enforced server-side on create | open |
| 4 | low | Controller re-based ControllerBase→ApiController against 9-1's recorded ruling (aligns with CLAUDE.md — ruling conflict, not defect) | open — reconcile ruling |

Verified true: caller scope pinning (ApplyCallerScope), clamps, date bounds, RefuseReason seeded + exposed, review rulings honoured.

## C2 — backend extracts & reports root (33 findings)

### Blind Hunter (20)
| # | Sev | Finding | Loc | Status |
|---|-----|---------|-----|--------|
| 1 | high | OrphanReportService tenancy fail-open + country-blind (`_currentUser.CharityId ?? filter.CharityId`) | OrphanReportService:69,427 | confirmed by ECH#2 + AA#2 |
| 2 | high | Soft-deleted reports leak into all 3 OrphanReportService reads | service ~250,440,460 | confirmed by ECH#1 (critical) + AA#1 |
| 3 | high | ReportService.ApplyCharityScopeAsync default-open fallthrough (`return query;`) | ReportService | open |
| 4 | medium | HQ narrow bypasses country pin | ReportService | open |
| 5 | medium | Print-form endpoint passes no caller scope | ReportsController:500 | open (C1-ECH cleared GetPrintFormAsync as scope-filtered — verify which call path) |
| 6 | medium | [co-owned] Missing-files coded-only predicate omitted | ReportService | open |
| 7 | medium | Grouped stats count reports not orphans + ignore date window | OrphanReportService | confirmed by ECH#5 (row vs orphan count); window half open |
| 8 | medium | 11 validators injected, 1 shipped → Page=0 negative-skip 500 | controller/service | open |
| 9 | medium | [co-owned] DateDiffYear age overstate | ReportService | confirmed by ECH low |
| 10 | medium | [co-owned] beneficiary-families full-table materialization | ReportService | confirmed by ECH#4 |
| 11 | medium | IncludeReportNumbers no-window silent no-op | OrphanReportService | confirmed by ECH#6 |
| 12 | medium | 500s swallowed unlogged | ReportsController | confirmed by AA#3 |
| 13 | medium | Detail extract runs on every GenerateReport | OrphanReportService | open |
| 14–20 | low | GetUserRole first-claim; mixed error envelopes; same-date supersede; non-deterministic 1000-cap truncation; Excluded>AllOrphans; CountOnly PageSize=0; DTOs in interface file | — | open |

### Edge Case Hunter (10)
| # | Sev | Finding | Loc | Status |
|---|-----|---------|-----|--------|
| 1 | critical | Soft-delete leak verified: RepositoryBase.AsQueryable():162-165 bare, no global filter | ground truth | confirmed |
| 2 | high | Caller-scope pin has no country/HQ gate (spec §14.U.14 pre-condition 3) | OrphanReportService | confirmed by AA#2 |
| 3 | medium | MezaCards ReportNo=0 → 400 not grid | ReportsController | open |
| 4 | medium | Beneficiary-families unbounded materialization | ReportService | confirmed (BH#10) |
| 5 | medium | Grouped stats row-count not orphan-count (vs TotalOrphans distinct) | OrphanReportService | confirmed (BH#7) |
| 6 | medium | IncludeReportNumbers partial window indistinguishable from empty | OrphanReportService | confirmed (BH#11) |
| 7–10 | low | null gender totals don't sum; DateDiffYear; GetUserRole; Guid.Empty drill-down silently empty | — | open |

Verified-cleared: non-renewed window math inclusive-correct; EducationalLevelId sentinel guarded; CountOnly no div-by-zero; GetPrintFormAsync already scope+IsDeleted filtered.

### Acceptance Auditor (3)
| # | Sev | Finding | Status |
|---|-----|---------|--------|
| 1 | high | Soft-delete leak violates 9-10 AC4/5, 9-11 AC1, 9-15 AC3; 9-10 reconciliation with 9-1 list can never pass | confirmed |
| 2 | high | Country scoping + HQ gate missing on shared statistics/generate scope (§14.U.10/11/15 pre-cond 3) | confirmed |
| 3 | low | ReportsController catch-alls never log (ILogger injected, unused) | confirmed |

Verified-satisfied: 9-10 grouped dimensions/query/DTO; 9-11 ReportNo/window/cap/toggle; 9-12/9-13 flag predicates; 9-14 countOnly + BR-11 + coded-only + route ruling; 9-15 CreatedOn window + unnumbered bucket; 9-17 variant resolver + 5 slots + 404-on-null + roles; DI convention scan holds.

## C3 — frontend core & register screens (47 findings)

### Blind Hunter (26)
| # | Sev | Finding | Loc | Status |
|---|-----|---------|-----|--------|
| 1 | critical | `:id` route declared before `orphan-reports` → grouped-statistics screen unreachable (sidebar link lands on detail with id='orphan-reports') | periodic-orphan-reports-routing.module.ts | open — UNVERIFIED by other layers; verify route order at triage |
| 2 | high | Missions menu shown to Charity vs permission map (Missions.View: SuperAdmin/Admin); stale comment; ungated /missions/create | main-layout.component.html:470-476 | [co-owned] open |
| 3 | high | OnPush: lookupOrphan/loadEducationLevels/save callbacks mutate state without markForCheck | periodic-report-form.component.ts | confirmed by ECH#1 (extended to save-failure paths) |
| 4 | high | `Number(null)=0` → refuseReasonId: 0 sent as FK when dropdown null | periodic-report-review.component.ts | open |
| 5 | high | `toISOString()` off-by-one day for UTC+3 (fmtDate in search columns + Excel) | orphan-report-search.component.ts | confirmed by ECH#12 (which rates low) — arbitrate severity |
| 6 | high | reviewStatus filter lowercase regression vs PascalCase server enum | periodic-reports-list.component.ts | refuted — ECH verified backend `ToLowerInvariant()` switch accepts lowercase |
| 7 | high | Office Development Projects menu admits Charity vs map | main-layout.component.html:198 | [co-owned] open |
| 8 | high | [co-owned] Failed login (200, no token) falls through to success path | auth.service.ts:182-189 | confirmed by ECH#13 (low; login navigates then bounces) |
| 9 | medium | educationalLevelId select `[value]` string vs numeric patch — never pre-selects in edit | periodic-report-form.component.html:2885 | open |
| 10 | medium | Server-error mapping reads `body.errors`/`body.message`, not envelope modelStateErrors | form + review components | partially confirmed by ECH#2 (review) — merge |
| 11 | medium | statusClass returns BS4 `badge-*` on BS5.3 stack | periodic-reports-list.component.ts:4350 | open |
| 12 | medium | Hard-coded Arabic filter option values (schoolType/medicalStatus/educationDegree) vs English enums elsewhere | orphan-report-search.component.html:1117-1207 | open |
| 13 | medium | [co-owned] Incoming/Outgoing submenu labels crossed with routes; import/history menu entries deleted | main-layout.component.html:254-273 | confirmed by ECH#17 |
| 14 | medium | Routing imports 4 components + gallery not in THIS diff | routing module | resolved — they are C4 files; only a defect if C4 misses them (C4 review checks) |
| 15 | medium | Excel export recursion: no cancel/unsubscribe, outlives component | orphan-report-search.component.ts:1835 | open |
| 16 | medium | Inconsistent error unwrapping (`error?.message` vs `error?.error?.message`) across siblings | 3 components | confirmed by ECH#2/#9 — merge into unwrap family |
| 17 | low | [co-owned] HQ Transfers submenu duplicates parent title | main-layout:525,533 | open |
| 18 | low | orphan/:orphanId prefill pin cannot be cleared from UI | orphan-report-search | open |
| 19 | low | [co-owned] getReportLabel fallback hard-coded English `Report ${n}` | orphan-report-comparison.component.ts:989 | open |
| 20 | low | toLocaleDateString() without locale | comparison component | open |
| 21 | low | serverErrorsKeys *ngFor lacks trackBy | form html:2959 | confirmed by ECH#6 |
| 22 | low | Pagination footer rendered when single page | orphan-report-search html:1373 | open (AA's zero-page finding is the inverse condition — merge at triage) |
| 23 | low | [co-owned] getPrintForm URL builds from environment.apiUrl — /api doubling risk | periodic-orphan-report.service.ts | open |
| 24 | low | Any lookup failure (500/403/network) shown as "unknown orphan" | form component | open |
| 25 | low | viewReport dead code after [routerLink] switch | search component | open |
| 26 | low | window.open of object URL in subscribe = popup-blocker bait; 60s revoke closes viewer | detail + review | confirmed by ECH#8 |
| 27 | low | [co-owned] families create link ungated beside gated refugee link | main-layout:296-306 | open |
| 28 | low | reportPeriodFrom label renders from-to range | detail html | open |
| 29 | low | andOr always sent; radio toggle doesn't trigger search | search component | open |

### Edge Case Hunter (19)
| # | Sev | Finding | Loc | Status |
|---|-----|---------|-----|--------|
| 1 | high | OnPush stale view — SweetAlert2 runs outside Angular; spinner never stops, header/save never update; save-failure leaves button disabled | form component 168-186, 433-462, 101-104 | confirmed (BH#3 merged) |
| 2 | high | Review submit double-unwraps error (service rethrows error.error) → BR-14 translated mapping dead code | review component:260 + service:47-50 | confirmed |
| 3 | high | orphanReports.* i18n keys missing from BOTH ar/en (top-level object only has statistics+generate; ~30+ keys incl. noHistoryMessage, untitledReport, breadcrumb.*, common.exportExcel) | ar.json/en.json | arbitrate — CONTRADICTS c4-i18n-appendix (61 keys, full parity); C4-ECH instructed to resolve (multiple objects at different JSON paths?) |
| 4 | medium | history + schedule routes have AuthGuard only (no PermissionGuard) — inconsistent with siblings | routing module:176,200 | open |
| 5 | medium | [co-owned] OrphanReports.Compare absent from PERMISSION_ROLES; hasPermission fails OPEN for unknown keys | routing:191 + auth.service:447-458 | open |
| 6 | medium | trackBy missing on serverErrorsKeys *ngFor | form html:398 | confirmed (BH#21) |
| 7 | medium | Load failure = silent blank screen (console.error only, no toast/redirect) | detail:77, review:171-175, form:284-287 | open |
| 8 | medium | window.open blob from async callback popup-blocked — print/download button "does nothing" | detail:150-160, review:128-140 | confirmed (BH#26) |
| 9 | medium | Delete-failure message double-unwrap → always generic message | list:158, detail:112 | confirmed (BH#16 family) |
| 10 | medium | Server-required fields lack client validators (orphanImageId, annualFeeForStudy) → guaranteed 400 round-trips | form ts:266, html:298 | open |
| 11 | low | Uncoded orphan resolves silently non-actionable (save disabled, no message) | form ts:172 | open |
| 12 | low | toISOString off-by-one + form default reportDate = UTC yesterday 00:00–03:00 Riyadh | search ts:292-295, form ts:200 | confirmed (BH#5) |
| 13 | low | [co-owned] Token-less 200 login still next()s → navigates to /dashboard → AuthGuard bounce, no error message | auth.service:213-230 + login.component:60 | confirmed (BH#8) |
| 14 | low | checkEditable fails open on HTTP error → edit offered then server-rejected | form ts:302-305 | open |
| 15 | low | Editing code after successful lookup leaves stale resolution (header A, input B, save A) | form ts:152-174 | open |
| 16 | low | HQ charity/level dropdowns may stay empty (async next without markForCheck) | search ts:136-144, list ts:87-90 | open |
| 17 | low | [co-owned] Incoming/Outgoing labels crossed (dup BH#13) | main-layout | confirmed |

Verified-cleared (C3): empty-state/page-header/breadcrumb inputs real; pagination clamps + last-row step-back; date windows inclusive server-side; reviewStatus lowercase ACCEPTED (refutes BH#6); ''-initial numerics safe via Newtonsoft; endpoints exist; double-submit protected; trackBy on all other loops; i18n keys for register screens all present both locales; null-guards on status; Excel cap 2000 + stops on empty page; interlocks clear residue; gallery OnPush correct + URL cleanup; 401 interceptor excludes login, no 403 ejection.

### Acceptance Auditor (2)
| # | Sev | Finding | Status |
|---|-----|---------|--------|
| 1 | low | 9-1 AC6 "paging control reports zero pages" — pager hidden entirely instead (unrecorded) | open |
| 2 | low | 9-9 AC4 same zero-page deviation (unrecorded) | open |

Verified-satisfied: full story-by-story list 9-1..9-9 (AC-level detail in agent output; recorded deviations honoured: by-code route name, blob download, flat i18n keys, refuse-reasons route, tri-state tokens, single-select reason, full-replace update).

## C4 — frontend extracts/print + i18n (45 findings)

### Blind Hunter (23)
| # | Sev | Finding | Loc | Status |
|---|-----|---------|-----|--------|
| 1 | high | Date sentinels `0001-01-01T00:00:00Z` collapse default window; one-sided date → from>to silent empty | orphan-reports-generate.component.ts:787 | refuted — ECH verified sentinel reads as `default(DateTime)` = "no filter"; inversion blocked client-side; server bounds inclusive |
| 2 | high | Auto-print never fires if any image fails (`complete` not called after `error`) | periodic-report-print.component.ts:144-159 | confirmed by ECH (medium) |
| 3 | high | Charity dropdown without markForCheck under OnPush | generate:72, list:1274 | confirmed by ECH (medium; 4 screens + list self-heals) |
| 4 | medium | "+ i18n appendix" not in diff — raw keys on screen | diff header | refuted — appendix is separate artifact; working-tree JSON verified by script + 2 layers (163 keys resolve; only history/compare/schedule keys missing — see ECH#5) |
| 5 | medium | Zero ApiResponse unwrapping — silent empty screens | report.service + generate | refuted — these endpoints return raw DTOs via Ok(...); shape verified |
| 6 | medium | `(change)` with `[(ngModel)]` reads stale model | orphan-reports-list.component.html:1032 | open — use (ngModelChange); verify |
| 7 | medium | Pagination `; false` doesn't preventDefault | generate html:523-530 | refuted — Angular maps trailing `false` return to preventDefault (ECH verified) |
| 8 | medium | Rapid charity switches race, no switchMap | orphan-reports-list.component.ts:1288 | confirmed by ECH (low) |
| 9 | medium | Gallery blob subscriptions never unsubscribed — URL leak + post-destroy callbacks | report-attachment-gallery.component.ts | confirmed by ECH (low) |
| 10 | medium | `exporting` flag never set — double-click duplicates export | generate + list | confirmed by ECH (medium; not re-entrancy guarded on any of 5 screens) |
| 11 | medium | [co-owned] Hard-coded English error fallback string | report.service.ts:2966 | open |
| 12 | medium | [co-owned] ReportService bare HttpClient, no auth header | report.service.ts:2944 | refuted — global AuthInterceptor exists (verified C3); gallery/print stream via ApiService |
| 13 | medium | Auto-print setTimeout never cancelled on destroy | periodic-report-print.component.ts:2316 | open |
| 14 | low | Page-number *ngFor no trackBy + `[].constructor(n)` trick | generate html:525 | open |
| 15 | low | reportDate cell lacks `|| '—'` fallback | generate html:490 | open |
| 16 | low | snapshot.paramMap — stale report on same-route id change | print ts:2216 | open |
| 17 | low | annualFeeForStudy hidden when 0; ragged row | print html:1856 | open |
| 18 | low | Gallery download filename no extension | gallery ts:1685 | open |
| 19 | low | charityName printed twice | print html:1747,1772 | open |
| 20 | low | Dead code: RouterLink imported unused; `auth` injected unused ×3 | generate/print/list | open |
| 21 | low | `const filter: any` discards DTO typing | generate ts:787 | open |
| 22 | low | [co-owned] getFamilyEntryTracking naked union type | report.service.ts:2917 | open |
| 23 | low | HQ charity dropdowns silently cap at 500 | generate/list | open |

### Edge Case Hunter (17)
| # | Sev | Finding | Loc | Status |
|---|-----|---------|-----|--------|
| 1 | critical | `:id` route shadows 9-10 statistics screen — `/orphan-reports` renders detail with id "orphan-reports" | routing:63 vs :113 + main-layout:629 | CONFIRMED (matches C3-BH#1) — move route above `:id` or rename prefix |
| 2 | high | Soft-deleted reports counted/listed on 9-10/9-11/9-15 (3rd confirmation of the C2 critical) | OrphanReportService.cs:250,454,510 | confirmed ×3 |
| 3 | high | Country-claim tenancy pin missing on shared statistics/generate path (3rd confirmation) | OrphanReportService.cs:427,69 | confirmed ×3 |
| 4 | high | 9-12/9-13 silent 2000-row cap — server totalCount discarded | state-extract ts:133,148 | confirmed (upgrades AA#2) |
| 5 | high | Route permission admits roles the endpoints reject — Accountant/Employee dead screen (403 → generic loadFailed) | auth.service:124 vs OrphanReportsController:38,321 | open — relates AA#5 |
| 6 | medium | OrphanReports.Compare not in PERMISSION_ROLES — guard fails open | routing:191 + auth.service:452 | confirmed (C3-ECH#5) |
| 7 | medium | i18n RESOLVED: one orphanReports object, 61 leaves parity (appendix accurate); MISSING: orphanReports.history/compare/schedule + breadcrumb.* (6 routing keys) + 59 template keys + common.exportExcel — both locales | ar.json:3206/en.json:3207 + routing:178,189,202 | RESOLVED — C3-ECH#3 stands narrowed to history/compare/schedule screens |
| 8 | medium | Pagination boundary clicks blank grid (page 0 / last+1) — no clamp | generate:124, state-extract:163, non-renewed:133, report-numbers:127 | open |
| 9 | medium | OnPush: HQ charity dropdown empty (4 screens) | generate:72, state-extract:88, non-renewed:73, report-numbers:71 | confirmed (BH#3 family) |
| 10 | medium | Auto-print dead on any image failure (dup BH#2) | print ts:144-159 | confirmed |
| 11 | medium | 9-14 non-renewed fetches only page 1 (100 rows) — count>100 silently truncated; export ships 100 rows as complete | non-renewed-reports.component.ts:106-118 | open ([co-owned] model already supports paging) |
| 12 | medium | `exporting` never set (dup BH#10) | generate:44, list:47 | confirmed |
| 13 | medium | codesOnly grid misalignment (dup AA#1: orphanCode td outside codesOnly guard) | generate html:85-136 | confirmed |
| 14 | low | Gallery subscriptions outlive report — unrevoked URLs + destroyed-view markForCheck | gallery ts:55-79,129 | confirmed (BH#9) |
| 15 | low | Failed re-run leaves previous result under new criteria | list:104-117 + pattern | open |
| 16 | low | Rapid charity switch race (dup BH#8) | list:84-119 | confirmed |
| 17 | low | Any `:state` other than `refused` silently renders accepted extract | state-extract ts:82-85 | open |

Verified-cleared (C4): raw-DTO envelope handling correct on all 5 screens; date-window semantics correct (sentinels + inclusive bounds); 9-10/9-15 endpoint discrimination correct; print variant/slot keys resolve; print object-URL lifecycle correct; gallery empty/failed states; `; false` preventDefault; charities shape; model parity; 9-14 coded-only + HQ narrow correct.

### Acceptance Auditor (5)
| # | Sev | Finding | Status |
|---|-----|---------|--------|
| 1 | medium | 9-11 grid headers misaligned one column (orphanCode no th; codesOnly 2-vs-3 cells) | confirmed by ECH#13 |
| 2 | medium | 9-12/9-13 "capped indicator" claim FALSE — totalCount from collected rows; 5000→2000 silent | confirmed/upgraded by ECH#4 (high) |
| 3 | low | 9-13 tab title shows accepted title on refused screen (route pageTitle hardcoded) | open |
| 4 | low | 9-13 refuse-reason column not red/prominent (record overstates; AC met) | open — story correction |
| 5 | low | Charity criterion hidden from Accountant/Employee HQ roles on 5 screens (unrecorded) | open — relates ECH#5 |

Verified-satisfied: 9-9 Excel scope; 9-10 full §14.S.3; 9-11 criteria/cap/paging/export (envelope read correctly on this screen); 9-12 wire keys + drain + register command; 9-13 reason column + refused branch; 9-14 countOnly variant + window validation; 9-15 shared endpoint + columns + tiles; 9-16 gallery 5 slots + lightbox + revoke; 9-17 print variant + footer key + 404 panel + auto/manual print; i18n 151 keys spot-checked 0 missing; all routes guarded; column counts consistent.

## Final totals
C1: 34 · C2: 33 · C3: 47 · C4: 45 — **159 findings** (confirmed-critical: route shadowing; confirmed-high: soft-delete leak ×3, tenancy pin ×3, silent 2000-cap, role/endpoint mismatch, OnPush family, print settle)

---

# STEP-03 TRIAGE (2026-08-24)

Arbitrations performed by orchestrator (not agent vote):
- **C1-BH#1 REFUTED** — migration omission claim: real InitialCreate is `Migrations/20260421205436_InitialCreate.cs` (not `Data/Migrations/…InitialApplicationCreate`); it created PeriodicOrphanReport WITH ReportYear/ReportMonth/ReviewStatus/IsDeleted. Repair migration is delta-only, column-exact vs snapshot. No defect.
- **C1-ECH#10 DISMISSED** — bool nullability drift already corrected in-file (inline review comment, migration lines 58-61).
- **C3-BH#1 / C4-ECH#1 CONFIRMED by direct read** — routing `:id` at line 64 declared before `orphan-reports` at line 114.

Unified deduplicated list: **159 raw → 96 unique** (49 merged dups/clears) → buckets below.

## decision_needed (2)

| ID | Finding | The choice |
|---|---------|-----------|
| D1 | Role/endpoint mismatch on extract screens: `PeriodicReports.View` map admits Accountant/Employee (auth.service:124) but `/api/OrphanReports/generate|statistics` authorize only SuperAdmin/Admin/Charity (OrphanReportsController:38,321) → those roles open 9-10/9-11/9-15 and every run 403s as generic "load failed"; same root hides the charity filter from them on 5 screens (C4-ECH#5 + C4-AA#5 + C3-AA role lists) | Widen endpoint roles to include Accountant/Employee, or narrow the route permission map — a product authorisation choice |
| D2 | Co-owned-file defects (other epics' parallel sessions own these): main-layout menu defects (Missions + OfficeDevProjects shown to Charity vs map; incoming/outgoing labels crossed + import/history entries removed; HQ-transfers duplicate label; families create-link ungated), auth.service login fall-through on 200-without-token, ReportService family (missing-files coded-only predicate, DateDiffYear, beneficiary-families materialization, HQ narrow bypass country pin, ApplyCharityScopeAsync fallthrough*, hard-coded error fallback, naked union return) | Patch under epic-9 review vs leave for the owning sessions (*note: ApplyCharityScopeAsync fallthrough is arguably epic-9's own 9-14 helper — patch candidate if user agrees) |

## patch (57) — unambiguous fixes

**Backend — tenancy & correctness (6)**
- P1 [critical→high] Soft-delete leak: 3 OrphanReportService reads (cs:250,454,510) lack `!r.IsDeleted` — add predicate (3-layer confirmed; breaks 9-10/11/15 ACs + 9-10↔9-1 reconciliation)
- P2 [high] Tenancy pin: `effectiveCharityId = _currentUser.CharityId ?? filter.CharityId` (cs:69,427) — mirror ReportService.ApplyCharityScopeAsync (HQ gate + country pin) per spec §14.U.10/11/15 pre-cond 3
- P3 [high] Scope fail-open when caller has neither charity nor country claim — fail closed (400) instead of unscoped query (feeds P2)
- P4 [medium] Null-CharityId stamp at create → report invisible to own charity + 201 null body — stamp from claims, 400 if absent
- P5 [medium] Unknown ReviewStatus token silently ignored → unfiltered register — validate, 400
- P6 [medium] GetApproved/Rejected… (recorded) — REPLACED: unknown-state API guard on `:state`-style enum accepts — see P41; keep server token validation only

**Backend — register business rules (8)**
- P7 [high] Soft-delete squats (orphan,year,month) unique slot — filtered unique index (WHERE IsDeleted=0); mind QUOTED_IDENTIFIER for filtered-index DDL via sqlcmd
- P8 [medium] Guardian "one per family/month" plain IX, not filtered-unique (story claim false) — same filtered unique index treatment (merge with P7 if same index)
- P9 [medium] Duplicate race → raw 500 — catch 2601/unique violation → friendly 400
- P10 [medium] Charity self-accept not blocked (self-refuse is) — symmetric guard
- P11 [medium] Update skips guardian/family-month recheck on re-dating — run same rule on update
- P12 [medium] `ChildOrparent == Child` / `!Reviewed` strict predicates drop NULL legacy rows — null-tolerant predicates
- P13 [low] Empty refusal reason persists when accepting — clear reason fields on accept
- P14 [low] Coded-orphan pre-condition not enforced server-side on create — validate Orphan.Coded

**Backend — reports/extracts (7)**
- P15 [high] Grouped stats count reports, not orphans (spec counts orphans; totals disagree with tiles) — distinct-orphan counting per spec
- P16 [medium] IncludeReportNumbers no-window → silent no-op / partial window indistinguishable — document via response or require window
- P17 [medium] MezaCards ReportNo=0 → 400 instead of grid — treat 0 as unnumbered
- P18 [medium] ReportsController catch-alls never log — `_logger.LogError(ex, …)` per repo convention
- P19 [medium] 11 validators injected, 1 shipped → Page=0 negative skip 500 — clamp page ≥ 1
- P20 [low] 1000-cap truncation non-deterministic (no OrderBy) — deterministic ordering
- P21 [low] SummaryDto.OrphanId int vs list Guid — align wire contract

**Frontend — critical routing & guards (4)**
- P22 [CRITICAL] Route shadowing: `:id` (routing:64) before `orphan-reports` (routing:114) — 9-10 screen unreachable via sidebar — move static route above `:id`
- P23 [medium] history + schedule routes lack PermissionGuard (routing:176,200) — add guard + data.permission (sibling pattern)
- P24 [medium] `OrphanReports.Compare` missing from PERMISSION_ROLES (fails open) — add map entry (additive, co-owned file but safe)
- P25 [medium] `:state` other than refused silently renders accepted — guard unknown state

**Frontend — i18n (3)**
- P26 [medium] ~65 keys missing from BOTH locales: `orphanReports.history|compare|schedule(.breadcrumb)` routing keys + 59 template keys + `common.exportExcel` — history/compare/schedule screens render raw keys — add ar+en entries
- P27 [low] Hard-coded English `Report ${n}` fallback (comparison) — i18n key
- P28 [low] `toLocaleDateString()` without locale (comparison) — locale-aware pipe

**Frontend — OnPush / async family (6)**
- P29 [high] form: lookupOrphan/loadEducationLevels/save-error mutate state without markForCheck (spinner stuck, header/save dead) — add cdr.markForCheck()
- P30 [medium] HQ charity dropdowns empty on 4 extract screens + search/list (async next, OnPush) — markForCheck
- P31 [medium] Rapid charity-switch race (no switchMap/unsubscribe) — switchMap
- P32 [medium] Failed re-run leaves previous results under new criteria — clear on error/new run
- P33 [medium] Gallery/print blob subscriptions outlive component (URL leak, destroyed-view writes) — teardown
- P34 [low] Excel export recursion (search) no cancel — takeUntil

**Frontend — error handling family (4)**
- P35 [high] Review submit double-unwrap → BR-14 translated mapping dead — `error ?? {}` fallback (form's pattern)
- P36 [medium] Delete-failure + form server-error mapping reads wrong shape — align to handleError contract
- P37 [medium] Load failure = silent blank screen (detail/review/form) — toast + redirect (search's pattern)
- P38 [low] Any lookup failure shows "unknown orphan" — differentiate messages

**Frontend — data/display correctness (12)**
- P39 [high] 9-12/9-13 silent 2000-row cap (totalCount from collected rows; recorded claim false) — use server totalCount for capped indicator
- P40 [medium] 9-14 non-renewed fetches only page 1 (100 rows) — drain pages or capped notice; export must not ship partial as complete
- P41 [medium] 9-11 grid headers misaligned (orphanCode no th; codesOnly 2-vs-3 cells) — fix thead/guards
- P42 [medium] Pagination boundary page 0 / last+1 blanks grid (4 screens) — clamp
- P43 [medium] `exporting` flag never set (5 screens) — toggle in exportData
- P44 [medium] Auto-print never fires if any image fails — decrement in error handler too
- P45 [medium] Auto-print setTimeout not cancelled on destroy — track + clearTimeout
- P46 [high] refuseReasonId `Number(null)=0` sent as FK — null-guard
- P47 [medium] toISOString off-by-one (UTC+3) in fmtDate + form default date — local-date formatting
- P48 [medium] educationalLevelId select `[value]` string vs numeric patch — `[ngValue]`
- P49 [medium] statusClass BS4 `badge-*` on BS5 — `bg-*`
- P50 [medium] `(change)` reads stale ngModel (list charity select) — `(ngModelChange)`

**Frontend — small fixes (10)**
- P51 trackBy ×2 (serverErrorsKeys, page-numbers) · P52 reportDate `|| '—'` · P53 snapshot.paramMap stale id · P54 annualFeeForStudy 0 hidden + ragged row · P55 gallery filename extension · P56 charityName duplicated in print · P57 dead code (viewReport, RouterLink/auth unused ×3) · P58 `const filter: any` → typed DTO · P59 getPrintForm URL `environment.apiUrl` doubling check · P60 uncoded-orphan save-disabled hint + stale-resolution reset + checkEditable fail-closed

**Docs / story-file corrections (4)**
- P61 9-6 claim "framework does soft delete" FALSE — correct Change Log
- P62 9-12/9-13 "capped indicator" claim FALSE — correct after P39
- P63 Record pager zero-page deviation (9-1 AC6, 9-9 AC4) + manual `!IsDeleted` deviation note (platform rule)
- P64 Reconcile 9-1's ControllerBase ruling with the ApiController re-base (CLAUDE.md side wins — update the ruling)

## defer (8)

| ID | Finding | Why deferred |
|---|---------|--------------|
| F1 | Concurrent reviews last-write-wins (no RowVersion) | Schema + concurrency work, beyond review scope |
| F2 | Detail extract runs on every GenerateReport (perf) | Perf refactor; measured impact unknown |
| F3 | Same-date supersede semantics (non-renewed window) | Minor spec ambiguity |
| F4 | API-caller inverted date range silently empty (UI already validates) | Direct-API edge |
| F5 | HQ charity dropdowns cap at 500 silently | Tenant-scale dependent |
| F6 | POR ReportNo nvarchar(max), count+1 race, D4 rollover | Numbering scheme redesign |
| F7 | 9-13 "prominent red" reason column styling (AC met, record overstates — tiny CSS; folded here as cosmetic) | Cosmetic |
| F8 | `andOr` always sent on wire | Harmless no-op without status predicates |

## dismiss (29) — dropped

Refuted (verified false): C1-BH#1 migration omission · C1-BH#8 validators unregistered · C1-BH#14 GetPrintFormAsync dead · C1-ECH#10 snapshot drift (fixed in-file) · C2-BH#5 print no-scope · C2-BH CountOnly PageSize=0 · C3-BH#6 reviewStatus lowercase · C3-BH#14 routing refs · C4-BH#1 date sentinel · C4-BH#4 i18n appendix missing · C4-BH#5 ApiResponse unwrapping · C4-BH#7 `; false` · C4-BH#12 bare HttpClient · C1-BH#10 attachment /image unauthenticated (bearer-only verified)
Recorded rulings (story files): update full-replace (9-5) · forced flags pair (9-12/13) · export stubs (9-17) · Arabic filter option tokens (9-3/9-9 tri-state ruling) · DropColumn data-free (migration header documents empty table)
Noise/by-design: dual Deleted/IsDeleted flags · DTO classes in interface file location · pagination footer on single page · HQ dropdown 500-cap display (dup F5)

**Counts: 96 unique → 2 decision_needed · 57 patch (4 critical/high clusters) · 8 defer · 29 dismiss**

## Cross-chunk triage flags (step-03 must arbitrate)

1. **C1-BH#1 vs C1-ECH#10** — repair migration "omits columns" vs "columns exist but nullable-vs-NOT-NULL drift": read the model snapshot + migration together.
2. **C1-BH#2** — "no unique index in diff": refuted by InitialCreate:514 unique:true; re-check what the duplicate guard actually races on.
3. **C3-BH#6 refuted** — reviewStatus lowercase is accepted by backend (ToLowerInvariant).
4. **C3-ECH#3 vs C4 appendix** — orphanReports.* i18n: multiple objects at different JSON paths suspected; C4-ECH resolves.
5. **C3-BH#1 critical route shadowing** — no second layer examined route ORDER; verify `:id` vs `orphan-reports` declaration order directly before patching.
6. **C3-BH#14** — components referenced but absent from C3 diff: resolved iff C4 diff ships them (C4 layers confirm existence).
7. **C1-BH#7 / C1-BH#15** — update full-replace + forced flags: check 9-5/9-12/9-13 recorded rulings before counting as defects.
8. **C1-BH#14 vs C2-ECH cleared** — GetPrintFormAsync dead-code claim vs "used by 9-17, scope-filtered": verify call graph.
9. **Severity arbitrations**: toISOString off-by-one (BH high vs ECH low); orphanReports i18n (ECH high — impact depends on #4).
10. **[co-owned] items** (main-layout, auth.service, ReportService family, report.model/service): defects live in files other epic sessions own — route via user, don't patch unilaterally.
