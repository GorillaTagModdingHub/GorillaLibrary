using ExitGames.Client.Photon;
using GorillaLibrary;
using GorillaLibrary.Events;
using MelonLoader;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System.Reflection;
using GorillaLibrary.Patches;
using GorillaLibrary.Utilities;

[assembly: MelonInfo(typeof(Mod), "GorillaLibrary", "1.0.0", "dev9998")]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]

namespace GorillaLibrary;

internal class Mod : GorillaMod
{
    public override void OnEarlyInitializeMelon()
    {
        MothershipClientApiUnity.OnMessageNotificationSocket += OnMothershipMessageRecieved;
    }

    protected override void Start()
    {
        NetworkSystem.Instance.OnMultiplayerStarted += OnRoomJoined;
        NetworkSystem.Instance.OnReturnedToSinglePlayer += OnRoomLeft;
        NetworkSystem.Instance.OnPlayerJoined += OnPlayerEntered;
        NetworkSystem.Instance.OnPlayerLeft += OnPlayerLeft;

        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;

        ZoneManagement.OnZoneChange += zoneData =>
        {
            IEnumerable<GTZone> activeZones = zoneData.Where(data => data.active).Select(data => data.zone);
            ZoneEvents.OnZonesChanged?.InvokeSafe(activeZones);
        };

        GameUtility.Initialize();
        InputUtility.Initialize();

        if (AccessTools.Method(typeof(GorillaTagger), "OnGameOverlayActivated") is MethodInfo method)
        {
            HarmonyInstance.Patch(method, postfix: new(AccessTools.Method(typeof(GameOverlayPatch), nameof(GameOverlayPatch.Postfix)), priority: HarmonyLib.Priority.First));
        }

        Events.GameEvents.OnGameInitialized?.InvokeSafe();
    }

    private void OnMothershipMessageRecieved(NotificationsMessageResponse notification, nint _)
    {
        ServerEvents.OnMothershipMessageRecieved?.InvokeSafe(notification.Title, notification.Body);
    }

    private void OnRoomJoined()
    {
        RoomEvents.OnRoomJoined?.InvokeSafe();
    }

    private void OnRoomLeft()
    {
        RoomEvents.OnRoomLeft?.InvokeSafe();
    }

    private void OnPlayerEntered(NetPlayer netPlayer)
    {
        PlayerEvents.OnPlayerEnteredRoom?.InvokeSafe(netPlayer);
    }

    private void OnPlayerLeft(NetPlayer netPlayer)
    {
        PlayerEvents.OnPlayerLeftRoom?.InvokeSafe(netPlayer);
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
                        PlayerEvents.OnPlayerNameChanged.InvokeSafe(netPlayer, nickName);
                    }
                    break;
            }
        }
        catch(Exception ex)
        {
            LoggerInstance.Error(ex);
        }
    }
}
