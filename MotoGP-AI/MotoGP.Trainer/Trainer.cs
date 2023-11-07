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
    private readonly ILogger<Trainer> logger;

    public Trainer(ILogger<Trainer> logger)
    {
        this.logger = logger;
    }

    public Task<object> TrainModel(Season[] seasons)
    {
        logger.LogInformation("Attempting to train a model on '{seasonCount}' season(s)", seasons.Length);

        var trackNames = new List<string>();
        var riderNames = new List<string>();
        IEnumerable<TrainingMotoGpEvent> data = PrepBeforeEvent(seasons, trackNames, riderNames);
        logger.LogDebug("Track Names: {trackNames}", string.Join(", ", trackNames));
        logger.LogDebug("Rider Names: {riderNames}", string.Join(", ", riderNames));

        var context = new MLContext(seed: 0); // TODO
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

        PredictionEngine<MotoGpEvent, MotoGpEventPrediction>? engine =
            context.Model.CreatePredictionEngine<MotoGpEvent, MotoGpEventPrediction>(model);

        var example = new MotoGpEvent
        {
            Year = 2023,
            TrackNameEncoded = 5
        };

        MotoGpEventPrediction? prediction = engine.Predict(example);

        var rider = (int)Math.Round(prediction.Score, MidpointRounding.AwayFromZero);
        Console.WriteLine($"prediction {prediction.Score} '{riderNames[rider]}'");

        return Task.FromResult(model as object);
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

public class MotoGpEventPrediction
{
    public float Score { get; set; }
}

public class MotoGpEvent
{
    public float RaceWinnerEncoded { get; set; }

    public float TrackNameEncoded { get; set; }

    public float Year { get; set; }
}

public class TrainingMotoGpEvent
{
    public float RaceWinnerEncoded { get; set; }

    public float TrackNameEncoded { get; set; }

    public float Year { get; set; }
}