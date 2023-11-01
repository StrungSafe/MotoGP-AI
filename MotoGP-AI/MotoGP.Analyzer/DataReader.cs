using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MotoGP.Interfaces;

namespace MotoGP.Analyzer;

public class DataReader : IDataReader
{
    private readonly IConfiguration configuration;

    private readonly ILogger<DataReader> logger;

    public DataReader(ILogger<DataReader> logger, IConfiguration configuration)
    {
        this.logger = logger;
        this.configuration = configuration;
    }

    public async Task<Season[]> ReadData()
    {
        string? filePath = configuration["FilePath"];
        if (!File.Exists(filePath))
        {
            logger.LogCritical("File does not exist...unable to continue");
            throw new Exception("Configured file does not exist");
        }

        string contents = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<Season[]>(contents);
    }
}