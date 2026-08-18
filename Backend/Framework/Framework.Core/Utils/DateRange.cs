// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateRange.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Utils
{
    #region usings

    using System;

    #endregion usings

    /// <summary>
    /// The date range.
    /// </summary>
    public class DateRange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateRange"/> class.
        /// </summary>
        public DateRange()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateRange"/> class.
        /// </summary>
        /// <param name="startDate">
        /// The start date.
        /// </param>
        /// <param name="endDate">
        /// The end date.
        /// </param>
        public DateRange(DateTime startDate, DateTime endDate)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
        }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The total days.
        /// </summary>
        public double TotalDays => (this.EndDate - this.StartDate).TotalDays;

        /// <summary>
        /// The includes.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Includes(DateTime value)
        {
            return (this.StartDate <= value) && (value <= this.EndDate);
        }
    }
}