namespace Scraper;

public interface IDataWriter
{
    Task SaveData(IEnumerable<Season> seasons);
}