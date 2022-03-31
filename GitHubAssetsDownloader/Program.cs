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
				Help();
				return;
			}

			string release = null;

			var i = 0;
			if (args[0] == "--release")
			{
				release = args[1];
				i = 2;
			}

			if (args.Length < i + 3)
			{
				Help();
				return;
			}

			using var downloader = new Downloader();
			try
			{
				dynamic artifacts = await Downloader.GetRelease(args[i], args[i+1], release);
				var filter = args.Length > i+3 ? args[i+3] : null;
				await downloader.DownloadAssets(args[i+2], filter, artifacts);
			}
			catch (HttpRequestException e)
			{
				Console.WriteLine($"ERROR: {e.Message}");
			}
		}

		private static void Help()
		{
			Console.WriteLine(
				"Usage: GitHubAssetsDownloader [--release <release>] <user> <repo> <outputPath> [filter]");
		}
	}
}