# OpenSecurity.Oauth
OAuth implemention in asp.net core, and you can customize the behavior after authorization

适用于 asp.net core 的 OAuth 实现，框架支持在授权后自定义操作，比如用户入库、生成JWT便于后续鉴权等

## Nuget

[OpenSecurity.Oauth](https://www.nuget.org/packages/OpenSecurity.Oauth)

## Supported Services

### Github

```
"Oauth": {
    "Github": {
        "client_id": "df616e7178cb8e9dc69b",
        "client_secret": "0c4d02ace8ccf8cf4d7a4e7c30ff15cdb474be10"
    }
}
```
The Github callback URL should like `https://localhost:7167/opensecurity`, replace `https://localhost:7167` using your application root url

### Gitee

```
"Oauth": {
    "Gitee": {
        "client_id": "7d6b21f22edb6dcb6493a5acafe197037153e5f54fa2d8e33b5f6128a540eae2",
        "client_secret": "9a4c83aa2f951d9901710e1f494069af2644f24df199e218453d007f9d8b972b"
    }
}
```
应用回调地址格式为：`https://localhost:7167/opensecurity/authorize?service=gitee`，使用项目根路径替换掉 `https://localhost:7167`

## Using in asp.net core

1. Create a login service implement `ILogin` interface
    ```
    namespace OpenSecurity.Oauth.TestWeb;

    public class LoginService : ILogin
    {
        public async Task Login(HttpContext context, UserInfo user)
        {
            if (user.Source == "github")
            {
                // do something
            }
            if (user.Source == "gitee")
            {
                // do something
            }
            await context.Response.WriteAsJsonAsync(user);
        }
    }
    ```
2. Add login service into DI `services.AddTransient<ILogin, LoginService>()`
3. Add Oauth into application
    ```
    using OpenSecurity.Oauth;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOauth();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseOauth();
    }
    ```
4. link to https://localhost:7167/opensecurity/oauth?service=github, and start your authorization process

    Note: if you are using github oauth, you can add any parameters after `https://localhost:7167/opensecurity/oauth?service=github`, like `https://localhost:7167/opensecurity/oauth?service=github&key1=value1&key2=value2`, and you can get these parameters in Login callback method.
    
    但是如果用的是 Gitee，在 url 添加的参数将无法在 Login 方法中获取到，因为 Gitee 的回调链接必须是固定配置的，不接受动态参数。因此，若在使用 Gitee 授权结束后，希望能有不同的处理流程，需要在后续流程进行处理，比如需要区分平台，打开不同的页面，则可以在 Login 方法中返回一个中间页面，处理跳转逻辑。