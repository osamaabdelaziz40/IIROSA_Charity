# Angular Frontend Architecture - IIROSA System

## Technology Stack

- **Framework**: Angular 18+
- **Language**: TypeScript 5.x
- **State Management**: NgRx (for complex state) + Signals (for local component state)
- **UI Framework**: Bootstrap 5.3 + TinyDash Dark RTL Template
- **Icons**: Feather Icons (from TinyDash)
- **HTTP Client**: HttpClient with Interceptors
- **Forms**: Reactive Forms + Form Validation
- **Charts**: ApexCharts (from TinyDash)
- **Tables**: DataTables (from TinyDash)
- **Date Range Picker**: DateRangePicker (from TinyDash)
- **File Upload**: Dropzone (from TinyDash)
- **Real-time**: @microsoft/signalr
- **Localization**: @ngx-translate/core
- **Excel Export**: ExcelJS
- **PDF Export**: jsPDF

---

## Solution Structure

```
IIROSA.Web.Client/
├── src/
│   ├── app/
│   │   ├── core/                          # Core services, interceptors, guards
│   │   │   ├── interceptors/
│   │   │   │   ├── auth.interceptor.ts
│   │   │   │   ├── error.interceptor.ts
│   │   │   │   ├── loading.interceptor.ts
│   │   │   │   └── locale.interceptor.ts
│   │   │   ├── guards/
│   │   │   │   ├── auth.guard.ts
│   │   │   │   ├── permission.guard.ts
│   │   │   │   └── module.guard.ts
│   │   │   ├── services/
│   │   │   │   ├── signalr.service.ts
│   │   │   │   ├── notification.service.ts
│   │   │   │   ├── attachment.service.ts
│   │   │   │   ├── auth.service.ts
│   │   │   │   ├── localstorage.service.ts
│   │   │   │   └── settings.service.ts
│   │   │   └── models/
│   │   │       ├── user.model.ts
│   │   │       ├── role.model.ts
│   │   │       └── claim.model.ts
│   │   │
│   │   ├── shared/                        # Shared components & pipes
│   │   │   ├── components/
│   │   │   │   ├── attachment/
│   │   │   │   │   ├── attachment.component.ts
│   │   │   │   │   ├── attachment.component.html
│   │   │   │   │   ├── attachment.component.scss
│   │   │   │   │   └── attachment.module.ts
│   │   │   │   ├── input-fields/
│   │   │   │   │   ├── input-fields.component.ts
│   │   │   │   │   ├── input-fields.component.html
│   │   │   │   │   ├── input-fields.component.scss
│   │   │   │   │   └── input-fields.module.ts
│   │   │   │   ├── data-list/
│   │   │   │   │   ├── data-list.component.ts
│   │   │   │   │   ├── data-list.component.html
│   │   │   │   │   ├── data-list.component.scss
│   │   │   │   │   └── data-list.module.ts
│   │   │   │   ├── modal/
│   │   │   │   │   ├── modal.component.ts
│   │   │   │   │   ├── modal.component.html
│   │   │   │   │   └── modal.module.ts
│   │   │   │   ├── toast/
│   │   │   │   │   ├── toast.component.ts
│   │   │   │   │   ├── toast.component.html
│   │   │   │   │   └── toast.service.ts
│   │   │   │   ├── loading/
│   │   │   │   │   ├── loading.component.ts
│   │   │   │   │   └── loading.component.html
│   │   │   │   ├── confirm-dialog/
│   │   │   │   │   ├── confirm-dialog.component.ts
│   │   │   │   │   └── confirm-dialog.service.ts
│   │   │   │   ├── page-header/
│   │   │   │   │   ├── page-header.component.ts
│   │   │   │   │   └── page-header.component.html
│   │   │   │   ├── empty-state/
│   │   │   │   │   ├── empty-state.component.ts
│   │   │   │   │   └── empty-state.component.html
│   │   │   │   └── chart/
│   │   │   │       ├── chart.component.ts
│   │   │   │       ├── chart.component.html
│   │   │   │       └── chart.module.ts
│   │   │   ├── pipes/
│   │   │   │   ├── date.pipe.ts
│   │   │   │   ├── number.pipe.ts
│   │   │   │   ├── truncate.pipe.ts
│   │   │   │   ├── safe-html.pipe.ts
│   │   │   │   └── file-size.pipe.ts
│   │   │   ├── directives/
│   │   │   │   ├── has-permission.directive.ts
│   │   │   │   ├── auto-focus.directive.ts
│   │   │   │   └── click-outside.directive.ts
│   │   │   └── validators/
│   │   │       ├── custom-email.validator.ts
│   │   │       ├── password-strength.validator.ts
│   │   │       └── unique.validator.ts
│   │   │
│   │   ├── assets/
│   │   │   ├── i18n/                      # Localization files
│   │   │   │   ├── en.json
│   │   │   │   ├── ar.json
│   │   │   │   └── fr.json
│   │   │   ├── css/                       # TinyDash CSS
│   │   │   │   ├── app-dark.css
│   │   │   │   ├── app-light.css
│   │   │   │   ├── app-rtl.css
│   │   │   │   ├── feather.css
│   │   │   │   ├── dropzone.css
│   │   │   │   └── ...
│   │   │   ├── js/                        # TinyDash JS (adapted)
│   │   │   └── images/
│   │   │
│   │   ├── layouts/                       # Layout components
│   │   │   ├── main-layout/
│   │   │   │   ├── main-layout.component.ts
│   │   │   │   ├── main-layout.component.html
│   │   │   │   ├── sidebar/
│   │   │   │   │   ├── sidebar.component.ts
│   │   │   │   │   └── sidebar.component.html
│   │   │   │   ├── header/
│   │   │   │   │   ├── header.component.ts
│   │   │   │   │   └── header.component.html
│   │   │   │   └── footer/
│   │   │   │       ├── footer.component.ts
│   │   │   │       └── footer.component.html
│   │   │   └── auth-layout/
│   │   │       ├── auth-layout.component.ts
│   │   │       └── auth-layout.component.html
│   │   │
│   │   ├── modules/                       # Feature modules
│   │   │   ├── home/
│   │   │   │   ├── dashboard/             # Home dashboard
│   │   │   │   │   ├── components/
│   │   │   │   │   ├── dashboard.component.ts
│   │   │   │   │   ├── dashboard.component.html
│   │   │   │   │   ├── dashboard.module.ts
│   │   │   │   │   └── dashboard-routing.module.ts
│   │   │   │   └── home.module.ts
│   │   │   │
│   │   │   ├── user-management/
│   │   │   │   ├── users/
│   │   │   │   ├── roles/
│   │   │   │   ├── permissions/
│   │   │   │   └── user-management.module.ts
│   │   │   │
│   │   │   ├── employees/
│   │   │   │   ├── components/
│   │   │   │   │   ├── employee-list/
│   │   │   │   │   ├── employee-create/
│   │   │   │   │   ├── employee-edit/
│   │   │   │   │   └── employee-detail/
│   │   │   │   ├── services/
│   │   │   │   │   └── employee.service.ts
│   │   │   │   ├── models/
│   │   │   │   │   └── employee.model.ts
│   │   │   │   └── employees.module.ts
│   │   │   │
│   │   │   ├── charities/
│   │   │   ├── families/
│   │   │   ├── orphans/
│   │   │   ├── sponsors/
│   │   │   ├── periodic-reports/
│   │   │   ├── notifications/
│   │   │   └── settings/
│   │   │
│   │   ├── store/                         # NgRx store
│   │   │   ├── actions/
│   │   │   ├── reducers/
│   │   │   ├── effects/
│   │   │   ├── selectors/
│   │   │   └── state.ts
│   │   │
│   │   ├── app.component.ts
│   │   ├── app.component.html
│   │   ├── app.component.scss
│   │   ├── app.module.ts
│   │   ├── app-routing.module.ts
│   │   └── app.config.ts
│   │
│   ├── environments/
│   │   ├── environment.ts
│   │   └── environment.prod.ts
│   │
│   ├── index.html
│   ├── main.ts
│   ├── styles.scss
│   └── polyfills.ts
│
├── angular.json
├── package.json
├── tsconfig.json
└── tsconfig.app.json
```

---

## 1. Localization (i18n) Implementation

### 1.1 Localization JSON Structure

#### **en.json** (English)
```json
{
  "common": {
    "actions": "Actions",
    "add": "Add",
    "edit": "Edit",
    "delete": "Delete",
    "save": "Save",
    "cancel": "Cancel",
    "submit": "Submit",
    "search": "Search",
    "filter": "Filter",
    "export": "Export",
    "import": "Import",
    "print": "Print",
    "view": "View",
    "details": "Details",
    "back": "Back",
    "next": "Next",
    "previous": "Previous",
    "close": "Close",
    "confirm": "Confirm",
    "yes": "Yes",
    "no": "No",
    "loading": "Loading...",
    "noData": "No data available",
    "success": "Success",
    "error": "Error",
    "warning": "Warning",
    "info": "Information",
    "selectAll": "Select All",
    "clear": "Clear",
    "active": "Active",
    "inactive": "Inactive",
    "status": "Status",
    "date": "Date",
    "createdDate": "Created Date",
    "modifiedDate": "Modified Date",
    "createdBy": "Created By",
    "modifiedBy": "Modified By",
    "total": "Total",
    "selected": "Selected",
    "records": "records",
    "perPage": "Per Page",
    "showing": "Showing",
    "to": "to",
    "of": "of",
    "page": "Page",
    "isRequired": "is required",
    "invalidFormat": "Invalid format",
    "operationSuccess": "Operation completed successfully",
    "operationFailed": "Operation failed",
    "deleteConfirm": "Are you sure you want to delete this record?",
    "deleteSuccess": "Record deleted successfully",
    "saveSuccess": "Record saved successfully",
    "exportToExcel": "Export to Excel"
  },
  "auth": {
    "login": "Login",
    "logout": "Logout",
    "username": "Username",
    "password": "Password",
    "rememberMe": "Remember Me",
    "forgotPassword": "Forgot Password?",
    "resetPassword": "Reset Password",
    "loginTitle": "Sign In to IIROSA",
    "loginSubtitle": "Enter your credentials to access your account",
    "invalidCredentials": "Invalid username or password",
    "loginSuccess": "Login successful",
    "logoutSuccess": "Logout successful"
  },
  "dashboard": {
    "title": "Dashboard",
    "welcome": "Welcome",
    "statistics": "Statistics",
    "totalCharities": "Total Charities",
    "totalFamilies": "Total Families",
    "totalOrphans": "Total Orphans",
    "totalSponsors": "Total Sponsors",
    "activeReports": "Active Reports",
    "pendingApprovals": "Pending Approvals",
    "recentActivities": "Recent Activities",
    "notifications": "Notifications",
    "charts": {
      "orphanGrowth": "Orphan Growth Trend",
      "sponsorshipDistribution": "Sponsorship Distribution",
      "familyStatus": "Family Status Overview",
      "monthlyDonations": "Monthly Donations"
    }
  },
  "userManagement": {
    "title": "User Management",
    "users": "Users",
    "roles": "Roles",
    "permissions": "Permissions",
    "addUser": "Add User",
    "editUser": "Edit User",
    "deleteUser": "Delete User",
    "userDetails": "User Details",
    "fullName": "Full Name",
    "email": "Email",
    "phoneNumber": "Phone Number",
    "role": "Role",
    "permissions": "Permissions",
    "assignRoles": "Assign Roles",
    "assignPermissions": "Assign Permissions",
    "userName": "Username",
    "isActive": "Is Active",
    "lastLogin": "Last Login"
  },
  "employees": {
    "title": "Employees",
    "addEmployee": "Add Employee",
    "editEmployee": "Edit Employee",
    "deleteEmployee": "Delete Employee",
    "employeeDetails": "Employee Details",
    "employeeCode": "Employee Code",
    "fullName": "Full Name",
    "nationalId": "National ID",
    "dateOfBirth": "Date of Birth",
    "gender": "Gender",
    "male": "Male",
    "female": "Female",
    "phoneNumber": "Phone Number",
    "email": "Email",
    "address": "Address",
    "department": "Department",
    "position": "Position",
    "hireDate": "Hire Date",
    "salary": "Salary",
    "isActive": "Is Active",
    "notes": "Notes",
    "genderList": {
      "male": "Male",
      "female": "Female"
    }
  },
  "charities": {
    "title": "Charities",
    "addCharity": "Add Charity",
    "editCharity": "Edit Charity",
    "deleteCharity": "Delete Charity",
    "charityDetails": "Charity Details",
    "charityCode": "Charity Code",
    "charityName": "Charity Name",
    "contactPerson": "Contact Person",
    "contactPhone": "Contact Phone",
    "contactEmail": "Contact Email",
    "address": "Address",
    "country": "Country",
    "city": "City",
    "licenseNumber": "License Number",
    "licenseExpiry": "License Expiry Date",
    "isActive": "Is Active",
    "totalOrphans": "Total Orphans",
    "totalFamilies": "Total Families",
    "establishmentDate": "Establishment Date",
    "website": "Website",
    "notes": "Notes"
  },
  "families": {
    "title": "Families",
    "addFamily": "Add Family",
    "editFamily": "Edit Family",
    "deleteFamily": "Delete Family",
    "familyDetails": "Family Details",
    "familyCode": "Family Code",
    "headOfFamily": "Head of Family",
    "phoneNumber": "Phone Number",
    "address": "Address",
    "country": "Country",
    "city": "City",
    "charity": "Charity",
    "familyStatus": "Family Status",
    "financialStatus": "Financial Status",
    "housingStatus": "Housing Status",
    "familyMembersCount": "Family Members Count",
    "orphansCount": "Orphans Count",
    "monthlyIncome": "Monthly Income",
    "monthlyAssistance": "Monthly Assistance",
    "notes": "Notes"
  },
  "orphans": {
    "title": "Orphans",
    "addOrphan": "Add Orphan",
    "editOrphan": "Edit Orphan",
    "deleteOrphan": "Delete Orphan",
    "orphanDetails": "Orphan Details",
    "orphanCode": "Orphan Code",
    "fullName": "Full Name",
    "family": "Family",
    "dateOfBirth": "Date of Birth",
    "gender": "Gender",
    "male": "Male",
    "female": "Female",
    "nationalId": "National ID",
    "educationLevel": "Education Level",
    "healthStatus": "Health Status",
    "sponsor": "Sponsor",
    "sponsorshipStartDate": "Sponsorship Start Date",
    "monthlyAmount": "Monthly Amount",
    "notes": "Notes"
  },
  "sponsors": {
    "title": "Sponsors",
    "addSponsor": "Add Sponsor",
    "editSponsor": "Edit Sponsor",
    "deleteSponsor": "Delete Sponsor",
    "sponsorDetails": "Sponsor Details",
    "sponsorCode": "Sponsor Code",
    "fullName": "Full Name",
    "phoneNumber": "Phone Number",
    "email": "Email",
    "address": "Address",
    "country": "Country",
    "city": "City",
    "sponsorshipType": "Sponsorship Type",
    "monthlyAmount": "Monthly Amount",
    "paymentMethod": "Payment Method",
    "isActive": "Is Active",
    "orphanCount": "Number of Orphans",
    "totalSponsored": "Total Sponsored",
    "notes": "Notes"
  },
  "periodicReports": {
    "title": "Periodic Orphan Reports",
    "addReport": "Add Report",
    "editReport": "Edit Report",
    "deleteReport": "Delete Report",
    "reportDetails": "Report Details",
    "orphan": "Orphan",
    "reportDate": "Report Date",
    "reportYear": "Report Year",
    "reportMonth": "Report Month",
    "charity": "Charity",
    "submittedBy": "Submitted By",
    "submissionDate": "Submission Date",
    "reviewStatus": "Review Status",
    "reviewedBy": "Reviewed By",
    "reviewDate": "Review Date",
    "reviewComments": "Review Comments",
    "religiousSection": "Religious Information",
    "prayerStatus": "Prayer Status",
    "quranMemorization": "Quran Memorization",
    "quranParts": "Quran Parts Memorized",
    "mannersStatus": "Manners & Behavior Status",
    "educationSection": "Education Information",
    "educationLevel": "Education Level",
    "educationStage": "Education Stage",
    "academicPerformance": "Academic Performance",
    "schoolName": "School Name",
    "healthSection": "Health Information",
    "healthStatus": "Health Status",
    "chronicDiseases": "Chronic Diseases",
    "medicalNotes": "Medical Notes",
    "personalDevelopmentSection": "Personal Development",
    "skills": "Skills & Talents",
    "hobbies": "Hobbies",
    "personalNotes": "Personal Development Notes",
    "lifeEventsSection": "Life Events",
    "majorEvents": "Major Life Events",
    "achievements": "Achievements",
    "challenges": "Challenges",
    "documentsSection": "Documents",
    "attachedDocuments": "Attached Documents",
    "exportReport": "Export Report",
    "printReport": "Print Report",
    "approveReport": "Approve Report",
    "rejectReport": "Reject Report",
    "status": {
      "pending": "Pending",
      "approved": "Approved",
      "rejected": "Rejected"
    }
  },
  "notifications": {
    "title": "Notifications",
    "markAllRead": "Mark All as Read",
    "markAsRead": "Mark as Read",
    "delete": "Delete",
    "notificationSettings": "Notification Settings",
    "enableNotifications": "Enable Notifications",
    "quietHours": "Quiet Hours",
    "quietHoursStart": "Quiet Hours Start",
    "quietHoursEnd": "Quiet Hours End",
    "types": {
      "system": "System",
      "approval": "Approval",
      "report": "Report",
      "reminder": "Reminder",
      "alert": "Alert"
    }
  },
  "settings": {
    "title": "Settings",
    "generalSettings": "General Settings",
    "notificationSettings": "Notification Settings",
    "securitySettings": "Security Settings",
    "profileSettings": "Profile Settings",
    "systemSettings": "System Settings",
    "language": "Language",
    "theme": "Theme",
    "darkMode": "Dark Mode",
    "lightMode": "Light Mode",
    "timezone": "Timezone",
    "dateFormat": "Date Format",
    "timeFormat": "Time Format",
    "changePassword": "Change Password",
    "currentPassword": "Current Password",
    "newPassword": "New Password",
    "confirmPassword": "Confirm Password"
  },
  "validation": {
    "required": "This field is required",
    "email": "Invalid email address",
    "minLength": "Minimum length is {{minLength}} characters",
    "maxLength": "Maximum length is {{maxLength}} characters",
    "pattern": "Invalid format",
    "min": "Minimum value is {{min}}",
    "max": "Maximum value is {{max}}",
    "dateInvalid": "Invalid date",
    "phoneInvalid": "Invalid phone number",
    "nationalIdInvalid": "Invalid National ID",
    "passwordMismatch": "Passwords do not match",
    "passwordWeak": "Password is too weak",
    "emailExists": "Email already exists",
    "usernameExists": "Username already exists",
    "codeExists": "Code already exists"
  },
  "errors": {
    "serverError": "Server error occurred. Please try again later.",
    "networkError": "Network error. Please check your connection.",
    "unauthorized": "You are not authorized to perform this action.",
    "forbidden": "Access denied.",
    "notFound": "Resource not found.",
    "unknownError": "An unknown error occurred."
  }
}
```

#### **ar.json** (Arabic)
```json
{
  "common": {
    "actions": "الإجراءات",
    "add": "إضافة",
    "edit": "تعديل",
    "delete": "حذف",
    "save": "حفظ",
    "cancel": "إلغاء",
    "submit": "إرسال",
    "search": "بحث",
    "filter": "تصفية",
    "export": "تصدير",
    "import": "استيراد",
    "print": "طباعة",
    "view": "عرض",
    "details": "التفاصيل",
    "back": "رجوع",
    "next": "التالي",
    "previous": "السابق",
    "close": "إغلاق",
    "confirm": "تأكيد",
    "yes": "نعم",
    "no": "لا",
    "loading": "جاري التحميل...",
    "noData": "لا توجد بيانات",
    "success": "نجاح",
    "error": "خطأ",
    "warning": "تحذير",
    "info": "معلومات",
    "selectAll": "تحديد الكل",
    "clear": "مسح",
    "active": "نشط",
    "inactive": "غير نشط",
    "status": "الحالة",
    "date": "التاريخ",
    "createdDate": "تاريخ الإنشاء",
    "modifiedDate": "تاريخ التعديل",
    "createdBy": "أنشأ بواسطة",
    "modifiedBy": "عدل بواسطة",
    "total": "الإجمالي",
    "selected": "المحدد",
    "records": "سجلات",
    "perPage": "لكل صفحة",
    "showing": "عرض",
    "to": "إلى",
    "of": "من",
    "page": "صفحة",
    "isRequired": "مطلوب",
    "invalidFormat": "تنسيق غير صالح",
    "operationSuccess": "تمت العملية بنجاح",
    "operationFailed": "فشلت العملية",
    "deleteConfirm": "هل أنت متأكد من حذف هذا السجل؟",
    "deleteSuccess": "تم حذف السجل بنجاح",
    "saveSuccess": "تم حفظ السجل بنجاح",
    "exportToExcel": "تصدير إلى Excel"
  },
  "auth": {
    "login": "تسجيل الدخول",
    "logout": "تسجيل الخروج",
    "username": "اسم المستخدم",
    "password": "كلمة المرور",
    "rememberMe": "تذكرني",
    "forgotPassword": "نسيت كلمة المرور؟",
    "resetPassword": "إعادة تعيين كلمة المرور",
    "loginTitle": "تسجيل الدخول إلى منظمة الأيتام",
    "loginSubtitle": "أدخل بيانات الاعتماد الخاصة بك للوصول إلى حسابك",
    "invalidCredentials": "اسم المستخدم أو كلمة المرور غير صالحة",
    "loginSuccess": "تم تسجيل الدخول بنجاح",
    "logoutSuccess": "تم تسجيل الخروج بنجاح"
  },
  "dashboard": {
    "title": "لوحة التحكم",
    "welcome": "مرحباً",
    "statistics": "الإحصائيات",
    "totalCharities": "إجمالي الجمعيات",
    "totalFamilies": "إجمالي الأسر",
    "totalOrphans": "إجمالي الأيتام",
    "totalSponsors": "إجمالي الكفلاء",
    "activeReports": "التقارير النشطة",
    "pendingApprovals": "الموافقات المعلقة",
    "recentActivities": "الأنشطة الأخيرة",
    "notifications": "الإشعارات",
    "charts": {
      "orphanGrowth": "معدل نمو الأيتام",
      "sponsorshipDistribution": "توزيع الكفالات",
      "familyStatus": "نظرة عامة على حالة الأسرة",
      "monthlyDonations": "التبرعات الشهرية"
    }
  },
  "userManagement": {
    "title": "إدارة المستخدمين",
    "users": "المستخدمون",
    "roles": "الأدوار",
    "permissions": "الصلاحيات",
    "addUser": "إضافة مستخدم",
    "editUser": "تعديل مستخدم",
    "deleteUser": "حذف مستخدم",
    "userDetails": "تفاصيل المستخدم",
    "fullName": "الاسم الكامل",
    "email": "البريد الإلكتروني",
    "phoneNumber": "رقم الهاتف",
    "role": "الدور",
    "permissions": "الصلاحيات",
    "assignRoles": "تعيين الأدوار",
    "assignPermissions": "تعيين الصلاحيات",
    "userName": "اسم المستخدم",
    "isActive": "نشط",
    "lastLogin": "آخر تسجيل دخول"
  },
  "employees": {
    "title": "الموظفون",
    "addEmployee": "إضافة موظف",
    "editEmployee": "تعديل موظف",
    "deleteEmployee": "حذف موظف",
    "employeeDetails": "تفاصيل الموظف",
    "employeeCode": "رمز الموظف",
    "fullName": "الاسم الكامل",
    "nationalId": "رقم الهوية",
    "dateOfBirth": "تاريخ الميلاد",
    "gender": "الجنس",
    "male": "ذكر",
    "female": "أنثى",
    "phoneNumber": "رقم الهاتف",
    "email": "البريد الإلكتروني",
    "address": "العنوان",
    "department": "القسم",
    "position": "المنصب",
    "hireDate": "تاريخ التعيين",
    "salary": "الراتب",
    "isActive": "نشط",
    "notes": "ملاحظات"
  },
  "charities": {
    "title": "الجمعيات",
    "addCharity": "إضافة جمعية",
    "editCharity": "تعديل جمعية",
    "deleteCharity": "حذف جمعية",
    "charityDetails": "تفاصيل الجمعية",
    "charityCode": "رمز الجمعية",
    "charityName": "اسم الجمعية",
    "contactPerson": "الشخص المسؤول",
    "contactPhone": "هاتف الاتصال",
    "contactEmail": "البريد الإلكتروني",
    "address": "العنوان",
    "country": "الدولة",
    "city": "المدينة",
    "licenseNumber": "رقم الترخيص",
    "licenseExpiry": "تاريخ انتهاء الترخيص",
    "isActive": "نشط",
    "totalOrphans": "إجمالي الأيتام",
    "totalFamilies": "إجمالي الأسر",
    "establishmentDate": "تاريخ التأسيس",
    "website": "الموقع الإلكتروني",
    "notes": "ملاحظات"
  },
  "families": {
    "title": "الأسر",
    "addFamily": "إضافة أسرة",
    "editFamily": "تعديل أسرة",
    "deleteFamily": "حذف أسرة",
    "familyDetails": "تفاصيل الأسرة",
    "familyCode": "رمز الأسرة",
    "headOfFamily": "معيل الأسرة",
    "phoneNumber": "رقم الهاتف",
    "address": "العنوان",
    "country": "الدولة",
    "city": "المدينة",
    "charity": "الجمعية",
    "familyStatus": "حالة الأسرة",
    "financialStatus": "الحالة المالية",
    "housingStatus": "حالة السكن",
    "familyMembersCount": "عدد أفراد الأسرة",
    "orphansCount": "عدد الأيتام",
    "monthlyIncome": "الدخل الشهري",
    "monthlyAssistance": "المساعدة الشهرية",
    "notes": "ملاحظات"
  },
  "orphans": {
    "title": "الأيتام",
    "addOrphan": "إضافة يتيم",
    "editOrphan": "تعديل يتيم",
    "deleteOrphan": "حذف يتيم",
    "orphanDetails": "تفاصيل اليتيم",
    "orphanCode": "رمز اليتيم",
    "fullName": "الاسم الكامل",
    "family": "الأسرة",
    "dateOfBirth": "تاريخ الميلاد",
    "gender": "الجنس",
    "male": "ذكر",
    "female": "أنثى",
    "nationalId": "رقم الهوية",
    "educationLevel": "المستوى التعليمي",
    "healthStatus": "الحالة الصحية",
    "sponsor": "الكافل",
    "sponsorshipStartDate": "تاريخ بدء الكفالة",
    "monthlyAmount": "المبلغ الشهري",
    "notes": "ملاحظات"
  },
  "sponsors": {
    "title": "الكفلاء",
    "addSponsor": "إضافة كافل",
    "editSponsor": "تعديل كافل",
    "deleteSponsor": "حذف كافل",
    "sponsorDetails": "تفاصيل الكافل",
    "sponsorCode": "رمز الكافل",
    "fullName": "الاسم الكامل",
    "phoneNumber": "رقم الهاتف",
    "email": "البريد الإلكتروني",
    "address": "العنوان",
    "country": "الدولة",
    "city": "المدينة",
    "sponsorshipType": "نوع الكفالة",
    "monthlyAmount": "المبلغ الشهري",
    "paymentMethod": "طريقة الدفع",
    "isActive": "نشط",
    "orphanCount": "عدد الأيتام",
    "totalSponsored": "إجمالي المكفولين",
    "notes": "ملاحظات"
  },
  "periodicReports": {
    "title": "التقارير الدورية للأيتام",
    "addReport": "إضافة تقرير",
    "editReport": "تعديل تقرير",
    "deleteReport": "حذف تقرير",
    "reportDetails": "تفاصيل التقرير",
    "orphan": "اليتيم",
    "reportDate": "تاريخ التقرير",
    "reportYear": "سنة التقرير",
    "reportMonth": "شهر التقرير",
    "charity": "الجمعية",
    "submittedBy": "قدم بواسطة",
    "submissionDate": "تاريخ التقديم",
    "reviewStatus": "حالة المراجعة",
    "reviewedBy": "راجع بواسطة",
    "reviewDate": "تاريخ المراجعة",
    "reviewComments": "تعليقات المراجعة",
    "religiousSection": "المعلومات الدينية",
    "prayerStatus": "حالة الصلاة",
    "quranMemorization": "حفظ القرآن",
    "quranParts": "الأجزاء المحفوظة",
    "mannersStatus": "حالة السلوك والأخلاق",
    "educationSection": "المعلومات التعليمية",
    "educationLevel": "المستوى التعليمي",
    "educationStage": "المرحلة التعليمية",
    "academicPerformance": "الأداء الأكاديمي",
    "schoolName": "اسم المدرسة",
    "healthSection": "المعلومات الصحية",
    "healthStatus": "الحالة الصحية",
    "chronicDiseases": "الأمراض المزمنة",
    "medicalNotes": "ملاحظات طبية",
    "personalDevelopmentSection": "التطور الشخصي",
    "skills": "المهارات والمواهب",
    "hobbies": "الهوايات",
    "personalNotes": "ملاحظات التطور الشخصي",
    "lifeEventsSection": "أحداث الحياة",
    "majorEvents": "الأحداث الرئيسية",
    "achievements": "الإنجازات",
    "challenges": "التحديات",
    "documentsSection": "المستندات",
    "attachedDocuments": "المستندات المرفقة",
    "exportReport": "تصدير التقرير",
    "printReport": "طباعة التقرير",
    "approveReport": "موافقة على التقرير",
    "rejectReport": "رفض التقرير",
    "status": {
      "pending": "معلق",
      "approved": "مقبول",
      "rejected": "مرفوض"
    }
  },
  "notifications": {
    "title": "الإشعارات",
    "markAllRead": "وضع علامة مقروء على الكل",
    "markAsRead": "وضع علامة مقروء",
    "delete": "حذف",
    "notificationSettings": "إعدادات الإشعارات",
    "enableNotifications": "تفعيل الإشعارات",
    "quietHours": "ساعات الهدوء",
    "quietHoursStart": "بداية ساعات الهدوء",
    "quietHoursEnd": "نهاية ساعات الهدوء",
    "types": {
      "system": "نظام",
      "approval": "موافقة",
      "report": "تقرير",
      "reminder": "تذكير",
      "alert": "تنبيه"
    }
  },
  "settings": {
    "title": "الإعدادات",
    "generalSettings": "الإعدادات العامة",
    "notificationSettings": "إعدادات الإشعارات",
    "securitySettings": "إعدادات الأمان",
    "profileSettings": "إعدادات الملف الشخصي",
    "systemSettings": "إعدادات النظام",
    "language": "اللغة",
    "theme": "المظهر",
    "darkMode": "الوضع الداكن",
    "lightMode": "الوضع الفاتح",
    "timezone": "المنطقة الزمنية",
    "dateFormat": "تنسيق التاريخ",
    "timeFormat": "تنسيق الوقت",
    "changePassword": "تغيير كلمة المرور",
    "currentPassword": "كلمة المرور الحالية",
    "newPassword": "كلمة المرور الجديدة",
    "confirmPassword": "تأكيد كلمة المرور"
  },
  "validation": {
    "required": "هذا الحقل مطلوب",
    "email": "عنوان بريد إلكتروني غير صالح",
    "minLength": "الحد الأدنى للطول {{minLength}} أحرف",
    "maxLength": "الحد الأقصى للطول {{maxLength}} أحرف",
    "pattern": "تنسيق غير صالح",
    "min": "الحد الأدنى للقيمة {{min}}",
    "max": "الحد الأقصى للقيمة {{max}}",
    "dateInvalid": "تاريخ غير صالح",
    "phoneInvalid": "رقم هاتف غير صالح",
    "nationalIdInvalid": "رقم هوية غير صالح",
    "passwordMismatch": "كلمات المرور غير متطابقة",
    "passwordWeak": "كلمة المرور ضعيفة جداً",
    "emailExists": "البريد الإلكتروني موجود بالفعل",
    "usernameExists": "اسم المستخدم موجود بالفعل",
    "codeExists": "الرمز موجود بالفعل"
  },
  "errors": {
    "serverError": "حدث خطأ في الخادم. يرجى المحاولة مرة أخرى لاحقاً.",
    "networkError": "خطأ في الشبكة. يرجى التحقق من الاتصال الخاص بك.",
    "unauthorized": "لست مخولاً للقيام بهذا الإجراء.",
    "forbidden": "تم رفض الوصول.",
    "notFound": "المورد غير موجود.",
    "unknownError": "حدث خطأ غير معروف."
  }
}
```

### 1.2 Translation Service Setup

```typescript
// app.config.ts
import { ApplicationConfig } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { HttpClient } from '@angular/common/http';

export function HttpLoaderFactory(http: HttpClient): TranslateHttpLoader {
  return new TranslateHttpLoader(http, './assets/i18n/', '.json');
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(),
    TranslateModule.forRoot({
      defaultLanguage: 'ar',
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient]
      }
    }).providers!
  ]
};
```

---

## 2. Shared Components

### 2.1 Attachment Component

#### **attachment.component.ts**
```typescript
import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface AttachedFile {
  id: string;
  name: string;
  size: number;
  type: string;
  url?: string;
  file?: File;
  uploadedAt?: Date;
}

@Component({
  selector: 'app-attachment',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './attachment.component.html',
  styleUrls: ['./attachment.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => AttachmentComponent),
      multi: true
    }
  ]
})
export class AttachmentComponent implements ControlValueAccessor {
  @Input() maxFiles: number = 5;
  @Input() maxFileSize: number = 10 * 1024 * 1024; // 10MB
  @Input() allowedTypes: string[] = ['image/*', 'application/pdf', '.doc', '.docx'];
  @Input() accept: string = '*';
  @Input() multiple: boolean = true;
  @Input() showSize: boolean = true;
  @Input() showType: boolean = true;
  @Input() disabled: boolean = false;
  @Input() uploadUrl?: string;
  @Input() autoUpload: boolean = false;

  @Output() onFileSelect = new EventEmitter<File[]>();
  @Output() onFileRemove = new EventEmitter<AttachedFile>();
  @Output() onUploadStart = new EventEmitter();
  @Output() onUploadComplete = new EventEmitter<AttachedFile[]>();
  @Output() onUploadError = new EventEmitter<any>();

  selectedFiles: AttachedFile[] = [];
  isDragging: boolean = false;
  uploadProgress: number = 0;
  isUploading: boolean = false;

  private onChange: (value: AttachedFile[]) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(value: AttachedFile[]): void {
    this.selectedFiles = value || [];
  }

  registerOnChange(fn: (value: AttachedFile[]) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    if (!this.disabled) {
      this.isDragging = true;
    }
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;

    if (this.disabled) return;

    const files = event.dataTransfer?.files;
    if (files) {
      this.handleFiles(Array.from(files));
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.handleFiles(Array.from(input.files));
      input.value = ''; // Reset input
    }
  }

  private handleFiles(files: File[]): void {
    const validFiles: File[] = [];

    // Check max files limit
    if (this.selectedFiles.length + files.length > this.maxFiles) {
      alert(`Maximum ${this.maxFiles} files allowed`);
      return;
    }

    // Validate each file
    files.forEach(file => {
      if (file.size > this.maxFileSize) {
        alert(`File ${file.name} exceeds maximum size of ${this.formatFileSize(this.maxFileSize)}`);
        return;
      }

      if (!this.isFileTypeAllowed(file)) {
        alert(`File type ${file.type} is not allowed`);
        return;
      }

      validFiles.push(file);
    });

    // Create attachment objects
    validFiles.forEach(file => {
      const attachment: AttachedFile = {
        id: this.generateId(),
        name: file.name,
        size: file.size,
        type: file.type,
        file: file,
        url: this.createObjectURL(file)
      };

      this.selectedFiles.push(attachment);
    });

    this.onTouched();
    this.onChange(this.selectedFiles);
    this.onFileSelect.emit(validFiles);

    if (this.autoUpload && this.uploadUrl) {
      this.uploadFiles();
    }
  }

  private isFileTypeAllowed(file: File): boolean {
    if (this.allowedTypes.includes('*')) return true;

    return this.allowedTypes.some(type => {
      if (type.startsWith('.')) {
        return file.name.toLowerCase().endsWith(type.toLowerCase());
      }
      return file.type.match(type.replace('*', '.*')) !== null;
    });
  }

  removeFile(file: AttachedFile): void {
    const index = this.selectedFiles.findIndex(f => f.id === file.id);
    if (index > -1) {
      this.selectedFiles.splice(index, 1);
      this.revokeObjectURL(file.url);
      this.onTouched();
      this.onChange(this.selectedFiles);
      this.onFileRemove.emit(file);
    }
  }

  getFileIcon(file: AttachedFile): string {
    const type = file.type.toLowerCase();
    if (type.includes('image')) return 'fe fe-image';
    if (type.includes('pdf')) return 'fe fe-file-text';
    if (type.includes('word') || type.includes('document')) return 'fe fe-file';
    if (type.includes('excel') || type.includes('spreadsheet')) return 'fe fe-file-plus';
    if (type.includes('video')) return 'fe fe-video';
    if (type.includes('audio')) return 'fe fe-music';
    return 'fe fe-file';
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  async uploadFiles(): Promise<void> {
    if (!this.uploadUrl || this.isUploading) return;

    this.isUploading = true;
    this.uploadProgress = 0;
    this.onUploadStart.emit();

    try {
      const formData = new FormData();
      this.selectedFiles.forEach(file => {
        if (file.file) {
          formData.append('files', file.file);
        }
      });

      // TODO: Implement actual upload with your HTTP service
      // const response = await this.http.post(this.uploadUrl, formData).toPromise();

      this.uploadProgress = 100;
      this.onUploadComplete.emit(this.selectedFiles);
    } catch (error) {
      this.onUploadError.emit(error);
    } finally {
      this.isUploading = false;
    }
  }

  private generateId(): string {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  private createObjectURL(file: File): string {
    return URL.createObjectURL(file);
  }

  private revokeObjectURL(url?: string): void {
    if (url) {
      URL.revokeObjectURL(url);
    }
  }

  ngOnDestroy(): void {
    this.selectedFiles.forEach(file => this.revokeObjectURL(file.url));
  }
}
```

#### **attachment.component.html**
```html
<div class="attachment-container"
     [class.dragging]="isDragging"
     [class.disabled]="disabled">

  <!-- Drop Zone -->
  <div class="drop-zone"
       [class.drag-over]="isDragging"
       dragover="false"
       (dragover)="onDragOver($event)"
       (dragleave)="onDragLeave($event)"
       (drop)="onDrop($event)"
       [class.d-none]="selectedFiles?.length >= maxFiles">

    <div class="drop-zone-content">
      <i class="fe fe-upload-cloud fe-3x mb-3"></i>
      <p class="mb-2">{{ 'common.dragDrop' | translate }}</p>
      <p class="text-muted mb-3">{{ 'common.or' | translate }}</p>
      <label class="btn btn-primary">
        <i class="fe fe-folder mr-1"></i>
        {{ 'common.browse' | translate }}
        <input type="file"
               class="d-none"
               [accept]="accept"
               [multiple]="multiple"
               [disabled]="disabled"
               (change)="onFileSelected($event)">
      </label>
      <p class="text-muted small mt-2">
        {{ 'common.maxFiles' | translate }}: {{ maxFiles }} |
        {{ 'common.maxSize' | translate }}: {{ formatFileSize(maxFileSize) }}
      </p>
    </div>
  </div>

  <!-- Upload Progress -->
  <div class="upload-progress" *ngIf="isUploading">
    <div class="progress">
      <div class="progress-bar progress-bar-striped progress-bar-animated"
           role="progressbar"
           [style.width.%]="uploadProgress">
        {{ uploadProgress }}%
      </div>
    </div>
  </div>

  <!-- Selected Files List -->
  <div class="files-list" *ngIf="selectedFiles?.length > 0">
    <div class="file-item"
         *ngFor="let file of selectedFiles"
         [class.uploading]="isUploading">

      <div class="file-icon">
        <i [class]="getFileIcon(file)"></i>
      </div>

      <div class="file-info">
        <div class="file-name" [title]="file.name">{{ file.name }}</div>
        <div class="file-meta" *ngIf="showSize || showType">
          <span *ngIf="showSize" class="file-size">{{ formatFileSize(file.size) }}</span>
          <span *ngIf="showSize && showType" class="mx-1">•</span>
          <span *ngIf="showType" class="file-type">{{ file.type || 'Unknown' }}</span>
        </div>
      </div>

      <div class="file-actions">
        <button type="button"
                class="btn btn-sm btn-icon btn-danger"
                [disabled]="disabled || isUploading"
                (click)="removeFile(file)"
                [title]="'common.remove' | translate">
          <i class="fe fe-trash"></i>
        </button>
      </div>
    </div>
  </div>

  <!-- Validation Message -->
  <div class="invalid-feedback d-block"
       *ngIf="selectedFiles?.length >= maxFiles">
    {{ 'validation.maxFilesReached' | translate }}
  </div>
</div>
```

#### **attachment.component.scss**
```scss
.attachment-container {
  border: 2px dashed var(--border-color);
  border-radius: 0.5rem;
  padding: 1rem;
  transition: all 0.3s ease;

  &.dragging {
    border-color: var(--primary);
    background-color: rgba(var(--primary-rgb), 0.05);
  }

  &.disabled {
    opacity: 0.6;
    pointer-events: none;
  }

  .drop-zone {
    text-align: center;
    padding: 2rem;
    transition: all 0.3s ease;

    &.drag-over {
      background-color: rgba(var(--primary-rgb), 0.1);
      border-radius: 0.5rem;
    }

    .drop-zone-content {
      i {
        color: var(--primary);
      }
    }
  }

  .upload-progress {
    margin-top: 1rem;
  }

  .files-list {
    margin-top: 1rem;

    .file-item {
      display: flex;
      align-items: center;
      padding: 0.75rem;
      border: 1px solid var(--border-color);
      border-radius: 0.375rem;
      margin-bottom: 0.5rem;
      transition: all 0.2s ease;

      &:hover {
        background-color: rgba(var(--primary-rgb), 0.05);
      }

      &.uploading {
        opacity: 0.6;
      }

      .file-icon {
        width: 40px;
        height: 40px;
        display: flex;
        align-items: center;
        justify-content: center;
        background-color: rgba(var(--primary-rgb), 0.1);
        border-radius: 0.375rem;
        margin-right: 0.75rem;

        i {
          font-size: 1.25rem;
          color: var(--primary);
        }
      }

      .file-info {
        flex: 1;
        min-width: 0;

        .file-name {
          font-weight: 500;
          white-space: nowrap;
          overflow: hidden;
          text-overflow: ellipsis;
        }

        .file-meta {
          font-size: 0.875rem;
          color: var(--text-muted);
          margin-top: 0.25rem;
        }
      }

      .file-actions {
        .btn-icon {
          width: 32px;
          height: 32px;
          padding: 0;
          display: inline-flex;
          align-items: center;
          justify-content: center;
          border-radius: 0.375rem;
        }
      }
    }
  }
}
```

---

### 2.2 Input Fields Component (Dynamic)

#### **input-fields.component.ts**
```typescript
import { Component, Input, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormControl } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

export interface InputFieldConfig {
  key: string;
  type: 'text' | 'number' | 'email' | 'password' | 'tel' | 'date' | 'datetime-local' |
        'textarea' | 'select' | 'multiselect' | 'checkbox' | 'radio' | 'file' | 'hidden';
  label?: string;
  placeholder?: string;
  required?: boolean;
  disabled?: boolean;
  readonly?: boolean;
  minLength?: number;
  maxLength?: number;
  min?: number;
  max?: number;
  pattern?: string;
  options?: Array<{ value: any; label: string; disabled?: boolean }>;
  defaultValue?: any;
  rows?: number; // For textarea
  accept?: string; // For file input
  multiple?: boolean; // For file/multiselect
  icon?: string; // Feather icon class
  prefix?: string; // Prefix text/icon
  suffix?: string; // Suffix text/icon
  helpText?: string;
  cssClass?: string;
  colClass?: string; // Column width class (e.g., 'col-md-6')
}

@Component({
  selector: 'app-input-fields',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './input-fields.component.html',
  styleUrls: ['./input-fields.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputFieldsComponent),
      multi: true
    }
  ]
})
export class InputFieldsComponent implements ControlValueAccessor {
  @Input() fields: InputFieldConfig[] = [];
  @Input() layout: 'vertical' | 'horizontal' | 'inline' = 'vertical';
  @Input() labelWidth: string = '150px';
  @Input() showRequiredStar: boolean = true;
  @Input() floatingLabels: boolean = false;
  @Input() compactMode: boolean = false;

  formControls: { [key: string]: FormControl } = {};
  formData: any = {};

  private onChange: (value: any) => void = () => {};
  private onTouched: () => void = () => {};

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    this.fields.forEach(field => {
      const defaultValue = field.defaultValue !== undefined ? field.defaultValue : this.getDefaultValue(field.type);
      this.formControls[field.key] = new FormControl({
        value: defaultValue,
        disabled: field.disabled
      });
      this.formData[field.key] = defaultValue;
    });
  }

  private getDefaultValue(type: string): any {
    switch (type) {
      case 'checkbox':
        return false;
      case 'multiselect':
        return [];
      case 'number':
        return null;
      default:
        return null;
    }
  }

  writeValue(value: any): void {
    if (value) {
      this.formData = { ...value };
      Object.keys(value).forEach(key => {
        if (this.formControls[key]) {
          this.formControls[key].setValue(value[key]);
        }
      });
    }
  }

  registerOnChange(fn: (value: any) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    Object.values(this.formControls).forEach(control => {
      if (isDisabled) {
        control.disable();
      } else {
        control.enable();
      }
    });
  }

  onFieldValueChange(fieldKey: string, value: any): void {
    this.formData[fieldKey] = value;
    this.onTouched();
    this.onChange(this.formData);
  }

  getFieldTypeClass(type: string): string {
    const baseClass = 'form-control';
    switch (type) {
      case 'checkbox':
        return 'form-check-input';
      case 'radio':
        return 'form-check-input';
      case 'select':
      case 'multiselect':
        return `${baseClass} custom-select`;
      default:
        return baseClass;
    }
  }

  isInputType(type: string): boolean {
    return ['text', 'number', 'email', 'password', 'tel', 'date', 'datetime-local', 'hidden'].includes(type);
  }

  isSelectType(type: string): boolean {
    return ['select', 'multiselect'].includes(type);
  }

  isCheckableType(type: string): boolean {
    return ['checkbox', 'radio'].includes(type);
  }

  getValidationClasses(field: InputFieldConfig): { [key: string]: boolean } {
    const control = this.formControls[field.key];
    return {
      'is-invalid': control?.invalid && control?.touched,
      'is-valid': control?.valid && control?.touched
    };
  }

  getErrorMessage(field: InputFieldConfig): string {
    const control = this.formControls[field.key];
    if (!control?.errors) return '';

    if (control.errors['required']) {
      return `${field.label} ${'validation.required' | translate}`;
    }
    if (control.errors['minlength']) {
      return `${field.label} ${'validation.minLength' | translate}: ${field.minLength}`;
    }
    if (control.errors['maxlength']) {
      return `${field.label} ${'validation.maxLength' | translate}: ${field.maxLength}`;
    }
    if (control.errors['min']) {
      return `${field.label} ${'validation.min' | translate}: ${field.min}`;
    }
    if (control.errors['max']) {
      return `${field.label} ${'validation.max' | translate}: ${field.max}`;
    }
    if (control.errors['email']) {
      return `validation.email`;
    }
    if (control.errors['pattern']) {
      return `${field.label} ${'validation.pattern' | translate}`;
    }

    return 'validation.invalidFormat';
  }

  isFieldVisible(field: InputFieldConfig): boolean {
    return field.type !== 'hidden';
  }

  getFieldColClass(field: InputFieldConfig): string {
    return field.colClass || 'col-12';
  }
}
```

#### **input-fields.component.html**
```html
<div [class.form-horizontal]="layout === 'horizontal'"
     [class.form-inline]="layout === 'inline'">

  <div *ngFor="let field of fields"
       class="form-group"
       [class.row]="layout === 'horizontal'"
       [class.form-row]="layout === 'inline'"
       [ngClass]="getFieldColClass(field)"
       [style.display]="field.type === 'hidden' ? 'none' : 'block'">

    <!-- Label -->
    <label *ngIf="field.label && isFieldVisible(field)"
           [for]="field.key"
           [ngClass]="layout === 'horizontal' ? 'col-sm-' + (12 - getColSize(field)) : ''"
           [style.width]="layout === 'horizontal' ? labelWidth : ''"
           [class.required-field]="showRequiredStar && field.required">
      {{ field.label }}
    </label>

    <!-- Input Container -->
    <div [ngClass]="layout === 'horizontal' ? 'col-sm-' + getColSize(field) : ''"
         [class.input-group]="hasPrefixOrSuffix(field)">

      <!-- Prefix -->
      <div *ngIf="field.prefix" class="input-group-prepend">
        <span class="input-group-text">{{ field.prefix }}</span>
      </div>

      <!-- Icon Prefix -->
      <div *ngIf="field.icon && layout === 'vertical'" class="input-group-prepend">
        <span class="input-group-text"><i [class]="field.icon"></i></span>
      </div>

      <!-- Text/Number/Email/Password/Tel/Date Inputs -->
      <input *ngIf="isInputType(field.type)"
             [type]="field.type"
             [id]="field.key"
             [class]="getFieldTypeClass(field.type)"
             [placeholder]="field.placeholder"
             [required]="field.required"
             [disabled]="field.disabled"
             [readonly]="field.readonly"
             [minLength]="field.minLength"
             [maxLength]="field.maxLength"
             [min]="field.min"
             [max]="field.max"
             [pattern]="field.pattern"
             [formControl]="formControls[field.key]"
             [ngClass]="getValidationClasses(field)"
             [class.form-control-sm]="compactMode"
             [class.form-control-lg]="!compactMode && layout !== 'inline'"
             (change)="onFieldValueChange(field.key, $event.target.value)">

      <!-- Textarea -->
      <textarea *ngIf="field.type === 'textarea'"
                [id]="field.key"
                class="form-control"
                [placeholder]="field.placeholder"
                [required]="field.required"
                [disabled]="field.disabled"
                [readonly]="field.readonly"
                [rows]="field.rows || 3"
                [minLength]="field.minLength"
                [maxLength]="field.maxLength"
                [formControl]="formControls[field.key]"
                [ngClass]="getValidationClasses(field)"
                (change)="onFieldValueChange(field.key, $event.target.value)"></textarea>

      <!-- Select -->
      <select *ngIf="field.type === 'select'"
              [id]="field.key"
              class="form-control custom-select"
              [required]="field.required"
              [disabled]="field.disabled"
              [formControl]="formControls[field.key]"
              [ngClass]="getValidationClasses(field)"
              (change)="onFieldValueChange(field.key, $event.target.value)">
        <option value="">{{ field.placeholder || ('common.select' | translate) }}</option>
        <option *ngFor="let option of field.options"
                [value]="option.value"
                [disabled]="option.disabled">
          {{ option.label }}
        </option>
      </select>

      <!-- Multi Select -->
      <select *ngIf="field.type === 'multiselect'"
              [id]="field.key"
              class="form-control custom-select"
              [multiple]="true"
              [disabled]="field.disabled"
              [formControl]="formControls[field.key]"
              [ngClass]="getValidationClasses(field)"
              (change)="onFieldValueChange(field.key, $event)">
        <option *ngFor="let option of field.options"
                [value]="option.value"
                [disabled]="option.disabled">
          {{ option.label }}
        </option>
      </select>

      <!-- Checkbox -->
      <div *ngIf="field.type === 'checkbox'" class="form-check">
        <input [id]="field.key"
               type="checkbox"
               class="form-check-input"
               [disabled]="field.disabled"
               [formControl]="formControls[field.key]"
               (change)="onFieldValueChange(field.key, $event.target.checked)">
        <label [for]="field.key" class="form-check-label">
          {{ field.label }}
        </label>
      </div>

      <!-- Radio Group -->
      <div *ngIf="field.type === 'radio'" class="radio-group">
        <div *ngFor="let option of field.options" class="form-check">
          <input [id]="`${field.key}_${option.value}`"
                 type="radio"
                 [name]="field.key"
                 class="form-check-input"
                 [value]="option.value"
                 [disabled]="field.disabled || option.disabled"
                 [formControl]="formControls[field.key]"
                 (change)="onFieldValueChange(field.key, $event.target.value)">
          <label [for]="`${field.key}_${option.value}`" class="form-check-label">
            {{ option.label }}
          </label>
        </div>
      </div>

      <!-- File Input -->
      <input *ngIf="field.type === 'file'"
             [id]="field.key"
             type="file"
             class="form-control-file"
             [accept]="field.accept"
             [multiple]="field.multiple"
             [disabled]="field.disabled"
             (change)="onFieldValueChange(field.key, $event.target.files)">

      <!-- Suffix -->
      <div *ngIf="field.suffix" class="input-group-append">
        <span class="input-group-text">{{ field.suffix }}</span>
      </div>

      <!-- Validation Error -->
      <div *ngIf="formControls[field.key]?.invalid && formControls[field.key]?.touched"
           class="invalid-feedback d-block">
        {{ getErrorMessage(field) }}
      </div>
    </div>

    <!-- Help Text -->
    <small *ngIf="field.helpText && isFieldVisible(field)"
           class="form-text text-muted"
           [ngClass]="layout === 'horizontal' ? 'offset-sm-' + (12 - getColSize(field)) : ''">
      {{ field.helpText }}
    </small>
  </div>
</div>
```

---

### 2.3 Data List Component (Listing with Actions)

#### **data-list.component.ts**
```typescript
import { Component, Input, Output, EventEmitter, TemplateRef, ContentChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

export interface DataColumn {
  key: string;
  title: string;
  type?: 'text' | 'number' | 'date' | 'datetime' | 'boolean' | 'image' | 'actions' | 'custom';
  sortable?: boolean;
  filterable?: boolean;
  sortableDefault?: 'asc' | 'desc';
  width?: string;
  cssClass?: string;
  format?: string; // For date/number formatting
  template?: TemplateRef<any>;
}

export interface ActionItem {
  label: string;
  icon: string;
  action: string;
  cssClass?: string;
  disabled?: boolean;
  show?: (item: any) => boolean;
}

export interface ListAction {
  key: string;
  label: string;
  icon?: string;
  cssClass?: string;
  disabled?: boolean;
  show?: (item: any) => boolean;
}

@Component({
  selector: 'app-data-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './data-list.component.html',
  styleUrls: ['./data-list.component.scss']
})
export class DataListComponent {
  @Input() data: any[] = [];
  @Input() columns: DataColumn[] = [];
  @Input() actions: ActionItem[] = [];
  @Input() rowActions: ListAction[] = [];
  @Input() loading: boolean = false;
  @Input() emptyMessage: string = 'common.noData';
  @Input() selectable: boolean = false;
  @Input() multiSelect: boolean = false;
  @Input() showPagination: boolean = true;
  @Input() showSearch: boolean = true;
  @Input() showFilter: boolean = true;
  @Input() pageSize: number = 10;
  @Input() pageSizeOptions: number[] = [10, 25, 50, 100];
  @Input() serverSide: boolean = false;
  @Input() totalRecords: number = 0;

  @Output() onRowClick = new EventEmitter<any>();
  @Output() onRowDoubleClick = new EventEmitter<any>();
  @Output() onSelectionChange = new EventEmitter<any[]>();
  @Output() onPageChange = new EventEmitter<{ page: number; pageSize: number }>();
  @Output() onSort = new EventEmitter<{ column: string; direction: 'asc' | 'desc' }>();
  @Output() onFilter = new EventEmitter<{ [key: string]: any }>();
  @Output() onSearch = new EventEmitter<string>();
  @Output() onAction = new EventEmitter<{ item: any; action: string }>();
  @Output() onRefresh = new EventEmitter<void>();

  @ContentChild('actionsTemplate') actionsTemplate!: TemplateRef<any>;
  @ContentChild('rowTemplate') rowTemplate!: TemplateRef<any>;

  searchTerm: string = '';
  filters: { [key: string]: any } = {};
  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';
  currentPage: number = 1;
  currentPageSize: number = 10;
  selectedItems: Set<any> = new Set();
  expandedRows: Set<any> = new Set();
  visibleColumnFilters: { [key: string]: boolean } = {};

  ngOnInit(): void {
    // Initialize visible column filters
    this.columns.forEach(col => {
      if (col.filterable) {
        this.visibleColumnFilters[col.key] = false;
      }
    });

    // Set default sort
    const defaultSortCol = this.columns.find(c => c.sortableDefault);
    if (defaultSortCol) {
      this.sortColumn = defaultSortCol.key;
      this.sortDirection = defaultSortCol.sortableDefault || 'asc';
    }
  }

  get filteredAndSortedData(): any[] {
    if (this.serverSide) {
      return this.data;
    }

    let result = [...this.data];

    // Apply search
    if (this.searchTerm) {
      result = this.filterBySearchTerm(result);
    }

    // Apply filters
    if (Object.keys(this.filters).length > 0) {
      result = this.filterByFilters(result);
    }

    // Apply sorting
    if (this.sortColumn) {
      result = this.sortData(result);
    }

    // Apply pagination
    if (this.showPagination) {
      const start = (this.currentPage - 1) * this.currentPageSize;
      const end = start + this.currentPageSize;
      result = result.slice(start, end);
    }

    return result;
  }

  private filterBySearchTerm(data: any[]): any[] {
    const term = this.searchTerm.toLowerCase();
    return data.filter(item =>
      this.columns.some(col => {
        const value = this.getNestedValue(item, col.key);
        return value !== null &&
               value !== undefined &&
               value.toString().toLowerCase().includes(term);
      })
    );
  }

  private filterByFilters(data: any[]): any[] {
    return data.filter(item => {
      return Object.keys(this.filters).every(key => {
        const filterValue = this.filters[key];
        const itemValue = this.getNestedValue(item, key);
        return itemValue === filterValue;
      });
    });
  }

  private sortData(data: any[]): any[] {
    return data.sort((a, b) => {
      const aVal = this.getNestedValue(a, this.sortColumn);
      const bVal = this.getNestedValue(b, this.sortColumn);

      let comparison = 0;
      if (aVal > bVal) comparison = 1;
      if (aVal < bVal) comparison = -1;

      return this.sortDirection === 'desc' ? comparison * -1 : comparison;
    });
  }

  private getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((current, key) => current?.[key], obj);
  }

  get totalPages(): number {
    if (this.serverSide) {
      return Math.ceil(this.totalRecords / this.currentPageSize);
    }
    return Math.ceil(this.data.length / this.currentPageSize);
  }

  get displayedRecords(): { start: number; end: number; total: number } {
    const total = this.serverSide ? this.totalRecords : this.filteredAndSortedData.length;
    const start = (this.currentPage - 1) * this.currentPageSize + 1;
    const end = Math.min(start + this.currentPageSize - 1, total);
    return { start, end, total };
  }

  onRowClicked(item: any): void {
    this.onRowClick.emit(item);
  }

  onRowDoubleClicked(item: any): void {
    this.onRowDoubleClick.emit(item);
  }

  onRowSelected(item: any, event: Event): void {
    event.stopPropagation();

    if (this.multiSelect) {
      if (this.selectedItems.has(item)) {
        this.selectedItems.delete(item);
      } else {
        this.selectedItems.add(item);
      }
    } else {
      this.selectedItems.clear();
      this.selectedItems.add(item);
    }

    this.onSelectionChange.emit(Array.from(this.selectedItems));
  }

  isRowSelected(item: any): boolean {
    return this.selectedItems.has(item);
  }

  isAllSelected(): boolean {
    return this.filteredAndSortedData.length > 0 &&
           this.filteredAndSortedData.every(item => this.selectedItems.has(item));
  }

  onSelectAll(event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;

    if (checked) {
      this.filteredAndSortedData.forEach(item => this.selectedItems.add(item));
    } else {
      this.filteredAndSortedData.forEach(item => this.selectedItems.delete(item));
    }

    this.onSelectionChange.emit(Array.from(this.selectedItems));
  }

  onSortClicked(column: DataColumn): void {
    if (!column.sortable) return;

    if (this.sortColumn === column.key) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column.key;
      this.sortDirection = column.sortableDefault || 'asc';
    }

    this.onSort.emit({ column: this.sortColumn, direction: this.sortDirection });
    if (!this.serverSide) {
      this.currentPage = 1;
    }
  }

  onPageChanged(page: number): void {
    this.currentPage = page;
    this.onPageChange.emit({ page, pageSize: this.currentPageSize });
  }

  onPageSizeChanged(size: number): void {
    this.currentPageSize = size;
    this.currentPage = 1;
    this.onPageChange.emit({ page: this.currentPage, pageSize: size });
  }

  onSearchChanged(): void {
    this.currentPage = 1;
    this.onSearch.emit(this.searchTerm);
  }

  onFilterChanged(key: string, value: any): void {
    if (value === '' || value === null || value === undefined) {
      delete this.filters[key];
    } else {
      this.filters[key] = value;
    }
    this.currentPage = 1;
    this.onFilter.emit(this.filters);
  }

  toggleColumnFilter(key: string): void {
    this.visibleColumnFilters[key] = !this.visibleColumnFilters[key];
  }

  onActionClicked(item: any, action: string): void {
    this.onAction.emit({ item, action });
  }

  isActionVisible(action: ListAction, item: any): boolean {
    return action.show ? action.show(item) : true;
  }

  isActionDisabled(action: ListAction, item: any): boolean {
    return action.disabled || false;
  }

  getCellValue(item: any, column: DataColumn): any {
    return this.getNestedValue(item, column.key);
  }

  formatCellValue(value: any, column: DataColumn): string {
    if (value === null || value === undefined) return '-';

    switch (column.type) {
      case 'date':
        return this.formatDate(value, column.format || 'mediumDate');
      case 'datetime':
        return this.formatDate(value, column.format || 'medium');
      case 'number':
        return this.formatNumber(value, column.format || '1.2-2');
      case 'boolean':
        return value ? ('common.yes' | translate) : ('common.no' | translate);
      default:
        return value.toString();
    }
  }

  private formatDate(value: any, format: string): string {
    // Implement date formatting based on locale
    return new Date(value).toLocaleDateString();
  }

  private formatNumber(value: any, format: string): string {
    // Implement number formatting
    return new Intl.NumberFormat().format(value);
  }

  toggleRowExpansion(item: any): void {
    if (this.expandedRows.has(item)) {
      this.expandedRows.delete(item);
    } else {
      this.expandedRows.add(item);
    }
  }

  isRowExpanded(item: any): boolean {
    return this.expandedRows.has(item);
  }

  exportToExcel(): void {
    // Implement Excel export
  }

  exportToPDF(): void {
    // Implement PDF export
  }

  print(): void {
    window.print();
  }

  refresh(): void {
    this.onRefresh.emit();
  }
}
```

#### **data-list.component.html**
```html
<div class="data-list">

  <!-- Toolbar -->
  <div class="data-list-toolbar" *ngIf="showSearch || showFilter || actions.length > 0">
    <div class="row align-items-center">
      <!-- Search -->
      <div class="col-md-4" *ngIf="showSearch">
        <div class="input-group">
          <input type="text"
                 class="form-control"
                 [placeholder]="'common.search' | translate"
                 [(ngModel)]="searchTerm"
                 (input)="onSearchChanged()">
          <div class="input-group-append">
            <span class="input-group-text"><i class="fe fe-search"></i></span>
          </div>
        </div>
      </div>

      <!-- Actions -->
      <div class="col-md-8 text-md-left" [ngClass]="actions.length > 0 ? 'text-right' : ''">
        <div class="btn-group" *ngIf="actionsTemplate">
          <ng-container *ngTemplateOutlet="actionsTemplate"></ng-container>
        </div>
        <div class="btn-group ml-2" *ngFor="let action of actions">
          <button class="btn btn-sm"
                  [ngClass]="action.cssClass || 'btn-outline-primary'"
                  [title]="action.label"
                  (click)="onActionClicked(null, action.action)">
            <i [class]="action.icon" *ngIf="action.icon"></i>
            <span *ngIf="!action.icon">{{ action.label }}</span>
          </button>
        </div>
        <button class="btn btn-sm btn-outline-secondary ml-2"
                [title]="'common.refresh' | translate"
                (click)="refresh()">
          <i class="fe fe-refresh-cw"></i>
        </button>
      </div>
    </div>
  </div>

  <!-- Loading State -->
  <div class="data-list-loading" *ngIf="loading">
    <div class="text-center py-5">
      <div class="spinner-border text-primary" role="status">
        <span class="sr-only">{{ 'common.loading' | translate }}</span>
      </div>
    </div>
  </div>

  <!-- Data Table -->
  <div class="data-list-table" *ngIf="!loading">
    <div class="table-responsive">
      <table class="table table-hover table-striped">
        <thead>
          <tr>
            <!-- Select All Checkbox -->
            <th *ngIf="selectable" class="select-column" style="width: 40px;">
              <input type="checkbox"
                     class="form-check-input"
                     [checked]="isAllSelected()"
                     (change)="onSelectAll($event)">
            </th>

            <!-- Column Headers -->
            <th *ngFor="let column of columns"
                [style.width]="column.width"
                [ngClass]="column.cssClass"
                [class.sortable]="column.sortable"
                (click)="onSortClicked(column)">
              <div class="d-flex align-items-center">
                <span>{{ column.title }}</span>
                <i *ngIf="column.sortable"
                   class="fe sort-icon ml-1"
                   [ngClass]="{
                     'fe-chevron-up': sortColumn === column.key && sortDirection === 'asc',
                     'fe-chevron-down': sortColumn === column.key && sortDirection === 'desc'
                   }">
                </i>
                <i *ngIf="column.filterable"
                   class="fe fe-filter filter-toggle ml-2"
                   [class.active]="filters[column.key]"
                   (click)="toggleColumnFilter(column.key); $event.stopPropagation()">
                </i>
              </div>

              <!-- Column Filter Dropdown -->
              <div *ngIf="visibleColumnFilters[column.key]"
                   class="column-filter-dropdown"
                   (click)="$event.stopPropagation()">
                <input type="text"
                       class="form-control form-control-sm"
                       [placeholder]="'common.filter' | translate"
                       [value]="filters[column.key] || ''"
                       (input)="onFilterChanged(column.key, $event.target.value)">
              </div>
            </th>

            <!-- Row Actions Column -->
            <th *ngIf="rowActions.length > 0" class="actions-column" style="width: 100px;">
              {{ 'common.actions' | translate }}
            </th>
          </tr>
        </thead>

        <tbody>
          <tr *ngFor="let item of filteredAndSortedData"
              [class.selected]="isRowSelected(item)"
              [class.expanded]="isRowExpanded(item)"
              (click)="onRowClicked(item)"
              (dblclick)="onRowDoubleClicked(item)">

            <!-- Select Checkbox -->
            <td *ngIf="selectable">
              <input type="checkbox"
                     class="form-check-input"
                     [checked]="isRowSelected(item)"
                     (click)="onRowSelected(item, $event)">
            </td>

            <!-- Data Cells -->
            <td *ngFor="let column of columns"
                [ngClass]="column.cssClass">

              <!-- Custom Template -->
              <ng-container *ngIf="column.template">
                <ng-container *ngTemplateOutlet="column.template; context: { $implicit: item, column: column }"></ng-container>
              </ng-container>

              <!-- Standard Cell -->
              <ng-container *ngIf="!column.template">
                <span [class]="'cell-value cell-value-' + column.type">
                  {{ formatCellValue(getCellValue(item, column), column) }}
                </span>
              </ng-container>
            </td>

            <!-- Row Actions -->
            <td *ngIf="rowActions.length > 0" class="actions-column">
              <div class="dropdown">
                <button class="btn btn-sm btn-icon btn-light"
                        data-toggle="dropdown"
                        (click)="$event.stopPropagation()">
                  <i class="fe fe-more-vertical"></i>
                </button>
                <div class="dropdown-menu dropdown-menu-right">
                  <a *ngFor="let action of rowActions"
                     href="javascript:void(0)"
                     class="dropdown-item"
                     [class.disabled]="isActionDisabled(action, item)"
                     [style.display]="isActionVisible(action, item) ? 'block' : 'none'"
                     (click)="$event.stopPropagation(); onActionClicked(item, action.key)">
                    <i [class]="action.icon" *ngIf="action.icon"></i>
                    {{ action.label }}
                  </a>
                </div>
              </div>
            </td>
          </tr>

          <!-- Empty State -->
          <tr *ngIf="filteredAndSortedData.length === 0">
            <td [colSpan]="columns.length + (selectable ? 1 : 0) + (rowActions.length > 0 ? 1 : 0)" class="text-center py-5">
              <div class="empty-state">
                <i class="fe fe-inbox fe-3x text-muted mb-3"></i>
                <p class="text-muted">{{ emptyMessage | translate }}</p>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>

  <!-- Pagination -->
  <div class="data-list-pagination" *ngIf="showPagination && !loading">
    <div class="row align-items-center">
      <div class="col-md-6">
        <div class="pagination-info">
          {{ 'common.showing' | translate }} {{ displayedRecords.start }} {{ 'common.to' | translate }}
          {{ displayedRecords.end }} {{ 'common.of' | translate }} {{ displayedRecords.total }} {{ 'common.records' | translate }}
        </div>
      </div>
      <div class="col-md-6">
        <nav class="d-flex justify-content-md-end">
          <ul class="pagination pagination-sm mb-0">
            <li class="page-item" [class.disabled]="currentPage === 1">
              <a class="page-link" href="javascript:void(0)" (click)="onPageChanged(1)">
                <i class="fe fe-chevrons-left"></i>
              </a>
            </li>
            <li class="page-item" [class.disabled]="currentPage === 1">
              <a class="page-link" href="javascript:void(0)" (click)="onPageChanged(currentPage - 1)">
                <i class="fe fe-chevron-left"></i>
              </a>
            </li>
            <li class="page-item" *ngFor="let page of getPageRange()" [class.active]="page === currentPage">
              <a class="page-link" href="javascript:void(0)" (click)="onPageChanged(page)">{{ page }}</a>
            </li>
            <li class="page-item" [class.disabled]="currentPage === totalPages">
              <a class="page-link" href="javascript:void(0)" (click)="onPageChanged(currentPage + 1)">
                <i class="fe fe-chevron-right"></i>
              </a>
            </li>
            <li class="page-item" [class.disabled]="currentPage === totalPages">
              <a class="page-link" href="javascript:void(0)" (click)="onPageChanged(totalPages)">
                <i class="fe fe-chevrons-right"></i>
              </a>
            </li>
          </ul>
          <select class="form-control form-control-sm page-size-select ml-3"
                  [ngModel]="pageSize"
                  (ngModelChange)="onPageSizeChanged($event)">
            <option *ngFor="let size of pageSizeOptions" [value]="size">{{ size }}</option>
          </select>
        </nav>
      </div>
    </div>
  </div>

</div>
```

#### **data-list.component.scss**
```scss
.data-list {
  .data-list-toolbar {
    margin-bottom: 1rem;
    padding: 1rem;
    background-color: var(--card-bg);
    border-radius: 0.5rem;

    .input-group {
      .form-control {
        border-right: none;
      }

      .input-group-text {
        background-color: transparent;
        border-left: none;
      }
    }
  }

  .data-list-loading {
    min-height: 300px;
  }

  .data-list-table {
    background-color: var(--card-bg);
    border-radius: 0.5rem;
    overflow: hidden;

    .table {
      margin-bottom: 0;

      thead {
        th {
          border-top: none;
          border-bottom-width: 2px;
          font-weight: 600;
          text-transform: uppercase;
          font-size: 0.75rem;
          letter-spacing: 0.5px;
          position: relative;
          cursor: default;

          &.sortable {
            cursor: pointer;
            user-select: none;

            &:hover {
              background-color: rgba(var(--primary-rgb), 0.05);
            }
          }

          .sort-icon {
            opacity: 0.3;
            transition: opacity 0.2s;
          }

          &:hover .sort-icon {
            opacity: 0.7;
          }
        }

        .filter-toggle {
          cursor: pointer;
          opacity: 0.5;
          transition: opacity 0.2s;

          &:hover {
            opacity: 1;
          }

          &.active {
            opacity: 1;
            color: var(--primary);
          }
        }

        .column-filter-dropdown {
          position: absolute;
          top: 100%;
          right: 0;
          z-index: 10;
          background: var(--card-bg);
          border: 1px solid var(--border-color);
          border-radius: 0.375rem;
          padding: 0.5rem;
          box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
          min-width: 200px;
        }
      }

      tbody {
        tr {
          cursor: pointer;
          transition: background-color 0.2s;

          &:hover {
            background-color: rgba(var(--primary-rgb), 0.05);
          }

          &.selected {
            background-color: rgba(var(--primary-rgb), 0.1);
          }

          &.expanded {
            background-color: rgba(var(--primary-rgb), 0.05);
          }
        }

        .cell-value {
          &.cell-value-boolean {
            .badge {
              font-size: 0.75rem;
            }
          }

          &.cell-value-image {
            img {
              width: 40px;
              height: 40px;
              object-fit: cover;
              border-radius: 0.375rem;
            }
          }
        }
      }

      .select-column {
        width: 40px;
        text-align: center;
      }

      .actions-column {
        text-align: center;

        .btn-icon {
          width: 32px;
          height: 32px;
          padding: 0;
          display: inline-flex;
          align-items: center;
          justify-content: center;
          border-radius: 0.375rem;
        }
      }
    }

    .empty-state {
      padding: 3rem 0;
    }
  }

  .data-list-pagination {
    margin-top: 1rem;
    padding: 1rem;
    background-color: var(--card-bg);
    border-radius: 0.5rem;

    .pagination-info {
      font-size: 0.875rem;
      color: var(--text-muted);
    }

    .page-size-select {
      width: 70px;
    }
  }

  // RTL Support
  &[dir="rtl"] {
    .data-list-table {
      .table {
        thead {
          th {
            text-align: right;
          }
        }

        .sort-icon {
          margin-right: 0.25rem;
          margin-left: 0;
        }
      }
    }
  }
}
```

---

## 3. TinyDash Design Integration

### 3.1 Main Layout with TinyDash Structure

```html
<!-- main-layout.component.html -->
<div class="wrapper">
  <!-- Top Navigation -->
  <nav class="topnav navbar navbar-light">
    <button type="button" class="navbar-toggler text-muted mt-2 p-0 mr-3 collapseSidebar">
      <i class="fe fe-menu navbar-toggler-icon"></i>
    </button>

    <form class="form-inline mr-auto searchform text-muted">
      <input class="form-control mr-sm-2 bg-transparent border-0 pl-4 text-muted"
             type="search"
             [placeholder]="'common.search' | translate"
             aria-label="Search">
    </form>

    <ul class="nav">
      <!-- Theme Switcher -->
      <li class="nav-item">
        <a class="nav-link text-muted my-2" href="#" (click)="toggleTheme()">
          <i class="fe fe-sun fe-16"></i>
        </a>
      </li>

      <!-- Quick Actions -->
      <li class="nav-item">
        <a class="nav-link text-muted my-2" href="#" (click)="openShortcutsModal()">
          <span class="fe fe-grid fe-16"></span>
        </a>
      </li>

      <!-- Notifications -->
      <li class="nav-item nav-notif">
        <a class="nav-link text-muted my-2" href="#" (click)="openNotificationsModal()">
          <span class="fe fe-bell fe-16"></span>
          <span class="dot dot-md bg-success" *ngIf="unreadCount > 0">{{ unreadCount }}</span>
        </a>
      </li>

      <!-- User Menu -->
      <li class="nav-item dropdown">
        <a class="nav-link dropdown-toggle text-muted pr-0"
           href="#"
           role="button"
           data-toggle="dropdown">
          <span class="avatar avatar-sm mt-2">
            <img *ngIf="currentUser.avatarUrl"
                 [src]="currentUser.avatarUrl"
                 [alt]="currentUser.fullName"
                 class="avatar-img rounded-circle">
            <span *ngIf="!currentUser.avatarUrl"
                  class="avatar-title rounded-circle bg-primary">
              {{ currentUser.fullName | initials }}
            </span>
          </span>
        </a>
        <div class="dropdown-menu dropdown-menu-right">
          <a class="dropdown-item" href="#" routerLink="/profile">
            <i class="fe fe-user mr-2"></i>
            {{ 'settings.profileSettings' | translate }}
          </a>
          <a class="dropdown-item" href="#" routerLink="/settings">
            <i class="fe fe-settings mr-2"></i>
            {{ 'settings.generalSettings' | translate }}
          </a>
          <div class="dropdown-divider"></div>
          <a class="dropdown-item" href="#" (click)="logout()">
            <i class="fe fe-log-out mr-2"></i>
            {{ 'auth.logout' | translate }}
          </a>
        </div>
      </li>
    </ul>
  </nav>

  <!-- Sidebar -->
  <aside class="sidebar-left border-right bg-white shadow" id="leftSidebar">
    <a href="#" class="btn collapseSidebar toggle-btn d-lg-none text-muted ml-2 mt-3">
      <i class="fe fe-x"></i>
    </a>

    <nav class="vertnav navbar navbar-light">
      <!-- Logo -->
      <div class="w-100 mb-4 d-flex">
        <a class="navbar-brand mx-auto mt-2 flex-fill text-center" routerLink="/">
          <svg version="1.1" class="navbar-brand-img brand-sm" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 120 120">
            <g>
              <polygon points="78,105 15,105 24,87 87,87" />
              <polygon points="96,69 33,69 42,51 105,51" />
              <polygon points="78,33 15,33 24,15 87,15" />
            </g>
          </svg>
        </a>
      </div>

      <!-- Navigation Menu -->
      <ul class="navbar-nav flex-fill w-100 mb-2">
        <!-- Dashboard -->
        <li class="nav-item">
          <a class="nav-link" routerLink="/dashboard" routerLinkActive="active">
            <i class="fe fe-home fe-16"></i>
            <span class="ml-3 item-text">{{ 'dashboard.title' | translate }}</span>
          </a>
        </li>

        <!-- User Management -->
        <li class="nav-item dropdown" *ngIf="hasPermission('users.view')">
          <a href="#userManagement" data-toggle="collapse" [attr.aria-expanded]="isMenuExpanded('userManagement')"
             class="dropdown-toggle nav-link">
            <i class="fe fe-users fe-16"></i>
            <span class="ml-3 item-text">{{ 'userManagement.title' | translate }}</span>
          </a>
          <ul class="collapse list-unstyled pl-4 w-100" id="userManagement">
            <li class="nav-item">
              <a class="nav-link pl-3" routerLink="/users" routerLinkActive="active">
                <span class="ml-1 item-text">{{ 'userManagement.users' | translate }}</span>
              </a>
            </li>
            <li class="nav-item">
              <a class="nav-link pl-3" routerLink="/roles" routerLinkActive="active">
                <span class="ml-1 item-text">{{ 'userManagement.roles' | translate }}</span>
              </a>
            </li>
          </ul>
        </li>

        <!-- Add other menu items similarly -->
      </ul>
    </nav>
  </aside>

  <!-- Main Content -->
  <main class="main-content" role="main">
    <div class="container-fluid">
      <router-outlet></router-outlet>
    </div>
  </main>

  <!-- Footer -->
  <footer class="footer">
    <div class="container-fluid">
      <div class="row">
        <div class="col-12 col-md-6">
          <p class="mb-0">© {{ currentYear }} IIROSA. All rights reserved.</p>
        </div>
        <div class="col-12 col-md-6 text-md-right">
          <p class="mb-0">Version {{ appVersion }}</p>
        </div>
      </div>
    </div>
  </footer>
</div>
```

### 3.2 CSS Integration (styles.scss)

```scss
// Import TinyDash CSS
@import 'assets/css/app-dark';
@import 'assets/css/app-rtl';

// Bootstrap RTL Support
[dir="rtl"] {
  @import 'bootstrap/dist/css/bootstrap.rtl.min';
}

// Custom Overrides
:root {
  --primary: #4d7cfe;
  --primary-rgb: 77, 124, 254;
  --success: #3cc09a;
  --info: #38adf9;
  --warning: #f5b921;
  --danger: #e55b6b;

  --card-bg: #1c1e21;
  --body-bg: #121417;
  --text-color: #e1e1e1;
  --text-muted: #6c7293;
  --border-color: #2c2f36;
}

body {
  background-color: var(--body-bg);
  color: var(--text-color);
  font-family: 'Overpass', sans-serif;

  &.vertical.dark.rtl {
    direction: rtl;
  }
}

// Scrollbar Styling
::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}

::-webkit-scrollbar-track {
  background: var(--body-bg);
}

::-webkit-scrollbar-thumb {
  background: var(--border-color);
  border-radius: 4px;

  &:hover {
    background: var(--text-muted);
  }
}

// Custom Components
.required-field::after {
  content: ' *';
  color: var(--danger);
}

.btn {
  border-radius: 0.375rem;
  font-weight: 500;
  transition: all 0.2s ease;

  .fe {
    vertical-align: middle;
  }
}

.card {
  background-color: var(--card-bg);
  border-color: var(--border-color);

  .card-header {
    border-bottom-color: var(--border-color);
    background-color: transparent;
  }

  .card-footer {
    border-top-color: var(--border-color);
    background-color: transparent;
  }
}

.form-control {
  background-color: var(--body-bg);
  border-color: var(--border-color);
  color: var(--text-color);

  &:focus {
    background-color: var(--body-bg);
    border-color: var(--primary);
    color: var(--text-color);
    box-shadow: 0 0 0 2px rgba(var(--primary-rgb), 0.25);
  }

  &::placeholder {
    color: var(--text-muted);
  }
}

.table {
  --table-bg: var(--card-bg);
  --table-border-color: var(--border-color);
  --table-color: var(--text-color);

  thead th {
    border-bottom-color: var(--border-color);
  }

  tbody tr {
    border-bottom-color: var(--border-color);
  }
}

// Loading Spinner
.spinner-border {
  border-color: var(--primary);
  border-right-color: transparent;
}

// Toast Notifications
.toast-container {
  position: fixed;
  top: 1rem;
  right: 1rem;
  z-index: 9999;

  [dir="rtl"] & {
    right: auto;
    left: 1rem;
  }
}

// RTL Specific
[dir="rtl"] {
  .ml-auto {
    margin-left: 0 !important;
    margin-right: auto !important;
  }

  .mr-auto {
    margin-right: 0 !important;
    margin-left: auto !important;
  }

  .text-right {
    text-align: left !important;
  }

  .text-left {
    text-align: right !important;
  }
}
```

---

## 4. Integration Examples

### 4.1 Using the Shared Components

```typescript
// employees/employee-list/employee-list.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [CommonModule, RouterModule, DataListComponent, PageHeaderComponent],
  templateUrl: './employee-list.component.html'
})
export class EmployeeListComponent implements OnInit {
  columns: DataColumn[] = [
    { key: 'code', title: 'employees.employeeCode', sortable: true, filterable: true },
    { key: 'fullName', title: 'employees.fullName', sortable: true, filterable: true },
    { key: 'nationalId', title: 'employees.nationalId', sortable: true },
    { key: 'phoneNumber', title: 'employees.phoneNumber', filterable: true },
    { key: 'department', title: 'employees.department', filterable: true },
    { key: 'isActive', title: 'employees.isActive', type: 'boolean', sortable: true }
  ];

  rowActions: ListAction[] = [
    { key: 'view', label: 'common.view', icon: 'fe fe-eye', cssClass: 'btn-info' },
    { key: 'edit', label: 'common.edit', icon: 'fe fe-edit', cssClass: 'btn-primary' },
    { key: 'delete', label: 'common.delete', icon: 'fe fe-trash', cssClass: 'btn-danger' }
  ];

  actions: ActionItem[] = [
    { label: 'employees.addEmployee', icon: 'fe fe-plus', action: 'add', cssClass: 'btn-primary' },
    { label: 'common.exportToExcel', icon: 'fe fe-file-plus', action: 'export', cssClass: 'btn-success' }
  ];

  employees: any[] = [];
  loading: boolean = false;

  constructor(private employeeService: EmployeeService) {}

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.loading = true;
    this.employeeService.getAll().subscribe({
      next: (data) => {
        this.employees = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  onAction(event: { item: any; action: string }): void {
    switch (event.action) {
      case 'add':
        // Navigate to create page
        break;
      case 'edit':
        // Navigate to edit page
        break;
      case 'delete':
        // Show confirmation dialog
        break;
      case 'export':
        // Export to Excel
        break;
    }
  }
}
```

```html
<!-- employee-list.component.html -->
<app-page-header
  [title]="'employees.title' | translate"
  [subtitle]="'employees.manageEmployees' | translate"
  [icon]="'fe fe-users'">
</app-page-header>

<app-data-list
  [data]="employees"
  [columns]="columns"
  [rowActions]="rowActions"
  [actions]="actions"
  [loading]="loading"
  (onAction)="onAction($event)">
</app-data-list>
```

---

## 5. Implementation Roadmap

### Phase 1: Foundation (Weeks 1-2)
- Set up Angular 18+ project
- Configure TinyDash Dark RTL template
- Implement localization (en, ar, fr)
- Create core services (auth, http interceptors, error handling)
- Set up routing and layout structure

### Phase 2: Shared Components (Weeks 3-4)
- Attachment component
- Input Fields component
- Data List component
- Modal, Toast, Loading components
- Custom validators and pipes

### Phase 3: Core Features (Weeks 5-8)
- Authentication module (login, logout, reset password)
- User & Role Management
- Dashboard with charts and statistics
- Notifications with SignalR

### Phase 4: Business Modules (Weeks 9-12)
- Employees module
- Charities module
- Families & Orphans module
- Sponsors module
- Periodic Reports module

### Phase 5: Advanced Features (Weeks 13-14)
- Excel export functionality
- Advanced filtering and search
- Permission-based UI
- Performance optimization

---

## 6. Best Practices

### 6.1 Performance
- Use `OnPush` change detection strategy
- Implement virtual scrolling for large lists
- Lazy load feature modules
- Use trackBy functions in ngFor
- Optimize images and assets

### 6.2 Security
- Sanitize all user inputs
- Use Angular's built-in XSS protection
- Implement CSRF protection
- Secure HTTP headers
- JWT token management with refresh tokens

### 6.3 Accessibility
- ARIA labels for screen readers
- Keyboard navigation support
- High contrast mode support
- Focus management in modals
- Semantic HTML

### 6.4 Code Quality
- Consistent naming conventions
- Component composition over inheritance
- Single responsibility principle
- DRY (Don't Repeat Yourself)
- Comprehensive unit testing

---

This architecture provides a solid foundation for building a scalable, maintainable Angular frontend that integrates seamlessly with the TinyDash Dark RTL design and your existing .NET backend.
