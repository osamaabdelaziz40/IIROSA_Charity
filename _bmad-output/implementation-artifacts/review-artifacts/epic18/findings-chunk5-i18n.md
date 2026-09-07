# Epic-18 review — chunk 5 (i18n parity) — auditor findings

## Check 1 — Key parity (ar.json vs en.json)
**`reports.*` parity is clean — 650 leaf keys in each locale, zero path mismatches.** Deep compare of `reports` subtree: identical key sets, no type conflicts. — pass

## Check 2 — Usage coverage
**No referenced-but-missing leaf keys — zero broken renders.** 552 quoted `reports.*` references across 116 module files + main-layout; 549 resolve to existing leaf keys in both locales; the 3 non-leaf refs are dynamic prefix roots (`reports.finishedSponsorship` / `reports.unsponsoredOrphans` via keyPrefix getter; `reports.surveyQuestionnaire` via i18nPrefix). All dynamically composed keys (22 variant keys, 35 questionnaire keys, export-path compositions, 24 cross-namespace keys) verified present in both locales. — pass

## Check 3 — Dead keys
**6 dead keys, all under `reports.print.*` (leftovers of a guardians/widows print feature never wired in):** `printGuardians`, `printWidows`, `titleGuardians`, `titleWidows`, `familyCode`, `familyCodePlaceholder`. No file in Frontend/src references them. — hygiene

## Check 4 — Hard-coded user-facing strings
**No violations.** Char-by-char state-machine extraction across all 29 report templates: zero literal Arabic/English display text; Arabic literals only inside comments; all ~134 `notification.*` calls route through translate wrappers; no `Swal.fire`/`alert()`/`confirm()`; main-layout reports menu (26 labels) all `| translate`. — pass

## Check 5 — Placeholder consistency
**Clean.** One parametrised key (`reports.charityTracking.updatesLoaded`, `{{count}}` in both locales); sole call site passes `{ count: ... }`. — pass

## Informational
Duplicate-style generic export-failure keys both live: `reports.exportFailed` (most components) vs `reports.export.failed` (family-follow-up-report) — both render; inconsistent naming is a small maintenance hazard. — hygiene

## Counts
| Metric | Value |
| --- | --- |
| Keys checked (reports.* leaf keys, per locale) | 650 / 650 |
| Referenced keys (literal) | 552 |
| …plus dynamically composed | 57 |
| Referenced-but-missing | 0 |
| ar/en parity mismatches | 0 |
| Dead keys | 6 |
| Hard-coded UI strings | 0 |
| Placeholder mismatches | 0 |
