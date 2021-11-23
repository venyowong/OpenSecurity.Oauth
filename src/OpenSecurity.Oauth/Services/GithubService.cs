using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenSecurity.Oauth.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace OpenSecurity.Oauth.Services;

public class GithubService : IOauthService
{
    private IConfiguration config;
    private IHttpClientFactory clientFactory;

    public GithubService(IHttpClientFactory clientFactory, IConfiguration config)
    {
        this.clientFactory = clientFactory;
        this.config = config;
    }

    public string Name => "github";

    public string GetAuthorizeUrl(HttpContext context)
    {
        var redirectUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}/opensecurity/authorize{context.Request.QueryString}";
        redirectUrl = WebUtility.UrlEncode(redirectUrl);
        return $"https://github.com/login/oauth/authorize?client_id={this.config["Oauth:Github:client_id"]}&redirect_uri={redirectUrl}";
    }

    public async Task<(UserInfo?, Exception?)> GetUserInfo(HttpContext context, string authCode)
    {
        var client = this.clientFactory.CreateClient();
        var tokenRequest = new
        {
            client_id = this.config["Oauth:Github:client_id"],
            client_secret = this.config["Oauth:Github:client_secret"],
            code = authCode
        };
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token");
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

        requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        requestMessage.Headers.Add("User-Agent", ".net core");
        requestMessage.Headers.Add("Accept", "application/json");
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
            Source = "github",
            Mail = result["email"]?.ToString() ?? string.Empty,
            Url = result["url"].ToString(),
            Location = result["location"]?.ToString() ?? string.Empty,
            Company = result["company"]?.ToString() ?? string.Empty,
            Blog = result["blog"]?.ToString() ?? string.Empty,
            Bio = result["bio"]?.ToString() ?? string.Empty
        }, null);
    }
}
