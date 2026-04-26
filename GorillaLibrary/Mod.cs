using ExitGames.Client.Photon;
using GorillaLibrary;
using GorillaLibrary.Attributes;
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
[assembly: WardrobeCategory("Outfits", typeof(OutfitSection))]

namespace GorillaLibrary;

internal sealed class Mod : MelonMod
{
    internal Action unityAction;

    internal List<WardrobeCategoryAttribute> wardrobeCategories;

    private MelonPreferences_Category _stateCategory;

    private GameObject sharedObject;

    public sealed override void OnEarlyInitializeMelon()
    {
        RuntimeHelpers.RunClassConstructor(typeof(Events).TypeHandle);

        Events.Core.OnGameInitialized.Subscribe(OnGameInitialized);
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

        CoreUtility.Initialize();
        InputUtility.Initialize();
        RigUtility.Initialize();

        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;

        wardrobeCategories = [];

        foreach (var melonBase in MelonBase.RegisteredMelons)
        {
            var assembly = melonBase.GetType().Assembly;

            assembly.GetCustomAttributes().ForEach(attribute =>
            {
                if (attribute is WardrobeCategoryAttribute category)
                {
                    wardrobeCategories.Add(category);
                }
            });
        }

        sharedObject = new GameObject("GorillaLibrary");
        UnityEngine.Object.DontDestroyOnLoad(sharedObject);

        wardrobeCategories.ForEach(category =>
        {
            var types = category.SectionTypes?.Where(type => typeof(WardrobeSection).IsAssignableFrom(type));
            var list = new List<WardrobeSection>();
            types.ForEach(type => list.Add((WardrobeSection)sharedObject.AddComponent(type)));
            category.sections = list;
        });

        GorillaTagger.OnPlayerSpawned(Events.Core.OnGameInitialized.Invoke);
    }

    public override void OnUpdate()
    {
        InputUtility.Update();

        if (unityAction != null)
        {
            foreach (Action action in unityAction.GetInvocationList().Cast<Action>())
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

            unityAction = null;
        }
    }

    private void OnGameInitialized()
    {
        sharedObject.AddComponent<NetworkController>();
        sharedObject.AddComponent<GameModeManager>();
        sharedObject.AddComponent<ConductBoardManager>();
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
