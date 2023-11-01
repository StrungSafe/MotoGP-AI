using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MotoGP.Scraper
{
    public class MotoGpScraper : IScraper
    {
        private readonly IHttpClientFactory clientFactory;

        private readonly IConfiguration configuration;

        private readonly ILogger<MotoGpScraper> logger;

        public MotoGpScraper(ILogger<MotoGpScraper> logger, IHttpClientFactory clientFactory,
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

                    Category? category = categories.FirstOrDefault(c =>
                        c.Name.StartsWith("motogp", StringComparison.CurrentCultureIgnoreCase));

                    if (category == default)
                    {
                        logger.LogWarning("The motogp category was not found...skipping the event {eventName}",
                            _event.Name);
                        continue;
                    }

                    Session[] allSessions =
                        await client.GetFromJson<Session[]>(
                            $"sessions?eventUuid={_event.Id}&categoryUuid={category.Id}");

                    string[] captureSessions = { "Q", "RAC", "RACE" };
                    IEnumerable<Session> sessions = allSessions.Where(s =>
                        captureSessions.Any(t => string.Equals(t, s.Type, StringComparison.CurrentCultureIgnoreCase)));

                    _event.Sessions.AddRange(sessions);

                    foreach (Session session in _event.Sessions)
                    {
                        var sessionClassification =
                            await client.GetFromJson<SessionClassification>(
                                $"session/{session.Id}/classification?test=false");

                        session.SessionClassification = sessionClassification;
                    }
                }
            }));

            return seasons;
        }
    }
}