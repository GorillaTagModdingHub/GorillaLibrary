using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using ExitGames.Client.Photon;
using GorillaLibrary.Attributes;
using GorillaLibrary.Behaviours;
using GorillaLibrary.Patches;
using GorillaLibrary.Utilities;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: ModdedWardrobeSection("Outfits", typeof(OutfitSection_Load), typeof(OutfitSection_Clone))]

namespace GorillaLibrary;

[BepInPlugin("dev.gorillalibrary", "GorillaLibrary", "1.0.3")]
[BepInIncompatibility("org.legoandmars.gorillatag.utilla")]
internal sealed class Plugin : BaseUnityPlugin
{
    internal static new BepInEx.PluginInfo Info;

    internal static new ManualLogSource Logger;

    internal static List<ModdedWardrobeSectionAttribute> Sections;

    private GameObject sharedObject;

    public void Awake()
    {
        Info = base.Info;
        Logger = base.Logger;

        RuntimeHelpers.RunClassConstructor(typeof(Events).TypeHandle);

        MothershipClientApiUnity.OnMessageNotificationSocket += (notif, _) => Events.Server.OnMothershipMessageRecieved.Invoke(notif.Title, notif.Body);

        Harmony harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

        if (AccessTools.Method(typeof(GorillaTagger), "OnGameOverlayActivated") is MethodInfo method)
        {
            harmony.Patch(method, postfix: new(AccessTools.Method(typeof(GameOverlayPatch), nameof(GameOverlayPatch.Postfix)), priority: HarmonyLib.Priority.First));
        }

        Assembly gtAssembly = typeof(GorillaGameManager).Assembly;
        Type gtModeSerializeType = gtAssembly.GetType("GameModeSerializer");

        if (gtModeSerializeType != null)
        {
            harmony.Patch(AccessTools.Method(gtModeSerializeType, "BroadcastTag", parameters: [typeof(NetPlayer), typeof(NetPlayer), typeof(PhotonMessageInfo)]), postfix: new(AccessTools.Method(typeof(GameManagerPatches), nameof(GameManagerPatches.ClientTagPatch))));
            harmony.Patch(AccessTools.Method(gtModeSerializeType, "BroadcastRoundComplete", parameters: [typeof(PhotonMessageInfoWrapped)]), postfix: new(AccessTools.Method(typeof(GameManagerPatches), nameof(GameManagerPatches.ClientRoundCompletePatch))));
        }
    }

    public void Update()
    {
        InputUtility.Update();
    }

    private void OnGameInitialized()
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

        InputUtility.Initialize();
        RigUtility.Initialize();

        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;

        Sections = [];

        foreach (var (guid, pluginInfo) in Chainloader.PluginInfos)
        {
            var assembly = pluginInfo?.Instance?.GetType().Assembly;
            assembly?.GetCustomAttributes().ForEach(attribute =>
            {
                if (attribute is ModdedWardrobeSectionAttribute category)
                {
                    Sections.Add(category);
                }
            });

            if (pluginInfo.Instance is not GorillaUnityPlugin gup) continue;

            ConfigEntry<bool> stateEntry = Config.Bind("State", pluginInfo.Metadata.GUID, true);
            gup._stateEntry = stateEntry;
            gup.Enabled = stateEntry.Value;
        }

        sharedObject = new GameObject($"{Info.Metadata.Name} {Info.Metadata.Version}");
        DontDestroyOnLoad(sharedObject);
        sharedObject.AddComponent<NetworkController>();
        sharedObject.AddComponent<GameModeManager>();
        sharedObject.AddComponent<ConductBoardManager>();

        Sections.ForEach(category =>
        {
            var types = category.SectionTypes?.Where(type => typeof(WardrobeCategory).IsAssignableFrom(type));
            var list = new List<WardrobeCategory>();
            types.ForEach(type => list.Add((WardrobeCategory)sharedObject.AddComponent(type)));
            category.categories = list;
        });

        Events.Core.OnGameInitialized.Invoke();
        CoreUtility.Initialize();
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
            Logger.LogError(ex);
        }
    }
}
