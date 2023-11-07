using System.Text.Json.Serialization;

namespace MotoGP.Interfaces
{
    public class SessionClassification
    {
        [JsonPropertyName("classification")]
        public List<Classification> Classifications { get; set; }

        public List<Record> Records { get; set; }
    }
}