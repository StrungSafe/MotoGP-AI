using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Scraper;

public class JsonDataWriter : IDataWriter
{
    private readonly IConfiguration configuration;

    private readonly ILogger<JsonDataWriter> logger;

    public JsonDataWriter(ILogger<JsonDataWriter> logger, IConfiguration configuration)
    {
        this.logger = logger;
        this.configuration = configuration;
    }

    public Task SaveData(IEnumerable<Season> seasons)
    {
        string? filePath = configuration["FilePath"];
        if (!Uri.TryCreate(filePath, UriKind.Absolute, out Uri? result))
        {
            logger.LogCritical("Unable to save data...the configured file path is invalid");
            throw new Exception("Configured file is not a valid Uri");
        }

        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
        {
            logger.LogCritical("Unable to save data...the configured directory does not exist.");
            throw new Exception("Directory defined does not exist");
        }

        string contents = JsonSerializer.Serialize(seasons);
        return File.WriteAllTextAsync(filePath, contents);
    }
}