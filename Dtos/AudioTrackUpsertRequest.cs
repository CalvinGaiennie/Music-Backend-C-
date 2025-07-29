using System.Text.Json;
using System.Text.Json.Serialization;

namespace Music.Dtos;

public class AudioTrackUpsertRequest
{
    public int AudioTrackId { get; set; }
    public string SongName { get; set; } = string.Empty;
    public string SongTip { get; set; } = string.Empty;
    public string SongKey { get; set; } = string.Empty;
    public string SongChords { get; set; } = string.Empty;
    public string SongInstrument { get; set; } = string.Empty;
    public string SongDifficulty { get; set; } = string.Empty;
    public string SongArtist { get; set; } = string.Empty;
    public string SongAlbum { get; set; } = string.Empty;
    public string SongLength { get; set; } = string.Empty;
    public string RecordingQuality { get; set; } = string.Empty;


    [JsonConverter(typeof(Base64ByteArrayConverter))]
    public byte[] SongData { get; set; } = Array.Empty<byte>();
}

public class Base64ByteArrayConverter : JsonConverter<byte[]>
{
    public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? base64String = reader.GetString();
            if (string.IsNullOrEmpty(base64String))
                return Array.Empty<byte>();

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