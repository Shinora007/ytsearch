using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ytsearch.Models;

namespace ytsearch
{
    class Program
    {
        private const string HelpText = 
@"# Search video [here l is max result limit, by default will be 10]
> ytsearch -l 5 ""video search term""

# Search channel [will return code, id  and name of channel]
> ytsearch -l 5 ""channel search term"" -c

# Get Videos from selected channel code
> ytsearch -l 5 ""channel search term"" -c 12

# Get videos published today only
> ytsearch -l 5 ""video search term"" -t
";

        public static void Main(string[] args)
        {
            if (Debugger.IsAttached)
            {
                //args = new[] { "-c", "3", "akash banerjee", "-t" };
                args = new[] { "-c", "akash banerjee" };
            }

            InputArgument inputArgs = null;
            try
            {
                inputArgs = ArgumentParser.Parse(args);
            } catch
            {
                Console.WriteLine(HelpText);
                return;
            }

            var youTubeClient = new YouTubeServiceClient();

            List<YouTubeVideo> videos = null;
            List<YouTubeChannel> channels = null;

            switch (inputArgs.SearchType)
            {
                case SearchType.VideoSearch:
                    videos = youTubeClient.SearchVideos(inputArgs);
                    Console.WriteLine(JsonConvert.SerializeObject(videos, Formatting.Indented));
                    break;
                case SearchType.ChannelSearch:
                    channels = youTubeClient.SearchChannels(inputArgs);
                    Console.WriteLine(JsonConvert.SerializeObject(channels, Formatting.Indented));
                    break;
                case SearchType.VideoUnderChannelSearch:
                    videos = youTubeClient.GetVideosUnderChannel(inputArgs);
                    Console.WriteLine(JsonConvert.SerializeObject(videos, Formatting.Indented));
                    break;
            }

            // Find a way to nicely show the result in tabular format.
        }
    }
}
