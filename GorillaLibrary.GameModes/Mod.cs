using GorillaLibrary.GameModes;
using GorillaLibrary.GameModes.Attributes;
using GorillaLibrary.GameModes.Behaviours;
using GorillaLibrary.GameModes.Patches;
using HarmonyLib;
using MelonLoader;
using Photon.Pun;
using System;
using System.Reflection;
using UnityEngine;

[assembly: MelonInfo(typeof(Mod), "GorillaLibrary.GameModes", "1.0.0", "dev9998")]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]
[assembly: MelonAdditionalDependencies("GorillaLibrary")]

namespace GorillaLibrary.GameModes;

[ModdedGamemode]
internal class Mod : MelonMod
{
    public override void OnEarlyInitializeMelon()
    {
        GorillaLibrary.Events.Game.OnGameInitialized.Subscribe(OnGameInitialized);

        Assembly gtAssembly = typeof(GorillaGameManager).Assembly;
        Type gtModeSerializeType = gtAssembly?.GetType("GameModeSerializer");
        if (gtModeSerializeType != null)
        {
            HarmonyInstance.Patch(AccessTools.Method(gtModeSerializeType, "BroadcastTag", parameters: [typeof(NetPlayer), typeof(NetPlayer), typeof(PhotonMessageInfo)]), postfix: new(AccessTools.Method(typeof(GameManagerPatches), nameof(GameManagerPatches.ClientTagPatch))));
            HarmonyInstance.Patch(AccessTools.Method(gtModeSerializeType, "BroadcastRoundComplete", parameters: [typeof(PhotonMessageInfoWrapped)]), postfix: new(AccessTools.Method(typeof(GameManagerPatches), nameof(GameManagerPatches.ClientRoundCompletePatch))));
        }
    }

    public void OnGameInitialized()
    {
        UnityEngine.Object.DontDestroyOnLoad(new GameObject("Utilla", typeof(NetworkController), typeof(GameModeManager)));
    }

    [ModdedGamemodeJoin]
    public void OnModdedJoin()
    {
        LoggerInstance.Msg("ModdedGamemodeJoin");
    }

    [ModdedGamemodeLeave]
    public void OnModdedLeave()
    {
        LoggerInstance.Msg("ModdedGamemodeLeave");
    }
}
