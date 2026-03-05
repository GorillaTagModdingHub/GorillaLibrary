using ExitGames.Client.Photon;
using GorillaLibrary;
using MelonLoader;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System.Reflection;
using GorillaLibrary.Patches;
using GorillaLibrary.Utilities;

#region Events
using static GorillaLibrary.Events.GameEvents;
using static GorillaLibrary.Events.PlayerEvents;
using static GorillaLibrary.Events.RoomEvents;
using static GorillaLibrary.Events.ServerEvents;
using static GorillaLibrary.Events.ZoneEvents;
using GorillaLibrary.Events.System;
#endregion

[assembly: MelonInfo(typeof(Mod), "GorillaLibrary", "1.0.0", "dev9998")]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]

namespace GorillaLibrary;

internal class Mod : GorillaMod
{
    protected override bool Toggleable => false;

    protected override void OnEarlyInitialize()
    {
        MothershipClientApiUnity.OnMessageNotificationSocket += OnMothershipMessageRecieved;
    }

    protected override void OnLateInitialize()
    {
        NetworkSystem.Instance.OnMultiplayerStarted += OnRoomJoined;
        NetworkSystem.Instance.OnReturnedToSinglePlayer += OnRoomLeft;
        NetworkSystem.Instance.OnPlayerJoined += OnPlayerEntered;
        NetworkSystem.Instance.OnPlayerLeft += OnPlayerLeft;

        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;

        ZoneManagement.OnZoneChange += zoneData =>
        {
            IEnumerable<GTZone> activeZones = zoneData.Where(data => data.active).Select(data => data.zone);
            Bus.Publish(new ZonesChangedEvent(activeZones));
        };

        GameUtility.Initialize();
        InputUtility.Initialize();

        if (AccessTools.Method(typeof(GorillaTagger), "OnGameOverlayActivated") is MethodInfo method)
        {
            HarmonyInstance.Patch(method, postfix: new(AccessTools.Method(typeof(GameOverlayPatch), nameof(GameOverlayPatch.Postfix)), priority: HarmonyLib.Priority.First));
        }

        Bus.Publish(new GameInitializedEvent());
    }

    private void OnMothershipMessageRecieved(NotificationsMessageResponse notification, nint _)
    {
        Bus.Publish(new MothershipMessageReceivedEvent(notification));
    }

    private void OnRoomJoined()
    {
        Bus.Publish(new RoomJoinedEvent());
    }

    private void OnRoomLeft()
    {
        Bus.Publish(new RoomLeftEvent());
    }

    private void OnPlayerEntered(NetPlayer netPlayer)
    {
        Bus.Publish(new PlayerEnteredRoomEvent(netPlayer));
    }

    private void OnPlayerLeft(NetPlayer netPlayer)
    {
        Bus.Publish(new PlayerLeftRoomEvent(netPlayer));
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
                        Bus.Publish(new PlayerNameChangedEvent(netPlayer, nickName));
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
