using GorillaGameModes;
using HarmonyLib;

namespace GorillaLibrary.GameModes.Utilities;

public static class PlayerUtility
{
    public static bool IsTagged(NetPlayer player, GorillaTagManager tagManager)
    {
        return tagManager.currentInfected?.Contains(player) ?? false;
    }

    public static bool IsTagger(NetPlayer player, GorillaTagManager tagManager)
    {
        return tagManager.currentIt == player;
    }

    public static bool IsTagged(NetPlayer player, GorillaHuntManager huntManager)
    {
        return huntManager.currentHunted?.Contains(player) ?? false;
    }

    public static bool IsParticipant(NetPlayer player)
    {
        return (bool)AccessTools.Method(typeof(GameMode), "CanParticipate").Invoke(player, null);
    }
}
