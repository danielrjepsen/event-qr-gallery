// Infrastructure/Hangfire/HangfireAuthorizationFilter.cs
using Hangfire.Dashboard;

namespace Nory.Infrastructure.Hangfire;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow access in development
        var env = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        if (env.IsDevelopment())
        {
            return true;
        }

        // In production, require authentication
        return httpContext.User.Identity?.IsAuthenticated ?? false;
    }
}
