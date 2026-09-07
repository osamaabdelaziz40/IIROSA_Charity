using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Check repository (chapter 16, UC-CHQ). The register is paged and tenancy-scoped;
/// the application service composes filters over <see cref="Query"/> so charity
/// scoping is applied before any paging.
/// </summary>
public interface ICheckRepository : IRepository<Check>
{
    /// <summary>
    /// Cheque query with Bank, ChequeBeneficiary and Charity included, ordered for the register.
    /// </summary>
    IQueryable<Check> Query();
}
