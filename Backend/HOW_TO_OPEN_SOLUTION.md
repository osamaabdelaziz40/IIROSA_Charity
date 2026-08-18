# 🚀 How to Open the Solution

## Option 1: Open Solution File (Recommended)
```
D:\Osama\IIROSA Claude\Backend\IIROSA.sln
```

This opens the entire solution with all projects.

## Option 2: Open Main API Project
```
D:\Osama\IIROSA Claude\Backend\src\API\API.csproj
```

This opens just the API project (the startup project).

---

## 📁 Solution Structure

```
Backend\
├── IIROSA.sln                    ← Open this file in Visual Studio!
│
├── Framework/
│   ├── Framework.Core/
│   └── Framework.Identity/
│
└── src/
    ├── Domain/
    ├── Application/
    ├── Infrastructure/
    └── API/
        └── API.csproj         ← Startup Project (Set as StartUp Project)
```

---

## 🎯 Startup Project

**API.csproj** is the startup project (the Web API application)

Location: `Backend\src\API\API.csproj`

When you run the solution, it will start the API project.

---

## ✅ Quick Start

1. **Open in Visual Studio:**
   - File → Open → Project/Solution
   - Navigate to: `D:\Osama\IIROSA Claude\Backend\IIROSA.sln`

2. **Set Startup Project:**
   - Right-click on `src/API` in Solution Explorer
   - Select "Set as StartUp Project"

3. **Run:**
   - Press F5 or click "Start" button
   - API will run on: `https://localhost:5001`
   - Swagger UI: `https://localhost:5001/swagger`

---

## 📦 Projects in Solution

1. **Framework.Core** - Base entities and interfaces
2. **Framework.Identity** - User and role management
3. **Domain** - Business entities (Charity, Employee, Orphan, etc.)
4. **Application** - DTOs, Services, AutoMapper profiles
5. **Infrastructure** - DbContext, Repositories
6. **API** ← **Startup Project** - Controllers, Hubs, Program.cs

---

## 🔧 Build & Run

### Using Visual Studio
```
1. Open IIROSA.sln
2. Build Solution (Ctrl+Shift+B)
3. Run (F5)
```

### Using Command Line
```bash
cd "D:\Osama\IIROSA Claude\Backend"

# Restore packages
dotnet restore

# Build
dotnet build

# Run API project
dotnet run --project src/API/API.csproj
```

---

## 🌐 API Endpoints (After Running)

- **API**: https://localhost:5001
- **Swagger**: https://localhost:5001/swagger
- **Health**: https://localhost:5001/health

---

**Open `Backend\IIROSA.sln` to start development!** 🚀
