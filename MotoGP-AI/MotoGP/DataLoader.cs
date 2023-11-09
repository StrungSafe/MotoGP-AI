using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MotoGP.Configuration;
using MotoGP.Interfaces;

namespace MotoGP
{
    public class DataLoader : IDataLoader
    {
        private readonly AppSettings settings;

        private readonly ILogger<DataLoader> logger;

        private readonly IDataRepository repo;

        public DataLoader(ILogger<DataLoader> logger,
            IOptions<AppSettings> settings, IDataRepository repo)
        {
            this.logger = logger;
            this.settings = settings.Value;
            this.repo = repo;
        }

        public async Task<IEnumerable<Season>> Load(CancellationToken token)
        {
            Season[] allSeasons = await repo.GetSeasons(token);

            // scrapping after season end and before start of new year will "miss" that season
            IEnumerable<Season> seasons = allSeasons
                                          .Where(s => s.Year < DateTime.Now.Year)
                                          .OrderByDescending(s => s.Year)
                                          .Take(settings.MaxYearsToScrape);

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = -1, //TODO Default for now, no upper bound, http client is throttled
                CancellationToken = token
            };
            await Parallel.ForEachAsync(seasons, options, async (season, seasonToken) =>
            {
                Event[] events = await repo.GetEvents(season.Id, true, seasonToken);

                season.Events.AddRange(events);

                await Parallel.ForEachAsync(season.Events, options, async (_event, eventToken) =>
                {
                    Category[] categories = await repo.GetCategories(season.Id, _event.Id, eventToken);

                    _event.Categories.AddRange(categories);

                    await Parallel.ForEachAsync(_event.Categories, options, async (category, categoryToken) =>
                    {
                        Session[] sessions = await repo.GetSessions(season.Id, _event.Id, category.Id, categoryToken);

                        category.Sessions.AddRange(sessions);

                        await Parallel.ForEachAsync(category.Sessions, options, async (session, sessionToken) =>
                        {
                            SessionClassification sessionClassification =
                                await repo.GetSessionClassification(season.Id, _event.Id, category.Id, session.Id,
                                    false, sessionToken);

                            session.SessionClassification = sessionClassification;
                        });
                    });
                });
            });

            return seasons;
        }
    }
}