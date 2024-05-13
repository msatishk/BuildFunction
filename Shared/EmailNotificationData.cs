using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class EmailNotificationData
    {
        public string to { get; set; }

        public string subject { get; set; }

        public string email_body { get; set; }

        public string importance { get; set; }
    }
}
