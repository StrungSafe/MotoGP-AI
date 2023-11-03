using MotoGP.Interfaces;

namespace MotoGP;

public interface IDataRepository
{
    Task<Category[]> GetCategories(Guid seasonId, Guid eventId);

    Task<Event[]> GetEvents(Guid seasonId, bool isFinished);

    Task<Season[]> GetSeasons();

    Task<SessionClassification> GetSessionClassification(Guid seasonId, Guid eventId, Guid categoryId, Guid sessionId,
        bool test);

    Task<Session[]> GetSessions(Guid seasonId, Guid eventId, Guid categoryId);
}