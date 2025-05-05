using System.Text.Json.Serialization;

public class User
{
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("password")]
    public required string Password { get; set; }
}
