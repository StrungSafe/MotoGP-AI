using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using MotoGP.Interfaces;

namespace MotoGP.Trainer;

public class Trainer : ITrainer
{
    private readonly IConfiguration configuration;

    private readonly IDataFormatter dataFormatter;

    private readonly ILogger<Trainer> logger;

    private readonly IDataReader reader;

    public Trainer(ILogger<Trainer> logger, IConfiguration configuration, IDataReader reader,
        IDataFormatter dataFormatter)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.reader = reader;
        this.dataFormatter = dataFormatter;
    }

    public async Task TrainAndSaveModel()
    {
        Season[] seasons = await reader.Read<Season[]>(configuration["DataPath"]);
        var testFraction = configuration.GetValue<double>("TestFraction");

        logger.LogInformation("Attempting to train a model on '{seasonCount}' season(s)", seasons.Length);

        IEnumerable<TrainingMotoGpEvent> data = await dataFormatter.PreProcessData(seasons);

        var context = new MLContext(seed: configuration.GetValue<int>("Seed"));
        context.Log += (sender, args) => logger.LogTrace(args.Message);

        IDataView? dataView = context.Data.LoadFromEnumerable(data);

        IDataView trainView = dataView;
        IDataView testView = dataView;

        if (testFraction > 0)
        {
            DataOperationsCatalog.TrainTestData splitView = context.Data.TrainTestSplit(dataView, testFraction);
            trainView = splitView.TrainSet;
            testView = splitView.TestSet; //TODO
        }

        IEstimator<ITransformer> conversionPipeline = dataFormatter.GetConversionPipeline(context);

        //TODO Configure algo
        //TODO evaluate multiple algos
        EstimatorChain<RegressionPredictionTransformer<LinearRegressionModelParameters>>? sdcaPipeline =
            conversionPipeline
                .Append(context.Regression.Trainers.Sdca(maximumNumberOfIterations: 100));

        EstimatorChain<RegressionPredictionTransformer<PoissonRegressionModelParameters>>? lbfgsPoissonPipeline =
            conversionPipeline.Append(context.Regression.Trainers.LbfgsPoissonRegression());

        //EstimatorChain<RegressionPredictionTransformer<LinearRegressionModelParameters>>? onlineGradientPipeline =
        //    conversionPipeline.Append(context.Regression.Trainers.OnlineGradientDescent());

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

        TransformerChain<RegressionPredictionTransformer<LinearRegressionModelParameters>>? sdcaModel =
            sdcaPipeline.Fit(trainView);

        context.Model.Save(sdcaModel, trainView.Schema,
            configuration.GetValue<string>("ModelPath")
                         .Replace("{algo}", "sdca")
                         .Replace("{timestamp}", DateTime.Now.ToFileTimeUtc().ToString()));
    }
}