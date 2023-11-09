using System.Text.Json.Serialization;

namespace MotoGP.Data;

public class Rider
{
    [JsonPropertyName("full_name")]
    public string FullName { get; set; }

    public Guid Id { get; set; }
}