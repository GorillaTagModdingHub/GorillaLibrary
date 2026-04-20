using GorillaGameModes;
using GorillaLibrary.Models;
using System;

namespace GorillaLibrary.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ModdedGamemodeAttribute : Attribute
{
    public readonly GameModeWrapper gamemode;

    public ModdedGamemodeAttribute()
    {
        gamemode = null;
    }

    public ModdedGamemodeAttribute(string id, string displayName, GameModeType gameModeType = GameModeType.Infection)
    {
        gamemode = new GameModeWrapper(id, displayName, gameModeType);
    }

    public ModdedGamemodeAttribute(string id, string displayName, Type gameManager)
    {
        gamemode = new GameModeWrapper(id, displayName, gameManager);
    }
}
