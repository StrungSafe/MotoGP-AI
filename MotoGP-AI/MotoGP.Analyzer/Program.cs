using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MotoGP.Extensions;

namespace MotoGP.Analyzer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.AddHelpers();

            builder.Services.AddSingleton<IDataAnalyzer, DataAnalyzer>();

            IHost host = builder.Build();

            var analyzer = host.Services.GetRequiredService<IDataAnalyzer>();

            await analyzer.AnalyzeData();
        }
    }
}