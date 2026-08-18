# Backend JWT Configuration Fix

## **Problem:**
Your backend is missing the JWT signing key configuration.

## **Solutions:**

### **Solution 1: Add to appsettings.json**

Add this to your `appsettings.json`:

```json
{
  "JwtSettings": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "YourIssuer",
    "Audience": "YourAudience",
    "ExpiryMinutes": 60
  }
}
```

**Important:** The Key must be at least 32 characters long!

### **Solution 2: Add to appsettings.Development.json**

```json
{
  "JwtSettings": {
    "Key": "Development_Secret_Key_32_Characters_Long!!",
    "Issuer": "https://localhost:60960",
    "Audience": "https://localhost:60960",
    "ExpiryMinutes": 60
  }
}
```

### **Solution 3: Generate a Secure Key**

Run this in PowerShell to generate a secure key:
```powershell
# Generate a random 32-byte key and convert to Base64
$key = New-Object byte[] 32
[Security.Cryptography.RNGCryptoServiceProvider]::CreateBytes($key)
$base64Key = [System.Convert]::ToBase64String($key)
Write-Host "Your JWT Key: $base64Key"
```

Or use this simpler key (for development only):
```
YourDevelopmentSecretKey_32CharactersLong!
```

### **Solution 4: Update Program.cs or Startup.cs**

Make sure your JWT configuration is properly set up:

```csharp
// In Program.cs or Startup.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = builder.Configuration["JwtSettings:Key"]
        };
    });

// Ensure JWT Settings are loaded
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);
```

## **Quick Test:**

1. Add this to your `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "JwtSettings": {
    "Key": "Development_Secret_Key_32_Characters_Long!!",
    "Issuer": "https://localhost:60960",
    "Audience": "https://localhost:60960"
  }
}
```

2. Restart your backend application

3. Test the frontend again

## **Security Note:**

⚠️ **For Development Only**: The simple key above is fine for development.

🔒 **For Production**: Use a properly generated secure key!

```powershell
# Generate secure key for production
$key = New-Object byte[] 64
[Security.Cryptography.RNGCryptoServiceProvider]::CreateBytes($key)
$secureKey = [System.Convert]::ToBase64String($key)
Write-Host "Production JWT Key: $secureKey"
```
