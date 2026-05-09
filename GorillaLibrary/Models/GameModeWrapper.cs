using GorillaGameModes;
using GorillaLibrary.Utilities;
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

        ID = game_mode_type.HasValue && !id.Contains(game_mode_type.Value.ToString()) ? string.Concat(id, game_mode_type) : id;
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
