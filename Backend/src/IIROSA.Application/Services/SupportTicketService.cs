using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IIROSA.Application.DTOs.TechnicalSupport;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities.TechnicalSupport;
using IIROSA.Domain.Interfaces;
using AutoMapper;

namespace IIROSA.Application.Services;

/// <summary>
/// Support Ticket Service Implementation
/// Implements all use cases UC-13.1 through UC-13.10
/// </summary>
public class SupportTicketService : ISupportTicketService
{
    private readonly ISupportTicketRepository _ticketRepository;
    private readonly ITicketResponseRepository _responseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SupportTicketService> _logger;

    public SupportTicketService(
        ISupportTicketRepository ticketRepository,
        ITicketResponseRepository responseRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SupportTicketService> logger)
    {
        _ticketRepository = ticketRepository;
        _responseRepository = responseRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // UC-13.1: Create Support Ticket
    public async Task<SupportTicketDto> CreateTicketAsync(CreateSupportTicketDto dto, string userId)
    {
        _logger.LogInformation("Creating support ticket for user {UserId}", userId);

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

    // UC-13.3: View My Tickets
    public async Task<(IEnumerable<SupportTicketListDto> Items, int TotalCount)> GetMyTicketsAsync(string userId, SupportTicketFilterDto filter)
    {
        _logger.LogInformation("Getting tickets for user {UserId}", userId);

        var (tickets, totalCount) = await _ticketRepository.GetAllTicketsPagedAsync(
            filter.PageNumber,
            filter.PageSize,
            filter.CategoryId,
            filter.PriorityId,
            filter.StatusId,
            userId, // createdByUserId
            filter.StartDate,
            filter.EndDate);

        var ticketDtos = _mapper.Map<IEnumerable<SupportTicketListDto>>(tickets);

        return (ticketDtos, totalCount);
    }

    // UC-13.4: View All Tickets (Admin/Super Admin only)
    public async Task<(IEnumerable<SupportTicketListDto> Items, int TotalCount)> GetAllTicketsAsync(SupportTicketFilterDto filter)
    {
        _logger.LogInformation("Getting all tickets (admin view)");

        var (tickets, totalCount) = await _ticketRepository.GetAllTicketsPagedAsync(
            filter.PageNumber,
            filter.PageSize,
            filter.CategoryId,
            filter.PriorityId,
            filter.StatusId,
            filter.AssignedTo,
            filter.StartDate,
            filter.EndDate);

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
    public async Task<SupportTicketDetailDto> GetTicketDetailsAsync(Guid ticketId, string userId)
    {
        _logger.LogInformation("Getting details for ticket {TicketId}", ticketId);

        var hasAccess = await _ticketRepository.HasUserAccessAsync(ticketId, userId);
        if (!hasAccess)
            throw new UnauthorizedAccessException("You do not have permission to view this ticket");

        var ticket = await _ticketRepository.IncludeAllNavigationProperties()
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            throw new KeyNotFoundException($"Ticket with ID {ticketId} not found");

        var responses = await _responseRepository.GetByTicketIdAsync(ticketId);

        var ticketDetail = _mapper.Map<SupportTicketDetailDto>(ticket);
        ticketDetail.Responses = _mapper.Map<IEnumerable<TicketResponseDto>>(responses);
        ticketDetail.PublicResponses = ticketDetail.Responses.Where(r => !r.IsInternalNote);
        ticketDetail.InternalNotes = ticketDetail.Responses.Where(r => r.IsInternalNote);

        return ticketDetail;
    }

    // UC-13.9: Search Tickets (Admin/Super Admin only)
    public async Task<(IEnumerable<SupportTicketListDto> Items, int TotalCount)> SearchTicketsAsync(SupportTicketFilterDto filter)
    {
        _logger.LogInformation("Searching tickets with filter: {@Filter}", filter);

        var (tickets, totalCount) = await _ticketRepository.GetAllTicketsPagedAsync(
            filter.PageNumber,
            filter.PageSize,
            filter.CategoryId,
            filter.PriorityId,
            filter.StatusId,
            filter.AssignedTo,
            filter.StartDate,
            filter.EndDate);

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

        // Get status counts
        report.OpenTickets = await _ticketRepository.GetTicketsByStatusCountAsync(1); // Assuming 1 = Open
        report.InProgressTickets = await _ticketRepository.GetTicketsByStatusCountAsync(2); // Assuming 2 = In Progress
        report.ResolvedTickets = await _ticketRepository.GetTicketsByStatusCountAsync(3); // Assuming 3 = Resolved
        report.ClosedTickets = await _ticketRepository.GetTicketsByStatusCountAsync(4); // Assuming 4 = Closed

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

    public async Task DeleteTicketAsync(Guid id)
    {
        _logger.LogInformation("Deleting ticket {TicketId}", id);

        var ticket = await _ticketRepository.GetByIdAsync(id);
        if (ticket == null)
            throw new KeyNotFoundException($"Ticket with ID {id} not found");

        _ticketRepository.Delete(ticket);
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
        return (await _ticketRepository.GetByUserIdAsync(string.Empty)).Count();
    }

    public async Task<int> GetAllTicketsCountAsync()
    {
        return await _ticketRepository.GetTotalTicketsCountAsync();
    }

    public async Task<int> GetUnsolvedTicketsCountAsync()
    {
        return await _ticketRepository.GetUnsolvedTicketsCountAsync();
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
