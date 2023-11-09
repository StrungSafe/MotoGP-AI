namespace MotoGP.Predictor;

public interface IDataPredictor
{
    Task<MotoGpEventPrediction> Predict(MotoGpEvent example);
}