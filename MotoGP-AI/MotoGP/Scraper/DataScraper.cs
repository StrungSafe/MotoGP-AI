using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MotoGP.Data;
using MotoGP.Utilities;

namespace MotoGP.Scraper;

public class DataScraper : IDataScraper
{
    private readonly IConfiguration configuration;

    private readonly IDataLoader loader;

    private readonly ILogger<DataScraper> logger;

    private readonly IDataWriter writer;

    public DataScraper(ILogger<DataScraper> logger, IDataLoader loader, IDataWriter writer,
        IConfiguration configuration)
    {
        this.logger = logger;
        this.loader = loader;
        this.writer = writer;
        this.configuration = configuration;
    }

    public async Task Scrape(CancellationToken token)
    {
        try
        {
            IEnumerable<Season> seasons = await loader.Load();
            await Task.WhenAll(WriteSeasons(seasons, token),
                WriteEvents(seasons, token),
                WriteRiders(seasons, token));
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "There was an unhandled exception...unable to continue, shutting down.");
        }
    }

    private async Task WriteEvents(IEnumerable<Season> seasons, CancellationToken token)
    {
        Dictionary<int, Event> events = seasons.SelectMany(s => s.Events)
                                               .DistinctBy(e => e.Name)
                                               .Select(e => new Event
                                               {
                                                   Name = e.Name
                                               })
                                               .Select((value, index) => new { Index = index, Value = value })
                                               .ToDictionary(item => item.Index, item => item.Value);
        await writer.Write(configuration["EventsPath"], events, token);
    }

    private async Task WriteRiders(IEnumerable<Season> seasons, CancellationToken token)
    {
        Dictionary<int, Rider> riders = seasons.SelectMany(s => s.Events).SelectMany(e => e.Categories)
                                               .SelectMany(c => c.Sessions).SelectMany(s =>
                                                   s.SessionClassification.Classifications)
                                               .DistinctBy(c => c.Rider.FullName)
                                               .Select(c => new Rider
                                               {
                                                   FullName = c.Rider.FullName
                                               })
                                               .Select((value, index) => new { Index = index, Value = value })
                                               .ToDictionary(item => item.Index, item => item.Value);
        await writer.Write(configuration["RidersPath"], riders, token);
    }

    private Task WriteSeasons(IEnumerable<Season> seasons, CancellationToken token)
    {
        return writer.Write(configuration["FilePath"], seasons, token);
    }
}