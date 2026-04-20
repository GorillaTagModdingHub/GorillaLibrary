using GorillaGameModes;
using GorillaLibrary.Utilities;

namespace GorillaLibrary.Extensions;

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
