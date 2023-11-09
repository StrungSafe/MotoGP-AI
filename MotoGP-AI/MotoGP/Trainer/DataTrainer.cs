using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using MotoGP.Api;
using MotoGP.Configuration;
using MotoGP.Utilities;

namespace MotoGP.Trainer;

public class DataTrainer : IDataTrainer
{
    private readonly IDataFormatter dataFormatter;

    private readonly ILogger<DataTrainer> logger;

    private readonly IDataReader reader;
    private readonly MachineLearning settings;

    public DataTrainer(ILogger<DataTrainer> logger, IOptions<MachineLearning> settings, IDataReader reader,
        IDataFormatter dataFormatter)
    {
        this.logger = logger;
        this.settings = settings.Value;
        this.reader = reader;
        this.dataFormatter = dataFormatter;
    }

    public async Task TrainAndSaveModel()
    {
        var dataPath = Path.Join(settings.Objects.LocalPath, "seasons.json");
        Season[] seasons = await reader.Read<Season[]>(dataPath);
        var testFraction = settings.TestFraction;

        logger.LogInformation("Attempting to train a model on '{seasonCount}' season(s)", seasons.Length);

        IEnumerable<TrainingMotoGpEvent> data = await dataFormatter.PreProcessData(seasons);

        var context = new MLContext(settings.Seed);
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

        string GetResults(IReadOnlyList<TrainCatalogBase.CrossValidationResult<RegressionMetrics>> results)
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

        //TODO Ensure directory exists....model.save won't create the directory
        var modelPath = Path.Join(settings.Models.LocalPath, $"model_{"sdca"}_{DateTime.UtcNow.ToFileTimeUtc()}.zip");
        context.Model.Save(sdcaModel, trainView.Schema, modelPath);
    }
}