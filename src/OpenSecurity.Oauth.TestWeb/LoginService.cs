namespace OpenSecurity.Oauth.TestWeb;

public class LoginService : ILogin
{
    public async Task Login(HttpContext context, UserInfo user)
    {
        await context.Response.WriteAsJsonAsync(user);
    }
}
