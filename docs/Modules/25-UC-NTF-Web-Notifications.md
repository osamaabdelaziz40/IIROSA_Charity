# WAR.IIROSA - Web Notifications

الإشعارات | use case prefix `UC-NTF` | new-platform capability (no WAR.IIROSA chapter)


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Web Notifications |
| Module | Web Notifications — الإشعارات |
| Use case prefix | UC-NTF |
| Chapter in master document | none — new capability requested for the re-platform |
| Documented use cases | 7 |
| Principal routes | `#/notifications`, `#/notifications/create`, `#/notifications/:id/edit` (all delivered) |
| Version | 1.0 |
| Status | Implemented on the new stack (.NET 8 + Angular 18 + SignalR) |
| Date | 10 September 2026 |
| Parent document | architecture.md (authoritative) |

## Contents of this document

1. Module purpose and use-case catalogue
2. §25.S — screen field specifications
3. §25.U — one expanded scenario per use case
4. §25.A — annex: Web API controller and realisation notes

## 25. Web Notifications

الإشعارات — Admin and SuperAdmin compose a web notification (title, message) and push it live over SignalR to a chosen audience: specific users and/or whole charities (every active user of each selected charity). Every push is persisted in `NotificationsLog` (schema `IIROSA`) stamped with the seeded `common.NotificationType` row "Web". Every authenticated user has a notifications screen in the menu; recipients additionally receive a live toast from the layout on delivery.

| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-NTF-01 | List notifications قائمة الإشعارات | Admin, SuperAdmin | The full register of pushed notifications, newest first, with title, message, audience kind, send count and last-sent timestamp. Search by title/message; paged. | Route `#/notifications` → GET /api/Notification |
| UC-NTF-02 | View my notifications إشعاراتي | Any authenticated user | Rows whose audience contains the caller — directly (IsUser) or through their charity (IsCharity). Live-refreshed when a push arrives over SignalR. | Route `#/notifications` → GET /api/Notification/my |
| UC-NTF-03 | Compose and push a notification إرسال إشعار | Admin, SuperAdmin | Enter Title + Message, raise IsUser and/or IsCharity, pick the recipients (multi-select users and/or charities), send. The row is stored and pushed live in the same request. | Route `#/notifications/create` → POST /api/Notification |
| UC-NTF-04 | Edit a stored notification تعديل الإشعار | Admin, SuperAdmin | Amend the content or the audience of a stored notification. Editing does not push — resend does. | Route `#/notifications/:id/edit` → PUT /api/Notification/{id} |
| UC-NTF-05 | Resend a notification إعادة الإرسال | Admin, SuperAdmin | Re-push the stored content as-is to a freshly resolved audience (users added to a selected charity since the last push are included). SentCount and LastSentOn advance. | POST /api/Notification/{id}/resend |
| UC-NTF-06 | Load the audience catalogues تحميل المستهدفين | Admin, SuperAdmin | The user picker loads the identity store; the charity picker loads the charity register (active only). | GET /api/usermanagement, GET /api/Charities |
| UC-NTF-07 | Receive a live notification استلام الإشعار | Any authenticated user | On delivery, the layout toasts the notification (title + message) and the notifications grid refreshes if open. | SignalR `NotificationHub` event `ReceiveNotification` |

### 25.S  Screen field specifications

#### 25.S.1  Screen `#/notifications`

| Property | Value |
| --- | --- |
| Angular route | `#/notifications` |
| Feature module | `notifications` (lazy-loaded) |
| Component | `NotificationListComponent` |
| Data-entry fields | 1 (search) |
| Grids on this screen | 1 |
| Commands | Add Notification (HQ only), Edit, Resend (both HQ only) |

#### 25.S.2  Screen `#/notifications/create` and `#/notifications/:id/edit`

| Field | Bound control | Type | Mandatory | Rule |
| --- | --- | --- | --- | --- |
| العنوان (Title) | `title` | text | yes | ≤ 250 chars |
| الرسالة (Message) | `message` | textarea | yes | ≤ 4000 chars |
| مستخدمون محددون (IsUser) | `isUser` | checkbox | one of the two flags | — |
| جمعيات (IsCharity) | `isCharity` | checkbox | one of the two flags | — |
| المستخدمون (users) | `recipientUserIds` | multi-select (Select2) | yes while IsUser is raised | at least one user |
| الجمعيات (charities) | `recipientCharityIds` | multi-select (Select2) | yes while IsCharity is raised | at least one charity |

Commands: Send/Save, Resend (edit mode only), Cancel.

### 25.U  Expanded scenarios

**UC-NTF-03 — Compose and push.** Trigger: HQ actor opens إضافة إشعار. Pre-conditions: authenticated as Admin/SuperAdmin; `common.NotificationType` row "Web" seeded (runtime seed, identity column). Main flow: form validates (title, message, at least one target flag, each raised flag carries ≥ 1 recipient); POST /api/Notification persists the row (SentCount = 1, LastSentOn = now, recipients stored as CSV Guid lists), resolves the audience — direct user ids plus every active user of each selected charity — and pushes `{ id, title, message, type, timestamp }` over `IHubContext<NotificationHub>` to those users. Post-condition: every connected recipient gets the toast; the register shows the row. Alternates: a raised flag with no recipients → 400 with a per-field `errors` map the form flags; hub failure → logged, the stored row is unaffected.

**UC-NTF-05 — Resend.** Trigger: the resend command (edit screen or register row). Main flow: the service re-resolves the audience from the stored selection (so charity membership changes are picked up), bumps SentCount/LastSentOn, and the controller pushes the stored content. Declining the confirm dialog pushes nothing.

### 25.A  Annex — realisation notes

- `NotificationController` (`api/Notification`): class-level `[Authorize]`; register/detail/create/edit/resend carry `[Authorize(Roles = "Admin,SuperAdmin")]`; `my` is any authenticated caller. FluentValidation failures return `BadRequest(new { message, errors })` per the MissionManagementController idiom.
- Audience resolution and persistence live in `NotificationsLogService` (Application layer); only the controller touches `IHubContext` — the service stays hub-agnostic and returns `DeliveredToUserIds`.
- Delivery targets the hub's per-user group (`User_{userId}` = `Context.UserIdentifier` = the ApplicationUser id claim), matching `SignalRService` on the client.
- Recipient lists are stored as CSV Guid strings; the my-notifications read matches with a delimited `",<guid>,"` LIKE in SQL, so one id never matches a substring of its neighbour.
- `Notifications.View` admits every role; `Notifications.Create` / `Notifications.Edit` are SuperAdmin/Admin (frontend `PERMISSION_ROLES` mirrors the server's `[Authorize]` sets).
- Migration `20260910062938_AddNotificationsLog` creates `IIROSA.NotificationsLog`; applied automatically by `Database.Migrate()` at API start.
