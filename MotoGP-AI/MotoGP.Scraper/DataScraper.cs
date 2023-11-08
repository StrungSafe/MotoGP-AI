using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MotoGP.Interfaces;

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

    public async Task Scrape()
    {
        try
        {
            IEnumerable<Season> seasons = await loader.Load();
            await writer.Write(configuration["FilePath"], seasons);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "There was an unhandled exception...unable to continue, shutting down.");
        }
    }
}