using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// A flat employee candidate row (identity user) for the UC-COR-09 screen — projection
/// only, never an entity crossing the boundary.
/// </summary>
public class EmployeeCandidateRow
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
}

/// <summary>
/// Incoming ↔ Employee link repository (epic 16, UC-COR-09)
/// </summary>
public interface IIncomingEmployeeRepository : IRepository<IncomingEmployee>
{
    /// <summary>Live links of a letter, with the user navigation loaded.</summary>
    Task<IEnumerable<IncomingEmployee>> GetByIncomingAsync(Guid incomingId);

    /// <summary>Whether the (letter, user) link already exists.</summary>
    Task<bool> ExistsAsync(Guid incomingId, Guid userId);

    /// <summary>
    /// Active identity users of the letter's charity not yet attached to the letter
    /// (§21.S.3 drop-down). A null charity (HQ-owned letter) offers all active users.
    /// </summary>
    Task<IEnumerable<EmployeeCandidateRow>> GetAvailableEmployeesAsync(Guid incomingId, Guid? charityId);
}
