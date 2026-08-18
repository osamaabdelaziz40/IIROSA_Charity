// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PagedList.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using PagedList.Core;

namespace Framework.Core
{
    using Microsoft.EntityFrameworkCore;
    #region usings

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    #endregion usings

    public class PagingViewModel<T>
    {
        public PagingViewModel()
        {
            this.Items = new List<T>();
            this.TotalItemsCount = 0;
        }

        public PagingViewModel(StaticPagedList<T> pagedList)
        {
            this.Items = pagedList.ToList();
            this.TotalItemsCount = pagedList.TotalItemCount;
        }

        /// <summary>
        ///     Gets or sets the items.
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        /// Gets or sets the page number.
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int? PageSize { get; set; }

        public int TotalItemsCount { get; set; }
    }

    /// <summary>
    /// https://github.com/martijnboland/MvcPaging/blob/master/src/MvcPaging/PagedList.cs
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class PagedList<T> : List<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="totalCount">
        /// The total count.
        /// </param>
        public PagedList(IEnumerable<T> source, int index, int pageSize, int? totalCount = null)
            : this(source.AsQueryable(), index, pageSize, totalCount)
        {
        }
        private PagedList()
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="pageNum">
        /// The page num.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="totalCount">
        /// The total count.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public PagedList(IQueryable<T> source, int pageNum, int pageSize, int? totalCount = null)
        {
            if (pageNum < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNum), "Value can not be below 0.");
            }

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Value can not be less than 1.");
            }

            if (source == null)
            {
                source = new List<T>().AsQueryable();
            }

            var realTotalCount = source.Count();

            this.PageSize = pageSize;
            this.PageIndex = pageNum - 1;
            this.TotalItemCount = totalCount ?? realTotalCount;
            this.PageCount = this.TotalItemCount > 0
                                 ? (int)Math.Ceiling(this.TotalItemCount / (double)this.PageSize)
                                 : 0;

            this.HasPreviousPage = this.PageIndex > 0;
            this.HasNextPage = this.PageIndex < this.PageCount - 1;
            this.IsFirstPage = this.PageIndex <= 0;
            this.IsLastPage = this.PageIndex >= this.PageCount - 1;

            this.ItemStart = this.PageIndex * this.PageSize + 1;
            this.ItemEnd = Math.Min(this.PageIndex * this.PageSize + this.PageSize, this.TotalItemCount);

            if (this.TotalItemCount <= 0)
            {
                return;
            }

            var realTotalPages = (int)Math.Ceiling(realTotalCount / (double)this.PageSize);

            if (realTotalCount < this.TotalItemCount && realTotalPages <= this.PageIndex)
            {
                this.AddRange(source.Skip((realTotalPages - 1) * this.PageSize).Take(this.PageSize));
            }
            else
            {
                this.AddRange(source.Skip(this.PageIndex * this.PageSize).Take(this.PageSize));
            }
        }
        public static async Task<PagedList<T>> GetPagedAsyc(IQueryable<T> source, int pageNum, int pageSize, int? totalCount = null)
        {
            PagedList<T> result = new PagedList<T>();
            if (pageNum < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNum), "Value can not be below 1.");
            }

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Value can not be less than 1.");
            }

            if (source == null)
            {
                source = new List<T>().AsQueryable();
            }

            var realTotalCount = await source.CountAsync();

            result.PageSize = pageSize;
            result.PageIndex = pageNum - 1;
            result.TotalItemCount = totalCount ?? realTotalCount;
            result.PageCount = result.TotalItemCount > 0
                                 ? (int)Math.Ceiling(result.TotalItemCount / (double)result.PageSize)
                                 : 0;

            result.HasPreviousPage = result.PageIndex > 0;
            result.HasNextPage = result.PageIndex < result.PageCount - 1;
            result.IsFirstPage = result.PageIndex <= 0;
            result.IsLastPage = result.PageIndex >= result.PageCount - 1;

            result.ItemStart = result.PageIndex * result.PageSize + 1;
            result.ItemEnd = Math.Min(result.PageIndex * result.PageSize + result.PageSize, result.TotalItemCount);

            if (result.TotalItemCount <= 0)
            {
                return null;
            }

            var realTotalPages = (int)Math.Ceiling(realTotalCount / (double)result.PageSize);

            if (realTotalCount < result.TotalItemCount && realTotalPages <= result.PageIndex)
            {
                result.AddRange(await source.Skip((realTotalPages - 1) * result.PageSize).Take(result.PageSize).ToListAsync());
            }
            else
            {
                result.AddRange(await source.Skip(result.PageIndex * result.PageSize).Take(result.PageSize).ToListAsync());
            }
            return result;
        }
        /// <summary>
        ///     Gets a value indicating whether has next page.
        /// </summary>
        public bool HasNextPage { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether has previous page.
        /// </summary>
        public bool HasPreviousPage { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether is first page.
        /// </summary>
        public bool IsFirstPage { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether is last page.
        /// </summary>
        public bool IsLastPage { get; private set; }

        /// <summary>
        ///     Gets the item end.
        /// </summary>
        public int ItemEnd { get; private set; }

        /// <summary>
        ///     Gets the item start.
        /// </summary>
        public int ItemStart { get; private set; }

        /// <summary>
        ///     Gets the page count.
        /// </summary>
        public int PageCount { get; private set; }

        /// <summary>
        ///     Gets the page index.
        /// </summary>
        public int PageIndex { get; private set; }

        /// <summary>
        ///     The page number.
        /// </summary>
        public int PageNumber => this.PageIndex + 1;

        /// <summary>
        ///     Gets the page size.
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        ///     Gets the total item count.
        /// </summary>
        public int TotalItemCount { get; private set; }

        /// <summary>
        ///     The get paging meta data.
        /// </summary>
        /// <returns>
        ///     The <see cref="PagingMetaData" />.
        /// </returns>
        public PagingMetaData GetPagingMetaData()
        {
            return new PagingMetaData
            {
                HasNextPage = this.HasNextPage,
                HasPreviousPage = this.HasPreviousPage,
                IsFirstPage = this.IsFirstPage,
                IsLastPage = this.IsLastPage,
                ItemEnd = this.ItemEnd,
                ItemStart = this.ItemStart,
                PageCount = this.PageCount,
                PageIndex = this.PageIndex,
                PageNumber = this.PageNumber,
                PageSize = this.PageSize,
                TotalItemCount = this.TotalItemCount
            };
        }
    }

    /// <summary>
    ///     The paging meta data.
    /// </summary>
    public class PagingMetaData
    {
        /// <summary>
        ///     Gets or sets a value indicating whether has next page.
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether has previous page.
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is first page.
        /// </summary>
        public bool IsFirstPage { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is last page.
        /// </summary>
        public bool IsLastPage { get; set; }

        /// <summary>
        ///     Gets or sets the item end.
        /// </summary>
        public int ItemEnd { get; set; }

        /// <summary>
        ///     Gets or sets the item start.
        /// </summary>
        public int ItemStart { get; set; }

        /// <summary>
        ///     Gets or sets the page count.
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        ///     Gets or sets the page index.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        ///     Gets or sets the page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        ///     Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        ///     Gets or sets the total item count.
        /// </summary>
        public int TotalItemCount { get; set; }
    }
}