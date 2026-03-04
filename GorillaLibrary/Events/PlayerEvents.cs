// using ExitGames.Client.Photon;
using GorillaTag;

namespace GorillaLibrary.Events;

public class PlayerEvents
{
    public static DelegateListProcessor<NetPlayer> OnPlayerEnteredRoom;

    public static DelegateListProcessor<NetPlayer> OnPlayerLeftRoom;

    public static DelegateListProcessor<NetPlayer, string> OnPlayerNameChanged;

    // public static DelegateListProcessor<NetPlayer, Hashtable> OnPlayerCustomPropertiesChanged;

    // Local

    public static DelegateListProcessor<bool> OnGameOverlayActivation;
}
