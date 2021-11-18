using OpenSecurity.Oauth;
using OpenSecurity.Oauth.TestWeb;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost
    .ConfigureServices(services =>
    {
        services.AddOauth()
            .AddTransient<ILogin, LoginService>();
    });
var app = builder.Build();

app.UseOauth();
app.MapGet("/", () => "Hello World!");

app.Run();
