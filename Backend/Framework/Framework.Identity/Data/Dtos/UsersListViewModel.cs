using Framework.Identity.Data.Entities;
using PagedList.Core;
using System;

namespace Framework.Identity.Data.Dtos
{
    /// <summary>
    ///     The users list view model.
    /// </summary>
    public class UsersListViewModel
    {
        /// <summary>
        ///     Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     Gets or sets the full name.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is active.
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        ///     Gets or sets the items.
        /// </summary>
        public StaticPagedList<ApplicationUser> Items { get; set; }

        /// <summary>
        ///     Gets or sets the page number.
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        ///     Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        ///     Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Gets or sets the user role id.
        /// </summary>
        public int? UserRoleId { get; set; }

        public String NationalId { get; set; }

        public String Phone { get; set; }
    }
}