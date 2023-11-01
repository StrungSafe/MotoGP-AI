namespace MotoGP.Scraper
{
    public class Season
    {
        public bool Current { get; set; }

        public List<Event> Events { get; } = new List<Event>();

        public Guid Id { get; set; }

        public int Year { get; set; }
    }
}