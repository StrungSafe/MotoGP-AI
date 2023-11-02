﻿using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MotoGP.Scraper;

public class JsonDataService : IDataReader, IDataWriter
{
    private readonly ILogger<JsonDataService> logger;

    public JsonDataService(ILogger<JsonDataService> logger)
    {
        this.logger = logger;
    }

    public async Task<T> Read<T>(string filePath)
    {
        if (!File.Exists(filePath))
        {
            logger.LogCritical("File {filePath} does not exist...unable to continue", filePath);
            throw new Exception("Configured file does not exist");
        }

        string contents = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<T>(contents);
    }

    public Task Write<T>(string filePath, T data)
    {
        if (!Uri.TryCreate(filePath, UriKind.Absolute, out Uri? result))
        {
            logger.LogCritical("Unable to save data...the file path '{filePath}' is invalid", filePath);
            throw new Exception("Configured file is not a valid Uri");
        }

        string? directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string contents = JsonSerializer.Serialize(data);
        return File.WriteAllTextAsync(filePath, contents);
    }
}