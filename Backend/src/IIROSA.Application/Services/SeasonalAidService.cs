using Microsoft.Extensions.Logging;
using IIROSA.Domain.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.SeasonalAid;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Application.Services;

/// <summary>
/// Seasonal Aid Service Implementation
/// Implements all use cases UC-9.1 through UC-9.11
/// </summary>
public class SeasonalAidService : ISeasonalAidService
{
    private readonly ISeasonalAidCampaignRepository _campaignRepository;
    private readonly ISeasonalAidBeneficiaryRepository _beneficiaryRepository;
    private readonly ISeasonalAidDistributionRepository _distributionRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ICharityRepository _charityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SeasonalAidService> _logger;
    private readonly ICurrentUserService _currentUser;

    private readonly IValidator<CreateSeasonalAidCampaignDto> _createCampaignValidator;
    private readonly IValidator<UpdateSeasonalAidCampaignDto> _updateCampaignValidator;
    private readonly IValidator<CreateSeasonalAidBeneficiaryDto> _registerBeneficiariesValidator;
    private readonly IValidator<UpdateSeasonalAidBeneficiariesDto> _updateBeneficiariesValidator;
    private readonly IValidator<CreateSeasonalAidDistributionDto> _distributionValidator;
    private readonly IValidator<SetFamilyReceivedFlagDto> _setReceivedFlagValidator;

    public SeasonalAidService(
        ISeasonalAidCampaignRepository campaignRepository,
        ISeasonalAidBeneficiaryRepository beneficiaryRepository,
        ISeasonalAidDistributionRepository distributionRepository,
        IFamilyRepository familyRepository,
        ICharityRepository charityRepository,
        IUnitOfWork unitOfWork,
        ILogger<SeasonalAidService> logger,
        ICurrentUserService currentUser,
        IValidator<CreateSeasonalAidCampaignDto> createCampaignValidator,
        IValidator<UpdateSeasonalAidCampaignDto> updateCampaignValidator,
        IValidator<CreateSeasonalAidBeneficiaryDto> registerBeneficiariesValidator,
        IValidator<UpdateSeasonalAidBeneficiariesDto> updateBeneficiariesValidator,
        IValidator<CreateSeasonalAidDistributionDto> distributionValidator,
        IValidator<SetFamilyReceivedFlagDto> setReceivedFlagValidator)
    {
        _campaignRepository = campaignRepository;
        _beneficiaryRepository = beneficiaryRepository;
        _distributionRepository = distributionRepository;
        _familyRepository = familyRepository;
        _charityRepository = charityRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUser = currentUser;
        _createCampaignValidator = createCampaignValidator;
        _updateCampaignValidator = updateCampaignValidator;
        _registerBeneficiariesValidator = registerBeneficiariesValidator;
        _updateBeneficiariesValidator = updateBeneficiariesValidator;
        _distributionValidator = distributionValidator;
        _setReceivedFlagValidator = setReceivedFlagValidator;
    }

    #region Caller scoping

    /// <summary>
    /// Copies the filter and pins it to the caller's country when the token carries one.
    /// Campaigns are not charity-owned — they are a shared, country-scoped catalogue — so the
    /// only tenancy dimension left is the country claim. An unauthenticated caller gets a
    /// filter that matches nothing.
    /// </summary>
    private SeasonalAidCampaignFilterDto ApplyCallerScope(SeasonalAidCampaignFilterDto filter)
    {
        var scoped = new SeasonalAidCampaignFilterDto
        {
            SearchTerm = filter.SearchTerm,
            CampaignType = filter.CampaignType,
            IsActive = filter.IsActive,
            IsClosed = filter.IsClosed,
            CountryId = filter.CountryId,
            RegionId = filter.RegionId,
            CenterId = filter.CenterId,
            StartDateFrom = filter.StartDateFrom,
            StartDateTo = filter.StartDateTo,
            EndDateFrom = filter.EndDateFrom,
            EndDateTo = filter.EndDateTo,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            SortBy = filter.SortBy,
            SortDescending = filter.SortDescending
        };

        if (!_currentUser.IsAuthenticated)
        {
            return DenyAll(scoped, "request is not authenticated");
        }

        if (_currentUser.CountryId.HasValue)
        {
            if (scoped.CountryId.HasValue && scoped.CountryId != _currentUser.CountryId)
            {
                _logger.LogWarning(
                    "User {UserId} of country {CallerCountryId} asked for country {RequestedCountryId}; scope forced to their own country",
                    _currentUser.UserId, _currentUser.CountryId, scoped.CountryId);
            }

            scoped.CountryId = _currentUser.CountryId;
        }

        return scoped;
    }

    private SeasonalAidCampaignFilterDto DenyAll(SeasonalAidCampaignFilterDto filter, string reason)
    {
        _logger.LogWarning("Seasonal aid query denied for user {UserId}: {Reason}", _currentUser.UserId, reason);
        // No campaign can carry this id — country ids are positive lookup identities.
        filter.CountryId = -1;
        return filter;
    }

    /// <summary>
    /// True when the campaign is inside the caller's country scope. Campaigns carry no charity,
    /// so the only narrowing dimension is the country claim; a caller without one sees the
    /// shared catalogue.
    /// </summary>
    private bool IsCampaignVisibleToCaller(SeasonalAidCampaign campaign)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return false;
        }

        if (_currentUser.CountryId.HasValue)
        {
            return !campaign.CountryId.HasValue || campaign.CountryId == _currentUser.CountryId;
        }

        return true;
    }

    /// <summary>
    /// Throws <see cref="KeyNotFoundException"/> when the campaign is outside the caller's
    /// tenancy — out-of-scope records read as absent so their existence is not leaked.
    /// </summary>
    private void EnsureCampaignScope(SeasonalAidCampaign campaign, string operation)
    {
        if (IsCampaignVisibleToCaller(campaign))
        {
            return;
        }

        _logger.LogWarning(
            "Seasonal aid {Operation} on campaign {CampaignId} denied for user {UserId}: outside caller scope",
            operation, campaign.Id, _currentUser.UserId);
        throw new KeyNotFoundException($"Campaign with ID '{campaign.Id}' not found");
    }

    /// <summary>
    /// The charity whose families are in scope for the caller: their own charity for a
    /// charity-bound caller, every charity for a head-office caller. Campaigns carry no
    /// charity, so the scope comes from the caller alone.
    /// </summary>
    private Guid? ResolveCallerFamilyCharityId()
    {
        return _currentUser.CharityId;
    }

    /// <summary>
    /// Names of the charities behind the families on a page — families carry their charity on
    /// <c>FK_CharityId</c>, which has no navigation property, so the names are resolved by id.
    /// </summary>
    private async Task<Dictionary<Guid, string>> GetCharityNamesAsync(IEnumerable<Guid?> charityIds)
    {
        var names = new Dictionary<Guid, string>();

        foreach (var id in charityIds.Where(id => id.HasValue).Select(id => id.Value).Distinct())
        {
            var charity = await _charityRepository.GetByIdAsync(id);
            if (charity != null)
            {
                names[id] = charity.Name;
            }
        }

        return names;
    }

    #endregion

    #region UC-9.1: Create Seasonal Aid Campaign

    public async Task<SeasonalAidCampaignDto> CreateCampaignAsync(CreateSeasonalAidCampaignDto dto)
    {
        _logger.LogInformation("Creating new seasonal aid campaign: {Name}", dto.Name);

        _createCampaignValidator.ValidateAndThrow(dto);

        // Validate uniqueness
        if (!await IsCampaignNameUniqueAsync(dto.Name))
        {
            throw new InvalidOperationException($"Campaign with name '{dto.Name}' already exists");
        }

        // Tenancy: the country is defaulted from the caller's claim and pinned to it when the
        // token carries one. Campaigns carry no charity — they are shared, country-scoped
        // programmes.
        int? countryId = dto.CountryId;
        if (_currentUser.CountryId.HasValue)
        {
            if (dto.CountryId.HasValue && dto.CountryId != _currentUser.CountryId)
            {
                _logger.LogWarning(
                    "User {UserId} of country {CallerCountryId} tried to create a campaign in country {RequestedCountryId}; country pinned to their own",
                    _currentUser.UserId, _currentUser.CountryId, dto.CountryId);
            }

            countryId = _currentUser.CountryId;
        }

        var campaign = new SeasonalAidCampaign
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            CampaignType = dto.CampaignType,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalBudget = dto.TotalBudget,
            BudgetCurrency = dto.BudgetCurrency,
            PerFamilyAllocation = dto.PerFamilyAllocation,
            CountryId = countryId,
            RegionId = dto.RegionId,
            CenterId = dto.CenterId,
            MaximumFamilies = dto.MaximumFamilies,
            FamilyType = dto.FamilyType,
            MinChildrenAge = dto.MinChildrenAge,
            MaxChildrenAge = dto.MaxChildrenAge,
            IsActive = dto.IsActive,
            IsClosed = false
        };

        await _campaignRepository.AddAsync(campaign);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Campaign created successfully with ID: {Id}", campaign.Id);

        return (await GetCampaignByIdAsync(campaign.Id))!;
    }

    #endregion

    #region UC-9.2: Set Campaign Period

    public async Task SetCampaignPeriodAsync(Guid campaignId, DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Setting campaign period for campaign: {CampaignId}", campaignId);

        var campaign = await _campaignRepository.GetByIdAsync(campaignId);
        if (campaign == null)
        {
            throw new KeyNotFoundException($"Campaign with ID '{campaignId}' not found");
        }

        if (campaign.IsClosed)
        {
            throw new InvalidOperationException("Cannot modify period of a closed campaign");
        }

        if (endDate < startDate)
        {
            throw new ArgumentException("End date must be greater than or equal to start date");
        }

        campaign.StartDate = startDate;
        campaign.EndDate = endDate;

        _campaignRepository.Update(campaign);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Campaign period updated successfully for campaign: {CampaignId}", campaignId);
    }

    #endregion

    #region UC-9.3: Allocate Campaign Budget

    public async Task SetCampaignBudgetAsync(Guid campaignId, decimal totalBudget, string currency, decimal perFamilyAllocation)
    {
        _logger.LogInformation("Setting budget for campaign: {CampaignId}", campaignId);

        var campaign = await _campaignRepository.GetByIdAsync(campaignId);
        if (campaign == null)
        {
            throw new KeyNotFoundException($"Campaign with ID '{campaignId}' not found");
        }

        if (campaign.IsClosed)
        {
            throw new InvalidOperationException("Cannot modify budget of a closed campaign");
        }

        if (totalBudget <= 0)
        {
            throw new ArgumentException("Total budget must be greater than zero");
        }

        if (perFamilyAllocation <= 0)
        {
            throw new ArgumentException("Per-family allocation must be greater than zero");
        }

        campaign.TotalBudget = totalBudget;
        campaign.BudgetCurrency = currency;
        campaign.PerFamilyAllocation = perFamilyAllocation;

        _campaignRepository.Update(campaign);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Budget updated successfully for campaign: {CampaignId}", campaignId);
    }

    #endregion

    #region UC-9.4: Register Beneficiary for Aid

    public async Task<(int RegisteredCount, decimal TotalAllocation, decimal BudgetImpact)> RegisterBeneficiariesAsync(CreateSeasonalAidBeneficiaryDto dto)
    {
        _logger.LogInformation("Registering beneficiaries for campaign: {CampaignId}", dto.CampaignId);

        _registerBeneficiariesValidator.ValidateAndThrow(dto);

        var campaign = await GetScopedCampaignAsync(dto.CampaignId, "register beneficiaries");

        if (!campaign.IsActive)
        {
            throw new InvalidOperationException("Cannot register beneficiaries for an inactive campaign");
        }

        if (campaign.IsClosed)
        {
            throw new InvalidOperationException("Cannot register beneficiaries for a closed campaign");
        }

        var allocationAmount = dto.AllocationAmount ?? campaign.PerFamilyAllocation;
        var currency = dto.Currency ?? campaign.BudgetCurrency;

        // Check if campaign has available slots
        if (campaign.MaximumFamilies.HasValue)
        {
            var currentBeneficiaries = await _beneficiaryRepository.GetTotalBeneficiariesAsync(campaign.Id);
            if (currentBeneficiaries + dto.FamilyIds.Count > campaign.MaximumFamilies.Value)
            {
                throw new InvalidOperationException(
                    $"Registering {dto.FamilyIds.Count} families would exceed maximum families limit ({campaign.MaximumFamilies})");
            }
        }

        int registeredCount = 0;
        decimal totalAllocation = 0;
        var beneficiaries = new List<SeasonalAidBeneficiary>();
        var familyCharityId = ResolveCallerFamilyCharityId();

        foreach (var familyId in dto.FamilyIds)
        {
            // Check if family is already registered
            if (await _beneficiaryRepository.IsFamilyRegisteredAsync(campaign.Id, familyId))
            {
                _logger.LogWarning("Family {FamilyId} is already registered for campaign {CampaignId}", familyId, campaign.Id);
                continue;
            }

            var family = await _familyRepository.GetByIdAsync(familyId);
            if (family == null)
            {
                _logger.LogWarning("Family {FamilyId} not found", familyId);
                continue;
            }

            // BR-24: only families owned by the campaign's charity may be registered for it
            if (familyCharityId.HasValue && family.FK_CharityId != familyCharityId)
            {
                throw new InvalidOperationException(
                    $"Family '{family.Code}' does not belong to the charity this campaign is scoped to");
            }

            var beneficiary = new SeasonalAidBeneficiary
            {
                Id = Guid.NewGuid(),
                CampaignId = campaign.Id,
                FamilyId = familyId,
                AllocationAmount = allocationAmount,
                Currency = currency,
                IsRegistered = true,
                RegistrationDate = DateTime.UtcNow,
                RegistrationNotes = dto.RegistrationNotes
            };

            beneficiaries.Add(beneficiary);
            registeredCount++;
            totalAllocation += allocationAmount;
        }

        if (beneficiaries.Any())
        {
            await _beneficiaryRepository.AddRangeAsync(beneficiaries);
            await _unitOfWork.SaveChangesAsync();
        }

        var budgetImpact = campaign.AllocatedBudget + totalAllocation;

        _logger.LogInformation("Registered {Count} beneficiaries for campaign {CampaignId} with total allocation: {Allocation}",
            registeredCount, campaign.Id, totalAllocation);

        return (registeredCount, totalAllocation, budgetImpact);
    }

    /// <summary>
    /// UC-PRJ-07 full sync of a campaign's family registrations. <paramref name="dto"/> carries
    /// the desired final set; families are added and removed as a single diff, validated
    /// all-or-nothing (BR-23 duplicate registration, BR-24 charity ownership, HQ quota) and
    /// persisted in one save.
    /// </summary>
    public async Task<UpdateBeneficiariesResultDto> UpdateCampaignBeneficiariesAsync(
        Guid campaignId, UpdateSeasonalAidBeneficiariesDto dto)
    {
        _logger.LogInformation("Syncing beneficiaries for campaign: {CampaignId}", campaignId);

        _updateBeneficiariesValidator.ValidateAndThrow(dto);

        var campaign = await GetScopedCampaignAsync(campaignId, "update beneficiaries");

        if (!campaign.IsActive)
        {
            throw new InvalidOperationException("Cannot update beneficiaries for an inactive campaign");
        }

        if (campaign.IsClosed)
        {
            throw new InvalidOperationException("Cannot update beneficiaries for a closed campaign");
        }

        // Desired final set — duplicates in the payload collapse silently.
        var desiredFamilyIds = dto.FamilyIds.Distinct().ToList();

        var existing = (await _beneficiaryRepository.GetByCampaignAsync(campaignId)).ToList();
        var existingByFamilyId = existing.ToDictionary(b => b.FamilyId);

        var adds = desiredFamilyIds.Where(id => !existingByFamilyId.ContainsKey(id)).ToList();
        var removeFamilyIds = existingByFamilyId.Keys.Where(id => !desiredFamilyIds.Contains(id)).ToList();

        // Quota (A1): the resulting registration count may not exceed the HQ-defined maximum.
        if (campaign.MaximumFamilies.HasValue &&
            existing.Count + adds.Count - removeFamilyIds.Count > campaign.MaximumFamilies.Value)
        {
            throw new InvalidOperationException(
                $"The selection would exceed the maximum families limit ({campaign.MaximumFamilies})");
        }

        // Removals: a registration that has already been distributed is a closed delivery
        // record and may not be silently dropped from the project (BR-25).
        foreach (var familyId in removeFamilyIds)
        {
            var beneficiary = existingByFamilyId[familyId];
            if (beneficiary.IsDistributed)
            {
                throw new InvalidOperationException(
                    "A family that has already received its assistance cannot be deselected; it must stay on the project");
            }
        }

        var familyCharityId = ResolveCallerFamilyCharityId();
        var allocationAmount = dto.AllocationAmount ?? campaign.PerFamilyAllocation;
        var currency = dto.Currency ?? campaign.BudgetCurrency;

        // Additions: BR-24 — only families owned by the campaign's charity.
        var beneficiariesToAdd = new List<SeasonalAidBeneficiary>();
        foreach (var familyId in adds)
        {
            var family = await _familyRepository.GetByIdAsync(familyId);
            if (family == null)
            {
                throw new KeyNotFoundException($"Family with ID '{familyId}' not found");
            }

            if (familyCharityId.HasValue && family.FK_CharityId != familyCharityId)
            {
                throw new InvalidOperationException(
                    $"Family '{family.Code}' does not belong to the charity this campaign is scoped to");
            }

            beneficiariesToAdd.Add(new SeasonalAidBeneficiary
            {
                Id = Guid.NewGuid(),
                CampaignId = campaignId,
                FamilyId = familyId,
                AllocationAmount = allocationAmount,
                Currency = currency,
                IsRegistered = true,
                RegistrationDate = DateTime.UtcNow,
                RegistrationNotes = dto.Notes
            });
        }

        if (beneficiariesToAdd.Any())
        {
            await _beneficiaryRepository.AddRangeAsync(beneficiariesToAdd);
        }

        foreach (var familyId in removeFamilyIds)
        {
            var beneficiary = existingByFamilyId[familyId];
            // Soft delete: the row keeps its history and stops matching the filtered unique
            // index, so the family can be re-registered in a later campaign rotation.
            beneficiary.IsRegistered = false;
            beneficiary.IsDeleted = true;
            _beneficiaryRepository.Update(beneficiary);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Beneficiaries synced for campaign {CampaignId}: {Added} added, {Removed} removed, {Total} registered",
            campaignId, beneficiariesToAdd.Count, removeFamilyIds.Count, existing.Count + adds.Count - removeFamilyIds.Count);

        return new UpdateBeneficiariesResultDto
        {
            AddedCount = beneficiariesToAdd.Count,
            RemovedCount = removeFamilyIds.Count,
            TotalRegistered = existing.Count + adds.Count - removeFamilyIds.Count,
            MaximumFamilies = campaign.MaximumFamilies
        };
    }

    /// <summary>
    /// UC-PRJ-08: flags the project-family registration of <paramref name="familyId"/> in the
    /// given campaign as delivered (or clears the flag when no distribution record backs it).
    /// </summary>
    public async Task SetFamilyReceivedFlagAsync(Guid familyId, SetFamilyReceivedFlagDto dto)
    {
        _setReceivedFlagValidator.ValidateAndThrow(dto);

        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family == null)
        {
            throw new KeyNotFoundException($"Family with ID '{familyId}' not found");
        }

        // A charity user may only confirm delivery for their own families.
        if (_currentUser.CharityId.HasValue && family.FK_CharityId != _currentUser.CharityId)
        {
            _logger.LogWarning(
                "User {UserId} of charity {CallerCharityId} tried to set the received flag of family {FamilyId} belonging to another charity",
                _currentUser.UserId, _currentUser.CharityId, familyId);
            throw new KeyNotFoundException($"Family with ID '{familyId}' not found");
        }

        var beneficiary = await _beneficiaryRepository.GetByCampaignAndFamilyAsync(dto.CampaignId, familyId);
        if (beneficiary == null)
        {
            throw new KeyNotFoundException(
                $"Family with ID '{familyId}' is not registered for campaign '{dto.CampaignId}'");
        }

        var campaign = await _campaignRepository.GetByIdAsync(beneficiary.CampaignId);
        if (campaign == null || campaign.IsClosed)
        {
            throw new InvalidOperationException("Cannot change the delivery state of a closed campaign");
        }

        if (dto.IsReceived)
        {
            // Idempotent: confirming an already-delivered registration keeps its original date.
            beneficiary.IsDistributed = true;
            beneficiary.DistributionDate ??= DateTime.UtcNow;
        }
        else
        {
            var distributions = await _distributionRepository.GetByBeneficiaryAsync(beneficiary.Id);
            if (distributions.Any())
            {
                throw new InvalidOperationException(
                    "Delivery cannot be withdrawn once a distribution record exists; remove the distribution record first");
            }

            beneficiary.IsDistributed = false;
            beneficiary.DistributionDate = null;
        }

        _beneficiaryRepository.Update(beneficiary);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Received flag of family {FamilyId} in campaign {CampaignId} set to {IsReceived}",
            familyId, dto.CampaignId, dto.IsReceived);
    }

    /// <summary>
    /// Loads a campaign and fails with 404 semantics when it does not exist or sits outside the
    /// caller's tenancy.
    /// </summary>
    private async Task<SeasonalAidCampaign> GetScopedCampaignAsync(Guid campaignId, string operation)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId);
        if (campaign == null)
        {
            throw new KeyNotFoundException($"Campaign with ID '{campaignId}' not found");
        }

        EnsureCampaignScope(campaign, operation);
        return campaign;
    }

    public async Task<(IEnumerable<SeasonalAidBeneficiaryDto> Items, int TotalCount)> GetEligibleFamiliesAsync(EligibleFamiliesFilterDto filter)
    {
        _logger.LogInformation("Getting eligible families for campaign: {CampaignId}", filter.CampaignId);

        var campaign = await GetScopedCampaignAsync(filter.CampaignId, "list eligible families");

        // The charity whose families are in scope: the charity-bound caller's own charity.
        // A charity user cannot widen this by passing a filter value.
        var scopedCharityId = ResolveCallerFamilyCharityId();
        if (scopedCharityId.HasValue)
        {
            filter.CharityId = scopedCharityId;
        }

        // Build query for eligible families
        var familiesQuery = _familyRepository.IncludeNavigationProperties();

        // Families carry their charity on FK_CharityId (the column the family module writes).
        if (filter.CharityId.HasValue)
        {
            familiesQuery = familiesQuery.Where(f => f.FK_CharityId == filter.CharityId.Value);
        }

        if (filter.RegionId.HasValue)
        {
            familiesQuery = familiesQuery.Where(f => f.CityId == filter.RegionId.Value);
        }

        if (filter.CenterId.HasValue)
        {
            familiesQuery = familiesQuery.Where(f => f.CityId == filter.CenterId.Value);
        }

        // Exclude families already registered for this campaign — the remaining set is the
        // rotation pool UC-PRJ-10 reports on.
        var registeredFamilyIds = (await _beneficiaryRepository.GetByCampaignAsync(filter.CampaignId))
            .Select(b => b.FamilyId)
            .ToList();

        if (registeredFamilyIds.Any())
        {
            var registered = registeredFamilyIds;
            familiesQuery = familiesQuery.Where(f => !registered.Contains(f.Id));
        }

        // Apply search filter
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            familiesQuery = familiesQuery.Where(f =>
                f.Code.Contains(filter.SearchTerm) ||
                (f.Address != null && f.Address.Contains(filter.SearchTerm)));
        }

        // Get total count
        var totalCount = await familiesQuery.CountAsync();

        // Apply pagination
        var families = await familiesQuery
            .OrderBy(f => f.Code)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // Resolve charity names by id: FK_CharityId has no navigation property.
        var charityNames = await GetCharityNamesAsync(families.Select(f => f.FK_CharityId));

        // Convert to DTOs
        var beneficiaryDtos = families.Select(f => new SeasonalAidBeneficiaryDto
        {
            Id = Guid.Empty,
            CampaignId = filter.CampaignId,
            FamilyId = f.Id,
            FamilyCode = f.Code,
            FamilyAddress = f.Address,
            OrphansCount = f.OrphansCount,
            FamilyMembersCount = f.FamilyMembersCount,
            CharityName = f.FK_CharityId.HasValue && charityNames.TryGetValue(f.FK_CharityId.Value, out var charityName)
                ? charityName
                : null,
            RegionName = f.City?.Name,
            CenterName = f.City?.Name,
            AllocationAmount = campaign.PerFamilyAllocation,
            Currency = campaign.BudgetCurrency,
            IsRegistered = false,
            RegistrationDate = DateTime.UtcNow,
            IsDistributed = false
        });

        return (beneficiaryDtos, totalCount);
    }

    public async Task RemoveBeneficiaryAsync(Guid beneficiaryId)
    {
        _logger.LogInformation("Removing beneficiary: {BeneficiaryId}", beneficiaryId);

        var beneficiary = await _beneficiaryRepository.GetByIdAsync(beneficiaryId);

        if (beneficiary == null)
        {
            throw new KeyNotFoundException($"Beneficiary with ID '{beneficiaryId}' not found");
        }

        if (beneficiary.IsDistributed)
        {
            throw new InvalidOperationException("Cannot remove a beneficiary that has already received distribution");
        }

        // Soft delete: the row keeps its history and frees the (CampaignId, FamilyId) slot for
        // a later campaign rotation.
        beneficiary.IsRegistered = false;
        beneficiary.IsDeleted = true;
        _beneficiaryRepository.Update(beneficiary);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Beneficiary removed successfully: {BeneficiaryId}", beneficiaryId);
    }

    #endregion

    #region UC-9.5: Record Aid Distribution

    public async Task<SeasonalAidDistributionDto> RecordDistributionAsync(CreateSeasonalAidDistributionDto dto)
    {
        _logger.LogInformation("Recording distribution for beneficiary: {BeneficiaryId}", dto.BeneficiaryId);

        _distributionValidator.ValidateAndThrow(dto);

        var beneficiary = await _beneficiaryRepository.GetByIdAsync(dto.BeneficiaryId);

        if (beneficiary == null)
        {
            throw new KeyNotFoundException($"Beneficiary with ID '{dto.BeneficiaryId}' not found");
        }

        // GetByIdAsync does not load navigations; fetch the campaign by its key rather than
        // dereferencing beneficiary.Campaign (which is null without lazy loading).
        var campaign = await _campaignRepository.GetByIdAsync(beneficiary.CampaignId);
        if (campaign == null || !IsCampaignVisibleToCaller(campaign))
        {
            throw new KeyNotFoundException($"Beneficiary with ID '{dto.BeneficiaryId}' not found");
        }

        if (campaign.IsClosed)
        {
            throw new InvalidOperationException("Cannot record distribution for a closed campaign");
        }

        var distribution = new SeasonalAidDistribution
        {
            Id = Guid.NewGuid(),
            BeneficiaryId = dto.BeneficiaryId,
            IsDistributed = true,
            DistributionDate = dto.DistributionDate,
            AmountDistributed = dto.AmountDistributed,
            Currency = dto.Currency,
            ReceivedBy = dto.ReceivedBy,
            RecipientRelationship = dto.RecipientRelationship,
            Notes = dto.Notes,
            SignatureImageUrl = dto.SignatureImageUrl,
            AttachmentId = dto.AttachmentId,
            DistributionMethod = dto.DistributionMethod,
            DistributorName = dto.DistributorName,
            DistributorRole = dto.DistributorRole
        };

        await _distributionRepository.AddAsync(distribution);

        // Update beneficiary status
        beneficiary.IsDistributed = true;
        beneficiary.DistributionDate = dto.DistributionDate;
        _beneficiaryRepository.Update(beneficiary);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Distribution recorded successfully for beneficiary: {BeneficiaryId}", dto.BeneficiaryId);

        var family = await _familyRepository.GetByIdAsync(beneficiary.FamilyId);
        return MapToDistributionDto(distribution, beneficiary, campaign.Name, family?.Code);
    }

    public async Task RecordDistributionsAsync(List<CreateSeasonalAidDistributionDto> distributions)
    {
        _logger.LogInformation("Recording {Count} distributions", distributions.Count);

        foreach (var dto in distributions)
        {
            await RecordDistributionAsync(dto);
        }

        _logger.LogInformation("All distributions recorded successfully");
    }

    #endregion

    #region UC-9.6: View Campaign List

    public async Task<(IEnumerable<SeasonalAidCampaignListDto> Items, int TotalCount)> GetCampaignsAsync(SeasonalAidCampaignFilterDto filter)
    {
        _logger.LogInformation("Getting campaigns with filter: {@Filter}", filter);

        // Tenancy first: the caller's scope decides which campaigns exist for them, whatever
        // the request asked for. Named arguments throughout — the signature is all-optional.
        var scoped = ApplyCallerScope(filter);
        var (campaigns, totalCount) = await _campaignRepository.GetFilteredPaginatedAsync(
            searchTerm: scoped.SearchTerm,
            campaignType: scoped.CampaignType,
            isActive: scoped.IsActive,
            isClosed: scoped.IsClosed,
            countryId: scoped.CountryId,
            regionId: scoped.RegionId,
            centerId: scoped.CenterId,
            startDateFrom: scoped.StartDateFrom,
            startDateTo: scoped.StartDateTo,
            endDateFrom: scoped.EndDateFrom,
            endDateTo: scoped.EndDateTo,
            pageNumber: scoped.PageNumber,
            pageSize: scoped.PageSize,
            sortBy: scoped.SortBy,
            sortDescending: scoped.SortDescending);

        var campaignDtos = campaigns.Select(c => new SeasonalAidCampaignListDto
        {
            Id = c.Id,
            Name = c.Name,
            CampaignType = c.CampaignType,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            TotalBudget = c.TotalBudget,
            BudgetCurrency = c.BudgetCurrency,
            AllocatedBudget = c.AllocatedBudget,
            DistributedBudget = c.DistributedBudget,
            RegisteredBeneficiariesCount = c.RegisteredBeneficiariesCount,
            DistributedBeneficiariesCount = c.DistributedBeneficiariesCount,
            IsActive = c.IsActive,
            IsClosed = c.IsClosed,
            CountryName = c.Country?.Name
        });

        return (campaignDtos, totalCount);
    }

    public async Task<IEnumerable<SeasonalAidCampaignListDto>> GetActiveCampaignsAsync()
    {
        _logger.LogInformation("Getting active campaigns");

        var campaigns = await _campaignRepository.GetActiveCampaignsAsync();

        // Country scope when the token carries one — campaigns are a shared catalogue.
        if (_currentUser.IsAuthenticated && _currentUser.CountryId.HasValue)
        {
            campaigns = campaigns.Where(c => !c.CountryId.HasValue || c.CountryId == _currentUser.CountryId);
        }

        return campaigns.Select(c => new SeasonalAidCampaignListDto
        {
            Id = c.Id,
            Name = c.Name,
            CampaignType = c.CampaignType,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            TotalBudget = c.TotalBudget,
            BudgetCurrency = c.BudgetCurrency,
            AllocatedBudget = c.AllocatedBudget,
            DistributedBudget = c.DistributedBudget,
            RegisteredBeneficiariesCount = c.RegisteredBeneficiariesCount,
            DistributedBeneficiariesCount = c.DistributedBeneficiariesCount,
            IsActive = c.IsActive,
            IsClosed = c.IsClosed,
            CountryName = c.Country?.Name
        });
    }

    #endregion

    #region UC-9.7: View Campaign Beneficiaries

    public async Task<(IEnumerable<SeasonalAidBeneficiaryDto> Items, int TotalCount)> GetBeneficiariesAsync(
        Guid campaignId, SeasonalAidBeneficiaryFilterDto filter)
    {
        _logger.LogInformation("Getting beneficiaries for campaign: {CampaignId}", campaignId);

        await GetScopedCampaignAsync(campaignId, "list beneficiaries");

        // A charity-bound caller reads registrations of their own families only.
        if (_currentUser.CharityId.HasValue)
        {
            filter.CharityId = _currentUser.CharityId;
        }

        var (beneficiaries, totalCount) = await _beneficiaryRepository.GetByCampaignFilteredPaginatedAsync(
            campaignId,
            searchTerm: filter.SearchTerm,
            isDistributed: filter.IsDistributed,
            charityId: filter.CharityId,
            regionId: filter.RegionId,
            centerId: filter.CenterId,
            registrationDateFrom: filter.RegistrationDateFrom,
            registrationDateTo: filter.RegistrationDateTo,
            distributionDateFrom: filter.DistributionDateFrom,
            distributionDateTo: filter.DistributionDateTo,
            pageNumber: filter.PageNumber,
            pageSize: filter.PageSize,
            sortBy: filter.SortBy,
            sortDescending: filter.SortDescending);

        var beneficiariesList = beneficiaries.ToList();
        var charityNames = await GetCharityNamesAsync(beneficiariesList.Select(b => b.Family?.FK_CharityId));

        var beneficiaryDtos = beneficiariesList.Select(b => new SeasonalAidBeneficiaryDto
        {
            Id = b.Id,
            CampaignId = b.CampaignId,
            FamilyId = b.FamilyId,
            FamilyCode = b.Family?.Code ?? string.Empty,
            FamilyAddress = b.Family?.Address,
            OrphansCount = b.Family?.OrphansCount ?? 0,
            FamilyMembersCount = b.Family?.FamilyMembersCount ?? 0,
            CharityName = b.Family?.FK_CharityId.HasValue == true && charityNames.TryGetValue(b.Family.FK_CharityId.Value, out var charityName)
                ? charityName
                : null,
            RegionName = b.Family?.City?.Name,
            CenterName = b.Family?.City?.Name,
            AllocationAmount = b.AllocationAmount,
            Currency = b.Currency,
            IsRegistered = b.IsRegistered,
            RegistrationDate = b.RegistrationDate,
            RegistrationNotes = b.RegistrationNotes,
            IsDistributed = b.IsDistributed,
            DistributionDate = b.DistributionDate,
            DistributedAmount = b.Distributions.Sum(d => d.AmountDistributed),
            ReceivedBy = b.Distributions.FirstOrDefault()?.ReceivedBy,
            Notes = b.Distributions.FirstOrDefault()?.Notes,
            CreatedOn = b.CreatedOn
        });

        return (beneficiaryDtos, totalCount);
    }

    public async Task<SeasonalAidBeneficiaryDto?> GetBeneficiaryAsync(Guid beneficiaryId)
    {
        _logger.LogInformation("Getting beneficiary: {BeneficiaryId}", beneficiaryId);

        var beneficiary = await _beneficiaryRepository.GetByIdAsync(beneficiaryId);

        if (beneficiary == null)
        {
            return null;
        }

        // GetByIdAsync does not load navigations; scope-check via the owning campaign and,
        // for a charity-bound caller, the family's own charity.
        var campaign = await _campaignRepository.GetByIdAsync(beneficiary.CampaignId);
        if (campaign == null || !IsCampaignVisibleToCaller(campaign))
        {
            return null;
        }

        var family = await _familyRepository.GetByIdAsync(beneficiary.FamilyId);
        if (_currentUser.CharityId.HasValue && family?.FK_CharityId != _currentUser.CharityId)
        {
            return null;
        }

        var distributions = await _distributionRepository.GetByBeneficiaryAsync(beneficiary.Id);
        var latestDistribution = distributions.OrderByDescending(d => d.DistributionDate).FirstOrDefault();
        var charityNames = await GetCharityNamesAsync(new[] { family?.FK_CharityId });

        return new SeasonalAidBeneficiaryDto
        {
            Id = beneficiary.Id,
            CampaignId = beneficiary.CampaignId,
            FamilyId = beneficiary.FamilyId,
            FamilyCode = family?.Code ?? string.Empty,
            FamilyAddress = family?.Address,
            OrphansCount = family?.OrphansCount ?? 0,
            FamilyMembersCount = family?.FamilyMembersCount ?? 0,
            CharityName = family?.FK_CharityId.HasValue == true && charityNames.TryGetValue(family.FK_CharityId.Value, out var charityName)
                ? charityName
                : null,
            RegionName = family?.City?.Name,
            CenterName = family?.City?.Name,
            AllocationAmount = beneficiary.AllocationAmount,
            Currency = beneficiary.Currency,
            IsRegistered = beneficiary.IsRegistered,
            RegistrationDate = beneficiary.RegistrationDate,
            RegistrationNotes = beneficiary.RegistrationNotes,
            IsDistributed = beneficiary.IsDistributed,
            DistributionDate = beneficiary.DistributionDate,
            DistributedAmount = distributions.Sum(d => d.AmountDistributed),
            ReceivedBy = latestDistribution?.ReceivedBy,
            Notes = latestDistribution?.Notes,
            CreatedOn = beneficiary.CreatedOn
        };
    }

    #endregion

    #region UC-9.8: Update Campaign Details

    public async Task<SeasonalAidCampaignDto> UpdateCampaignAsync(UpdateSeasonalAidCampaignDto dto)
    {
        _logger.LogInformation("Updating campaign: {Id}", dto.Id);

        _updateCampaignValidator.ValidateAndThrow(dto);

        var campaign = await GetScopedCampaignAsync(dto.Id, "update");

        if (campaign.IsClosed)
        {
            throw new InvalidOperationException("Cannot modify a closed campaign");
        }

        // The country follows the caller's claim exactly as on create; campaigns carry no
        // charity, so there is no charity tenancy to preserve.
        int? countryId = dto.CountryId ?? campaign.CountryId;
        if (_currentUser.CountryId.HasValue)
        {
            countryId = _currentUser.CountryId;
        }

        // Validate uniqueness
        if (!await IsCampaignNameUniqueAsync(dto.Name, dto.Id))
        {
            throw new InvalidOperationException($"Campaign with name '{dto.Name}' already exists");
        }

        // Validate date range
        if (dto.EndDate < dto.StartDate)
        {
            throw new ArgumentException("End date must be greater than or equal to start date");
        }

        // Update fields
        campaign.Name = dto.Name;
        campaign.CampaignType = dto.CampaignType;
        campaign.Description = dto.Description;
        campaign.StartDate = dto.StartDate;
        campaign.EndDate = dto.EndDate;
        campaign.TotalBudget = dto.TotalBudget;
        campaign.BudgetCurrency = dto.BudgetCurrency;
        campaign.PerFamilyAllocation = dto.PerFamilyAllocation;
        campaign.CountryId = countryId;
        campaign.RegionId = dto.RegionId;
        campaign.CenterId = dto.CenterId;
        campaign.MaximumFamilies = dto.MaximumFamilies;
        campaign.FamilyType = dto.FamilyType;
        campaign.MinChildrenAge = dto.MinChildrenAge;
        campaign.MaxChildrenAge = dto.MaxChildrenAge;
        campaign.IsActive = dto.IsActive;

        _campaignRepository.Update(campaign);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Campaign updated successfully: {Id}", dto.Id);

        return await GetCampaignByIdAsync(campaign.Id);
    }

    #endregion

    #region UC-9.9: Close Campaign

    public async Task CloseCampaignAsync(CloseCampaignDto dto)
    {
        _logger.LogInformation("Closing campaign: {CampaignId}", dto.CampaignId);

        var campaign = await GetScopedCampaignAsync(dto.CampaignId, "close");

        if (campaign.IsClosed)
        {
            throw new InvalidOperationException("Campaign is already closed");
        }

        campaign.IsClosed = true;
        campaign.ClosedDate = DateTime.UtcNow;
        campaign.ClosureNotes = dto.ClosureNotes;

        _campaignRepository.Update(campaign);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Campaign closed successfully: {CampaignId}", dto.CampaignId);
    }

    public async Task ReopenCampaignAsync(Guid campaignId)
    {
        _logger.LogInformation("Reopening campaign: {CampaignId}", campaignId);

        var campaign = await GetScopedCampaignAsync(campaignId, "reopen");

        if (!campaign.IsClosed)
        {
            throw new InvalidOperationException("Campaign is not closed");
        }

        campaign.IsClosed = false;
        campaign.ClosedDate = null;
        campaign.ClosureNotes = null;

        _campaignRepository.Update(campaign);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Campaign reopened successfully: {CampaignId}", campaignId);
    }

    #endregion

    #region UC-9.10: Generate Campaign Report

    public async Task<SeasonalAidCampaignReportDto> GenerateCampaignReportAsync(Guid campaignId)
    {
        _logger.LogInformation("Generating report for campaign: {CampaignId}", campaignId);

        var campaign = await GetScopedCampaignAsync(campaignId, "generate report");

        var beneficiaries = await _beneficiaryRepository.GetByCampaignWithDistributionsAsync(campaignId);
        var beneficiariesList = beneficiaries.ToList();
        var charityNames = await GetCharityNamesAsync(beneficiariesList.Select(b => b.Family?.FK_CharityId));

        var report = new SeasonalAidCampaignReportDto
        {
            CampaignId = campaign.Id,
            CampaignName = campaign.Name,
            CampaignType = campaign.CampaignType,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            Description = campaign.Description,
            TotalBudget = campaign.TotalBudget,
            BudgetCurrency = campaign.BudgetCurrency,
            AllocatedBudget = campaign.AllocatedBudget,
            DistributedBudget = campaign.DistributedBudget,
            RemainingBudget = campaign.TotalBudget - campaign.AllocatedBudget,
            TotalBeneficiaries = beneficiariesList.Count,
            DistributedBeneficiaries = beneficiariesList.Count(b => b.IsDistributed),
            PendingBeneficiaries = beneficiariesList.Count(b => !b.IsDistributed),
            BeneficiariesByRegion = (await _campaignRepository.GetBeneficiariesByRegionAsync(campaignId)),
            // Grouped here rather than in the repository: the repository groups by the
            // Family.Charity navigation, which is bound to a legacy column that stays null.
            BeneficiariesByCharity = beneficiariesList
                .GroupBy(b => b.Family?.FK_CharityId.HasValue == true && charityNames.TryGetValue(b.Family.FK_CharityId.Value, out var charityName)
                    ? charityName
                    : "Unknown")
                .ToDictionary(g => g.Key, g => g.Count()),
            IsActive = campaign.IsActive,
            IsClosed = campaign.IsClosed,
            ClosedDate = campaign.ClosedDate,
            ClosureNotes = campaign.ClosureNotes,
            ReportGeneratedOn = DateTime.UtcNow
        };

        // Build distribution details
        report.DistributionDetails = beneficiariesList.Select(b => new BeneficiaryDistributionDetail
        {
            BeneficiaryId = b.Id,
            FamilyCode = b.Family?.Code ?? string.Empty,
            FamilyAddress = b.Family?.Address,
            CharityName = b.Family?.FK_CharityId.HasValue == true && charityNames.TryGetValue(b.Family.FK_CharityId.Value, out var charityName)
                ? charityName
                : null,
            RegionName = b.Family?.City?.Name,
            AllocationAmount = b.AllocationAmount,
            DistributedAmount = b.Distributions.Sum(d => d.AmountDistributed),
            IsDistributed = b.IsDistributed,
            DistributionDate = b.DistributionDate,
            ReceivedBy = b.Distributions.FirstOrDefault()?.ReceivedBy,
            Notes = b.Distributions.FirstOrDefault()?.Notes
        }).ToList();

        // Calculate impact metrics
        report.TotalFamiliesServed = beneficiariesList.Count;
        report.TotalOrphansServed = beneficiariesList.Sum(b => b.Family?.OrphansCount ?? 0);
        report.EstimatedIndividualsServed = beneficiariesList.Sum(b => b.Family?.FamilyMembersCount ?? 0);

        // Build geographic coverage
        report.CoveredCountries = beneficiariesList
            .Where(b => b.Family?.City?.CountryId != null)
            .Select(b => b.Family?.City?.Country?.Name)
            .Distinct()
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList()!;

        report.CoveredRegions = beneficiariesList
            .Where(b => b.Family?.CityId != null)
            .Select(b => b.Family?.City?.Name)
            .Distinct()
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList()!;

        _logger.LogInformation("Report generated successfully for campaign: {CampaignId}", campaignId);

        return report;
    }

    public async Task<byte[]> ExportCampaignReportToPdfAsync(Guid campaignId)
    {
        _logger.LogInformation("Exporting campaign report to PDF: {CampaignId}", campaignId);

        var report = await GenerateCampaignReportAsync(campaignId);

        // TODO: Implement PDF generation using a library like iTextSharp or QuestPDF
        // For now, return a placeholder
        throw new NotImplementedException("PDF export not yet implemented");
    }

    public async Task<byte[]> ExportCampaignReportToExcelAsync(Guid campaignId)
    {
        _logger.LogInformation("Exporting campaign report to Excel: {CampaignId}", campaignId);

        var report = await GenerateCampaignReportAsync(campaignId);

        // TODO: Implement Excel generation using a library like EPPlus or ClosedXML
        // For now, return a placeholder
        throw new NotImplementedException("Excel export not yet implemented");
    }

    #endregion

    #region Additional Helper Methods

    public async Task<SeasonalAidCampaignDto?> GetCampaignByIdAsync(Guid id)
    {
        var campaign = await _campaignRepository.GetByIdAsync(id);

        if (campaign == null || !IsCampaignVisibleToCaller(campaign))
        {
            return null;
        }

        return MapToCampaignDto(campaign);
    }

    public async Task<SeasonalAidCampaignDto?> GetCampaignByNameAsync(string name)
    {
        var campaigns = await _campaignRepository.SearchAsync(name);
        var campaign = campaigns.FirstOrDefault();

        return campaign == null ? null : MapToCampaignDto(campaign);
    }

    public async Task<bool> IsCampaignNameUniqueAsync(string name, Guid? excludeId = null)
    {
        return await _campaignRepository.IsNameUniqueAsync(name, excludeId);
    }

    public async Task<bool> IsCampaignActiveAsync(Guid campaignId)
    {
        return await _campaignRepository.IsActiveAsync(campaignId);
    }

    public async Task<decimal> GetCampaignRemainingBudgetAsync(Guid campaignId)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId);
        if (campaign == null)
        {
            throw new KeyNotFoundException($"Campaign with ID '{campaignId}' not found");
        }

        var allocatedBudget = await _campaignRepository.GetAllocatedBudgetAsync(campaignId);
        return campaign.TotalBudget - allocatedBudget;
    }

    public async Task<int> GetCampaignAvailableSlotsAsync(Guid campaignId)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId);
        if (campaign == null)
        {
            throw new KeyNotFoundException($"Campaign with ID '{campaignId}' not found");
        }

        if (!campaign.MaximumFamilies.HasValue)
        {
            return int.MaxValue; // No limit
        }

        var currentBeneficiaries = await _campaignRepository.GetBeneficiaryCountAsync(campaignId);
        return campaign.MaximumFamilies.Value - currentBeneficiaries;
    }

    public async Task DeleteCampaignAsync(Guid id)
    {
        _logger.LogInformation("Deleting campaign: {Id}", id);

        var campaign = await GetScopedCampaignAsync(id, "delete");

        if (campaign.IsClosed)
        {
            throw new InvalidOperationException("Cannot delete a closed campaign");
        }

        // Counted from the store: the entity's calculated counters read an unloaded
        // navigation here and would always report zero.
        if (await _beneficiaryRepository.GetTotalBeneficiariesAsync(id) > 0)
        {
            throw new InvalidOperationException("Cannot delete a campaign with registered beneficiaries");
        }

        // Soft delete per the platform rule — the row stays, every read filters it out.
        campaign.IsActive = false;
        campaign.IsDeleted = true;
        _campaignRepository.Update(campaign);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Campaign deleted successfully: {Id}", id);
    }

    #endregion

    #region Private Helper Methods

    private SeasonalAidCampaignDto MapToCampaignDto(SeasonalAidCampaign campaign)
    {
        return new SeasonalAidCampaignDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            CampaignType = campaign.CampaignType,
            Description = campaign.Description,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            TotalBudget = campaign.TotalBudget,
            BudgetCurrency = campaign.BudgetCurrency,
            PerFamilyAllocation = campaign.PerFamilyAllocation,
            AllocatedBudget = campaign.AllocatedBudget,
            DistributedBudget = campaign.DistributedBudget,
            CountryId = campaign.CountryId,
            CountryName = campaign.Country != null ? (campaign.Country.NameAr ?? campaign.Country.NameEn) : null,
            RegionId = campaign.RegionId,
            RegionName = campaign.Region != null ? (campaign.Region.NameAr ?? campaign.Region.NameEn) : null,
            CenterId = campaign.CenterId,
            CenterName = campaign.Center != null ? (campaign.Center.NameAr ?? campaign.Center.NameEn) : null,
            MaximumFamilies = campaign.MaximumFamilies,
            FamilyType = campaign.FamilyType,
            MinChildrenAge = campaign.MinChildrenAge,
            MaxChildrenAge = campaign.MaxChildrenAge,
            RegisteredBeneficiariesCount = campaign.RegisteredBeneficiariesCount,
            DistributedBeneficiariesCount = campaign.DistributedBeneficiariesCount,
            IsActive = campaign.IsActive,
            IsClosed = campaign.IsClosed,
            ClosedDate = campaign.ClosedDate,
            ClosureNotes = campaign.ClosureNotes,
            CreatedOn = campaign.CreatedOn,
            CreatedBy = Guid.TryParse(campaign.CreatedBy, out var createdById) ? createdById.ToString() : null,
            UpdatedOn = campaign.UpdatedOn ?? DateTime.UtcNow,
            UpdatedBy = Guid.TryParse(campaign.UpdatedBy, out var updatedById) ? updatedById.ToString() : null
        };
    }

    private SeasonalAidDistributionDto MapToDistributionDto(
        SeasonalAidDistribution distribution, SeasonalAidBeneficiary beneficiary,
        string campaignName, string? familyCode)
    {
        return new SeasonalAidDistributionDto
        {
            Id = distribution.Id,
            BeneficiaryId = distribution.BeneficiaryId,
            FamilyCode = familyCode ?? string.Empty,
            CampaignName = campaignName,
            IsDistributed = distribution.IsDistributed,
            DistributionDate = distribution.DistributionDate,
            AmountDistributed = distribution.AmountDistributed,
            Currency = distribution.Currency,
            ReceivedBy = distribution.ReceivedBy,
            RecipientRelationship = distribution.RecipientRelationship,
            Notes = distribution.Notes,
            SignatureImageUrl = distribution.SignatureImageUrl,
            AttachmentId = distribution.AttachmentId,
            DistributionMethod = distribution.DistributionMethod,
            DistributorName = distribution.DistributorName,
            DistributorRole = distribution.DistributorRole,
            CreatedOn = distribution.CreatedOn,
            CreatedBy = Guid.TryParse(distribution.CreatedBy, out var distCreatedById) ? distCreatedById.ToString() : null
        };
    }

    #endregion
}
