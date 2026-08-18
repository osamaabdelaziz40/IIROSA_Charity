# API Connection Guide

## Current Setup

### Frontend Configuration
- **Development URL**: `http://localhost:4200`
- **API Proxy**: `/api/*` → `https://localhost:60960`
- **API Base URL**: `/api` (via proxy during development)

### Backend Configuration
- **Backend URL**: `https://localhost:60960`
- **Auth Endpoint**: `https://localhost:60960/api/auth/login`
- **Users Endpoint**: `https://localhost:60960/api/usermanagement`

## Testing the Connection

### 1. Test Backend Access
Open your browser and try to access:
```
https://localhost:60960/api/auth/login
```

If you see SSL certificate warnings, you may need to:
- Accept the self-signed certificate
- Or run the backend with HTTP instead of HTTPS for development

### 2. Test Login Endpoint
Use Postman or curl to test:
```bash
curl -X POST https://localhost:60960/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"password"}'
```

Expected response:
```json
{
  "token": "jwt-token-here",
  "refreshToken": "refresh-token",
  "expiration": "2024-12-31T23:59:59Z",
  "user": {
    "id": "user-id",
    "username": "admin",
    "email": "admin@example.com",
    "fullName": "Admin User",
    "roles": ["Admin"]
  }
}
```

### 3. Test Users Endpoint
After getting a token:
```bash
curl -X GET "https://localhost:60960/api/usermanagement?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Common Issues & Solutions

### 1. SSL Certificate Error
**Problem**: `ERR_SSL_PROTOCOL_ERROR` or certificate warnings

**Solution A**: Accept the certificate
- Open `https://localhost:60960` in browser
- Accept the security warning
- Add exception for the certificate

**Solution B**: Use HTTP for development
- Change backend to run on HTTP (port 60961)
- Update proxy config to target HTTP

### 2. CORS Issues
**Problem**: `CORS policy: No 'Access-Control-Allow-Origin' header`

**Solution**: The proxy configuration should handle this, but you can also:
- Add CORS middleware to your backend
- Use a browser extension to bypass CORS (for testing only)

### 3. 401 Unauthorized
**Problem**: Getting 401 errors even after login

**Solution**: Check that:
- Token is being stored in localStorage: `localStorage.getItem('accessToken')`
- Token is being sent in Authorization header
- Token format matches backend expectations (Bearer scheme)
- Token hasn't expired

### 4. Connection Refused
**Problem**: `ECONNREFUSED` when trying to connect

**Solution**: 
- Verify backend is running on port 60960
- Check firewall settings
- Ensure backend accepts HTTPS connections

## Debug Steps

### 1. Check Network Requests
1. Open Browser DevTools (F12)
2. Go to Network tab
3. Try to login
4. Check the request:
   - URL being called
   - Headers (especially Authorization)
   - Request body
   - Response status and body

### 2. Check Console Logs
Look for error messages in browser console that might indicate:
- SSL certificate issues
- CORS errors
- Network failures
- Authentication failures

### 3. Verify Token Storage
In browser console:
```javascript
// Check if token exists
localStorage.getItem('accessToken')
localStorage.getItem('currentUser')

// Check authentication status
// This should return true if logged in
```

## Backend Requirements

Your backend should support:

### Authentication Endpoint
```
POST /api/auth/login
Content-Type: application/json

Request:
{
  "email": "user@example.com",
  "password": "password",
  "rememberMe": false
}

Response:
{
  "token": "jwt-token",
  "refreshToken": "refresh-token", 
  "expiration": "ISO-date-string",
  "user": {
    "id": "user-id",
    "username": "username",
    "email": "user@example.com",
    "fullName": "Full Name",
    "roles": ["role1", "role2"]
  }
}
```

### Users Endpoint
```
GET /api/usermanagement?pageNumber=1&pageSize=10
Authorization: Bearer jwt-token

Response:
{
  "items": [...],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 10,
  "hasPrevious": false,
  "hasNext": true
}
```

## Quick Test
1. Navigate to `http://localhost:4200`
2. You should be redirected to login page
3. Enter valid credentials
4. Check browser Network tab to see API calls
5. Verify you can access the users list after login