using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MotoGP.Scraper
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddSingleton<IScraper, MotoGpScraper>();
            builder.Services.AddSingleton<IDataWriter, JsonDataWriter>();
            builder.Services.AddHttpClient(builder.Configuration["MotoGP:Name"],
                client =>
                {
                    client.BaseAddress = new Uri(builder.Configuration["MotoGP:BaseAddress"], UriKind.Absolute);
                });
            IHost host = builder.Build();

            var scraper = host.Services.GetRequiredService<IScraper>();
            var writer = host.Services.GetRequiredService<IDataWriter>();

            IEnumerable<Season> data = await scraper.Scrape();
            await writer.SaveData(data);

            Console.WriteLine("Finished w/ no errors...");
            Console.WriteLine("Press <enter> to close");
            Console.ReadLine();
        }
    }
}