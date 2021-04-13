using System;
using System.Threading.Tasks;

namespace AssetsDownloader
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			if (args.Length < 3)
			{
				Console.WriteLine("Usage: AssetsDownloader <user> <repo> <outputPath>");
				return;
			}

			using var downloader = new Downloader();
			dynamic release = Downloader.GetLatestRelease(args[0], args[1]);
			await downloader.DownloadAssets(args[2], release);
		}
	}
}