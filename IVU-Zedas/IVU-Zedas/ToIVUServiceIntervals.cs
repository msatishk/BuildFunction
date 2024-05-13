using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using IVUServiceIntervals;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs.ServiceBus;
using System.Xml.Serialization;
using Shared.Utils;
using System.Net;
using Shared;
namespace ToIVUServiceIntervals
{
    public static class ToIVUServiceIntervals
    {
        private const string functionName = "ToIVUServiceIntervals";

        [FunctionName(functionName)]
        public static async Task Run([ServiceBusTrigger("%ServiceIntervalsTopicName%", "%ServiceIntervalsSubscriptionName%", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, ILogger log)
        {
            bool NetworkError = false;
            Exception exception = null;
            VehicleWebFacadeClient client = null;
            try
            {
                VehicleRequest envelope = null;
                log.LogInformation($"{functionName}" + " Message ID: {id}", message.MessageId);

                // Create an XML serializer for the VehicleRequest type
                XmlSerializer serializer = new XmlSerializer(typeof(VehicleRequest));

                // Deserialize the XML message body into an VehicleRequest object
                string xmlString = Encoding.UTF8.GetString(message.Body);
                using (StringReader reader = new StringReader(xmlString))
                {
                    envelope = (VehicleRequest)serializer.Deserialize(reader);
                }

                // Validate app settings are present
                string username = await Utils.GetSecret("ToIVUServiceIntervals-ServiceUsername", log);
                string password = await Utils.GetSecret("ToIVUServiceIntervals-ServicePassword", log);
                ValidateAppSettings(out string serviceUrl, out string topicLogEnable, out string containerName, out string maxRetries, out string pauseBetweenFailures);

                log.LogInformation($"!!!!!!Message!!!!!!: {xmlString}");
                log.LogInformation($"!!!!!!Username!!!!!: {username}, password: {password.Substring(password.Length - 3)}");

                Utils.ExecuteArchiveLog(log, topicLogEnable, functionName, xmlString, containerName, "xml");

                // Initialize a client for the web service
                client = GetVehicleWebFacade(serviceUrl);
                VehicleResponse result;
                try
                {
                    // Set up the context for the web service operation
                    using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
                    {
                        // Create and configure an HTTP request message property with basic authentication
                        HttpRequestMessageProperty httpRequestProperty = new HttpRequestMessageProperty();
                        httpRequestProperty.Headers[System.Net.HttpRequestHeader.Authorization] = "Basic " +
                            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

                        // Add the HTTP request message property to the outgoing message properties
                        System.ServiceModel.OperationContext.Current.OutgoingMessageProperties[
                            HttpRequestMessageProperty.Name] = httpRequestProperty;

                        // Call the web service method and handle the result
                        result = Utils.Execute(() => client.importVehicles(envelope), log,
                           Convert.ToInt32(maxRetries), TimeSpan.FromSeconds(Convert.ToInt16(pauseBetweenFailures)));
                    }
                }
                catch (Exception ex)
                {
                    NetworkError = true;
                    throw;
                }
                //foreach (VehicleKeyResponse resp in result.vehicleKeyResponses)
                //{
                //    if ( !string.IsNullOrEmpty(resp.number))

                //    {                
                //        log.LogInformation($"{functionName} response number : " + resp.number + ", division abbrevation:" + resp.divisionAbbreviation);               
                //    }
                //}
                log.LogInformation($"{functionName} process was Completed");
            }
            catch (Exception ex)
            {
                exception = ex;
                log.LogError(0, ex, $"Error running function {functionName}.");
                client?.Abort();
            }
            finally
            {
                if (client != null && client.State == CommunicationState.Opened)
                    client.Close();
                if (NetworkError)
                    await NotifyNetworkError(exception, log);
                log.LogInformation($"The {functionName} function was completed.");
            }
        }

        private static async Task NotifyNetworkError(Exception ex, ILogger log)
        {
            await Send_Email_Notifications_HelpDesk_IVU_Zedas(ex != null ? ex.ToString() : "", log);
            await Send_Email_Notifications_Users_IVU_Zedas(ex != null ? ex.ToString() : "", log);
        }

        public static VehicleWebFacadeClient GetVehicleWebFacade(string serviceUrl)
        {
            VehicleWebFacadeClient.EndpointConfiguration endpoint = new VehicleWebFacadeClient.EndpointConfiguration();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            VehicleWebFacadeClient client = new VehicleWebFacadeClient(endpoint, serviceUrl);
            return client;
        }

        private static void ValidateAppSettings(out string serviceUrl, out string topicLogEnable, out string containerName, out string maxRetries, out string pauseBetweenFailures)
        {
            serviceUrl = Utils.VerifyAppSettingString("ToIVUServiceIntervals_ServiceUrl");
            topicLogEnable = Utils.VerifyAppSettingString("ToIVUServiceIntervalsLogEnable");
            containerName = Utils.VerifyAppSettingString("AzureBlobStorageONXArchiveContainerName");
            maxRetries = Utils.VerifyAppSettingString("IVU_Zedas_MaxRetries");
            pauseBetweenFailures = Utils.VerifyAppSettingString("IVU_Zedas_TimeIntervalBetweenFailures");
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
}