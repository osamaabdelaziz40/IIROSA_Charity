# Backend API Implementation Guide

## **Expected API Request from Frontend:**

### **Request Details:**
```
GET /api/usermanagement?pageNumber=1&pageSize=10
Host: localhost:4200 (proxied to localhost:60960)
Authorization: Bearer {jwt_token}
```

### **Expected Response Format:**
```json
{
  "items": [
    {
      "id": "1",
      "userName": "admin",
      "fullName": "Admin User",
      "email": "admin@iirosa.org",
      "phoneNumber": "+966501234567",
      "isActive": true,
      "createdOn": "2024-01-15T10:30:00",
      "updatedOn": "2024-04-20T14:22:00",
      "roles": ["Super Admin", "Admin"]
    }
  ],
  "totalCount": 5,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1,
  "hasPrevious": false,
  "hasNext": false
}
```

## **ASP.NET Core Backend Implementation:**

### **Controller (C#):**
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsermanagementController : ControllerBase
{
    private readonly IUserService _userService;

    public UsermanagementController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<UserListResponse>> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchText = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string[]? roles = null)
    {
        try
        {
            var request = new UserSearchRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchText = searchText,
                IsActive = isActive,
                Roles = roles?.ToList()
            };

            var response = await _userService.GetUsersAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

// DTOs
public class UserListResponse
{
    public List<UserDto> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}

public class UserDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public List<string> Roles { get; set; }
}
```

### **Program.cs Configuration:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Enable CORS
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run("https://localhost:60960");
```

### **appsettings.json:**
```json
{
  "Urls": "https://localhost:60960",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": ["*"]
}
```

## **Node.js/Express Backend Implementation:**

### **Controller (JavaScript):**
```javascript
const express = require('express');
const cors = require('cors');

const app = express();
const PORT = 60960;

// Middleware
app.use(cors());
app.use(express.json());

// Mock user data
const users = [
  {
    id: "1",
    userName: "admin",
    fullName: "Admin User",
    email: "admin@iirosa.org",
    phoneNumber: "+966501234567",
    isActive: true,
    createdOn: new Date('2024-01-15'),
    updatedOn: new Date('2024-04-20'),
    roles: ["Super Admin", "Admin"]
  }
];

// User management endpoint
app.get('/api/usermanagement', (req, res) => {
  const { pageNumber = 1, pageSize = 10, searchText, isActive, roles } = req.query;

  console.log('GET /api/usermanagement called with params:', req.query);

  // Filter users
  let filteredUsers = [...users];

  if (searchText) {
    filteredUsers = filteredUsers.filter(user =>
      user.fullName.toLowerCase().includes(searchText.toLowerCase()) ||
      user.email.toLowerCase().includes(searchText.toLowerCase())
    );
  }

  if (isActive !== undefined) {
    filteredUsers = filteredUsers.filter(user =>
      user.isActive === (isActive === 'true')
    );
  }

  if (roles) {
    const roleArray = Array.isArray(roles) ? roles : [roles];
    filteredUsers = filteredUsers.filter(user =>
      user.roles.some(role => roleArray.includes(role))
    );
  }

  // Pagination
  const startIndex = (pageNumber - 1) * pageSize;
  const endIndex = startIndex + parseInt(pageSize);
  const paginatedUsers = filteredUsers.slice(startIndex, endIndex);

  const response = {
    items: paginatedUsers,
    totalCount: filteredUsers.length,
    pageNumber: parseInt(pageNumber),
    pageSize: parseInt(pageSize),
    totalPages: Math.ceil(filteredUsers.length / pageSize),
    hasPrevious: pageNumber > 1,
    hasNext: endIndex < filteredUsers.length
  };

  res.json(response);
});

// Start server
app.listen(PORT, () => {
  console.log(`Server running on https://localhost:${PORT}`);
});
```

## **Python Flask Backend Implementation:**

### **Controller (Python):**
```python
from flask import Flask, request, jsonify
from flask_cors import CORS
from datetime import datetime

app = Flask(__name__)
CORS(app)

# Mock user data
users = [
    {
        "id": "1",
        "userName": "admin",
        "fullName": "Admin User",
        "email": "admin@iirosa.org",
        "phoneNumber": "+966501234567",
        "isActive": True,
        "createdOn": "2024-01-15T10:30:00",
        "updatedOn": "2024-04-20T14:22:00",
        "roles": ["Super Admin", "Admin"]
    }
]

@app.route('/api/usermanagement', methods=['GET'])
def get_users():
    page_number = int(request.args.get('pageNumber', 1))
    page_size = int(request.args.get('pageSize', 10))
    search_text = request.args.get('searchText')
    is_active = request.args.get('isActive')
    roles = request.args.getlist('roles')

    print(f"GET /api/usermanagement called with params: {request.args}")

    # Filter users
    filtered_users = users[:]

    if search_text:
        filtered_users = [u for u in filtered_users 
                           if search_text.lower() in u['fullName'].lower() 
                           or search_text.lower() in u['email'].lower()]

    if is_active is not None:
        filtered_users = [u for u in filtered_users 
                           if u['isActive'] == (is_active == 'true')]

    if roles:
        filtered_users = [u for u in filtered_users 
                           if any(role in u['roles'] for role in roles)]

    # Pagination
    start_index = (page_number - 1) * page_size
    end_index = start_index + page_size
    paginated_users = filtered_users[start_index:end_index]

    response = {
        "items": paginated_users,
        "totalCount": len(filtered_users),
        "pageNumber": page_number,
        "pageSize": page_size,
        "totalPages": (len(filtered_users) + page_size - 1) // page_size,
        "hasPrevious": page_number > 1,
        "hasNext": end_index < len(filtered_users)
    }

    return jsonify(response)

if __name__ == '__main__':
    app.run(host='localhost', port=60960, debug=True)
```

## **Testing Your Backend:**

### **1. Test with curl:**
```bash
curl -X GET "https://localhost:60960/api/usermanagement?pageNumber=1&pageSize=10" \
  -H "Content-Type: application/json"
```

### **2. Test with Postman:**
- Method: GET
- URL: `https://localhost:60960/api/usermanagement?pageNumber=1&pageSize=10`
- Headers: `Content-Type: application/json`

### **3. Check Browser Console:**
1. Open browser DevTools (F12)
2. Go to Network tab
3. Navigate to users page
4. Look for the request to `/api/usermanagement`
5. Check if it shows:
   - Request URL
   - Request headers
   - Response status
   - Response body

## **Common Issues & Solutions:**

### **Issue 1: 404 Not Found**
**Problem:** Backend doesn't have the `/api/usermanagement` endpoint

**Solution:** Add the endpoint to your backend following the examples above

### **Issue 2: 401 Unauthorized**
**Problem:** Missing or invalid authentication token

**Solution:** Ensure backend accepts JWT tokens and the user is logged in

### **Issue 3: 500 Internal Server Error**
**Problem:** Backend code has an error

**Solution:** Check backend logs for detailed error information

### **Issue 4: CORS Errors**
**Problem:** Backend doesn't allow requests from localhost:4200

**Solution:** Add CORS middleware to your backend

The frontend is working correctly! Once you implement the backend endpoint `/api/usermanagement`, the users will load automatically. 🚀