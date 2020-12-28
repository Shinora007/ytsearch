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

        public List<YouTubeVideo> SearchVideos(InputArgument inputArg)
        {
            // Searching video with matching query [Cost: 100qc]
            var searchListRequest = provideYouTubeService().Search.List("snippet");
            searchListRequest.Type = "video";
            searchListRequest.Q = inputArg.SearchTerm;
            searchListRequest.MaxResults = inputArg.QueryLimit;

            if (inputArg.FilterForToday)
            {
                searchListRequest.PublishedAfter = DateTime.Now.AddDays(-1);
            }

            SearchListResponse searchListResponse = searchListRequest.Execute();

            var videos = searchListResponse.Items
                .Where(r => r.Id.Kind == "youtube#video")
                .Select(x => new YouTubeVideo
                {
                    Id = x.Id.VideoId,
                    Title = x.Snippet.Title,
                    ChannelTitle = x.Snippet.ChannelTitle,
                    PublishedAt = DateTime.Parse(x.Snippet.PublishedAt)
                }).ToList();

            videos.Sort((x, y) => DateTime.Compare(x.PublishedAt, y.PublishedAt));

            return videos;
        }

        public List<YouTubeChannel> SearchChannels(InputArgument inputArg)
        {
            // Searching channel with matching query [Cost: 100qc]
            var searchListRequest = provideYouTubeService().Search.List("snippet");
            searchListRequest.Type = "channel";
            searchListRequest.Q = inputArg.SearchTerm;
            searchListRequest.MaxResults = 3;

            SearchListResponse searchListResponse = searchListRequest.Execute();

            var channels = new List<YouTubeChannel>();
            for (int i = 0; i < searchListResponse.Items.Count; i++)
            {
                channels.Add(new YouTubeChannel
                {
                    Id = searchListResponse.Items[i].Id.ChannelId,
                    ChannelNumber = i + 1,
                    Title = searchListResponse.Items[i].Snippet.Title
                });
            }

            return channels;
        }

        public List<YouTubeVideo> GetVideosUnderChannel(InputArgument inputArg)
        {
            // Search channel with given number [Cost: 100qc]
            var selectedChannel = SearchChannels(inputArg).Single(x => x.ChannelNumber == inputArg.ChannelNumber);

            // Get channels upload playlist [Cost: 1qc]
            var channelListRequest = provideYouTubeService().Channels.List("contentDetails");
            channelListRequest.Id = selectedChannel.Id;
            var channelListResponse = channelListRequest.Execute();
            string uploadPlayListId = channelListResponse.Items[0].ContentDetails.RelatedPlaylists.Uploads;

            // Get Channel's video list [Cost: 1qc]
            var playListRequest = provideYouTubeService().PlaylistItems.List("snippet");
            playListRequest.MaxResults = inputArg.QueryLimit;
            playListRequest.PlaylistId = uploadPlayListId;
            var playListResponse = playListRequest.Execute();

            var videos = playListResponse.Items
                .Select(x => new YouTubeVideo
                {
                    Id = x.Snippet.ResourceId.VideoId,
                    Title = x.Snippet.Title,
                    ChannelTitle = x.Snippet.ChannelTitle,
                    PublishedAt = DateTime.Parse(x.Snippet.PublishedAt)
                }).ToList();

            if (inputArg.FilterForToday)
            {
                videos = videos.Where(x => x.PublishedAt >= DateTime.Now.AddDays(-1)).ToList();
            }

            videos.Sort((x, y) => DateTime.Compare(x.PublishedAt, y.PublishedAt));

            return videos;
        }

        private YouTubeService provideYouTubeService()
        {
            return new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = youTubeApiKey,
                ApplicationName = this.GetType().ToString()
            });
        }
    }
}
