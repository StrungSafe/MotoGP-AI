using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using MotoGP.Api;
using MotoGP.Configuration;
using MotoGP.Extensions;
using MotoGP.Utilities;

namespace MotoGP.Trainer;

public class SimpleDataFormatter : IDataFormatter
{

    private readonly ILogger<SimpleDataFormatter> logger;

    private readonly IDataReader reader;
    private readonly MachineLearning settings;

    public SimpleDataFormatter(ILogger<SimpleDataFormatter> logger, IOptions<MachineLearning> settings, IDataReader reader)
    {
        this.logger = logger;
        this.settings = settings.Value;
        this.reader = reader;
    }

    public IEstimator<ITransformer> GetConversionPipeline(MLContext context)
    {
        return
            context.Transforms.Concatenate("Features", "EventNameEncoded")
                   .Append(context.Transforms.CopyColumns("Label", "RaceWinnerEncoded"));
    }

    public async Task<IEnumerable<TrainingMotoGpEvent>> PreProcessData(Season[] seasons)
    {
        var circuitsPath = Path.Join(settings.Objects.LocalPath, "circuits.json");
        var ridersPath = Path.Join(settings.Objects.LocalPath, "riders.json");
        Dictionary<string, int> circuits =
            (await reader.Read<Dictionary<int, string>>(circuitsPath)).ToDictionary(e => e.Value,
                e => e.Key);
        Dictionary<string, int> riders =
            (await reader.Read<Dictionary<int, string>>(ridersPath)).ToDictionary(r => r.Value,
                r => r.Key);

        logger.LogDebug("Circuit Names: {circuitNames}", string.Join(", ", circuits.Values));
        logger.LogDebug("Rider Names: {riderNames}", string.Join(", ", riders.Values));

        return seasons.SelectMany(season =>
        {
            return season.Events.Where(_event => _event.HasMotoGpWinner()).Select(_event =>
            {
                string circuitName = _event.Categories.First().Sessions.First().Circuit;
                string raceWinner = _event.GetMotoGpWinner();

                return new TrainingMotoGpEvent
                {
                    EventNameEncoded = circuits[circuitName],
                    RaceWinnerEncoded = riders[raceWinner]
                };
            });
        });
    }
}