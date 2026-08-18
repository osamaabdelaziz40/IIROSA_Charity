# 🔧 Debugging User Detail API Failure

## **Problem:**
The code stops at the error handler:
```typescript
error: () => {
  debugger;  // ← STOPS HERE
  this.notification.error('Failed to load user');
  this.loading = false;
}
```

## **We need to see the actual error!** Let me add proper error logging.

---

## 🚀 **Quick Fix: Add Error Logging**

Replace your current code with this **enhanced debugging version**:

```typescript
loadUser() {
  debugger;
  this.loading = true;

  console.log('=== USER DETAIL DEBUG ===');
  console.log('Loading user with ID:', this.userId);
  console.log('Full URL being called:', `/api/usermanagement/${this.userId}`);

  this.userManagementService.getUser(this.userId!).subscribe({
    next: (user: any) => {
      debugger;
      console.log('✅ SUCCESS: User loaded:', user);
      this.user = user;
      this.loading = false;
    },
    error: (error: any) => {
      debugger;
      console.error('❌ ERROR: Failed to load user');
      console.error('Error details:', {
        status: error.status,
        statusText: error.statusText,
        message: error.message,
        error: error.error,
        url: error.url
      });

      // Show specific error message
      let errorMessage = 'Failed to load user';
      if (error.status === 401) {
        errorMessage = 'Unauthorized - Please login again';
      } else if (error.status === 404) {
        errorMessage = 'User not found';
      } else if (error.status === 0) {
        errorMessage = 'Cannot connect to backend - Check if backend is running';
      } else if (error.error?.message) {
        errorMessage = error.error.message;
      }

      this.notification.error(errorMessage);
      this.loading = false;
    }
  });
}
```

---

## 🔍 **What to Check When Debugging**

### **1. Check Browser Console Tab**

When the debugger stops at the error handler, look at your **Console** tab (F12 → Console).

**You should see:**
```
=== USER DETAIL DEBUG ===
Loading user with ID: some-guid-here
Full URL being called: /api/usermanagement/some-guid-here
❌ ERROR: Failed to load user
Error details: { status: 404, statusText: "Not Found", ... }
```

### **2. Check Network Tab**

1. Open **DevTools** (F12)
2. Go to **Network** tab
3. Filter by "usermanagement"
4. Find the failing request: `GET /api/usermanagement/{id}`
5. Click on it and check:
   - **Headers** tab - Is the `Authorization` header present with JWT token?
   - **Response** tab - What's the actual error response from backend?
   - **Status** - What's the HTTP status code? (404, 401, 500, etc.)

### **3. Common Issues & Solutions**

#### **Issue A: Status 0 / "Cannot connect"**
**Problem:** Request never reached the backend
**Solution:**
```bash
# Test if backend is running
curl https://localhost:60960/health

# Should return:
# { "Status": "Healthy", "Timestamp": "...", "Version": "v1.0.0" }
```

#### **Issue B: Status 401 Unauthorized**
**Problem:** JWT token is missing or invalid
**Solution:**
1. Check if you're logged in
2. Check Network tab → Headers → Look for `Authorization: Bearer TOKEN`
3. If missing, login again with seed user:
   - Email: `OsamaSuper@IIROSA.com`
   - Password: `P@ssw0rd@2022`

#### **Issue C: Status 404 Not Found**
**Problem:** User ID doesn't exist OR backend endpoint not found
**Solution:**
1. Check if the user ID is a real GUID from your database
2. Test the endpoint directly:
```bash
# First login to get token
TOKEN=$(curl -s -X POST https://localhost:60960/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"OsamaSuper@IIROSA.com","password":"P@ssw0rd@2022"}' \
  | jq -r '.token')

# Then get user list to see real user IDs
curl -X GET "https://localhost:60960/api/usermanagement?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN" \
  | jq '.items[0].id'
```

#### **Issue D: Status 500 Server Error**
**Problem:** Backend code has an error
**Solution:**
1. Check backend console/logs for error details
2. Look for stack traces
3. Check if JWT settings are loaded correctly

#### **Issue E: CORS Error**
**Problem:** Browser blocking cross-origin request
**Solution:**
1. Make sure backend is running with CORS enabled
2. Check proxy configuration is working
3. Verify backend URL in proxy.conf.json matches backend actual URL

---

## 🧪 **Complete Debugging Workflow**

### **Step 1: Check Backend is Running**
```bash
# In terminal, test health endpoint
curl https://localhost:60960/health
```

**Expected:** JSON response with `"Status": "Healthy"`
**If fails:** Backend is not running - start it with `dotnet run`

### **Step 2: Check Login Works**
```bash
# Test login
curl -X POST https://localhost:60960/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"OsamaSuper@IIROSA.com","password":"P@ssw0rd@2022"}'
```

**Expected:** JSON response with `"token": "..."`
**If fails:** JWT configuration issue or seed data not created

### **Step 3: Check User List Returns Real Users**
```bash
# Use the token from step 2
TOKEN="paste-token-here"

curl -X GET "https://localhost:60960/api/usermanagement?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"
```

**Expected:** JSON with `"items"` array containing 5 seed users
**If fails:** Authorization issue or database issue

### **Step 4: Test Specific User ID**
```bash
# Copy a real user ID from step 3
USER_ID="paste-real-guid-here"

curl -X GET "https://localhost:60960/api/usermanagement/$USER_ID" \
  -H "Authorization: Bearer $TOKEN"
```

**Expected:** JSON with user details
**If fails:** User doesn't exist or endpoint issue

---

## 🐛 **Most Likely Issues**

Based on your setup, here are the most likely causes:

### **1. Not Logged In / No JWT Token**
**Symptoms:** 401 Unauthorized error
**Fix:** Make sure you login first before trying to access user details

### **2. Backend Not Running**
**Symptoms:** Status 0 or connection refused
**Fix:** Start the backend with `dotnet run`

### **3. Fake User ID**
**Symptoms:** 404 Not Found with ID like `a1111111-2222-3333-4444-555555555555`
**Fix:** Use real user IDs from the database (from seed data)

### **4. Proxy Not Working**
**Symptoms:** Requests going to `http://localhost:4200/api/...` instead of backend
**Fix:** Make sure you restarted Angular after adding proxy config to `angular.json`

---

## 💡 **Quick Test: Open Browser Console**

Open your browser's **Console** tab (F12 → Console) and run this:

```javascript
// Check if user is logged in
console.log('JWT Token:', localStorage.getItem('access_token'));

// Check current user ID
console.log('User ID from route:', window.location.pathname.split('/').pop());
```

This will show you:
- If you have a JWT token (should be a long string starting with `eyJ`)
- What user ID is being requested (should be a real GUID format)

---

## 📋 **Debug Checklist**

When the debugger stops at the error handler, check these:

**In Browser Console:**
- [ ] What's the `error.status`? (401, 404, 500, 0?)
- [ ] What's the `error.statusText`?
- [ ] What's the `error.url`? (Should be `/api/usermanagement/{id}`)
- [ ] Is there an `error.error.message` with more details?

**In Network Tab:**
- [ ] Is the request showing in the Network tab?
- [ ] What's the HTTP status code?
- [ ] Is the `Authorization` header present?
- [ ] What's the response body from the server?

**In Backend:**
- [ ] Is the backend running?
- [ ] Are there any error logs in the backend console?
- [ ] Is the endpoint actually being called?

---

## 🎯 **Next Steps**

1. **Add the enhanced error logging** code above to your component
2. **Refresh the page** and try again
3. **When debugger stops**, check the Console tab for the error details
4. **Copy and paste the error details** here so I can see what's actually failing

The error details will tell us exactly what's wrong and we can fix it! 🔧
