# ASP.NET Core JWT Configuration Fix

## **Problem:**
JwtSettings is instantiated with default values instead of reading from appsettings.json

## **Solution:**

### **1. Check appsettings.json Content**

Make sure your `appsettings.json` contains:

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

### **2. Update .csproj File**

Add this to your backend .csproj file to ensure appsettings.json is copied:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <!-- Other properties -->
  </PropertyGroup>

  <!-- Add this ItemGroup -->
  <ItemGroup>
    <Content Include="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Include="appsettings.*.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      <CopyToPublishDirectory>Never</CopyToPublishDirectory>
    </Content>
  </ItemGroup>
</Project>
```

### **3. Create Proper JwtSettings Class**

```csharp
namespace YourProject.Models  // Adjust namespace as needed
{
    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; } = 60;
    }
}
```

### **4. Configure in Program.cs (NET 6+)**

```csharp
using YourProject.Models;

var builder = WebApplication.CreateBuilder(args);

// Add this BEFORE builder.Build()

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Register the JwtSettings for dependency injection
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddSingleton(jwtSettings);

// OR use the options pattern
builder.Services.AddSingleton(new JwtSettings
{
    Key = builder.Configuration["JwtSettings:Key"],
    Issuer = builder.Configuration["JwtSettings:Issuer"],
    Audience = builder.Configuration["JwtSettings:Audience"],
    ExpiryMinutes = Convert.ToInt32(builder.Configuration["JwtSettings:ExpiryMinutes"] ?? "60")
});

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Your middleware...
app.MapControllers();

app.Run();
```

### **5. Alternative: Use Options Pattern**

Update your `JwtIdentityTokenManager.cs`:

```csharp
using Microsoft.Extensions.Options;

public class JwtIdentityTokenManager : IIdentityTokenManager // if you have an interface
{
    private readonly JwtSettings _jwtSettings;

    // Constructor injection with IOptions
    public JwtIdentityTokenManager(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;

        // Debug logging to verify loading
        Console.WriteLine($"JWT Settings Loaded:");
        Console.WriteLine($"Key Length: {_jwtSettings.Key.Length}");
        Console.WriteLine($"Issuer: {_jwtSettings.Issuer}");
        Console.WriteLine($"Audience: {_jwtSettings.Audience}");
    }

    public string? GetCurrentUserName(string token)
    {
        // Validate settings are loaded
        if (_jwtSettings == null)
        {
            throw new InvalidOperationException("JWT settings are not configured. Check appsettings.json and Program.cs configuration.");
        }

        if (string.IsNullOrEmpty(_jwtSettings.Key))
        {
            throw new InvalidOperationException("JWT Key is missing. Add 'JwtSettings:Key' to appsettings.json");
        }

        if (token == null || token == "null" || token == "Bearer null")
        {
            return null;
        }

        token = token.Replace("Bearer ", "");
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

        if (jwtToken == null)
            return null;

        var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

        var validationParameters = new TokenValidationParameters()
        {
            RequireExpirationTime = true,
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = symmetricKey
        };

        var name = jwtToken.Claims?.FirstOrDefault(x => x.Type.Equals("name", StringComparison.OrdinalIgnoreCase))?.Value;

        return name;
    }
}
```

## **🔍 Debugging Steps:**

### **1. Check File Location**
Make sure `appsettings.json` is in the **root of your project** (not in a subfolder).

### **2. Test Configuration Loading**

Add this to your Program.cs to debug:

```csharp
// Debug configuration loading
var config = builder.Configuration;
Console.WriteLine($"Configuration Source: {config.GetDebugView()}");
Console.WriteLine($"JWT Key exists: {config["JwtSettings:Key"] != null}");
Console.WriteLine($"JWT Key value: {config["JwtSettings:Key"]}");

var jwtSection = config.GetSection("JwtSettings");
Console.WriteLine($"JWT Section exists: {jwtSection.Exists}");
Console.WriteLine($"JWT Section value: {jwtSection.Value}");
```

### **3. Check File Properties**

Right-click on `appsettings.json` → Properties → Make sure:
- **Build Action** = "Content" (not "None")
- **Copy to Output Directory** = "Copy always" or "Preserve newest"
- **Copy to Publish Directory** = "Never" (for local dev)

## **🚀 Quick Test:**

Add this temporary debug code in your constructor:

```csharp
public JwtIdentityTokenManager(IOptions<JwtSettings> jwtSettings)
{
    _jwtSettings = jwtSettings.Value;

    // Debug: Check if settings are loaded
    var key = _jwtSettings.Key;
    Console.WriteLine($"DEBUG: JWT Key loaded = {(!string.IsNullOrEmpty(key))}");
    Console.WriteLine($"DEBUG: Key length = {key?.Length ?? 0}");
    Console.WriteLine($"DEBUG: Key preview = {key?.Substring(0, Math.Min(10, key?.Length ?? 0))}...");
}
```

## **📋 Most Common Issues:**

1. **appsettings.json not in project root**
2. **Wrong namespace in JwtSettings class**
3. **Configuration not loaded before dependency injection**
4. **appsettings.json properties don't match class properties**
5. **File not being copied to output directory**

Try these steps and let me know which one fixes it! 🎯
