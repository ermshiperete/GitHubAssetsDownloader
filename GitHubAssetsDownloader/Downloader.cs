using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace GitHubAssetsDownloader
{
	public class Downloader: IDisposable
	{
		private HttpClient _client;
		private Regex      _fileFilter;

		public static async Task<object> GetRelease(string user, string repo, string release)
		{
			var uriPart = string.IsNullOrEmpty(release) ? "latest" : $"tags/{release}";

			var client = new RestClient("https://api.github.com/repos");
			client.UseNewtonsoftJson();
			var request = new RestRequest($"{user}/{repo}/releases/{uriPart}");
			dynamic result = null;
			try
			{
				result = await client.GetAsync<ExpandoObject>(request);
			}
			catch (Exception e)
			{
				throw new HttpRequestException($"Can't get releases for {user}/{repo}", e);
			}

			if (result == null ||
				(((IDictionary<string, object>)result).ContainsKey("message") && result.message == "Not Found"))
			{
				throw new HttpRequestException($"Can't get releases for {user}/{repo}: {result?.message}");
			}

			return result;
		}

		private async Task DownloadFile(string uri, string filePath)
		{
			var response = await _client.GetAsync(uri);
			await using var stream = new FileStream(filePath, FileMode.CreateNew);
			await response.Content.CopyToAsync(stream);
			Console.WriteLine($"Downloaded {filePath}");
		}

		private void StoreFilterAsRegex(string filter)
		{
			if (string.IsNullOrEmpty(filter))
			{
				_fileFilter = null;
				return;
			}

			var bldr = new StringBuilder();
			foreach (var c in filter)
			{
				switch (c)
				{
					case '*':
						bldr.Append(".*");
						break;
					case '?':
						bldr.Append(".");
						break;
					case '.':
						bldr.Append("\\.");
						break;
					default:
						bldr.Append(c);
						break;
				}
			}

			_fileFilter = new Regex(bldr.ToString());
		}

		private bool IncludeFile(string fileName)
		{
			return _fileFilter?.IsMatch(fileName) ?? true;
		}

		public async Task DownloadAssets(string path, string filter, dynamic release)
		{
			StoreFilterAsRegex(filter);

			var tasks = new List<Task>();
			if (Directory.Exists(path))
				Directory.Delete(path, true);
			Directory.CreateDirectory(path);
			_client = new HttpClient();
			foreach (var asset in release.assets)
			{
				if (!IncludeFile(asset.name))
					continue;

				tasks.Add(DownloadFile(asset.browser_download_url, Path.Combine(path, asset.name)));
			}

			await Task.WhenAll(tasks);
		}

		public void Dispose()
		{
			_client?.Dispose();
			_client = null;
			GC.SuppressFinalize(this);
		}
	}
}