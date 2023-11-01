using Microsoft.ML;
using MotoGP.Interfaces;

namespace MotoGP.Trainer;

public class MlTrainer : IMlTrainer
{
    public Task<object> TrainModel(Season[] data)
    {
        var context = new MLContext();
        context.Transforms.Text.FeaturizeText("TrackName");
        context.Transforms.Text.FeaturizeText("Track");
        context.Transforms.Text.FeaturizeText("Weather");
        context.Transforms.Text.FeaturizeText("RaceWinner");
        context.Data.LoadFromEnumerable(Prep(data));
        return null;
    }

    private string GetRaceWinner(Event _event)
    {
        Session race = _event.Sessions.First(s => s.Type == "RAC");
        Classification winner = race.SessionClassification.Classifications.First(c => c.Position == 1);
        return winner.Rider.FullName;
    }

    private object[] Prep(Season[] seasons)
    {
        var events = seasons.SelectMany(season =>
        {
            return season.Events.Select(_event =>
            {
                string raceWinner = GetRaceWinner(_event);
                Session session = _event.Sessions.First(s => s.Type == "RAC");

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