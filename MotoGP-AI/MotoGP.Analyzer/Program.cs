using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MotoGP.Interfaces;

namespace MotoGP.Analyzer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.Services
                   .AddSingleton<IDataAnalyzer, DataAnalyzer>()
                   .AddSingleton<IDataReader, JsonDataService>();

            IHost host = builder.Build();

            var reader = host.Services.GetRequiredService<IDataReader>();
            var analyzer = host.Services.GetRequiredService<IDataAnalyzer>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();

            Season[] data = await reader.Read<Season[]>(configuration["FilePath"]);
            await analyzer.AnalyzeData(data);

            Console.WriteLine("Finished w/ no errors...");
            Console.WriteLine("Press <enter> to close");
            Console.ReadLine();
        }
    }
}