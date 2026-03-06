using GorillaGameModes;
using GorillaLibrary.GameModes;
using GorillaLibrary.GameModes.Utilities;
using MelonLoader;
using System;

namespace GorillaLibrary.GameModes.Models;

public class Gamemode
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
    public GameModeType? BaseGamemode { get; }

    /// <summary>
    /// An optional reference of a game mode manager to create
    /// </summary>
    public Type GameManager { get; }

    internal Gamemode(GameModeType gameModeType)
    {
        BaseGamemode = gameModeType;

        ID = gameModeType.ToString();
        DisplayName = GameModeUtility.GetGameModeName(gameModeType);

        Melon<Mod>.Logger.Msg($"Replicated base gamemode: based on {gameModeType} type");
    }

    public Gamemode(string id, string displayName, GameModeType? game_mode_type = null)
    {
        BaseGamemode = game_mode_type;

        ID = game_mode_type.HasValue && !id.Contains(game_mode_type.Value.ToString()) ? string.Concat(id, game_mode_type) : id;
        DisplayName = displayName;

        Melon<Mod>.Logger.Msg($"Constructed custom gamemode: {id} based on {(game_mode_type.HasValue ? game_mode_type.Value : "no")} type");
    }

    public Gamemode(string id, string displayName, Type gameManager)
    {
        ID = id;
        DisplayName = displayName;
        GameManager = gameManager;

        Melon<Mod>.Logger.Msg($"Constructed custom gamemode: {id} with {gameManager.GetType()} manager");
    }
}
