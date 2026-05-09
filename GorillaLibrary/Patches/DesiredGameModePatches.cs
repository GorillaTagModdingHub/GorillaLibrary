using GorillaGameModes;
using GorillaLibrary.Utilities;
using GorillaNetworking;
using HarmonyLib;
using System;

namespace GorillaLibrary.Patches
{
    [HarmonyPatch(typeof(GorillaNetworkJoinTrigger))]
    internal class DesiredGameModePatches
    {
        [HarmonyPatch(nameof(GorillaNetworkJoinTrigger.GetDesiredGameType)), HarmonyPrefix]
        public static bool DesiredGameTypePatch(GorillaNetworkJoinTrigger __instance, ref string __result, ref GTZone ___zone)
        {
            Type joinTriggerType = __instance.GetType();

            Plugin.Logger.LogMessage($"{joinTriggerType.Name}.{nameof(GorillaNetworkJoinTrigger.GetDesiredGameType)}");

            if (joinTriggerType == typeof(GorillaNetworkRankedJoinTrigger) || ___zone == GTZone.ranked)
            {
                Plugin.Logger.LogMessage($"Ranked JoinTrigger resorting to hardcoded infection mode");
                __result = GameModeType.InfectionCompetitive.ToString();
                return false;
            }

            string currentGameMode = GorillaComputer.instance.currentGameMode.Value;

            if (!Enum.IsDefined(typeof(GameModeType), currentGameMode))
            {
                if (GameModeUtility.FindGameModeFromId(currentGameMode) is Models.GameModeWrapper gamemode && gamemode.BaseGameMode.HasValue && gamemode.BaseGameMode.Value < GameModeType.Count)
                {
                    GameModeType gameModeType = gamemode.BaseGameMode.Value;

                    GameModeType verifiedGameMode = (GameModeType)AccessTools.Method(GorillaGameModes.GameMode.GameModeZoneMapping.GetType(), "VerifyModeForZone").Invoke(GorillaGameModes.GameMode.GameModeZoneMapping, [__instance.zone, gameModeType, NetworkSystem.Instance.SessionIsPrivate]);
                    if (verifiedGameMode == gameModeType)
                    {
                        Plugin.Logger.LogMessage($"JoinTrigger of {___zone.GetName()} allowing generic game mode: {currentGameMode} under {gameModeType}");
                        __result = currentGameMode;
                        return false;
                    }

                    Plugin.Logger.LogMessage($"JoinTrigger of {___zone.GetName()} changing unsupported game mode: {currentGameMode} under {gameModeType}");
                    __result = verifiedGameMode.ToString();
                    return false;
                }

                Plugin.Logger.LogMessage($"JoinTrigger of {___zone.GetName()} allowing custom game mode: {currentGameMode}");
                __result = currentGameMode;
                return false;
            }

            Plugin.Logger.LogMessage($"JoinTrigger of {___zone.GetName()} naturally allows game mode: {currentGameMode}");
            return true;
        }

        [HarmonyPatch(nameof(GorillaNetworkJoinTrigger.GetDesiredGameTypeLocalized)), HarmonyPrefix]
        public static bool DesiredLocalizedGameTypePatch(GorillaNetworkJoinTrigger __instance, ref string __result, ref GTZone ___zone) => DesiredGameTypePatch(__instance, ref __result, ref ___zone);
    }
}
