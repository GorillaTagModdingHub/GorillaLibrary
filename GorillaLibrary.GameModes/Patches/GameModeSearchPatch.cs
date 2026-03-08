using GorillaGameModes;
using GorillaLibrary.GameModes.Models;
using GorillaLibrary.GameModes.Utilities;
using HarmonyLib;
using MelonLoader;

namespace GorillaLibrary.GameModes.Patches;

[HarmonyPatch(typeof(GameMode), "FindGameModeInString")]
internal class GameModeSearchPatch
{
    public static bool Prefix(string gmString, ref string __result)
    {
        if (GameModeUtility.FindGamemodeInString(gmString) is Gamemode gamemode)
        {
            __result = gamemode.ID;
            return false;
        }

        Melon<Mod>.Logger.Error("NOT GOOD");
        return true;
    }
}
