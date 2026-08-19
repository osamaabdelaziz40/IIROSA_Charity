# WAR.IIROSA - Authentication & User Account

ادارة الدخول والحساب | use case prefix `UC-AUT` | chapter 6 of the master document


| Field | Value |
| --- | --- |
| Document title | WAR.IIROSA - Module Specification: Authentication & User Account |
| Module | Authentication & User Account - ادارة الدخول والحساب |
| Use case prefix | UC-AUT |
| Chapter in master document | Chapter 6 |
| Documented use cases | 9 |
| Principal routes | `#/auth/login`, `#/auth/register`, `#/auth/forgot-password`, `#/auth/reset-password/:userId/:code`, `#/auth/change-password` |
| Version | 1.1 |
| Status | Chapter content extracted verbatim; screen fields and scenarios derived from the source code |
| Date | 18 August 2026 |
| Parent document | WAR.IIROSA-Project-Documentation.docx v1.0 (repository WAR.IIROSA, commit 4beb736) |

## Contents of this document

1. Chapter 6 — module purpose and use-case catalogue (verbatim from the master document)
2. §6.D — detailed specifications carried over from chapter 25
3. §6.S — screen field specifications (every field of every screen, derived from the AngularJS views)
4. §6.U — one expanded scenario per use case (trigger, pre-conditions, main flow, alternates, exceptions, post-conditions)
5. §6.A / §6.B — annexes: screens and Web API controllers of this module

## 6. Authentication & User Account

إدارة الدخول والحساب — establishes the caller's identity, role and country, which every other module depends on for scoping and authorisation.

### 6.1 Use cases


| ID | Use case | Primary actor | Description & main flow | Realisation |
| --- | --- | --- | --- | --- |
| UC-AUT-01 | Log in to the system تسجيل الدخول | All roles | The user submits username and password. The system validates the credentials against ASP.NET Identity through `Framework.Identity`, resolves the caller's roles and permissions, and issues a short-lived JWT access token plus a refresh token carrying the user, role, charity and country claims. Alternate: invalid credentials or a locked-out account return loginStatus = false and the user stays on the login screen. | Route `#/auth/login` → POST /api/Auth/login → IAuthService.Login |
| UC-AUT-02 | Retrieve signed-in user profile بيانات المستخدم | All roles | After login the shell fetches the display name, charity, country and role of the current user in order to render the header and to decide which menu groups are visible. | GET /api/Auth/me |
| UC-AUT-03 | Log out خروج | All roles | The user clicks Logout; the SPA clears the identity cookies and the cached role, and routes back to the login state. The Forms ticket is abandoned. | index.html → LogOut() |
| UC-AUT-04 | Register a new charity account تسجيل جمعية | Prospective charity / General Director | The registration form captures the charity name, login credentials, country, region, centre and contact details. The system verifies the name is not already taken, creates the membership user, assigns it the charity role GUID and creates the linked charity record. Alternate: duplicate name or a membership failure aborts the whole creation. | Route `#/auth/register` → POST /api/Auth/register, POST /api/UserManagement → ICharityService |
| UC-AUT-05 | Request a password-reset link نسيت كلمة المرور | All roles | The user enters their username. The system locates the account, generates a reset code, composes a reset URL and dispatches it to the registered e-mail address, returning a status code that tells the UI whether the username existed. | Route `#/auth/forgot-password` → GET /api/Auth/forgot-password |
| UC-AUT-06 | Validate a password-reset link التحقق من رابط الاستعادة | All roles | When the emailed link is opened, the system verifies that the user id and code pair is valid and unexpired before showing the new-password form. Alternate: an invalid or consumed code shows an error and blocks the reset. | Route `#/auth/reset-password/:userId/:code` → GET /api/Auth/validate-reset-token |
| UC-AUT-07 | Reset a forgotten password إعادة تعيين كلمة المرور | All roles | After a valid link is confirmed the user enters and confirms a new password; the system replaces the stored password and invalidates the reset code. | Route `#/auth/reset-password/:userId/:code` → POST /api/Auth/change-password |
| UC-AUT-08 | Change own password تغيير الرقم السرى | All roles | A signed-in user supplies a new password from the account menu; the system updates the membership record and returns a success indicator. | Route `#/auth/change-password` → POST /api/Auth/change-password |
| UC-AUT-09 | Enforce role-based navigation إظهار القوائم حسب الصلاحية | System | On every state change the shell shows or hides menu groups according to the cached role code — administrative groups for role 0/3, financial groups for role 4, and the reduced charity menu for role 1. | index.html menu classes adminRule, staff, NotTransferRole, TransfersRole |

### 6.D  Detailed use case specifications (from chapter 25)

Reproduced verbatim from chapter 25 of the master document — the fully expanded specification of this module’s critical end-to-end scenarios. Every other use case of the module is specified in §6.U.

**25.1 UC-AUT-01 — Log in to the system**


| Use case ID | UC-AUT-01 |
| --- | --- |
| Name | Log in to the system — تسجيل الدخول |
| Primary actor | Any registered user (General Director, Staff, Financial Director, charity, Guest) |
| Goal | Establish an authenticated session and obtain the role and country that scope every later operation. |
| Pre-conditions | The account exists in ASP.NET Membership, is assigned exactly one role, and is not locked out. |
| Trigger | The user opens the application and submits the login form. |
| Main flow | 1. The user enters username and password on the login state. 2. The client calls POST /api/Auth/login with the credentials. 3. IAuthService.Login validates the credentials against the membership store. 4. The system resolves the user's role GUID against the five configured role ids and maps it to the internal role code 0–4. 5. The system signs a short-lived JWT access token and issues a refresh token alongside it; token rotation is handled by `POST /api/Auth/refresh-token`. 6. The system determines the caller's country from the linked employee record, or from the linked charity record when the caller is a charity. 7. The response returns the user id, role code, country id and loginStatus = true. 8. The client caches the identity, renders the menu groups permitted for that role, and routes to the home state. |
| Alternate flows | A1 — Invalid credentials. Steps 3 fails; the response carries loginStatus = false; the user remains on the login screen with an error. A2 — Locked-out charity. The account was locked by UC-CHR-07; membership refuses the sign-in and A1 applies. A3 — Forgotten password. The user leaves the flow for UC-AUT-05. |
| Exception flow | E1. Any exception is caught, the login is reported as failed, and no detail is returned to the client. |
| Post-conditions | An authentication cookie exists; the client holds the user id, role code and country id used as parameters on subsequent API calls. |
| Business rules | BR-01 A user has exactly one role. BR-02 The role and permission claims determine menu visibility on the client and, decisively, the authorisation check on every endpoint. BR-03 The ticket lifetime is 15 days. |


### 6.S  Screen field specifications

Derived from the AngularJS views of this module. For every screen the table lists each data-entry field in the order it appears, the scope property it binds to, its control type, whether the form marks it mandatory, and the lookup or rule that governs it. Labels are reproduced as they appear on screen (Arabic, right-to-left).

#### 6.S.1  Screen `#/auth/login`


| Property | Value |
| --- | --- |
| Angular route | `#/auth/login` |
| Feature module | `auth` (lazy-loaded) |
| Component | `LoginComponent` |
| Route status | implemented |
| Data-entry fields | 3 |
| Grids on the screen | 0 |
| Commands | 3 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | اسم المستخدم | UserName | Text box | Mandatory |
| — | تذكرنى | UserPassword | Password box | Mandatory |
| — | برجاء ادخال كل البيانات المطلوبة | remember | Check box | Optional · on click: rememberMe() |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| تسجيل الدخول | LoginFun() | always |
| (icon only) | rememberMe() | always |
| (icon only) | ForgetPassword() | always |

#### 6.S.2  Screen `#/auth/register`


| Property | Value |
| --- | --- |
| Angular route | `#/auth/register` |
| Feature module | `auth` (lazy-loaded) |
| Component | `RegisterComponent` |
| Route status | planned |
| Data-entry fields | 10 |
| Grids on the screen | 0 |
| Commands | 1 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| المعلومات الشخصية | البريد الالكترونى | UserEmail | E-mail box | Optional |
| المعلومات الشخصية | اسم المستخدم | RegUserName | Text box | Optional |
| المعلومات الشخصية | اعادة كتابة الرقم السرى | UserPassCon | Password box | Optional |
| المعلومات الشخصية | الرقم السري | UserPass | Password box | Optional |
| المعلومات الشخصية | الهاتف  | UserMobile | Phone box | Optional |
| المعلومات الشخصية | التليفون  | UserPhone | Phone box | Optional |
| المعلومات الشخصية |  العنوان | CharityAdd | Text box | Optional |
| المعلومات الشخصية |  اسم الجمعية | CharityName | Text box | Optional |
| المعلومات الشخصية | (unlabelled) | RegBanks | Drop-down list | Optional · options: lookup: Banks |
| المعلومات الشخصية | (unlabelled) | RegCountries | Drop-down list | Optional · options: lookup: Countries · on change: GetBanks() |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| تسجيل | RegisterFun() | always |

#### 6.S.3  Screen `#/auth/forgot-password`


| Property | Value |
| --- | --- |
| Angular route | `#/auth/forgot-password` |
| Feature module | `auth` (lazy-loaded) |
| Component | `ForgotPasswordComponent` |
| Route status | planned |
| Data-entry fields | 1 |
| Grids on the screen | 0 |
| Commands | 1 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | ادخل الاسم ليتم الفحص | UserNameFP | Text box | Optional |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| تم | ForgetPass() | always |

#### 6.S.4  Screen `#/auth/reset-password/:userId/:code`


| Property | Value |
| --- | --- |
| Angular route | `#/auth/reset-password/:userId/:code` |
| Feature module | `auth` (lazy-loaded) |
| Component | `ResetPasswordComponent` |
| Route status | planned |
| Data-entry fields | 3 |
| Grids on the screen | 0 |
| Commands | 1 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | * الرقم السرى الجديد | userNameFP | Text box | Optional |
| — | * اعادة كتبة الرقم السرى | NewPassword | Password box | Optional |
| — | من فضلك ادخل كتابة كل البيانات | ConfirmNewPassword | Password box | Optional |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | ChangePass() | always |

#### 6.S.5  Screen `#/auth/change-password`


| Property | Value |
| --- | --- |
| Angular route | `#/auth/change-password` |
| Feature module | `auth` (lazy-loaded) |
| Component | `ChangePasswordComponent` |
| Route status | planned |
| Data-entry fields | 2 |
| Grids on the screen | 0 |
| Commands | 1 |


| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | * اعادة كتبة الرقم السرى | NewPassword | Password box | Optional |
| — | من فضلك ادخل كتابة كل البيانات | ConfirmNewPassword | Password box | Optional |

Commands on this screen:

| Command | Handler | Shown when |
| --- | --- | --- |
| حفظ | ChangePass() | always |

### 6.U  Use case scenarios

One expanded scenario for every use case of this module. Pre-conditions, flows and post-conditions are derived from the screen that hosts the function, the Web API action that serves it and the business-layer method that executes it; quoted messages are the literal strings returned by the business layer.

#### 6.U.1  UC-AUT-01 — Log in to the system تسجيل الدخول


| Item | Specification |
| --- | --- |
| Use case ID | UC-AUT-01 |
| Name | Log in to the system تسجيل الدخول |
| Type | Authentication / credential handling |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The user submits username and password. The system validates the credentials against ASP.NET Identity through `Framework.Identity`, resolves the caller's roles and permissions, and issues a short-lived JWT access token plus a refresh token carrying the user, role, charity and country claims. Alternate: invalid credentials or a locked-out account return loginStatus = false and the user stays on the login screen. |
| Trigger | The actor presses «تسجيل الدخول» on the screen login. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. The SPA route `#/auth/login` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/auth/login`.<br>2. The actor enters اسم المستخدم، تذكرنى.<br>3. The actor presses «تسجيل الدخول» (LoginFun()).<br>4. The SPA issues `POST /api/Auth/login` carrying userName, password.<br>5. `AuthController` binds the typed request DTO and delegates to the application service.<br>6. `IAuthService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The credentials are validated against ASP.NET Membership; the role GUID configured in Web.config is resolved to the internal role code and the country of the user is read.<br>8. On success a short-lived JWT access token and a refresh token are issued; the access token carries the user, role, charity and country claims, and the `auth` interceptor attaches it to every later call. |
| Alternate flows | None recorded. |
| Exception flows | • The credentials do not validate, or the account is locked out — loginStatus is returned false and the actor stays on the login screen. |
| Post-conditions | • An authenticated session exists (or has been ended); the role code and country id that scope every later request are held by the SPA. |
| Realisation | Route `#/auth/login` → `LoginComponent`<br>`POST /api/Auth/login` → `AuthController` → `IAuthService` |

#### 6.U.2  UC-AUT-02 — Retrieve signed-in user profile بيانات المستخدم


| Item | Specification |
| --- | --- |
| Use case ID | UC-AUT-02 |
| Name | Retrieve signed-in user profile بيانات المستخدم |
| Type | Read a record |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | After login the shell fetches the display name, charity, country and role of the current user in order to render the header and to decide which menu groups are visible. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA issues `GET /api/Auth/me` carrying id.<br>4. `AuthController` binds the typed request DTO and delegates to the application service.<br>5. `IAuthService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | `GET /api/Auth/me` → `AuthController` → `IAuthService` |

#### 6.U.3  UC-AUT-03 — Log out خروج


| Item | Specification |
| --- | --- |
| Use case ID | UC-AUT-03 |
| Name | Log out خروج |
| Type | Authentication / credential handling |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The user clicks Logout; the SPA clears the identity cookies and the cached role, and routes back to the login state. The Forms ticket is abandoned. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor enters the credentials the screen asks for.<br>3. The actor presses the command button of the screen.<br>4. The SPA calls the service endpoint that backs the function.<br>5. The Web API controller receives the request and delegates to the business layer.<br>6. The business layer executes the rules and the data access.<br>7. The credentials are validated against ASP.NET Membership; the role GUID configured in Web.config is resolved to the internal role code and the country of the user is read.<br>8. On success a short-lived JWT access token and a refresh token are issued; the access token carries the user, role, charity and country claims, and the `auth` interceptor attaches it to every later call. |
| Alternate flows | None recorded. |
| Exception flows | • The credentials do not validate, or the account is locked out — loginStatus is returned false and the actor stays on the login screen. |
| Post-conditions | • An authenticated session exists (or has been ended); the role code and country id that scope every later request are held by the SPA. |
| Realisation | index.html → LogOut() |

#### 6.U.4  UC-AUT-04 — Register a new charity account تسجيل جمعية


| Item | Specification |
| --- | --- |
| Use case ID | UC-AUT-04 |
| Name | Register a new charity account تسجيل جمعية |
| Type | Create a record |
| Primary actor | Prospective charity / General Director |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The registration form captures the charity name, login credentials, country, region, centre and contact details. The system verifies the name is not already taken, creates the membership user, assigns it the charity role GUID and creates the linked charity record. Alternate: duplicate name or a membership failure aborts the whole creation. |
| Trigger | The actor presses «تسجيل» on the screen register. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: Prospective charity / General Director.<br>3. The SPA route `#/auth/register` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/auth/register`. The screen opens in add mode.<br>2. The system loads the reference data the form needs (regions, centres, banks, types and the other lookup lists bound to the drop-downs).<br>3. The actor completes the input fields of the form.<br>4. The actor presses «تسجيل» (RegisterFun()).<br>5. The SPA issues `POST /api/Auth/register` carrying [FromBody]CreateCharityDto dto.<br>6. ``AuthController`` receives the request and delegates to the business layer.<br>7. `IAuthService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>8. The business layer validates the payload, stamps the owning charity and the creating user, and persists the new record.<br>9. The system returns the outcome and the SPA confirms the save and returns to the list screen. |
| Alternate flows | • The actor abandons the form before saving — nothing is written and the record keeps its previous state. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state.<br>• A mandatory field is empty or fails its format check — the save is refused and the field is flagged on the form. |
| Post-conditions | • A new record exists, owned by the charity of the creating user, and appears in the list screen of the module. |
| Realisation | Route `#/auth/register` → `RegisterComponent`<br>`POST /api/Auth/register` · `POST /api/UserManagement` → `AuthController` → `IAuthService` |

#### 6.U.5  UC-AUT-05 — Request a password-reset link نسيت كلمة المرور


| Item | Specification |
| --- | --- |
| Use case ID | UC-AUT-05 |
| Name | Request a password-reset link نسيت كلمة المرور |
| Type | Authentication / credential handling |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | The user enters their username. The system locates the account, generates a reset code, composes a reset URL and dispatches it to the registered e-mail address, returning a status code that tells the UI whether the username existed. |
| Trigger | The actor presses «تم» on the screen forgetpassword. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. The SPA route `#/auth/forgot-password` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/auth/forgot-password`.<br>2. The actor enters the credentials the screen asks for.<br>3. The actor presses «تم» (ForgetPass()).<br>4. The SPA issues `GET /api/Auth/forgot-password` carrying UserName.<br>5. `AuthController` binds the typed request DTO and delegates to the application service.<br>6. `IAuthService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The credentials are validated against ASP.NET Membership; the role GUID configured in Web.config is resolved to the internal role code and the country of the user is read.<br>8. On success a short-lived JWT access token and a refresh token are issued; the access token carries the user, role, charity and country claims, and the `auth` interceptor attaches it to every later call. |
| Alternate flows | None recorded. |
| Exception flows | • The credentials do not validate, or the account is locked out — loginStatus is returned false and the actor stays on the login screen. |
| Post-conditions | • An authenticated session exists (or has been ended); the role code and country id that scope every later request are held by the SPA. |
| Realisation | Route `#/auth/forgot-password` → `ForgotPasswordComponent`<br>`GET /api/Auth/forgot-password` → `AuthController` → `IAuthService` |

#### 6.U.6  UC-AUT-06 — Validate a password-reset link التحقق من رابط الاستعادة


| Item | Specification |
| --- | --- |
| Use case ID | UC-AUT-06 |
| Name | Validate a password-reset link التحقق من رابط الاستعادة |
| Type | Authentication / credential handling |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | When the emailed link is opened, the system verifies that the user id and code pair is valid and unexpired before showing the new-password form. Alternate: an invalid or consumed code shows an error and blocks the reset. |
| Trigger | The actor presses «حفظ» on the screen resetpassword. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/auth/reset-password/:userId/:code` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/auth/reset-password/:userId/:code`.<br>2. The actor enters the credentials the screen asks for.<br>3. The actor presses «حفظ» (ChangePass()).<br>4. The SPA issues `GET /api/Auth/validate-reset-token` carrying userid, code.<br>5. `AuthController` binds the typed request DTO and delegates to the application service.<br>6. `IAuthService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The credentials are validated against ASP.NET Membership; the role GUID configured in Web.config is resolved to the internal role code and the country of the user is read.<br>8. On success a short-lived JWT access token and a refresh token are issued; the access token carries the user, role, charity and country claims, and the `auth` interceptor attaches it to every later call. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The credentials do not validate, or the account is locked out — loginStatus is returned false and the actor stays on the login screen. |
| Post-conditions | • An authenticated session exists (or has been ended); the role code and country id that scope every later request are held by the SPA. |
| Realisation | Route `#/auth/reset-password/:userId/:code` → `ResetPasswordComponent`<br>`GET /api/Auth/validate-reset-token` → `AuthController` → `IAuthService` |

#### 6.U.7  UC-AUT-07 — Reset a forgotten password إعادة تعيين كلمة المرور


| Item | Specification |
| --- | --- |
| Use case ID | UC-AUT-07 |
| Name | Reset a forgotten password إعادة تعيين كلمة المرور |
| Type | Authentication / credential handling |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | After a valid link is confirmed the user enters and confirms a new password; the system replaces the stored password and invalidates the reset code. |
| Trigger | The actor presses «حفظ» on the screen resetpassword. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/auth/reset-password/:userId/:code` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/auth/reset-password/:userId/:code`.<br>2. The actor enters the credentials the screen asks for.<br>3. The actor presses «حفظ» (ChangePass()).<br>4. The SPA issues `POST /api/Auth/change-password` carrying userId, password.<br>5. `AuthController` binds the typed request DTO and delegates to the application service.<br>6. `IAuthService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The credentials are validated against ASP.NET Membership; the role GUID configured in Web.config is resolved to the internal role code and the country of the user is read.<br>8. On success a short-lived JWT access token and a refresh token are issued; the access token carries the user, role, charity and country claims, and the `auth` interceptor attaches it to every later call. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The credentials do not validate, or the account is locked out — loginStatus is returned false and the actor stays on the login screen.<br>• The business layer returns «Please Enter All Requird Data» and the operation is not applied.<br>• The business layer returns «Please Enter  valid password must contain All Type Characters and min Length 8» and the operation is not applied.<br>• The business layer returns «User Not Found» and the operation is not applied.<br>• The business layer returns «Password Changed» and the operation is not applied.<br>• The business layer returns «Password change failed. Please re-enter your values and try again» and the operation is not applied.<br>• The business layer returns «There Problem With Saving» and the operation is not applied. |
| Post-conditions | • An authenticated session exists (or has been ended); the role code and country id that scope every later request are held by the SPA. |
| Realisation | Route `#/auth/reset-password/:userId/:code` → `ResetPasswordComponent`<br>`POST /api/Auth/change-password` → `AuthController` → `IAuthService` |

#### 6.U.8  UC-AUT-08 — Change own password تغيير الرقم السرى


| Item | Specification |
| --- | --- |
| Use case ID | UC-AUT-08 |
| Name | Change own password تغيير الرقم السرى |
| Type | Authentication / credential handling |
| Primary actor | All roles |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | A signed-in user supplies a new password from the account menu; the system updates the membership record and returns a success indicator. |
| Trigger | The actor presses «حفظ» on the screen ChangePassword. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: All roles.<br>3. Caller identity comes from the JWT, never from the request; the application service scopes the query to the charity (and country) that owns the user, unless the role is an HQ role permitted to pass an explicit `charityId` filter.<br>4. The SPA route `#/auth/change-password` has loaded and its reference-data lookups have been populated. |
| Main flow | 1. The actor navigates to the screen at `#/auth/change-password`.<br>2. The actor enters the credentials the screen asks for.<br>3. The actor presses «حفظ» (ChangePass()).<br>4. The SPA issues `POST /api/Auth/change-password` carrying string userId, string password.<br>5. `AuthController` binds the typed request DTO and delegates to the application service.<br>6. `IAuthService` validates the payload with its FluentValidation validator, applies the business rules and the charity scope, and persists through `IUnitOfWork`.<br>7. The credentials are validated against ASP.NET Membership; the role GUID configured in Web.config is resolved to the internal role code and the country of the user is read.<br>8. On success a short-lived JWT access token and a refresh token are issued; the access token carries the user, role, charity and country claims, and the `auth` interceptor attaches it to every later call. |
| Alternate flows | • An HQ role (General Director, Financial Director, Staff) may pass an explicit charity id and so read across the charity boundary; a charity user may not and always sees its own data. |
| Exception flows | • The credentials do not validate, or the account is locked out — loginStatus is returned false and the actor stays on the login screen.<br>• The business layer returns «Please Enter All Requird Data» and the operation is not applied.<br>• The business layer returns «Please Enter  valid password must contain All Type Characters and min Length 8» and the operation is not applied.<br>• The business layer returns «User Not Found» and the operation is not applied.<br>• The business layer returns «Password Changed» and the operation is not applied.<br>• The business layer returns «Password change failed. Please re-enter your values and try again» and the operation is not applied.<br>• The business layer returns «There Problem With Saving» and the operation is not applied. |
| Post-conditions | • An authenticated session exists (or has been ended); the role code and country id that scope every later request are held by the SPA. |
| Realisation | Route `#/auth/change-password` → `ChangePasswordComponent`<br>`POST /api/Auth/change-password` → `AuthController` → `IAuthService` |

#### 6.U.9  UC-AUT-09 — Enforce role-based navigation إظهار القوائم حسب الصلاحية


| Item | Specification |
| --- | --- |
| Use case ID | UC-AUT-09 |
| Name | Enforce role-based navigation إظهار القوائم حسب الصلاحية |
| Type | Read a record |
| Primary actor | System |
| Secondary actors | The system (Web API + business layer); the database. |
| Summary | On every state change the shell shows or hides menu groups according to the cached role code — administrative groups for role 0/3, financial groups for role 4, and the reduced charity menu for role 1. |
| Trigger | The actor selects the function from the module menu or the screen that hosts it. |
| Pre-conditions | 1. The actor is authenticated; the JWT access token is valid and the client holds the role, charity and country claims it carries.<br>2. The actor holds one of: System.<br>3. The target record exists and its identifier is known to the screen. |
| Main flow | 1. The actor reaches the function from the screen of this module that hosts it.<br>2. The actor selects the record to open.<br>3. The SPA calls the service endpoint that backs the function.<br>4. The Web API controller receives the request and delegates to the business layer.<br>5. The business layer executes the rules and the data access.<br>6. The system returns the record with its child collections and the SPA binds them to the form fields. |
| Alternate flows | None recorded. |
| Exception flows | • The session has expired or the role is not permitted — the request is rejected and the SPA routes back to the login state. |
| Post-conditions | • No stored data is changed — the operation is a read. |
| Realisation | index.html menu classes adminRule, staff, NotTransferRole, TransfersRole |

### 6.A  Annex - Angular routes of this module

Routes are hash-based (`useHash: true`), rendered inside `MainLayoutComponent` behind
`AuthGuard` and `PermissionGuard`. *implemented* means the route exists in
`Frontend/src/app`; *planned* means it is specified here and not yet built.

| Angular route | Feature module | Component | Status |
| --- | --- | --- | --- |
| `#/auth` | `auth` | redirects to `#/auth/login` | implemented |
| `#/auth/login` | `auth` | `LoginComponent` | implemented |
| `#/auth/register` | `auth` | `RegisterComponent` | planned |
| `#/auth/forgot-password` | `auth` | `ForgotPasswordComponent` | planned |
| `#/auth/reset-password/:userId/:code` | `auth` | `ResetPasswordComponent` | planned |
| `#/auth/change-password` | `auth` | `ChangePasswordComponent` | planned |

### 6.B  Annex - API controllers of this module

Every controller inherits `ApiController` (`[Authorize]`, route `api/[controller]`) and
returns `Framework.Core.ApiResponse<T>`. Business rules live in the application service
behind each controller, never in the controller itself.

| Controller | Base route | Responsibility |
| --- | --- | --- |
| `AuthController` | `api/Auth` | Login; charity user creation. Signed-in user detail; payment summary pages. Charity self-registration. Reset-link generation and validation. Password update. |

---

Shared context - system architecture, actors and roles, the sponsorship lifecycle, the full module map and all appendices - is in the companion document [00-Overview-and-Common-Context](00-Overview-and-Common-Context.md). The delivery backlog for this module is epic EP-01 in [00-EPICS-and-User-Stories](00-EPICS-and-User-Stories.md).

