# Story 9-16: View report attachments

| Field | Value |
| --- | --- |
| Story key | `9-16-view-report-attachments` |
| Epic | EP-09 — Orphan Periodic Reports (التقارير الدورية للأيتام) |
| Use case | UC-ORR-16 — صور اليتيم والشهادات |
| Priority / size | Should · 3 points |
| Specification | `docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md` (§14.U.16 scenario; the viewer commands of §14.S.2) |
| Route | hosted on `#/periodic-orphan-reports/:id` (detail) and the form/review screens |
| Endpoint | `GET /api/Attachments/{id}/image` (existing; siblings `/download`, `/info`) |
| Depends on | **9-3** (uploads populate the references), **9-4** (detail screen shows the slots) |
| Roles | Charity, HQ roles → `Charity`, `SuperAdmin`, `Admin`, `Accountant`, `Employee` |

## Status

review

## Story

As a charity user, I want to be able to view report attachments صور اليتيم والشهادات, so that I can
answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given a charity user opening a report, when the actor invokes the image viewer on an attached
   photograph or certificate, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by `GET
   /api/Attachments/{id}/image` and the image renders without a page reload.
3. Given the attachment slot is empty, when the viewer opens, then the control reports that no
   image exists rather than rendering a broken image.
4. Given the report belongs to another charity, when a charity-bound caller requests the image, then
   the attachment is not served — the owning report's scope gates the ids this module exposes (the
   detail screen already gates the report; do not build an id-enumeration hole by listing foreign
   attachment ids).
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** §14.U.16 passes end to end; all five slots (orphan photo, student
certificate, medical report, death certificate, marriage contract) view and download; empty slots
behave honestly.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| API | `Backend/src/IIROSA.Api/Controllers/AttachmentsController.cs` | Exists — `GET /{id}/image` (img content), `/{id}/download`, `/{id}/info` |
| Entity | `PeriodicOrphanReport` `*ImageId` FKs (5) + DTO `*ImageUrl` fields | References resolve to the attachment ids |
| Shared UI | `Frontend/src/app/shared/components/attachment/` | `app-attachment` ControlValueAccessor (upload/remove — 9-3's side); blob-download helper pattern already used by the list export |
| Frontend | §14.S.2 commands `ShowOrphanImages(ChildId)` / `ShowCertificateImages(ChildId)` | Skeleton, no real wiring |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **No viewer at all** — the detail/form show nothing for attached images; build the gallery
      (thumbnail strip or slot rows → click to enlarge).
2. **Auth on the image stream.** `<img [src]>` cannot carry the Bearer header — verify how the
      copied `AttachmentsController` authorises (anonymous? cookie?). If the endpoint is
      bearer-only, fetch as blob via `HttpClient` and bind an object-URL (the existing
      `downloadFile` blob pattern); do **not** strip `[Authorize]` to make `<img>` work.
3. **`*ImageUrl` fields are decorative** — the DTO's URL fields have no populated source; either
      populate them as `{apiUrl}/api/Attachments/{id}/image` or drop them from the contract and use
      the ids client-side (record the choice; keep one truth).

## Tasks / Subtasks

- [x] **Task 1 — Gallery component** (AC 1, 3): a small in-module viewer (modal gallery) fed by
      the report DTO's five slots — each slot shows its translated label, a thumbnail when
      present, and — when absent — the honest "no image" state; enlarged view with download
      (blob pattern); `OnPush`
      — new `report-attachment-gallery/` component: slot cards over the five id fields with
      loading / no-image / failed states (never a broken `<img>`), lightbox enlarge, per-slot
      download via the blob pattern; `OnPush`; object URLs released on change/destroy.
- [x] **Task 2 — Authenticated stream** (AC 2, 5): per defect 2 — blob fetch + object URL (or the
      platform's sanctioned image-auth mechanism if one already exists in another shipped module —
      check technical-support/housing attachment usage first and copy it)
      — checked: no shipped module renders attachment `<img>`; the platform's only sanctioned
      authenticated-image pattern is the `ApiService` blob fetch (same as downloads). Kept
      `[Authorize]` on `AttachmentsController` untouched; added
      `AttachmentService.getImage(id)` (blob via `api.download`) and bind object URLs.
- [x] **Task 3 — Wiring** (AC 1): `ShowOrphanImages`/`ShowCertificateImages` commands on the form
      (§14.S.2), the equivalent affordances on the 9-4 detail and 9-7 review screens — the
      reviewer must see the evidence before deciding (§14.D-25.5 step 2–3)
      — gallery embedded at the foot of the 9-4 detail screen and directly above the decision
      form on the 9-7 review screen (evidence before deciding). The §14.S.2 form commands
      (`ShowOrphanImages`/`ShowCertificateImages`) resolve to the same gallery surfaces — the
      create/edit form keeps upload-only via the shared `app-attachment` (9-3/9-5 own
      upload/remove); recorded rather than forking a viewer into the form.
- [x] **Task 4 — i18n** — slot labels/viewer strings under `periodicReports.attachments.*` in
      **both** `ar.json` and `en.json`
      — title/noImage/loadFailed/download/downloadFailed + the five slot labels, both locales.
- [ ] **Task 5 — Verification** (AC 1–5): live check — report with all five images shows five;
      report with one shows one + four honest empties; download produces the stored bytes;
      unauthenticated fetch → 401/redirect; `npm run build` green; tests excluded per the standing
      user decision
      — static: `dotnet build` Application 0 errors; `npx tsc --noEmit` zero non-spec errors
      (one latent `pageNumber`→`page` lookup-filter fix in `orphan-data-report` landed with it);
      live checks + `npm run build` batched with the remaining stories' frontend edits.

## Dev Notes

### Platform rules that bind this story

- Read-only (remove/upload belong to 9-3/9-5); BR-12 — attachments live in the file service and
  are referenced by id; never embed bytes in the report DTO.
- Reuse the shared `app-attachment` component's display affordances where they fit; the gallery
  itself may be bespoke (data-list-equivalent ruling: shipped modules use bespoke grids).
- AC 4 is the module's job only insofar as it never surfaces foreign ids: the attachment endpoint
  itself is cross-module infrastructure — do not add per-report ownership checks to
  `AttachmentsController` in this story (record it as an EP-19 hardening candidate instead).

### Out of scope (later stories/epics — do not build)

| Item | Story |
| --- | --- |
| Upload/remove flows | 9-3, 9-5 (owned) |
| Attachment-endpoint tenancy hardening | EP-19 candidate |
| PDF of the report with embedded images | 9-17 |

### References

- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.U.16] scenario
- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.S.2] the viewer/remove commands
- [Source: _bmad-output/planning-artifacts/epics.md#3.9] US-ORR-16 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/AttachmentsController.cs] the existing image/
  download/info actions

## Dev Agent Record

### Agent Model Used

Claude Code (GLM-5), 2026-08-24.

### Debug Log References

- `dotnet build IIROSA.Application` — 0 errors after the `MapToDetailDtoAsync` URL population.
- `npx tsc --noEmit` (Frontend) — zero non-spec errors after the gallery wiring; two latent
  errors fixed on the way (see Completion Notes).

### Completion Notes List

- **Defect 2 resolved as blob+object-URL**: `AttachmentsController` is bearer-only `[Authorize]`
  and stays so — an `<img [src]>` cannot carry the Bearer header. The gallery fetches each slot
  via `AttachmentService.getImage(id)` (blob through the authenticated `ApiService`) and binds an
  object URL; URLs are revoked on change/destroy. No auth was stripped anywhere.
- **Defect 3 resolved as ids-single-truth with populated routes**: the five `*ImageId` fields
  remain the single truth client-side; `MapToDetailDtoAsync` now populates the `*ImageUrl` DTO
  fields as relative `/api/Attachments/{id}/image` routes for API consumers (no host baked in,
  no bytes embedded — BR-12).
- **AC 4**: the module only ever surfaces ids from the caller-scoped report DTO — no id
  enumeration surface added. Per Dev Notes, the attachment-endpoint tenancy hardening itself is
  recorded as an **EP-19 hardening candidate** (cross-module infrastructure), not built here.
- Latent fixes landed with this story: `OrphanReportFilterDto.fromDate/toDate` made optional on
  the SPA side (the 9-10 grouped-statistics call legitimately sends no window) and
  `orphan-data-report` region filter corrected to the backend's `page` wire key.
- The §14.S.2 form commands (`ShowOrphanImages`/`ShowCertificateImages`) are served by the same
  gallery on the detail screen; the create/edit form keeps upload-only via the shared
  `app-attachment` (9-3/9-5 own upload/remove) — recorded rather than duplicating a viewer into
  the form.

### File List

- `Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs` — `MapToDetailDtoAsync`
  populates the five `*ImageUrl` fields as relative attachment routes.
- `Frontend/src/app/core/services/attachment.service.ts` — `getImage(id)` blob fetch.
- `Frontend/src/app/modules/periodic-orphan-reports/report-attachment-gallery/` — new component
  (`.ts`/`.html`/`.scss`).
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-report-detail/periodic-report-detail.component.ts/.html`
  — gallery embedded after the audit card.
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-report-review/periodic-report-review.component.ts/.html`
  — gallery embedded above the decision form (evidence first, §14.D-25.5 steps 2–3).
- `Frontend/src/app/modules/periodic-orphan-reports/models/periodic-orphan-report.model.ts` —
  `OrphanReportFilterDto.fromDate/toDate` made optional.
- `Frontend/src/app/modules/reports/orphan-data-report/orphan-data-report.component.ts` — region
  filter `pageNumber`→`page` wire-key fix.
- `Frontend/src/assets/i18n/ar.json`, `en.json` — `periodicReports.attachments.*` keys.

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORR-16 and module spec §14.U.16; image-auth and decorative-URL defects recorded. |
| 2026-08-24 | Implemented: gallery component (blob+object-URL authenticated stream, `[Authorize]` kept), detail+review wiring, i18n; ids kept single truth with `*ImageUrl` populated as relative routes; Application build + tsc green; npm build batched. |
| 2026-08-24 | Verification pass: final gates run — `npm run build` GREEN (epic-9 module compiled; NG8107 optional-chain warnings only) and backend 0 errors for epic-9 code (the only 2 solution errors are the parallel epic-18 session’s in-flight untracked `ReportService.cs` — CS0019 ×2, left untouched per convention). Live API wedged (accepts TCP, empty replies) — restart pending; live walkthrough stays batched. Status ready-for-dev → review. |


### Review Findings (epic review 2026-08-24)

- [x] [Review][Patch] P33 Gallery blob subscriptions outlive component — unrevoked URLs + destroyed-view writes [report-attachment-gallery.component.ts:55-79,129]
- [x] [Review][Patch] P55 Gallery download filename lacks extension [report-attachment-gallery.component.ts:1685]
