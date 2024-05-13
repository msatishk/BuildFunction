using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToOracleFileUploadStatusCheck.Models
{
    public class Settings
    {
        public string HDLLoadEndpoint { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string EmailNotificationURL { get; set; }
        public string EmailSubject { get; set; }
        public string EmailImportance { get; set; }
        public string EmailBody{ get; set; }
        public int RedisDatabase { get; set; }
        public string RedisConnectionString { get; set; }
        public string EmailTo { get; set; }

    }
}
