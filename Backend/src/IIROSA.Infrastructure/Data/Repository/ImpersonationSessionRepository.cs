//using Framework.Core.Data.Repositories;
//using IIROSA.Domain.Contracts;
//using IIROSA.Domain.Entities;
//using IIROSA.Domain.Interfaces;
//using Microsoft.EntityFrameworkCore;

//namespace IIROSA.Infrastructure.Data.Repository;

///// <summary>
///// Repository implementation for ImpersonationSession entity
///// </summary>
//public class ImpersonationSessionRepository : RepositoryBase<IAppDbContext, ImpersonationSession>, IImpersonationSessionRepository
//{
//    public ImpersonationSessionRepository(IAppDbContext dbContext) : base(dbContext)
//    {
//    }

//    public async Task<IIROSA.Domain.Entities.ImpersonationSession?> GetActiveSessionByImpersonatorAsync(Guid impersonatorId, System.Threading.CancellationToken cancellationToken = default)
//    {
//        return await DbSet
//            .Where(s => s.ImpersonatorUserId == impersonatorId && s.IsActive && !s.IsDeleted)
//            .OrderByDescending(s => s.StartTime)
//            .FirstOrDefaultAsync(cancellationToken);
//    }

//    public async Task<IEnumerable<IIROSA.Domain.Entities.ImpersonationSession>> GetActiveSessionsAsync(System.Threading.CancellationToken cancellationToken = default)
//    {
//        return await DbSet
//            .Where(s => s.IsActive && !s.IsDeleted)
//            .OrderByDescending(s => s.StartTime)
//            .ToListAsync(cancellationToken);
//    }

//    public async Task<IEnumerable<IIROSA.Domain.Entities.ImpersonationSession>> GetHistoryByUserAsync(Guid userId, System.Threading.CancellationToken cancellationToken = default)
//    {
//        return await DbSet
//            .Where(s => (s.ImpersonatorUserId == userId || s.ImpersonatedUserId == userId) && !s.IsDeleted)
//            .OrderByDescending(s => s.StartTime)
//            .ToListAsync(cancellationToken);
//    }

//    public async Task<IEnumerable<IIROSA.Domain.Entities.ImpersonationSession>> GetHistoryAsync(
//        Guid? impersonatorId = null,
//        Guid? impersonatedId = null,
//        DateTime? startDate = null,
//        DateTime? endDate = null,
//        System.Threading.CancellationToken cancellationToken = default)
//    {
//        var query = DbSet.Where(s => !s.IsDeleted);

//        if (impersonatorId.HasValue)
//        {
//            query = query.Where(s => s.ImpersonatorUserId == impersonatorId.Value);
//        }

//        if (impersonatedId.HasValue)
//        {
//            query = query.Where(s => s.ImpersonatedUserId == impersonatedId.Value);
//        }

//        if (startDate.HasValue)
//        {
//            query = query.Where(s => s.StartTime >= startDate.Value);
//        }

//        if (endDate.HasValue)
//        {
//            query = query.Where(s => s.StartTime <= endDate.Value);
//        }

//        return await query
//            .OrderByDescending(s => s.StartTime)
//            .ToListAsync(cancellationToken);
//    }

//    public async Task IncrementActionsCountAsync(Guid sessionId, System.Threading.CancellationToken cancellationToken = default)
//    {
//        var session = await GetByIdAsync(sessionId, cancellationToken);
//        if (session != null)
//        {
//            session.ActionsPerformedCount++;
//            session.UpdatedOn = DateTime.UtcNow;
//            Update(session);
//            // Note: SaveChangesAsync should be called by the service
//        }
//    }

//    public async Task TerminateSessionAsync(Guid sessionId, Guid? terminatedBy = null, string? reason = null, System.Threading.CancellationToken cancellationToken = default)
//    {
//        var session = await GetByIdAsync(sessionId, cancellationToken);
//        if (session != null && session.IsActive)
//        {
//            session.EndTime = DateTime.UtcNow;
//            session.IsActive = false;
//            session.TerminatedBy = terminatedBy;
//            session.TerminationReason = reason;
//            session.UpdatedOn = DateTime.UtcNow;
//            Update(session);
//            // Note: SaveChangesAsync should be called by the service
//        }
//    }
//}
