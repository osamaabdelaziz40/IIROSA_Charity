using Framework.Core.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Core.Data
{
    public abstract class BaseDbContext<TContext> : DbContext, IBaseDbContext
        where TContext : DbContext
    {
        protected BaseDbContext(DbContextOptions<TContext> options, IHttpContextAccessor httpContextAccessor, IIdentityTokenManager identityTokenManager) : base(options)
        {
            CurrentUserName = httpContextAccessor?.HttpContext?.User?.Identity?.Name;
            var token = httpContextAccessor?.HttpContext?.Request.Headers.Authorization;
            if (token.HasValue)
            {
                CurrentUserName = string.IsNullOrEmpty(token.Value) ? "" : identityTokenManager.GetCurrentUserName(token.Value.ToString());

            }
        }

        public string CurrentUserName { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //TODO: Upgrade the code to EF Core 3.1
            //foreach (var property in modelBuilder.Model.GetEntityTypes()
            //.SelectMany(t => t.GetProperties())
            //.Where(p => p.ClrType == typeof(decimal)))
            //{
            //    property.Relational().ColumnType = "decimal(18, 6)";
            //}

            //foreach (var property in modelBuilder.Model.GetEntityTypes()
            //.SelectMany(t => t.GetProperties())
            //.Where(p => p.ClrType == typeof(DateTime)))
            //{
            //    property.Relational().ColumnType = "datetime2";
            //}

            //foreach (var property in modelBuilder.Model.GetEntityTypes()
            //.SelectMany(t => t.GetProperties())
            //.Where(p => p.ClrType == typeof(string)))
            //{
            //    if (property.GetMaxLength() == null)
            //        property.SetMaxLength(100);
            //}

            //foreach (var property in modelBuilder.Model.GetEntityTypes()
            //.SelectMany(t => t.GetProperties())
            //.Where(p => p.ClrType == typeof(Guid) && p.Name == "Id"))
            //{
            //    property.SqlServer().DefaultValueSql = "newsequentialid()";
            //}
            //foreach (var property in modelBuilder.Model.GetEntityTypes()
            //    .Where(e => e.ClrType.BaseType == typeof(LookupEntityBase<>))
            //.SelectMany(t => t.GetProperties())
            //.Where(p => p.ClrType == typeof(int) && p.Name == "Id"))
            //{
            //    property.SqlServer().ValueGenerationStrategy =
            //       SqlServerValueGenerationStrategy.SequenceHiLo;
            //}

            //foreach (var property in modelBuilder.Model.GetEntityTypes()
            //.SelectMany(t => t.GetProperties())
            //.Where(p => p.Name == "CreatedOn"))
            //{
            //    property.SqlServer().DefaultValueSql = "getDate()";
            //}

            //foreach (var property in modelBuilder.Model.GetEntityTypes()
            //.SelectMany(t => t.GetProperties())
            //.Where(p => p.Name == "IsActive"))
            //{
            //    property.SqlServer().DefaultValueSql = "1";
            //}

            //foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            //{
            //    entityType.Relational().TableName = entityType.DisplayName();
            //    entityType.GetForeignKeys()
            //        .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade)
            //        .ToList()
            //        .ForEach(fk => fk.DeleteBehavior = DeleteBehavior.Restrict);
            //}

            base.OnModelCreating(modelBuilder);
        }

        public new int SaveChanges()
        {
            try
            {
                ChangeTracker.SetShadowProperties(CurrentUserName);
                ChangeTracker.Validate();

                return base.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        public new async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                ChangeTracker.SetShadowProperties(CurrentUserName);
                ChangeTracker.Validate();

                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        public async Task<int> SaveChangesWithAuditAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        /// <summary>
        /// Modify the input SQL query by adding passed parameters
        /// </summary>
        /// <param name="sql">The raw SQL query</param>
        /// <param name="parameters">The values to be assigned to parameters</param>
        /// <returns>Modified raw SQL query</returns>
        protected virtual string CreateSqlWithParameters(string sql, params object[] parameters)
        {
            //add parameters to sql
            for (var i = 0; i <= (parameters?.Length ?? 0) - 1; i++)
            {
                if (!(parameters[i] is DbParameter parameter))
                    continue;

                sql = $"{sql}{(i > 0 ? "," : string.Empty)} @{parameter.ParameterName}";

                //whether parameter is output
                if (parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Output)
                    sql = $"{sql} output";
            }

            return sql;
        }

        /// <summary>
        /// Creates a LINQ query for the entity based on a raw SQL query
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="sql">The raw SQL query</param>
        /// <param name="parameters">The values to be assigned to parameters</param>
        /// <returns>An IQueryable representing the raw SQL query</returns>
        public virtual IQueryable<TEntity> EntityFromSql<TEntity>(string sql, params object[] parameters) where TEntity : class
        {
            return Set<TEntity>().FromSqlRaw(CreateSqlWithParameters(sql, parameters), parameters);
        }
    }
}