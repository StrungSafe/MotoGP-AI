using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MotoGP.Interfaces;

namespace MotoGP.Scraper;

public class DataRepository : IDataRepository
{
    private readonly IHttpClientFactory clientFactory;

    private readonly IConfiguration configuration;

    private readonly ILogger<DataRepository> logger;

    private readonly IDataReader reader;

    private readonly IDataWriter writer;

    public DataRepository(ILogger<DataRepository> logger, IHttpClientFactory clientFactory,
        IConfiguration configuration, IDataWriter writer, IDataReader reader)
    {
        this.logger = logger;
        this.clientFactory = clientFactory;
        this.configuration = configuration;
        this.writer = writer;
        this.reader = reader;
    }

    public Task<Category[]> GetCategories(Guid seasonId, Guid eventId)
    {
        return GetFromJson<Category[]>($"categories?eventUuid={eventId}",
            $"{seasonId}/{eventId}/categories.json");
    }

    public Task<Event[]> GetEvents(Guid seasonId, bool isFinished)
    {
        return GetFromJson<Event[]>($"events?seasonUuid={seasonId}&isFinished={isFinished}",
            $"{seasonId}/events_{isFinished}.json");
    }

    public Task<Season[]> GetSeasons()
    {
        return GetFromJson<Season[]>("seasons", "seasons.json");
    }

    public Task<SessionClassification> GetSessionClassification(Guid seasonId, Guid eventId, Guid categoryId,
        Guid sessionId, bool test)
    {
        return GetFromJson<SessionClassification>(
            $"session/{sessionId}/classification?test={test}",
            $"{seasonId}/{eventId}/{categoryId}/{sessionId}/classifications_{test}.json");
    }

    public Task<Session[]> GetSessions(Guid seasonId, Guid eventId, Guid categoryId)
    {
        return GetFromJson<Session[]>(
            $"sessions?eventUuid={eventId}&categoryUuid={categoryId}",
            $"{seasonId}/{eventId}/{categoryId}/sessions.json");
    }

    private async Task<T> GetFromJson<T>(string relativeUrl, string relativeUri)
    {
        string path = Path.Join(configuration["DataRepository:RepoDirectory"], relativeUri);
        bool overwrite = configuration.GetValue<bool>("DataRepository:OverwriteData");
        if (!File.Exists(path) || overwrite)
        {
            using HttpClient client = clientFactory.CreateClient(configuration["MotoGP:Name"]);
            var data = await client.GetFromJsonAsync<T>(new Uri(relativeUrl, UriKind.Relative));
            await writer.Write(path, data);
            return data;
        }

        logger.LogInformation("Retrieving data from local source instead of API. Url: {relativeUrl} Uri: {relativeUri}",
            relativeUrl, relativeUri);
        return await reader.Read<T>(path);
    }
}