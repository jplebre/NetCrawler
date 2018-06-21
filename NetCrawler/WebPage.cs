using System;
using System.Collections.Generic;

namespace NetCrawler
{
    public class WebPage
    {
        public Uri PageUrl { get; set; }
        public List<string> Links { get; set; } = new List<string>();
        public bool DeadLink { get; set; } = false;
        public bool WasCrawled { get; set; } = false;
    }
}
