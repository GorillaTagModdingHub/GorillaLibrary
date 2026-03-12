using MelonLoader;

namespace GorillaLibrary.GameModes;

public static class Events
{
    public static readonly MelonEvent<GorillaGameManager, NetPlayer, NetPlayer> OnPlayerTagged = new();

    public static readonly MelonEvent<GorillaGameManager> OnRoundCompleted = new();
}
