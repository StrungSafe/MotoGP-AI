namespace Scraper
{
    public interface IScraper
    {
        Task<IEnumerable<Season>> Scrape();
    }
}