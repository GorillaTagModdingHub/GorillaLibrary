using MelonLoader;

namespace GorillaLibrary.GameModes;

public static class Events
{
    public static readonly MelonEvent<GorillaGameManager, NetPlayer, NetPlayer> OnPlayerTagged;

    public static readonly MelonEvent<GorillaGameManager> OnRoundCompleted;
}
