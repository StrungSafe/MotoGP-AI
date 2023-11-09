using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using MotoGP.Configuration;
using MotoGP.Utilities;

namespace MotoGP.Predictor;

public class DataPredictor : IDataPredictor
{
    

    private readonly MachineLearning settings;
    private readonly ILogger<DataPredictor> logger;
    private readonly IDataReader reader;

    public DataPredictor(ILogger<DataPredictor> logger, IOptions<MachineLearning> settings, IDataReader reader)
    {
        this.logger = logger;
        this.reader = reader;
        this.settings = settings.Value;
        this.logger = logger;
    }

    public async Task<MotoGpEventPrediction> Predict(MotoGpEvent example)
    {
        var context = new MLContext(settings.Seed);

        var path = Path.Join(settings.Models.LocalPath, "model.zip");
        ITransformer? model = context.Model.Load(path, out DataViewSchema schema);

        PredictionEngine<MotoGpEvent, MotoGpEventPrediction> engine =
            context.Model.CreatePredictionEngine<MotoGpEvent, MotoGpEventPrediction>(model);

        MotoGpEventPrediction prediction = engine.Predict(example);

        var ridersPath = Path.Join(settings.Objects.LocalPath, "riders.json");
        var riders = await reader.Read<Dictionary<int, string>>(ridersPath);

        var roudedScore = (int)Math.Round(prediction.WinnerEncoded, MidpointRounding.AwayFromZero);
        prediction.Winner = riders[roudedScore];
        return prediction;
    }
}