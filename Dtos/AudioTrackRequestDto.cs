namespace Music.Dtos;

public class AudioTrackRequest
{
    public int AudioTrackId { get; set; }
    public int UserId { get; set; }
    public string SongName { get; set; }
    public string SongTip { get; set; }
    public string SongKey { get; set; }
    public string SongChords { get; set; }
    public string SongInstrument { get; set; }
    public string SongDifficulty { get; set; }
    public string SongData { get; set; } // Base64 string
}