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

        foreach (Season season in seasons)
        {
            foreach (Event _event in season.Events)
            {
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
                        }

                        foreach (Record record in session.SessionClassification.Records)
                        {
                            recordTypes.TryAdd(record.Type, record.Type);
                        }
                    }
                }
            }
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Category Types: {string.Join(", ", categoryTypes.Values)}");
        builder.AppendLine($"Session Types: {string.Join(", ", sessionTypes.Values)}");
        builder.AppendLine($"Track Types: {string.Join(", ", trackTypes.Values)}");
        builder.AppendLine($"Weather Types: {string.Join(", ", weatherTypes.Values)}");
        builder.AppendLine($"Record Types: {string.Join(", ", recordTypes.Values)}");
        logger.LogInformation(builder.ToString());
    }
}