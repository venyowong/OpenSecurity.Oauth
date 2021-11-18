using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OpenSecurity.Oauth.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace OpenSecurity.Oauth.Services;

public class GiteeService : IOauthService
{
    private IConfiguration config;
    private IHttpClientFactory clientFactory;

    public GiteeService(IHttpClientFactory clientFactory, IConfiguration config)
    {
        this.clientFactory = clientFactory;
        this.config = config;
    }

    public string Name => "gitee";

    public string GetAuthorizeUrl(HttpContext context)
    {
        var redirectUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}/opensecurity/authorize?service=gitee";
        redirectUrl = WebUtility.UrlEncode(redirectUrl);
        return $"https://gitee.com/oauth/authorize?client_id={this.config["Oauth:Gitee:client_id"]}&redirect_uri={redirectUrl}&response_type=code&scope=user_info";
    }

    public async Task<(UserInfo?, Exception?)> GetUserInfo(HttpContext context, string authCode)
    {
        var client = this.clientFactory.CreateClient();
        var redirectUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}/opensecurity/authorize?service=gitee";
        var tokenRequest = new
        {
            client_id = this.config["Oauth:Gitee:client_id"],
            client_secret = this.config["Oauth:Gitee:client_secret"],
            code = authCode,
            redirect_uri = redirectUrl
        };
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://gitee.com/oauth/token?grant_type=authorization_code");
        requestMessage.Content = new StringContent(tokenRequest.ToJson().Item1, Encoding.UTF8, "application/json");
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var response = await client.SendAsync(requestMessage);
        var (tokenResult, e) = await response.ReadAsObj<JObject>();
        
        if (e != null)
        {
            return (null, e);
        }
        if (tokenResult == null)
        {
            return (null, null);
        }
        var token = tokenResult["access_token"]?.ToString();
        if (string.IsNullOrEmpty(token))
        {
            return (null, null);
        }

        requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://gitee.com/api/v5/user?access_token={token}");
        response = await client.SendAsync(requestMessage);
        if (!response.IsSuccessStatusCode || response.Content == null)
        {
            return (null, null);
        }

        var json = await response.Content.ReadAsStringAsync();
        var (result, e2) = json.ToObj<JObject>();
        if (e2 != null)
        {
            return (null, e2);
        }
        if (result == null)
        {
            return (null, null);
        }

        return (new UserInfo
        {
            Id = result["id"].ToString(),
            Name = result["name"].ToString(),
            AvatarUrl = result["avatar_url"].ToString(),
            Source = "gitee",
            Mail = result["email"]?.ToString() ?? string.Empty,
            Url = result["url"].ToString(),
            Blog = result["blog"]?.ToString() ?? string.Empty,
            Bio = result["bio"]?.ToString() ?? string.Empty
        }, null);
    }
}
