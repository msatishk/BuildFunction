using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs.ServiceBus;

namespace IVU_Zedas
{
    //public static class IVU_Zedas
    //{
    //    [FunctionName("IVU_Zedas")]
    //    public static async Task<IActionResult> Run(
    //        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
    //        ILogger log)
    //    {
    //        log.LogInformation("C# HTTP trigger function processed a request.");

    //        string name = req.Query["name"];

    //        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    //        dynamic data = JsonConvert.DeserializeObject(requestBody);
    //        name = name ?? data?.name;

    //        string responseMessage = string.IsNullOrEmpty(name)
    //            ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
    //            : $"Hello, {name}. This HTTP triggered function executed successfully.";

    //        return new OkObjectResult(responseMessage);
    //    }
    //}

    //namespace Main_ToIVUDeploymentRestrictions
    //{
    //    public static class Main_ToIVUDeploymentRestrictions
    //    {
    //        [FunctionName("ToIVUDeploymentRestrictions")]
    //        public static async Task Run([ServiceBusTrigger("%DeploymentRestrictionsTopicName%", "%DeploymentRestrictionsSubscriptionName%", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, ILogger log)
    //        {
    //            await ToIVUDeploymentRestrictions.ToIVUDeploymentRestrictions.Run(message, messageActions, log);
    //        }
    //    }
    //}

    //namespace Main_ToIVUServiceIntervals
    //{
    //    public static class Main_ToIVUServiceIntervals
    //    {
    //        [FunctionName("ToIVUServiceIntervals")]
    //        public static async Task Run([ServiceBusTrigger("%ServiceIntervalsTopicName%", "%ServiceIntervalsSubscriptionName%", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, ILogger log)
    //        {
    //            await ToIVUServiceIntervals.ToIVUServiceIntervals.Run(message, messageActions, log);
    //        }
    //    }
    //}

    //namespace Main_ToZedasVehicleConsist
    //{
    //    public static class Main_ToZedasVehicleConsist
    //    {
    //        [FunctionName("ToZedasVehicleConsist")]
    //        public static async Task Run([ServiceBusTrigger("%IVUVehicleConsistTopicName%", "%ZedasVehicleConsistSubscriptionName%", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, ILogger log)
    //        {
    //            await ToZedasVehicleConsist.ToZedasVehicleConsist.Run(message, messageActions, log);
    //        }
    //    }
    //}
}
