using BepInEx;
using BepInEx.Bootstrap;
using GorillaGameModes;
using GorillaLibrary.Attributes;
using GorillaLibrary.Models;
using GorillaLibrary.Patches;
using GorillaLibrary.Utilities;
using GorillaNetworking;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaLibrary.Behaviours;

internal class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }
    public static bool HasInstance => Instance != null;

    public static TaskCompletionSource<GameModeManager> Initialization { get; private set; } = new();

    public List<GameModeWrapper> Gamemodes { get; private set; }
    public List<GameModeWrapper> ModdedGamemodes { get; private set; }

    public readonly Dictionary<GameModeType, GameModeWrapper> DefaultGameModesPerMode = [];
    public readonly Dictionary<GameModeType, GameModeWrapper> ModdedGamemodesPerMode = [];

    // Custom game modes
    public List<GameModeWrapper> CustomGameModes;
    private GameObject customGameModeContainer;
    private List<PluginInfo> pluginInfos;

    private Dictionary<int, GorillaGameManager> gameModeTable;

    public void Awake()
    {
        if (Initialization.Task.IsCompleted) return;

        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        gameModeTable = (Dictionary<int, GorillaGameManager>)AccessTools.Field(typeof(GameMode), "gameModeTable").GetValue(null);

        customGameModeContainer = new GameObject("Utilla Custom Game Modes");
        customGameModeContainer.transform.SetParent(((GameMode)AccessTools.Field(typeof(GameMode), "instance").GetValue(null)).gameObject.transform);

        string currentGameMode = PlayerPrefs.GetString(GorillaComputerPatches.ModePreferenceKey, GameModeType.Infection.ToString());
        GorillaComputer.instance.currentGameMode.Value = currentGameMode;

        GameModeType[] gameModeTypes = [.. Enum.GetValues(typeof(GameModeType)).Cast<GameModeType>()];
        for (int i = 0; i < gameModeTypes.Length; i++)
        {
            if (i == (int)GameModeType.Count) break;

            GameModeType modeType = gameModeTypes[i];
            if (!DefaultGameModesPerMode.TryAdd(modeType, new GameModeWrapper(modeType))) continue;
            ModdedGamemodesPerMode.Add(modeType, new GameModeWrapper(Constants.ModdedPrefix, $"Modded {GameModeUtility.GetGameModeName(modeType)}", modeType));
        }

        Plugin.Logger.LogMessage($"Modded Game Modes: {string.Join(", ", ModdedGamemodesPerMode.Select(item => item.Value).Select(mode => mode.DisplayName).Select(displayName => string.Format("\"{0}\"", displayName)))}");
        ModdedGamemodes = [.. ModdedGamemodesPerMode.Values];

        Gamemodes = [.. DefaultGameModesPerMode.Values];

        pluginInfos = GetPluginInfos();
        CustomGameModes = GetGamemodes(pluginInfos);
        Plugin.Logger.LogMessage($"Custom Game Modes: {string.Join(", ", CustomGameModes.Select(mode => mode.DisplayName).Select(displayName => string.Format("\"{0}\"", displayName)))}");
        Gamemodes.AddRange(ModdedGamemodes.Concat(CustomGameModes));
        Gamemodes.ForEach(AddGamemodeToPrefabPool);
        Plugin.Logger.LogMessage($"Game Modes: {string.Join(", ", Gamemodes.Select(mode => mode.DisplayName).Select(displayName => string.Format("\"{0}\"", displayName)))}");

        Initialization.SetResult(this);
    }

    public List<GameModeWrapper> GetGamemodes(List<PluginInfo> infos)
    {
        List<GameModeWrapper> gamemodes = [];

        HashSet<GameModeWrapper> additonalGamemodes = [];
        foreach (var info in infos)
        {
            additonalGamemodes.UnionWith(info.Gamemodes);
        }

        foreach (var gamemode in ModdedGamemodes)
        {
            additonalGamemodes.Remove(gamemode);
        }

        gamemodes.AddRange(additonalGamemodes);

        return gamemodes;
    }

    List<PluginInfo> GetPluginInfos()
    {
        List<PluginInfo> infos = [];

        foreach (var info in Chainloader.PluginInfos)
        {
            if (info.Value is null) continue;
            BaseUnityPlugin plugin = info.Value.Instance;
            if (plugin is null) continue;
            Type type = plugin.GetType();

            IEnumerable<GameModeWrapper> gamemodes = GetGamemodes(type);

            if (gamemodes.Any())
            {
                infos.Add(new PluginInfo
                {
                    Plugin = plugin,
                    Gamemodes = [.. gamemodes],
                    OnGamemodeJoin = CreateJoinLeaveAction(plugin, type, typeof(ModdedGamemodeJoinAttribute)),
                    OnGamemodeLeave = CreateJoinLeaveAction(plugin, type, typeof(ModdedGamemodeLeaveAttribute))
                });
            }
        }

        return infos;
    }

    Action<string> CreateJoinLeaveAction(BaseUnityPlugin plugin, Type baseType, Type attribute)
    {
        ParameterExpression param = Expression.Parameter(typeof(string));
        ParameterExpression[] paramExpression = [param];
        ConstantExpression instance = Expression.Constant(plugin);
        BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        Action<string> action = null;
        foreach (var method in baseType.GetMethods(bindingFlags).Where(m => m.GetCustomAttribute(attribute) != null))
        {
            var parameters = method.GetParameters();
            MethodCallExpression methodCall;
            if (parameters.Length == 0)
            {
                methodCall = Expression.Call(instance, method);
            }
            else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
            {
                methodCall = Expression.Call(instance, method, param);
            }
            else
            {
                continue;
            }

            action += Expression.Lambda<Action<string>>(methodCall, paramExpression).Compile();
        }

        return action;
    }

    HashSet<GameModeWrapper> GetGamemodes(Type type)
    {
        IEnumerable<ModdedGamemodeAttribute> attributes = type.GetCustomAttributes<ModdedGamemodeAttribute>();

        HashSet<GameModeWrapper> gamemodes = [];
        if (attributes is not null)
        {
            foreach (ModdedGamemodeAttribute attribute in attributes)
            {
                if (attribute.gamemode is not null)
                {
                    gamemodes.Add(attribute.gamemode);
                    continue;
                }
                gamemodes.UnionWith(ModdedGamemodes);
            }
        }

        return gamemodes;
    }

    void AddGamemodeToPrefabPool(GameModeWrapper gamemode)
    {
        if (GameMode.gameModeKeyByName.ContainsKey(gamemode.ID))
        {
            Plugin.Logger.LogWarning($"Game Mode already exists: has ID {gamemode.ID}");
            return;
        }

        if (gamemode.BaseGameMode.HasValue && gamemode.ID != gamemode.BaseGameMode.Value.GetName())
        {
            GameMode.gameModeKeyByName.Add(gamemode.ID, GameMode.gameModeKeyByName[gamemode.BaseGameMode.Value.GetName()]);
            return;
        }

        if (gamemode.GameManager is null) return;

        Type gmType = gamemode.GameManager;

        if (gmType is null || !gmType.IsSubclassOf(typeof(GorillaGameManager)))
        {
            GameModeType? gmKey = gamemode.BaseGameMode;

            if (gmKey == null)
            {
                Plugin.Logger.LogWarning($"Game Mode not made cuz lack of info: has ID {gamemode.ID}");
                return;
            }

            GameMode.gameModeKeyByName[gamemode.ID] = (int)gmKey;
            //GameMode.gameModeKeyByName[gamemode.DisplayName] = (int)gmKey;
            GameMode.gameModeNames.Add(gamemode.ID);
            return;
        }

        GameObject prefab = new($"{gamemode.ID}: {gmType.Name}");
        prefab.SetActive(false);

        GorillaGameManager gameMode = prefab.AddComponent(gmType) as GorillaGameManager;
        int gameModeKey = (int)gameMode.GameType();

        if (gameModeTable.ContainsKey(gameModeKey))
        {
            Plugin.Logger.LogWarning($"Game Mode with name '{gameModeTable[gameModeKey].GameModeName()}' is already using GameType '{gameModeKey}'.");
            Destroy(prefab);
            return;
        }

        gameModeTable[gameModeKey] = gameMode;
        GameMode.gameModeKeyByName[gamemode.ID] = gameModeKey;
        GameMode.gameModeNames.Add(gamemode.ID);
        GameMode.gameModes.Add(gameMode);

        prefab.transform.SetParent(customGameModeContainer.transform);
        prefab.SetActive(true);

        if (gameMode.fastJumpLimit == 0 || gameMode.fastJumpMultiplier == 0)
        {
            Plugin.Logger.LogWarning($"FAST JUMP SPEED AREN'T ASSIGNED FOR {gmType.Name}!!! ASSIGN THESE ASAP");

            float[] speed = gameMode.LocalPlayerSpeed();
            gameMode.fastJumpLimit = speed[0];
            gameMode.fastJumpMultiplier = speed[1];
        }
    }

    internal void OnRoomJoin(InternalRoom args)
    {
        string gamemode = args.Gamemode;

        Plugin.Logger.LogMessage($"Joined room: with game mode {gamemode}");

        foreach (var pluginInfo in pluginInfos)
        {
            Plugin.Logger.LogMessage($"Plugin {pluginInfo}");

            if (pluginInfo.Gamemodes.Any(x => gamemode.Contains(x.ID)))
            {
                try
                {
                    pluginInfo.OnGamemodeJoin?.Invoke(gamemode);
                    Plugin.Logger.LogMessage("Plugin is suitable for game mode");
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError($"Join action could not be called");
                    Plugin.Logger.LogError(ex);
                }
                continue;
            }

            Plugin.Logger.LogMessage("Plugin is unsupported for game mode");
        }
    }

    internal void OnRoomLeft(InternalRoom args)
    {
        string gamemode = args.Gamemode;

        Plugin.Logger.LogMessage($"Left room: with game mode {gamemode}");

        foreach (var pluginInfo in pluginInfos)
        {
            if (pluginInfo.Gamemodes.Any(x => gamemode.Contains(x.ID)))
            {
                try
                {
                    pluginInfo.OnGamemodeLeave?.Invoke(gamemode);
                    //Logging.Info($"Plugin {pluginInfo.Plugin.Info.Metadata.Name} is suitable for game mode");
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError($"Leave action could not be called");
                    Plugin.Logger.LogError(ex);
                }
            }
        }
    }
}
