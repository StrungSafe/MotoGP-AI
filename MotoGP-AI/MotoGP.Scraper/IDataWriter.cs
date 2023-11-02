namespace MotoGP.Scraper;

public interface IDataWriter
{
    Task Write<T>(string filePath, T data);
}