using Newtonsoft.Json;

public class Flashcard
{
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Id { get; set; }
    
    [JsonProperty("pl")]
    public string Polish { get; set; }
    
    [JsonProperty("es")]
    public string Spanish { get; set; }
    
    [JsonProperty("level")]
    public int Level { get; set; }
}

