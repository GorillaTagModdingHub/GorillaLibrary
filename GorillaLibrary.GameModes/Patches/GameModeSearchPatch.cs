using GorillaGameModes;
using GorillaLibrary.Modding.Models;
using GorillaLibrary.Modding.Utilities;
using HarmonyLib;
using MelonLoader;

namespace GorillaLibrary.Modding.Patches;

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
