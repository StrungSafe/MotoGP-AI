using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MotoGP.Interfaces;

namespace MotoGP.Analyzer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddSingleton<IDataAnalyzer, DataAnalyzer>();
            builder.Services.AddSingleton<IDataReader, DataReader>();

            IHost host = builder.Build();

            var reader = host.Services.GetRequiredService<IDataReader>();
            var analyzer = host.Services.GetRequiredService<IDataAnalyzer>();

            Season[] data = await reader.ReadData();
            await analyzer.AnalyzeData(data);

            Console.WriteLine("Finished w/ no errors...");
            Console.WriteLine("Press <enter> to close");
            Console.ReadLine();
        }
    }

    public interface IDataAnalyzer
    {
        Task AnalyzeData(Season[] seasons);
    }

    public class DataAnalyzer : IDataAnalyzer
    {
        private readonly ILogger<DataAnalyzer> logger;

        public DataAnalyzer(ILogger<DataAnalyzer> logger)
        {
            this.logger = logger;
        }

        public async Task AnalyzeData(Season[] seasons)
        {
        }
    }
}