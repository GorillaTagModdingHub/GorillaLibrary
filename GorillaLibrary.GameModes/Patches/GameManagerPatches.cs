// https://github.com/Not-A-Bird-07/Utilla/commit/c813503da35b39e63290a776af447e16a88d64c5

using GorillaGameModes;
using HarmonyLib;
using MelonLoader;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaLibrary.GameModes.Patches;

[HarmonyPatch(typeof(GorillaGameManager)), HarmonyWrapSafe]
internal class GameManagerPatches
{
    [HarmonyPatch(nameof(GorillaGameManager.GameTypeName)), HarmonyPrefix]
    public static bool GameTypeNamePatch(GorillaGameManager __instance, ref string __result)
    {
        if (Enum.IsDefined(typeof(GameModeType), (int)__instance.GameType())) return true;

        var gameModeTable = (Dictionary<int, GorillaGameManager>)AccessTools.Field(typeof(GameMode), "gameModeTable").GetValue(null);
        int index = gameModeTable.LastOrDefault(pair => pair.Value == __instance).Key;
        __result = GameMode.gameModeKeyByName.LastOrDefault(pair => pair.Value == index).Key;

        return false;
    }

    [HarmonyPatch(typeof(GameMode), nameof(GameMode.BroadcastTag)), HarmonyPostfix]
    public static void MasterTagPatch(NetPlayer taggedPlayer, NetPlayer taggingPlayer)
    {
        if (!NetworkSystem.Instance.IsMasterClient) return;

        MonoBehaviour behaviour = (MonoBehaviour)AccessTools.Field(typeof(GameMode), "activeNetworkHandler").GetValue(null);
        if (behaviour == null || !behaviour) return;

        Events.OnPlayerTagged.Invoke((GorillaGameManager)AccessTools.Field(behaviour.GetType(), "gameModeInstance").GetValue(behaviour), taggedPlayer, taggingPlayer);
    }

    [HarmonyPatch(typeof(GameMode), nameof(GameMode.BroadcastRoundComplete)), HarmonyPostfix]
    public static void MasterRoundCompletePatch()
    {
        if (!NetworkSystem.Instance.IsMasterClient) return;

        MonoBehaviour behaviour = (MonoBehaviour)AccessTools.Field(typeof(GameMode), "activeNetworkHandler").GetValue(null);
        if (behaviour == null || !behaviour) return;

        Events.OnRoundCompleted.Invoke((GorillaGameManager)AccessTools.Field(behaviour.GetType(), "gameModeInstance").GetValue(behaviour));
    }

    // [HarmonyPatch(typeof(GameModeSerializer), nameof(GameModeSerializer.BroadcastTag), argumentTypes: [typeof(NetPlayer), typeof(NetPlayer), typeof(PhotonMessageInfo)]), HarmonyPostfix]
    public static void ClientTagPatch(GorillaGameManager ___gameModeInstance, NetPlayer taggedPlayer, NetPlayer taggingPlayer, PhotonMessageInfo info)
    {
        if (NetworkSystem.Instance.IsMasterClient || taggedPlayer == null || taggingPlayer == null || !info.Sender.IsMasterClient) return;

        Melon<Mod>.Logger.Msg("BroadcastTag");
        Events.OnPlayerTagged.Invoke(___gameModeInstance, taggedPlayer, taggingPlayer);
    }

    // [HarmonyPatch(typeof(GameModeSerializer), nameof(GameModeSerializer.BroadcastRoundComplete), argumentTypes: [typeof(PhotonMessageInfoWrapped)]), HarmonyPostfix]
    public static void ClientRoundCompletePatch(GorillaGameManager ___gameModeInstance, PhotonMessageInfoWrapped info)
    {
        if (NetworkSystem.Instance.IsMasterClient || !info.Sender.IsMasterClient) return;

        Melon<Mod>.Logger.Msg("BroadcastRoundComplete");
        Events.OnRoundCompleted.Invoke(___gameModeInstance);
    }
}