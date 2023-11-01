using MotoGP.Interfaces;

namespace MotoGP.Scraper
{
    public interface IDataScraper
    {
        Task<IEnumerable<Season>> Scrape();
    }
}