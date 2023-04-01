using System.Threading.Channels;

namespace SteamWorkshopSynchronizer.Steam
{
    public interface ISteamCmdProcess
    {
        ChannelWriter<string> Inputs { get; }
        
        ChannelReader<string> Outputs { get; }
    }
}