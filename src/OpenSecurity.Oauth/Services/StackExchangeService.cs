using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OpenSecurity.Oauth.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenSecurity.Oauth.Services;

public class StackExchangeService : IOauthService
{
    private IConfiguration config;
    private IHttpClientFactory clientFactory;

    private static readonly Regex _tokenRegex = new Regex("access_token=([^&]+)");

    public StackExchangeService(IHttpClientFactory clientFactory, IConfiguration config)
    {
        this.clientFactory = clientFactory;
        this.config = config;
    }

    public string Name => "stackexchange";

    public string GetAuthorizeUrl(HttpContext context)
    {
        var redirectUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}/opensecurity/authorize{context.Request.QueryString}";
        redirectUrl = WebUtility.UrlEncode(redirectUrl);
        return $"https://stackoverflow.com/oauth?client_id={this.config["Oauth:Stackexchange:client_id"]}&redirect_uri={redirectUrl}";
    }

    public async Task<(UserInfo?, Exception?)> GetUserInfo(HttpContext context, string authCode)
    {
        var client = this.clientFactory.CreateClient();
        var queryString = string.Join("&", context.Request.Query.Where(x => x.Key != "code")
            .Select(x => $"{x.Key}={x.Value}")
            .ToArray());
        var redirectUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}/opensecurity/authorize?{queryString}";
        redirectUrl = WebUtility.UrlEncode(redirectUrl);
        var form = $"client_id={this.config["Oauth:Stackexchange:client_id"]}&client_secret={this.config["Oauth:Stackexchange:client_secret"]}&code={authCode}&redirect_uri={redirectUrl}";
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://stackoverflow.com/oauth/access_token");
        requestMessage.Content = new StringContent(form, Encoding.UTF8, "application/x-www-form-urlencoded");
        var response = await client.SendAsync(requestMessage);
        var tokenResult = await response.ReadAsString();
        var match = _tokenRegex.Match(tokenResult);
        if (!match.Success)
        {
            return (null, null);
        }
        var token = match.Groups[1].Value;
        if (string.IsNullOrEmpty(token))
        {
            return (null, null);
        }

        response = await client.GetAsync($"https://api.stackexchange.com/2.3/me?key={this.config["Oauth:Stackexchange:key"]}&access_token={token}&site=stackoverflow");
        var json = await response.ReadAsDecompressedString();
        var (result, e2) = json.ToObj<JObject>();
        if (e2 != null)
        {
            return (null, e2);
        }
        if (result == null)
        {
            return (null, null);
        }
        var items = result["items"] as JArray;
        if (items!.IsNullOrEmpty())
        {
            return (null, null);
        }
        var user = items![0];

        return (new UserInfo
        {
            Id = user["user_id"].ToString(),
            Name = user["display_name"].ToString(),
            AvatarUrl = user["profile_image"].ToString(),
            Source = "stackexchange",
            Url = user["link"].ToString()
        }, null);
    }
}
