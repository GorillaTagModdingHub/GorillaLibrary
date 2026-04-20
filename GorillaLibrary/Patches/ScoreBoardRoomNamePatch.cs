using GorillaLibrary.Models;
using GorillaLibrary.Utilities;
using HarmonyLib;

namespace GorillaLibrary.Patches
{
    [HarmonyPatch(typeof(GorillaScoreBoard), "RoomType"), HarmonyPriority(Priority.VeryHigh)]
    internal class ScoreBoardRoomNamePatch
    {
        public static bool Prefix(ref string __result)
        {
            GameModeWrapper gamemode = GameModeUtility.CurrentGameMode;

            if (gamemode != null)
            {
                __result = gamemode.DisplayName.ToUpper();
                return false;
            }

            return true;
        }
    }
}
