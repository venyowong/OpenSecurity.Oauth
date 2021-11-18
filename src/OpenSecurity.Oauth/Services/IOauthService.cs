using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenSecurity.Oauth.Services;

public interface IOauthService
{
    string Name { get; }

    string GetAuthorizeUrl(HttpContext context);

    Task<(UserInfo?, Exception?)> GetUserInfo(HttpContext context, string authCode);
}
