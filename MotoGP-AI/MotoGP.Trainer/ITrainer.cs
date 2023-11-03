using MotoGP.Interfaces;

namespace MotoGP.Trainer;

public interface ITrainer
{
    Task<object> TrainModel(Season[] data);
}