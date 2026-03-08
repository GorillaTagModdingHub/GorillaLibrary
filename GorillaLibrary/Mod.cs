using ExitGames.Client.Photon;
using GorillaLibrary;
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

[assembly: MelonInfo(typeof(Mod), "GorillaLibrary", "1.0.0", "dev9998")]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]

namespace GorillaLibrary;

internal sealed class Mod : MelonMod
{
    internal static Action _unityAction;

    private MelonPreferences_Category _stateCategory;

    public sealed override void OnEarlyInitializeMelon()
    {
        RuntimeHelpers.RunClassConstructor(typeof(Events).TypeHandle);

        MothershipClientApiUnity.OnMessageNotificationSocket += (notif, _) => Events.Server.OnMothershipMessageRecieved.Invoke(notif.Title, notif.Body);

        if (AccessTools.Method(typeof(GorillaTagger), "OnGameOverlayActivated") is MethodInfo method)
        {
            HarmonyInstance.Patch(method, postfix: new(AccessTools.Method(typeof(GameOverlayPatch), nameof(GameOverlayPatch.Postfix)), priority: HarmonyLib.Priority.First));
        }

        GorillaTagger.OnPlayerSpawned(Events.Game.OnGameInitialized.Invoke);
    }

    public sealed override void OnInitializeMelon()
    {
        string preferencePath = Path.Combine(MelonEnvironment.UserDataDirectory, "GorillaLibrary.cfg");
        _stateCategory = MelonPreferences.CreateCategory("State", "State");
        _stateCategory.SetFilePath(preferencePath);

        foreach (MelonBase mb in MelonBase.RegisteredMelons)
        {
            if (mb is not GorillaMod gm) continue;
            MelonInfoAttribute info = gm.Info;
            MelonPreferences_Entry<bool> statePreference = _stateCategory.CreateEntry(info.Name, true, info.Name, null, false, false, null);
            gm.Enabled = statePreference.Value;
            gm._statePreference = statePreference;
        }
    }

    public sealed override void OnLateInitializeMelon()
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
