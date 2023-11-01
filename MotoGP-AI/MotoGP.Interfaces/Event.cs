namespace MotoGP.Interfaces
{
    public class Event
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public List<Category> Categories { get; set; } = new List<Category>();

        public bool Test { get; set; }
    }
}