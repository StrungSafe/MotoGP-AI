using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MotoGP.Interfaces;

namespace MotoGP
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

            await Parallel.ForEachAsync(seasons, async (season, seasonToken) =>
            {
                Event[] events = await repo.GetEvents(season.Id, true);

                season.Events.AddRange(events);

                await Parallel.ForEachAsync(season.Events, seasonToken, async (_event, eventToken) =>
                {
                    Category[] categories = await repo.GetCategories(season.Id, _event.Id);

                    _event.Categories.AddRange(categories);

                    await Parallel.ForEachAsync(_event.Categories, eventToken, async (category, categoryToken) =>
                    {
                        Session[] sessions = await repo.GetSessions(season.Id, _event.Id, category.Id);

                        category.Sessions.AddRange(sessions);

                        await Parallel.ForEachAsync(category.Sessions,  categoryToken,async (session, sessionToken) =>
                        {
                            SessionClassification sessionClassification =
                                await repo.GetSessionClassification(season.Id, _event.Id, category.Id, session.Id,
                                    false);

                            session.SessionClassification = sessionClassification;
                        });
                    });
                });
            });

            return seasons;
        }
    }
}