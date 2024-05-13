using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Constants
{
    public static class Constants
    {
        public const string FileNameTimeStampFormat = "yyyyMMdd-HHmmss.ffff";
        public const string FileNameTimeOffsetFormat = "yyyyMMdd-HHmmss.ffff";
        public const string FolderNameTimeStampFormat = "yyyy-MM-dd";
        public const string OracleFileUploadStatusRedisKey = "OracleFileUploadStatus";
        public const string EmailTo = "_Email_To";
        public const int RedisDatabase = 1;
    }

}