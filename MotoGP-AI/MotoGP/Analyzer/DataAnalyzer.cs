using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MotoGP.Data;
using MotoGP.Utilities;

namespace MotoGP.Analyzer;

public class DataAnalyzer : IDataAnalyzer
{
    private readonly IConfiguration configuration;

    private readonly ILogger<DataAnalyzer> logger;

    private readonly IDataReader reader;

    public DataAnalyzer(ILogger<DataAnalyzer> logger, IConfiguration configuration, IDataReader reader)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.reader = reader;
    }

    public async Task AnalyzeData()
    {
        logger.LogInformation("Analyzing the data...");
        Season[] seasons = await reader.Read<Season[]>(configuration["FilePath"]);

        var categoryTypes = new Dictionary<string, string>();
        var sessionTypes = new Dictionary<string, string>();
        var eventTypes = new Dictionary<string, string>();
        var weatherTypes = new Dictionary<string, string>();
        var recordTypes = new Dictionary<string, string>();
        var eventNames = new Dictionary<string, string>();
        var riderNames = new Dictionary<string, string>();

        foreach (Season season in seasons)
        {
            foreach (Event _event in season.Events)
            {
                eventNames.TryAdd(_event.Name, _event.Name);

                foreach (Category category in _event.Categories)
                {
                    categoryTypes.TryAdd(category.Name, category.Name);

                    foreach (Session session in category.Sessions)
                    {
                        sessionTypes.TryAdd(session.Type, session.Type);

                        eventTypes.TryAdd(session.Condition.Track, session.Condition.Track);
                        weatherTypes.TryAdd(session.Condition.Weather, session.Condition.Weather);

                        foreach (Classification classification in session.SessionClassification.Classifications)
                        {
                            riderNames.TryAdd(classification.Rider.FullName, classification.Rider.FullName);
                        }

                        foreach (Record record in session.SessionClassification.Records)
                        {
                            recordTypes.TryAdd(record.Type, record.Type);
                        }
                    }
                }
            }
        }

        string Join(IEnumerable<string> values, string separator = ", ")
        {
            return string.Join(separator, values);
        }

        logger.LogDebug("Category Types: {categoryTypes}", Join(categoryTypes.Values));
        logger.LogDebug("Session Types: {sessionTypes}", Join(sessionTypes.Values));
        logger.LogDebug("Event Types: {eventTypes}", Join(eventTypes.Values));
        logger.LogDebug("Weather Types: {weatherTypes}", Join(weatherTypes.Values));
        logger.LogDebug("Record Types: {recordTypes}", Join(recordTypes.Values));
        logger.LogDebug("Event Names ({eventCount}): {eventNames}", eventNames.Count,
            Join(eventNames.Values, $", {Environment.NewLine}"));
        logger.LogDebug("Riders ({riderCount}): {riderNames}", riderNames.Values.Count,
            Join(riderNames.Values, $", {Environment.NewLine}"));

        await Task.Delay(100); //TODO logger not flushing...there is a way to flush but need to find out again
    }
}