using IIROSA.Domain.Enums;

namespace IIROSA.Application.DTOs.OrphanReportCycles;

/// <summary>
/// Cycle read DTO — the register row and the edit-screen load (UC-ORC-01…03).
/// Status is carried for display only; it is never accepted back by any write DTO.
/// </summary>
public class OrphanReportCycleDto
{
    public Guid Id { get; set; }

    /// <summary>العنوان — Arabic title, the primary display name.</summary>
    public string TitleAr { get; set; } = string.Empty;

    /// <summary>English title (optional).</summary>
    public string? TitleEn { get; set; }

    public string? DescriptionAr { get; set; }

    public string? DescriptionEn { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    /// <summary>Derived status — computed by the background service, read-only on every screen.</summary>
    public OrphanReportCycleStatus Status { get; set; }

    public DateTime? StatusChangedOn { get; set; }

    /// <summary>Live reports submitted inside the cycle (the register's count column).</summary>
    public int ReportsCount { get; set; }

    /// <summary>Coded orphans across the platform — the denominator of the progress column.</summary>
    public int CodedOrphansCount { get; set; }

    /// <summary>Submitted ÷ coded, rounded — 0 when there is no population yet.</summary>
    public int ProgressPercent { get; set; }

    public DateTime CreatedOn { get; set; }
}

/// <summary>
/// Create payload (UC-ORC-02). Status is not on the wire — the service computes it
/// from the dates against now.
/// </summary>
public class CreateOrphanReportCycleDto
{
    /// <summary>العنوان بالعربية — required, ≤ 250 chars.</summary>
    public string TitleAr { get; set; } = string.Empty;

    public string? TitleEn { get; set; }

    public string? DescriptionAr { get; set; }

    public string? DescriptionEn { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}

/// <summary>
/// Update payload (UC-ORC-03). The cycle identity is the route id; BR-ORC-08 guards
/// (completed read-only, end date not pulled before today) run in the service.
/// </summary>
public class UpdateOrphanReportCycleDto : CreateOrphanReportCycleDto
{
    public Guid Id { get; set; }
}

/// <summary>Register filter (UC-ORC-01) — free text over the titles/description plus status.</summary>
public class OrphanReportCycleFilterDto
{
    /// <summary>Free-text search across TitleAr/TitleEn/DescriptionAr/DescriptionEn.</summary>
    public string? Search { get; set; }

    public OrphanReportCycleStatus? Status { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

/// <summary>Paged register result (UC-ORC-01).</summary>
public class OrphanReportCyclePagedResult
{
    public List<OrphanReportCycleDto> Items { get; set; } = new();

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
