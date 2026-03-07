using GorillaGameModes;
using GorillaLibrary.GameModes.Models;
using System;

namespace GorillaLibrary.GameModes.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ModdedGamemodeAttribute : Attribute
{
    public readonly Gamemode gamemode;

    public ModdedGamemodeAttribute()
    {
        gamemode = null;
    }

    public ModdedGamemodeAttribute(string id, string displayName, GameModeType gameModeType = GameModeType.Infection)
    {
        gamemode = new Gamemode(id, displayName, gameModeType);
    }

    public ModdedGamemodeAttribute(string id, string displayName, Type gameManager)
    {
        gamemode = new Gamemode(id, displayName, gameManager);
    }
}
