using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TASVideos.Core.HttpClientExtensions;
using TASVideos.Core.Services.Youtube.Dtos;
using TASVideos.Core.Settings;
using TASVideos.Data.Entity;
using TASVideos.Extensions;

namespace TASVideos.Core.Services.Youtube
{
	public interface IYoutubeSync
	{
		bool IsYoutubeUrl(string? url);
		Task SyncYouTubeVideo(YoutubeVideo video);
		Task UnlistVideo(string url);
	}

	internal class YouTubeSync : IYoutubeSync
	{
		private static readonly string[] BaseTags = { "TAS", "TASVideos", "Tool-Assisted", "Video Game" };
		private readonly HttpClient _client;
		private readonly IGoogleAuthService _googleAuthService;
		private readonly AppSettings _settings;

		public YouTubeSync(
			IHttpClientFactory httpClientFactory,
			IGoogleAuthService googleAuthService,
			AppSettings settings)
		{
			_client = httpClientFactory.CreateClient(HttpClients.Youtube)
				?? throw new InvalidOperationException($"Unable to initalize {HttpClients.Youtube} client");
			_googleAuthService = googleAuthService;
			_settings = settings;
		}

		public async Task SyncYouTubeVideo(YoutubeVideo video)
		{
			if (!IsYoutubeUrl(video.Url))
			{
				return;
			}

			if (!_googleAuthService.IsEnabled())
			{
				return;
			}

			var videoId = VideoId(video.Url);
			var videoDetails = await HasAccessToChannel(videoId);
			if (videoDetails is null)
			{
				return;
			}

			await SetAccessToken();

			var descriptionBase = $"This is a tool-assisted speedrun. For more information, see {_settings.BaseUrl}/{video.Id}M";
			if (video.ObsoletedBy.HasValue)
			{
				descriptionBase += $"\n\nThis movie has been obsoleted by {_settings.BaseUrl}/{video.ObsoletedBy.Value}M";
			}

			descriptionBase += $"\nTAS originally published on {video.PublicationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}\n\n";
			var renderedDescription = YoutubeHelper.RenderWikiForYoutube(video.WikiPage, _settings.BaseUrl);

			var requestBody = new VideoUpdateRequest
			{
				VideoId = videoId,
				Snippet = new ()
				{
					Title = $"[TAS] {(video.ObsoletedBy.HasValue ? "[Obsoleted]" : "")} {video.Title}",
					Description = descriptionBase + renderedDescription,
					CategoryId = videoDetails.CategoryId,
					Tags = BaseTags.Concat(video.Tags).ToList()
				}
			}.ToStringContent();

			var response = await _client.PutAsync("videos?part=status,snippet", requestBody);
			response.EnsureSuccessStatusCode();
		}

		public async Task UnlistVideo(string url)
		{
			if (!IsYoutubeUrl(url))
			{
				return;
			}

			if (!_googleAuthService.IsEnabled())
			{
				return;
			}

			var videoId = VideoId(url);
			if (await HasAccessToChannel(videoId) is null)
			{
				return;
			}

			await SetAccessToken();
			var requestBody = new VideoUpdateRequest
			{
				VideoId = videoId,
				Status = new ()
				{
					PrivacyStatus = "unlisted"
				}
			}.ToStringContent();

			var response = await _client.PutAsync("videos?part=status", requestBody);
			response.EnsureSuccessStatusCode();
		}

		public bool IsYoutubeUrl(string? url)
		{
			return !string.IsNullOrWhiteSpace(url) && (url.Contains("youtube.com") || url.Contains("youtu.be"));
		}

		internal static string VideoId(string youtubeUrl)
		{
			if (string.IsNullOrWhiteSpace(youtubeUrl))
			{
				return "";
			}

			if (youtubeUrl.Contains("https://youtu.be"))
			{
				return youtubeUrl.SplitWithEmpty("/").Last();
			}

			var result = youtubeUrl[(youtubeUrl.IndexOf("v=") + 2)..];

			if (!string.IsNullOrWhiteSpace(result))
			{
				// Account for anchors
				result = result.SplitWithEmpty("#")[0];

				// Account for additional query string params
				result = result.SplitWithEmpty("&")[0].TrimEnd('?');
			}

			return result;
		}

		private async Task SetAccessToken()
		{
			var accessToken = await _googleAuthService.GetAccessToken();
			_client.SetBearerToken(accessToken);
		}

		private async Task<YoutubeVideoSnippet?> HasAccessToChannel(string videoId)
		{
			await SetAccessToken();

			// fileDetails require authorization to see, so this serves as a way to determine access
			// there may be a more intended strategy to use
			var result = await _client.GetAsync($"videos?id={videoId}&part=snippet,fileDetails");
			if (result.IsSuccessStatusCode)
			{
				var getResponse = await result.ReadAsync<YoutubeGetResponse>();
				return getResponse.Items.First().Snippet;
			}

			return null;
		}
	}

	public record YoutubeVideo(
		int Id,
		DateTime PublicationDate,
		string Url,
		string Title,
		WikiPage WikiPage,
		string SystemCode,
		IEnumerable<string> Authors,
		string? SearchKey,
		int? ObsoletedBy)
	{
		public IEnumerable<string> Tags
		{
			get
			{
				var tags = new[] { SystemCode }
					.Concat(Authors);

				if (!string.IsNullOrWhiteSpace(SearchKey))
				{
					tags = tags.Concat(SearchKey.SplitWithEmpty("-"));
				}

				tags = tags.Select(t => t.ToLower()).Distinct();
				return tags;
			}
		} 
	}
}
