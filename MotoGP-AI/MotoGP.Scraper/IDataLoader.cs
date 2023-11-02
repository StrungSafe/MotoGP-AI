using MotoGP.Interfaces;

namespace MotoGP.Scraper
{
    public interface IDataLoader
    {
        Task<IEnumerable<Season>> Load();
    }
}