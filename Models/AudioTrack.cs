using System.Text.Json;
using System.Text.Json.Serialization;

namespace Music.Models;

public class AudioTrack
{
    public int AudioTrackId { get; set; }
    public int UserId { get; set; }
    public string SongName { get; set; }
    public string SongTip { get; set; }
    public string SongKey { get; set; }
    public string SongChords { get; set; }
    public string SongInstrument { get; set; }
    public string SongDifficulty { get; set; }

    [JsonConverter(typeof(Base64ByteArrayConverter))]
    public byte[] SongData { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Base64ByteArrayConverter : JsonConverter<byte[]>
{
    public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string base64String = reader.GetString();
            if (string.IsNullOrEmpty(base64String))
                return new byte[0];

            try
            {
                return Convert.FromBase64String(base64String);
            }
            catch (FormatException)
            {
                throw new JsonException("Invalid Base64 string format");
            }
        }

        throw new JsonException("Expected string for byte array");
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        if (value == null || value.Length == 0)
        {
            writer.WriteStringValue("");
        }
        else
        {
            writer.WriteStringValue(Convert.ToBase64String(value));
        }
    }
}