// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FireBaseResponse.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Notifications
{
    #region usings

    using System.Collections.Generic;

    #endregion usings

    /// <summary>
    ///     The fcm response.
    /// </summary>
    public class FirebaseResponse
    {
        /// <summary>
        ///     Gets or sets the canonical_ids.
        /// </summary>
        public int canonical_ids { get; set; }

        /// <summary>
        ///     Gets or sets the failure.
        /// </summary>
        public int failure { get; set; }

        /// <summary>
        ///     Gets or sets the multicast_id.
        /// </summary>
        public long multicast_id { get; set; }

        /// <summary>
        ///     Gets or sets the results.
        /// </summary>
        public List<MessageResult> results { get; set; }

        /// <summary>
        ///     Gets or sets the success.
        /// </summary>
        public int success { get; set; }
    }

    /// <summary>
    ///     The fcm result.
    /// </summary>
    public class MessageResult
    {
        /// <summary>
        ///     Gets or sets the message_id.
        /// </summary>
        public string message_id { get; set; }
    }
}