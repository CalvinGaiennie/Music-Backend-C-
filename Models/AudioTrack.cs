namespace Music.Models;

public class AudioTrack
{
    public int AudioTrackId { get; set; }
    public int UserId { get; set; }
    public string SongName { get; set; } = string.Empty;
    public string SongTip { get; set; } = string.Empty;
    public string SongKey { get; set; } = string.Empty;
    public string SongChords { get; set; } = string.Empty;
    public string SongInstrument { get; set; } = string.Empty;
    public string SongDifficulty { get; set; } = string.Empty;
    public string SongBlobUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}