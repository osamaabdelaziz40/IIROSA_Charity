# Story 19-3: Save a generated report file

| Field | Value |
| --- | --- |
| Story key | `19-3-save-a-generated-report-file` |
| Epic | EP-19 — Cross-Cutting Services — الخدمات المشتركة |
| Use case | UC-SYS-03 — حفظ ملف التقرير |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/24-UC-SYS-Cross-Cutting-Services.md` (§24.U.3 scenario) |
| Route | none — the reports module's export engine produces the file; this story adds the attachment list read the board names |
| Endpoint | `GET /api/Attachments` (batch metadata read — re-cut ruling below) |
| Depends on | EP-18 export engine (18-40/18-41, in progress); 19-1 upload (storage writer) |
| Roles | System → the endpoints behind it are authenticated reads |

## Status

done

## Story

As a system, I want to be able to save a generated report file حفظ ملف التقرير, so that the
paper document the process depends on can be produced and filed.

## Acceptance Criteria

1. Given a report is generated, when the export command runs, then a printable/downloadable
   document has been produced **client-side** (ExcelJS workbook / browser print-to-PDF) — the
   recorded platform ruling; no server-side rendering exists on this stack and none is added.
2. Given a list of attachment identifiers is needed (report file manifests, detail screens),
   when the SPA asks, then it is served by a batch metadata read on `GET /api/Attachments`
   returning `{ id, fileName, contentType, fileSizeBytes, createdOn }` per row — same shape as
   `GET {id}/info`.
3. Given the selection returns no row (empty ids / no report data), when the export or the
   metadata read runs, then the actor is told that there is nothing to produce
   (`reports.nothingToExport` / empty array) rather than receiving an empty file or an error.
4. Given the session has expired or the role is not permitted, when the function is invoked,
   then the request is rejected and the actor is routed back to the login screen.

> **Re-cut ruling (recorded, do not "fix" toward the legacy doc):** §24.U.3 describes the
> legacy behaviour — persisting a rendered report to the server file system. This stack has a
> recorded platform decision (epics 9/10/18 context passes; `ReportsController.cs`'s own doc
> comment) that **report documents are produced client-side** — endpoints return JSON sheet
> payloads and the SPA renders/prints/downloads. Server-side report persistence is therefore
> **superseded**; the deliverable that remains from this UC is the attachment batch-metadata
> read the board names (`GET /api/Attachments`) plus the empty-selection guard.

**Definition of done:** the batch read serves the 18-24/18-25 image manifests (and any detail
screen that lists files); empty selections are refused with the localised message; nothing in
this story re-introduces server-side PDF generation.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Export engine | `Frontend/src/app/modules/reports/services/report-export.service.ts` | Exists — ExcelJS workbooks per report, image embedding via authenticated `attachmentService.download(id)` (up to 40 images, hyperlink fallback), browser download |
| Per-file reads | `AttachmentsController` `GET {id}/info` / `GET {id}/download` / `GET {id}/image` | Exist and live — but there is **no batch/list read**, so every manifest consumer must fetch rows one-by-one |
| Empty-guard | reports module i18n `nothingToExport` (added by epic-16) + the 18-40/18-41 engine guards | Exists — verify, do not rebuild |
| Print path | `POST /api/Reports/{reportKey}/export/pdf` returns JSON payloads; client prints | Recorded deviation — leave as is |

## Verified gaps this story must fix

1. **No `GET /api/Attachments` list/batch endpoint.** The board names it; the controller has
   only per-id routes. Manifest screens (18-24 orphan files, 18-25 certificates) and any
   file-listing detail need metadata for a set of ids in one call.
2. **One-by-one metadata fetching** — every current consumer loops `{id}/info`; N+1 round
   trips.

## Tasks / Subtasks

- [x] **Task 1 — Batch metadata endpoint** (AC 2, 4)
  - [x] `GET /api/Attachments?ids=<guid>{&guid…}` on `AttachmentsController`, cap 50 (400
        beyond, verified live with 51 valid guids): two new Framework reads —
        `GetAttachmentsMetadataAsync` (rows + type, no `AttachmentContent` include) and
        `GetAttachmentSizesAsync` (`DATALENGTH` projection, no bytes over the wire); returns
        the `info` anonymous shape per row; unknown ids omitted, not errored
  - [x] Raw envelope; `[Authorize]` the control; no `ex.Message` in failure bodies
- [x] **Task 2 — Frontend service method + consumer** (AC 2)
  - [x] `attachment.service.ts.getInfoBatch(ids)` — empty array short-circuits to `[]`
        client-side without a round trip
  - [x] Consumer hand-off recorded: the 18-24 orphan-files/certificates manifest (epic-18
        WIP) carries `attachmentId` + `fileName` **server-side on the report payload** — no
        per-id `{id}/info` loop exists to migrate (the anticipated N+1 defect never landed);
        first natural consumer is any future screen listing bare attachment ids. No artificial
        consumer wired (PRD §7 duplicate-capability bar).
- [x] **Task 3 — Empty-selection guard** (AC 3)
  - [x] Verified present: `reports.nothingToExport` fires across the reports export screens
        (excluded-orphans, missing-reports, beneficiary-family-details, missing-files, …)
        before any workbook opens
- [x] **Task 4 — Verification**
  - [x] Backend compile clean; live smoke on 61970: mixed known+unknown ids → 200 single row
        (unknown omitted); no ids → 200 `[]`; 51 ids → 400 `{"message":"A maximum of 50
        attachment ids per request is allowed"}`; unauthenticated → 401. Tests excluded per
        the standing decision

### Review Findings

- [x] [Review][Decision] Batch metadata read is tenant-unscoped [Backend/src/IIROSA.Api/Controllers/AttachmentsController.cs — GET /api/Attachments] — any authenticated user from any charity can read `{fileName, contentType, fileSizeBytes, createdOn}` for any attachment Guid they learn of; Framework `Attachment` rows carry no CharityId **by design** (tenancy lives on the owning record — recorded platform posture). **Resolved 2026-08-26 (user decision): option (a) — Guid-as-capability accepted.** Attachment Guids are unguessable; ids only ever reach a client through its own scoped owning-record payloads. Ruling recorded here and applies platform-wide to the sibling `{id}/download` / `{id}/info` / `{id}/image` actions; revisit only if attachment ids ever appear in cross-tenant surfaces (logs, exports, URLs shared between charities).

## Dev Notes

### Platform rules that bind this story

- **No server-side report rendering** — client-side export is the recorded platform decision;
  do not add PDF/Excel generation packages to the backend.
- Attachments are Framework.Core entities — read via the existing Framework service; no new
  entity, no charity scoping on the file rows themselves.
- Raw envelope stays on this controller (2026-08-19 standing decision).

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| Upload / download / ex.Message fixes | 19-1, 19-2 |
| The 41 report sheets themselves | EP-18 (18-2…18-41) |
| Attachment delete endpoint | deferred-work.md (no owning UC) |

### References

- [Source: docs/Modules/24-UC-SYS-Cross-Cutting-Services.md#24.U.3] scenario + §24.1 UC-SYS-03 row
- [Source: _bmad-output/planning-artifacts/epics.md#3.19] US-SYS-03 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/ReportsController.cs] doc comment — the client-side print ruling
- [Source: Frontend/src/app/modules/reports/services/report-export.service.ts] the export engine this story guards

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- Private instance 61970; cap proof used 51 valid distinct guids (earlier malformed-guid
  attempts hit model binding, not the cap — retested clean).

### Completion Notes List

- Batch endpoint + Framework metadata reads landed with 19-1 (shared controller/build);
  `getInfoBatch` short-circuits empty selections client-side.
- 18-24/18-25 manifests already receive `fileName` on the report payload — no N+1 loop exists;
  hand-off recorded instead of wiring an artificial consumer.

### File List

- Backend/src/IIROSA.Api/Controllers/AttachmentsController.cs (GET batch action — shared with 19-1)
- Backend/Framework/Framework.Core/SharedServices/Services/AttachmentService.cs (GetAttachmentsMetadataAsync + GetAttachmentSizesAsync)
- Frontend/src/app/core/services/attachment.service.ts (getInfoBatch)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-25 | Story file created from `epics.md` US-SYS-03 and module spec §24.U.3; re-cut to the platform's client-side export ruling — deliverable is the batch metadata read + empty-selection guard; server-side report persistence ruled out. |
| 2026-08-25 | Implemented + verified (review-and-complete pass): batch metadata endpoint (cap 50, unknown-omit, DATALENGTH sizes) + getInfoBatch; empty-selection guard verified present across the reports screens; 18-24 consumer hand-off recorded (payload already carries fileName). |
| 2026-08-26 | Epic-19 code review: 1 decision-needed finding written to Review Findings (batch metadata read is tenant-unscoped — Guid-as-capability vs scoping is a product call). |
