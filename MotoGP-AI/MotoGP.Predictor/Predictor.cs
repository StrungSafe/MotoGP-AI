using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace MotoGP.Predictor;

public class Predictor : IPredictor
{
    private readonly IConfiguration configuration;

    private readonly ILogger<Predictor> logger;

    public Predictor(ILogger<Predictor> logger, IConfiguration configuration)
    {
        this.logger = logger;
        this.configuration = configuration;
    }

    public async Task Predict()
    {
        var context = new MLContext(seed: configuration.GetValue<int>("Seed"));

        ITransformer? model = context.Model.Load(configuration["ModelPath"], out DataViewSchema schema);

        PredictionEngine<MotoGpEvent, MotoGpEventPrediction>? engine =
            context.Model.CreatePredictionEngine<MotoGpEvent, MotoGpEventPrediction>(model);

        var example = new MotoGpEvent
        {
            Year = 2023,
            EventNameEncoded = 5
        };

        MotoGpEventPrediction? prediction = engine.Predict(example);

        var rider = (int)Math.Round(prediction.Score, MidpointRounding.AwayFromZero);
        Console.WriteLine($"prediction {prediction.Score} rider {rider}");
        await Task.Delay(0);
    }
}