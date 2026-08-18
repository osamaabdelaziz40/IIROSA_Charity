# Quick Start Guide - Creating a New Module

This guide shows you how to quickly create a new module following the Repository & Service Pattern architecture.

---

## Step-by-Step Module Creation

### **Example: Creating a "Document Management" Module**

#### **Step 1: Create Domain Entity (2 minutes)**

```csharp
// IIROSA.Domain/Entities/Document.cs
using System;

namespace IIROSA.Domain.Entities;

public class Document : IAuditable
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public string MimeType { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? FK_CharityId { get; set; }
    public Guid? FK_OrphanId { get; set; }
    public Guid? FK_FamilyId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // Navigation Properties
    public virtual Charity? Charity { get; set; }
    public virtual Orphan? Orphan { get; set; }
    public virtual Family? Family { get; set; }
    public virtual DocumentCategory? Category { get; set; }

    // IAuditable
    public DateTime CreatedDate { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
    public Guid? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
}

// IIROSA.Domain/Entities/DocumentCategory.cs
public class DocumentCategory : IAuditable
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }

    // Navigation
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    // IAuditable
    public DateTime CreatedDate { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
    public Guid? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
}
```

#### **Step 2: Create Repository Interface & Implementation (5 minutes)**

```csharp
// IIROSA.Domain/Interfaces/IDocumentRepository.cs
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

public interface IDocumentRepository : IRepository<Document>
{
    Task<IEnumerable<Document>> GetActiveDocumentsAsync();
    Task<IEnumerable<Document>> GetDocumentsByCharityAsync(Guid charityId);
    Task<IEnumerable<Document>> GetDocumentsByCategoryAsync(Guid categoryId);
    Task<IEnumerable<Document>> GetDocumentsByOrphanAsync(Guid orphanId);
    Task<IEnumerable<Document>> GetDocumentsByFamilyAsync(Guid familyId);
    Task<Document?> GetDocumentWithPathAsync(Guid id);
    Task<IEnumerable<Document>> GetExpiringDocumentsAsync(int daysThreshold);
}

// IIROSA.Infrastructure/Data/Repository/DocumentRepository.cs
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

public class DocumentRepository : Repository<Document>, IDocumentRepository
{
    public DocumentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Document>> GetActiveDocumentsAsync()
    {
        return await _context.Documents
            .Include(d => d.Category)
            .Where(d => d.IsActive && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetDocumentsByCharityAsync(Guid charityId)
    {
        return await _context.Documents
            .Include(d => d.Category)
            .Where(d => d.FK_CharityId == charityId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetDocumentsByCategoryAsync(Guid categoryId)
    {
        return await _context.Documents
            .Include(d => d.Category)
            .Where(d => d.CategoryId == categoryId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetDocumentsByOrphanAsync(Guid orphanId)
    {
        return await _context.Documents
            .Include(d => d.Category)
            .Where(d => d.FK_OrphanId == orphanId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetDocumentsByFamilyAsync(Guid familyId)
    {
        return await _context.Documents
            .Include(d => d.Category)
            .Where(d => d.FK_FamilyId == familyId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<Document?> GetDocumentWithPathAsync(Guid id)
    {
        return await _context.Documents
            .Include(d => d.Category)
            .Include(d => d.Charity)
            .Include(d => d.Orphan)
            .Include(d => d.Family)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<Document>> GetExpiringDocumentsAsync(int daysThreshold)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);
        return await _context.Documents
            .Include(d => d.Category)
            .Where(d => d.ExpiryDate.HasValue && d.ExpiryDate <= thresholdDate && !d.IsDeleted)
            .OrderBy(d => d.ExpiryDate)
            .ToListAsync();
    }
}
```

#### **Step 3: Create DTOs (3 minutes)**

```csharp
// IIROSA.Application/DTOs/Document/DocumentDto.cs
namespace IIROSA.Application.DTOs.Document;

public class DocumentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public string FileSizeFormatted { get; set; }
    public string MimeType { get; set; }
    public Guid? CategoryId { get; set; }
    public string CategoryName { get; set; }
    public Guid? CharityId { get; set; }
    public Guid? OrphanId { get; set; }
    public string OrphanName { get; set; }
    public Guid? FamilyId { get; set; }
    public string FamilyName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate < DateTime.UtcNow;
    public DateTime CreatedDate { get; set; }
    public string DownloadUrl { get; set; }
}

public class CreateDocumentDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public IFormFile File { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? CharityId { get; set; }
    public Guid? OrphanId { get; set; }
    public Guid? FamilyId { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class UpdateDocumentDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class DocumentFilterDto
{
    public Guid? CategoryId { get; set; }
    public Guid? CharityId { get; set; }
    public Guid? OrphanId { get; set; }
    public Guid? FamilyId { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
```

#### **Step 4: Create Service Interface & Implementation (10 minutes)**

```csharp
// IIROSA.Application/Interfaces/IDocumentService.cs
using IIROSA.Application.DTOs.Document;

namespace IIROSA.Application.Interfaces;

public interface IDocumentService : IService<DocumentDto, Document>
{
    Task<IEnumerable<DocumentDto>> GetActiveDocumentsAsync();
    Task<IEnumerable<DocumentDto>> GetDocumentsByCharityAsync(Guid charityId);
    Task<IEnumerable<DocumentDto>> GetDocumentsByOrphanAsync(Guid orphanId);
    Task<IEnumerable<DocumentDto>> GetDocumentsByFamilyAsync(Guid familyId);
    Task<DocumentDto> UploadDocumentAsync(CreateDocumentDto dto);
    Task<byte[]> DownloadDocumentAsync(Guid id);
    Task<IEnumerable<DocumentDto>> GetExpiringDocumentsAsync(int daysThreshold);
    Task MarkAsExpiredAsync(Guid id);
    Task<string> GetDownloadUrlAsync(Guid id);
}

// IIROSA.Application/Services/DocumentService.cs
using IIROSA.Application.DTOs.Document;
using IIROSA.Application.Interfaces;
using IIROSA.Application.Exceptions;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;

namespace IIROSA.Application.Services;

public class DocumentService : Service<DocumentDto, Document, IDocumentRepository>, IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;

    public DocumentService(
        IDocumentRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<DocumentService> logger,
        IFileStorageService fileStorageService)
        : base(repository, unitOfWork, mapper, logger)
    {
        _documentRepository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<IEnumerable<DocumentDto>> GetActiveDocumentsAsync()
    {
        var documents = await _documentRepository.GetActiveDocumentsAsync();
        var dtos = _mapper.Map<IEnumerable<DocumentDto>>(documents);
        
        // Add download URLs
        foreach (var dto in dtos)
        {
            dto.DownloadUrl = await GetDownloadUrlAsync(dto.Id);
        }
        
        return dtos;
    }

    public async Task<IEnumerable<DocumentDto>> GetDocumentsByCharityAsync(Guid charityId)
    {
        var documents = await _documentRepository.GetDocumentsByCharityAsync(charityId);
        var dtos = _mapper.Map<IEnumerable<DocumentDto>>(documents);
        
        foreach (var dto in dtos)
        {
            dto.DownloadUrl = await GetDownloadUrlAsync(dto.Id);
        }
        
        return dtos;
    }

    public async Task<IEnumerable<DocumentDto>> GetDocumentsByOrphanAsync(Guid orphanId)
    {
        var documents = await _documentRepository.GetDocumentsByOrphanAsync(orphanId);
        var dtos = _mapper.Map<IEnumerable<DocumentDto>>(documents);
        
        foreach (var dto in dtos)
        {
            dto.DownloadUrl = await GetDownloadUrlAsync(dto.Id);
        }
        
        return dtos;
    }

    public async Task<IEnumerable<DocumentDto>> GetDocumentsByFamilyAsync(Guid familyId)
    {
        var documents = await _documentRepository.GetDocumentsByFamilyAsync(familyId);
        var dtos = _mapper.Map<IEnumerable<DocumentDto>>(documents);
        
        foreach (var dto in dtos)
        {
            dto.DownloadUrl = await GetDownloadUrlAsync(dto.Id);
        }
        
        return dtos;
    }

    public async Task<DocumentDto> UploadDocumentAsync(CreateDocumentDto dto)
    {
        // Validate file
        if (dto.File == null || dto.File.Length == 0)
        {
            throw new ValidationException(new List<string> { "File is required" });
        }

        // Upload file to storage
        var filePath = await _fileStorageService.SaveFileAsync(dto.File);

        // Create document entity
        var document = new Document
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            FileName = dto.File.FileName,
            FilePath = filePath,
            FileSize = dto.File.Length,
            MimeType = dto.File.ContentType,
            CategoryId = dto.CategoryId,
            FK_CharityId = dto.CharityId,
            FK_OrphanId = dto.OrphanId,
            FK_FamilyId = dto.FamilyId,
            IsActive = true,
            ExpiryDate = dto.ExpiryDate,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = GetCurrentUserId()
        };

        await _documentRepository.AddAsync(document);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Document uploaded: {DocumentId} - {FileName}", document.Id, document.FileName);

        var result = _mapper.Map<DocumentDto>(document);
        result.DownloadUrl = await GetDownloadUrlAsync(document.Id);
        return result;
    }

    public async Task<byte[]> DownloadDocumentAsync(Guid id)
    {
        var document = await _documentRepository.GetDocumentWithPathAsync(id);
        if (document == null)
        {
            throw new NotFoundException(nameof(Document), id);
        }

        if (!System.IO.File.Exists(document.FilePath))
        {
            throw new BusinessException("File not found on server");
        }

        return await System.IO.File.ReadAllBytesAsync(document.FilePath);
    }

    public async Task<IEnumerable<DocumentDto>> GetExpiringDocumentsAsync(int daysThreshold)
    {
        var documents = await _documentRepository.GetExpiringDocumentsAsync(daysThreshold);
        var dtos = _mapper.Map<IEnumerable<DocumentDto>>(documents);
        
        foreach (var dto in dtos)
        {
            dto.DownloadUrl = await GetDownloadUrlAsync(dto.Id);
        }
        
        return dtos;
    }

    public async Task MarkAsExpiredAsync(Guid id)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        if (document == null)
        {
            throw new NotFoundException(nameof(Document), id);
        }

        document.IsActive = false;
        _documentRepository.Update(document);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Document marked as expired: {DocumentId}", id);
    }

    public async Task<string> GetDownloadUrlAsync(Guid id)
    {
        // Generate signed URL with expiry (e.g., 1 hour)
        return $"/api/v1/documents/{id}/download?token={GenerateDownloadToken(id)}";
    }

    private string GenerateDownloadToken(Guid documentId)
    {
        // Generate secure token for download
        var tokenData = $"{documentId}:{DateTime.UtcNow.AddHours(1):Ticks}";
        var tokenBytes = System.Text.Encoding.UTF8.GetBytes(tokenData);
        return Convert.ToBase64String(tokenBytes);
    }
}
```

#### **Step 5: Update AutoMapper Profile (2 minutes)**

```csharp
// IIROSA.Application/Mappers/MappingProfile.cs
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ... existing mappings ...

        // Document Mappings
        CreateMap<Document, DocumentDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.OrphanName, opt => opt.MapFrom(src => src.Orphan != null ? src.Orphan.FullName : null))
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? $"{src.Family.FatherName} Family" : null))
            .ForMember(dest => dest.FileSizeFormatted, opt => opt.MapFrom(src => FormatFileSize(src.FileSize)))
            .ForMember(dest => dest.DownloadUrl, opt => opt.Ignore()); // Set in service

        // No need to map CreateDocumentDto/UpdateDocumentDto - use manual mapping or automapper
        // CreateMap<CreateDocumentDto, Document>();
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
```

#### **Step 6: Create Controller (5 minutes)**

```csharp
// IIROSA.Web/Controllers/Api/V1/DocumentsController.cs
using IIROSA.Application.DTOs.Document;
using IIROSA.Application.Interfaces;
using IIROSA.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace IIROSA.Web.Controllers.Api.V1;

[ApiVersion("1.0")]
public class DocumentsController : BaseController<DocumentDto, Document>
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService, ILogger<DocumentsController> logger)
        : base(documentService, logger)
    {
        _documentService = documentService;
    }

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DocumentDto>>>> GetActiveDocuments()
    {
        try
        {
            var documents = await _documentService.GetActiveDocumentsAsync();
            return Ok(ApiResponse<IEnumerable<DocumentDto>>.Success(documents));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active documents");
            return StatusCode(500, ApiResponse<IEnumerable<DocumentDto>>.Error("An error occurred"));
        }
    }

    [HttpGet("charity/{charityId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DocumentDto>>>> GetDocumentsByCharity(Guid charityId)
    {
        try
        {
            var documents = await _documentService.GetDocumentsByCharityAsync(charityId);
            return Ok(ApiResponse<IEnumerable<DocumentDto>>.Success(documents));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents for charity {CharityId}", charityId);
            return StatusCode(500, ApiResponse<IEnumerable<DocumentDto>>.Error("An error occurred"));
        }
    }

    [HttpGet("orphan/{orphanId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DocumentDto>>>> GetDocumentsByOrphan(Guid orphanId)
    {
        try
        {
            var documents = await _documentService.GetDocumentsByOrphanAsync(orphanId);
            return Ok(ApiResponse<IEnumerable<DocumentDto>>.Success(documents));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents for orphan {OrphanId}", orphanId);
            return StatusCode(500, ApiResponse<IEnumerable<DocumentDto>>.Error("An error occurred"));
        }
    }

    [HttpGet("family/{familyId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DocumentDto>>>> GetDocumentsByFamily(Guid familyId)
    {
        try
        {
            var documents = await _documentService.GetDocumentsByFamilyAsync(familyId);
            return Ok(ApiResponse<IEnumerable<DocumentDto>>.Success(documents));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents for family {FamilyId}", familyId);
            return StatusCode(500, ApiResponse<IEnumerable<DocumentDto>>.Error("An error occurred"));
        }
    }

    [HttpGet("expiring")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DocumentDto>>>> GetExpiringDocuments([FromQuery] int daysThreshold = 30)
    {
        try
        {
            var documents = await _documentService.GetExpiringDocumentsAsync(daysThreshold);
            return Ok(ApiResponse<IEnumerable<DocumentDto>>.Success(documents));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expiring documents");
            return StatusCode(500, ApiResponse<IEnumerable<DocumentDto>>.Error("An error occurred"));
        }
    }

    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<DocumentDto>>> Upload([FromForm] CreateDocumentDto dto)
    {
        try
        {
            var result = await _documentService.UploadDocumentAsync(dto);
            return Ok(ApiResponse<DocumentDto>.Success(result, "Document uploaded successfully"));
        }
        catch (ValidationException ex)
        {
            return BadRequest(ApiResponse<DocumentDto>.ValidationError(ex.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return StatusCode(500, ApiResponse<DocumentDto>.Error("An error occurred"));
        }
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        try
        {
            var fileBytes = await _documentService.DownloadDocumentAsync(id);
            
            var document = await _documentService.GetByIdAsync(id);
            if (document == null)
                return NotFound();

            return File(fileBytes, document.MimeType, document.FileName);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document {DocumentId}", id);
            return StatusCode(500, "An error occurred while downloading");
        }
    }

    [HttpPost("{id}/expire")]
    public async Task<ActionResult<ApiResponse>> MarkAsExpired(Guid id)
    {
        try
        {
            await _documentService.MarkAsExpiredAsync(id);
            return Ok(ApiResponse.Success("Document marked as expired"));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking document as expired");
            return StatusCode(500, ApiResponse.Error("An error occurred"));
        }
    }
}
```

#### **Step 7: Register Services (1 minute)**

The dynamic registration in `ServiceCollectionExtensions.cs` will automatically pick up your new interfaces and implementations! Just ensure:

1. Interface name: `IDocumentService`
2. Class name: `DocumentService` (implements `IDocumentService`)
3. Both are in the correct assemblies

The reflection-based registration will automatically register them!

#### **Step 8: Update DbContext (2 minutes)**

```csharp
// IIROSA.Infrastructure/Data/ApplicationDbContext.cs
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // ... existing DbSets ...

    // Add new DbSets
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentCategory> DocumentCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ... existing configurations ...

        // Document Configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Documents");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired();
            entity.Property(e => e.FileSize).IsRequired();
            entity.Property(e => e.MimeType).IsRequired();
            
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasOne(d => d.Category)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(d => d.Charity)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.FK_CharityId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(d => d.Orphan)
                .WithMany(o => o.Documents)
                .HasForeignKey(d => d.FK_OrphanId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(d => d.Family)
                .WithMany(f => f.Documents)
                .HasForeignKey(d => d.FK_FamilyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DocumentCategory>(entity =>
        {
            entity.ToTable("DocumentCategories");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            
            entity.HasIndex(e => e.Name).IsUnique();
        });
    }
}
```

#### **Step 9: Update UnitOfWork (1 minute)**

```csharp
// IIROSA.Infrastructure/Data/UnitOfWork/UnitOfWork.cs
public class UnitOfWork : IUnitOfWork
{
    // ... existing repositories ...

    public IDocumentRepository Documents =>
        GetRepository<IDocumentRepository, DocumentRepository>();

    // ... rest of UnitOfWork ...
}
```

And update the interface:

```csharp
// IIROSA.Domain/Interfaces/IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    // ... existing repositories ...

    IDocumentRepository Documents { get; }

    // ... rest of interface ...
}
```

---

## Module Creation Checklist

Use this checklist to ensure you don't miss any steps:

### **Domain Layer**
- [ ] Create entity class(es)
- [ ] Implement `IAuditable` interface
- [ ] Add navigation properties
- [ ] Create repository interface in `IIROSA.Domain/Interfaces`

### **Infrastructure Layer**
- [ ] Create repository implementation in `IIROSA.Infrastructure/Data/Repository`
- [ ] Add to `ApplicationDbContext` (DbSet + OnModelCreating)
- [ ] Add to `UnitOfWork` and `IUnitOfWork`

### **Application Layer**
- [ ] Create DTOs (Dto, CreateDto, UpdateDto, FilterDto)
- [ ] Create service interface in `IIROSA.Application/Interfaces`
- [ ] Create service implementation in `IIROSA.Application/Services`
- [ ] Add mapping to `MappingProfile`
- [ ] Create FluentValidation validator (optional)

### **Presentation Layer**
- [ ] Create controller in `IIROSA.Web/Controllers/Api/V1`
- [ ] Inherit from `BaseController<TDto, TEntity>`
- [ ] Add all endpoint methods
- [ ] Add Swagger XML comments (optional)

### **Testing**
- [ ] Create unit tests for repository
- [ ] Create unit tests for service
- [ ] Create integration tests for controller

### **Documentation**
- [ ] Create/update use case document
- [ ] Add API documentation
- [ ] Update README

---

## Module Creation Template

Save this as a template for rapid module creation:

### **Entity Template**

```csharp
// [Entity].cs
public class [Entity] : IAuditable
{
    public Guid Id { get; set; }
    
    // Add your properties here
    
    // Navigation Properties
    // public virtual [RelatedEntity] [RelatedProperty] { get; set; }

    // IAuditable
    public DateTime CreatedDate { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
    public Guid? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
}
```

### **Repository Interface Template**

```csharp
// I[Entity]Repository.cs
public interface I[Entity]Repository : IRepository<[Entity]>
{
    Task<IEnumerable<[Entity]>> Get[Entity]By[Criteria]Async([Type] [Property]Id);
    Task<[Entity]?> Get[Entity]With[Details]Async(Guid id);
}
```

### **Repository Implementation Template**

```csharp
// [Entity]Repository.cs
public class [Entity]Repository : Repository<[Entity]>, I[Entity]Repository
{
    public [Entity]Repository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<[Entity]>> Get[Entity]By[Criteria]Async([Type] [Property]Id)
    {
        return await _context.[Entities]
            .Include(e => e.[Navigation])
            .Where(e => e.[Property]Id == [Property]Id && !e.IsDeleted)
            .OrderByDescending(e => e.CreatedDate)
            .ToListAsync();
    }

    public async Task<[Entity]?> Get[Entity]With[Details]Async(Guid id)
    {
        return await _context.[Entities]
            .Include(e => e.[Navigation1])
            .Include(e => e.[Navigation2])
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}
```

### **Service Interface Template**

```csharp
// I[Entity]Service.cs
public interface I[Entity]Service : IService<[Entity]Dto, [Entity]>
{
    Task<IEnumerable<[Entity]Dto>> Get[Entity]By[Criteria]Async([Type] [Property]Id);
    Task<[Entity]Dto> [CustomAction]Async(Guid id, [CustomAction]Dto dto);
}
```

### **Service Implementation Template**

```csharp
// [Entity]Service.cs
public class [Entity]Service : Service<[Entity]Dto, [Entity], I[Entity]Repository>, I[Entity]Service
{
    private readonly I[Entity]Repository _[entity]Repository;

    public [Entity]Service(
        I[Entity]Repository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<[Entity]Service> logger)
        : base(repository, unitOfWork, mapper, logger)
    {
        _[entity]Repository = repository;
    }

    public async Task<IEnumerable<[Entity]Dto>> Get[Entity]By[Criteria]Async([Type] [Property]Id)
    {
        var entities = await _[entity]Repository.Get[Entity]By[Criteria]Async([Property]Id);
        return _mapper.Map<IEnumerable<[Entity]Dto>>(entities);
    }

    public async Task<[Entity]Dto> [CustomAction]Async(Guid id, [CustomAction]Dto dto)
    {
        var entity = await _[entity]Repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException(nameof([Entity]), id);

        // Perform custom action
        entity.[Property] = dto.[Property];

        _[entity]Repository.Update(entity);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<[Entity]Dto>(entity);
    }
}
```

### **Controller Template**

```csharp
// [Entity]sController.cs
[ApiVersion("1.0")]
public class [Entity]sController : BaseController<[Entity]Dto, [Entity]>
{
    private readonly I[Entity]Service _[entity]Service;

    public [Entity]sController(I[Entity]Service [entity]Service, ILogger<[Entity]sController> logger)
        : base([entity]Service, logger)
    {
        _[entity]Service = [entity]Service;
    }

    [HttpGet("[custom-action]")]
    public async Task<ActionResult<ApiResponse<IEnumerable<[Entity]Dto>>>> Get[CustomAction]([Type] [Property]Id)
    {
        try
        {
            var results = await _[entity]Service.Get[Entity]By[Criteria]Async([Property]Id);
            return Ok(ApiResponse<IEnumerable<[Entity]Dto>>.Success(results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing [custom action]");
            return StatusCode(500, ApiResponse<IEnumerable<[Entity]Dto>>.Error("An error occurred"));
        }
    }

    [HttpPost("{id}/[custom-action]")]
    public async Task<ActionResult<ApiResponse<[Entity]Dto>>> [CustomAction](Guid id, [FromBody] [CustomAction]Dto dto)
    {
        try
        {
            var result = await _[entity]Service.[CustomAction]Async(id, dto);
            return Ok(ApiResponse<[Entity]Dto>.Success(result, "[Action] successful"));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<[Entity]Dto>.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing [custom action]");
            return StatusCode(500, ApiResponse<[Entity]Dto>.Error("An error occurred"));
        }
    }
}
```

---

## Time Estimates for Module Creation

| Task | Time | Notes |
|------|------|-------|
| Create Domain Entity | 2 min | Copy template, add properties |
| Create Repository Interface | 2 min | Define domain-specific methods |
| Create Repository Implementation | 3 min | Implement query methods |
| Create DTOs | 3 min | Dto, CreateDto, UpdateDto |
| Create Service Interface | 2 min | Define business logic methods |
| Create Service Implementation | 10 min | Implement business logic + validation |
| Update AutoMapper | 2 min | Add mapping profile |
| Create Controller | 5 min | Implement API endpoints |
| Update DbContext | 2 min | Add DbSet + configuration |
| Update UnitOfWork | 1 min | Add repository property |
| **Total** | **~30 minutes** | For a simple CRUD module |

Complex modules with many relationships may take 45-60 minutes.

---

## Common Patterns & Best Practices

### **1. Always Include Navigation Properties in Queries**

```csharp
// GOOD - Includes related data
var report = await _context.PeriodicReports
    .Include(pr => pr.Child)
    .Include(pr => pr.Child.Charity)
    .FirstOrDefaultAsync(pr => pr.Id == id);

// AVOID - Missing includes, causes N+1 queries
var report = await _context.PeriodicReports
    .FirstOrDefaultAsync(pr => pr.Id == id);
```

### **2. Use Specification Pattern for Complex Queries**

```csharp
// ISpecification.cs
public interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
}

// Example: PendingReportsSpecification.cs
public class PendingReportsSpecification : ISpecification<PeriodicReport>
{
    public Expression<Func<PeriodicReport, bool>> ToExpression()
    {
        return pr => !pr.Reviewed.HasValue && pr.Active == true && !pr.IsDeleted;
    }
}

// Usage
var spec = new PendingReportsSpecification();
var reports = await _repository.FindAsync(spec.ToExpression());
```

### **3. Always Validate in Service Layer**

```csharp
public override async Task<ValidationResult> ValidateAsync(TDto dto)
{
    var errors = new List<string>();

    // Business validation
    if (string.IsNullOrEmpty(dto.Name))
        errors.Add("Name is required");

    if (dto.StartDate > dto.EndDate)
        errors.Add("Start date must be before end date");

    return new ValidationResult(errors);
}
```

### **4. Use Transactions for Multi-Step Operations**

```csharp
public async Task TransferOrphanAsync(Guid orphanId, Guid newFamilyId)
{
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        var orphan = await _orphanRepository.GetByIdAsync(orphanId);
        orphan.FK_FamilyId = newFamilyId;
        
        _orphanRepository.Update(orphan);
        
        // Log transfer
        await _auditLogService.LogTransferAsync(orphanId, newFamilyId);
        
        await _unitOfWork.CommitTransactionAsync();
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

### **5. Handle Soft Deletes Properly**

```csharp
// Always include !IsDeleted in queries
.Where(o => !o.IsDeleted)

// For hard delete (rare, use with caution)
_context.Database.ExecuteSqlRaw("DELETE FROM Entities WHERE Id = @id", new SqlParameter("@id", id));
```

### **6. Use AutoMapper Configuration for Complex Mappings**

```csharp
CreateMap<PeriodicReport, PeriodicReportDto>()
    .ForMember(dest => dest.OrphanName, opt => opt.MapFrom(src => src.Child != null ? src.Child.FullName : null))
    .ForMember(dest => dest.CharityName, opt => opt.MapFrom(src => src.Child?.Charity != null ? src.Child.Charity.Name : null))
    .ForMember(dest => dest.ReviewerName, opt => opt.MapFrom(src => src.ReviewerUser != null ? $"{src.ReviewerUser.FirstName} {src.ReviewerUser.LastName}" : null));
```

---

## Troubleshooting Common Issues

### **Issue: "Cannot resolve service for type"**
**Solution:** Ensure:
1. Interface and class names follow convention: `IModuleService` and `ModuleService`
2. Both are in correct assemblies (Application for interface, Application for implementation)
3. Dynamic registration is enabled in `ServiceCollectionExtensions`

### **Issue: "Navigation property not loading"**
**Solution:** Use `.Include()` in repository:
```csharp
.Include(e => e.RelatedEntity)
```

### **Issue: "Circular reference in JSON serialization"**
**Solution:** Add to `Startup.cs`:
```csharp
services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
```

### **Issue: "Validation not working"**
**Solution:** Register FluentValidation:
```csharp
services.AddValidators(); // Called in ServiceCollectionExtensions
```

---

## Next Steps

1. **Create your first module** using this Quick Start Guide
2. **Test the module** with Postman or Swagger UI
3. **Add unit tests** for repository and service
4. **Document the module** with use cases and API docs

Need help creating a specific module? Just let me know the module name and requirements, and I'll generate all the code for you!
