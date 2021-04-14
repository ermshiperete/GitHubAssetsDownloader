# GitHubAssetsDownloader

Download assets of a GitHub release

## Installation

This is a dotnet global tool, so you can install it with:

```bash
dotnet tool install --global GitHubAssetsDownloader
```

and then run

```bash
GitHubAssetsDownloader MyGitHubUser MyRepo TargetDir *.nupkg
```

The last parameter is optional. If it exists it will only download files that match
the file spec.
