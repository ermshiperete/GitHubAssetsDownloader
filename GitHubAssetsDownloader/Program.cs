using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GitHubAssetsDownloader
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			if (args.Length < 3)
			{
				Console.WriteLine("Usage: GitHubAssetsDownloader <user> <repo> <outputPath>");
				return;
			}

			using var downloader = new Downloader();
			try
			{
				dynamic release = Downloader.GetLatestRelease(args[0], args[1]);
				await downloader.DownloadAssets(args[2], release);
			}
			catch (HttpRequestException e)
			{
				Console.WriteLine($"ERROR: {e.Message}");
			}
		}
	}
}