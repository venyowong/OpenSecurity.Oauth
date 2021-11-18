using Newtonsoft.Json;

namespace OpenSecurity.Oauth.Extensions;

internal static class StringExtension
{
    public static (string, Exception?) ToJson(this object obj)
    {
        if (obj == null)
        {
            return (string.Empty, new ArgumentNullException(nameof(obj)));
        }

        try
        {
            return (JsonConvert.SerializeObject(obj), null);
        }
        catch (Exception e)
        {
            return (string.Empty, e);
        }
    }

    public static (T?, Exception?) ToObj<T>(this string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return (default, null);
        }

        try
        {
            return (JsonConvert.DeserializeObject<T>(json), null);
        }
        catch (Exception e)
        {
            return (default, e);
        }
    }
}
