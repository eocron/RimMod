using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SteamWorkshopSynchronizer.Steam
{
    public sealed class SteamCmdInstance : ISteamCmdInstance
    {
        private bool _disposed;
        private readonly Channel<string> _output;
        private readonly Process _process;
        public ChannelReader<string> Output => _output.Reader;


        public SteamCmdInstance(string exePath, string arguments)
        {
            _process = CreateBackgroundProcess(
                exePath ?? throw new ArgumentNullException(nameof(exePath)),
                arguments ?? throw new ArgumentNullException(nameof(arguments)));
            _process.OutputDataReceived+= ProcessOnOutputDataReceived;
            _process.ErrorDataReceived+= ProcessOnErrorDataReceived;
            _process.Exited+= ProcessOnExited;
            _output = Channel.CreateUnbounded<string>();
        }

        public async Task<int> RunToCompletionAsync(CancellationToken ct)
        {
            Validate();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            await using var reg = cts.Token.Register(KillOnCancellation);
            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();
            try
            {
                await _process.WaitForExitAsync(cts.Token).ConfigureAwait(false);
            }
            finally
            {
                _process.CancelErrorRead();
                _process.CancelOutputRead();
            }

            return _process.ExitCode;
        }

        private void ProcessOnExited(object sender, EventArgs e)
        {
            _output.Writer.TryComplete();
        }

        private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                _output.Writer.TryWrite(e.Data.Trim());
            }
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                _output.Writer.TryWrite(e.Data.Trim());
            }
        }
        private static Process CreateBackgroundProcess(string exePath, string arguments)
        {
            var process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo = new ProcessStartInfo
            {
                Arguments = arguments,
                FileName = exePath,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
                ErrorDialog = false,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
            };
            return process;
        }
        
        private void KillOnCancellation()
        {
            try
            {
                _process.Kill(true);
            }
            catch
            {
                // ignored
            }
        }

        private void Validate()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("SteamCMD process disposed");
            }
        }
        
        public void Dispose()
        {
            if(_disposed)
                return;
            
            _process.OutputDataReceived-= ProcessOnOutputDataReceived;
            _process.ErrorDataReceived-= ProcessOnErrorDataReceived;
            _process.Exited-= ProcessOnExited;
            _output.Writer.TryComplete();
            _disposed = true;
        }
    }
}