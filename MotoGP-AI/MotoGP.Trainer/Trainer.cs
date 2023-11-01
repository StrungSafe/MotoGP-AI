using MotoGP.Interfaces;

namespace MotoGP.Trainer;

public class Trainer : ITrainer
{
    public Task<object> TrainModel(Season[] data)
    {
        throw new NotImplementedException();
    }
}