using System.Net.Http.Json;

namespace Scraper
{
    public class Program
    {
        public static async Task Main()
        {
            var program = new Program();
            await program.TryMotoGp();
            Console.WriteLine("Finished");
            Console.ReadKey();
        }

        private async Task TryMotoGp()
        {
            using (var client = new HttpClient())
            {
                client.ConfigureMotoGp();

                var allSeasons = await client.GetFromJson<Season[]>("seasons");
                // scrapping after season end and before start of new year will "miss" that season
                var seasons = allSeasons.Where(s => s.Year < DateTime.Now.Year).Where(s => s.Year >= 2020);

                if(seasons.Any())
                {
                    foreach (var season in seasons)
                    {
                        var allEvents = await client.GetFromJsonAsync<Event[]>($"events?seasonUuid={season.Id}&isFinished={true}");
                        // TODO Ensure events exist

                        var events = allEvents.Where(e => !e.Test);

                        season.Events.AddRange(events);

                        foreach (var _event in season.Events)
                        {
                            var categories = await client.GetFromJson<Category[]>($"categories?eventUuid={_event.Id}");
                            // TODO Ensure categories exist

                            var category = categories.FirstOrDefault(c => c.Name.StartsWith("motogp", StringComparison.CurrentCultureIgnoreCase));

                            if(category == default)
                            {
                                Console.WriteLine("The motogp category was not found...skipping this event");
                                continue;
                            }

                            var allSessions = await client.GetFromJson<Session[]>($"sessions?eventUuid={_event.Id}&categoryUuid={category.Id}");

                            var sessions = allSessions.Where(s => s.IsInterestingSession);

                            _event.Sessions.AddRange(sessions);

                            foreach(var session in _event.Sessions)
                            {
                                var sessionClassification = await client.GetFromJson<SessionClassification>($"session/{session.Id}/classification?test=false");

                                session.SessionClassification = sessionClassification;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No seasons were returned from the API...unable to continue");
                }
            }
        }
    }

    public static class HttpClientExtensions 
    {
        public static void ConfigureMotoGp(this HttpClient client)
        {
            client.BaseAddress = new Uri("https://api.motogp.pulselive.com/motogp/v1/results/", UriKind.Absolute);
        }

        public static Task<T> GetFromJson<T>(this HttpClient client, string relativeUrl)
        {
            return client.GetFromJsonAsync<T>(new Uri(relativeUrl, UriKind.Relative));
        }
    }
}