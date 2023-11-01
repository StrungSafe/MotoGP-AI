namespace Scraper
{
    public class Event
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public List<Session> Sessions { get; } = new List<Session>();

        public bool Test { get; set; }
    }
}