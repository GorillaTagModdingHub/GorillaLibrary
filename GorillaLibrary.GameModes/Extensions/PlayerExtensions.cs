using GorillaLibrary.GameModes.Utilities;

namespace GorillaLibrary.GameModes.Extensions;

public static class PlayerExtensions
{
    public static bool IsTagged(this NetPlayer player)
    {
        GorillaGameManager gameManager = GorillaGameManager.instance;
        return gameManager != null && IsTagged(player, gameManager);
    }

    public static bool IsTagged(this NetPlayer player, GorillaGameManager gameManager)
    {
        if (gameManager is GorillaHuntManager huntManager)
            return PlayerUtility.IsTagged(player, huntManager);

        if (gameManager is GorillaTagManager tagManager)
            return tagManager.isCurrentlyTag ? PlayerUtility.IsTagger(player, tagManager) : PlayerUtility.IsTagged(player, tagManager);

        return false;
    }

    public static bool IsParticipant(this NetPlayer player)
    {
        return PlayerUtility.IsParticipant(player);
    }
}
