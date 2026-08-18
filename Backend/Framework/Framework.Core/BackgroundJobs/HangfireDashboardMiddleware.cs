// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireDashboardMiddleware.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.BackgroundJobs
{
    #region usings

    using Hangfire;
    using Hangfire.Dashboard;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    #endregion usings

    /// <summary>
    /// The hangfire dashboard middleware.
    /// </summary>
    public class HangfireDashboardMiddleware
    {
        /// <summary>
        /// The dashboard options.
        /// </summary>
        private readonly DashboardOptions dashboardOptions;

        /// <summary>
        /// The job storage.
        /// </summary>
        private readonly JobStorage jobStorage;

        /// <summary>
        /// The next request delegate.
        /// </summary>
        private readonly RequestDelegate nextRequestDelegate;

        /// <summary>
        /// The route collection.
        /// </summary>
        private readonly RouteCollection routeCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireDashboardMiddleware"/> class.
        /// </summary>
        /// <param name="nextRequestDelegate">
        /// The next request delegate.
        /// </param>
        /// <param name="storage">
        /// The storage.
        /// </param>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <param name="routes">
        /// The routes.
        /// </param>
        public HangfireDashboardMiddleware(
            RequestDelegate nextRequestDelegate,
            JobStorage storage,
            DashboardOptions options,
            RouteCollection routes)
        {
            this.nextRequestDelegate = nextRequestDelegate;
            this.jobStorage = storage;
            this.dashboardOptions = options;
            this.routeCollection = routes;
        }

        /// <summary>
        /// The invoke.
        /// </summary>
        /// <param name="httpContext">
        /// The http context.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task Invoke(HttpContext httpContext)
        {
            var aspNetCoreDashboardContext = new AspNetCoreDashboardContext(
                this.jobStorage,
                this.dashboardOptions,
                httpContext);

            var findResult = this.routeCollection.FindDispatcher(httpContext.Request.Path.Value);
            if (findResult == null)
            {
                await this.nextRequestDelegate.Invoke(httpContext);
                return;
            }

            // attempt to authenticate against default auth scheme (this will attempt to authenticate using data in request, but doesn't send challenge)
            var result = await httpContext.AuthenticateAsync();

            if (!httpContext.User.Identity.IsAuthenticated)
            {
                // request was not authenticated, send challenge and do not continue processing this request
                await httpContext.ChallengeAsync();
            }

            if (this.dashboardOptions.Authorization.Any(filter =>
                filter.Authorize(aspNetCoreDashboardContext) == false))
            {
                var isAuthenticated = httpContext.User?.Identity?.IsAuthenticated;
                httpContext.Response.StatusCode = isAuthenticated == true
                                                      ? (int)HttpStatusCode.Forbidden
                                                      : (int)HttpStatusCode.Unauthorized;
                return;
            }

            aspNetCoreDashboardContext.UriMatch = findResult.Item2;
            await findResult.Item1.Dispatch(aspNetCoreDashboardContext);
        }
    }
}