using System.Net.Http.Json;

namespace MotoGP.Scraper
{
    public static class HttpClientExtensions
    {
        public static async Task<T> GetFromJson<T>(this HttpClient client, string relativeUrl)
        {
            try
            {
                return await client.GetFromJsonAsync<T>(new Uri(relativeUrl, UriKind.Relative));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Silentily eating an exception...{ex.Message}");
                return default;
            }
        }
    }
}