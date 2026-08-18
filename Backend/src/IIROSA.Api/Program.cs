using AutoMapper;
using FluentValidation;
using Framework.Core.AutoMapper;
using Framework.Core.Notifications;
using Framework.Core.SharedServices.Services;
using Framework.Identity.Data;
using IIROSA.Api.Hubs;
using IIROSA.Api.Middleware;
using IIROSA.Application;
using IIROSA.Application.Profiles;
using IIROSA.Application.Services;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using IIROSA.Infrastructure.Data.Repository;
using IIROSA.Infrastructure.Data.SeedData;
using IIROSA.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Web;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


#region CORS
//services cors
//builder.Services.AddCors(p =>
//{
//    p.AddPolicy("corsapp", builder =>
//    {
//        builder.WithOrigins(ClientAppURL.ToArray())
//        .WithMethods("POST", "GET")
//        .AllowAnyHeader()
//        .AllowCredentials();

//    });
//});

// Add CORS - MUST be configured before authentication to work properly
builder.Services.AddCors(options =>
{
    // Development policy - reads origins from appsettings.Development.json
    options.AddPolicy("AllowAll", policy =>
    {
        var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] {
                // Fallback origins if configuration is missing
                "http://localhost:3000",
                "http://localhost:4200",
                "http://localhost:5000",
                "http://localhost:5001",
                "https://localhost:3000",
                "https://localhost:4200"
            };

        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromSeconds(86400)); // Cache preflight for 24 hours
    });

    // Production policy - reads origins from appsettings.json
    options.AddPolicy("Production", policy =>
    {
        var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "https://yourdomain.com" };

        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromSeconds(86400));
    });
});
#endregion CORS




ConfigurationManager configuration = builder.Configuration;
// Add services to the container.
builder.Services.ConfigureSharedApplicationServices(connectionString);
builder.Services.ConfigureApplicationServices(configuration);
builder.Services.IdentityConfigureServices(connectionString);  // MUST be before AddAuth - Identity adds cookie auth
builder.Services.AddAuth(configuration);                        // THEN override with JWT Bearer as default
builder.Services.AddInfrastructure(configuration);
//builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddAntiforgery(options => options.SuppressXFrameOptionsHeader = false);
builder.Services.AddApplicationServices();

//// AutoMapper Configuration
//var config = new MapperConfiguration(cfg =>
//{
//    var assembly = Assembly.GetExecutingAssembly();

//    // Get all types in the assembly that are profiles and match the specified condition
//    var profiles = assembly.GetTypes()
//        .Where(type => type.IsSubclassOf(typeof(Profile)) &&
//                       type.Name.EndsWith("Profile"));

//    // Register each profile
//    foreach (var profile in profiles)
//    {
//        cfg.AddProfile(profile);
//    }
//});

//AutoMapperConfiguration.Init(config);

builder.Services.Configure<FormOptions>(o =>
{
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartBodyLengthLimit = int.MaxValue;
    o.MemoryBufferThreshold = int.MaxValue;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.InitHangfire(connectionString);

#region Nlog

builder.Logging.ClearProviders();
builder.Host.UseNLog();

#endregion Nlog



// Add Controllers
builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
})
.AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IIROSA API",
        Version = "v1",
        Description = "IIROSA Orphan Management System API - Built with Framework.Core & Framework.Identity",
        Contact = new OpenApiContact
        {
            Name = "IIROSA Team",
            Email = "info@iirosa.org"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments from Framework projects
    var frameworkCoreXml = "Framework.Framework.Core.xml";
    var frameworkCoreXmlPath = Path.Combine(AppContext.BaseDirectory, frameworkCoreXml);
    if (File.Exists(frameworkCoreXmlPath))
    {
        options.IncludeXmlComments(frameworkCoreXmlPath);
    }

    var frameworkIdentityXml = "Framework.Framework.Identity.xml";
    var frameworkIdentityXmlPath = Path.Combine(AppContext.BaseDirectory, frameworkIdentityXml);
    if (File.Exists(frameworkIdentityXmlPath))
    {
        options.IncludeXmlComments(frameworkIdentityXmlPath);
    }
});
builder.Services.AddHttpClient();
builder.Services.AddSignalR();

// Add Application services (Token, Settings, Notifications, Attachments)


//// Add AutoMapper using the same approach as the working project
//Console.WriteLine("🔧 Registering AutoMapper...");
//try
//{
//    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
//    var config = new MapperConfiguration(cfg =>
//    {
//        // Get all Profile types automatically
//        var profiles = assembly.GetTypes()
//            .Where(type => type.IsSubclassOf(typeof(AutoMapper.Profile)) &&
//                       type.Name.EndsWith("Profile") &&
//                       type.GetTypeInfo().IsClass &&
//                       !type.GetTypeInfo().IsAbstract);

//        Console.WriteLine($"   Found {profiles.Count()} profiles:");
//        foreach (var profile in profiles)
//        {
//            Console.WriteLine($"   - {profile.Name}");
//            cfg.AddProfile(profile);
//        }
//    });

//    // Initialize using Framework's AutoMapper configuration
//    Framework.Core.AutoMapper.AutoMapperConfiguration.Init(config);
//    Console.WriteLine("   ✅ AutoMapper configured successfully");
//}
//catch (Exception ex)
//{
//    Console.WriteLine($"❌ CRITICAL: Failed to configure AutoMapper!");
//    Console.WriteLine($"   Exception: {ex.GetType().Name}");
//    Console.WriteLine($"   Message: {ex.Message}");
//    Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
//    throw;
//}

// DIRECT REGISTRATION - Create and register JWT settings object directly
var jwtSettingsDto = new Framework.Core.Helper.JwtIdentitySettingDto
{
    Key = builder.Configuration["Jwt:Key"]
           ?? builder.Configuration["Jwt:Key"]
           ?? "YourSuperSecretKeyWith32CharactersLength!!",
    Issuer = builder.Configuration["Jwt:Issuer"]
             ?? builder.Configuration["Jwt:Issuer"]
             ?? "IIROSAApi",
    Audience = builder.Configuration["Jwt:Audience"]
               ?? builder.Configuration["Jwt:Audience"]
               ?? "IIROSAClient",
    ExpirationInMinutes = double.Parse(builder.Configuration["Jwt:ExpirationInMinutes"])
};

Console.WriteLine($"\n✅ JWT Settings Object Created:");
Console.WriteLine($"   Key Length: {jwtSettingsDto.Key?.Length ?? 0}");
Console.WriteLine($"   Key Value: {jwtSettingsDto.Key}");
Console.WriteLine($"   Issuer: {jwtSettingsDto.Issuer}");
Console.WriteLine($"   Audience: {jwtSettingsDto.Audience}");

// Register as singleton for direct injection (no IOptions needed)
builder.Services.AddSingleton(jwtSettingsDto);

//// Add Authentication
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    // Try to get JWT settings from multiple possible locations
//    var jwtKey = builder.Configuration["JwtIdentitySettingDto:Key"]
//                 ?? builder.Configuration["Jwt:Key"]
//                 ?? builder.Configuration["JWT:Key"]
//                 ?? "YourSuperSecretKeyHere123456789012";

//    var jwtIssuer = builder.Configuration["JwtIdentitySettingDto:Issuer"]
//                    ?? builder.Configuration["Jwt:Issuer"]
//                    ?? builder.Configuration["JWT:Issuer"]
//                    ?? "IIROSAApi";

//    var jwtAudience = builder.Configuration["JwtIdentitySettingDto:Audience"]
//                     ?? builder.Configuration["Jwt:Audience"]
//                     ?? builder.Configuration["JWT:Audience"]
//                     ?? "IIROSAClient";

//    Console.WriteLine($"🔧 JWT Bearer Configuration:");
//    Console.WriteLine($"   Key source: {(builder.Configuration["JwtIdentitySettingDto:Key"] != null ? "JwtIdentitySettingDto" : builder.Configuration["Jwt:Key"] != null ? "Jwt" : "Fallback")}");
//    Console.WriteLine($"   Key Length: {jwtKey.Length}");
//    Console.WriteLine($"   Issuer: {jwtIssuer}");
//    Console.WriteLine($"   Audience: {jwtAudience}");

//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = jwtIssuer,
//        ValidAudience = jwtAudience,
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
//    };
//});

//// Add Authorization with IIROSA role policies
//builder.Services.AddAuthorization(options =>
//{
//    // IIROSA Role Policies as per UC-1.1
//    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
//    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
//    options.AddPolicy("CharityOnly", policy => policy.RequireRole("Charity"));
//    options.AddPolicy("AccountantOnly", policy => policy.RequireRole("Accountant"));
//    options.AddPolicy("FinancialOfficerOnly", policy => policy.RequireRole("FinancialOfficer"));

//    // Combined policies for broader access
//    options.AddPolicy("ManagementOnly", policy => policy.RequireRole("SuperAdmin", "Admin"));
//    options.AddPolicy("FinancialOnly", policy => policy.RequireRole("SuperAdmin", "Admin", "Accountant", "FinancialOfficer"));
//    options.AddPolicy("AllRoles", policy => policy.RequireRole("SuperAdmin", "Admin", "Charity", "Accountant", "FinancialOfficer"));

//    // Operational policies
//    options.AddPolicy("CanManageUsers", policy => policy.RequireRole("SuperAdmin"));
//    options.AddPolicy("CanManageRoles", policy => policy.RequireRole("SuperAdmin"));
//    options.AddPolicy("CanViewReports", policy => policy.RequireRole("SuperAdmin", "Admin", "Charity", "Accountant", "FinancialOfficer"));
//    options.AddPolicy("CanManageFinance", policy => policy.RequireRole("SuperAdmin", "Accountant", "FinancialOfficer"));
//});

//// Register Lookup Repositories FIRST (before dynamic DI) to ensure they're available
//builder.Services.AddScoped<ICountryRepository, CountryRepository>();
//builder.Services.AddScoped<IRegionRepository, RegionRepository>();
//builder.Services.AddScoped<ICenterRepository, CenterRepository>();
//builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
//builder.Services.AddScoped<IMissionTypeRepository, MissionTypeRepository>();
//builder.Services.AddScoped<IProjectTypeRepository, ProjectTypeRepository>();
//builder.Services.AddScoped<IBankRepository, BankRepository>();
//builder.Services.AddScoped<INGOTypeRepository, NGOTypeRepository>();
//Console.WriteLine("✅ Explicitly registered lookup repositories");

// Ensure IIROSA.Infrastructure assembly is loaded before dynamic DI
//var infrastructureAssembly = System.Reflection.Assembly.GetAssembly(typeof(CountryRepository));
//if (infrastructureAssembly != null)
//{
//    Console.WriteLine($"✅ IIROSA.Infrastructure assembly loaded: {infrastructureAssembly.GetName().Name}");
//}
//else
//{
//    Console.WriteLine("⚠️  Failed to load IIROSA.Infrastructure assembly");
//}

// Register Services - Dynamic DI (Comprehensive)
// Get all assemblies that contain our services
//try
//{
//    var assembliesToScan = AppDomain.CurrentDomain.GetAssemblies()
//        .Where(a => !string.IsNullOrEmpty(a.FullName))
//        .Where(a => (a.FullName.StartsWith("IIROSA") || a.FullName.StartsWith("Framework")) &&
//                    !a.FullName.Contains("Microsoft") &&
//                    !a.FullName.Contains("System"))
//        .ToList();

//    foreach (var assembly in assembliesToScan)
//    {
//        if (assembly == null) continue;

//        try
//        {
//            var serviceTypes = assembly.GetTypes()
//                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
//                .Where(t => t.Name.EndsWith("Service") || t.Name.EndsWith("Repository") || t.Name.EndsWith("AppService"));

//            foreach (var serviceType in serviceTypes)
//            {
//                try
//                {
//                    // Skip extended service registration - we'll handle them manually below
//                    if (serviceType.Name.EndsWith("Extended"))
//                    {
//                        continue;
//                    }

//                    // Get the direct interface first (I{ServiceName})
//                    var directInterface = serviceType.GetInterfaces()
//                        .FirstOrDefault(i => i.Name.Equals($"I{serviceType.Name}", StringComparison.OrdinalIgnoreCase) &&
//                                           !i.IsGenericType &&
//                                           !i.Namespace?.StartsWith("System") == true &&
//                                           !i.Namespace?.StartsWith("Microsoft") == true);

//                    // Get additional interfaces (excluding system interfaces and the direct interface)
//                    var additionalInterfaces = serviceType.GetInterfaces()
//                        .Where(i => i.Name != $"I{serviceType.Name}" &&
//                                   !i.Name.StartsWith("IDisposable") &&
//                                   !i.Namespace?.StartsWith("System") == true &&
//                                   !i.Namespace?.StartsWith("Microsoft") == true &&
//                                   !i.IsGenericType)
//                        .ToList();

//                    if (directInterface != null)
//                    {
//                        // Register the primary interface mapping
//                        builder.Services.AddScoped(directInterface, serviceType);

//                        // Also register the concrete class for direct injection
//                        builder.Services.AddScoped(serviceType);

//                        // Register additional interfaces if they exist
//                        foreach (var additionalInterface in additionalInterfaces)
//                        {
//                            builder.Services.AddScoped(additionalInterface, serviceType);
//                        }
//                    }
//                    else
//                    {
//                        // No direct interface found, register as self
//                        builder.Services.AddScoped(serviceType);

//                        // Register any additional interfaces
//                        foreach (var additionalInterface in additionalInterfaces)
//                        {
//                            builder.Services.AddScoped(additionalInterface, serviceType);
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    // Log service registration error but continue with other services
//                    Console.WriteLine($"⚠️  Error registering service {serviceType.Name}: {ex.Message}");
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            // Log assembly scanning error but continue with other assemblies
//            Console.WriteLine($"⚠️  Error scanning assembly {assembly.GetName().Name}: {ex.Message}");
//        }
//    }
//}
//catch (Exception ex)
//{
//    Console.WriteLine($"⚠️  Error in dynamic DI registration: {ex.Message}");
//}

// Log DI registration success
Console.WriteLine("✅ Dynamic DI Registration completed - Services and interfaces auto-registered");

// Register extended services explicitly to override base services
builder.Services.AddScoped<Framework.Identity.Data.Services.Interfaces.IUserAppServiceExtended, Framework.Identity.Data.Services.UserAppServiceExtended>();

// Note: IUserAppService is handled by dynamic DI registration above
// If you want to use UserAppServiceExtended instead of UserAppService, uncomment below:
// builder.Services.AddScoped<Framework.Identity.Data.Services.Interfaces.IUserAppService, Framework.Identity.Data.Services.UserAppServiceExtended>();


// Add SignalR







var app = builder.Build();

//app.UseCustomExceptionHandler();
//app.UseCustomLanguageHandler();



// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "IIROSA API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "IIROSA API Documentation";
        options.DefaultModelsExpandDepth(-1); // Hide schemas by default
        options.DefaultModelExpandDepth(2); // Expand models when clicked
    });

    // Enable XML comments file generation
    app.UseSwaggerUI(options =>
    {
        // options.DisplayOperationIdCore();
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableFilter();
        options.ShowExtensions();
        // options.DocExpansion(SwaggerDocExpansion.None);
    });
//}

app.UseHttpsRedirection();

// Global exception handling - MUST be early in the pipeline
app.UseExceptionMiddleware();

// Enable CORS - use different policies based on environment
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
    Console.WriteLine("CORS: Using AllowAll policy for development");
}
else
{
    app.UseCors("Production");
    Console.WriteLine("CORS: Using Production policy");
}

// Use authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub with CORS support
app.MapHub<NotificationHub>("/hubs/notifications")
   .RequireCors(
       builder => builder
           .WithOrigins("http://localhost:4200", "https://localhost:4200", "http://localhost:3000", "https://localhost:3000")
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials()
   );

// Add health check endpoint
app.MapGet("/health", () =>
{
    return new
    {
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        Version = "v1.0.0"
    };
})
.WithName("Health Check");

// Initialize seed data (only in Development or on first run)
if (app.Environment.IsDevelopment() || IsFirstRun(app))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            Console.WriteLine("Applying pending migrations...");
            dbContext.Database.Migrate();
            Console.WriteLine("Migrations applied successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying migrations: {ex.Message}");
            // Handle the exception as needed
        }
    }
    using (var scope = app.Services.CreateScope())
    {
        var seedDataInitializer = scope.ServiceProvider.GetRequiredService<IIROSASeedDataInitializer>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Starting seed data initialization...");
            await seedDataInitializer.InitializeAsync();

            // Verify seed data
            var isVerified = await seedDataInitializer.VerifySeedDataAsync();
            if (isVerified)
            {
                logger.LogInformation("Seed data verification successful");
            }
            else
            {
                logger.LogWarning("Seed data verification failed");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize seed data");
            // Don't throw - allow application to start even if seeding fails
        }
    }
}

app.Run();

// Helper method to check if this is first run
static bool IsFirstRun(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
        try
        {
            // Check if we have any users in the database
            var userCount = context.Users.Count();
            return userCount == 0;
        }
        catch
        {
            return true;
        }
    }
}
