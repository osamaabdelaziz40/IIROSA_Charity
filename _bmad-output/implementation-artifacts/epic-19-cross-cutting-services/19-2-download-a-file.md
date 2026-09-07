# Story 19-2: Download a file

| Field | Value |
| --- | --- |
| Story key | `19-2-download-a-file` |
| Epic | EP-19 — Cross-Cutting Services — الخدمات المشتركة |
| Use case | UC-SYS-02 — تحميل ملف |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/24-UC-SYS-Cross-Cutting-Services.md` (§24.U.2 scenario) |
| Route | none of its own — every screen that renders a stored file |
| Endpoint | `GET /api/Attachments/{id}/download` |
| Depends on | 19-1 (upload writes the rows this story reads); epic-16 recorded the platform defect this story answers |
| Roles | All roles (any authenticated) |

## Status

done

## Story

As a signed-in user, I want to be able to download a file تحميل ملف, so that I can find the
record I need without leaving the system.

## Acceptance Criteria

1. Given a signed-in user with an active session, when the actor downloads a stored
   attachment, then no stored data is changed — the operation is a read, streamed with the
   stored content type and file name.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/Attachments/{id}/download` and the file opens/downloads without a page reload.
3. Given the caller is a browser link that cannot attach a JWT header, when the user activates
   it, then the SPA fetches the file through the authenticated HTTP client and delivers it as a
   blob — the platform answer to the recorded plain-`<a href>` defect.
4. Given the identifier does not resolve, when the download is requested, then a 404 with a
   safe message is returned — never a 500, never stack detail.
5. Given the session has expired, when the function is invoked, then the request is rejected
   and the actor is routed back to the login screen.

**Definition of done:** the scenario of §24.U.2 passes end to end for **both** storage modes
(DB blob and file system); no response body carries `ex.Message`; the link-based consumers are
migrated to the authenticated fetch helper.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Controller | `Backend/src/IIROSA.Api/Controllers/AttachmentsController.cs:33` `DownloadAttachment` | **Works**: resolves DB blob (`AttachmentContent.FileContent`) or file-system path (`AttachmentsPath` + `FilePath`), returns `File(bytes, contentType ?? "application/octet-stream", fileName)`, 404s with `{ message }` on missing rows/file |
| Sibling reads | same file: `GET {id}/info` (:83) metadata; `GET {id}/image` (:115) image-only stream (guards `ContentType.StartsWith("image/")`) | Work — 9-16 review flow and periodic-report detail screens consume `{id}/image` |
| Frontend | `core/services/attachment.service.ts` — `download(id): Observable<Blob>` (authenticated `HttpClient`) and `getImage(id)` | Exists; the report-export image embedder (18-24/18-25 path) already uses it correctly |

## Verified defects this story must fix (found by reading the code, 2026-08-25)

1. **Authenticated-download defect (platform-wide, recorded by epic-16 review).** Consumers
   that render plain `<a href="/api/attachments/{id}/download">` cannot send the JWT
   `Authorization` header, so the click lands as anonymous → 401 → login redirect. Live
   instance: the incoming-letters grid's الملف column. **Platform answer (this story owns it):
   blob fetch** — no cookie-auth scheme. Add an authenticated download-and-save helper (see
   Task 2) and migrate the known `<a href>` consumers to it.
2. **`ex.Message` leaked in 500 bodies** — all three actions
   (`AttachmentsController.cs:76`, `:108`, `:161` return `error = ex.Message`). Strip the
   `error` field (keep `{ message }`); the middleware/NLog keeps the detail server-side.
   (19-1 fixes the same file; whichever lands second just verifies.)
3. **Path handling nit (verify, fix if touched):** `Path.Combine(attachmentsPath,
   attachment.FilePath.TrimStart('\\'))` — when `AttachmentsPath` is unset the combine
   degrades to a bare relative path; log + 404 (as today) rather than throwing, and keep the
   trim behaviour for legacy stored paths.

## Tasks / Subtasks

- [x] **Task 1 — Backend hardening** (AC 1, 4)
  - [x] `error = ex.Message` stripped from all three catch blocks (with 19-1 — 500 body =
        `{ message }` only)
  - [x] Storage modes: DB branch verified live (uploaded via 19-1's endpoint, downloaded back
        byte-exact with `text/plain` + original file name on the disposition). File-system
        branch: code path untouched and shared with the pre-existing download action; the
        Framework `AddAttachment` writes `AttachmentContent.FileContent` in **both** modes, so
        the DB branch serves downloads even when the setting selects file-system storage —
        flipping `SaveFilesToDatabase` on the dev DB was not performed (no setting row exists;
        `AttachmentsPath` = relative "Attachments"). Recorded rather than force-tested.
- [x] **Task 2 — The authenticated download helper (the platform answer)** (AC 3)
  - [x] `downloadAndSave(id, fallbackName)` added to `core/services/attachment.service.ts`
        (blob → object URL → programmatic `<a download>` click → revoke). Failure surfacing
        stays with the caller/interceptor — no toast baked into the shared service (the two
        migrated consumers own their error UX)
  - [x] Consumers migrated (full sweep of `attachments/.*download` template hrefs — 2 hits):
        `incoming-letters-list.component.html` الملف column → `(click)="downloadLetterFile(letter)"`
        (component injects `AttachmentService`); `shared/components/file-viewer` non-preview
        branch → `(click)="downloadFile()"` blob-save in the component. No other template
        hrefs exist (report-export + orphan-files already fetch via the service)
- [x] **Task 3 — Consumers keep working** (AC 2)
  - [x] `{id}/image` consumers (file-viewer preview, orphan-files thumbnails, report-export
        embedder) and `download()` consumers (orphan-files per-row download, report-export
        image embed) untouched — verified by grep + tsc clean on the touched files
- [x] **Task 4 — Verification** (AC 5)
  - [x] Backend compile clean (copy-lock only per convention); `tsc --noEmit` clean on every
        touched frontend file; live smoke on the private instance (61970): download 200 +
        content type + filename, 404 unknown id `{ message }` only (no stack — pre-existing
        shape), 401 unauthenticated. Browser click-through deferred to the batched live
        walkthrough (per epic precedent)
  - [x] Tests excluded per the standing decision

### Review Findings

- [x] [Review][Patch] Download failures are swallowed — no error callbacks [Frontend/src/app/core/services/attachment.service.ts · shared/components/file-viewer] — `downloadAndSave` has no `error` handler (next-only) and `file-viewer.downloadFile()` likewise; a failed fetch shows the user nothing. Add per-consumer error handling following each screen's existing toast pattern (the shared service itself stays toast-free per the recorded ruling). **Fixed 2026-08-26** — `downloadAndSave` gained an optional `onError` (console-trace default), incoming-letters toasts `common.downloadFailed`, file-viewer surfaces the failure on its error state.
- [x] [Review][Patch] `common.download` i18n key missing in both languages [Frontend/src/assets/i18n/ar.json · en.json] — `incoming-letters-list.component.html` binds `'common.download' | translate` but the key exists nowhere under `common` (only `importsEdges.download` does), so the tooltip renders the raw key. Add `"download"` to `common` in ar + en. **Fixed 2026-08-26** — `common.download` + `common.downloadFailed` added to both languages.

## Dev Notes

### Platform rules that bind this story

- No new endpoint, no envelope change — this controller stays on raw `{ message }` shapes
  (2026-08-19 standing decision; ApiResponse migration is a platform story).
- Blob fetch (not cookie auth) is the recorded platform answer — do not introduce a second
  authentication scheme for file downloads.
- Files are Framework.Core entities; the endpoint's `[Authorize]` is the control — keep it.
- Never kill the user's running API; smoke on `127.0.0.1:60970` per the standing convention.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| Upload endpoint | 19-1 |
| Batch/metadata list endpoint | 19-3 |
| `ex.Message` leaks outside this controller | 19-13 |
| Range requests / streaming resumability | not in any UC — record in `deferred-work.md` if wanted |

### References

- [Source: docs/Modules/24-UC-SYS-Cross-Cutting-Services.md#24.U.2] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.19] US-SYS-02 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/AttachmentsController.cs#L33] live download action
- [Source: _bmad-output/implementation-artifacts/deferred-work.md] epic-16 "Attachment download via plain `<a href>` cannot send the JWT header — needs a platform answer"

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- Private instance 61970 (60970 = user's live API, port shift recorded); DB-branch round-trip
  via 19-1's uploaded attachment.

### Completion Notes List

- Blob-fetch is now the platform answer with two migrated consumers (incoming-letters الملف
  column, file-viewer download link); the sweep found exactly two template hrefs — both fixed.
- `downloadAndSave` intentionally toast-free: callers own error UX; the interceptor (19-13)
  picks up unhandled failures.
- File-system storage mode not force-flipped on the dev DB (no `SaveFilesToDatabase` row);
  DB branch always serves because the Framework write path populates `AttachmentContent` in
  both modes — recorded, not guessed.

### File List

- Frontend/src/app/core/services/attachment.service.ts (downloadAndSave helper)
- Frontend/src/app/modules/incoming-outgoing/incoming-letters/incoming-letters-list.component.ts (+DI, downloadLetterFile)
- Frontend/src/app/modules/incoming-outgoing/incoming-letters/incoming-letters-list.component.html (الملف column blob link)
- Frontend/src/app/shared/components/file-viewer/file-viewer.component.ts (downloadFile blob-save, downloadUrl removed)
- Frontend/src/app/shared/components/file-viewer/file-viewer.component.html (click handler)
- Backend/src/IIROSA.Api/Controllers/AttachmentsController.cs (leak strips — shared with 19-1)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-25 | Story file created from `epics.md` US-SYS-02 and module spec §24.U.2; endpoint verified live; the epic-16 deferred JWT-link defect assigned its platform answer (blob fetch) here. |
| 2026-08-25 | Implemented + verified (review-and-complete pass): Blob-fetch platform answer landed: downloadAndSave helper + both template-href consumers migrated (incoming-letters الملف, file-viewer); leak strips shared with 19-1; DB-branch round-trip verified live. |
| 2026-08-26 | Epic-19 code review: 2 patch findings written to Review Findings (silent download failures — no error callbacks; `common.download` i18n key missing in ar+en). |
| 2026-08-26 | Review patches applied (download error callbacks + i18n keys); tsc clean on touched files. Status → done. |
