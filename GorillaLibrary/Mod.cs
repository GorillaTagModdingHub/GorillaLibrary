using ExitGames.Client.Photon;
using GorillaLibrary;
using GorillaLibrary.Behaviours;
using GorillaLibrary.Patches;
using GorillaLibrary.Utilities;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: MelonInfo(typeof(Mod), "GorillaLibrary", "1.0.2", "dev9998")]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]
[assembly: MelonIncompatibleAssemblies("GorillaLibrary.GameModes", "GorillaLibrary.Content")]

namespace GorillaLibrary;

internal sealed class Mod : MelonMod
{
    internal static Action _unityAction;

    private MelonPreferences_Category _stateCategory;

    public sealed override void OnEarlyInitializeMelon()
    {
        Events.Core.OnGameInitialized.Subscribe(OnGameInitialized);

        RuntimeHelpers.RunClassConstructor(typeof(Events).TypeHandle);

        MothershipClientApiUnity.OnMessageNotificationSocket += (notif, _) => Events.Server.OnMothershipMessageRecieved.Invoke(notif.Title, notif.Body);

        if (AccessTools.Method(typeof(GorillaTagger), "OnGameOverlayActivated") is MethodInfo method)
        {
            HarmonyInstance.Patch(method, postfix: new(AccessTools.Method(typeof(GameOverlayPatch), nameof(GameOverlayPatch.Postfix)), priority: HarmonyLib.Priority.First));
        }

        Assembly gtAssembly = typeof(GorillaGameManager).Assembly;
        Type gtModeSerializeType = gtAssembly.GetType("GameModeSerializer");

        if (gtModeSerializeType != null)
        {
            HarmonyInstance.Patch(AccessTools.Method(gtModeSerializeType, "BroadcastTag", parameters: [typeof(NetPlayer), typeof(NetPlayer), typeof(PhotonMessageInfo)]), postfix: new(AccessTools.Method(typeof(GameManagerPatches), nameof(GameManagerPatches.ClientTagPatch))));
            HarmonyInstance.Patch(AccessTools.Method(gtModeSerializeType, "BroadcastRoundComplete", parameters: [typeof(PhotonMessageInfoWrapped)]), postfix: new(AccessTools.Method(typeof(GameManagerPatches), nameof(GameManagerPatches.ClientRoundCompletePatch))));
        }
    }

    public override void OnInitializeMelon()
    {
        string preferencePath = Path.Combine(MelonEnvironment.UserDataDirectory, "GorillaLibrary.cfg");
        _stateCategory = MelonPreferences.CreateCategory("State");
        _stateCategory.SetFilePath(preferencePath);

        foreach (MelonBase mb in MelonBase.RegisteredMelons)
        {
            if (mb is not GorillaMod gm) continue;

            try
            {
                MelonInfoAttribute info = gm.Info;
                MelonPreferences_Entry<bool> statePreference = _stateCategory.CreateEntry(info.Name, true, info.Name);
                gm._statePreference = statePreference;
                gm.Enabled = statePreference.Value;
            }
            catch (Exception ex)
            {
                LoggerInstance.Error(ex);
            }
        }
    }

    public override void OnLateInitializeMelon()
    {
        NetworkSystem.Instance.OnMultiplayerStarted += Events.Room.OnRoomJoined.Invoke;
        NetworkSystem.Instance.OnReturnedToSinglePlayer += Events.Room.OnRoomLeft.Invoke;
        NetworkSystem.Instance.OnPlayerJoined += Events.Player.OnPlayerEnteredRoom.Invoke;
        NetworkSystem.Instance.OnPlayerLeft += Events.Player.OnPlayerLeftRoom.Invoke;

        ZoneManagement.OnZoneChange += zoneData =>
        {
            IEnumerable<GTZone> activeZones = zoneData.Where(data => data.active).Select(data => data.zone);
            Events.Zone.OnZonesChanged.Invoke(activeZones);
        };

        GameUtility.Initialize();
        InputUtility.Initialize();
        RigUtility.Initialize();

        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        GorillaTagger.OnPlayerSpawned(Events.Core.OnGameInitialized.Invoke);
    }

    public override void OnUpdate()
    {
        InputUtility.Update();

        if (_unityAction != null)
        {
            foreach (Action action in _unityAction.GetInvocationList().Cast<Action>())
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    LoggerInstance.Error(ex);
                }
            }

            _unityAction = null;
        }
    }

    private void OnGameInitialized()
    {
        UnityEngine.Object.DontDestroyOnLoad(new GameObject("GorillaLibrary", typeof(NetworkController), typeof(GameModeManager)));
    }

    private void OnEvent(EventData data)
    {
        try
        {
            switch (data.Code)
            {
                case 255:
                    Hashtable hashtable = (Hashtable)data[249];
                    if (NetworkSystem.Instance is NetworkSystem netSys && netSys.GetPlayer(data.Sender) is NetPlayer netPlayer && hashtable.TryGetValue(byte.MaxValue, out object value))
                    {
                        string nickName = value as string;
                        Events.Player.OnPlayerNameChanged.Invoke(netPlayer, nickName);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            LoggerInstance.Error(ex);
        }
    }
}
