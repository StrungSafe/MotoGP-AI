using Microsoft.ML;
using MotoGP.Interfaces;
using MotoGP.Extensions;

namespace MotoGP.Trainer;

public class Trainer : ITrainer
{
    public Task<object> TrainModel(Season[] seasons)
    {
        var context = new MLContext();
        //context.Transforms.Text.FeaturizeText("TrackName");
        //context.Transforms.Text.FeaturizeText("Track");
        //context.Transforms.Text.FeaturizeText("Weather");
        //context.Transforms.Text.FeaturizeText("RaceWinner");

        //var textEstimator = mlContext.Transforms.Text.NormalizeText("Description")
        //                             .Append(mlContext.Transforms.Text.TokenizeIntoWords("Description"))
        //                             .Append(mlContext.Transforms.Text.RemoveDefaultStopWords("Description"))
        //                             .Append(mlContext.Transforms.Conversion.MapValueToKey("Description"))
        //                             .Append(mlContext.Transforms.Text.ProduceNgrams("Description"))
        //                             .Append(mlContext.Transforms.NormalizeLpNorm("Description"));

        context.Transforms.Conversion
               .MapValueToKey("TrackName")
               .MapValueToKey("");

        var data = Prep(seasons);

        var view = context.Data.LoadFromEnumerable(data);

        var split = context.Data.TrainTestSplit(view, 0.2);

        return null;
    }

    private string GetRaceWinner(Event _event)
    {
        //Session race = _event.Sessions.First(s => s.Type == "RAC");
        //Classification winner = race.SessionClassification.Classifications.First(c => c.Position == 1);
        //return winner.Rider.FullName;
        return null;
    }

    private object[] Prep(Season[] seasons)
    {
        var events = seasons.SelectMany(season =>
        {
            return season.Events.Select(_event =>
            {
                string raceWinner = GetRaceWinner(_event);
                //Session session = _event.Sessions.First(s => s.Type == "RAC");
                Session session = default;

                return new
                {
                    Year = (float)season.Year,
                    TrackName = _event.Name,
                    session.Condition.Track,
                    session.Condition.Weather,
                    Air = float.Parse(session.Condition.Air.Replace("\u00BA", string.Empty)),
                    Humidity = float.Parse(session.Condition.Humidity.Replace("%", string.Empty)),
                    Ground = float.Parse(session.Condition.Ground.Replace("\u00BA", string.Empty)),
                    RaceWinner = raceWinner
                };
            });
        });

        //qualyifying{}
        //classifications[]
        //rider
        //time

        return events.ToArray();
    }
}