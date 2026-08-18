// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Data
{
    #region usings

    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    #endregion usings

    /// <summary>
    ///     The extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     The member access.
        /// </summary>
        public const BindingFlags MemberAccess = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
                                                 | BindingFlags.Instance | BindingFlags.IgnoreCase;

        /// <summary>
        ///     The member public instance access.
        /// </summary>
        public const BindingFlags MemberPublicInstanceAccess =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        /// <summary>
        /// Copies the content of a data row to another. Runs through the target's fields
        ///     and looks for fields of the same name in the source row. Structure must mathc
        ///     or fields are skipped.
        /// </summary>
        /// <param name="source">
        /// </param>
        /// <param name="target">
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool CopyDataRow(this DataRow source, DataRow target)
        {
            var columns = target.Table.Columns;

            for (var x = 0; x < columns.Count; x++)
            {
                var fieldname = columns[x].ColumnName;

                try
                {
                    target[x] = source[fieldname];
                }
                catch
                {
                    // ignored
                }

                // skip any errors
            }

            return true;
        }

        /// <summary>
        /// The copy object from data row.
        /// </summary>
        /// <param name="row">
        /// The row.
        /// </param>
        /// <param name="targetObject">
        /// The target object.
        /// </param>
        public static void CopyObjectFromDataRow(DataRow row, object targetObject)
        {
            var miT = targetObject.GetType().FindMembers(
                MemberTypes.Field | MemberTypes.Property,
                MemberAccess,
                null,
                null);
            foreach (var field in miT)
            {
                var name = field.Name;
                if (!row.Table.Columns.Contains(name))
                {
                    continue;
                }

                if (field.MemberType == MemberTypes.Field)
                {
                    ((FieldInfo)field).SetValue(targetObject, row[name]);
                }
                else if (field.MemberType == MemberTypes.Property)
                {
                    ((PropertyInfo)field).SetValue(targetObject, row[name], null);
                }
            }
        }

        // public static async Task<PagedList<T>> GetPagedAsync<T>(this IQueryable<T> query, int pageNum, int pageSize)
        // {
        // //IQueryable<T> collection  = query.Skip((pageNum - 1) * pageSize).Take(pageSize);
        // return new PagedList<T>(query, pageNum, pageSize);
        // }

        /// <summary>
        /// The get paged.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <typeparam name="TOrderBy">
        /// The type of the order by.
        /// </typeparam>
        /// <param name="query">
        /// The query.
        /// </param>
        /// <param name="orderBy">
        /// The order by.
        /// </param>
        /// <param name="isDescending">
        /// The is descending.
        /// </param>
        /// <param name="pageNum">
        /// The page num.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <returns>
        /// The <see cref="PagedList"/>.
        /// </returns>
        /// <exception cref="System.Exception">
        /// To do Paging you MUST provide valid OrderBy value
        /// </exception>
        /// <exception cref="Exception">
        /// </exception>
        public static PagedList<T> GetPaged<T, TOrderBy>(
            this IQueryable<T> query,
            Expression<Func<T, TOrderBy>> orderBy,
            bool isDescending,
            int pageNum,
            int pageSize)
        {
            if (orderBy != null)
            {
                query = isDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
            }
            else
            {
                throw new Exception("To do Paging you MUST provide valid OrderBy value");
            }

            return new PagedList<T>(query, pageNum, pageSize);
        }

        /// <summary>
        /// The get paged.
        /// </summary>
        /// <param name="query">
        /// The query.
        /// </param>
        /// <param name="orderBy">
        /// The order by.
        /// </param>
        /// <param name="isDescending">
        /// The is descending.
        /// </param>
        /// <param name="pageNum">
        /// The page num.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <typeparam name="TOrderBy">
        /// </typeparam>
        /// <returns>
        /// The <see cref="PagedList"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        public static PagedList<T> GetPaged<T, TOrderBy>(
            this List<T> query,
            Func<T, TOrderBy> orderBy,
            bool isDescending,
            int pageNum,
            int pageSize, bool isOrderBy = true)
        {
            if (orderBy != null && isOrderBy)
            {
                query = isDescending ? query.OrderByDescending(orderBy).ToList() : query.OrderBy(orderBy).ToList();
            }
            else
            {
                if (isOrderBy)
                    throw new Exception("To do Paging you MUST provide valid OrderBy value");
            }

            return new PagedList<T>(query, pageNum, pageSize);
        }

        /// <summary>
        /// The get paged async.
        /// </summary>
        /// <param name="query">
        /// The query.
        /// </param>
        /// <param name="orderBy">
        /// The order by.
        /// </param>
        /// <param name="isDescending">
        /// The is descending.
        /// </param>
        /// <param name="pageNum">
        /// The page num.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <typeparam name="TOrderBy">
        /// </typeparam>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        public static async Task<PagedList<T>> GetPagedAsync<T, TOrderBy>(
            this IQueryable<T> query,
            Expression<Func<T, TOrderBy>> orderBy,
            bool isDescending,
            int pageNum,
            int pageSize)
        {
            if (orderBy != null)
            {
                query = isDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
            }
            else
            {
                throw new Exception("To do Paging you MUST provide valid OrderBy value");
            }
            int totalCount = await query.CountAsync();
            query = query.Skip((pageNum - 1) * pageSize).Take(pageSize);

            var result = await query.ToListAsync();
            return new PagedList<T>(result, pageNum, pageSize, totalCount);
        }

        public static async Task<PagedList<T>> GetPagedAsync<T>(this IQueryable<T> query, string orderBy, bool isDescending, int pageNum, int pageSize)
        {
            Expression<Func<T, object>> expression = GetExpression<T>(orderBy);
            return await query.GetPagedAsync(expression, isDescending, pageNum, pageSize);
        }

        /// <summary>
        /// The include multiple.
        /// </summary>
        /// <typeparam name="T">
        /// The Type
        /// </typeparam>
        /// <param name="query">
        /// The query.
        /// </param>
        /// <param name="includes">
        /// The includes.
        /// </param>
        /// <returns>
        /// The <see cref="IQueryable"/>.
        /// </returns>
        public static IQueryable<T> IncludeMultiple<T>(
            this IQueryable<T> query,
            params Expression<Func<T, object>>[] includes)
            where T : class
        {
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return query;
        }

        /// <summary>
        /// The is dirty.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsDirty(this DbContext context)
        {
            // Query the change tracker entries for any adds, modifications, or deletes.
            var res = from e in context.ChangeTracker.Entries()
                      where e.State.HasFlag(EntityState.Added) || e.State.HasFlag(EntityState.Modified)
                                                               || e.State.HasFlag(EntityState.Deleted)
                      select e;

            if (res.Any())
            {
                return true;
            }

            return false;
        }
        public static Expression<Func<T, object>> GetExpression<T>(string propertyName)
        {
            Expression<Func<T, object>> result = null;
            if ((object)typeof(T).GetProperty(propertyName) != null)
            {
                ParameterExpression parameterExpression = Expression.Parameter(typeof(T));
                result = Expression.Lambda<Func<T, object>>(Expression.Convert(Expression.Property(parameterExpression, propertyName), typeof(object)), new ParameterExpression[1] { parameterExpression });
            }

            return result;
        }
    }
}