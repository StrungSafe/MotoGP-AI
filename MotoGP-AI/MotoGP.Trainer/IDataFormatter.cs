using Microsoft.ML;
using MotoGP.Interfaces;

namespace MotoGP.Trainer;

public interface IDataFormatter
{
    IEstimator<ITransformer> GetConversionPipeline(MLContext context);

    IEnumerable<TrainingMotoGpEvent> PreProcessData(Season[] seasons, List<string> trackNames, List<string> riderNames);
}