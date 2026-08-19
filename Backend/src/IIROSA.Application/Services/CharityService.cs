using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IIROSA.Domain.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.Charity;
using AutoMapper;
using Framework.Core.SharedServices.Services;
using Framework.Core.SharedServices.Dto;
using Framework.Core.SharedServices.Entities;
using Framework.Identity.Data.Services.Interfaces;
using Framework.Identity.Data.Services;
using Framework.Identity.Data.Dtos;
using FluentValidation;
using FluentValidation.Results;

namespace IIROSA.Application.Services;

/// <summary>
/// Charity Service Implementation
/// Implements all use cases UC-3.1 through UC-3.14
/// </summary>
public class CharityService : ICharityService
{
    private readonly ICharityRepository _charityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CharityService> _logger;
    private readonly AttachmentService _attachmentService;
    private readonly IAttachmentHelperService _attachmentHelperService;
    private readonly UserAppService _userAppService;
    private readonly IRoleAppService _roleAppService;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateCharityDto> _createCharityValidator;
    private readonly IValidator<UpdateCharityDto> _updateCharityValidator;

    public CharityService(
        ICharityRepository charityRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CharityService> logger,
        AttachmentService attachmentService,
        IAttachmentHelperService attachmentHelperService,
        UserAppService userAppService,
        IRoleAppService roleAppService,
        ICurrentUserService currentUser,
        IValidator<CreateCharityDto> createCharityValidator,
        IValidator<UpdateCharityDto> updateCharityValidator)
    {
        _charityRepository = charityRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _attachmentService = attachmentService;
        _attachmentHelperService = attachmentHelperService;
        _userAppService = userAppService;
        _roleAppService = roleAppService;
        _currentUser = currentUser;
        _createCharityValidator = createCharityValidator;
        _updateCharityValidator = updateCharityValidator;
    }

    #region UC-3.1: Register Charity

    public async Task<CharityDto> CreateCharityAsync(CreateCharityDto dto)
    {
        _logger.LogInformation("Creating new charity: {Name}", dto.Name);

        // Shape first, then business rules. Running the validator up front means an empty name is
        // reported as an empty name, rather than failing a uniqueness lookup on an empty string.
        await _createCharityValidator.ValidateAndThrowAsync(dto);

        // Uniqueness conflicts are raised against the field that caused them, not as a bare
        // message, so the client can flag that field. (An earlier version of this comment claimed
        // InvalidOperationException produced a 404 — that was wrong: CharitiesController catches it
        // and returns 400. The reason to change was field identity, not the status code.)
        var conflicts = new List<ValidationFailure>();

        if (!await IsNameUniqueAsync(dto.Name))
        {
            conflicts.Add(new ValidationFailure(
                nameof(dto.Name), $"Charity with name '{dto.Name}' already exists"));
        }

        if (!await IsEmailUniqueAsync(dto.Email))
        {
            conflicts.Add(new ValidationFailure(
                nameof(dto.Email), $"Charity with email '{dto.Email}' already exists"));
        }

        if (conflicts.Count > 0)
        {
            throw new ValidationException(conflicts);
        }

        // Generate code if not provided
        var code = dto.Code ?? await GenerateCharityCodeAsync();

        // Process icon attachment
        Guid? iconId = null;
        if (dto.Icon_Attach != null && dto.Icon_Attach.Any())
        {
            var attachment = dto.Icon_Attach.FirstOrDefault(a => !string.IsNullOrEmpty(a.FileName) && a.FileData != null);
            if (attachment != null)
            {
                iconId = await _attachmentHelperService.SaveAttachmentAsync(attachment);
            }
        }

        string? createdPassword = null;

        // Allocated up front because the user account must carry this charity's id as its tenancy
        // claim, and the account is created before the charity row is persisted.
        var charityId = Guid.NewGuid();

        // Create user account if requested
        if (dto.CreateUserAccount)
        {
            var userDto = await CreateUserAccountForCharity(
                dto.Username,
                dto.Password,
                dto.Name,
                charityId,
                dto.CountryId
            );

            if (userDto == null)
            {
                throw new InvalidOperationException("Failed to create user account. The username may already exist.");
            }

            createdPassword = dto.Password;
        }

        var charity = new Charity
        {
            Id = charityId,
            Code = code,
            Name = NormaliseName(dto.Name),
            NGOType = dto.NGOType,
            Address = dto.Address,
            StreetName = dto.StreetName,
            Village = dto.Village,
            PostalCode = dto.PostalCode,
            MailBox = dto.MailBox,
            Phone = dto.Phone,
            Phone2 = dto.Phone2,
            HomePhone = dto.HomePhone,
            Fax = dto.Fax,
            Email = dto.Email,
            CountryId = dto.CountryId,
            RegionId = dto.RegionId,
            CenterId = dto.CenterId,
            NgoMapLocation = dto.NgoMapLocation,
            BankId = dto.BankId,
            BankAccount = dto.BankAccount,
            IBAN = dto.IBAN,
            BossName = dto.BossName,
            BossJobName = dto.BossJobName,
            BossPhone1 = dto.BossPhone1,
            BossPhone2 = dto.BossPhone2,
            ResponsibleJobName = dto.ResponsibleJobName,
            ResponsiblePhone1 = dto.ResponsiblePhone1,
            ResponsiblePhone2 = dto.ResponsiblePhone2,
            IconId = iconId,
            ReceivingDonations = dto.ReceivingDonations,
            Notes = dto.Notes,
            UserId = createdPassword != null ? (await GetUserByUsernameAsync(dto.Username))?.Id.ToString() : null,
            IsActive = true,
            IsAddEnabled = true,
            IsUpdateEnabled = true,
            IsLocked = false
        };

        await _charityRepository.AddAsync(charity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Charity created successfully with ID: {Id}", charity.Id);

        var result = await GetByIdAsync(charity.Id);

        // Include the password in the response only when user account is created
        if (createdPassword != null)
        {
            result.Password = createdPassword;
        }

        return result;
    }

    #endregion

    #region UC-3.2: Update Charity Details

    public async Task<CharityDto> UpdateCharityAsync(UpdateCharityDto dto)
    {
        _logger.LogInformation("Updating charity: {Id}", dto.Id);

        var charity = await _charityRepository.GetByIdAsync(dto.Id);
        if (charity == null)
        {
            throw new KeyNotFoundException($"Charity with ID '{dto.Id}' not found");
        }

        // Validate uniqueness
        // Same shape-then-rules ordering as create, and conflicts raised against the field that
        // caused them so the form can flag it.
        await _updateCharityValidator.ValidateAndThrowAsync(dto);

        var conflicts = new List<ValidationFailure>();

        if (!await IsNameUniqueAsync(dto.Name, dto.Id))
        {
            conflicts.Add(new ValidationFailure(
                nameof(dto.Name), $"Charity with name '{dto.Name}' already exists"));
        }

        if (!await IsEmailUniqueAsync(dto.Email, dto.Id))
        {
            conflicts.Add(new ValidationFailure(
                nameof(dto.Email), $"Charity with email '{dto.Email}' already exists"));
        }

        if (conflicts.Count > 0)
        {
            throw new ValidationException(conflicts);
        }

        // Handle user account creation/update
        string? createdPassword = null;

        // Check if we need to create a new user account
        if (dto.CreateUserAccount == true && string.IsNullOrEmpty(charity.UserId))
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
            {
                throw new InvalidOperationException("Username is required when creating a user account");
            }
            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new InvalidOperationException("Password is required when creating a user account");
            }
            if (dto.Password.Length < 8)
            {
                throw new InvalidOperationException("Password must be at least 8 characters long");
            }

            var userDto = await CreateUserAccountForCharity(
                dto.Username,
                dto.Password,
                dto.Name,
                charity.Id,
                dto.CountryId
            );

            if (userDto == null)
            {
                throw new InvalidOperationException("Failed to create user account. The username may already exist.");
            }

            charity.UserId = userDto.Id.ToString();
            createdPassword = dto.Password;
        }

        // Process icon attachment
        Guid? iconId = charity.IconId; // Keep existing by default
        if (dto.Icon_Attach != null && dto.Icon_Attach.Any())
        {
            var newAttachment = dto.Icon_Attach.FirstOrDefault(a => !string.IsNullOrEmpty(a.FileName) && a.FileData != null && !a.IsDeleted && a.IsNew);
            var deletedAttachment = dto.Icon_Attach.FirstOrDefault(a => a.IsDeleted);

            if (deletedAttachment != null && charity.IconId.HasValue)
            {
                // Delete old attachment
                await _attachmentService.RemoveAsync(charity.IconId.Value);
                iconId = null;
            }

            if (newAttachment != null)
            {
                // Save new attachment
                iconId = await _attachmentHelperService.SaveAttachmentAsync(newAttachment, iconId);
            }
        }

        // Update fields
        charity.Name = NormaliseName(dto.Name);
        charity.NGOType = dto.NGOType;
        charity.Address = dto.Address;
        charity.StreetName = dto.StreetName;
        charity.Village = dto.Village;
        charity.PostalCode = dto.PostalCode;
        charity.MailBox = dto.MailBox;
        charity.Phone = dto.Phone;
        charity.Phone2 = dto.Phone2;
        charity.HomePhone = dto.HomePhone;
        charity.Fax = dto.Fax;
        charity.Email = dto.Email;
        charity.CountryId = dto.CountryId;
        charity.RegionId = dto.RegionId;
        charity.CenterId = dto.CenterId;
        charity.NgoMapLocation = dto.NgoMapLocation;
        charity.BankId = dto.BankId;
        charity.BankAccount = dto.BankAccount;
        charity.IBAN = dto.IBAN;
        charity.BossName = dto.BossName;
        charity.BossJobName = dto.BossJobName;
        charity.BossPhone1 = dto.BossPhone1;
        charity.BossPhone2 = dto.BossPhone2;
        charity.ResponsibleJobName = dto.ResponsibleJobName;
        charity.ResponsiblePhone1 = dto.ResponsiblePhone1;
        charity.ResponsiblePhone2 = dto.ResponsiblePhone2;
        charity.IconId = iconId;
        charity.ReceivingDonations = dto.ReceivingDonations;
        charity.Notes = dto.Notes;

        _charityRepository.Update(charity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Charity updated successfully: {Id}", charity.Id);

        var result = await GetByIdAsync(charity.Id);

        // Include the password in the response only when user account is created
        if (createdPassword != null)
        {
            result.Password = createdPassword;
        }

        return result;
    }

    #endregion

    #region UC-3.3: Activate Charity

    public async Task ActivateCharityAsync(Guid id)
    {
        _logger.LogInformation("Activating charity: {Id}", id);

        var charity = await _charityRepository.GetByIdAsync(id);
        if (charity == null)
        {
            throw new KeyNotFoundException($"Charity with ID '{id}' not found");
        }

        charity.IsActive = true;
        charity.IsLocked = false;

        _charityRepository.Update(charity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Charity activated successfully: {Id}", id);
    }

    #endregion

    #region UC-3.4: Deactivate Charity

    public async Task DeactivateCharityAsync(Guid id)
    {
        _logger.LogInformation("Deactivating charity: {Id}", id);

        var charity = await _charityRepository.GetByIdAsync(id);
        if (charity == null)
        {
            throw new KeyNotFoundException($"Charity with ID '{id}' not found");
        }

        charity.IsActive = false;

        _charityRepository.Update(charity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Charity deactivated successfully: {Id}", id);
    }

    #endregion

    #region UC-3.5: Change Charity Password

    public async Task<string> ResetPasswordAsync(Guid id)
    {
        _logger.LogInformation("Resetting password for charity: {Id}", id);

        var charity = await _charityRepository.GetByIdAsync(id);
        if (charity == null)
        {
            throw new KeyNotFoundException($"Charity with ID '{id}' not found");
        }

        if (string.IsNullOrEmpty(charity.UserId))
        {
            throw new InvalidOperationException($"Charity does not have a linked user account");
        }

        // Generate new password
        var newPassword = GenerateRandomPassword();

        // TODO: Reset password via user management service
        // await _userManagementService.ResetPasswordAsync(charity.UserId, newPassword);

        _logger.LogInformation("Password reset successfully for charity: {Id}", id);

        // TODO: Send email with new password

        return newPassword;
    }

    #endregion

    #region UC-3.6: Enable/Disable Add Rights

    public async Task SetAddRightsAsync(Guid id, bool isEnabled)
    {
        _logger.LogInformation("Setting add rights for charity {Id} to {IsEnabled}", id, isEnabled);

        var charity = await _charityRepository.GetByIdAsync(id);
        if (charity == null)
        {
            throw new KeyNotFoundException($"Charity with ID '{id}' not found");
        }

        charity.IsAddEnabled = isEnabled;

        _charityRepository.Update(charity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Add rights updated successfully for charity: {Id}", id);
    }

    #endregion

    #region UC-3.7: Enable/Disable Update Rights

    public async Task SetUpdateRightsAsync(Guid id, bool isEnabled)
    {
        _logger.LogInformation("Setting update rights for charity {Id} to {IsEnabled}", id, isEnabled);

        var charity = await _charityRepository.GetByIdAsync(id);
        if (charity == null)
        {
            throw new KeyNotFoundException($"Charity with ID '{id}' not found");
        }

        charity.IsUpdateEnabled = isEnabled;

        _charityRepository.Update(charity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Update rights updated successfully for charity: {Id}", id);
    }

    #endregion

    #region UC-3.8: Lock/Unlock Charity

    public async Task LockCharityAsync(Guid id)
    {
        _logger.LogInformation("Locking charity: {Id}", id);

        var charity = await _charityRepository.GetByIdAsync(id);
        if (charity == null)
        {
            throw new KeyNotFoundException($"Charity with ID '{id}' not found");
        }

        charity.IsLocked = true;

        _charityRepository.Update(charity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Charity locked successfully: {Id}", id);
    }

    public async Task UnlockCharityAsync(Guid id)
    {
        _logger.LogInformation("Unlocking charity: {Id}", id);

        var charity = await _charityRepository.GetByIdAsync(id);
        if (charity == null)
        {
            throw new KeyNotFoundException($"Charity with ID '{id}' not found");
        }

        charity.IsLocked = false;

        _charityRepository.Update(charity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Charity unlocked successfully: {Id}", id);
    }

    #endregion

    #region UC-3.9: Set Bank Account Details

    public async Task SetBankDetailsAsync(CharityBankDetailsDto dto)
    {
        _logger.LogInformation("Setting bank details for charity: {CharityId}", dto.CharityId);

        var charity = await _charityRepository.GetByIdAsync(dto.CharityId);
        if (charity == null)
        {
            throw new KeyNotFoundException($"Charity with ID '{dto.CharityId}' not found");
        }

        // Validate IBAN format if provided
        if (!string.IsNullOrWhiteSpace(dto.IBAN) && !IsValidIBAN(dto.IBAN))
        {
            throw new ArgumentException("Invalid IBAN format");
        }

        charity.BankId = dto.BankId;
        charity.BankAccount = dto.BankAccount;
        charity.IBAN = dto.IBAN;

        _charityRepository.Update(charity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Bank details set successfully for charity: {CharityId}", dto.CharityId);
    }

    #endregion

    #region UC-3.10: View All Charities

    public async Task<(IEnumerable<CharityListDto> Items, int TotalCount)> GetCharitiesAsync(CharityFilterDto filter)
    {
        _logger.LogInformation("Getting charities with filter: {@Filter}", filter);

        var scoped = ApplyCallerScope(filter);

        // Named arguments throughout: this signature is all-optional, so a positional call would
        // bind silently to the wrong parameter if the list ever changes again.
        var (charities, totalCount) = await _charityRepository.GetFilteredPaginatedAsync(
            searchTerm: scoped.SearchTerm,
            countryId: scoped.CountryId,
            regionId: scoped.RegionId,
            centerId: scoped.CenterId,
            isActive: scoped.IsActive,
            isLocked: scoped.IsLocked,
            isAddEnabled: scoped.IsAddEnabled,
            isUpdateEnabled: scoped.IsUpdateEnabled,
            pageNumber: scoped.PageNumber,
            pageSize: scoped.PageSize,
            sortBy: scoped.SortBy,
            sortDescending: scoped.SortDescending,
            charityId: scoped.CharityId);

        // Map each charity individually to avoid collection mapping issues
        var charityDtos = charities.Select(c => _mapper.Map<CharityListDto>(c)).Where(dto => dto != null).Cast<CharityListDto>();

        return (charityDtos, totalCount);
    }

    /// <summary>
    /// Narrows a caller-supplied filter to what the caller is allowed to see.
    /// </summary>
    /// <remarks>
    /// Applied here rather than in the controller so that every caller of
    /// <see cref="GetCharitiesAsync"/> inherits the same scope.
    ///
    /// A charity-bound caller has their charity id written over whatever they sent, so asking for
    /// another charity returns their own record rather than someone else's. A head-office caller
    /// keeps any charity id they chose, but is still pinned to their own country when the token
    /// carries one — this is what stops an HQ user in one country from enumerating another's by
    /// simply omitting the country filter.
    /// </remarks>
    private CharityFilterDto ApplyCallerScope(CharityFilterDto filter)
    {
        // Copy rather than mutate: the argument is the model-bound request object, and a caller
        // that echoes its filter back in a paged response or reuses it to build next-page links
        // must not observe a tenant id it never sent.
        var scoped = CloneFilter(filter);

        if (!_currentUser.IsAuthenticated)
        {
            return DenyAll(scoped, "request is not authenticated");
        }

        var callerCharityId = _currentUser.CharityId;
        if (callerCharityId.HasValue)
        {
            if (filter.CharityId.HasValue && filter.CharityId != callerCharityId)
            {
                _logger.LogWarning(
                    "User {UserId} of charity {CallerCharityId} asked for charity {RequestedCharityId}; scope forced to their own charity",
                    _currentUser.UserId, callerCharityId, filter.CharityId);
            }

            scoped.CharityId = callerCharityId;
            return scoped;
        }

        // No charity claim. Only a head-office role may legitimately look across charities;
        // anyone else reaching this point has an incomplete token and must see nothing.
        // Failing open here would hand the whole register to any caller whose CharityId was
        // never populated, whose token predates these claims, or whose claim failed to parse.
        if (!_currentUser.IsHeadOffice)
        {
            return DenyAll(
                scoped,
                "caller has no charity claim and holds no head-office role");
        }

        var callerCountryId = _currentUser.CountryId;
        if (callerCountryId.HasValue)
        {
            // A head-office user recorded against a country is pinned to it. An explicit charity
            // id from another country would otherwise AND with the country filter and silently
            // return an empty page, so the conflict is reported rather than hidden.
            if (scoped.CharityId.HasValue && filter.CountryId.HasValue && filter.CountryId != callerCountryId)
            {
                _logger.LogWarning(
                    "User {UserId} of country {CallerCountryId} asked for country {RequestedCountryId}; scope forced to their own country",
                    _currentUser.UserId, callerCountryId, filter.CountryId);
            }

            scoped.CountryId = callerCountryId;
        }

        return scoped;
    }

    /// <summary>
    /// Returns a filter that cannot match any row, so an unscopeable caller gets an empty page
    /// rather than the whole register.
    /// </summary>
    private CharityFilterDto DenyAll(CharityFilterDto filter, string reason)
    {
        _logger.LogWarning(
            "Charity query denied for user {UserId}: {Reason}", _currentUser.UserId, reason);

        filter.CharityId = Guid.Empty;
        return filter;
    }

    private static CharityFilterDto CloneFilter(CharityFilterDto filter) => new()
    {
        SearchTerm = filter.SearchTerm,
        CharityId = filter.CharityId,
        CountryId = filter.CountryId,
        RegionId = filter.RegionId,
        CenterId = filter.CenterId,
        IsActive = filter.IsActive,
        IsLocked = filter.IsLocked,
        IsAddEnabled = filter.IsAddEnabled,
        IsUpdateEnabled = filter.IsUpdateEnabled,
        PageNumber = filter.PageNumber,
        PageSize = filter.PageSize,
        SortBy = filter.SortBy,
        SortDescending = filter.SortDescending
    };

    #endregion

    #region UC-3.11: View Charity Profile

    public async Task<CharityProfileDto> GetCharityProfileAsync(Guid id)
    {
        _logger.LogInformation("Getting charity profile: {Id}", id);

        var charity = await _charityRepository.IncludeNavigationProperties()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (charity == null)
        {
            throw new KeyNotFoundException($"Charity with ID '{id}' not found");
        }

        var profileDto = _mapper.Map<CharityProfileDto>(charity);

        // Load statistics
        profileDto.FamilyCount = await _charityRepository.GetFamilyCountAsync(id);
        profileDto.OrphanCount = await _charityRepository.GetOrphanCountAsync(id);
        profileDto.SponsorCount = await _charityRepository.GetSponsorCountAsync(id);

        // Load username if UserId exists
        if (!string.IsNullOrEmpty(charity.UserId))
        {
            _logger.LogInformation("Charity profile has UserId: {UserId}, loading username from Users table", charity.UserId);
            try
            {
                var userIdGuid = Guid.Parse(charity.UserId);
                var user = await _userAppService.FindByIdAsync(userIdGuid);
                if (user != null)
                {
                    profileDto.Username = user.UserName;
                    profileDto.UserId = charity.UserId;
                    _logger.LogInformation("Successfully loaded username: {Username} for charity profile: {Id}", user.UserName, id);
                    // Load last login if available
                    if (user.CurrentToken != null)
                    {
                        // LastLogin would be tracked separately - for now just set to null
                        // You may want to implement proper last login tracking
                        profileDto.LastLoginDate = null;
                    }
                }
                else
                {
                    _logger.LogWarning("User not found in Users table for UserId: {UserId}", charity.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load username for charity profile: {Id}", id);
            }
        }
        else
        {
            _logger.LogInformation("Charity profile {Id} has no UserId", id);
        }

        // Ensure password field is present (will be null for existing accounts as passwords are hashed)
        profileDto.Password = null;

        return profileDto;
    }

    public async Task<CharityProfileDto> GetMyProfileAsync(string userId)
    {
        _logger.LogInformation("Getting profile for user: {UserId}", userId);

        var charity = await _charityRepository.GetByUserIdAsync(userId);
        if (charity == null)
        {
            throw new KeyNotFoundException($"Charity for user '{userId}' not found");
        }

        return await GetCharityProfileAsync(charity.Id);
    }

    #endregion

    #region UC-3.12: Assign Charity to Center

    public async Task SetLocationAsync(CharityLocationDto dto)
    {
        _logger.LogInformation("Setting location for charity {CharityId}: {@Location}", dto.CharityId, dto);

        var charity = await _charityRepository.GetByIdAsync(dto.CharityId);
        if (charity == null)
        {
            throw new KeyNotFoundException($"Charity with ID '{dto.CharityId}' not found");
        }

        charity.CountryId = dto.CountryId;
        charity.RegionId = dto.RegionId;
        charity.CenterId = dto.CenterId;

        _charityRepository.Update(charity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Location set successfully for charity: {CharityId}", dto.CharityId);
    }

    #endregion

    #region UC-3.13: Manage Charity Contacts

    public async Task UpdateManagementContactsAsync(CharityManagementContactsDto dto)
    {
        _logger.LogInformation("Updating management contacts for charity: {CharityId}", dto.CharityId);

        var charity = await _charityRepository.GetByIdAsync(dto.CharityId);
        if (charity == null)
        {
            throw new KeyNotFoundException($"Charity with ID '{dto.CharityId}' not found");
        }

        charity.BossName = dto.BossName;
        charity.BossJobName = dto.BossJobName;
        charity.BossPhone1 = dto.BossPhone1;
        charity.BossPhone2 = dto.BossPhone2;
        charity.ResponsibleJobName = dto.ResponsibleJobName;
        charity.ResponsiblePhone1 = dto.ResponsiblePhone1;
        charity.ResponsiblePhone2 = dto.ResponsiblePhone2;

        _charityRepository.Update(charity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Management contacts updated successfully for charity: {CharityId}", dto.CharityId);
    }

    #endregion

    #region UC-3.14: Set Map Location

    public async Task SetMapLocationAsync(Guid id, string mapLocation)
    {
        _logger.LogInformation("Setting map location for charity: {Id}", id);

        var charity = await _charityRepository.GetByIdAsync(id);
        if (charity == null)
        {
            throw new KeyNotFoundException($"Charity with ID '{id}' not found");
        }

        charity.NgoMapLocation = mapLocation;

        _charityRepository.Update(charity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Map location set successfully for charity: {Id}", id);
    }

    #endregion

    #region Additional Helper Methods

    public async Task<CharityDto?> GetByIdAsync(Guid id)
    {
        var charity = await _charityRepository.IncludeNavigationProperties()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (charity == null) return null;

        var dto = _mapper.Map<CharityDto>(charity);

        // Load icon attachment if IconId exists
        if (charity.IconId.HasValue)
        {
            var attachments = await _attachmentService.GetAttachmentAsync(new List<Guid> { charity.IconId.Value });
            if (attachments != null && attachments.Any())
            {
                dto.Icon_Attach = attachments.Select(a => new Framework.Core.SharedServices.Dto.AttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    FilePath = a.FilePath,
                    Extension = a.Extension.TrimStart('.'),
                    FileData = a.AttachmentContent.FileContent
                }).ToList();
            }
        }

        // Load username if UserId exists
        if (!string.IsNullOrEmpty(charity.UserId))
        {
            _logger.LogInformation("Charity has UserId: {UserId}, loading username from Users table", charity.UserId);
            try
            {
                var userIdGuid = Guid.Parse(charity.UserId);
                var user = await _userAppService.FindByIdAsync(userIdGuid);
                if (user != null)
                {
                    dto.Username = user.UserName;
                    dto.UserId = charity.UserId;
                    _logger.LogInformation("Successfully loaded username: {Username} for charity: {Id}", user.UserName, id);
                    // Load last login if available
                    if (user.CurrentToken != null)
                    {
                        // LastLogin would be tracked separately - for now just set to null
                        // You may want to implement proper last login tracking
                        dto.LastLogin = null;
                    }
                }
                else
                {
                    _logger.LogWarning("User not found in Users table for UserId: {UserId}", charity.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load username for charity: {Id}", id);
            }
        }
        else
        {
            _logger.LogInformation("Charity {Id} has no UserId", id);
        }

        // Ensure password field is present (will be null for existing accounts as passwords are hashed)
        dto.Password = null;

        return dto;
    }

    public async Task<CharityDto?> GetByCodeAsync(string code)
    {
        var charity = await _charityRepository.GetByCodeAsync(code);
        return charity == null ? null : _mapper.Map<CharityDto>(charity);
    }

    public async Task<CharityDto?> GetByUserIdAsync(string userId)
    {
        var charity = await _charityRepository.GetByUserIdAsync(userId);
        if (charity == null) return null;

        var dto = _mapper.Map<CharityDto>(charity);

        // Load username if UserId exists
        if (!string.IsNullOrEmpty(charity.UserId))
        {
            try
            {
                var userIdGuid = Guid.Parse(charity.UserId);
                var user = await _userAppService.FindByIdAsync(userIdGuid);
                if (user != null)
                {
                    dto.Username = user.UserName;
                    dto.UserId = charity.UserId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load username for charity: {UserId}", userId);
            }
        }

        return dto;
    }

    public async Task<IEnumerable<CharityListDto>> GetActiveCharitiesAsync()
    {
        var charities = await _charityRepository.GetActiveCharitiesAsync();
        return charities.Select(c => _mapper.Map<CharityListDto>(c)).Where(dto => dto != null).Cast<CharityListDto>();
    }

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
    {
        // Normalised here so every caller compares the same thing. SQL Server ignores trailing
        // spaces in an equality test but not leading ones, so " Alpha" and "Alpha" would otherwise
        // be distinct rows: the availability check (which trimmed) would report the name free, the
        // save (which did not) would store the padded value, and every later check would keep
        // reporting it free.
        return await _charityRepository.IsNameUniqueAsync(NormaliseName(name), excludeId);
    }

    /// <summary>
    /// The canonical form of a charity name for comparison and storage.
    /// </summary>
    private static string NormaliseName(string? name) => (name ?? string.Empty).Trim();

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null)
    {
        return await _charityRepository.IsEmailUniqueAsync(email, excludeId);
    }

    public async Task DeleteCharityAsync(Guid id)
    {
        _logger.LogInformation("Deleting charity: {Id}", id);

        var charity = await _charityRepository.GetByIdAsync(id);
        if (charity == null)
        {
            throw new KeyNotFoundException($"Charity with ID '{id}' not found");
        }

        _charityRepository.Delete(charity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Charity deleted successfully: {Id}", id);
    }

    #endregion

    #region Private Helper Methods

    private async Task<string> GenerateCharityCodeAsync()
    {
        // Generate code in format: CHA-YYYY-XXXX
        var year = DateTime.Now.Year;
        var random = new Random();
        string code;
        bool isUnique;

        do
        {
            code = $"CHA-{year}-{random.Next(1000, 9999)}";
            isUnique = await _charityRepository.IsCodeUniqueAsync(code);
        } while (!isUnique);

        return code;
    }

    private string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private bool IsValidIBAN(string iban)
    {
        // Basic IBAN validation
        if (string.IsNullOrWhiteSpace(iban) || iban.Length < 15 || iban.Length > 34)
            return false;

        iban = iban.Replace(" ", "").ToUpper();

        if (!iban.All(char.IsLetterOrDigit))
            return false;

        // Move first four characters to the end
        var rearranged = iban.Substring(4) + iban.Substring(0, 4);

        // Replace letters with numbers
        var numeric = new string(rearranged.Select(c => char.IsLetter(c)
            ? (c - 'A' + 10).ToString()
            : c.ToString()).SelectMany(s => s).ToArray());

        // Check if divisible by 97
        if (!numeric.All(char.IsDigit) || numeric.Length == 0)
            return false;

        // Use mod 97 check
        int remainder = 0;
        foreach (var digit in numeric)
        {
            remainder = (remainder * 10 + (digit - '0')) % 97;
        }

        return remainder == 1;
    }

    /// <summary>
    /// Creates a user account for a charity with the Charity role
    /// </summary>
    /// <summary>
    /// Creates the login for a charity, stamped with the charity and country it belongs to.
    /// </summary>
    /// <remarks>
    /// <paramref name="charityId"/> is not optional in practice: an account created without it has
    /// no tenancy claim in its token, and the application layer cannot scope such a caller. It is
    /// taken as a parameter rather than read back from the entity because on the create path the
    /// account is made before the charity row is persisted.
    /// </remarks>
    private async Task<UserDto?> CreateUserAccountForCharity(
        string username,
        string password,
        string fullName,
        Guid charityId,
        int? countryId)
    {
        try
        {
            _logger.LogInformation("Creating user account for charity: {Username}", username);

            // Create the user account
            var userCreateDto = new UserCreateDto
            {
                UserName = username,
                Email = username, // Username is the email
                FullName = fullName,
                Password = password,
                IsActive = true,
                RoleNames = new[] { "Charity" },
                CharityId = charityId,
                CountryId = countryId
            };

            var user = await _userAppService.CreateAsync(userCreateDto);

            if (user != null)
            {
                _logger.LogInformation("User account created successfully: {UserId}", user.Id);
                return user;
            }

            _logger.LogWarning("Failed to create user account for: {Username}", username);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user account for charity: {Username}", username);
            return null;
        }
    }

    /// <summary>
    /// Gets a user by username
    /// </summary>
    private async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        try
        {
            return await _userAppService.FindByUsernameAsync(username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding user by username: {Username}", username);
            return null;
        }
    }

    #endregion
}
