using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Common
{
    public class Video
    {
        public Guid Id { get; set; }
        public DateTime UploadedDate { get; set; }
        public string OriginalUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public AdaptiveStreamingInfo StreamingInfo { get; set; }
    }
}
