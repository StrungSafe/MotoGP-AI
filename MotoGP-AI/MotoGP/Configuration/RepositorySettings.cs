namespace MotoGP.Configuration
{
    public class RepositorySettings
    {
        public Uri BaseAddress { get; set; }

        public LocalRepositorySettings LocalRepositorySettings { get; set; }

        public string Name { get; set; }
    }
}