namespace Music.Models;

public class AudioFile
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public byte[] FileData { get; set; }
}