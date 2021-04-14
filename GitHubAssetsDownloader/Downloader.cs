using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace GitHubAssetsDownloader
{
	public class Downloader: IDisposable
	{
		private HttpClient _client;

		public static object GetLatestRelease(string user, string repo)
		{
			var client = new RestClient("https://api.github.com/repos");
			client.UseNewtonsoftJson();
			var request = new RestRequest($"{user}/{repo}/releases/latest", DataFormat.Json);
			var response = client.Get<ExpandoObject>(request);
			if (response.StatusCode != HttpStatusCode.OK)
			{
				throw new HttpRequestException($"Can't get releases for {user}/{repo}",
					response.ErrorException, response.StatusCode);
			}

			return response.Data;
		}

		private async Task DownloadFile(string uri, string filePath)
		{
			var response = await _client.GetAsync(uri);
			await using var stream = new FileStream(filePath, FileMode.CreateNew);
			await response.Content.CopyToAsync(stream);
			Console.WriteLine($"Downloaded {filePath}");
		}

		public async Task DownloadAssets(string path, dynamic release)
		{
			var tasks = new List<Task>();
			if (Directory.Exists(path))
				Directory.Delete(path, true);
			Directory.CreateDirectory(path);
			_client = new HttpClient();
			foreach (var asset in release.assets)
			{
				tasks.Add(DownloadFile(asset.browser_download_url, Path.Combine(path, asset.name)));
			}

			await Task.WhenAll(tasks);
		}

		public void Dispose()
		{
			_client?.Dispose();
			_client = null;
			GC.SuppressFinalize(true);
		}
	}
}