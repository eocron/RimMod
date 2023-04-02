using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SteamWorkshopSynchronizer.Steam
{
    public sealed class SteamClient : ISteamClient
    {
        private readonly SteamClientCredentials _credentials;
        private readonly string _steamCmdExePath;
        private readonly ILogger _logger;

        public SteamClient(SteamClientCredentials credentials, string steamCmdFolderPath, ILogger logger)
        {
            _credentials = credentials;
            _steamCmdExePath = Path.Combine(steamCmdFolderPath, "steamcmd.exe");
            _logger = logger;
        }

        public async Task<string> DownloadWorkshopItemAndReturnPathAsync(int appId, long fileId, CancellationToken ct)
        {
            if (appId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(appId));
            }

            if (fileId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fileId));
            }

            var paths = new List<string>();
            await new SteamCmdBuilder()
                .WithExePath(_steamCmdExePath)
                .WithLogger(_logger)
                .WithOutputRegex("Downloaded.+?\"(?<path>.+?)\"", x => paths.Add(x.Groups["path"].Value))
                .AddCommand(CreateLoginCommand())
                .AddCommand(CreateWorkshopDownloadItemCommand(appId, fileId))
                .ExecuteAsync(ct)
                .ConfigureAwait(false);
            var path = paths.SingleOrDefault();
            if (path == null)
                throw new Exception($"Download of {fileId} failed.");
            return path;
        }

        private static string CreateWorkshopDownloadItemCommand(int appId, long fileId)
        {
            return $"workshop_download_item {appId} {fileId}";
        }

        private string CreateLoginCommand()
        {
            if (_credentials == null)
            {
                return "login anonymous";
            }

            return $"login {_credentials.UserName} {_credentials.Password} {_credentials.GuardKey}";
        }
    }
}