using System.Text.Json.Serialization;

namespace Scraper
{
    public class SessionClassification
    {
        [JsonPropertyName("classification")]
        public Classification[] Classifications { get; set; }
        public Record[] Records { get; set; }
    }
}