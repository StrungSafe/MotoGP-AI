using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using MotoGP.Extensions;
using MotoGP.Interfaces;

namespace MotoGP.Trainer;

public class Trainer : ITrainer
{
    private readonly IConfiguration configuration;

    private readonly ILogger<Trainer> logger;

    private readonly IDataReader reader;

    public Trainer(ILogger<Trainer> logger, IConfiguration configuration, IDataReader reader)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.reader = reader;
    }

    public async Task TrainAndSaveModel()
    {
        Season[] seasons = await reader.Read<Season[]>(configuration["DataPath"]);
        var testFraction = configuration.GetValue<double>("TestFraction");

        logger.LogInformation("Attempting to train a model on '{seasonCount}' season(s)", seasons.Length);

        var trackNames = new List<string>();
        var riderNames = new List<string>();
        //.Select((str, index) => new { Index = index, Value = str })
        //.ToDictionary(item => item.Index, item => item.Value);
        IEnumerable<TrainingMotoGpEvent> data = PreProcessData(seasons, trackNames, riderNames);
        logger.LogDebug("Track Names: {trackNames}", string.Join(", ", trackNames));
        logger.LogDebug("Rider Names: {riderNames}", string.Join(", ", riderNames));

        var context = new MLContext(seed: configuration.GetValue<int>("Seed"));
        context.Log += (sender, args) => logger.LogTrace(args.Message);

        IDataView? dataView = context.Data.LoadFromEnumerable(data);
        //TODO configure train split

        IDataView trainView = dataView;
        IDataView testView = dataView;

        if (testFraction > 0)
        {
            DataOperationsCatalog.TrainTestData splitView = context.Data.TrainTestSplit(dataView, testFraction);
            trainView = splitView.TrainSet;
            testView = splitView.TestSet;
        }

        EstimatorChain<ColumnCopyingTransformer>? conversionPipeline =
            context.Transforms.Concatenate("Features", "Year", "TrackNameEncoded")
                   .Append(context.Transforms.CopyColumns("Label", "RaceWinnerEncoded"));

        //TODO Configure algo
        //TODO evaluate multiple algos
        EstimatorChain<RegressionPredictionTransformer<LinearRegressionModelParameters>>? sdcaPipeline =
            conversionPipeline
                .Append(context.Regression.Trainers.Sdca(maximumNumberOfIterations: 100));

        EstimatorChain<RegressionPredictionTransformer<PoissonRegressionModelParameters>>? lbfgsPoissonPipeline =
            conversionPipeline.Append(context.Regression.Trainers.LbfgsPoissonRegression());

        EstimatorChain<RegressionPredictionTransformer<LinearRegressionModelParameters>>? onlineGradientPipeline =
            conversionPipeline.Append(context.Regression.Trainers.OnlineGradientDescent());

        //TODO evaluate algos
        IReadOnlyList<TrainCatalogBase.CrossValidationResult<RegressionMetrics>>? sdcaResults =
            context.Regression.CrossValidate(dataView, sdcaPipeline);

        IReadOnlyList<TrainCatalogBase.CrossValidationResult<RegressionMetrics>>? lbfgsPoissonResults =
            context.Regression.CrossValidate(dataView, lbfgsPoissonPipeline);

        //IReadOnlyList<TrainCatalogBase.CrossValidationResult<RegressionMetrics>>? onlineGradientResults =
        //    context.Regression.CrossValidate(dataView, onlineGradientPipeline);

        string GetResults(IReadOnlyList<TrainCatalogBase.CrossValidationResult<RegressionMetrics>>? results)
        {
            var builder = new StringBuilder();
            foreach (TrainCatalogBase.CrossValidationResult<RegressionMetrics> result in results)
            {
                builder.AppendLine($"\t{result.Fold}: {result.Metrics.RSquared}");
            }

            return builder.ToString();
        }

        logger.LogInformation("SDCA Results: {results}", GetResults(sdcaResults));

        logger.LogInformation("LBFGS Poisson Results: {results}", GetResults(lbfgsPoissonResults));

        //logger.LogInformation("Online Gradient Results: {results}", GetResults(onlineGradientResults));

        //TransformerChain<RegressionPredictionTransformer<LinearRegressionModelParameters>>? sdcaModel =
        //    sdcaPipeline.Fit(trainView);

        //context.Model.Save(sdcaModel, trainView.Schema,
        //    configuration.GetValue<string>("ModelPath")
        //                 .Replace("{algo}", "sdca")
        //                 .Replace("{timestamp}", DateTime.Now.ToFileTimeUtc().ToString()));
    }

    private IEnumerable<TrainingMotoGpEvent> PreProcessData(Season[] seasons, List<string> trackNames,
        List<string> riderNames)
    {
        IEnumerable<TrainingMotoGpEvent> events = seasons.SelectMany(season =>
        {
            return season.Events.Where(_event => _event.HasMotoGpWinner()).Select(_event =>
            {
                string trackName = _event.Name;
                string raceWinner = _event.GetMotoGpWinner();

                if (!trackNames.Contains(trackName))
                {
                    trackNames.Add(trackName);
                }

                if (!riderNames.Contains(raceWinner))
                {
                    riderNames.Add(raceWinner);
                }

                return new TrainingMotoGpEvent
                {
                    Year = season.Year,
                    TrackNameEncoded = trackNames.IndexOf(trackName),
                    RaceWinnerEncoded = riderNames.IndexOf(raceWinner)
                };
            });
        });

        return events.ToArray();
    }
}

public class TrainingMotoGpEvent
{
    public float RaceWinnerEncoded { get; set; }

    public float TrackNameEncoded { get; set; }

    public float Year { get; set; }
}