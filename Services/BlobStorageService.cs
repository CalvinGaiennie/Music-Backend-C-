using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Music.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");

        // Handle environment variable substitution manually
        if (connectionString != null)
        {
            var accountName = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_NAME");
            var accountKey = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_KEY");

            if (!string.IsNullOrEmpty(accountName) && !string.IsNullOrEmpty(accountKey))
            {
                connectionString = connectionString
                    .Replace("${AZURE_STORAGE_ACCOUNT_NAME}", accountName)
                    .Replace("${AZURE_STORAGE_ACCOUNT_KEY}", accountKey);
            }
        }

        _blobServiceClient = new BlobServiceClient(connectionString);
        _containerName = configuration.GetSection("BlobStorage:ContainerName").Value ?? "cgmusicblobs";
    }

    public async Task<string> UploadAudioFileAsync(string fileName, byte[] audioData)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            // Generate unique blob name
            var blobName = $"{Guid.NewGuid()}_{fileName}";
            var blobClient = containerClient.GetBlobClient(blobName);

            // Upload the audio data
            using var stream = new MemoryStream(audioData);
            await blobClient.UploadAsync(stream, overwrite: true);

            // Return the blob URL
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to upload audio file: {ex.Message}", ex);
        }
    }

    public async Task<byte[]> DownloadAudioFileAsync(string blobUrl)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(blobUrl));

            var response = await blobClient.DownloadAsync();
            using var stream = new MemoryStream();
            await response.Value.Content.CopyToAsync(stream);

            return stream.ToArray();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to download audio file: {ex.Message}", ex);
        }
    }

    public async Task DeleteAudioFileAsync(string blobUrl)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(blobUrl));
            await blobClient.DeleteIfExistsAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete audio file: {ex.Message}", ex);
        }
    }

    public async Task<bool> AudioFileExistsAsync(string blobUrl)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(blobUrl));
            var response = await blobClient.ExistsAsync();
            return response.Value;
        }
        catch
        {
            return false;
        }
    }
}