namespace Music.Models;

public class AudioFile
{
    public int AudioFileId { get; set; }
    public int UserId { get; set; }
    public string FileName { get; set; }
    public byte[] FileData { get; set; }
}