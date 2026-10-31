using ExitGames.Client.Photon;
using Photon.Realtime;

namespace GorillaLibrary.Extensions;

public static class EventDataExtensions
{
    /// <summary>Parses out the otherwise unexposed RaiseEventOptions that are sent with every photon event. Included for network security and use with the NetChannels.</summary>
    /// <returns>The RaiseEventOptions object the event was sent with, doesn't include everything as it is partially stripped when sent.</returns>
    public static RaiseEventOptions ExtractOptions(this EventData ev)
    {
        RaiseEventOptions options = new RaiseEventOptions();

        if (ev.Parameters.ContainsKey(247) && ev.Parameters[247] is EventCaching eventCaching)
            options.CachingOption = eventCaching;

        if (ev.Parameters.ContainsKey(252) && ev.Parameters[252] is int[] targetActors)
            options.TargetActors = targetActors;

        if (ev.Parameters.ContainsKey(240) && ev.Parameters[240] is byte interestGroup)
            options.InterestGroup = interestGroup;

        if (ev.Parameters.ContainsKey(246) && ev.Parameters[246] is ReceiverGroup receivers)
            options.Receivers = receivers;

        if (ev.Parameters.ContainsKey(234) && ev.Parameters[234] is byte flags)
            options.Flags = new WebFlags(flags);

        return options;
    }
}