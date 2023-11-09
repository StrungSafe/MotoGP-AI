using Microsoft.Extensions.Logging;
using MotoGP.Analyzer;
using MotoGP.Predictor;
using MotoGP.Scraper;
using MotoGP.Trainer;

namespace MotoGP
{
    public class MotoGpHost
    {
        private readonly IDataAnalyzer analyzer;

        private readonly ILogger<MotoGpHost> logger;

        private readonly IDataPredictor predictor;

        private readonly IDataScraper scraper;

        private readonly IDataTrainer trainer;

        public MotoGpHost(ILogger<MotoGpHost> logger, IDataScraper scraper, IDataAnalyzer analyzer,
            IDataTrainer trainer, IDataPredictor predictor)
        {
            this.logger = logger;
            this.scraper = scraper;
            this.analyzer = analyzer;
            this.trainer = trainer;
            this.predictor = predictor;
        }

        public async Task Run()
        {
            var input = string.Empty;
            int service;
            do
            {
                try
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
                }
                catch (Exception ex)
                {
                    service = -1;
                    logger.LogError(ex, "There was an unhandled exception while running the service {serviceEncoded}",
                        service);
                    Console.WriteLine("***************");
                    Console.WriteLine();
                }
            } while (!string.Equals("exit", input) && service != 0);
        }
    }
}