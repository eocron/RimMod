using System;
using System.Threading;
using System.Threading.Tasks;
using SteamCmdFluentApi;

namespace SteamWorkshopSynchronizer.Steam
{
    public class SteamCmdClient : ISteamCmdClient
    {
        private readonly ISteamCmdProcess _cmd;

        private string CreateWorkshopDownloadItemCommand(int appId, long fileId)
        {
            return $"workshop_download_item {appId} {fileId}";
        }

        private string CreateLoginCommand(string username = null, string password = null, string steamGuardKey = null)
        {
            if (username == null)
            {
                return "login anonymous";
            }
            else
            {
                return $"login {username} {password} {steamGuardKey}";
            }
        }
        
        public SteamCmdClient(ISteamCmdProcess cmd)
        {
            _cmd = cmd;
        }

        public async Task LoginAnonymousAsync(CancellationToken ct)
        {
            await _cmd.Inputs.WriteAsync("login anonymous", ct).ConfigureAwait(false);
            while (true)
            {
                await _cmd.Inputs.WriteAsync("login anonymous", ct).ConfigureAwait(false);
                await Task.Delay(1000);
            }
            //await SteamCmdHelper.WaitForAsync(_cmd.Outputs, x => x.StartsWith("Waiting for user info...OK"), ct).ConfigureAwait(false);
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
            throw new System.NotImplementedException();
        }


    }
}