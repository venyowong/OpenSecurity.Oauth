using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OpenSecurity.Oauth.Extensions;
using OpenSecurity.Oauth.Services;

namespace OpenSecurity.Oauth;

public class OauthMiddleware
{
    private RequestDelegate requestDelegate;
    private IEnumerable<IOauthService> services;
    private ILogger logger;
    private ILogin login;

    private const string _base_path = "/opensecurity";

    public OauthMiddleware(RequestDelegate requestDelegate, IEnumerable<IOauthService> services, 
        ILogger<OauthMiddleware> logger, ILogin login)
    {
        this.requestDelegate = requestDelegate;
        this.services = services;
        this.logger = logger;
        this.login = login;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.HasValue)
        {
            var path = context.Request.Path.Value.ToLower();
            var index = path.IndexOf(_base_path);
            if (index >= 0)
            {
                path = path.Substring(index + _base_path.Length);
                if (string.IsNullOrEmpty(path))
                {
                    await requestDelegate.Invoke(context);
                    return;
                }

                IOauthService? service = null;
                var routes = path.Trim('/').Split('/');
                switch (routes[0])
                {
                    case "oauth":
                        service = this.GetOauthService(context.Request.Query["service"]);
                        if (service != null)
                        {
                            context.Response.Redirect(service.GetAuthorizeUrl(context));
                            return;
                        }
                        break;
                    case "authorize":
                        service = this.GetOauthService(context.Request.Query["service"]);
                        if (service != null)
                        {
                            var authCode = context.Request.Query["code"];
                            var (user, e) = await service.GetUserInfo(context, authCode);
                            if (e != null)
                            {
                                this.logger.LogWarning(e, $"catch an exception when request {context.Request.Path}{context.Request.QueryString}");
                                context.Response.StatusCode = 500;
                                return;
                            }
                            if (user == null)
                            {
                                this.logger.LogWarning($"cannot get user info when request {context.Request.Path}{context.Request.QueryString}");
                                context.Response.StatusCode = 500;
                                return;
                            }

                            try
                            {
                                await this.login.Login(context, user);
                                return;
                            }
                            catch (Exception ex)
                            {
                                this.logger.LogWarning(ex, $"catch an exception when call login method, user: {user.ToJson()}");
                                context.Response.StatusCode = 500;
                                return;
                            }
                        }
                        break;
                }
            }
        }

        await requestDelegate.Invoke(context);
    }

    private IOauthService GetOauthService(string name) => this.services.FirstOrDefault(s => s.Name == name);
}
