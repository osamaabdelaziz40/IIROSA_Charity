# 🔧 Debugging 404 Errors - User Management API

## **Problem:**
```
GET http://localhost:4200/api/usermanagement/a1111111-2222-3333-4444-555555555555 404 (Not Found)
POST http://localhost:4200/api/usermanagement/a1111111-2222-3333-4444-555555555555/deactivate 404 (Not Found)
POST http://localhost:4200/api/usermanagement/a1111111-2222-3333-4444-555555555555/reset-password 404 (Not Found)
```

## **Root Cause:**
The ID `a1111111-2222-3333-4444-555555555555` is a **fake test ID** that doesn't exist in your database.

---

## 🚀 Quick Solution

### **Step 1: Check if Backend is Running**
Open browser and test:
```
http://localhost:60960/health
```

**Expected Response:**
```json
{
  "Status": "Healthy",
  "Timestamp": "2025-04-25T...",
  "Version": "v1.0.0"
}
```

**If this fails:** Your backend is not running. Start it with:
```bash
cd "d:\Osama\IIROSA Claude\Backend\src\IIROSA.Api"
dotnet run
```

---

### **Step 2: Check if Seed Data Has Run**

The backend should have automatically created 5 seed users when it first started. Let's verify:

#### **Test 1: Try to Login with Seed User**
```bash
# Test login endpoint
curl -X POST http://localhost:60960/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "OsamaSuper@IIROSA.com",
    "password": "P@ssw0rd@2022"
  }'
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "OsamaSuper@IIROSA.com",
  "roles": ["Super Admin"]
}
```

**If this fails:** Seed data hasn't run or JWT is not configured.

---

#### **Test 2: Get User List (Should Return Seed Users)**
```bash
# First login to get token, then:
curl -X GET "http://localhost:60960/api/usermanagement?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

**Expected Response:**
```json
{
  "items": [
    {
      "id": "real-guid-here-1",
      "email": "OsamaSuper@IIROSA.com",
      "fullName": "Osama Abdelaziz",
      "roles": ["Super Admin"],
      "isActive": true
    },
    {
      "id": "real-guid-here-2",
      "email": "Admin@IIROSA.com",
      "fullName": "System Admin",
      "roles": ["Admin"],
      "isActive": true
    }
    // ... 3 more users
  ],
  "totalCount": 5,
  "pageNumber": 1,
  "pageSize": 10
}
```

**If this returns empty:** Seed data didn't run. See "Force Seed Data" below.

---

### **Step 3: Check Frontend is Loading Real User IDs**

Open your browser's **Developer Tools** (F12) and check:

#### **Console Tab:**
Look for debug messages like:
```
=== USER LIST API CALL DEBUG ===
Loading users with request: { pageNumber: 1, pageSize: 10, ... }
✅ SUCCESS: Users loaded successfully: { items: [...], totalCount: 5 }
```

#### **Network Tab:**
1. Filter by "usermanagement"
2. Look for `GET /api/usermanagement?pageNumber=1&pageSize=10`
3. Click on it and check the **Response** tab

**What you should see:**
- Real GUIDs in the `id` field (not `a1111111-2222-3333-4444-555555555555`)
- 5 users from seed data
- Valid email addresses and names

---

## 🔍 Common Issues & Solutions

### **Issue 1: Backend Returns Empty User List**

**Diagnosis:**
```bash
# Check if database exists and has users
cd "d:\Osama\IIROSA Claude\Backend\src\IIROSA.Api"
dotnet ef database update
```

**Solution:** Force seed data to run:
```bash
# Delete the database to force re-seeding
# (This will delete all data, so be careful)
cd "d:\Osama\IIROSA Claude\Backend\src\IIROSA.Api"
rm -f "../IIROSA.Infrastructure/Data/IIROSA_Db.db"  # Or use Windows Explorer

# Restart backend - it will recreate database and seed data
dotnet run
```

Or modify `Program.cs` to always run seed data in development:
```csharp
// In Program.cs, change this line:
if (app.Environment.IsDevelopment() || IsFirstRun(app))

// To this (force seed data every time in development):
if (app.Environment.IsDevelopment())  // Remove || IsFirstRun(app)
```

---

### **Issue 2: Frontend Shows Loading Spinner Forever**

**Diagnosis:** JWT authentication is failing

**Solution:** Check the JWT configuration fixes I provided earlier:
1. Verify `appsettings.json` has JWT settings
2. Verify backend started successfully with debug messages
3. Check browser console for 401 Unauthorized errors

---

### **Issue 3: User List Loads but Actions Use Wrong IDs**

**Diagnosis:** Frontend component has hardcoded test IDs

**Solution:** Check these files for hardcoded GUIDs:

**File:** `src/app/modules/user-management/users/user-list.component.ts`
```typescript
// Look for lines like:
viewUser(id: string) {
  // Should use the user.id from the API response
  this.router.navigate(['/user-management/users', id]);
}

// NOT hardcoded like:
viewUser(id: string) {
  id = 'a1111111-2222-3333-4444-555555555555';  // ❌ WRONG!
}
```

**File:** `src/app/modules/user-management/users/user-detail.component.ts`
```typescript
// Should use route parameter:
this.userId = this.route.snapshot.params['id'];  // ✅ CORRECT

// NOT hardcoded like:
this.userId = 'a1111111-2222-3333-4444-555555555555';  // ❌ WRONG!
```

---

## 🧪 Testing with Real User IDs

### **Method 1: Use Browser DevTools**

1. Open your Angular app
2. Go to User Management page
3. Open DevTools (F12) → Network tab
4. Filter by "usermanagement"
5. Find the `GET /api/usermanagement` call
6. Check the Response → Look at `items[0].id`
7. Copy that real GUID
8. Test it:
   ```
   http://localhost:4200/api/usermanagement/[REAL_GUID_HERE]
   ```

### **Method 2: Use curl with Real Token**

```bash
# 1. Login to get token
TOKEN=$(curl -s -X POST http://localhost:60960/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"OsamaSuper@IIROSA.com","password":"P@ssw0rd@2022"}' \
  | jq -r '.token')

# 2. Get user list
curl -X GET "http://localhost:60960/api/usermanagement?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN" \
  | jq '.items[0].id'

# 3. Use that real ID for other calls
REAL_ID="[paste the real ID here]"

curl -X GET "http://localhost:60960/api/usermanagement/$REAL_ID" \
  -H "Authorization: Bearer $TOKEN"
```

---

## 🎯 Complete Testing Flow

### **1. Start Backend**
```bash
cd "d:\Osama\IIROSA Claude\Backend\src\IIROSA.Api"
dotnet run
```

**Look for these success messages:**
```
✅ JWT Settings Loaded:
   Key Length: 40
   Issuer: IIROSAApi
   Audience: IIROSAClient

✅ JWT IdentityTokenManager Initialized:
   Key Length: 40

✅ Dynamic DI Registration completed
✅ Loaded AutoMapper profiles
✅ Seed data initialization started
✅ User 'OsamaSuper@IIROSA.com' created and assigned to role 'Super Admin'
✅ Seed data verification successful
```

### **2. Start Frontend**
```bash
cd "d:\Osama\IIROSA Claude\Frontend"
ng serve
```

### **3. Login with Seed User**
```
Email: OsamaSuper@IIROSA.com
Password: P@ssw0rd@2022
```

### **4. Navigate to User Management**
```
Should see 5 users:
1. Osama Abdelaziz (Super Admin)
2. System Admin (Admin)
3. Charity User (Charity)
4. Accountant User (Accountant)
5. Financial Officer (FinancialOfficer)
```

### **5. Test Actions**
- Click on any user → Should load user details
- Click Edit → Should load edit form
- Click Deactivate → Should deactivate user
- Click Reset Password → Should reset password

---

## 🐛 Debug Checklist

- [ ] Backend is running (test `/health`)
- [ ] JWT settings are loaded (check backend console)
- [ ] Seed data has run (should see 5 users)
- [ ] Can login with seed user (get JWT token)
- [ ] User list loads in frontend
- [ ] User IDs are real GUIDs (not `a1111111-2222-3333-4444-555555555555`)
- [ ] Actions use real user IDs from the list
- [ ] No 401 Unauthorized errors
- [ ] No 404 Not Found errors (except for invalid IDs)

---

## 💡 Quick Fix for Common Scenario

**If you're seeing the fake ID `a1111111-2222-3333-4444-555555555555`:**

This is likely because:
1. User list is empty (no seed data)
2. Frontend has a fallback/test ID being used
3. Frontend routing is using a default test ID

**Immediate fix:**
```bash
# Force seed data to run
cd "d:\Osama\IIROSA Claude\Backend\src\IIROSA.Api"
# Delete database file if it exists
rm -f IIROSA.db
# Restart backend
dotnet run
```

Then refresh your frontend and login again.

---

## 📞 Next Steps

1. **Test backend health endpoint** - Verify backend is running
2. **Test login** - Verify JWT and seed data are working
3. **Test user list** - Verify you see real users with real GUIDs
4. **Test user actions** - Verify all buttons work with real IDs
5. **Check browser console** - Look for any JavaScript errors

**If you're still seeing the fake ID**, please share:
- Screenshot of the Network tab showing the `/api/usermanagement` response
- Screenshot of the browser Console tab
- Copy of the backend startup logs

This will help me identify exactly where the issue is! 🎯
