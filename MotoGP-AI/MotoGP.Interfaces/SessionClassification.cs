using System.Text.Json.Serialization;

namespace MotoGP.Interfaces
{
    public class SessionClassification
    {
        [JsonPropertyName("classification")]
        public Classification[] Classifications { get; set; }

        public Record[] Records { get; set; }
    }
}