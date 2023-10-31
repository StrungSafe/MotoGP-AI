namespace Scraper
{
    public class Event
    {
        public string Name { get; set; }
        public Season Season { get; set; }
        public Guid Id { get; set; }
        public bool Test { get; set; }
        public List<Session> Sessions { get; } = new List<Session>();
    }
}