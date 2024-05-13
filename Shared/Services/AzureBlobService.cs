using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Shared.Interfaces;
using System.Text;
namespace Shared.Services
{
    public class AzureBlobService : IAzureBlobService
    {

        public BlobContainerClient GetBlobContainer(string containerName, string connectionString)
        {
            BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);
            return containerClient;
        }


        public async Task MoveBlobToArchiveFolder(string functionName, string connectionString, string sourceContainerName, string destinationContainerName, string directoryPath, string blobName, bool IsSourceFileRequiredToBeDeleted, ILogger log)
        {
            try
            {
                var sourceContainerClient = GetBlobContainer(sourceContainerName, connectionString);
                string blobAbsolutePath = $"{directoryPath}{blobName}";
                var sourceBlob = sourceContainerClient.GetBlobClient(blobAbsolutePath);
                var destinationContainerClient = GetBlobContainer(destinationContainerName, connectionString);
                DateTime currentDateTime = DataFeedLogger.DataFeedLogger.GetESTDateTime(DateTime.UtcNow);
                string fileName = $"{Path.GetFileNameWithoutExtension(blobName)}-{currentDateTime.ToString(Constants.Constants.FileNameTimeStampFormat)}{Path.GetExtension(blobName)}";
                string destinationBlobPath = (Path.Combine(functionName, currentDateTime.ToString(Constants.Constants.FolderNameTimeStampFormat), fileName)).Replace("\\", "/");

                var destinationBlob = destinationContainerClient.GetBlobClient(destinationBlobPath);
                if (await sourceBlob.ExistsAsync())
                {
                    CopyFromUriOperation response = await destinationBlob.StartCopyFromUriAsync(sourceBlob.Uri);
                    if (response != null && response.GetRawResponse().Status == 202)
                    {
                        log.LogInformation($"Shared - MoveBlobToArchiveFolder, blob with name {blobAbsolutePath} is successfully copied to destination container :{destinationContainerName}");
                        if (IsSourceFileRequiredToBeDeleted)
                        {
                            bool deleteStatus = await sourceBlob.DeleteIfExistsAsync();
                            if (deleteStatus)
                            {
                                log.LogInformation($"Shared - MoveBlobToArchiveFolder, blob with name: {blobAbsolutePath} is successfully deleted from source container :{sourceContainerName}");
                            }
                            else
                            {
                                log.LogError($"Shared - MoveBlobToArchiveFolder, Failed to delete the blob with name: {blobAbsolutePath} from source container :{sourceContainerName}");
                                throw new ArgumentNullException($"OracleCommon - MoveBlobToArchiveFolder, Failed to copy the blob with name: {blobAbsolutePath} from source container :{sourceContainerName}");
                            }
                        }
                        else
                        {
                            log.LogInformation($"Shared - MoveBlobToArchiveFolder,Skipping deletion of blob from source container as per selection");
                        }
                    }
                    else
                    {
                        log.LogError($"Shared - MoveBlobToArchiveFolder, Failed to copy the source blob with name: {blobAbsolutePath} to destination container {destinationContainerName}");
                        throw new ArgumentNullException($"OracleCommon - MoveBlobToArchiveFolder, Failed to copy the source blob with name: {blobAbsolutePath} to destination container {destinationContainerName}");
                    }
                }
                else
                {
                    log.LogError($"Shared - MoveBlobToArchiveFolder, blob with name {blobAbsolutePath} doesn't exist in source container: {sourceContainerName}");
                    throw new ArgumentNullException($"OracleCommon - MoveBlobToArchiveFolder, blob with name {blobAbsolutePath} doesn't exist in source container : {sourceContainerName}");
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Shared - MoveBlobToArchiveFolder, Exception during Archive of blob with name : {blobName} and message :{ex.Message}");
                throw;
            }

        }

        /// <summary>
        /// Checks every container in blob directory path and creates it if it doesn't exist
        /// </summary>
        /// <param name="blobFullPath">Blob Container Name</param>
        /// <param name="connectionString">Blob Storage Connection String</param>
        /// <param name="log">Logger</param>
        /// <returns></returns>
        public async Task UploadBlobToOPSStorage(string blobContainerName, string blobFullPath, string connectionString, string fileName, string ivuDateTimeWithOffset, string blobFile, ILogger log)
        {
            BlobContainerClient containerClient = GetBlobContainer(blobContainerName, connectionString);
            if (!containerClient.Exists())
            {
                log.LogError($"UploadBlobToOPSStorage, Blob Container: {blobContainerName} does not exist.");
                throw new ArgumentNullException($"UploadBlobToOPSStorage, Blob Container: {blobContainerName} does not exist.");
            }
            DateTime currentDateTime = DataFeedLogger.DataFeedLogger.GetESTDateTime(DateTime.UtcNow);
            TimeZoneInfo easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var utcOffset = easternTimeZone.GetUtcOffset(currentDateTime);
            var currentDateTimeWithOffset = currentDateTime.ToString(ivuDateTimeWithOffset) + ((utcOffset < TimeSpan.Zero) ? "-" : "+") + utcOffset.ToString("hh':'mm");
            fileName = $"{Path.GetFileNameWithoutExtension(fileName)}-{currentDateTimeWithOffset}{Path.GetExtension(fileName)}";
            string blobAbsolutePath = $"{blobFullPath}{fileName}";
            BlobClient opsblobClient = containerClient.GetBlobClient(blobAbsolutePath);
            byte[] bytes = Encoding.UTF8.GetBytes(blobFile);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                var uploadresponse = opsblobClient.Upload(stream, overwrite: false).GetRawResponse();
                var uploadstatus = uploadresponse.ReasonPhrase;
                if (uploadstatus != null && uploadstatus.ToLower() == "created")
                {
                    log.LogInformation($"UploadBlobToOPSStorage, File Uploaded to OPSStorage folder successfully");
                }
                else
                {
                    log.LogError($"UploadBlobToOPSStorage, Unable to Upload file to  {blobAbsolutePath}");
                    throw new Exception($"UploadBlobToOPSStorage, Unable to upload file to {blobAbsolutePath}");
                }
            }
            log.LogInformation($"UploadBlobToOPSStorage, blob with name: {blobAbsolutePath} is successfully uploaded to container :{blobContainerName}");
        }
    }
}