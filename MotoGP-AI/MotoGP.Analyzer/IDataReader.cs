using MotoGP.Interfaces;

namespace MotoGP.Analyzer;

public interface IDataReader
{
    Task<Season[]> ReadData();
}