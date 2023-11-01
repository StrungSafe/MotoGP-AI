using MotoGP.Interfaces;

namespace MotoGP.Trainer;

public interface IDataReader
{
    Task<Season[]> ReadTrainingData();
}