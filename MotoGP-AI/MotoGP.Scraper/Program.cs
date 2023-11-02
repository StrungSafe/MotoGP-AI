using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MotoGP.Scraper
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.Services
                   .AddSingleton<IDataScraper, DataScraper>()
                   .AddSingleton<IDataRepository, DataRepository>()
                   .AddSingleton<IDataLoader, DataLoader>()
                   .AddSingleton<IDataWriter, JsonDataService>()
                   .AddSingleton<IDataReader, JsonDataService>();

            builder.Services
                   .AddHttpClient(builder.Configuration["MotoGP:Name"],
                       client =>
                       {
                           client.BaseAddress = new Uri(builder.Configuration["MotoGP:BaseAddress"], UriKind.Absolute);
                       });

            IHost host = builder.Build();

            var scraper = host.Services.GetRequiredService<IDataScraper>();

            await scraper.Scrape();

            Console.WriteLine("Finished w/ no errors...");
            Console.WriteLine("Press <enter> to close");
            Console.ReadLine();
        }
    }
}