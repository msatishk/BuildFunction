using IVUWS;
using Microsoft.Extensions.Logging;
using Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using ToIVUMultipleFromOracle.Services;

namespace ToIVUMultipleFromOracle.Models
{
    public class IVUPayloadSettings
    {
        public IVUServiceWrapper GetIVUServiceWrapper()
        {
            return new IVUServiceWrapper(this);
        }
        public IVUServiceWrapper iVUServiceWrapper { get; set; }
        public string IVUFileTypeCode { get; set; }
        public int PersonnelDataCount { get; set; }
        public ILogger log { get; set; }
        public string IVUEndpoint { get; init; } = Utils.VerifyAppSettingString("ToIVUMultipleFromOracle_IVUServerEndpoint");
        public string Username { get; set; }
        public string Password { get; set; } 
        public string SourceContainerName { get; init; } = Utils.VerifyAppSettingString("AzureBlobStorageONXContainerName");
        public string DestinationContainerName { get; init; } = Utils.VerifyAppSettingString("AzureBlobStorageONXArchiveContainerName");
        public string ConnectionString { get; init; } = Utils.VerifyAppSettingString("AzureBlobStorageConnectionString");
        public string BlobVirtualPath { get; init; } = Utils.VerifyAppSettingString("ToIVUMultipleFromOracle_BlobVirtualPath");
        public bool IsArchiveofBlobRequired { get; init; } = Convert.ToBoolean(
                                   Utils.VerifyAppSettingString("ToIVUMultipleFromOracle_IsArchiveOfBlobRequired"));
        public bool IsSourceFileRequiredTobeDeleted { get; init; } = Convert.ToBoolean(
                                                           Utils.VerifyAppSettingString("ToIVUMultipleFromOracle_IsSourceFileRequiredToBeDeleted"));
        public string FileList { get; init; } = Utils.VerifyAppSettingString("FromOracleMultipleForIVU_Params");
        public string MaxRetries { get; init; } = Utils.VerifyAppSettingString("ToIVUMultipleFromOracle_MaxRetries");
        public string ThreadWaitTimeToHold1 { get; init; } = Utils.VerifyAppSettingString("ToIVUMultipleFromOracle_ThreadWaitTimeToHold1");
        public string ThreadWaitTimeToHold2 { get; init; } = Utils.VerifyAppSettingString("ToIVUMultipleFromOracle_ThreadWaitTimeToHold2");
        public string UsersEmailTo { get; init; } = Utils.VerifyAppSettingString("Notifications_Users_IVU_Oracle_EmailTo");
        public string HelpDeskEmailTo { get; init; } = Utils.VerifyAppSettingString("Notifications_HelpDesk_IVU_Oracle_EmailTo");
        public string EmailNotificationImportance { get; init; } = Utils.VerifyAppSettingString("Notifications_Oracle_IVU_EmailNotificationImportance");
        public string EmailNotificationSubject { get; init; } = Utils.VerifyAppSettingString("ToIVUMultipleFromOracle_EmailNotificationSubject");
        public string EmailNotificationBody { get; init; } = Utils.VerifyAppSettingString("ToIVUMultipleFromOracle_EmailNotificationBody");
        public string LogicAppURL { get; init; } = Utils.VerifyAppSettingString("EmailNotificationLogicAppURL");
        public string FunctionName { get; set; }
        public bool NetworkError {  get; set; }
    }
}
