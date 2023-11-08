﻿using Microsoft.ML;
using MotoGP.Interfaces;

namespace MotoGP.Trainer;

public interface IDataFormatter
{
    IEstimator<ITransformer> GetConversionPipeline(MLContext context);

    Task<IEnumerable<TrainingMotoGpEvent>> PreProcessData(Season[] seasons);
}