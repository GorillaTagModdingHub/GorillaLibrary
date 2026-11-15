using GorillaGameModes;
using GorillaLibrary.Models;
using System;

namespace GorillaLibrary.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ModdedGamemodeAttribute : Attribute
{
    public readonly GameModeWrapper gamemode;

    /// <summary>
    /// A blank null GameModeWrapper
    /// </summary>
    public ModdedGamemodeAttribute()
    {
        gamemode = null;
    }

    /// <summary>
    /// Creates a new GameModeWrapper based on an internal id, display name, and base game mode.
    /// </summary>
    /// <param name="id">Internal ID, not usually shown to players unless a mod specifically shows it or uses it.</param>
    /// <param name="displayName">In-game name that would be displayed on things such as the scoreboard and gamemode selector.</param>
    /// <param name="gameModeType">Base GameModeType to be used.</param>
    public ModdedGamemodeAttribute(string id, string displayName, GameModeType gameModeType = GameModeType.Infection)
    {
        gamemode = new GameModeWrapper(id, displayName, gameModeType);
    }

    /// <summary>
    /// Creates a new GameModeWrapper based on an internal id, display name, and game manager.
    /// </summary>
    /// <param name="id">Internal ID, not usually shown to players unless a mod specifically shows it or uses it.</param>
    /// <param name="displayName">In-game name that would be displayed on things such as the scoreboard and gamemode selector.</param>
    /// <param name="gameManager">Custom game manager to use.</param>
    public ModdedGamemodeAttribute(string id, string displayName, Type gameManager)
    {
        gamemode = new GameModeWrapper(id, displayName, gameManager);
    }

    /// <summary>
    /// Creates a new GameModeWrapper based on an internal id, display name, base game mode, exclude flag, and zones.
    /// </summary>
    /// <param name="id">Internal ID, not usually shown to players unless a mod specifically shows it or uses it.</param>
    /// <param name="displayName">In-game name that would be displayed on things such as the scoreboard and gamemode selector.</param>
    /// <param name="gameModeType">Base GameModeType to be used.</param>
    /// <param name="exclude">If true, the zones will be excluded instead of included.</param>
    /// <param name="zones">The zones on which this game mode is allowed in.</param>
    public ModdedGamemodeAttribute(string id, string displayName, GameModeType gameModeType, bool exclude, params GTZone[] zones)
    {
        gamemode = new GameModeWrapper(id, displayName, gameModeType);
        if (zones.Length == 0) { return; }
        if (exclude) { gamemode.ExcludedZones = zones; }
        else { gamemode.Zones = zones; }
    }
}
