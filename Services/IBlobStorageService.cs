namespace Music.Services;

public interface IBlobStorageService
{
    Task<string> UploadAudioFileAsync(string fileName, byte[] audioData);
    Task<byte[]> DownloadAudioFileAsync(string blobUrl);
    Task DeleteAudioFileAsync(string blobUrl);
    Task<bool> AudioFileExistsAsync(string blobUrl);
}