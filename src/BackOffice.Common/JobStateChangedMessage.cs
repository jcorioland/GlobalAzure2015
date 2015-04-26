using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Common
{
    public class JobStateChangedMessage
    {
        public string JobId { get; set; }
        public string NewState { get; set; }
        public string OldState { get; set; }
        public Common.AdaptiveStreamingInfo StreamingInfo { get; set; }
    }
}
