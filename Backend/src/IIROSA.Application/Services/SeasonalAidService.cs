using Microsoft.Extensions.Logging;
using IIROSA.Domain.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.SeasonalAid;
using AutoMapper;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SeasonalAidService> _logger;

    public SeasonalAidService(
        ISeasonalAidCampaignRepository campaignRepository,
        ISeasonalAidBeneficiaryRepository beneficiaryRepository,
        ISeasonalAidDistributionRepository distributionRepository,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        ILogger<SeasonalAidService> logger)
    {
        _campaignRepository = campaignRepository;
        _beneficiaryRepository = beneficiaryRepository;
        _distributionRepository = distributionRepository;
        _familyRepository = familyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #region UC-9.1: Create Seasonal Aid Campaign

    public async Task<SeasonalAidCampaignDto> CreateCampaignAsync(CreateSeasonalAidCampaignDto dto)
    {
        _logger.LogInformation("Creating new seasonal aid campaign: {Name}", dto.Name);

        // Validate uniqueness
        if (!await IsCampaignNameUniqueAsync(dto.Name))
        {
            throw new InvalidOperationException($"Campaign with name '{dto.Name}' already exists");
        }

        // Validate date range
        if (dto.EndDate < dto.StartDate)
        {
            throw new ArgumentException("End date must be greater than or equal to start date");
        }

        // Validate budget
        if (dto.TotalBudget <= 0)
        {
            throw new ArgumentException("Total budget must be greater than zero");
        }

        if (dto.PerFamilyAllocation <= 0)
        {
            throw new ArgumentException("Per-family allocation must be greater than zero");
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
            CountryId = dto.CountryId,
            RegionId = dto.RegionId,
            CenterId = dto.CenterId,
            CharityId = dto.CharityId,
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

        return await GetCampaignByIdAsync(campaign.Id);
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

        var campaign = await _campaignRepository.GetByIdAsync(dto.CampaignId);

        if (campaign == null)
        {
            throw new KeyNotFoundException($"Campaign with ID '{dto.CampaignId}' not found");
        }

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

    public async Task<(IEnumerable<SeasonalAidBeneficiaryDto> Items, int TotalCount)> GetEligibleFamiliesAsync(EligibleFamiliesFilterDto filter)
    {
        _logger.LogInformation("Getting eligible families for campaign: {CampaignId}", filter.CampaignId);

        var campaign = await _campaignRepository.GetByIdAsync(filter.CampaignId);
        if (campaign == null)
        {
            throw new KeyNotFoundException($"Campaign with ID '{filter.CampaignId}' not found");
        }

        // Build query for eligible families
        var familiesQuery = _familyRepository.IncludeNavigationProperties();

        // Apply geographic filters
        if (filter.CharityId.HasValue)
        {
            familiesQuery = familiesQuery.Where(f => f.CharityId == filter.CharityId.Value);
        }

        if (filter.RegionId.HasValue)
        {
            familiesQuery = familiesQuery.Where(f => f.CityId == filter.RegionId.Value);
        }

        if (filter.CenterId.HasValue)
        {
            familiesQuery = familiesQuery.Where(f => f.CityId == filter.CenterId.Value);
        }

        // Apply family type filter
        if (!string.IsNullOrEmpty(filter.FamilyType))
        {
            // This would need to be implemented based on your family classification logic
            // For now, we'll skip this filter
        }

        // Apply age range filter for children
        if (filter.MinChildrenAge.HasValue || filter.MaxChildrenAge.HasValue)
        {
            // This would need to query the Orphans table to check ages
            // For now, we'll skip this filter
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
            CharityName = f.Charity?.Name,
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

        _beneficiaryRepository.Delete(beneficiary);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Beneficiary removed successfully: {BeneficiaryId}", beneficiaryId);
    }

    #endregion

    #region UC-9.5: Record Aid Distribution

    public async Task<SeasonalAidDistributionDto> RecordDistributionAsync(CreateSeasonalAidDistributionDto dto)
    {
        _logger.LogInformation("Recording distribution for beneficiary: {BeneficiaryId}", dto.BeneficiaryId);

        var beneficiary = await _beneficiaryRepository.GetByIdAsync(dto.BeneficiaryId);

        if (beneficiary == null)
        {
            throw new KeyNotFoundException($"Beneficiary with ID '{dto.BeneficiaryId}' not found");
        }

        if (beneficiary.Campaign.IsClosed)
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

        return MapToDistributionDto(distribution, beneficiary);
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

        var (campaigns, totalCount) = await _campaignRepository.GetFilteredAsync(
            filter.SearchTerm,
            filter.CampaignType,
            filter.IsActive,
            filter.IsClosed,
            filter.CountryId,
            filter.RegionId,
            filter.CenterId,
            filter.CharityId);

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
            CharityName = c.Charity?.Name,
            CountryName = c.Country?.Name
        });

        return (campaignDtos, totalCount);
    }

    public async Task<IEnumerable<SeasonalAidCampaignListDto>> GetActiveCampaignsAsync()
    {
        _logger.LogInformation("Getting active campaigns");

        var campaigns = await _campaignRepository.GetActiveCampaignsAsync();

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
            CharityName = c.Charity?.Name,
            CountryName = c.Country?.Name
        });
    }

    #endregion

    #region UC-9.7: View Campaign Beneficiaries

    public async Task<(IEnumerable<SeasonalAidBeneficiaryDto> Items, int TotalCount)> GetBeneficiariesAsync(
        Guid campaignId, SeasonalAidBeneficiaryFilterDto filter)
    {
        _logger.LogInformation("Getting beneficiaries for campaign: {CampaignId}", campaignId);

        var (beneficiaries, totalCount) = await _beneficiaryRepository.GetByCampaignFilteredAsync(
            campaignId,
            filter.IsDistributed,
            filter.CharityId,
            filter.RegionId,
            filter.CenterId);

        var beneficiaryDtos = beneficiaries.Select(b => new SeasonalAidBeneficiaryDto
        {
            Id = b.Id,
            CampaignId = b.CampaignId,
            FamilyId = b.FamilyId,
            FamilyCode = b.Family?.Code ?? string.Empty,
            FamilyAddress = b.Family?.Address,
            OrphansCount = b.Family?.OrphansCount ?? 0,
            FamilyMembersCount = b.Family?.FamilyMembersCount ?? 0,
            CharityName = b.Family?.Charity?.Name,
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

        var latestDistribution = beneficiary.Distributions.OrderByDescending(d => d.DistributionDate).FirstOrDefault();

        return new SeasonalAidBeneficiaryDto
        {
            Id = beneficiary.Id,
            CampaignId = beneficiary.CampaignId,
            FamilyId = beneficiary.FamilyId,
            FamilyCode = beneficiary.Family?.Code ?? string.Empty,
            FamilyAddress = beneficiary.Family?.Address,
            OrphansCount = beneficiary.Family?.OrphansCount ?? 0,
            FamilyMembersCount = beneficiary.Family?.FamilyMembersCount ?? 0,
            CharityName = beneficiary.Family?.Charity?.Name,
            RegionName = beneficiary.Family?.City?.Name,
            CenterName = beneficiary.Family?.City?.Name,
            AllocationAmount = beneficiary.AllocationAmount,
            Currency = beneficiary.Currency,
            IsRegistered = beneficiary.IsRegistered,
            RegistrationDate = beneficiary.RegistrationDate,
            RegistrationNotes = beneficiary.RegistrationNotes,
            IsDistributed = beneficiary.IsDistributed,
            DistributionDate = beneficiary.DistributionDate,
            DistributedAmount = beneficiary.Distributions.Sum(d => d.AmountDistributed),
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

        var campaign = await _campaignRepository.GetByIdAsync(dto.Id);
        if (campaign == null)
        {
            throw new KeyNotFoundException($"Campaign with ID '{dto.Id}' not found");
        }

        if (campaign.IsClosed)
        {
            throw new InvalidOperationException("Cannot modify a closed campaign");
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
        campaign.CountryId = dto.CountryId;
        campaign.RegionId = dto.RegionId;
        campaign.CenterId = dto.CenterId;
        campaign.CharityId = dto.CharityId;
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

        var campaign = await _campaignRepository.GetByIdAsync(dto.CampaignId);

        if (campaign == null)
        {
            throw new KeyNotFoundException($"Campaign with ID '{dto.CampaignId}' not found");
        }

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

        var campaign = await _campaignRepository.GetByIdAsync(campaignId);
        if (campaign == null)
        {
            throw new KeyNotFoundException($"Campaign with ID '{campaignId}' not found");
        }

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

        var campaign = await _campaignRepository.GetByIdAsync(campaignId);

        if (campaign == null)
        {
            throw new KeyNotFoundException($"Campaign with ID '{campaignId}' not found");
        }

        var beneficiaries = await _beneficiaryRepository.GetByCampaignWithDistributionsAsync(campaignId);

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
            TotalBeneficiaries = beneficiaries.Count(),
            DistributedBeneficiaries = beneficiaries.Count(b => b.IsDistributed),
            PendingBeneficiaries = beneficiaries.Count(b => !b.IsDistributed),
            BeneficiariesByRegion = (await _campaignRepository.GetBeneficiariesByRegionAsync(campaignId)),
            BeneficiariesByCharity = (await _campaignRepository.GetBeneficiariesByCharityAsync(campaignId)),
            IsActive = campaign.IsActive,
            IsClosed = campaign.IsClosed,
            ClosedDate = campaign.ClosedDate,
            ClosureNotes = campaign.ClosureNotes,
            ReportGeneratedOn = DateTime.UtcNow
        };

        // Build distribution details
        report.DistributionDetails = beneficiaries.Select(b => new BeneficiaryDistributionDetail
        {
            BeneficiaryId = b.Id,
            FamilyCode = b.Family?.Code ?? string.Empty,
            FamilyAddress = b.Family?.Address,
            CharityName = b.Family?.Charity?.Name,
            RegionName = b.Family?.City?.Name,
            AllocationAmount = b.AllocationAmount,
            DistributedAmount = b.Distributions.Sum(d => d.AmountDistributed),
            IsDistributed = b.IsDistributed,
            DistributionDate = b.DistributionDate,
            ReceivedBy = b.Distributions.FirstOrDefault()?.ReceivedBy,
            Notes = b.Distributions.FirstOrDefault()?.Notes
        }).ToList();

        // Calculate impact metrics
        report.TotalFamiliesServed = beneficiaries.Count();
        report.TotalOrphansServed = beneficiaries.Sum(b => b.Family?.OrphansCount ?? 0);
        report.EstimatedIndividualsServed = beneficiaries.Sum(b => b.Family?.FamilyMembersCount ?? 0);

        // Build geographic coverage
        report.CoveredCountries = beneficiaries
            .Where(b => b.Family?.City?.CountryId != null)
            .Select(b => b.Family?.City?.Country?.Name)
            .Distinct()
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList()!;

        report.CoveredRegions = beneficiaries
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

    #region UC-9.11: Assign Campaign to Charity

    public async Task AssignCampaignToCharityAsync(Guid campaignId, Guid? charityId)
    {
        _logger.LogInformation("Assigning campaign {CampaignId} to charity {CharityId}", campaignId, charityId);

        var campaign = await _campaignRepository.GetByIdAsync(campaignId);
        if (campaign == null)
        {
            throw new KeyNotFoundException($"Campaign with ID '{campaignId}' not found");
        }

        if (campaign.IsClosed)
        {
            throw new InvalidOperationException("Cannot modify charity assignment for a closed campaign");
        }

        campaign.CharityId = charityId;

        _campaignRepository.Update(campaign);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Campaign assigned to charity successfully: {CampaignId}", campaignId);
    }

    #endregion

    #region Additional Helper Methods

    public async Task<SeasonalAidCampaignDto?> GetCampaignByIdAsync(Guid id)
    {
        var campaign = await _campaignRepository.GetByIdAsync(id);

        if (campaign == null)
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

        var campaign = await _campaignRepository.GetByIdAsync(id);
        if (campaign == null)
        {
            throw new KeyNotFoundException($"Campaign with ID '{id}' not found");
        }

        if (campaign.IsClosed)
        {
            throw new InvalidOperationException("Cannot delete a closed campaign");
        }

        if (campaign.RegisteredBeneficiariesCount > 0)
        {
            throw new InvalidOperationException("Cannot delete a campaign with registered beneficiaries");
        }

        _campaignRepository.Delete(campaign);
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
            CharityId = campaign.CharityId,
            CharityName = campaign.Charity?.Name,
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

    private SeasonalAidDistributionDto MapToDistributionDto(SeasonalAidDistribution distribution, SeasonalAidBeneficiary beneficiary)
    {
        return new SeasonalAidDistributionDto
        {
            Id = distribution.Id,
            BeneficiaryId = distribution.BeneficiaryId,
            FamilyCode = beneficiary.Family?.Code ?? string.Empty,
            CampaignName = beneficiary.Campaign?.Name ?? string.Empty,
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
