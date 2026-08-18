// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireActivator.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.BackgroundJobs
{
    #region usings

    using Hangfire;
    using System;

    #endregion usings

    /// <summary>
    ///     The hangfire activator.
    ///     Enables hangfire to access the .net core services registered at startup using the .net core IoC
    /// </summary>
    public class HangfireActivator : JobActivator
    {
        /// <summary>
        ///     The _service provider.
        /// </summary>
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireActivator"/> class.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider.
        /// </param>
        public HangfireActivator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// The activate job.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public override object ActivateJob(Type type)
        {
            return this.serviceProvider.GetService(type);
        }
    }
}