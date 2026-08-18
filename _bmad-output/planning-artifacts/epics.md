# IIROSA Charities - Epics & User Stories

Delivery backlog for the whole system. Every functional module is one epic; every documented
use case is one user story with its own acceptance criteria. Story and use case identifiers are
aligned (`US-PAY-14` is `UC-PAY-14`), so the backlog and the specification stay traceable to each other.

Every story is delivered on the **approved architecture** — .NET 8 Clean Architecture + Angular 18,
fixed by `Architecture/01`-`04` and consolidated in `_bmad-output/planning-artifacts/architecture.md`.
A story is not done until it follows the build order Domain → Infrastructure → Application → API →
Frontend → Tests → Docs, reuses the three mandatory shared components (`data-list`, `input-fields`,
`attachment`), and authorises server-side.

> **Terminology (v1.2).** The tenant organisation is a **Charity** (الجمعية); "NGO" and "Association"
> are retired. EP-03 is now **Charity Administration** and its prefix changed from `UC-NGO` / `US-NGO`
> to `UC-CHR` / `US-CHR` — numbering unchanged, so `US-NGO-01` ≡ `US-CHR-01`.
>
> **Routing (v1.2).** Every endpoint and screen named in this backlog is this project's: the Angular 18
> route and the ASP.NET Core endpoint. The legacy AngularJS states, `/api/*` controllers, `/Print/*`
> actions and BLL classes have been replaced throughout. The conventions, the full crosswalk and the
> known gaps are in `docs/Modules/00-ROUTING-MAP.md`.


| Field | Value |
| --- | --- |
| Document title | IIROSA Charities - Epics & User Stories |
| Purpose | Delivery backlog derived from the use case catalogue: one epic per functional module, one user story per use case |
| Epics | 19 |
| User stories | 230 |
| Estimated story points | 838 (relative sizing, not a schedule) |
| Version | 1.2 |
| Date | 18 August 2026 |
| Target architecture | `_bmad-output/planning-artifacts/architecture.md` (authoritative), from `Architecture/01`-`04` |
| Delivery board | `_bmad-output/implementation-artifacts/sprint-status.yaml` |
| Specification | `docs/Modules/` — one module document per epic |
| Routing | `docs/Modules/00-ROUTING-MAP.md` — the Angular 18 route and API endpoint behind every story |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## 1. How this backlog is organised


| Level | Identifier | Meaning | Where it is specified |
| --- | --- | --- | --- |
| Epic | EP-nn | One functional module — a body of work with a single business goal | The module document of that chapter |
| User story | US-xxx-nn | One use case, expressed from the actor’s point of view | §<ch>.U of the module document |
| Acceptance criteria | Given / When / Then | The conditions under which the story is considered done | Derived from the scenario: post-conditions, exceptions and business-layer messages |
| Screen contract | §<ch>.S | The fields a story must implement, with their mandatory flags and lookups | The module document |

Priority uses MoSCoW. Points are a relative size (Fibonacci) taken from the kind of the use case and the number of fields on its screen — they size the work, they do not schedule it.

## 2. Epic summary


| Epic | Module | Ch. | Prefix | Stories | Points |
| --- | --- | --- | --- | --- | --- |
| EP-01 | Authentication & User Account | 6 | UC-AUT | 9 | 21 |
| EP-02 | Home Dashboard | 7 | UC-DSH | 3 | 7 |
| EP-03 | Charity Administration | 8 | UC-CHR | 9 | 36 |
| EP-04 | Employee & User Administration | 9 | UC-EMP | 6 | 18 |
| EP-05 | Family Register | 10 | UC-FAM | 14 | 68 |
| EP-06 | Housing Project | 11 | UC-HOU | 8 | 36 |
| EP-07 | Refugee Families | 12 | UC-REF | 4 | 19 |
| EP-08 | Orphan Register & Coding | 13 | UC-ORP | 11 | 31 |
| EP-09 | Orphan Periodic Reports | 14 | UC-ORR | 17 | 80 |
| EP-10 | Orphan Payments & Disbursement | 15 | UC-PAY | 24 | 95 |
| EP-11 | General Cheques | 16 | UC-CHQ | 10 | 35 |
| EP-12 | Seasonal Assistance Projects | 17 | UC-PRJ | 13 | 43 |
| EP-13 | Office Development Projects | 18 | UC-OFP | 6 | 22 |
| EP-14 | Technical Support | 19 | UC-CST | 6 | 19 |
| EP-15 | Missions | 20 | UC-MSN | 9 | 30 |
| EP-16 | Correspondence — Incoming & Outgoing | 21 | UC-COR | 19 | 54 |
| EP-17 | HQ Financial Transfers | 22 | UC-TRF | 8 | 28 |
| EP-18 | Reports & Printing | 23 | UC-RPT | 41 | 151 |
| EP-19 | Cross-Cutting Services | 24 | UC-SYS | 13 | 45 |

## 3. Epics

### 3.1  EP-01 — Authentication & User Account


| Item | Value |
| --- | --- |
| Epic | EP-01 |
| Module | Authentication & User Account — ادارة الدخول والحساب |
| Chapter / prefix | Chapter 6 · UC-AUT |
| Business goal | To establish who the caller is, with which role and in which country, because every other module is scoped by that answer. |
| Stories | 9 (9 Must) |
| Points | 21 |
| Routes | `#/auth/login`, `#/auth/register`, `#/auth/forgot-password`, `#/auth/reset-password/:userId/:code`, `#/auth/change-password` |
| Specification | Module document 06-UC-AUT-Authentication-and-User-Account — §6.S for the screen contract, §6.U for the scenarios |
| Depends on | Nothing — every other epic depends on this one. |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-AUT-01 | UC-AUT-01 | As a registered user, I want to be able to log in to the system تسجيل الدخول, so that only the right people reach the register and each of them sees only their own scope. | Authentication / credential handling | Must · 2 |
| US-AUT-02 | UC-AUT-02 | As a signed-in user, I want to be able to retrieve signed-in user profile بيانات المستخدم, so that I can see the full detail of a single record before acting on it. | Read a record | Must · 2 |
| US-AUT-03 | UC-AUT-03 | As a registered user, I want to be able to log out خروج, so that only the right people reach the register and each of them sees only their own scope. | Authentication / credential handling | Must · 2 |
| US-AUT-04 | UC-AUT-04 | As a prospective charity, I want to be able to register a new charity account تسجيل جمعية, so that the register reflects reality as soon as the fact is known. | Create a record | Must · 5 |
| US-AUT-05 | UC-AUT-05 | As a registered user, I want to be able to request a password-reset link نسيت كلمة المرور, so that only the right people reach the register and each of them sees only their own scope. | Authentication / credential handling | Must · 2 |
| US-AUT-06 | UC-AUT-06 | As a registered user, I want to be able to validate a password-reset link التحقق من رابط الاستعادة, so that only the right people reach the register and each of them sees only their own scope. | Authentication / credential handling | Must · 2 |
| US-AUT-07 | UC-AUT-07 | As a registered user, I want to be able to reset a forgotten password إعادة تعيين كلمة المرور, so that only the right people reach the register and each of them sees only their own scope. | Authentication / credential handling | Must · 2 |
| US-AUT-08 | UC-AUT-08 | As a registered user, I want to be able to change own password تغيير الرقم السرى, so that only the right people reach the register and each of them sees only their own scope. | Authentication / credential handling | Must · 2 |
| US-AUT-09 | UC-AUT-09 | As a system, I want to be able to enforce role-based navigation إظهار القوائم حسب الصلاحية, so that I can see the full detail of a single record before acting on it. | Read a record | Must · 2 |


**Acceptance criteria**


| Story | US-AUT-01 (UC-AUT-01) |
| --- | --- |
| User story | As a registered user, I want to be able to log in to the system تسجيل الدخول, so that only the right people reach the register and each of them sees only their own scope. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a registered user holding valid credentials on the screen at `#/auth/login`, when the actor presses «تسجيل الدخول» with valid input, then an authenticated session exists (or has been ended); the role code and country id that scope every later request are held by the SPA.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Auth/login` and the response is rendered on the screen without a page reload.<br>3. Given the credentials are wrong or the account is locked out, when the actor submits them, then the attempt fails, no ticket is issued and the actor stays on the screen. |
| Definition of done | The screen fields of §6.S are implemented with their mandatory flags and lookups; the scenario of §6.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-AUT-02 (UC-AUT-02) |
| --- | --- |
| User story | As a signed-in user, I want to be able to retrieve signed-in user profile بيانات المستخدم, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a signed-in user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Auth/me` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §6.S are implemented with their mandatory flags and lookups; the scenario of §6.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-AUT-03 (UC-AUT-03) |
| --- | --- |
| User story | As a registered user, I want to be able to log out خروج, so that only the right people reach the register and each of them sees only their own scope. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a registered user holding valid credentials in the module, when the actor invokes the function with valid input, then an authenticated session exists (or has been ended); the role code and country id that scope every later request are held by the SPA.<br>2. Given the credentials are wrong or the account is locked out, when the actor submits them, then the attempt fails, no ticket is issued and the actor stays on the screen. |
| Definition of done | The screen fields of §6.S are implemented with their mandatory flags and lookups; the scenario of §6.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-AUT-04 (UC-AUT-04) |
| --- | --- |
| User story | As a prospective charity, I want to be able to register a new charity account تسجيل جمعية, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Must · 5 points |
| Acceptance criteria | 1. Given a prospective charity with an active session on the screen at `#/auth/register`, when the actor presses «تسجيل» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Auth/register` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §6.S are implemented with their mandatory flags and lookups; the scenario of §6.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-AUT-05 (UC-AUT-05) |
| --- | --- |
| User story | As a registered user, I want to be able to request a password-reset link نسيت كلمة المرور, so that only the right people reach the register and each of them sees only their own scope. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a registered user holding valid credentials on the screen at `#/auth/forgot-password`, when the actor presses «تم» with valid input, then an authenticated session exists (or has been ended); the role code and country id that scope every later request are held by the SPA.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Auth/forgot-password` and the response is rendered on the screen without a page reload.<br>3. Given the credentials are wrong or the account is locked out, when the actor submits them, then the attempt fails, no ticket is issued and the actor stays on the screen. |
| Definition of done | The screen fields of §6.S are implemented with their mandatory flags and lookups; the scenario of §6.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-AUT-06 (UC-AUT-06) |
| --- | --- |
| User story | As a registered user, I want to be able to validate a password-reset link التحقق من رابط الاستعادة, so that only the right people reach the register and each of them sees only their own scope. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a registered user holding valid credentials on the screen at `#/auth/reset-password/:userId/:code`, when the actor presses «حفظ» with valid input, then an authenticated session exists (or has been ended); the role code and country id that scope every later request are held by the SPA.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Auth/validate-reset-token` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the credentials are wrong or the account is locked out, when the actor submits them, then the attempt fails, no ticket is issued and the actor stays on the screen. |
| Definition of done | The screen fields of §6.S are implemented with their mandatory flags and lookups; the scenario of §6.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-AUT-07 (UC-AUT-07) |
| --- | --- |
| User story | As a registered user, I want to be able to reset a forgotten password إعادة تعيين كلمة المرور, so that only the right people reach the register and each of them sees only their own scope. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a registered user holding valid credentials on the screen at `#/auth/reset-password/:userId/:code`, when the actor presses «حفظ» with valid input, then an authenticated session exists (or has been ended); the role code and country id that scope every later request are held by the SPA.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Auth/change-password` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the credentials are wrong or the account is locked out, when the actor submits them, then the attempt fails, no ticket is issued and the actor stays on the screen.<br>6. Given the business rule behind «Please Enter All Requird Data» is broken, when the operation is attempted, then it is refused with that message and nothing is written.<br>7. Given the business rule behind «Please Enter  valid password must contain All Type Characters and min Length 8» is broken, when the operation is attempted, then it is refused with that message and nothing is written.<br>8. Given the business rule behind «User Not Found» is broken, when the operation is attempted, then it is refused with that message and nothing is written.<br>9. Given the business rule behind «Password Changed» is broken, when the operation is attempted, then it is refused with that message and nothing is written.<br>10. Given the business rule behind «Password change failed. Please re-enter your values and try again» is broken, when the operation is attempted, then it is refused with that message and nothing is written.<br>11. Given the business rule behind «There Problem With Saving» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §6.S are implemented with their mandatory flags and lookups; the scenario of §6.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-AUT-08 (UC-AUT-08) |
| --- | --- |
| User story | As a registered user, I want to be able to change own password تغيير الرقم السرى, so that only the right people reach the register and each of them sees only their own scope. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a registered user holding valid credentials on the screen at `#/auth/change-password`, when the actor presses «حفظ» with valid input, then an authenticated session exists (or has been ended); the role code and country id that scope every later request are held by the SPA.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Auth/change-password` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the credentials are wrong or the account is locked out, when the actor submits them, then the attempt fails, no ticket is issued and the actor stays on the screen.<br>6. Given the business rule behind «Please Enter All Requird Data» is broken, when the operation is attempted, then it is refused with that message and nothing is written.<br>7. Given the business rule behind «Please Enter  valid password must contain All Type Characters and min Length 8» is broken, when the operation is attempted, then it is refused with that message and nothing is written.<br>8. Given the business rule behind «User Not Found» is broken, when the operation is attempted, then it is refused with that message and nothing is written.<br>9. Given the business rule behind «Password Changed» is broken, when the operation is attempted, then it is refused with that message and nothing is written.<br>10. Given the business rule behind «Password change failed. Please re-enter your values and try again» is broken, when the operation is attempted, then it is refused with that message and nothing is written.<br>11. Given the business rule behind «There Problem With Saving» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §6.S are implemented with their mandatory flags and lookups; the scenario of §6.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-AUT-09 (UC-AUT-09) |
| --- | --- |
| User story | As a system, I want to be able to enforce role-based navigation إظهار القوائم حسب الصلاحية, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a system with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §6.S are implemented with their mandatory flags and lookups; the scenario of §6.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.2  EP-02 — Home Dashboard


| Item | Value |
| --- | --- |
| Epic | EP-02 |
| Module | Home Dashboard — الصفحة الرئيسية |
| Chapter / prefix | Chapter 7 · UC-DSH |
| Business goal | To give each role an immediate picture of the work waiting for it as soon as it signs in. |
| Stories | 3 (2 Should, 1 Could) |
| Points | 7 |
| Routes | `#/dashboard` |
| Specification | Module document 07-UC-DSH-Home-Dashboard — §7.S for the screen contract, §7.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution) |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-DSH-01 | UC-DSH-01 | As a signed-in user, I want to be able to view beneficiary family count عدد الأسر, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-DSH-02 | UC-DSH-02 | As a signed-in user, I want to be able to view statistical breakdown charts الرسوم البيانية, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Could · 3 |
| US-DSH-03 | UC-DSH-03 | As a signed-in user, I want to be able to navigate to a functional module التنقل بين الوحدات, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |


**Acceptance criteria**


| Story | US-DSH-01 (UC-DSH-01) |
| --- | --- |
| User story | As a signed-in user, I want to be able to view beneficiary family count عدد الأسر, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a signed-in user with an active session on the screen at `#/dashboard`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Dashboard/summary` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §7.S are implemented with their mandatory flags and lookups; the scenario of §7.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-DSH-02 (UC-DSH-02) |
| --- | --- |
| User story | As a signed-in user, I want to be able to view statistical breakdown charts الرسوم البيانية, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a signed-in user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Dashboard/charts` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §7.S are implemented with their mandatory flags and lookups; the scenario of §7.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-DSH-03 (UC-DSH-03) |
| --- | --- |
| User story | As a signed-in user, I want to be able to navigate to a functional module التنقل بين الوحدات, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a signed-in user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §7.S are implemented with their mandatory flags and lookups; the scenario of §7.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.3  EP-03 — Charity Administration


| Item | Value |
| --- | --- |
| Epic | EP-03 |
| Module | Charity Administration — ادارة الجمعيات |
| Chapter / prefix | Chapter 8 · UC-CHR |
| Business goal | To onboard partner charities and control what each of them may add or change. |
| Stories | 9 (9 Should) |
| Points | 36 |
| Routes | `#/charities`, `#/charities/create` |
| Specification | Module document 08-UC-CHR-Charity-Administration — §8.S for the screen contract, §8.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution) |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-CHR-01 | UC-CHR-01 | As a general director, I want to be able to list all charities قائمة الجمعيات, so that I can find the record I need without leaving the system. | Browse a list | Should · 5 |
| US-CHR-02 | UC-CHR-02 | As a general director, I want to be able to verify charity name availability التحقق من اسم الجمعية, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-CHR-03 | UC-CHR-03 | As a general director, I want to be able to create a charity اضافة جمعية, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 8 |
| US-CHR-04 | UC-CHR-04 | As a general director, I want to be able to view a charity profile بيانات الجمعية, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-CHR-05 | UC-CHR-05 | As a general director, I want to be able to update a charity profile تعديل بيانات الجمعية, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |
| US-CHR-06 | UC-CHR-06 | As a general director, I want to be able to open the permissions screen الصلاحيات, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-CHR-07 | UC-CHR-07 | As a general director, I want to be able to lock or unlock a charity account إيقاف / تفعيل حساب الجمعية, so that head office can freeze a charity’s data entry when the cycle requires it. | Lock control | Should · 2 |
| US-CHR-08 | UC-CHR-08 | As a general director, I want to be able to enable or disable data entry for a charity السماح بالإضافة, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 5 |
| US-CHR-09 | UC-CHR-09 | As a general director, I want to be able to enable or disable data editing for a charity السماح بالتعديل, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |


**Acceptance criteria**


| Story | US-CHR-01 (UC-CHR-01) |
| --- | --- |
| User story | As a general director, I want to be able to list all charities قائمة الجمعيات, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a general director with an active session on the screen at `#/charities`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Charities` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §8.S are implemented with their mandatory flags and lookups; the scenario of §8.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CHR-02 (UC-CHR-02) |
| --- | --- |
| User story | As a general director, I want to be able to verify charity name availability التحقق من اسم الجمعية, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a general director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Charities/check-name` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §8.S are implemented with their mandatory flags and lookups; the scenario of §8.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CHR-03 (UC-CHR-03) |
| --- | --- |
| User story | As a general director, I want to be able to create a charity اضافة جمعية, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 8 points |
| Acceptance criteria | 1. Given a general director with an active session on the screen at `#/charities/create`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Charities` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §8.S are implemented with their mandatory flags and lookups; the scenario of §8.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CHR-04 (UC-CHR-04) |
| --- | --- |
| User story | As a general director, I want to be able to view a charity profile بيانات الجمعية, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a general director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Charities/{id}` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §8.S are implemented with their mandatory flags and lookups; the scenario of §8.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CHR-05 (UC-CHR-05) |
| --- | --- |
| User story | As a general director, I want to be able to update a charity profile تعديل بيانات الجمعية, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a general director with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/Charities/{id}` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §8.S are implemented with their mandatory flags and lookups; the scenario of §8.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CHR-06 (UC-CHR-06) |
| --- | --- |
| User story | As a general director, I want to be able to open the permissions screen الصلاحيات, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a general director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Charities` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §8.S are implemented with their mandatory flags and lookups; the scenario of §8.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CHR-07 (UC-CHR-07) |
| --- | --- |
| User story | As a general director, I want to be able to lock or unlock a charity account إيقاف / تفعيل حساب الجمعية, so that head office can freeze a charity’s data entry when the cycle requires it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a general director with an active session in the module, when the actor invokes the function with valid input, then the lock value is stored against the charity and is enforced on every later add/edit attempt.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Charities/{id}/lock · POST /api/Charities/{id}/unlock` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §8.S are implemented with their mandatory flags and lookups; the scenario of §8.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CHR-08 (UC-CHR-08) |
| --- | --- |
| User story | As a general director, I want to be able to enable or disable data entry for a charity السماح بالإضافة, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a general director with an active session in the module, when the actor invokes the function with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Charities/{id}/lock · POST /api/Charities/{id}/unlock` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §8.S are implemented with their mandatory flags and lookups; the scenario of §8.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CHR-09 (UC-CHR-09) |
| --- | --- |
| User story | As a general director, I want to be able to enable or disable data editing for a charity السماح بالتعديل, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a general director with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Charities/{id}/lock · POST /api/Charities/{id}/unlock` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §8.S are implemented with their mandatory flags and lookups; the scenario of §8.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.4  EP-04 — Employee & User Administration


| Item | Value |
| --- | --- |
| Epic | EP-04 |
| Module | Employee & User Administration — ادارة الموظفين |
| Chapter / prefix | Chapter 9 · UC-EMP |
| Business goal | To create and maintain head-office staff accounts and the roles attached to them. |
| Stories | 6 (6 Should) |
| Points | 18 |
| Routes | `#/user-management/users`, `#/user-management/users/:id`, `#/employees`, `#/employees/:id` |
| Specification | Module document 09-UC-EMP-Employee-and-User-Administration — §9.S for the screen contract, §9.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution) |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-EMP-01 | UC-EMP-01 | As a general director, I want to be able to list employees قائمة الموظفين, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-EMP-02 | UC-EMP-02 | As a general director, I want to be able to verify employee username availability التحقق من اسم المستخدم, so that only the right people reach the register and each of them sees only their own scope. | Authentication / credential handling | Should · 2 |
| US-EMP-03 | UC-EMP-03 | As a general director, I want to be able to create an employee account اضافة موظف, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 5 |
| US-EMP-04 | UC-EMP-04 | As a general director, I want to be able to view an employee record بيانات الموظف, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-EMP-05 | UC-EMP-05 | As a general director, I want to be able to update an employee and change their role تعديل بيانات الموظف, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |
| US-EMP-06 | UC-EMP-06 | As a general director, I want to be able to suspend or reactivate an employee إيقاف / تفعيل الموظف, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |


**Acceptance criteria**


| Story | US-EMP-01 (UC-EMP-01) |
| --- | --- |
| User story | As a general director, I want to be able to list employees قائمة الموظفين, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a general director with an active session on the screen at `#/employees`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/EmployeeManagement` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §9.S are implemented with their mandatory flags and lookups; the scenario of §9.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-EMP-02 (UC-EMP-02) |
| --- | --- |
| User story | As a general director, I want to be able to verify employee username availability التحقق من اسم المستخدم, so that only the right people reach the register and each of them sees only their own scope. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a general director holding valid credentials in the module, when the actor invokes the function with valid input, then an authenticated session exists (or has been ended); the role code and country id that scope every later request are held by the SPA.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/EmployeeManagement/check-national-id` and the response is rendered on the screen without a page reload.<br>3. Given the credentials are wrong or the account is locked out, when the actor submits them, then the attempt fails, no ticket is issued and the actor stays on the screen. |
| Definition of done | The screen fields of §9.S are implemented with their mandatory flags and lookups; the scenario of §9.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-EMP-03 (UC-EMP-03) |
| --- | --- |
| User story | As a general director, I want to be able to create an employee account اضافة موظف, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a general director with an active session on the screen at `#/employees/:id`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/EmployeeManagement` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §9.S are implemented with their mandatory flags and lookups; the scenario of §9.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-EMP-04 (UC-EMP-04) |
| --- | --- |
| User story | As a general director, I want to be able to view an employee record بيانات الموظف, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a general director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/EmployeeManagement/{id}` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §9.S are implemented with their mandatory flags and lookups; the scenario of §9.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-EMP-05 (UC-EMP-05) |
| --- | --- |
| User story | As a general director, I want to be able to update an employee and change their role تعديل بيانات الموظف, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a general director with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/EmployeeManagement` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §9.S are implemented with their mandatory flags and lookups; the scenario of §9.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-EMP-06 (UC-EMP-06) |
| --- | --- |
| User story | As a general director, I want to be able to suspend or reactivate an employee إيقاف / تفعيل الموظف, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a general director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `PATCH /api/EmployeeManagement/{id}/deactivate` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §9.S are implemented with their mandatory flags and lookups; the scenario of §9.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.5  EP-05 — Family Register


| Item | Value |
| --- | --- |
| Epic | EP-05 |
| Module | Family Register — سجل الاسر |
| Chapter / prefix | Chapter 10 · UC-FAM |
| Business goal | To hold the beneficiary household file that every orphan, payment and report hangs from. |
| Stories | 14 (8 Must, 6 Should) |
| Points | 68 |
| Routes | `#/families`, `#/families/:id/edit`, `#/families/:id/members`, `#/families/provider-requests` |
| Specification | Module document 10-UC-FAM-Family-Register — §10.S for the screen contract, §10.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution) |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-FAM-01 | UC-FAM-01 | As a charity user, I want to be able to list families of a charity قائمة الأسر, so that I can find the record I need without leaving the system. | Browse a list | Must · 2 |
| US-FAM-02 | UC-FAM-02 | As a charity user, I want to be able to search families البحث عن أسرة, so that I can locate a record from partial information. | Search / filter | Should · 2 |
| US-FAM-03 | UC-FAM-03 | As a charity user, I want to be able to register a new family اضافة اسرة, so that the register reflects reality as soon as the fact is known. | Create a record | Must · 10 |
| US-FAM-04 | UC-FAM-04 | As a charity user, I want to be able to view a family file عرض بيانات الأسرة, so that I can see the full detail of a single record before acting on it. | Read a record | Must · 2 |
| US-FAM-05 | UC-FAM-05 | As a charity user, I want to be able to update a family file تعديل بيانات الأسرة, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Must · 5 |
| US-FAM-06 | UC-FAM-06 | As a HQ roles, I want to be able to transfer a family to another charity نقل الأسرة لجمعية أخرى, so that a beneficiary file follows the charity that actually serves the family. | Transfer of ownership | Should · 8 |
| US-FAM-07 | UC-FAM-07 | As a General Director, I want to be able to move an orphan between families نقل يتيم بين الأسر, so that a beneficiary file follows the charity that actually serves the family. | Transfer of ownership | Should · 8 |
| US-FAM-08 | UC-FAM-08 | As a General Director, I want to be able to move a guardian between families نقل العائل بين الأسر, so that the register reflects reality as soon as the fact is known. | Create a record | Must · 5 |
| US-FAM-09 | UC-FAM-09 | As a general director, I want to be able to review guardian-change requests طلبات تعديل العائل, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Must · 5 |
| US-FAM-10 | UC-FAM-10 | As a general director, I want to be able to approve a guardian-change request اعتماد تعديل العائل, so that head office keeps control of what is accepted into the sponsorship cycle. | Review decision | Must · 8 |
| US-FAM-11 | UC-FAM-11 | As a HQ roles, I want to be able to track family follow-up activity متابعة إدخالات الأسر, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Should · 3 |
| US-FAM-12 | UC-FAM-12 | As a charity user, I want to be able to verify a guardian can be added التحقق من إمكانية إضافة عائل, so that the register reflects reality as soon as the fact is known. | Create a record | Must · 5 |
| US-FAM-13 | UC-FAM-13 | As a HQ roles, I want to be able to remove a guardian's sponsorship link حذف كفالة العائل, so that records entered in error do not distort the register or the reporting. | Delete a record | Should · 2 |
| US-FAM-14 | UC-FAM-14 | As a HQ roles, I want to be able to print family follow-up and identification sheets طباعة كشوف المتابعة, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Should · 3 |


**Acceptance criteria**


| Story | US-FAM-01 (UC-FAM-01) |
| --- | --- |
| User story | As a charity user, I want to be able to list families of a charity قائمة الأسر, so that I can find the record I need without leaving the system. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session on the screen at `#/families`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families?charityId=` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §10.S are implemented with their mandatory flags and lookups; the scenario of §10.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-FAM-02 (UC-FAM-02) |
| --- | --- |
| User story | As a charity user, I want to be able to search families البحث عن أسرة, so that I can locate a record from partial information. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families?charityId=&search=` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §10.S are implemented with their mandatory flags and lookups; the scenario of §10.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-FAM-03 (UC-FAM-03) |
| --- | --- |
| User story | As a charity user, I want to be able to register a new family اضافة اسرة, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Must · 10 points |
| Acceptance criteria | 1. Given a charity user with an active session on the screen at `#/families/:id/edit`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Families` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «أحد المعيلين مكرر من قبل أكثر من مرة» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §10.S are implemented with their mandatory flags and lookups; the scenario of §10.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-FAM-04 (UC-FAM-04) |
| --- | --- |
| User story | As a charity user, I want to be able to view a family file عرض بيانات الأسرة, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/{id}` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §10.S are implemented with their mandatory flags and lookups; the scenario of §10.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-FAM-05 (UC-FAM-05) |
| --- | --- |
| User story | As a charity user, I want to be able to update a family file تعديل بيانات الأسرة, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Must · 5 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/Families` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «لا يمكن تعديل اسم لمعيل صرف شيك مسبقا» is broken, when the operation is attempted, then it is refused with that message and nothing is written.<br>7. Given the business rule behind «تم اضافه الطلب من قبل . انتظر موافه مسئول المكتب» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §10.S are implemented with their mandatory flags and lookups; the scenario of §10.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-FAM-06 (UC-FAM-06) |
| --- | --- |
| User story | As a HQ roles, I want to be able to transfer a family to another charity نقل الأسرة لجمعية أخرى, so that a beneficiary file follows the charity that actually serves the family. |
| Priority / size | Should · 8 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor invokes the function with valid input, then the file and its dependent records belong to the receiving charity.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/Families/{id}/charity` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «Faild Operation» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §10.S are implemented with their mandatory flags and lookups; the scenario of §10.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-FAM-07 (UC-FAM-07) |
| --- | --- |
| User story | As a General Director, I want to be able to move an orphan between families نقل يتيم بين الأسر, so that a beneficiary file follows the charity that actually serves the family. |
| Priority / size | Should · 8 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/families/:id/members`, when the actor presses «غلق» with valid input, then the file and its dependent records belong to the receiving charity.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Families/{familyId}/members/{memberId}/control` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §10.S are implemented with their mandatory flags and lookups; the scenario of §10.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-FAM-08 (UC-FAM-08) |
| --- | --- |
| User story | As a General Director, I want to be able to move a guardian between families نقل العائل بين الأسر, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Must · 5 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Families/{familyId}/members/{memberId}/control` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §10.S are implemented with their mandatory flags and lookups; the scenario of §10.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-FAM-09 (UC-FAM-09) |
| --- | --- |
| User story | As a general director, I want to be able to review guardian-change requests طلبات تعديل العائل, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Must · 5 points |
| Acceptance criteria | 1. Given a general director with an active session on the screen at `#/families/provider-requests`, when the actor presses «EnsureUpdate» with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/provider-requests` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>8. Given the business rule behind «Faild Operation» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §10.S are implemented with their mandatory flags and lookups; the scenario of §10.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-FAM-10 (UC-FAM-10) |
| --- | --- |
| User story | As a general director, I want to be able to approve a guardian-change request اعتماد تعديل العائل, so that head office keeps control of what is accepted into the sponsorship cycle. |
| Priority / size | Must · 8 points |
| Acceptance criteria | 1. Given a general director with an active session in the module, when the actor invokes the function with valid input, then the item carries its new state, the deciding user and the decision date, and moves out of the pending queue.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Families/provider-requests/{id}/approve` and the response is rendered on the screen without a page reload.<br>3. Given the decision is a refusal, when no reason is given, then the refusal is not accepted.<br>4. Given the decision is recorded, when the charity opens the item, then it sees the new state and, on refusal, the reason.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>8. Given the business rule behind «Faild Operation» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §10.S are implemented with their mandatory flags and lookups; the scenario of §10.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-FAM-11 (UC-FAM-11) |
| --- | --- |
| User story | As a HQ roles, I want to be able to track family follow-up activity متابعة إدخالات الأسر, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Should · 3 points |
| Acceptance criteria | 1. Given a HQ roles with an active session on the screen at `#/reports/family-orphans`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/{id}/follow-up` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §10.S are implemented with their mandatory flags and lookups; the scenario of §10.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-FAM-12 (UC-FAM-12) |
| --- | --- |
| User story | As a charity user, I want to be able to verify a guardian can be added التحقق من إمكانية إضافة عائل, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Must · 5 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Families/{familyId}/verify-provider` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §10.S are implemented with their mandatory flags and lookups; the scenario of §10.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-FAM-13 (UC-FAM-13) |
| --- | --- |
| User story | As a HQ roles, I want to be able to remove a guardian's sponsorship link حذف كفالة العائل, so that records entered in error do not distort the register or the reporting. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor invokes the function with valid input, then the record is no longer returned by the list and read endpoints of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `DELETE /api/Families/{familyId}/provider/sponsor` and the response is rendered on the screen without a page reload.<br>3. Given the actor requests deletion, when the confirmation is declined, then nothing is deleted.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §10.S are implemented with their mandatory flags and lookups; the scenario of §10.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-FAM-14 (UC-FAM-14) |
| --- | --- |
| User story | As a HQ roles, I want to be able to print family follow-up and identification sheets طباعة كشوف المتابعة, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Should · 3 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/family-update-tracking/export/pdf` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §10.S are implemented with their mandatory flags and lookups; the scenario of §10.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.6  EP-06 — Housing Project


| Item | Value |
| --- | --- |
| Epic | EP-06 |
| Module | Housing Project — مشروع الاسكان |
| Chapter / prefix | Chapter 11 · UC-HOU |
| Business goal | To run the parallel register of families housed in organisation-owned buildings. |
| Stories | 8 (7 Should, 1 Could) |
| Points | 36 |
| Routes | `#/housing-projects`, `#/housing-projects/:id/edit`, `#/housing-projects/:id/reports`, `#/housing-projects/:id/reports/:reportId` |
| Specification | Module document 11-UC-HOU-Housing-Project — §11.S for the screen contract, §11.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution) |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-HOU-01 | UC-HOU-01 | As a charity user, I want to be able to list housing families قائمة الأسر الساكنة, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-HOU-02 | UC-HOU-02 | As a charity user, I want to be able to search housing families البحث في الأسر الساكنة, so that I can locate a record from partial information. | Search / filter | Should · 2 |
| US-HOU-03 | UC-HOU-03 | As a charity user, I want to be able to register a housing family اضافة أسرة ساكنة, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 10 |
| US-HOU-04 | UC-HOU-04 | As a charity user, I want to be able to view / update a housing family بيانات الأسرة الساكنة, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |
| US-HOU-05 | UC-HOU-05 | As a charity user, I want to be able to select building and flat اختيار المبنى والشقة, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-HOU-06 | UC-HOU-06 | As a charity user, I want to be able to list periodic reports of a housing beneficiary التقارير الدورية للأسر الساكنة, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Could · 3 |
| US-HOU-07 | UC-HOU-07 | As a charity user, I want to be able to look up a housing beneficiary by code البحث بالكود, so that I can locate a record from partial information. | Search / filter | Should · 2 |
| US-HOU-08 | UC-HOU-08 | As a charity user, I want to be able to create a housing periodic report تقرير دوري لأسرة ساكنة, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 10 |


**Acceptance criteria**


| Story | US-HOU-01 (UC-HOU-01) |
| --- | --- |
| User story | As a charity user, I want to be able to list housing families قائمة الأسر الساكنة, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session on the screen at `#/housing-projects`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families?familyType=Housing&charityId=` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §11.S are implemented with their mandatory flags and lookups; the scenario of §11.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-HOU-02 (UC-HOU-02) |
| --- | --- |
| User story | As a charity user, I want to be able to search housing families البحث في الأسر الساكنة, so that I can locate a record from partial information. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families?familyType=Housing&search=` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §11.S are implemented with their mandatory flags and lookups; the scenario of §11.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-HOU-03 (UC-HOU-03) |
| --- | --- |
| User story | As a charity user, I want to be able to register a housing family اضافة أسرة ساكنة, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 10 points |
| Acceptance criteria | 1. Given a charity user with an active session on the screen at `#/housing-projects/:id/edit`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/HousingProjects/projects` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «أحد المعيلين مكرر من قبل أكثر من مرة» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §11.S are implemented with their mandatory flags and lookups; the scenario of §11.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-HOU-04 (UC-HOU-04) |
| --- | --- |
| User story | As a charity user, I want to be able to view / update a housing family بيانات الأسرة الساكنة, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/HousingProjects/projects/{id}` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §11.S are implemented with their mandatory flags and lookups; the scenario of §11.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-HOU-05 (UC-HOU-05) |
| --- | --- |
| User story | As a charity user, I want to be able to select building and flat اختيار المبنى والشقة, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/LookupManagement/housing-buildings` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §11.S are implemented with their mandatory flags and lookups; the scenario of §11.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-HOU-06 (UC-HOU-06) |
| --- | --- |
| User story | As a charity user, I want to be able to list periodic reports of a housing beneficiary التقارير الدورية للأسر الساكنة, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a charity user with an active session on the screen at `#/housing-projects/:id/reports`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/PeriodicOrphanReports/by-orphan/{orphanId}` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §11.S are implemented with their mandatory flags and lookups; the scenario of §11.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-HOU-07 (UC-HOU-07) |
| --- | --- |
| User story | As a charity user, I want to be able to look up a housing beneficiary by code البحث بالكود, so that I can locate a record from partial information. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/PeriodicOrphanReports/by-orphan/{orphanId}` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §11.S are implemented with their mandatory flags and lookups; the scenario of §11.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-HOU-08 (UC-HOU-08) |
| --- | --- |
| User story | As a charity user, I want to be able to create a housing periodic report تقرير دوري لأسرة ساكنة, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 10 points |
| Acceptance criteria | 1. Given a charity user with an active session on the screen at `#/housing-projects/:id/reports/:reportId`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/PeriodicOrphanReports` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §11.S are implemented with their mandatory flags and lookups; the scenario of §11.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.7  EP-07 — Refugee Families


| Item | Value |
| --- | --- |
| Epic | EP-07 |
| Module | Refugee Families — الاسر اللاجئة |
| Chapter / prefix | Chapter 12 · UC-REF |
| Business goal | To keep displaced families in their own register with their own contract and search. |
| Stories | 4 (4 Should) |
| Points | 19 |
| Routes | `#/families/refugees`, `#/families/refugees/:id/edit` |
| Specification | Module document 12-UC-REF-Refugee-Families — §12.S for the screen contract, §12.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution) |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-REF-01 | UC-REF-01 | As a charity user, I want to be able to list refugee families قائمة الأسر اللاجئة, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-REF-02 | UC-REF-02 | As a charity user, I want to be able to search refugee families البحث في الأسر اللاجئة, so that I can locate a record from partial information. | Search / filter | Should · 2 |
| US-REF-03 | UC-REF-03 | As a charity user, I want to be able to register a refugee family اضافة أسرة لاجئة, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 10 |
| US-REF-04 | UC-REF-04 | As a charity user, I want to be able to view / update a refugee family بيانات الأسرة اللاجئة, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |


**Acceptance criteria**


| Story | US-REF-01 (UC-REF-01) |
| --- | --- |
| User story | As a charity user, I want to be able to list refugee families قائمة الأسر اللاجئة, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session on the screen at `#/families/refugees`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §12.S are implemented with their mandatory flags and lookups; the scenario of §12.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-REF-02 (UC-REF-02) |
| --- | --- |
| User story | As a charity user, I want to be able to search refugee families البحث في الأسر اللاجئة, so that I can locate a record from partial information. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families?familyType=Refugee&search=` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §12.S are implemented with their mandatory flags and lookups; the scenario of §12.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-REF-03 (UC-REF-03) |
| --- | --- |
| User story | As a charity user, I want to be able to register a refugee family اضافة أسرة لاجئة, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 10 points |
| Acceptance criteria | 1. Given a charity user with an active session on the screen at `#/families/refugees/:id/edit`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Families` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «أحد المعيلين مكرر من قبل أكثر من مرة» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §12.S are implemented with their mandatory flags and lookups; the scenario of §12.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-REF-04 (UC-REF-04) |
| --- | --- |
| User story | As a charity user, I want to be able to view / update a refugee family بيانات الأسرة اللاجئة, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/{id}` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §12.S are implemented with their mandatory flags and lookups; the scenario of §12.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.8  EP-08 — Orphan Register & Coding


| Item | Value |
| --- | --- |
| Epic | EP-08 |
| Module | Orphan Register & Coding — سجل الايتام والتكويد |
| Chapter / prefix | Chapter 13 · UC-ORP |
| Business goal | To admit an orphan to sponsorship by giving it a code — without a code it can neither be reported on nor paid. |
| Stories | 11 (9 Must, 2 Should) |
| Points | 31 |
| Routes | `#/families/orphans/coding`, `#/families/orphans/coding/worklist` |
| Specification | Module document 13-UC-ORP-Orphan-Register-and-Coding — §13.S for the screen contract, §13.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution); EP-05 (family register) — an orphan exists only inside a family file |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-ORP-01 | UC-ORP-01 | As a charity user, I want to be able to check whether an orphan may be added التحقق من إمكانية إضافة يتيم, so that the register reflects reality as soon as the fact is known. | Create a record | Must · 5 |
| US-ORP-02 | UC-ORP-02 | As a signed-in user, I want to be able to find an orphan by name البحث باسم اليتيم, so that I can locate a record from partial information. | Search / filter | Should · 2 |
| US-ORP-03 | UC-ORP-03 | As a General Director, I want to be able to open the coding worklist تكويد الأيتام, so that I can see the full detail of a single record before acting on it. | Read a record | Must · 2 |
| US-ORP-04 | UC-ORP-04 | As a General Director, I want to be able to search orphans for coding البحث في قائمة التكويد, so that I can locate a record from partial information. | Search / filter | Should · 2 |
| US-ORP-05 | UC-ORP-05 | As a General Director, I want to be able to verify a code is not already used التحقق من تفرد الكود, so that I can see the full detail of a single record before acting on it. | Read a record | Must · 2 |
| US-ORP-06 | UC-ORP-06 | As a General Director, I want to be able to assign a sponsorship code to an orphan إسناد كود لليتيم, so that the register reflects reality as soon as the fact is known. | Create a record | Must · 5 |
| US-ORP-07 | UC-ORP-07 | As a signed-in user, I want to be able to resolve an orphan's name from a code استعلام بالكود, so that I can see the full detail of a single record before acting on it. | Read a record | Must · 2 |
| US-ORP-08 | UC-ORP-08 | As a charity user, I want to be able to view an orphan's payment history دفعات اليتيم, so that I can see the full detail of a single record before acting on it. | Read a record | Must · 2 |
| US-ORP-09 | UC-ORP-09 | As a charity user, I want to be able to view an orphan's payment details تفاصيل دفعة اليتيم, so that I can see the full detail of a single record before acting on it. | Read a record | Must · 2 |
| US-ORP-10 | UC-ORP-10 | As a charity user, I want to be able to check a phone number is not duplicated التحقق من رقم الهاتف, so that the register reflects reality as soon as the fact is known. | Create a record | Must · 5 |
| US-ORP-11 | UC-ORP-11 | As a charity user, I want to be able to load orphan reference data القوائم المرجعية لليتيم, so that I can find the record I need without leaving the system. | Browse a list | Must · 2 |


**Acceptance criteria**


| Story | US-ORP-01 (UC-ORP-01) |
| --- | --- |
| User story | As a charity user, I want to be able to check whether an orphan may be added التحقق من إمكانية إضافة يتيم, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Must · 5 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/orphans/check-national-id` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §13.S are implemented with their mandatory flags and lookups; the scenario of §13.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORP-02 (UC-ORP-02) |
| --- | --- |
| User story | As a signed-in user, I want to be able to find an orphan by name البحث باسم اليتيم, so that I can locate a record from partial information. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a signed-in user with an active session in the module, when the actor invokes the function with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/orphans?search=` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §13.S are implemented with their mandatory flags and lookups; the scenario of §13.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORP-03 (UC-ORP-03) |
| --- | --- |
| User story | As a General Director, I want to be able to open the coding worklist تكويد الأيتام, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/families/orphans/coding/worklist`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/orphans?codingStatus=Pending` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «Unthorized User» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §13.S are implemented with their mandatory flags and lookups; the scenario of §13.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORP-04 (UC-ORP-04) |
| --- | --- |
| User story | As a General Director, I want to be able to search orphans for coding البحث في قائمة التكويد, so that I can locate a record from partial information. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/families/orphans/coding`, when the actor presses «بحث» with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/orphans?search=` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «Unthorized User» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §13.S are implemented with their mandatory flags and lookups; the scenario of §13.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORP-05 (UC-ORP-05) |
| --- | --- |
| User story | As a General Director, I want to be able to verify a code is not already used التحقق من تفرد الكود, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/orphans/check-code` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §13.S are implemented with their mandatory flags and lookups; the scenario of §13.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORP-06 (UC-ORP-06) |
| --- | --- |
| User story | As a General Director, I want to be able to assign a sponsorship code to an orphan إسناد كود لليتيم, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Must · 5 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Families/orphans/{orphanId}/code` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §13.S are implemented with their mandatory flags and lookups; the scenario of §13.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORP-07 (UC-ORP-07) |
| --- | --- |
| User story | As a signed-in user, I want to be able to resolve an orphan's name from a code استعلام بالكود, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a signed-in user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/orphans?search=` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §13.S are implemented with their mandatory flags and lookups; the scenario of §13.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORP-08 (UC-ORP-08) |
| --- | --- |
| User story | As a charity user, I want to be able to view an orphan's payment history دفعات اليتيم, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/OrphanPayments?orphanId=` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §13.S are implemented with their mandatory flags and lookups; the scenario of §13.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORP-09 (UC-ORP-09) |
| --- | --- |
| User story | As a charity user, I want to be able to view an orphan's payment details تفاصيل دفعة اليتيم, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/OrphanPayments` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §13.S are implemented with their mandatory flags and lookups; the scenario of §13.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORP-10 (UC-ORP-10) |
| --- | --- |
| User story | As a charity user, I want to be able to check a phone number is not duplicated التحقق من رقم الهاتف, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Must · 5 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/{familyId}/provider/check-phone` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §13.S are implemented with their mandatory flags and lookups; the scenario of §13.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORP-11 (UC-ORP-11) |
| --- | --- |
| User story | As a charity user, I want to be able to load orphan reference data القوائم المرجعية لليتيم, so that I can find the record I need without leaving the system. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/OrphanPayments` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §13.S are implemented with their mandatory flags and lookups; the scenario of §13.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.9  EP-09 — Orphan Periodic Reports


| Item | Value |
| --- | --- |
| Epic | EP-09 |
| Module | Orphan Periodic Reports — التقارير الدورية للايتام |
| Chapter / prefix | Chapter 14 · UC-ORR |
| Business goal | To prove that sponsorship is being delivered, by collecting a recurring evidenced status report on each orphan and having head office accept or refuse it. |
| Stories | 17 (10 Should, 7 Must) |
| Points | 80 |
| Routes | `#/periodic-orphan-reports`, `#/periodic-orphan-reports/:id/edit`, `#/periodic-orphan-reports/orphan-reports`, `#/periodic-orphan-reports/orphan-reports/search` |
| Specification | Module document 14-UC-ORR-Orphan-Periodic-Reports — §14.S for the screen contract, §14.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution); EP-05 (family register) — an orphan exists only inside a family file |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-ORR-01 | UC-ORR-01 | As a charity user, I want to be able to list an orphan's periodic reports التقارير الدورية لليتيم, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Should · 3 |
| US-ORR-02 | UC-ORR-02 | As a charity user, I want to be able to look up an orphan by code before reporting استدعاء اليتيم بالكود, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Should · 3 |
| US-ORR-03 | UC-ORR-03 | As a charity user, I want to be able to create a periodic report إضافة تقرير دوري, so that the register reflects reality as soon as the fact is known. | Create a record | Must · 10 |
| US-ORR-04 | UC-ORR-04 | As a charity user, I want to be able to view a periodic report عرض التقرير, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Should · 3 |
| US-ORR-05 | UC-ORR-05 | As a charity user, I want to be able to update a periodic report تعديل التقرير, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Must · 5 |
| US-ORR-06 | UC-ORR-06 | As a HQ roles, I want to be able to delete a periodic report حذف التقرير, so that records entered in error do not distort the register or the reporting. | Delete a record | Should · 2 |
| US-ORR-07 | UC-ORR-07 | As a General Director, I want to be able to accept a periodic report اعتماد التقرير, so that head office keeps control of what is accepted into the sponsorship cycle. | Review decision | Must · 8 |
| US-ORR-08 | UC-ORR-08 | As a General Director, I want to be able to refuse a periodic report with reasons رفض التقرير مع الأسباب, so that head office keeps control of what is accepted into the sponsorship cycle. | Review decision | Must · 8 |
| US-ORR-09 | UC-ORR-09 | As a HQ roles, I want to be able to filter reports by status حالة اليتيم, so that I can locate a record from partial information. | Search / filter | Should · 2 |
| US-ORR-10 | UC-ORR-10 | As a general director, I want to be able to view report statistics by group احصائيات عامة للأيتام, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Should · 3 |
| US-ORR-11 | UC-ORR-11 | As a HQ roles, I want to be able to extract detailed report data تفاصيل التقارير, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Should · 3 |
| US-ORR-12 | UC-ORR-12 | As a HQ roles, I want to be able to extract accepted reports التقارير المعتمدة, so that head office keeps control of what is accepted into the sponsorship cycle. | Review decision | Must · 8 |
| US-ORR-13 | UC-ORR-13 | As a HQ roles, I want to be able to extract refused reports التقارير المرفوضة, so that head office keeps control of what is accepted into the sponsorship cycle. | Review decision | Must · 8 |
| US-ORR-14 | UC-ORR-14 | As a HQ roles, I want to be able to list orphans with no renewed report الأيتام بدون تقرير مجدد, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Should · 3 |
| US-ORR-15 | UC-ORR-15 | As a HQ roles, I want to be able to extract report numbers added in a period أرقام التقارير المضافة, so that the register reflects reality as soon as the fact is known. | Create a record | Must · 5 |
| US-ORR-16 | UC-ORR-16 | As a charity user, I want to be able to view report attachments صور اليتيم والشهادات, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Should · 3 |
| US-ORR-17 | UC-ORR-17 | As a charity user, I want to be able to print the periodic report form طباعة التقرير الدوري, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Should · 3 |


**Acceptance criteria**


| Story | US-ORR-01 (UC-ORR-01) |
| --- | --- |
| User story | As a charity user, I want to be able to list an orphan's periodic reports التقارير الدورية لليتيم, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Should · 3 points |
| Acceptance criteria | 1. Given a charity user with an active session on the screen at `#/periodic-orphan-reports`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/OrphanReports` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §14.S are implemented with their mandatory flags and lookups; the scenario of §14.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORR-02 (UC-ORR-02) |
| --- | --- |
| User story | As a charity user, I want to be able to look up an orphan by code before reporting استدعاء اليتيم بالكود, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Should · 3 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/PeriodicOrphanReports/by-orphan/{orphanId}` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §14.S are implemented with their mandatory flags and lookups; the scenario of §14.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORR-03 (UC-ORR-03) |
| --- | --- |
| User story | As a charity user, I want to be able to create a periodic report إضافة تقرير دوري, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Must · 10 points |
| Acceptance criteria | 1. Given a charity user with an active session on the screen at `#/periodic-orphan-reports/:id/edit`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/PeriodicOrphanReports` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §14.S are implemented with their mandatory flags and lookups; the scenario of §14.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORR-04 (UC-ORR-04) |
| --- | --- |
| User story | As a charity user, I want to be able to view a periodic report عرض التقرير, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Should · 3 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/PeriodicOrphanReports` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §14.S are implemented with their mandatory flags and lookups; the scenario of §14.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORR-05 (UC-ORR-05) |
| --- | --- |
| User story | As a charity user, I want to be able to update a periodic report تعديل التقرير, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Must · 5 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/PeriodicOrphanReports` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>8. Given the business rule behind «You can not update old report» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §14.S are implemented with their mandatory flags and lookups; the scenario of §14.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORR-06 (UC-ORR-06) |
| --- | --- |
| User story | As a HQ roles, I want to be able to delete a periodic report حذف التقرير, so that records entered in error do not distort the register or the reporting. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor invokes the function with valid input, then the record is no longer returned by the list and read endpoints of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `DELETE /api/PeriodicOrphanReports` and the response is rendered on the screen without a page reload.<br>3. Given the actor requests deletion, when the confirmation is declined, then nothing is deleted.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §14.S are implemented with their mandatory flags and lookups; the scenario of §14.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORR-07 (UC-ORR-07) |
| --- | --- |
| User story | As a General Director, I want to be able to accept a periodic report اعتماد التقرير, so that head office keeps control of what is accepted into the sponsorship cycle. |
| Priority / size | Must · 8 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then the item carries its new state, the deciding user and the decision date, and moves out of the pending queue.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/PeriodicOrphanReports` and the response is rendered on the screen without a page reload.<br>3. Given the decision is a refusal, when no reason is given, then the refusal is not accepted.<br>4. Given the decision is recorded, when the charity opens the item, then it sees the new state and, on refusal, the reason.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>8. Given the business rule behind «You can not update old report» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §14.S are implemented with their mandatory flags and lookups; the scenario of §14.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORR-08 (UC-ORR-08) |
| --- | --- |
| User story | As a General Director, I want to be able to refuse a periodic report with reasons رفض التقرير مع الأسباب, so that head office keeps control of what is accepted into the sponsorship cycle. |
| Priority / size | Must · 8 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then the item carries its new state, the deciding user and the decision date, and moves out of the pending queue.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/PeriodicOrphanReports` and the response is rendered on the screen without a page reload.<br>3. Given the decision is a refusal, when no reason is given, then the refusal is not accepted.<br>4. Given the decision is recorded, when the charity opens the item, then it sees the new state and, on refusal, the reason.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>8. Given the business rule behind «You can not update old report» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §14.S are implemented with their mandatory flags and lookups; the scenario of §14.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORR-09 (UC-ORR-09) |
| --- | --- |
| User story | As a HQ roles, I want to be able to filter reports by status حالة اليتيم, so that I can locate a record from partial information. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a HQ roles with an active session on the screen at `#/periodic-orphan-reports/orphan-reports/search`, when the actor presses «بحث» with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/PeriodicOrphanReports/{id}/review` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>4. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §14.S are implemented with their mandatory flags and lookups; the scenario of §14.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORR-10 (UC-ORR-10) |
| --- | --- |
| User story | As a general director, I want to be able to view report statistics by group احصائيات عامة للأيتام, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Should · 3 points |
| Acceptance criteria | 1. Given a general director with an active session on the screen at `#/periodic-orphan-reports/orphan-reports`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/OrphanReports/statistics` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «Faild Operation» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §14.S are implemented with their mandatory flags and lookups; the scenario of §14.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORR-11 (UC-ORR-11) |
| --- | --- |
| User story | As a HQ roles, I want to be able to extract detailed report data تفاصيل التقارير, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Should · 3 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/OrphanReports/generate` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §14.S are implemented with their mandatory flags and lookups; the scenario of §14.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORR-12 (UC-ORR-12) |
| --- | --- |
| User story | As a HQ roles, I want to be able to extract accepted reports التقارير المعتمدة, so that head office keeps control of what is accepted into the sponsorship cycle. |
| Priority / size | Must · 8 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor invokes the function with valid input, then the item carries its new state, the deciding user and the decision date, and moves out of the pending queue.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/PeriodicOrphanReports/approved` and the response is rendered on the screen without a page reload.<br>3. Given the decision is a refusal, when no reason is given, then the refusal is not accepted.<br>4. Given the decision is recorded, when the charity opens the item, then it sees the new state and, on refusal, the reason.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §14.S are implemented with their mandatory flags and lookups; the scenario of §14.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORR-13 (UC-ORR-13) |
| --- | --- |
| User story | As a HQ roles, I want to be able to extract refused reports التقارير المرفوضة, so that head office keeps control of what is accepted into the sponsorship cycle. |
| Priority / size | Must · 8 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor invokes the function with valid input, then the item carries its new state, the deciding user and the decision date, and moves out of the pending queue.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/PeriodicOrphanReports/rejected` and the response is rendered on the screen without a page reload.<br>3. Given the decision is a refusal, when no reason is given, then the refusal is not accepted.<br>4. Given the decision is recorded, when the charity opens the item, then it sees the new state and, on refusal, the reason.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §14.S are implemented with their mandatory flags and lookups; the scenario of §14.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORR-14 (UC-ORR-14) |
| --- | --- |
| User story | As a HQ roles, I want to be able to list orphans with no renewed report الأيتام بدون تقرير مجدد, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Should · 3 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/non-renewed-reports` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §14.S are implemented with their mandatory flags and lookups; the scenario of §14.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORR-15 (UC-ORR-15) |
| --- | --- |
| User story | As a HQ roles, I want to be able to extract report numbers added in a period أرقام التقارير المضافة, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Must · 5 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor invokes the function with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/OrphanReports/statistics` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §14.S are implemented with their mandatory flags and lookups; the scenario of §14.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORR-16 (UC-ORR-16) |
| --- | --- |
| User story | As a charity user, I want to be able to view report attachments صور اليتيم والشهادات, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Should · 3 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Attachments/{id}/image` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §14.S are implemented with their mandatory flags and lookups; the scenario of §14.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-ORR-17 (UC-ORR-17) |
| --- | --- |
| User story | As a charity user, I want to be able to print the periodic report form طباعة التقرير الدوري, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Should · 3 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/orphan-report-form/export/pdf` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §14.S are implemented with their mandatory flags and lookups; the scenario of §14.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.10  EP-10 — Orphan Payments & Disbursement


| Item | Value |
| --- | --- |
| Epic | EP-10 |
| Module | Orphan Payments & Disbursement — دفعات الايتام |
| Chapter / prefix | Chapter 15 · UC-PAY |
| Business goal | To get the sponsorship money from head office to the guardian, and be able to prove afterwards that it arrived. |
| Stories | 24 (18 Must, 6 Should) |
| Points | 95 |
| Routes | `#/orphan-payments`, `#/orphan-payments/:id/edit`, `#/orphan-payments/:id/cheques`, `#/orphan-payments/:id/bank-file` |
| Specification | Module document 15-UC-PAY-Orphan-Payments-and-Disbursement — §15.S for the screen contract, §15.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution); EP-05 (family register) — an orphan exists only inside a family file; EP-08 (orphan coding) — only coded orphans may be paid |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-PAY-01 | UC-PAY-01 | As a General Director, I want to be able to list payment batches قائمة دفعات الأيتام, so that I can find the record I need without leaving the system. | Browse a list | Must · 2 |
| US-PAY-02 | UC-PAY-02 | As a General Director, I want to be able to create a payment batch اضافة دفعة مالية, so that the register reflects reality as soon as the fact is known. | Create a record | Must · 5 |
| US-PAY-03 | UC-PAY-03 | As a General Director, I want to be able to view a payment batch عرض الدفعة, so that I can see the full detail of a single record before acting on it. | Read a record | Must · 2 |
| US-PAY-04 | UC-PAY-04 | As a General Director, I want to be able to update a payment batch تعديل الدفعة, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Must · 5 |
| US-PAY-05 | UC-PAY-05 | As a General Director, I want to be able to delete a payment batch حذف الدفعة, so that records entered in error do not distort the register or the reporting. | Delete a record | Should · 2 |
| US-PAY-06 | UC-PAY-06 | As a charity user, I want to be able to list a charity's batch numbers أرقام الدفعات للجمعية, so that I can find the record I need without leaving the system. | Browse a list | Must · 2 |
| US-PAY-07 | UC-PAY-07 | As a charity user, I want to be able to view payment details for a charity تفاصيل الدفعة للجمعية, so that I can see the full detail of a single record before acting on it. | Read a record | Must · 2 |
| US-PAY-08 | UC-PAY-08 | As a charity user, I want to be able to list orphans in a batch الأيتام في الدفعة, so that I can find the record I need without leaving the system. | Browse a list | Must · 2 |
| US-PAY-09 | UC-PAY-09 | As a charity user, I want to be able to stop or resume an orphan's payment إيقاف / استئناف صرف اليتيم, so that the register reflects reality as soon as the fact is known. | Create a record | Must · 5 |
| US-PAY-10 | UC-PAY-10 | As a charity user, I want to be able to mark a payment row printed تعليم كمطبوع, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Should · 3 |
| US-PAY-11 | UC-PAY-11 | As a charity user, I want to be able to confirm receipt of a payment تأكيد الاستلام, so that the register reflects reality as soon as the fact is known. | Create a record | Must · 5 |
| US-PAY-12 | UC-PAY-12 | As a charity user, I want to be able to record cheque number, date and collector تسجيل رقم الشيك والمستلم, so that the register reflects reality as soon as the fact is known. | Create a record | Must · 5 |
| US-PAY-13 | UC-PAY-13 | As a charity user, I want to be able to update payment-detail flags تحديث بيانات الصرف, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Must · 5 |
| US-PAY-14 | UC-PAY-14 | As a Financial Director, I want to be able to generate the bank transfer file كشف التحويلات البنكية, so that the data can be handed to the bank, the auditor or the donor in the format they expect. | Export data | Must · 8 |
| US-PAY-15 | UC-PAY-15 | As a Financial Director, I want to be able to import transfer numbers from the bank رفع أرقام الحوالات, so that data produced outside the system is carried in without manual re-keying. | Import a file | Must · 8 |
| US-PAY-16 | UC-PAY-16 | As a Financial Director, I want to be able to import a batch reconciliation file رفع ملف الدفعة, so that data produced outside the system is carried in without manual re-keying. | Import a file | Must · 8 |
| US-PAY-17 | UC-PAY-17 | As a Financial Director, I want to be able to import exchange (execution) statuses رفع حالات الصرف, so that data produced outside the system is carried in without manual re-keying. | Import a file | Must · 8 |
| US-PAY-18 | UC-PAY-18 | As a HQ roles, I want to be able to report payments received المستلمون, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Should · 3 |
| US-PAY-19 | UC-PAY-19 | As a HQ roles, I want to be able to report payments not received غير المستلمين, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Should · 3 |
| US-PAY-20 | UC-PAY-20 | As a HQ roles, I want to be able to report stopped payments الموقوفون, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Should · 3 |
| US-PAY-21 | UC-PAY-21 | As a HQ roles, I want to be able to view batch summary pages ملخص الدفعة, so that I can see the full detail of a single record before acting on it. | Read a record | Must · 2 |
| US-PAY-22 | UC-PAY-22 | As a HQ roles, I want to be able to view batch transfers and cheques حوالات وشيكات الأيتام, so that I can see the full detail of a single record before acting on it. | Read a record | Must · 2 |
| US-PAY-23 | UC-PAY-23 | As a HQ roles, I want to be able to identify orphans sponsored elsewhere أيتام لهم كافل آخر, so that I can find the record I need without leaving the system. | Browse a list | Must · 2 |
| US-PAY-24 | UC-PAY-24 | As a charity user, I want to be able to print disbursement documents طباعة مستندات الصرف, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Should · 3 |


**Acceptance criteria**


| Story | US-PAY-01 (UC-PAY-01) |
| --- | --- |
| User story | As a General Director, I want to be able to list payment batches قائمة دفعات الأيتام, so that I can find the record I need without leaving the system. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/orphan-payments`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/OrphanPayments` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-02 (UC-PAY-02) |
| --- | --- |
| User story | As a General Director, I want to be able to create a payment batch اضافة دفعة مالية, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Must · 5 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/orphan-payments/:id/edit`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/OrphanPayments` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-03 (UC-PAY-03) |
| --- | --- |
| User story | As a General Director, I want to be able to view a payment batch عرض الدفعة, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/OrphanPayments` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-04 (UC-PAY-04) |
| --- | --- |
| User story | As a General Director, I want to be able to update a payment batch تعديل الدفعة, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Must · 5 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/OrphanPayments` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-05 (UC-PAY-05) |
| --- | --- |
| User story | As a General Director, I want to be able to delete a payment batch حذف الدفعة, so that records entered in error do not distort the register or the reporting. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then the record is no longer returned by the list and read endpoints of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `DELETE /api/OrphanPayments` and the response is rendered on the screen without a page reload.<br>3. Given the actor requests deletion, when the confirmation is declined, then nothing is deleted.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-06 (UC-PAY-06) |
| --- | --- |
| User story | As a charity user, I want to be able to list a charity's batch numbers أرقام الدفعات للجمعية, so that I can find the record I need without leaving the system. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/OrphanPayments/by-batch-no/{batchNo}` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-07 (UC-PAY-07) |
| --- | --- |
| User story | As a charity user, I want to be able to view payment details for a charity تفاصيل الدفعة للجمعية, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/OrphanPayments/{id}/details` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «Faild Operation» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-08 (UC-PAY-08) |
| --- | --- |
| User story | As a charity user, I want to be able to list orphans in a batch الأيتام في الدفعة, so that I can find the record I need without leaving the system. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/OrphanPayments/{id}/details` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-09 (UC-PAY-09) |
| --- | --- |
| User story | As a charity user, I want to be able to stop or resume an orphan's payment إيقاف / استئناف صرف اليتيم, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Must · 5 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/OrphanPayments/orphan-items` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>8. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-10 (UC-PAY-10) |
| --- | --- |
| User story | As a charity user, I want to be able to mark a payment row printed تعليم كمطبوع, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Should · 3 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/OrphanPayments/orphan-items` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>7. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-11 (UC-PAY-11) |
| --- | --- |
| User story | As a charity user, I want to be able to confirm receipt of a payment تأكيد الاستلام, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Must · 5 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/OrphanPayments/orphan-items` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>8. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-12 (UC-PAY-12) |
| --- | --- |
| User story | As a charity user, I want to be able to record cheque number, date and collector تسجيل رقم الشيك والمستلم, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Must · 5 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/OrphanPayments/orphan-items` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>8. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-13 (UC-PAY-13) |
| --- | --- |
| User story | As a charity user, I want to be able to update payment-detail flags تحديث بيانات الصرف, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Must · 5 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/OrphanPayments/{id}` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>8. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-14 (UC-PAY-14) |
| --- | --- |
| User story | As a Financial Director, I want to be able to generate the bank transfer file كشف التحويلات البنكية, so that the data can be handed to the bank, the auditor or the donor in the format they expect. |
| Priority / size | Must · 8 points |
| Acceptance criteria | 1. Given a Financial Director with an active session on the screen at `#/orphan-payments/:id/bank-file`, when the actor presses «كشف التحويلات» with valid input, then a workbook has been delivered to the actor. No stored data is changed.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/OrphanPayments/{id}/export` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>7. Given the business rule behind «Faild Operation» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-15 (UC-PAY-15) |
| --- | --- |
| User story | As a Financial Director, I want to be able to import transfer numbers from the bank رفع أرقام الحوالات, so that data produced outside the system is carried in without manual re-keying. |
| Priority / size | Must · 8 points |
| Acceptance criteria | 1. Given a Financial Director with an active session in the module, when the actor invokes the function with valid input, then the matched records carry the imported values; unmatched rows are left untouched and reported.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/OrphanPayments/{id}/import/transfer-numbers` and the response is rendered on the screen without a page reload.<br>3. Given the uploaded file does not match the expected layout, when the import runs, then no row is changed and the actor is told why.<br>4. Given some rows cannot be matched to an existing record, when the import completes, then those rows are reported back and the rest are applied.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-16 (UC-PAY-16) |
| --- | --- |
| User story | As a Financial Director, I want to be able to import a batch reconciliation file رفع ملف الدفعة, so that data produced outside the system is carried in without manual re-keying. |
| Priority / size | Must · 8 points |
| Acceptance criteria | 1. Given a Financial Director with an active session in the module, when the actor invokes the function with valid input, then the matched records carry the imported values; unmatched rows are left untouched and reported.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/OrphanPayments/{id}/import/bank-file` and the response is rendered on the screen without a page reload.<br>3. Given the uploaded file does not match the expected layout, when the import runs, then no row is changed and the actor is told why.<br>4. Given some rows cannot be matched to an existing record, when the import completes, then those rows are reported back and the rest are applied.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-17 (UC-PAY-17) |
| --- | --- |
| User story | As a Financial Director, I want to be able to import exchange (execution) statuses رفع حالات الصرف, so that data produced outside the system is carried in without manual re-keying. |
| Priority / size | Must · 8 points |
| Acceptance criteria | 1. Given a Financial Director with an active session in the module, when the actor invokes the function with valid input, then the matched records carry the imported values; unmatched rows are left untouched and reported.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/OrphanPayments/{id}/import/exchange-status` and the response is rendered on the screen without a page reload.<br>3. Given the uploaded file does not match the expected layout, when the import runs, then no row is changed and the actor is told why.<br>4. Given some rows cannot be matched to an existing record, when the import completes, then those rows are reported back and the rest are applied.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-18 (UC-PAY-18) |
| --- | --- |
| User story | As a HQ roles, I want to be able to report payments received المستلمون, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Should · 3 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/OrphanPayments/{id}/details?received=true` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-19 (UC-PAY-19) |
| --- | --- |
| User story | As a HQ roles, I want to be able to report payments not received غير المستلمين, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Should · 3 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/payments-not-received` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-20 (UC-PAY-20) |
| --- | --- |
| User story | As a HQ roles, I want to be able to report stopped payments الموقوفون, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Should · 3 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/payments-stopped` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-21 (UC-PAY-21) |
| --- | --- |
| User story | As a HQ roles, I want to be able to view batch summary pages ملخص الدفعة, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Dashboard/payment-summary` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-22 (UC-PAY-22) |
| --- | --- |
| User story | As a HQ roles, I want to be able to view batch transfers and cheques حوالات وشيكات الأيتام, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a HQ roles with an active session on the screen at `#/orphan-payments/:id/cheques`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/OrphanPayments/{id}/details` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-23 (UC-PAY-23) |
| --- | --- |
| User story | As a HQ roles, I want to be able to identify orphans sponsored elsewhere أيتام لهم كافل آخر, so that I can find the record I need without leaving the system. |
| Priority / size | Must · 2 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/orphans-other-sponsor` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PAY-24 (UC-PAY-24) |
| --- | --- |
| User story | As a charity user, I want to be able to print disbursement documents طباعة مستندات الصرف, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Should · 3 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/payments-received/export/pdf` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>7. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §15.S are implemented with their mandatory flags and lookups; the scenario of §15.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.11  EP-11 — General Cheques


| Item | Value |
| --- | --- |
| Epic | EP-11 |
| Module | General Cheques — الشيكات العامة |
| Chapter / prefix | Chapter 16 · UC-CHQ |
| Business goal | To issue and account for cheques raised outside the orphan payment cycle. |
| Stories | 10 (7 Should, 3 Could) |
| Points | 35 |
| Routes | `#/general-checks`, `#/general-checks/edit/:id`, `#/general-checks/statement` |
| Specification | Module document 16-UC-CHQ-General-Cheques — §16.S for the screen contract, §16.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution) |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-CHQ-01 | UC-CHQ-01 | As a Financial Director, I want to be able to list cheques قائمة الشيكات, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-CHQ-02 | UC-CHQ-02 | As a Financial Director, I want to be able to issue a cheque اضافة شيك, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 5 |
| US-CHQ-03 | UC-CHQ-03 | As a Financial Director, I want to be able to view a cheque عرض الشيك, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-CHQ-04 | UC-CHQ-04 | As a Financial Director, I want to be able to update a cheque تعديل الشيك, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |
| US-CHQ-05 | UC-CHQ-05 | As a Financial Director, I want to be able to select a beneficiary اختيار المستفيد, so that I can locate a record from partial information. | Search / filter | Should · 2 |
| US-CHQ-06 | UC-CHQ-06 | As a Financial Director, I want to be able to select the currency اختيار العملة, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-CHQ-07 | UC-CHQ-07 | As a system, I want to be able to convert an amount to Arabic words تفقيط المبلغ, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Could · 3 |
| US-CHQ-08 | UC-CHQ-08 | As a Financial Director, I want to be able to load bank cheque print positions مواضع الطباعة على الشيك, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Could · 3 |
| US-CHQ-09 | UC-CHQ-09 | As a Financial Director, I want to be able to produce a cheque statement بيان الشيكات, so that data produced outside the system is carried in without manual re-keying. | Import a file | Should · 8 |
| US-CHQ-10 | UC-CHQ-10 | As a Financial Director, I want to be able to print a cheque and the cheque report طباعة الشيك والتقرير, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Could · 3 |


**Acceptance criteria**


| Story | US-CHQ-01 (UC-CHQ-01) |
| --- | --- |
| User story | As a Financial Director, I want to be able to list cheques قائمة الشيكات, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a Financial Director with an active session on the screen at `#/general-checks`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/CheckManagement` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §16.S are implemented with their mandatory flags and lookups; the scenario of §16.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CHQ-02 (UC-CHQ-02) |
| --- | --- |
| User story | As a Financial Director, I want to be able to issue a cheque اضافة شيك, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a Financial Director with an active session on the screen at `#/general-checks/edit/:id`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/CheckManagement` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §16.S are implemented with their mandatory flags and lookups; the scenario of §16.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CHQ-03 (UC-CHQ-03) |
| --- | --- |
| User story | As a Financial Director, I want to be able to view a cheque عرض الشيك, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a Financial Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/CheckManagement/{id}` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §16.S are implemented with their mandatory flags and lookups; the scenario of §16.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CHQ-04 (UC-CHQ-04) |
| --- | --- |
| User story | As a Financial Director, I want to be able to update a cheque تعديل الشيك, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a Financial Director with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/CheckManagement` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §16.S are implemented with their mandatory flags and lookups; the scenario of §16.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CHQ-05 (UC-CHQ-05) |
| --- | --- |
| User story | As a Financial Director, I want to be able to select a beneficiary اختيار المستفيد, so that I can locate a record from partial information. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a Financial Director with an active session in the module, when the actor invokes the function with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/LookupManagement/cheque-beneficiaries` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §16.S are implemented with their mandatory flags and lookups; the scenario of §16.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CHQ-06 (UC-CHQ-06) |
| --- | --- |
| User story | As a Financial Director, I want to be able to select the currency اختيار العملة, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a Financial Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/LookupManagement/currencies` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>4. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §16.S are implemented with their mandatory flags and lookups; the scenario of §16.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CHQ-07 (UC-CHQ-07) |
| --- | --- |
| User story | As a system, I want to be able to convert an amount to Arabic words تفقيط المبلغ, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a system with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/CheckManagement/amount-in-words` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §16.S are implemented with their mandatory flags and lookups; the scenario of §16.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CHQ-08 (UC-CHQ-08) |
| --- | --- |
| User story | As a Financial Director, I want to be able to load bank cheque print positions مواضع الطباعة على الشيك, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a Financial Director with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/LookupManagement/banks/{id}/cheque-positions` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>7. Given the business rule behind «Operation Faild» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §16.S are implemented with their mandatory flags and lookups; the scenario of §16.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CHQ-09 (UC-CHQ-09) |
| --- | --- |
| User story | As a Financial Director, I want to be able to produce a cheque statement بيان الشيكات, so that data produced outside the system is carried in without manual re-keying. |
| Priority / size | Should · 8 points |
| Acceptance criteria | 1. Given a Financial Director with an active session on the screen at `#/general-checks/statement`, when the actor presses «طباعة» with valid input, then the matched records carry the imported values; unmatched rows are left untouched and reported.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/CheckManagement/report` and the response is rendered on the screen without a page reload.<br>3. Given the uploaded file does not match the expected layout, when the import runs, then no row is changed and the actor is told why.<br>4. Given some rows cannot be matched to an existing record, when the import completes, then those rows are reported back and the rest are applied.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §16.S are implemented with their mandatory flags and lookups; the scenario of §16.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CHQ-10 (UC-CHQ-10) |
| --- | --- |
| User story | As a Financial Director, I want to be able to print a cheque and the cheque report طباعة الشيك والتقرير, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a Financial Director with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/general-cheque/export/pdf` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §16.S are implemented with their mandatory flags and lookups; the scenario of §16.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.12  EP-12 — Seasonal Assistance Projects


| Item | Value |
| --- | --- |
| Epic | EP-12 |
| Module | Seasonal Assistance Projects — المساعدات الموسمية ومشاريع الاسر |
| Chapter / prefix | Chapter 17 · UC-PRJ |
| Business goal | To run seasonal and one-off aid campaigns and record which family received what. |
| Stories | 13 (11 Should, 2 Could) |
| Points | 43 |
| Routes | `#/seasonal-aid`, `#/seasonal-aid/:id/edit`, `#/seasonal-aid/:id/beneficiaries`, `#/seasonal-aid/:id/eligible-families`, `#/seasonal-aid/:id/report` |
| Specification | Module document 17-UC-PRJ-Seasonal-Assistance-Projects — §17.S for the screen contract, §17.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution) |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-PRJ-01 | UC-PRJ-01 | As a General Director, I want to be able to list projects المشاريع, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-PRJ-02 | UC-PRJ-02 | As a General Director, I want to be able to create a project إضافة مشروع, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 5 |
| US-PRJ-03 | UC-PRJ-03 | As a General Director, I want to be able to view a project عرض المشروع, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-PRJ-04 | UC-PRJ-04 | As a General Director, I want to be able to update a project تعديل المشروع, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |
| US-PRJ-05 | UC-PRJ-05 | As a General Director, I want to be able to delete a project حذف المشروع, so that records entered in error do not distort the register or the reporting. | Delete a record | Should · 2 |
| US-PRJ-06 | UC-PRJ-06 | As a charity user, I want to be able to open the family-selection screen مشاريع الأسر, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-PRJ-07 | UC-PRJ-07 | As a charity user, I want to be able to register families for a project اختيار الأسر للمشروع, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |
| US-PRJ-08 | UC-PRJ-08 | As a charity user, I want to be able to confirm a family received the assistance تأكيد استلام الأسرة, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-PRJ-09 | UC-PRJ-09 | As a HQ roles, I want to be able to list registered families الأسر المختارة للمشروع, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 5 |
| US-PRJ-10 | UC-PRJ-10 | As a HQ roles, I want to be able to list non-registered families كافة الأسر للمشروع, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 5 |
| US-PRJ-11 | UC-PRJ-11 | As a HQ roles, I want to be able to view the project summary ملخص المشروع, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-PRJ-12 | UC-PRJ-12 | As a HQ roles, I want to be able to project detail report by charity تقرير تفصيلي للمشروع حسب الجمعيات, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Could · 3 |
| US-PRJ-13 | UC-PRJ-13 | As a charity user, I want to be able to print project distribution documents طباعة كشوف التوزيع, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Could · 3 |


**Acceptance criteria**


| Story | US-PRJ-01 (UC-PRJ-01) |
| --- | --- |
| User story | As a General Director, I want to be able to list projects المشاريع, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/seasonal-aid`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/SeasonalAid/campaigns` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §17.S are implemented with their mandatory flags and lookups; the scenario of §17.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PRJ-02 (UC-PRJ-02) |
| --- | --- |
| User story | As a General Director, I want to be able to create a project إضافة مشروع, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/seasonal-aid/:id/edit`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/SeasonalAid/campaigns` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §17.S are implemented with their mandatory flags and lookups; the scenario of §17.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PRJ-03 (UC-PRJ-03) |
| --- | --- |
| User story | As a General Director, I want to be able to view a project عرض المشروع, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/SeasonalAid/campaigns` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §17.S are implemented with their mandatory flags and lookups; the scenario of §17.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PRJ-04 (UC-PRJ-04) |
| --- | --- |
| User story | As a General Director, I want to be able to update a project تعديل المشروع, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/SeasonalAid/campaigns` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §17.S are implemented with their mandatory flags and lookups; the scenario of §17.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PRJ-05 (UC-PRJ-05) |
| --- | --- |
| User story | As a General Director, I want to be able to delete a project حذف المشروع, so that records entered in error do not distort the register or the reporting. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then the record is no longer returned by the list and read endpoints of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `DELETE /api/SeasonalAid/campaigns` and the response is rendered on the screen without a page reload.<br>3. Given the actor requests deletion, when the confirmation is declined, then nothing is deleted.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §17.S are implemented with their mandatory flags and lookups; the scenario of §17.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PRJ-06 (UC-PRJ-06) |
| --- | --- |
| User story | As a charity user, I want to be able to open the family-selection screen مشاريع الأسر, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session on the screen at `#/seasonal-aid/:id/beneficiaries`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/SeasonalAid/campaigns/{campaignId}/beneficiaries` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §17.S are implemented with their mandatory flags and lookups; the scenario of §17.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PRJ-07 (UC-PRJ-07) |
| --- | --- |
| User story | As a charity user, I want to be able to register families for a project اختيار الأسر للمشروع, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/SeasonalAid/campaigns/{campaignId}/beneficiaries` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «Operation Faild» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §17.S are implemented with their mandatory flags and lookups; the scenario of §17.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PRJ-08 (UC-PRJ-08) |
| --- | --- |
| User story | As a charity user, I want to be able to confirm a family received the assistance تأكيد استلام الأسرة, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/Families/{id}/received-flag` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §17.S are implemented with their mandatory flags and lookups; the scenario of §17.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PRJ-09 (UC-PRJ-09) |
| --- | --- |
| User story | As a HQ roles, I want to be able to list registered families الأسر المختارة للمشروع, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a HQ roles with an active session on the screen at `#/seasonal-aid/:id/beneficiaries`, when the actor presses «استخراج البيانات» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/SeasonalAid/campaigns/{campaignId}/beneficiaries` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §17.S are implemented with their mandatory flags and lookups; the scenario of §17.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PRJ-10 (UC-PRJ-10) |
| --- | --- |
| User story | As a HQ roles, I want to be able to list non-registered families كافة الأسر للمشروع, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a HQ roles with an active session on the screen at `#/seasonal-aid/:id/eligible-families`, when the actor presses «استخراج البيانات» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/SeasonalAid/campaigns/{campaignId}/eligible-families` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §17.S are implemented with their mandatory flags and lookups; the scenario of §17.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PRJ-11 (UC-PRJ-11) |
| --- | --- |
| User story | As a HQ roles, I want to be able to view the project summary ملخص المشروع, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/SeasonalAid/campaigns/{id}/report` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §17.S are implemented with their mandatory flags and lookups; the scenario of §17.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PRJ-12 (UC-PRJ-12) |
| --- | --- |
| User story | As a HQ roles, I want to be able to project detail report by charity تقرير تفصيلي للمشروع حسب الجمعيات, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a HQ roles with an active session on the screen at `#/seasonal-aid/:id/report`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/SeasonalAid/campaigns/{id}/report` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «Operation Faild» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §17.S are implemented with their mandatory flags and lookups; the scenario of §17.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-PRJ-13 (UC-PRJ-13) |
| --- | --- |
| User story | As a charity user, I want to be able to print project distribution documents طباعة كشوف التوزيع, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/family-cards/export/pdf` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>7. Given the business rule behind «Operation Faild» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §17.S are implemented with their mandatory flags and lookups; the scenario of §17.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.13  EP-13 — Office Development Projects


| Item | Value |
| --- | --- |
| Epic | EP-13 |
| Module | Office Development Projects — المشاريع التنموية للمكتب |
| Chapter / prefix | Chapter 18 · UC-OFP |
| Business goal | To track the capital and development projects the head office runs itself. |
| Stories | 6 (5 Should, 1 Could) |
| Points | 22 |
| Routes | `#/office-development-projects`, `#/office-development-projects/:id/edit` |
| Specification | Module document 18-UC-OFP-Office-Development-Projects — §18.S for the screen contract, §18.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution) |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-OFP-01 | UC-OFP-01 | As a General Director, I want to be able to list development projects المشاريع التنموية, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-OFP-02 | UC-OFP-02 | As a General Director, I want to be able to select a project type نوع المشروع, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-OFP-03 | UC-OFP-03 | As a General Director, I want to be able to create a development project اضافة مشروع تنموي, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 8 |
| US-OFP-04 | UC-OFP-04 | As a General Director, I want to be able to view / update a development project تعديل المشروع التنموي, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |
| US-OFP-05 | UC-OFP-05 | As a General Director, I want to be able to delete a development project حذف المشروع التنموي, so that records entered in error do not distort the register or the reporting. | Delete a record | Should · 2 |
| US-OFP-06 | UC-OFP-06 | As a General Director, I want to be able to report on development projects تقرير المشاريع التنموية, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Could · 3 |


**Acceptance criteria**


| Story | US-OFP-01 (UC-OFP-01) |
| --- | --- |
| User story | As a General Director, I want to be able to list development projects المشاريع التنموية, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/office-development-projects`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/OfficeProjectManagement` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §18.S are implemented with their mandatory flags and lookups; the scenario of §18.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-OFP-02 (UC-OFP-02) |
| --- | --- |
| User story | As a General Director, I want to be able to select a project type نوع المشروع, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/LookupManagement/office-project-types` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §18.S are implemented with their mandatory flags and lookups; the scenario of §18.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-OFP-03 (UC-OFP-03) |
| --- | --- |
| User story | As a General Director, I want to be able to create a development project اضافة مشروع تنموي, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 8 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/office-development-projects/:id/edit`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/OfficeProjectManagement` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §18.S are implemented with their mandatory flags and lookups; the scenario of §18.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-OFP-04 (UC-OFP-04) |
| --- | --- |
| User story | As a General Director, I want to be able to view / update a development project تعديل المشروع التنموي, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/OfficeProjectManagement/{id}` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §18.S are implemented with their mandatory flags and lookups; the scenario of §18.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-OFP-05 (UC-OFP-05) |
| --- | --- |
| User story | As a General Director, I want to be able to delete a development project حذف المشروع التنموي, so that records entered in error do not distort the register or the reporting. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then the record is no longer returned by the list and read endpoints of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `DELETE /api/OfficeProjectManagement` and the response is rendered on the screen without a page reload.<br>3. Given the actor requests deletion, when the confirmation is declined, then nothing is deleted.<br>4. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §18.S are implemented with their mandatory flags and lookups; the scenario of §18.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-OFP-06 (UC-OFP-06) |
| --- | --- |
| User story | As a General Director, I want to be able to report on development projects تقرير المشاريع التنموية, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/OfficeProjectManagement/export` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §18.S are implemented with their mandatory flags and lookups; the scenario of §18.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.14  EP-14 — Technical Support


| Item | Value |
| --- | --- |
| Epic | EP-14 |
| Module | Technical Support — الدعم الفني |
| Chapter / prefix | Chapter 19 · UC-CST |
| Business goal | To let charities raise problems and let the office track them to closure. |
| Stories | 6 (5 Should, 1 Could) |
| Points | 19 |
| Routes | `#/technical-support`, `#/technical-support/:id/edit` |
| Specification | Module document 19-UC-CST-Technical-Support — §19.S for the screen contract, §19.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution) |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-CST-01 | UC-CST-01 | As a General Director, I want to be able to list support tickets قائمة الدعم الفني, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-CST-02 | UC-CST-02 | As a General Director, I want to be able to raise a support ticket اضافة دعم فني, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 5 |
| US-CST-03 | UC-CST-03 | As a General Director, I want to be able to view a support ticket عرض الطلب, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-CST-04 | UC-CST-04 | As a General Director, I want to be able to update a support ticket تعديل الطلب, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |
| US-CST-05 | UC-CST-05 | As a General Director, I want to be able to delete a support ticket حذف الطلب, so that records entered in error do not distort the register or the reporting. | Delete a record | Should · 2 |
| US-CST-06 | UC-CST-06 | As a General Director, I want to be able to report on support tickets تقرير الدعم الفني, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Could · 3 |


**Acceptance criteria**


| Story | US-CST-01 (UC-CST-01) |
| --- | --- |
| User story | As a General Director, I want to be able to list support tickets قائمة الدعم الفني, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/technical-support`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/SupportTickets/all-tickets` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §19.S are implemented with their mandatory flags and lookups; the scenario of §19.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CST-02 (UC-CST-02) |
| --- | --- |
| User story | As a General Director, I want to be able to raise a support ticket اضافة دعم فني, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/technical-support/:id/edit`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/SupportTickets` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §19.S are implemented with their mandatory flags and lookups; the scenario of §19.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CST-03 (UC-CST-03) |
| --- | --- |
| User story | As a General Director, I want to be able to view a support ticket عرض الطلب, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/SupportTickets/{id}` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §19.S are implemented with their mandatory flags and lookups; the scenario of §19.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CST-04 (UC-CST-04) |
| --- | --- |
| User story | As a General Director, I want to be able to update a support ticket تعديل الطلب, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/SupportTickets` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §19.S are implemented with their mandatory flags and lookups; the scenario of §19.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CST-05 (UC-CST-05) |
| --- | --- |
| User story | As a General Director, I want to be able to delete a support ticket حذف الطلب, so that records entered in error do not distort the register or the reporting. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then the record is no longer returned by the list and read endpoints of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `DELETE /api/SupportTickets` and the response is rendered on the screen without a page reload.<br>3. Given the actor requests deletion, when the confirmation is declined, then nothing is deleted.<br>4. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §19.S are implemented with their mandatory flags and lookups; the scenario of §19.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-CST-06 (UC-CST-06) |
| --- | --- |
| User story | As a General Director, I want to be able to report on support tickets تقرير الدعم الفني, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/SupportTickets/report` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §19.S are implemented with their mandatory flags and lookups; the scenario of §19.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.15  EP-15 — Missions


| Item | Value |
| --- | --- |
| Epic | EP-15 |
| Module | Missions — المأموريات |
| Chapter / prefix | Chapter 20 · UC-MSN |
| Business goal | To plan field visits to charities and record what the visit found. |
| Stories | 9 (9 Should) |
| Points | 30 |
| Routes | `#/missions`, `#/missions/:id/edit`, `#/missions/:id/register` |
| Specification | Module document 20-UC-MSN-Missions — §20.S for the screen contract, §20.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution) |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-MSN-01 | UC-MSN-01 | As a General Director, I want to be able to list missions قائمة المأموريات, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-MSN-02 | UC-MSN-02 | As a General Director, I want to be able to filter missions by date البحث بالتاريخ, so that I can locate a record from partial information. | Search / filter | Should · 2 |
| US-MSN-03 | UC-MSN-03 | As a General Director, I want to be able to select the mission type نوع المأمورية, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-MSN-04 | UC-MSN-04 | As a General Director, I want to be able to select the interview type نوع المقابلة, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-MSN-05 | UC-MSN-05 | As a General Director, I want to be able to select the time type التوقيت, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-MSN-06 | UC-MSN-06 | As a General Director, I want to be able to create a mission تسجيل المأمورية, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 5 |
| US-MSN-07 | UC-MSN-07 | As a General Director, I want to be able to view / update a mission تعديل المأمورية, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |
| US-MSN-08 | UC-MSN-08 | As a General Director, I want to be able to delete a mission حذف المأمورية, so that records entered in error do not distort the register or the reporting. | Delete a record | Should · 2 |
| US-MSN-09 | UC-MSN-09 | As a General Director, I want to be able to register a mission result تسجيل نتيجة المأمورية, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 8 |


**Acceptance criteria**


| Story | US-MSN-01 (UC-MSN-01) |
| --- | --- |
| User story | As a General Director, I want to be able to list missions قائمة المأموريات, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/missions`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/MissionManagement/my-missions` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §20.S are implemented with their mandatory flags and lookups; the scenario of §20.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-MSN-02 (UC-MSN-02) |
| --- | --- |
| User story | As a General Director, I want to be able to filter missions by date البحث بالتاريخ, so that I can locate a record from partial information. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/MissionManagement` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §20.S are implemented with their mandatory flags and lookups; the scenario of §20.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-MSN-03 (UC-MSN-03) |
| --- | --- |
| User story | As a General Director, I want to be able to select the mission type نوع المأمورية, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/MissionManagement/mission-types` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §20.S are implemented with their mandatory flags and lookups; the scenario of §20.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-MSN-04 (UC-MSN-04) |
| --- | --- |
| User story | As a General Director, I want to be able to select the interview type نوع المقابلة, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/LookupManagement/mission-interview-types` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §20.S are implemented with their mandatory flags and lookups; the scenario of §20.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-MSN-05 (UC-MSN-05) |
| --- | --- |
| User story | As a General Director, I want to be able to select the time type التوقيت, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/MissionManagement/mission-time-types` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §20.S are implemented with their mandatory flags and lookups; the scenario of §20.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-MSN-06 (UC-MSN-06) |
| --- | --- |
| User story | As a General Director, I want to be able to create a mission تسجيل المأمورية, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/missions/:id/edit`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/MissionManagement` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §20.S are implemented with their mandatory flags and lookups; the scenario of §20.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-MSN-07 (UC-MSN-07) |
| --- | --- |
| User story | As a General Director, I want to be able to view / update a mission تعديل المأمورية, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/MissionManagement/{id}` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §20.S are implemented with their mandatory flags and lookups; the scenario of §20.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-MSN-08 (UC-MSN-08) |
| --- | --- |
| User story | As a General Director, I want to be able to delete a mission حذف المأمورية, so that records entered in error do not distort the register or the reporting. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then the record is no longer returned by the list and read endpoints of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `DELETE /api/MissionManagement` and the response is rendered on the screen without a page reload.<br>3. Given the actor requests deletion, when the confirmation is declined, then nothing is deleted.<br>4. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §20.S are implemented with their mandatory flags and lookups; the scenario of §20.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-MSN-09 (UC-MSN-09) |
| --- | --- |
| User story | As a General Director, I want to be able to register a mission result تسجيل نتيجة المأمورية, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 8 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/missions/:id/register`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/MissionManagement/{id}/event` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §20.S are implemented with their mandatory flags and lookups; the scenario of §20.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.16  EP-16 — Correspondence — Incoming & Outgoing


| Item | Value |
| --- | --- |
| Epic | EP-16 |
| Module | Correspondence — Incoming & Outgoing — الصادر والوارد |
| Chapter / prefix | Chapter 21 · UC-COR |
| Business goal | To keep the official letter register, and carry orphan periodic reports to and from the charities as letter attachments. |
| Stories | 19 (18 Should, 1 Could) |
| Points | 54 |
| Routes | `#/incoming-outgoing/incoming`, `#/incoming-outgoing/incoming/:id/edit`, `#/incoming-outgoing/export/incoming`, `#/incoming-outgoing/outgoing`, `#/incoming-outgoing/outgoing/:id/edit`, `#/incoming-outgoing/export/outgoing`, `#/incoming-outgoing/export/outgoing-orphans` |
| Specification | Module document 21-UC-COR-Correspondence-Incoming-and-Outgoing — §21.S for the screen contract, §21.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution) |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-COR-01 | UC-COR-01 | As a head-office staff member, I want to be able to list incoming letters الوارد, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-COR-02 | UC-COR-02 | As a head-office staff member, I want to be able to search incoming letters البحث في الوارد, so that I can locate a record from partial information. | Search / filter | Should · 2 |
| US-COR-03 | UC-COR-03 | As a head-office staff member, I want to be able to obtain the next incoming serial رقم الوارد التالي, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-COR-04 | UC-COR-04 | As a head-office staff member, I want to be able to register an incoming letter تسجيل وارد, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 5 |
| US-COR-05 | UC-COR-05 | As a head-office staff member, I want to be able to view an incoming letter عرض الوارد, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-COR-06 | UC-COR-06 | As a head-office staff member, I want to be able to update an incoming letter تعديل الوارد, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |
| US-COR-07 | UC-COR-07 | As a head-office staff member, I want to be able to delete an incoming letter حذف الوارد, so that records entered in error do not distort the register or the reporting. | Delete a record | Should · 2 |
| US-COR-08 | UC-COR-08 | As a head-office staff member, I want to be able to select the routing department الإدارة المختصة, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-COR-09 | UC-COR-09 | As a head-office staff member, I want to be able to attach employees to an incoming letter ربط الموظفين بخطاب وارد, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-COR-10 | UC-COR-10 | As a head-office staff member, I want to be able to list outgoing letters الصادر, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-COR-11 | UC-COR-11 | As a head-office staff member, I want to be able to search outgoing letters البحث في الصادر, so that I can locate a record from partial information. | Search / filter | Should · 2 |
| US-COR-12 | UC-COR-12 | As a head-office staff member, I want to be able to obtain the next outgoing serial رقم الصادر التالي, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-COR-13 | UC-COR-13 | As a head-office staff member, I want to be able to register an outgoing letter تسجيل صادر, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 5 |
| US-COR-14 | UC-COR-14 | As a head-office staff member, I want to be able to view an outgoing letter عرض الصادر, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-COR-15 | UC-COR-15 | As a head-office staff member, I want to be able to update an outgoing letter تعديل الصادر, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |
| US-COR-16 | UC-COR-16 | As a head-office staff member, I want to be able to delete an outgoing letter حذف الصادر, so that records entered in error do not distort the register or the reporting. | Delete a record | Should · 2 |
| US-COR-17 | UC-COR-17 | As a head-office staff member, I want to be able to select the outgoing category تصنيف الصادر, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-COR-18 | UC-COR-18 | As a head-office staff member, I want to be able to attach orphan reports to an outgoing letter إضافة تقارير الأيتام إلى خطاب صادر, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 5 |
| US-COR-19 | UC-COR-19 | As a head-office staff member, I want to be able to report orphans by outgoing letter تقرير الأيتام حسب الخطاب الصادر, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Could · 3 |


**Acceptance criteria**


| Story | US-COR-01 (UC-COR-01) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to list incoming letters الوارد, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session on the screen at `#/incoming-outgoing/incoming`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/IncomingOutgoing/incoming` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-02 (UC-COR-02) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to search incoming letters البحث في الوارد, so that I can locate a record from partial information. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session in the module, when the actor invokes the function with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/IncomingOutgoing/incoming` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-03 (UC-COR-03) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to obtain the next incoming serial رقم الوارد التالي, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/IncomingOutgoing/incoming/next-serial` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-04 (UC-COR-04) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to register an incoming letter تسجيل وارد, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session on the screen at `#/incoming-outgoing/incoming/:id/edit`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/IncomingOutgoing/incoming` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-05 (UC-COR-05) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to view an incoming letter عرض الوارد, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/IncomingOutgoing/incoming/{id}` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-06 (UC-COR-06) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to update an incoming letter تعديل الوارد, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/IncomingOutgoing/incoming` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-07 (UC-COR-07) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to delete an incoming letter حذف الوارد, so that records entered in error do not distort the register or the reporting. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session in the module, when the actor invokes the function with valid input, then the record is no longer returned by the list and read endpoints of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `DELETE /api/IncomingOutgoing/incoming` and the response is rendered on the screen without a page reload.<br>3. Given the actor requests deletion, when the confirmation is declined, then nothing is deleted.<br>4. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-08 (UC-COR-08) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to select the routing department الإدارة المختصة, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/LookupManagement/departments` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-09 (UC-COR-09) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to attach employees to an incoming letter ربط الموظفين بخطاب وارد, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session on the screen at `#/incoming-outgoing/export/incoming`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/IncomingOutgoing/incoming` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «Operation Faild» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-10 (UC-COR-10) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to list outgoing letters الصادر, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session on the screen at `#/incoming-outgoing/outgoing`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/IncomingOutgoing/outgoing` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-11 (UC-COR-11) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to search outgoing letters البحث في الصادر, so that I can locate a record from partial information. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session in the module, when the actor invokes the function with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/IncomingOutgoing/outgoing` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-12 (UC-COR-12) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to obtain the next outgoing serial رقم الصادر التالي, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/IncomingOutgoing/outgoing/next-serial` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-13 (UC-COR-13) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to register an outgoing letter تسجيل صادر, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session on the screen at `#/incoming-outgoing/outgoing/:id/edit`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/IncomingOutgoing/outgoing` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-14 (UC-COR-14) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to view an outgoing letter عرض الصادر, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/IncomingOutgoing/outgoing/{id}` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-15 (UC-COR-15) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to update an outgoing letter تعديل الصادر, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/IncomingOutgoing/outgoing` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-16 (UC-COR-16) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to delete an outgoing letter حذف الصادر, so that records entered in error do not distort the register or the reporting. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session in the module, when the actor invokes the function with valid input, then the record is no longer returned by the list and read endpoints of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `DELETE /api/IncomingOutgoing/outgoing` and the response is rendered on the screen without a page reload.<br>3. Given the actor requests deletion, when the confirmation is declined, then nothing is deleted.<br>4. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-17 (UC-COR-17) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to select the outgoing category تصنيف الصادر, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/IncomingOutgoing/outgoing/categories` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-18 (UC-COR-18) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to attach orphan reports to an outgoing letter إضافة تقارير الأيتام إلى خطاب صادر, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session on the screen at `#/incoming-outgoing/export/outgoing`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/IncomingOutgoing/outgoing/{parentOutgoingId}/children` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>8. Given the business rule behind «Operation Faild» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-COR-19 (UC-COR-19) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to report orphans by outgoing letter تقرير الأيتام حسب الخطاب الصادر, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session on the screen at `#/incoming-outgoing/export/outgoing-orphans`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/IncomingOutgoing/outgoing?orphanNumber=` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «Operation Faild» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §21.S are implemented with their mandatory flags and lookups; the scenario of §21.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.17  EP-17 — HQ Financial Transfers


| Item | Value |
| --- | --- |
| Epic | EP-17 |
| Module | HQ Financial Transfers — الحوالات المالية للادارة المالية |
| Chapter / prefix | Chapter 22 · UC-TRF |
| Business goal | To record the money head office sends to a charity or a country and what happened to it. |
| Stories | 8 (8 Should) |
| Points | 28 |
| Routes | `#/hq-transfers`, `#/hq-transfers/:id/edit`, `#/hq-transfers/:id/details`, `#/hq-transfers/max-amounts` |
| Specification | Module document 22-UC-TRF-HQ-Financial-Transfers — §22.S for the screen contract, §22.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution) |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-TRF-01 | UC-TRF-01 | As a Financial Director, I want to be able to list transfers قائمة الحوالات, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-TRF-02 | UC-TRF-02 | As a Financial Director, I want to be able to create a transfer اضافة حوالة, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 5 |
| US-TRF-03 | UC-TRF-03 | As a Financial Director, I want to be able to view a transfer عرض الحوالة, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-TRF-04 | UC-TRF-04 | As a Financial Director, I want to be able to update a transfer تعديل الحوالة, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |
| US-TRF-05 | UC-TRF-05 | As a Financial Director, I want to be able to select the issuing department الإدارة المصدرة, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-TRF-06 | UC-TRF-06 | As a Financial Director, I want to be able to view the maximum transfer amount for a country الحد الأعلى للحوالة, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-TRF-07 | UC-TRF-07 | As a Financial Director, I want to be able to set the maximum transfer amount تعيين الحد الأعلى للحوالة, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |
| US-TRF-08 | UC-TRF-08 | As a Financial Director, I want to be able to manage transfer detail lines تفاصيل الحوالة, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |


**Acceptance criteria**


| Story | US-TRF-01 (UC-TRF-01) |
| --- | --- |
| User story | As a Financial Director, I want to be able to list transfers قائمة الحوالات, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a Financial Director with an active session on the screen at `#/hq-transfers`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/HqTransfers` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §22.S are implemented with their mandatory flags and lookups; the scenario of §22.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-TRF-02 (UC-TRF-02) |
| --- | --- |
| User story | As a Financial Director, I want to be able to create a transfer اضافة حوالة, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a Financial Director with an active session on the screen at `#/hq-transfers/:id/edit`, when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/HqTransfers` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §22.S are implemented with their mandatory flags and lookups; the scenario of §22.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-TRF-03 (UC-TRF-03) |
| --- | --- |
| User story | As a Financial Director, I want to be able to view a transfer عرض الحوالة, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a Financial Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/HqTransfers/{id}` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §22.S are implemented with their mandatory flags and lookups; the scenario of §22.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-TRF-04 (UC-TRF-04) |
| --- | --- |
| User story | As a Financial Director, I want to be able to update a transfer تعديل الحوالة, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a Financial Director with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/HqTransfers` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §22.S are implemented with their mandatory flags and lookups; the scenario of §22.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-TRF-05 (UC-TRF-05) |
| --- | --- |
| User story | As a Financial Director, I want to be able to select the issuing department الإدارة المصدرة, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a Financial Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/LookupManagement/departments` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §22.S are implemented with their mandatory flags and lookups; the scenario of §22.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-TRF-06 (UC-TRF-06) |
| --- | --- |
| User story | As a Financial Director, I want to be able to view the maximum transfer amount for a country الحد الأعلى للحوالة, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a Financial Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/HqTransfers/max-amount` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §22.S are implemented with their mandatory flags and lookups; the scenario of §22.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-TRF-07 (UC-TRF-07) |
| --- | --- |
| User story | As a Financial Director, I want to be able to set the maximum transfer amount تعيين الحد الأعلى للحوالة, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a Financial Director with an active session on the screen at `#/hq-transfers/max-amounts`, when the actor presses «حفظ» with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `PUT /api/HqTransfers/max-amount` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §22.S are implemented with their mandatory flags and lookups; the scenario of §22.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-TRF-08 (UC-TRF-08) |
| --- | --- |
| User story | As a Financial Director, I want to be able to manage transfer detail lines تفاصيل الحوالة, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a Financial Director with an active session on the screen at `#/hq-transfers/:id/details`, when the actor presses «حفظ» with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/HqTransfers/{id}/details` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>8. Given the business rule behind «Failed Operation» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §22.S are implemented with their mandatory flags and lookups; the scenario of §22.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.18  EP-18 — Reports & Printing


| Item | Value |
| --- | --- |
| Epic | EP-18 |
| Module | Reports & Printing — التقارير والطباعة |
| Chapter / prefix | Chapter 23 · UC-RPT |
| Business goal | To answer the operational, compliance and financial questions asked of the register, and produce the printed documents the process depends on. |
| Stories | 41 (16 Should, 25 Could) |
| Points | 151 |
| Routes | `#/periodic-orphan-reports/orphan-reports/search`, `#/seasonal-aid/:id/report`, `#/reports/orphans`, `#/reports/excluded-orphans`, `#/reports/finished-sponsorship-orphans`, `#/reports/unsponsored-orphans`, `#/reports/registered-family-projects`, `#/reports/meza-cards`, `#/reports/widows-allowing-sponsorship`, `#/reports/family-orphans`, `#/reports/missed-payments`, `#/reports/charity-payment-tracking`, `#/reports/beneficiary-family-details`, `#/reports/orphans-missing-reports`, `#/reports/orphans-missing-files`, `#/reports/reports-awaiting-approval`, `#/reports/refused-reports`, `#/reports/orphan-files`, `#/reports/provider-sponsor-changes` |
| Specification | Module document 23-UC-RPT-Reports-and-Printing — §23.S for the screen contract, §23.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution) |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-RPT-01 | UC-RPT-01 | As a signed-in user, I want to be able to orphan data بيانات الأيتام, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-RPT-02 | UC-RPT-02 | As a signed-in user, I want to be able to orphan status حالة اليتيم, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-RPT-03 | UC-RPT-03 | As a signed-in user, I want to be able to excluded orphans المستبعدون, so that records entered in error do not distort the register or the reporting. | Delete a record | Should · 2 |
| US-RPT-04 | UC-RPT-04 | As a General Director, I want to be able to orphans with ended sponsorship أيتام انتهت كفالتهم, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-RPT-05 | UC-RPT-05 | As a General Director, I want to be able to unsponsored orphans أيتام غير مكفولين, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 5 |
| US-RPT-06 | UC-RPT-06 | As a signed-in user, I want to be able to widows requiring sponsorship أرامل مطلوب لهم كفالة, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Could · 3 |
| US-RPT-07 | UC-RPT-07 | As a General Director, I want to be able to registered Meza cards تقرير الكروت المسجلة, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 5 |
| US-RPT-08 | UC-RPT-08 | As a General Director, I want to be able to extract guardian Meza cards استخراج كروت العائل, so that the data can be handed to the bank, the auditor or the donor in the format they expect. | Export data | Could · 8 |
| US-RPT-09 | UC-RPT-09 | As a signed-in user, I want to be able to assistance family data بيانات أسر المساعدات, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-RPT-10 | UC-RPT-10 | As a General Director, I want to be able to beneficiary statistics احصائيات المستفيدين, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Could · 3 |
| US-RPT-11 | UC-RPT-11 | As a General Director, I want to be able to family projects مشاريع الأسر, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 5 |
| US-RPT-12 | UC-RPT-12 | As a signed-in user, I want to be able to guardian change history تقارير تعديل المعيل, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |
| US-RPT-13 | UC-RPT-13 | As a signed-in user, I want to be able to family and orphan entry tracking متابعة إدخالات الأسر والأيتام, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Could · 3 |
| US-RPT-14 | UC-RPT-14 | As a General Director, I want to be able to general orphan statistics احصائيات عامة للأيتام, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Could · 3 |
| US-RPT-15 | UC-RPT-15 | As a General Director, I want to be able to coded orphans needing a report أيتام مكودون مطلوب لهم تقرير, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Could · 3 |
| US-RPT-16 | UC-RPT-16 | As a General Director, I want to be able to orphans missing files أيتام مطلوب لهم ملفات, so that data produced outside the system is carried in without manual re-keying. | Import a file | Should · 8 |
| US-RPT-17 | UC-RPT-17 | As a General Director, I want to be able to reports awaiting approval تقارير في انتظار الموافقة, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Could · 3 |
| US-RPT-18 | UC-RPT-18 | As a signed-in user, I want to be able to refused reports تقارير تم رفضها, so that head office keeps control of what is accepted into the sponsorship cycle. | Review decision | Should · 8 |
| US-RPT-19 | UC-RPT-19 | As a General Director, I want to be able to orphans without a renewed report أيتام بدون تقرير مجدد, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Could · 3 |
| US-RPT-20 | UC-RPT-20 | As a General Director, I want to be able to charity follow-up متابعة الجمعيات, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Could · 3 |
| US-RPT-21 | UC-RPT-21 | As a General Director, I want to be able to family update tracking متابعة تحديث بيانات الأسر, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Could · 3 |
| US-RPT-22 | UC-RPT-22 | As a General Director, I want to be able to missed payments — current user scope أيتام مستحقون دفعات سابقة, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-RPT-23 | UC-RPT-23 | As a General Director, I want to be able to missed payments — all جميع الدفعات الفائتة, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-RPT-24 | UC-RPT-24 | As a signed-in user, I want to be able to export orphan photographs صور الأيتام, so that the data can be handed to the bank, the auditor or the donor in the format they expect. | Export data | Could · 8 |
| US-RPT-25 | UC-RPT-25 | As a signed-in user, I want to be able to export certificate images صور الشهادات, so that the data can be handed to the bank, the auditor or the donor in the format they expect. | Export data | Could · 8 |
| US-RPT-26 | UC-RPT-26 | As a signed-in user, I want to be able to print the survey questionnaire طباعة الاستبانة, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Could · 3 |
| US-RPT-27 | UC-RPT-27 | As a HQ roles, I want to be able to orphans in a payment batch أيتام الدفعة, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-RPT-28 | UC-RPT-28 | As a HQ roles, I want to be able to orphans receiving nothing أيتام لم يصرف لهم, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-RPT-29 | UC-RPT-29 | As a HQ roles, I want to be able to received / not received / stopped lists, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Could · 3 |
| US-RPT-30 | UC-RPT-30 | As a charity user, I want to be able to cheque numbers list أرقام الشيكات, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Could · 3 |
| US-RPT-31 | UC-RPT-31 | As a charity user, I want to be able to receipt cards كروت الاستلام, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Could · 3 |
| US-RPT-32 | UC-RPT-32 | As a HQ roles, I want to be able to payment summary pages صفحات ملخص الدفعة, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-RPT-33 | UC-RPT-33 | As a Financial Director, I want to be able to cheque statement report بيان الشيكات, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Could · 3 |
| US-RPT-34 | UC-RPT-34 | As a charity user, I want to be able to project distribution sheets كشوف توزيع المشاريع, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Could · 3 |
| US-RPT-35 | UC-RPT-35 | As a HQ roles, I want to be able to new orphans and new widows الأيتام والأرامل الجدد, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Could · 3 |
| US-RPT-36 | UC-RPT-36 | As a HQ roles, I want to be able to follow-up and handover sheets كشوف المتابعة والتسليم, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Could · 3 |
| US-RPT-37 | UC-RPT-37 | As a HQ roles, I want to be able to guardian and widow identification sheets كشوف تعريف العائل والأرامل, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Could · 3 |
| US-RPT-38 | UC-RPT-38 | As a head-office staff member, I want to be able to missing outgoing attachments مرفقات الصادر الناقصة, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Could · 3 |
| US-RPT-39 | UC-RPT-39 | As a HQ roles, I want to be able to family orphan list by date أيتام الأسر بتاريخ, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Could · 3 |
| US-RPT-40 | UC-RPT-40 | As a signed-in user, I want to be able to display a report in the browser عرض التقرير, so that I can answer the operational, compliance or financial question being asked of me. | Query a report | Could · 3 |
| US-RPT-41 | UC-RPT-41 | As a signed-in user, I want to be able to export a report to Excel تصدير إلى إكسل, so that the data can be handed to the bank, the auditor or the donor in the format they expect. | Export data | Could · 8 |


**Acceptance criteria**


| Story | US-RPT-01 (UC-RPT-01) |
| --- | --- |
| User story | As a signed-in user, I want to be able to orphan data بيانات الأيتام, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a signed-in user with an active session on the screen at `#/reports/orphans`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/orphans` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-02 (UC-RPT-02) |
| --- | --- |
| User story | As a signed-in user, I want to be able to orphan status حالة اليتيم, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a signed-in user with an active session on the screen at `#/periodic-orphan-reports/orphan-reports/search`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/PeriodicOrphanReports/{id}/review` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>4. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-03 (UC-RPT-03) |
| --- | --- |
| User story | As a signed-in user, I want to be able to excluded orphans المستبعدون, so that records entered in error do not distort the register or the reporting. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a signed-in user with an active session on the screen at `#/reports/excluded-orphans`, when the actor presses «بحث» with valid input, then the record is no longer returned by the list and read endpoints of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/excluded-orphans` and the response is rendered on the screen without a page reload.<br>3. Given the actor requests deletion, when the confirmation is declined, then nothing is deleted.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>7. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-04 (UC-RPT-04) |
| --- | --- |
| User story | As a General Director, I want to be able to orphans with ended sponsorship أيتام انتهت كفالتهم, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/reports/finished-sponsorship-orphans`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/finished-sponsorship-orphans` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-05 (UC-RPT-05) |
| --- | --- |
| User story | As a General Director, I want to be able to unsponsored orphans أيتام غير مكفولين, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/reports/unsponsored-orphans`, when the actor presses «بحث» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/unsponsored-orphans` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-06 (UC-RPT-06) |
| --- | --- |
| User story | As a signed-in user, I want to be able to widows requiring sponsorship أرامل مطلوب لهم كفالة, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a signed-in user with an active session on the screen at `#/reports/widows-allowing-sponsorship`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/widows-allowing-sponsorship` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-07 (UC-RPT-07) |
| --- | --- |
| User story | As a General Director, I want to be able to registered Meza cards تقرير الكروت المسجلة, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/reports/meza-cards`, when the actor presses «بحث» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/meza-cards` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>8. Given the business rule behind «Faild Operation» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-08 (UC-RPT-08) |
| --- | --- |
| User story | As a General Director, I want to be able to extract guardian Meza cards استخراج كروت العائل, so that the data can be handed to the bank, the auditor or the donor in the format they expect. |
| Priority / size | Could · 8 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then a workbook has been delivered to the actor. No stored data is changed.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/meza-cards` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-09 (UC-RPT-09) |
| --- | --- |
| User story | As a signed-in user, I want to be able to assistance family data بيانات أسر المساعدات, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a signed-in user with an active session on the screen at `#/reports/beneficiary-family-details`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/beneficiary-family-details` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-10 (UC-RPT-10) |
| --- | --- |
| User story | As a General Director, I want to be able to beneficiary statistics احصائيات المستفيدين, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/seasonal-aid/:id/report`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-11 (UC-RPT-11) |
| --- | --- |
| User story | As a General Director, I want to be able to family projects مشاريع الأسر, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/reports/registered-family-projects`, when the actor presses «بحث» with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/registered-family-projects` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>8. Given the business rule behind «Faild Operation» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-12 (UC-RPT-12) |
| --- | --- |
| User story | As a signed-in user, I want to be able to guardian change history تقارير تعديل المعيل, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a signed-in user with an active session on the screen at `#/reports/provider-sponsor-changes`, when the actor presses «حفظ» with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/provider-sponsor-changes` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-13 (UC-RPT-13) |
| --- | --- |
| User story | As a signed-in user, I want to be able to family and orphan entry tracking متابعة إدخالات الأسر والأيتام, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a signed-in user with an active session on the screen at `#/reports/family-orphans`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/{id}/follow-up` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-14 (UC-RPT-14) |
| --- | --- |
| User story | As a General Director, I want to be able to general orphan statistics احصائيات عامة للأيتام, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/periodic-orphan-reports/orphan-reports`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-15 (UC-RPT-15) |
| --- | --- |
| User story | As a General Director, I want to be able to coded orphans needing a report أيتام مكودون مطلوب لهم تقرير, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/reports/orphans-missing-reports`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/orphans-missing-reports` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-16 (UC-RPT-16) |
| --- | --- |
| User story | As a General Director, I want to be able to orphans missing files أيتام مطلوب لهم ملفات, so that data produced outside the system is carried in without manual re-keying. |
| Priority / size | Should · 8 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/reports/orphans-missing-files`, when the actor presses «حفظ» with valid input, then the matched records carry the imported values; unmatched rows are left untouched and reported.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/orphans-missing-files` and the response is rendered on the screen without a page reload.<br>3. Given the uploaded file does not match the expected layout, when the import runs, then no row is changed and the actor is told why.<br>4. Given some rows cannot be matched to an existing record, when the import completes, then those rows are reported back and the rest are applied.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-17 (UC-RPT-17) |
| --- | --- |
| User story | As a General Director, I want to be able to reports awaiting approval تقارير في انتظار الموافقة, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/reports/reports-awaiting-approval`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/reports-awaiting-approval` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-18 (UC-RPT-18) |
| --- | --- |
| User story | As a signed-in user, I want to be able to refused reports تقارير تم رفضها, so that head office keeps control of what is accepted into the sponsorship cycle. |
| Priority / size | Should · 8 points |
| Acceptance criteria | 1. Given a signed-in user with an active session on the screen at `#/reports/refused-reports`, when the actor presses «حفظ» with valid input, then the item carries its new state, the deciding user and the decision date, and moves out of the pending queue.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/refused-reports` and the response is rendered on the screen without a page reload.<br>3. Given the decision is a refusal, when no reason is given, then the refusal is not accepted.<br>4. Given the decision is recorded, when the charity opens the item, then it sees the new state and, on refusal, the reason.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-19 (UC-RPT-19) |
| --- | --- |
| User story | As a General Director, I want to be able to orphans without a renewed report أيتام بدون تقرير مجدد, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/non-renewed-reports` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-20 (UC-RPT-20) |
| --- | --- |
| User story | As a General Director, I want to be able to charity follow-up متابعة الجمعيات, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/reports/charity-payment-tracking`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/charity-payment-tracking` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-21 (UC-RPT-21) |
| --- | --- |
| User story | As a General Director, I want to be able to family update tracking متابعة تحديث بيانات الأسر, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/family-update-tracking/export/pdf` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-22 (UC-RPT-22) |
| --- | --- |
| User story | As a General Director, I want to be able to missed payments — current user scope أيتام مستحقون دفعات سابقة, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session on the screen at `#/reports/missed-payments`, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/missed-payments` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-23 (UC-RPT-23) |
| --- | --- |
| User story | As a General Director, I want to be able to missed payments — all جميع الدفعات الفائتة, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/missed-payments` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-24 (UC-RPT-24) |
| --- | --- |
| User story | As a signed-in user, I want to be able to export orphan photographs صور الأيتام, so that the data can be handed to the bank, the auditor or the donor in the format they expect. |
| Priority / size | Could · 8 points |
| Acceptance criteria | 1. Given a signed-in user with an active session on the screen at `#/reports/orphan-files`, when the actor presses «ExportReportData» with valid input, then a workbook has been delivered to the actor. No stored data is changed.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/orphan-files/export` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-25 (UC-RPT-25) |
| --- | --- |
| User story | As a signed-in user, I want to be able to export certificate images صور الشهادات, so that the data can be handed to the bank, the auditor or the donor in the format they expect. |
| Priority / size | Could · 8 points |
| Acceptance criteria | 1. Given a signed-in user with an active session in the module, when the actor invokes the function with valid input, then a workbook has been delivered to the actor. No stored data is changed.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/certificate-files/export` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-26 (UC-RPT-26) |
| --- | --- |
| User story | As a signed-in user, I want to be able to print the survey questionnaire طباعة الاستبانة, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a signed-in user with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-27 (UC-RPT-27) |
| --- | --- |
| User story | As a HQ roles, I want to be able to orphans in a payment batch أيتام الدفعة, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/OrphanPayments/{id}/details` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-28 (UC-RPT-28) |
| --- | --- |
| User story | As a HQ roles, I want to be able to orphans receiving nothing أيتام لم يصرف لهم, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/orphans-without-payment` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>6. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-29 (UC-RPT-29) |
| --- | --- |
| User story | As a HQ roles, I want to be able to received / not received / stopped lists, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/payments-received/export/pdf` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>7. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-30 (UC-RPT-30) |
| --- | --- |
| User story | As a charity user, I want to be able to cheque numbers list أرقام الشيكات, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/cheque-numbers/export/pdf` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>7. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-31 (UC-RPT-31) |
| --- | --- |
| User story | As a charity user, I want to be able to receipt cards كروت الاستلام, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/receipt-cards/export/pdf` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>7. Given the business rule behind «Faliure» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-32 (UC-RPT-32) |
| --- | --- |
| User story | As a HQ roles, I want to be able to payment summary pages صفحات ملخص الدفعة, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Dashboard/payment-summary` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-33 (UC-RPT-33) |
| --- | --- |
| User story | As a Financial Director, I want to be able to cheque statement report بيان الشيكات, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a Financial Director with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/cheque-statement/export/pdf` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-34 (UC-RPT-34) |
| --- | --- |
| User story | As a charity user, I want to be able to project distribution sheets كشوف توزيع المشاريع, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/family-cards/export/pdf` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen.<br>7. Given the business rule behind «Operation Faild» is broken, when the operation is attempted, then it is refused with that message and nothing is written. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-35 (UC-RPT-35) |
| --- | --- |
| User story | As a HQ roles, I want to be able to new orphans and new widows الأيتام والأرامل الجدد, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-36 (UC-RPT-36) |
| --- | --- |
| User story | As a HQ roles, I want to be able to follow-up and handover sheets كشوف المتابعة والتسليم, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-37 (UC-RPT-37) |
| --- | --- |
| User story | As a HQ roles, I want to be able to guardian and widow identification sheets كشوف تعريف العائل والأرامل, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/guardian-identification-sheets/export/pdf` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-38 (UC-RPT-38) |
| --- | --- |
| User story | As a head-office staff member, I want to be able to missing outgoing attachments مرفقات الصادر الناقصة, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a head-office staff member with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-39 (UC-RPT-39) |
| --- | --- |
| User story | As a HQ roles, I want to be able to family orphan list by date أيتام الأسر بتاريخ, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Reports/family-orphans/export/pdf` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>5. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>6. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-40 (UC-RPT-40) |
| --- | --- |
| User story | As a signed-in user, I want to be able to display a report in the browser عرض التقرير, so that I can answer the operational, compliance or financial question being asked of me. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a signed-in user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-RPT-41 (UC-RPT-41) |
| --- | --- |
| User story | As a signed-in user, I want to be able to export a report to Excel تصدير إلى إكسل, so that the data can be handed to the bank, the auditor or the donor in the format they expect. |
| Priority / size | Could · 8 points |
| Acceptance criteria | 1. Given a signed-in user with an active session in the module, when the actor invokes the function with valid input, then a workbook has been delivered to the actor. No stored data is changed.<br>2. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §23.S are implemented with their mandatory flags and lookups; the scenario of §23.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

### 3.19  EP-19 — Cross-Cutting Services


| Item | Value |
| --- | --- |
| Epic | EP-19 |
| Module | Cross-Cutting Services — الخدمات المشتركة |
| Chapter / prefix | Chapter 24 · UC-SYS |
| Business goal | To provide the attachment handling, reference data and validation that every other module depends on. |
| Stories | 13 (12 Should, 1 Could) |
| Points | 45 |
| Routes | `#/error` |
| Specification | Module document 24-UC-SYS-Cross-Cutting-Services — §24.S for the screen contract, §24.U for the scenarios |
| Depends on | EP-01 (authentication and role resolution) |

Stories in this epic:

| Story | Use case | User story | Type | MoSCoW · pts |
| --- | --- | --- | --- | --- |
| US-SYS-01 | UC-SYS-01 | As a charity user, I want to be able to upload a file رفع ملف, so that data produced outside the system is carried in without manual re-keying. | Import a file | Should · 8 |
| US-SYS-02 | UC-SYS-02 | As a signed-in user, I want to be able to download a file تحميل ملف, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-SYS-03 | UC-SYS-03 | As a system, I want to be able to save a generated report file حفظ ملف التقرير, so that the paper document the process depends on can be produced and filed. | Print / produce a document | Could · 3 |
| US-SYS-04 | UC-SYS-04 | As a signed-in user, I want to be able to load a generic lookup list القوائم المرجعية, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-SYS-05 | UC-SYS-05 | As a signed-in user, I want to be able to load guardian lookup lists قوائم العائل, so that I can find the record I need without leaving the system. | Browse a list | Should · 2 |
| US-SYS-06 | UC-SYS-06 | As a General Director, I want to be able to load refusal reasons أسباب الرفض, so that head office keeps control of what is accepted into the sponsorship cycle. | Review decision | Should · 8 |
| US-SYS-07 | UC-SYS-07 | As a signed-in user, I want to be able to select country, region and centre الدولة والمنطقة والمركز, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-SYS-08 | UC-SYS-08 | As a Financial Director, I want to be able to select a bank اختيار البنك, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-SYS-09 | UC-SYS-09 | As a charity user, I want to be able to select a job اختيار المهنة, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-SYS-10 | UC-SYS-10 | As a HQ roles, I want to be able to select the orphan payment category فئة الصرف, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-SYS-11 | UC-SYS-11 | As a signed-in user, I want to be able to load country validation rules قواعد التحقق للدول, so that I can see the full detail of a single record before acting on it. | Read a record | Should · 2 |
| US-SYS-12 | UC-SYS-12 | As a charity user, I want to be able to check a national ID is unique التحقق من الرقم القومي, so that a record that was entered wrongly or has changed can be corrected. | Update a record | Should · 5 |
| US-SYS-13 | UC-SYS-13 | As a system, I want to be able to log and surface an error تسجيل الأخطاء, so that the register reflects reality as soon as the fact is known. | Create a record | Should · 5 |


**Acceptance criteria**


| Story | US-SYS-01 (UC-SYS-01) |
| --- | --- |
| User story | As a charity user, I want to be able to upload a file رفع ملف, so that data produced outside the system is carried in without manual re-keying. |
| Priority / size | Should · 8 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then the matched records carry the imported values; unmatched rows are left untouched and reported.<br>2. Given the request is accepted, when it is served, then it is handled by `POST /api/Attachments` and the response is rendered on the screen without a page reload.<br>3. Given the uploaded file does not match the expected layout, when the import runs, then no row is changed and the actor is told why.<br>4. Given some rows cannot be matched to an existing record, when the import completes, then those rows are reported back and the rest are applied.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §24.S are implemented with their mandatory flags and lookups; the scenario of §24.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-SYS-02 (UC-SYS-02) |
| --- | --- |
| User story | As a signed-in user, I want to be able to download a file تحميل ملف, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a signed-in user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Attachments/{id}/download` and the response is rendered on the screen without a page reload.<br>3. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §24.S are implemented with their mandatory flags and lookups; the scenario of §24.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-SYS-03 (UC-SYS-03) |
| --- | --- |
| User story | As a system, I want to be able to save a generated report file حفظ ملف التقرير, so that the paper document the process depends on can be produced and filed. |
| Priority / size | Could · 3 points |
| Acceptance criteria | 1. Given a system with an active session in the module, when the actor invokes the function with valid input, then a printable document has been produced. Where the module records printing, the row is flagged as printed.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Attachments` and the response is rendered on the screen without a page reload.<br>3. Given the selection returns no row, when the document is produced, then the actor is told that there is nothing to produce rather than receiving an empty file.<br>4. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §24.S are implemented with their mandatory flags and lookups; the scenario of §24.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-SYS-04 (UC-SYS-04) |
| --- | --- |
| User story | As a signed-in user, I want to be able to load a generic lookup list القوائم المرجعية, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a signed-in user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/LookupManagement` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §24.S are implemented with their mandatory flags and lookups; the scenario of §24.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-SYS-05 (UC-SYS-05) |
| --- | --- |
| User story | As a signed-in user, I want to be able to load guardian lookup lists قوائم العائل, so that I can find the record I need without leaving the system. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a signed-in user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/LookupManagement` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §24.S are implemented with their mandatory flags and lookups; the scenario of §24.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-SYS-06 (UC-SYS-06) |
| --- | --- |
| User story | As a General Director, I want to be able to load refusal reasons أسباب الرفض, so that head office keeps control of what is accepted into the sponsorship cycle. |
| Priority / size | Should · 8 points |
| Acceptance criteria | 1. Given a General Director with an active session in the module, when the actor invokes the function with valid input, then the item carries its new state, the deciding user and the decision date, and moves out of the pending queue.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/LookupManagement/general-reasons` and the response is rendered on the screen without a page reload.<br>3. Given the decision is a refusal, when no reason is given, then the refusal is not accepted.<br>4. Given the decision is recorded, when the charity opens the item, then it sees the new state and, on refusal, the reason.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §24.S are implemented with their mandatory flags and lookups; the scenario of §24.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-SYS-07 (UC-SYS-07) |
| --- | --- |
| User story | As a signed-in user, I want to be able to select country, region and centre الدولة والمنطقة والمركز, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a signed-in user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/LookupManagement/countries` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §24.S are implemented with their mandatory flags and lookups; the scenario of §24.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-SYS-08 (UC-SYS-08) |
| --- | --- |
| User story | As a Financial Director, I want to be able to select a bank اختيار البنك, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a Financial Director with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/LookupManagement/countries` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §24.S are implemented with their mandatory flags and lookups; the scenario of §24.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-SYS-09 (UC-SYS-09) |
| --- | --- |
| User story | As a charity user, I want to be able to select a job اختيار المهنة, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Reports` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §24.S are implemented with their mandatory flags and lookups; the scenario of §24.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-SYS-10 (UC-SYS-10) |
| --- | --- |
| User story | As a HQ roles, I want to be able to select the orphan payment category فئة الصرف, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a HQ roles with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/OrphanPayments?orphanId=` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §24.S are implemented with their mandatory flags and lookups; the scenario of §24.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-SYS-11 (UC-SYS-11) |
| --- | --- |
| User story | As a signed-in user, I want to be able to load country validation rules قواعد التحقق للدول, so that I can see the full detail of a single record before acting on it. |
| Priority / size | Should · 2 points |
| Acceptance criteria | 1. Given a signed-in user with an active session in the module, when the actor opens the screen with valid input, then no stored data is changed — the operation is a read.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/LookupManagement/countries` and the response is rendered on the screen without a page reload.<br>3. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>5. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §24.S are implemented with their mandatory flags and lookups; the scenario of §24.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-SYS-12 (UC-SYS-12) |
| --- | --- |
| User story | As a charity user, I want to be able to check a national ID is unique التحقق من الرقم القومي, so that a record that was entered wrongly or has changed can be corrected. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a charity user with an active session in the module, when the actor invokes the function with valid input, then the stored record carries the new values; no other record is affected.<br>2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/check-national-id` and the response is rendered on the screen without a page reload.<br>3. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>4. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>5. Given a charity user, when the function is invoked, then only records owned by that charity (and country) are returned or affected.<br>6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that charity’s data.<br>7. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §24.S are implemented with their mandatory flags and lookups; the scenario of §24.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |


| Story | US-SYS-13 (UC-SYS-13) |
| --- | --- |
| User story | As a system, I want to be able to log and surface an error تسجيل الأخطاء, so that the register reflects reality as soon as the fact is known. |
| Priority / size | Should · 5 points |
| Acceptance criteria | 1. Given a system with an active session in the module, when the actor invokes the function with valid input, then a new record exists, owned by the charity of the creating user, and appears in the list screen of the module.<br>2. Given a mandatory field listed in the screen field specification is empty, when the actor saves, then the save is refused and the offending field is flagged.<br>3. Given the save succeeds, when the actor returns to the list screen, then the record appears there with the values just entered.<br>4. Given the session has expired or the role is not permitted, when the function is invoked, then the request is rejected and the actor is routed back to the login screen. |
| Definition of done | The screen fields of §24.S are implemented with their mandatory flags and lookups; the scenario of §24.U passes end to end; the role and charity scoping is enforced server-side, not only in the menu. |

## 4. Traceability — story to use case to realisation


| Story | Use case | Epic | Route | Endpoint | Application service |
| --- | --- | --- | --- | --- | --- |
| US-AUT-01 | UC-AUT-01 | EP-01 | `#/auth/login` | POST /api/Auth/login | IAuthService.Login |
| US-AUT-02 | UC-AUT-02 | EP-01 | — | GET /api/Auth/me | IAuthService.GetUserDetail |
| US-AUT-03 | UC-AUT-03 | EP-01 | — | — | — |
| US-AUT-04 | UC-AUT-04 | EP-01 | `#/auth/register` | POST /api/Auth/register | IAuthService.Register |
| US-AUT-05 | UC-AUT-05 | EP-01 | `#/auth/forgot-password` | GET /api/Auth/forgot-password | IAuthService.GenerateUrl |
| US-AUT-06 | UC-AUT-06 | EP-01 | `#/auth/reset-password/:userId/:code` | GET /api/Auth/validate-reset-token | IAuthService.ValidateForPassparams |
| US-AUT-07 | UC-AUT-07 | EP-01 | `#/auth/reset-password/:userId/:code` | POST /api/Auth/change-password | IAuthService.UpdatePassword |
| US-AUT-08 | UC-AUT-08 | EP-01 | `#/auth/change-password` | POST /api/Auth/change-password | IAuthService.UpdatePassword |
| US-AUT-09 | UC-AUT-09 | EP-01 | — | — | — |
| US-DSH-01 | UC-DSH-01 | EP-02 | `#/dashboard` | GET /api/Dashboard/summary | IFamilyService.GetFamiliesCount |
| US-DSH-02 | UC-DSH-02 | EP-02 | — | GET /api/Dashboard/charts | IFamilyService.GetDataForBieChart |
| US-DSH-03 | UC-DSH-03 | EP-02 | — | — | — |
| US-CHR-01 | UC-CHR-01 | EP-03 | `#/charities` | GET /api/Charities | ICharityService.GetCharities |
| US-CHR-02 | UC-CHR-02 | EP-03 | — | GET /api/Charities/check-name | ICharityService.CheckCharityName |
| US-CHR-03 | UC-CHR-03 | EP-03 | `#/charities/create` | POST /api/Charities | ICharityService.AddCharity |
| US-CHR-04 | UC-CHR-04 | EP-03 | — | GET /api/Charities/{id} | ICharityService.GetCharityById |
| US-CHR-05 | UC-CHR-05 | EP-03 | — | PUT /api/Charities/{id} | ICharityService.UpdateCharity |
| US-CHR-06 | UC-CHR-06 | EP-03 | — | GET /api/Charities | ICharityService.GetCharities |
| US-CHR-07 | UC-CHR-07 | EP-03 | — | POST /api/Charities/{id}/lock · POST /api/Charities/{id}/unlock | ICharityService.LockCharity |
| US-CHR-08 | UC-CHR-08 | EP-03 | — | POST /api/Charities/{id}/lock · POST /api/Charities/{id}/unlock | ICharityService.LockCharity |
| US-CHR-09 | UC-CHR-09 | EP-03 | — | POST /api/Charities/{id}/lock · POST /api/Charities/{id}/unlock | ICharityService.LockCharity |
| US-EMP-01 | UC-EMP-01 | EP-04 | `#/employees` | GET /api/EmployeeManagement | IEmployeeService.GetEmployeeDetails |
| US-EMP-02 | UC-EMP-02 | EP-04 | — | GET /api/EmployeeManagement/check-national-id | IEmployeeService.CanAddEmployee |
| US-EMP-03 | UC-EMP-03 | EP-04 | `#/employees/:id` | POST /api/EmployeeManagement | IEmployeeService.AddEmployee |
| US-EMP-04 | UC-EMP-04 | EP-04 | — | GET /api/EmployeeManagement/{id} | IEmployeeService.GetEmployeeById |
| US-EMP-05 | UC-EMP-05 | EP-04 | — | PUT /api/EmployeeManagement | IEmployeeService.UpdateEmployee |
| US-EMP-06 | UC-EMP-06 | EP-04 | — | PATCH /api/EmployeeManagement/{id}/deactivate | IEmployeeService.StopEmployee |
| US-FAM-01 | UC-FAM-01 | EP-05 | `#/families` | GET /api/Families?charityId= | IFamilyService.GetFamilies |
| US-FAM-02 | UC-FAM-02 | EP-05 | — | GET /api/Families?charityId=&search= | IFamilyService.GetFamilies |
| US-FAM-03 | UC-FAM-03 | EP-05 | `#/families/:id/edit` | POST /api/Families | IFamilyService.AddNewFamily |
| US-FAM-04 | UC-FAM-04 | EP-05 | — | GET /api/Families/{id} | IFamilyService.GetFamily |
| US-FAM-05 | UC-FAM-05 | EP-05 | — | PUT /api/Families | IFamilyService.UpdateFamily |
| US-FAM-06 | UC-FAM-06 | EP-05 | — | PUT /api/Families/{id}/charity | IFamilyService.TransferFamilyToCharity |
| US-FAM-07 | UC-FAM-07 | EP-05 | `#/families/:id/members` | POST /api/Families/{familyId}/members/{memberId}/control | IFamilyService.ControlFamilyMember |
| US-FAM-08 | UC-FAM-08 | EP-05 | — | POST /api/Families/{familyId}/members/{memberId}/control | IFamilyService.ControlFamilyMember |
| US-FAM-09 | UC-FAM-09 | EP-05 | `#/families/provider-requests` | GET /api/Families/provider-requests | IFamilyService.TransferFamilyToCharity |
| US-FAM-10 | UC-FAM-10 | EP-05 | — | POST /api/Families/provider-requests/{id}/approve | IFamilyService.TransferFamilyToCharity |
| US-FAM-11 | UC-FAM-11 | EP-05 | `#/reports/family-orphans` | GET /api/Families/{id}/follow-up | IReportService.GetFamilyFollowUpDetails |
| US-FAM-12 | UC-FAM-12 | EP-05 | — | POST /api/Families/{familyId}/verify-provider | IFamilyService.CanAddParent |
| US-FAM-13 | UC-FAM-13 | EP-05 | — | DELETE /api/Families/{familyId}/provider/sponsor | IFamilyService.DeleteParent |
| US-FAM-14 | UC-FAM-14 | EP-05 | — | POST /api/Reports/family-update-tracking/export/pdf | IOrphanPaymentService.GetFamilyUpdateTracking |
| US-HOU-01 | UC-HOU-01 | EP-06 | `#/housing-projects` | GET /api/Families?familyType=Housing&charityId= | IFamilyService.GetAllHousingFamilyById |
| US-HOU-02 | UC-HOU-02 | EP-06 | — | GET /api/Families?familyType=Housing&search= | IFamilyService.GetHousingFamilies |
| US-HOU-03 | UC-HOU-03 | EP-06 | `#/housing-projects/:id/edit` | POST /api/HousingProjects/projects | IFamilyService.AddNewHousingFamily |
| US-HOU-04 | UC-HOU-04 | EP-06 | — | GET /api/HousingProjects/projects/{id} | IFamilyService.GetHousingFamily |
| US-HOU-05 | UC-HOU-05 | EP-06 | — | GET /api/LookupManagement/housing-buildings | ILookupService.GetHousingBuildings |
| US-HOU-06 | UC-HOU-06 | EP-06 | `#/housing-projects/:id/reports` | GET /api/PeriodicOrphanReports/by-orphan/{orphanId} | IPeriodicOrphanReportService.GetHousingOrpReportsById |
| US-HOU-07 | UC-HOU-07 | EP-06 | — | GET /api/PeriodicOrphanReports/by-orphan/{orphanId} | IOrphanService.GetHousingChildByCode |
| US-HOU-08 | UC-HOU-08 | EP-06 | `#/housing-projects/:id/reports/:reportId` | POST /api/PeriodicOrphanReports | IPeriodicOrphanReportService.AddNewHousingOrphanReport |
| US-REF-01 | UC-REF-01 | EP-07 | `#/families/refugees` | — | — |
| US-REF-02 | UC-REF-02 | EP-07 | — | GET /api/Families?familyType=Refugee&search= | IFamilyService.GetFamiliesRefugees |
| US-REF-03 | UC-REF-03 | EP-07 | `#/families/refugees/:id/edit` | POST /api/Families | IFamilyService.AddNewRefugeesFamily |
| US-REF-04 | UC-REF-04 | EP-07 | — | GET /api/Families/{id} | IFamilyService.GetRefugeesFamily |
| US-ORP-01 | UC-ORP-01 | EP-08 | — | GET /api/Families/orphans/check-national-id | IOrphanService.CanAddChild |
| US-ORP-02 | UC-ORP-02 | EP-08 | — | GET /api/Families/orphans?search= | IOrphanService.GetChildByName |
| US-ORP-03 | UC-ORP-03 | EP-08 | `#/families/orphans/coding/worklist` | GET /api/Families/orphans?codingStatus=Pending | IOrphanService.GetAllChildrenPerEncodeing |
| US-ORP-04 | UC-ORP-04 | EP-08 | `#/families/orphans/coding` | GET /api/Families/orphans?search= | IOrphanService.GetAllChildrenByName |
| US-ORP-05 | UC-ORP-05 | EP-08 | — | GET /api/Families/orphans/check-code | IOrphanService.ChildCodeIsUniqe |
| US-ORP-06 | UC-ORP-06 | EP-08 | — | POST /api/Families/orphans/{orphanId}/code | IOrphanService.SetOrphanCode |
| US-ORP-07 | UC-ORP-07 | EP-08 | — | GET /api/Families/orphans?search= | IOrphanService.GetChildNameAndCode |
| US-ORP-08 | UC-ORP-08 | EP-08 | — | GET /api/OrphanPayments?orphanId= | IOrphanPaymentService.GetPaymentByOrphanId |
| US-ORP-09 | UC-ORP-09 | EP-08 | — | GET /api/OrphanPayments | IOrphanPaymentService.GetOrphansInPaymentBatch |
| US-ORP-10 | UC-ORP-10 | EP-08 | — | GET /api/Families/{familyId}/provider/check-phone | IFamilyService.CanAddPhoneNumber |
| US-ORP-11 | UC-ORP-11 | EP-08 | — | GET /api/OrphanPayments | IOrphanPaymentService.GetBatchNos |
| US-ORR-01 | UC-ORR-01 | EP-09 | `#/periodic-orphan-reports` | GET /api/OrphanReports | IPeriodicOrphanReportService.GetOrphanStatuses |
| US-ORR-02 | UC-ORR-02 | EP-09 | — | GET /api/PeriodicOrphanReports/by-orphan/{orphanId} | IOrphanService.GetChildByCode |
| US-ORR-03 | UC-ORR-03 | EP-09 | `#/periodic-orphan-reports/:id/edit` | POST /api/PeriodicOrphanReports | IPeriodicOrphanReportService.AddNewOrphanReport |
| US-ORR-04 | UC-ORR-04 | EP-09 | — | GET /api/PeriodicOrphanReports | IPeriodicOrphanReportService.GetOrphanStatus |
| US-ORR-05 | UC-ORR-05 | EP-09 | — | PUT /api/PeriodicOrphanReports | IPeriodicOrphanReportService.UpdateOrphanReport |
| US-ORR-06 | UC-ORR-06 | EP-09 | — | DELETE /api/PeriodicOrphanReports | IPeriodicOrphanReportService.DeleteOrphanStatus |
| US-ORR-07 | UC-ORR-07 | EP-09 | — | PUT /api/PeriodicOrphanReports | IPeriodicOrphanReportService.UpdateOrphanReport |
| US-ORR-08 | UC-ORR-08 | EP-09 | — | PUT /api/PeriodicOrphanReports | IPeriodicOrphanReportService.UpdateOrphanReport |
| US-ORR-09 | UC-ORR-09 | EP-09 | `#/periodic-orphan-reports/orphan-reports/search` | POST /api/PeriodicOrphanReports/{id}/review | IPeriodicOrphanReportService.GetOrphanStatusRefinedStatus |
| US-ORR-10 | UC-ORR-10 | EP-09 | `#/periodic-orphan-reports/orphan-reports` | POST /api/OrphanReports/statistics | IFamilyService.TransferFamilyToCharity |
| US-ORR-11 | UC-ORR-11 | EP-09 | — | POST /api/OrphanReports/generate | IPeriodicOrphanReportService.GetOrphanStatusDetailed |
| US-ORR-12 | UC-ORR-12 | EP-09 | — | GET /api/PeriodicOrphanReports/approved | IPeriodicOrphanReportService.GetAcceptedOrphanStatusDetailed |
| US-ORR-13 | UC-ORR-13 | EP-09 | — | GET /api/PeriodicOrphanReports/rejected | IPeriodicOrphanReportService.GetRefusedOrphanStatusDetailed |
| US-ORR-14 | UC-ORR-14 | EP-09 | — | POST /api/Reports/non-renewed-reports | IPeriodicOrphanReportService.GetOrphanNonRenewedReport |
| US-ORR-15 | UC-ORR-15 | EP-09 | — | POST /api/OrphanReports/statistics | IPeriodicOrphanReportService.ExtractReportNumbersThatAdded |
| US-ORR-16 | UC-ORR-16 | EP-09 | — | GET /api/Attachments/{id}/image | IPeriodicOrphanReportService.GetOrphanImages |
| US-ORR-17 | UC-ORR-17 | EP-09 | — | POST /api/Reports/orphan-report-form/export/pdf | IAttachmentService.GetImage |
| US-PAY-01 | UC-PAY-01 | EP-10 | `#/orphan-payments` | GET /api/OrphanPayments | IOrphanPaymentService.GetAllOrphanPayment |
| US-PAY-02 | UC-PAY-02 | EP-10 | `#/orphan-payments/:id/edit` | POST /api/OrphanPayments | IOrphanPaymentService.AddNewOrphanPayment |
| US-PAY-03 | UC-PAY-03 | EP-10 | — | GET /api/OrphanPayments | IOrphanPaymentService.GetOrphanPaymentById |
| US-PAY-04 | UC-PAY-04 | EP-10 | — | PUT /api/OrphanPayments | IOrphanPaymentService.UpdateOrphanPayment |
| US-PAY-05 | UC-PAY-05 | EP-10 | — | DELETE /api/OrphanPayments | IOrphanPaymentService.DeleteOrphanPayment |
| US-PAY-06 | UC-PAY-06 | EP-10 | — | GET /api/OrphanPayments/by-batch-no/{batchNo} | IOrphanPaymentService.GetBatchNos |
| US-PAY-07 | UC-PAY-07 | EP-10 | — | GET /api/OrphanPayments/{id}/details | IOrphanPaymentService.GetPaymentDetails |
| US-PAY-08 | UC-PAY-08 | EP-10 | — | GET /api/OrphanPayments/{id}/details | IOrphanPaymentService.GetOrphansInPaymentBatch |
| US-PAY-09 | UC-PAY-09 | EP-10 | — | POST /api/OrphanPayments/orphan-items | IOrphanPaymentService.UpdateOrphansInPaymentBatch |
| US-PAY-10 | UC-PAY-10 | EP-10 | — | POST /api/OrphanPayments/orphan-items | IOrphanPaymentService.UpdateOrphansInPaymentBatch |
| US-PAY-11 | UC-PAY-11 | EP-10 | — | POST /api/OrphanPayments/orphan-items | IOrphanPaymentService.UpdateOrphansInPaymentBatch |
| US-PAY-12 | UC-PAY-12 | EP-10 | — | POST /api/OrphanPayments/orphan-items | IOrphanPaymentService.UpdateOrphansInPaymentBatch |
| US-PAY-13 | UC-PAY-13 | EP-10 | — | PUT /api/OrphanPayments/{id} | IOrphanPaymentService.UpdateOrphansInPaymentBatch |
| US-PAY-14 | UC-PAY-14 | EP-10 | `#/orphan-payments/:id/bank-file` | GET /api/OrphanPayments/{id}/export | IOrphanPaymentService.GetBankFile |
| US-PAY-15 | UC-PAY-15 | EP-10 | — | POST /api/OrphanPayments/{id}/import/transfer-numbers | IOrphanPaymentService.UpdateTransferNo |
| US-PAY-16 | UC-PAY-16 | EP-10 | — | POST /api/OrphanPayments/{id}/import/bank-file | IOrphanPaymentService.InesrtValuesIntoDb |
| US-PAY-17 | UC-PAY-17 | EP-10 | — | POST /api/OrphanPayments/{id}/import/exchange-status | IOrphanPaymentService.UpdateChequeExchangeStatus |
| US-PAY-18 | UC-PAY-18 | EP-10 | — | GET /api/OrphanPayments/{id}/details?received=true | IOrphanPaymentService.GetRecievedPaymentDetails |
| US-PAY-19 | UC-PAY-19 | EP-10 | — | POST /api/Reports/payments-not-received | IOrphanPaymentService.GetGotItNotPaymentDetails |
| US-PAY-20 | UC-PAY-20 | EP-10 | — | POST /api/Reports/payments-stopped | IOrphanPaymentService.GetOrphansInPaymentBatch |
| US-PAY-21 | UC-PAY-21 | EP-10 | — | GET /api/Dashboard/payment-summary | IOrphanPaymentService.GetPaymentSummeryPages |
| US-PAY-22 | UC-PAY-22 | EP-10 | `#/orphan-payments/:id/cheques` | GET /api/OrphanPayments/{id}/details | IOrphanPaymentService.GetOrphanPaymentTransfers |
| US-PAY-23 | UC-PAY-23 | EP-10 | — | POST /api/Reports/orphans-other-sponsor | IReportService.GetOrphanOtherSponser |
| US-PAY-24 | UC-PAY-24 | EP-10 | — | POST /api/Reports/payments-received/export/pdf | IOrphanPaymentService.GetOrphansInPaymentBatch |
| US-CHQ-01 | UC-CHQ-01 | EP-11 | `#/general-checks` | GET /api/CheckManagement | ICheckService.GetAllCheques |
| US-CHQ-02 | UC-CHQ-02 | EP-11 | `#/general-checks/edit/:id` | POST /api/CheckManagement | ICheckService.AddNewCehque |
| US-CHQ-03 | UC-CHQ-03 | EP-11 | — | GET /api/CheckManagement/{id} | ICheckService.GetChequeById |
| US-CHQ-04 | UC-CHQ-04 | EP-11 | — | PUT /api/CheckManagement | ICheckService.UpdateCehque |
| US-CHQ-05 | UC-CHQ-05 | EP-11 | — | GET /api/LookupManagement/cheque-beneficiaries | ICheckService.GetAllBeneficiaries |
| US-CHQ-06 | UC-CHQ-06 | EP-11 | — | GET /api/LookupManagement/currencies | ICheckService.GetCurrencies |
| US-CHQ-07 | UC-CHQ-07 | EP-11 | — | GET /api/CheckManagement/amount-in-words | ICheckService.GetAmountInArabic |
| US-CHQ-08 | UC-CHQ-08 | EP-11 | — | GET /api/LookupManagement/banks/{id}/cheque-positions | ICheckService.GetChequePositions |
| US-CHQ-09 | UC-CHQ-09 | EP-11 | `#/general-checks/statement` | GET /api/CheckManagement/report | ICheckService.GetBankGeneralChecks |
| US-CHQ-10 | UC-CHQ-10 | EP-11 | — | POST /api/Reports/general-cheque/export/pdf | ICheckService.GetAmountInArabic |
| US-PRJ-01 | UC-PRJ-01 | EP-12 | `#/seasonal-aid` | GET /api/SeasonalAid/campaigns | ISeasonalAidService.GetProjectById |
| US-PRJ-02 | UC-PRJ-02 | EP-12 | `#/seasonal-aid/:id/edit` | POST /api/SeasonalAid/campaigns | ISeasonalAidService.AddNewProject |
| US-PRJ-03 | UC-PRJ-03 | EP-12 | — | GET /api/SeasonalAid/campaigns | ISeasonalAidService.GetProjectById |
| US-PRJ-04 | UC-PRJ-04 | EP-12 | — | PUT /api/SeasonalAid/campaigns | ISeasonalAidService.UpdateProject |
| US-PRJ-05 | UC-PRJ-05 | EP-12 | — | DELETE /api/SeasonalAid/campaigns | ISeasonalAidService.DeleteProject |
| US-PRJ-06 | UC-PRJ-06 | EP-12 | `#/seasonal-aid/:id/beneficiaries` | GET /api/SeasonalAid/campaigns/{campaignId}/beneficiaries | ISeasonalAidService.GetProjectsLookUp |
| US-PRJ-07 | UC-PRJ-07 | EP-12 | — | PUT /api/SeasonalAid/campaigns/{campaignId}/beneficiaries | ISeasonalAidService.UpdateCampaignBeneficiaries |
| US-PRJ-08 | UC-PRJ-08 | EP-12 | — | PUT /api/Families/{id}/received-flag | ISeasonalAidService.UpdateIsReceivedValueOfFamily |
| US-PRJ-09 | UC-PRJ-09 | EP-12 | `#/seasonal-aid/:id/beneficiaries` | GET /api/SeasonalAid/campaigns/{campaignId}/beneficiaries | ISeasonalAidService.GetRegisteredResults |
| US-PRJ-10 | UC-PRJ-10 | EP-12 | `#/seasonal-aid/:id/eligible-families` | GET /api/SeasonalAid/campaigns/{campaignId}/eligible-families | ISeasonalAidService.GetNonRegisteredResults |
| US-PRJ-11 | UC-PRJ-11 | EP-12 | — | GET /api/SeasonalAid/campaigns/{id}/report | IReportService.GetProjectSummery |
| US-PRJ-12 | UC-PRJ-12 | EP-12 | `#/seasonal-aid/:id/report` | GET /api/SeasonalAid/campaigns/{id}/report | ISeasonalAidService.GetCampaignReport |
| US-PRJ-13 | UC-PRJ-13 | EP-12 | — | POST /api/Reports/family-cards/export/pdf | ISeasonalAidService.GetCampaignBeneficiaries |
| US-OFP-01 | UC-OFP-01 | EP-13 | `#/office-development-projects` | GET /api/OfficeProjectManagement | IOfficeProjectService.GetOfiiceProjects |
| US-OFP-02 | UC-OFP-02 | EP-13 | — | GET /api/LookupManagement/office-project-types | IOfficeProjectService.GetOfiiceProjectTypes |
| US-OFP-03 | UC-OFP-03 | EP-13 | `#/office-development-projects/:id/edit` | POST /api/OfficeProjectManagement | IOfficeProjectService.AddNewOfficeProject |
| US-OFP-04 | UC-OFP-04 | EP-13 | — | GET /api/OfficeProjectManagement/{id} | IOfficeProjectService.GetOfiiceProjectById |
| US-OFP-05 | UC-OFP-05 | EP-13 | — | DELETE /api/OfficeProjectManagement | IOfficeProjectService.DeleteOfficeProject |
| US-OFP-06 | UC-OFP-06 | EP-13 | — | GET /api/OfficeProjectManagement/export | IOfficeProjectService.GetOfficeProjectsForReport |
| US-CST-01 | UC-CST-01 | EP-14 | `#/technical-support` | GET /api/SupportTickets/all-tickets | ISupportTicketService.GetCustomerSupports |
| US-CST-02 | UC-CST-02 | EP-14 | `#/technical-support/:id/edit` | POST /api/SupportTickets | ISupportTicketService.AddNewCustomerSupport |
| US-CST-03 | UC-CST-03 | EP-14 | — | GET /api/SupportTickets/{id} | ISupportTicketService.GetCustomerSupportById |
| US-CST-04 | UC-CST-04 | EP-14 | — | PUT /api/SupportTickets | ISupportTicketService.UpdateCustomerSupport |
| US-CST-05 | UC-CST-05 | EP-14 | — | DELETE /api/SupportTickets | ISupportTicketService.DeleteCustomerSupport |
| US-CST-06 | UC-CST-06 | EP-14 | — | GET /api/SupportTickets/report | ISupportTicketService.GetCustomerSupportsForReport |
| US-MSN-01 | UC-MSN-01 | EP-15 | `#/missions` | GET /api/MissionManagement/my-missions | IMissionService.Missions |
| US-MSN-02 | UC-MSN-02 | EP-15 | — | GET /api/MissionManagement | IMissionService.Missions |
| US-MSN-03 | UC-MSN-03 | EP-15 | — | GET /api/MissionManagement/mission-types | IMissionService.MissionTypes |
| US-MSN-04 | UC-MSN-04 | EP-15 | — | GET /api/LookupManagement/mission-interview-types | IMissionService.MissionInterviewTypes |
| US-MSN-05 | UC-MSN-05 | EP-15 | — | GET /api/MissionManagement/mission-time-types | IMissionService.MissionTimeTypes |
| US-MSN-06 | UC-MSN-06 | EP-15 | `#/missions/:id/edit` | POST /api/MissionManagement | IMissionService.AddNewMission |
| US-MSN-07 | UC-MSN-07 | EP-15 | — | GET /api/MissionManagement/{id} | IMissionService.GetMissionById |
| US-MSN-08 | UC-MSN-08 | EP-15 | — | DELETE /api/MissionManagement | IMissionService.DeleteMission |
| US-MSN-09 | UC-MSN-09 | EP-15 | `#/missions/:id/register` | POST /api/MissionManagement/{id}/event | IMissionService.RegisterMission |
| US-COR-01 | UC-COR-01 | EP-16 | `#/incoming-outgoing/incoming` | GET /api/IncomingOutgoing/incoming | IIncomingService.GetAllIncoming |
| US-COR-02 | UC-COR-02 | EP-16 | — | GET /api/IncomingOutgoing/incoming | IIncomingService.GetAllIncoming |
| US-COR-03 | UC-COR-03 | EP-16 | — | GET /api/IncomingOutgoing/incoming/next-serial | IIncomingService.GetIncomingNewSerial |
| US-COR-04 | UC-COR-04 | EP-16 | `#/incoming-outgoing/incoming/:id/edit` | POST /api/IncomingOutgoing/incoming | IIncomingService.AddNewIncoming |
| US-COR-05 | UC-COR-05 | EP-16 | — | GET /api/IncomingOutgoing/incoming/{id} | IIncomingService.GetIncomingById |
| US-COR-06 | UC-COR-06 | EP-16 | — | PUT /api/IncomingOutgoing/incoming | IIncomingService.UpdateIncoming |
| US-COR-07 | UC-COR-07 | EP-16 | — | DELETE /api/IncomingOutgoing/incoming | IIncomingService.DeleteIncoming |
| US-COR-08 | UC-COR-08 | EP-16 | — | GET /api/LookupManagement/departments | IOutgoingService.GetDepartmentsLookUp |
| US-COR-09 | UC-COR-09 | EP-16 | `#/incoming-outgoing/export/incoming` | GET /api/IncomingOutgoing/incoming | IOutgoingService.GetEmployeeIncoming |
| US-COR-10 | UC-COR-10 | EP-16 | `#/incoming-outgoing/outgoing` | GET /api/IncomingOutgoing/outgoing | IOutgoingService.GetAllOutGoing |
| US-COR-11 | UC-COR-11 | EP-16 | — | GET /api/IncomingOutgoing/outgoing | IOutgoingService.GetAllOutGoing_Filter |
| US-COR-12 | UC-COR-12 | EP-16 | — | GET /api/IncomingOutgoing/outgoing/next-serial | IOutgoingService.GetNewSerial |
| US-COR-13 | UC-COR-13 | EP-16 | `#/incoming-outgoing/outgoing/:id/edit` | POST /api/IncomingOutgoing/outgoing | IOutgoingService.AddNewOutGoing |
| US-COR-14 | UC-COR-14 | EP-16 | — | GET /api/IncomingOutgoing/outgoing/{id} | IOutgoingService.GetOutGoingById |
| US-COR-15 | UC-COR-15 | EP-16 | — | PUT /api/IncomingOutgoing/outgoing | IOutgoingService.UpdateOutGoing |
| US-COR-16 | UC-COR-16 | EP-16 | — | DELETE /api/IncomingOutgoing/outgoing | IOutgoingService.DeleteOutGoing |
| US-COR-17 | UC-COR-17 | EP-16 | — | GET /api/IncomingOutgoing/outgoing/categories | IOutgoingService.GetAllOutGoingCategory |
| US-COR-18 | UC-COR-18 | EP-16 | `#/incoming-outgoing/export/outgoing` | GET /api/IncomingOutgoing/outgoing/{parentOutgoingId}/children | IOutgoingService.GetChildOutGoing |
| US-COR-19 | UC-COR-19 | EP-16 | `#/incoming-outgoing/export/outgoing-orphans` | GET /api/IncomingOutgoing/outgoing?orphanNumber= | IOutgoingService.SearchOutGoingsOrphansNumber |
| US-TRF-01 | UC-TRF-01 | EP-17 | `#/hq-transfers` | GET /api/HqTransfers | IHqTransferService.GetHqTransfers |
| US-TRF-02 | UC-TRF-02 | EP-17 | `#/hq-transfers/:id/edit` | POST /api/HqTransfers | IHqTransferService.AddNewTransfer |
| US-TRF-03 | UC-TRF-03 | EP-17 | — | GET /api/HqTransfers/{id} | IHqTransferService.GetHqTransferById |
| US-TRF-04 | UC-TRF-04 | EP-17 | — | PUT /api/HqTransfers | IHqTransferService.UpdateHqTransfer |
| US-TRF-05 | UC-TRF-05 | EP-17 | — | GET /api/LookupManagement/departments | IHqTransferService.GetHqDepartments |
| US-TRF-06 | UC-TRF-06 | EP-17 | — | GET /api/HqTransfers/max-amount | IHqTransferService.GetMaxTransferAmount |
| US-TRF-07 | UC-TRF-07 | EP-17 | `#/hq-transfers/max-amounts` | PUT /api/HqTransfers/max-amount | IHqTransferService.UpdateCountryMaxTransferAmount |
| US-TRF-08 | UC-TRF-08 | EP-17 | `#/hq-transfers/:id/details` | GET /api/HqTransfers/{id}/details | IHqTransferService.GetTransferDetails |
| US-RPT-01 | UC-RPT-01 | EP-18 | `#/reports/orphans` | POST /api/Reports/orphans | IReportService.GetOrphans |
| US-RPT-02 | UC-RPT-02 | EP-18 | `#/periodic-orphan-reports/orphan-reports/search` | POST /api/PeriodicOrphanReports/{id}/review | IPeriodicOrphanReportService.GetOrphanStatusRefinedStatus |
| US-RPT-03 | UC-RPT-03 | EP-18 | `#/reports/excluded-orphans` | POST /api/Reports/excluded-orphans | IReportService.GetOrphansBy_Excluded |
| US-RPT-04 | UC-RPT-04 | EP-18 | `#/reports/finished-sponsorship-orphans` | POST /api/Reports/finished-sponsorship-orphans | IReportService.GetFinishedSponsorshipOrphans |
| US-RPT-05 | UC-RPT-05 | EP-18 | `#/reports/unsponsored-orphans` | POST /api/Reports/unsponsored-orphans | IReportService.GetNotSponsorshipOrphans |
| US-RPT-06 | UC-RPT-06 | EP-18 | `#/reports/widows-allowing-sponsorship` | POST /api/Reports/widows-allowing-sponsorship | IPeriodicOrphanReportService.ExtractWidowsThatAllowWidowSponsorship |
| US-RPT-07 | UC-RPT-07 | EP-18 | `#/reports/meza-cards` | POST /api/Reports/meza-cards | IReportService.GetMezaCardReport |
| US-RPT-08 | UC-RPT-08 | EP-18 | — | POST /api/Reports/meza-cards | IPeriodicOrphanReportService.ExtractParentMezaCard |
| US-RPT-09 | UC-RPT-09 | EP-18 | `#/reports/beneficiary-family-details` | POST /api/Reports/beneficiary-family-details | IFamilyService.GetBeneficiaryFamilyDetails |
| US-RPT-10 | UC-RPT-10 | EP-18 | `#/seasonal-aid/:id/report` | — | — |
| US-RPT-11 | UC-RPT-11 | EP-18 | `#/reports/registered-family-projects` | POST /api/Reports/registered-family-projects | IReportService.GetRegisterdFamilyProject |
| US-RPT-12 | UC-RPT-12 | EP-18 | `#/reports/provider-sponsor-changes` | POST /api/Reports/provider-sponsor-changes | ISeasonalAidService.GetParentSonsorNewAndOld |
| US-RPT-13 | UC-RPT-13 | EP-18 | `#/reports/family-orphans` | GET /api/Families/{id}/follow-up | IReportService.GetFamilyFollowUpDetails |
| US-RPT-14 | UC-RPT-14 | EP-18 | `#/periodic-orphan-reports/orphan-reports` | — | — |
| US-RPT-15 | UC-RPT-15 | EP-18 | `#/reports/orphans-missing-reports` | POST /api/Reports/orphans-missing-reports | ISeasonalAidService.GetNeedReportForOrphans |
| US-RPT-16 | UC-RPT-16 | EP-18 | `#/reports/orphans-missing-files` | POST /api/Reports/orphans-missing-files | ISeasonalAidService.GetNeedFileForOrphans |
| US-RPT-17 | UC-RPT-17 | EP-18 | `#/reports/reports-awaiting-approval` | POST /api/Reports/reports-awaiting-approval | ISeasonalAidService.GetReportsNotAcceptedYet |
| US-RPT-18 | UC-RPT-18 | EP-18 | `#/reports/refused-reports` | POST /api/Reports/refused-reports | ISeasonalAidService.GetRefusedReports |
| US-RPT-19 | UC-RPT-19 | EP-18 | — | POST /api/Reports/non-renewed-reports | IPeriodicOrphanReportService.GetOrphanNonRenewedReport |
| US-RPT-20 | UC-RPT-20 | EP-18 | `#/reports/charity-payment-tracking` | POST /api/Reports/charity-payment-tracking | IOrphanPaymentService.GetCharityPaymentTracking |
| US-RPT-21 | UC-RPT-21 | EP-18 | — | POST /api/Reports/family-update-tracking/export/pdf | IOrphanPaymentService.GetFamilyUpdateTracking |
| US-RPT-22 | UC-RPT-22 | EP-18 | `#/reports/missed-payments` | POST /api/Reports/missed-payments | IOrphanPaymentService.GetMissedPayments |
| US-RPT-23 | UC-RPT-23 | EP-18 | — | POST /api/Reports/missed-payments | IOrphanPaymentService.GetMissedPaymentsAll |
| US-RPT-24 | UC-RPT-24 | EP-18 | `#/reports/orphan-files` | POST /api/Reports/orphan-files/export | ISeasonalAidService.GetExportImagesAndFiles |
| US-RPT-25 | UC-RPT-25 | EP-18 | — | POST /api/Reports/certificate-files/export | ISeasonalAidService.GetExportCertificateImg |
| US-RPT-26 | UC-RPT-26 | EP-18 | — | — | — |
| US-RPT-27 | UC-RPT-27 | EP-18 | — | GET /api/OrphanPayments/{id}/details | IReportService.GetAllPaymentOrphans |
| US-RPT-28 | UC-RPT-28 | EP-18 | — | POST /api/Reports/orphans-without-payment | IReportService.GetAllOrphansThatDontTakeAnyAmount |
| US-RPT-29 | UC-RPT-29 | EP-18 | — | POST /api/Reports/payments-received/export/pdf | IOrphanPaymentService.GetOrphansInPaymentBatch |
| US-RPT-30 | UC-RPT-30 | EP-18 | — | POST /api/Reports/cheque-numbers/export/pdf | IOrphanPaymentService.GetOrphansInPaymentBatch |
| US-RPT-31 | UC-RPT-31 | EP-18 | — | POST /api/Reports/receipt-cards/export/pdf | IOrphanPaymentService.GetOrphansInPaymentBatch |
| US-RPT-32 | UC-RPT-32 | EP-18 | — | GET /api/Dashboard/payment-summary | IOrphanPaymentService.GetPaymentSummeryPages |
| US-RPT-33 | UC-RPT-33 | EP-18 | — | POST /api/Reports/cheque-statement/export/pdf | ICheckService.GetBankGeneralChecks |
| US-RPT-34 | UC-RPT-34 | EP-18 | — | POST /api/Reports/family-cards/export/pdf | ISeasonalAidService.GetCampaignBeneficiaries |
| US-RPT-35 | UC-RPT-35 | EP-18 | — | — | — |
| US-RPT-36 | UC-RPT-36 | EP-18 | — | — | — |
| US-RPT-37 | UC-RPT-37 | EP-18 | — | POST /api/Reports/guardian-identification-sheets/export/pdf | IPeriodicOrphanReportService.GetGuardianIdentificationSheets |
| US-RPT-38 | UC-RPT-38 | EP-18 | — | — | — |
| US-RPT-39 | UC-RPT-39 | EP-18 | — | POST /api/Reports/family-orphans/export/pdf | IReportService.GetFamilyFollowUp |
| US-RPT-40 | UC-RPT-40 | EP-18 | — | POST /api/Reports/{reportKey}/export/pdf | — |
| US-RPT-41 | UC-RPT-41 | EP-18 | — | — | — |
| US-SYS-01 | UC-SYS-01 | EP-19 | — | POST /api/Attachments | IAttachmentService.SaveImage |
| US-SYS-02 | UC-SYS-02 | EP-19 | — | GET /api/Attachments/{id}/download | IAttachmentService.GetImage |
| US-SYS-03 | UC-SYS-03 | EP-19 | — | GET /api/Attachments | — |
| US-SYS-04 | UC-SYS-04 | EP-19 | — | GET /api/LookupManagement | ICharityService.GetHouseOwnerShips |
| US-SYS-05 | UC-SYS-05 | EP-19 | — | GET /api/LookupManagement | IFamilyService.GetEducationalStatuss |
| US-SYS-06 | UC-SYS-06 | EP-19 | — | GET /api/LookupManagement/general-reasons | ILookupService.GetGeneralReasons |
| US-SYS-07 | UC-SYS-07 | EP-19 | — | GET /api/LookupManagement/countries | ILookupService.GetCountries |
| US-SYS-08 | UC-SYS-08 | EP-19 | — | GET /api/LookupManagement/countries | ILookupService.GetCountries |
| US-SYS-09 | UC-SYS-09 | EP-19 | — | GET /api/Reports | ILookupService.GetJobs |
| US-SYS-10 | UC-SYS-10 | EP-19 | — | GET /api/OrphanPayments?orphanId= | IOrphanService.GetOrphanPayments |
| US-SYS-11 | UC-SYS-11 | EP-19 | — | GET /api/LookupManagement/countries | ILookupService.GetCountriesDetail |
| US-SYS-12 | UC-SYS-12 | EP-19 | — | GET /api/Families/check-national-id | IFamilyService.CheckNId |
| US-SYS-13 | UC-SYS-13 | EP-19 | — | — | — |

