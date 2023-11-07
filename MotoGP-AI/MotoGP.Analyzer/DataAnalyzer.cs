using System.Text;
using Microsoft.Extensions.Logging;
using MotoGP.Interfaces;

namespace MotoGP.Analyzer;

public class DataAnalyzer : IDataAnalyzer
{
    private readonly ILogger<DataAnalyzer> logger;

    public DataAnalyzer(ILogger<DataAnalyzer> logger)
    {
        this.logger = logger;
    }

    public async Task AnalyzeData(Season[] seasons)
    {
        var categoryTypes = new Dictionary<string, string>();
        var sessionTypes = new Dictionary<string, string>();
        var trackTypes = new Dictionary<string, string>();
        var weatherTypes = new Dictionary<string, string>();
        var recordTypes = new Dictionary<string, string>();
        var trackNames = new Dictionary<string, string>();
        var racerNames = new Dictionary<string, string>();

        foreach (Season season in seasons)
        {
            foreach (Event _event in season.Events)
            {
                trackNames.TryAdd(_event.Name, _event.Name);

                foreach (Category category in _event.Categories)
                {
                    categoryTypes.TryAdd(category.Name, category.Name);

                    foreach (Session session in category.Sessions)
                    {
                        sessionTypes.TryAdd(session.Type, session.Type);

                        trackTypes.TryAdd(session.Condition.Track, session.Condition.Track);
                        weatherTypes.TryAdd(session.Condition.Weather, session.Condition.Weather);

                        foreach (Classification classification in session.SessionClassification.Classifications)
                        {
                            racerNames.TryAdd(classification.Rider.FullName, classification.Rider.FullName);
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

        var builder = new StringBuilder();
        builder.AppendLine($"Category Types: {Join(categoryTypes.Values)}");
        builder.AppendLine($"Session Types: {Join(sessionTypes.Values)}");
        builder.AppendLine($"Track Types: {Join(trackTypes.Values)}");
        builder.AppendLine($"Weather Types: {Join(weatherTypes.Values)}");
        builder.AppendLine($"Record Types: {Join(recordTypes.Values)}");
        builder.AppendLine($"Track Names: {Join(trackNames.Values, $", {Environment.NewLine}")}");
        builder.AppendLine($"Riders: {Join(racerNames.Values, $", {Environment.NewLine}")}");
        logger.LogInformation(builder.ToString());
    }
}