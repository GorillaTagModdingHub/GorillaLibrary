using GorillaLibrary.Utilities;
using HarmonyLib;

namespace GorillaLibrary.Patches;

[HarmonyPatch(typeof(GorillaGameModes.GameMode), "FindGameModeInPropertyString")]
internal class GameModeSearchPatch
{
    public static bool Prefix(string gmString, ref string __result)
    {
        if (GameModeUtility.FindGameModeInString(gmString) is Models.GameModeWrapper gamemode)
        {
            __result = gamemode.ID;
            return false;
        }

        Plugin.Logger.LogError($"Gamemode could not be found in string: {gmString}");
        return true;
    }
}
