using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MotoGP.Interfaces;

namespace MotoGP;

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
            $"seasons/{seasonId}/{eventId}/categories.json");
    }

    public Task<Event[]> GetEvents(Guid seasonId, bool isFinished)
    {
        return GetFromJson<Event[]>($"events?seasonUuid={seasonId}&isFinished={isFinished}",
            $"seasons/{seasonId}/events_{isFinished}.json");
    }

    public Task<Season[]> GetSeasons()
    {
        return GetFromJson<Season[]>("seasons", "seasons/seasons.json");
    }

    public Task<SessionClassification> GetSessionClassification(Guid seasonId, Guid eventId, Guid categoryId,
        Guid sessionId, bool test)
    {
        return GetFromJson<SessionClassification>(
            $"session/{sessionId}/classification?test={test}",
            $"seasons/{seasonId}/{eventId}/{categoryId}/{sessionId}/classifications_{test}.json");
    }

    public Task<Session[]> GetSessions(Guid seasonId, Guid eventId, Guid categoryId)
    {
        return GetFromJson<Session[]>(
            $"sessions?eventUuid={eventId}&categoryUuid={categoryId}",
            $"seasons/{seasonId}/{eventId}/{categoryId}/sessions.json");
    }

    private async Task<T> GetFromJson<T>(string relativeUrl, string relativeUri)
    {
        string path = Path.Join(configuration["DataRepository:RepoDirectory"], relativeUri);
        bool overwrite = configuration.GetValue<bool?>("DataRepository:Overwrite") ?? false;
        bool overwriteOnError = configuration.GetValue<bool?>("DataRepository:OverwriteOnError") ?? true;

        async Task<T> FromApi()
        {
            using HttpClient client = clientFactory.CreateClient(configuration["MotoGP:Name"]);
            var data = await client.GetFromJsonAsync<T>(new Uri(relativeUrl, UriKind.Relative));
            await writer.Write(path, data);
            return data;
        }

        if (!File.Exists(path) || overwrite)
        {
            return await FromApi();
        }

        try
        {
            logger.LogInformation(
                "Retrieving data from local source instead of API. Url: {relativeUrl} Uri: {relativeUri}",
                relativeUrl, relativeUri);

            return await reader.Read<T>(path);
        }
        catch (Exception ex)
        {
            if (overwriteOnError)
            {
                logger.LogWarning(ex,
                    "Exception caught while trying to read a local data file...attempting to refresh from the API");
                return await FromApi();
            }

            throw;
        }
    }
}