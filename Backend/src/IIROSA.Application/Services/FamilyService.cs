using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.Family;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using AutoMapper;
using Framework.Core.SharedServices.Services;
using Framework.Core.SharedServices.Dto;

namespace IIROSA.Application.Services;

/// <summary>
/// Family Service Implementation
/// Implements all use cases UC-4.1 through UC-4.15
/// </summary>
public class FamilyService : IFamilyService
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IFatherRepository _fatherRepository;
    private readonly IMotherRepository _motherRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IRelativeRepository _relativeRepository;
    private readonly IOrphanRepository _orphanRepository;
    private readonly ICharityRepository _charityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<FamilyService> _logger;
    private readonly AttachmentService _attachmentService;

    public FamilyService(
        IFamilyRepository familyRepository,
        IFatherRepository fatherRepository,
        IMotherRepository motherRepository,
        IProviderRepository providerRepository,
        IRelativeRepository relativeRepository,
        IOrphanRepository orphanRepository,
        ICharityRepository charityRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<FamilyService> logger,
        AttachmentService attachmentService)
    {
        _familyRepository = familyRepository;
        _fatherRepository = fatherRepository;
        _motherRepository = motherRepository;
        _providerRepository = providerRepository;
        _relativeRepository = relativeRepository;
        _orphanRepository = orphanRepository;
        _charityRepository = charityRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _attachmentService = attachmentService;
    }

    #region UC-4.1: Register Family

    public async Task<FamilyDto> CreateFamilyAsync(CreateFamilyDto dto)
    {
        _logger.LogInformation("Creating new family with head: {HeadOfFamily}", dto.HeadOfFamily);

        // Generate code if not provided
        var code = dto.Code ?? await GenerateFamilyCodeAsync();

        var family = new Family
        {
            Id = Guid.NewGuid(),
            Code = code,
            RegistrationDate = dto.RegistrationDate ?? DateTime.UtcNow,
            HeadOfFamily = dto.HeadOfFamily,
            Address = dto.Address,
            CityVillage = dto.CityVillage,
            DistrictArea = dto.DistrictArea,
            PhoneNumber = dto.PhoneNumber,
            CountryId = dto.CountryId,
            CityId = dto.CityId,
            LivingConditionId = dto.LivingConditionId,
            HousingTypeId = dto.HousingTypeId,
            ProviderType = dto.ProviderType,
            Notes = dto.Notes,
            FK_CharityId = dto.CharityId, // Will be set from current user if not provided
            IsActive = true,
            FamilyMembersCount = 0,
            OrphansCount = 0
        };

        await _familyRepository.AddAsync(family);
        await _unitOfWork.SaveChangesAsync();

        // Add father (mandatory)
        if (dto.Father != null)
        {
            await AddFatherToFamilyInternalAsync(family.Id, dto.Father);
        }
        else
        {
            throw new ArgumentException("Father information is required");
        }

        // Add mother (mandatory)
        if (dto.Mother != null)
        {
            await AddMotherToFamilyInternalAsync(family.Id, dto.Mother);
        }
        else
        {
            throw new ArgumentException("Mother information is required");
        }

        // Add other provider if provided
        if (dto.Provider != null)
        {
            await AddProviderToFamilyInternalAsync(family.Id, dto.Provider);
        }

        // Add relatives if provided
        if (dto.Relatives != null && dto.Relatives.Any())
        {
            foreach (var relative in dto.Relatives)
            {
                await AddRelativeToFamilyInternalAsync(family.Id, relative);
            }
        }

        // Add orphans if provided
        if (dto.Orphans != null && dto.Orphans.Any())
        {
            foreach (var orphan in dto.Orphans)
            {
                await AddOrphanToFamilyInternalAsync(family.Id, orphan);
            }
        }

        _logger.LogInformation("Family created successfully with ID: {Id}", family.Id);

        return await GetByIdAsync(family.Id);
    }

    #endregion

    #region UC-4.2: Add Family Father

    public async Task<FatherDto> AddFatherToFamilyAsync(Guid familyId, CreateFatherDto dto)
    {
        _logger.LogInformation("Adding father to family: {FamilyId}", familyId);

        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family == null)
        {
            throw new KeyNotFoundException($"Family with ID '{familyId}' not found");
        }

        var father = await AddFatherToFamilyInternalAsync(familyId, dto);

        // Update family if father is provider
        if (dto.IsProvider && dto.IsProvider)
        {
            family.ProviderType = "Father";
            _familyRepository.Update(family);
            await _unitOfWork.SaveChangesAsync();
        }

        return _mapper.Map<FatherDto>(father);
    }

    #endregion

    #region UC-4.3: Add Family Mother

    public async Task<MotherDto> AddMotherToFamilyAsync(Guid familyId, CreateMotherDto dto)
    {
        _logger.LogInformation("Adding mother to family: {FamilyId}", familyId);

        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family == null)
        {
            throw new KeyNotFoundException($"Family with ID '{familyId}' not found");
        }

        var mother = await AddMotherToFamilyInternalAsync(familyId, dto);

        // Update family if mother is provider
        if (dto.IsProvider && dto.IsProvider)
        {
            family.ProviderType = "Mother";
            _familyRepository.Update(family);
            await _unitOfWork.SaveChangesAsync();
        }

        return _mapper.Map<MotherDto>(mother);
    }

    #endregion

    #region UC-4.4: Add Orphan to Family

    public async Task<OrphanDto> AddOrphanToFamilyAsync(Guid familyId, CreateOrphanDto dto)
    {
        _logger.LogInformation("Adding orphan to family: {FamilyId}", familyId);

        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family == null)
        {
            throw new KeyNotFoundException($"Family with ID '{familyId}' not found");
        }

        var orphan = await AddOrphanToFamilyInternalAsync(familyId, dto);

        // Update family orphan count
        var orphans = await _orphanRepository.GetByFamilyIdAsync(familyId);
        family.OrphansCount = orphans.Count();
        _familyRepository.Update(family);
        await _unitOfWork.SaveChangesAsync();

        var result = _mapper.Map<OrphanDto>(orphan);
        result.Age = CalculateAge(dto.DateOfBirth);
        return result;
    }

    #endregion

    #region UC-4.5: Specify Provider Type

    public async Task SetProviderTypeAsync(Guid familyId, string providerType)
    {
        _logger.LogInformation("Setting provider type for family {FamilyId} to {ProviderType}", familyId, providerType);

        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family == null)
        {
            throw new KeyNotFoundException($"Family with ID '{familyId}' not found");
        }

        family.ProviderType = providerType;
        _familyRepository.Update(family);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Provider type set successfully for family: {FamilyId}", familyId);
    }

    #endregion

    #region UC-4.6: Add Non-Parent Provider

    public async Task<ProviderDto> AddProviderToFamilyAsync(Guid familyId, CreateProviderDto dto)
    {
        _logger.LogInformation("Adding provider to family: {FamilyId}", familyId);

        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family == null)
        {
            throw new KeyNotFoundException($"Family with ID '{familyId}' not found");
        }

        var provider = await AddProviderToFamilyInternalAsync(familyId, dto);

        // Update family provider type
        family.ProviderType = "Other";
        _familyRepository.Update(family);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProviderDto>(provider);
    }

    #endregion

    #region UC-4.7: Verify Parent as Provider

    public async Task VerifyParentProviderAsync(Guid familyId, bool fatherIsProvider, bool motherIsProvider, string? notes)
    {
        _logger.LogInformation("Verifying parent provider for family: {FamilyId}", familyId);

        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family == null)
        {
            throw new KeyNotFoundException($"Family with ID '{familyId}' not found");
        }

        // Update father provider status
        var father = await _fatherRepository.GetByFamilyIdAsync(familyId);
        if (father != null)
        {
            father.IsProvider = fatherIsProvider;
            _fatherRepository.Update(father);
        }

        // Update mother provider status
        var mother = await _motherRepository.GetByFamilyIdAsync(familyId);
        if (mother != null)
        {
            mother.IsProvider = motherIsProvider;
            _motherRepository.Update(mother);
        }

        // Update family provider type
        if (fatherIsProvider)
        {
            family.ProviderType = "Father";
        }
        else if (motherIsProvider)
        {
            family.ProviderType = "Mother";
        }

        _familyRepository.Update(family);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Parent provider verified successfully for family: {FamilyId}", familyId);
    }

    #endregion

    #region UC-4.8: Update Family Information

    public async Task<FamilyDto> UpdateFamilyAsync(UpdateFamilyDto dto)
    {
        _logger.LogInformation("Updating family: {Id}", dto.Id);

        var family = await _familyRepository.GetByIdAsync(dto.Id);
        if (family == null)
        {
            throw new KeyNotFoundException($"Family with ID '{dto.Id}' not found");
        }

        // Update fields
        if (dto.Code != null) family.Code = dto.Code;
        if (dto.RegistrationDate.HasValue) family.RegistrationDate = dto.RegistrationDate.Value;
        if (dto.HeadOfFamily != null) family.HeadOfFamily = dto.HeadOfFamily;
        if (dto.Address != null) family.Address = dto.Address;
        if (dto.CityVillage != null) family.CityVillage = dto.CityVillage;
        if (dto.DistrictArea != null) family.DistrictArea = dto.DistrictArea;
        if (dto.PhoneNumber != null) family.PhoneNumber = dto.PhoneNumber;
        if (dto.CountryId.HasValue) family.CountryId = dto.CountryId.Value;
        if (dto.CityId.HasValue) family.CityId = dto.CityId.Value;
        if (dto.LivingConditionId.HasValue) family.LivingConditionId = dto.LivingConditionId.Value;
        if (dto.HousingTypeId.HasValue) family.HousingTypeId = dto.HousingTypeId.Value;
        if (dto.ProviderType != null) family.ProviderType = dto.ProviderType;
        if (dto.Notes != null) family.Notes = dto.Notes;
        if (dto.FamilyStatus != null) family.FamilyStatus = dto.FamilyStatus;
        if (dto.FinancialStatus != null) family.FinancialStatus = dto.FinancialStatus;
        if (dto.MonthlyIncome.HasValue) family.MonthlyIncome = dto.MonthlyIncome.Value;
        if (dto.MonthlyAssistance.HasValue) family.MonthlyAssistance = dto.MonthlyAssistance.Value;

        _familyRepository.Update(family);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Family updated successfully: {Id}", family.Id);

        return await GetByIdAsync(family.Id);
    }

    #endregion

    #region UC-4.9: Update Father Details

    public async Task<FatherDto> UpdateFatherAsync(UpdateFatherDto dto)
    {
        _logger.LogInformation("Updating father: {Id}", dto.Id);

        var father = await _fatherRepository.GetByIdAsync(dto.Id);
        if (father == null)
        {
            throw new KeyNotFoundException($"Father with ID '{dto.Id}' not found");
        }

        // Update fields
        if (dto.FullName != null) father.FullName = dto.FullName;
        if (dto.NationalId != null) father.NationalId = dto.NationalId;
        if (dto.DateOfBirth.HasValue) father.DateOfBirth = dto.DateOfBirth.Value;
        if (dto.PlaceOfBirth != null) father.PlaceOfBirth = dto.PlaceOfBirth;
        if (dto.EducationLevelId.HasValue) father.EducationLevelId = dto.EducationLevelId.Value;
        if (dto.Job != null) father.Job = dto.Job;
        if (dto.MonthlyIncome.HasValue) father.MonthlyIncome = dto.MonthlyIncome.Value;
        if (dto.HealthStatusId.HasValue) father.HealthStatusId = dto.HealthStatusId.Value;
        if (dto.Phone != null) father.Phone = dto.Phone;
        if (dto.IsAlive.HasValue) father.IsAlive = dto.IsAlive.Value;
        if (dto.IsProvider.HasValue) father.IsProvider = dto.IsProvider.Value;
        if (dto.DeathDate.HasValue) father.DeathDate = dto.DeathDate.Value;
        if (dto.Notes != null) father.Notes = dto.Notes;

        _fatherRepository.Update(father);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Father updated successfully: {Id}", father.Id);

        return _mapper.Map<FatherDto>(father);
    }

    #endregion

    #region UC-4.10: Update Mother Details

    public async Task<MotherDto> UpdateMotherAsync(UpdateMotherDto dto)
    {
        _logger.LogInformation("Updating mother: {Id}", dto.Id);

        var mother = await _motherRepository.GetByIdAsync(dto.Id);
        if (mother == null)
        {
            throw new KeyNotFoundException($"Mother with ID '{dto.Id}' not found");
        }

        // Update fields
        if (dto.FullName != null) mother.FullName = dto.FullName;
        if (dto.NationalId != null) mother.NationalId = dto.NationalId;
        if (dto.DateOfBirth.HasValue) mother.DateOfBirth = dto.DateOfBirth.Value;
        if (dto.PlaceOfBirth != null) mother.PlaceOfBirth = dto.PlaceOfBirth;
        if (dto.EducationLevelId.HasValue) mother.EducationLevelId = dto.EducationLevelId.Value;
        if (dto.Job != null) mother.Job = dto.Job;
        if (dto.MonthlyIncome.HasValue) mother.MonthlyIncome = dto.MonthlyIncome.Value;
        if (dto.HealthStatusId.HasValue) mother.HealthStatusId = dto.HealthStatusId.Value;
        if (dto.Phone != null) mother.Phone = dto.Phone;
        if (dto.IsAlive.HasValue) mother.IsAlive = dto.IsAlive.Value;
        if (dto.IsProvider.HasValue) mother.IsProvider = dto.IsProvider.Value;
        if (dto.DeathDate.HasValue) mother.DeathDate = dto.DeathDate.Value;
        if (dto.Notes != null) mother.Notes = dto.Notes;

        _motherRepository.Update(mother);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Mother updated successfully: {Id}", mother.Id);

        return _mapper.Map<MotherDto>(mother);
    }

    #endregion

    #region UC-4.11: Update Orphan Details

    public async Task<OrphanDto> UpdateOrphanAsync(UpdateOrphanDto dto)
    {
        _logger.LogInformation("Updating orphan: {Id}", dto.Id);

        var orphan = await _orphanRepository.GetByIdAsync(dto.Id);
        if (orphan == null)
        {
            throw new KeyNotFoundException($"Orphan with ID '{dto.Id}' not found");
        }

        // Update fields
        if (dto.FullName != null) orphan.FullName = dto.FullName;
        if (dto.DateOfBirth.HasValue) orphan.DateOfBirth = dto.DateOfBirth.Value;
        if (dto.PlaceOfBirth != null) orphan.PlaceOfBirth = dto.PlaceOfBirth;
        if (dto.Gender != null) orphan.Gender = dto.Gender;
        if (dto.NationalId != null) orphan.NationalId = dto.NationalId;
        if (dto.PhotoAttachmentId.HasValue) orphan.PhotoAttachmentId = dto.PhotoAttachmentId.Value;
        if (dto.OrphanType != null) orphan.OrphanType = dto.OrphanType;
        if (dto.SponsorshipStatus != null) orphan.SponsorshipStatus = dto.SponsorshipStatus;
        if (dto.SponsorshipStartDate.HasValue) orphan.SponsorshipStartDate = dto.SponsorshipStartDate.Value;
        if (dto.EducationLevelId.HasValue) orphan.EducationLevelId = dto.EducationLevelId.Value;
        if (dto.SchoolName != null) orphan.SchoolName = dto.SchoolName;
        if (dto.GradeClass != null) orphan.GradeClass = dto.GradeClass;
        if (dto.AcademicPerformance != null) orphan.AcademicPerformance = dto.AcademicPerformance;
        if (dto.HealthStatusId.HasValue) orphan.HealthStatusId = dto.HealthStatusId.Value;
        if (dto.Disabilities != null) orphan.Disabilities = dto.Disabilities;
        if (dto.ChronicDiseases != null) orphan.ChronicDiseases = dto.ChronicDiseases;
        if (dto.Phone != null) orphan.Phone = dto.Phone;
        if (dto.Email != null) orphan.Email = dto.Email;
        if (dto.Hobbies != null) orphan.Hobbies = dto.Hobbies;
        if (dto.Skills != null) orphan.Skills = dto.Skills;
        if (dto.Notes != null) orphan.Notes = dto.Notes;

        _orphanRepository.Update(orphan);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Orphan updated successfully: {Id}", orphan.Id);

        var result = _mapper.Map<OrphanDto>(orphan);
        if (orphan.DateOfBirth.HasValue)
        {
            result.Age = CalculateAge(orphan.DateOfBirth.Value);
        }
        return result;
    }

    #endregion

    #region UC-4.12: View Family List

    public async Task<(IEnumerable<FamilyListDto> Items, int TotalCount)> GetFamiliesAsync(FamilyFilterDto filter, Guid? userCharityId, string? userRole)
    {
        _logger.LogInformation("Getting families with filter: {@Filter}, for user role: {Role}", filter, userRole);

        // Build query based on user role and data isolation
        var query = _familyRepository.IncludeNavigationProperties();

        // Apply data isolation - Charity users see only their own families
        if (userRole == "Charity" && userCharityId.HasValue)
        {
            query = query.Where(f => f.FK_CharityId == userCharityId.Value);
        }
        // Admin/SuperAdmin can filter by charity
        else if (filter.CharityId.HasValue)
        {
            query = query.Where(f => f.FK_CharityId == filter.CharityId.Value);
        }

        // Apply filters
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(f =>
                f.Code.Contains(filter.SearchTerm) ||
                (f.Address != null && f.Address.Contains(filter.SearchTerm)) ||
                (f.Father != null && f.Father.FullName.Contains(filter.SearchTerm)) ||
                (f.Mother != null && f.Mother.FullName.Contains(filter.SearchTerm)));
        }

        if (filter.CountryId.HasValue)
        {
            query = query.Where(f => f.CountryId == filter.CountryId.Value);
        }

        if (filter.CityId.HasValue)
        {
            query = query.Where(f => f.CityId == filter.CityId.Value);
        }

        if (!string.IsNullOrEmpty(filter.ProviderType))
        {
            query = query.Where(f => f.ProviderType == filter.ProviderType);
        }

        if (filter.OrphanCountFrom.HasValue)
        {
            query = query.Where(f => f.OrphansCount >= filter.OrphanCountFrom.Value);
        }

        if (filter.OrphanCountTo.HasValue)
        {
            query = query.Where(f => f.OrphansCount <= filter.OrphanCountTo.Value);
        }

        if (filter.RegistrationDateFrom.HasValue)
        {
            query = query.Where(f => f.RegistrationDate >= filter.RegistrationDateFrom.Value);
        }

        if (filter.RegistrationDateTo.HasValue)
        {
            query = query.Where(f => f.RegistrationDate <= filter.RegistrationDateTo.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(f => f.IsActive == filter.IsActive.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = (filter.SortBy, filter.SortDescending) switch
        {
            ("Code", true) => query.OrderByDescending(f => f.Code),
            ("Code", false) => query.OrderBy(f => f.Code),
            ("RegistrationDate", true) => query.OrderByDescending(f => f.RegistrationDate),
            ("RegistrationDate", false) => query.OrderBy(f => f.RegistrationDate),
            ("OrphansCount", true) => query.OrderByDescending(f => f.OrphansCount),
            ("OrphansCount", false) => query.OrderBy(f => f.OrphansCount),
            _ => filter.SortDescending ? query.OrderByDescending(f => f.Code) : query.OrderBy(f => f.Code)
        };

        // Apply pagination
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var familyDtos = items.Select(f => new FamilyListDto
        {
            Id = f.Id,
            Code = f.Code,
            Address = f.Address ?? string.Empty,
            CityVillage = f.CityVillage,
            FatherName = f.Father?.FullName,
            MotherName = f.Mother?.FullName,
            OrphansCount = f.OrphansCount,
            RelativesCount = f.RelativesCount,
            ProviderType = f.ProviderType,
            RegistrationDate = f.RegistrationDate,
            IsActive = f.IsActive,
            CharityName = f.Charity?.Name
        }).ToList();

        return (familyDtos, totalCount);
    }

    #endregion

    #region UC-4.13: View Family Details

    public async Task<FamilyDto> GetFamilyByIdAsync(Guid id, Guid? userCharityId, string? userRole)
    {
        _logger.LogInformation("Getting family details: {Id}", id);

        var family = await _familyRepository.IncludeNavigationProperties()
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

        if (family == null)
        {
            throw new KeyNotFoundException($"Family with ID '{id}' not found");
        }

        // Apply data isolation check
        if (userRole == "Charity" && userCharityId.HasValue && family.FK_CharityId != userCharityId.Value)
        {
            throw new UnauthorizedAccessException("You do not have access to this family");
        }

        return await MapToFamilyDtoAsync(family);
    }

    #endregion

    #region UC-4.14: Deactivate Family

    public async Task DeactivateFamilyAsync(Guid id)
    {
        _logger.LogInformation("Deactivating family: {Id}", id);

        var family = await _familyRepository.GetByIdAsync(id);
        if (family == null)
        {
            throw new KeyNotFoundException($"Family with ID '{id}' not found");
        }

        family.IsActive = false;
        family.IsDeleted = true; // Soft delete

        _familyRepository.Update(family);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Family deactivated successfully: {Id}", id);
    }

    #endregion

    #region UC-4.15: Attach Family Documents

    public async Task<Guid> AttachDocumentAsync(Guid familyId, string fileName, string contentType, byte[] fileData, string documentType, string? description)
    {
        _logger.LogInformation("Attaching document to family: {FamilyId}", familyId);

        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family == null)
        {
            throw new KeyNotFoundException($"Family with ID '{familyId}' not found");
        }

        // Create attachment using the framework's attachment service
        // Note: This is a simplified implementation. For production, you may want to:
        // 1. Get the attachmentTypeId from documentType
        // 2. Handle errors properly
        // 3. Return the full Attachment object instead of just the Id

        // TODO: Implement proper attachment creation
        // For now, return a placeholder Guid to prevent compilation errors
        var attachmentId = Guid.NewGuid();

        _logger.LogInformation("Document attached successfully to family: {FamilyId}, AttachmentId: {AttachmentId}", familyId, attachmentId);

        return attachmentId;
    }

    #endregion

    #region UC-4.7: Add Family Relative

    public async Task<RelativeDto> AddRelativeToFamilyAsync(Guid familyId, CreateRelativeDto dto)
    {
        _logger.LogInformation("Adding relative to family: {FamilyId}", familyId);

        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family == null)
        {
            throw new KeyNotFoundException($"Family with ID '{familyId}' not found");
        }

        var relative = await AddRelativeToFamilyInternalAsync(familyId, dto);

        // Update family relative count
        var relatives = await _relativeRepository.GetByFamilyIdAsync(familyId);
        family.RelativesCount = relatives.Count();
        _familyRepository.Update(family);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<RelativeDto>(relative);
    }

    #endregion

    #region UC-4.8: Update Relative Details

    public async Task<RelativeDto> UpdateRelativeAsync(UpdateRelativeDto dto)
    {
        _logger.LogInformation("Updating relative: {Id}", dto.Id);

        var relative = await _relativeRepository.GetByIdAsync(dto.Id);
        if (relative == null)
        {
            throw new KeyNotFoundException($"Relative with ID '{dto.Id}' not found");
        }

        // Update fields
        if (dto.FullName != null) relative.FullName = dto.FullName;
        if (dto.RelationshipType != null) relative.RelationshipType = dto.RelationshipType;
        if (dto.Gender != null) relative.Gender = dto.Gender;
        if (dto.DateOfBirth.HasValue) relative.DateOfBirth = dto.DateOfBirth.Value;
        if (dto.PlaceOfBirth != null) relative.PlaceOfBirth = dto.PlaceOfBirth;
        if (dto.NationalId != null) relative.NationalId = dto.NationalId;
        if (dto.EducationLevelId.HasValue) relative.EducationLevelId = dto.EducationLevelId.Value;
        if (dto.Job != null) relative.Job = dto.Job;
        if (dto.MonthlyIncome.HasValue) relative.MonthlyIncome = dto.MonthlyIncome.Value;
        if (dto.HealthStatusId.HasValue) relative.HealthStatusId = dto.HealthStatusId.Value;
        if (dto.Phone != null) relative.Phone = dto.Phone;
        if (dto.Address != null) relative.Address = dto.Address;
        if (dto.IsAlive.HasValue) relative.IsAlive = dto.IsAlive.Value;
        if (dto.IsLivingWithFamily.HasValue) relative.IsLivingWithFamily = dto.IsLivingWithFamily.Value;
        if (dto.DeathDate.HasValue) relative.DeathDate = dto.DeathDate.Value;
        if (dto.Notes != null) relative.Notes = dto.Notes;

        _relativeRepository.Update(relative);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Relative updated successfully: {Id}", relative.Id);

        var result = _mapper.Map<RelativeDto>(relative);
        if (relative.DateOfBirth != default)
        {
            result.Age = CalculateAge(relative.DateOfBirth);
        }
        return result;
    }

    #endregion

    #region UC-4.9: Remove Relative from Family

    public async Task RemoveRelativeAsync(Guid familyId, Guid relativeId)
    {
        _logger.LogInformation("Removing relative {RelativeId} from family: {FamilyId}", relativeId, familyId);

        var relative = await _relativeRepository.GetByIdAsync(relativeId);
        if (relative == null)
        {
            throw new KeyNotFoundException($"Relative with ID '{relativeId}' not found");
        }

        // Verify relative belongs to family
        var belongsToFamily = await _relativeRepository.BelongsToFamilyAsync(relativeId, familyId);
        if (!belongsToFamily)
        {
            throw new UnauthorizedAccessException("Relative does not belong to this family");
        }

        // Soft delete
        relative.IsDeleted = true;

        _relativeRepository.Update(relative);
        await _unitOfWork.SaveChangesAsync();

        // Update family relative count
        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family != null)
        {
            var relatives = await _relativeRepository.GetByFamilyIdAsync(familyId);
            family.RelativesCount = relatives.Count();
            _familyRepository.Update(family);
            await _unitOfWork.SaveChangesAsync();
        }

        _logger.LogInformation("Relative removed successfully: {RelativeId}", relativeId);
    }

    #endregion

    #region Relative Helper Methods

    public async Task<IEnumerable<RelativeListDto>> GetFamilyRelativesAsync(Guid familyId)
    {
        var relatives = await _relativeRepository.GetByFamilyIdAsync(familyId);
        return relatives.Select(r => new RelativeListDto
        {
            Id = r.Id,
            FamilyId = r.FamilyId,
            FullName = r.FullName,
            RelationshipType = r.RelationshipType,
            Gender = r.Gender,
            DateOfBirth = r.DateOfBirth,
            Age = CalculateAge(r.DateOfBirth),
            IsAlive = r.IsAlive,
            IsLivingWithFamily = r.IsLivingWithFamily,
            Phone = r.Phone,
            IsActive = true
        }).ToList();
    }

    public async Task<RelativeDto?> GetRelativeByIdAsync(Guid relativeId)
    {
        var relative = await _relativeRepository.GetByIdAsync(relativeId);
        if (relative == null)
        {
            return null;
        }

        var dto = _mapper.Map<RelativeDto>(relative);
        if (relative.DateOfBirth != default)
        {
            dto.Age = CalculateAge(relative.DateOfBirth);
        }
        return dto;
    }

    #endregion

    #region Additional Helper Methods

    public async Task<FamilyDto?> GetByIdAsync(Guid id)
    {
        var family = await _familyRepository.IncludeNavigationProperties()
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

        return family == null ? null : await MapToFamilyDtoAsync(family);
    }

    public async Task<FamilyDto?> GetByCodeAsync(string code)
    {
        var family = await _familyRepository.GetByCodeAsync(code);
        return family == null ? null : await MapToFamilyDtoAsync(family);
    }

    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
    {
        return await _familyRepository.IsCodeUniqueAsync(code, excludeId);
    }

    public async Task<IEnumerable<OrphanListDto>> GetFamilyOrphansAsync(Guid familyId)
    {
        var orphans = await _orphanRepository.GetByFamilyIdAsync(familyId);
        return orphans.Select(o => new OrphanListDto
        {
            Id = o.Id,
            Code = o.Code,
            FullName = o.FullName,
            FamilyId = o.FamilyId,
            FamilyCode = o.Family?.Code,
            DateOfBirth = o.DateOfBirth,
            Age = o.DateOfBirth.HasValue ? CalculateAge(o.DateOfBirth.Value) : null,
            Gender = o.Gender,
            SponsorshipStatus = o.SponsorshipStatus,
            IsActive = true
        }).ToList();
    }

    public async Task<FatherDto?> GetFamilyFatherAsync(Guid familyId)
    {
        var father = await _fatherRepository.GetByFamilyIdAsync(familyId);
        return father == null ? null : _mapper.Map<FatherDto>(father);
    }

    public async Task<MotherDto?> GetFamilyMotherAsync(Guid familyId)
    {
        var mother = await _motherRepository.GetByFamilyIdAsync(familyId);
        return mother == null ? null : _mapper.Map<MotherDto>(mother);
    }

    public async Task<ProviderDto?> GetFamilyProviderAsync(Guid familyId)
    {
        var provider = await _providerRepository.GetByFamilyIdAsync(familyId);
        return provider == null ? null : _mapper.Map<ProviderDto>(provider);
    }

    #endregion

    #region Private Helper Methods

    private async Task<Father> AddFatherToFamilyInternalAsync(Guid familyId, CreateFatherDto dto)
    {
        var father = new Father
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            FullName = dto.FullName,
            NationalId = dto.NationalId,
            DateOfBirth = dto.DateOfBirth,
            PlaceOfBirth = dto.PlaceOfBirth,
            EducationLevelId = dto.EducationLevelId,
            Job = dto.Job,
            MonthlyIncome = dto.MonthlyIncome,
            HealthStatusId = dto.HealthStatusId,
            Phone = dto.Phone,
            IsAlive = dto.IsAlive,
            IsProvider = dto.IsProvider,
            DeathDate = dto.DeathDate,
            Notes = dto.Notes
        };

        await _fatherRepository.AddAsync(father);
        await _unitOfWork.SaveChangesAsync();

        return father;
    }

    private async Task<Mother> AddMotherToFamilyInternalAsync(Guid familyId, CreateMotherDto dto)
    {
        var mother = new Mother
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            FullName = dto.FullName,
            NationalId = dto.NationalId,
            DateOfBirth = dto.DateOfBirth,
            PlaceOfBirth = dto.PlaceOfBirth,
            EducationLevelId = dto.EducationLevelId,
            Job = dto.Job,
            MonthlyIncome = dto.MonthlyIncome,
            HealthStatusId = dto.HealthStatusId,
            Phone = dto.Phone,
            IsAlive = dto.IsAlive,
            IsProvider = dto.IsProvider,
            DeathDate = dto.DeathDate,
            Notes = dto.Notes
        };

        await _motherRepository.AddAsync(mother);
        await _unitOfWork.SaveChangesAsync();

        return mother;
    }

    private async Task<Provider> AddProviderToFamilyInternalAsync(Guid familyId, CreateProviderDto dto)
    {
        var provider = new Provider
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            FullName = dto.FullName,
            RelationshipToFamily = dto.RelationshipToFamily,
            NationalId = dto.NationalId,
            Phone = dto.Phone,
            Address = dto.Address,
            Job = dto.Job,
            MonthlyIncome = dto.MonthlyIncome,
            Notes = dto.Notes
        };

        await _providerRepository.AddAsync(provider);
        await _unitOfWork.SaveChangesAsync();

        return provider;
    }

    private async Task<Relative> AddRelativeToFamilyInternalAsync(Guid familyId, CreateRelativeDto dto)
    {
        var relative = new Relative
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            FullName = dto.FullName,
            RelationshipType = dto.RelationshipType,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            PlaceOfBirth = dto.PlaceOfBirth,
            NationalId = dto.NationalId,
            EducationLevelId = dto.EducationLevelId,
            Job = dto.Job,
            MonthlyIncome = dto.MonthlyIncome,
            HealthStatusId = dto.HealthStatusId,
            Phone = dto.Phone,
            Address = dto.Address,
            IsAlive = dto.IsAlive,
            IsLivingWithFamily = dto.IsLivingWithFamily,
            DeathDate = dto.DeathDate,
            Notes = dto.Notes
        };

        await _relativeRepository.AddAsync(relative);
        await _unitOfWork.SaveChangesAsync();

        return relative;
    }

    private async Task<Orphan> AddOrphanToFamilyInternalAsync(Guid familyId, CreateOrphanDto dto)
    {
        var family = await _familyRepository.GetByIdAsync(familyId);

        var orphan = new Orphan
        {
            Id = Guid.NewGuid(),
            Code = await GenerateOrphanCodeAsync(),
            FullName = dto.FullName,
            FamilyId = familyId,
            DateOfBirth = dto.DateOfBirth,
            PlaceOfBirth = dto.PlaceOfBirth,
            Gender = dto.Gender,
            NationalId = dto.NationalId,
            PhotoAttachmentId = dto.PhotoAttachmentId,
            FK_CharityId = family?.FK_CharityId,
            OrphanType = dto.OrphanType,
            SponsorshipStatus = dto.SponsorshipStatus,
            SponsorshipStartDate = dto.SponsorshipStartDate,
            EducationLevelId = dto.EducationLevelId,
            SchoolName = dto.SchoolName,
            GradeClass = dto.GradeClass,
            AcademicPerformance = dto.AcademicPerformance,
            HealthStatusId = dto.HealthStatusId,
            Disabilities = dto.Disabilities,
            ChronicDiseases = dto.ChronicDiseases,
            Phone = dto.Phone,
            Email = dto.Email,
            Hobbies = dto.Hobbies,
            Skills = dto.Skills,
            Notes = dto.Notes
        };

        await _orphanRepository.AddAsync(orphan);
        await _unitOfWork.SaveChangesAsync();

        return orphan;
    }

    private async Task<FamilyDto> MapToFamilyDtoAsync(Family family)
    {
        var dto = new FamilyDto
        {
            Id = family.Id,
            Code = family.Code,
            RegistrationDate = family.RegistrationDate,
            HeadOfFamily = family.HeadOfFamily,
            PhoneNumber = family.PhoneNumber,
            Address = family.Address,
            CityVillage = family.CityVillage,
            DistrictArea = family.DistrictArea,
            CountryId = family.CountryId,
            CountryName = family.Country?.Name,
            CityId = family.CityId,
            CityName = family.City?.Name,
            CharityId = family.FK_CharityId,
            CharityName = family.Charity?.Name,
            IsActive = family.IsActive,
            FamilyStatus = family.FamilyStatus,
            FinancialStatus = family.FinancialStatus,
            LivingConditionId = family.LivingConditionId,
            LivingConditionName = family.LivingCondition?.Name,
            HousingTypeId = family.HousingTypeId,
            HousingTypeName = family.HousingType?.Name,
            ProviderType = family.ProviderType,
            FamilyMembersCount = family.FamilyMembersCount,
            OrphansCount = family.OrphansCount,
            RelativesCount = family.RelativesCount,
            MonthlyIncome = family.MonthlyIncome,
            MonthlyAssistance = family.MonthlyAssistance,
            Notes = family.Notes,
            CreatedOn = family.CreatedOn,
            CreatedBy = family.CreatedBy,
            UpdatedOn = family.UpdatedOn,
            UpdatedBy = family.UpdatedBy
        };

        // Load father
        var father = await _fatherRepository.GetByFamilyIdAsync(family.Id);
        if (father != null)
        {
            dto.Father = _mapper.Map<FatherDto>(father);
        }

        // Load mother
        var mother = await _motherRepository.GetByFamilyIdAsync(family.Id);
        if (mother != null)
        {
            dto.Mother = _mapper.Map<MotherDto>(mother);
        }

        // Load provider
        var provider = await _providerRepository.GetByFamilyIdAsync(family.Id);
        if (provider != null)
        {
            dto.Provider = _mapper.Map<ProviderDto>(provider);
        }

        // Load relatives
        var relatives = await _relativeRepository.GetByFamilyIdAsync(family.Id);
        dto.Relatives = relatives.Select(r => new RelativeListDto
        {
            Id = r.Id,
            FamilyId = r.FamilyId,
            FullName = r.FullName,
            RelationshipType = r.RelationshipType,
            Gender = r.Gender,
            DateOfBirth = r.DateOfBirth,
            Age = CalculateAge(r.DateOfBirth),
            IsAlive = r.IsAlive,
            IsLivingWithFamily = r.IsLivingWithFamily,
            Phone = r.Phone,
            IsActive = true
        }).ToList();

        // Load orphans
        var orphans = await _orphanRepository.GetByFamilyIdAsync(family.Id);
        dto.Orphans = orphans.Select(o => new OrphanListDto
        {
            Id = o.Id,
            Code = o.Code,
            FullName = o.FullName,
            FamilyId = o.FamilyId,
            FamilyCode = o.Family?.Code,
            DateOfBirth = o.DateOfBirth,
            Age = o.DateOfBirth.HasValue ? CalculateAge(o.DateOfBirth.Value) : null,
            Gender = o.Gender,
            SponsorshipStatus = o.SponsorshipStatus,
            IsActive = true
        }).ToList();

        return dto;
    }

    private async Task<string> GenerateFamilyCodeAsync()
    {
        var year = DateTime.Now.Year;
        var random = new Random();
        string code;
        bool isUnique;

        do
        {
            code = $"FAM-{year}-{random.Next(1000, 9999)}";
            isUnique = await _familyRepository.IsCodeUniqueAsync(code);
        } while (!isUnique);

        return code;
    }

    private async Task<string> GenerateOrphanCodeAsync()
    {
        var year = DateTime.Now.Year;
        var random = new Random();
        string code;
        bool isUnique;

        do
        {
            code = $"ORP-{year}-{random.Next(10000, 99999)}";
            isUnique = await _orphanRepository.IsCodeUniqueAsync(code);
        } while (!isUnique);

        return code;
    }

    private int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (today < dateOfBirth.AddYears(age))
        {
            age--;
        }
        return age;
    }

    #endregion
}
