using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
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
        public List<YouTubeVideo> SearchVideos(InputArgument inputArg)
        {
            // Searching video with matching query [Cost: 100qc]
            var searchListRequest = YouTubeServiceProvider().Search.List("snippet");
            searchListRequest.Type = "video";
            searchListRequest.Q = inputArg.SearchTerm;
            searchListRequest.MaxResults = inputArg.QueryLimit;
            searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;

            if (inputArg.FilterForToday)
            {
                searchListRequest.PublishedAfter = DateTime.Now.AddDays(-1);
            }

            SearchListResponse searchListResponse = searchListRequest.Execute();

            // Fetching duration of these videos [Cost: 1qc]
            var videoListRequest = YouTubeServiceProvider().Videos.List("contentDetails");
            videoListRequest.Id = string.Join(",", searchListResponse.Items.Select(x => x.Id.VideoId));
            var videoListResponse = videoListRequest.Execute();

            var videos = searchListResponse.Items.Zip(videoListResponse.Items, (x, y) => new YouTubeVideo
            {
                Id = x.Id.VideoId,
                Title = x.Snippet.Title,
                ChannelTitle = x.Snippet.ChannelTitle,
                Duration = XmlConvert.ToTimeSpan(y.ContentDetails.Duration),
                PublishedAt = DateTime.Parse(x.Snippet.PublishedAt)
            }).ToList();

            return videos;
        }

        public List<YouTubeChannel> SearchChannels(InputArgument inputArg)
        {
            // Searching channel with matching query [Cost: 100qc]
            var searchListRequest = YouTubeServiceProvider().Search.List("snippet");
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
            var channelListRequest = YouTubeServiceProvider().Channels.List("contentDetails");
            channelListRequest.Id = selectedChannel.Id;
            var channelListResponse = channelListRequest.Execute();
            string uploadPlayListId = channelListResponse.Items[0].ContentDetails.RelatedPlaylists.Uploads;

            // Get Channel's video list [Cost: 1qc]
            var playListRequest = YouTubeServiceProvider().PlaylistItems.List("snippet");
            playListRequest.MaxResults = inputArg.QueryLimit;
            playListRequest.PlaylistId = uploadPlayListId;
            var playListResponse = playListRequest.Execute();

            // Fetching duration of these videos [Cost: 1qc]
            var videoListRequest = YouTubeServiceProvider().Videos.List("contentDetails");
            videoListRequest.Id = string.Join(",", playListResponse.Items.Select(x => x.Snippet.ResourceId.VideoId));
            var videoListResponse = videoListRequest.Execute();

            var videos = playListResponse.Items.Zip(videoListResponse.Items, (x, y) => new YouTubeVideo
            {
                Id = x.Snippet.ResourceId.VideoId,
                Title = x.Snippet.Title,
                ChannelTitle = x.Snippet.ChannelTitle,
                Duration = XmlConvert.ToTimeSpan(y.ContentDetails.Duration),
                PublishedAt = DateTime.Parse(x.Snippet.PublishedAt)
            }).ToList();

            //var videos = playListResponse.Items
            //    .Select(x => new YouTubeVideo
            //    {
            //        Id = x.Snippet.ResourceId.VideoId,
            //        Title = x.Snippet.Title,
            //        ChannelTitle = x.Snippet.ChannelTitle,
            //        PublishedAt = DateTime.Parse(x.Snippet.PublishedAt)
            //    }).ToList();

            if (inputArg.FilterForToday)
            {
                videos = videos.Where(x => x.PublishedAt >= DateTime.Now.AddDays(-1)).ToList();
            }

            videos.Sort((x, y) => DateTime.Compare(x.PublishedAt, y.PublishedAt));

            return videos;
        }

        private YouTubeService YouTubeServiceProvider()
        {
            string youTubeApiKey = string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                youTubeApiKey = Environment.GetEnvironmentVariable("YouTubeApiKey", EnvironmentVariableTarget.User);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                youTubeApiKey = Environment.GetEnvironmentVariable("YouTubeApiKey");
            }

            return new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = youTubeApiKey,
                ApplicationName = this.GetType().ToString()
            });
        }
    }
}
