using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Outgoing ↔ Orphan report link repository (epic 16, UC-COR-18 / UC-COR-19).
/// The §21.S.6 grids and the §21.S.7 report are served as flat projections — orphan
/// name/code/guarantor/kinship and charity name are joined server-side.
/// </summary>
public class OutgoingOrphanReportRepository : Repository<OutgoingOrphanReport>, IOutgoingOrphanReportRepository
{
    private readonly DbSet<OutgoingOrphanReport> _dbSet;
    private readonly ApplicationDbContext _context;

    public OutgoingOrphanReportRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<OutgoingOrphanReport>();
        _context = context;
    }

    public async Task<IEnumerable<OutgoingOrphanReport>> GetByOutgoingAsync(Guid outgoingId)
    {
        // Soft delete is NOT a global query filter in this platform — detached
        // (soft-deleted) links stay out of the attached grid (review P1)
        return await _dbSet
            .Include(r => r.Orphan).ThenInclude(o => o!.Family)
            .Where(r => r.OutgoingId == outgoingId && !r.IsDeleted)
            .OrderBy(r => r.CreatedOn)
            .ToListAsync();
    }

    public Task<bool> IsOrphanAttachedAsync(Guid orphanId)
    {
        // Only live links count — a detached orphan can be re-attached elsewhere (review P1)
        return _dbSet.AnyAsync(r => r.OrphanId == orphanId && !r.IsDeleted);
    }

    public Task<OutgoingOrphanReport?> FindByLetterAndOrphanAsync(Guid outgoingId, Guid orphanId)
    {
        return _dbSet.FirstOrDefaultAsync(r => r.OutgoingId == outgoingId && r.OrphanId == orphanId && !r.IsDeleted);
    }

    public async Task<IEnumerable<OrphanCandidateRow>> GetUnattachedOrphansAsync(Guid? charityId)
    {
        // Only live links mark an orphan as attached (review P1)
        var attachedOrphanIds = await _dbSet
            .Where(r => !r.IsDeleted)
            .Select(r => r.OrphanId)
            .ToListAsync();

        var query = _context.Set<Orphan>()
            .Where(o => !attachedOrphanIds.Contains(o.Id));

        if (charityId.HasValue)
        {
            // BR-27 — the selection is scoped to the letter's charity; a null charity
            // (HQ-owned letter, D1 decision) deliberately offers all charities' orphans
            query = query.Where(o => o.FK_CharityId == charityId.Value);
        }

        return await query
            .OrderBy(o => o.Code)
            .Select(o => new OrphanCandidateRow
            {
                OrphanId = o.Id,
                Code = o.Code,
                FullName = o.FullName,
                GuarantorName = o.Family != null ? o.Family.HeadOfFamily : null,
                Kinship = o.Family != null ? o.Family.ProviderType : null
            })
            .ToListAsync();
    }

    public async Task<(IEnumerable<OutgoingOrphanReportRow> Rows, int TotalCount)> GetOrphanReportAsync(
        int? serial,
        int? year,
        Guid? charityId,
        int? countryId,
        DateTime? dateFrom,
        DateTime? dateTo,
        string childCode,
        int pageNumber,
        int pageSize)
    {
        // §21.S.7 (review P8): rows are the LIVE outgoing letters matching the criteria —
        // اليتيم مضاف للتقرير flags whether the requested orphan is actually attached,
        // so the column discriminates instead of rendering a constant ✓. The orphan is
        // resolved by code and matched against each letter's live links.
        var targetOrphanIds = _context.Set<Orphan>()
            .Where(o => o.Code == childCode)
            .Select(o => o.Id);

        var letters = from outgoing in _context.Set<Outgoing>().Where(o => !o.IsDeleted)
                      join charity in _context.Set<Charity>() on outgoing.FK_CharityId equals charity.Id into charityGroup
                      from charity in charityGroup.DefaultIfEmpty()
                      select new
                      {
                          Outgoing = outgoing,
                          CharityName = charity != null ? charity.Name : null,
                          CountryId = charity != null ? charity.CountryId : (int?)null
                      };

        var filtered = letters;
        if (serial.HasValue)
        {
            filtered = filtered.Where(x => x.Outgoing.Serial == serial.Value);
        }

        if (year.HasValue)
        {
            filtered = filtered.Where(x => x.Outgoing.Year == year.Value);
        }

        if (charityId.HasValue)
        {
            filtered = filtered.Where(x => x.Outgoing.FK_CharityId == charityId.Value);
        }

        if (countryId.HasValue)
        {
            // Country pin (review P7) — rides through the owning charity
            filtered = filtered.Where(x => x.CountryId == countryId.Value);
        }

        if (dateFrom.HasValue)
        {
            filtered = filtered.Where(x => x.Outgoing.Date >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            filtered = filtered.Where(x => x.Outgoing.Date <= dateTo.Value);
        }

        var totalCount = await filtered.CountAsync();

        var page = await filtered
            .OrderByDescending(x => x.Outgoing.Date ?? x.Outgoing.CreatedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var letterIds = page.Select(x => x.Outgoing.Id).ToList();

        // Per-letter orphan counts come from the full live-link set of the matched letters
        var counts = await _dbSet
            .Where(r => !r.IsDeleted && letterIds.Contains(r.OutgoingId))
            .GroupBy(r => r.OutgoingId)
            .Select(g => new { OutgoingId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OutgoingId, x => x.Count);

        // The attached flag — live links between the matched letters and the requested orphan
        var attachedLetterIds = (await _dbSet
                .Where(r => !r.IsDeleted && letterIds.Contains(r.OutgoingId) && targetOrphanIds.Contains(r.OrphanId))
                .Select(r => r.OutgoingId)
                .ToListAsync())
            .ToHashSet();

        var rows = page.Select(x => new OutgoingOrphanReportRow
        {
            OutgoingId = x.Outgoing.Id,
            Serial = x.Outgoing.Serial,
            Year = x.Outgoing.Year,
            LetterDate = x.Outgoing.Date,
            CharityName = x.CharityName,
            OrphanCount = counts.TryGetValue(x.Outgoing.Id, out var count) ? count : 0,
            OrphanAttached = attachedLetterIds.Contains(x.Outgoing.Id)
        }).ToList();

        return (rows, totalCount);
    }
}
