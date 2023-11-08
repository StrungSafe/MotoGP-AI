using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using MotoGP.Extensions;
using MotoGP.Interfaces;

namespace MotoGP.Trainer;

public class SimpleDataFormatter : IDataFormatter
{
    private readonly IConfiguration configuration;

    private readonly ILogger<SimpleDataFormatter> logger;

    private readonly IDataReader reader;

    public SimpleDataFormatter(ILogger<SimpleDataFormatter> logger, IConfiguration configuration, IDataReader reader)
    {
        this.logger = logger;
        this.configuration = configuration;
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
        Dictionary<string, int> events =
            (await reader.Read<Dictionary<int, Event>>(configuration["EventsPath"])).ToDictionary(e => e.Value.Name,
                e => e.Key);
        Dictionary<string, int> riders =
            (await reader.Read<Dictionary<int, Rider>>(configuration["RidersPath"])).ToDictionary(r => r.Value.FullName,
                r => r.Key);

        logger.LogDebug("Event Names: {eventNames}", string.Join(", ", events.Values));
        logger.LogDebug("Rider Names: {riderNames}", string.Join(", ", riders.Values));

        return seasons.SelectMany(season =>
        {
            return season.Events.Where(_event => _event.HasMotoGpWinner()).Select(_event =>
            {
                string eventName = _event.Name;
                string raceWinner = _event.GetMotoGpWinner();

                return new TrainingMotoGpEvent
                {
                    EventNameEncoded = events[eventName],
                    RaceWinnerEncoded = riders[raceWinner]
                };
            });
        });
    }
}