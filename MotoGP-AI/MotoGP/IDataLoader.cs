using MotoGP.Interfaces;

namespace MotoGP
{
    public interface IDataLoader
    {
        Task<IEnumerable<Season>> Load();
    }
}