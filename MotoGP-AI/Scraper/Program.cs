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
                client.BaseAddress = new Uri("https://api.motogp.pulselive.com/", UriKind.Absolute);

                var seasons = await client.GetFromJsonAsync<Season[]>(new Uri("motogp/v1/results/seasons", UriKind.Relative));
                
                // TODO Ensure seasons exist

                var events = await client.GetFromJsonAsync<Event[]>(new Uri($"motogp/v1/results/events?seasonUuid={seasons[0].Id}&isFinished={true}", UriKind.Relative));

                // TODO Ensure events exist

                var categories = await client.GetFromJsonAsync<Category[]>(new Uri($"motogp/v1/results/categories?eventUuid={events[0].Id}", UriKind.Relative));

                // TODO Find motogp id

                var sessions = await client.GetFromJsonAsync<Session[]>(new Uri($"motogp/v1/results/sessions?eventUuid={events[0].Id}&categoryUuid={categories[0].Id}", UriKind.Relative));

                // TODO Find Qualy and Race sessions

                var classifications = await client.GetFromJsonAsync<SessionClassification>(new Uri($"motogp/v1/results/session/{sessions[0].Id}/classification?test=false", UriKind.Relative));
            }
        }
    }
}