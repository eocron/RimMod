using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SteamWorkshopSynchronizer.Steam
{
    public sealed class SteamCmdBuilder
    {
        private readonly StringBuilder _fullCommand = new();
        private readonly List<(Regex, Action<Match>)> _matches = new();
        
        public string SteamCmdExecutablePath { get; set; }
        public ILogger Logger { get; set; }
        
        public SteamCmdBuilder AddCommand(string command)
        {
            if(string.IsNullOrWhiteSpace(command))
                return this;
            command = command.Trim();
            if (_fullCommand.Length > 0)
            {
                _fullCommand.Append(' ');
            }
            _fullCommand.Append('+');
            _fullCommand.Append(command);
            return this;
        }

        public void AddPerLineOutputMatcher(string regexPattern, Action<Match> onLineMatch)
        {
            _matches.Add((new Regex(regexPattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture), onLineMatch));
        }

        public async Task<int> ExecuteAsync(CancellationToken ct)
        {
            using var instance = new SteamCmdInstance(SteamCmdExecutablePath, _fullCommand + " +quit");
            var readTask = Task.Run(async () =>
            {
                try
                {
                    await foreach (var o in instance.Output.ReadAllAsync(ct))
                    {
                        foreach (var t in _matches)
                        {
                            foreach (Match m in t.Item1.Matches(o))
                            {
                                t.Item2(m);
                            }
                        }

                        TryHandleLog(o);
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
                await readTask.ConfigureAwait(false);
            }

            return statusCode;
        }

        private bool TryHandleLog(string logLine)
        {
            if(Logger == null)
                return false;
            
            if (Logger.IsEnabled(LogLevel.Error) && (logLine.Contains("error", StringComparison.OrdinalIgnoreCase) || logLine.Contains("failed", StringComparison.OrdinalIgnoreCase)))
            {
                Logger.LogError(logLine);
            }
            else if (Logger.IsEnabled(LogLevel.Warning) && logLine.Contains("warning", StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogWarning(logLine);
            }
            else
            {
                Logger.LogDebug(logLine);
            }

            return true;
        }
    }
}