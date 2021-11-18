namespace OpenSecurity.Oauth.Extensions;
 
internal static class HttpResponseMessageExtension
{
    public static async Task<(T?, Exception?)> ReadAsObj<T>(this HttpResponseMessage response)
    {
        if (!response?.IsSuccessStatusCode ?? false)
        {
            return (default, null);
        }
        if (response?.Content == null)
        {
            return (default, null);
        }

        try
        {
            var json = await response.Content.ReadAsStringAsync();
            var (obj, e) = json.ToObj<T>();
            if (e != null)
            {
                return (default, e);
            }

            return (obj, null);
        }
        catch (Exception e)
        {
            return (default, e);
        }
    }
}
