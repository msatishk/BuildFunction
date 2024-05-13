using System;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using ZedasVehicleConsist;
using Shared.Utils;
using System.Net;
using Shared;

namespace ToZedasVehicleConsist
{
    public class ToZedasVehicleConsist
    {
        private const string functionName = "ToZedasVehicleConsist";

        [FunctionName(functionName)]
        public static async Task Run(
            [ServiceBusTrigger("%IVUVehicleConsistTopicName%", "%ZedasVehicleConsistSubscriptionName%",
                Connection = "ServiceBusConnectionString")]
            ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, ILogger log)
        {
            bool networkOrDataError = false;
            Exception exception = null;
            IntfJobsPushPortTypeClient client = null;

            try
            {
                log.LogInformation($"{functionName} Message ID: {message.MessageId}");
                log.LogInformation($"{functionName} Message Content-Type: {message.ContentType}");
                string username = await Utils.GetSecret("ToZedasFromIVU-Username", log);
                string password = await Utils.GetSecret("ToZedasFromIVU-Password", log);

                ValidateAppSettings(out string serviceUrl, out string topicLogEnable, out string containerName,
                    out string maxRetries, out string pauseBetweenFailures);
                client = GetIntfJobsPushPortTypeClient(username, password, serviceUrl);
                string xmlString = Encoding.UTF8.GetString(message.Body);
                VehicleGroupExport vehicleGroupExport = DeserializeFromXmlString<VehicleGroupExport>(xmlString);
                Utils.ExecuteArchiveLog(log, topicLogEnable, functionName, xmlString, containerName, "xml");

                Utils.Execute(() => client.sendIntfJobsExport(vehicleGroupExport), log,
                        Convert.ToInt32(maxRetries), TimeSpan.FromSeconds(Convert.ToInt16(pauseBetweenFailures)));
            }
            catch (Exception ex)
            {
                networkOrDataError = true;
                log.LogError(0, ex, $"{functionName} exception");
                client?.Abort();
                exception = ex;
            }
            finally
            {
                if (client != null && client.State == CommunicationState.Opened)
                    client.Close();
                if (networkOrDataError)
                    await NotifyNetworkOrDataError(exception, log);
                log.LogInformation($"The {functionName} function was completed.");
            }
        }

        private static async Task NotifyNetworkOrDataError(Exception ex, ILogger log)
        {
            await Send_Email_Notifications_HelpDesk_IVU_Zedas(ex != null ? ex.ToString() : "", log);
            await Send_Email_Notifications_Users_IVU_Zedas(ex != null ? ex.ToString() : "", log);
        }
        
        private static async Task Send_Email_Notifications_HelpDesk_IVU_Zedas(string error, ILogger log)
        {
            EmailNotificationData email = new EmailNotificationData
            {
                importance = Utils.VerifyAppSettingString("Notifications_IVU_Zedas_EmailNotificationImportance"),
                subject = Utils.VerifyAppSettingString("Notifications_IVU_Zedas_EmailNotificationsubject").Replace("{0}", functionName),
                email_body = Utils.VerifyAppSettingString("Notifications_IVU_Zedas_EmailNotificationBody").Replace("{0}", functionName).Replace("{1}", error),
                to = Utils.VerifyAppSettingString($"Notifications_HelpDesk_IVU_Zedas_EmailTo"),
            };
            string EmailNotificationURL = Utils.VerifyAppSettingString("EmailNotificationLogicAppURL");
            await Utils.SendMessageNotification(EmailNotificationURL, email, log);
        }
        private static async Task Send_Email_Notifications_Users_IVU_Zedas(string error, ILogger log)
        {
            EmailNotificationData email = new EmailNotificationData
            {
                importance = Utils.VerifyAppSettingString("Notifications_IVU_Zedas_EmailNotificationImportance"),
                subject = Utils.VerifyAppSettingString("Notifications_IVU_Zedas_EmailNotificationsubject").Replace("{0}", functionName),
                email_body = Utils.VerifyAppSettingString("Notifications_IVU_Zedas_EmailNotificationBody").Replace("{0}", functionName).Replace("{1}", error),
                to = Utils.VerifyAppSettingString($"Notifications_Users_IVU_Zedas_EmailTo"),
            };
            string EmailNotificationURL = Utils.VerifyAppSettingString("EmailNotificationLogicAppURL");
            await Utils.SendMessageNotification(EmailNotificationURL, email, log);
        }

        private static IntfJobsPushPortTypeClient GetIntfJobsPushPortTypeClient(string username, string password, string serviceUrl)
        {
            BasicHttpsBinding binding = GetBindingSettings();
            EndpointAddress endpointAddress = new EndpointAddress(serviceUrl);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            IntfJobsPushPortTypeClient client = new IntfJobsPushPortTypeClient(binding, endpointAddress);
            client.ClientCredentials.UserName.UserName = username;
            client.ClientCredentials.UserName.Password = password;
            return client;
        }

        private static void ValidateAppSettings(out string serviceUrl, out string topicLogEnable, out string containerName, out string maxRetries, out string pauseBetweenFailures)
        {
            serviceUrl = Utils.VerifyAppSettingString("ToZedasVehicleConsist_APIUrl");
            topicLogEnable = Utils.VerifyAppSettingString("ToZedasVehicleConsistLogEnable");
            containerName = Utils.VerifyAppSettingString("AzureBlobStorageONXArchiveContainerName");
            maxRetries = Utils.VerifyAppSettingString("IVU_Zedas_MaxRetries");
            pauseBetweenFailures = Utils.VerifyAppSettingString("IVU_Zedas_TimeIntervalBetweenFailures");
        }
        public static T DeserializeFromXmlString<T>(string xmlString)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            using MemoryStream memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlString));
            T deserializedObject = (T)xs.Deserialize(memoryStream);
            return deserializedObject;
        }

        private static BasicHttpsBinding GetBindingSettings()
        {
            BasicHttpsBinding binding = new BasicHttpsBinding();
            binding.MaxBufferPoolSize = int.MaxValue;
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.Security.Mode = BasicHttpsSecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            return binding;
        }
    }
}
