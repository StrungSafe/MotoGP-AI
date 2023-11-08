using Microsoft.ML;
using MotoGP.Extensions;
using MotoGP.Interfaces;

namespace MotoGP.Trainer;

public class SimpleDataFormatter : IDataFormatter
{
    public IEstimator<ITransformer> GetConversionPipeline(MLContext context)
    {
        return
            context.Transforms.Concatenate("Features", "TrackNameEncoded")
                   .Append(context.Transforms.CopyColumns("Label", "RaceWinnerEncoded"));
    }

    public IEnumerable<TrainingMotoGpEvent> PreProcessData(Season[] seasons, List<string> trackNames,
        List<string> riderNames)
    {
        IEnumerable<TrainingMotoGpEvent> events = seasons.SelectMany(season =>
        {
            return season.Events.Where(_event => _event.HasMotoGpWinner()).Select(_event =>
            {
                string trackName = _event.Name;
                string raceWinner = _event.GetMotoGpWinner();

                if (!trackNames.Contains(trackName))
                {
                    trackNames.Add(trackName);
                }

                if (!riderNames.Contains(raceWinner))
                {
                    riderNames.Add(raceWinner);
                }

                return new TrainingMotoGpEvent
                {
                    TrackNameEncoded = trackNames.IndexOf(trackName),
                    RaceWinnerEncoded = riderNames.IndexOf(raceWinner)
                };
            });
        });

        return events.ToArray();
    }
}