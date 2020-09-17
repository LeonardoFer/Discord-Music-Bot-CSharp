using System.Text;
using System.Configuration;
using System.Collections.Generic;

using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

using static System.Console;

namespace DiscordRadioBot.Classes
{
    public class YoutubeSearchEngine
    {
        readonly string youtubeApiKey = BotClient.configJson.YoutubeApiKey;

        private readonly short _maxSearchResults;
        /// <summary>
        /// YoutubeSearchEngine Constructor.
        /// </summary>
        /// <param name="wordList"> List of Words to use for the Search Query. </param>
        /// <param name="maxSearchResults"> Maximum amount of results. </param>
        public YoutubeSearchEngine(short maxSearchResults)
        {
            _maxSearchResults = maxSearchResults;
        }
        /// <summary>
        /// Search in YouTube and returns a dictionary containing Titles of Music Videos and the Video Url.
        /// </summary>
        /// <param name="wordList"> A list of keywords to use for the search query. </param>
        /// <returns></returns>
        public Dictionary<string, string> SearchVideos(List<string> wordList)
        {
            YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer { ApiKey = youtubeApiKey, ApplicationName = $"YouTubeCustomSearchEngine" });
            SearchResource.ListRequest searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = ParseSearchQuery(wordList);
            searchListRequest.Type = "video";
            searchListRequest.MaxResults = _maxSearchResults;
            searchListRequest.VideoCategoryId = "10";
            searchListRequest.EventType = SearchResource.ListRequest.EventTypeEnum.None;

            SearchListResponse searchListResponse = searchListRequest.Execute();

            return GetYouTubeSearchResults(searchListResponse);
        }
        /// <summary>
        /// Search in YouTube and returns a dictionary containing Titles of Music Videos and the Video Url.
        /// </summary>
        /// <param name="searchQuery"> Search string to use for the search query. </param>
        /// <returns></returns>
        public Dictionary<string, string> SearchVideos(string searchQuery)
        {
            YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer { ApiKey = youtubeApiKey, ApplicationName = $"YouTubeCustomSearchEngine" });
            SearchResource.ListRequest searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = searchQuery;
            searchListRequest.Type = "video";
            searchListRequest.MaxResults = _maxSearchResults;
            searchListRequest.VideoCategoryId = "10";
            searchListRequest.EventType = SearchResource.ListRequest.EventTypeEnum.None;

            SearchListResponse searchListResponse = searchListRequest.Execute();

            return GetYouTubeSearchResults(searchListResponse);
        }
        /// <summary>
        /// Parses a Search Query using the list of words given.
        /// </summary>
        /// <param name="words"> List of Words for the Search Query. </param>
        /// <returns></returns>
        private string ParseSearchQuery(List<string> words)
        {
            StringBuilder stringBuilder = new StringBuilder();
            WriteLine($"\nList of Words: ");
            foreach(string word in words)
            {
                if(word != words[words.Count - 1])
                {
                    stringBuilder.Append(word);
                    stringBuilder.Append(" ");
                    Write($"{word}, ");
                }
                else
                {
                    stringBuilder.Append(word);
                    Write($"{word}.\n");
                }
            }
            string searchQuery = stringBuilder.ToString();
            WriteLine($"\nSearch Query: {searchQuery}\n");
            return searchQuery;
        }
        /// <summary>
        /// Method used to treat the title of the music video, trying to remove unecessary 
        /// stuff that isn't really part of the Author - Song Name format.
        /// </summary>
        /// <param name="result"> The instance of the YouTube SearchResult. </param>
        /// <returns></returns>
        private string ParseVideoTitle(SearchResult result)
        {
            /* TODO: Make this whole method better. Maybe using LINQ but I got to learn how 
             * to use it first. Theres also some other special characters that appear very 
             * rarely that would be nice to remove from the Title. */
            string videoTitle1, videoTitle2, videoTitle3, videoTitle4, videoTitle5;
            #region Counting the amount of special chars in the title.
            int hifenCount = 0;
            int parentesisCount = 0;
            int squareBracketsCount = 0;
            int horizontalBarCount = 0;
            foreach(char character in result.Snippet.Title)
            {
                if(character == '-')
                {
                    hifenCount++;
                }
                else if(character == '(')
                {
                    parentesisCount++;
                }
                else if(character == '[')
                {
                    squareBracketsCount++;
                }
                else if(character == '|')
                {
                    horizontalBarCount++;
                }
            }
            #endregion
            videoTitle1 = result.Snippet.Title;

            int hifenLastIndex = videoTitle1.LastIndexOf('-');
            videoTitle2 = videoTitle1.Contains('-') && hifenCount >= 2 ? videoTitle1.Substring(0, hifenLastIndex) : videoTitle1;

            //int parentesisLastIndex = videoTitle2.LastIndexOf('(');
            //videoTitle3 = videoTitle2.Contains('(') && (parentesisCount > 0 && parentesisLastIndex >= 5) ? videoTitle2.Substring(0, parentesisLastIndex) : videoTitle2;

            videoTitle3 = videoTitle2;

            int squareBracketsFirstIndex = videoTitle3.IndexOf('[');
            videoTitle4 = videoTitle3.Contains('[') && squareBracketsFirstIndex >= 5 ? videoTitle3.Substring(0, squareBracketsFirstIndex) : videoTitle3;

            int horizontalBarFirstIndex = result.Snippet.Title.IndexOf('|');
            videoTitle5 = videoTitle4.Contains('|') && horizontalBarFirstIndex >= 5 ? videoTitle4.Substring(0, horizontalBarFirstIndex) : videoTitle4;
            videoTitle5 = videoTitle5[videoTitle5.Length - 1] == ' ' ? videoTitle5.Remove(videoTitle5.Length - 1) : videoTitle5;

            return videoTitle5;
        }
        /// <summary>
        /// Searches in YouTube using the given search query and returns a list of parsed data. 
        /// Excludes all Channels, Playlists and Broadcasts from the search.
        /// </summary>
        /// <param name="searchListResponse"> Search Response from YouTube. </param>
        /// <returns></returns>
        private Dictionary<string, string> GetYouTubeSearchResults(SearchListResponse searchListResponse)
        {
            Dictionary<string, string> videos = new Dictionary<string, string>();

            foreach(SearchResult result in searchListResponse.Items)
            {
                if(result.Snippet.LiveBroadcastContent == "none")
                {
                    string title = ParseVideoTitle(result);

                    if(!videos.ContainsKey(title))
                    {
                        videos.Add(title, @$"https://www.youtube.com/watch?v=" + $"{result.Id.VideoId}");
                        WriteLine($"Music video added to the list of Videos. Original music title: {result.Snippet.Title}");
                    }
                    else
                    {
                        WriteLine($"Music video {title} is already in the list! Skipping...");
                    }
                }
            }
            WriteLine($"\nSearch done! I've searched for {_maxSearchResults} and I've found {videos.Count} videos.\n");
            return videos;
        }
    }
}