using System.Collections.Generic;

namespace BackOffice.Common
{
    public class AdaptiveStreamingInfo
    {
        public AdaptiveStreamingInfo()
        {
            this.Posters = new List<string>();
        }

        public string SmoothStreamingUrl { get; set; }
        public string MpegDashUrl { get; set; }
        public string HlsUrl { get; set; }
        public IList<string> Posters { get; set; }
    }
}
