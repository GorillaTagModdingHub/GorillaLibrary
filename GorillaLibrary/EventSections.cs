using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaLibrary;

public class CoreEvents
{
    /// <summary>
    /// Called when the game has been initialized
    /// </summary>
    public Action OnGameInitialized;
}

public class PlayerEvents
{
    /// <summary>
    /// Called both when a player enters the room and when joining a room (with the local player referenced)
    /// </summary>
    public Action<NetPlayer> OnPlayerEnteredRoom;

    /// <summary>
    /// Called when a player leaves the room
    /// </summary>
    public Action<NetPlayer> OnPlayerLeftRoom;

    /// <summary>
    /// Called when a player changes their name
    /// </summary>
    public Action<NetPlayer, string> OnPlayerNameChanged;

    /// <summary>
    /// Called when the game overlay (like the SteamVR dashboard) is activated and deactivated
    /// </summary>
    public Action<bool> OnGameOverlayActivation;
}


public class RigEvents
{
    /// <summary>
    /// Called when a rig has been added for a player
    /// </summary>
    public Action<VRRig, NetPlayer> OnRigAdded;

    /// <summary>
    /// Called when a rig has been removed for a player
    /// </summary>
    public Action<VRRig> OnRigRemoved;

    /// <summary>
    /// Called when the colour of a rig has been changed
    /// </summary>
    public Action<VRRig, Color> OnColourChanged;
}

public class RoomEvents
{
    /// <summary>
    /// Called when a room has been joined
    /// </summary>
    public Action OnRoomJoined;

    /// <summary>
    /// Called when a room has been left
    /// </summary>
    public Action OnRoomLeft;
}


public class ServerEvents
{
    /// <summary>
    /// Called when a message has been recieved from Mothership
    /// </summary>
    public Action<string, string> OnMothershipMessageRecieved;
}


public class ZoneEvents
{
    /// <summary>
    /// Called when the loaded zones have been changed
    /// </summary>
    public Action<IEnumerable<GTZone>> OnZonesChanged;
}

public class GameModeEvents
{
    /// <summary>
    /// Called when a player in the current room has been tagged
    /// </summary>
    public Action<GorillaGameManager, NetPlayer, NetPlayer> OnPlayerTagged;

    /// <summary>
    /// Called when the round in the current room has been completed
    /// </summary>
    public Action<GorillaGameManager> OnRoundCompleted;
}

public class CosmeticEvents
{
    public Action OnWornCosmeticsUpdated;
}