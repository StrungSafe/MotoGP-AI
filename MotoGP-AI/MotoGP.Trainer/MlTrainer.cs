using Microsoft.ML;
using MotoGP.Interfaces;

namespace MotoGP.Trainer;

public class MlTrainer : IMlTrainer
{
    public Task<object> TrainModel(Season[] data)
    {
        var context = new MLContext();
        //context.Transforms.Text.FeaturizeText()
        //context.Data.LoadFromEnumerable(Prep(data));
        return null;
    }

    private string GetRaceWinner(Event _event)
    {
        var race = _event.Sessions.First(s => s.Type == "RAC");
        var winner = race.SessionClassification.Classifications.First(c => c.Position == 1);
        return winner.Rider.FullName;
    }

    private object[] Prep(Season[] data)
    {
        Season season = data[0];

        Event _event = season.Events[0];

        Session session = _event.Sessions[0];

        //session.Condition.Air
        // session.Condition.Ground
        //session.Condition.Humidity
        //session.Condition.Track
        //session.Condition.Weather

        var raceWinner = GetRaceWinner(_event);

        var train = new
        {
            year = (float)season.Year,
            trackName = _event.Name,
            track = session.Condition.Track, // string
            air = float.Parse(session.Condition.Air.Replace('\u00BA', '')), // degrees
            humidity = float.Parse(session.Condition.Humidity.Replace("%", string.Empty)), // percentage
            ground = float.Parse(session.Condition.Ground.Replace('\u00BA', '')), // degrees
            weather = session.Condition.Weather, // string
            raceWinner // string
        };

        return new[]{ train };
    }
}