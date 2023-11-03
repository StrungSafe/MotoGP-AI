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

            var scraper = host.Services.GetRequiredService<IDataScraper>();

            await scraper.Scrape();

            Console.WriteLine("Finished w/ no errors...");
            Console.WriteLine("Press <enter> to close");
            Console.ReadLine();
        }
    }
}