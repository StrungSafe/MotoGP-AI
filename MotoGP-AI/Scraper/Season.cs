namespace Scraper
{
    public class Season
    {
        public Guid Id { get; set; }
        public int Year { get; set; }
        public bool Current { get; set; }
        public List<Event> Events { get; } = new List<Event>();
    }
}