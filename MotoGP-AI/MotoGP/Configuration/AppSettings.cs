namespace MotoGP.Configuration
{
    public class AppSettings
    {
        public Uri EventsPath { get; set; }

        public Uri FilePath { get; set; }

        public int MaxDegreeOfParallelism { get; set; }

        public int MaxYearsToScrape { get; set; }

        public Uri RidersPath { get; set; }
    }
}