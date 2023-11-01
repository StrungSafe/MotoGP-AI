using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MotoGP.Interfaces;

namespace MotoGP.Scraper
{
    public class DataScraper : IDataScraper
    {
        private readonly IHttpClientFactory clientFactory;

        private readonly IConfiguration configuration;

        private readonly ILogger<DataScraper> logger;

        public DataScraper(ILogger<DataScraper> logger, IHttpClientFactory clientFactory,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.clientFactory = clientFactory;
            this.configuration = configuration;
        }

        public async Task<IEnumerable<Season>> Scrape()
        {
            using HttpClient client = clientFactory.CreateClient(configuration["MotoGP:Name"]);
            Season[] allSeasons = await client.GetFromJson<Season[]>("seasons");
            // scrapping after season end and before start of new year will "miss" that season
            IEnumerable<Season> seasons = allSeasons
                                          .Where(s => s.Year < DateTime.Now.Year)
                                          .OrderByDescending(s => s.Year)
                                          .Take(configuration.GetValue<int>("MaxYearsToScrape"));

            await Task.WhenAll(seasons.Select(async season =>
            {
                Event[]? allEvents =
                    await client.GetFromJsonAsync<Event[]>($"events?seasonUuid={season.Id}&isFinished=true");

                IEnumerable<Event> events = allEvents.Where(e => !e.Test);

                season.Events.AddRange(events);

                foreach (Event _event in season.Events)
                {
                    Category[] categories = await client.GetFromJson<Category[]>($"categories?eventUuid={_event.Id}");

                    foreach (Category category in categories)
                    {
                        Session[] allSessions =
                            await client.GetFromJson<Session[]>(
                                $"sessions?eventUuid={_event.Id}&categoryUuid={category.Id}");
                    
                        IEnumerable<Session> sessions = allSessions;

                        category.Sessions.AddRange(sessions);

                        foreach (Session session in category.Sessions)
                        {
                            var sessionClassification =
                                await client.GetFromJson<SessionClassification>(
                                    $"session/{session.Id}/classification?test=false");

                            session.SessionClassification = sessionClassification;
                        }
                    }
                }
            }));

            return seasons;
        }
    }
}