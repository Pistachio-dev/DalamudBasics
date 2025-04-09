using Dalamud.Game.Text;

namespace DalamudBasics.Chat.Listener
{
    public static class ChatChannelSets
    {
        public static XivChatType[] CommonChannels = [XivChatType.Say, XivChatType.Shout, XivChatType.Yell, XivChatType.Party, XivChatType.Alliance];

        public static XivChatType[] CommonChannelsAndLinkshells = [XivChatType.Say,
            XivChatType.Shout,
            XivChatType.Yell,
            XivChatType.Party,
            XivChatType.Alliance,
            XivChatType.CrossLinkShell1,
            XivChatType.CrossLinkShell2,
            XivChatType.CrossLinkShell3,
            XivChatType.CrossLinkShell4,
            XivChatType.CrossLinkShell5,
            XivChatType.CrossLinkShell6,
            XivChatType.CrossLinkShell7,
            XivChatType.CrossLinkShell8,
            XivChatType.Ls1,
            XivChatType.Ls2,
            XivChatType.Ls3,
            XivChatType.Ls4,
            XivChatType.Ls5,
            XivChatType.Ls6,
            XivChatType.Ls7,
            XivChatType.Ls8];

        public static XivChatType[] PartyFinderchannels = [XivChatType.Party, XivChatType.Alliance];
    }
}
