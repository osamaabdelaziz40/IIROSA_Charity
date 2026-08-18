using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Extension methods for specific repository interfaces
/// Bridges the gap between base interface methods and commonly used method names
/// Note: SaveChangesAsync is provided by RepositoryGenericExtensions
/// </summary>
public static class RepositoryInterfaceExtensions
{
    // Employee extensions
    public static async Task<Employee?> GetByIdAsync(this IEmployeeRepository repository, Guid id)
    {
        return await ((IRepository<Employee>)repository).GetByIdAsync((object)id);
    }

    public static async Task<IEnumerable<Employee>> GetAllAsync(this IEmployeeRepository repository)
    {
        return await ((IRepository<Employee>)repository).GetAllAsync();
    }

    public static async Task AddAsync(this IEmployeeRepository repository, Employee employee)
    {
        await ((IRepository<Employee>)repository).InsertAsync(employee);
    }

    public static void Update(this IEmployeeRepository repository, Employee employee)
    {
        ((IRepository<Employee>)repository).Update(employee);
    }

    public static void Delete(this IEmployeeRepository repository, Employee employee)
    {
        ((IRepository<Employee>)repository).Delete(employee);
    }

    // Charity extensions
    public static async Task<Charity?> GetByIdAsync(this ICharityRepository repository, Guid id)
    {
        return await ((IRepository<Charity>)repository).GetByIdAsync((object)id);
    }

    public static async Task<IEnumerable<Charity>> GetAllAsync(this ICharityRepository repository)
    {
        return await ((IRepository<Charity>)repository).GetAllAsync();
    }

    public static async Task AddAsync(this ICharityRepository repository, Charity charity)
    {
        await ((IRepository<Charity>)repository).InsertAsync(charity);
    }

    public static void Update(this ICharityRepository repository, Charity charity)
    {
        ((IRepository<Charity>)repository).Update(charity);
    }

    public static void Delete(this ICharityRepository repository, Charity charity)
    {
        ((IRepository<Charity>)repository).Delete(charity);
    }

    // Incoming extensions
    public static async Task<Incoming?> GetByIdAsync(this IIncomingRepository repository, Guid id)
    {
        return await ((IRepository<Incoming>)repository).GetByIdAsync((object)id);
    }

    public static async Task<IEnumerable<Incoming>> GetAllAsync(this IIncomingRepository repository)
    {
        return await ((IRepository<Incoming>)repository).GetAllAsync();
    }

    public static async Task AddAsync(this IIncomingRepository repository, Incoming entity)
    {
        await ((IRepository<Incoming>)repository).InsertAsync(entity);
    }

    public static void Update(this IIncomingRepository repository, Incoming entity)
    {
        ((IRepository<Incoming>)repository).Update(entity);
    }

    public static void Delete(this IIncomingRepository repository, Incoming entity)
    {
        ((IRepository<Incoming>)repository).Delete(entity);
    }

    // Outgoing extensions
    public static async Task<Outgoing?> GetByIdAsync(this IOutgoingRepository repository, Guid id)
    {
        return await ((IRepository<Outgoing>)repository).GetByIdAsync((object)id);
    }

    public static async Task<IEnumerable<Outgoing>> GetAllAsync(this IOutgoingRepository repository)
    {
        return await ((IRepository<Outgoing>)repository).GetAllAsync();
    }

    public static async Task AddAsync(this IOutgoingRepository repository, Outgoing entity)
    {
        await ((IRepository<Outgoing>)repository).InsertAsync(entity);
    }

    public static void Update(this IOutgoingRepository repository, Outgoing entity)
    {
        ((IRepository<Outgoing>)repository).Update(entity);
    }

    public static void Delete(this IOutgoingRepository repository, Outgoing entity)
    {
        ((IRepository<Outgoing>)repository).Delete(entity);
    }

    // Mission extensions
    public static async Task<Mission?> GetByIdAsync(this IMissionRepository repository, Guid id)
    {
        return await ((IRepository<Mission>)repository).GetByIdAsync((object)id);
    }

    public static async Task<IEnumerable<Mission>> GetAllAsync(this IMissionRepository repository)
    {
        return await ((IRepository<Mission>)repository).GetAllAsync();
    }

    public static async Task AddAsync(this IMissionRepository repository, Mission mission)
    {
        await ((IRepository<Mission>)repository).InsertAsync(mission);
    }

    public static void Update(this IMissionRepository repository, Mission mission)
    {
        ((IRepository<Mission>)repository).Update(mission);
    }

    public static void Delete(this IMissionRepository repository, Mission mission)
    {
        ((IRepository<Mission>)repository).Delete(mission);
    }

    // OfficeProject extensions
    public static async Task<OfficeProject?> GetByIdAsync(this IOfficeProjectRepository repository, Guid id)
    {
        return await ((IRepository<OfficeProject>)repository).GetByIdAsync((object)id);
    }

    public static async Task<IEnumerable<OfficeProject>> GetAllAsync(this IOfficeProjectRepository repository)
    {
        return await ((IRepository<OfficeProject>)repository).GetAllAsync();
    }

    public static async Task AddAsync(this IOfficeProjectRepository repository, OfficeProject project)
    {
        await ((IRepository<OfficeProject>)repository).InsertAsync(project);
    }

    public static void Update(this IOfficeProjectRepository repository, OfficeProject project)
    {
        ((IRepository<OfficeProject>)repository).Update(project);
    }

    public static void Delete(this IOfficeProjectRepository repository, OfficeProject project)
    {
        ((IRepository<OfficeProject>)repository).Delete(project);
    }
}
