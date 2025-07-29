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
            // Parse the blob URL to get container name and blob name
            var uri = new Uri(blobUrl);
            var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments.Length < 2)
            {
                throw new Exception("Invalid blob URL format");
            }

            var containerName = pathSegments[0];
            var blobName = string.Join("/", pathSegments.Skip(1));

            // Get the container and blob client using the authenticated service client
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

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
            // Parse the blob URL to get container name and blob name
            var uri = new Uri(blobUrl);
            var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments.Length < 2)
            {
                throw new Exception("Invalid blob URL format");
            }

            var containerName = pathSegments[0];
            var blobName = string.Join("/", pathSegments.Skip(1));

            // Get the container and blob client using the authenticated service client
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

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
            // Parse the blob URL to get container name and blob name
            var uri = new Uri(blobUrl);
            var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments.Length < 2)
            {
                return false;
            }

            var containerName = pathSegments[0];
            var blobName = string.Join("/", pathSegments.Skip(1));

            // Get the container and blob client using the authenticated service client
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.ExistsAsync();
            return response.Value;
        }
        catch
        {
            return false;
        }
    }
}