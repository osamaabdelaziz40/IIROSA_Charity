# Epic 7 Review — Group C (i18n + shared wiring) — Blind Hunter findings

> Provenance: adversarial review of `epic7-C-shared.diff` ONLY (no project access, no spec).
> 24 raw findings, unverified. The i18n files contain OTHER concurrent epics' keys — triage
> separates epic-7 surface (families.refugee*, refugee menu, Families.* permissions, §12.S.2
> catalogue getters) from other epics' changes.

1. [Critical] `main-layout.component.html` adds `'Charity'` to the Office Development Projects dropdown (`hasAnyRole(['Admin', 'SuperAdmin', 'Charity'])`) while the same diff's `PERMISSION_ROLES` in `auth.service.ts` defines `'OfficeDevelopmentProjects.View': ['SuperAdmin', 'Admin']` and its own comment says "everything is Admin,SuperAdmin except delete" — and the HTML comment directly above the change still reads "(Admin/SuperAdmin only)". Two artifacts in one diff contradict each other; Charity users get a menu whose endpoints the map (and per that map, the server) refuse. The comment was not even updated to match the code it sits on.

2. [Critical] Identical drift on Missions: menu widened to `hasAnyRole(['Admin', 'SuperAdmin', 'Charity'])` with the stale "(Admin/SuperAdmin only)" comment left in place, but `'Missions.View'/'Missions.Create'` are `['SuperAdmin', 'Admin']` per the map. The "Add Mission" child link in the visible context carries no `*ngIf` at all, so a Charity user is offered a create action the permission system denies. Same defect class as #1, shipped twice in one diff.

3. [High] The new sidebar entries consume `"IIROSA.refugeeFamilies"`, `"IIROSA.addRefugeeFamily"`, and `"IIROSA.periodicOrphanReports"` (menu lines for refugee register, add-refugee-family, and the Periodic Orphan Reports parent), but the catalogues only add `shared.refugeeFamilies`, `shared.addRefugeeFamily`, `shared.periodicOrphanReports` — no `IIROSA.*` additions appear anywhere in either `ar.json` or `en.json`. Unless an undocumented alias maps `IIROSA.` to `shared.`, three brand-new menu items render raw translation keys in both languages.

4. [High] Incoming/Outgoing menu rewiring is semantically incoherent: "Attach Employees to Incoming Letter" routes to `/incoming-outgoing/export/incoming`, "Attach Orphan Reports" to `/export/outgoing`, and the orphans report to `/export/outgoing-orphans`. Mutation and reporting features are hung on legacy `export`-named routes; either the routes are stale misnomers or the wrong routes are wired.

5. [High] The `technicalSupport` duplicate-key fix silently changes behavior: previously `"title"` appeared twice in the section; JSON parsing lets the last occurrence win. After the rename to `ticketTitle`, `technicalSupport.title` now resolves to the section name instead of the ticket-title label. Every unmigrated consumer of the old winning value regresses (both languages).

6. [Medium] Mass key deletions with no migration evidence: `generalChecks` loses `reconciliation`, `checkStatuses`, `voidReasons`, `currencies`, the whole void/clear workflow; `incomingOutgoing` loses ~90 keys (import/export wizards, history, rollback, renames); `housingProjects` deletes its entire project-management vocabulary while the menu route stays `/housing-projects`. No proof every referencing template was updated; any survivor renders raw keys or empty labels.

7. [Medium] `en.json` deletes the top-level `validation` section and re-homes `fixErrors` into the common errors object — but `ar.json` shows no corresponding deletion. The two catalogues now structurally diverge; a template binding `validation.required` resolves in Arabic and breaks in English.

8. [Medium] Asymmetric role-key surgery in `users`/`employees`: both files remove `users.roles` and `users.permissions`, but `en.json` then adds `employees.roles` while `ar.json` adds no `roles` key to its employees section; `en.json` removes `users.assignRoles` with no visible `ar.json` counterpart removal. Templates binding these now behave differently per language.

9. [Medium] The employees-section additions are grossly asymmetric: `en.json` gains ~30 keys where `ar.json` gains 14. Some may pre-exist in Arabic, but the diff demonstrates no parity check. No ar/en parity test or CI gate appears anywhere — that is the missing control that would have caught items 7–9.

10. [Medium] `CountryDto.maxTransferAmount` is added to the read DTO only. `CreateCountryDto` receives no matching field, and `lookup-management.service.ts` gains no method to read or save per-country ceilings, despite the new `hqTransfers.maxAmounts.*` strings implying a full management screen. Dead weight in this diff.

11. [Medium] `ar.json` ships `"generalChecks.isDone": "CheckDone"` — a camelCase code identifier presented as the Arabic display string (English value is the same token). An RTL Arabic UI will show literal "CheckDone".

12. [Medium] Both catalogues embed `"Operation Faild:"` — an English fragment with a typo inside `incomingOutgoing.childCodeRequired`, in Arabic and English alike.

13. [Medium] The login fix in `auth.service.ts` papers over the failure instead of surfacing it: when `response.token` is absent the code calls `clearAuthData()` and falls through to the "verify token was stored" block. No error thrown, no failure signal returned — subscribers cannot distinguish failed from successful login; `clearAuthData()` on a failed attempt may wipe state a retry needed.

14. [Medium] The Orphan Payments menu is opened to Charity while the diff's own annotation concedes "The Charity-without-orphanId list guard on GET /api/OrphanPayments stays a server-side 403 regardless of this map." The top-level entry leads a Charity user straight into a refusing list view in the default case.

15. [Medium] Split-brain gating: the same diff gates Families/PeriodicReports/HqTransfers via `hasPermission('<Constant>')` but OfficeDev/Housing/SeasonalAid/Missions/OrphanPayments/Cheques via duplicated `hasAnyRole([...])` literals — and findings 1–2 prove the two mechanisms already disagree. New constants `OrphanPayments.BankFile`, `OrphanPayments.Import`, `OrphanPayments.AddOrphans`, `SeasonalAid.ManageBeneficiaries`, `SeasonalAid.RecordDistribution`, `SeasonalAid.Reports`, `HqTransfers.ManageLimits`, `GeneralChecks.Create/Edit`, `PeriodicReports.Create/Edit/Delete/Review`, `Families.Transfer`, `Families.Members` have no visible consumer in the menu diff — defined permissions that are either dead or enforced nowhere shown.

16. [Medium] `getHousingFlats(buildingId: number)`: the doc comment admits the endpoint returns 400 when `buildingId` is absent, yet the method has no guard — `undefined`/`NaN` produces `buildingId=NaN` and a raw 400. Every new method also uses `catchError(this.handleError)` with an unbound prototype-method reference — if its body ever touches `this`, the error handler itself throws.

17. [Low] `getRefuseReasons()` is documented as the UC-ORR-08 periodic-report refusal catalogue but is filed under the "§11.S.2 FORM CATALOGUE (UC-HOU-03)" section header — wrong grouping.

18. [Low] The HQ Transfers menu parent and its only child item are both labelled with the identical key `hqTransfers.title` — a dropdown whose sole entry repeats its own header.

19. [Low] `housingProjects.form.child.fullName` is translated "First name" in Arabic and English alike — a full-name field labelled as its first component, a visible copy-paste from `guardian.firstName`.

20. [Low] `hqTransfers.details` value drift: `isExecuted` is "مصير الحوالة" in Arabic versus "Transfer Status" in English, and `addLine` — an add-detail-line action — is "تفاصيل"/"Details" in both languages.

21. [Low] Unproofread Arabic shipped user-visible: "إيذون" for receipt (correct: إيصال) three times in seasonal-aid receipt strings, "موافه" for موافقة in `providerRequests.raiseSuccess`, hamza-less spellings inconsistent with neighbours, "الى تاريخ" beside "إلى تاريخ" in the same section.

22. [Low] A whole new top-level namespace `periodicReviews` exists to hold exactly one key (`reviewedBy`), and its Arabic value "المعتمِد" does not match the English "Reviewed by" — a reviewer and an approver are different actors.

23. [Low] Button vocabulary drifts across the new sections: "تم" used as confirm/submit in `members.confirmMove`, `providerRequests.submitDecision`, `guardian.confirm`, while the established pattern elsewhere is "تأكيد"/"حفظ"; English capitalization flips between adjacent sections; Arabic mixes كافة/كل/جميع within sibling objects.

24. [Low] `main-layout.component.ts` removes the ApexCharts global scripts on the strength of a comment claiming the npm wrapper owns charting, while still loading `CustomJsCodes1.js` and `jquery.sparkline.min.js` from the same legacy bundle. Any residual `window.ApexCharts` consumer in that not-yet-removed legacy JS silently breaks.
