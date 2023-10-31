namespace Scraper
{
    public class Session
    {
        public Guid Id { get; set; }
        public Condition Condition { get; set; }
        public SessionClassification SessionClassification { get; set; }
        public string Type { get; set; }
        public bool IsInterestingSession => Type.Equals("RACE", StringComparison.CurrentCultureIgnoreCase);
        //Q (1 AND 2), SPR, RACE
    }
}