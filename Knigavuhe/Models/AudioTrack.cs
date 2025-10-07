namespace Knigavuhe.Models;

using Newtonsoft.Json;

public class AudioTrack
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; } = null!;

    [JsonProperty("url")]
    public string Url { get; set; } = null!;

    [JsonProperty("player_data")]
    public PlayerData PlayerData { get; set; } = new();

    [JsonProperty("duration")]
    public double Duration { get; set; }
}

public class PlayerData
{
    [JsonProperty("title")]
    public string Title { get; set; } = null!;

    [JsonProperty("cover")]
    public string Cover { get; set; } = null!;

    [JsonProperty("cover_type")]
    public string CoverType { get; set; } = null!;

    [JsonProperty("readers")]
    public string Readers { get; set; } = null!;

    [JsonProperty("series")]
    public string Series { get; set; } = null!;
}