using System;

namespace ytsearch.Models
{
    public class YouTubeVideo
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string ChannelTitle { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime PublishedAt { get; set; }
    }
}
