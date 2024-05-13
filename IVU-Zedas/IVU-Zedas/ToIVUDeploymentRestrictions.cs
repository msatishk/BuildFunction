using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs.ServiceBus;
using System.Xml.Serialization;
using FromZedasDeploymentRestrictions.IVUZedas;
using Shared.Utils;
using System.Net;
using Shared;


namespace ToIVUDeploymentRestrictions
{
    public static class ToIVUDeploymentRestrictions
    {
        private const string functionName = "ToIVUDeploymentRestrictions";

        [FunctionName(functionName)]
        public static async Task Run([ServiceBusTrigger("%DeploymentRestrictionsTopicName%", "%DeploymentRestrictionsSubscriptionName%", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, ILogger log)
        {
            Exception exception = null;
            ErrorObject eo = new ErrorObject();
            DeploymentRestrictionServicePortTypeClient client = null;
            try
            {
                string username = await Utils.GetSecret("ToIVUDeploymentRestrictions-ServiceUsername", log);
                string password = await Utils.GetSecret("ToIVUDeploymentRestrictions-ServicePassword", log);
                ValidateAppSettings(out string topicLogEnable, out string containerName, out string serviceUrl);
                string xmlString = Encoding.UTF8.GetString(message.Body);
                Utils.ExecuteArchiveLog(log, topicLogEnable, functionName, xmlString, containerName, "xml");
                //    string firstLine = xmlString.Split('\n')[1];
                client = GetTimeInformationImportFacade(serviceUrl, username, password);

                if (xmlString.StartsWith("<createDeploymentRestrictionRequest"))
                {
                    ProcessRequest<createDeploymentRestrictionRequest>("create", log, message, serviceUrl, client, eo);
                }

                else if (xmlString.StartsWith("<modifyDeploymentRestrictionRequest"))
                {
                    ProcessRequest<modifyDeploymentRestrictionRequest>("modify", log, message, serviceUrl, client, eo);
                }

                else if (xmlString.StartsWith("<deleteDeploymentRestrictionRequest"))
                {
                    ProcessRequest<deleteDeploymentRestrictionRequest>("delete", log, message, serviceUrl, client, eo);
                }
                else
                {
                    throw new Exception("Input did not match any of the method");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"{functionName} Error :" + " {error}", ex.Message);
                client?.Abort();
                exception = ex;
            }
            finally
            {
                if (client != null && client.State == CommunicationState.Opened)
                    client.Close();
                if (eo.NetworkError)
                    await NotifyNetworkError(exception, log);
                log.LogInformation($"The {functionName} function was completed.");
            }
        }

        private static async Task NotifyNetworkError(Exception ex, ILogger log)
        {
            await Send_Email_Notifications_HelpDesk_IVU_Zedas(ex != null ? ex.ToString() : "", log);
            await Send_Email_Notifications_Users_IVU_Zedas(ex != null ? ex.ToString() : "", log);
        }

        public static DeploymentRestrictionServicePortTypeClient GetTimeInformationImportFacade(string serviceUrl, string username, string password)
        {
            EndpointAddress endpointAddress = new EndpointAddress(serviceUrl);
            BasicHttpsBinding binding = new BasicHttpsBinding();
            binding.MaxBufferPoolSize = int.MaxValue;
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.Security.Mode = BasicHttpsSecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            DeploymentRestrictionServicePortTypeClient client = new DeploymentRestrictionServicePortTypeClient(binding, endpointAddress);
            client.ClientCredentials.UserName.UserName = username;
            client.ClientCredentials.UserName.Password = password;
            return client;
        }

        private static void ValidateAppSettings(out string topicLogEnable, out string containerName, out string serviceUrl)
        {
            serviceUrl = Utils.VerifyAppSettingString("ToIVUDeploymentRestrictions_ServiceUrl");
            topicLogEnable = Utils.VerifyAppSettingString("ToIVUDeploymentRestrictionsLogEnable");
            containerName = Utils.VerifyAppSettingString("AzureBlobStorageONXArchiveContainerName");
        }

        public static void ProcessRequest<T>(string type, ILogger log, ServiceBusReceivedMessage message, string serviceUrl, DeploymentRestrictionServicePortTypeClient client, ErrorObject obj) where T : class
        {
            log.LogInformation($"{functionName} Running " + type + "DeploymentRestrictionRequest");
            T envelope = default(T);
            log.LogInformation($"{functionName}" + " Message ID: {id}", message.MessageId);
            string maxRetries = Utils.VerifyAppSettingString("IVU_Zedas_DeploymentRestrictions_MaxRetries");
            string pauseBetweenFailures = Utils.VerifyAppSettingString("IVU_Zedas_TimeIntervalBetweenFailures");
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (StringReader reader = new StringReader(Encoding.UTF8.GetString(message.Body)))
            {
                envelope = (T)serializer.Deserialize(reader);
            }

            string text = null;

            int result = 1;
            if (envelope is createDeploymentRestrictionRequest)
            {
                createDeploymentRestrictionRequest createEnvelope = envelope as createDeploymentRestrictionRequest;
                try
                {
                    result = Utils.Execute(
                        () => client.createDeploymentRestriction(createEnvelope.division, createEnvelope.vehicle,
                            createEnvelope.startTime, createEnvelope.endTime, createEnvelope.reason,
                            createEnvelope.comment, createEnvelope.user, createEnvelope.serializedKey, out text), log,
                        Convert.ToInt32(maxRetries), TimeSpan.FromSeconds(Convert.ToInt16(pauseBetweenFailures)));
                }
                catch
                {
                    obj.NetworkError = true;
                    throw;
                }
            }
            else if (envelope is modifyDeploymentRestrictionRequest)
            {
                modifyDeploymentRestrictionRequest modifyEnvelope = envelope as modifyDeploymentRestrictionRequest;
                try
                {

                    result = Utils.Execute(() => client.modifyDeploymentRestriction(modifyEnvelope.division,
                    modifyEnvelope.vehicle, modifyEnvelope.startTime, modifyEnvelope.endTime, modifyEnvelope.reason,
                    modifyEnvelope.comment, modifyEnvelope.user, modifyEnvelope.serializedKey, out text), log,
                    Convert.ToInt32(maxRetries), TimeSpan.FromSeconds(Convert.ToInt16(pauseBetweenFailures)));
                }
                catch
                {
                    obj.NetworkError = true;
                    throw;
                }
            }
            else if (envelope is deleteDeploymentRestrictionRequest)
            {
                deleteDeploymentRestrictionRequest deleteEnvelope = envelope as deleteDeploymentRestrictionRequest;
                try
                {
                    result = Utils.Execute(
                        () => client.deleteDeploymentRestriction(deleteEnvelope.division, deleteEnvelope.vehicle,
                            deleteEnvelope.startTime, deleteEnvelope.endTime, deleteEnvelope.reason,
                            deleteEnvelope.comment, deleteEnvelope.user, deleteEnvelope.serializedKey, out text), log,
                        Convert.ToInt32(maxRetries), TimeSpan.FromSeconds(Convert.ToInt16(pauseBetweenFailures)));
                }
                catch
                {
                    obj.NetworkError = true;
                    throw;
                }
            }
            if (result != 0)
            {
                log.LogError($"{functionName} Error sending message: Errorcode : " + result + ", Errormsg:" + text);
            }
            log.LogInformation($"The {functionName} was Completed");
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
    }

    public class ErrorObject
    {
        public bool NetworkError { get; set; }
    }
}


