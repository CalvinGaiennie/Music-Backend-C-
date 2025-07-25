namespace Music.Dtos;

public class AudioTrackRequest
{
    public int AudioTrackId { get; set; }
    public int UserId { get; set; }
    public string SongName { get; set; } = string.Empty;
    public string SongTip { get; set; } = string.Empty;
    public string SongKey { get; set; } = string.Empty;
    public string SongChords { get; set; } = string.Empty;
    public string SongInstrument { get; set; } = string.Empty;
    public string SongDifficulty { get; set; } = string.Empty;
    public string SongData { get; set; } = string.Empty; // Base64 string
}