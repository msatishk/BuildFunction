using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;


namespace Shared.Interfaces
{
    public interface IAzureBlobService

    {
        Task MoveBlobToArchiveFolder(string functionName, string connectionString, string sourceContainerName, string destinationContainerName, string directoryPath, string blobName, bool IsSourceFileRequiredToBeDeleted, ILogger log);
        BlobContainerClient GetBlobContainer(string containerName, string connectionString);
        public Task UploadBlobToOPSStorage(string blobContainerName, string blobFullPath, string connectionString, string fileName, string ivuDateTimeWithOffset, string blobFile, ILogger log);

    }


}
