using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OracleCommon.Constants;
using OracleCommon.Models;
using OracleCommon.Services;
using Shared;
using Shared.Utils;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ToOracleFileUploadStatusCheck.Interfaces;
using ToOracleFileUploadStatusCheck.Models;

namespace ToOracleFileUploadStatusCheck.Services
{
    public class RedisService : IRedisService
    {
        public void ReadDataFromRedis(Settings settings, ILogger log)
        {
            try
            {
                Utils utility = new Utils();
                var redis = ConnectionMultiplexer.Connect(settings.RedisConnectionString);
                IDatabase redisCache = utility.GetRedisDatabase(settings.RedisConnectionString, settings.RedisDatabase, log);
                RedisValue existingValues = redisCache.StringGet(Shared.Constants.Constants.OracleFileUploadStatusRedisKey);
                List<OracleStatus> documentsPending;
                if (string.IsNullOrEmpty(existingValues))
                {
                    log.LogInformation($"ToOracleFileUploadStatusCheck_ReadDataFromRedis no data retrieved from Redis for key : {Shared.Constants.Constants.OracleFileUploadStatusRedisKey}");
                }
                else
                {
                    documentsPending = JsonConvert.DeserializeObject<List<OracleStatus>>(existingValues);
                    List<OracleStatus> documentsPendingTemp = new List<OracleStatus>();
                    documentsPendingTemp.AddRange(documentsPending);
                    OracleSoapService oracleSoapService = new OracleSoapService();
                    OraclePayloadSettings payloadSettings = new OraclePayloadSettings { HDLLoadEndpoint = settings.HDLLoadEndpoint, Password = settings.Password, Username = settings.Username, RedisConnectionString = settings.RedisConnectionString };
                    if (documentsPending.Any())
                    {
                        var exceptions = new ConcurrentQueue<Exception>();
                        documentsPending.ForEach(async x =>
                        {
                            try
                            {
                                var currentDataSet = oracleSoapService.FetchTheLoadStatus(x.ContentID, x.ProcessID, payloadSettings, log)?.Result;
                                if (currentDataSet != null)
                                {
                                    if (string.Equals(currentDataSet.Status, OracleConstants.StatusCompleted, StringComparison.InvariantCultureIgnoreCase)
                                    && currentDataSet.Import != null && string.Equals(currentDataSet.Import.Status, OracleConstants.StatusSuccess, StringComparison.InvariantCultureIgnoreCase)
                                    && currentDataSet.Load != null && string.Equals(currentDataSet.Load.Status, OracleConstants.StatusSuccess, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        log.LogInformation($"ToOracleFileUploadStatusCheck_ReadDataFromRedis Successfully uploaded blob to Oracle UCM for ContentID: {x.ContentID} and ProcessID: {x.ProcessID}");
                                        documentsPendingTemp.Remove(x);
                                        UpdateDataInRedis(documentsPendingTemp, settings.RedisConnectionString, settings.RedisDatabase, log);
                                    }
                                    else if (currentDataSet.ErrorCode == OracleConstants.DataSetNotFoundErrorCode)
                                    {
                                        log.LogError($"ToOracleFileUploadStatusCheck_ReadDataFromRedis no data set found for combination of ContentID: {x.ContentID} and ProcessID: {x.ProcessID} in Oracle");
                                        documentsPendingTemp.Remove(x);
                                        UpdateDataInRedis(documentsPendingTemp, settings.RedisConnectionString, settings.RedisDatabase, log);
                                    }
                                    else if ((currentDataSet.ErrorCode == OracleConstants.DataSetErrorCode)
                                    || currentDataSet.Import != null && !string.Equals(currentDataSet.Import.Status, OracleConstants.StatusSuccess, StringComparison.InvariantCultureIgnoreCase)
                                    || currentDataSet.Load != null && !string.Equals(currentDataSet.Load.Status, OracleConstants.StatusSuccess, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        documentsPendingTemp.Remove(x);
                                        UpdateDataInRedis(documentsPendingTemp, settings.RedisConnectionString, settings.RedisDatabase, log);
                                        log.LogError($"ToOracleFileUploadStatusCheck_ReadDataFromRedis Error occurred for ContentID: {x.ContentID} and ProcessID: {x.ProcessID} in Oracle");
                                        var email = new EmailNotificationData
                                        {
                                            to = settings.EmailTo,
                                            importance = settings.EmailImportance,
                                            subject = settings.EmailSubject.Replace("{0}", x.ContentID),
                                            email_body = settings.EmailBody.Replace("{0}", x.ContentID)
                                            .Replace("{1}", x.LoggedTime?.ToString(Shared.Constants.Constants.FileNameTimeStampFormat))
                                            .Replace("{2}", JsonConvert.SerializeObject(currentDataSet.Import))
                                            .Replace("{3}", JsonConvert.SerializeObject(currentDataSet.Load))
                                        };
                                        await Utils.SendMessageNotification(settings.EmailNotificationURL, email, log);
                                    }
                                }
                                else
                                {
                                    log.LogError($"ToOracleFileUploadStatusCheck_ReadDataFromRedis received null currentDataSet as response from the getDataSetStatusAsync Oracle Method for ContentID: {x.ContentID} and ProcessID: {x.ProcessID}");
                                }
                            }
                            catch (Exception ex)
                            {
                                exceptions.Enqueue(ex);
                            }
                        });
                        if (!exceptions.IsEmpty)
                        {
                            throw new AggregateException(exceptions);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"ToOracleFileUploadStatusCheck_ReadDataFromRedis Exception occurred during runtime while storing the data in Redis with message: {ex.Message}");
                throw;
            }
        }

        private void UpdateDataInRedis(List<OracleStatus> documentsPending, string connectionString, int redisDatabase, ILogger log)
        {
            Utils utility = new Utils();
            IDatabase redisCache = utility.GetRedisDatabase(connectionString, redisDatabase ,log);
            string serializedContent = JsonConvert.SerializeObject(documentsPending);
            if (string.IsNullOrEmpty(serializedContent))
            {
                log.LogError($"ToOracleFileUploadStatusCheck_UpdateDataInRedis Failed to serialize object");
                throw new ArgumentNullException($"ToOracleFileUploadStatusCheck_UpdateDataInRedis the serialized object is null");
            }
            else
            {
                if (redisCache.StringSet(Shared.Constants.Constants.OracleFileUploadStatusRedisKey, serializedContent))
                {
                    log.LogInformation($"ToOracleFileUploadStatusCheck_UpdateDataInRedis Successfully wrote data for oracle file status upload check with Key: {Shared.Constants.Constants.OracleFileUploadStatusRedisKey} into redis cache.");
                }
                else
                {
                    log.LogError($"ToOracleFileUploadStatusCheck_UpdateDataInRedis Failed to write data for oracle file status upload check with Key: {Shared.Constants.Constants.OracleFileUploadStatusRedisKey} into redis cache.");
                }
            }
        }
    }
}
