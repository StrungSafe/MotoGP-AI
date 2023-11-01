using MotoGP.Interfaces;

namespace MotoGP.Trainer;

public interface IMlTrainer
{
    Task<object> TrainModel(Season[] data);
}