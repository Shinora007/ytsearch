using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using ytsearch.Models;

namespace ytsearch
{
    public class YouTubeServiceClient
    {
        private static int RandomSeem = 7;
        private static int RandomUpperLimit = 10;
        
        private string youTubeApiKey;

        public YouTubeServiceClient()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                this.youTubeApiKey = Environment.GetEnvironmentVariable("YouTubeApiKey", EnvironmentVariableTarget.User);
            } 
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                this.youTubeApiKey = Environment.GetEnvironmentVariable("YouTubeApiKey");
            }
        }


        public List<YouTubeVideo> GetVideos(InputArgument inputArg)
        {
            SearchListResponse searchListResponse = SearchYouTube(inputArg);

            var rand = new Random(RandomSeem);
            return searchListResponse.Items
                .Where(r => r.Id.Kind == "youtube#video")
                .Select(x => new YouTubeVideo
                {
                    Id = x.Id.VideoId,
                    VideoNumber = rand.Next(0, RandomUpperLimit),
                    Title = x.Snippet.Title,
                    ChannelTitle = x.Snippet.ChannelTitle,
                    PublishedAt = DateTime.Parse(x.Snippet.PublishedAt)
                }).ToList();
        }

        public List<YouTubeChannel> GetChannels(InputArgument inputArg)
        {
            SearchListResponse searchListResponse = SearchYouTube(inputArg);

            var rand = new Random(RandomSeem);
            return searchListResponse.Items
                .Where(r => r.Id.Kind == "youtube#channel")
                .Select(x => new YouTubeChannel
                {
                    Id = x.Id.ChannelId,
                    ChannelNumber = rand.Next(0, RandomUpperLimit),
                    Title = x.Snippet.Title
                }).ToList();
        }

        public List<YouTubeVideo> GetVideosUnderChannel(InputArgument inputArg)
        {
            var youTubeChannels = GetChannels(inputArg);


            var selectedChannel = youTubeChannels.Single(x => x.ChannelNumber == inputArg.ChannelNumber);

            SearchListResponse searchListResponse = SearchYouTube(inputArg, selectedChannel.Id);

            var rand = new Random(RandomSeem);
            return searchListResponse.Items
                .Where(r => r.Id.Kind == "youtube#video")
                .Select(x => new YouTubeVideo
                {
                    Id = x.Id.VideoId,
                    VideoNumber = rand.Next(0, RandomUpperLimit),
                    Title = x.Snippet.Title,
                    ChannelTitle = x.Snippet.ChannelTitle,
                    PublishedAt = DateTime.Parse(x.Snippet.PublishedAt)
                }).ToList();
        }

        private SearchListResponse SearchYouTube(InputArgument inputArg, string channelId = null)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = youTubeApiKey,
                ApplicationName = this.GetType().ToString()
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = inputArg.SearchTerm;
            searchListRequest.MaxResults = inputArg.QueryLimit;
            searchListRequest.ChannelId = channelId;

            var searchListResponse = searchListRequest.Execute();
            return searchListResponse;
        }
    }
}
