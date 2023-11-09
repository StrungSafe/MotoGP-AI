using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MotoGP.Analyzer;
using MotoGP.Extensions;
using MotoGP.Predictor;
using MotoGP.Scraper;
using MotoGP.Trainer;

namespace MotoGP
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            IHost host = builder.AddAll().Build();

            var scraper = host.Services.GetRequiredService<IDataScraper>();
            var analyzer = host.Services.GetRequiredService<IDataAnalyzer>();
            var trainer = host.Services.GetRequiredService<IDataTrainer>();
            var predictor = host.Services.GetRequiredService<IDataPredictor>();

            // TODO: show menu
            // TODO: accept input and start service

            var input = string.Empty;
            int service = -1;
            do
            {
                Console.WriteLine("Enter a # and enter to start a service");
                Console.WriteLine("0. Exit");
                Console.WriteLine("1. Scraper");
                Console.WriteLine("2. Analyzer");
                Console.WriteLine("3. Trainer");
                Console.WriteLine("4. Predictor");
                input = Console.ReadLine();
                if (int.TryParse(input, out service))
                {
                    switch (service)
                    {
                        case 1:
                            await scraper.Scrape();
                            break;
                        case 2:
                            await analyzer.AnalyzeData();
                            break;
                        case 3:
                            await trainer.TrainAndSaveModel();
                            break;
                        case 4:
                            await predictor.Predict();
                            break;
                    }
                }
            } while (!string.Equals("exit", input) && service > 1);
        }
    }
}