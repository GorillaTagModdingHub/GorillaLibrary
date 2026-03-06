using GorillaLibrary.GameModes.Models;
using GorillaLibrary.GameModes.Utilities;
using HarmonyLib;

namespace GorillaLibrary.GameModes.Patches
{
    [HarmonyPatch(typeof(GorillaScoreBoard), "RoomType"), HarmonyPriority(Priority.VeryHigh)]
    internal class ScoreBoardRoomNamePatch
    {
        public static bool Prefix(ref string __result)
        {
            Gamemode gamemode = GameModeUtility.CurrentGamemode;

            if (gamemode != null)
            {
                __result = gamemode.DisplayName.ToUpper();
                return false;
            }

            return true;
        }
    }
}
