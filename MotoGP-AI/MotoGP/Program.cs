using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MotoGP.Extensions;
using MotoGP.Scraper;

namespace MotoGP
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.AddMotoGp();

            builder.Services.AddSingleton<IDataScraper, DataScraper>();

            IHost host = builder.Build();

            var scraper = host.Services.GetRequiredService<IDataScraper>();

            await scraper.Scrape();
        }
    }
}