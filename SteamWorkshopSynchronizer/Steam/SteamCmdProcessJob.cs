using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SteamWorkshopSynchronizer.Commands;

namespace SteamWorkshopSynchronizer.Steam
{
    public sealed class SteamCmdProcessJob : ISteamCmdProcess, IAsyncJob
    {
        public const string ExitedMessage = "STEAM_CMD_EXITED";
        
        private readonly string _exeFilePath;
        private readonly ILogger _logger;
        private readonly Channel<string> _inputs;
        private readonly Channel<string> _outputs;

        public SteamCmdProcessJob(string exeFilePath, ILogger logger)
        {
            _exeFilePath = exeFilePath;
            _logger = logger;
            _inputs = Channel.CreateUnbounded<string>();
            _outputs = Channel.CreateUnbounded<string>();
        }

        private async Task ProcessInputsAsync(Process process, CancellationToken ct)
        {
            await Task.Yield();
            try
            {
                await SteamCmdHelper.WaitForComplete(_outputs.Reader, ct).ConfigureAwait(false);
                await foreach (var line in _inputs.Reader.ReadAllAsync(ct).ConfigureAwait(false))
                {
                    await process.StandardInput.WriteLineAsync(line).ConfigureAwait(false);
                    await process.StandardInput.FlushAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when(ct.IsCancellationRequested)
            {
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        private Process CreateProcess()
        {
            var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = _exeFilePath,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
                StandardInputEncoding = Encoding.UTF8,
                //ErrorDialog = false,
                UseShellExecute = false,
                //WindowStyle = ProcessWindowStyle.Hidden,
                //CreateNoWindow = false
            };
            return process;
        }
        public async Task RunAsync(CancellationToken ct)
        {
            await Task.Yield();
            using var process = CreateProcess();
            await using var reg = ct.Register(() =>
            {
                try
                {
                    if (!process.WaitForExit(5000))
                    {
                        process.Kill();
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to kill process {processId} on cancellation", process.Id);
                }
            });
            process.OutputDataReceived+= ProcessOnOutputDataReceived;
            process.ErrorDataReceived+= ProcessOnErrorDataReceived;
            process.Exited+= ProcessOnExited;
            process.EnableRaisingEvents = true;
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var inputsProcessor = ProcessInputsAsync(process, cts.Token);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                try
                {
                    await process.WaitForExitAsync(cts.Token).ConfigureAwait(false);
                    cts.Cancel();
                    await inputsProcessor;
                }
                finally
                {
                    process.CancelErrorRead();
                    process.CancelOutputRead();
                }
            }
            finally
            {
                process.OutputDataReceived-= ProcessOnOutputDataReceived;
                process.ErrorDataReceived-= ProcessOnErrorDataReceived;
                process.Exited-= ProcessOnExited;
            }
        }

        private void ProcessOnExited(object sender, EventArgs e)
        {
            _outputs.Writer.TryWrite(ExitedMessage);
            _logger.LogDebug(ExitedMessage);
        }

        private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                _outputs.Writer.TryWrite(e.Data);
                _logger.LogDebug(e.Data);
            }
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                _outputs.Writer.TryWrite(e.Data);
                _logger.LogDebug(e.Data);
            }
        }

        public ChannelWriter<string> Inputs => _inputs.Writer;
        public ChannelReader<string> Outputs => _outputs.Reader;
    }
}