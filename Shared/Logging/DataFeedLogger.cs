using System;
using System.IO;
using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace DataFeedLogger
{
    public class DataFeedLogger
    {

        public static void LogMessage(ILogger log, string functionName, string message, string containerName, string ext)
        {
            try
            {
                ValidateAppSettings(out string blobConnectionString, containerName);

                // Create a BlobServiceClient using the storage account connection string
                BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionString);

                // Get a reference to the container
                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // Create the container if it doesn't exist
                blobContainerClient.CreateIfNotExists();

                DateTime currentDateTime = GetESTDateTime(DateTime.UtcNow); // Converts current time in utc to EST/ET timezone 

                // Generate log file name using the function name and current date
                string logFileName = $"{functionName}-{currentDateTime.ToString("yyyyMMdd-HHmmss.ffff")}.{ext}";

                string filePath = (Path.Combine(functionName, currentDateTime.ToString("yyyy-MM-dd"), logFileName)).Replace("\\", "/");

                // Get a reference to the log file after combining path to file name
                BlobClient blobClient = blobContainerClient.GetBlobClient(filePath);

                // Check if the blob already exists
                if (!blobClient.Exists())
                {
                    // If the blob doesn't exist, upload data
                    using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(message)))
                    {
                        blobClient.Upload(stream, overwrite: false);
                    }
                    log.LogInformation($"[{logFileName}] Message logged successfully.");
                }else{
                    string errorMsg = "Error: Conflict - Blob creation failed. A blob with the specified identifier already exists.";
                    string errorDetails = $"\nFile Name: {logFileName}\nFile Path: {filePath}\nTimestamp: {currentDateTime.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}";
                    log.LogError(0,errorMsg+errorDetails);
                }    
            }
            catch (Exception ex)
            {
                try
                {
                    log.LogError(0, ex, $"DataFeedLogger[{functionName}] exception");
                }
                catch (Exception e2)
                {
                    // DO Nothing
                }
            }
        }

        private static void ValidateAppSettings(out string blobConnectionString, string containerName)
        {
            blobConnectionString = Environment.GetEnvironmentVariable("AzureBlobStorageConnectionString");
            if (string.IsNullOrEmpty(blobConnectionString))
            {
                throw new Exception("Please set the environment variable: AzureBlobStorageConnectionString");
            }

            if (string.IsNullOrEmpty(containerName))
            {
                throw new Exception("Please set the parameter variable: 'containerName'. LogMessage(, , , containerName, )");
            }
        }

        public static DateTime GetESTDateTime(DateTime utcTime)
        {
            TimeZoneInfo easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, easternTimeZone);
        }

    }
}
