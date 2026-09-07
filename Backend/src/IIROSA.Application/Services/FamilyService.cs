using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.Family;
using IIROSA.Application.DTOs.Reports;
using IIROSA.Application.Exceptions;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using AutoMapper;
using FluentValidation;
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
    private readonly IIROSA.Application.Interfaces.ICharityWriteGuard _charityWriteGuard;
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
    private readonly IValidator<OrphanSearchFilterDto> _orphanSearchValidator;
    private readonly IValidator<OrphanEligibilityCheckDto> _orphanEligibilityValidator;
    private readonly IValidator<OrphanCodeCheckFilterDto> _orphanCodeCheckValidator;
    private readonly IValidator<AssignOrphanCodeDto> _assignOrphanCodeValidator;
    private readonly IValidator<PhoneCheckFilterDto> _phoneCheckValidator;
    private readonly IValidator<CheckFamilyNationalIdDto> _familyNationalIdCheckValidator;
    private readonly IFamilyCharityTransferRepository _transferRepository;
    private readonly IValidator<TransferFamilyDto> _transferFamilyValidator;
    private readonly IValidator<MemberControlDto> _memberControlValidator;
    // UC-HOU-03: housing allocation lookups (6-5 catalogues) for flat⊂building resolution
    private readonly ILookupRepository<Domain.Entities.Lookups.HousingBuilding> _housingBuildingLookupRepository;
    private readonly ILookupRepository<Domain.Entities.Lookups.HousingFlat> _housingFlatLookupRepository;

    // Review P3 2026-08-26: the 18-13 follow-up read scopes from CLAIMS, not from
    // controller-supplied strings (the rest of this service keeps its historical
    // controller-passed scoping parameters — the families epic owns that pattern).
    private readonly IIROSA.Application.Interfaces.ICurrentUserService _currentUser;
    // UC-REF-03: resolves to CreateRefugeeFamilyValidator (the only IValidator<CreateFamilyDto>)
    private readonly IValidator<CreateFamilyDto> _createFamilyValidator;
    // UC-FAM-11: follow-up report filter (unique DTO type, safe via DI)
    private readonly IValidator<FamilyFollowUpFilterDto> _followUpFilterValidator;
    private readonly IValidator<FamilyEntryTrackingFilterDto> _familyEntryTrackingValidator;
    // UC-FAM-13: guardian sponsorship-link removal (التعليق bound from query)
    private readonly IValidator<RemoveProviderSponsorLinkDto> _removeSponsorLinkValidator;

    // UC-HOU-03 (§11.S.2 mandatory flags): instantiated directly, NOT via IValidator<CreateFamilyDto> —
    // a second DI registration would hijack the refugee validator's resolution above.
    // FluentValidation validators are stateless, so direct construction is safe.
    private readonly Validators.Family.CreateHousingFamilyValidator _createHousingFamilyValidator = new();

    // UC-REF-04 (§12.S.2 on update): same direct-construction precedent — UpdateFamilyAsync
    // validates the effective post-copy state of a Refugee-register family with it.
    private readonly Validators.Family.UpdateRefugeeFamilyValidator _updateRefugeeFamilyValidator = new();

    public FamilyService(
        IFamilyRepository familyRepository,
        IIROSA.Application.Interfaces.ICharityWriteGuard charityWriteGuard,
        IFatherRepository fatherRepository,
        IMotherRepository motherRepository,
        IProviderRepository providerRepository,
        IRelativeRepository relativeRepository,
        IOrphanRepository orphanRepository,
        ICharityRepository charityRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<FamilyService> logger,
        AttachmentService attachmentService,
        IValidator<OrphanSearchFilterDto> orphanSearchValidator,
        IValidator<OrphanEligibilityCheckDto> orphanEligibilityValidator,
        IValidator<OrphanCodeCheckFilterDto> orphanCodeCheckValidator,
        IValidator<AssignOrphanCodeDto> assignOrphanCodeValidator,
        IValidator<PhoneCheckFilterDto> phoneCheckValidator,
        IValidator<CheckFamilyNationalIdDto> familyNationalIdCheckValidator,
        IFamilyCharityTransferRepository transferRepository,
        IValidator<TransferFamilyDto> transferFamilyValidator,
        IValidator<MemberControlDto> memberControlValidator,
        IValidator<CreateFamilyDto> createFamilyValidator,
        IValidator<FamilyFollowUpFilterDto> followUpFilterValidator,
        IValidator<FamilyEntryTrackingFilterDto> familyEntryTrackingValidator,
        IValidator<RemoveProviderSponsorLinkDto> removeSponsorLinkValidator,
        ILookupRepository<Domain.Entities.Lookups.HousingBuilding> housingBuildingLookupRepository,
        ILookupRepository<Domain.Entities.Lookups.HousingFlat> housingFlatLookupRepository,
        IIROSA.Application.Interfaces.ICurrentUserService currentUser)
    {
        _familyRepository = familyRepository;
        _fatherRepository = fatherRepository;
        _charityWriteGuard = charityWriteGuard;
        _motherRepository = motherRepository;
        _providerRepository = providerRepository;
        _relativeRepository = relativeRepository;
        _orphanRepository = orphanRepository;
        _charityRepository = charityRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _followUpFilterValidator = followUpFilterValidator;
        _familyEntryTrackingValidator = familyEntryTrackingValidator;
        _removeSponsorLinkValidator = removeSponsorLinkValidator;
        _logger = logger;
        _attachmentService = attachmentService;
        _orphanSearchValidator = orphanSearchValidator;
        _orphanEligibilityValidator = orphanEligibilityValidator;
        _orphanCodeCheckValidator = orphanCodeCheckValidator;
        _assignOrphanCodeValidator = assignOrphanCodeValidator;
        _phoneCheckValidator = phoneCheckValidator;
        _familyNationalIdCheckValidator = familyNationalIdCheckValidator;
        _transferRepository = transferRepository;
        _transferFamilyValidator = transferFamilyValidator;
        _memberControlValidator = memberControlValidator;
        _createFamilyValidator = createFamilyValidator;
        _housingBuildingLookupRepository = housingBuildingLookupRepository;
        _housingFlatLookupRepository = housingFlatLookupRepository;
        _currentUser = currentUser;
    }

    #region UC-4.1: Register Family

    public async Task<FamilyDto> CreateFamilyAsync(CreateFamilyDto dto)
    {
        _logger.LogInformation("Creating new family with head: {HeadOfFamily}", dto.HeadOfFamily);

        // UC-CHR-08: head office can disable adding for a charity, and UC-CHR-07 can lock it
        // outright. Enforced here because this is the charity-facing write, not in the controller.
        await _charityWriteGuard.EnsureCanAddAsync();

        // Register discriminator (epic 7, UC-REF-03 / 6-1 convention): string on the wire,
        // parsed with Enum.TryParse — absent/unknown ⇒ Regular (legacy behaviour).
        var familyType = Domain.Enums.FamilyType.Regular;
        if (!string.IsNullOrWhiteSpace(dto.FamilyType))
        {
            // On parse failure the out param is default(FamilyType) = 0 — an UNDEFINED
            // discriminator that would vanish from every register filter. Guard it: an
            // absent/unknown value keeps Regular (legacy behaviour), never 0.
            if (!Enum.TryParse<Domain.Enums.FamilyType>(dto.FamilyType, ignoreCase: true, out var parsedType)
                || !Enum.IsDefined(parsedType))
            {
                parsedType = Domain.Enums.FamilyType.Regular;
            }
            familyType = parsedType;
        }

        // §12.S.2 mandatory flags + charity-scope rule — refugee creates only.
        if (familyType == Domain.Enums.FamilyType.Refugee)
        {
            await _createFamilyValidator.ValidateAndThrowAsync(dto);

            // «أحد المعيلين مكرر من قبل أكثر من مرة» — checked BEFORE the family row is saved
            // (the create flow saves the family before its members; a late throw would leave a
            // partial family behind). AddProviderToFamilyInternalAsync keeps the same guard for
            // the standalone member endpoint.
            if (dto.Provider != null && await _providerRepository.IsNationalIdExistsAsync(dto.Provider.NationalId))
            {
                throw new Exceptions.BusinessException("أحد المعيلين مكرر من قبل أكثر من مرة");
            }
        }

        // §11.S.2 mandatory flags — housing creates (UC-HOU-03; flat⊂building + allocation
        // lookups are additionally enforced by AddNewHousingFamilyAsync).
        if (familyType == Domain.Enums.FamilyType.Housing)
        {
            await _createHousingFamilyValidator.ValidateAndThrowAsync(dto);

            // «أحد المعيلين مكرر من قبل أكثر من مرة» — checked BEFORE the family row is
            // saved, mirroring the refugee branch above (review 2026-08-24, High): the
            // create flow commits the family at the SaveChanges below, and a late guard
            // inside AddProviderToFamilyInternalAsync left a phantom guardian-less family
            // behind on every refused retry.
            if (dto.Provider != null && await _providerRepository.IsNationalIdExistsAsync(dto.Provider.NationalId))
            {
                throw new Exceptions.BusinessException("أحد المعيلين مكرر من قبل أكثر من مرة");
            }
        }

        // Generate code if not provided
        var code = dto.Code ?? await GenerateFamilyCodeAsync();

        // Household register fields are stamped for BOTH non-regular registers — refugee (§12.S.2)
        // and housing (§11.S.2) share the Family household columns; Regular rows keep them null.
        var isHouseholdRegister = familyType is Domain.Enums.FamilyType.Refugee or Domain.Enums.FamilyType.Housing;

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
            FK_CharityId = dto.CharityId, // Charity callers are pinned by the controller; HQ must name it (refugee validator enforces)
            // CharityId is the mirror the EF Charity navigation binds to (FamilyConfiguration) —
            // every resolved charity name reads it. Stamp BOTH columns like TransferFamilyToCharityAsync
            // does; stamping only one left charityName null for every API-created family (review P1).
            CharityId = dto.CharityId,
            IsActive = true,
            FamilyMembersCount = 0,
            OrphansCount = 0,
            FamilyType = familyType,
            // Household register fields (§12.S.2 refugee / §11.S.2 housing) — null on Regular rows
            RegionId = isHouseholdRegister ? dto.RegionId : null,
            CenterId = isHouseholdRegister ? dto.CenterId : null,
            NearBy = isHouseholdRegister ? dto.NearBy : null,
            Street = isHouseholdRegister ? dto.Street : null,
            RentAmount = isHouseholdRegister ? dto.RentAmount : null,
            HouseOwnershipId = isHouseholdRegister ? dto.HouseOwnershipId : null,
            HouseStatusId = isHouseholdRegister ? dto.HouseStatusId : null,
            IncomeTypeId = isHouseholdRegister ? dto.IncomeTypeId : null,
            // Housing allocation (§11.S.2 رقم العماره / رقم الشقه) — housing rows only
            FK_HousingBuildingId = familyType == Domain.Enums.FamilyType.Housing ? dto.HousingBuildingId : null,
            FK_HousingFlatId = familyType == Domain.Enums.FamilyType.Housing ? dto.HousingFlatId : null
        };

        await _familyRepository.AddAsync(family);
        await _unitOfWork.SaveChangesAsync();

        // A refugee family (§12.S.2) and a housing family (§11.S.2) have no father/mother
        // sections — guardian (provider) + children instead.
        if (isHouseholdRegister)
        {
            if (dto.Provider != null)
            {
                await AddProviderToFamilyInternalAsync(family.Id, dto.Provider);
            }
        }
        else
        {
            // Add father (mandatory for the regular register)
            if (dto.Father != null)
            {
                await AddFatherToFamilyInternalAsync(family.Id, dto.Father);
            }
            else
            {
                throw new ArgumentException("Father information is required");
            }

            // Add mother (mandatory for the regular register)
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

    /// <summary>
    /// UC-HOU-03 (§11.U.3): register a housing family. Forces the Housing discriminator
    /// server-side (a housing-endpoint caller can never mint a Refugee/Regular row), verifies
    /// the building/flat allocation lookups exist and that the flat belongs to the chosen
    /// building, then delegates to the shared create path (§11.S.2 mandatory flags are
    /// enforced there by CreateHousingFamilyValidator; the duplicate-guardian rule
    /// «أحد المعيلين مكرر من قبل أكثر من مرة» fires in AddProviderToFamilyInternalAsync).
    /// </summary>
    public async Task<FamilyDto> AddNewHousingFamilyAsync(CreateFamilyDto dto)
    {
        // Server-side discriminator stamp — never trusted from the client.
        dto.FamilyType = nameof(Domain.Enums.FamilyType.Housing);

        if (dto.HousingBuildingId is null || dto.HousingFlatId is null)
        {
            // The validator flags the fields with proper messages; this guard keeps the
            // allocation resolution below from dereferencing nulls.
            await _createHousingFamilyValidator.ValidateAndThrowAsync(dto);
        }

        var building = await _housingBuildingLookupRepository.GetByIdAsync(dto.HousingBuildingId!.Value);
        if (building == null)
        {
            throw new Exceptions.BusinessException("رقم العماره غير موجود");
        }

        var flat = await _housingFlatLookupRepository.GetByIdAsync(dto.HousingFlatId!.Value);
        if (flat == null || flat.BuildingId != building.Id)
        {
            throw new Exceptions.BusinessException("الشقه لا تتبع العماره المختاره");
        }

        // Review 2026-08-24 (decision D2): a flat is exclusive to one live housing family —
        // refuse the allocation when another non-deleted housing family already holds it.
        var flatTaken = await _familyRepository.AsQueryable()
            .AnyAsync(f => f.FK_HousingFlatId == flat.Id
                        && f.FamilyType == Domain.Enums.FamilyType.Housing
                        && !f.IsDeleted);
        if (flatTaken)
        {
            throw new Exceptions.BusinessException("الشقه مسجله بالفعل لأسرة أخرى");
        }

        return await CreateFamilyAsync(dto);
    }

    /// <summary>
    /// UC-HOU-04 (§11.U.4): the housing-family aggregate for the view/edit screen. Unknown,
    /// non-housing and foreign rows all answer <see cref="Exceptions.NotFoundException"/> — a
    /// foreign id must not prove the record exists (pin-never-widen, OfficeProjectService shape).
    /// </summary>
    public async Task<HousingFamilyDetailDto> GetHousingFamilyAsync(Guid id, Guid? userCharityId, string? userRole)
    {
        var family = await _familyRepository.IncludeNavigationProperties()
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted
                && f.FamilyType == Domain.Enums.FamilyType.Housing);

        if (family == null || IsOutsideCallerCharityScope(family, userCharityId, userRole))
        {
            throw new Exceptions.NotFoundException(typeof(Family), id);
        }

        return await MapToHousingFamilyDetailAsync(family);
    }

    /// <summary>
    /// UC-HOU-07 (§11.U.7 البحث بالكود): the housing family's beneficiaries — each live child
    /// as a Child row (the sponsorship-code carrier) plus the guardian as a Parent row. A
    /// non-blank code resolves a child's code EXACTLY within the family's scope; the family is
    /// already charity-pinned by the 6-4 gate, so another charity's code simply does not match
    /// and the caller renders an explicit not-found from the EMPTY list. The guardian is picked
    /// from the list, never resolved by code (legacy resolves كود الطالب الابن).
    /// </summary>
    public async Task<List<DTOs.Family.HousingBeneficiaryDto>> GetHousingBeneficiariesAsync(
        Guid familyId, string? code, Guid? userCharityId, string? userRole)
    {
        var family = await _familyRepository.IncludeNavigationProperties()
            .FirstOrDefaultAsync(f => f.Id == familyId && !f.IsDeleted
                && f.FamilyType == Domain.Enums.FamilyType.Housing);

        if (family == null || IsOutsideCallerCharityScope(family, userCharityId, userRole))
        {
            throw new Exceptions.NotFoundException(typeof(Family), familyId);
        }

        var children = (await _orphanRepository.GetByFamilyIdAsync(family.Id))
            .Where(o => !o.IsDeleted);

        if (!string.IsNullOrWhiteSpace(code))
        {
            children = children.Where(o =>
                string.Equals(o.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        var beneficiaries = children.Select(o => new DTOs.Family.HousingBeneficiaryDto
        {
            BeneficiaryId = o.Id,
            ChildOrParent = nameof(Domain.Enums.ReportBeneficiaryType.Child),
            Code = o.Code,
            FullName = o.FullName,
            NationalId = o.NationalId,
            Age = o.DateOfBirth.HasValue ? CalculateAge(o.DateOfBirth.Value) : null
        }).ToList();

        // The guardian row joins only the unfiltered listing — a code query resolves children
        // alone, and a guardian carries no code by design.
        if (string.IsNullOrWhiteSpace(code) && family.Provider is { IsDeleted: false })
        {
            beneficiaries.Add(new DTOs.Family.HousingBeneficiaryDto
            {
                BeneficiaryId = family.Provider.Id,
                ChildOrParent = nameof(Domain.Enums.ReportBeneficiaryType.Parent),
                Code = null,
                FullName = family.Provider.FullName,
                NationalId = family.Provider.NationalId,
                Age = family.Provider.DateOfBirth.HasValue ? CalculateAge(family.Provider.DateOfBirth.Value) : null
            });
        }

        return beneficiaries;
    }

    /// <summary>
    /// UC-HOU-04 (§11.U.4): update a housing family under the same §11.S.2 contract as the
    /// create. Family fields + allocation (flat ⊂ building re-validated) + guardian + children
    /// sync (id-matched update, id-less add, absent soft-remove). Ownership NEVER moves — any
    /// client-sent charity field is ignored — and the register discriminator is immutable.
    /// </summary>
    public async Task<HousingFamilyDetailDto> UpdateHousingFamilyAsync(Guid id, CreateFamilyDto dto, Guid? userCharityId, string? userRole, string? userName = null)
    {
        // Review 2026-08-24: the charity write guard the regular update runs — a locked or
        // add-disabled charity must not rewrite through the housing register either.
        await _charityWriteGuard.EnsureCanUpdateAsync();

        var family = await _familyRepository.IncludeNavigationProperties()
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted
                && f.FamilyType == Domain.Enums.FamilyType.Housing);

        if (family == null || IsOutsideCallerCharityScope(family, userCharityId, userRole))
        {
            throw new Exceptions.NotFoundException(typeof(Family), id);
        }

        // Same §11.S.2 mandatory contract as the create — the update refuses an emptied field
        // exactly like the register does.
        await _createHousingFamilyValidator.ValidateAndThrowAsync(dto);

        // Allocation re-validated with the create refusals — a move to another flat/building
        // pair must still satisfy flat ⊂ building.
        var building = await _housingBuildingLookupRepository.GetByIdAsync(dto.HousingBuildingId!.Value);
        if (building == null)
        {
            throw new Exceptions.BusinessException("رقم العماره غير موجود");
        }

        var flat = await _housingFlatLookupRepository.GetByIdAsync(dto.HousingFlatId!.Value);
        if (flat == null || flat.BuildingId != building.Id)
        {
            throw new Exceptions.BusinessException("الشقه لا تتبع العماره المختاره");
        }

        // Review 2026-08-24 (decision D2): flat exclusivity re-checked on update, excluding
        // this family itself — moving to an occupied flat is refused; leaving a flat frees it.
        var flatTaken = await _familyRepository.AsQueryable()
            .AnyAsync(f => f.FK_HousingFlatId == flat.Id
                        && f.FamilyType == Domain.Enums.FamilyType.Housing
                        && !f.IsDeleted
                        && f.Id != family.Id);
        if (flatTaken)
        {
            throw new Exceptions.BusinessException("الشقه مسجله بالفعل لأسرة أخرى");
        }

        // Family-level §11.S.2 fields. FK_CharityId and FamilyType are deliberately NOT
        // stamped: ownership never moves and the discriminator is immutable.
        family.CityVillage = dto.CityVillage;
        family.RegionId = dto.RegionId;
        family.CenterId = dto.CenterId;
        family.Address = dto.Address;
        family.NearBy = dto.NearBy;
        family.Street = dto.Street;
        family.RentAmount = dto.RentAmount;
        family.IncomeTypeId = dto.IncomeTypeId;
        family.Notes = dto.Notes;
        family.PhoneNumber = dto.PhoneNumber;
        family.FK_HousingBuildingId = dto.HousingBuildingId;
        family.FK_HousingFlatId = dto.HousingFlatId;
        if (dto.Provider != null)
        {
            family.HeadOfFamily = dto.Provider.FullName;
        }
        _familyRepository.Update(family);

        // Guardian block — update the family's provider, or create it when the register
        // predates the guardian (none exists).
        if (dto.Provider != null)
        {
            var provider = await _providerRepository.GetByFamilyIdAsync(family.Id);
            if (provider == null)
            {
                // AddProviderToFamilyInternalAsync runs the duplicate-guardian guard
                // («أحد المعيلين مكرر من قبل أكثر من مرة») before any write.
                await AddProviderToFamilyInternalAsync(family.Id, dto.Provider);
            }
            else
            {
                // The rule re-checked against the NEW national id, excluding this provider
                // itself — editing another field of the same guardian is not a duplication.
                if (await _providerRepository.IsNationalIdExistsAsync(dto.Provider.NationalId, provider.Id))
                {
                    throw new Exceptions.BusinessException("أحد المعيلين مكرر من قبل أكثر من مرة");
                }

                provider.FullName = dto.Provider.FullName;
                provider.RelationshipToFamily = dto.Provider.RelationshipToFamily;
                provider.NationalId = dto.Provider.NationalId;
                provider.Phone = dto.Provider.Phone;
                provider.Address = dto.Provider.Address;
                provider.Job = dto.Provider.Job;
                provider.MonthlyIncome = dto.Provider.MonthlyIncome;
                provider.Notes = dto.Provider.Notes;
                provider.DateOfBirth = dto.Provider.DateOfBirth;
                provider.NationalityCountryId = dto.Provider.NationalityCountryId;
                provider.ReasonOfRelationId = dto.Provider.ReasonOfRelationId;
                // Housing register extensions (§11.S.2 اضافة معيل)
                provider.RelationId = dto.Provider.RelationId;
                provider.MainRelation = dto.Provider.MainRelation;
                provider.SocialStatusId = dto.Provider.SocialStatusId;
                provider.HealthStatusId = dto.Provider.HealthStatusId;
                provider.EducationLevelId = dto.Provider.EducationLevelId;
                provider.WidowSponsorship = dto.Provider.WidowSponsorship;
                provider.AnotherSponsor = dto.Provider.AnotherSponsor;
                provider.MotherIsMar = dto.Provider.MotherIsMar;
                provider.IsCaring = dto.Provider.IsCaring;
                _providerRepository.Update(provider);
            }
        }

        // Children sync: a payload child carrying an id updates that orphan; one without an id
        // is added; an existing orphan absent from the payload is soft-removed. The §11.S.2
        // edit screen owns the full child set, so absence IS a removal instruction.
        var existingChildren = (await _orphanRepository.GetByFamilyIdAsync(family.Id))
            .Where(o => !o.IsDeleted)
            .ToList();
        var payloadIds = new HashSet<Guid>();

        foreach (var child in dto.Orphans ?? Enumerable.Empty<CreateOrphanDto>())
        {
            if (child.Id.HasValue && child.Id.Value != Guid.Empty)
            {
                var orphan = existingChildren.FirstOrDefault(o => o.Id == child.Id.Value);
                if (orphan == null)
                {
                    // Foreign orphan id on this family's update — refuse, never silently skip
                    throw new Exceptions.BusinessException("أحد الأبناء لا ينتمي لهذه الأسرة");
                }

                orphan.FullName = child.FullName;
                orphan.DateOfBirth = child.DateOfBirth;
                orphan.Gender = child.Gender;
                orphan.NationalId = child.NationalId;
                orphan.EducationLevelId = child.EducationLevelId;
                orphan.SchoolName = child.SchoolName;
                orphan.GradeClass = child.GradeClass;
                orphan.HealthStatusId = child.HealthStatusId;
                orphan.Notes = child.Notes;
                orphan.SocialStatusId = child.SocialStatusId;
                // Housing register extensions (§11.S.2 اضافة ابن)
                orphan.Profession = child.Profession;
                orphan.DepartmentName = child.DepartmentName;
                orphan.FacultyName = child.FacultyName;
                orphan.EducationalQualificationId = child.EducationalQualificationId;
                _orphanRepository.Update(orphan);
                payloadIds.Add(orphan.Id);
            }
            else
            {
                var created = await AddOrphanToFamilyInternalAsync(family.Id, child);
                payloadIds.Add(created.Id);
            }
        }

        foreach (var removed in existingChildren.Where(o => !payloadIds.Contains(o.Id)))
        {
            // Soft delete (platform convention — IsDeleted via the change tracker, never a hard row loss).
            // Review 2026-08-24: stamp the full deletion audit like the provider detach does —
            // IsDeleted alone left DeletedOn/DeletedBy null, breaking the audit trail.
            removed.IsDeleted = true;
            removed.DeletedOn = DateTime.UtcNow;
            removed.DeletedBy = userName ?? "HQ";
            _orphanRepository.Update(removed);
        }

        await _unitOfWork.SaveChangesAsync();

        return await MapToHousingFamilyDetailAsync(family);
    }

    /// <summary>
    /// UC-HOU-04 tenancy gate: a Charity-role caller may only touch rows owned by their own
    /// charity; HQ roles (Admin/SuperAdmin) have no pin. Unknown vs foreign both surface as
    /// 404 at the caller — the check itself never reveals which one fired.
    /// </summary>
    private static bool IsOutsideCallerCharityScope(Family family, Guid? userCharityId, string? userRole)
    {
        return string.Equals(userRole, "Charity", StringComparison.OrdinalIgnoreCase)
            && (userCharityId is null || family.FK_CharityId != userCharityId.Value);
    }

    /// <summary>
    /// UC-HOU-04: the shared FamilyDto projection lifted onto the housing detail aggregate,
    /// with the family's children attached as full OrphanDto rows (every §11.S.2 field).
    /// </summary>
    private async Task<HousingFamilyDetailDto> MapToHousingFamilyDetailAsync(Family family)
    {
        var detail = _mapper.Map<HousingFamilyDetailDto>(await MapToFamilyDtoAsync(family));
        var children = await _orphanRepository.GetByFamilyIdAsync(family.Id);
        detail.Children = children
            .Where(o => !o.IsDeleted)
            .Select(o => _mapper.Map<OrphanDto>(o))
            .ToList();
        return detail;
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

    public async Task<ProviderDto> AddProviderToFamilyAsync(Guid familyId, CreateProviderDto dto, Guid? userCharityId, string? userRole)
    {
        _logger.LogInformation("Adding provider to family: {FamilyId}", familyId);

        // UC-FAM-12: a guardian may only be attached while the charity's add permission is on
        // (UC-CHR-08/07) — same guard as family create, applied to the standalone attach path.
        await _charityWriteGuard.EnsureCanAddAsync();

        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family == null || family.IsDeleted)
        {
            throw new KeyNotFoundException($"Family with ID '{familyId}' not found");
        }

        // Tenancy — UpdateProviderAsync pattern (review 5-12, applied 2026-08-24): the ownership
        // check must live in the service, not only the endpoint, so every caller of this method
        // is scoped. A Charity caller may only attach a guardian to their own family, and a
        // Charity token with no parseable charity claim is refused rather than falling through
        // to unscoped access.
        if (userRole == "Charity" && (userCharityId == null || family.FK_CharityId != userCharityId.Value))
        {
            throw new UnauthorizedAccessException("You do not have access to this family");
        }

        // UC-FAM-12 check 1 — "the family does not already have one". The national-ID rule alone
        // let a second guardian with a DIFFERENT id create a duplicate seat, which every read
        // then resolved nondeterministically (GetByFamilyIdAsync is a bare FirstOrDefault).
        var existingProvider = await _providerRepository.GetByFamilyIdAsync(familyId);
        if (existingProvider != null)
        {
            throw new Exceptions.BusinessException("This family already has a guardian of record");
        }

        var provider = await AddProviderToFamilyInternalAsync(familyId, dto);

        // Update family provider type
        family.ProviderType = "Other";
        _familyRepository.Update(family);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProviderDto>(provider);
    }

    /// <summary>
    /// UC-REF-04: standalone guardian edit — PUT api/Families/{familyId}/provider. The §12.S.2
    /// refugee edit screen saves members through per-member endpoints (family PUT + provider
    /// PUT + orphan/relative PUTs); until now the guardian seat had no update path outside the
    /// housing-register composite PUT. Patch-style copies like UpdateRelativeAsync, and the
    /// family's derived HeadOfFamily (إسم الأسرة) follows a renamed guardian.
    /// </summary>
    public async Task<ProviderDto> UpdateProviderAsync(Guid familyId, UpdateProviderDto dto, Guid? userCharityId, string? userRole)
    {
        _logger.LogInformation("Updating provider for family: {FamilyId}", familyId);

        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family == null || family.IsDeleted)
        {
            throw new KeyNotFoundException($"Family with ID '{familyId}' not found");
        }

        // Tenancy — same fail-closed rule as GetProviderByFamilyAsync: a Charity caller may only
        // edit their own family's guardian, and a Charity token with no parseable charity claim
        // is refused rather than falling through to unscoped access (D4).
        if (userRole == "Charity" && (userCharityId == null || family.FK_CharityId != userCharityId.Value))
        {
            throw new UnauthorizedAccessException("You do not have access to this family");
        }

        var provider = await _providerRepository.GetByFamilyIdAsync(familyId);
        if (provider == null)
        {
            throw new KeyNotFoundException($"No provider found for family '{familyId}'");
        }

        // §12.S.2 mandatory provider fields — a whitespace-only value must be refused here,
        // not silently blank the stored data (the duplicate-معيل rule keys on NationalId).
        if (dto.FullName != null && string.IsNullOrWhiteSpace(dto.FullName))
        {
            throw new Exceptions.BusinessException("إسم المعيل مطلوب");
        }
        if (dto.NationalId != null && string.IsNullOrWhiteSpace(dto.NationalId))
        {
            throw new Exceptions.BusinessException("الرقم القومي للمعيل مطلوب");
        }
        if (dto.RelationshipToFamily != null && string.IsNullOrWhiteSpace(dto.RelationshipToFamily))
        {
            throw new Exceptions.BusinessException("العلاقة (نوعها) مطلوبة");
        }

        // Duplicate-guardian rule («أحد المعيلين مكرر من قبل أكثر من مرة») re-checked against
        // the NEW national id, excluding this provider itself — editing another field of the
        // same guardian is not a duplication (same semantics as the housing-register update).
        if (dto.NationalId != null && await _providerRepository.IsNationalIdExistsAsync(dto.NationalId, provider.Id))
        {
            throw new Exceptions.BusinessException("أحد المعيلين مكرر من قبل أكثر من مرة");
        }

        if (dto.FullName != null) provider.FullName = dto.FullName;
        if (dto.RelationshipToFamily != null) provider.RelationshipToFamily = dto.RelationshipToFamily;
        if (dto.NationalId != null) provider.NationalId = dto.NationalId;
        if (dto.Phone != null) provider.Phone = dto.Phone;
        if (dto.Address != null) provider.Address = dto.Address;
        if (dto.Job != null) provider.Job = dto.Job;
        if (dto.MonthlyIncome.HasValue) provider.MonthlyIncome = dto.MonthlyIncome.Value;
        if (dto.Notes != null) provider.Notes = dto.Notes;
        // Refugee register extensions (§12.S.2 اضافة معيل — edit mode)
        if (dto.DateOfBirth.HasValue) provider.DateOfBirth = dto.DateOfBirth.Value;
        if (dto.NationalityCountryId.HasValue) provider.NationalityCountryId = dto.NationalityCountryId.Value;
        if (dto.IsAlive.HasValue) provider.IsAlive = dto.IsAlive.Value;
        if (dto.DeathDate.HasValue) provider.DeathDate = dto.DeathDate.Value;
        if (dto.DeathReason != null) provider.DeathReason = dto.DeathReason;
        if (dto.ReasonOfRelationId.HasValue) provider.ReasonOfRelationId = dto.ReasonOfRelationId.Value;
        if (dto.MainRelation != null) provider.MainRelation = dto.MainRelation;
        // Review 2026-08-24: the shared §12.S.2/§11.S.2 guardian extensions the DTO declares
        // were silently dropped here (only MainRelation was copied) — the refugee edit screen
        // saving through this endpoint lost them. Same patch-style null-tolerant copies.
        if (dto.RelationId.HasValue) provider.RelationId = dto.RelationId.Value;
        if (dto.SocialStatusId.HasValue) provider.SocialStatusId = dto.SocialStatusId.Value;
        if (dto.HealthStatusId.HasValue) provider.HealthStatusId = dto.HealthStatusId.Value;
        if (dto.EducationLevelId.HasValue) provider.EducationLevelId = dto.EducationLevelId.Value;
        if (dto.WidowSponsorship.HasValue) provider.WidowSponsorship = dto.WidowSponsorship.Value;
        if (dto.AnotherSponsor.HasValue) provider.AnotherSponsor = dto.AnotherSponsor.Value;
        if (dto.MotherIsMar.HasValue) provider.MotherIsMar = dto.MotherIsMar.Value;
        if (dto.IsCaring.HasValue) provider.IsCaring = dto.IsCaring.Value;
        _providerRepository.Update(provider);

        // إسم الأسرة is derived from the guardian's name — keep the mirror in step. Refugee
        // register only: on the regular register HeadOfFamily tracks the father, not the
        // non-parent provider seat this endpoint edits.
        if (!string.IsNullOrWhiteSpace(dto.FullName) && family.FamilyType == Domain.Enums.FamilyType.Refugee)
        {
            family.HeadOfFamily = dto.FullName;
            _familyRepository.Update(family);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Provider updated successfully for family: {FamilyId}", familyId);
        return _mapper.Map<ProviderDto>(provider);
    }

    /// <summary>
    /// UC-REF-04: read the family's guardian — GET api/Families/{familyId}/provider. The §12.S.2
    /// view/edit screens load the guardian block through this per-member endpoint; charity-role
    /// callers cannot read another charity's guardian (same isolation as GetFamilyByIdAsync).
    /// </summary>
    public async Task<ProviderDto> GetProviderByFamilyAsync(Guid familyId, Guid? userCharityId, string? userRole)
    {
        _logger.LogInformation("Getting provider for family: {FamilyId}", familyId);

        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family == null || family.IsDeleted)
        {
            throw new KeyNotFoundException($"Family with ID '{familyId}' not found");
        }

        // Apply data isolation check (same rule as the family read) — fail CLOSED: a Charity
        // role with no parseable charity claim never falls through to the unscoped branch (D4).
        if (userRole == "Charity" && (userCharityId == null || family.FK_CharityId != userCharityId.Value))
        {
            throw new UnauthorizedAccessException("You do not have access to this family");
        }

        var provider = await _providerRepository.GetByFamilyIdAsync(familyId);
        if (provider == null)
        {
            throw new KeyNotFoundException($"No provider found for family '{familyId}'");
        }

        return _mapper.Map<ProviderDto>(provider);
    }

    #endregion

    #region UC-4.7: Verify Parent as Provider

    public async Task VerifyParentProviderAsync(Guid familyId, bool fatherIsProvider, bool motherIsProvider, string? notes)
    {
        _logger.LogInformation("Verifying parent provider for family: {FamilyId}", familyId);

        // UC-FAM-12: the verify/attach guard runs here too — changing the guardian seat is a
        // register write, so a locked charity or a disabled add permission refuses it.
        await _charityWriteGuard.EnsureCanAddAsync();

        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family == null || family.IsDeleted)
        {
            throw new KeyNotFoundException($"Family with ID '{familyId}' not found");
        }

        // BR-06 — one guardian of record per family: both parents flagged at once would create
        // two simultaneous guardians while ProviderType records only one.
        if (fatherIsProvider && motherIsProvider)
        {
            throw new Exceptions.BusinessException("Only one parent can be the family's provider of record");
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
        else
        {
            // Clearing both flags vacates the designation — leaving it stale fed contradictory
            // state to BR-06 (member control) and the widow sheet (Mother.IsProvider).
            family.ProviderType = null;
        }

        // The verify call's justification is part of the register record — without this the
        // operator's notes evaporate with the request.
        if (!string.IsNullOrWhiteSpace(notes))
        {
            var stamp = $"[{DateTime.Now:yyyy-MM-dd HH:mm}] provider designation change: {notes.Trim()}";
            family.Notes = string.IsNullOrWhiteSpace(family.Notes) ? stamp : $"{family.Notes}\n{stamp}";
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

        // UC-CHR-09 / UC-CHR-07: editing may be disabled, or the account locked.
        await _charityWriteGuard.EnsureCanUpdateAsync();

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

        // Register discriminator (epic 7 review): IMMUTABLE on the generic PUT. Each register
        // carries its own mandatory contract (§11.S.2 / §12.S.2) and allocation rules — a
        // mid-life restamp here would let a caller flip a Refugee row to Regular BEFORE the
        // effective-state validation below and escape the §12.S.2 mandatory set entirely.
        // The refugee edit screen re-sends the stored value, so ignoring dto.FamilyType is
        // a no-op for legitimate callers.

        // Refugee register household fields (§12.S.2) — applied when present
        if (dto.RegionId.HasValue) family.RegionId = dto.RegionId.Value;
        if (dto.CenterId.HasValue) family.CenterId = dto.CenterId.Value;
        if (dto.NearBy != null) family.NearBy = dto.NearBy;
        if (dto.Street != null) family.Street = dto.Street;
        if (dto.RentAmount.HasValue) family.RentAmount = dto.RentAmount.Value;
        if (dto.HouseOwnershipId.HasValue) family.HouseOwnershipId = dto.HouseOwnershipId.Value;
        if (dto.HouseStatusId.HasValue) family.HouseStatusId = dto.HouseStatusId.Value;
        if (dto.IncomeTypeId.HasValue) family.IncomeTypeId = dto.IncomeTypeId.Value;

        // UC-REF-04 §12.U.4: an update to a Refugee-register family must leave the §12.S.2
        // mandatory set complete. The copies above are patch-style (absent ⇒ keep stored value),
        // so the state to validate is the entity AFTER the copies — validating the raw DTO would
        // let a PATCH that omits everything fail spuriously, and a direct API PUT that empties a
        // mandatory field would silently keep the old value instead of being refused.
        if (family.FamilyType == Domain.Enums.FamilyType.Refugee)
        {
            var effective = new UpdateFamilyDto
            {
                CityVillage = family.CityVillage,
                RegionId = family.RegionId,
                CenterId = family.CenterId,
                Address = family.Address,
                HouseOwnershipId = family.HouseOwnershipId,
                HouseStatusId = family.HouseStatusId,
                HousingTypeId = family.HousingTypeId,
                IncomeTypeId = family.IncomeTypeId,
                RentAmount = family.RentAmount
            };
            await _updateRefugeeFamilyValidator.ValidateAndThrowAsync(effective);
        }

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

        // Review 2026-08-24 (6-1): no global soft-delete filter exists on this platform —
        // the register must drop deleted families explicitly or they list forever.
        query = query.Where(f => !f.IsDeleted);

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
            var term = filter.SearchTerm.Trim(); // typed code searches are exact — never miss on stray whitespace

            // Typed search (UC-HOU-02 §11.S.1) — unknown/absent selector keeps the
            // legacy 4-field behaviour (epic-5 back-compat)
            switch (filter.SearchType?.Trim().ToLowerInvariant())
            {
                case "father":
                    query = query.Where(f => f.Father != null && f.Father.FullName.Contains(term));
                    break;
                case "mother":
                    query = query.Where(f => f.Mother != null && f.Mother.FullName.Contains(term));
                    break;
                case "student":
                    // Review 2026-08-24 (6-2): deleted orphans must not surface families.
                    query = query.Where(f => f.Orphans.Any(o => !o.IsDeleted && o.FullName.Contains(term)));
                    break;
                case "nationalid":
                    // UC-HOU-02: guardian national id (partial); UC-REF-02 widens the SAME branch
                    // to the whole household (father | mother | provider | orphan) — the
                    // guardians 6-2 searches stay covered inside the OR.
                    query = query.Where(f =>
                        (f.Father != null && f.Father.NationalId != null && f.Father.NationalId.Contains(term)) ||
                        (f.Mother != null && f.Mother.NationalId != null && f.Mother.NationalId.Contains(term)) ||
                        (f.Provider != null && !f.Provider.IsDeleted && f.Provider.NationalId != null && f.Provider.NationalId.Contains(term)) ||
                        f.Orphans.Any(o => !o.IsDeleted && o.NationalId != null && o.NationalId.Contains(term)));
                    break;
                case "code":
                    // The student sponsorship code (Orphan.Code), NOT the family register code.
                    // Codes are exact (UC-REF-02 §12.S.1) — a partial code is not a match.
                    // Review 2026-08-24 (6-2): deleted orphans hold no searchable code slot.
                    query = query.Where(f => f.Orphans.Any(o => !o.IsDeleted && o.Code == term));
                    break;
                case "provider":
                    // UC-REF-02: إسم المعيل — a detached (soft-deleted) guardian is not searchable
                    query = query.Where(f => f.Provider != null && !f.Provider.IsDeleted && f.Provider.FullName.Contains(term));
                    break;
                case "phone":
                    query = query.Where(f =>
                        (f.PhoneNumber != null && f.PhoneNumber.Contains(term)) ||
                        (f.Father != null && f.Father.Phone != null && f.Father.Phone.Contains(term)) ||
                        (f.Mother != null && f.Mother.Phone != null && f.Mother.Phone.Contains(term)) ||
                        (f.Provider != null && !f.Provider.IsDeleted && f.Provider.Phone != null && f.Provider.Phone.Contains(term)) ||
                        f.Orphans.Any(o => !o.IsDeleted && o.Phone != null && o.Phone.Contains(term)));
                    break;
                default:
                    query = query.Where(f =>
                        f.Code.Contains(term) ||
                        (f.Address != null && f.Address.Contains(term)) ||
                        (f.Father != null && f.Father.FullName.Contains(term)) ||
                        (f.Mother != null && f.Mother.FullName.Contains(term)));
                    break;
            }
        }

        // Register discriminator (UC-HOU-01: ?familyType=Housing) — invalid values ignored
        if (!string.IsNullOrEmpty(filter.FamilyType) &&
            Enum.TryParse<Domain.Enums.FamilyType>(filter.FamilyType, ignoreCase: true, out var parsedFamilyType))
        {
            query = query.Where(f => f.FamilyType == parsedFamilyType);
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
            FamilyType = f.FamilyType.ToString(),
            PhoneNumber = f.PhoneNumber,
            CharityId = f.FK_CharityId,
            CharityName = f.Charity?.Name,
            IsHoldingFamily = f.IsHoldingFamily
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

    #region UC-FAM-06: Transfer Family to Another Charity

    public async Task TransferFamilyToCharityAsync(Guid familyId, TransferFamilyDto dto, Guid? userCharityId, string? userRole)
    {
        await _transferFamilyValidator.ValidateAndThrowAsync(dto);

        // Defense in depth: the endpoint is HQ-only ([Authorize]), and the service refuses a
        // charity-role caller outright — only HQ roles move a family between charities.
        if (string.Equals(userRole, "Charity", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Only head-office roles can transfer a family between charities");
        }

        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family is null || family.IsDeleted)
        {
            throw new NotFoundException(typeof(Family), familyId);
        }

        var targetCharity = await _charityRepository.GetByIdAsync(dto.NewCharityId);
        if (targetCharity is null || targetCharity.IsDeleted)
        {
            throw new BusinessException("Receiving charity does not exist");
        }
        if (!targetCharity.IsActive)
        {
            throw new BusinessException("Receiving charity is not active");
        }
        // An inbound transfer is an add to the target's register — respect the target's
        // write state the way CharityWriteGuard does for family creates (UC-CHR-07/08).
        if (targetCharity.IsLocked || !targetCharity.IsAddEnabled)
        {
            throw new BusinessException("The receiving charity is locked or not accepting new families");
        }
        // Tenancy is charity AND country (CLAUDE.md) — a cross-country transfer would leave the
        // family answering to a charity whose country differs from its own geography.
        if (targetCharity.CountryId != family.CountryId)
        {
            throw new BusinessException("The receiving charity belongs to a different country than the family");
        }
        if (family.FK_CharityId is null)
        {
            // FamilyCharityTransfer.FromCharityId is a required FK — a null current charity must
            // be refused as bad register data, not surface as an FK-violation 500.
            throw new BusinessException("The family has no current charity recorded; fix its register data before transferring");
        }
        if (targetCharity.Id == family.FK_CharityId)
        {
            throw new BusinessException("The family already belongs to the receiving charity");
        }

        Guid? fromCharityId = null;
        var cascadedOrphans = 0;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Read the cascade set inside the transaction — an orphan added between the
            // pre-checks and this point must not be stranded in the old charity.
            var orphans = (await _orphanRepository.GetByFamilyIdAsync(familyId)).ToList();
            fromCharityId = family.FK_CharityId;
            cascadedOrphans = orphans.Count;

            // FK_CharityId is the live tenancy column every read scopes on (GetFamiliesAsync);
            // CharityId is mirrored so the EF navigation agrees — the duality is inherited,
            // write both, do not "fix" one side only.
            family.FK_CharityId = dto.NewCharityId;
            family.CharityId = dto.NewCharityId;

            foreach (var orphan in orphans)
            {
                orphan.FK_CharityId = dto.NewCharityId;
            }

            await _transferRepository.InsertAsync(new FamilyCharityTransfer
            {
                FamilyId = familyId,
                FromCharityId = fromCharityId ?? Guid.Empty,
                ToCharityId = dto.NewCharityId,
                Reason = dto.Reason
            });

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        _logger.LogInformation(
            "Family {FamilyId} transferred from charity {FromCharityId} to {ToCharityId} ({OrphanCount} orphans cascaded)",
            familyId, fromCharityId, dto.NewCharityId, cascadedOrphans);
    }

    #endregion

    #region UC-FAM-07/08: Member Control (move an orphan / guardian between families)

    public async Task ControlFamilyMemberAsync(Guid familyId, Guid memberId, MemberControlDto dto, Guid? userCharityId, string? userRole, string? userName = null)
    {
        await _memberControlValidator.ValidateAndThrowAsync(dto);

        // Endpoint is HQ-only ([Authorize]); the service re-refuses a charity-role caller.
        if (string.Equals(userRole, "Charity", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Only head-office roles can move a family member");
        }

        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family is null || family.IsDeleted)
        {
            throw new NotFoundException(typeof(Family), familyId);
        }

        // Defense-in-depth tenancy (UpdateProviderAsync pattern): the role refusal above
        // blocks Charity-role callers outright, but a token with no role claim would slip
        // past a role-string check alone — any caller carrying a charity claim is pinned
        // to their own families regardless of the role string.
        if (userCharityId is not null && family.FK_CharityId != userCharityId)
        {
            throw new UnauthorizedAccessException("You do not have access to this family");
        }

        // Resolve the mover by member type (legacy contract: 1 = orphan, 2 = guardian/provider)
        // before anything is written — every refusal below must leave the register untouched.
        Orphan? orphan = null;
        Provider? provider = null;
        if (dto.MemberType == 1)
        {
            orphan = await _orphanRepository.GetByIdAsync(memberId);
            if (orphan is null || orphan.IsDeleted)
            {
                throw new NotFoundException(typeof(Orphan), memberId);
            }
            if (orphan.FamilyId != familyId)
            {
                throw new BusinessException("The member does not belong to this family");
            }
        }
        else if (dto.MemberType == 2)
        {
            provider = await _providerRepository.GetByIdAsync(memberId);
            if (provider is null || provider.IsDeleted)
            {
                throw new NotFoundException(typeof(Provider), memberId);
            }
            if (provider.FamilyId != familyId)
            {
                throw new BusinessException("The member does not belong to this family");
            }

            // BR-06 — one guardian of record per family. Ruling 2026-08-24 (replaces the old
            // refusal): the acting guardian ("Other") MAY move — the seat is vacated inside
            // the transaction (see the provider write below) and the family remains without a
            // guardian of record until HQ re-fills it via the 5-9/5-10 raise-and-approve flow.
            // The move modal warns the operator before confirming. Father/Mother designations
            // never seated this Provider row, so nothing is vacated for them.
        }
        else
        {
            throw new BusinessException("Unsupported member type for this operation");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Membership re-check inside the transaction (fresh read; the pre-transaction check
            // is check-then-act and these entities carry no concurrency tokens).
            var stillMember = dto.MemberType == 1
                ? await _orphanRepository.TableNoTracking.AnyAsync(o => o.Id == memberId && o.FamilyId == familyId && !o.IsDeleted)
                : await _providerRepository.TableNoTracking.AnyAsync(p => p.Id == memberId && p.FamilyId == familyId && !p.IsDeleted);
            if (!stillMember)
            {
                throw new BusinessException("The member no longer belongs to this family");
            }

            Family targetFamily;
            if (dto.Action == 0)
            {
                // Detach: the member gets a brand-new holding family under the SAME charity —
                // the source family's charity, never the caller's (an HQ user moving a charity's
                // member must not create an HQ-owned family).
                targetFamily = new Family
                {
                    Code = await GenerateFamilyCodeAsync(),
                    FK_CharityId = family.FK_CharityId,
                    CharityId = family.CharityId,
                    CountryId = family.CountryId,
                    CityId = family.CityId,
                    IsActive = true,
                    FamilyMembersCount = 0,
                    OrphansCount = 0,
                    // Holding-family marker (ruling 2026-08-24): this row exists only to hold a
                    // detached member — flagged so lists/reports can tell it from a register family.
                    IsHoldingFamily = true
                };
                await _familyRepository.InsertAsync(targetFamily);
            }
            else
            {
                // Attach: resolve the target by register code (case-insensitive, not deleted).
                var targetCode = dto.TargetFamilyCode!.Trim();
                targetFamily = await _familyRepository.IncludeNavigationProperties()
                    .FirstOrDefaultAsync(f => f.Code.ToLower() == targetCode.ToLower() && !f.IsDeleted)
                    ?? throw new BusinessException("No family exists with the given code");
                if (targetFamily.Id == familyId)
                {
                    throw new BusinessException("The member already belongs to that family");
                }
                if (targetFamily.FK_CharityId != family.FK_CharityId)
                {
                    // Moves are within-charity corrections; cross-charity is UC-FAM-06 (transfer).
                    throw new BusinessException("A member can only be moved within the same charity");
                }
                if (!targetFamily.IsActive)
                {
                    throw new BusinessException("The receiving family is not active");
                }
                if (targetFamily.FamilyType != family.FamilyType)
                {
                    // Regular / Housing / Refugee registers carry their own mandatory contracts —
                    // a member move must stay inside one register type.
                    throw new BusinessException("A member can only be moved within the same register type");
                }
                if (dto.MemberType == 2)
                {
                    // BR-06 on the receiving side: attaching a guardian onto a family that already
                    // has a live provider would create a second guardian of record (the seat is
                    // resolved with FirstOrDefault — no unique constraint backs it).
                    var occupiedSeat = await _providerRepository.GetByFamilyIdAsync(targetFamily.Id);
                    if (occupiedSeat is not null)
                    {
                        throw new BusinessException("The receiving family already has a guardian of record");
                    }
                }
            }

            var stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            var note = $"[{stamp}] Moved to family '{targetFamily.Code}' by {userName ?? "HQ"}: {dto.Justification ?? dto.TargetFamilyCode}";
            if (orphan is not null)
            {
                orphan.FamilyId = targetFamily.Id;
                // The stamped note (with the operator's justification) records BOTH directions —
                // attach dropped it silently before, losing the reason the modal collects.
                orphan.Notes = string.IsNullOrWhiteSpace(orphan.Notes) ? note : $"{orphan.Notes}\n{note}";
            }
            else if (provider is not null)
            {
                provider.FamilyId = targetFamily.Id;
                provider.Notes = string.IsNullOrWhiteSpace(provider.Notes) ? note : $"{provider.Notes}\n{note}";

                // BR-06 ruling 2026-08-24: vacate the guardian seat when the mover was the
                // acting guardian ("Other") — true for detach and attach alike, since either
                // direction leaves the source family without its provider of record. The seat
                // stays empty until HQ approves a replacement through 5-9/5-10; HeadOfFamily is
                // deliberately left naming the last head (5-10 re-points it on approval).
                if (string.Equals(family.ProviderType, "Other", StringComparison.OrdinalIgnoreCase))
                {
                    family.ProviderType = null;
                }
            }

            await _unitOfWork.SaveChangesAsync();

            // OrphansCount is recomputed from the live register on both sides — the flush above
            // makes the moves visible to these counts inside the same transaction, so the figures
            // cannot drift under concurrent moves (the old increment/decrement did).
            // FamilyMembersCount is deliberately untouched: no code path feeds that legacy
            // column (always 0) — mutating it only here made the two families disagree forever.
            family.OrphansCount = await _orphanRepository.TableNoTracking
                .CountAsync(o => o.FamilyId == familyId && !o.IsDeleted);
            targetFamily.OrphansCount = await _orphanRepository.TableNoTracking
                .CountAsync(o => o.FamilyId == targetFamily.Id && !o.IsDeleted);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        _logger.LogInformation(
            "Member type {MemberType} {MemberId} moved from family {FromFamilyId} to {ToFamilyId} (action {Action}, by {User})",
            dto.MemberType, memberId, familyId, dto.Action == 0 ? "new holding family" : dto.TargetFamilyCode, dto.Action, userName ?? "HQ");
    }

    #endregion

    #region UC-FAM-13: Remove Guardian Sponsorship Link (حذف كفالة العائل)

    public async Task RemoveProviderSponsorLinkAsync(Guid familyId, RemoveProviderSponsorLinkDto dto, Guid? userCharityId, string? userRole, string? userName = null)
    {
        await _removeSponsorLinkValidator.ValidateAndThrowAsync(dto);

        // Endpoint is HQ-only ([Authorize]); the service re-refuses a charity-role caller.
        if (string.Equals(userRole, "Charity", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Only head-office roles can remove a guardian's sponsorship link");
        }

        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family is null || family.IsDeleted)
        {
            throw new NotFoundException(typeof(Family), familyId);
        }

        // Defense-in-depth tenancy (UpdateProviderAsync pattern) — see ControlFamilyMemberAsync.
        if (userCharityId is not null && family.FK_CharityId != userCharityId)
        {
            throw new UnauthorizedAccessException("You do not have access to this family");
        }

        var provider = await _providerRepository.GetByFamilyIdAsync(familyId);

        // Parent-designated families (guardian of record = Father/Mother flag, no Provider row)
        // are served too — 5-13 ruling 2026-08-24. Their "link" is the designation itself, so the
        // removal clears the parent's IsProvider flag instead of soft-deleting a member row (the
        // parent stays a family member). A family with neither shape has no guardian to unlink.
        Father? designatedFather = null;
        Mother? designatedMother = null;
        string linkDescription;
        if (provider is not null)
        {
            linkDescription = $"provider {provider.Id}";
        }
        else if (string.Equals(family.ProviderType, "Father", StringComparison.OrdinalIgnoreCase))
        {
            designatedFather = await _fatherRepository.GetByFamilyIdAsync(familyId)
                ?? throw new NotFoundException(typeof(Family), familyId);
            linkDescription = "father designation";
        }
        else if (string.Equals(family.ProviderType, "Mother", StringComparison.OrdinalIgnoreCase))
        {
            designatedMother = await _motherRepository.GetByFamilyIdAsync(familyId)
                ?? throw new NotFoundException(typeof(Family), familyId);
            linkDescription = "mother designation";
        }
        else
        {
            throw new NotFoundException(typeof(Provider), familyId);
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // §10.U.13 — the link may only be removed while NO orphan of the family is still
            // referenced by an active sponsorship. Keyed on SponsorId alone: no application code
            // writes SponsorshipStatus, and the platform's own derivation treats SponsorId.HasValue
            // as sponsored (OrphanPaymentService) — the old `status == "Sponsored"` conjunct
            // false-allowed imported rows. Checked inside the transaction so a sponsorship
            // attached between check and commit still blocks.
            var sponsoredCodes = await _orphanRepository.TableNoTracking
                .Where(o => o.FamilyId == familyId && !o.IsDeleted && o.SponsorId != null)
                .Select(o => string.IsNullOrEmpty(o.Code) ? o.FullName : o.Code)
                .ToListAsync();
            if (sponsoredCodes.Count > 0)
            {
                throw new BusinessException(
                    "The guardian's link cannot be removed while the family still has an active sponsorship (orphans: "
                    + string.Join(", ", sponsoredCodes) + "). End the sponsorship first");
            }

            // التعليق lands on the row's notes in the member-control stamp format BEFORE the
            // write, so the reason survives for the audit trail in both shapes below.
            var stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            var note = $"[{stamp}] Guardian sponsorship link removed by {userName ?? "HQ"}"
                       + (string.IsNullOrWhiteSpace(dto.Comment) ? string.Empty : $": {dto.Comment.Trim()}");

            if (provider is not null)
            {
                provider.Notes = string.IsNullOrWhiteSpace(provider.Notes) ? note : $"{provider.Notes}\n{note}";

                // Soft delete only — reads (GetByFamilyIdAsync, IsNationalIdExistsAsync) filter
                // IsDeleted, so the row vanishes from the module and the national id becomes
                // attachable to another family, which is the point of the use case. Deletion audit
                // is stamped manually — ChangeTrackerExtensions covers only Created/Updated (the
                // Incoming/Outgoing/SupportTicket services stamp DeletedBy the same way).
                provider.IsDeleted = true;
                provider.DeletedOn = DateTime.Now;
                provider.DeletedBy = userName ?? "HQ";

                // When the provider row was the acting guardian ("Other"), the seat is now vacant;
                // Father/Mother designations keep their parent-based meaning (the row was auxiliary).
                if (string.Equals(family.ProviderType, "Other", StringComparison.OrdinalIgnoreCase))
                {
                    family.ProviderType = null;
                }
            }
            else
            {
                // Parent-designated shape (5-13 ruling 2026-08-24): clear the designation —
                // the flag and ProviderType reset ARE the unlink; the parent row itself is a
                // family member and is never soft-deleted here.
                if (designatedFather is not null)
                {
                    designatedFather.Notes = string.IsNullOrWhiteSpace(designatedFather.Notes)
                        ? note
                        : $"{designatedFather.Notes}\n{note}";
                    designatedFather.IsProvider = false;
                }
                if (designatedMother is not null)
                {
                    designatedMother.Notes = string.IsNullOrWhiteSpace(designatedMother.Notes)
                        ? note
                        : $"{designatedMother.Notes}\n{note}";
                    designatedMother.IsProvider = false;
                }
                family.ProviderType = null;
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        _logger.LogInformation(
            "Guardian sponsorship link removed: {LinkDescription} of family {FamilyId} (by {User})",
            linkDescription, familyId, userName ?? "HQ");
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

        if (fileData == null || fileData.Length == 0)
        {
            throw new ArgumentException("File data is required");
        }

        // Persist through the framework attachment service (dual storage mode + thumbnails) —
        // the same writer the charity-icon path uses. The attachment type id 1 is the
        // platform's generic document type until UC-4.15 grows a typed catalogue.
        // (Implements the placeholder flagged by the epic-19 code review, 2026-08-26 —
        // previously returned a random Guid and stored nothing.)
        var savedAttachment = _attachmentService.AddOrUpdateAttachment(
            fileName,
            contentType ?? "application/octet-stream",
            fileData,
            1,
            null,
            description ?? fileName,
            description ?? fileName);

        if (savedAttachment == null)
        {
            throw new InvalidOperationException($"Failed to store document '{fileName}' for family '{familyId}'");
        }

        _logger.LogInformation("Document attached successfully to family: {FamilyId}, AttachmentId: {AttachmentId}", familyId, savedAttachment.Id);

        return savedAttachment.Id;
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
            IsActive = true,
            // Refugee register extensions (UC-REF-04) — the edit screen reloads مرافق rows
            // with their NID and free-text صلة القرابة (stored as notes)
            NationalId = r.NationalId,
            Notes = r.Notes,
            // الحالة الصحية (epic-7 review P12) — resolved off the included HealthStatus nav
            HealthStatusId = r.HealthStatusId,
            HealthStatusName = r.HealthStatus != null ? r.HealthStatus.NameAr ?? r.HealthStatus.NameEn : null
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
            NationalId = o.NationalId,
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
        // UC-REF-03 «أحد المعيلين مكرر من قبل أكثر من مرة» — a provider national id may be
        // linked to ONE family only (refugee or not; the rule is the legacy register's).
        if (await _providerRepository.IsNationalIdExistsAsync(dto.NationalId))
        {
            throw new Exceptions.BusinessException("أحد المعيلين مكرر من قبل أكثر من مرة");
        }

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
            Notes = dto.Notes,
            // Refugee register extensions (§12.S.2 اضافة معيل)
            DateOfBirth = dto.DateOfBirth,
            NationalityCountryId = dto.NationalityCountryId,
            IsAlive = dto.IsAlive,
            DeathDate = dto.DeathDate,
            DeathReason = dto.DeathReason,
            ReasonOfRelationId = dto.ReasonOfRelationId,
            // Housing register extensions (§11.S.2 اضافة معيل)
            RelationId = dto.RelationId,
            MainRelation = dto.MainRelation,
            SocialStatusId = dto.SocialStatusId,
            HealthStatusId = dto.HealthStatusId,
            EducationLevelId = dto.EducationLevelId,
            WidowSponsorship = dto.WidowSponsorship,
            AnotherSponsor = dto.AnotherSponsor,
            MotherIsMar = dto.MotherIsMar,
            IsCaring = dto.IsCaring
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
            Notes = dto.Notes,
            // Refugee register extension (§12.S.2 اضافة ابن)
            SocialStatusId = dto.SocialStatusId,
            // Housing register extensions (§11.S.2 اضافة ابن)
            Profession = dto.Profession,
            DepartmentName = dto.DepartmentName,
            FacultyName = dto.FacultyName,
            BirthCertificateAttachmentId = dto.BirthCertificateAttachmentId,
            EnrollmentAttachmentId = dto.EnrollmentAttachmentId
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
            // Refugee register household fields (§12.S.2) — null on Regular/Housing rows
            FamilyType = family.FamilyType.ToString(),
            RegionId = family.RegionId,
            RegionName = family.Region?.NameAr ?? family.Region?.NameEn,
            CenterId = family.CenterId,
            CenterName = family.Center?.NameAr ?? family.Center?.NameEn,
            NearBy = family.NearBy,
            Street = family.Street,
            RentAmount = family.RentAmount,
            HouseOwnershipId = family.HouseOwnershipId,
            HouseOwnershipName = family.HouseOwnership?.NameAr ?? family.HouseOwnership?.NameEn,
            HouseStatusId = family.HouseStatusId,
            HouseStatusName = family.HouseStatus?.NameAr ?? family.HouseStatus?.NameEn,
            IncomeTypeId = family.IncomeTypeId,
            IncomeTypeName = family.IncomeType?.NameAr ?? family.IncomeType?.NameEn,
            // Housing allocation (§11.S.2) — null on non-housing rows
            HousingBuildingId = family.FK_HousingBuildingId,
            HousingBuildingName = family.HousingBuilding?.NameAr ?? family.HousingBuilding?.NameEn,
            HousingFlatId = family.FK_HousingFlatId,
            HousingFlatName = family.HousingFlat?.NameAr ?? family.HousingFlat?.NameEn,
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
            IsActive = true,
            // Refugee register extensions (UC-REF-04)
            NationalId = r.NationalId,
            Notes = r.Notes
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
            // Refugee register extension (§12.S.2 اضافة ابن)
            SocialStatusId = o.SocialStatusId,
            SocialStatusName = o.SocialStatus != null ? o.SocialStatus.NameAr ?? o.SocialStatus.NameEn : null,
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

    #region Orphan Register & Coding (UC-ORP-01 through UC-ORP-10)

    /// <summary>
    /// Resolves the charity an orphan belongs to: its own <c>FK_CharityId</c>, falling back to its
    /// family's. Uncoded orphans registered under a family carry the family's charity.
    /// </summary>
    private static Guid? ResolveOrphanCharity(Orphan orphan)
    {
        return orphan.FK_CharityId ?? orphan.Family?.FK_CharityId;
    }

    public async Task<(IEnumerable<OrphanLookupDto> Items, int TotalCount)> SearchOrphansAsync(OrphanSearchFilterDto filter, Guid? userCharityId, string? userRole)
    {
        await _orphanSearchValidator.ValidateAndThrowAsync(filter);

        // UC-ORP-03: the coding worklist is a head-office function (legacy «Gen. Director, Staff»).
        // One endpoint, two query modes — supplying codingStatus makes it the worklist, and the
        // role gate travels with that mode rather than with the route.
        if (!string.IsNullOrEmpty(filter.CodingStatus) && userRole == "Charity")
        {
            throw new UnauthorizedAccessException("The coding worklist is a head-office function");
        }

        _logger.LogInformation("Searching orphans (UC-ORP) with filter: {@Filter}, for user role: {Role}", filter, userRole);

        IQueryable<Domain.Entities.Orphan> query = _orphanRepository.IncludeNavigationProperties()
            .Include(o => o.EducationLevel)
            .Include(o => o.HealthStatus)
            .Include(o => o.Family).ThenInclude(f => f.Father)
            .Include(o => o.Family).ThenInclude(f => f.Mother)
            .Include(o => o.Family).ThenInclude(f => f.Charity)
            // No global soft-delete filter exists on this context (8-2 review finding): deleted
            // orphans — and orphans riding on a deleted family — are filtered out by hand.
            .Where(o => !o.IsDeleted && (o.Family == null || !o.Family.IsDeleted));

        // Charity scoping as in GetFamiliesAsync: a charity caller is pinned to its own register;
        // HQ may narrow by an explicit charity id — empty = كافة الجهات (all charities).
        if (userRole == "Charity" && userCharityId.HasValue)
        {
            query = query.Where(o => o.FK_CharityId == userCharityId.Value ||
                                     (o.Family != null && o.Family.FK_CharityId == userCharityId.Value));
        }
        else if (filter.CharityId.HasValue)
        {
            query = query.Where(o => o.FK_CharityId == filter.CharityId.Value ||
                                     (o.Family != null && o.Family.FK_CharityId == filter.CharityId.Value));
        }

        // UC-ORP-03: uncoded is an EMPTY code — never a SponsorshipStatus value (§13.D).
        if (string.Equals(filter.CodingStatus, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(o => string.IsNullOrEmpty(o.Code));
        }
        else if (string.Equals(filter.CodingStatus, "Coded", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(o => !string.IsNullOrEmpty(o.Code));
        }

        // One term serves both directions: a name finds the orphan (UC-ORP-02), a code typed here
        // resolves back to the name (UC-ORP-07).
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(o => o.FullName.Contains(term) || o.Code.Contains(term));
        }

        var totalCount = await query.CountAsync();

        query = (filter.SortBy, filter.SortDescending) switch
        {
            ("Code", true) => query.OrderByDescending(o => o.Code),
            ("Code", false) => query.OrderBy(o => o.Code),
            ("DateOfBirth", true) => query.OrderByDescending(o => o.DateOfBirth),
            ("DateOfBirth", false) => query.OrderBy(o => o.DateOfBirth),
            ("MotherName", true) => query.OrderByDescending(o => o.Family != null && o.Family.Mother != null ? o.Family.Mother.FullName : string.Empty),
            ("MotherName", false) => query.OrderBy(o => o.Family != null && o.Family.Mother != null ? o.Family.Mother.FullName : string.Empty),
            ("CharityName", true) => query.OrderByDescending(o => o.Family != null && o.Family.Charity != null ? o.Family.Charity.Name : string.Empty),
            ("CharityName", false) => query.OrderBy(o => o.Family != null && o.Family.Charity != null ? o.Family.Charity.Name : string.Empty),
            _ => filter.SortDescending ? query.OrderByDescending(o => o.FullName) : query.OrderBy(o => o.FullName)
        };

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = items.Select(o => new OrphanLookupDto
        {
            OrphanId = o.Id,
            FullName = o.FullName,
            Code = string.IsNullOrEmpty(o.Code) ? null : o.Code,
            FatherName = o.Family?.Father?.FullName,
            MotherName = o.Family?.Mother?.FullName,
            CharityName = o.Family?.Charity?.Name,
            CharityId = ResolveOrphanCharity(o),
            FamilyId = o.FamilyId,
            FamilyCode = o.Family?.Code,
            DateOfBirth = o.DateOfBirth,
            Age = o.DateOfBirth.HasValue ? CalculateAge(o.DateOfBirth.Value) : null,
            Gender = o.Gender,
            NationalId = o.NationalId,
            Phone = o.Phone,
            SponsorshipStatus = o.SponsorshipStatus,
            EducationLevelName = o.EducationLevel?.NameAr ?? o.EducationLevel?.NameEn,
            HealthStatusName = o.HealthStatus?.NameAr ?? o.HealthStatus?.NameEn
        }).ToList();

        return (dtos, totalCount);
    }

    public async Task<OrphanEligibilityDto> CheckOrphanCanBeAddedAsync(OrphanEligibilityCheckDto check, Guid? userCharityId, string? userRole)
    {
        await _orphanEligibilityValidator.ValidateAndThrowAsync(check);

        _logger.LogInformation("Checking whether an orphan may be added (UC-ORP-01), role: {Role}", userRole);

        // Charity write state — the same flags ICharityWriteGuard enforces for the save, returned
        // as a verdict instead of a throw. Head-office callers are exempt, as in the guard: the
        // flags exist for head office to restrain a charity, not its supervisor.
        if (userRole == "Charity" && userCharityId.HasValue)
        {
            var charity = await _charityRepository.GetByIdAsync(userCharityId.Value);
            if (charity == null)
            {
                return new OrphanEligibilityDto { CanBeAdded = false, Field = "charity", ReasonCode = "charityNotFound" };
            }
            if (charity.IsLocked)
            {
                return new OrphanEligibilityDto { CanBeAdded = false, Field = "charity", ReasonCode = "charityLocked" };
            }
            if (!charity.IsAddEnabled)
            {
                return new OrphanEligibilityDto { CanBeAdded = false, Field = "charity", ReasonCode = "charityAddDisabled" };
            }
            if (!charity.IsActive)
            {
                return new OrphanEligibilityDto { CanBeAdded = false, Field = "charity", ReasonCode = "charityInactive" };
            }
        }

        // P3 — the family gate: an orphan is added on a family form, so when the family id is
        // given it must exist, be active and sit inside the resolved scope. An out-of-scope
        // family reports as not found (no cross-tenant existence leak).
        if (check.FamilyId.HasValue)
        {
            var resolvedScopeCharityId = userRole == "Charity" ? userCharityId : check.CharityId;
            var family = await _familyRepository.IncludeNavigationProperties()
                .FirstOrDefaultAsync(f => f.Id == check.FamilyId.Value && !f.IsDeleted);
            var familyInScope = family != null && family.FK_CharityId == resolvedScopeCharityId;
            if (family == null || !familyInScope)
            {
                return new OrphanEligibilityDto { CanBeAdded = false, Field = "family", ReasonCode = "familyNotFound" };
            }
            if (!family.IsActive)
            {
                return new OrphanEligibilityDto { CanBeAdded = false, Field = "family", ReasonCode = "familyInactive" };
            }
        }

        // Prior registration of the national ID within the caller's scope — a duplicate national
        // ID is the same person already on the register, in any family.
        var nationalId = check.NationalId?.Trim();
        if (!string.IsNullOrEmpty(nationalId))
        {
            var scopeCharityId = userRole == "Charity" ? userCharityId : check.CharityId;
            var dupQuery = _orphanRepository.IncludeNavigationProperties()
                .Where(o => o.NationalId == nationalId && !o.IsDeleted && (o.Family == null || !o.Family.IsDeleted));
            if (scopeCharityId.HasValue)
            {
                dupQuery = dupQuery.Where(o => o.FK_CharityId == scopeCharityId.Value ||
                                               (o.Family != null && o.Family.FK_CharityId == scopeCharityId.Value));
            }

            var existing = await dupQuery.FirstOrDefaultAsync();
            if (existing != null)
            {
                return new OrphanEligibilityDto
                {
                    CanBeAdded = false,
                    Field = "nationalId",
                    ReasonCode = "nationalIdRegistered",
                    ExistingOrphanName = existing.FullName
                };
            }
        }

        return new OrphanEligibilityDto { CanBeAdded = true };
    }

    public async Task<FamilyNationalIdCheckResultDto> CheckFamilyNationalIdAsync(CheckFamilyNationalIdDto check, Guid? userCharityId, string? userRole)
    {
        await _familyNationalIdCheckValidator.ValidateAndThrowAsync(check);

        _logger.LogInformation("Checking national id uniqueness across family holders (UC-SYS-12), role: {Role}", userRole);

        // Scope — the register the check runs against. A charity claim always wins (server-side
        // tenancy); an HQ caller must name the register or the call is refused naming the field.
        Guid scopeCharityId;
        if (userRole == "Charity")
        {
            // The controller fail-closes claim-less Charity tokens before we get here.
            scopeCharityId = userCharityId!.Value;
        }
        else if (check.CharityId.HasValue)
        {
            scopeCharityId = check.CharityId.Value;
        }
        else
        {
            throw new FluentValidation.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("charityId", "charityId is required for head-office callers")
            });
        }

        var nationalId = check.NationalId.Trim();

        // The dedup span: every person holder, soft-delete-aware on BOTH the holder row and its
        // family (epic-5 phone-check precedent — deleted rows must never block a registration).
        // The family being edited is excluded below so an unchanged person does not self-report.
        var candidates = new List<(string HolderType, string HolderName, Guid FamilyId, Guid? DirectCharityId)>();

        var fathers = await _fatherRepository.TableNoTracking
            .Where(x => x.NationalId == nationalId && !x.IsDeleted && x.FamilyId != null)
            .Select(x => new { x.FullName, x.FamilyId }).ToListAsync();
        candidates.AddRange(fathers.Select(x => ("Father", x.FullName, x.FamilyId!.Value, (Guid?)null)));

        var mothers = await _motherRepository.TableNoTracking
            .Where(x => x.NationalId == nationalId && !x.IsDeleted && x.FamilyId != null)
            .Select(x => new { x.FullName, x.FamilyId }).ToListAsync();
        candidates.AddRange(mothers.Select(x => ("Mother", x.FullName, x.FamilyId!.Value, (Guid?)null)));

        var providers = await _providerRepository.TableNoTracking
            .Where(x => x.NationalId == nationalId && !x.IsDeleted && x.FamilyId != null)
            .Select(x => new { x.FullName, x.FamilyId }).ToListAsync();
        candidates.AddRange(providers.Select(x => ("Provider", x.FullName, x.FamilyId!.Value, (Guid?)null)));

        var relatives = await _relativeRepository.TableNoTracking
            .Where(x => x.NationalId == nationalId && !x.IsDeleted && x.FamilyId != null)
            .Select(x => new { x.FullName, x.FamilyId }).ToListAsync();
        candidates.AddRange(relatives.Select(x => ("Relative", x.FullName, x.FamilyId!.Value, (Guid?)null)));

        // Orphans may also be registered directly against the charity (FK_CharityId) — the same
        // scope rule the 8-1 check applies: in scope via their family OR their direct charity link.
        var orphans = await _orphanRepository.TableNoTracking
            .Where(o => o.NationalId == nationalId && !o.IsDeleted && o.FamilyId != null)
            .Select(o => new { o.FullName, o.FamilyId, o.FK_CharityId }).ToListAsync();
        candidates.AddRange(orphans.Select(x => ("Orphan", x.FullName, x.FamilyId!.Value, x.FK_CharityId)));

        foreach (var (holderType, holderName, familyId, directCharityId) in candidates)
        {
            if (check.FamilyId == familyId)
            {
                continue; // the record being edited — never a self-clash (AC 4)
            }

            if (directCharityId == scopeCharityId)
            {
                // Directly-registered orphan in scope; resolve its family code for the message.
                var directFamily = await _familyRepository.TableNoTracking
                    .FirstOrDefaultAsync(f => f.Id == familyId && !f.IsDeleted);
                if (directFamily != null)
                {
                    return new FamilyNationalIdCheckResultDto
                    {
                        IsUnique = false,
                        HolderName = holderName,
                        HolderFamilyCode = directFamily.Code,
                        HolderType = holderType
                    };
                }
                continue;
            }

            var family = await _familyRepository.TableNoTracking
                .FirstOrDefaultAsync(f => f.Id == familyId && !f.IsDeleted);
            if (family == null || family.FK_CharityId != scopeCharityId)
            {
                continue; // deleted family, or outside the resolved register
            }

            return new FamilyNationalIdCheckResultDto
            {
                IsUnique = false,
                HolderName = holderName,
                HolderFamilyCode = family.Code,
                HolderType = holderType
            };
        }

        return new FamilyNationalIdCheckResultDto { IsUnique = true };
    }

    public async Task<OrphanCodeCheckDto> CheckOrphanCodeUniqueAsync(OrphanCodeCheckFilterDto filter, Guid? userCharityId, string? userRole)
    {
        await _orphanCodeCheckValidator.ValidateAndThrowAsync(filter);

        _logger.LogInformation("Verifying orphan code is not already used (UC-ORP-05)");

        var code = filter.Code!.Trim();

        // BR-07 scope — the register the code will live in. When re-coding an orphan
        // (ExcludeOrphanId, the worklist edit) that is the orphan's own register: the form may
        // echo a stale charityId, and the verdict must judge the register that will hold the
        // code (P4). Charity callers stay pinned to their own register either way.
        Guid? scopeCharityId;
        if (userRole == "Charity")
        {
            scopeCharityId = userCharityId;
        }
        else if (filter.ExcludeOrphanId.HasValue)
        {
            var target = await _orphanRepository.IncludeNavigationProperties()
                .FirstOrDefaultAsync(o => o.Id == filter.ExcludeOrphanId.Value && !o.IsDeleted);
            scopeCharityId = target != null ? ResolveOrphanCharity(target) : filter.CharityId;
        }
        else
        {
            scopeCharityId = filter.CharityId;
        }

        // BR-07: the sponsorship code is unique within the charity's register. Deleted rows
        // release their codes (hand-filtered — no global soft-delete filter on this context).
        var query = _orphanRepository.IncludeNavigationProperties()
            .Where(o => o.Code == code && !o.IsDeleted && (o.Family == null || !o.Family.IsDeleted));
        if (scopeCharityId.HasValue)
        {
            query = query.Where(o => o.FK_CharityId == scopeCharityId.Value ||
                                     (o.Family != null && o.Family.FK_CharityId == scopeCharityId.Value));
        }
        if (filter.ExcludeOrphanId.HasValue)
        {
            query = query.Where(o => o.Id != filter.ExcludeOrphanId.Value);
        }

        var existing = await query.FirstOrDefaultAsync();
        if (existing == null)
        {
            return new OrphanCodeCheckDto { IsAvailable = true };
        }

        string? existingCharityName = null;
        var existingCharityId = ResolveOrphanCharity(existing);
        if (existingCharityId.HasValue)
        {
            existingCharityName = (await _charityRepository.GetByIdAsync(existingCharityId.Value))?.Name;
        }

        return new OrphanCodeCheckDto
        {
            IsAvailable = false,
            ExistingOrphanName = existing.FullName,
            ExistingCharityName = existingCharityName
        };
    }

    public async Task<OrphanDto> AssignOrphanCodeAsync(AssignOrphanCodeDto dto, Guid? userCharityId, string? userRole)
    {
        await _assignOrphanCodeValidator.ValidateAndThrowAsync(dto);

        _logger.LogInformation("Assigning sponsorship code to orphan: {OrphanId} (UC-ORP-06)", dto.OrphanId);

        var orphan = await _orphanRepository.IncludeNavigationProperties()
            .FirstOrDefaultAsync(o => o.Id == dto.OrphanId && !o.IsDeleted && (o.Family == null || !o.Family.IsDeleted));
        if (orphan == null)
        {
            throw new KeyNotFoundException($"Orphan with ID '{dto.OrphanId}' not found");
        }

        // HQ-only write (D3): the endpoint authorizes SuperAdmin/Admin only, so there is no
        // charity tenancy branch here — the scope parameters travel for interface symmetry.

        var code = dto.Code.Trim();

        // BR-07 re-checked inside the save path — the worklist check (UC-ORP-05) races with other
        // coders, this is the authoritative gate. The register judged is the orphan's own (D3).
        var scopeCharityId = ResolveOrphanCharity(orphan);
        var dupQuery = _orphanRepository.IncludeNavigationProperties()
            .Where(o => o.Code == code && o.Id != orphan.Id && !o.IsDeleted && (o.Family == null || !o.Family.IsDeleted));
        if (scopeCharityId.HasValue)
        {
            dupQuery = dupQuery.Where(o => o.FK_CharityId == scopeCharityId.Value ||
                                           (o.Family != null && o.Family.FK_CharityId == scopeCharityId.Value));
        }
        if (await dupQuery.AnyAsync())
        {
            throw new InvalidOperationException($"Code '{code}' is already used within this charity");
        }

        orphan.Code = code;

        // The first code assignment moves the orphan out of the uncoded pool: SponsorshipStatus
        // null/"Pending" becomes "Unsponsored" — coded but not yet matched with a sponsor.
        if (string.IsNullOrEmpty(orphan.SponsorshipStatus) || orphan.SponsorshipStatus == "Pending")
        {
            orphan.SponsorshipStatus = "Unsponsored";
        }

        _orphanRepository.Update(orphan);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Code assigned to orphan {OrphanId}: {Code}", orphan.Id, code);

        var result = _mapper.Map<OrphanDto>(orphan);
        if (orphan.DateOfBirth.HasValue)
        {
            result.Age = CalculateAge(orphan.DateOfBirth.Value);
        }
        return result;
    }

    /// <summary>
    /// Finds which of a family's phone holders matches the number, returning the holder as
    /// structured data (family code, holder name, type key) — the translated label is composed
    /// client-side (P16). Null when the family holds no match.
    /// </summary>
    private static (string FamilyCode, string Name, string Type)? MatchFamilyPhone(Family family, string number)
    {
        if (string.Equals(family.PhoneNumber?.Trim(), number, StringComparison.Ordinal))
        {
            return (family.Code ?? string.Empty, family.HeadOfFamily ?? string.Empty, "family");
        }
        if (family.Father != null && string.Equals(family.Father.Phone?.Trim(), number, StringComparison.Ordinal))
        {
            return (family.Code ?? string.Empty, family.Father.FullName ?? string.Empty, "father");
        }
        if (family.Mother != null && string.Equals(family.Mother.Phone?.Trim(), number, StringComparison.Ordinal))
        {
            return (family.Code ?? string.Empty, family.Mother.FullName ?? string.Empty, "mother");
        }
        if (family.Provider != null && string.Equals(family.Provider.Phone?.Trim(), number, StringComparison.Ordinal))
        {
            return (family.Code ?? string.Empty, family.Provider.FullName ?? string.Empty, "provider");
        }
        var orphan = family.Orphans?.FirstOrDefault(o => string.Equals(o.Phone?.Trim(), number, StringComparison.Ordinal));
        if (orphan != null)
        {
            return (family.Code ?? string.Empty, orphan.FullName ?? string.Empty, "orphan");
        }
        return null;
    }

    public async Task<PhoneCheckDto> CheckPhoneNumberDuplicateAsync(Guid familyId, PhoneCheckFilterDto filter, Guid? userCharityId, string? userRole)
    {
        await _phoneCheckValidator.ValidateAndThrowAsync(filter);

        _logger.LogInformation("Checking phone number duplication (UC-ORP-10) for family: {FamilyId}", familyId);

        // GetByIdAsync is an unfiltered Find — a soft-deleted family must not anchor a check.
        var family = await _familyRepository.IncludeNavigationProperties()
            .FirstOrDefaultAsync(f => f.Id == familyId && !f.IsDeleted);
        if (family == null)
        {
            throw new KeyNotFoundException($"Family with ID '{familyId}' not found");
        }

        if (userRole == "Charity" && userCharityId.HasValue && family.FK_CharityId != userCharityId.Value)
        {
            throw new UnauthorizedAccessException("This family belongs to another charity");
        }

        // Normalisation: trim only. Folding a leading trunk/zero prefix is locale-dependent and
        // would merge distinct numbers, so the compare is exact (recorded in the 8-10 notes).
        var number = filter.Number!.Trim();

        // The five-table span: every other family in scope with the number on any of its phone
        // holders — the family being edited never clashes with itself. The whole span runs in
        // SQL (P11): only the first matching family crosses the wire, and deleted families are
        // hand-filtered (no global soft-delete filter on this context).
        var scopeCharityId = userRole == "Charity" ? userCharityId : filter.CharityId;
        var query = _familyRepository.IncludeNavigationProperties()
            .Include(f => f.Father)
            .Include(f => f.Mother)
            .Include(f => f.Provider)
            .Where(f => f.Id != familyId && !f.IsDeleted);
        if (scopeCharityId.HasValue)
        {
            query = query.Where(f => f.FK_CharityId == scopeCharityId.Value);
        }

        // Review D4 2026-08-26: soft-deleted HOLDERS must not flag false duplicates — a deleted
        // relative's or orphan's number no longer occupies the slot (the family-level flag is
        // already filtered above).
        query = query.Where(f =>
            (f.PhoneNumber != null && f.PhoneNumber.Trim() == number) ||
            (f.Father != null && !f.Father.IsDeleted && f.Father.Phone != null && f.Father.Phone.Trim() == number) ||
            (f.Mother != null && !f.Mother.IsDeleted && f.Mother.Phone != null && f.Mother.Phone.Trim() == number) ||
            (f.Provider != null && !f.Provider.IsDeleted && f.Provider.Phone != null && f.Provider.Phone.Trim() == number) ||
            f.Orphans.Any(o => !o.IsDeleted && o.Phone != null && o.Phone.Trim() == number));

        var match = await query.FirstOrDefaultAsync();
        var holder = match != null ? MatchFamilyPhone(match, number) : null;

        // Type-agnostic match (simplification): filter.Type is accepted but no phone column in the
        // Domain carries a type today — see the 8-10 deferral note.
        return new PhoneCheckDto
        {
            IsDuplicate = holder != null,
            HolderFamilyCode = holder?.FamilyCode,
            HolderName = holder?.Name,
            HolderType = holder?.Type
        };
    }

    #endregion

    #region UC-FAM-11: Follow-Up Activity Report

    /// <summary>
    /// UC-FAM-11 متابعة إدخالات الأسر — one day of family-file register activity. A pure read
    /// over the inherited audit columns: created-today rows report kind Created (creation wins
    /// over a same-day update), otherwise a genuine update today reports kind Updated — a row
    /// whose UpdatedOn still equals its CreatedOn (never updated since insert) is not activity.
    /// </summary>
    public async Task<(IEnumerable<FamilyFollowUpListDto> Items, int TotalCount)> GetFollowUpActivityAsync(
        FamilyFollowUpFilterDto filter,
        Guid? userCharityId,
        string? userRole,
        int maxPageSize = 100)
    {
        await _followUpFilterValidator.ValidateAndThrowAsync(filter);

        var pageNumber = Math.Max(1, filter.PageNumber ?? 1);
        // Interactive reads clamp at 100; only the print path (UC-FAM-14's tracking sheet,
        // which prints the whole day's selection) passes the internal 5000 ceiling — a public
        // caller must not be able to pull the register through it.
        var pageSize = Math.Clamp(filter.PageSize ?? 10, 1, Math.Max(1, maxPageSize));
        var date = filter.Date.Date;

        var query = _familyRepository.TableNoTracking
            .Where(f => !f.IsDeleted);

        // Tenancy: a Charity-role caller only ever sees its own register's activity; HQ may
        // narrow to one charity through the filter. Scoped on FK_CharityId — the LIVE tenancy
        // column; OR-ing the CharityId mirror leaks rows the charity no longer owns after a
        // transfer. Legacy rows carrying only the mirror fall out (fail-closed), matching the
        // platform's one-tenancy-column rule.
        // Review P3 2026-08-26: scope from the caller's CLAIMS (pin-never-widen), not from the
        // controller-supplied role/charity strings — a CharityId claim pins whatever the role
        // string says; the payload narrow is HQ semantics; with neither, fail closed. The
        // legacy parameters stay on the signature (interface stability) but no longer scope.
        if (_currentUser.CharityId.HasValue)
        {
            var pinned = _currentUser.CharityId.Value;
            query = query.Where(f => f.FK_CharityId == pinned);
        }
        else if (filter.CharityId.HasValue)
        {
            var charityId = filter.CharityId.Value;
            query = query.Where(f => f.FK_CharityId == charityId);
        }
        else if (!_currentUser.IsHeadOffice)
        {
            // Review P3 2026-08-26: fail closed — neither a claim nor a payload narrow nor HQ.
            throw new UnauthorizedAccessException(
                "Caller has no charity or head-office scope; refusing unscoped query.");
        }

        var rows = await query
            .Where(f =>
                f.CreatedOn.Date == date ||
                (f.UpdatedOn != null && f.UpdatedOn != f.CreatedOn && f.UpdatedOn.Value.Date == date))
            .Select(f => new FamilyFollowUpListDto
            {
                FamilyId = f.Id,
                Code = f.Code,
                HeadOfFamily = f.HeadOfFamily,
                CharityName = f.Charity != null ? f.Charity.Name : null,
                ChangeKind = f.CreatedOn.Date == date ? "Created" : "Updated",
                ChangedBy = f.CreatedOn.Date == date ? f.CreatedBy : f.UpdatedBy,
                ChangedOn = f.CreatedOn.Date == date ? f.CreatedOn : f.UpdatedOn!.Value
            })
            .OrderByDescending(r => r.ChangedOn)
            .ThenBy(r => r.Code)
            .ToListAsync();

        // Orphans touched the same day — one grouped query over the page's families (no N+1).
        var page = rows.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        if (page.Count > 0)
        {
            var pageFamilyIds = page.Select(r => r.FamilyId).ToList();
            var orphanCounts = await _orphanRepository.TableNoTracking
                .Where(o => o.FamilyId != null && pageFamilyIds.Contains(o.FamilyId.Value) && !o.IsDeleted &&
                    (o.CreatedOn.Date == date ||
                     (o.UpdatedOn != null && o.UpdatedOn.Value.Date == date)))
                .GroupBy(o => o.FamilyId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();

            var byFamily = orphanCounts.ToDictionary(x => x.Key, x => x.Count);
            foreach (var row in page)
            {
                row.OrphansTouched = byFamily.TryGetValue(row.FamilyId, out var count) ? count : 0;
            }
        }

        return (page, rows.Count);
    }

    /// <inheritdoc />
    public async Task<object> GetFamilyFollowUpAsync(
        FamilyEntryTrackingFilterDto filter,
        Guid? userCharityId,
        string? userRole)
    {
        await _familyEntryTrackingValidator.ValidateAndThrowAsync(filter);

        // UC-RPT-13 vs UC-FAM-11: this is ENTRY tracking (registrations on/after a date),
        // not the one-day activity report — RegistrationDate drives the family set and the
        // orphans' CreatedOn drives the orphan totals. No global soft-delete filter on this
        // platform — filter explicitly at every level.
        var since = filter.Date?.Date;

        var familiesQuery = _familyRepository.TableNoTracking
            .Where(f => !f.IsDeleted && f.FK_CharityId != null);
        var orphansQuery = _orphanRepository.TableNoTracking
            .Where(o => !o.IsDeleted && o.FamilyId != null && o.Family != null && !o.Family.IsDeleted);

        // Tenancy (the file's GetFollowUpActivityAsync convention): a Charity-role caller is
        // clamped to its own register whatever the route id says; HQ may name any charity;
        // Guid.Empty means كل الجهات. Scoped on FK_CharityId — the LIVE tenancy column.
        if (string.Equals(userRole, "Charity", StringComparison.OrdinalIgnoreCase))
        {
            var pinned = userCharityId
                ?? throw new Exceptions.BusinessException("No charity is associated with this account");
            familiesQuery = familiesQuery.Where(f => f.FK_CharityId == pinned);
            orphansQuery = orphansQuery.Where(o => o.Family!.FK_CharityId == pinned);
        }
        else if (filter.CharityId.HasValue && filter.CharityId.Value != Guid.Empty)
        {
            var narrowed = filter.CharityId.Value;
            familiesQuery = familiesQuery.Where(f => f.FK_CharityId == narrowed);
            orphansQuery = orphansQuery.Where(o => o.Family!.FK_CharityId == narrowed);
        }

        if (since.HasValue)
        {
            var from = since.Value;
            familiesQuery = familiesQuery.Where(f => f.RegistrationDate >= from);
            orphansQuery = orphansQuery.Where(o => o.CreatedOn >= from);
        }

        if (string.Equals(filter.Mode, "totals", StringComparison.OrdinalIgnoreCase))
        {
            var newFamilies = await familiesQuery.CountAsync();
            var newOrphans = await orphansQuery.CountAsync();

            return new FamilyFollowUpTotalsDto
            {
                NewFamilies = newFamilies,
                NewOrphans = newOrphans,
                SinceDate = filter.Date
            };
        }

        var totalCount = await familiesQuery.CountAsync();

        var items = await familiesQuery
            .OrderByDescending(f => f.RegistrationDate)
            .ThenBy(f => f.Code)
            .Select(f => new FamilyFollowUpDetailDto
            {
                FamilyId = f.Id,
                FamilyCode = f.Code,
                HeadOfFamily = f.HeadOfFamily,
                // The EF Charity nav binds the legacy CharityId MIRROR — families carrying only
                // the live FK_CharityId column (post-transfer shape) would resolve null through
                // it. Resolve off the live column instead (smoke-caught 2026-08-24).
                CharityId = f.FK_CharityId,
                RegistrationDate = f.RegistrationDate
            })
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // اسم الجمعية — dictionary over the page's live-column charity ids.
        var charityIds = items.Where(r => r.CharityId.HasValue).Select(r => r.CharityId!.Value).Distinct().ToList();
        if (charityIds.Count > 0)
        {
            var names = await _charityRepository.TableNoTracking
                .Where(c => !c.IsDeleted && charityIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);
            foreach (var row in items)
            {
                if (row.CharityId.HasValue && names.TryGetValue(row.CharityId.Value, out var name))
                {
                    row.CharityName = name;
                }
            }
        }

        // عدد الأيتام — the family's orphan count, one grouped query over the page (no N+1).
        if (items.Count > 0)
        {
            var pageFamilyIds = items.Select(r => r.FamilyId).ToList();
            var counts = await _orphanRepository.TableNoTracking
                .Where(o => !o.IsDeleted && o.FamilyId != null && pageFamilyIds.Contains(o.FamilyId.Value))
                .GroupBy(o => o.FamilyId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();
            var byFamily = counts.ToDictionary(x => x.Key, x => x.Count);
            foreach (var row in items)
            {
                row.OrphansCount = byFamily.TryGetValue(row.FamilyId, out var count) ? count : 0;
            }
        }

        return new ReportPagedResult<FamilyFollowUpDetailDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            // Never 0 — TotalPages divides by it (divide-by-zero on serialise caught in 18-8's smoke).
            PageSize = filter.PageSize
        };
    }

    #endregion
}
