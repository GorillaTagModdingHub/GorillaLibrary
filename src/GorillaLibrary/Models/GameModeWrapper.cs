using GorillaGameModes;
using GorillaLibrary.Utilities;
using GorillaNetworking;
using System;

namespace GorillaLibrary.Models;

public class GameModeWrapper
{
    /// <summary>
    /// The title of the Gamemode visible through the gamemode selector
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// The internal ID of the Gamemode
    /// </summary>
    public string ID { get; }

    /// <summary>
    /// An optional reference of a game mode to inherit
    /// </summary>
    public GameModeType? BaseGameMode { get; }

    /// <summary>
    /// An optional reference of a game mode manager to create
    /// </summary>
    public Type GameManager { get; }

    /// <summary>
    /// Maps the zones on which this game mode is allowed in. If null, it will default to the base game mode's allowed zones.
    /// </summary>
    public GTZone[] Zones { get; internal set; }

    /// <summary>
    /// Maps the zones on which this game mode is not allowed in. If null, it will default to the base game mode's allowed zones.
    /// </summary>
    public GTZone[] ExcludedZones { get; internal set; }

    internal bool AllowedZones(GTZone zone, bool isPrivate)
    {
        if (GorillaComputer.instance != null && GorillaComputer.instance.IsPlayerInVirtualStump()) zone = GTZone.customMaps;
        if (Zones != null) { return Array.IndexOf(Zones, zone) >= 0; }
        if (ExcludedZones != null) { return Array.IndexOf(ExcludedZones, zone) < 0; }
        if (!BaseGameMode.HasValue) { return true; }
        return GameMode.GameModeZoneMapping.GetModesForZone(zone, isPrivate).Contains(BaseGameMode.Value);
    }

    public string GameModeName
    {
        get
        {
            GameModeType? baseGameMode = BaseGameMode;
            return baseGameMode.HasValue ? baseGameMode.ToString() : ID;
        }
    }

    internal GameModeWrapper(GameModeType gameModeType)
    {
        BaseGameMode = gameModeType;

        ID = gameModeType.ToString();
        DisplayName = GameModeUtility.GetGameModeName(gameModeType);

        Plugin.Logger.LogInfo($"Replicated base gamemode: based on {gameModeType} type");
    }

    public GameModeWrapper(string id, string displayName, GameModeType? game_mode_type = null)
    {
        BaseGameMode = game_mode_type;

        ID = id;
        DisplayName = displayName;

        Plugin.Logger.LogInfo($"Constructed custom gamemode: {id} based on {(game_mode_type.HasValue ? game_mode_type.Value : "no")} type");
    }

    public GameModeWrapper(string id, string displayName, Type gameManager)
    {
        ID = id;
        DisplayName = displayName;
        GameManager = gameManager;

        Plugin.Logger.LogInfo($"Constructed custom gamemode: {id} with {gameManager.GetType()} manager");
    }
}
