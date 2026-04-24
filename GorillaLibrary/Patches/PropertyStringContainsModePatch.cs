using GorillaGameModes;
using GorillaLibrary.Models;
using GorillaLibrary.Utilities;
using HarmonyLib;

namespace GorillaLibrary.Patches;

[HarmonyPatch(typeof(GameModeString), "DoesPropertyStringContainGameMode")]
internal class PropertyStringContainsModePatch
{
    public static bool Prefix(string propertyString, string gameMode, ref bool __result)
    {
        string separator = PropertyStringSeparatorTranspiler.Separator;

        if (propertyString.Contains(separator))
        {
            string[] split = propertyString.Split(separator);
            bool useSeperator = split.Length > 2;
            GameModeWrapper wrapper = useSeperator ? GameModeUtility.FindGameModeFromId(split[2]) : null;
            __result = wrapper != null && wrapper.GameModeName == gameMode;
            return false;
        }

        return true;
    }
}
