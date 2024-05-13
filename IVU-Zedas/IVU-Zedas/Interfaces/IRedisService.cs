using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OracleCommon.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using ToOracleFileUploadStatusCheck.Models;

namespace ToOracleFileUploadStatusCheck.Interfaces
{
    public interface IRedisService
    {
        public void ReadDataFromRedis(Settings settings,ILogger log);
    }
}
