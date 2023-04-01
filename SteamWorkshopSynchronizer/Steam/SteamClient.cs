﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

            var output =
                await ExecuteSteamCmdAsync(ct,
                        CreateLoginCommand(),
                        CreateWorkshopDownloadItemCommand(appId, fileId))
                    .ConfigureAwait(false);
            var path = Regex.Matches(output, "Downloaded.+?\"(?<path>.+?)\"",
                    RegexOptions.Compiled | RegexOptions.ExplicitCapture).Cast<Match>()
                .Select(x => x.Groups["path"].Value)
                .SingleOrDefault();
            if (path == null)
                throw new Exception($"Download of {fileId} failed.");
            return path;
        }

        private static string CreateExitCommand()
        {
            return "quit";
        }

        private async Task<string> ExecuteSteamCmdAsync(CancellationToken ct, params string[] commands)
        {
            var sb = new StringBuilder();
            foreach (var cmd in commands)
            {
                TryAddCommand(sb, cmd);
            }
            TryAddCommand(sb, CreateExitCommand());
            var args = sb.ToString();
            var output = await ExecuteSteamCmdAsync(args, ct).ConfigureAwait(false);
            return output;
        }

        private async Task<string> ExecuteSteamCmdAsync(string arguments, CancellationToken ct)
        {
            using var instance = new SteamCmdInstance(_steamCmdExePath, arguments);
            var allOutput = new StringBuilder();
            var readTask = Task.Run(async () =>
            {
                try
                {
                    await foreach (var o in instance.Output.ReadAllAsync(ct))
                    {
                        allOutput.AppendLine(o);

                        if (o.Contains("error", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogError(o);
                        }
                        else if (o.Contains("warning", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogWarning(o);
                        }
                        else
                        {
                            _logger.LogDebug(o);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    
                }
            }, ct);
            int statusCode = -1;
            try
            {
                statusCode = await instance.RunToCompletionAsync(ct).ConfigureAwait(false);
            }
            finally
            {
                await readTask;
            }

            allOutput.AppendLine("Status code: " + statusCode);
            return allOutput.ToString();
        }

        private static bool TryAddCommand(StringBuilder sb, string cmd)
        {
            if(string.IsNullOrWhiteSpace(cmd))
                return false;
            
            if (sb.Length > 0)
            {
                sb.Append(' ');
            }

            sb.Append('+');
            sb.Append(cmd);
            return true;
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