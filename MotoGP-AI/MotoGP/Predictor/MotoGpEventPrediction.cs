using Microsoft.ML.Data;

namespace MotoGP.Predictor;

public class MotoGpEventPrediction
{
    [ColumnName("Score")]
    public float WinnerEncoded { get; set; }
    public string Winner { get; set; }
}