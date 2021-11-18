using System;
using System.Collections.Generic;
using System.Text;

namespace OpenSecurity.Oauth;

public class UserInfo
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string AvatarUrl { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string Mail { get; set; } = string.Empty;

    public string Url { get;set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Company { get; set; } = string.Empty;

    public string Blog { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;
}
