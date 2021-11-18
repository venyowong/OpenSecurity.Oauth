using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenSecurity.Oauth.Services;

namespace OpenSecurity.Oauth;

public static class ServiceExtension
{
    public static IServiceCollection AddOauth(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddTransient<IOauthService, GithubService>()
            .AddTransient<IOauthService, GiteeService>();
        return services;
    }

    public static IApplicationBuilder UseOauth(this IApplicationBuilder app)
    {
        app.UseMiddleware<OauthMiddleware>();
        return app;
    }
}
