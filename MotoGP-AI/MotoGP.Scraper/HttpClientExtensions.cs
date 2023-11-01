using System.Net.Http.Json;

namespace MotoGP.Scraper
{
    public static class HttpClientExtensions
    {
        public static Task<T> GetFromJson<T>(this HttpClient client, string relativeUrl)
        {
            return client.GetFromJsonAsync<T>(new Uri(relativeUrl, UriKind.Relative));
        }
    }
}