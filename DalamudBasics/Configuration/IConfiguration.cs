using Dalamud.Game.Text;

namespace DalamudBasics.Configuration
{
    public interface IConfiguration
    {
        // If your plugin does not write to chat, set this to None
        public XivChatType DefaultOutputChatType { get; set; }

        // If true, all chat meant to be sent to the server will be logged.
        public bool LogOutgoingChatOutput { get; set; }

        // If true, all chat written only visually and not sent will be logged.
        public bool LogClientOnlyChatOutput { get; set; }

        public int LimitedChatChannelsMessageDelayInMs { get; set; }
    }
}
