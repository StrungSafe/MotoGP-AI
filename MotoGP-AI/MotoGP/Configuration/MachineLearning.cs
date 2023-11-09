namespace MotoGP.Configuration
{
    public class MachineLearning
    {
        public Uri Models { get; set; }

        public Uri Objects { get; set; }
        public double TestFraction { get; set; }
        public int Seed { get; set; }
    }
}