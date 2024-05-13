using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Models;
using StackExchange.Redis;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Xml.Serialization;


namespace Shared.Utils
{
    public class Utils
    {
        /// <summary>
        /// Establishes a connection to a given topic or queue and attempts to send a message to it. Can throw exception if message fails to deliver.
        /// </summary>
        /// <param name="connectionString"> Connection string for service bus namespace that the message is being sent to.</param>
        /// <param name="payload"> The message being sent. </param>
        /// <param name="topicOrQueueName"> The name of the topic or queue that the message is being sent to. </param>
        /// <exception cref="ServiceBusException"></exception>
        /// <exception cref="System.Runtime.Serialization.SerializationException"></exception>
        public static async void SendMessage(string connectionString, string topicOrQueueName, string payload, HttpRequest req, Dictionary<string, string>? applicationProperties = null)
        {
            await using ServiceBusClient serviceBusClient = new ServiceBusClient(connectionString);
            ServiceBusSender sender = serviceBusClient.CreateSender(topicOrQueueName);

            ServiceBusMessage message = new(payload)
            {
                //Only add content type if a HttpRequest is passed and the content type is not null
                ContentType = req != null && req.ContentType != null ? req.ContentType : null
            };
            if (req != null)
                message.ApplicationProperties.Add("User-Agent", string.IsNullOrEmpty(req.Headers["User-Agent"].ToString()) ? "Other" : req.Headers["User-Agent"].ToString());
            if (applicationProperties != null)
            {
                foreach (var (key, value) in applicationProperties)
                {
                    message.ApplicationProperties.Add(key, value);
                }
            }

            await sender.SendMessageAsync(message);
        }

        public static async Task SendMessage(string connectionString, string topicOrQueueName, string payload, string contentType, string userAgent, Dictionary<string, string>? applicationProperties = null)
        {
            await using var serviceBusClient = new ServiceBusClient(connectionString);
            var sender = serviceBusClient.CreateSender(topicOrQueueName);

            ServiceBusMessage message = new(payload)
            {
                ContentType = contentType
            };
            message.ApplicationProperties.Add("User-Agent", userAgent);
            if (applicationProperties != null)
            {
                foreach (var (key, value) in applicationProperties)
                {
                    message.ApplicationProperties.Add(key, value);
                }
            }
            await sender.SendMessageAsync(message);
        }

        /// <summary>
        /// Verify if key is found in appsetting.json/local.settings.json before retrieving the value
        /// </summary>
        /// <param name="SettingName">Appsetting key name</param>
        /// <returns>Value of appsettings key</returns>
        /// <exception cref="Exception"></exception>
        public static string VerifyAppSettingString(string SettingName)
        {
            string? Setting = Environment.GetEnvironmentVariable(SettingName);
            if (string.IsNullOrEmpty(Setting))
            {
                throw new Exception($"Please set Environment variable {SettingName}.");
            }
            return Setting;
        }

        public static double VerifyAppSettingDouble(string settingName)
        {
            bool isValid = double.TryParse(Environment.GetEnvironmentVariable(settingName), out double setting);
            if (!isValid)
            {
                throw new Exception($"Please set the environment variable {settingName}.");
            }
            return setting;
        }

        /// <summary>
        /// Method for creating a connection to redis Database.
        /// </summary>
        public IDatabase GetRedisDatabase(string connectionString, int database, ILogger log)
        {
            try
            {
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString);
                return redis.GetDatabase(database);
            }
            catch (Exception ex)
            {
                log.LogError($"Shared_GetRedisDatabase Exception occurred during runtime while connecting to redis with message: {ex.Message}");
                throw;
            }
        }

        public static void ExecuteArchiveLog(ILogger log, string topicLogEnable, string functionName, string reqBody, string containerName, string ext)
        {
            if (topicLogEnable.ToLower().Equals("true"))
            {
                DataFeedLogger.DataFeedLogger.LogMessage(log, functionName, reqBody, containerName, ext);
            }
        }

        public static int VerifyAppSettingInt(string settingName)
        {
            bool isValid = int.TryParse(Environment.GetEnvironmentVariable(settingName), out int setting);
            if (!isValid)
            {
                throw new Exception($"Please set the environment variable {settingName}.");
            }
            return setting;
        }

        public static async Task SendMessageNotification(string logicAppUrl, EmailNotificationData emailInformation, ILogger log)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(emailInformation), System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(logicAppUrl, content);

                    response.EnsureSuccessStatusCode();
                    log.LogError("SendMessageNotification completed successfully.");
                }
            }
            catch (Exception ex)
            {
                log.LogError(0, ex, "SendMessageNotification: Error processing request");
                throw;
            }
        }

        public static async Task<string> GetSecret(string secretName, ILogger log)
        {
            string? res = null;
            try
            {
                string keyVaultUri = VerifyAppSettingString("KeyVaultURI");
                SecretClient client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
                Azure.Response<KeyVaultSecret> secret = await client.GetSecretAsync(secretName);
                res = secret?.Value?.Value;
                if (string.IsNullOrEmpty(res))
                {
                    throw new Exception($"Please set KeyVault secret for variable {secretName}.");
                }
            }
            catch (Exception ex)
            {
                log.LogError(0, ex, $"{secretName} Exception.");
                throw;
            }
            return res;
        }

        public static T DeserializeFromXmlString<T>(string xmlString)
        {
            XmlSerializer xs = new(typeof(T));
            using MemoryStream memoryStream = new(System.Text.Encoding.UTF8.GetBytes(xmlString));
            T deserializedObject = (T)xs.Deserialize(memoryStream);
            return deserializedObject;
        }
        public static string GetBasicAuth(string username, string password)
        {
            return $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))}";
        }

        public static TResult Execute<TResult>(Func<TResult> function, ILogger log, int tryTimes, TimeSpan interval)
        {
            for (int i = 0; i < tryTimes - 1; i++)
            {
                try
                {
                    return function();
                }
                catch (Exception ex)
                {
                    log.LogError(0, ex, $"Execute method exception: Retry Attempt:{i + 1}");
                    Thread.Sleep(interval);
                }
            }
            return function();
        }

        public static void Execute(Action action, ILogger log, int tryTimes, TimeSpan interval)
        {
            Execute<object>(() =>
            {
                action();
                return null;
            }, log, tryTimes, interval);
        }


        /// <summary>
        /// Function to send http request to rest api
        /// </summary>
        /// <param name="url">The URL we are sending the request to</param>
        /// <param name="requestBody">The content</param>
        /// <param name="username">The username for basic authorization</param>
        /// <param name="password">The password for basic authorization</param>
        /// <param name="contentType">The content type to be used. It's set to application/json by default</param>
        /// <param name="reqType">The request type (Get/Post/Put/Patch).</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> SendBasicRequest(string reqType, string url, string requestBody, string username, string password, string contentType = "application/json")
        {
            // Check if request type is provided
            if (string.IsNullOrEmpty(reqType))
            {
                throw new Exception("SendBasicRequest: The request type value provided, is null or empty.");
            }
            else if (string.IsNullOrEmpty(url))
            {
                throw new Exception("SendBasicRequest: The URL value provided, is null or empty.");
            }
            else if (string.IsNullOrEmpty(requestBody))
            {
                throw new Exception("SendBasicRequest: The request body value provided, is null or empty.");
            }
            else if (string.IsNullOrEmpty(username))
            {
                throw new Exception("SendBasicRequest: The username provided, is null or empty.");
            }
            else if (string.IsNullOrEmpty(password))
            {
                throw new Exception("SendBasicRequest: The password provided, is null or empty.");
            }
            else if (string.IsNullOrEmpty(contentType))
            {
                throw new Exception("SendBasicRequest: The content type provided, is null or empty.");
            }
            // Create HttpClient instance
            using (HttpClient httpClient = new HttpClient())
            {
                // Set authentication header if username and password are provided
                string base64Auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);
                // Set content type header if provided
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
                // Create StringContent for the request body
                StringContent content = new StringContent(requestBody, Encoding.UTF8);
                // Send request
                HttpResponseMessage response = await SendByRequestType(httpClient, reqType, url, content);
                // Read and return response
                return response;
            }
        }


        /// <summary>
        /// Function to send http request depending on the request type specified
        /// </summary>
        /// <param name="cli">The http client</param>
        /// <param name="type">The http request type</param>
        /// <param name="url">The url</param>
        /// <param name="content">The content message</param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> SendByRequestType(HttpClient cli, string type, string url, StringContent content)
        {
            // Send request
            HttpResponseMessage response = new HttpResponseMessage();
            if (type.ToUpper() == "GET") { response = await cli.GetAsync(url); }
            else if (type.ToUpper() == "POST") { response = await cli.PostAsync(url, content); }
            else if (type.ToUpper() == "PUT") { response = await cli.PutAsync(url, content); }
            else if (type.ToUpper() == "PATCH") { response = await cli.PatchAsync(url, content); }
            else
            {
                throw new Exception($"SendByRequestType: Incorrect HTTP Request Type, {type.ToUpper()}.");
            }
            return response;
        }


        public static BasicHttpsBinding GetBindingSettings(int seconds = int.MinValue)
        {
            BasicHttpsBinding binding = new BasicHttpsBinding();
            binding.MaxBufferPoolSize = int.MaxValue;
            binding.MaxReceivedMessageSize = int.MaxValue;
            if (seconds != int.MinValue)
            {
                binding.SendTimeout = TimeSpan.FromSeconds(seconds);
            }
            binding.Security.Mode = BasicHttpsSecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            return binding;
        }
    }
}
