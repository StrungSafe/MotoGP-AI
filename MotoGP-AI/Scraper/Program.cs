using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Scraper
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddTransient<IScraper, MotoGPScraper>();
            builder.Services.AddHttpClient("MotoGP", client =>
            {
                client.BaseAddress = new Uri("https://api.motogp.pulselive.com/motogp/v1/results/", UriKind.Absolute);
            });
            var host = builder.Build();

            var scraper = host.Services.GetRequiredService<IScraper>();
            await scraper.Scrape();

            Console.WriteLine("Finished");
            Console.ReadKey();
        }
    }
}