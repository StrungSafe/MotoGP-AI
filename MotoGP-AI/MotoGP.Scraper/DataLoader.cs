using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MotoGP.Interfaces;

namespace MotoGP.Scraper
{
    public class DataLoader : IDataLoader
    {
        private readonly IConfiguration configuration;

        private readonly ILogger<DataLoader> logger;

        private readonly IDataRepository repo;

        public DataLoader(ILogger<DataLoader> logger,
            IConfiguration configuration, IDataRepository repo)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.repo = repo;
        }

        public async Task<IEnumerable<Season>> Load()
        {
            Season[] allSeasons = await repo.GetSeasons();
            // scrapping after season end and before start of new year will "miss" that season
            IEnumerable<Season> seasons = allSeasons
                                          .Where(s => s.Year < DateTime.Now.Year)
                                          .OrderByDescending(s => s.Year)
                                          .Take(configuration.GetValue<int>("MaxYearsToScrape"));

            await Task.WhenAll(seasons.Select(async season =>
            {
                Event[] allEvents = await repo.GetEvents(season.Id, true);

                IEnumerable<Event> events = allEvents.Where(e => !e.Test);

                season.Events.AddRange(events);

                foreach (Event _event in season.Events)
                {
                    Category[] allCategories = await repo.GetCategories(season.Id, _event.Id);

                    Category[] categories = allCategories;

                    _event.Categories.AddRange(categories);

                    foreach (Category category in categories)
                    {
                        Session[] allSessions = await repo.GetSessions(season.Id, _event.Id, category.Id);

                        IEnumerable<Session> sessions = allSessions;

                        category.Sessions.AddRange(sessions);

                        foreach (Session session in category.Sessions)
                        {
                            SessionClassification sessionClassification =
                                await repo.GetSessionClassification(season.Id, _event.Id, category.Id, session.Id,
                                    false);

                            session.SessionClassification = sessionClassification;
                        }
                    }
                }
            }));

            return seasons;
        }
    }
}