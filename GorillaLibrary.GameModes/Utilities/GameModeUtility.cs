using GorillaGameModes;
using GorillaLibrary.Extensions;
using GorillaLibrary.GameModes.Behaviours;
using GorillaLibrary.GameModes.Extensions;
using GorillaLibrary.GameModes.Models;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GorillaLibrary.GameModes.Utilities;

public static class GameModeUtility
{
    public static Gamemode CurrentGamemode { get; internal set; }

    public static Gamemode FindGamemodeInString(string gmString)
    {
        if (GameModeString.FromString(gmString) is GameModeString gmStringInstance)
        {
            string gameId = gmStringInstance.gameType;
            return GetGamemode(gamemode => gamemode.ID == gameId);
        }

        return GetGamemode(gamemode => gmString.EndsWith(gamemode.ID));
    }

    public static Gamemode GetGamemodeFromId(string id) => GetGamemode(gamemode => gamemode.ID == id);

    public static Gamemode GetGamemode(Func<Gamemode, bool> predicate)
    {
        return (GameModeManager.HasInstance && GameModeManager.Instance.Gamemodes.LastOrDefault(predicate) is Gamemode gameMode) ? gameMode : null;
    }

    public static string GetGameModeName(GameModeType gameModeType)
    {
        string modeName = (GetGameModeInstance(gameModeType) is GorillaGameManager gameManager) ? gameManager.GameModeName() : (string)AccessTools.Method(GameMode.GameModeZoneMapping.GetType(), "GetModeName").Invoke(GameMode.GameModeZoneMapping, [gameModeType]);
        return (modeName.ToLower() == gameModeType.GetName().ToLower()) ? gameModeType.GetName() : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(modeName.ToLower());
    }

    public static GorillaGameManager GetGameModeInstance(GameModeType gameModeType)
    {
        return (GameMode.GetGameModeInstance(gameModeType) is GorillaGameManager gameManager && gameManager.IsObjectExistent()) ? gameManager : null;
    }

    public static bool IsSuperGameMode(this GameModeType gameMode)
    {
        return Enum.IsDefined(typeof(GameModeType), (int)gameMode) && gameMode.GetName().ToLower().StartsWith("super");
    }

    public static IEnumerable<NetPlayer> GetTaggedPlayers(GorillaGameManager gameManager)
    {
        return NetworkSystem.Instance.InRoom ? NetworkSystem.Instance.AllNetPlayers.Where(player => player.IsTagged(gameManager)) : [];
    }

    public static IEnumerable<NetPlayer> GetParticipants()
    {
        return NetworkSystem.Instance.InRoom ? NetworkSystem.Instance.AllNetPlayers.Where(player => player.IsParticipant()) : [];
    }
}
