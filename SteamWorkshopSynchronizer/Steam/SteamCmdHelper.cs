using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SteamWorkshopSynchronizer.Steam
{
    public class SteamCmdHelper
    {
        public static Task WaitForComplete(ChannelReader<string> channelReader, CancellationToken ct)
        {
            return WaitForAsync(channelReader, x => x.StartsWith("Steam>") || x.StartsWith("Loading Steam API...OK"), ct);
        }

        public static async Task<string> WaitForAsync(ChannelReader<string> channelReader, Func<string, bool> check, CancellationToken ct)
        {
            await foreach (var d in channelReader.ReadAllAsync(ct))
            {
                if (check(d))
                {
                    return d;
                }
            }

            throw new Exception("Channel stopped.");
        }
    }
}