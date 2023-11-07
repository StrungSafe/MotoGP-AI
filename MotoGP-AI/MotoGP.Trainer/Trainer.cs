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
        Season[] seasons = await reader.Read<Season[]>(configuration["FilePath"]);

        logger.LogInformation("Attempting to train a model on '{seasonCount}' season(s)", seasons.Length);

        var trackNames = new List<string>();
        var riderNames = new List<string>();
        IEnumerable<TrainingMotoGpEvent> data = PrepBeforeEvent(seasons, trackNames, riderNames);
        logger.LogDebug("Track Names: {trackNames}", string.Join(", ", trackNames));
        logger.LogDebug("Rider Names: {riderNames}", string.Join(", ", riderNames));

        var context = new MLContext(seed: configuration.GetValue<int>("Seed"));
        //context.Log += (sender, args) => logger.LogDebug(args.Message);

        IDataView? dataView = context.Data.LoadFromEnumerable(data);
        DataOperationsCatalog.TrainTestData view = context.Data.TrainTestSplit(dataView, 0.2);

        EstimatorChain<ColumnCopyingTransformer>? conversionPipeline = context
                                                                       .Transforms.Concatenate("Features", "Year",
                                                                           "TrackNameEncoded")
                                                                       .Append(context.Transforms.CopyColumns("Label",
                                                                           "RaceWinnerEncoded"));

        EstimatorChain<RegressionPredictionTransformer<LinearRegressionModelParameters>>? trainingPipeline =
            conversionPipeline
                .Append(context.Regression.Trainers.Sdca(maximumNumberOfIterations: 100));

        TransformerChain<RegressionPredictionTransformer<LinearRegressionModelParameters>>? model =
            trainingPipeline.Fit(view.TrainSet);

        context.Model.Save(model, view.TrainSet.Schema, configuration.GetValue<string>("ModelPath"));
    }

    private IEnumerable<TrainingMotoGpEvent> PrepBeforeEvent(Season[] seasons, List<string> trackNames,
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