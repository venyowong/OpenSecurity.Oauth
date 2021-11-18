using Microsoft.AspNetCore.Http;

namespace OpenSecurity.Oauth;

public interface ILogin
{
    /// <summary>
    /// login callback
    /// <para>should set http response in this method</para>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="user"></param>
    Task Login(HttpContext context, UserInfo user);
}
