using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MotoGP.Extensions;

namespace MotoGP.Scraper
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.AddMotoGp();

            builder.Services.AddSingleton<IDataScraper, DataScraper>();

            IHost host = builder.Build();

            var exceptionHandler = host.Services.GetRequiredService<IHostExceptionHandler>();
            var scraper = host.Services.GetRequiredService<IDataScraper>();

            await exceptionHandler.RunAsync(scraper.Scrape());
        }
    }
}