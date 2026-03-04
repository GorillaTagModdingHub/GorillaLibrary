using GorillaLibrary.Modding.Models;
using GorillaLibrary.Modding.Utilities;
using HarmonyLib;

namespace GorillaLibrary.Modding.Patches
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
