using GorillaGameModes;
using GorillaLibrary.GameModes;
using GorillaLibrary.GameModes.Models;
using GorillaLibrary.GameModes.Utilities;
using GorillaNetworking;
using HarmonyLib;
using MelonLoader;
using System;

namespace GorillaLibrary.GameModes.Patches
{
    [HarmonyPatch(typeof(GorillaNetworkJoinTrigger))]
    internal class DesiredGameModePatches
    {
        [HarmonyPatch(nameof(GorillaNetworkJoinTrigger.GetDesiredGameType)), HarmonyPrefix]
        public static bool DesiredGameTypePatch(GorillaNetworkJoinTrigger __instance, ref string __result, ref GTZone ___zone)
        {
            Type joinTriggerType = __instance.GetType();

            Melon<Mod>.Logger.Msg($"{joinTriggerType.Name}.{nameof(GorillaNetworkJoinTrigger.GetDesiredGameType)}");

            if (joinTriggerType == typeof(GorillaNetworkRankedJoinTrigger) || ___zone == GTZone.ranked)
            {
                Melon<Mod>.Logger.Msg($"Ranked JoinTrigger resorting to hardcoded infection mode");
                __result = GameModeType.InfectionCompetitive.ToString();
                return false;
            }

            string currentGameMode = GorillaComputer.instance.currentGameMode.Value;

            if (!Enum.IsDefined(typeof(GameModeType), currentGameMode))
            {
                if (GameModeUtility.GetGamemodeFromId(currentGameMode) is Gamemode gamemode && gamemode.BaseGamemode.HasValue && gamemode.BaseGamemode.Value < GameModeType.Count)
                {
                    GameModeType gameModeType = gamemode.BaseGamemode.Value;

                    GameModeType verifiedGameMode = (GameModeType)AccessTools.Method(GameMode.GameModeZoneMapping.GetType(), "VerifyModeForZone").Invoke(GameMode.GameModeZoneMapping, [__instance.zone, gameModeType, NetworkSystem.Instance.SessionIsPrivate]);
                    if (verifiedGameMode == gameModeType)
                    {
                        Melon<Mod>.Logger.Msg($"JoinTrigger of {___zone.GetName()} allowing generic game mode: {currentGameMode} under {gameModeType}");
                        __result = currentGameMode;
                        return false;
                    }

                    Melon<Mod>.Logger.Msg($"JoinTrigger of {___zone.GetName()} changing unsupported game mode: {currentGameMode} under {gameModeType}");
                    __result = verifiedGameMode.ToString();
                    return false;
                }

                Melon<Mod>.Logger.Msg($"JoinTrigger of {___zone.GetName()} allowing custom game mode: {currentGameMode}");
                __result = currentGameMode;
                return false;
            }

            Melon<Mod>.Logger.Msg($"JoinTrigger of {___zone.GetName()} naturally allows game mode: {currentGameMode}");
            return true;
        }

        [HarmonyPatch(nameof(GorillaNetworkJoinTrigger.GetDesiredGameTypeLocalized)), HarmonyPrefix]
        public static bool DesiredLocalizedGameTypePatch(GorillaNetworkJoinTrigger __instance, ref string __result, ref GTZone ___zone) => DesiredGameTypePatch(__instance, ref __result, ref ___zone);
    }
}
