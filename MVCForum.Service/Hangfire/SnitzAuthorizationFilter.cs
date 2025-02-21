using Hangfire.Dashboard;

namespace SnitzCore.Service.Hangfire
{
    public class SnitzAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Allow all authenticated users to see the Dashboard (potentially dangerous).
            if (httpContext.User.Identity == null || !httpContext.User.Identity.IsAuthenticated) return false;

            return httpContext.User.IsInRole("Administrator");
        }
    }
}
