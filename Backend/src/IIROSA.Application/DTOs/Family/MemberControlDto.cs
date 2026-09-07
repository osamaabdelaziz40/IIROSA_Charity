namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// UC-FAM-07 نقل يتيم بين الأسر (and UC-FAM-08 for guardians) — the member-control request.
/// The MemberType and Action values are the legacy wire contract, kept literal:
/// MemberType 1 = orphan/child, 2 = guardian; Action 0 = detach to a new holding family,
/// 1 = attach to an existing family by code.
/// </summary>
public class MemberControlDto
{
    /// <summary>1 = orphan (UC-FAM-07), 2 = guardian (UC-FAM-08).</summary>
    public int MemberType { get; set; } = 1;

    /// <summary>0 = detach to a new holding family, 1 = attach to the family whose code is given.</summary>
    public int Action { get; set; }

    /// <summary>Target family register code — required when Action = 1.</summary>
    public string? TargetFamilyCode { get; set; }

    /// <summary>Why the member is being detached — required when Action = 0 (appended to the orphan's Notes).</summary>
    public string? Justification { get; set; }
}
