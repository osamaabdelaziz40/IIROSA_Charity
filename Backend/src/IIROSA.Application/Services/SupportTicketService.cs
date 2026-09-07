using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IIROSA.Application.DTOs.TechnicalSupport;
using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities.TechnicalSupport;
using IIROSA.Domain.Interfaces;
using AutoMapper;
using FluentValidation;

namespace IIROSA.Application.Services;

/// <summary>
/// Support Ticket Service Implementation
/// Implements all use cases UC-13.1 through UC-13.10
/// </summary>
public class SupportTicketService : ISupportTicketService
{
    private readonly ISupportTicketRepository _ticketRepository;
    private readonly ITicketResponseRepository _responseRepository;
    private readonly ISupportTicketLookupRepository _lookupRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SupportTicketService> _logger;
    private readonly IValidator<UpdateSupportTicketDto> _updateValidator;

    public SupportTicketService(
        ISupportTicketRepository ticketRepository,
        ITicketResponseRepository responseRepository,
        ISupportTicketLookupRepository lookupRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SupportTicketService> logger,
        IValidator<UpdateSupportTicketDto> updateValidator)
    {
        _ticketRepository = ticketRepository;
        _responseRepository = responseRepository;
        _lookupRepository = lookupRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _updateValidator = updateValidator;
    }

    // UC-13.1: Create Support Ticket
    public async Task<SupportTicketDto> CreateTicketAsync(CreateSupportTicketDto dto, string userId)
    {
        _logger.LogInformation("Creating support ticket for user {UserId}", userId);

        await ValidateLookupsAsync(dto.CategoryId, dto.PriorityId, 1);

        var ticket = new SupportTicket
        {
            Title = dto.Title,
            Message = dto.Message,
            CategoryId = dto.CategoryId,
            PriorityId = dto.PriorityId,
            StatusId = 1, // Default to "Open" status
            IsSolved = false,
            CreatedByUserId = userId,
            BrowserInfo = dto.BrowserInfo,
            PageUrl = dto.PageUrl,
            UserAction = dto.UserAction,
            AttachmentFileName = dto.AttachmentFileName,
            AttachmentFilePath = dto.AttachmentFilePath,
            AttachmentFileSize = dto.AttachmentFileSize
        };

        await _ticketRepository.AddAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Support ticket {TicketId} created successfully", ticket.Id);

        return await GetTicketWithDetailsAsync(ticket.Id);
    }

    // UC-13.2: Attach File to Ticket
    public async Task<SupportTicketDto> AttachFileToTicketAsync(Guid ticketId, string fileName, string filePath, long fileSize)
    {
        _logger.LogInformation("Attaching file to ticket {TicketId}", ticketId);

        var ticket = await _ticketRepository.GetByIdAsync(ticketId);
        if (ticket == null)
            throw new KeyNotFoundException($"Ticket with ID {ticketId} not found");

        ticket.AttachmentFileName = fileName;
        ticket.AttachmentFilePath = filePath;
        ticket.AttachmentFileSize = fileSize;

        _ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("File attached to ticket {TicketId} successfully", ticketId);

        return await GetTicketWithDetailsAsync(ticket.Id);
    }

    // UC-CST-04: Update Support Ticket (Admin/Super Admin only)
    public async Task<SupportTicketDto> UpdateTicketAsync(UpdateSupportTicketDto dto, string userId)
    {
        _logger.LogInformation("Updating ticket {TicketId} by user {UserId}", dto.Id, userId);

        await _updateValidator.ValidateAndThrowAsync(dto);
        await ValidateLookupsAsync(dto.CategoryId, dto.PriorityId, dto.StatusId);

        var ticket = await _ticketRepository.GetByIdAsync(dto.Id);
        if (ticket == null)
            throw new KeyNotFoundException($"Ticket with ID {dto.Id} not found");

        ticket.Title = dto.Title;
        ticket.Message = dto.Message;
        ticket.CategoryId = dto.CategoryId;
        ticket.PriorityId = dto.PriorityId;
        // Optional on the wire — null leaves the status unchanged (the edit form has no
        // status control; a stale snapshot used to revert concurrent status changes).
        if (dto.StatusId.HasValue)
            ticket.StatusId = dto.StatusId.Value;

        _ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Ticket {TicketId} updated successfully", dto.Id);

        return await GetTicketWithDetailsAsync(ticket.Id);
    }

    // Ticket form lookups (categories, priorities, statuses)
    public async Task<TicketLookupsDto> GetTicketLookupsAsync()
    {
        var categories = await _lookupRepository.GetCategoriesAsync();
        var priorities = await _lookupRepository.GetPrioritiesAsync();
        var statuses = await _lookupRepository.GetStatusesAsync();

        return new TicketLookupsDto
        {
            Categories = _mapper.Map<IEnumerable<LookupDto>>(categories),
            Priorities = _mapper.Map<IEnumerable<LookupDto>>(priorities),
            Statuses = _mapper.Map<IEnumerable<LookupDto>>(statuses)
        };
    }

    // UC-13.3: View My Tickets
    public async Task<(IEnumerable<SupportTicketListDto> Items, int TotalCount)> GetMyTicketsAsync(string userId, SupportTicketFilterDto filter)
    {
        _logger.LogInformation("Getting tickets for user {UserId}", userId);

        // SearchTerm/sort pass through; createdByUserId is forced server-side so a caller
        // cannot read others' tickets.
        var (tickets, totalCount) = await _ticketRepository.GetAllTicketsPagedAsync(
            filter.PageNumber, filter.PageSize,
            categoryId: filter.CategoryId, priorityId: filter.PriorityId, statusId: filter.StatusId,
            createdByUserId: userId,
            startDate: filter.StartDate, endDate: filter.EndDate,
            searchTerm: filter.SearchTerm, sortBy: filter.SortBy, sortDirection: filter.SortDirection);

        var ticketDtos = _mapper.Map<IEnumerable<SupportTicketListDto>>(tickets);

        return (ticketDtos, totalCount);
    }

    // UC-13.4: View All Tickets (Admin/Super Admin only)
    public async Task<(IEnumerable<SupportTicketListDto> Items, int TotalCount)> GetAllTicketsAsync(SupportTicketFilterDto filter)
    {
        _logger.LogInformation("Getting all tickets (admin view)");

        // AssignedTo binds to its own parameter — the old call passed it into the
        // createdByUserId slot, filtering creators instead of assignees.
        var (tickets, totalCount) = await _ticketRepository.GetAllTicketsPagedAsync(
            filter.PageNumber, filter.PageSize,
            categoryId: filter.CategoryId, priorityId: filter.PriorityId, statusId: filter.StatusId,
            assignedTo: filter.AssignedTo,
            startDate: filter.StartDate, endDate: filter.EndDate,
            isSolved: filter.IsSolved,
            searchTerm: filter.SearchTerm, sortBy: filter.SortBy, sortDirection: filter.SortDirection);

        var ticketDtos = _mapper.Map<IEnumerable<SupportTicketListDto>>(tickets);

        return (ticketDtos, totalCount);
    }

    // UC-13.5: Update Ticket Status (Admin/Super Admin only)
    public async Task UpdateTicketStatusAsync(UpdateTicketStatusDto dto, string adminUserId)
    {
        _logger.LogInformation("Updating status for ticket {TicketId} to status {StatusId}", dto.TicketId, dto.StatusId);

        var ticket = await _ticketRepository.GetByIdAsync(dto.TicketId);
        if (ticket == null)
            throw new KeyNotFoundException($"Ticket with ID {dto.TicketId} not found");

        ticket.StatusId = dto.StatusId;
        _ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync();

        // Add status note as a response if provided
        if (!string.IsNullOrWhiteSpace(dto.StatusNote))
        {
            var response = new TicketResponse
            {
                TicketId = dto.TicketId,
                ResponseText = $"Status updated to: {dto.StatusNote}",
                IsInternalNote = false,
                RespondedByUserId = adminUserId,
                ResponderName = "System",
                ResponderEmail = "system@iirosa.com"
            };

            await _responseRepository.AddAsync(response);
            await _unitOfWork.SaveChangesAsync();
        }

        _logger.LogInformation("Ticket status updated successfully");
    }

    // UC-13.6: Mark Ticket as Solved (Admin/Super Admin only)
    public async Task MarkTicketAsSolvedAsync(MarkTicketSolvedDto dto, string adminUserId)
    {
        _logger.LogInformation("Marking ticket {TicketId} as solved", dto.TicketId);

        var ticket = await _ticketRepository.GetByIdAsync(dto.TicketId);
        if (ticket == null)
            throw new KeyNotFoundException($"Ticket with ID {dto.TicketId} not found");

        if (ticket.IsSolved)
            throw new InvalidOperationException("Ticket is already marked as solved");

        ticket.IsSolved = true;
        ticket.ResolutionDescription = dto.ResolutionDescription;
        ticket.ResolvedOn = DateTime.UtcNow;
        ticket.ResolvedBy = adminUserId;
        ticket.StatusId = 3; // Set to "Resolved" status

        // Add attachment if provided
        if (!string.IsNullOrWhiteSpace(dto.AttachmentFileName))
        {
            ticket.AttachmentFileName = dto.AttachmentFileName;
            ticket.AttachmentFilePath = dto.AttachmentFilePath;
            ticket.AttachmentFileSize = dto.AttachmentFileSize;
        }

        _ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync();

        // Add resolution as a response
        var responseText = $"Ticket marked as solved.\n\nResolution: {dto.ResolutionDescription}";
        if (!string.IsNullOrWhiteSpace(dto.SolutionSteps))
        {
            responseText += $"\n\nSolution Steps:\n{dto.SolutionSteps}";
        }

        var response = new TicketResponse
        {
            TicketId = dto.TicketId,
            ResponseText = responseText,
            IsInternalNote = false,
            RespondedByUserId = adminUserId,
            ResponderName = "Support Team",
            ResponderEmail = "support@iirosa.com"
        };

        await _responseRepository.AddAsync(response);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Ticket marked as solved successfully");
    }

    // UC-13.7: Add Ticket Response (Admin/Super Admin only)
    public async Task<TicketResponseDto> AddTicketResponseAsync(CreateTicketResponseDto dto, string responderUserId, string responderName, string responderEmail)
    {
        _logger.LogInformation("Adding response to ticket {TicketId}", dto.TicketId);

        var ticket = await _ticketRepository.GetByIdAsync(dto.TicketId);
        if (ticket == null)
            throw new KeyNotFoundException($"Ticket with ID {dto.TicketId} not found");

        var response = new TicketResponse
        {
            TicketId = dto.TicketId,
            ResponseText = dto.ResponseText,
            IsInternalNote = dto.IsInternalNote,
            RespondedByUserId = responderUserId,
            ResponderName = responderName,
            ResponderEmail = responderEmail,
            AttachmentFileName = dto.AttachmentFileName,
            AttachmentFilePath = dto.AttachmentFilePath,
            AttachmentFileSize = dto.AttachmentFileSize
        };

        await _responseRepository.AddAsync(response);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Response added to ticket {TicketId} successfully", dto.TicketId);

        return _mapper.Map<TicketResponseDto>(response);
    }

    // UC-13.8: View Ticket Details
    public async Task<SupportTicketDetailDto> GetTicketDetailsAsync(Guid ticketId, string userId, bool isAdmin = false)
    {
        _logger.LogInformation("Getting details for ticket {TicketId}", ticketId);

        // Admins may view any ticket — previously the controller admitted them past its own
        // check and this creator-only re-check then threw, 403-ing the whole admin flow.
        if (!isAdmin)
        {
            var hasAccess = await _ticketRepository.HasUserAccessAsync(ticketId, userId);
            if (!hasAccess)
                throw new UnauthorizedAccessException("You do not have permission to view this ticket");
        }

        var ticket = await _ticketRepository.IncludeAllNavigationProperties()
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            throw new KeyNotFoundException($"Ticket with ID {ticketId} not found");

        var responses = await _responseRepository.GetByTicketIdAsync(ticketId);

        var ticketDetail = _mapper.Map<SupportTicketDetailDto>(ticket);
        var responseDtos = _mapper.Map<IEnumerable<TicketResponseDto>>(responses);
        ticketDetail.PublicResponses = responseDtos.Where(r => !r.IsInternalNote).ToList();
        // Internal notes must never reach a non-admin caller (the ticket creator) — the
        // frontend only hides them visually, so the payload itself is filtered.
        ticketDetail.Responses = isAdmin ? responseDtos.ToList() : ticketDetail.PublicResponses.ToList();
        ticketDetail.InternalNotes = isAdmin
            ? responseDtos.Where(r => r.IsInternalNote).ToList()
            : new List<TicketResponseDto>();

        return ticketDetail;
    }

    // UC-13.9: Search Tickets (Admin/Super Admin only)
    public async Task<(IEnumerable<SupportTicketListDto> Items, int TotalCount)> SearchTicketsAsync(SupportTicketFilterDto filter)
    {
        _logger.LogInformation("Searching tickets with filter: {@Filter}", filter);

        var (tickets, totalCount) = await _ticketRepository.GetAllTicketsPagedAsync(
            filter.PageNumber, filter.PageSize,
            categoryId: filter.CategoryId, priorityId: filter.PriorityId, statusId: filter.StatusId,
            assignedTo: filter.AssignedTo,
            startDate: filter.StartDate, endDate: filter.EndDate,
            isSolved: filter.IsSolved,
            searchTerm: filter.SearchTerm, sortBy: filter.SortBy, sortDirection: filter.SortDirection);

        var ticketDtos = _mapper.Map<IEnumerable<SupportTicketListDto>>(tickets);

        return (ticketDtos, totalCount);
    }

    // UC-13.10: Generate Support Report (Admin/Super Admin only)
    public async Task<SupportTicketReportDto> GenerateReportAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Generating support report from {StartDate} to {EndDate}", startDate, endDate);

        var tickets = await _ticketRepository.GetByDateRangeAsync(startDate, endDate);
        var report = new SupportTicketReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalTickets = tickets.Count(),
            SolvedTickets = tickets.Count(t => t.IsSolved),
            UnsolvedTickets = tickets.Count(t => !t.IsSolved),
            Tickets = _mapper.Map<IEnumerable<SupportTicketListDto>>(tickets)
        };

        // Group by status
        report.TicketsByStatus = tickets
            .GroupBy(t => t.Status != null ? (t.Status.NameAr ?? t.Status.NameEn) : "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        // Group by priority
        report.TicketsByPriority = tickets
            .GroupBy(t => t.Priority != null ? (t.Priority.NameAr ?? t.Priority.NameEn) : "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        // Group by category
        report.TicketsByCategory = tickets
            .GroupBy(t => t.Category != null ? (t.Category.NameAr ?? t.Category.NameEn) : "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        // Group by creator
        report.TicketsByCreator = tickets
            .GroupBy(t => t.CreatedByUserId)
            .ToDictionary(g => g.Key, g => g.Count());

        // Calculate average resolution time
        var solvedTickets = tickets.Where(t => t.IsSolved && t.ResolvedOn.HasValue);
        if (solvedTickets.Any())
        {
            var totalHours = solvedTickets
                .Sum(t => (t.ResolvedOn!.Value - t.CreatedOn).TotalHours);
            report.AverageResolutionTimeHours = (decimal)(totalHours / solvedTickets.Count());
        }

        // Calculate SLA compliance (assuming 24-hour SLA)
        var slaHours = 24;
        report.TicketsResolvedWithinSLA = solvedTickets
            .Count(t => (t.ResolvedOn!.Value - t.CreatedOn).TotalHours <= slaHours);
        report.TicketsBreachedSLA = solvedTickets.Count() - report.TicketsResolvedWithinSLA;

        // Status tiles are period-scoped — the previous global GetTicketsByStatusCountAsync calls
        // ignored the date range and could contradict the period's own totalTickets.
        // Seed ids: 1 = Open, 2 = In Progress, 3 = Resolved, 4 = Closed.
        report.OpenTickets = tickets.Count(t => t.StatusId == 1);
        report.InProgressTickets = tickets.Count(t => t.StatusId == 2);
        report.ResolvedTickets = tickets.Count(t => t.StatusId == 3);
        report.ClosedTickets = tickets.Count(t => t.StatusId == 4);

        return report;
    }

    // Additional helper methods
    public async Task<SupportTicketDto?> GetByIdAsync(Guid id)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);
        return ticket == null ? null : await GetTicketWithDetailsAsync(id);
    }

    public async Task<SupportTicketDto?> GetByCodeAsync(string code)
    {
        // Tickets don't have codes, but method is required by interface
        throw new NotImplementedException("Tickets do not have codes");
    }

    public async Task<bool> HasUserAccessAsync(Guid ticketId, string userId)
    {
        return await _ticketRepository.HasUserAccessAsync(ticketId, userId);
    }

    public async Task DeleteTicketAsync(Guid id, string deletedBy)
    {
        _logger.LogInformation("Deleting ticket {TicketId}", id);

        var ticket = await _ticketRepository.GetByIdAsync(id);
        if (ticket == null)
            throw new KeyNotFoundException($"Ticket with ID {id} not found");

        // Soft delete per platform rule — a hard delete destroyed the ticket and cascaded
        // its response history.
        ticket.IsDeleted = true;
        ticket.DeletedOn = DateTime.UtcNow;
        ticket.DeletedBy = deletedBy;
        _ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Ticket deleted successfully");
    }

    public async Task AssignTicketAsync(Guid ticketId, string assignedToUserId)
    {
        _logger.LogInformation("Assigning ticket {TicketId} to user {UserId}", ticketId, assignedToUserId);

        var ticket = await _ticketRepository.GetByIdAsync(ticketId);
        if (ticket == null)
            throw new KeyNotFoundException($"Ticket with ID {ticketId} not found");

        ticket.AssignedTo = assignedToUserId;
        _ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Ticket assigned successfully");
    }

    public async Task<int> GetMyTicketsCountAsync(string userId)
    {
        // Was querying CreatedByUserId == string.Empty — permanently 0.
        return (await _ticketRepository.GetByUserIdAsync(userId)).Count();
    }

    public async Task<int> GetAllTicketsCountAsync()
    {
        return await _ticketRepository.GetTotalTicketsCountAsync();
    }

    public async Task<int> GetUnsolvedTicketsCountAsync()
    {
        return await _ticketRepository.GetUnsolvedTicketsCountAsync();
    }

    // FK guards — lookup ids are Restrict; a missing id must surface as a 400 field error,
    // not a 500 leaking the SQL constraint message.
    private async Task ValidateLookupsAsync(int categoryId, int priorityId, int? statusId)
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>();

        if (!await _lookupRepository.CategoryExistsAsync(categoryId))
            failures.Add(new("CategoryId", $"Category {categoryId} does not exist"));
        if (!await _lookupRepository.PriorityExistsAsync(priorityId))
            failures.Add(new("PriorityId", $"Priority {priorityId} does not exist"));
        if (statusId.HasValue && !await _lookupRepository.StatusExistsAsync(statusId.Value))
            failures.Add(new("StatusId", $"Status {statusId} does not exist"));

        if (failures.Count > 0)
            throw new ValidationException(failures);
    }

    // Private helper method to get ticket with full details
    private async Task<SupportTicketDto> GetTicketWithDetailsAsync(Guid ticketId)
    {
        var ticket = await _ticketRepository.IncludeAllNavigationProperties()
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            throw new KeyNotFoundException($"Ticket with ID {ticketId} not found");

        var responses = await _responseRepository.GetByTicketIdAsync(ticketId);

        var ticketDto = _mapper.Map<SupportTicketDto>(ticket);
        ticketDto.ResponseCount = responses.Count();

        return ticketDto;
    }
}
