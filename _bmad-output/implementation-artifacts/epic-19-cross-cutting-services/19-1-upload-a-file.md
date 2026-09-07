# Story 19-1: Upload a file

| Field | Value |
| --- | --- |
| Story key | `19-1-upload-a-file` |
| Epic | EP-19 — Cross-Cutting Services — الخدمات المشتركة |
| Use case | UC-SYS-01 — رفع ملف |
| Priority / size | Should · 8 points |
| Specification | `docs/Modules/24-UC-SYS-Cross-Cutting-Services.md` (§24.U.1 scenario) |
| Route | none of its own — consumed through every host form's attachment field |
| Endpoint | `POST /api/Attachments/upload` (board shorthand `POST /api/Attachments` — route ruling below) |
| Depends on | EP-01 auth (live); feeds epic-14 support tickets (recorded deferral) and any standalone upload consumer |
| Roles | Charity + HQ (any authenticated role) |

## Status

done

## Story

As a charity user, I want to be able to upload a file رفع ملف, so that data produced outside
the system is carried in without manual re-keying.

## Acceptance Criteria

1. Given a charity user with an active session, when the actor uploads a valid file, then the
   system stores it (per the configured storage mode), classifies it by attachment type and
   returns the identifier the calling record stores as its attachment reference.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Attachments/upload` as multipart content and the response is rendered on the
   screen without a page reload.
3. Given the uploaded file fails validation (empty, oversize, disallowed extension), when the
   upload runs, then **nothing is stored**, the request is refused with a field-level reason,
   and the actor is told why.
4. Given the file stores successfully, when the response returns, then it carries the new
   attachment's `id`, `fileName`, `contentType` and `fileSizeBytes` — the same shape as
   `GET {id}/info`.
5. Given the session has expired, when the function is invoked, then the request is rejected
   and the actor is routed back to the login screen.

> **AC interpretation note (generator wording):** epics.md's US-SYS-01 template speaks of
> "matched/unmatched rows" (the generic import-a-file template). §24.U.1 is the governing
> contract (epic-18 precedent: §x.U wins over the generator's type template) — this use case is
> a multipart photo/document upload, and "unmatched rows reported" maps to per-file validation
> refusals (AC 3). No workbook/CSV row-import belongs here.

**Definition of done:** the scenario of §24.U.1 passes end to end; storage respects the
configured mode (DB blob vs file system); size/type limits are enforced server-side; the role
check is the endpoint's `[Authorize]`, not a menu.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Controller | `Backend/src/IIROSA.Api/Controllers/AttachmentsController.cs` | **Read-only today**: `GET {id}/download`, `GET {id}/info`, `GET {id}/image` (:33, :83, :115). **No POST exists** — the upload action lived only in `AttachmentsController.cs.bak.bak2` |
| Service | `Framework/Core/Framework.Core/SharedServices/Services/AttachmentService.cs` | **Exists, complete**: `AddAttachment(IFormFile file, string title, string contentType, int? attachType)` → `ReturnResult<Attachment>`; dual storage (DB blob in `AttachmentContent.FileContent`, or file system `{AttachmentsPath}\{Year}\{Month}\{Day}\…`) chosen by `AppSettingsService.SaveFilesToDatabase`; thumbnails; remove paths |
| App service | `IIROSA.Application/Services/AttachmentHelperService.cs` (`IAttachmentHelperService`) | **Exists** — the base64-in-DTO path (`SaveAttachmentAsync(AttachmentDto, existingAttachmentId, attachmentTypeId)`) used live by `CharityService` (charity icon) and `OfficeProjectService` (project images). **Do not touch this path** |
| Entities | `Framework.Core/SharedServices/Entities/Attachment.cs` (+ `AttachmentContent`, `AttachmentType`) | Exist — attachments live in Framework.Core, **not** IIROSA.Domain; `AttachmentType` carries `AllowedFilesExtension`, `MaxSizeInMegabytes`, `ImageMaxHeight/Width` |
| Config | `IIROSA.Api/Program.cs:127-132` (`FormOptions` limits = `int.MaxValue`); `appsettings.json` `"UploadPath": "Uploads"` | Exists — the transport allows any size; the *policy* limits come from `AppSettingsService` (`AttachmentsMaxSize`, `AttachmentsAllowedTypes`, dims) read from the `SystemSetting` table |
| Frontend | `core/services/attachment.service.ts` — `upload(file, moduleName?, recordId?)` already POSTs to `/api/attachments/upload`; `shared/components/attachment-input` (base64 emitter for forms) | Service call exists but **404s today** — the backend action is missing |

## Verified defects this story must fix (found by reading the code, 2026-08-25)

1. **The upload endpoint does not exist.** `attachment.service.ts.upload()` posts to
   `/api/attachments/upload` and gets a 404 — the action was dropped when the controller was
   recopied (only the `.bak.bak2` still has it). Re-add it as a **typed multipart action** on
   `AttachmentsController` (route ruling below).
2. **Policy limits are dead config.** `AppSettingsService.AttachmentsMaxSize` /
   `AttachmentsAllowedTypes` / image dims are loaded but checked **nowhere**. With
   `FormOptions` at `int.MaxValue` the API accepts arbitrarily large files. Enforce in the new
   action (or a thin validator it calls): empty file → 400; size over `AttachmentsMaxSize`
   MB → 400; extension not in `AttachmentsAllowedTypes` (when that setting is populated) → 400.
   Every refusal carries a machine-readable field key + localisable message, per the
   FluentValidation convention (this controller has no service-layer validator — validate in a
   small private guard or an `Application/Validators` validator invoked before
   `_attachmentService.AddAttachment`).
3. **500 bodies leak `ex.Message`.** All three existing catch blocks return
   `error = ex.Message` (:76, :108, :161). Strip the `error` field from the catch bodies this
   story touches (the middleware logs the detail; the client gets `{ message }` only) — the
   platform-wide sweep is 19-13; this file is in epic-19 ownership, so fix it here.
4. **`AttachmentsController` inherits `ControllerBase`, not the platform `ApiController`** —
   recorded platform deferral (epics 3/5/17). Do **not** re-base this controller in this story
   (its `[Route]/[Authorize(JwtBearer)]]` shape is the file's live idiom); 19-13 decides the
   platform sweep.

**Route ruling.** Board says `POST /api/Attachments`; the live frontend contract (and the
`.bak.bak2` precedent) is `POST /api/Attachments/upload`. Take **`POST /api/Attachments/upload`**
and record the deviation — a literal `/upload` segment also keeps the route unambiguous against
`GET {id}/download`-style templates.

## Tasks / Subtasks

- [x] **Task 1 — The upload action** (AC 1, 2, 4)
  - [x] `POST /api/Attachments/upload` on `AttachmentsController`: `IFormFile file`
        (model-bound); calls the existing `AttachmentService.AddAttachment(file, fileName,
        contentType)`; returns 200 with `{ id, fileName, contentType, fileSizeBytes, createdOn }`
        (mirrors the `{id}/info` anonymous shape — raw envelope per the 2026-08-19 standing
        decision)
  - [x] Handles both storage modes via the Framework service (no storage code in the
        controller); `ReturnResult` failure → 400 with an `errors` dictionary
- [x] **Task 2 — Validation + limits** (AC 3)
  - [x] Empty file / no file → 400; oversize (`AttachmentsMaxSize` MB) → 400; disallowed
        extension (`AttachmentsAllowedTypes`, when populated) → 400; image dims
        (`AttachmentsAllowedWidth/Height`, when configured + image content type) → 400 with
        invalid-image rejection on decode failure. Every refusal carries
        `{ message, errors.file }`; nothing persisted on refusal
- [x] **Task 3 — Frontend contract** (AC 2, 4)
  - [x] `attachment.service.ts.upload()` verified matching — `FormData` with `file`; no
        service change needed. End-to-end proof delivered by the live smoke battery (below)
        instead of a spec (tests excluded per the standing decision). The `attachment-input`
        base64 path untouched (separate controller-less flow — no regression surface)
- [x] **Task 4 — Verification** (AC 5)
  - [x] `dotnet build` — 0 compile errors (MSB3021/3027 copy-lock from the user's live API
        only, per convention); smoke instance on a private port (`127.0.0.1:61970` — 60970 is
        now the user's live API): upload 200 `{id, fileName, contentType, fileSizeBytes: 20,
        createdOn}`; unauthenticated → 401; no file → 400; download of the uploaded id returns
        the exact bytes (DB-content branch — `AttachmentContent.FileContent` always written by
        the Framework service)
  - [x] Tests excluded per the standing decision

### Review Findings

- [x] [Review][Patch] GDI+ `OutOfMemoryException` escapes the image-dimension guard → 500 [Backend/src/IIROSA.Api/Controllers/AttachmentsController.cs] — `Image.FromStream` throws `OutOfMemoryException` (not only `ArgumentException`) on corrupt bytes with an image extension/content-type; the guard maps only `ArgumentException` → invalid-image 400, so a corrupt "image" reaches the generic 500 path. Catch both → 400 `{ message, errors.file }`. **Fixed 2026-08-26** — second catch added beside the ArgumentException one; build 0 errors.
- [x] [Review][Defer] System.Drawing is Windows-only and the stored contentType is client-supplied [Backend/Framework/Framework.Core/SharedServices/Services/AttachmentService.cs] — deferred, pre-existing framework posture (host is Windows; the extension allow-list is the real gate); revisit only if the platform moves to Linux containers.

## Dev Notes

### Platform rules that bind this story

- **Reuse, do not fork**: the Framework `AttachmentService` is the only storage writer. No new
  attachment entity, no new storage path, no second upload service.
- **Raw envelope stays** on this controller (`{ message }` / anonymous shapes) — the
  ApiResponse migration is a platform story, not this one.
- Attachments are Framework.Core entities with **no CharityId** — tenancy lives on the owning
  record (charity icon, project image, report image), not on the file. The upload endpoint is
  open to authenticated roles by design; do not invent a charity column.
- `.bak.bak2` files are known debt — read for reference only, never restore or edit them.
- Never kill the user's running `IIROSA.Api`; smoke on `--no-build --urls 127.0.0.1:60970`.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| `DELETE /api/Attachments/{id}` (also missing vs frontend `delete()`) | record in `deferred-work.md`; no epic-19 UC names it |
| Support-ticket controller attachment actions | epic-14 deferral |
| `ex.Message` leaks in other controllers / middleware | 19-13 |
| Server-side report persistence | 19-3 (ruled out — client-side rendering platform decision) |

### References

- [Source: docs/Modules/24-UC-SYS-Cross-Cutting-Services.md#24.U.1] scenario — multipart upload, classification, identifier returned
- [Source: _bmad-output/planning-artifacts/epics.md#3.19] US-SYS-01 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/AttachmentsController.cs] live controller — 3 GET actions, no POST
- [Source: Backend/Framework/Framework.Core/SharedServices/Services/AttachmentService.cs] `AddAttachment(IFormFile…)` — the writer to call
- [Source: Frontend/src/app/core/services/attachment.service.ts] `upload()` — the 404ing consumer contract
- [Source: _bmad-output/implementation-artifacts/deferred-work.md] epic-14 "No attachment endpoints on the controller" deferral

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- Smoke instance: private port `127.0.0.1:61970` (60970 is now the user's live API — port
  shift recorded), built to `src/IIROSA.Api/bin/Smoke` to bypass the live API's output lock.
- Battery: upload 200 (info shape, fileSizeBytes 20) · upload unauth 401 · upload no-file 400
  (model binding) · download of the uploaded id → exact bytes, `text/plain`.
- Dev DB `common.SystemSetting` holds only `AttachmentsPath`/`AttachmentsServer` — no
  `AttachmentsMaxSize`/`AttachmentsAllowedTypes`/dims rows, so the limit guards are live code
  but dormant until configured (0/empty = unrestricted by design).

### Completion Notes List

- Upload implemented in the controller (Framework service unchanged for the write path); limits
  enforced pre-write: size (MB), extension allow-list, image dims + invalid-image decode guard.
- `ReturnResult` failure path → 400 `{ message, errors }`; the three pre-existing catch blocks
  no longer return `error = ex.Message` (500s carry `{ message }` only; detail stays in logs).
- `AttachmentService.AddAttachment` always writes `AttachmentContent.FileContent` (mode only
  decides the additional file-system copy) — DB branch serves downloads in both modes.
- Route ruling honoured: `POST /api/Attachments/upload`.
- Tests excluded per the standing decision; verification = build + live smoke battery above.

### File List

- Backend/src/IIROSA.Api/Controllers/AttachmentsController.cs (upload action + batch action + leak strips)
- Backend/Framework/Framework.Core/SharedServices/Services/AttachmentService.cs (metadata-batch reads for 19-3)
- Frontend/src/app/core/services/attachment.service.ts (downloadAndSave + getInfoBatch — 19-2/19-3, shared file)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-25 | Story file created from `epics.md` US-SYS-01 and module spec §24.U.1; copied-code state audited (missing POST endpoint, dead size/type config, ex.Message leaks recorded); route ruling `POST /api/Attachments/upload`. |
| 2026-08-25 | Implemented + verified (review-and-complete pass): upload action with pre-write limit guards, ReturnResult 400 mapping, catch-block leak strips; live smoke battery green on the private instance (61970). |
| 2026-08-26 | Epic-19 code review: 1 patch (GDI+ OOM escapes the image guard → 500) + 1 defer (System.Drawing platform posture) written to Review Findings. |
| 2026-08-26 | Review patch applied: OutOfMemoryException joined the invalid-image catch — corrupt "images" now 400, never 500. Build 0 errors. Status → done. |
