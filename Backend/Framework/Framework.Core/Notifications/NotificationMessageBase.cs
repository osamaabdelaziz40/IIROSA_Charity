// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationMessageBase.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Notifications
{
    #region usings

    using System.Collections.Generic;

    #endregion usings

    /// <summary>
    ///     The notification message base.
    /// </summary>
    public class NotificationMessageBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NotificationMessageBase" /> class.
        /// </summary>
        public NotificationMessageBase()
        {
            this.TemplateData = new Dictionary<string, string>();
        }

        /// <summary>
        ///     Gets or sets the application id.
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        ///     Gets or sets the created by.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        ///     Gets or sets the data.
        /// </summary>
        public Dictionary<string, string> TemplateData { get; set; }

        /// <summary>
        ///     Gets or sets the template name.
        /// </summary>
        public string TemplateName { get; set; }
    }
}