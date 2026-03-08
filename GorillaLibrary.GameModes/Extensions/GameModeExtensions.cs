using GorillaGameModes;
using GorillaLibrary.GameModes.Utilities;

namespace GorillaLibrary.GameModes.Extensions;

public static class GameModeExtensions
{
    public static string GetName(this GameModeType gameMode)
    {
        return GameModeUtility.GetGameModeName(gameMode);
    }

    public static GorillaGameManager GetGameManager(this GameModeType gameMode)
    {
        return GameModeUtility.GetGameModeInstance(gameMode);
    }
}
