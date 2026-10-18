using System;
using System.Collections.Generic;

namespace IIROSA.Application.DTOs.Charity
{
    /// <summary>
    /// One audited action attributed to a charity (UC-17 charity view) — Create or Update —
    /// with its field changes parsed out of the stored FieldChanges JSON.
    /// </summary>
    public class CharityAuditLogDto
    {
        public Guid Id { get; set; }
        public Guid CharityId { get; set; }
        public string CharityName { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public string PerformedBy { get; set; }
        public DateTime PerformedOn { get; set; }
        public string IpAddress { get; set; }
        public List<CharityAuditLogChangeDto> Changes { get; set; } = new List<CharityAuditLogChangeDto>();
    }

    /// <summary>
    /// A single field change inside an audited action
    /// </summary>
    public class CharityAuditLogChangeDto
    {
        public string Field { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

    /// <summary>
    /// Paged audit trail for one charity
    /// </summary>
    public class CharityAuditLogsDto
    {
        public List<CharityAuditLogDto> Items { get; set; } = new List<CharityAuditLogDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
