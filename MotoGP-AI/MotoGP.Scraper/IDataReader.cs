namespace MotoGP.Scraper;

public interface IDataReader
{
    Task<T> Read<T>(string filePath);
}