using MelonLoader;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaLibrary;

public class Events
{
    public static GameEvents Game = new();

    public static PlayerEvents Player = new();

    public static RigEvents Rig = new();

    public static RoomEvents Room = new();

    public static ServerEvents Server = new();

    public static ZoneEvents Zone = new();

    public class GameEvents
    {
        /// <summary>
        /// Called when the game has been initialized
        /// </summary>
        public readonly MelonEvent OnGameInitialized = new(true);
    }

    public class PlayerEvents
    {
        /// <summary>
        /// Called both when a player enters the room and when joining a room (with the local player referenced)
        /// </summary>
        public readonly MelonEvent<NetPlayer> OnPlayerEnteredRoom = new();

        /// <summary>
        /// Called when a player leaves the room
        /// </summary>
        public readonly MelonEvent<NetPlayer> OnPlayerLeftRoom = new();

        /// <summary>
        /// Called when a player changes their name
        /// </summary>
        public readonly MelonEvent<NetPlayer, string> OnPlayerNameChanged = new();

        /// <summary>
        /// Called when the game overlay (like the SteamVR dashboard) is activated and deactivated
        /// </summary>
        public readonly MelonEvent<bool> OnGameOverlayActivation = new();
    }


    public class RigEvents
    {
        /// <summary>
        /// Called when a rig has been added for a player
        /// </summary>
        public readonly MelonEvent<VRRig, NetPlayer> OnRigAdded = new();

        /// <summary>
        /// Called when a rig has been removed for a player
        /// </summary>
        public readonly MelonEvent<VRRig> OnRigRemoved = new();

        /// <summary>
        /// Called when the colour of a rig has been changed
        /// </summary>
        public readonly MelonEvent<VRRig, Color> OnColourChanged = new();
    }

    public class RoomEvents
    {
        /// <summary>
        /// Called when a room has been joined
        /// </summary>
        public readonly MelonEvent OnRoomJoined = new();

        /// <summary>
        /// Called when a room has been left
        /// </summary>
        public readonly MelonEvent OnRoomLeft = new();
    }


    public class ServerEvents
    {
        /// <summary>
        /// Called when a message has been recieved from Mothership
        /// </summary>
        public readonly MelonEvent<string, string> OnMothershipMessageRecieved = new();
    }


    public class ZoneEvents
    {
        /// <summary>
        /// Called when the loaded zones have been changed
        /// </summary>
        public readonly MelonEvent<IEnumerable<GTZone>> OnZonesChanged = new();
    }
}
